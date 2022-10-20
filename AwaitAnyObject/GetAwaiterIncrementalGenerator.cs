global using TargetType = System.Int32;

using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwaitAnyObject;

[Generator(LanguageNames.CSharp)]
public class GetAwaiterIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assembly = typeof(GetAwaiterIncrementalGenerator).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        string templateCode = string.Empty;

        {
            using var resourceStream = assembly.GetManifestResourceStream(resourceNames.Single(m => m.EndsWith("GetAwaiterExtensionTemplate.cs")));
            using var reader = new StreamReader(resourceStream);
            templateCode = reader.ReadToEnd();
        }

        var symbolProvider = context.SyntaxProvider.CreateSyntaxProvider((node, _) => node is AwaitExpressionSyntax, TransformAwaitExpressionSyntax)
                                                   .Where(m => m is not null)
                                                   .WithComparer(SymbolEqualityComparer.Default);

        context.RegisterSourceOutput(symbolProvider.Collect(),
                                    (ctx, input) =>
                                    {
                                        foreach (var item in input.Distinct(SymbolEqualityComparer.Default))
                                        {
                                            var fullyClassName = item!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                            var className = NormalizeClassName(fullyClassName);
                                            var code = templateCode.Replace("TargetTypeName", className)
                                                                   .Replace("TargetType", fullyClassName);

                                            if (item.DeclaredAccessibility != Accessibility.Public)
                                            {
                                                code = code.Replace("public static class", "internal static class");
                                            }

                                            ctx.AddSource($"GetAwaiterFor_{className}.g.cs", code);
                                        }
                                    });
    }

    private static string NormalizeClassName(string value)
    {
        return value.Replace('.', '_')
                    .Replace('<', '_')
                    .Replace('>', '_')
                    .Replace(' ', '_')
                    .Replace(',', '_')
                    .Replace(':', '_');
    }

    private static ITypeSymbol? TransformAwaitExpressionSyntax(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        var awaitExpressionSyntax = (AwaitExpressionSyntax)generatorSyntaxContext.Node;

        if (awaitExpressionSyntax.Expression is AwaitExpressionSyntax)
        {
            return null;
        }

        var awaitExpressionInfo = generatorSyntaxContext.SemanticModel.GetAwaitExpressionInfo(awaitExpressionSyntax);

        if (awaitExpressionInfo.GetAwaiterMethod is null)
        {
            return generatorSyntaxContext.SemanticModel.GetTypeInfo(awaitExpressionSyntax.Expression).Type;
        }

        return null;
    }
}
