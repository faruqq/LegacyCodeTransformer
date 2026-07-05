using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types;

/// <summary>
/// PL/I BIT(n) veri tipini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I tarafında BIT veri tipi karakter veya numeric alan değildir.
/// Belirli uzunlukta bit string alanı tanımlar.
///
/// Ne çözüyor?
/// ----------------------
/// BIT(n) declaration bilgisinin parser tarafından kaybedilmeden syntax tree
/// modeline taşınmasını sağlar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL FLAG BIT(1);
/// - DCL MASK BIT(8);
/// - DCL EOF BIT(1) INIT('0'B);
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I Parser veri tipi parse işleminde
/// - PL/I → EGL Transpiler unsupported diagnostic üretiminde
/// - Gelecekte BIT mapping kararı alındığında hedef dil dönüşümünde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// BIT(1), BIT(n), bit literal INIT ve ileride EGL/C#/COBOL hedeflerinde
/// bit string mapping davranışlarına temel olur.
/// </summary>
public sealed class Pl1BitType : Pl1DataType
{
    public int Length { get; }

    public Pl1BitType(
        int length,
        SourceLocation location)
        : base(location)
    {
        Length = length;
    }
}