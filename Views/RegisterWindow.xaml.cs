using System.Windows;
using LibraryManagementFE.ViewModels;
using LibraryManagementFE.Views.Register;

namespace LibraryManagementFE.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly RegisterViewModel _vm = new();

        private readonly RegisterStep1View _step1 = new();
        private readonly RegisterStep2View _step2 = new();
        private readonly RegisterStep3View _step3 = new();

        public RegisterWindow()
        {
            InitializeComponent();
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
            MessageBox.Show(
                $"Đăng ký thành công!\nChào mừng {_vm.FullName} đến với thư viện UIT.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
