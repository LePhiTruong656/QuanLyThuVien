using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementFE.Policies;
using LibraryManagementFE.Services;
using Microsoft.Win32;

namespace LibraryManagementFE.Views
{
    public partial class ReportsStatisticsView : UserControl, INotifyPropertyChanged
    {
        private readonly BorrowService _service;
        private DateTime? _fromDate;
        private DateTime? _toDate;

        public ReportsStatisticsView()
        {
            InitializeComponent();
            DataContext = this;
            _service = new BorrowService(LibraryPolicyStore.LoadOrCreate());
            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;
        }

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (_fromDate == value) return;
                _fromDate = value;
                OnPropertyChanged();
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
            }
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
            var records = _service.Borrows
                .Where(b => DateTime.TryParse(b.BorrowDate, out var borrowDate) && borrowDate.Date >= from.Date && borrowDate.Date <= to.Date)
                .ToList();

            var totalBorrows = records.Count;
            var activeBorrows = records.Count(b => b.Status == Models.BorrowStatus.DangMuon);
            var returnedBorrows = records.Count(b => b.Status == Models.BorrowStatus.DaTraTot || b.Status == Models.BorrowStatus.DaTraTre);
            var overdueBorrows = records.Count(b => b.Status == Models.BorrowStatus.QuaHan);
            var totalFines = _service.TotalFinesCollected(from, to);
            var totalFinesDisplay = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", totalFines);

            var monthlyRows = _service.MonthlyBorrowCounts(6)
                .Select(m => new ReportRow { Cells = new[] { m.Month, m.Count.ToString(CultureInfo.InvariantCulture) } })
                .ToList();

            var topBookRows = _service.TopBooks(10)
                .Select(t => new ReportRow { Cells = new[] { t.Rank.ToString(CultureInfo.InvariantCulture), t.Title, t.Author, t.Borrows.ToString(CultureInfo.InvariantCulture) } })
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
                    new ReportRow { Cells = new[] { "Tiền phạt đã thu", totalFinesDisplay } }
                },
                MonthlyRows = monthlyRows,
                TopBookRows = topBookRows
            };
        }

        private static string BuildCsv(ReportData report)
        {
            var sb = new StringBuilder();
            sb.AppendLine(EscapeCsv(report.Title));
            sb.AppendLine(EscapeCsv(report.DateRange));
            sb.AppendLine();
            foreach (var row in report.SummaryRows)
            {
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            }
            sb.AppendLine();
            sb.AppendLine("Tháng,Số lượt mượn");
            foreach (var row in report.MonthlyRows)
            {
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            }
            sb.AppendLine();
            sb.AppendLine("Hạng,Tên sách,Tác giả,Số lượt mượn");
            foreach (var row in report.TopBookRows)
            {
                sb.AppendLine(string.Join(",", row.Cells.Select(EscapeCsv)));
            }
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
            sb.AppendLine(BuildExcelRow(new[] { "Tháng", "Số lượt mượn" }));
            foreach (var row in report.MonthlyRows)
            {
                sb.AppendLine(BuildExcelRow(row.Cells));
            }
            sb.AppendLine("      <Row/>");
            sb.AppendLine(BuildExcelRow(new[] { "Hạng", "Tên sách", "Tác giả", "Số lượt mượn" }));
            foreach (var row in report.TopBookRows)
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
        }

        private sealed class ReportRow
        {
            public string[] Cells { get; set; } = Array.Empty<string>();
        }
    }
}
