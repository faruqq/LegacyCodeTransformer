using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types;

/// <summary>
/// EGL float veri tipini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I DOUBLE, DOUBLE PRECISION ve binary FLOAT tipleri EGL tarafında floating point
/// numeric type olarak üretilebilmelidir.
///
/// Ne çözüyor?
/// ----------------------
/// Generator'a raw string taşımadan, float veri tipini hedef dil syntax tree üzerinde
/// güçlü tipli model olarak temsil eder.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL RATE DOUBLE; => Rate float;
/// - DCL RATE DOUBLE PRECISION; => Rate float;
/// - DCL RATE FLOAT BIN(53); => Rate float;
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1ToEglTranspiler.TranspileFloatingType içinde
/// - EglCodeGenerator.GenerateDataType içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Floating point precision metadata, SQL type metadata veya farklı hedef dil mapping
/// kararları gerektiğinde bu model genişletilebilir.
/// </summary>
public sealed class EglFloatType : EglDataType
{
    public EglFloatType(SourceLocation location)
        : base(location)
    {
    }
}