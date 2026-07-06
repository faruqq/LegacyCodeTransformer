using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

/// <summary>
/// StatementParser foundation ve assignment routing davranışlarını test eder.
///
/// Neden var?
/// ----------------------
/// P05 statement parser altyapısında StatementParser concrete parser seçimini
/// yapmalı, desteklenen parser'a yönlenmeli ve henüz desteklenmeyen statement
/// türlerinde güvenli diagnostic / recovery davranışı sağlamalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// StatementParser'ın statement başlangıcı olmayan tokenlarda pozisyonu koruduğunu,
/// assignment statement için Pl1AssignmentStatement ürettiğini, CALL / IF / DO gibi
/// henüz concrete parser'ı olmayan statement türlerinde diagnostic üretip semicolon'a
/// kadar güvenli ilerlediğini doğrular.
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
/// P05.2 ve P05.3 parser foundation regression testlerinde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05.3 içinde CallStatementParser, P05.4 içinde IfStatementParser ve
/// DoStatementParser eklendiğinde bu class yeni başarılı model üretim testleriyle
/// genişletilecektir.
/// </summary>
public sealed class StatementParserTests
{
    /// <summary>
    /// Statement başlangıcı olmayan token için parser'ın pozisyonu değiştirmediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StatementParser, DCL tokenını statement başlangıcı kabul etmemeli ve token
    /// tüketmemelidir.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null, Position 0 ve diagnostic listesi boş olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithNonStatementStartToken_ShouldReturnNullAndKeepPosition()
    {
        var tokens = Tokenize("DCL PARAM CHAR(08);");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(0, result.Position);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Assignment statement başlangıcı için Pl1AssignmentStatement üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Identifier statement başlangıcı AssignmentStatementParser'a yönlenmeli ve
    /// Pl1AssignmentStatement üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM = 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value Pl1AssignmentStatement olmalı, target PARAM, value 'ABC' olmalı,
    /// position EOF öncesi statement sonrasına ilerlemeli ve diagnostic listesi boş kalmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithAssignmentStart_ShouldReturnAssignmentStatement()
    {
        var tokens = Tokenize("PARAM = 'ABC';");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(result.Value);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("PARAM", targetExpression.Text);
        Assert.Equal("'ABC'", valueExpression.Text);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Numeric value içeren assignment statement için raw expression üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// AssignmentStatementParser sayı tokenlarını value expression olarak taşımalıdır.
    ///
    /// Hangi input'u test eder?
    /// SQLCODE = 0;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Target SQLCODE, value 0 olan Pl1AssignmentStatement üretilmelidir.
    /// </summary>
    [Fact]
    public void ParseStatement_WithNumericAssignmentValue_ShouldReturnAssignmentStatement()
    {
        var tokens = Tokenize("SQLCODE = 0;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(result.Value);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("SQLCODE", targetExpression.Text);
        Assert.Equal("0", valueExpression.Text);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// Qualified member assignment için raw target expression üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// AssignmentStatementParser, structure member erişiminde kullanılan nokta tokenını
    /// target expression içinde doğru formatla korumalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCLGLAU.BRM_KOD = 888;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Target DCLGLAU.BRM_KOD, value 888 olan Pl1AssignmentStatement üretilmelidir.
    /// </summary>
    [Fact]
    public void ParseStatement_WithQualifiedMemberAssignment_ShouldReturnAssignmentStatement()
    {
        var tokens = Tokenize("DCLGLAU.BRM_KOD = 888;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var assignmentStatement = Assert.IsType<Pl1AssignmentStatement>(result.Value);

        var target = Assert.Single(assignmentStatement.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(assignmentStatement.Value);

        Assert.Equal("DCLGLAU.BRM_KOD", targetExpression.Text);
        Assert.Equal("888", valueExpression.Text);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// CALL statement başlangıcı için diagnostic üretildiğini ve statement'ın atlandığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL keyword statement başlangıcı olarak tanınır fakat concrete CallStatementParser
    /// henüz eklenmediği için diagnostic üretilir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null, Position EOF öncesi statement sonrasına ilerlemiş ve diagnostic
    /// içinde CALL parser henüz eklenmedi mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithCallStart_ShouldAddDiagnosticAndSkipStatement()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("CALL parser henüz eklenmedi"));
    }

    /// <summary>
    /// IF statement başlangıcı için diagnostic üretildiğini ve statement'ın atlandığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF keyword statement başlangıcı olarak tanınır fakat concrete IfStatementParser
    /// henüz eklenmediği için diagnostic üretilir.
    ///
    /// Hangi input'u test eder?
    /// IF SQLCODE = 0 THEN DO;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null, Position semicolon sonrasına ilerlemiş ve diagnostic içinde
    /// IF parser henüz eklenmedi mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithIfStart_ShouldAddDiagnosticAndSkipStatement()
    {
        var tokens = Tokenize("IF SQLCODE = 0 THEN DO;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("IF parser henüz eklenmedi"));
    }

    /// <summary>
    /// DO statement başlangıcı için diagnostic üretildiğini ve statement'ın atlandığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO keyword statement başlangıcı olarak tanınır fakat concrete DoStatementParser
    /// henüz eklenmediği için diagnostic üretilir.
    ///
    /// Hangi input'u test eder?
    /// DO WHILE(SQLCODE = 0);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null, Position EOF öncesi statement sonrasına ilerlemiş ve diagnostic
    /// içinde DO parser henüz eklenmedi mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithDoStart_ShouldAddDiagnosticAndSkipStatement()
    {
        var tokens = Tokenize("DO WHILE(SQLCODE = 0);");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("DO parser henüz eklenmedi"));
    }

    /// <summary>
    /// Semicolon olmayan unsupported statement için parser'ın EOF'a kadar güvenli ilerlediğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// StatementParser, henüz concrete parser'ı olmayan ve semicolon içermeyen CALL
    /// statement'ta sonsuz döngüye girmeden EOF tokenına kadar ilerlemelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null, Position EOF token pozisyonunda olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingSemicolonOnUnsupportedStatement_ShouldSkipUntilEndOfFile()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("CALL parser henüz eklenmedi"));
    }

    /// <summary>
    /// Semicolon olmayan assignment statement için diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// AssignmentStatementParser, semicolon eksik olduğunda model üretmemeli ve
    /// semicolon bekleniyordu diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM = 'ABC'
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde ';' bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingSemicolonOnAssignment_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("PARAM = 'ABC'");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("';' bekleniyordu"));
    }

    /// <summary>
    /// Equals olmayan assignment başlangıcı için diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Identifier ile başlayan fakat '=' içermeyen statement assignment olarak parse edilemez.
    /// Parser bu durumda '=' bekleniyordu diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde '=' bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingEqualsOnAssignment_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("PARAM 'ABC';");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("'=' bekleniyordu"));
    }

    private static IReadOnlyList<Pl1Token> Tokenize(string source)
    {
        return new Pl1Lexer(source).Tokenize();
    }
}