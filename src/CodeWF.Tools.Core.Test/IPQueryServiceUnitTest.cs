using CodeWF.Tools.Core.Models;

namespace CodeWF.Tools.Core.Test;

[TestClass]
public class IPQueryServiceUnitTest
{
    private const string ip = "32.32.5.33";
    private readonly IIPQueryService _aMapQueryService;
    private readonly IIPQueryService _baiDuQueryService;
    private readonly IIPQueryService _pcOnlineQueryService;
    private readonly IIPQueryService _taoBaoQueryService;
    private readonly IIPQueryService _tencentMapQueryService;

    public IPQueryServiceUnitTest()
    {
        HttpClient httpClient = new HttpClient();
        _taoBaoQueryService = new IPTaoBaoQueryService(httpClient);
        _baiDuQueryService = new IPBaiDuQueryService(httpClient);
        _tencentMapQueryService = new IPTencentQueryService(httpClient);
        _aMapQueryService = new IPAMapQueryService(httpClient);
        _pcOnlineQueryService = new IPPCOnlineQueryService(httpClient);
    }

    [TestMethod]
    public async Task Test_IPQueryAsync_SUCCESS()
    {
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        IPQueryInfo queryInfoFromTaoBao = await _taoBaoQueryService.QueryAsync(ip, cancelTokenSource.Token);
        IPQueryInfo queryInfoFromBaiDu = await _baiDuQueryService.QueryAsync(ip, cancelTokenSource.Token);
        IPQueryInfo queryInfoFromTencent = await _tencentMapQueryService.QueryAsync(ip, cancelTokenSource.Token);
        IPQueryInfo queryInfoFromAMap = await _aMapQueryService.QueryAsync(ip, cancelTokenSource.Token);
        IPQueryInfo queryInfoFromPCOnline = await _pcOnlineQueryService.QueryAsync(ip, cancelTokenSource.Token);

        Assert.IsNotNull(queryInfoFromTaoBao);
        Assert.IsNotNull(queryInfoFromBaiDu);
        Assert.IsNotNull(queryInfoFromTencent);
        Assert.IsNotNull(queryInfoFromAMap);
        Assert.IsNotNull(queryInfoFromPCOnline);
    }
}