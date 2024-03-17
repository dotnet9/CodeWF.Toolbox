namespace Tools.CodeWF.Fluent.Generators.Abstractions;

internal abstract class StaticFileGenerator
{
	public abstract IEnumerable<(string FileName, string Source)> Generate();
}
