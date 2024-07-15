using Prism.Regions;

namespace CodeWF.Tools.Desktop.Core.RegionAdapters;

public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel>
{
    public StackPanelRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
        : base(regionBehaviorFactory)
    {
    }

    protected override void Adapt(IRegion region, StackPanel regionTarget)
    {
        region.Views.CollectionChanged += (sender, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                foreach (Control item in e.NewItems)
                {
                    if (e.NewItems != null)
                    {
                        regionTarget.Children.Add(item);
                    }
                }
            }

            if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            {
                foreach (Control item in e.OldItems)
                {
                    if (e.OldItems != null)
                    {
                        regionTarget.Children.Remove(item);
                    }
                }
            }
        };
    }

    protected override IRegion CreateRegion()
    {
        return new SingleActiveRegion();
    }
}