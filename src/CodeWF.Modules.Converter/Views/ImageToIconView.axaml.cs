using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CodeWF.Modules.Converter.ViewModels;

namespace CodeWF.Modules.Converter;

public partial class ImageToIconView : UserControl
{
    public ImageToIconView()
    {
        InitializeComponent();
    }

    public void RaiseDropSourceImagePath(object? sender, DragEventArgs e)
    {
        if (this.DataContext is not ImageToIconViewModel vm)
        {
            return;
        }

        var files = e.Data.GetFiles();
        var file = files?.FirstOrDefault();
        if (file == null)
        {
            return;
        }

        vm.NeedConvertImagePath = file.TryGetLocalPath();
        e.Handled = true;
    }
}