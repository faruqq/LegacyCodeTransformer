using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Parser helper sınıfları arasında paylaşılan token state ve diagnostic context bilgisini taşır.
///
/// Neden var?
/// ----------------------
/// P04 refactor sonrası her helper parser kendi token listesi, position ve diagnostic bag
/// alanlarını tekrar etmeye başladı.
///
/// Ne çözüyor?
/// ----------------------
/// Token listesi, mevcut position ve diagnostic bag bilgisini tek context modeli altında
/// toplar. Böylece helper parser sınıfları aynı parse state üzerinde çalışabilir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CharacterTypeParser, BitTypeParser, NumericTypeParser, StructureParser gibi helper
/// parser sınıfları aynı ParseContext modeliyle çalışabilir.
///
/// Nerede kullanılır?
/// ----------------------
/// - ParserBase içinde
/// - Helper parser constructorlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement parser geliştirmelerinde IF, DO, CALL, assignment ve expression parser
/// sınıfları aynı context modeliyle çalışacaktır.
/// </summary>
internal sealed class ParseContext
{
    public IReadOnlyList<Pl1Token> Tokens { get; }

    public DiagnosticBag Diagnostics { get; }

    public int Position { get; set; }

    /// <summary>
    /// ParseContext instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Helper parser sınıflarına token listesi, başlangıç position ve diagnostic bag
    /// bilgisini standart bir modelle vermek gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Null token listesi yerine boş liste, null diagnostic bag yerine yeni DiagnosticBag
    /// kullanarak parser helper oluşturmayı güvenli hale getirir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// new ParseContext(tokens, position, diagnostics)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser helper dispatch noktalarında
    /// - DataTypeParser gibi başka helper parserları çağıran parserlarda
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Source file bilgisi, parse mode, recovery mode veya feature flag gibi ortak parser
    /// state alanları gerektiğinde bu model genişletilebilir.
    /// </summary>
    public ParseContext(
        IReadOnlyList<Pl1Token>? tokens,
        int position,
        DiagnosticBag? diagnostics)
    {
        Tokens = tokens ?? Array.Empty<Pl1Token>();
        Position = position;
        Diagnostics = diagnostics ?? new DiagnosticBag();
    }
}