using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeWF.Modules.Converter.ViewModels;

public class DateTimeConverterViewModel : ReactiveObject, IDisposable
{
    private long _currentTimestamp;
    private long _inputTimestamp;
    private string _outputDate;
    private DateTime _inputDate = DateTime.Now;
    private long _outputTimestamp;
    private bool _isMillisecondsForInput = false;
    private bool _isMillisecondsForOutput = false;
    private bool _isRefreshing;
    private string _inputDateFormat = "yyyy/MM/dd HH:mm:ss";
    private CompositeDisposable _disposables = new CompositeDisposable();

    public long CurrentTimestamp
    {
        get => _currentTimestamp;
        set => this.RaiseAndSetIfChanged(ref _currentTimestamp, value);
    }

    public long InputTimestamp
    {
        get => _inputTimestamp;
        set => this.RaiseAndSetIfChanged(ref _inputTimestamp, value);
    }

    public string OutputDate
    {
        get => _outputDate;
        set => this.RaiseAndSetIfChanged(ref _outputDate, value);
    }

    public DateTime InputDate
    {
        get => _inputDate;
        set => this.RaiseAndSetIfChanged(ref _inputDate, value);
    }

    public long OutputTimestamp
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
        CurrentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void ConvertTimestampToDate()
    {
        DateTime dateTime = IsMillisecondsForInput ? InputTimestamp.FromUnixTimeMillisecondsToDateTime() : InputTimestamp.FromUnixTimeSecondsToDateTime();
        OutputDate = dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public void ConvertDateToTimestamp()
    {
        OutputTimestamp = IsMillisecondsForOutput ? InputDate.GetUnixTimeMilliseconds() : InputDate.GetUnixTimeSeconds();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}