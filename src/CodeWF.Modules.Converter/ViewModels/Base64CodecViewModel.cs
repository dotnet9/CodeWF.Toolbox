using Avalonia.Controls;
using CodeWF.Core.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Text;

namespace CodeWF.Modules.Converter.ViewModels;

public class Base64CodecViewModel : ReactiveObject
{
    public Base64CodecViewModel()
    {
        EncodeCommand = ReactiveCommand.Create(Encode);
        DecodeCommand = ReactiveCommand.Create(Decode);
        SwapCommand = ReactiveCommand.Create(Swap);
        ClearCommand = ReactiveCommand.Create(Clear);
        CopyCommand = ReactiveCommand.CreateFromTask(CopyAsync);
    }

    public Control? ClipboardOwner { get; set; }

    public string InputText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public string OutputText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public string ErrorMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public ReactiveCommand<Unit, Unit> EncodeCommand { get; }

    public ReactiveCommand<Unit, Unit> DecodeCommand { get; }

    public ReactiveCommand<Unit, Unit> SwapCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

    public ReactiveCommand<Unit, Unit> CopyCommand { get; }

    private void Encode()
    {
        ErrorMessage = string.Empty;
        OutputText = Convert.ToBase64String(Encoding.UTF8.GetBytes(InputText));
    }

    private void Decode()
    {
        ErrorMessage = string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(InputText.Trim());
            OutputText = Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            OutputText = string.Empty;
            ErrorMessage = Localization.Base64CodecView.InvalidBase64Input;
        }
    }

    private void Swap()
    {
        ErrorMessage = string.Empty;
        (InputText, OutputText) = (OutputText, InputText);
    }

    private void Clear()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
        ErrorMessage = string.Empty;
    }

    private async Task CopyAsync()
    {
        await ClipboardHelper.SetTextAsync(ClipboardOwner, OutputText);
    }
}
