using Prism.Regions;
using System.Diagnostics;

namespace CodeWF.Tools.Common;

/// <summary>
///     Adapts TabControl's TabItem (content control) to a Prism Region
///     so that you can hook Regions to the TabControl in XAML.
///     <code><![CDATA[
///   <TabControl prism:RegionManager.RegionName="MailTabRegion" />
/// ]]></code>
///     Tab Control Adapter for hooking tabs to regions. a UserControl as a TabItem
///     * Tab Header: UserControl's `Tag` property
/// </summary>
public class TabControlAdapter : RegionAdapterBase<TabControl>
{
    public TabControlAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, TabControl regionTarget)
    {
        if (region == null)
        {
            throw new ArgumentNullException(nameof(region));
        }

        if (regionTarget == null)
        {
            throw new ArgumentNullException(nameof(regionTarget));
        }

        // Detect a Tab Selection Changed
        regionTarget.SelectionChanged += (s, e) =>
        {
            // The view navigating away from
            foreach (object? item in e.RemovedItems)
            {
                // NOTE: The selected item isn't always a TabItem, if the region contains
                //       a ListBox, it's SelecitonChange gets picked up.
                TargetSelectionChanged("Deactivating", item);
                //// region.Deactivate(item);
            }

            // The view navigating to
            foreach (object? item in e.AddedItems)
            {
                TargetSelectionChanged("Activating", item);
                ////region.Activate(item);
            }
        };

        // Detect when a TabItem has been added/removed to the TabControl
        region.Views.CollectionChanged += (s, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                foreach (UserControl item in e.NewItems)
                {
                    List<TabItem> items = regionTarget.Items.Cast<TabItem>().ToList();
                    items.Add(new TabItem { Header = item.Tag, Content = item });
                    //regionTarget.Items = items;           // Avalonia v0.10.x
                    //// regionTarget.Items.Set(items);   // Avalonia v11
                }
            }
            else if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            {
                foreach (UserControl item in e.OldItems)
                {
                    TabItem? tabToDelete = regionTarget.Items.OfType<TabItem>().FirstOrDefault(n => n.Content == item);
                    // regionTarget.Items.Remove(tabToDelete);  // WPF

                    List<TabItem> items = regionTarget.Items.Cast<TabItem>().ToList();
                    items.Remove(tabToDelete!);
                    //regionTarget.Items = items;
                    //// regionTarget.Items.Set(items);   // Avalonia v11
                }
            }
        };
    }

    /// <summary>
    ///     AllActiveRegion    - Can have multiple active views at the same time (i.e. multi-window views)
    ///     SingleActiveRegion - There is only one view active at a time. (i.e. Tab)
    /// </summary>
    /// <returns>Region</returns>
    protected override IRegion CreateRegion()
    {
        return new SingleActiveRegion();
    }

    private void TargetSelectionChanged(string changeAction, object itemChanged)
    {
        // The selected item isn't always a TabItem.
        // In some cases, it could be the Region's ListBox item

        TabItem? item = itemChanged as TabItem;
        if (item is null)
        {
            return;
        }

        Debug.WriteLine($"Tab {changeAction} (Header):    " + item.Header);
        Debug.WriteLine($"Tab {changeAction} (View):      " + item.Content);
        Debug.WriteLine($"Tab {changeAction} (ViewModel): " + item.DataContext);
    }
}