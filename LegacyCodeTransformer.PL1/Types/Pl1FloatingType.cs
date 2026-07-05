using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Types;

/// <summary>
/// PL/I FLOAT / REAL / DOUBLE veri tiplerini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I numeric type ailesi yalnızca FIXED DECIMAL ve FIXED BINARY tiplerinden
/// oluşmaz. FLOAT, REAL ve DOUBLE gibi floating point tipler de ayrı semantic
/// veri tipleridir.
///
/// Ne çözüyor?
/// ----------------------
/// Floating point declaration bilgisini parser aşamasında kaybetmeden AST modeline
/// taşır. Precision ve decimal/binary base bilgisi varsa korunur.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL A FLOAT;
/// - DCL A FLOAT DECIMAL;
/// - DCL A FLOAT DECIMAL(16);
/// - DCL A FLOAT BINARY;
/// - DCL A FLOAT BIN(53);
/// - DCL A REAL;
/// - DCL A DOUBLE;
/// - DCL A DOUBLE PRECISION;
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I Parser veri tipi parse işleminde
/// - PL/I → EGL Transpiler unsupported diagnostic üretiminde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// FLOAT ailesi için EGL mapping, C# mapping veya COBOL mapping kararları
/// bu model üzerinden geliştirilebilir.
/// </summary>
public sealed class Pl1FloatingType : Pl1DataType
{
    public Pl1FloatingTypeKind Kind { get; }

    public Pl1FloatingBase Base { get; }

    public int? Precision { get; }

    public Pl1FloatingType(
        Pl1FloatingTypeKind kind,
        Pl1FloatingBase floatingBase,
        int? precision,
        SourceLocation location)
        : base(location)
    {
        Kind = kind;
        Base = floatingBase;
        Precision = precision;
    }
}