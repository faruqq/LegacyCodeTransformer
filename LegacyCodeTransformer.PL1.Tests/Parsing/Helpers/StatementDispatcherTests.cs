using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// StatementDispatcher için statement başlangıç ve parser kind mapping testlerini içerir.
///
/// Neden var?
/// ----------------------
/// P05 statement parser altyapısında hangi token'ın hangi statement ailesine
/// yönleneceği merkezi dispatcher üzerinden belirlenir.
///
/// Ne çözüyor?
/// ----------------------
/// Assignment, CALL, IF ve DO başlangıçlarının doğru tanındığını, diagnostic/test
/// tarafında kullanılacak family name değerlerinin sabitlendiğini ve concrete parser
/// seçimi için StatementParserKind mapping davranışının doğru olduğunu doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - PARAM = 'ABC';
/// - CALL FETCH_CURSOR;
/// - IF SQLCODE = 0 THEN DO;
/// - DO WHILE(SQLCODE = 0);
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser orchestration testlerinde ve ileride concrete parser dispatch
/// davranışını sabitlemek için kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// SELECT, READ, WRITE, RETURN, STOP, LEAVE, EXEC SQL gibi yeni statement türleri
/// eklendikçe dispatcher testleri genişletilecektir.
/// </summary>
public sealed class StatementDispatcherTests
{
    /// <summary>
    /// Statement başlangıcı olan token türlerinin dispatcher tarafından tanındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Identifier, CALL, IF ve DO tokenları statement başlangıcı kabul edilmelidir.
    ///
    /// Hangi input'u test eder?
    /// Token kind seviyesinde Identifier, CallKeyword, IfKeyword ve DoKeyword.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// IsStatementStart true dönmelidir.
    /// </summary>
    [Theory]
    [InlineData(Pl1TokenKind.Identifier)]
    [InlineData(Pl1TokenKind.CallKeyword)]
    [InlineData(Pl1TokenKind.IfKeyword)]
    [InlineData(Pl1TokenKind.DoKeyword)]
    public void IsStatementStart_WithStatementStartToken_ShouldReturnTrue(Pl1TokenKind tokenKind)
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.IsStatementStart(tokenKind);

        Assert.True(result);
    }

    /// <summary>
    /// Statement başlangıcı olmayan token türlerinin dispatcher tarafından reddedildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DCL, semicolon ve EOF tokenları statement başlangıcı olarak kabul edilmemelidir.
    ///
    /// Hangi input'u test eder?
    /// Token kind seviyesinde DclKeyword, Semicolon ve EndOfFile.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// IsStatementStart false dönmelidir.
    /// </summary>
    [Theory]
    [InlineData(Pl1TokenKind.DclKeyword)]
    [InlineData(Pl1TokenKind.Semicolon)]
    [InlineData(Pl1TokenKind.EndOfFile)]
    public void IsStatementStart_WithNonStatementStartToken_ShouldReturnFalse(Pl1TokenKind tokenKind)
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.IsStatementStart(tokenKind);

        Assert.False(result);
    }

    /// <summary>
    /// Statement başlangıç tokenları için okunabilir statement family adlarının üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Dispatcher, statement başlangıç tokenlarını diagnostic ve testlerde kullanılacak
    /// okunabilir family name değerlerine map etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Identifier, CallKeyword, IfKeyword ve DoKeyword.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Assignment, CALL, IF ve DO değerleri dönmelidir.
    /// </summary>
    [Theory]
    [InlineData(Pl1TokenKind.Identifier, "Assignment")]
    [InlineData(Pl1TokenKind.CallKeyword, "CALL")]
    [InlineData(Pl1TokenKind.IfKeyword, "IF")]
    [InlineData(Pl1TokenKind.DoKeyword, "DO")]
    public void GetStatementFamilyName_WithKnownStatementStartToken_ShouldReturnFamilyName(
        Pl1TokenKind tokenKind,
        string expectedFamilyName)
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.GetStatementFamilyName(tokenKind);

        Assert.Equal(expectedFamilyName, result);
    }

    /// <summary>
    /// Bilinmeyen statement başlangıcı için Unknown family name döndüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Dispatcher statement başlangıcı olmayan tokenlarda güvenli fallback değer üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// DclKeyword.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Unknown değeri dönmelidir.
    /// </summary>
    [Fact]
    public void GetStatementFamilyName_WithUnknownToken_ShouldReturnUnknown()
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.GetStatementFamilyName(Pl1TokenKind.DclKeyword);

        Assert.Equal("Unknown", result);
    }

    /// <summary>
    /// Statement başlangıç tokenları için doğru parser kind değerlerinin üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StatementDispatcher token türünden concrete parser ailesini temsil eden
    /// StatementParserKind değerini doğru üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// Identifier, CallKeyword, IfKeyword ve DoKeyword.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Identifier için Assignment, CALL için Call, IF için If ve DO için Do dönmelidir.
    /// </summary>
    [Theory]
    [InlineData(Pl1TokenKind.Identifier, StatementParserKind.Assignment)]
    [InlineData(Pl1TokenKind.CallKeyword, StatementParserKind.Call)]
    [InlineData(Pl1TokenKind.IfKeyword, StatementParserKind.If)]
    [InlineData(Pl1TokenKind.DoKeyword, StatementParserKind.Do)]
    public void GetParserKind_WithStatementStartToken_ShouldReturnExpectedParserKind(
        Pl1TokenKind tokenKind,
        StatementParserKind expectedParserKind)
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.GetParserKind(tokenKind);

        Assert.Equal(expectedParserKind, result);
    }

    /// <summary>
    /// Statement başlangıcı olmayan token için Unknown parser kind döndüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StatementDispatcher statement başlangıcı olmayan tokenlarda güvenli fallback olarak
    /// StatementParserKind.Unknown üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// DclKeyword.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Unknown dönmelidir.
    /// </summary>
    [Fact]
    public void GetParserKind_WithNonStatementStartToken_ShouldReturnUnknown()
    {
        var dispatcher = new StatementDispatcher();

        var result = dispatcher.GetParserKind(Pl1TokenKind.DclKeyword);

        Assert.Equal(StatementParserKind.Unknown, result);
    }
}