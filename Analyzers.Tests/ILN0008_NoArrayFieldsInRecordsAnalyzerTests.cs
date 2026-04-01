using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0008_NoArrayFieldsInRecordsAnalyzerTests
{
    [Fact]
    public async Task Flags_Array_Field_In_Record()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public record RecordWithArrayField
{
    private readonly Array<double> {|ILN0008:_A|} = localMember<double>();
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Cell_Field_In_Record()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public record RecordWithCellField
{
    private readonly Cell {|ILN0008:_C|} = localCell();
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Logical_Field_In_Record()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public record RecordWithLogicalField
{
    private readonly Logical {|ILN0008:_L|} = localLogical();
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Array_Field_In_Record_Struct()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public record struct RecordStructWithArrayField
{
    private readonly Array<double> {|ILN0008:_A|};
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Flag_Array_Field_In_Class()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public class ClassWithArrayField
{
    private readonly Array<double> _A = localMember<double>();
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Flag_Array_Field_In_Struct()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public struct StructWithArrayField
{
    private readonly Array<double> _A = localMember<double>();

    public StructWithArrayField() { }
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Flag_NonILN_Field_In_Record()
    {
        var test = @"
public record RecordWithNormalField
{
    private readonly double[] _A = [];
}
";

        await new CSharpVerifier<ILN0008_NoArrayFieldsInRecordsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }
}
