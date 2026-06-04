using System;
using System.Windows;
using LibraryManagementFE.Data;
using LibraryManagementFE.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Views
{
    public partial class LoginWindow : Window
    {
        private bool _isPasswordVisible = false;
        private readonly LibraryDbContext _context;
        private readonly AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();

            // Khởi tạo DbContext và AuthService
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
            _context = new LibraryDbContext(optionsBuilder.Options);
            _authService = new AuthService(_context);
        }

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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();
            string password = _isPasswordVisible ? TxtPasswordVisible.Text : TxtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập email và mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Xác thực từ database
            var result = _authService.Login(email, password);

            if (!result.success)
            {
                MessageBox.Show(result.message, "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Lấy thông tin reader nếu có
            var reader = _authService.GetReaderByUserId(result.user!.Id);

            // Lưu thông tin user và reader vào session
            CurrentUser.SetUser(result.user!, reader);

            // Đăng nhập thành công
            MessageBox.Show($"Chào mừng {CurrentUser.GetDisplayName()} trở lại!",
                "Đăng nhập thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }

        private void BtnSocialLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được phát triển.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            this.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}
