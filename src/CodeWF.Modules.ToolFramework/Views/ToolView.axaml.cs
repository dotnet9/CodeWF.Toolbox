using Avalonia.Controls;
using Avalonia.Input;
using CodeWF.Modules.ToolFramework.ViewModels;

namespace CodeWF.Modules.ToolFramework.Views;

public partial class ToolView : UserControl
{
    public ToolView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => Focus();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ToolViewModel viewModel)
        {
            viewModel.ClipboardOwner = this;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ToolViewModel viewModel)
        {
            return;
        }

        viewModel.OnKeyDown(e.Key.ToString(), e.PhysicalKey.ToString(), e.KeyModifiers.ToString());
    }
}

