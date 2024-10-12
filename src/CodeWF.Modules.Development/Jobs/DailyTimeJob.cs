using CodeWF.Modules.Development.ViewModels;
using Quartz;

namespace CodeWF.Modules.Development.Jobs;

internal class DailyTimeJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Run(() =>
        {
            TestViewModel.Instance.DailyTimeTask = $"{DateTime.Now:yyyy:MM:dd HH:mm:ss}: The daily task has been triggered";
        });
    }
}