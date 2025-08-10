using Avalonia;
using Avalonia.Controls;

namespace CodeWF.Controls.Views;

public partial class MarkdownView : UserControl
{
    public MarkdownView()
    {
        InitializeComponent();
    }
    public static readonly StyledProperty<string?> MarkdownProperty =
        AvaloniaProperty.Register<MarkdownView, string?>(nameof(Markdown));

    public string? Markdown
    {
        get => GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }
}