using System.Text.RegularExpressions;
using System.Windows;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Views
{
    public partial class AddReaderWindow : Window
    {
        private readonly DateTime _defaultDateOfBirth = new(1999, 1, 1);

        public ReaderRecord? Reader { get; private set; }

        public AddReaderWindow()
        {
            InitializeComponent();
            DateOfBirthPicker.SelectedDate = _defaultDateOfBirth;
            DateOfBirthPicker.DisplayDateEnd = DateTime.Today.AddDays(-1);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text.Trim();
            var email = EmailTextBox.Text.Trim();
            var cardNumber = string.Empty;
            var dateOfBirth = DateOfBirthPicker.SelectedDate ?? _defaultDateOfBirth;
            var regDate = DateTime.Today;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Vui lòng nhập họ tên độc giả.");
                NameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                ShowError("Email không hợp lệ.");
                EmailTextBox.Focus();
                return;
            }

            if (dateOfBirth.Date >= DateTime.Today)
            {
                ShowError("Ngày sinh phải nhỏ hơn ngày hiện tại.");
                DateOfBirthPicker.Focus();
                return;
            }

            Reader = new ReaderRecord
            {
                Name = name,
                Email = email,
                CardNumber = cardNumber,
                DateOfBirth = dateOfBirth.ToString("dd/MM/yyyy"),
                RegDate = regDate.ToString("dd/MM/yyyy"),
                CardType = CardTypeComboBox.SelectedIndex == 1 ? CardType.GiaoVien : CardType.SinhVien,
                Status = ReaderStatus.HoatDong,
            };

            DialogResult = true;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
