using Microsoft.CodeAnalysis;

namespace Tools.CodeWF.Fluent.Generators.Abstractions;

internal record GeneratorStepContext(GeneratorExecutionContext Context, Compilation Compilation);
