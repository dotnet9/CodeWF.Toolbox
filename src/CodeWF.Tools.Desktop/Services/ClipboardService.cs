namespace CodeWF.Tools.Desktop.Services;

internal class ClipboardService : IClipboardService
{
    public async Task CopyToAsync(string content)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.Clipboard is not { } provider)
        {
            throw new NullReferenceException("Missing Clipboard instance.");
        }

        await provider.SetTextAsync(content);
    }
}