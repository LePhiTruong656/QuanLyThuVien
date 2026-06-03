using System;
using System.Globalization;
using System.Windows;
using LibraryManagementFE.Models;
using LibraryManagementFE.Services;
using LibraryManagementFE.ViewModels;
using LibraryManagementFE.Views.Register;

namespace LibraryManagementFE.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm = new();
        private readonly LibraryDataStore _store;

        private readonly RegisterStep1View _step1 = new();
        private readonly RegisterStep2View _step2 = new();
        private readonly RegisterStep3View _step3 = new();

        public RegisterWindow()
        {
            InitializeComponent();
            _store = LibraryDataStoreFile.LoadOrCreate();
            DataContext = _vm;
            StepContent.Content = _step1;
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            switch (_vm.CurrentStep)
            {
                case 1:
                    if (_vm.ValidateStep1(_step1.Password, _step1.ConfirmPassword))
                        GoToStep(2);
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
            SaveNewReader();

            MessageBox.Show(
                $"Đăng ký thành công!\nChào mừng {_vm.FullName} đến với thư viện UIT.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void SaveNewReader()
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

            _store.Readers.Add(newReader);
            LibraryDataStoreFile.Save(_store);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
