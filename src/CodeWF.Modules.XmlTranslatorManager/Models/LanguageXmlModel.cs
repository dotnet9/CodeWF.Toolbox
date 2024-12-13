namespace CodeWF.Modules.XmlTranslatorManager.Models;

public class LanguageXmlModel
{
    public string? Name { get; set; }
    public List<LanguageXmlFileInfo> Files { get; set; }

    public List<LanguageClassModel>? Classes { get; set; }
}