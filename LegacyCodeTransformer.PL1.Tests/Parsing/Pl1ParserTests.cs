using LegacyCodeTransformer.Pl1.Declarations;
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("MUST_NO", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("CUSTOMER_NO", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);

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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM
    /// - DataType: Pl1CharacterType
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);

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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM
    /// - DataType: Pl1CharacterType
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1CharacterType>(variableDeclaration.DataType);

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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM2
    /// - InitialValue.Value: ";"
    /// - InitialValue.RepeatCount: null
    /// - InitialValue.AppliesToAllElements: false
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM2", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(";", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM3
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM3", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Equal(8, variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM4
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM4", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal(" ", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.True(variableDeclaration.InitialValue.AppliesToAllElements);
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
    /// - Pl1VariableDeclaration
    /// - Name: PARAM5
    /// - InitialValue.Value: "ABCD"
    /// - InitialValue.RepeatCount: null
    /// - InitialValue.AppliesToAllElements: false
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
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM5", variableDeclaration.Name);
        Assert.NotNull(variableDeclaration.InitialValue);
        Assert.Equal("ABCD", variableDeclaration.InitialValue!.Value);
        Assert.Null(variableDeclaration.InitialValue.RepeatCount);
        Assert.False(variableDeclaration.InitialValue.AppliesToAllElements);
    }

    /// <summary>
    /// PL/I structure declaration ifadesinin Pl1StructureDeclaration olarak parse edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// DCL sonrasında seviye numarası geldiğinde parser tekil değişken değil,
    /// structure declaration üretmelidir.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08) INIT(' '),
    ///     5 PARAM2 CHAR(01) INIT(';');
    ///
    /// Beklenen model:
    /// - Pl1StructureDeclaration
    /// - Name: PARAME_LIST
    /// - Members.Count: 2
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - Structure declaration desteğini doğrulamada
    /// </summary>
    [Fact]
    public void Parse_WithStructureDeclaration_ShouldCreateStructureDeclaration()
    {
        // Arrange
        var source =
            "DCL 1 PARAME_LIST, " +
            "5 PARAM CHAR(08) INIT(' '), " +
            "5 PARAM2 CHAR(01) INIT(';');";

        var tokens = new Pl1Lexer(source).Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var structureDeclaration = Assert.IsType<Pl1StructureDeclaration>(declaration);

        Assert.Equal(1, structureDeclaration.Level);
        Assert.Equal("PARAME_LIST", structureDeclaration.Name);
        Assert.Equal(2, structureDeclaration.Members.Count);
    }

    /// <summary>
    /// PL/I structure member alanlarının veri tipi ve INIT bilgileriyle parse edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Structure declaration yalnızca ana structure adından ibaret değildir.
    /// Alt member alanların level, name, data type ve initial value bilgileri
    /// de syntax tree üzerinde korunmalıdır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser testlerinde
    /// - Structure member parse davranışını doğrulamada
    /// </summary>
    [Fact]
    public void Parse_WithStructureDeclaration_ShouldCreateStructureMembers()
    {
        // Arrange
        var source =
            "DCL 1 PARAME_LIST, " +
            "5 PARAM CHAR(08) INIT(' '), " +
            "5 PARAM2 CHAR(01) INIT(';');";

        var tokens = new Pl1Lexer(source).Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var structureDeclaration = Assert.IsType<Pl1StructureDeclaration>(declaration);

        var firstMember = structureDeclaration.Members[0];

        Assert.Equal(5, firstMember.Level);
        Assert.Equal("PARAM", firstMember.Name);

        var firstMemberType = Assert.IsType<Pl1CharacterType>(firstMember.DataType);
        Assert.Equal(8, firstMemberType.Length);

        Assert.NotNull(firstMember.InitialValue);
        Assert.Equal(" ", firstMember.InitialValue!.Value);

        var secondMember = structureDeclaration.Members[1];

        Assert.Equal(5, secondMember.Level);
        Assert.Equal("PARAM2", secondMember.Name);

        var secondMemberType = Assert.IsType<Pl1CharacterType>(secondMember.DataType);
        Assert.Equal(1, secondMemberType.Length);

        Assert.NotNull(secondMember.InitialValue);
        Assert.Equal(";", secondMember.InitialValue!.Value);
    }

    /// <summary>
    /// PL/I structure array tanımındaki dimension bilgisinin parse edilip
    /// Pl1StructureDeclaration.ArraySize üzerinde saklandığını doğrular.
    ///
    /// Test edilen PL/I:
    ///
    /// DCL 1 DIZI(6),
    ///     3 DIZI_PARAM1 CHAR(01) INIT((*)' '),
    ///     3 DIZI_PARAM2 CHAR(02) INIT((*)' ');
    ///
    /// Beklenen:
    /// - Structure adı: DIZI
    /// - ArraySize: 6
    /// - Member sayısı: 2
    /// </summary>
    [Fact]
    public void Parse_WithStructureArrayDeclaration_ShouldSetArraySize()
    {
        // Arrange
        var source =
            "DCL 1 DIZI(6), " +
            "3 DIZI_PARAM1 CHAR(01) INIT((*)' '), " +
            "3 DIZI_PARAM2 CHAR(02) INIT((*)' ');";

        var tokens = new Pl1Lexer(source).Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var structureDeclaration = Assert.IsType<Pl1StructureDeclaration>(declaration);

        Assert.Equal(1, structureDeclaration.Level);
        Assert.Equal("DIZI", structureDeclaration.Name);
        Assert.Equal(6, structureDeclaration.ArraySize);
        Assert.Equal(2, structureDeclaration.Members.Count);

        Assert.Equal("DIZI_PARAM1", structureDeclaration.Members[0].Name);
        Assert.Equal("DIZI_PARAM2", structureDeclaration.Members[1].Name);
    }
}