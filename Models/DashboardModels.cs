using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LibraryManagementFE.Models
{
    /// <summary>Trạng thái giao dịch mượn/trả sách.</summary>
    public enum TransactionStatus { DaTra, DangMuon, QuaHan }

    /// <summary>Dữ liệu một dòng trong bảng giao dịch.</summary>
    public class TransactionRecord : INotifyPropertyChanged
    {
        private string _bookTitle = string.Empty;
        private string _genre = string.Empty;
        private string _reader = string.Empty;
        private string _dueDate = string.Empty;
        private TransactionStatus _status;

        public string BookTitle
        {
            get => _bookTitle;
            set { _bookTitle = value; OnPropertyChanged(); }
        }

        public string Genre
        {
            get => _genre;
            set { _genre = value; OnPropertyChanged(); }
        }

        public string Reader
        {
            get => _reader;
            set { _reader = value; OnPropertyChanged(); }
        }

        public string DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }

        public TransactionStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
        }

        public string StatusText => Status switch
        {
            TransactionStatus.DaTra    => "Đã trả",
            TransactionStatus.DangMuon => "Đang mượn",
            TransactionStatus.QuaHan   => "Quá hạn",
            _                          => string.Empty
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>Dữ liệu một cột biểu đồ xu hướng mượn sách.</summary>
    public class BarChartItem
    {
        public string MonthLabel { get; set; } = string.Empty;
        public double Value      { get; set; }
        public Brush  Fill       { get; set; } = Brushes.LightBlue;
        /// <summary>Chiều cao tương đối (0–1) – được tính từ giá trị max.</summary>
        public double RelativeHeight { get; set; }
    }

    /// <summary>Dữ liệu một dòng trong bảng thể loại phổ biến.</summary>
    public class CategoryItem
    {
        public string Name       { get; set; } = string.Empty;
        public int    Count      { get; set; }
        public double Percentage { get; set; }   // 0–1
        public Brush  BarColor   { get; set; } = Brushes.Blue;
    }
}
