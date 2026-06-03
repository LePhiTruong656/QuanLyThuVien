using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.Views
{
    public partial class LendBorrowWindow : Window
    {
        private readonly CollectionViewSource _readerViewSource = new();
        private readonly CollectionViewSource _bookViewSource = new();

        private ObservableCollection<ReaderRecord> _readers = new();
        private ObservableCollection<BookRecord> _availableBooks = new();

        public ObservableCollection<ReaderRecord> Readers
        {
            get => _readers;
            set
            {
                _readers = value ?? new ObservableCollection<ReaderRecord>();
                _readerViewSource.Source = _readers;
            }
        }

        public ObservableCollection<BookRecord> AvailableBooks
        {
            get => _availableBooks;
            set
            {
                _availableBooks = value ?? new ObservableCollection<BookRecord>();
                _bookViewSource.Source = _availableBooks;
            }
        }

        public ICollectionView ReaderView => _readerViewSource.View;
        public ICollectionView BookView => _bookViewSource.View;

        public ReaderRecord? SelectedReader { get; set; }
        public BookRecord? SelectedBook { get; set; }
        public int LoanDays { get; private set; } = 14;
        public DateTime BorrowDate { get; private set; } = DateTime.Now;

        public LendBorrowWindow()
        {
            InitializeComponent();
            DataContext = this;

            _readerViewSource.Filter += ReaderFilter;
            _bookViewSource.Filter += BookFilter;
            BorrowDatePicker.SelectedDate = DateTime.Now;
        }

        private void ReaderFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not ReaderRecord reader)
            {
                e.Accepted = false;
                return;
            }

            var filter = ReaderComboBox?.Text?.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = reader.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                         || reader.CardNumber.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private void BookFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is not BookRecord book)
            {
                e.Accepted = false;
                return;
            }

            var filter = BookComboBox?.Text?.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = book.Title.Contains(filter, StringComparison.OrdinalIgnoreCase)
                         || book.Author.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private void ReaderComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not ComboBox combo) return;
            ReaderView.Refresh();
            
            if (!string.IsNullOrWhiteSpace(combo.Text))
            {
                combo.IsDropDownOpen = true;
                // Keep the text box focused and don't auto-select an item
                combo.SelectedIndex = -1;
            }
        }

        private void BookComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not ComboBox combo) return;
            BookView.Refresh();
            
            if (!string.IsNullOrWhiteSpace(combo.Text))
            {
                combo.IsDropDownOpen = true;
                // Keep the text box focused and don't auto-select an item
                combo.SelectedIndex = -1;
            }
        }

        private void ReaderComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedReader == null)
            {
                CardNumberTextBox.Text = string.Empty;
                CardTypeTextBox.Text = string.Empty;
                return;
            }

            CardNumberTextBox.Text = SelectedReader.CardNumber;
            CardTypeTextBox.Text = SelectedReader.CardType == CardType.GiaoVien ? "Giáo viên" : "Sinh viên";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedReader == null || SelectedBook == null)
            {
                ErrorTextBlock.Text = "Vui lòng chọn độc giả và sách.";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            if (!int.TryParse(LoanDaysTextBox.Text, out var days) || days <= 0)
            {
                ErrorTextBlock.Text = "Số ngày mượn phải là số nguyên dương.";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }
            LoanDays = days;
            BorrowDate = BorrowDatePicker.SelectedDate ?? DateTime.Now;
            DialogResult = true;
        }
    }
}
