using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CodeWF.Core.IServices;

namespace CodeWF.Core.Services;

public class FileChooserService : IFileChooserService
{
    private IStorageProvider? _storageProvider;

    public void SetHostWindow(TopLevel level)
    {
        _storageProvider = level?.StorageProvider;
    }

    public async Task<List<string>?> OpenFileAsync(string title, bool allowMultiple,
        IReadOnlyList<FilePickerFileType>? fileTypeFilters)
    {
        var result = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = title, FileTypeFilter = fileTypeFilters, AllowMultiple = allowMultiple,
        });

        return result.Any() ? result.Select(file => file.Path.AbsolutePath).ToList() : default;
    }

    public async Task<string?> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType>? fileTypeFilters)
    {
        var result = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = title, FileTypeChoices = fileTypeFilters
        });

        return result == null ? default : result.Path.AbsolutePath;
    }

    public async Task<List<string>?> OpenFolderAsync(string title, bool allowMultiple = false)
    {
        var result = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = title, AllowMultiple = allowMultiple,
        });

        return result.Any() ? result.Select(file => file.Path.AbsolutePath).ToList() : default;
    }
}