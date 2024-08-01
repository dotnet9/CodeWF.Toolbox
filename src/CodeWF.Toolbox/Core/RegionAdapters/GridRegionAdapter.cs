using Avalonia.Controls;
using Prism.Regions;
using System.Collections.Specialized;

namespace CodeWF.Toolbox.Core.RegionAdapters;

public class GridRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
    : RegionAdapterBase<Grid>(regionBehaviorFactory)
{
    protected override void Adapt(IRegion region, Grid regionTarget)
    {
        region.Views.CollectionChanged += (sender, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                foreach (Control item in e.NewItems)
                {
                    regionTarget.Children.Add(item);
                }
            }

            if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            {
                foreach (Control item in e.OldItems)
                {
                    regionTarget.Children.Remove(item);
                }
            }
        };
    }

    protected override IRegion CreateRegion()
    {
        return new SingleActiveRegion();
    }
}