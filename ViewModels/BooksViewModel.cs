using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.ViewModels
{
    /// <summary>MVVM layer for Quản lý Sách (Figma: search, table, pagination, stats).</summary>
    public class BooksViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BookRecord> Books { get; } = new()
        {
            new BookRecord
            {
                Stt = 1,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                CategoryLine1 = "CNTT",
                CategoryLine2 = "TECH",
                CategoryPillBg = "#EFF6FF",
                CategoryPillFg = "#1978E5",
                Year = 2008,
                Availability = BookAvailability.SanCo,
            },
            new BookRecord
            {
                Stt = 2,
                Title = "Introduction to Algorithms",
                Author = "Thomas H. Cormen",
                CategoryLine1 = "CNTT",
                CategoryLine2 = "TECH",
                CategoryPillBg = "#EFF6FF",
                CategoryPillFg = "#1978E5",
                Year = 2009,
                Availability = BookAvailability.DangMuon,
            },
            new BookRecord
            {
                Stt = 3,
                Title = "Mắt Biếc",
                Author = "Nguyễn Nhật Ánh",
                CategoryLine1 = "Văn",
                CategoryLine2 = "học",
                CategoryPillBg = "#FAF5FF",
                CategoryPillFg = "#9333EA",
                Year = 1990,
                Availability = BookAvailability.SanCo,
            },
        };

        public string TotalBooksDisplay => "1.240";
        public string NewThisMonthDisplay => "42";
        public string BorrowedDisplay => "86";

        public string PaginationInfo => "Hiển thị 1 – 3 của 150 đầu sách";

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }
        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }

        public BooksViewModel()
        {
            FilterCommand = new RelayCommand(_ =>
                MessageBox.Show("Bộ lọc nâng cao đang phát triển.", "Thông báo", MessageBoxButton.OK));

            AddBookCommand = new RelayCommand(_ =>
                MessageBox.Show("Thêm sách mới đang phát triển.", "Thông báo", MessageBoxButton.OK));

            EditBookCommand = new RelayCommand(p =>
            {
                if (p is BookRecord b)
                    MessageBox.Show($"Sửa: {b.Title}", "Thông báo", MessageBoxButton.OK);
            });

            DeleteBookCommand = new RelayCommand(p =>
            {
                if (p is BookRecord b)
                    MessageBox.Show($"Xóa: {b.Title}", "Thông báo", MessageBoxButton.OK);
            });

            PrevPageCommand = new RelayCommand(_ =>
                MessageBox.Show("Trang trước (demo).", "Phân trang", MessageBoxButton.OK));

            NextPageCommand = new RelayCommand(_ =>
                MessageBox.Show("Trang sau (demo).", "Phân trang", MessageBoxButton.OK));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
