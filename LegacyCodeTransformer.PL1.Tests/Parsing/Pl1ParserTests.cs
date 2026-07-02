using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing;

/// <summary>
/// Pl1Parser için syntax tree üretim testlerini içerir.
///
/// Neden var?
/// ----------------------
/// Lexer token üretimini doğruladık.
/// Şimdi bu token'ların doğru PL/I syntax modeline dönüştürüldüğünü
/// garanti altına almamız gerekir.
///
/// Bu testler özellikle DCL FIXED DECIMAL ifadelerinin
/// Pl1VariableDeclaration ve Pl1FixedDecimalType olarak parse edildiğini doğrular.
/// </summary>
public sealed class Pl1ParserTests
{
    [Fact]
    public void Parse_WithFixedDecimalDeclaration_ShouldCreateVariableDeclaration()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL MUST_NO FIXED DECIMAL(8);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("MUST_NO", declaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(declaration.DataType);

        Assert.Equal(8, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    [Fact]
    public void Parse_WithFixedDecimalScaleDeclaration_ShouldCreateVariableDeclarationWithScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL CUSTOMER_NO FIXED DECIMAL(10,2);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("CUSTOMER_NO", declaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(declaration.DataType);

        Assert.Equal(10, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    [Fact]
    public void Parse_WithMissingSemicolon_ShouldReturnDiagnosticError()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL MUST_NO FIXED DECIMAL(8)").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Diagnostics);
        Assert.Contains(result.Diagnostics, x => x.Message.Contains("';' bekleniyordu."));
    }

    /// <summary>
    /// PL/I CHAR(n) tanımının Pl1CharacterType olarak parse edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Lexer CHAR keyword'ünü ürettikten sonra parser'ın bu token'ı
    /// karakter veri tipi modeline dönüştürmesi gerekir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM CHAR(25);
    ///
    /// Beklenen model:
    /// - Pl1VariableDeclaration
    /// - Name: PARAM
    /// - DataType: Pl1CharacterType
    /// - Length: 25
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - CHAR declaration desteğini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// CHAR üzerine INIT, VARYING, structure field gibi özellikler eklendiğinde
    /// temel CHAR parse davranışının bozulmadığını garanti eder.
    /// </summary>
    [Fact]
    public void Parse_WithCharDeclaration_ShouldCreateCharacterTypeDeclaration()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM CHAR(25);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM", declaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(declaration.DataType);

        Assert.Equal(25, dataType.Length);
    }

    /// <summary>
    /// PL/I CHAR uzunluğunda baştaki sıfırların modelde normalize edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I kodlarında CHAR(08), CHAR(01), CHAR(02) gibi başında
    /// sıfır bulunan uzunluk değerleri kullanılabilir.
    /// Parser bu değeri model üzerinde sayısal uzunluk olarak tutmalıdır.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM CHAR(08);
    ///
    /// Beklenen model:
    /// - Length: 8
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - Numeric token normalizasyon davranışını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Array dimension, precision, scale ve tekrar sayısı gibi numeric
    /// alanlarda benzer normalizasyon yaklaşımına referans olur.
    /// </summary>
    [Fact]
    public void Parse_WithCharDeclarationHavingLeadingZeroLength_ShouldNormalizeLength()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM CHAR(08);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM", declaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(declaration.DataType);

        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I CHARACTER(n) tanımının Pl1CharacterType olarak parse edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I dilinde CHARACTER uzun form, CHAR ise kısa form olarak kullanılır.
    /// Parser iki kullanımı da aynı veri tipi modeliyle temsil etmelidir.
    ///
    /// Test edilen PL/I:
    ///
    /// DECLARE PARAM CHARACTER(08);
    ///
    /// Beklenen model:
    /// - Pl1CharacterType
    /// - Length: 8
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - DECLARE alias ve CHARACTER veri tipi desteğini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Kod tabanında farklı PL/I ekipleri CHAR yerine CHARACTER kullansa bile
    /// dönüşüm davranışının aynı kalmasını garanti eder.
    /// </summary>
    [Fact]
    public void Parse_WithCharacterDeclaration_ShouldCreateCharacterTypeDeclaration()
    {
        // Arrange
        var tokens = new Pl1Lexer("DECLARE PARAM CHARACTER(08);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM", declaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(declaration.DataType);

        Assert.Equal(8, dataType.Length);
    }

    /// <summary>
    /// PL/I INIT(' ') başlangıç değerinin declaration modeline taşındığını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I kaynaklarında CHAR alanlar çoğunlukla boşluk karakteriyle
    /// initialize edilir.
    /// Parser bu bilgiyi kaybetmeden Pl1VariableDeclaration.InitialValue
    /// üzerinde saklamalıdır.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Beklenen model:
    /// - InitialValue.Value: " "
    /// - InitialValue.RepeatCount: null
    /// - InitialValue.AppliesToAllElements: false
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - INIT başlangıç değeri desteğini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL default value mapping kararı alındığında bu bilginin Syntax Tree
    /// üzerinde hazır olduğunu garanti eder.
    /// </summary>
    [Fact]
    public void Parse_WithSimpleInitValue_ShouldSetInitialValue()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM CHAR(08) INIT(' ');").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM", declaration.Name);
        Assert.NotNull(declaration.InitialValue);
        Assert.Equal(" ", declaration.InitialValue!.Value);
        Assert.Null(declaration.InitialValue.RepeatCount);
        Assert.False(declaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT(';') başlangıç değerinin karakter sabiti olarak taşındığını doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynaklarında başlangıç değeri yalnızca boşluk olmayabilir.
    /// Noktalı virgül gibi özel karakterler de karakter sabiti olarak
    /// kullanılabilir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM2 CHAR(01) INIT(';');
    ///
    /// Beklenen model:
    /// - InitialValue.Value: ";"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - StringLiteral token içeriğinin doğru taşındığını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Özel karakterli default value dönüşümlerinde string literal bilgisinin
    /// korunmasını garanti eder.
    /// </summary>
    [Fact]
    public void Parse_WithSemicolonInitValue_ShouldSetInitialValue()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM2 CHAR(01) INIT(';');").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM2", declaration.Name);
        Assert.NotNull(declaration.InitialValue);
        Assert.Equal(";", declaration.InitialValue!.Value);
        Assert.Null(declaration.InitialValue.RepeatCount);
        Assert.False(declaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT((08)' ') tekrar faktörünün sayısal olarak parse edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I başlangıç değeri söz diziminde aynı değerin birden fazla kez
    /// tekrar ettirilmesi için tekrar faktörü kullanılabilir.
    /// Bu bilgi Syntax Tree üzerinde kaybedilmeden tutulmalıdır.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM3 CHAR(8) INIT((08)' ');
    ///
    /// Beklenen model:
    /// - InitialValue.Value: " "
    /// - InitialValue.RepeatCount: 8
    /// - InitialValue.AppliesToAllElements: false
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - INIT repeat factor desteğini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Array, structure ve fixed-length alan initialization dönüşümleri
    /// desteklendiğinde repeat count bilgisi kullanılacaktır.
    /// </summary>
    [Fact]
    public void Parse_WithRepeatedInitValue_ShouldSetRepeatCount()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM3 CHAR(8) INIT((08)' ');").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM3", declaration.Name);
        Assert.NotNull(declaration.InitialValue);
        Assert.Equal(" ", declaration.InitialValue!.Value);
        Assert.Equal(8, declaration.InitialValue.RepeatCount);
        Assert.False(declaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INIT((*)' ') tekrar faktörünün tüm elemanlar anlamında parse
    /// edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I array ve structure initialization söz diziminde (*) kullanımı,
    /// başlangıç değerinin ilgili tüm elemanlara uygulanacağını ifade eder.
    /// Basit declaration aşamasında bile bu bilginin model üzerinde
    /// korunması gerekir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM4 CHAR(8) INIT((*)' ');
    ///
    /// Beklenen model:
    /// - InitialValue.Value: " "
    /// - InitialValue.RepeatCount: null
    /// - InitialValue.AppliesToAllElements: true
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - INIT all-elements semantics bilgisini doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Structure array desteği geldiğinde INIT((*)' ') davranışı bu bilgi
    /// üzerinden yorumlanacaktır.
    /// </summary>
    [Fact]
    public void Parse_WithAllElementsInitValue_ShouldSetAppliesToAllElements()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM4 CHAR(8) INIT((*)' ');").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM4", declaration.Name);
        Assert.NotNull(declaration.InitialValue);
        Assert.Equal(" ", declaration.InitialValue!.Value);
        Assert.Null(declaration.InitialValue.RepeatCount);
        Assert.True(declaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I INITIAL keyword'ünün INIT ile aynı başlangıç değeri davranışını
    /// ürettiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I dilinde INITIAL uzun kullanım, INIT ise kısa kullanım olarak
    /// kullanılabilir.
    /// Parser iki keyword'ü de aynı Pl1InitialValue modeline dönüştürmelidir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL PARAM5 CHAR(4) INITIAL('ABCD');
    ///
    /// Beklenen model:
    /// - InitialValue.Value: "ABCD"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - INIT / INITIAL alias davranışını doğrulamada
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Farklı PL/I kod standartlarında INIT yerine INITIAL kullanılsa bile
    /// dönüşüm hattının aynı davranışı korumasını sağlar.
    /// </summary>
    [Fact]
    public void Parse_WithInitialKeyword_ShouldSetInitialValue()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM5 CHAR(4) INITIAL('ABCD');").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);

        Assert.Equal("PARAM5", declaration.Name);
        Assert.NotNull(declaration.InitialValue);
        Assert.Equal("ABCD", declaration.InitialValue!.Value);
        Assert.Null(declaration.InitialValue.RepeatCount);
        Assert.False(declaration.InitialValue.AppliesToAllElements);
    }
}