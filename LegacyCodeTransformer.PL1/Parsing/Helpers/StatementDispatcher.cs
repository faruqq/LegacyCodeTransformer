using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I statement parser yönlendirme kararlarını merkezi olarak verir.
///
/// Neden var?
/// ----------------------
/// Statement parser büyüdükçe CALL, IF, DO, assignment, SELECT, READ, WRITE,
/// RETURN, STOP, LEAVE ve EXEC SQL gibi farklı statement türleri ortaya çıkacaktır.
/// Bu türlerin tamamını StatementParser içinde if/switch bloklarıyla büyütmek
/// StatementParser'ı tekrar orchestration dışına çıkarır.
///
/// Ne çözüyor?
/// ----------------------
/// Mevcut token'ın hangi statement ailesine ait olduğunu ve hangi concrete parser
/// türüne yönlendirileceğini merkezi olarak belirler.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PARAM = 'ABC';              -> Assignment
/// - CALL FETCH_CURSOR;          -> CALL
/// - IF SQLCODE = 0 THEN DO;     -> IF
/// - DO WHILE(SQLCODE = 0);      -> DO
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser içinde, mevcut token'ın statement başlangıcı olup olmadığını
/// anlamak ve uygun parser türünü seçmek için kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// AssignmentStatementParser, CallStatementParser, IfStatementParser,
/// DoStatementParser ve ileride legacy statement parser modülleri bu dispatcher
/// üzerinden sisteme eklenecektir.
/// </summary>
internal sealed class StatementDispatcher
{
    /// <summary>
    /// Verilen token türünün desteklenen veya planlanan bir statement başlangıcı
    /// olup olmadığını belirtir.
    ///
    /// Neden var?
    /// ----------------------
    /// Top-level parser declaration dışındaki tokenları statement parser'a
    /// yönlendirebilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Statement başlangıç kararını Pl1Parser veya StatementParser içine dağıtmak
    /// yerine tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Identifier ile başlayan assignment statement:
    ///
    ///     PARAM = 'ABC';
    ///
    /// CALL keyword ile başlayan procedure call:
    ///
    ///     CALL FETCH_CURSOR;
    ///
    /// IF keyword ile başlayan condition:
    ///
    ///     IF SQLCODE = 0 THEN DO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement methodu içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni statement türleri eklendikçe yalnızca GetParserKind methodu
    /// genişletilerek dispatcher sözleşmesi korunur.
    /// </summary>
    public bool IsStatementStart(Pl1TokenKind tokenKind)
    {
        return GetParserKind(tokenKind) != StatementParserKind.Unknown;
    }

    /// <summary>
    /// Statement başlangıç token'ı için ilgili parser türünü döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.3 ile birlikte dispatcher yalnızca statement ailesini değil,
    /// kullanılacak concrete parser türünü de belirlemeye başlayacaktır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// StatementParser içerisindeki parser seçim mantığını merkezi hale getirir.
    /// Böylece StatementParser büyüyen if/switch kararlarıyla kirlenmez.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Identifier  -> AssignmentStatementParser
    /// - CALL        -> CallStatementParser
    /// - IF          -> IfStatementParser
    /// - DO          -> DoStatementParser
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser içinde concrete parser seçimi için kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// SELECT, READ, WRITE, RETURN, STOP, LEAVE, EXEC SQL gibi yeni statement
    /// parser modülleri yalnızca bu dispatch tablosuna eklenerek sisteme dahil
    /// edilecektir.
    /// </summary>
    public StatementParserKind GetParserKind(Pl1TokenKind tokenKind)
    {
        return tokenKind switch
        {
            Pl1TokenKind.Identifier => StatementParserKind.Assignment,
            Pl1TokenKind.CallKeyword => StatementParserKind.Call,
            Pl1TokenKind.IfKeyword => StatementParserKind.If,
            Pl1TokenKind.DoKeyword => StatementParserKind.Do,
            _ => StatementParserKind.Unknown
        };
    }

    /// <summary>
    /// Statement başlangıç token'ı için beklenen statement ailesinin adını döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Diagnostic ve testlerde statement ailesini okunabilir şekilde göstermek gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token kind değerlerini kullanıcıya ve testlere daha anlamlı statement ailesi
    /// isimleriyle sunar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Identifier  -> Assignment
    /// - CallKeyword -> CALL
    /// - IfKeyword   -> IF
    /// - DoKeyword   -> DO
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser diagnostic mesajlarında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parser dispatch loglama, diagnostic code standardı veya parser selection
    /// metricleri gerektiğinde bu method genişletilebilir.
    /// </summary>
    public string GetStatementFamilyName(Pl1TokenKind tokenKind)
    {
        return GetParserKind(tokenKind) switch
        {
            StatementParserKind.Assignment => "Assignment",
            StatementParserKind.Call => "CALL",
            StatementParserKind.If => "IF",
            StatementParserKind.Do => "DO",
            _ => "Unknown"
        };
    }
}