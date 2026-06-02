using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LibraryManagementFE.Models;
using LibraryManagementFE.Policies;
using LibraryManagementFE.Services;

namespace LibraryManagementFE.Views
{
    public partial class CollectFineWindow : Window, INotifyPropertyChanged
    {
        private readonly BorrowService _service;
        private readonly CollectionViewSource _readerViewSource = new();

        private ObservableCollection<ReaderRecord> _readers = new();
        private ReaderRecord? _selectedReader;
        private string _selectedReaderId = string.Empty;
        private decimal _paymentAmount;
        private ObservableCollection<FineDetailModel> _fineDetails = new();
        private decimal _totalFineAmount;
        private decimal _unpaidFineAmount;

        public ObservableCollection<ReaderRecord> Readers
        {
            get => _readers;
            set
            {
                _readers = value ?? new ObservableCollection<ReaderRecord>();
                _readerViewSource.Source = _readers;
            }
        }

        public ICollectionView ReaderView => _readerViewSource.View;

        public ReaderRecord? SelectedReader
        {
            get => _selectedReader;
            set
            {
                if (_selectedReader == value) return;
                _selectedReader = value;
                OnPropertyChanged();
            }
        }

        public string SelectedReaderId
        {
            get => _selectedReaderId;
            set
            {
                if (_selectedReaderId == value) return;
                _selectedReaderId = value;
                OnPropertyChanged();
            }
        }

        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                if (_paymentAmount == value) return;
                _paymentAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingAmount));
            }
        }

        public ObservableCollection<FineDetailModel> FineDetails
        {
            get => _fineDetails;
            set
            {
                if (_fineDetails == value) return;
                _fineDetails = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalFineAmount
        {
            get => _totalFineAmount;
            set
            {
                if (_totalFineAmount == value) return;
                _totalFineAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal UnpaidFineAmount
        {
            get => _unpaidFineAmount;
            set
            {
                if (_unpaidFineAmount == value) return;
                _unpaidFineAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal RemainingAmount => Math.Max(0, UnpaidFineAmount - PaymentAmount);

        // Thay constructor trong CollectFineWindow.xaml.cs

        public CollectFineWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Lỗi InitializeComponent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }

            DataContext = this;
            _service = new BorrowService(LibraryPolicyStore.LoadOrCreate());

            _readerViewSource.Filter += ReaderFilter;
            LoadReaders();
        }



        private void LoadReaders()
        {
            var readers = _service.GetReaders() ?? Enumerable.Empty<ReaderRecord>();
            Readers = new ObservableCollection<ReaderRecord>(readers);
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

        private void ReaderComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not ComboBox combo) return;
            ReaderView.Refresh();

            if (!string.IsNullOrWhiteSpace(combo.Text) && combo.Items.Count == 0)
            {
                ErrorMessageTextBlock.Text = "Không tìm thấy độc giả phù hợp.";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedReaderId) && string.IsNullOrWhiteSpace(ReaderComboBox.Text))
            {
                ShowError("Vui lòng chọn hoặc nhập tên / mã thẻ độc giả.");
                return;
            }

            var reader = _readers.FirstOrDefault(r => r.Id == SelectedReaderId);
            if (reader == null && !string.IsNullOrWhiteSpace(ReaderComboBox.Text))
            {
                var searchText = ReaderComboBox.Text.Trim();
                reader = _readers.FirstOrDefault(r =>
                    r.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    r.CardNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (reader == null)
            {
                ShowError("Không tìm thấy độc giả phù hợp.");
                return;
            }

            SelectedReader = reader;
            SelectedReaderId = reader.Id;
            ClearError();
            LoadFineDetails(reader.Id);
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void ClearError()
        {
            ErrorMessageTextBlock.Text = string.Empty;
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
        }

        private void LoadFineDetails(string readerId)
        {
            var borrows = _service.Borrows
                .Where(b => b.ReaderId == readerId && b.FineAmount > 0)
                .ToList();

            if (!borrows.Any())
            {
                ShowError("Độc giả không có khoản nợ phạt.");
                FineDetailsPanel.Visibility = Visibility.Collapsed;
                PaymentPanel.Visibility = Visibility.Collapsed;
                return;
            }

            FineDetails.Clear();
            TotalFineAmount = 0;
            UnpaidFineAmount = 0;

            foreach (var borrow in borrows)
            {
                var remaining = borrow.FineAmount - borrow.PaidFineAmount;
                var detail = new FineDetailModel
                {
                    MaPhieu = borrow.MaPhieu,
                    BookTitle = borrow.BookTitle,
                    FineAmount = borrow.FineAmount,
                    PaidAmount = borrow.PaidFineAmount,
                    RemainingAmount = remaining
                };
                FineDetails.Add(detail);
                TotalFineAmount += borrow.FineAmount;
                UnpaidFineAmount += remaining;
            }

            PaymentAmount = UnpaidFineAmount;   // mặc định thu hết nợ
            ErrorMessageTextBlock.Text = string.Empty;
            FineDetailsPanel.Visibility = Visibility.Visible;
            PaymentPanel.Visibility = Visibility.Visible;
        }

        private void PaymentAmountTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Only allow numbers and one decimal point
            e.Handled = !Regex.IsMatch(e.Text, @"[0-9.]");
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedReader == null)
            {
                ErrorMessageTextBlock.Text = "Vui lòng chọn độc giả.";
                return;
            }
            if (PaymentAmount <= 0)
            {
                ErrorMessageTextBlock.Text = "Vui lòng nhập số tiền lớn hơn 0.";
                return;
            }
            if (PaymentAmount > UnpaidFineAmount)
            {
                ErrorMessageTextBlock.Text = $"Số tiền thu không được vượt quá {UnpaidFineAmount:N0} đ.";
                return;
            }

            try
            {
                decimal totalCollected = 0;
                // Lấy danh sách phiếu còn nợ, sắp xếp tuỳ ý (có thể theo thời gian)
                var unpaidBorrows = _service.Borrows
                    .Where(b => b.ReaderId == SelectedReader.Id &&
                                b.FineAmount > 0 &&
                                (b.FineAmount - b.PaidFineAmount) > 0)
                    .ToList();

                foreach (var borrow in unpaidBorrows)
                {
                    if (totalCollected >= PaymentAmount) break;

                    var remaining = borrow.FineAmount - borrow.PaidFineAmount;
                    var amount = Math.Min(PaymentAmount - totalCollected, remaining);
                    if (amount > 0)
                    {
                        _service.CollectFine(borrow.MaPhieu, amount, "Thu tiền phạt qua cửa sổ");
                        totalCollected += amount;
                    }
                }

                MessageBox.Show($"Đã thu phạt {totalCollected:N0} đ cho {SelectedReader.Name}.",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearError();
                LoadFineDetails(SelectedReader.Id);

                if (UnpaidFineAmount <= 0)
                {
                    DialogResult = true;   // ← báo thành công và đóng
                    this.Close();
                }
                else
                    PaymentAmount = UnpaidFineAmount;   // đề xuất thu tiếp số còn lại
            }
            catch (Exception ex)
            {
                ErrorMessageTextBlock.Text = $"Lỗi: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class FineDetailModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
