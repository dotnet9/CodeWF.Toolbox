using Test;

namespace CodeWF.Tools.Module.Test.ViewModels;

public class MessageTestViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly INotificationService _notificationService;

    public MessageTestViewModel(INotificationService notificationService, IMessenger messenger)
    {
        _notificationService = notificationService;
        _messenger = messenger;

        _messenger.Subscribe(this);
    }


    public Task ExecuteEventBusAsync()
    {
        _messenger.Publish(this, new TestMessage(this, nameof(MessageTestViewModel), TestClass.CurrentTime()));
        return Task.CompletedTask;
    }


    [EventHandler]
    public void ReceiveEventBusMessage(TestMessage message)
    {
        _notificationService?.Show("CodeWF EventBus",
            $"模块【Test】收到{nameof(TestMessage)}，Name: {message.Name}, Time: {message.CurrentTime}");
    }

    public Task ExecuteThrowExceptionAsync()
    {
        throw new Exception("这是测试抛出的异常");
    }

    public Task ExecuteDependentLibAsync()
    {
        _notificationService?.Show("模块调用依赖", $"从依赖库得到的时间：{TestClass.CurrentTime()}");
        return Task.CompletedTask;
    }
}