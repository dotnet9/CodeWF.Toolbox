using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Core.Helpers;
using CodeWF.Modules.Development.ViewModels;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.Development.Views;

public partial class YamlPrettifyView : UserControl
{
    public YamlPrettifyView()
    {
        InitializeComponent();

        // AvaloniaEdit 的语法高亮需要在控件加载后绑定 TextMate 语法。
        var textEditor = this.FindControl<TextEditor>("Editor")
            ?? throw new InvalidOperationException("Editor 控件未找到。");
        textEditor.ApplyCodeEditorStyle();

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        var textMateInstallation = textEditor.InstallTextMate(registryOptions);

        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".yaml").Id));

        if (DataContext is YamlPrettifyViewModel vm)
        {
            vm.YamlTextEditor = textEditor;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
