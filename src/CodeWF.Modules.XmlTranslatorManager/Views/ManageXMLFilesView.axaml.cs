using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CodeWF.Modules.XmlTranslatorManager.ViewModels;

namespace CodeWF.Modules.XmlTranslatorManager.Views;

public partial class ManageXmlFilesView : UserControl
{
    public ManageXmlFilesView()
    {
        InitializeComponent();
        BindLanguageDataGrid();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BindLanguageDataGrid()
    {
        var dataGrid = this.FindControl<DataGrid>("LanguageDataGrid")
            ?? throw new InvalidOperationException("LanguageDataGrid 控件未找到。");

        // 避免在 XAML 行为里传递控件实例，减少裁剪/AOT 发布时对反射行为的依赖。
        AttachDataGridToViewModel(dataGrid);
        DataContextChanged += (_, _) => AttachDataGridToViewModel(dataGrid);
    }

    private void AttachDataGridToViewModel(DataGrid dataGrid)
    {
        if (DataContext is ManageXmlFilesViewModel viewModel)
        {
            viewModel.AttachDataGrid(dataGrid);
        }
    }
}
