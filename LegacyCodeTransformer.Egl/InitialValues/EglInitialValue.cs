using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.InitialValues;

/// <summary>
/// EGL declaration üzerinde üretilecek başlangıç değerini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I INIT / INITIAL bilgisi parser tarafında korunmaktadır. Bu bilginin EGL output'a
/// güvenli şekilde taşınabilmesi için hedef dil tarafında ayrı bir initial value modeli
/// gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// PL/I başlangıç değerini doğrudan string concatenation ile generator'a taşımak yerine,
/// EGL syntax tree üzerinde tip güvenli bir model olarak saklar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL PARAM CHAR(4) INIT('ABCD'); => Param char(4) = "ABCD";
/// - DCL PARAM CHAR(1) INIT(';'); => Param char(1) = ";";
///
/// Nerede kullanılır?
/// ----------------------
/// - EglVariableDeclaration içinde
/// - Pl1ToEglTranspiler initial value mapping işleminde
/// - EglCodeGenerator variable declaration üretiminde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Numeric default value, array initialization, record field default value ve daha gelişmiş
/// EGL initialization syntax desteği bu model üzerinden genişletilebilir.
/// </summary>
public sealed class EglInitialValue : SyntaxNode
{
    public string Value { get; }

    public EglInitialValue(
        string value,
        SourceLocation location)
        : base(location)
    {
        Value = value;
    }
}