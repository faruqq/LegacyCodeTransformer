using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.InitialValues;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Declarations;

/// <summary>
/// EGL değişken tanımını temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I declaration ifadeleri hedef dilde EGL değişken declaration modeline dönüştürülecektir.
///
/// Ne çözüyor?
/// ----------------------
/// Scalar, array ve başlangıç değeri taşıyan EGL variable declaration ifadelerini tek modelde temsil eder.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - MustNo decimal(8);
/// - Param char(10);
/// - Param char(10)[2];
/// - Param char(4) = "ABCD";
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I → EGL Transpiler çıktısında
/// - EGL Code Generator içerisinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Default value, nullable bilgisi, array metadata ve daha karmaşık EGL declaration üretimleri bu model üzerinden genişletilebilir.
/// </summary>
public sealed class EglVariableDeclaration : EglDeclaration
{
    public string Name { get; }

    public EglDataType DataType { get; }

    public int? ArraySize { get; }

    public EglInitialValue? InitialValue { get; }

    /// <summary>
    /// EGL değişken declaration modelini oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler PL/I variable declaration bilgisini EGL variable declaration modeline taşımak zorundadır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Değişken adı, EGL veri tipi, optional array size ve optional başlangıç değeri bilgisini tek modelde tutar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Param char(10);
    /// - Param char(10)[2];
    /// - Param char(4) = "ABCD";
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1ToEglTranspiler.TranspileVariableDeclaration içinde
    /// - EglCodeGenerator.GenerateVariableDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Top-level array initialization, default value ve annotation üretimi gibi davranışlara temel olur.
    /// </summary>
    public EglVariableDeclaration(
        string name,
        EglDataType dataType,
        SourceLocation location,
        int? arraySize = null,
        EglInitialValue? initialValue = null)
        : base(location)
    {
        Name = name;
        DataType = dataType;
        ArraySize = arraySize;
        InitialValue = initialValue;
    }
}