using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Core.Helpers;
using CodeWF.Modules.LogViewer.ViewModels;
using System;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.LogViewer.Views;

public partial class LogViewerView : UserControl
{
    private readonly TextEditor _logEditor;
    private LogViewerViewModel? _viewModel;

    public LogViewerView()
    {
        InitializeComponent();

        _logEditor = this.FindControl<TextEditor>("LogEditor")
            ?? throw new InvalidOperationException("LogEditor 控件未找到。");
        _logEditor.ApplyCodeEditorStyle(enableTextDragDrop: false);
        InstallDefaultHighlighting(_logEditor);

        DataContextChanged += OnDataContextChanged;
        OnDataContextChanged(this, EventArgs.Empty);
    }

    private static void InstallDefaultHighlighting(TextEditor logEditor)
    {
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        var textMateInstallation = logEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".json").Id));
        logEditor.ApplyCodeEditorStyle(enableTextDragDrop: false);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.DetachEditor(_logEditor);
        }

        _viewModel = DataContext as LogViewerViewModel;
        if (_viewModel != null)
        {
            _viewModel.AttachEditor(_logEditor);
        }
    }
}
