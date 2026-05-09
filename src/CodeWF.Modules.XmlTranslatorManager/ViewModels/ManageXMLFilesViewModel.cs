using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Platform.Storage;
using CodeWF.AvaloniaControls.Extensions;
using CodeWF.Core.IServices;
using CodeWF.Modules.XmlTranslatorManager.Models;
using CodeWF.Tools.Exports;
using CodeWF.Tools.FileExtensions;
using Lang.Avalonia;
using ReactiveUI;
using System.Data;
using System.Text;
using System.Xml.Linq;

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

    public LanguageClassModel? SelectedClassItem
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            ChangeLanguageClass();
        }
    }

    public string? LanguageDir
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);

            ReadXmlFiles();
        }
    }

    #endregion

    #region Command handler

    public void AttachDataGrid(DataGrid dataGrid)
    {
        _languagePropertyDataGrid = dataGrid;
        ChangeLanguageClass();
    }

    public async Task RaiseChoiceLanguageDirHandlerAsync()
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

    public async Task RaiseExportHandlerAsync()
    {
        var fileTypeFilters = new List<FilePickerFileType>
        {
            new("CSV files")
            {
                Patterns = ["*.csv"], MimeTypes = ["text/csv"]
            },
            new("Excel Workbook")
            {
                Patterns = ["*.xlsx"],
                MimeTypes =
                [
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                ]
            }
        };
        var savePath = await _fileChooserService.SaveFileAsync("Export", fileTypeFilters);
        if (string.IsNullOrWhiteSpace(savePath))
        {
            return;
        }

        GetDataGridData(out var errorMsg, out var data);
        data.Export(savePath, Encoding.Default, out errorMsg);
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

    private void ReadXmlFiles()
    {
        XmlFiles.Clear();
        if (string.IsNullOrWhiteSpace(LanguageDir) || !Directory.Exists(LanguageDir))
        {
            return;
        }

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
                    var cultureName = root?.Attribute("cultureName")?.Value ?? Path.GetFileNameWithoutExtension(file);
                    var fileInfo = new LanguageXmlFileInfo
                    {
                        FileName = Path.GetFileName(file),
                        Language = root?.Attribute("language")?.Value,
                        Description = root?.Attribute("description")?.Value,
                        CultureName = cultureName,
                        FilePath = file
                    };
                    languageXmlModel.Files.Add(fileInfo);

                    var classElements = xDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                        .Where(e => e.Descendants().Count() == 0 && e.Parent != null)
                        .Select(e => e.Parent!)
                        .Distinct()
                        .ToList();
                    foreach (var classElement in classElements)
                    {
                        var className = classElement.Name.LocalName;
                        var classModel = languageXmlModel.Classes?.FirstOrDefault(c => c.Name == className);
                        if (classModel == null)
                        {
                            classModel = new LanguageClassModel { Name = className, Properties = new() };
                            languageXmlModel.Classes!.Add(classModel);
                        }

                        foreach (var propertyElement in classElement.Elements())
                        {
                            var propertyName = propertyElement.Name.LocalName;
                            var property = classModel.Properties!.FirstOrDefault(p => p.Key == propertyName);
                            if (property == null)
                            {
                                property = new LanguageProperty()
                                {
                                    Key = propertyName,
                                    Values = new Dictionary<string, string>(),
                                    PersistValueAction = Save
                                };
                                classModel.Properties!.Add(property);
                            }

                            property.Values![cultureName] = propertyElement.Value;
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

        foreach (var property in SelectedClassItem.Properties)
        {
            property.PersistValueAction = Save;
        }

        _languagePropertyDataGrid.Columns.Add(new DataGridTemplateColumn()
        {
            Header = nameof(LanguageProperty.Key),
            IsReadOnly = true,
            CellTemplate = CreateKeyColumnTemplate()
        });

        var cultureNames = SelectedClassItem.Properties.First().Values!.Keys.ToList();
        var propertyColumns = cultureNames.Select(cultureName => new DataGridTemplateColumn()
        {
            Header = cultureName,
            CellTemplate = CreateCultureColumnTemplate(cultureName),
            IsReadOnly = false
        });
        foreach (var column in propertyColumns)
        {
            _languagePropertyDataGrid.Columns.Add(column);
        }
    }

    private static FuncDataTemplate<LanguageProperty> CreateKeyColumnTemplate()
    {
        return new FuncDataTemplate<LanguageProperty>((property, _) => new TextBlock
        {
            Margin = new Avalonia.Thickness(8, 4),
            Text = property?.Key ?? string.Empty,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
    }

    private static FuncDataTemplate<LanguageProperty> CreateCultureColumnTemplate(string cultureName)
    {
        return new FuncDataTemplate<LanguageProperty>((property, _) =>
        {
            var textBox = new TextBox
            {
                BorderThickness = new Avalonia.Thickness(0),
                Text = property?[cultureName] ?? string.Empty,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // 动态语言列使用代码模板读写字典，避免 ReflectionBinding 在 NativeAOT 下产生动态代码警告。
            textBox.LostFocus += (_, _) =>
            {
                if (property != null)
                {
                    property[cultureName] = textBox.Text ?? string.Empty;
                }
            };

            return textBox;
        });
    }

    private void Save(string propertyName, string cultureName, string value)
    {
        try
        {
            var xmlFile = GetCurrentXmlFile(cultureName);

            if (xmlFile?.FilePath == null || SelectedClassItem?.Name == null)
            {
                return;
            }

            var xDoc = XDocument.Load(xmlFile.FilePath);

            var propertyNode = xDoc.Nodes().OfType<XElement>().DescendantsAndSelf()
                .Where(e => e.Name.LocalName == propertyName && e.Parent?.Name.LocalName == SelectedClassItem.Name)
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
        dataTable = new DataTable();

        if (SelectedClassItem?.Properties?.Any() != true)
        {
            errorMsg = "Empty data";
            return false;
        }

        if (_languagePropertyDataGrid == null)
        {
            errorMsg = "Data grid is not ready";
            return false;
        }

        foreach (var column in _languagePropertyDataGrid.Columns)
        {
            dataTable.Columns.Add(column.Header?.ToString() ?? string.Empty);
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
                    var colName = _languagePropertyDataGrid.Columns[colIndex].Header?.ToString() ?? string.Empty;
                    if (colName == nameof(LanguageProperty.Key))
                    {
                        row[colIndex] = data?.Key ?? string.Empty;
                    }
                    else if (data?.Values?.ContainsKey(colName) == true)
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
