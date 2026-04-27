using CodeWF.Core.RegionAdapters;
using ReactiveUI;
using System;
using System.IO;

namespace CodeWF.Toolbox.ViewModels;

public class UpdateLogViewModel : ViewModelBase, ITabItemBase
{
    public UpdateLogViewModel()
    {
        // 发布后工作目录不一定是程序目录，读取随应用打包的更新日志要以 BaseDirectory 为准。
        var path = Path.Combine(AppContext.BaseDirectory, "UpdateLog.md");
        if (File.Exists(path))
        {
            UpdateLogMarkdownContent = File.ReadAllText(path);
        }
        else
        {
            UpdateLogMarkdownContent = "## 更新日志\n\n暂无内容。";
        }
    }

    public string? TitleKey { get; set; } = Localization.UpdateLogView.Title;
    public string? MessageKey { get; set; } = Localization.UpdateLogView.Description;


    public string? UpdateLogMarkdownContent
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}
