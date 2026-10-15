using Avalonia.Controls;
using Avalonia.Threading;
using CodeWF.Core.Models;
using CodeWF.Modules.ToolFramework.Views;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolbox.Services;
using Prism.Regions;
using System;

namespace CodeWF.Toolbox.Views;

public partial class ToolDocumentView : UserControl, IDisposable
{
    private readonly IRegionManager _regions;
    private readonly string _regionName = "Document_" + Guid.NewGuid().ToString("N");
    private bool _navigated;
    private bool _disposed;
    public ToolMenuItem Item { get; }
    public object? ToolModel => (ToolHost.Content as Control)?.DataContext;

    public ToolDocumentView() : this(new ToolMenuItem(), new RegionManager()) { }

    public ToolDocumentView(ToolMenuItem item, IRegionManager regions)
    {
        Item = item;
        _regions = regions;
        InitializeComponent();
        RegionManager.SetRegionManager(ToolHost, regions);
        RegionManager.SetRegionName(ToolHost, _regionName);
        I18nManager.Instance.CultureChanged += OnCultureChanged;
        OnCultureChanged(null, EventArgs.Empty);
        AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(Navigate);
    }

    private void Navigate()
    {
        if (_navigated || _disposed) return;
        RegionManager.UpdateRegions();
        if (!_regions.Regions.ContainsRegionWithName(_regionName)) return;
        _navigated = true;
        _regions.RequestNavigate(_regionName, Item.ViewName, result =>
        {
            if (result.Error is { } error) ToolHost.Content = new TextBlock { Text = error.Message, TextWrapping = global::Avalonia.Media.TextWrapping.Wrap };
            if (ToolHost.Content is ToolView)
            {
                PropertyPanel.IsVisible = PropertySplitter.IsVisible = false;
                DocumentGrid.ColumnDefinitions[2].Width = new GridLength(0);
                DocumentGrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        });
    }

    private void OnCultureChanged(object? sender, EventArgs args)
    {
        if (_disposed) return;
        if (!Dispatcher.UIThread.CheckAccess()) { Dispatcher.UIThread.Post(() => OnCultureChanged(sender, args)); return; }
        ToolName.Text = ToolWorkspace.Translate(Item.Name);
        ToolDescription.Text = ToolWorkspace.Translate(Item.Description);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        I18nManager.Instance.CultureChanged -= OnCultureChanged;
        (ToolModel as IDisposable)?.Dispose();
        if (_regions.Regions.ContainsRegionWithName(_regionName)) _regions.Regions.Remove(_regionName);
    }
}
