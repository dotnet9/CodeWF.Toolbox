using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Modules.Converter.ViewModels;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.Converter.Views;

public partial class JsonToYamlView : UserControl
{
    public JsonToYamlView()
    {
        InitializeComponent();

        //First of all you need to have a reference for your TextEditor for it to be used inside AvaloniaEdit.TextMate project.
        var jsonEditor = this.FindControl<TextEditor>("JsonEditor");
        var yamlEditor = this.FindControl<TextEditor>("YamlEditor");

        //Here we initialize RegistryOptions with the theme we want to use.
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        //Initial setup of TextMate.
        var jsonTextMateInstallation = jsonEditor.InstallTextMate(registryOptions);
        var yamlTextMateInstallation = yamlEditor.InstallTextMate(registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all ??, you are ready to use AvaloniaEdit with syntax highlighting!
        jsonTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));
        yamlTextMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));

        if (DataContext is not JsonToYamlViewModel vm)
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