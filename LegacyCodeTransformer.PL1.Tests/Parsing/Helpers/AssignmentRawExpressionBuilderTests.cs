using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// AssignmentRawExpressionBuilder için raw expression text üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// P05.3 aşamasında assignment statement parser henüz gerçek expression tree
/// üretmez. Expression tarafı Pl1RawExpression olarak taşındığı için tokenlardan
/// üretilen raw text formatının stabil olması gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Identifier, number, string literal, qualified member access, array access ve
/// arithmetic operator içeren token dizilerinin beklenen raw expression metnine
/// dönüştüğünü doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PARAM
/// - 'ABC'
/// - DCLGLAU.BRM_KOD
/// - DIZI(I)
/// - B + C
///
/// Nerede kullanılır?
/// ----------------------
/// AssignmentStatementParser target ve value expression üretiminde dolaylı olarak
/// bu helper davranışına bağlıdır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// ExpressionParser geldiğinde raw expression fallback davranışının bozulmadığını
/// garanti eden regression testleri olarak kalabilir.
/// </summary>
public sealed class AssignmentRawExpressionBuilderTests
{
    /// <summary>
    /// Identifier tokenının raw expression olarak üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Tek identifier tokenı doğrudan aynı text ile raw expression'a dönüşmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM
    ///
    /// Beklenen temel model/çıktı nedir?
    /// PARAM metni dönmelidir.
    /// </summary>
    [Fact]
    public void Build_WithIdentifierToken_ShouldReturnIdentifierText()
    {
        var tokens = new[]
        {
            CreateToken(Pl1TokenKind.Identifier, "PARAM")
        };

        var result = AssignmentRawExpressionBuilder.Build(tokens);

        Assert.Equal("PARAM", result);
    }

    /// <summary>
    /// String literal tokenının quote restore edilerek raw expression üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Lexer string literal token text değerini tırnaksız taşır. Raw expression üretiminde
    /// bu değer PL/I kaynak görünümüne uygun şekilde tek tırnakla restore edilmelidir.
    ///
    /// Hangi input'u test eder?
    /// StringLiteral token text değeri ABC.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// 'ABC' metni dönmelidir.
    /// </summary>
    [Fact]
    public void Build_WithStringLiteralToken_ShouldRestoreQuotes()
    {
        var tokens = new[]
        {
            CreateToken(Pl1TokenKind.StringLiteral, "ABC")
        };

        var result = AssignmentRawExpressionBuilder.Build(tokens);

        Assert.Equal("'ABC'", result);
    }

    /// <summary>
    /// Qualified member access için nokta etrafında boşluk üretilmediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Structure member access raw expression text içinde DCLGLAU.BRM_KOD formatında
    /// korunmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCLGLAU . BRM_KOD tokenları.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DCLGLAU.BRM_KOD metni dönmelidir.
    /// </summary>
    [Fact]
    public void Build_WithQualifiedMemberTokens_ShouldReturnTextWithoutSpacesAroundDot()
    {
        var tokens = new[]
        {
            CreateToken(Pl1TokenKind.Identifier, "DCLGLAU"),
            CreateToken(Pl1TokenKind.Dot, "."),
            CreateToken(Pl1TokenKind.Identifier, "BRM_KOD")
        };

        var result = AssignmentRawExpressionBuilder.Build(tokens);

        Assert.Equal("DCLGLAU.BRM_KOD", result);
    }

    /// <summary>
    /// Array access için parantez etrafında gereksiz boşluk üretilmediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Array index access raw expression text içinde DIZI(I) formatında korunmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DIZI ( I ) tokenları.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DIZI(I) metni dönmelidir.
    /// </summary>
    [Fact]
    public void Build_WithArrayAccessTokens_ShouldReturnTextWithoutSpacesAroundParentheses()
    {
        var tokens = new[]
        {
            CreateToken(Pl1TokenKind.Identifier, "DIZI"),
            CreateToken(Pl1TokenKind.OpenParenthesis, "("),
            CreateToken(Pl1TokenKind.Identifier, "I"),
            CreateToken(Pl1TokenKind.CloseParenthesis, ")")
        };

        var result = AssignmentRawExpressionBuilder.Build(tokens);

        Assert.Equal("DIZI(I)", result);
    }

    /// <summary>
    /// Arithmetic expression için operator etrafında okunabilir boşluk üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Binary arithmetic raw expression text içinde B + C formatında taşınmalıdır.
    ///
    /// Hangi input'u test eder?
    /// B + C tokenları.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// B + C metni dönmelidir.
    /// </summary>
    [Fact]
    public void Build_WithArithmeticExpressionTokens_ShouldReturnTextWithSpacesAroundOperator()
    {
        var tokens = new[]
        {
            CreateToken(Pl1TokenKind.Identifier, "B"),
            CreateToken(Pl1TokenKind.Plus, "+"),
            CreateToken(Pl1TokenKind.Identifier, "C")
        };

        var result = AssignmentRawExpressionBuilder.Build(tokens);

        Assert.Equal("B + C", result);
    }

    private static Pl1Token CreateToken(
        Pl1TokenKind kind,
        string text)
    {
        return new Pl1Token(
            kind,
            text,
            SourceLocation.Unknown);
    }
}