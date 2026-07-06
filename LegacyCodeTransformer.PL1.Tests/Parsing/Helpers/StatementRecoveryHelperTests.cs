using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// StatementRecoveryHelper için statement recovery testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Desteklenmeyen veya hatalı statement görüldüğünde parser'ın güvenli şekilde
/// statement sonrasına ilerlemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Recovery helper'ın semicolon'a kadar ilerlediğini, semicolon varsa onu da
/// tükettiğini ve semicolon yoksa EOF üzerinde durduğunu doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - CALL FETCH_CURSOR;
/// - PARAM 'ABC';
/// - CALL FETCH_CURSOR
///
/// Nerede kullanılır?
/// ----------------------
/// StatementParser unsupported statement recovery ve AssignmentStatementParser
/// hatalı assignment recovery davranışında kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Nested block recovery ve unsupported syntax recovery davranışları bu helper
/// üzerinden genişletilecektir.
/// </summary>
public sealed class StatementRecoveryHelperTests
{
    /// <summary>
    /// Semicolon içeren statement'ta statement sonrasına ilerlediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Recovery helper statement içeriğini ve semicolon tokenını tüketmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Context position EOF tokenında olmalıdır.
    /// </summary>
    [Fact]
    public void SkipCurrentStatement_WithSemicolon_ShouldMovePositionAfterSemicolon()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR;");
        var context = new ParseContext(tokens, 0, new DiagnosticBag());
        var recoveryHelper = new StatementRecoveryHelper(context);

        recoveryHelper.SkipCurrentStatement();

        Assert.Equal(tokens.Count - 1, context.Position);
        Assert.Equal(Pl1TokenKind.EndOfFile, tokens[context.Position].Kind);
    }

    /// <summary>
    /// Semicolon bulunmayan statement'ta EOF'a kadar ilerlediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Recovery helper semicolon yoksa EOF'a kadar güvenli ilerlemelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Context position EOF tokenında olmalıdır.
    /// </summary>
    [Fact]
    public void SkipCurrentStatement_WithoutSemicolon_ShouldMovePositionToEndOfFile()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR");
        var context = new ParseContext(tokens, 0, new DiagnosticBag());
        var recoveryHelper = new StatementRecoveryHelper(context);

        recoveryHelper.SkipCurrentStatement();

        Assert.Equal(tokens.Count - 1, context.Position);
        Assert.Equal(Pl1TokenKind.EndOfFile, tokens[context.Position].Kind);
    }

    /// <summary>
    /// Recovery'nin yalnızca mevcut statement'ı tükettiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Recovery helper ilk semicolon sonrasında durmalı, sonraki statement'ı tüketmemelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL PROC1; CALL PROC2;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Context position ikinci CALL tokenında kalmalıdır.
    /// </summary>
    [Fact]
    public void SkipCurrentStatement_WithMultipleStatements_ShouldStopAfterFirstSemicolon()
    {
        var tokens = Tokenize("CALL PROC1; CALL PROC2;");
        var context = new ParseContext(tokens, 0, new DiagnosticBag());
        var recoveryHelper = new StatementRecoveryHelper(context);

        recoveryHelper.SkipCurrentStatement();

        Assert.Equal(3, context.Position);
        Assert.Equal(Pl1TokenKind.CallKeyword, tokens[context.Position].Kind);
        Assert.Equal("CALL", tokens[context.Position].Text);
    }

    private static IReadOnlyList<Pl1Token> Tokenize(string source)
    {
        return new Pl1Lexer(source).Tokenize();
    }
}