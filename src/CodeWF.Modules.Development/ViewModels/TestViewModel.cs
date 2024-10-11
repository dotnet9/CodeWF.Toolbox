using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CodeWF.Core.IServices;
using CodeWF.Modules.Development.Jobs;
using Quartz;
using Quartz.Impl;
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
        Instance = this;
        StartTaskAsync();
    }

    public static TestViewModel Instance { get; private set; }
    private string _currentTime;

    public string CurrentTime
    {
        get => _currentTime;
        set => this.RaiseAndSetIfChanged(ref _currentTime, value);
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

    private async Task StartTaskAsync()
    {
        await Task.Run(async () =>
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();

            var job = JobBuilder.Create<UpdateTimeJob>()
                .WithIdentity("UpdateTime", "Update")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("UpdateTime", "Update")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        });
    }
}