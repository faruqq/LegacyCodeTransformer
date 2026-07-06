using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Statement parser recovery davranışlarını merkezi hale getirir.
///
/// Neden var?
/// ----------------------
/// Desteklenmeyen veya hatalı statement görüldüğünde parser'ın aynı token üzerinde
/// takılıp sonsuz döngüye girmemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Statement sonuna kadar güvenli ilerleme davranışını tek helper altında toplar.
/// Böylece StatementParser, AssignmentStatementParser ve ileride eklenecek diğer
/// parser'larda aynı SkipCurrentStatement kodu tekrar edilmez.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     CALL FETCH_CURSOR;
///     PARAM 'ABC';
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser unsupported statement recovery içinde ve AssignmentStatementParser
/// hatalı assignment recovery içinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Recovery mode, nested block recovery ve unsupported syntax recovery davranışları
/// bu helper üzerinden genişletilebilir.
/// </summary>
internal sealed class StatementRecoveryHelper : ParserBase
{
    public StatementRecoveryHelper(ParseContext context)
        : base(context)
    {
    }

    public void SkipCurrentStatement()
    {
        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            Advance();
        }

        if (Current.Kind == Pl1TokenKind.Semicolon)
        {
            Advance();
        }
    }
}