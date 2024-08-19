using Avalonia.Controls.Platform;
using CodeWF.WebAPI.ViewModels;
using ReactiveUI;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CodeWF.Modules.AI.ViewModels;

public class AskBotViewModel : ReactiveObject
{
    private readonly HttpClient? _client;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

    private string? _askContent;

    public string? AskContent
    {
        get => _askContent;
        set => this.RaiseAndSetIfChanged(ref _askContent, value);
    }

    private string? _responseContent;

    public string? ResponseContent
    {
        get => _responseContent;
        set => this.RaiseAndSetIfChanged(ref _responseContent, value);
    }

    public AskBotViewModel(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new Uri("http://localhost:5242/");
    }

    public async Task RaiseAskAICommandHandler()
    {
        if (string.IsNullOrWhiteSpace(AskContent))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(ResponseContent))
        {
            ResponseContent = "";
        }

        await CallStreamingPostApiAsync("/api/askbot",
            JsonSerializer.Serialize(new AskBotRequest(AskContent!), _jsonSerializerOptions));
    }

    public async Task CallStreamingPostApiAsync(string url, string jsonBody)
    {
        try
        {
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var msg = new HttpRequestMessage(HttpMethod.Post, url);
            msg.Content = content;
            HttpResponseMessage response = await _client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(responseStream))
            {
                var buffer = new Memory<char>(new char[5]);
                int writeLength = 0;
                while ((writeLength = await streamReader.ReadBlockAsync(buffer)) > 0)
                {
                    if (writeLength < buffer.Length)
                    {
                        buffer = buffer[..writeLength];
                    }

                    ResponseContent += buffer.ToString();
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
}