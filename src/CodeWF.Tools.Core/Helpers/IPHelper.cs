using System.Net;

namespace CodeWF.Tools.Core.Helpers;

public static class IPHelper
{
    public static async Task<string?> GetLocalIPAsync()
    {
        HttpClient httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync("https://ipv4.gdt.qq.com/get_client_ip");
        string str = await response.Content.ReadAsStringAsync();
        return IPAddress.TryParse(str, out IPAddress? ip) ? ip.ToString() : string.Empty;
    }
}