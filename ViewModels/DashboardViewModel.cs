using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        // ── Metric Cards ────────────────────────────────────────────────
        public string TotalBooks        { get; } = "12,482";
        public string BorrowedBooks     { get; } = "856";
        public string ActiveReaders     { get; } = "3,120";
        public string OverdueBooks      { get; } = "42";

        public string TotalBooksBadge   { get; } = "+2.5%";
        public string BorrowedBadge     { get; } = "+1.8%";
        public string ActiveBadge       { get; } = "0%";
        public string OverdueBadge      { get; } = "+5%";

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
            ChartItems = new ObservableCollection<BarChartItem>();
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

            // ── Sample category data ──────────────────────────────────────
            Categories = new ObservableCollection<CategoryItem>
            {
                new CategoryItem { Name="Khoa học Viễn tưởng", Count=3_201, Percentage=0.42, BarColor=new SolidColorBrush(Color.FromRgb(0x06,0xB6,0xD4)) },
                new CategoryItem { Name="Tiểu sử",             Count=2_140, Percentage=0.28, BarColor=new SolidColorBrush(Color.FromRgb(0x13,0x5B,0xEC)) },
                new CategoryItem { Name="Công nghệ",           Count=1_550, Percentage=0.18, BarColor=new SolidColorBrush(Color.FromRgb(0xA8,0x55,0xF7)) },
            };

            // ── Sample transaction data ───────────────────────────────────
            Transactions = new ObservableCollection<TransactionRecord>
            {
                new TransactionRecord
                {
                    BookTitle = "Dune",
                    Genre     = "Khoa học Viễn tưởng",
                    Reader    = "Nguyễn Thị B",
                    DueDate   = "15/05/2025",
                    Status    = TransactionStatus.DaTra,
                },
                new TransactionRecord
                {
                    BookTitle = "Atomic Habits",
                    Genre     = "Tâm lý học",
                    Reader    = "Trần Văn C",
                    DueDate   = "22/05/2025",
                    Status    = TransactionStatus.DangMuon,
                },
                new TransactionRecord
                {
                    BookTitle = "Clean Code",
                    Genre     = "Công nghệ",
                    Reader    = "Lê Thị D",
                    DueDate   = "30/04/2025",
                    Status    = TransactionStatus.QuaHan,
                },
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
