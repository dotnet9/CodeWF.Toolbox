using CodeWF.Core.RegionAdapters;
using CodeWF.Toolkit.Core.Extensions;
using CodeWF.Toolkit.Core.UpdateChecking;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CodeWF.Toolbox.ViewModels;

public sealed class AboutViewModel : ViewModelBase, ITabItemBase, IDisposable
{
    public string? TitleKey { get; set; } = Localization.AboutView.Title;
    public string? MessageKey { get; set; } = Localization.AboutView.Description;
    public string? Product { get; } = typeof(App).Assembly.Product();
    public string? Version { get; } = typeof(App).Assembly.Version();
    public string Platform { get; } = $"{RuntimeInformation.OSDescription} / {RuntimeInformation.ProcessArchitecture} · Avalonia {typeof(global::Avalonia.Application).Assembly.GetName().Version} · {RuntimeInformation.FrameworkDescription}";
    public string? Copyright { get; } = typeof(App).Assembly.Copyright();
    public string? Author { get; } = typeof(App).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(attribute => attribute.Key == "Author")?.Value;
    public string? CompileTime { get; } = typeof(App).Assembly.CompileTime()?.ToString("yyyy-MM-dd HH:mm:ss");
    public UpdateViewModel Update { get; }
    public AboutViewModel(IUpdateService updates) => Update = new UpdateViewModel(updates);
    public void Dispose() => Update.Dispose();
}
