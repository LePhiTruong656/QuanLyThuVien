using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementFE.ViewModels;

namespace LibraryManagementFE.Views
{
    public partial class BorrowReturnView : UserControl
    {
        public BorrowReturnView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RegisterViewModel(DataContext as BorrowReturnViewModel);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RegisterViewModel(e.NewValue as BorrowReturnViewModel);
        }

        private void RegisterViewModel(BorrowReturnViewModel? viewModel)
        {
            if (viewModel == null)
                return;

            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BorrowReturnViewModel.SelectedBorrow))
                ScrollSelectedBorrowIntoView();
        }

        private void ScrollSelectedBorrowIntoView()
        {
            if (BorrowListView?.SelectedItem != null)
            {
                BorrowListView.ScrollIntoView(BorrowListView.SelectedItem);
            }
        }

        private void ReturnSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BorrowReturnViewModel viewModel)
            {
                viewModel.ShowFilterPanel = true;
            }

            // Wait for layout to update then scroll the main ScrollViewer so the filter, header and list are visible
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Action(() =>
            {
                try
                {
                    if (MainScrollViewer != null)
                    {
                        var target = SearchToolbarGrid ?? FilterPanel ?? (FrameworkElement)BorrowListView;
                        var transform = target.TransformToAncestor(MainScrollViewer);
                        var point = transform.Transform(new System.Windows.Point(0, 0));
                        // scroll slightly above the target so search/filter are visible
                        var desired = MainScrollViewer.VerticalOffset + point.Y - 16;
                        if (desired < 0) desired = 0;
                        MainScrollViewer.ScrollToVerticalOffset(desired);
                    }

                    if (BorrowListView?.SelectedItem != null)
                        BorrowListView.ScrollIntoView(BorrowListView.SelectedItem);
                }
                catch
                {
                    FilterPanel?.BringIntoView();
                    BorrowListView?.BringIntoView();
                }
            }));
        }
    }
}
