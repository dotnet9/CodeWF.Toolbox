using Avalonia.Controls.Notifications;
using CodeWF.Core;
using CodeWF.Core.Models;
using Prism.Regions;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeWF.Toolbox.ViewModels;

internal class MainMenuViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    private readonly IToolMenuService _toolMenuService;
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

    internal MainMenuViewModel(IRegionManager regionManager, IToolMenuService toolMenuService)
    {
        _regionManager = regionManager;
        _toolMenuService = toolMenuService;

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
    }
}