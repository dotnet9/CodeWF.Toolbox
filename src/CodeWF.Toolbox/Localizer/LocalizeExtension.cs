using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CodeWF.Toolbox.Localizer;

public class LocalizeExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public string? Context { get; set; }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Localizer))]
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var keyToUse = Key;
        if (!string.IsNullOrWhiteSpace(Context))
            keyToUse = $"{Context}/{Key}";

        var binding = new ReflectionBindingExtension($"[{keyToUse}]")
        {
            Mode = BindingMode.OneWay, Source = Localizer.Instance,
        };

        return binding.ProvideValue(serviceProvider);
    }
}