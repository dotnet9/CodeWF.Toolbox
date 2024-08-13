using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using CodeWF.Toolbox.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CodeWF.Toolbox.Localization;

public class LocalizableResourceExtension : MultiBinding
{
    public LocalizableResourceExtension(object key)
    {
        this.Mode = BindingMode.OneWay;
        this.Converter = new LocalizableResourceConverter(this);
        this.KeyConverter = new LocalizableKeyConverter();
        this.ValueConverter = new LocalizableValueConverter();
        this.Args = new ArgCollection(this);

        var cultureBinding = new Binding
        {
            Source = LocalizationManager.Instance, Path = nameof(LocalizationManager.Culture)
        };
        this.Bindings.Add(cultureBinding);

        this.Key = key;
        if (this.Key is not BindingBase keyBinding)
        {
            keyBinding = new Binding { Source = key };
        }

        this.Bindings.Add(keyBinding);
    }

    public LocalizableResourceExtension(object key, params object[] args) : this(key)
    {
        foreach (var arg in args)
        {
            this.Args.Add(arg);
        }
    }

    public object Key { get; }
    public ArgCollection Args { get; }

    [ConstructorArgument(nameof(KeyConverter))]
    public IValueConverter KeyConverter { get; set; }

    [ConstructorArgument(nameof(ValueConverter))]
    public IValueConverter ValueConverter { get; set; }

    public class ArgCollection : Collection<object>
    {
        internal List<(bool IsBinding, int Index)> Indexes { get; } = new();
        private readonly LocalizableResourceExtension owner;

        public ArgCollection(LocalizableResourceExtension owner)
        {
            this.owner = owner;
        }

        protected override void InsertItem(int index, object item)
        {
            if (item is BindingBase binding)
            {
                this.Indexes.Add((true, this.owner.Bindings.Count));
                this.owner.Bindings.Add(binding);
            }
            else
            {
                this.Indexes.Add((false, this.Count));
                base.InsertItem(index, item);
            }
        }
    }
}