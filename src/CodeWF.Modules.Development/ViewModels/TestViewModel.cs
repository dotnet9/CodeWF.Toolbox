using Avalonia.Platform.Storage;
using AvaloniaExtensions.Axaml.Markup;
using CodeWF.Core.IServices;
using CodeWF.Modules.Development.I18n;
using CodeWF.Modules.Development.Jobs;
using CodeWF.Tools.FileExtensions;
using Quartz;
using Quartz.Impl;
using ReactiveUI;
using System.Reactive;

namespace CodeWF.Modules.Development.ViewModels;

public class TestViewModel : ReactiveObject
{
    private readonly IFileChooserService _fileChooserService;
    private readonly INotificationService _notificationService;

    private readonly FilePickerFileType _zipFilePickerFileType =
        new("Zip file") { Patterns = ["*.zip"] };

    public TestViewModel(IFileChooserService fileChooserService, INotificationService notificationService)
    {
        _fileChooserService = fileChooserService;
        _notificationService = notificationService;
        RaiseCompressCommand = ReactiveCommand.CreateFromTask(RaiseCompressHandler);
        RaiseDecompressionCommand = ReactiveCommand.CreateFromTask(RaiseDecompressionHandler);
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

    private string _dailyTimeTask;

    public string DailyTimeTask
    {
        get => _dailyTimeTask;
        set => this.RaiseAndSetIfChanged(ref _dailyTimeTask, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseCompressCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseDecompressionCommand { get; }

    private async Task RaiseCompressHandler()
    {
        try
        {
            var files = await _fileChooserService.OpenFileAsync(I18nManager.GetString(Language.SelectCompressFiles)!,
                true,
                [FilePickerFileTypes.All]);
            if (!(files?.Count > 0))
            {
                return;
            }

            var saveFile = await _fileChooserService.SaveFileAsync(I18nManager.GetString(Language.SaveCompressedFile)!,
                [_zipFilePickerFileType]);
            if (string.IsNullOrWhiteSpace(saveFile))
            {
                return;
            }

            ISevenZipCompressor zipHelper = new SevenZipCompressor();
            zipHelper.Zip(files, saveFile);
            FileHelper.OpenFolderAndSelectFile(saveFile);
        }
        catch (Exception ex)
        {
            _notificationService.Show(I18nManager.GetString(Language.CompressFileExceptionTitle)!,
                string.Format(I18nManager.GetString(Language.CompressFileExceptionContent)!, ex));
        }
    }

    private async Task RaiseDecompressionHandler()
    {
        try
        {
            var files = await _fileChooserService.OpenFileAsync(
                I18nManager.GetString(Language.SelectDecompressionFile)!, false,
                [_zipFilePickerFileType]);
            if (!(files?.Count > 0))
            {
                return;
            }

            var zipFile = files[0];

            var dirs = await _fileChooserService.OpenFolderAsync(I18nManager.GetString(Language.SelectDirectory)!);
            if (!(dirs?.Count > 0))
            {
                return;
            }

            var saveDir = dirs[0];
            ISevenZipCompressor zipHelper = new SevenZipCompressor();
            zipHelper.Decompress(zipFile, saveDir);

            FileHelper.OpenFolder(saveDir);
        }
        catch (Exception ex)
        {
            _notificationService.Show(I18nManager.GetString(Language.DecompressionFileExceptionTitle)!,
                string.Format(I18nManager.GetString(Language.DecompressionFileExceptionContent)!, ex));
        }
    }

    private async Task StartTaskAsync()
    {
        await Task.Run(async () =>
        {
            var factory = new StdSchedulerFactory();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();

            var simpleJob = JobBuilder.Create<UpdateTimeJob>()
                .WithIdentity(nameof(UpdateTimeJob), nameof(UpdateTimeJob))
                .Build();

            var simpleTrigger = TriggerBuilder.Create()
                .WithIdentity(nameof(UpdateTimeJob), nameof(UpdateTimeJob))
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .Build();

            var dailyTaskJob = JobBuilder.Create<DailyTimeJob>()
                .WithIdentity(nameof(DailyTimeJob), nameof(DailyTimeJob))
                .Build();

            var dailyTaskTrigger = TriggerBuilder.Create()
                .WithIdentity(nameof(DailyTimeJob), nameof(DailyTimeJob))
                .WithDailyTimeIntervalSchedule(s =>
                    s.OnEveryDay().StartingDailyAt(new TimeOfDay(DateTime.Now.Hour, DateTime.Now.Minute + 1,
                        DateTime.Now.Second)))
                .Build();

            await scheduler.ScheduleJob(simpleJob, simpleTrigger);
            await scheduler.ScheduleJob(dailyTaskJob, dailyTaskTrigger);
        });
    }
}