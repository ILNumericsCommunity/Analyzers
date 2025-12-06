using Microsoft.CodeAnalysis;

namespace ILNumerics.Community.Analyzers;

internal static class IlnTypes
{
    private const string Namespace = "ILNumerics";

    public static bool IsArray(INamedTypeSymbol? t)
    {
        return IsNamed(t, Namespace, "Array`1") || IsNamed(t, Namespace, "Logical") || IsNamed(t, Namespace, "Cell");
    }

    public static bool IsIlnIn(INamedTypeSymbol? t)
    {
        return IsNamed(t, Namespace, "InArray`1") || IsNamed(t, Namespace, "InLogical") || IsNamed(t, Namespace, "InCell");
    }

    public static bool IsIlnOut(INamedTypeSymbol? t)
    {
        return IsNamed(t, Namespace, "OutArray`1") || IsNamed(t, Namespace, "OutLogical") || IsNamed(t, Namespace, "OutCell");
    }

    public static bool IsIlnRet(INamedTypeSymbol? t)
    {
        return IsNamed(t, Namespace, "RetArray`1") || IsNamed(t, Namespace, "RetLogical") || IsNamed(t, Namespace, "RetCell");
    }

    public static bool IsAnyIln(INamedTypeSymbol? t)
    {
        return IsArray(t) || IsIlnIn(t) || IsIlnOut(t) || IsIlnRet(t);
    }

    private static bool IsNamed(INamedTypeSymbol? t, string ns, string metadataName)
    {
        return t is not null && t.ContainingNamespace?.ToDisplayString() == ns && t.MetadataName == metadataName;
    }
}
