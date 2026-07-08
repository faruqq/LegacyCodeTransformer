using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I embedded SQL statement parse eden concrete parser sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Kurumsal PL/I kodlarında EXEC SQL statement'ları çok yaygındır. Parser'ın
/// bu statement'ları hata olarak atlaması, kaynak kodun iş mantığını ve veri erişim
/// noktalarını kaybetmesine neden olur.
///
/// Ne çözüyor?
/// ----------------------
/// EXEC SQL ile başlayan ve noktalı virgül ile biten statement'ı Pl1EmbeddedSqlStatement
/// olarak syntax tree'ye taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// EXEC SQL INCLUDE SQLCA;
///
/// EXEC SQL SELECT CUSTOMER_NO
///     INTO :CUSTOMER_NO
///     FROM CUSTOMER_TABLE;
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser, ExecKeyword gördüğünde bu parser'ı çağırır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// SQL statement türü ayrıştırma, host variable parse etme, SQLCA include handling
/// ve EGL SQL generation bu parser çıktısı üzerinden geliştirilecektir.
/// </summary>
internal sealed class EmbeddedSqlStatementParser : ParserBase
{
    public EmbeddedSqlStatementParser(ParseContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Mevcut pozisyondan EXEC SQL statement parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// StatementParser'ın embedded SQL detaylarını bilmeden tek bir concrete parser'a
    /// yönlenmesini sağlar.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EXEC SQL keyword çiftini doğrular, statement içeriğini semicolon'a kadar okur
    /// ve raw SQL statement modelini üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// EXEC SQL INCLUDE SQLCA;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementParser.ParseStatement içinde EmbeddedSql parser seçildiğinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EXEC SQL SELECT / INSERT / UPDATE / DELETE ayrıştırması ve SQL INCLUDE
    /// özel modeli bu methodun çıktısı üzerine inşa edilebilir.
    /// </summary>
    public HelperParseResult<Pl1Statement> ParseEmbeddedSqlStatement()
    {
        var startLocation = Current.Location;
        var tokens = new List<Pl1Token>();

        if (Current.Kind != Pl1TokenKind.ExecKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "EXEC bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        tokens.Add(Advance());

        if (Current.Kind != Pl1TokenKind.SqlKeyword)
        {
            Diagnostics.Add(
                ParserDiagnosticFactory.ExpectedToken(
                    "SQL bekleniyordu.",
                    Current));

            return new HelperParseResult<Pl1Statement>(
                null,
                Position);
        }

        tokens.Add(Advance());

        while (!IsAtEnd() && Current.Kind != Pl1TokenKind.Semicolon)
        {
            tokens.Add(Advance());
        }

        Consume(Pl1TokenKind.Semicolon, "';' bekleniyordu.");

        var rawSqlText = BuildRawText(tokens);

        var statement = new Pl1EmbeddedSqlStatement(
            rawSqlText,
            startLocation);

        return new HelperParseResult<Pl1Statement>(
            statement,
            Position);
    }

    private static string BuildRawText(IReadOnlyList<Pl1Token> tokens)
    {
        return string.Join(
            " ",
            tokens.Select(GetTokenText));
    }

    private static string GetTokenText(Pl1Token token)
    {
        return token.Kind == Pl1TokenKind.StringLiteral
            ? $"'{token.Text}'"
            : token.Text;
    }
}