using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Types;

/// <summary>
/// EGL smallfloat veri tipini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I REAL tipi single precision floating semantic taşıdığı için EGL tarafında
/// float yerine smallfloat olarak temsil edilmelidir.
///
/// Ne çözüyor?
/// ----------------------
/// REAL tipini EGL syntax tree üzerinde ayrı bir numeric floating model olarak korur.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL RATE REAL; => Rate smallfloat;
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1ToEglTranspiler.TranspileFloatingType içinde
/// - EglCodeGenerator.GenerateDataType içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Single precision floating metadata veya farklı hedef dil mapping kararları
/// gerektiğinde bu model genişletilebilir.
/// </summary>
public sealed class EglSmallFloatType : EglDataType
{
    public EglSmallFloatType(SourceLocation location)
        : base(location)
    {
    }
}