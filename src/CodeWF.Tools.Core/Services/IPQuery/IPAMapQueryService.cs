namespace CodeWF.Tools.Core.Services.IPQuery;

public class IPAMapQueryService : IIPQueryService
{
    private readonly HttpClient _httpClient;

    public IPAMapQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken)
    {
        string key = "0f48c54461148ea1e670b676cbd1700b";
        string url = $"https://restapi.amap.com/v5/ip?key={key}&ip={ip}&type=4";
        string json = await _httpClient.GetStringAsync(url, cancellationToken);
        AmapResponse? obj = JsonSerializer.Deserialize<AmapResponse>(json);
        return new IPQueryInfo("高德地图", ip, obj.ToString());
    }
}

public class AmapResponse
{
    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("info")] public string? Info { get; set; }

    [JsonPropertyName("infocode")] public string? InfoCode { get; set; }

    [JsonPropertyName("country")] public string? Country { get; set; }

    [JsonPropertyName("province")] public string? Province { get; set; }

    [JsonPropertyName("city")] public string? City { get; set; }

    [JsonPropertyName("district")] public string? District { get; set; }

    [JsonPropertyName("isp")] public string? Isp { get; set; }

    [JsonPropertyName("location")] public string? Location { get; set; }

    [JsonPropertyName("ip")] public string? Ip { get; set; }

    public override string? ToString()
    {
        if (Status != "1")
        {
            return Info;
        }

        List<string> list = new List<string>
        {
            Country ?? string.Empty,
            Province ?? string.Empty,
            City ?? string.Empty,
            District ?? string.Empty,
            Isp ?? string.Empty
        };
        list.RemoveAll(string.IsNullOrWhiteSpace);
        return string.Join(" ", list);
    }
}