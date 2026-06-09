using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using LibraryManagementFE.Data;
using LibraryManagementFE.Models;
using LibraryManagementFE.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Views
{
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private bool _isPasswordVisible = false;
        private readonly LibraryDbContext _context;
        private readonly AuthService _authService;

        // Statistics properties
        private string _totalBooks = "0";
        private string _totalReaders = "0";
        private string _onTimeRate = "0%";

        public string TotalBooks
        {
            get => _totalBooks;
            set { _totalBooks = value; OnPropertyChanged(); }
        }

        public string TotalReaders
        {
            get => _totalReaders;
            set { _totalReaders = value; OnPropertyChanged(); }
        }

        public string OnTimeRate
        {
            get => _onTimeRate;
            set { _onTimeRate = value; OnPropertyChanged(); }
        }

        public string StatsDescription => $"Quản lý hơn {TotalBooks} đầu sách, theo dõi mượn–trả, phục vụ {TotalReaders} độc giả trên một nền tảng duy nhất.";

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Khởi tạo DbContext và AuthService
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
            _context = new LibraryDbContext(optionsBuilder.Options);
            _authService = new AuthService(_context);

            // Load statistics
            LoadStatistics();

            // Load saved credentials nếu có
            LoadSavedCredentials();
        }

        private void LoadStatistics()
        {
            try
            {
                // Total books
                var totalBooksCount = _context.Books.Count();
                TotalBooks = totalBooksCount >= 1000
                    ? $"{totalBooksCount / 1000.0:N1}K".Replace(".0", "")
                    : totalBooksCount.ToString();

                // Total active readers
                var totalReadersCount = _context.Readers.Count(r => r.Status == ReaderStatus.HoatDong);
                TotalReaders = totalReadersCount >= 1000
                    ? $"{totalReadersCount / 1000.0:N1}K+".Replace(".0", "")
                    : $"{totalReadersCount}+";

                // On-time return rate
                var allBorrows = _context.Borrows.ToList();
                var returned = allBorrows.Count(b => b.Status == BorrowStatus.DaTraTot || b.Status == BorrowStatus.DaTraTre);
                var onTime = allBorrows.Count(b => b.Status == BorrowStatus.DaTraTot);

                if (returned > 0)
                {
                    var rate = (double)onTime / returned * 100;
                    OnTimeRate = $"{rate:F1}%";
                }

                OnPropertyChanged(nameof(StatsDescription));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Load thông tin đăng nhập đã lưu (nếu user đã chọn Remember Me trước đó)
        /// </summary>
        private void LoadSavedCredentials()
        {
            var preferences = UserPreferences.Load();

            if (preferences.RememberMe && !string.IsNullOrEmpty(preferences.SavedEmail))
            {
                TxtEmail.Text = preferences.SavedEmail;
                ChkRememberMe.IsChecked = true;

                // Giải mã password nếu có
                if (!string.IsNullOrEmpty(preferences.EncryptedPassword))
                {
                    string decryptedPassword = CredentialHelper.Unprotect(preferences.EncryptedPassword);
                    if (!string.IsNullOrEmpty(decryptedPassword))
                    {
                        TxtPassword.Password = decryptedPassword;
                    }
                }
            }
        }

        /// <summary>
        /// Lưu credentials nếu user chọn Remember Me
        /// </summary>
        private void SaveCredentials(string email, string password, bool rememberMe)
        {
            var preferences = new UserPreferences
            {
                RememberMe = rememberMe,
                SavedEmail = rememberMe ? email : string.Empty,
                EncryptedPassword = rememberMe ? CredentialHelper.Protect(password) : string.Empty
            };

            preferences.Save();
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

            // Lưu credentials nếu user chọn Remember Me
            bool rememberMe = ChkRememberMe.IsChecked == true;
            SaveCredentials(email, password, rememberMe);

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
