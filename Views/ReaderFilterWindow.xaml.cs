using System.Windows;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Views
{
    public partial class ReaderFilterWindow : Window
    {
        public CardType? SelectedCardType { get; private set; }
        public ReaderStatus? SelectedStatus { get; private set; }
        public DateTime? RegisteredFrom { get; private set; }
        public DateTime? RegisteredTo { get; private set; }

        public ReaderFilterWindow(
            CardType? selectedCardType,
            ReaderStatus? selectedStatus,
            DateTime? registeredFrom,
            DateTime? registeredTo)
        {
            InitializeComponent();

            CardTypeComboBox.SelectedIndex = selectedCardType switch
            {
                CardType.SinhVien => 1,
                CardType.GiaoVien => 2,
                _ => 0
            };

            StatusComboBox.SelectedIndex = selectedStatus switch
            {
                ReaderStatus.HoatDong => 1,
                ReaderStatus.HetHan => 2,
                _ => 0
            };

            RegisteredFromPicker.SelectedDate = registeredFrom;
            RegisteredToPicker.SelectedDate = registeredTo;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCardType = CardTypeComboBox.SelectedIndex switch
            {
                1 => CardType.SinhVien,
                2 => CardType.GiaoVien,
                _ => null
            };

            SelectedStatus = StatusComboBox.SelectedIndex switch
            {
                1 => ReaderStatus.HoatDong,
                2 => ReaderStatus.HetHan,
                _ => null
            };

            RegisteredFrom = RegisteredFromPicker.SelectedDate?.Date;
            RegisteredTo = RegisteredToPicker.SelectedDate?.Date;
            DialogResult = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedCardType = null;
            SelectedStatus = null;
            RegisteredFrom = null;
            RegisteredTo = null;
            DialogResult = true;
        }
    }
}
