using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CodeWF.Modules.XmlTranslatorManager.ViewModels;
using TextMateSharp.Grammars;

namespace CodeWF.Modules.XmlTranslatorManager.Views;

public partial class MergeXmlFilesView : UserControl
{
    public MergeXmlFilesView()
    {
        InitializeComponent();

        //First of all you need to have a reference for your TextEditor for it to be used inside AvaloniaEdit.TextMate project.
        var textEditor = this.FindControl<TextEditor>("Editor");

        //Here we initialize RegistryOptions with the theme we want to use.
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);

        //Initial setup of TextMate.
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all ??, you are ready to use AvaloniaEdit with syntax highlighting!
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));
        ((DataContext as MergeXmlFilesViewModel)!).XmlTextEditor = textEditor;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}