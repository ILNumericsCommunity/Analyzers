using System.Diagnostics.CodeAnalysis;
using ILNumerics;
using static ILNumerics.ILMath;

namespace Playground;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static class Program
{
    public static void Main()
    {
        // Example usages of the ILNxxxx diagnostics
        ILN0001_NoVarForArrays();

        var c = new ILN0002_NoInOutRetInBody_Class();
        c.M();

        var d = ILN0003_SignatureFlavor(zeros<double>(2, 2));

        ILN0004_NoRefOrOutWithILNumericsArrays(out var e);
    }

    // ILN0001: No 'var' for ILNumerics arrays
    private static void ILN0001_NoVarForArrays()
    {
        var a = zeros<double>(3, 4);
        var b = zeros<double>(3, 4);
    }

    // ILN0002: No In/Out/Ret in method body
    private class ILN0002_NoInOutRetInBody_Class
    {
        // Field
        private InArray<double> f;

        // Property
        private OutArray<double> P { get; set; }

        public void M()
        {
            // Local
            RetArray<double> r = null;
        }
    }

    // ILN0003: Suggest In/Out/Ret in signatures
    public static Array<double> ILN0003_SignatureFlavor(Array<double> a)
    {
        return zeros<double>(3, 4);
    }

    // ILN0004: No 'ref' or 'out' with ILNumerics arrays
    public static void ILN0004_NoRefOrOutWithILNumericsArrays(out Array<double> outA)
    {
        outA = zeros<double>(3, 4);
    }
}
