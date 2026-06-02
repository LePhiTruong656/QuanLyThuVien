using System.Windows;

namespace LibraryManagementFE.Views
{
    public partial class ConfirmDeleteWindow : Window
    {
        public ConfirmDeleteWindow(string itemType, string itemName)
            : this(itemType, itemName, "Thao tác này không thể hoàn tác.")
        {
        }

        public ConfirmDeleteWindow(string itemType, string itemName, string warning)
        {
            InitializeComponent();

            Title = $"Xóa {itemType}";
            TitleTextBlock.Text = $"Xóa {itemType}?";
            SubtitleTextBlock.Text = itemName;
            MessageTextBlock.Text = $"Bạn có chắc chắn muốn xóa {itemType} \"{itemName}\"? {warning}";
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
