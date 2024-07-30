namespace CodeWF.Tools.Desktop.IServices;

public interface IClipboardService
{
    Task CopyToAsync(string content);
}