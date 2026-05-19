using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using LibraryManagementFE.Models;
using LibraryManagementFE.Policies;
using LibraryManagementFE.Services;
using LibraryManagementFE.Views;

namespace LibraryManagementFE.ViewModels
{
    public class BorrowReturnViewModel : INotifyPropertyChanged
    {
        private readonly BorrowService _service;
        private readonly LibraryPolicy _policy;
        private string _searchText = string.Empty;
        private string _statusFilter = "Tất cả";
        private bool _showOverdueOnly;
        private bool _showUnpaidOnly;
        private bool _showLateReturnOnly;
        private bool _showFilterPanel;
        private int _currentPage = 1;
        private int _pageSize = 12;
        private int _totalPages = 1;

        public ObservableCollection<BorrowRecord> Borrows { get; } = new();
        public ObservableCollection<BorrowRecord> FilteredBorrows { get; } = new();
        public ObservableCollection<string> StatusFilters { get; } = new ObservableCollection<string> { "Tất cả", "Đang mượn", "Quá hạn", "Đã trả" };
        public ObservableCollection<MonthlyBorrowPoint> BorrowTrend { get; } = new();
        public ObservableCollection<BookRecord> AvailableBooks { get; } = new();
        public ObservableCollection<ReaderRecord> Readers { get; } = new();
        public ObservableCollection<PaymentRecord> SelectedPaymentHistory { get; } = new();
        public ObservableCollection<TopBookRecord> TopBooks { get; } = new();

        public ICommand LendCommand { get; }
        public ICommand ReturnCommand { get; }
        public ICommand ReturnSelectedCommand { get; }
        public ICommand CollectFineCommand { get; }
        public ICommand RenewLoanCommand { get; }
        public ICommand ToggleFilterPanelCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }

        private BorrowRecord? _selectedBorrow;
        public BorrowRecord? SelectedBorrow
        {
            get => _selectedBorrow;
            set
            {
                if (_selectedBorrow == value) return;
                _selectedBorrow = value;
                OnPropertyChanged();
                UpdateSelectedPaymentHistory();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (_statusFilter == value) return;
                _statusFilter = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public bool ShowOverdueOnly
        {
            get => _showOverdueOnly;
            set
            {
                if (_showOverdueOnly == value) return;
                _showOverdueOnly = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public bool ShowUnpaidOnly
        {
            get => _showUnpaidOnly;
            set
            {
                if (_showUnpaidOnly == value) return;
                _showUnpaidOnly = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public bool ShowLateReturnOnly
        {
            get => _showLateReturnOnly;
            set
            {
                if (_showLateReturnOnly == value) return;
                _showLateReturnOnly = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public bool ShowFilterPanel
        {
            get => _showFilterPanel;
            set
            {
                if (_showFilterPanel == value) return;
                _showFilterPanel = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                OnPropertyChanged();
                RefreshFilteredBorrows();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize == value) return;
                _pageSize = value;
                OnPropertyChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            private set
            {
                if (_totalPages == value) return;
                _totalPages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
            }
        }

        public string PageInfo => $"{CurrentPage}/{TotalPages}";
        public string BorrowSummary => $"Hiển thị {FilteredBorrows.Count} trên tổng số {Borrows.Count} giao dịch";
        public int ActiveBorrowCount => Borrows.Count(b => b.Status == BorrowStatus.DangMuon);
        public int ReturnedThisMonthCount => Borrows.Count(b => (b.Status == BorrowStatus.DaTraTot || b.Status == BorrowStatus.DaTraTre) && ParseDate(b.ReturnDate) is DateTime returned && returned.Year == DateTime.Now.Year && returned.Month == DateTime.Now.Month);
        public int OverdueCount => Borrows.Count(b => b.Status == BorrowStatus.QuaHan);
        public int DueSoonCount => Borrows.Count(b => b.Status == BorrowStatus.DangMuon && GetDueDate(b) <= DateTime.Now.AddDays(5));
        public decimal TotalFineCollected => _service.TotalFinesCollected(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now);
        public string TotalFineCollectedDisplay => string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalFineCollected);
        public string FinePeriodLabel => $"Phạt thu tháng {DateTime.Now:MM/yyyy}";
        public decimal TotalOutstandingFine => Borrows.Where(b => b.FineAmount > 0 && !b.FinePaid).Sum(b => b.FineAmount);
        public string TotalOutstandingFineDisplay => string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalOutstandingFine);
        public string PolicySummary => $"Quy định: {_policy.MaxBooksPerReader} sách/người • {_policy.MaxLoanDays} ngày • {_policy.MaxRenewals} lần gia hạn • {_policy.PenaltyPerDay:N0} đ/ngày";
        public string PolicyStatus => _policy.NotBorrowWhenCardLocked ? "Thẻ phải hoạt động để mượn" : "Không giới hạn trạng thái thẻ";

        public BorrowReturnViewModel()
        {
            _policy = LibraryPolicyStore.LoadOrCreate();
            _service = new BorrowService(_policy);
            LoadMasterData();

            LendCommand = new RelayCommand(_ => ExecuteLendCommand());
            ReturnCommand = new RelayCommand(p => ExecuteReturnByPhieu(p as string));
            ReturnSelectedCommand = new RelayCommand(_ => ExecuteReturnSelected());
            CollectFineCommand = new RelayCommand(p => ExecuteCollectFine(p as string));
            RenewLoanCommand = new RelayCommand(_ => ExecuteRenewLoan());
            ToggleFilterPanelCommand = new RelayCommand(_ => ShowFilterPanel = !ShowFilterPanel);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage = Math.Max(1, CurrentPage - 1), _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => CurrentPage = Math.Min(TotalPages, CurrentPage + 1), _ => CurrentPage < TotalPages);

            RefreshBorrowTrend();
            RefreshFilteredBorrows();
        }

        private void LoadMasterData()
        {
            Borrows.Clear();
            foreach (var borrow in _service.Borrows)
                Borrows.Add(borrow);

            AvailableBooks.Clear();
            foreach (var book in _service.GetAvailableBooks())
                AvailableBooks.Add(book);

            Readers.Clear();
            foreach (var reader in _service.GetReaders())
                Readers.Add(reader);

            SelectedPaymentHistory.Clear();
        }

        private void ExecuteLendCommand()
        {
            // Refresh dữ liệu từ service trước khi mở dialog
            LoadMasterData();
            
            var window = new LendBorrowWindow
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
                AvailableBooks = AvailableBooks,
                Readers = Readers
            };

            if (window.ShowDialog() != true)
                return;

            try
            {
                if (window.SelectedReader == null || window.SelectedBook == null)
                {
                    MessageBox.Show("Vui lòng chọn độc giả và sách.", "Lỗi mượn sách", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rec = _service.LendBook(window.SelectedReader, window.SelectedBook, window.BorrowDate, window.LoanDays);
                Borrows.Add(rec);
                LoadMasterData();
                OnBorrowsChanged();
                ResetPaging();
                RefreshFilteredBorrows();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi mượn sách", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteReturnSelected()
        {
            if (SelectedBorrow == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu mượn để trả sách.", "Xác nhận trả sách", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ExecuteReturnByPhieu(SelectedBorrow.MaPhieu);
            LoadMasterData();
        }

        private void ExecuteReturnByPhieu(string? maPhieu)
        {
            if (string.IsNullOrWhiteSpace(maPhieu)) return;
            var (rec, fine) = _service.ReturnBook(maPhieu);
            if (rec == null) return;

            var idx = Borrows.IndexOf(Borrows.First(b => b.MaPhieu == rec.MaPhieu));
            if (idx >= 0) Borrows[idx] = rec;
            SelectedBorrow = rec;

            if (fine > 0)
            {
                MessageBox.Show($"Phiếu trả trễ. Phạt: {fine:N0} đ. Vui lòng thu phạt khi đã trả sách.", "Trả sách", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            RefreshAfterUpdate();
            RefreshTopBooks();
        }

        private void ExecuteCollectFine(string? maPhieu)
        {
            if (string.IsNullOrWhiteSpace(maPhieu)) return;

            var amount = _service.CollectFine(maPhieu);
            if (amount <= 0) return;

            MessageBox.Show($"Đã thu phạt {amount:N0} đ.", "Thu tiền phạt", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshAfterUpdate();
        }

        private void ExecuteRenewLoan()
        {
            if (SelectedBorrow == null)
            {
                MessageBox.Show("Vui lòng chọn một phiếu mượn để gia hạn.", "Gia hạn mượn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (rec, error) = _service.RenewLoan(SelectedBorrow.MaPhieu, _policy.MaxLoanDays);
            if (rec == null)
            {
                MessageBox.Show(error, "Gia hạn mượn", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Gia hạn mượn thành công.", "Gia hạn mượn", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectedBorrow = rec;
            RefreshAfterUpdate();
        }

        private void RefreshAfterUpdate()
        {
            LoadMasterData();
            RefreshTopBooks();
            OnBorrowsChanged();
            RefreshFilteredBorrows();
        }

        private void RefreshTopBooks()
        {
            TopBooks.Clear();
            foreach (var book in _service.TopBooks(5))
                TopBooks.Add(book);
            OnPropertyChanged(nameof(TopBooks));
        }

        private void OnBorrowsChanged()
        {
            OnPropertyChanged(nameof(ActiveBorrowCount));
            OnPropertyChanged(nameof(ReturnedThisMonthCount));
            OnPropertyChanged(nameof(OverdueCount));
            OnPropertyChanged(nameof(DueSoonCount));
            OnPropertyChanged(nameof(BorrowSummary));
            OnPropertyChanged(nameof(TotalFineCollected));
            OnPropertyChanged(nameof(TotalFineCollectedDisplay));
            OnPropertyChanged(nameof(TotalOutstandingFine));
            OnPropertyChanged(nameof(TotalOutstandingFineDisplay));
            OnPropertyChanged(nameof(FinePeriodLabel));
            RefreshBorrowTrend();
        }

        private static DateTime GetDueDate(BorrowRecord record)
        {
            return DateTime.TryParse(record.DueDate, out var due) ? due : DateTime.MaxValue;
        }

        private static DateTime? ParseDate(string date)
        {
            return DateTime.TryParse(date, out var result) ? result : (DateTime?)null;
        }

        private void ResetPaging()
        {
            CurrentPage = 1;
        }

        private void UpdatePaging(int totalItems)
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;
            OnPropertyChanged(nameof(PageInfo));
        }

        private void RefreshFilteredBorrows()
        {
            UpdateOverdueStatuses();
            FilteredBorrows.Clear();
            var query = Borrows.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var keyword = SearchText.Trim();
                query = query.Where(b => b.MaPhieu.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                       || b.ReaderName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                       || b.BookTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            if (StatusFilter != "Tất cả")
            {
                query = StatusFilter switch
                {
                    "Đang mượn" => query.Where(b => b.Status == BorrowStatus.DangMuon),
                    "Quá hạn" => query.Where(b => b.Status == BorrowStatus.QuaHan),
                    "Đã trả" => query.Where(b => b.Status == BorrowStatus.DaTraTot || b.Status == BorrowStatus.DaTraTre),
                    _ => query
                };
            }

            if (ShowOverdueOnly)
                query = query.Where(b => b.Status == BorrowStatus.QuaHan);

            if (ShowUnpaidOnly)
                query = query.Where(b => b.FineAmount > 0 && !b.FinePaid);

            if (ShowLateReturnOnly)
                query = query.Where(b => b.Status == BorrowStatus.DaTraTre);

            var allFiltered = query.ToList();
            UpdatePaging(allFiltered.Count);
            foreach (var borrow in allFiltered.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
                FilteredBorrows.Add(borrow);

            OnPropertyChanged(nameof(FilteredBorrows));
            OnPropertyChanged(nameof(BorrowSummary));
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PageInfo));
        }

        private void UpdateSelectedPaymentHistory()
        {
            SelectedPaymentHistory.Clear();
            if (SelectedBorrow == null) return;
            foreach (var payment in _service.GetPaymentsForBorrow(SelectedBorrow.Id))
                SelectedPaymentHistory.Add(payment);
            OnPropertyChanged(nameof(SelectedPaymentHistory));
        }

        private void RefreshBorrowTrend()
        {
            BorrowTrend.Clear();
            var items = _service.MonthlyBorrowCounts(6).ToList();
            var max = items.Count == 0 ? 1 : items.Max(i => i.Count);
            foreach (var item in items)
            {
                item.RelativeHeight = max <= 0 ? 0.1 : item.Count / (double)max;
                BorrowTrend.Add(item);
            }
            OnPropertyChanged(nameof(BorrowTrend));
        }

        private void ReturnBookByPhieu(string maPhieu)
        {
            ExecuteReturnByPhieu(maPhieu);
        }

        private void UpdateOverdueStatuses()
        {
            foreach (var record in Borrows.Where(b => b.Status == BorrowStatus.DangMuon && GetDueDate(b) < DateTime.Now))
            {
                record.Status = BorrowStatus.QuaHan;
            }
        }

        public decimal GetTotalFines(DateTime from, DateTime to) => _service.TotalFinesCollected(from, to);
        public System.Collections.Generic.IEnumerable<MonthlyBorrowPoint> GetMonthlyBorrowPoints(int monthsBack) => _service.MonthlyBorrowCounts(monthsBack);
        public System.Collections.Generic.IEnumerable<TopBookRecord> GetTopBooks(int topN) => _service.TopBooks(topN);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
