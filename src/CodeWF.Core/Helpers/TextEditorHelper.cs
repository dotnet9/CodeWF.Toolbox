using Avalonia.Media;
using AvaloniaEdit;

namespace CodeWF.Core.Helpers;

public static class TextEditorHelper
{
    public static void ApplyCodeEditorStyle(this TextEditor editor, bool enableTextDragDrop = true)
    {
        editor.Options.AllowScrollBelowDocument = false;
        editor.Options.EnableTextDragDrop = enableTextDragDrop;
        editor.TextArea.SelectionBrush = new SolidColorBrush(Color.Parse("#2563EB"));
        editor.TextArea.SelectionForeground = Brushes.White;
        editor.TextArea.SelectionBorder = new Pen(new SolidColorBrush(Color.Parse("#1D4ED8")), 1);
        editor.TextArea.SelectionCornerRadius = 2;
    }
}
