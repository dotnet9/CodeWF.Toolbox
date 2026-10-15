using CodeWF.Toolkit.Lang;
using System.Globalization;

namespace CodeWF.Toolbox.Services;

public static class ToolboxText
{
    public static CultureInfo Culture => I18nManager.Instance.Culture ?? CultureInfo.GetCultureInfo("zh-CN");
    public static string Get(string key) => I18nManager.Instance.GetResource(key);
    public static string Format(string key, params object?[] args) => string.Format(Culture, Get(key), args);
}
