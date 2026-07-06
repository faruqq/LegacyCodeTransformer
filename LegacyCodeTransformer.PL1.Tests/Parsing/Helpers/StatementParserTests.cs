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
    /// CALL statement başlangıcı için Pl1CallStatement üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL keyword statement başlangıcı CallStatementParser'a yönlenmeli ve
    /// Pl1CallStatement üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value Pl1CallStatement olmalı, ProcedureName FETCH_CURSOR olmalı, Arguments boş
    /// olmalı, position EOF öncesi statement sonrasına ilerlemeli ve diagnostic listesi boş kalmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithCallStart_ShouldReturnCallStatement()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var callStatement = Assert.IsType<Pl1CallStatement>(result.Value);

        Assert.Equal("FETCH_CURSOR", callStatement.ProcedureName);
        Assert.Empty(callStatement.Arguments);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// IF statement başlangıcı için Pl1IfStatement üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF keyword statement başlangıcı IfStatementParser'a yönlenmeli ve Pl1IfStatement
    /// üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Condition SQLCODE = 0, ThenStatement Pl1CallStatement ve diagnostic listesi boş olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithIfStart_ShouldReturnIfStatement()
    {
        var tokens = Tokenize("IF SQLCODE = 0 THEN CALL FETCH_CURSOR;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var ifStatement = Assert.IsType<Pl1IfStatement>(result.Value);
        var condition = Assert.IsType<Pl1RawExpression>(ifStatement.Condition);
        var thenStatement = Assert.IsType<Pl1CallStatement>(ifStatement.ThenStatement);

        Assert.Equal("SQLCODE = 0", condition.Text);
        Assert.Equal("FETCH_CURSOR", thenStatement.ProcedureName);
        Assert.Null(ifStatement.ElseStatement);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DO WHILE statement başlangıcı için Pl1DoStatement üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO keyword statement başlangıcı DoStatementParser'a yönlenmeli ve DO WHILE block
    /// Pl1DoStatement olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Kind While, condition SQLCODE = 0, body içinde FETCH_CURSOR CALL statement olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithDoWhileStatement_ShouldReturnDoStatement()
    {
        var tokens = Tokenize("DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var doStatement = Assert.IsType<Pl1DoStatement>(result.Value);
        var condition = Assert.IsType<Pl1RawExpression>(doStatement.Condition);
        var bodyStatement = Assert.Single(doStatement.Body.Statements);
        var callStatement = Assert.IsType<Pl1CallStatement>(bodyStatement);

        Assert.Equal(Pl1DoStatementKind.While, doStatement.Kind);
        Assert.Equal("SQLCODE = 0", condition.Text);
        Assert.Equal("FETCH_CURSOR", callStatement.ProcedureName);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Empty(diagnostics.Diagnostics);
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

    /// <summary>
    /// Parametreli CALL statement için argument expression modellerinin üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CallStatementParser parantez içindeki argument listesini virgül bazlı ayırmalı ve
    /// her argument için Pl1RawExpression üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL PROC1(A, 'ABC', B);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ProcedureName PROC1, Arguments A, 'ABC', B olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithCallArguments_ShouldReturnCallStatementWithArguments()
    {
        var tokens = Tokenize("CALL PROC1(A, 'ABC', B);");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var callStatement = Assert.IsType<Pl1CallStatement>(result.Value);

        Assert.Equal("PROC1", callStatement.ProcedureName);
        Assert.Equal(3, callStatement.Arguments.Count);

        Assert.Equal("A", Assert.IsType<Pl1RawExpression>(callStatement.Arguments[0]).Text);
        Assert.Equal("'ABC'", Assert.IsType<Pl1RawExpression>(callStatement.Arguments[1]).Text);
        Assert.Equal("B", Assert.IsType<Pl1RawExpression>(callStatement.Arguments[2]).Text);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// CALL procedure adı eksik olduğunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL keyword sonrasında identifier gelmiyorsa parser model üretmemeli ve procedure adı
    /// bekleniyordu diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL ;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde CALL procedure adı bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingCallProcedureName_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("CALL ;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Equal(tokens.Count - 1, result.Position);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("CALL procedure adı bekleniyordu"));
    }

    /// <summary>
    /// CALL statement semicolon eksik olduğunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL statement sonunda semicolon yoksa parser model üretmemeli ve semicolon bekleniyordu
    /// diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde ';' bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingSemicolonOnCall_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("CALL FETCH_CURSOR");
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
    /// CALL argument içinde qualified member expression taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CallStatementParser, argument expression üretimini ExpressionFactory üzerinden
    /// yapmalı ve qualified member access formatını korumalıdır.
    ///
    /// Hangi input'u test eder?
    /// CALL PROC1(DCLGLAU.BRM_KOD);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Argument Text değeri DCLGLAU.BRM_KOD olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithCallQualifiedMemberArgument_ShouldPreserveArgumentExpression()
    {
        var tokens = Tokenize("CALL PROC1(DCLGLAU.BRM_KOD);");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var callStatement = Assert.IsType<Pl1CallStatement>(result.Value);

        var argument = Assert.Single(callStatement.Arguments);
        var rawArgument = Assert.IsType<Pl1RawExpression>(argument);

        Assert.Equal("PROC1", callStatement.ProcedureName);
        Assert.Equal("DCLGLAU.BRM_KOD", rawArgument.Text);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// IF THEN assignment statement parse davranışını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF parser THEN sonrasındaki assignment statement'ı StatementParser üzerinden
    /// parse edebilmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF A = B THEN PARAM = 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Condition A = B, ThenStatement Pl1AssignmentStatement olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithIfThenAssignment_ShouldReturnIfStatement()
    {
        var tokens = Tokenize("IF A = B THEN PARAM = 'ABC';");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var ifStatement = Assert.IsType<Pl1IfStatement>(result.Value);
        var condition = Assert.IsType<Pl1RawExpression>(ifStatement.Condition);
        var thenAssignment = Assert.IsType<Pl1AssignmentStatement>(ifStatement.ThenStatement);

        var target = Assert.Single(thenAssignment.Targets);
        var targetExpression = Assert.IsType<Pl1RawExpression>(target);
        var valueExpression = Assert.IsType<Pl1RawExpression>(thenAssignment.Value);

        Assert.Equal("A = B", condition.Text);
        Assert.Equal("PARAM", targetExpression.Text);
        Assert.Equal("'ABC'", valueExpression.Text);
        Assert.Null(ifStatement.ElseStatement);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// IF THEN ELSE statement parse davranışını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF parser optional ELSE kolunu parse etmeli ve ElseStatement alanını doldurmalıdır.
    ///
    /// Hangi input'u test eder?
    /// IF A = B THEN CALL PROC1; ELSE CALL PROC2;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ThenStatement PROC1, ElseStatement PROC2 çağrısı olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithIfThenElseCallStatements_ShouldReturnIfStatementWithElse()
    {
        var tokens = Tokenize("IF A = B THEN CALL PROC1; ELSE CALL PROC2;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var ifStatement = Assert.IsType<Pl1IfStatement>(result.Value);
        var condition = Assert.IsType<Pl1RawExpression>(ifStatement.Condition);
        var thenStatement = Assert.IsType<Pl1CallStatement>(ifStatement.ThenStatement);
        var elseStatement = Assert.IsType<Pl1CallStatement>(ifStatement.ElseStatement);

        Assert.Equal("A = B", condition.Text);
        Assert.Equal("PROC1", thenStatement.ProcedureName);
        Assert.Equal("PROC2", elseStatement.ProcedureName);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// THEN keyword eksik olduğunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF parser condition sonrasında THEN keyword bulamazsa model üretmemeli ve
    /// THEN bekleniyordu diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF A = B CALL PROC1;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde THEN bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingThenKeyword_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("IF A = B CALL PROC1;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("THEN bekleniyordu"));
    }

    /// <summary>
    /// IF condition eksik olduğunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF parser THEN keyword öncesinde condition tokenı bulamazsa model üretmemelidir.
    ///
    /// Hangi input'u test eder?
    /// IF THEN CALL PROC1;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde IF condition bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingIfCondition_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("IF THEN CALL PROC1;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("IF condition bekleniyordu"));
    }

    /// <summary>
    /// Block DO statement parse davranışını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO; ... END; yapısı koşulsuz block statement olarak parse edilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO; PARAM = 'ABC'; CALL PROC1; END;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Kind Block, condition null, body içinde assignment ve CALL statement bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithBlockDoStatement_ShouldReturnDoStatement()
    {
        var tokens = Tokenize("DO; PARAM = 'ABC'; CALL PROC1; END;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var doStatement = Assert.IsType<Pl1DoStatement>(result.Value);

        Assert.Equal(Pl1DoStatementKind.Block, doStatement.Kind);
        Assert.Null(doStatement.Condition);
        Assert.Equal(2, doStatement.Body.Statements.Count);
        Assert.IsType<Pl1AssignmentStatement>(doStatement.Body.Statements[0]);
        Assert.IsType<Pl1CallStatement>(doStatement.Body.Statements[1]);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DO UNTIL statement parse davranışını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO UNTIL condition bilgisini Pl1RawExpression olarak taşımalı ve body statement
    /// listesini parse etmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO UNTIL(EOF); PARAM = 'ABC'; END;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Kind Until, condition EOF ve body içinde assignment statement olmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithDoUntilStatement_ShouldReturnDoStatement()
    {
        var tokens = Tokenize("DO UNTIL(EOF); PARAM = 'ABC'; END;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var doStatement = Assert.IsType<Pl1DoStatement>(result.Value);
        var condition = Assert.IsType<Pl1RawExpression>(doStatement.Condition);
        var bodyStatement = Assert.Single(doStatement.Body.Statements);

        Assert.Equal(Pl1DoStatementKind.Until, doStatement.Kind);
        Assert.Equal("EOF", condition.Text);
        Assert.IsType<Pl1AssignmentStatement>(bodyStatement);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// IF THEN DO block parse davranışını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF parser THEN sonrasında gelen DO block statement'ı child statement olarak
    /// parse edebilmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF A = B THEN DO; CALL PROC1; END;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ThenStatement Pl1DoStatement olmalı ve body içinde PROC1 CALL statement bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithIfThenDoBlock_ShouldReturnIfStatementWithDoThenStatement()
    {
        var tokens = Tokenize("IF A = B THEN DO; CALL PROC1; END;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        var ifStatement = Assert.IsType<Pl1IfStatement>(result.Value);
        var thenDoStatement = Assert.IsType<Pl1DoStatement>(ifStatement.ThenStatement);
        var bodyStatement = Assert.Single(thenDoStatement.Body.Statements);
        var callStatement = Assert.IsType<Pl1CallStatement>(bodyStatement);

        Assert.Equal(Pl1DoStatementKind.Block, thenDoStatement.Kind);
        Assert.Equal("PROC1", callStatement.ProcedureName);
        Assert.Empty(diagnostics.Diagnostics);
    }

    /// <summary>
    /// DO statement END keyword eksik olduğunda diagnostic üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO block parser END keyword bulamazsa model üretmemeli ve END bekleniyordu
    /// diagnostic'i üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO; CALL PROC1;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Value null olmalı ve diagnostic içinde END bekleniyordu mesajı bulunmalıdır.
    /// </summary>
    [Fact]
    public void ParseStatement_WithMissingEndKeywordOnDo_ShouldReturnDiagnostic()
    {
        var tokens = Tokenize("DO; CALL PROC1;");
        var diagnostics = new DiagnosticBag();
        var parser = new StatementParser(tokens, 0, diagnostics);

        var result = parser.ParseStatement();

        Assert.Null(result.Value);
        Assert.Contains(
            diagnostics.Diagnostics,
            diagnostic => diagnostic.Message.Contains("END bekleniyordu"));
    }

    private static IReadOnlyList<Pl1Token> Tokenize(string source)
    {
        return new Pl1Lexer(source).Tokenize();
    }
}