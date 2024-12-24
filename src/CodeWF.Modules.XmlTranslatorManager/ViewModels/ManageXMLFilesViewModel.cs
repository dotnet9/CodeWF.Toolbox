using Avalonia.Controls;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.AvaloniaControls.Extensions;
using CodeWF.Core.IServices;
using CodeWF.Modules.XmlTranslatorManager.Models;
using CodeWF.Tools.Exports;
using CodeWF.Tools.FileExtensions;
using ReactiveUI;
using System.Data;
using System.Xml.Linq;
using Ursa.Controls;

namespace CodeWF.Modules.XmlTranslatorManager.ViewModels;

public class ManageXmlFilesViewModel : ReactiveObject
{
    #region Fields

    public INotificationService _notificationService { get; }
    private readonly IFileChooserService _fileChooserService;
    private DataGrid? _languagePropertyDataGrid;

    #endregion

    public ManageXmlFilesViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _fileChooserService = fileChooserService;
        ReadSampleDir();
    }

    #region Properties

    public RangeObservableCollection<LanguageXmlModel> XmlFiles { get; } = new();

    private LanguageClassModel? _selectedClassItem;

    public LanguageClassModel? SelectedClassItem
    {
        get => _selectedClassItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedClassItem, value);
            ChangeLanguageClass();
        }
    }

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

    #endregion

    #region Command handler

    public void RaiseDataGridLoadHandler(DataGrid dataGrid)
    {
        _languagePropertyDataGrid = dataGrid;
    }

    public async Task RaiseChoiceLanguageDirHandler()
    {
        var dirs = await _fileChooserService.OpenFolderAsync(
            I18nManager.Instance.GetResource(Localization.MergeXmlFilesView.SelectLanguageDirectory));
        if (!(dirs?.Count > 0))
        {
            LanguageDir = default;
            return;
        }

        LanguageDir = dirs[0];
    }

    public async Task RaiseExportHandler()
    {
        var fileTypeFilters = new List<FilePickerFileType>
        {
            new("CSV files")
            {
                Patterns = new List<string> { "*.csv" }, MimeTypes = new List<string> { "text/csv" }
            },
            new("Excel Workbook")
            {
                Patterns = new List<string> { "*.xlsx" },
                MimeTypes = new List<string>
                {
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                }
            }
        };
        var savePath = await _fileChooserService.SaveFileAsync("Export", fileTypeFilters);
        if (string.IsNullOrWhiteSpace(savePath))
        {
            return;
        }

        GetDataGridData(out var errorMsg, out var data);
        data.Export(savePath, out errorMsg);
        FileHelper.OpenFolderAndSelectFile(savePath);
    }

    #endregion

    #region Private methods

    private void ReadSampleDir()
    {
        var i18nDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "I18n");
        if (Directory.Exists(i18nDir))
        {
            LanguageDir = i18nDir;
        }
    }

    private async Task ReadXmlFiles()
    {
        XmlFiles.Clear();

        try
        {
            var xmlFiles = Directory.GetFiles(LanguageDir, "*.xml");
            var groupedFiles = xmlFiles.GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('.')[0]);
            foreach (var group in groupedFiles)
            {
                var languageXmlModel = new LanguageXmlModel()
                {
                    Name = group.Key,
                    Files = new List<LanguageXmlFileInfo>(),
                    Classes = new List<LanguageClassModel>()
                };

                foreach (var file in group)
                {
                    var xDoc = XDocument.Load(file);
                    var root = xDoc.Root;
                    var fileInfo = new LanguageXmlFileInfo
                    {
                        FileName = Path.GetFileName(file),
                        Language = root.Attribute("language")?.Value,
                        Description = root.Attribute("description")?.Value,
                        CultureName = root.Attribute("cultureName")?.Value,
                        FilePath = file
                    };
                    languageXmlModel.Files.Add(fileInfo);

                    var classElements = xDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                        .Where(e => e.Descendants().Count() == 0).Select(e => e.Parent).Distinct().ToList();
                    foreach (var classElement in classElements)
                    {
                        var className = classElement.Name.LocalName;
                        var classModel = languageXmlModel.Classes?.FirstOrDefault(c => c.Name == className);
                        if (classModel == null)
                        {
                            classModel = new LanguageClassModel { Name = className, Properties = new() };
                            languageXmlModel.Classes.Add(classModel);
                        }

                        // 遍历类元素下的子节点作为属性填充到Properties列表中
                        foreach (var propertyElement in classElement.Elements())
                        {
                            var propertyName = propertyElement.Name.LocalName;
                            var property = classModel.Properties!.FirstOrDefault(p => p.Key == propertyName);
                            if (property == null)
                            {
                                property = new LanguageProperty()
                                {
                                    Key = propertyName, Values = new Dictionary<string, string>()
                                };
                                classModel.Properties!.Add(property);
                            }

                            property.Values![fileInfo.CultureName] = propertyElement.Value;
                        }
                    }
                }

                XmlFiles.Add(languageXmlModel);
            }
        }
        catch (Exception ex)
        {
            _notificationService.Show("Read xml file exception", ex.Message);
        }
    }

    private void ChangeLanguageClass()
    {
        if (_languagePropertyDataGrid == null)
        {
            return;
        }

        _languagePropertyDataGrid.Columns.Clear();
        if (SelectedClassItem == null || SelectedClassItem.Properties?.Any() != true)
        {
            return;
        }

        _languagePropertyDataGrid.Columns.Add(new DataGridTextColumn()
        {
            Header = nameof(LanguageProperty.Key),
            Binding = new CompiledBindingExtension(new CompiledBindingPathBuilder()
                .Property(new ClrPropertyInfo(nameof(LanguageProperty.Key),
                        obj => ((LanguageProperty)obj).Key,
                        (_, _) => { },
                        typeof(string)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build())
        });

        var cultureNames = SelectedClassItem.Properties.First().Values!.Keys.ToList();
        var propertyColumns = cultureNames.Select(cultureName => new DataGridTextColumn()
        {
            Header = cultureName,
            Binding = new CompiledBindingExtension(new CompiledBindingPathBuilder()
                .Property(new ClrPropertyInfo(cultureName,
                        obj =>
                        {
                            ((LanguageProperty)obj).Values!.TryGetValue(cultureName, out var value);
                            return value;
                        },
                        (obj, value) =>
                        {
                            if (value is string newValue)
                            {
                                ((LanguageProperty)obj).Values[cultureName] = newValue;
                                Save(((LanguageProperty)obj).Key, cultureName, newValue);
                            }
                        },
                        typeof(string)),
                    PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
                .Build()),
            IsReadOnly = false
        });
        foreach (var column in propertyColumns)
        {
            _languagePropertyDataGrid.Columns.Add(column);
        }
    }

    private void Save(string propertyName, string cultureName, string value)
    {
        try
        {
            var xmlFile = GetCurrentXmlFile(cultureName);

            var xDoc = XDocument.Load(xmlFile.FilePath);

            var propertyNode = xDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => e.Name.LocalName == propertyName && e.Parent.Name.LocalName == SelectedClassItem.Name)
                ?.FirstOrDefault();
            if (propertyNode == null)
            {
                return;
            }

            propertyNode.Value = value;
            xDoc.Save(xmlFile.FilePath);
        }
        catch (Exception ex)
        {
            _notificationService.Show("Save xml file exception", ex.Message);
        }
    }

    private LanguageXmlFileInfo? GetCurrentXmlFile(string cultureName)
    {
        var currentXmlFile =
            XmlFiles.FirstOrDefault(file => file.Classes?.Exists(classObj => classObj == SelectedClassItem) == true);
        return currentXmlFile?.Files.FirstOrDefault(file => file.CultureName == cultureName);
    }

    private bool GetDataGridData(out string? errorMsg, out DataTable dataTable)
    {
        errorMsg = default;
        dataTable = default;

        if (SelectedClassItem?.Properties?.Any() != true)
        {
            errorMsg = "Empty data";
            return false;
        }

        dataTable = new DataTable();

        foreach (var column in _languagePropertyDataGrid.Columns)
        {
            dataTable.Columns.Add(column.Header.ToString());
        }

        var itemsSource = _languagePropertyDataGrid.ItemsSource;
        if (itemsSource != null)
        {
            foreach (var item in itemsSource)
            {
                var data = item as LanguageProperty;
                var row = dataTable.NewRow();
                for (int colIndex = 0; colIndex < _languagePropertyDataGrid.Columns.Count; colIndex++)
                {
                    var colName = _languagePropertyDataGrid.Columns[colIndex].Header.ToString();
                    if (colName == nameof(LanguageProperty.Key))
                    {
                        row[colIndex] = data.Key;
                    }
                    else if (data.Values.ContainsKey(colName))
                    {
                        row[colIndex] = data.Values[colName];
                    }
                }

                dataTable.Rows.Add(row);
            }
        }

        return true;
    }

    #endregion
}