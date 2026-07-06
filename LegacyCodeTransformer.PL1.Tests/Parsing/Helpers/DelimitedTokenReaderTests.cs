using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// DelimitedTokenReader için delimiter bazlı token okuma testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Statement parser sınıflarında belirli delimiter token'a kadar token okuma
/// davranışı tekrar edecektir.
///
/// Ne çözüyor?
/// ----------------------
/// Delimiter bulunana kadar tokenların okunduğunu, delimiter token'ın tüketilmeden
/// yerinde bırakıldığını ve delimiter bulunamazsa EOF'a kadar güvenli ilerlediğini
/// doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - A = B;
/// - CALL PROC(A, B);
/// - IF SQLCODE = 0 THEN DO;
///
/// Nerede kullanılır?
/// ----------------------
/// AssignmentStatementParser içinde target ve value tokenlarını ayırmak için kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// CALL argument parsing, IF condition parsing ve DO condition parsing için ortak
/// delimiter okuma davranışı olarak kullanılacaktır.
/// </summary>
public sealed class DelimitedTokenReaderTests
{
    /// <summary>
    /// Delimiter token'a kadar tokenların okunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Reader, '=' delimiter'ına kadar olan tokenları döndürmeli fakat '=' tokenını
    /// tüketmemelidir.
    ///
    /// Hangi input'u test eder?
    /// A = B;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Okunan token listesinde yalnızca A olmalı, context position '=' tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void ReadUntil_WithDelimiter_ShouldReturnTokensBeforeDelimiterAndKeepDelimiter()
    {
        var tokens = Tokenize("A = B;");
        var context = new ParseContext(tokens, 0, new DiagnosticBag());
        var reader = new DelimitedTokenReader(context);

        var result = reader.ReadUntil(Pl1TokenKind.Equals);

        var token = Assert.Single(result);

        Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
        Assert.Equal("A", token.Text);
        Assert.Equal(1, context.Position);
        Assert.Equal(Pl1TokenKind.Equals, tokens[context.Position].Kind);
    }

    /// <summary>
    /// Value tarafı için semicolon'a kadar tokenların okunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Reader, semicolon delimiter'ına kadar expression tokenlarını döndürmeli fakat
    /// semicolon tokenını tüketmemelidir.
    ///
    /// Hangi input'u test eder?
    /// A = B + C;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// B + C tokenları okunmalı, context position Semicolon tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void ReadUntil_WithSemicolonDelimiter_ShouldReturnValueTokensAndKeepSemicolon()
    {
        var tokens = Tokenize("A = B + C;");
        var context = new ParseContext(tokens, 2, new DiagnosticBag());
        var reader = new DelimitedTokenReader(context);

        var result = reader.ReadUntil(Pl1TokenKind.Semicolon);

        Assert.Collection(
            result,
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("B", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Plus, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("C", token.Text);
            });

        Assert.Equal(5, context.Position);
        Assert.Equal(Pl1TokenKind.Semicolon, tokens[context.Position].Kind);
    }

    /// <summary>
    /// Delimiter bulunamadığında EOF'a kadar güvenli ilerlediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Reader, aranan delimiter yoksa sonsuz döngüye girmeden EOF tokenına kadar
    /// ilerlemelidir.
    ///
    /// Hangi input'u test eder?
    /// A = B
    ///
    /// Beklenen temel model/çıktı nedir?
    /// B tokenı okunmalı ve context position EOF tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void ReadUntil_WithoutDelimiter_ShouldReadUntilEndOfFile()
    {
        var tokens = Tokenize("A = B");
        var context = new ParseContext(tokens, 2, new DiagnosticBag());
        var reader = new DelimitedTokenReader(context);

        var result = reader.ReadUntil(Pl1TokenKind.Semicolon);

        var token = Assert.Single(result);

        Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
        Assert.Equal("B", token.Text);
        Assert.Equal(tokens.Count - 1, context.Position);
        Assert.Equal(Pl1TokenKind.EndOfFile, tokens[context.Position].Kind);
    }

    /// <summary>
    /// Birden fazla delimiter token'dan ilkine kadar okuma yapıldığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ReadUntilAny, verilen delimiter listesinden hangisi önce gelirse orada durmalı
    /// ve delimiter tokenını tüketmemelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL PROC1(A, B);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// A tokenı okunmalı ve context position Comma tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void ReadUntilAny_WithMultipleDelimiters_ShouldStopAtFirstMatchingDelimiter()
    {
        var tokens = Tokenize("CALL PROC1(A, B);");
        var context = new ParseContext(tokens, 3, new DiagnosticBag());
        var reader = new DelimitedTokenReader(context);

        var result = reader.ReadUntilAny(
            Pl1TokenKind.Comma,
            Pl1TokenKind.CloseParenthesis);

        var token = Assert.Single(result);

        Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
        Assert.Equal("A", token.Text);
        Assert.Equal(4, context.Position);
        Assert.Equal(Pl1TokenKind.Comma, tokens[context.Position].Kind);
    }

    /// <summary>
    /// ReadUntil methodunun ReadUntilAny davranışını tek delimiter ile kullandığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ReadUntil, tek delimiter için semantik olarak ReadUntilAny ile aynı davranmalıdır.
    ///
    /// Hangi input'u test eder?
    /// A = B;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// A tokenı okunmalı ve context position Equals tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void ReadUntil_WithSingleDelimiter_ShouldBehaveLikeReadUntilAny()
    {
        var tokens = Tokenize("A = B;");
        var context = new ParseContext(tokens, 0, new DiagnosticBag());
        var reader = new DelimitedTokenReader(context);

        var result = reader.ReadUntil(Pl1TokenKind.Equals);

        var token = Assert.Single(result);

        Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
        Assert.Equal("A", token.Text);
        Assert.Equal(1, context.Position);
        Assert.Equal(Pl1TokenKind.Equals, tokens[context.Position].Kind);
    }

    private static IReadOnlyList<Pl1Token> Tokenize(string source)
    {
        return new Pl1Lexer(source).Tokenize();
    }
}