namespace CodeWF.Tools.IServices;

public interface IFileChooserService
{
    void SetHostWindow(TopLevel window);

    Task<List<string>?> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType>? fileTypeFilter,
        bool allowMultiple);
}