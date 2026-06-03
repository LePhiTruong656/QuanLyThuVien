using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LibraryManagementFE.Models;
using LibraryManagementFE.Services;
using LibraryManagementFE.Views;

namespace LibraryManagementFE.ViewModels
{
    /// <summary>MVVM layer for Quan ly Sach (Figma: search, table, pagination, stats).</summary>
    public class BooksViewModel : INotifyPropertyChanged
    {
        private const int PageSize = 5;

        public ObservableCollection<BookRecord> Books { get; } = new();
        private readonly LibraryDataStore _store;

        public ObservableCollection<BookRecord> PagedBooks { get; } = new();
        public ObservableCollection<PageNumberItem> PageNumbers { get; } = new();

        public string TotalBooksDisplay => Books.Count.ToString();
        public string NewThisMonthDisplay => Books.Count(book =>
            book.AddedDate.Month == DateTime.Now.Month && book.AddedDate.Year == DateTime.Now.Year).ToString();
        public string BorrowedDisplay => Books.Count(book => book.Availability == BookAvailability.DangMuon).ToString();

        private int? _filterYear;
        private BookAvailability? _filterAvailability;
        private DateTime? _filterAddedFrom;
        private DateTime? _filterAddedTo;

        private IEnumerable<BookRecord> FilteredBooks
        {
            get
            {
                IEnumerable<BookRecord> query = Books;

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var keyword = SearchText.Trim();
                    query = query.Where(book =>
                        book.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        book.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        book.CategoryLine1.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        book.CategoryLine2.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                }

                if (_filterYear is int year)
                {
                    query = query.Where(book => book.Year == year);
                }

                if (_filterAvailability is BookAvailability availability)
                {
                    query = query.Where(book => book.Availability == availability);
                }

                if (_filterAddedFrom is DateTime addedFrom)
                {
                    query = query.Where(book => book.AddedDate.Date >= addedFrom.Date);
                }

                if (_filterAddedTo is DateTime addedTo)
                {
                    query = query.Where(book => book.AddedDate.Date <= addedTo.Date);
                }

                return query;
            }
        }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredBooks.Count() / (double)PageSize));

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
                var total = FilteredBooks.Count();
                if (total == 0)
                {
                    return "Hiển thị 0 của 0 đầu sách";
                }

                var start = ((CurrentPage - 1) * PageSize) + 1;
                var end = Math.Min(CurrentPage * PageSize, total);
                return $"Hiển thị {start} - {end} của {total} đầu sách";
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

        public ICommand FilterCommand { get; }
        public ICommand AddBookCommand { get; }
        public ICommand EditBookCommand { get; }
        public ICommand DeleteBookCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public BooksViewModel()
        {
            _store = LibraryDataStoreFile.LoadOrCreate();
            // populate from shared data store so menus and borrow/return use same book data
            Books.Clear();
            foreach (var b in _store.Books)
                Books.Add(b);

            FilterCommand = new RelayCommand(OpenFilterDialog);

            AddBookCommand = new RelayCommand(OpenAddBookDialog);

            EditBookCommand = new RelayCommand(p =>
            {
                if (p is BookRecord book)
                {
                    OpenEditBookDialog(book);
                }
            });

            DeleteBookCommand = new RelayCommand(p =>
            {
                if (p is BookRecord book)
                {
                    OpenDeleteBookDialog(book);
                }
            });

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

            RefreshPagedBooks();
        }

        private void OpenAddBookDialog(object? _)
        {
            var dialog = new AddBookWindow
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var book in dialog.Books)
                {
                    book.Stt = Books.Count + 1;
                    // ensure book has an Id for persistence and lookup
                    if (string.IsNullOrWhiteSpace(book.Id))
                        book.Id = System.Guid.NewGuid().ToString("N");
                    Books.Add(book);
                    _store.Books.Add(book);
                    LibraryDataStoreFile.Save(_store);
                }

                RefreshStats();
                GoToPage(TotalPages);
            }
        }

        private void OpenFilterDialog(object? _)
        {
            var dialog = new BookFilterWindow(
                _filterYear,
                _filterAvailability,
                _filterAddedFrom,
                _filterAddedTo)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true)
            {
                _filterYear = dialog.SelectedYear;
                _filterAvailability = dialog.SelectedAvailability;
                _filterAddedFrom = dialog.AddedFrom;
                _filterAddedTo = dialog.AddedTo;
                GoToPage(1);
            }
        }

        private void OpenEditBookDialog(BookRecord book)
        {
            var dialog = new AddBookWindow(book)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true && dialog.Books.FirstOrDefault() is BookRecord editedBook)
            {
                var index = Books.IndexOf(book);
                if (index < 0)
                {
                    return;
                }

                editedBook.Stt = book.Stt;
                Books[index] = editedBook;
                // update shared store
                var storeIndex = _store.Books.FindIndex(b => b.Id == book.Id);
                if (storeIndex >= 0)
                {
                    editedBook.Id = _store.Books[storeIndex].Id;
                    _store.Books[storeIndex] = editedBook;
                    LibraryDataStoreFile.Save(_store);
                }
                RefreshStats();
                RefreshPagedBooks();
            }
        }

        private void OpenDeleteBookDialog(BookRecord book)
        {
            var dialog = new ConfirmDeleteWindow("sách", book.Title)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (dialog.ShowDialog() == true)
            {
                var index = Books.IndexOf(book);
                Books.Remove(book);
                // remove from shared store
                var storeIndex = _store.Books.FindIndex(b => b.Id == book.Id);
                if (storeIndex >= 0)
                {
                    _store.Books.RemoveAt(storeIndex);
                    LibraryDataStoreFile.Save(_store);
                }
                ReorderBooks(index);
                RefreshStats();
                GoToPage(Math.Min(CurrentPage, TotalPages));
            }
        }

        private void ReorderBooks(int startIndex)
        {
            for (var index = Math.Max(0, startIndex); index < Books.Count; index++)
            {
                Books[index].Stt = index + 1;
            }
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(TotalBooksDisplay));
            OnPropertyChanged(nameof(NewThisMonthDisplay));
            OnPropertyChanged(nameof(BorrowedDisplay));
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PaginationInfo));
            CommandManager.InvalidateRequerySuggested();
        }

        private void GoToPage(int page)
        {
            CurrentPage = Math.Clamp(page, 1, TotalPages);
            RefreshPagedBooks();
        }

        private void RefreshPagedBooks()
        {
            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }

            var filteredBooks = FilteredBooks.ToList();
            PagedBooks.Clear();
            foreach (var book in filteredBooks.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            {
                PagedBooks.Add(book);
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
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PageNumberItem
    {
        public PageNumberItem(int number, bool isCurrent)
        {
            Number = number;
            IsCurrent = isCurrent;
        }

        public int Number { get; }
        public bool IsCurrent { get; }
    }
}
