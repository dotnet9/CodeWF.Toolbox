using CodeWF.Tools.EventBus.Commands;
using CodeWF.Tools.EventBus.Queries;
using Test;

namespace CodeWF.Tools.Module.Test.ViewModels;

public class EventBusTestViewModel : ViewModelBase
{
    private readonly IEventBus _eventBus;
    private readonly INotificationService _notificationService;

    public EventBusTestViewModel(INotificationService notificationService, IEventBus eventBus)
    {
        _notificationService = notificationService;
        _eventBus = eventBus;

        _eventBus.Subscribe(this);
    }


    public async Task ExecuteCommandAsync()
    {
        await _eventBus.PublishAsync(this, new TestCommand(nameof(EventBusTestViewModel), TestClass.CurrentTime()));
    }

    public async Task ExecuteQueryAsync()
    {
        var query = new TestQuery();
        await _eventBus.PublishAsync(this, query);
        _notificationService?.Show("CodeWF.EventBus",
            $"模块【Test】收到Query结果：{query.Result}");
    }


    [EventHandler]
    public void ReceiveCommandMessage(TestCommand message)
    {
        _notificationService?.Show("CodeWF.EventBus",
            $"模块【Test】收到{nameof(TestCommand)}，Name: {message.Name}, Time: {message.CurrentTime}");
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