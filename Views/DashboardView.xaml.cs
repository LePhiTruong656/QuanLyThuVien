using System.Windows;
using System.Windows.Controls;

namespace LibraryManagementFE.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        // ── Chart toggle ────────────────────────────────────────────────
        private void BtnWeek_Click(object sender, RoutedEventArgs e)
        {
            BtnWeek.Style  = (Style)FindResource("ToggleChartBtn.Active");
            BtnMonth.Style = (Style)FindResource("ToggleChartBtn");
        }

        private void BtnMonth_Click(object sender, RoutedEventArgs e)
        {
            BtnMonth.Style = (Style)FindResource("ToggleChartBtn.Active");
            BtnWeek.Style  = (Style)FindResource("ToggleChartBtn");
        }

        // ── Table row action ────────────────────────────────────────────
        private void BtnRowAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Models.TransactionRecord record)
            {
                MessageBox.Show(
                    $"Sách: {record.BookTitle}\nĐộc giả: {record.Reader}\nTrạng thái: {record.StatusText}",
                    "Chi tiết giao dịch",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnDetailReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng báo cáo chi tiết đang được phát triển.",
                            "Báo cáo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Transactions_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }
}
