namespace CodeWF.Tools.Core.Services.IPQuery;

public class IPTaoBaoQueryService : IIPQueryService
{
    private readonly HttpClient _httpClient;

    public IPTaoBaoQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IPQueryInfo> QueryAsync(string ip, CancellationToken cancellationToken)
    {
        string url = $"http://ip.taobao.com/outGetIpInfo?ip={ip}&accessKey=alibaba-inc";
        string json = await _httpClient.GetStringAsync(url, cancellationToken);
        json = json
            .Replace("xx", string.Empty)
            .Replace("XX", string.Empty);

        TaoBaoResponse? r = JsonSerializer.Deserialize<TaoBaoResponse>(json);
        string? result = r.Message;
        if (r.Code == 0)
        {
            TaoBaoData? data = r.Data;
            List<string> list = new List<string> { data.Country, data.Region, data.City, data.Isp };
            list.RemoveAll(string.IsNullOrWhiteSpace);
            result = string.Join(" ", list);
        }

        return new IPQueryInfo("淘宝", ip, result);
    }
}

public class TaoBaoResponse
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("data")] public TaoBaoData? Data { get; set; }
    [JsonPropertyName("msg")] public string? Message { get; set; }
}

public class TaoBaoData
{
    [JsonPropertyName("country")] public string? Country { get; set; }
    [JsonPropertyName("country_id")] public string? CountryId { get; set; }
    [JsonPropertyName("area")] public string? Area { get; set; }
    [JsonPropertyName("area_id")] public string? AreaId { get; set; }
    [JsonPropertyName("region")] public string? Region { get; set; }
    [JsonPropertyName("region_id")] public string? RegionId { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("city_id")] public string? CityId { get; set; }
    [JsonPropertyName("county")] public string? County { get; set; }
    [JsonPropertyName("county_id")] public string? CountyId { get; set; }
    [JsonPropertyName("isp")] public string? Isp { get; set; }
    [JsonPropertyName("isp_id")] public string? IspId { get; set; }
    [JsonPropertyName("ip")] public string? Ip { get; set; }
}