using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementFE.Models;
using LibraryManagementFE.Policies;
using LibraryManagementFE.Services;
using LibraryManagementFE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace LibraryManagementFE.Views
{
    public partial class ReportsStatisticsView : UserControl, INotifyPropertyChanged
    {
        private readonly BorrowService _service;
        private readonly LibraryDbContext _context;
        private DateTime? _fromDate;
        private DateTime? _toDate;

        // Report binding properties
        public ObservableCollection<MonthlyBorrowPoint> MonthlyStats { get; } = new();
        public ObservableCollection<TopBookRecord> TopBooksReport { get; } = new();
        public ObservableCollection<TopReaderRecord> TopReadersReport { get; } = new();
        public ObservableCollection<LateReturnRecord> LateReturnsReport { get; } = new();
        public ObservableCollection<CategoryBorrowStat> CategoryBorrowReport { get; } = new();

        private int _totalBorrows;
        private int _totalReturns;
        private int _activeBorrows;
        private int _overdueBorrows;
        private int _activeReaders;
        private decimal _totalFines;

        public int TotalBorrows { get => _totalBorrows; private set { if (_totalBorrows == value) return; _totalBorrows = value; OnPropertyChanged(); } }
        public int TotalReturns { get => _totalReturns; private set { if (_totalReturns == value) return; _totalReturns = value; OnPropertyChanged(); } }
        public int ActiveBorrows { get => _activeBorrows; private set { if (_activeBorrows == value) return; _activeBorrows = value; OnPropertyChanged(); } }
        public int OverdueBorrows { get => _overdueBorrows; private set { if (_overdueBorrows == value) return; _overdueBorrows = value; OnPropertyChanged(); } }
        public int ActiveReaders { get => _activeReaders; private set { if (_activeReaders == value) return; _activeReaders = value; OnPropertyChanged(); } }
        public decimal TotalFines { get => _totalFines; private set { if (_totalFines == value) return; _totalFines = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalFinesDisplay)); } }
        public string TotalFinesDisplay => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalFines);
        public string OnTimeReturnRateDisplay => TotalReturns == 0 ? "N/A" : string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:P1}", Math.Round(GetOnTimeReturnRate(), 3));

        public ReportsStatisticsView()
        {
            InitializeComponent();
            DataContext = this;
            _service = new BorrowService(LibraryPolicyStore.LoadOrCreate());

            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
            _context = new LibraryDbContext(optionsBuilder.Options);

            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;

            // Initialize report data bindings
            RefreshReportData();
        }

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate == value) return;
                _fromDate = value;
                OnPropertyChanged();
                RefreshReportData();
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (_toDate == value) return;
                _toDate = value;
                OnPropertyChanged();
                RefreshReportData();
            }
        }

        private double GetOnTimeReturnRate()
        {
            if (TotalReturns == 0) return 0.0;
            // Returned on time are returned records with FineAmount == 0
            var allRecords = _context.Borrows.ToList();
            var returnedOnTime = allRecords
                .Where(b => DateTime.TryParse(b.BorrowDate, out var bd) && FromDate.HasValue && ToDate.HasValue && bd.Date >= FromDate.Value.Date && bd.Date <= ToDate.Value.Date)
                .Count(b => (b.Status == Models.BorrowStatus.DaTraTot || b.Status == Models.BorrowStatus.DaTraTre) && b.FineAmount == 0);
            return (double)returnedOnTime / Math.Max(1, TotalReturns);
        }

        private void RefreshReportData()
        {
            if (!FromDate.HasValue || !ToDate.HasValue) return;

            var from = FromDate.Value;
            var to = ToDate.Value;

            // Fetch all records from database first, then filter in memory
            var allRecords = _context.Borrows.ToList();
            var records = allRecords
                .Where(b => DateTime.TryParse(b.BorrowDate, out var borrowDate) && borrowDate.Date >= from.Date && borrowDate.Date <= to.Date)
                .ToList();

            TotalBorrows = records.Count;
            ActiveBorrows = records.Count(b => b.Status == Models.BorrowStatus.DangMuon);
            TotalReturns = records.Count(b => b.Status == Models.BorrowStatus.DaTraTot || b.Status == Models.BorrowStatus.DaTraTre);
            OverdueBorrows = records.Count(b => b.Status == Models.BorrowStatus.QuaHan);
            ActiveReaders = _context.Readers.Count(r => r.Status == ReaderStatus.HoatDong);
            TotalFines = records.Where(b => b.Status == Models.BorrowStatus.DaTraTre).Sum(b => b.FineAmount);

            // Monthly stats - last 6 months
            MonthlyStats.Clear();
            var monthlyData = allRecords
                .Where(b => DateTime.TryParse(b.BorrowDate, out var bd) && bd >= DateTime.Now.AddMonths(-6))
                .GroupBy(b => DateTime.Parse(b.BorrowDate).ToString("yyyy-MM"))
                .Select(g => new MonthlyBorrowPoint
                {
                    Month = g.Key,
                    Count = g.Count(),
                    RelativeHeight = 0
                })
                .OrderBy(m => m.Month)
                .ToList();

            if (monthlyData.Any())
            {
                var maxCount = monthlyData.Max(m => m.Count);
                foreach (var m in monthlyData)
                {
                    m.RelativeHeight = maxCount > 0 ? (double)m.Count / maxCount : 0;
                    MonthlyStats.Add(m);
                }
            }

            // Top books
            TopBooksReport.Clear();
            var allBooks = _context.Books.ToList();
            var topBooks = allRecords
                .GroupBy(b => new { b.BookId, b.BookTitle, b.Author })
                .Select(g => new
                {
                    g.Key.BookId,
                    g.Key.BookTitle,
                    g.Key.Author,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            int rank = 1;
            foreach (var book in topBooks)
            {
                var bookRecord = allBooks.FirstOrDefault(b => b.Id == book.BookId);
                TopBooksReport.Add(new TopBookRecord
                {
                    Rank = rank++,
                    Title = book.BookTitle,
                    Author = book.Author,
                    Borrows = book.Count,
                    Category = bookRecord?.CategoryLine1 ?? "Chưa phân loại",
                    CatBg = bookRecord?.CategoryPillBg ?? "#EFF6FF",
                    CatFg = bookRecord?.CategoryPillFg ?? "#1978E5"
                });
            }

            // Top readers
            TopReadersReport.Clear();
            var topReaders = allRecords
                .GroupBy(b => new { b.ReaderId, b.ReaderName, b.CardNumber })
                .Select(g => new TopReaderRecord
                {
                    Name = g.Key.ReaderName,
                    CardNumber = g.Key.CardNumber,
                    Borrows = g.Count()
                })
                .OrderByDescending(r => r.Borrows)
                .Take(10)
                .ToList();

            rank = 1;
            foreach (var reader in topReaders)
            {
                reader.Rank = rank++;
                TopReadersReport.Add(reader);
            }

            // Late returns report
            LateReturnsReport.Clear();
            var lateReturns = allRecords
                .Where(b => b.Status == Models.BorrowStatus.DaTraTre &&
                           DateTime.TryParse(b.ReturnDate, out var rd) &&
                           DateTime.TryParse(b.DueDate, out var dd) &&
                           rd.Date >= from.Date && rd.Date <= to.Date)
                .OrderByDescending(b => DateTime.Parse(b.ReturnDate))
                .ToList();

            int stt = 1;
            foreach (var late in lateReturns)
            {
                if (DateTime.TryParse(late.ReturnDate, out var returnDate) &&
                    DateTime.TryParse(late.DueDate, out var dueDate))
                {
                    var daysLate = (returnDate.Date - dueDate.Date).Days;
                    LateReturnsReport.Add(new LateReturnRecord
                    {
                        Stt = stt++,
                        BookTitle = late.BookTitle,
                        BorrowDate = late.BorrowDate,
                        DaysLate = daysLate,
                        FineAmount = late.FineAmount,
                        ReaderName = late.ReaderName
                    });
                }
            }

            // Category borrow report (by month and category)
            CategoryBorrowReport.Clear();
            var categoryStats = records
                .Select(b =>
                {
                    var book = allBooks.FirstOrDefault(bk => bk.Id == b.BookId);
                    return new
                    {
                        Month = DateTime.Parse(b.BorrowDate).ToString("yyyy-MM"),
                        Category = book?.CategoryLine1 ?? "Chưa phân loại"
                    };
                })
                .GroupBy(x => new { x.Month, x.Category })
                .Select(g => new { g.Key.Month, g.Key.Category, Count = g.Count() })
                .ToList();

            var totalByMonth = categoryStats.GroupBy(x => x.Month).ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

            foreach (var stat in categoryStats.OrderBy(x => x.Month).ThenByDescending(x => x.Count))
            {
                var totalInMonth = totalByMonth.ContainsKey(stat.Month) ? totalByMonth[stat.Month] : 1;
                CategoryBorrowReport.Add(new CategoryBorrowStat
                {
                    Month = stat.Month,
                    Category = stat.Category,
                    BorrowCount = stat.Count,
                    Percentage = (double)stat.Count / totalInMonth
                });
            }

            OnPropertyChanged(nameof(MonthlyStats));
            OnPropertyChanged(nameof(TopBooksReport));
            OnPropertyChanged(nameof(TopReadersReport));
            OnPropertyChanged(nameof(LateReturnsReport));
            OnPropertyChanged(nameof(CategoryBorrowReport));
            OnPropertyChanged(nameof(TotalBorrows));
            OnPropertyChanged(nameof(TotalReturns));
            OnPropertyChanged(nameof(ActiveBorrows));
            OnPropertyChanged(nameof(OverdueBorrows));
            OnPropertyChanged(nameof(TotalFines));
            OnPropertyChanged(nameof(TotalFinesDisplay));
            OnPropertyChanged(nameof(OnTimeReturnRateDisplay));
        }

        private void ExportReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (FromDate == null || ToDate == null)
            {
                MessageBox.Show("Vui lòng chọn khoảng thời gian báo cáo.", "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FromDate > ToDate)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Lưu báo cáo thống kê",
                Filter = "CSV (Microsoft Excel)|*.csv|Excel XML 2003|*.xls",
                FileName = $"BaoCaoThongKe_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            try
            {
                var reportData = BuildReportData(FromDate.Value, ToDate.Value);
                var extension = Path.GetExtension(saveDialog.FileName).ToLowerInvariant();

                if (extension == ".xls")
                {
                    File.WriteAllText(saveDialog.FileName, BuildExcelXml(reportData), Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(saveDialog.FileName, BuildCsv(reportData), Encoding.UTF8);
                }

                MessageBox.Show($"Đã xuất báo cáo thành công: {saveDialog.FileName}", "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo:\n{ex.Message}", "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ReportData BuildReportData(DateTime from, DateTime to)
        {
            var allRecords = _context.Borrows.ToList();
            var allBooks = _context.Books.ToList();

            var records = allRecords
                .Where(b => DateTime.TryParse(b.BorrowDate, out var borrowDate) && borrowDate.Date >= from.Date && borrowDate.Date <= to.Date)
                .ToList();

            var totalBorrows = records.Count;
            var activeBorrows = records.Count(b => b.Status == Models.BorrowStatus.DangMuon);
            var returnedBorrows = records.Count(b => b.Status == Models.BorrowStatus.DaTraTot || b.Status == Models.BorrowStatus.DaTraTre);
            var overdueBorrows = records.Count(b => b.Status == Models.BorrowStatus.QuaHan);
            var totalFines = records.Where(b => b.Status == Models.BorrowStatus.DaTraTre).Sum(b => b.FineAmount);
            var totalOutstanding = allRecords.Where(b => b.Status == Models.BorrowStatus.QuaHan || b.Status == Models.BorrowStatus.DaTraTre).Sum(b => b.OutstandingFine);
            var totalFinesDisplay = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", totalFines);
            var totalOutstandingDisplay = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", totalOutstanding);

            var monthlyRows = allRecords
                .Where(b => DateTime.TryParse(b.BorrowDate, out var bd) && bd >= DateTime.Now.AddMonths(-6))
                .GroupBy(b => DateTime.Parse(b.BorrowDate).ToString("yyyy-MM"))
                .Select(g => new ReportRow { Cells = new[] { g.Key, g.Count().ToString(CultureInfo.InvariantCulture) } })
                .OrderBy(r => r.Cells[0])
                .ToList();

            var topBookRows = allRecords
                .GroupBy(b => new { b.BookId, b.BookTitle, b.Author })
                .Select(g => new { g.Key.BookTitle, g.Key.Author, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList()
                .Select((t, idx) => new ReportRow { Cells = new[] { (idx + 1).ToString(CultureInfo.InvariantCulture), t.BookTitle, t.Author, t.Count.ToString(CultureInfo.InvariantCulture) } })
                .ToList();

            var lateReturnRows = allRecords
                .Where(b => b.Status == Models.BorrowStatus.DaTraTre &&
                           DateTime.TryParse(b.ReturnDate, out var rd) &&
                           DateTime.TryParse(b.DueDate, out var dd) &&
                           rd.Date >= from.Date && rd.Date <= to.Date)
                .OrderByDescending(b => DateTime.Parse(b.ReturnDate))
                .Select((b, idx) =>
                {
                    if (DateTime.TryParse(b.ReturnDate, out var returnDate) &&
                        DateTime.TryParse(b.DueDate, out var dueDate))
                    {
                        var daysLate = (returnDate.Date - dueDate.Date).Days;
                        return new ReportRow { Cells = new[] { (idx + 1).ToString(), b.BookTitle, b.BorrowDate, daysLate.ToString(), b.FineAmount.ToString("N0") } };
                    }
                    return null;
                })
                .Where(x => x != null)
                .Cast<ReportRow>()
                .ToList();

            var categoryBorrowRows = records
                .Select(b =>
                {
                    var book = allBooks.FirstOrDefault(bk => bk.Id == b.BookId);
                    return new
                    {
                        Month = DateTime.Parse(b.BorrowDate).ToString("yyyy-MM"),
                        Category = book?.CategoryLine1 ?? "Chưa phân loại"
                    };
                })
                .GroupBy(x => new { x.Month, x.Category })
                .Select(g => new { g.Key.Month, g.Key.Category, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ThenByDescending(x => x.Count)
                .Select(x =>
                {
                    var totalInMonth = records
                        .Where(b => DateTime.TryParse(b.BorrowDate, out var bd) && bd.ToString("yyyy-MM") == x.Month)
                        .Count();
                    var percentage = totalInMonth > 0 ? ((double)x.Count / totalInMonth * 100) : 0;
                    return new ReportRow { Cells = new[] { x.Month, x.Category, x.Count.ToString(), percentage.ToString("F1") } };
                })
                .ToList();

            return new ReportData
            {
                Title = "Báo cáo thống kê thư viện",
                DateRange = $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}",
                SummaryRows = new[]
                {
                    new ReportRow { Cells = new[] { "Tổng số phiếu mượn", totalBorrows.ToString(CultureInfo.InvariantCulture) } },
                    new ReportRow { Cells = new[] { "Đang mượn", activeBorrows.ToString(CultureInfo.InvariantCulture) } },
                    new ReportRow { Cells = new[] { "Đã trả", returnedBorrows.ToString(CultureInfo.InvariantCulture) } },
                    new ReportRow { Cells = new[] { "Quá hạn", overdueBorrows.ToString(CultureInfo.InvariantCulture) } },
                    new ReportRow { Cells = new[] { "Tiền phạt đã thu", totalFinesDisplay } },
                    new ReportRow { Cells = new[] { "Tổng tiền phạt chưa thu", totalOutstandingDisplay } }
                },
                MonthlyRows = monthlyRows,
                TopBookRows = topBookRows,
                LateReturnRows = lateReturnRows,
                CategoryBorrowRows = categoryBorrowRows
            };
        }

        private static string BuildCsv(ReportData report)
        {
            var sb = new StringBuilder();
            sb.AppendLine(EscapeCsv(report.Title));
            sb.AppendLine(EscapeCsv(report.DateRange));
            sb.AppendLine();
            foreach (var row in report.SummaryRows)
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            sb.AppendLine();
            sb.AppendLine("----- XU HƯỚNG MƯỢN THEO THÁNG -----");
            sb.AppendLine("Tháng,Số lượt mượn");
            foreach (var row in report.MonthlyRows)
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            sb.AppendLine();
            sb.AppendLine("----- TOP SÁCH ĐƯỢC MƯỢN NHIỀU NHẤT -----");
            sb.AppendLine("Hạng,Tên sách,Tác giả,Số lượt mượn");
            foreach (var row in report.TopBookRows)
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            sb.AppendLine();
            sb.AppendLine("----- DANH SÁCH TRẢ TRỄ -----");
            sb.AppendLine("STT,Tên sách,Ngày mượn,Số ngày trễ,Tiền phạt");
            foreach (var row in report.LateReturnRows)
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            sb.AppendLine();
            sb.AppendLine("----- THỐNG KÊ MƯỢN THEO THỂ LOẠI -----");
            sb.AppendLine("Tháng,Thể loại,Số lượt mượn,Tỉ lệ (%)");
            foreach (var row in report.CategoryBorrowRows)
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            return sb.ToString();
        }

        private static string BuildExcelXml(ReportData report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.AppendLine("  <Worksheet ss:Name=\"Báo cáo\">");
            sb.AppendLine("    <Table>");
            sb.AppendLine(BuildExcelRow(new[] { report.Title }));
            sb.AppendLine(BuildExcelRow(new[] { report.DateRange }));
            sb.AppendLine("      <Row/>");
            foreach (var row in report.SummaryRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("      <Row/>");
            sb.AppendLine(BuildExcelRow(new[] { "----- XU HƯỚNG MƯỢN THEO THÁNG -----" }));
            sb.AppendLine(BuildExcelRow(new[] { "Tháng", "Số lượt mượn" }));
            foreach (var row in report.MonthlyRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("      <Row/>");
            sb.AppendLine(BuildExcelRow(new[] { "----- TOP SÁCH ĐƯỢC MƯỢN NHIỀU NHẤT -----" }));
            sb.AppendLine(BuildExcelRow(new[] { "Hạng", "Tên sách", "Tác giả", "Số lượt mượn" }));
            foreach (var row in report.TopBookRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("      <Row/>");
            sb.AppendLine(BuildExcelRow(new[] { "----- DANH SÁCH TRẢ TRỄ -----" }));
            sb.AppendLine(BuildExcelRow(new[] { "STT", "Tên sách", "Ngày mượn", "Số ngày trễ", "Tiền phạt" }));
            foreach (var row in report.LateReturnRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("      <Row/>");
            sb.AppendLine(BuildExcelRow(new[] { "----- THỐNG KÊ MƯỢN THEO THỂ LOẠI -----" }));
            sb.AppendLine(BuildExcelRow(new[] { "Tháng", "Thể loại", "Số lượt mượn", "Tỉ lệ (%)" }));
            foreach (var row in report.CategoryBorrowRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("    </Table>");
            sb.AppendLine("  </Worksheet>");
            sb.AppendLine("</Workbook>");
            return sb.ToString();
        }

        private static string BuildExcelRow(string[] cells)
        {
            var sb = new StringBuilder();
            sb.AppendLine("      <Row>");
            foreach (var value in cells)
            {
                var escaped = System.Security.SecurityElement.Escape(value);
                sb.AppendLine($"        <Cell><Data ss:Type=\"String\">{escaped}</Data></Cell>");
            }
            sb.AppendLine("      </Row>");
            return sb.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
                return string.Empty;

            var escaped = value.Replace("\"", "\"\"");
            if (escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r'))
                return $"\"{escaped}\"";
            return escaped;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class ReportData
        {
            public string Title { get; set; } = string.Empty;
            public string DateRange { get; set; } = string.Empty;
            public ReportRow[] SummaryRows { get; set; } = Array.Empty<ReportRow>();
            public System.Collections.Generic.List<ReportRow> MonthlyRows { get; set; } = new();
            public System.Collections.Generic.List<ReportRow> TopBookRows { get; set; } = new();
            public System.Collections.Generic.List<ReportRow> LateReturnRows { get; set; } = new();
            public System.Collections.Generic.List<ReportRow> CategoryBorrowRows { get; set; } = new();
        }

        private sealed class ReportRow
        {
            public string[] Cells { get; set; } = Array.Empty<string>();
        }
    }
}

