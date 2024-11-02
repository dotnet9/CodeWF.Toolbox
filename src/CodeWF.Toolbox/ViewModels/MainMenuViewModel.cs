using Avalonia.Controls.Notifications;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core;
using CodeWF.Core.Models;
using CodeWF.Toolbox.Commands;
using CodeWF.Toolbox.I18n;
using CodeWF.Toolbox.Views;
using DryIoc;
using Prism.Ioc;
using Prism.Regions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using Ursa.Controls;
using Ursa.PrismExtension;

namespace CodeWF.Toolbox.ViewModels;

internal class MainMenuViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IToolMenuService _toolMenuService;
    private readonly IContainerExtension _container;
    private readonly IUrsaOverlayDialogService _overlayDialogService;
    public ObservableCollection<ToolMenuItem>? MenuItems { get; }

    private ToolMenuItem? _selectedMenuItem;

    public ToolMenuItem? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
            ChangeTool();
        }
    }

    private NotificationType _selectedMenuStatus;

    public NotificationType SelectedMenuStatus
    {
        get => _selectedMenuStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMenuStatus, value);
    }

    internal MainMenuViewModel(IRegionManager regionManager, IToolMenuService toolMenuService,
        IContainerExtension container, IUrsaOverlayDialogService overlayDialogService)
    {
        _regionManager = regionManager;
        _toolMenuService = toolMenuService;
        _container = container;
        _overlayDialogService = overlayDialogService;

        MenuItems = _toolMenuService.MenuItems;
        _toolMenuService.ToolMenuChanged += MenuChangedHandler;
    }

    private void MenuChangedHandler()
    {
        SelectedMenuItem = SelectedMenuItem == null ? MenuItems?.First() : GetMenuItem(SelectedMenuItem.Name!);
    }

    private ToolMenuItem? GetMenuItem(string name)
    {
        if (MenuItems == null) return default;

        foreach (ToolMenuItem firstMenuItem in MenuItems)
        {
            if (name == firstMenuItem.Name)
            {
                return firstMenuItem;
            }

            foreach (ToolMenuItem secondMenuItem in firstMenuItem.Children)
            {
                if (name == secondMenuItem.Name)
                {
                    return secondMenuItem;
                }
            }
        }

        return default;
    }

    private void ChangeTool()
    {
        _regionManager.RequestNavigate(RegionNames.ContentRegion, _selectedMenuItem?.ViewName);
        SelectedMenuStatus = _selectedMenuItem?.Status switch
        {
            ToolStatus.Planned => NotificationType.Warning,
            ToolStatus.Developing => NotificationType.Information,
            ToolStatus.Complete => NotificationType.Success,
            _ => NotificationType.Information
        };
        EventBus.EventBus.Default.Publish(new ChangeToolMenuCommand(_selectedMenuItem!));
    }

    public async void RaiseOpenSettingHandlerAsync()
    {
        var option =
            new OverlayDialogOptions { Title = I18nManager.GetString(Language.Setting), Buttons = DialogButton.OK };
        var vm = ContainerLocator.Current.Resolve<SettingViewModel>();
        await _overlayDialogService.ShowModal(DialogNames.Setting, vm, HostIds.Main, option);
    }
}