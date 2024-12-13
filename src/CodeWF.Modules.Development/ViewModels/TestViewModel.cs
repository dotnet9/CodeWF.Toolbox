using Avalonia.Platform.Storage;
using AvaloniaXmlTranslator;
using CodeWF.Core.IServices;
using CodeWF.Modules.Development.Entities;
using CodeWF.Modules.Development.Jobs;
using CodeWF.Modules.Development.Models;
using CodeWF.Tools.Extensions;
using CodeWF.Tools.FileExtensions;
using Quartz;
using Quartz.Impl;
using ReactiveUI;
using System.Collections;
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
        RaiseHashtableSerializeCommand = ReactiveCommand.CreateFromTask(RaiseHashtableSerializeHandler);

        Instance = this;
        StartTaskAsync();

        InitData();
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

    public List<WarningKind> WarningItems { get; set; }

    private WarningKind _selectedPrompt;

    public WarningKind SelectedPrompt
    {
        get => _selectedPrompt;
        set => this.RaiseAndSetIfChanged(ref _selectedPrompt, value);
    }

    public ReactiveCommand<Unit, Unit> RaiseCompressCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseDecompressionCommand { get; }
    public ReactiveCommand<Unit, Unit> RaiseHashtableSerializeCommand { get; }

    private void InitData()
    {
        WarningItems = Enum.GetValues(typeof(WarningKind)).OfType<WarningKind>().ToList();
        SelectedPrompt = WarningItems!.First();
    }

    private async Task RaiseCompressHandler()
    {
        try
        {
            var files = await _fileChooserService.OpenFileAsync(
                I18nManager.GetString(Localization.TestView.SelectCompressFiles)!,
                true,
                [FilePickerFileTypes.All]);
            if (!(files?.Count > 0))
            {
                return;
            }

            var saveFile = await _fileChooserService.SaveFileAsync(
                I18nManager.GetString(Localization.TestView.SaveCompressedFile)!,
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
            _notificationService.Show(I18nManager.GetString(Localization.TestView.CompressFileExceptionTitle)!,
                string.Format(I18nManager.GetString(Localization.TestView.CompressFileExceptionContent)!, ex));
        }
    }

    private async Task RaiseDecompressionHandler()
    {
        try
        {
            var files = await _fileChooserService.OpenFileAsync(
                I18nManager.GetString(Localization.TestView.SelectDecompressionFile)!, false,
                [_zipFilePickerFileType]);
            if (!(files?.Count > 0))
            {
                return;
            }

            var zipFile = files[0];

            var dirs = await _fileChooserService.OpenFolderAsync(
                I18nManager.GetString(Localization.TestView.SelectDirectory)!);
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
            _notificationService.Show(I18nManager.GetString(Localization.TestView.DecompressionFileExceptionTitle)!,
                string.Format(I18nManager.GetString(Localization.TestView.DecompressionFileExceptionContent)!, ex));
        }
    }

    private async Task RaiseHashtableSerializeHandler()
    {
        var k3Value = new JsonPrettifyEntity { IndentSize = 2, IsSortKey = false };
        Hashtable hashtable = new();
        hashtable.Add("k1", "v1");
        hashtable.Add("k2", 2);
        hashtable.Add("k3", k3Value);

        if (!hashtable.ToJson(out var json, out var errorString))
        {
            _notificationService.Show("Hashtable serialize", errorString);
            return;
        }

        _notificationService.Show("Hashtable serialize", "To Json Success");

        if (!json.FromJson<Hashtable>(out var deserializeObj, out errorString) || deserializeObj == null)
        {
            _notificationService.Show("Hashtable deserialize", errorString);
            return;
        }

        _notificationService.Show("Hashtable deserialize", "From Json Success");
        if (deserializeObj.Contains("k1") && deserializeObj["k1"].ToString() == hashtable["k1"].ToString()
                                          && (deserializeObj.Contains("k2") &&
                                              int.Parse(deserializeObj["k2"].ToString()) ==
                                              int.Parse(hashtable["k2"].ToString()))
                                          && deserializeObj.Contains("k3"))
        {
            var k3Obj = deserializeObj["k3"];
            k3Obj.ToJson(out var k3ValueJson, out errorString);
            k3ValueJson.FromJson<JsonPrettifyEntity>(out var newK3Value, out errorString);
            if (newK3Value?.IndentSize == k3Value.IndentSize)
            {
                _notificationService.Show("Hashtable deserialize", "Deserialize success");
            }
            else
            {
                _notificationService.Show("Hashtable deserialize", "Deserialize fail");
            }
        }
        else
        {
            _notificationService.Show("Hashtable deserialize", "Deserialize fail");
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
                    s.WithIntervalInHours(24)
                        .OnEveryDay()
                        .StartingDailyAt(new TimeOfDay(DateTime.Now.Hour, DateTime.Now.Minute + 1,
                            0)))
                .Build();

            await scheduler.ScheduleJob(simpleJob, simpleTrigger);
            await scheduler.ScheduleJob(dailyTaskJob, dailyTaskTrigger);
        });
    }
}