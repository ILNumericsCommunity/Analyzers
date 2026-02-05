using Microsoft.CodeAnalysis;

namespace ILNumerics.Community.Analyzers;

internal static class ILNTypes
{
    private const string Namespace = "ILNumerics";

    public static bool IsArray(INamedTypeSymbol? t) => IsNamed(t, Namespace, "Array`1") || IsNamed(t, Namespace, "Logical") || IsNamed(t, Namespace, "Cell");

    public static bool IsIlnIn(INamedTypeSymbol? t) => IsNamed(t, Namespace, "InArray`1") || IsNamed(t, Namespace, "InLogical") || IsNamed(t, Namespace, "InCell");

    public static bool IsIlnOut(INamedTypeSymbol? t) => IsNamed(t, Namespace, "OutArray`1") || IsNamed(t, Namespace, "OutLogical") || IsNamed(t, Namespace, "OutCell");

    public static bool IsIlnRet(INamedTypeSymbol? t) => IsNamed(t, Namespace, "RetArray`1") || IsNamed(t, Namespace, "RetLogical") || IsNamed(t, Namespace, "RetCell");

    public static bool IsAnyIln(INamedTypeSymbol? t) => IsArray(t) || IsIlnIn(t) || IsIlnOut(t) || IsIlnRet(t);

    /// <summary>
    /// Returns the preferred local ILNumerics type name for a given symbol.
    /// E.g., InArray&lt;double&gt; -> Array&lt;double&gt;, RetLogical -> Logical, etc.
    /// </summary>
    public static string GetPreferredLocalTypeName(INamedTypeSymbol type)
    {
        if (type.ContainingNamespace?.ToDisplayString() == Namespace)
        {
            // In/Out/Ret/Array<T> => Array<T>
            if ((type.MetadataName == "Array`1" || type.MetadataName == "InArray`1" || type.MetadataName == "OutArray`1" || type.MetadataName == "RetArray`1")
                && type.TypeArguments.Length == 1)
            {
                var elemName = type.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                return $"Array<{elemName}>";
            }

            // In/Out/Ret/Logical => Logical
            if (type.MetadataName == "Logical" || type.MetadataName == "InLogical" || type.MetadataName == "OutLogical" || type.MetadataName == "RetLogical")
                return "Logical";

            // In/Out/Ret/Cell => Cell
            if (type.MetadataName == "Cell" || type.MetadataName == "InCell" || type.MetadataName == "OutCell" || type.MetadataName == "RetCell")
                return "Cell";
        }

        // Fallback to the exact inferred type name (minimally qualified)
        return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    private static bool IsNamed(INamedTypeSymbol? t, string ns, string metadataName)
        => t is not null && t.ContainingNamespace?.ToDisplayString() == ns && t.MetadataName == metadataName;
}
