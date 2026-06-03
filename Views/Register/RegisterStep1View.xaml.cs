using System.Windows;
using System.Windows.Controls;

namespace LibraryManagementFE.Views.Register
{
    public partial class RegisterStep1View : UserControl
    {
        private bool _isPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;

        public RegisterStep1View()
        {
            InitializeComponent();
        }

        public string Password =>
            _isPasswordVisible ? TxtPasswordVisible.Text : TxtPassword.Password;

        public string ConfirmPassword =>
            _isConfirmPasswordVisible ? TxtConfirmPasswordVisible.Text : TxtConfirmPassword.Password;

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                TxtPasswordVisible.Text = TxtPassword.Password;
                TxtPasswordVisible.Visibility = Visibility.Visible;
                TxtPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtPassword.Password = TxtPasswordVisible.Text;
                TxtPasswordVisible.Visibility = Visibility.Collapsed;
                TxtPassword.Visibility = Visibility.Visible;
            }
        }

        private void BtnToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            if (_isConfirmPasswordVisible)
            {
                TxtConfirmPasswordVisible.Text = TxtConfirmPassword.Password;
                TxtConfirmPasswordVisible.Visibility = Visibility.Visible;
                TxtConfirmPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtConfirmPassword.Password = TxtConfirmPasswordVisible.Text;
                TxtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
                TxtConfirmPassword.Visibility = Visibility.Visible;
            }
        }
    }
}
