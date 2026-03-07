using CodeWF.Tools.Extensions;
using ReactiveUI;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeWF.Modules.Converter.ViewModels;

public class DateTimeConverterViewModel : ReactiveObject, IDisposable
{
    private bool _isRefreshing;
    private CompositeDisposable _disposables = new();

    public long CurrentTimestamp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public long InputTimestamp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string OutputDate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public DateTime InputDate
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = DateTime.Now;

    public long OutputTimestamp
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsMillisecondsForInput
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public bool IsMillisecondsForOutput
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public string InputDateFormat
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "yyyy/MM/dd HH:mm:ss";

    public DateTimeConverterViewModel()
    {
        StartRefreshingAsync();
    }

    public async Task StartRefreshingAsync()
    {
        _isRefreshing = true;
        var timer = Observable.Interval(TimeSpan.FromSeconds(1))
           .ObserveOn(RxSchedulers.MainThreadScheduler)
           .Subscribe(_ => UpdateCurrentTimestamp());
        _disposables.Add(timer);
    }

    public async Task StopRefreshingAsync()
    {
        _isRefreshing = false;
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    public async Task RefreshTimestampAsync()
    {
        UpdateCurrentTimestamp();
    }

    private void UpdateCurrentTimestamp()
    {
        CurrentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public async Task ConvertTimestampToDateAsync()
    {
        DateTime dateTime = IsMillisecondsForInput ? InputTimestamp.FromUnixTimeMillisecondsToDateTime() : InputTimestamp.FromUnixTimeSecondsToDateTime();
        OutputDate = dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    public async Task ConvertDateToTimestampAsync()
    {
        OutputTimestamp = IsMillisecondsForOutput ? InputDate.GetUnixTimeMilliseconds() : InputDate.GetUnixTimeSeconds();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}