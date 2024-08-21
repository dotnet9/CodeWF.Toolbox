using AvaloniaAotDemo.Commands;
using CodeWF.EventBus;
using ReactiveUI;
using System;

namespace AvaloniaAotDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        EventBus.Default.Subscribe<ChangeTimeCommand>(ReceiveEvent);
    }

    private string _greeting = "我擦勒，你是真的帅！";

    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }


    public void RaiseSendCommandHandler()
    {
        EventBus.Default.Publish(new ChangeTimeCommand() { Time = DateTime.Now });
    }

    private void ReceiveEvent(ChangeTimeCommand command)
    {
        Greeting = command.Time.ToString("yyyy-MM-dd HH:mm:ss fff");
    }
}