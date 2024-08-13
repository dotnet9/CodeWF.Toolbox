using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CodeWF.Toolbox.Localization;

public class LocalizeExtension(object key) : MarkupExtension
{
    public object Key { get; set; } = key;

    public string? Context { get; set; }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Localizer))]
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Key is not BindingBase keyBinding)
        {
            var keyToUse = Key;
            if (!string.IsNullOrWhiteSpace(Context))
                keyToUse = $"{Context}/{Key}";

            var binding = new ReflectionBindingExtension($"[{keyToUse}]")
            {
                Mode = BindingMode.OneWay, 
                Source = Localizer.Instance,
            };

            return binding.ProvideValue(serviceProvider);
        }

        var multiBinding = new LocalizableResourceExtension(Key);
        return multiBinding;
    }
}