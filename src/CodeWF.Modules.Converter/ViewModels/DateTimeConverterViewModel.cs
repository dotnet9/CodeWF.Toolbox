using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeWF.Modules.Converter.ViewModels;

public class DateTimeConverterViewModel : ReactiveObject, IDisposable
{
    private string _currentTimestamp;
    private string _inputTimestamp;
    private string _outputDate;
    private string _inputDate;
    private string _outputTimestamp;
    private bool _isMillisecondsForInput;
    private bool _isMillisecondsForOutput;
    private bool _isRefreshing;
    private string _inputDateFormat = "yyyy/MM/dd HH:mm:ss";
    private CompositeDisposable _disposables = new CompositeDisposable();

    public string CurrentTimestamp
    {
        get => _currentTimestamp;
        set => this.RaiseAndSetIfChanged(ref _currentTimestamp, value);
    }

    public string InputTimestamp
    {
        get => _inputTimestamp;
        set => this.RaiseAndSetIfChanged(ref _inputTimestamp, value);
    }

    public string OutputDate
    {
        get => _outputDate;
        set => this.RaiseAndSetIfChanged(ref _outputDate, value);
    }

    public string InputDate
    {
        get => _inputDate;
        set => this.RaiseAndSetIfChanged(ref _inputDate, value);
    }

    public string OutputTimestamp
    {
        get => _outputTimestamp;
        set => this.RaiseAndSetIfChanged(ref _outputTimestamp, value);
    }

    public bool IsMillisecondsForInput
    {
        get => _isMillisecondsForInput;
        set => this.RaiseAndSetIfChanged(ref _isMillisecondsForInput, value);
    }

    public bool IsMillisecondsForOutput
    {
        get => _isMillisecondsForOutput;
        set => this.RaiseAndSetIfChanged(ref _isMillisecondsForOutput, value);
    }

    public string InputDateFormat
    {
        get => _inputDateFormat;
        set => this.RaiseAndSetIfChanged(ref _inputDateFormat, value);
    }

    public DateTimeConverterViewModel()
    {
        StartRefreshing();
    }

    public void StartRefreshing()
    {
        _isRefreshing = true;
        var timer = Observable.Interval(TimeSpan.FromSeconds(1))
           .ObserveOn(RxApp.MainThreadScheduler)
           .Subscribe(_ => UpdateCurrentTimestamp());
        _disposables.Add(timer);
    }

    public void StopRefreshing()
    {
        _isRefreshing = false;
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    public void RefreshTimestamp()
    {
        UpdateCurrentTimestamp();
    }

    private void UpdateCurrentTimestamp()
    {
        var now = DateTimeOffset.UtcNow;
        var timestamp = now.ToUnixTimeSeconds();
        CurrentTimestamp = timestamp.ToString();
    }

    public void ConvertTimestampToDate()
    {
        if (long.TryParse(InputTimestamp, out var timestamp))
        {
            DateTime dateTime = IsMillisecondsForInput ? timestamp.FromUnixTimeMillisecondsToDateTime() : timestamp.FromUnixTimeSecondsToDateTime();
            OutputDate = dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
        else
        {
            OutputDate = "输入的时间戳格式不正确";
        }
    }

    public void ConvertDateToTimestamp()
    {
        if (DateTime.TryParseExact(InputDate, InputDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            long timestamp = IsMillisecondsForOutput ? dateTime.GetUnixTimeMilliseconds() : dateTime.GetUnixTimeSeconds();
            OutputTimestamp = timestamp.ToString();
        }
        else
        {
            OutputTimestamp = "输入的日期格式不正确";
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}