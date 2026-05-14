using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LibraryManagementFE.Views;

namespace LibraryManagementFE
{
    public partial class MainWindow : Window
    {
        // map nav tag → nav button
        private readonly Dictionary<string, Button> _navButtons;
        // map nav tag → page factory
        private readonly Dictionary<string, Func<UIElement>> _pages;

        public MainWindow()
        {
            InitializeComponent();

            _navButtons = new Dictionary<string, Button>
            {
                ["dashboard"] = NavDashboard,
                ["books"]     = NavBooks,
                ["readers"]   = NavReaders,
                ["borrow"]    = NavBorrow,
                ["reports"]   = NavReports,
            };

            _pages = new Dictionary<string, Func<UIElement>>
            {
                ["dashboard"] = () => new DashboardView(),
                ["books"]     = () => new BooksView(),
                ["readers"]   = () => new ReadersView(),
                ["borrow"]    = () => new BorrowReturnView(),
                ["reports"]   = () => new ReportsStatisticsView(),
            };

            Loaded += (_, _) => ApplyNavSidebarVisuals("dashboard");
        }

        // ── Sidebar navigation ──────────────────────────────────────────
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string tag) return;

            // Reset all buttons to inactive style
            foreach (var kv in _navButtons)
                kv.Value.Style = (Style)FindResource("NavLink");

            // Activate clicked button
            btn.Style = (Style)FindResource("NavLink.Active");

            ApplyNavSidebarVisuals(tag);

            // Swap page content
            if (_pages.TryGetValue(tag, out var factory))
                PageContent.Content = factory();

            // Update topbar title
            PageTitle.Text = tag switch
            {
                "dashboard" => "Tổng quan Hệ thống",
                "books"     => "Quản lý Sách",
                "readers"   => "Quản lý Độc giả",
                "borrow"    => "Quản lý Mượn/Trả",
                "reports"   => "Báo cáo Thống kê",
                _           => "Hệ thống Thư viện",
            };
        }

        /// <summary>Mục đang chọn: chữ + icon trắng trên nền brand; các mục khác: slate (không phải trắng trên trắng).</summary>
        private void ApplyNavSidebarVisuals(string activeTag)
        {
            Brush muted = TryFindResource("TextMutedBrush") as Brush
                         ?? new SolidColorBrush(Color.FromRgb(100, 116, 139));
            Brush white = TryFindResource("WhiteBrush") as Brush ?? Brushes.White;

            bool dash = activeTag == "dashboard";
            NavDashboardTitle.Foreground = dash ? white : muted;
            NavDashSq1.Fill = dash ? white : muted;
            NavDashSq2.Fill = dash ? white : muted;
            NavDashSq3.Fill = dash ? white : muted;
            NavDashSq4.Fill = dash ? white : muted;

            ApplyStrokeNavItem(activeTag == "books", NavBooksTitle, NavBooks, white, muted);
            ApplyStrokeNavItem(activeTag == "readers", NavReadersTitle, NavReaders, white, muted);
            ApplyStrokeNavItem(activeTag == "borrow", NavBorrowTitle, NavBorrow, white, muted);
            ApplyStrokeNavItem(activeTag == "reports", NavReportsTitle, NavReports, white, muted);
        }

        private static void ApplyStrokeNavItem(bool selected, TextBlock title, Button btn, Brush white, Brush muted)
        {
            Brush fg = selected ? white : muted;
            title.Foreground = fg;
            TintShapesUnderViewbox(btn, fg);
        }

        private static void TintShapesUnderViewbox(Button btn, Brush brush)
        {
            if (btn.Content is not StackPanel sp) return;
            foreach (var vb in sp.Children.OfType<Viewbox>())
                TintVisualDescendantShapes(vb, brush);
        }

        private static void TintVisualDescendantShapes(DependencyObject root, Brush brush)
        {
            int n = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < n; i++)
            {
                var ch = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (ch is Shape shape)
                    ApplyNavGlyphBrush(shape, brush);
                TintVisualDescendantShapes(ch, brush);
            }
        }

        /// <summary>Glyphs vẽ bằng Stroke → tô Stroke; chỉ Fill (Không Stroke) như ô dashboard → tô Fill.</summary>
        private static void ApplyNavGlyphBrush(Shape shape, Brush brush)
        {
            bool strokeExplicit = shape.ReadLocalValue(Shape.StrokeProperty) != DependencyProperty.UnsetValue
                                  && shape.Stroke != null
                                  && !ReferenceEquals(shape.Stroke, Brushes.Transparent);

            bool opaqueFill = shape.ReadLocalValue(Shape.FillProperty) != DependencyProperty.UnsetValue
                              && shape.Fill != null
                              && !ReferenceEquals(shape.Fill, Brushes.Transparent);

            if (strokeExplicit)
                shape.Stroke = brush;
            else if (opaqueFill)
                shape.Fill = brush;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        // ── Placeholder page for unimplemented sections ─────────────────
        private static UIElement BuildPlaceholder(string title)
        {
            return new Border
            {
                Padding = new Thickness(40),
                Child = new StackPanel
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment   = System.Windows.VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(0x19, 0x1B, 0x24)),
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 12),
                        },
                        new TextBlock
                        {
                            Text = "Chức năng đang được phát triển...",
                            FontSize = 14,
                            Foreground = new System.Windows.Media.SolidColorBrush(
                                System.Windows.Media.Color.FromRgb(0x64, 0x74, 0x8B)),
                            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        }
                    }
                }
            };
        }
    }
}