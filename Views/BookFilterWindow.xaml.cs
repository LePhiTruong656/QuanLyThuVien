using System.Windows;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Views
{
    public partial class BookFilterWindow : Window
    {
        public int? SelectedYear { get; private set; }
        public BookAvailability? SelectedAvailability { get; private set; }
        public DateTime? AddedFrom { get; private set; }
        public DateTime? AddedTo { get; private set; }

        public BookFilterWindow(
            int? selectedYear,
            BookAvailability? selectedAvailability,
            DateTime? addedFrom,
            DateTime? addedTo)
        {
            InitializeComponent();

            YearTextBox.Text = selectedYear?.ToString() ?? string.Empty;
            AvailabilityComboBox.SelectedIndex = selectedAvailability switch
            {
                BookAvailability.SanCo => 1,
                BookAvailability.DangMuon => 2,
                _ => 0
            };
            AddedFromPicker.SelectedDate = addedFrom;
            AddedToPicker.SelectedDate = addedTo;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedYear = int.TryParse(YearTextBox.Text.Trim(), out var year) ? year : null;
            SelectedAvailability = AvailabilityComboBox.SelectedIndex switch
            {
                1 => BookAvailability.SanCo,
                2 => BookAvailability.DangMuon,
                _ => null
            };
            AddedFrom = AddedFromPicker.SelectedDate?.Date;
            AddedTo = AddedToPicker.SelectedDate?.Date;
            DialogResult = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedYear = null;
            SelectedAvailability = null;
            AddedFrom = null;
            AddedTo = null;
            DialogResult = true;
        }
    }
}
