using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using LibraryManagementFE.Models;
using LibraryManagementFE.Policies;

namespace LibraryManagementFE.Services
{
    /// <summary>
    /// Borrow service with inventory, policy enforcement, persistence and payment history.
    /// </summary>
    public class BorrowService
    {
        private readonly LibraryPolicy _policy;
        private readonly LibraryDataStore _store;

        public BorrowService(LibraryPolicy policy)
        {
            _policy = policy;
            _store = LibraryDataStoreFile.LoadOrCreate();
            EnsureSampleData();
        }

        public IReadOnlyList<BookRecord> Books => _store.Books.AsReadOnly();
        public IReadOnlyList<ReaderRecord> Readers => _store.Readers.AsReadOnly();
        public IReadOnlyList<BorrowRecord> Borrows => _store.Borrows.AsReadOnly();
        public IReadOnlyList<PaymentRecord> Payments => _store.Payments.AsReadOnly();

        public IEnumerable<BookRecord> GetAvailableBooks()
            => _store.Books.Where(b => b.Availability == BookAvailability.SanCo);

        public IEnumerable<ReaderRecord> GetReaders()
            => _store.Readers;

        public BorrowRecord LendBook(ReaderRecord reader, BookRecord book, DateTime? borrowDate = null, int? days = null)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var existingBook = _store.Books.FirstOrDefault(b => b.Id == book.Id);
            if (existingBook != null && existingBook.Availability == BookAvailability.DangMuon)
                throw new InvalidOperationException("Sách này đang được mượn.");

            var existingReader = _store.Readers.FirstOrDefault(r => r.Id == reader.Id || (!string.IsNullOrWhiteSpace(r.CardNumber) && r.CardNumber == reader.CardNumber));
            if (existingReader != null)
            {
                if (_policy.NotBorrowWhenCardLocked && existingReader.Status != ReaderStatus.HoatDong)
                    throw new InvalidOperationException("Thẻ độc giả đang không hoạt động và không được phép mượn.");

                var borrowedCount = _store.Borrows.Count(b => b.ReaderId == existingReader.Id && (b.Status == BorrowStatus.DangMuon || b.Status == BorrowStatus.QuaHan));
                if (borrowedCount >= _policy.MaxBooksPerReader)
                    throw new InvalidOperationException($"Độc giả đã mượn tối đa {_policy.MaxBooksPerReader} sách.");

                reader = existingReader;
            }

            if (existingBook != null)
            {
                book = existingBook;
            }

            var now = borrowDate ?? DateTime.Now;
            var loanDays = days ?? _policy.MaxLoanDays;
            var due = now.Date.AddDays(loanDays);

            EnsureBookExists(book);
            EnsureReaderExists(reader);

            var record = new BorrowRecord
            {
                Id = GenerateId(),
                Stt = _store.Borrows.Count + 1,
                MaPhieu = GenerateMaPhieu(now),
                ReaderId = reader.Id,
                ReaderName = reader.Name,
                ReaderInitials = reader.Initials,
                ReaderEmail = reader.Email,
                CardNumber = reader.CardNumber,
                BookId = book.Id,
                BookTitle = book.Title,
                Author = book.Author,
                LoanDays = loanDays,
                RenewalCount = 0,
                BorrowDate = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DueDate = due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ReturnDate = string.Empty,
                FineAmount = 0m,
                FinePaid = false,
                FinePaidDate = string.Empty,
                Status = BorrowStatus.DangMuon
            };

            book.Availability = BookAvailability.DangMuon;
            _store.Borrows.Add(record);
            Save();
            return record;
        }

        public (BorrowRecord? record, decimal fine) ReturnBook(string maPhieu, DateTime? returnDate = null)
        {
            var rec = _store.Borrows.FirstOrDefault(r => r.MaPhieu == maPhieu);
            if (rec == null) return (null, 0m);

            var returned = returnDate ?? DateTime.Now;
            rec.ReturnDate = returned.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            rec.ReturnNote = string.Empty;

            var due = DateTime.ParseExact(rec.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var daysLate = (returned.Date - due.Date).Days;
            var fine = daysLate > 0 ? daysLate * _policy.PenaltyPerDay : 0m;

            rec.FineAmount = fine;
            rec.FinePaid = false;
            rec.FinePaidDate = string.Empty;
            rec.Status = daysLate > 0 ? BorrowStatus.DaTraTre : BorrowStatus.DaTraTot;

            var book = _store.Books.FirstOrDefault(b => b.Id == rec.BookId);
            if (book != null)
                book.Availability = BookAvailability.SanCo;

            Save();
            return (rec, fine);
        }

        public (BorrowRecord? record, string error) RenewLoan(string maPhieu, int additionalDays)
        {
            var rec = _store.Borrows.FirstOrDefault(r => r.MaPhieu == maPhieu);
            if (rec == null) return (null, "Không tìm thấy phiếu mượn.");
            if (rec.Status != BorrowStatus.DangMuon) return (null, "Chỉ có thể gia hạn phiếu đang mượn.");
            if (rec.RenewalCount >= _policy.MaxRenewals) return (null, "Đã đạt số lần gia hạn tối đa.");

            var due = DateTime.ParseExact(rec.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (due.Date < DateTime.Now.Date) return (null, "Không thể gia hạn khi sách đã quá hạn.");

            rec.RenewalCount += 1;
            rec.LoanDays += additionalDays;
            rec.DueDate = due.Date.AddDays(additionalDays).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            Save();
            return (rec, string.Empty);
        }

        public decimal CollectFine(string maPhieu, decimal? amount = null, string note = "Thu phạt")
        {
            var rec = _store.Borrows.FirstOrDefault(r => r.MaPhieu == maPhieu);
            if (rec == null || rec.FineAmount <= 0 || rec.FinePaid)
                return 0m;

            var paymentAmount = amount ?? rec.FineAmount;
            rec.FinePaid = true;
            rec.FinePaidDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var payment = new PaymentRecord
            {
                Id = GenerateId(),
                BorrowId = rec.Id,
                BorrowMaPhieu = rec.MaPhieu,
                Amount = paymentAmount,
                PaidDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Note = note,
                ReceiptNumber = GenerateReceiptNumber(DateTime.Now)
            };
            _store.Payments.Add(payment);
            Save();
            return paymentAmount;
        }

        public IEnumerable<PaymentRecord> GetPaymentsForBorrow(string borrowId)
            => _store.Payments.Where(p => p.BorrowId == borrowId).OrderByDescending(p => p.PaidDate);

        public IEnumerable<PaymentRecord> GetPaymentHistory(DateTime from, DateTime to)
            => _store.Payments.Where(p => DateTime.TryParse(p.PaidDate, out var d) && d.Date >= from.Date && d.Date <= to.Date);

        public decimal TotalFinesCollected(DateTime from, DateTime to)
            => _store.Payments.Where(p => DateTime.TryParse(p.PaidDate, out var d) && d.Date >= from.Date && d.Date <= to.Date)
                              .Sum(p => p.Amount);

        public IEnumerable<MonthlyBorrowPoint> MonthlyBorrowCounts(int monthsBack = 6)
        {
            var now = DateTime.Now;
            var list = new List<MonthlyBorrowPoint>();
            for (int i = monthsBack - 1; i >= 0; i--)
            {
                var dt = now.AddMonths(-i);
                var count = _store.Borrows.Count(h => DateTime.ParseExact(h.BorrowDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Year == dt.Year
                                                   && DateTime.ParseExact(h.BorrowDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).Month == dt.Month);
                list.Add(new MonthlyBorrowPoint { Month = dt.ToString("MMM yyyy"), Count = count });
            }
            return list;
        }

        public IEnumerable<TopBookRecord> TopBooks(int topN = 10)
            => _store.Borrows.GroupBy(h => h.BookTitle)
                .Select(g => new TopBookRecord { Title = g.Key, Author = g.FirstOrDefault()?.Author ?? string.Empty, Borrows = g.Count(), Rank = 0 })
                .OrderByDescending(t => t.Borrows)
                .Take(topN)
                .Select((r, idx) => { r.Rank = idx + 1; return r; });

        private void EnsureSampleData()
        {
            if (!_store.Readers.Any())
            {
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Nguyễn An", Email = "an@example.com", CardNumber = "DG-10234", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Lê Thu", Email = "thu@example.com", CardNumber = "DG-09112", CardType = CardType.GiaoVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-2).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Phạm Hùng", Email = "hung@example.com", CardNumber = "DG-08890", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Trần Minh", Email = "minh@example.com", CardNumber = "DG-07656", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Hoàng Liên", Email = "lien@example.com", CardNumber = "DG-06543", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-5).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Bùi Văn Sơn", Email = "son@example.com", CardNumber = "DG-05432", CardType = CardType.GiaoVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-6).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Dương Khánh", Email = "khanh@example.com", CardNumber = "DG-04321", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-7).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Võ Thị Hương", Email = "huong@example.com", CardNumber = "DG-03210", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-8).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Nơi Tân Phát", Email = "tanphat@example.com", CardNumber = "DG-02109", CardType = CardType.GiaoVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-9).ToString("yyyy-MM-dd") });
                _store.Readers.Add(new ReaderRecord { Id = GenerateId(), Name = "Tạ Quý Tâm", Email = "quytam@example.com", CardNumber = "DG-01098", CardType = CardType.SinhVien, Status = ReaderStatus.HoatDong, RegDate = DateTime.Now.AddMonths(-10).ToString("yyyy-MM-dd") });
            }

            if (!_store.Books.Any())
            {
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 1, Title = "Clean Code", Author = "Robert C. Martin", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-6), CoverImagePath = "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 2, Title = "Sapiens", Author = "Yuval Noah Harari", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-5), CoverImagePath = "https://images.unsplash.com/photo-1512820790803-83ca734da794?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 3, Title = "Design Patterns", Author = "Erich Gamma", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-4), CoverImagePath = "https://images.unsplash.com/photo-1516979187457-637abb4f9353?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 4, Title = "The Pragmatic Programmer", Author = "David Thomas", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-3), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 5, Title = "Code Complete", Author = "Steve McConnell", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-2), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 6, Title = "The Mythical Man-Month", Author = "Frederick Brooks", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddMonths(-1), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 7, Title = "Introduction to Algorithms", Author = "Thomas H. Cormen", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-30), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 8, Title = "Refactoring", Author = "Martin Fowler", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-25), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 9, Title = "The C Programming Language", Author = "Brian W. Kernighan", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-20), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 10, Title = "Database Design", Author = "C.J. Date", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-15), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 11, Title = "Web Development with Node.js", Author = "Tom Hughes-Croucher", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-10), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
                _store.Books.Add(new BookRecord { Id = GenerateId(), Stt = 12, Title = "Learning JavaScript", Author = "Ethan Brown", Availability = BookAvailability.SanCo, AddedDate = DateTime.Now.AddDays(-5), CoverImagePath = "https://images.unsplash.com/photo-1507842217343-583f7270bfba?auto=format&fit=crop&w=256&q=80" });
            }

            if (!_store.Borrows.Any())
            {
                var reader1 = _store.Readers[0];
                var reader2 = _store.Readers[1];
                var reader3 = _store.Readers[2];
                var book1 = _store.Books[0];
                var book2 = _store.Books[1];
                var book3 = _store.Books[2];

                var borrow1 = LendBook(reader1, book1, DateTime.Now.AddDays(-16), 14);
                var borrow2 = LendBook(reader2, book2, DateTime.Now.AddDays(-10), 14);
                var overdueBorrow = LendBook(reader3, book3, DateTime.Now.AddDays(-40), 14);
                var returned = ReturnBook(overdueBorrow.MaPhieu, DateTime.Now.AddDays(-3));
                if (returned.record != null)
                {
                    var index = _store.Borrows.FindIndex(b => b.Id == returned.record.Id);
                    if (index >= 0)
                        _store.Borrows[index] = returned.record;
                }
                Save();
            }
        }

        private void EnsureBookExists(BookRecord book)
        {
            if (string.IsNullOrWhiteSpace(book.Id)) book.Id = GenerateId();
            if (!_store.Books.Any(b => b.Id == book.Id))
                _store.Books.Add(book);
        }

        private void EnsureReaderExists(ReaderRecord reader)
        {
            if (string.IsNullOrWhiteSpace(reader.Id)) reader.Id = GenerateId();
            if (!_store.Readers.Any(r => r.Id == reader.Id))
                _store.Readers.Add(reader);
        }

        private void Save()
            => LibraryDataStoreFile.Save(_store);

        private static string GenerateMaPhieu(DateTime dt)
            => $"P{dt:yyyyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 4).ToUpperInvariant()}";

        private static string GenerateId() => Guid.NewGuid().ToString("N");

        private static string GenerateReceiptNumber(DateTime dt)
            => $"RC{dt:yyyyMMddHHmmss}";
    }
}
