using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LibraryManagementFE.Models;

namespace LibraryManagementFE.ViewModels
{
    public class ReadersViewModel : INotifyPropertyChanged
    {
        // ── Sample data ────────────────────────────────────────
        public ObservableCollection<ReaderRecord> Readers { get; } = new()
        {
            new ReaderRecord { Name="Lê Hoàng Nam",    Email="namlh@gmail.com",       CardNumber="TV-2024-001", RegDate="12/03/2024", CardType=CardType.SinhVien,  Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Mai Thị Thu",     Email="thunmt@email.com",      CardNumber="TV-2024-002", RegDate="15/03/2024", CardType=CardType.GiaoVien,  Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Trần Văn Khoa",   Email="khoa.tran@student.edu", CardNumber="TV-2024-003", RegDate="18/03/2024", CardType=CardType.SinhVien,  Status=ReaderStatus.HetHan  },
            new ReaderRecord { Name="Kiều Nhã Phương", Email="phuong.kn@gmail.com",   CardNumber="TV-2024-004", RegDate="20/03/2024", CardType=CardType.SinhVien,  Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Nguyễn Thế Anh",  Email="anht@uni.edu.vn",       CardNumber="TV-2024-005", RegDate="22/03/2024", CardType=CardType.GiaoVien,  Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Phạm Thùy Linh",  Email="linh.pt@example.com",   CardNumber="TV-2024-006", RegDate="25/03/2024", CardType=CardType.SinhVien,  Status=ReaderStatus.HetHan  },
        };

        // ── Quick Stats ────────────────────────────────────────
        public string NewThisMonth    => "12";
        public string TotalReaders    => "1.2k";
        public string ExpiredReaders  => "8";
        public string ActiveReaders   => "1.2k";

        // ── Pagination ─────────────────────────────────────────
        public string PaginationInfo  => "Hiển thị 1 – 6 của 120 độc giả";
        public int    CurrentPage     { get; private set; } = 1;
        public int    TotalPages      { get; private set; } = 20;

        // ── Search ─────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // ── Commands ───────────────────────────────────────────
        public ICommand AddReaderCommand  { get; }
        public ICommand FilterCommand     { get; }
        public ICommand PrevPageCommand   { get; }
        public ICommand NextPageCommand   { get; }

        public ReadersViewModel()
        {
            AddReaderCommand = new RelayCommand(_ =>
                System.Windows.MessageBox.Show("Chức năng lập thẻ mới đang phát triển.",
                    "Thông báo", System.Windows.MessageBoxButton.OK));

            FilterCommand    = new RelayCommand(_ =>
                System.Windows.MessageBox.Show("Chức năng bộ lọc đang phát triển.",
                    "Thông báo", System.Windows.MessageBoxButton.OK));

            PrevPageCommand  = new RelayCommand(_ => { if (CurrentPage > 1) { CurrentPage--; OnPropertyChanged(nameof(CurrentPage)); } });
            NextPageCommand  = new RelayCommand(_ => { if (CurrentPage < TotalPages) { CurrentPage++; OnPropertyChanged(nameof(CurrentPage)); } });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // Minimal relay command so we don't need a library
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p)    => _execute(p);
        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
