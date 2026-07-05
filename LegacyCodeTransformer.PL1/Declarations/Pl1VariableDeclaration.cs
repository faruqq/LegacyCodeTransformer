using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.InitialValues;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Declarations;

/// <summary>
/// PL/I değişken tanımını temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I kodunda DCL / DECLARE ifadesiyle tanımlanan değişkenlerin syntax tree içerisinde tip güvenli şekilde tutulması için oluşturulmuştur.
///
/// Ne çözüyor?
/// ----------------------
/// Değişken adı, veri tipi, optional başlangıç değeri ve optional array/dimension bilgisini tek modelde toplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL MUST_NO FIXED DECIMAL(8);
/// - DCL PARAM CHAR(08) INIT(' ');
/// - DCL PARAM CHAR(10) DIM(2);
/// - DCL PARAM CHAR(10) DIMENSION(2);
/// - DCL PARAM(2) CHAR(10);
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I Parser çıktısında
/// - PL/I Normalizer içerisinde
/// - PL/I → EGL Transpiler dönüşüm kurallarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Array declaration, default value mapping, DIM/DIMENSION attribute ve ileride çok boyutlu array desteği için merkezi declaration modelidir.
/// </summary>
public sealed class Pl1VariableDeclaration : Pl1Declaration
{
    public string Name { get; }

    public Pl1DataType DataType { get; }

    public Pl1InitialValue? InitialValue { get; }

    public int? ArraySize { get; }

    /// <summary>
    /// PL/I değişken declaration modelini oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, DCL / DECLARE söz dizimini okuduğunda değişken adını, veri tipini, varsa başlangıç değerini ve varsa dimension bilgisini tek bir declaration modeli üzerinde saklamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Top-level variable declaration için scalar ve array değişkenleri aynı model üzerinden temsil eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08) INIT(' ');
    /// - DCL PARAM CHAR(10) DIM(2);
    /// - DCL PARAM(2) CHAR(10);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser içerisinde
    /// - Unit testlerde declaration modelini doğrulamada
    /// - Transpiler katmanına PL/I declaration bilgisini taşımada
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çok boyutlu DIMENSION desteği ve EGL/C# gibi hedef dillerde array mapping kuralları bu property üzerinden genişletilebilir.
    /// </summary>
    public Pl1VariableDeclaration(
        string name,
        Pl1DataType dataType,
        SourceLocation location,
        Pl1InitialValue? initialValue = null,
        int? arraySize = null)
        : base(location)
    {
        Name = name;
        DataType = dataType;
        InitialValue = initialValue;
        ArraySize = arraySize;
    }
}