namespace CodeWF.Tools.Core.Services;

public class IPTencentQueryService : IIPQueryService
{
    private readonly HttpClient _httpClient;

    public IPTencentQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken)
    {
        string key = "TAOBZ-YQ3KU-R4NVU-BT4IA-2P2MF-RDBVJ";
        string url = $"https://apis.map.qq.com/ws/location/v1/ip?ip={ip}&key={key}";
        string json = await _httpClient.GetStringAsync(url, cancellationToken);
        TencentLbsResponse? e = JsonSerializer.Deserialize<TencentLbsResponse>(json);

        return new IPQueryInfo("腾讯地图", ip, e.ToString());
    }
}

public partial class TencentLbsResponse
{
    public class Location2
    {
        [JsonPropertyName("lat")] public double Lat { get; set; }

        [JsonPropertyName("lng")] public double Lng { get; set; }
    }
}

public partial class TencentLbsResponse
{
    public class AdInfo
    {
        [JsonPropertyName("nation")] public string? Nation { get; set; }

        [JsonPropertyName("province")] public string? Province { get; set; }

        [JsonPropertyName("city")] public string? City { get; set; }

        [JsonPropertyName("district")] public string? District { get; set; }

        [JsonPropertyName("adcode")] public int AdCode { get; set; }
    }
}

public partial class TencentLbsResponse
{
    public class Result2
    {
        [JsonPropertyName("ip")] public string? Ip { get; set; }

        [JsonPropertyName("location")] public Location2? Location { get; set; }

        [JsonPropertyName("ad_info")] public AdInfo? AdInfo { get; set; }
    }
}

public partial class TencentLbsResponse
{
    [JsonPropertyName("status")] public int Status { get; set; }

    [JsonPropertyName("message")] public string? Message { get; set; }

    [JsonPropertyName("result")] public Result2? Result { get; set; }

    public override string? ToString()
    {
        if (Status != 0)
        {
            return Message ?? string.Empty;
        }

        List<string> list = new List<string>();
        if (Result?.AdInfo == null)
        {
            return string.Join(" ", list);
        }

        AdInfo? info = Result.AdInfo;
        list.Add(info.Nation ?? string.Empty);
        list.Add(info.Province ?? string.Empty);
        list.Add(info.City ?? string.Empty);
        list.Add(info.District ?? string.Empty);
        list.RemoveAll(string.IsNullOrWhiteSpace);

        return string.Join(" ", list);
    }
}