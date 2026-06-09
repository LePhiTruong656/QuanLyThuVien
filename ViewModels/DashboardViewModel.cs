using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LibraryManagementFE.Models;
using LibraryManagementFE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagementFE.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly LibraryDbContext _context;

        // ── Metric Cards ────────────────────────────────────────────────
        private string _totalBooks = "0";
        private string _borrowedBooks = "0";
        private string _activeReaders = "0";
        private string _overdueBooks = "0";

        private string _totalBooksBadge = "0%";
        private string _borrowedBadge = "0%";
        private string _activeBadge = "0%";
        private string _overdueBadge = "0%";

        public string TotalBooks
        {
            get => _totalBooks;
            set { _totalBooks = value; OnPropertyChanged(); }
        }

        public string BorrowedBooks
        {
            get => _borrowedBooks;
            set { _borrowedBooks = value; OnPropertyChanged(); }
        }

        public string ActiveReaders
        {
            get => _activeReaders;
            set { _activeReaders = value; OnPropertyChanged(); }
        }

        public string OverdueBooks
        {
            get => _overdueBooks;
            set { _overdueBooks = value; OnPropertyChanged(); }
        }

        public string TotalBooksBadge
        {
            get => _totalBooksBadge;
            set { _totalBooksBadge = value; OnPropertyChanged(); }
        }

        public string BorrowedBadge
        {
            get => _borrowedBadge;
            set { _borrowedBadge = value; OnPropertyChanged(); }
        }

        public string ActiveBadge
        {
            get => _activeBadge;
            set { _activeBadge = value; OnPropertyChanged(); }
        }

        public string OverdueBadge
        {
            get => _overdueBadge;
            set { _overdueBadge = value; OnPropertyChanged(); }
        }

        // ── Bar Chart ───────────────────────────────────────────────────
        public ObservableCollection<BarChartItem> ChartItems { get; }

        // ── Category Breakdown ──────────────────────────────────────────
        public ObservableCollection<CategoryItem> Categories { get; }

        // ── Transactions Table ──────────────────────────────────────────
        public ObservableCollection<TransactionRecord> Transactions { get; }

        // ── Selected nav item (for sidebar active state) ────────────────
        private string _activeNav = "dashboard";
        public string ActiveNav
        {
            get => _activeNav;
            set { _activeNav = value; OnPropertyChanged(); }
        }

        public DashboardViewModel()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
            _context = new LibraryDbContext(optionsBuilder.Options);

            ChartItems = new ObservableCollection<BarChartItem>();
            Categories = new ObservableCollection<CategoryItem>();
            Transactions = new ObservableCollection<TransactionRecord>();

            // Load data synchronously in constructor
            LoadDashboardDataSync();
        }

        private void LoadDashboardDataSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadDashboardDataSync: Starting...");

                // Card 1: Tổng số sách
                var totalBooksCount = _context.Books.Count();
                TotalBooks = totalBooksCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Total Books: {TotalBooks}");

                // Card 2: Đang được mượn (sách có Availability = DangMuon)
                var borrowedCount = _context.Books
                    .Where(b => b.Availability == BookAvailability.DangMuon)
                    .Count();
                BorrowedBooks = borrowedCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Borrowed Books: {BorrowedBooks}");

                // Card 3: Độc giả đang hoạt động (Status = HoatDong)
                var activeReadersCount = _context.Readers
                    .Where(r => r.Status == ReaderStatus.HoatDong)
                    .Count();
                ActiveReaders = activeReadersCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Active Readers: {ActiveReaders}");

                // Card 4: Sách trả trễ (BorrowStatus = DaTraTre)
                var overdueCount = _context.Borrows
                    .Where(b => b.Status == BorrowStatus.DaTraTre)
                    .Count();
                OverdueBooks = overdueCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Overdue Books: {OverdueBooks}");

                // Calculate badges (compare with last month)
                var now = DateTime.Now;
                var lastMonthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                var lastMonthEnd = new DateTime(now.Year, now.Month, 1).AddDays(-1);

                // Calculate percentage changes
                TotalBooksBadge = "+2.5%";  // Placeholder - would need historical data
                BorrowedBadge = "+1.8%";    // Placeholder
                ActiveBadge = "0%";          // Placeholder
                OverdueBadge = "+5%";        // Placeholder

                System.Diagnostics.Debug.WriteLine("LoadDashboardDataSync: Loading chart data...");
                // Load chart data
                LoadChartData();

                System.Diagnostics.Debug.WriteLine("LoadDashboardDataSync: Loading category data...");
                // Load category data
                LoadCategoryDataSync();

                System.Diagnostics.Debug.WriteLine("LoadDashboardDataSync: Loading transactions...");
                // Load recent transactions
                LoadTransactionsSync();

                System.Diagnostics.Debug.WriteLine("LoadDashboardDataSync: Completed successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private async void LoadDashboardData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadDashboardData: Starting...");

                // Card 1: Tổng số sách
                var totalBooksCount = await _context.Books.CountAsync();
                TotalBooks = totalBooksCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Total Books: {TotalBooks}");

                // Card 2: Đang được mượn (sách có Availability = DangMuon)
                var borrowedCount = await _context.Books
                    .Where(b => b.Availability == BookAvailability.DangMuon)
                    .CountAsync();
                BorrowedBooks = borrowedCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Borrowed Books: {BorrowedBooks}");

                // Card 3: Độc giả đang hoạt động (Status = HoatDong)
                var activeReadersCount = await _context.Readers
                    .Where(r => r.Status == ReaderStatus.HoatDong)
                    .CountAsync();
                ActiveReaders = activeReadersCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Active Readers: {ActiveReaders}");

                // Card 4: Sách trả trễ (BorrowStatus = DaTraTre)
                var overdueCount = await _context.Borrows
                    .Where(b => b.Status == BorrowStatus.DaTraTre)
                    .CountAsync();
                OverdueBooks = overdueCount.ToString("N0");
                System.Diagnostics.Debug.WriteLine($"Overdue Books: {OverdueBooks}");

                // Calculate badges (compare with last month)
                var now = DateTime.Now;
                var lastMonthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                var lastMonthEnd = new DateTime(now.Year, now.Month, 1).AddDays(-1);

                // Calculate percentage changes
                TotalBooksBadge = "+2.5%";  // Placeholder - would need historical data
                BorrowedBadge = "+1.8%";    // Placeholder
                ActiveBadge = "0%";          // Placeholder
                OverdueBadge = "+5%";        // Placeholder

                // Load chart data
                LoadChartData();

                // Load category data
                await LoadCategoryData();

                // Load recent transactions
                await LoadTransactions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private void LoadChartData()
        {
            // ── Sample bar chart data (10 months, relative heights from Figma) ──
            var rawValues = new[]
            {
                (Month: "T1", Val: 89.59),
                (Month: "T2", Val: 134.39),
                (Month: "T3", Val: 190.39),
                (Month: "T4", Val: 100.80),
                (Month: "T5", Val: 156.80),
                (Month: "T6", Val: 123.19),
                (Month: "T7", Val: 212.80),
                (Month: "T8", Val: 145.59),
                (Month: "T9", Val: 168.00),
                (Month: "T10",Val: 112.00),
            };

            // Accent the tallest bar with Brand500, others with shades of blue
            var fills = new Brush[]
            {
                new SolidColorBrush(Color.FromRgb(0xEF,0xF6,0xFF)), // lightest
                new SolidColorBrush(Color.FromRgb(0xDB,0xEA,0xFE)),
                new SolidColorBrush(Color.FromRgb(0x13,0x5B,0xEC)), // accent (T3)
                new SolidColorBrush(Color.FromRgb(0xBF,0xDB,0xFE)),
                new SolidColorBrush(Color.FromRgb(0x93,0xC5,0xFD)),
                new SolidColorBrush(Color.FromRgb(0x60,0xA5,0xFA)),
                new SolidColorBrush(Color.FromRgb(0xDB,0xEA,0xFE)),
                new SolidColorBrush(Color.FromRgb(0xBF,0xDB,0xFE)),
                new SolidColorBrush(Color.FromRgb(0x13,0x5B,0xEC)), // accent (T9)
                new SolidColorBrush(Color.FromRgb(0x93,0xC5,0xFD)),
            };

            double maxVal = 212.80;
            ChartItems.Clear();
            for (int i = 0; i < rawValues.Length; i++)
            {
                ChartItems.Add(new BarChartItem
                {
                    MonthLabel     = rawValues[i].Month,
                    Value          = rawValues[i].Val,
                    RelativeHeight = rawValues[i].Val / maxVal,
                    Fill           = fills[i]
                });
            }
        }

        private void LoadCategoryDataSync()
        {
            try
            {
                // Group books by CategoryLine1 and count
                var categoryStats = _context.Books
                    .GroupBy(b => b.CategoryLine1)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .ToList();

                var total = categoryStats.Sum(x => x.Count);
                var colors = new[]
                {
                    new SolidColorBrush(Color.FromRgb(0x06, 0xB6, 0xD4)),
                    new SolidColorBrush(Color.FromRgb(0x13, 0x5B, 0xEC)),
                    new SolidColorBrush(Color.FromRgb(0xA8, 0x55, 0xF7))
                };

                Categories.Clear();
                for (int i = 0; i < categoryStats.Count; i++)
                {
                    var stat = categoryStats[i];
                    Categories.Add(new CategoryItem
                    {
                        Name = string.IsNullOrWhiteSpace(stat.Name) ? "Chưa phân loại" : stat.Name,
                        Count = stat.Count,
                        Percentage = total > 0 ? (double)stat.Count / total : 0,
                        BarColor = colors[i % colors.Length]
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category data: {ex.Message}");
            }
        }

        private void LoadTransactionsSync()
        {
            try
            {
                // Get 10 most recent borrow records
                var recentBorrows = _context.Borrows
                    .OrderByDescending(b => b.BorrowDate)
                    .Take(10)
                    .ToList();

                Transactions.Clear();
                foreach (var borrow in recentBorrows)
                {
                    var book = _context.Books.FirstOrDefault(b => b.Id == borrow.BookId);

                    Transactions.Add(new TransactionRecord
                    {
                        BookTitle = borrow.BookTitle,
                        Genre = book?.CategoryLine1 ?? "Chưa phân loại",
                        Reader = borrow.ReaderName,
                        DueDate = borrow.DueDate,
                        Status = MapBorrowStatusToTransactionStatus(borrow.Status)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading transactions: {ex.Message}");
            }
        }

        private async Task LoadCategoryData()
        {
            try
            {
                // Group books by CategoryLine1 and count
                var categoryStats = await _context.Books
                    .GroupBy(b => b.CategoryLine1)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .ToListAsync();

                var total = categoryStats.Sum(x => x.Count);
                var colors = new[]
                {
                    new SolidColorBrush(Color.FromRgb(0x06, 0xB6, 0xD4)),
                    new SolidColorBrush(Color.FromRgb(0x13, 0x5B, 0xEC)),
                    new SolidColorBrush(Color.FromRgb(0xA8, 0x55, 0xF7))
                };

                Categories.Clear();
                for (int i = 0; i < categoryStats.Count; i++)
                {
                    var stat = categoryStats[i];
                    Categories.Add(new CategoryItem
                    {
                        Name = string.IsNullOrWhiteSpace(stat.Name) ? "Chưa phân loại" : stat.Name,
                        Count = stat.Count,
                        Percentage = total > 0 ? (double)stat.Count / total : 0,
                        BarColor = colors[i % colors.Length]
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category data: {ex.Message}");
            }
        }

        private async Task LoadTransactions()
        {
            try
            {
                // Get 10 most recent borrow records
                var recentBorrows = await _context.Borrows
                    .OrderByDescending(b => b.BorrowDate)
                    .Take(10)
                    .ToListAsync();

                Transactions.Clear();
                foreach (var borrow in recentBorrows)
                {
                    var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == borrow.BookId);

                    Transactions.Add(new TransactionRecord
                    {
                        BookTitle = borrow.BookTitle,
                        Genre = book?.CategoryLine1 ?? "Chưa phân loại",
                        Reader = borrow.ReaderName,
                        DueDate = borrow.DueDate,
                        Status = MapBorrowStatusToTransactionStatus(borrow.Status)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading transactions: {ex.Message}");
            }
        }

        private TransactionStatus MapBorrowStatusToTransactionStatus(BorrowStatus borrowStatus)
        {
            return borrowStatus switch
            {
                BorrowStatus.DangMuon => TransactionStatus.DangMuon,
                BorrowStatus.DaTraTot => TransactionStatus.DaTra,
                BorrowStatus.DaTraTre => TransactionStatus.DaTra,
                BorrowStatus.QuaHan => TransactionStatus.QuaHan,
                _ => TransactionStatus.DangMuon
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
