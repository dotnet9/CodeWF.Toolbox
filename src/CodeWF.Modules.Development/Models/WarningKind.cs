using System.ComponentModel;

namespace CodeWF.Modules.Development.Models;

public enum WarningKind
{
    [Description(Localization.TestView.ShowAll)]
    All,

    [Description(Localization.TestView.FilterAlarms)]
    Warning,

    [Description(Localization.TestView.FilterNormal)]
    Normal
}