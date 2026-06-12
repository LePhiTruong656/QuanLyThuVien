using System.Text.RegularExpressions;
using System.Windows;

namespace LibraryManagementFE.Views
{
    public partial class SocialDevLoginWindow : Window
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Email { get; private set; } = string.Empty;

        public SocialDevLoginWindow(string providerDisplayName, string? defaultEmail = null)
        {
            InitializeComponent();
            TxtTitle.Text = $"Đăng nhập {providerDisplayName} (Dev)";
            TxtDescription.Text =
                $"Chế độ phát triển: nhập email để mô phỏng đăng nhập {providerDisplayName}. " +
                "Khi đã có Client ID/App ID thật, điền vào appsettings.Development.json và đặt DevMode = false.";

            if (!string.IsNullOrWhiteSpace(defaultEmail))
                TxtEmail.Text = defaultEmail.Trim();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            var email = TxtEmail.Text.Trim();
            if (!EmailRegex.IsMatch(email))
            {
                MessageBox.Show("Vui lòng nhập email hợp lệ.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Email = email;
            DialogResult = true;
            Close();
        }
    }
}
