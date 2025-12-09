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

        var fieldClass = new ClassWithArrayFields(zeros<double>(2, 2));
        fieldClass.Update(zeros<double>(2, 1));
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

// ILN0005: Enforce localMember<T>() initialization for fields
// ILN0006: Enforce .a / Assign() for field writes
internal sealed class ClassWithArrayFields
{
    private Array<double> _fieldA;
    private Array<double> _fieldB = zeros<double>(2, 3);

    public ClassWithArrayFields(InArray<double> inA)
    {
        _fieldA = check(inA);
        _fieldB = ones<double>(3, 4);
    }

    public void Update(InArray<double> x)
    {
        _fieldA = x;
        _fieldB = x;
    }
}
