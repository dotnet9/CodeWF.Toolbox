using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CodeWF.Core.Events;
using CodeWF.EventBus;
using Ursa.Controls;

namespace CodeWF.Toolbox.Views;

public partial class SettingView : UrsaWindow
{
    private readonly TabControl? _tabControl;
    public SettingView()
    {
        InitializeComponent();
        _tabControl = this.FindControl<TabControl>(nameof(MyTab));
        EventBus.EventBus.Default.Subscribe(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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