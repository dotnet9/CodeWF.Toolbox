using AvaloniaEdit;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Modules.XmlTranslatorManager.Models;
using CodeWF.Tools.FileExtensions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Xml;
using System.Xml.Linq;

namespace CodeWF.Modules.XmlTranslatorManager.ViewModels;

public class MergeXmlFilesViewModel : ReactiveObject
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;
    private List<LanguageXmlFileInfo>? _oldXmlFiles;

    public TextEditor? XmlTextEditor { get; set; }
    private string? _languageDir;

    public string? LanguageDir
    {
        get => _languageDir;
        set
        {
            this.RaiseAndSetIfChanged(ref _languageDir, value);

            ReadXmlFiles();
        }
    }

    private string? _mergeXmlLanguageFileName;

    public string? MergeXmlLanguageFileName
    {
        get => _mergeXmlLanguageFileName;
        set => this.RaiseAndSetIfChanged(ref _mergeXmlLanguageFileName, value);
    }

    public ObservableCollection<LanguageXmlFileInfo> LanguageXmlFiles { get; } = new();

    private LanguageXmlFileInfo? _selectedXmlFile;

    public LanguageXmlFileInfo? SelectedXmlFile
    {
        get => _selectedXmlFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedXmlFile, value);

            if (XmlTextEditor != null)
            {
                if (!string.IsNullOrWhiteSpace(value?.FilePath) && System.IO.File.Exists(value.FilePath))
                {
                    XmlTextEditor.Text = File.ReadAllText(value.FilePath);
                }
                else
                {
                    XmlTextEditor.Text = default;
                }
            }
        }
    }

    public ReactiveCommand<Unit, Unit> ChoiceLanguageDirCommand { get; }
    public ReactiveCommand<Unit, Unit> MergeXmlFilesCommand { get; }

    public MergeXmlFilesViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;

        ChoiceLanguageDirCommand = ReactiveCommand.CreateFromTask(RaiseChoiceLanguageDir);
        MergeXmlFilesCommand = ReactiveCommand.CreateFromTask(RaiseMergeXmlFiles);

        ReadSampleDir();
        MergeXmlLanguageFileName = "Localization";
    }

    public async Task RaiseChoiceLanguageDir()
    {
        var dirs = await _fileChooserService.OpenFolderAsync(
            I18nManager.GetString(Localization.MergeXmlFilesView.SelectLanguageDirectory));
        if (!(dirs?.Count > 0))
        {
            LanguageDir = default;
            return;
        }

        LanguageDir = dirs[0];
    }

    public async Task RaiseMergeXmlFiles()
    {
        try
        {
            if (_oldXmlFiles?.Any() != true)
            {
                _notificationService.Show(I18nManager.GetString(Localization.MergeXmlFilesView.NoLanguageFilesTitle),
                    I18nManager.GetString(Localization.MergeXmlFilesView.NoLanguageFilesContent));
                return;
            }

            if (string.IsNullOrWhiteSpace(MergeXmlLanguageFileName))
            {
                _notificationService.Show(
                    I18nManager.GetString(Localization.MergeXmlFilesView.WrongNewXmlFileNameTitle),
                    I18nManager.GetString(Localization.MergeXmlFilesView.WrongNewXmlFileNameContent));
                return;
            }

            var cultureGroup = _oldXmlFiles.GroupBy(file => new { file.Language, file.Description, file.CultureName });
            foreach (var group in cultureGroup)
            {
                var groupDoc = new XmlDocument();
                var xmlDeclaration = groupDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                groupDoc.AppendChild(xmlDeclaration);

                var rootElement = groupDoc.CreateElement("Localization");
                rootElement.SetAttribute("language", group.Key.Language);
                rootElement.SetAttribute("description", group.Key.Description);
                rootElement.SetAttribute("cultureName", group.Key.CultureName);
                groupDoc.AppendChild(rootElement);

                foreach (var xmlFileInfo in group)
                {
                    var childDoc = new XmlDocument();
                    childDoc.Load(xmlFileInfo.FilePath!);
                    var childNodes = childDoc.DocumentElement.ChildNodes;
                    foreach (XmlNode childNode in childNodes)
                    {
                        var importedNode = groupDoc.ImportNode(childNode, true);
                        rootElement.AppendChild(importedNode);
                    }
                }

                var groupPath = Path.Combine(LanguageDir, $"{MergeXmlLanguageFileName}.{group.Key.CultureName}.xml");
                groupDoc.Save(groupPath);
            }

            foreach (var oldFile in _oldXmlFiles)
            {
                FileHelper.DeleteFileIfExist(oldFile.FilePath!);
            }

            ReadXmlFiles();
        }
        catch (Exception ex)
        {
            _notificationService.Show(I18nManager.GetString(Localization.MergeXmlFilesView.MergeXmlFilesExceptionTitle),
                string.Format(I18nManager.GetString(Localization.MergeXmlFilesView.MergeXmlFilesExceptionContent),
                    ex.Message));
        }
    }

    private void ReadSampleDir()
    {
        var i18nDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "I18n");
        if (Directory.Exists(i18nDir))
        {
            LanguageDir = i18nDir;
        }
    }

    private void ReadXmlFiles()
    {
        try
        {
            LanguageXmlFiles.Clear();
            SelectedXmlFile = null;
            if (string.IsNullOrWhiteSpace(LanguageDir))
            {
                return;
            }

            _oldXmlFiles = Directory.GetFiles(LanguageDir, "*.xml", SearchOption.AllDirectories)
                .Select(file =>
                {
                    ReadXmlFile(file, out var fileInfo);
                    return fileInfo;
                }).Where(file => file != null).ToList();
            if (_oldXmlFiles?.Any() == true)
            {
                LanguageXmlFiles.AddRange(_oldXmlFiles);
            }

            SelectedXmlFile = LanguageXmlFiles.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _notificationService.Show(I18nManager.GetString(Localization.MergeXmlFilesView.ReadXmlFilesExceptionTitle),
                string.Format(I18nManager.GetString(Localization.MergeXmlFilesView.ReadXmlFilesExceptionContent),
                    ex.Message));
        }
    }

    private bool ReadXmlFile(string xmlFilePath, out LanguageXmlFileInfo? fileInfo)
    {
        fileInfo = default;
        try
        {
            var doc = XDocument.Load(xmlFilePath);
            var root = doc.Root;
            var language = root?.Attribute(XmlTranslatorManager.Models.Consts.LanguageKey)?.Value;
            var description = root?.Attribute(XmlTranslatorManager.Models.Consts.DescriptionKey)?.Value;
            var cultureName = root?.Attribute(XmlTranslatorManager.Models.Consts.CultureNameKey)?.Value;
            var exist = !string.IsNullOrWhiteSpace(language)
                        && !string.IsNullOrWhiteSpace(description)
                        && !string.IsNullOrWhiteSpace(cultureName);
            if (exist)
            {
                fileInfo = new LanguageXmlFileInfo()
                {
                    FileName = new FileInfo(xmlFilePath).Name,
                    FilePath = xmlFilePath,
                    Language = language,
                    Description = description,
                    CultureName = cultureName
                };
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}