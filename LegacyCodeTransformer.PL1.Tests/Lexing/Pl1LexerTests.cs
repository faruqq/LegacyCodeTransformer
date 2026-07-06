using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Tests.Lexing;

/// <summary>
/// Pl1Lexer için token üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Parser'ın doğru çalışabilmesi için önce Lexer'ın kaynak kodu doğru token'lara
/// böldüğünü garanti altına almamız gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Declaration, type, initialization, statement keyword ve statement operator
/// tokenlarının doğru üretildiğini doğrular.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL MUST_NO FIXED DECIMAL(8);
/// - DCL PARAM CHAR(08) INIT(' ');
/// - PARAM = 'ABC';
/// - CALL FETCH_CURSOR;
/// - IF SQLCODE ^= 100 THEN DO;
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I lexer regression testlerinde
/// - Parser geliştirmeleri öncesinde token sözleşmesini sabitlemek için
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Statement parser, expression parser, procedure parser ve embedded SQL parser
/// geliştirmelerinde lexer davranışının bozulmadığını garanti eder.
/// </summary>
public sealed class Pl1LexerTests
{
    /// <summary>
    /// FIXED DECIMAL declaration için beklenen token sırasını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DCL, identifier, FIXED, DECIMAL, parantez, sayı ve noktalı virgül tokenlarının
    /// doğru sırada üretildiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Token listesi DclKeyword ile başlayıp EndOfFile ile bitmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithFixedDecimalDeclaration_ShouldReturnExpectedTokens()
    {
        var source = "DCL MUST_NO FIXED DECIMAL(8);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DclKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("MUST_NO", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.FixedKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.DecimalKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("8", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// Scale içeren FIXED DECIMAL declaration için comma token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DECIMAL(10,2) içindeki virgülün Comma token olarak üretildiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL CUSTOMER_NO FIXED DECIMAL(10,2);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Token listesinde Comma, Number(10) ve Number(2) bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithFixedDecimalScaleDeclaration_ShouldReturnCommaToken()
    {
        var source = "DCL CUSTOMER_NO FIXED DECIMAL(10,2);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Comma);
        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Number && x.Text == "10");
        Assert.Contains(tokens, x => x.Kind == Pl1TokenKind.Number && x.Text == "2");
    }

    /// <summary>
    /// PL/I CHAR veri tipi keyword'ünün doğru token'a dönüştürüldüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CHAR keyword'ü identifier olarak değil CharKeyword olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Token listesinde CharKeyword bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithCharDeclaration_ShouldReturnCharKeywordToken()
    {
        var source = "DCL PARAM CHAR(08);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DclKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("PARAM", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CharKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("08", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// PL/I CHARACTER veri tipi keyword'ünün doğru token'a dönüştürüldüğünü doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CHARACTER keyword'ü identifier olarak değil CharacterKeyword olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DECLARE PARAM CHARACTER(08);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Token listesinde DclKeyword ve CharacterKeyword bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithCharacterDeclaration_ShouldReturnCharacterKeywordToken()
    {
        var source = "DECLARE PARAM CHARACTER(08);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DclKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("PARAM", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CharacterKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("08", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// Assignment statement için equals token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Lexer, assignment operatörü olan = karakterini Equals token olarak üretmelidir.
    ///
    /// Hangi input'u test eder?
    /// PARAM = 'ABC';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Identifier, Equals, StringLiteral, Semicolon, EndOfFile sırası üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithAssignmentStatement_ShouldReturnEqualsToken()
    {
        var source = "PARAM = 'ABC';";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("PARAM", token.Text);
            },
            token =>
            {
                Assert.Equal(Pl1TokenKind.Equals, token.Kind);
                Assert.Equal("=", token.Text);
            },
            token =>
            {
                Assert.Equal(Pl1TokenKind.StringLiteral, token.Kind);
                Assert.Equal("ABC", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// CALL statement keyword token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL keyword'ü identifier olarak değil CallKeyword olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL FETCH_CURSOR;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// CallKeyword, Identifier, Semicolon, EndOfFile sırası üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithCallStatement_ShouldReturnCallKeywordToken()
    {
        var source = "CALL FETCH_CURSOR;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.CallKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("FETCH_CURSOR", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// Parametreli CALL statement için parantez ve comma token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// CALL argument listesi içindeki parantez, virgül, identifier ve string literal
    /// tokenları doğru üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// CALL PROC1(A, 'ABC', B);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// CallKeyword, procedure identifier, argument punctuation ve argument tokenları
    /// doğru sırada üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithCallStatementArguments_ShouldReturnArgumentTokens()
    {
        var source = "CALL PROC1(A, 'ABC', B);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.CallKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("PROC1", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("A", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Comma, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.StringLiteral, token.Kind);
                Assert.Equal("ABC", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Comma, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("B", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// IF THEN DO statement keyword token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// IF, THEN ve DO keyword'leri identifier olarak değil kendi keyword tokenlarıyla
    /// üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF SQLCODE = 0 THEN DO;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// IfKeyword, Equals, ThenKeyword ve DoKeyword tokenları üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithIfThenDoStatement_ShouldReturnStatementKeywordTokens()
    {
        var source = "IF SQLCODE = 0 THEN DO;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.IfKeyword, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("SQLCODE", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Equals, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("0", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.ThenKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.DoKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// ELSE ve END keyword token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ELSE ve END keyword'leri identifier olarak değil ElseKeyword ve EndKeyword
    /// olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// ELSE DO; END;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// ElseKeyword, DoKeyword, EndKeyword tokenları üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithElseDoEndStatement_ShouldReturnElseAndEndKeywordTokens()
    {
        var source = "ELSE DO; END;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.ElseKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.DoKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// DO WHILE statement keyword token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// DO ve WHILE keyword'leri statement parser için ayrı token olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO WHILE(SQLCODE = 0);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DoKeyword, WhileKeyword, condition tokenları ve Semicolon üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithDoWhileStatement_ShouldReturnDoWhileKeywordTokens()
    {
        var source = "DO WHILE(SQLCODE = 0);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DoKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.WhileKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("SQLCODE", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Equals, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("0", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// DO UNTIL statement keyword token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// UNTIL keyword'ü identifier olarak değil UntilKeyword olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DO UNTIL(EOF);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DoKeyword ve UntilKeyword tokenları üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithDoUntilStatement_ShouldReturnDoUntilKeywordTokens()
    {
        var source = "DO UNTIL(EOF);";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token => Assert.Equal(Pl1TokenKind.DoKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.UntilKeyword, token.Kind),
            token => Assert.Equal(Pl1TokenKind.OpenParenthesis, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("EOF", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.CloseParenthesis, token.Kind),
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }

    /// <summary>
    /// Comparison operator tokenlarının doğru üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PL/I karşılaştırma operatörleri statement/expression parser için ayrı token
    /// olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// A <= B; A >= C; A ^= D; A ^< E; A ^> F;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// LessThanOrEqual, GreaterThanOrEqual, NotEqual, NotLessThan ve
    /// NotGreaterThan tokenları bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithComparisonOperators_ShouldReturnComparisonOperatorTokens()
    {
        var source = "A <= B; A >= C; A ^= D; A ^< E; A ^> F;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.LessThanOrEqual && token.Text == "<=");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.GreaterThanOrEqual && token.Text == ">=");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.NotEqual && token.Text == "^=");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.NotLessThan && token.Text == "^<");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.NotGreaterThan && token.Text == "^>");
    }

    /// <summary>
    /// Arithmetic operator tokenlarının doğru üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// +, -, *, / operatorları expression parser için ayrı token olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// A = B + C - D * E / F;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Plus, Minus, Asterisk ve Slash tokenları bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithArithmeticOperators_ShouldReturnArithmeticOperatorTokens()
    {
        var source = "A = B + C - D * E / F;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Plus && token.Text == "+");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Minus && token.Text == "-");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Asterisk && token.Text == "*");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Slash && token.Text == "/");
    }

    /// <summary>
    /// Logical operator tokenlarının doğru üretildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PL/I logical operator karakterleri statement condition parsing için ayrı token
    /// olarak üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// IF A = 1 & B = 2 ! ^EOF THEN DO;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Ampersand, Exclamation ve Caret tokenları bulunmalıdır.
    /// </summary>
    [Fact]
    public void Tokenize_WithLogicalOperators_ShouldReturnLogicalOperatorTokens()
    {
        var source = "IF A = 1 & B = 2 ! ^EOF THEN DO;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Ampersand && token.Text == "&");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Exclamation && token.Text == "!");
        Assert.Contains(tokens, token => token.Kind == Pl1TokenKind.Caret && token.Text == "^");
    }

    /// <summary>
    /// Qualified member access için Dot token üretimini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// PL/I structure member erişiminde kullanılan nokta karakteri Dot token olarak
    /// üretilmelidir.
    ///
    /// Hangi input'u test eder?
    /// DCLGLAU.BRM_KOD = 888;
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Identifier, Dot, Identifier, Equals, Number, Semicolon sırası üretilmelidir.
    /// </summary>
    [Fact]
    public void Tokenize_WithQualifiedMemberAssignment_ShouldReturnDotToken()
    {
        var source = "DCLGLAU.BRM_KOD = 888;";
        var lexer = new Pl1Lexer(source);

        var tokens = lexer.Tokenize();

        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("DCLGLAU", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Dot, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Identifier, token.Kind);
                Assert.Equal("BRM_KOD", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Equals, token.Kind),
            token =>
            {
                Assert.Equal(Pl1TokenKind.Number, token.Kind);
                Assert.Equal("888", token.Text);
            },
            token => Assert.Equal(Pl1TokenKind.Semicolon, token.Kind),
            token => Assert.Equal(Pl1TokenKind.EndOfFile, token.Kind));
    }
}