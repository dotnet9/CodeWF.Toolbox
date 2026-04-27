using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CodeWF.Modules.LogViewer.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodeWF.Modules.LogViewer.Views;

public partial class LogViewerView : UserControl
{
    private LogViewerViewModel? _viewModel;

    public LogViewerView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.ScrollToTailRequested -= ScrollToTailRequested;
        }

        _viewModel = DataContext as LogViewerViewModel;
        if (_viewModel != null)
        {
            _viewModel.ScrollToTailRequested += ScrollToTailRequested;
        }
    }

    private void ScrollToTailRequested(object? sender, EventArgs e)
    {
        _ = ScrollToTailAfterLayoutAsync();
    }

    private async Task ScrollToTailAfterLayoutAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(ScrollLogLinesToTail, DispatcherPriority.Background);
        await Dispatcher.UIThread.InvokeAsync(ScrollLogLinesToTail, DispatcherPriority.Render);
    }

    private void ScrollLogLinesToTail()
    {
        if (_viewModel?.VisibleLines.LastOrDefault() is not { } lastLine)
        {
            return;
        }

        LogLines.ScrollIntoView(lastLine);

        var scrollViewer = LogLines
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (scrollViewer == null)
        {
            return;
        }

        var maxY = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, maxY);
    }
}
