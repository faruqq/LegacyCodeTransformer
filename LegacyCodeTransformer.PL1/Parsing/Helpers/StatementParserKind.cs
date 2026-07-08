namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Statement dispatcher tarafından seçilecek parser türünü belirtir.
///
/// Neden var?
/// ----------------------
/// Statement parser seçimini enum üzerinden yöneterek switch ifadelerini
/// okunabilir ve genişletilebilir hale getirir.
///
/// Ne çözüyor?
/// ----------------------
/// Token türü ile parser implementasyonu arasındaki ilişkiyi standartlaştırır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// Assignment, CALL, IF, DO ve EXEC SQL parser seçimleri.
///
/// Nerede kullanılır?
/// ----------------------
/// StatementDispatcher.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// SELECT, RETURN, LEAVE ve diğer legacy parser türleri de bu enum'a eklenecektir.
/// </summary>
internal enum StatementParserKind
{
    Unknown,
    Assignment,
    Call,
    If,
    Do,
    EmbeddedSql
}