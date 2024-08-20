using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CodeWF.Modules.AI.Helpers;

public class ApiClient(HttpClient httpClient)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    public async Task CreateChatGptClient(string url, object value, Action<string?> result, Action<bool> complete)
    {
        var response = await HttpRequestRaw(url, value);

        var resultAsString = string.Empty;
        await using var stream = await response.Content.ReadAsStreamAsync();
        using StreamReader reader = new(stream);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            resultAsString += line + Environment.NewLine;

            if (line.StartsWith("data:"))
                line = line.Substring("data:".Length);

            line = line.TrimStart();

            if (line == "[DONE]")
            {
                break;
            }
            else if (line.StartsWith(":"))
            {
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                result.Invoke(line);
            }
        }

        complete.Invoke(true);
    }

    public async Task<HttpResponseMessage> HttpRequestRaw(string url, object postData = null, bool streaming = true)
    {
        HttpResponseMessage response = null;
        string resultAsString = null;
        HttpRequestMessage req = new(HttpMethod.Post, url);

        if (postData != null)
        {
            if (postData is HttpContent)
            {
                req.Content = postData as HttpContent;
            }
            else
            {
                string jsonContent =
                    JsonSerializer.Serialize(postData, _jsonSerializerOptions);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                req.Content = stringContent;
            }
        }

        response = await httpClient.SendAsync(req,
            streaming ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }
        else
        {
            try
            {
                resultAsString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                resultAsString =
                    "Additionally, the following error was thrown when attemping to read the response content: " + e;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException(
                    "OpenAI rejected your authorization, most likely due to an invalid API Key.  Try checking your API Key and see https://github.com/OkGoDoIt/OpenAI-API-dotnet#authentication for guidance.  Full API response follows: " +
                    resultAsString);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException(
                    "OpenAI had an internal server error, which can happen occasionally.  Please retry your request. " +
                    resultAsString);
            }
            else
            {
                throw new HttpRequestException(resultAsString);
            }
        }
    }
}