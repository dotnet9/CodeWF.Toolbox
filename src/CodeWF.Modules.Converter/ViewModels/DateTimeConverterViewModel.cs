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
    } = string.Empty;

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
        StartRefreshing();
    }

    public Task StartRefreshingAsync()
    {
        StartRefreshing();
        return Task.CompletedTask;
    }

    public Task StopRefreshingAsync()
    {
        StopRefreshing();
        return Task.CompletedTask;
    }

    public Task RefreshTimestampAsync()
    {
        UpdateCurrentTimestamp();
        return Task.CompletedTask;
    }

    private void StartRefreshing()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;
        var timer = Observable.Interval(TimeSpan.FromSeconds(1))
           .ObserveOn(RxSchedulers.MainThreadScheduler)
           .Subscribe(_ => UpdateCurrentTimestamp());
        _disposables.Add(timer);
        UpdateCurrentTimestamp();
    }

    private void StopRefreshing()
    {
        if (!_isRefreshing)
        {
            return;
        }

        _isRefreshing = false;
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
    }

    private void UpdateCurrentTimestamp()
    {
        CurrentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public Task ConvertTimestampToDateAsync()
    {
        DateTime dateTime = IsMillisecondsForInput ? InputTimestamp.FromUnixTimeMillisecondsToDateTime() : InputTimestamp.FromUnixTimeSecondsToDateTime();
        OutputDate = dateTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        return Task.CompletedTask;
    }

    public Task ConvertDateToTimestampAsync()
    {
        OutputTimestamp = IsMillisecondsForOutput ? InputDate.GetUnixTimeMilliseconds() : InputDate.GetUnixTimeSeconds();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
