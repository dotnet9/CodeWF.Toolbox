using global::Avalonia.Controls;
using global::Avalonia.Markup.Xaml;
using CodeWF.Avalonia.Controls.Controls;
using CodeWF.Core.Events;
using CodeWF.Toolkit.EventBus;

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

    [EventHandler]
    private void ReceiveThemeChangedCommand(ThemeChangedCommand command)
    {
        var index = _tabControl!.SelectedIndex;
        _tabControl!.SelectedIndex = _tabControl!.ItemCount - 1;
        _tabControl!.SelectedIndex = index;
        //_tabControl?.InvalidateVisual();
    }
}