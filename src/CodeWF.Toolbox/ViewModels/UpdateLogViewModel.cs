using CodeWF.Core.RegionAdapters;
using ReactiveUI;
using System.IO;

namespace CodeWF.Toolbox.ViewModels;

public class UpdateLogViewModel : ViewModelBase, ITabItemBase
{
    public UpdateLogViewModel()
    {
        var path = "UpdateLog.md";
        if (File.Exists(path))
        {
            UpdateLogMarkdownContent = File.ReadAllText(path);
        }
        else
        {
            UpdateLogMarkdownContent = "Empty";
        }
    }

    public string? TitleKey { get; set; } = Localization.UpdateLogView.Title;
    public string? MessageKey { get; set; } = Localization.UpdateLogView.Description;


    public string? UpdateLogMarkdownContent
    {
        get ;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}