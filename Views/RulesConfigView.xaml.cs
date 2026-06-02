using System.Windows;
using System.Windows.Controls;
using LibraryManagementFE.Policies;

namespace LibraryManagementFE.Views
{
    public partial class RulesConfigView : UserControl
    {
        private string _configPath = string.Empty;

        public RulesConfigView() => InitializeComponent();

        private void SetError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void ClearError()
        {
            ErrorTextBlock.Text = string.Empty;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void Load()
        {
            ClearError();
            var policy = LibraryPolicyStore.LoadOrCreate(out _configPath);

            MinAgeTextBox.Text = policy.MinAge.ToString();
            MaxBooksTextBox.Text = policy.MaxBooksPerReader.ToString();
            MaxLoanDaysTextBox.Text = policy.MaxLoanDays.ToString();
            MaxRenewalsTextBox.Text = policy.MaxRenewals.ToString();
            PenaltyPerDayTextBox.Text = policy.PenaltyPerDay.ToString();

            NotBorrowWhenCardLockedCheckBox.IsChecked = policy.NotBorrowWhenCardLocked;
            AutoLockWhenLateReturnCheckBox.IsChecked = policy.AutoLockWhenLateReturn;

            ConfigPathTextBlock.Text = $"File config: {_configPath}";
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Load();
        }

        private static bool TryParsePositiveInt(string? s, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!int.TryParse(s.Trim(), out value)) return false;
            return value > 0;
        }

        private static bool TryParseNonNegativeDecimal(string? s, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (!decimal.TryParse(s.Trim(), out value)) return false;
            return value >= 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ClearError();

            if (!TryParsePositiveInt(MinAgeTextBox.Text, out var minAge))
            {
                SetError("Tuổi tối thiểu phải là số nguyên dương.");
                return;
            }

            if (!TryParsePositiveInt(MaxBooksTextBox.Text, out var maxBooks))
            {
                SetError("Số sách mượn tối đa phải là số nguyên dương.");
                return;
            }

            if (!TryParsePositiveInt(MaxLoanDaysTextBox.Text, out var maxLoanDays))
            {
                SetError("Số ngày mượn tối đa phải là số nguyên dương.");
                return;
            }

            if (!TryParsePositiveInt(MaxRenewalsTextBox.Text, out var maxRenewals))
            {
                SetError("Số lần gia hạn tối đa phải là số nguyên dương.");
                return;
            }

            if (!TryParseNonNegativeDecimal(PenaltyPerDayTextBox.Text, out var penaltyPerDay))
            {
                SetError("Phạt mỗi ngày phải là số (>= 0).");
                return;
            }

            var policy = new LibraryPolicy
            {
                MinAge = minAge,
                MaxBooksPerReader = maxBooks,
                MaxLoanDays = maxLoanDays,
                MaxRenewals = maxRenewals,
                PenaltyPerDay = penaltyPerDay,
                NotBorrowWhenCardLocked = NotBorrowWhenCardLockedCheckBox.IsChecked == true,
                AutoLockWhenLateReturn = AutoLockWhenLateReturnCheckBox.IsChecked == true,
            };

            LibraryPolicyStore.Save(_configPath, policy);
            MessageBox.Show("Đã lưu cấu hình.", "Quy định", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ClearError();

            var policy = LibraryPolicy.Default();
            LibraryPolicyStore.Save(_configPath, policy);
            Load();
            MessageBox.Show("Đã khôi phục mặc định.", "Quy định", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
