using global::Avalonia.Controls;
using global::Avalonia.Markup.Xaml;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Core.Events;
using CodeWF.Toolkit.EventBus;
using System;
using System.Linq;
using Prism.Ioc;
using Prism.Regions;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : CodeWFWindow
{
    private readonly TabControl? _tabControl;
    public SettingView()
    {
        InitializeComponent();
        _tabControl = this.FindControl<TabControl>(nameof(MyTab));
        EventBus.Default.Subscribe(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        foreach (var item in _tabControl?.Items.OfType<TabItem>() ?? [])
            if (item.Content is AboutView { DataContext: IDisposable model }) model.Dispose();
        var regions = ContainerLocator.Container.Resolve<IRegionManager>();
        if (regions.Regions.ContainsRegionWithName(CodeWF.Core.RegionNames.SettingRegion))
            regions.Regions.Remove(CodeWF.Core.RegionNames.SettingRegion);
        EventBus.Default.Unsubscribe(this);
        base.OnClosed(e);
    }

    [EventHandler]
    private void ReceiveThemeChangedCommand(ThemeChangedCommand command)
    {
        var index = _tabControl!.SelectedIndex;
        _tabControl!.SelectedIndex = _tabControl!.ItemCount - 1;
        _tabControl!.SelectedIndex = index;
        //_tabControl?.InvalidateVisual();
    }
}
