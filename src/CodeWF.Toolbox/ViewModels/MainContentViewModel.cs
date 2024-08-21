using Avalonia.Controls.Notifications;
using CodeWF.Core.Models;
using CodeWF.EventBus;
using CodeWF.Toolbox.Commands;
using ReactiveUI;

namespace CodeWF.Toolbox.ViewModels;

internal class MainContentViewModel : ViewModelBase
{
    private bool _bordered;

    public bool Bordered
    {
        get => _bordered;
        set => this.RaiseAndSetIfChanged(ref _bordered, value);
    }

    private NotificationType _selectedType;

    public NotificationType SelectedType
    {
        get => _selectedType;
        set => this.RaiseAndSetIfChanged(ref _selectedType, value);
    }

    private ToolMenuItem? _selectedMenuItem;

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
    }

    public MainContentViewModel()
    {
        EventBus.EventBus.Default.Subscribe(this);
    }

    [EventHandler]
    private void ChangeToolMenuHandler(ChangeToolMenuCommand command)
    {
        SelectedMenuItem = command.ToolMenuItem;
        Bordered = SelectedMenuItem.Status == ToolStatus.Developing;
        SelectedType = SelectedMenuItem.Status == ToolStatus.Complete
            ? NotificationType.Success
            : NotificationType.Warning;
    }
}