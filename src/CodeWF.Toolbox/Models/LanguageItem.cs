namespace CodeWF.Toolbox.Models;

public class LanguageItem(string culture, string name)
{
    public string Name { get; } = name;
    public string Culture { get; } = culture;
}