using Avalonia.Controls;
using Avalonia.Data;
using Lang.Avalonia.MarkupExtensions;
using Prism.Regions;
using System.Collections.Specialized;

namespace CodeWF.Core.RegionAdapters;

public interface ITabItemBase
{
    public string? TitleKey { get; set; }
    public string? MessageKey { get; set; }
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

        // Prism 默认不认识 Ursa/Semi 主题下的 TabControl 该如何承载 Region，
        // 这里把每个 View 包装成 TabItem，并把 ViewModel 暴露的语言 Key 绑定到页签头。
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
                                    ? tabItem.TitleKey ?? item.GetType().ToString()
                                    : item?.GetType().ToString() ?? string.Empty;
                                var newTabItem = new TabItem { Content = item };
                                if (new I18nBinding(header).ProvideValue(null!) is BindingBase headerBinding)
                                {
                                    newTabItem.Bind(TabItem.HeaderProperty, headerBinding);
                                }
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
                                if (tabToDelete != null)
                                {
                                    regionTarget.Items.Remove(tabToDelete);
                                }
                            }
                        }

                        break;
                    }
            }
        };
    }

    protected override IRegion CreateRegion() => new SingleActiveRegion();
}

