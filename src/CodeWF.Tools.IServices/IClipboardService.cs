namespace CodeWF.Tools.IServices;

public interface IClipboardService
{
    Task CopyToAsync(string content);
}