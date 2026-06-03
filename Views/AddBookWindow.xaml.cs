using System.Windows;
using System.Windows.Media.Imaging;
using LibraryManagementFE.Models;
using Microsoft.Win32;

namespace LibraryManagementFE.Views
{
    public partial class AddBookWindow : Window
    {
        private readonly BookRecord? _editingBook;
        private string _coverImagePath = string.Empty;

        public IReadOnlyList<BookRecord> Books { get; private set; } = Array.Empty<BookRecord>();

        public AddBookWindow()
        {
            InitializeComponent();
        }

        public AddBookWindow(BookRecord editingBook) : this()
        {
            _editingBook = editingBook;
            LoadBookForEdit(editingBook);
        }

        private void LoadBookForEdit(BookRecord book)
        {
            Title = "Chỉnh sửa sách";
            FormTitleTextBlock.Text = "Chỉnh sửa sách";
            SaveButtonTextBlock.Text = "Lưu thay đổi";

            TitleTextBox.Text = book.Title;
            AuthorTextBox.Text = book.Author;
            CategoriesTextBox.Text = BuildCategoriesText(book);
            YearTextBox.Text = book.Year.ToString();
            QuantityTextBox.Text = "1";
            QuantityTextBox.IsEnabled = false;

            if (!string.IsNullOrWhiteSpace(book.CoverImagePath))
            {
                SetCoverImage(book.CoverImagePath);
            }
        }

        private void UploadCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh bìa sách",
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) == true)
            {
                SetCoverImage(dialog.FileName);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var title = TitleTextBox.Text.Trim();
            var author = AuthorTextBox.Text.Trim();
            var categories = CategoriesTextBox.Text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(category => category.Trim())
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .ToList();

            if (string.IsNullOrWhiteSpace(title))
            {
                ShowError("Vui lòng nhập tên sách.");
                TitleTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(author))
            {
                ShowError("Vui lòng nhập tác giả.");
                AuthorTextBox.Focus();
                return;
            }

            if (categories.Count == 0)
            {
                ShowError("Vui lòng nhập ít nhất một thể loại.");
                CategoriesTextBox.Focus();
                return;
            }

            if (!int.TryParse(YearTextBox.Text.Trim(), out var year) || year < 1000 || year > 9999)
            {
                ShowError("Năm xuất bản phải là số có 4 chữ số.");
                YearTextBox.Focus();
                return;
            }

            var quantity = 1;
            if (_editingBook is null
                && (!int.TryParse(QuantityTextBox.Text.Trim(), out quantity) || quantity <= 0))
            {
                ShowError("Số lượng sách phải lớn hơn 0.");
                QuantityTextBox.Focus();
                return;
            }

            var categoryLine1 = categories[0];
            var categoryLine2 = categories.Count > 1 ? string.Join(", ", categories.Skip(1)) : string.Empty;
            var (pillBg, pillFg) = GetCategoryColors(categoryLine1);
            var availability = _editingBook?.Availability ?? BookAvailability.SanCo;

            Books = Enumerable.Range(0, quantity)
                .Select(_ => new BookRecord
                {
                    Stt = _editingBook?.Stt ?? 0,
                    Title = title,
                    Author = author,
                    CategoryLine1 = categoryLine1,
                    CategoryLine2 = categoryLine2,
                    CategoryPillBg = pillBg,
                    CategoryPillFg = pillFg,
                    CoverImagePath = _coverImagePath,
                    Year = year,
                    Availability = availability,
                })
                .ToList();

            DialogResult = true;
        }

        private void SetCoverImage(string path)
        {
            _coverImagePath = path;
            CoverPathTextBlock.Text = path;
            CoverPlaceholderText.Visibility = Visibility.Collapsed;
            CoverPreviewImage.Source = new BitmapImage(new Uri(path));
        }

        private static string BuildCategoriesText(BookRecord book)
        {
            var categories = new List<string>();
            if (!string.IsNullOrWhiteSpace(book.CategoryLine1))
            {
                categories.Add(book.CategoryLine1);
            }

            if (!string.IsNullOrWhiteSpace(book.CategoryLine2))
            {
                categories.AddRange(
                    book.CategoryLine2
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(category => category.Trim())
                        .Where(category => !string.IsNullOrWhiteSpace(category)));
            }

            return string.Join(Environment.NewLine, categories);
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private static (string Background, string Foreground) GetCategoryColors(string category)
        {
            var normalized = category.Trim().ToUpperInvariant();
            if (normalized.Contains("CNTT") || normalized.Contains("TECH"))
            {
                return ("#EFF6FF", "#1978E5");
            }

            if (normalized.Contains("VAN") || normalized.Contains("VĂN"))
            {
                return ("#FAF5FF", "#9333EA");
            }

            return ("#ECFDF5", "#047857");
        }
    }
}
