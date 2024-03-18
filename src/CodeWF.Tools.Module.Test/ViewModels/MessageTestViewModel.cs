namespace CodeWF.Tools.Module.Test.ViewModels;

public class MessageTestViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly INotificationService _notificationService;

    public MessageTestViewModel(INotificationService notificationService,
        IEventAggregator eventAggregator)
    {
        _notificationService = notificationService;
        _eventAggregator = eventAggregator;

        RegisterPrismEvent();
    }

    private void RegisterPrismEvent()
    {
        _eventAggregator.GetEvent<TestEvent>().Subscribe(args =>
        {
            _notificationService?.Show("Prism Event",
                $"模块【Test】Prism事件处理程序：Args = {args.Args}, Now = {DateTime.Now}");
        });
    }


    public Task ExecutePrismEventAsync()
    {
        TestEvent? prismEvent = _eventAggregator.GetEvent<TestEvent>();
        prismEvent.Publish(new TestEventParameter { Args = "ExecutePrismEventAsync" });
        return Task.CompletedTask;
    }

    public Task ExecuteThrowExceptionAsync()
    {
        throw new Exception("这是测试抛出的异常");
    }
}