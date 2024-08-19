using ReactiveUI;

namespace CodeWF.Modules.AI.ViewModels;

public class PolyTranslateViewModel : ReactiveObject
{
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

    public async void RaiseTranslateCommandHandler()
    {
        //if (!string.IsNullOrWhiteSpace(ResponseContent))
        //{
        //    ResponseContent = "";
        //}
        //string skPrompt = """
        //                  {{$input}}

        //                  将上面的输入翻译成{{$language}}，无需任何其他内容
        //                  """;

        //await foreach (var update in _kernel.InvokePromptStreamingAsync(skPrompt, new() { ["input"] = AskContent, ["language"] = "中文简体、中文繁体、英文、日语" }))
        //{
        //    ResponseContent += update.ToString();
        //}
    }
}