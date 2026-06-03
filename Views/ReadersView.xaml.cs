using System.Windows;
using System.Windows.Controls;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Views
{
    public partial class ReadersView : UserControl
    {
        public ReadersView() => InitializeComponent();

        private void BtnRowMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ReaderRecord r)
                MessageBox.Show($"Độc giả: {r.Name}\nEmail: {r.Email}\nMã thẻ: {r.CardNumber}\nTrạng thái: {r.StatusText}",
                    "Chi tiết độc giả", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
