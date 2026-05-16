using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LibraryManagementFE.Models;
using LibraryManagementFE.Views;

namespace LibraryManagementFE.ViewModels
{
    public class ReadersViewModel : INotifyPropertyChanged
    {
        private const int PageSize = 5;

        public ObservableCollection<ReaderRecord> Readers { get; } = new()
        {
            new ReaderRecord { Name="Le Hoang Nam",      Email="namlh@gmail.com",       CardNumber="TV-2024-001", RegDate="12/03/2024", CardType=CardType.SinhVien, Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Mai Thi Thu",       Email="thunmt@email.com",      CardNumber="TV-2024-002", RegDate="15/03/2024", CardType=CardType.GiaoVien, Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Tran Van Khoa",     Email="khoa.tran@student.edu", CardNumber="TV-2024-003", RegDate="18/03/2024", CardType=CardType.SinhVien, Status=ReaderStatus.HetHan },
            new ReaderRecord { Name="Kieu Nha Phuong",   Email="phuong.kn@gmail.com",   CardNumber="TV-2024-004", RegDate="20/03/2024", CardType=CardType.SinhVien, Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Nguyen The Anh",    Email="anht@uni.edu.vn",       CardNumber="TV-2024-005", RegDate="22/03/2024", CardType=CardType.GiaoVien, Status=ReaderStatus.HoatDong },
            new ReaderRecord { Name="Pham Thuy Linh",    Email="linh.pt@example.com",   CardNumber="TV-2024-006", RegDate="25/03/2024", CardType=CardType.SinhVien, Status=ReaderStatus.HetHan },
        };

        public ObservableCollection<ReaderRecord> PagedReaders { get; } = new();
        public ObservableCollection<PageNumberItem> PageNumbers { get; } = new();

        public string NewThisMonth => Readers.Count(IsRegisteredThisMonth).ToString();
        public string TotalReaders => Readers.Count.ToString();
        public string ExpiredReaders => Readers.Count(r => r.Status == ReaderStatus.HetHan).ToString();
        public string ActiveReaders => Readers.Count(r => r.Status == ReaderStatus.HoatDong).ToString();

        private CardType? _filterCardType;
        private ReaderStatus? _filterStatus;
        private DateTime? _filterRegisteredFrom;
        private DateTime? _filterRegisteredTo;

        private IEnumerable<ReaderRecord> FilteredReaders
        {
            get
            {
                IEnumerable<ReaderRecord> query = Readers;

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var keyword = SearchText.Trim();
                    query = query.Where(reader =>
                        reader.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        reader.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        reader.CardNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                }

                if (_filterCardType is CardType cardType)
                {
                    query = query.Where(reader => reader.CardType == cardType);
                }

                if (_filterStatus is ReaderStatus status)
                {
                    query = query.Where(reader => reader.Status == status);
                }

                if (_filterRegisteredFrom is DateTime registeredFrom)
                {
                    query = query.Where(reader =>
                        TryGetRegDate(reader, out var regDate) && regDate.Date >= registeredFrom.Date);
                }

                if (_filterRegisteredTo is DateTime registeredTo)
                {
                    query = query.Where(reader =>
                        TryGetRegDate(reader, out var regDate) && regDate.Date <= registeredTo.Date);
                }

                return query;
            }
        }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredReaders.Count() / (double)PageSize));

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (_currentPage == value)
                {
                    return;
                }

                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaginationInfo));
            }
        }

        public string PaginationInfo
        {
            get
            {
                var total = FilteredReaders.Count();
                if (total == 0)
                {
                    return "Hiển thị 0 của 0 độc giả";
                }

                var start = ((CurrentPage - 1) * PageSize) + 1;
                var end = Math.Min(CurrentPage * PageSize, total);
                return $"Hiển thị {start} - {end} của {total} độc giả";
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                {
                    return;
                }

                _searchText = value;
                OnPropertyChanged();
                GoToPage(1);
            }
        }

        public ICommand AddReaderCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public ReadersViewModel()
        {
            AddReaderCommand = new RelayCommand(OpenAddReaderDialog);

            FilterCommand = new RelayCommand(OpenFilterDialog);

            PrevPageCommand = new RelayCommand(
                _ => GoToPage(CurrentPage - 1),
                _ => CurrentPage > 1);

            NextPageCommand = new RelayCommand(
                _ => GoToPage(CurrentPage + 1),
                _ => CurrentPage < TotalPages);

            GoToPageCommand = new RelayCommand(p =>
            {
                if (p is int page)
                {
                    GoToPage(page);
                    return;
                }

                if (int.TryParse(p?.ToString(), out page))
                {
                    GoToPage(page);
                }
            });

            RefreshPagedReaders();
        }

        private void OpenAddReaderDialog(object? _)
        {
            var dialog = new AddReaderWindow
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true && dialog.Reader is not null)
            {
                dialog.Reader.CardNumber = GenerateCardNumber();
                Readers.Add(dialog.Reader);
                RefreshStats();
                GoToPage(TotalPages);
            }
        }

        private void OpenFilterDialog(object? _)
        {
            var dialog = new ReaderFilterWindow(
                _filterCardType,
                _filterStatus,
                _filterRegisteredFrom,
                _filterRegisteredTo)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true)
            {
                _filterCardType = dialog.SelectedCardType;
                _filterStatus = dialog.SelectedStatus;
                _filterRegisteredFrom = dialog.RegisteredFrom;
                _filterRegisteredTo = dialog.RegisteredTo;
                GoToPage(1);
            }
        }

        private string GenerateCardNumber()
        {
            return $"TV-{DateTime.Now:yyyy}-{Readers.Count + 1:000}";
        }

        private static bool IsRegisteredThisMonth(ReaderRecord reader)
        {
            return TryGetRegDate(reader, out var regDate)
                && regDate.Month == DateTime.Now.Month
                && regDate.Year == DateTime.Now.Year;
        }

        private static bool TryGetRegDate(ReaderRecord reader, out DateTime regDate)
        {
            return DateTime.TryParseExact(
                reader.RegDate,
                "dd/MM/yyyy",
                null,
                System.Globalization.DateTimeStyles.None,
                out regDate);
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(NewThisMonth));
            OnPropertyChanged(nameof(TotalReaders));
            OnPropertyChanged(nameof(ExpiredReaders));
            OnPropertyChanged(nameof(ActiveReaders));
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PaginationInfo));
            CommandManager.InvalidateRequerySuggested();
        }

        private void GoToPage(int page)
        {
            CurrentPage = Math.Clamp(page, 1, TotalPages);
            RefreshPagedReaders();
        }

        private void RefreshPagedReaders()
        {
            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }

            var filteredReaders = FilteredReaders.ToList();
            PagedReaders.Clear();
            foreach (var reader in filteredReaders.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            {
                PagedReaders.Add(reader);
            }

            RefreshPageNumbers();
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PaginationInfo));
            CommandManager.InvalidateRequerySuggested();
        }

        private void RefreshPageNumbers()
        {
            PageNumbers.Clear();
            for (var page = 1; page <= TotalPages; page++)
            {
                PageNumbers.Add(new PageNumberItem(page, page == CurrentPage));
            }
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
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p) => _execute(p);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
