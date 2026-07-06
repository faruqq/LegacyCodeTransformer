using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// ExpressionFactory için expression üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Statement parser sınıfları doğrudan Pl1RawExpression oluşturmamalıdır.
/// Expression üretimi ExpressionFactory üzerinden merkezi yönetilmelidir.
///
/// Ne çözüyor?
/// ----------------------
/// Token listesinden Pl1Expression üretiminin tek noktadan yapıldığını ve P05.3
/// aşamasında güvenli fallback olarak Pl1RawExpression döndüğünü doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PARAM
/// - 'ABC'
/// - DCLGLAU.BRM_KOD
///
/// Nerede kullanılır?
/// ----------------------
/// AssignmentStatementParser target ve value expression üretiminde ExpressionFactory
/// davranışına bağlıdır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Pl1IdentifierExpression, Pl1LiteralExpression, Pl1BinaryExpression gibi modeller
/// eklendiğinde factory testleri yeni expression türleriyle genişletilecektir.
/// </summary>
public sealed class ExpressionFactoryTests
{
    /// <summary>
    /// Identifier token listesinden Pl1RawExpression üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ExpressionFactory.Create P05.3 aşamasında identifier expression için raw expression
    /// fallback modelini üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM tokenı.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1RawExpression üretilmeli ve Text değeri PARAM olmalıdır.
    /// </summary>
    [Fact]
    public void Create_WithIdentifierToken_ShouldReturnRawExpression()
    {
        var tokens = new[]
        {
        CreateToken(Pl1TokenKind.Identifier, "PARAM")
    };

        var expression = ExpressionFactory.Create(
            tokens,
            SourceLocation.Unknown);

        var rawExpression = Assert.IsType<Pl1RawExpression>(expression);

        Assert.Equal("PARAM", rawExpression.Text);
    }

    /// <summary>
    /// String literal token listesinden quote restore edilmiş Pl1RawExpression üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ExpressionFactory.Create, string literal token değerini PL/I kaynak görünümüne uygun
    /// tek tırnaklı raw expression olarak üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// StringLiteral ABC.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1RawExpression Text değeri 'ABC' olmalıdır.
    /// </summary>
    [Fact]
    public void Create_WithStringLiteralToken_ShouldRestoreQuotes()
    {
        var tokens = new[]
        {
        CreateToken(Pl1TokenKind.StringLiteral, "ABC")
    };

        var expression = ExpressionFactory.Create(
            tokens,
            SourceLocation.Unknown);

        var rawExpression = Assert.IsType<Pl1RawExpression>(expression);

        Assert.Equal("'ABC'", rawExpression.Text);
    }

    /// <summary>
    /// Qualified member token listesinden doğru raw expression üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ExpressionFactory.Create, AssignmentRawExpressionBuilder üzerinden nokta etrafında
    /// gereksiz boşluk üretmeden raw expression oluşturmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCLGLAU . BRM_KOD tokenları.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1RawExpression Text değeri DCLGLAU.BRM_KOD olmalıdır.
    /// </summary>
    [Fact]
    public void Create_WithQualifiedMemberTokens_ShouldReturnQualifiedExpressionText()
    {
        var tokens = new[]
        {
        CreateToken(Pl1TokenKind.Identifier, "DCLGLAU"),
        CreateToken(Pl1TokenKind.Dot, "."),
        CreateToken(Pl1TokenKind.Identifier, "BRM_KOD")
    };

        var expression = ExpressionFactory.Create(
            tokens,
            SourceLocation.Unknown);

        var rawExpression = Assert.IsType<Pl1RawExpression>(expression);

        Assert.Equal("DCLGLAU.BRM_KOD", rawExpression.Text);
    }

    /// <summary>
    /// CreateRaw methodunun doğrudan Pl1RawExpression ürettiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ExpressionFactory.CreateRaw fallback raw expression üretimini doğrudan sağlamalıdır.
    ///
    /// Hangi input'u test eder?
    /// PRICE + TAX tokenları.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Pl1RawExpression Text değeri PRICE + TAX olmalıdır.
    /// </summary>
    [Fact]
    public void CreateRaw_WithArithmeticTokens_ShouldReturnRawExpression()
    {
        var tokens = new[]
        {
        CreateToken(Pl1TokenKind.Identifier, "PRICE"),
        CreateToken(Pl1TokenKind.Plus, "+"),
        CreateToken(Pl1TokenKind.Identifier, "TAX")
    };

        var expression = ExpressionFactory.CreateRaw(
            tokens,
            SourceLocation.Unknown);

        var rawExpression = Assert.IsType<Pl1RawExpression>(expression);

        Assert.Equal("PRICE + TAX", rawExpression.Text);
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