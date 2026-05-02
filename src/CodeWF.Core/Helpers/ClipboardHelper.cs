using Avalonia.Controls;
using Avalonia.Input;

namespace CodeWF.Core.Helpers;

public static class ClipboardHelper
{
    public static Task SetTextAsync(Control? owner, string? text)
    {
        if (owner == null || string.IsNullOrEmpty(text))
        {
            return Task.CompletedTask;
        }

        var clipboard = TopLevel.GetTopLevel(owner)?.Clipboard;
        if (clipboard == null)
        {
            return Task.CompletedTask;
        }

        var data = new DataTransfer();
        data.Add(DataTransferItem.CreateText(text));
        return clipboard.SetDataAsync(data);
    }
}
