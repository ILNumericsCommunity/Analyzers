using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ILNumerics.Community.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0003_SignatureFlavorFix))]
[Shared]
public sealed class ILN0003_SignatureFlavorFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0003", "ILN0003R"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan);

        if (node.FirstAncestorOrSelf<ParameterSyntax>() is { } param)
        {
            // If parameter has 'out' modifier, only offer OutArray<>
            var hasOut = param.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword));
            if (hasOut)
            {
                context.RegisterCodeFix(CodeAction.Create("Change parameter to OutArray<>", ct => ParamToOutAsync(context.Document, param, ct), "ILN0003"), diagnostic);
            }
            else
            {
                context.RegisterCodeFix(CodeAction.Create("Change parameter to InArray<>", ct => ParamToInAsync(context.Document, param, ct), "ILN0003"), diagnostic);
                context.RegisterCodeFix(CodeAction.Create("Change parameter to OutArray<>", ct => ParamToOutAsync(context.Document, param, ct), "ILN0003"), diagnostic);
            }
        }
        else if (node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } method)
        {
            context.RegisterCodeFix(CodeAction.Create("Change return type to RetArray<>", ct => ReturnToRetAsync(context.Document, method, ct), "ILN0003R"), diagnostic);
        }
    }

    private static async Task<Document> ParamToInAsync(Document doc, ParameterSyntax param, CancellationToken ct)
    {
        var newType = ToIlFlavor(param.Type!, "InArray");
        var newParam = param.WithType(newType);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(param, newParam));
    }

    private static async Task<Document> ParamToOutAsync(Document doc, ParameterSyntax param, CancellationToken ct)
    {
        var newType = ToIlFlavor(param.Type!, "OutArray");
        var newParam = param.WithType(newType);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(param, newParam));
    }

    private static async Task<Document> ReturnToRetAsync(Document doc, MethodDeclarationSyntax method, CancellationToken ct)
    {
        var retType = method.ReturnType;
        var newType = ToIlFlavor(retType, "RetArray");
        var newMethod = method.WithReturnType(newType);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(method, newMethod));
    }

    private static TypeSyntax ToIlFlavor(TypeSyntax type, string flavor)
    {
        if (type is GenericNameSyntax g && g.Identifier.Text == "Array")
            return g.WithIdentifier(SyntaxFactory.Identifier(flavor));

        // Fallback: wrap plain 'Array' without generic args
        if (type is IdentifierNameSyntax id && id.Identifier.Text == "Array")
        {
            return SyntaxFactory.GenericName(SyntaxFactory.Identifier(flavor))
                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName("double"))));
        }

        return type;
    }
}
