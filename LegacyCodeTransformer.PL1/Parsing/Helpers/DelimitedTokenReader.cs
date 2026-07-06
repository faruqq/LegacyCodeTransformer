using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Parser token stream üzerinde delimiter bazlı token okuma davranışını merkezi hale getirir.
///
/// Neden var?
/// ----------------------
/// Assignment, CALL, IF, DO, SELECT, PUT, GET ve EXEC SQL gibi statement parser'larda
/// belirli delimiter tokenlara kadar token toplama ihtiyacı tekrar edecektir.
///
/// Ne çözüyor?
/// ----------------------
/// Her parser içinde ReadTokensUntil, ReadArgumentTokens veya benzeri methodların
/// tekrar yazılmasını engeller.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     A = B;
///     CALL PROC1(A, 'ABC', B);
///     IF SQLCODE = 0 THEN DO;
///
/// Nerede kullanılır?
/// ----------------------
/// AssignmentStatementParser ve CallStatementParser içinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// IF condition parsing, DO condition parsing, SELECT branch parsing ve embedded SQL
/// statement boundary yönetimi için ortak helper olarak genişletilecektir.
/// </summary>
internal sealed class DelimitedTokenReader : ParserBase
{
    public DelimitedTokenReader(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Tek delimiter token'a kadar token okur.
    ///
    /// Neden var?
    /// ----------------------
    /// Assignment gibi yapılarda '=' veya ';' gibi tek bir sınır tokenına kadar
    /// token okumak gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Delimiter tokenını tüketmeden delimiter öncesindeki tokenları döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     A = B;
    ///
    /// '=' öncesinde A tokenı okunur, '=' tüketilmeden yerinde bırakılır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// AssignmentStatementParser içinde target ve value taraflarını ayırmak için.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement boundary ve expression boundary ayrımlarında kullanılmaya devam eder.
    /// </summary>
    public List<Pl1Token> ReadUntil(Pl1TokenKind delimiterKind)
    {
        return ReadUntilAny(delimiterKind);
    }

    /// <summary>
    /// Birden fazla delimiter token'dan herhangi birine kadar token okur.
    ///
    /// Neden var?
    /// ----------------------
    /// CALL argument parsing gibi yapılarda argument virgül veya kapanış parantezi ile
    /// bitebilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Birden fazla delimiter olasılığında ortak token okuma davranışı sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     CALL PROC1(A, 'ABC', B);
    ///
    /// A argument'ı Comma veya CloseParenthesis delimiterlarından birine kadar okunur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// CallStatementParser içinde argument tokenlarını okumak için.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// IF condition, SELECT WHEN listesi ve function argument parsing için genişletilebilir.
    /// </summary>
    public List<Pl1Token> ReadUntilAny(params Pl1TokenKind[] delimiterKinds)
    {
        var tokens = new List<Pl1Token>();

        while (!IsAtEnd() && !delimiterKinds.Contains(Current.Kind))
        {
            tokens.Add(Advance());
        }

        return tokens;
    }
}