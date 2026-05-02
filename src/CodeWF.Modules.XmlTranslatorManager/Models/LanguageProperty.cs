namespace CodeWF.Modules.XmlTranslatorManager.Models;

public class LanguageProperty
{
    public string? Key { get; set; }
    public Dictionary<string, string>? Values { get; set; }

    public Action<string, string, string>? PersistValueAction { get; set; }

    public string this[string cultureName]
    {
        get
        {
            if (Values != null && Values.TryGetValue(cultureName, out var value))
            {
                return value;
            }

            return string.Empty;
        }
        set
        {
            Values ??= new Dictionary<string, string>();
            Values[cultureName] = value;

            if (!string.IsNullOrWhiteSpace(Key))
            {
                PersistValueAction?.Invoke(Key, cultureName, value);
            }
        }
    }
}
