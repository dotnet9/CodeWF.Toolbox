using CodeWF.Modules.Development.ViewModels;
using Quartz;

namespace CodeWF.Modules.Development.Jobs;

internal class UpdateTimeJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Run(() =>
        {
            TestViewModel.Instance.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        });
    }
}