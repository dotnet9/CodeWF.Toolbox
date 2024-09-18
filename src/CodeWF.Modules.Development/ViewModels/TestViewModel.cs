using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CodeWF.Core.IServices;
using ReactiveUI;
using System.Reactive;

namespace CodeWF.Modules.Development.ViewModels;

public class TestViewModel : ReactiveObject
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;

    public TestViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;
        RaiseOpenFileCommand = ReactiveCommand.CreateFromTask(RaiseOpenFileHandler);
        RaiseSelectFolderCommand = ReactiveCommand.CreateFromTask(RaiseSelectFolderHandler);
        RaiseSaveFileCommand = ReactiveCommand.CreateFromTask(RaiseSaveFileHandler);
        RaiseNotificationCommand = ReactiveCommand.CreateFromTask(RaiseNotificationHandler);
    }

    public ReactiveCommand<Unit, Unit> RaiseOpenFileCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseSelectFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseSaveFileCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseNotificationCommand { get; }

    private async Task RaiseOpenFileHandler()
    {
        await _fileChooserService.OpenFileAsync("打开文件", false,
            new List<FilePickerFileType> { FilePickerFileTypes.TextPlain });
    }

    private async Task RaiseSelectFolderHandler()
    {
    }

    private async Task RaiseSaveFileHandler()
    {
    }

    private async Task RaiseNotificationHandler()
    {
        _notificationService.Show("New Message", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"));
    }
}