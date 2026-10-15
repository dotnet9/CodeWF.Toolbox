using CodeWF.Modules.ToolFramework.Models;
using CodeWF.Modules.ToolFramework.Services;

namespace CodeWF.Toolbox.Tests.UnitTests;

public class ToolAlgorithmsRegressionTests
{
    [Fact]
    public void DocumentsReceiveIndependentMutableFieldsAndOutputs()
    {
        var registry = new ToolRegistry();
        var first = registry.Create("base64-string-converter")!;
        var second = registry.Create("base64-string-converter")!;
        first.Fields.Single(field => field.Id == "text").Text = "first document";
        first.Outputs[0].Text = "first result";
        Assert.Empty(second.Fields.Single(field => field.Id == "text").Text);
        Assert.Empty(second.Outputs[0].Text);
    }

    [Theory]
    [InlineData("English", false)]
    [InlineData("中文与 emoji 🧰", false)]
    [InlineData("URL-safe: 你好?", true)]
    public async Task Base64RoundTripsUnicodeAndUrlSafeText(string input, bool urlSafe)
    {
        var tool = new ToolRegistry().Create("base64-string-converter")!;
        var text = tool.Fields.Single(field => field.Id == "text");
        var mode = tool.Fields.Single(field => field.Id == "mode");
        text.Text = input;
        tool.Fields.Single(field => field.Id == "urlSafe").Boolean = urlSafe;
        await Run(tool);
        var encoded = tool.Outputs[0].Text;
        text.Text = encoded;
        mode.SelectedOption = mode.Options.Single(option => option.Value == "Decode");
        await Run(tool);
        Assert.Equal(input, tool.Outputs[0].Text);
    }

    [Fact]
    public async Task JsonMinificationPreservesUnicodeAndNumbers()
    {
        var tool = new ToolRegistry().Create("json-minify")!;
        tool.Fields.Single(field => field.IsMultiLine).Text = "{ \"name\": \"中文\", \"amount\": 12.34 }";
        await Run(tool);
        using var json = System.Text.Json.JsonDocument.Parse(tool.Outputs[0].Text);
        Assert.Equal("中文", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(12.34m, json.RootElement.GetProperty("amount").GetDecimal());
        Assert.DoesNotContain('\n', tool.Outputs[0].Text);
    }

    private static Task Run(ToolSpec tool) => tool.RunAsync!(new ToolRunContext(
        tool.Fields.ToDictionary(field => field.Id), tool.Outputs.ToDictionary(output => output.Id)), CancellationToken.None);
}
