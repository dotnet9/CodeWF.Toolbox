using Avalonia.Controls;
using AvaloniaXmlTranslator.Markup;
using System.Collections.Specialized;

namespace CodeWF.Core.RegionAdapters;

public interface ITabItemBase
{
    public string? TitleKey { get; set; }
    public string MessageKey { get; set; }
}

public class TabControlRegionAdapter : RegionAdapterBase<TabControl>
{
    public TabControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, TabControl regionTarget)
    {
        if (region == null)
            throw new ArgumentNullException(nameof(region));

        if (regionTarget == null)
            throw new ArgumentNullException(nameof(regionTarget));

        regionTarget.SelectionChanged += (s, e) =>
        {
            if (regionTarget.SelectedItem is TabItem { Content: UserControl { DataContext: ITabItemBase vm } })
            {
                regionTarget.Tag = vm.MessageKey;
            }
        };

        region.Views.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                var header = item is UserControl { DataContext: ITabItemBase tabItem }
                                    ? tabItem.TitleKey
                                    : item?.GetType().ToString();
                                var newTabItem = new TabItem { Content = item };
                                newTabItem.Bind(TabItem.HeaderProperty, new I18nBinding(header));
                                regionTarget.Items.Add(newTabItem);
                            }
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                var tabToDelete = regionTarget.Items.OfType<TabItem>()
                                    .FirstOrDefault(n => n.Content == item);
                                regionTarget.Items.Remove(tabToDelete);
                            }
                        }

                        break;
                    }
            }
        };
    }

    protected override IRegion CreateRegion() => new SingleActiveRegion();
}