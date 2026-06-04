using System;
using System.Globalization;
using System.Windows;
using LibraryManagementFE.Models;
using LibraryManagementFE.Services;
using LibraryManagementFE.ViewModels;
using LibraryManagementFE.Views.Register;
using LibraryManagementFE.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementFE.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm = new();
        private readonly LibraryDbContext _context;
        private readonly AuthService _authService;

        private readonly RegisterStep1View _step1 = new();
        private readonly RegisterStep2View _step2 = new();
        private readonly RegisterStep3View _step3 = new();

        private string _registeredPassword = string.Empty; // Lưu password từ step 1

        public RegisterWindow()
        {
            InitializeComponent();

            // Khởi tạo DbContext và AuthService
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer(DatabaseSettings.GetConnectionString());
            _context = new LibraryDbContext(optionsBuilder.Options);
            _authService = new AuthService(_context);

            DataContext = _vm;
            StepContent.Content = _step1;
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            switch (_vm.CurrentStep)
            {
                case 1:
                    if (_vm.ValidateStep1(_step1.Password, _step1.ConfirmPassword))
                    {
                        // Kiểm tra email đã tồn tại chưa
                        if (_authService.EmailExists(_vm.Email))
                        {
                            MessageBox.Show("Email này đã được đăng ký. Vui lòng sử dụng email khác.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        _registeredPassword = _step1.Password; // Lưu password
                        GoToStep(2);
                    }
                    break;

                case 2:
                    if (_vm.ValidateStep2())
                        GoToStep(3);
                    break;

                case 3:
                    if (_vm.ValidateStep3())
                        FinishRegistration();
                    break;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.CurrentStep > 1)
                GoToStep(_vm.CurrentStep - 1);
        }

        private void GoToStep(int step)
        {
            _vm.CurrentStep = step;
            StepContent.Content = step switch
            {
                1 => (object)_step1,
                2 => _step2,
                3 => _step3,
                _ => _step1
            };
        }

        private void FinishRegistration()
        {
            try
            {
                // 1. Tạo Reader record
                var readerId = SaveNewReader();

                // 2. Đăng ký User với password đã hash
                var result = _authService.Register(_vm.Email, _registeredPassword, readerId);

                if (!result.success)
                {
                    MessageBox.Show(result.message, "Lỗi đăng ký", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show(
                    $"Đăng ký thành công!\nChào mừng {_vm.FullName} đến với thư viện UIT.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng ký: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string SaveNewReader()
        {
            var birthDate = _vm.DateOfBirth;
            if (!string.IsNullOrWhiteSpace(_vm.DateOfBirth) &&
                DateTime.TryParseExact(_vm.DateOfBirth, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDob))
            {
                birthDate = parsedDob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var newReader = new ReaderRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = _vm.FullName,
                Email = _vm.Email,
                CardNumber = _vm.StudentId,
                DateOfBirth = birthDate,
                RegDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CardType = _vm.IsStudent ? CardType.SinhVien : CardType.GiaoVien,
                Status = ReaderStatus.HoatDong
            };

            _context.Readers.Add(newReader);
            _context.SaveChanges();

            return newReader.Id;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
