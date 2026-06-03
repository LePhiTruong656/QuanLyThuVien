using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibraryManagementFE.Controls
{
    public partial class CategoryBarItem : UserControl
    {
        public static readonly DependencyProperty CategoryNameProperty =
            DependencyProperty.Register(nameof(CategoryName), typeof(string), typeof(CategoryBarItem),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CategoryCountProperty =
            DependencyProperty.Register(nameof(CategoryCount), typeof(string), typeof(CategoryBarItem),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(nameof(Percentage), typeof(double), typeof(CategoryBarItem),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty BarColorProperty =
            DependencyProperty.Register(nameof(BarColor), typeof(Brush), typeof(CategoryBarItem),
                new PropertyMetadata(Brushes.Blue));

        public string CategoryName
        {
            get => (string)GetValue(CategoryNameProperty);
            set => SetValue(CategoryNameProperty, value);
        }

        public string CategoryCount
        {
            get => (string)GetValue(CategoryCountProperty);
            set => SetValue(CategoryCountProperty, value);
        }

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public Brush BarColor
        {
            get => (Brush)GetValue(BarColorProperty);
            set => SetValue(BarColorProperty, value);
        }

        public CategoryBarItem()
        {
            InitializeComponent();
        }
    }
}
