using CodeWF.Modules.ToolFramework.Services;
using CodeWF.Modules.ToolFramework.Models;
using CodeWF.Toolkit.Lang;
using CodeWF.Toolkit.Lang.Json;
using CodeWF.Toolbox.Services;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodeWF.Toolbox.Diagnostics;

/// <summary>验证已发布原生产物的资源与算法，不创建窗口、不修改用户配置。</summary>
internal static class RuntimeVerification
{
    public static void Run(string directory)
    {
        if (RuntimeFeature.IsDynamicCodeSupported) throw new InvalidOperationException("NativeAOT executable required.");
        var output = Path.GetFullPath(directory);
        Directory.CreateDirectory(output);
        if (!I18nManager.Instance.Register(new JsonLangPlugin { ResourceFolder = ToolboxDataPaths.Resource("I18n") },
                CultureInfo.GetCultureInfo("en-US"), out var error)) throw new InvalidOperationException(error);
        if (I18nManager.Instance.GetResource(Localization.AboutView.Title) != "About")
            throw new InvalidOperationException("Published I18n resources are unavailable.");
        var text = "NativeAOT 中文 😀";
        var tool = new ToolRegistry().Create("base64-string-converter") ?? throw new InvalidOperationException("Base64 tool is unavailable.");
        tool.Fields.Single(field => field.Id == "text").Text = text;
        tool.RunAsync!(new ToolRunContext(tool.Fields.ToDictionary(field => field.Id), tool.Outputs.ToDictionary(result => result.Id)), default).GetAwaiter().GetResult();
        if (tool.Outputs.Single(result => result.Id == "result").Text != Convert.ToBase64String(Encoding.UTF8.GetBytes(text)))
            throw new InvalidOperationException("Native Base64 result mismatch.");
        var message = $"NativeAOT runtime verification passed; version={typeof(Program).Assembly.GetName().Version}; I18n + Unicode Base64";
        File.WriteAllText(Path.Combine(output, "runtime-verification.txt"), message);
        Console.WriteLine(message);
    }
}
