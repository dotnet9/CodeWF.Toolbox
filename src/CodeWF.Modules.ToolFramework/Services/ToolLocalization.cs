using System.Text;

namespace CodeWF.Modules.ToolFramework.Services;

public static class ToolLocalization
{
    private const string Root = "Localization.ToolboxTools";

    public static class Common
    {
        public const string Run = $"{Root}.Common.Run";
        public const string Browse = $"{Root}.Common.Browse";
        public const string Copy = $"{Root}.Common.Copy";
        public const string OpenFile = $"{Root}.Common.OpenFile";
        public const string SaveFile = $"{Root}.Common.SaveFile";
        public const string MissingToolId = $"{Root}.Common.MissingToolId";
        public const string ToolNotRegistered = $"{Root}.Common.ToolNotRegistered";
    }

    public static class Groups
    {
        public const string Security = $"{Root}.Groups.Security";
        public const string Web = $"{Root}.Groups.Web";
        public const string Media = $"{Root}.Groups.Media";
        public const string Network = $"{Root}.Groups.Network";
        public const string Math = $"{Root}.Groups.Math";
        public const string Measurement = $"{Root}.Groups.Measurement";
        public const string Text = $"{Root}.Groups.Text";
        public const string Data = $"{Root}.Groups.Data";
    }

    public static string ToolTitle(string toolId) => $"{Root}.Tools.{Sanitize(toolId)}.Title";

    public static string ToolDescription(string toolId) => $"{Root}.Tools.{Sanitize(toolId)}.Description";

    public static string FieldLabel(string toolId, string fieldId) => $"{Root}.Tools.{Sanitize(toolId)}.Fields.{Sanitize(fieldId)}";

    public static string FieldPlaceholder(string toolId, string fieldId) =>
        $"{Root}.Tools.{Sanitize(toolId)}.Placeholders.{Sanitize(fieldId)}";

    public static string OptionLabel(string toolId, string fieldId, string value) =>
        $"{Root}.Tools.{Sanitize(toolId)}.Options.{Sanitize(fieldId)}.{Sanitize(value)}";

    public static string OutputLabel(string toolId, string outputId) =>
        $"{Root}.Tools.{Sanitize(toolId)}.Outputs.{Sanitize(outputId)}";

    public static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastWasSeparator = false;

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasSeparator = false;
                continue;
            }

            if (!lastWasSeparator && builder.Length > 0)
            {
                builder.Append('_');
                lastWasSeparator = true;
            }
        }

        if (builder.Length > 0 && builder[^1] == '_')
        {
            builder.Length--;
        }

        if (builder.Length == 0 || char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }
}
