using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibraryManagementFE.Controls
{
    /// <summary>
    /// Reusable KPI / Metric card.
    /// </summary>
    public partial class MetricCard : UserControl
    {
        // ── Dependency Properties ────────────────────────────────────────

        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(MetricCard),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(MetricCard),
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty IconContentProperty =
            DependencyProperty.Register(nameof(IconContent), typeof(object), typeof(MetricCard));

        public static readonly DependencyProperty BadgeBackgroundProperty =
            DependencyProperty.Register(nameof(BadgeBackground), typeof(Brush), typeof(MetricCard),
                new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty BadgeForegroundProperty =
            DependencyProperty.Register(nameof(BadgeForeground), typeof(Brush), typeof(MetricCard),
                new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty BadgeTextProperty =
            DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(MetricCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(MetricCard),
                new PropertyMetadata("0"));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(MetricCard),
                new PropertyMetadata(string.Empty));

        // ── CLR Wrappers ────────────────────────────────────────────────

        public Brush IconBackground
        {
            get => (Brush)GetValue(IconBackgroundProperty);
            set => SetValue(IconBackgroundProperty, value);
        }

        public Brush IconForeground
        {
            get => (Brush)GetValue(IconForegroundProperty);
            set => SetValue(IconForegroundProperty, value);
        }

        public object IconContent
        {
            get => GetValue(IconContentProperty);
            set => SetValue(IconContentProperty, value);
        }

        public Brush BadgeBackground
        {
            get => (Brush)GetValue(BadgeBackgroundProperty);
            set => SetValue(BadgeBackgroundProperty, value);
        }

        public Brush BadgeForeground
        {
            get => (Brush)GetValue(BadgeForegroundProperty);
            set => SetValue(BadgeForegroundProperty, value);
        }

        public string BadgeText
        {
            get => (string)GetValue(BadgeTextProperty);
            set => SetValue(BadgeTextProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public MetricCard()
        {
            InitializeComponent();
        }
    }
}
