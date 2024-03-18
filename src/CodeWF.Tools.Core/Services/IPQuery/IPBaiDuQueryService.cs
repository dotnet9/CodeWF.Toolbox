namespace CodeWF.Tools.Core.Services.IPQuery;

public class IPBaiDuQueryService : IIPQueryService
{
    private readonly HttpClient _httpClient;

    public IPBaiDuQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken)
    {
        const string ak = "yBaSv2qHR4r540yBDWGPpC1bLZYK17ni";
        string url = $"http://api.map.baidu.com/location/ip?ak={ak}&ip={ip}";
        string json = await _httpClient.GetStringAsync(url, cancellationToken);
        BaiduResponse? obj = JsonSerializer.Deserialize<BaiduResponse>(json);
        string result = obj.Address;
        if (obj.Status != 0)
        {
            result = obj.Message;
        }

        return new IPQueryInfo("百度地图", ip, result);
    }
}

public class BaiduResponse
{
    [JsonPropertyName("address")] public string Address { get; set; }

    [JsonPropertyName("status")] public int Status { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }
}