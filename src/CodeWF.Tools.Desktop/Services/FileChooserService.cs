namespace CodeWF.Tools.Desktop.Services;

internal class FileChooserService : IFileChooserService
{
    private IStorageProvider? _storageProvider;

    public void SetHostWindow(TopLevel window)
    {
        var topLevel = TopLevel.GetTopLevel(window);
        _storageProvider = topLevel?.StorageProvider;
    }

    public async Task<List<string>?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType>? fileTypeFilter,
        bool allowMultiple)
    {
        if (_storageProvider is null)
            throw new ArgumentNullException(nameof(_storageProvider));
        var result = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = title, FileTypeFilter = fileTypeFilter, AllowMultiple = allowMultiple
        });
        return result.Any() ? result.Select(file => file.Path.AbsolutePath).ToList() : default;
    }
}