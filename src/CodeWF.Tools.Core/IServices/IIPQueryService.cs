namespace CodeWF.Tools.Core.IServices;

public interface IIPQueryService
{
    Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken);
}