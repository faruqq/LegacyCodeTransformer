using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Parser diagnostic mesajlarını standartlaştıran factory sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Parser helper sınıflarında aynı tip hata mesajları farklı yerlerde elle
/// oluşturulmaktadır.
///
/// Ne çözüyor?
/// ----------------------
/// Beklenen token, beklenmeyen token ve numeric parse hataları gibi ortak diagnostic
/// üretimlerini tek noktada toplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - "'(' bekleniyordu."
/// - "Beklenmeyen token: X. Beklenen: DCL."
/// - "BIT uzunluk değeri sayısal olmalıdır: ABC"
///
/// Nerede kullanılır?
/// ----------------------
/// - ParserBase içinde
/// - Helper parser sınıflarında özel diagnostic üretirken
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Diagnostic code, severity policy, localization veya richer location reporting
/// gerektiğinde parser diagnostic üretimi bu sınıf üzerinden standartlaştırılır.
/// </summary>
internal static class ParserDiagnosticFactory
{
    /// <summary>
    /// Beklenen token bulunamadığında diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser grammar belirli noktalarda belirli token türlerini bekler.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelmediğinde kullanılacak diagnostic formatını standartlaştırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// "';' bekleniyordu."
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ParserBase.Consume içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Expected token diagnostic code veya location enrichment gerekiyorsa buradan
    /// yönetilebilir.
    /// </summary>
    public static Diagnostic ExpectedToken(
        string message,
        Pl1Token actualToken)
    {
        return new Diagnostic(
            DiagnosticSeverity.Error,
            message,
            actualToken.Location);
    }

    /// <summary>
    /// Beklenmeyen token için diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser desteklemediği veya grammar dışı bir token gördüğünde kullanıcıya
    /// beklenen syntax bilgisini vermelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenmeyen token diagnostic mesajını tek formatta üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Beklenmeyen token: ABC. Beklenen: DCL.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser
    /// - StructureParser
    /// - Gelecekte statement parser helper sınıfları
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Recovery stratejileri ve diagnostic code standardı bu method üzerinden
    /// genişletilebilir.
    /// </summary>
    public static Diagnostic UnexpectedToken(
        Pl1Token actualToken,
        string expectedText)
    {
        return new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenmeyen token: {actualToken.Text}. Beklenen: {expectedText}.",
            actualToken.Location);
    }

    /// <summary>
    /// Sayısal olması beklenen token değeri parse edilemediğinde diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Length, precision, scale, array size ve repeat count gibi alanlarda numeric
    /// değer beklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Numeric parse diagnostic mesajını standartlaştırır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// BIT uzunluk değeri sayısal olmalıdır: ABC
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - BitTypeParser
    /// - CharacterTypeParser
    /// - DimensionParser
    /// - NumericTypeParser
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Numeric validation kuralları genişledikçe diagnostic standardı korunur.
    /// </summary>
    public static Diagnostic InvalidNumber(
        string messagePrefix,
        Pl1Token token)
    {
        return new Diagnostic(
            DiagnosticSeverity.Error,
            $"{messagePrefix}: {token.Text}",
            token.Location);
    }
}