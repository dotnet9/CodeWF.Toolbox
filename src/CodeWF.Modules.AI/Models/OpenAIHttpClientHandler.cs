using CodeWF.Modules.AI.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWF.Modules.AI.Models;
public class OpenAIHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UriBuilder uriBuilder;
        string url = OpenAIOption.Endpoint;
        Uri uri = new Uri(url);
        string host = uri.Host;
        switch (request.RequestUri?.LocalPath)
        {
            case "/v1/chat/completions":
                uriBuilder = new UriBuilder(request.RequestUri)
                {
                    // 这里是你要修改的 URL
                    Scheme = "https",
                    Host = host,
                    Path = "v1/chat/completions",
                };
                request.RequestUri = uriBuilder.Uri;
                break;
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}