using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Core.Helpers;
using CodeWF.Modules.Converter.ViewModels;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.Converter.Views;

public partial class YamlToJsonView : UserControl
{
    public YamlToJsonView()
    {
        InitializeComponent();

        // AvaloniaEdit 的语法高亮需要在控件加载后绑定 TextMate 语法。
        var jsonEditor = this.FindControl<TextEditor>("JsonEditor")
            ?? throw new InvalidOperationException("JsonEditor 控件未找到。");
        var yamlEditor = this.FindControl<TextEditor>("YamlEditor")
            ?? throw new InvalidOperationException("YamlEditor 控件未找到。");
        jsonEditor.ApplyCodeEditorStyle();
        yamlEditor.ApplyCodeEditorStyle();

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        var jsonTextMateInstallation = jsonEditor.InstallTextMate(registryOptions);
        var yamlTextMateInstallation = yamlEditor.InstallTextMate(registryOptions);

        jsonTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".json").Id));
        yamlTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".yaml").Id));

        if (DataContext is not YamlToJsonViewModel vm)
        {
            return;
        }

        vm.JsonEditor = jsonEditor;
        vm.YamlEditor = yamlEditor;
        vm.StartListen();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
