using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CodeWF.Core.IServices;

public interface IFileChooserService
{
    void SetHostWindow(TopLevel level);

    Task<List<string>?> OpenFileAsync(string title, bool allowMultiple,
        IReadOnlyList<FilePickerFileType>? fileTypeFilters);

    Task<string?> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType>? fileTypeFilters);

    Task<List<string>?> OpenFolderAsync(string title, bool allowMultiple = false);
}