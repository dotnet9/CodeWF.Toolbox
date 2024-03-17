using Microsoft.CodeAnalysis;
using Tools.CodeWF.Fluent.Generators.Abstractions;

namespace Tools.CodeWF.Fluent.Generators.Generators;

[Generator]
internal class MainGenerator : CombinedGenerator
{
	public MainGenerator()
	{
		AddStaticFileGenerator<AutoInterfaceAttributeGenerator>();
		AddStaticFileGenerator<AutoNotifyAttributeGenerator>();
		Add<AutoNotifyGenerator>();
		Add<AutoInterfaceGenerator>();
		Add<UiContextConstructorGenerator>();
		Add<FluentNavigationGenerator>();
	}
}
