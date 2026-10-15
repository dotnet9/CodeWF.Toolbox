using Avalonia.Threading;
using CodeWF.Core.Models;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolbox.Views;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeWF.Toolbox.Services;

/// <summary>每个工具 URI 对应一个文档与一个独立导航区域，切换时保留输入和执行结果。</summary>
public sealed class ToolWorkspace
{
    private readonly IRegionManager _regions;
    private readonly Dictionary<string, Document> _documents = new(StringComparer.Ordinal);
    public Factory Factory { get; } = new();
    public DocumentDock Documents { get; }
    public RootDock Layout { get; }
    public IReadOnlyCollection<Document> OpenDocuments => _documents.Values;

    private Document? ActiveDocument => Layout.FocusedDockable as Document ??
        _documents.Values.FirstOrDefault(document => (document.Owner as IDock)?.ActiveDockable == document);

    public ToolWorkspace(IRegionManager regions)
    {
        _regions = regions;
        Documents = new DocumentDock
        {
            Id = "Tools", CanCreateDocument = false, IsCollapsable = false,
            VisibleDockables = Factory.CreateList<IDockable>()
        };
        Layout = new RootDock
        {
            Id = "Workspace", IsCollapsable = false,
            VisibleDockables = Factory.CreateList<IDockable>(Documents), ActiveDockable = Documents
        };
        Factory.InitLayout(Layout);
        Factory.DockableClosed += (_, args) =>
        {
            if (args.Dockable is not Document document) return;
            var entry = _documents.FirstOrDefault(pair => ReferenceEquals(pair.Value, document));
            if (entry.Key is not null) _documents.Remove(entry.Key);
            if (global::Avalonia.Application.Current?.Resources.TryGetResource("ToolDocumentRecycling", null, out var cache) == true &&
                cache is global::Avalonia.Controls.Recycling.ControlRecycling recycling)
                recycling.Remove(document);
            (document.Context as ToolDocumentView)?.Dispose();
        };
        I18nManager.Instance.CultureChanged += (_, _) =>
        {
            foreach (var document in _documents.Values)
                if (document.Context is ToolDocumentView view) document.Title = Translate(view.Item.Name);
        };
    }

    public void Open(ToolMenuItem item)
    {
        if (string.IsNullOrWhiteSpace(item.ViewName)) return;
        if (_documents.TryGetValue(item.ViewName, out var existing))
        {
            Factory.SetActiveDockable(existing);
            Factory.SetFocusedDockable(Layout, existing);
            return;
        }
        var document = new Document
        {
            Id = item.ViewName, Title = Translate(item.Name), CanClose = true, CanFloat = true,
            Context = new ToolDocumentView(item, _regions)
        };
        _documents.Add(item.ViewName, document);
        Factory.AddDockable(Documents, document);
        Factory.SetActiveDockable(document);
        Factory.SetFocusedDockable(Layout, document);
    }

    public void CloseActive()
    {
        if (ActiveDocument is { } active) Factory.CloseDockable(active);
    }

    public void CloseAll()
    {
        foreach (var document in _documents.Values.ToArray()) Factory.CloseDockable(document);
    }

    public void SplitActive()
    {
        if (ActiveDocument is { Owner: IDock owner } active && owner.VisibleDockables?.Count > 1)
            Factory.SplitToDock(owner, active, DockOperation.Right);
    }

    public static string Translate(string? key) => string.IsNullOrWhiteSpace(key) ? string.Empty : I18nManager.Instance.GetResource(key);
}
