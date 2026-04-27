using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Core.Helpers;
using CodeWF.Modules.Development.ViewModels;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.Development.Views;

public partial class JsonPrettifyView : UserControl
{
    public JsonPrettifyView()
    {
        InitializeComponent();

        // AvaloniaEdit 的语法高亮需要在控件加载后绑定 TextMate 语法。
        var formatEditor = this.FindControl<TextEditor>("FormatEditor")
            ?? throw new InvalidOperationException("FormatEditor 控件未找到。");
        formatEditor.ApplyCodeEditorStyle();

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        var textMateInstallation = formatEditor.InstallTextMate(registryOptions);

        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".json").Id));

        var noFormatEditor = this.FindControl<TextBox>("NoFormatEditor")
            ?? throw new InvalidOperationException("NoFormatEditor 控件未找到。");

        if (DataContext is JsonPrettifyViewModel vm)
        {
            vm.FormatEditor = formatEditor;
            vm.NoFormatEditor = noFormatEditor;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
