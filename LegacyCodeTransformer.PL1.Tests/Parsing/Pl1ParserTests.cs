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
        Assert.Null(dataType.Scale);
    }

    [Fact]
    public void Parse_WithFixedDecimalZeroScaleDeclaration_ShouldCreateVariableDeclarationWithZeroScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL MUST_NO FIXED DECIMAL(8,0);").Tokenize();
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
    /// PL/I structure array declaration ifadesindeki array dimension bilgisinin
    /// Pl1StructureDeclaration.ArraySize alanına taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// DCL 1 DIZI(6), yapısındaki 6 değerinin parser tarafından kaybedilmeden
    /// syntax modeline taşındığını doğrular.
    ///
    /// Test edilen input:
    /// - DCL 1 DIZI(6)
    /// - Altında 5 adet CHAR member
    ///
    /// Beklenen temel model:
    /// - Pl1StructureDeclaration
    /// - Name: DIZI
    /// - Level: 1
    /// - ArraySize: 6
    /// - Members.Count: 5
    /// </summary>
    [Fact]
    public void Parse_WithStructureArrayDeclaration_ShouldSetArraySize()
    {
        // Arrange
        var source =
            "DCL 1 DIZI(6), " +
            "3 DIZI_PARAM1 CHAR(01), " +
            "3 DIZI_PARAM2 CHAR(02), " +
            "3 DIZI_PARAM3 CHAR(02), " +
            "3 DIZI_PARAM4 CHAR(02), " +
            "3 DIZI_PARAM5 CHAR(08);";

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
        Assert.Equal(5, structureDeclaration.Members.Count);

        Assert.Equal(3, structureDeclaration.Members[0].Level);
        Assert.Equal("DIZI_PARAM1", structureDeclaration.Members[0].Name);

        var firstMemberType = Assert.IsType<Pl1CharacterType>(
            structureDeclaration.Members[0].DataType);

        Assert.Equal(1, firstMemberType.Length);

        var fifthMemberType = Assert.IsType<Pl1CharacterType>(
            structureDeclaration.Members[4].DataType);

        Assert.Equal(8, fifthMemberType.Length);
    }

    /// <summary>
    /// PL/I structure member üzerinde bulunan array dimension bilgisinin
    /// Pl1StructureMember.ArraySize alanına taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın structure member adı sonrasında gelen `(n)` dimension bilgisini
    /// doğru okuyup member modeli üzerinde sakladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM_LIST(2) CHAR(10);
    ///
    /// Beklenen temel model:
    /// ----------------------
    /// - Pl1StructureDeclaration
    /// - Name: PARAME_LIST
    /// - Members.Count: 1
    /// - Members[0].Name: PARAM_LIST
    /// - Members[0].ArraySize: 2
    /// - Members[0].DataType: Pl1CharacterType
    /// - Members[0].DataType.Length: 10
    /// </summary>
    [Fact]
    public void Parse_WithStructureMemberArrayDeclaration_ShouldSetMemberArraySize()
    {
        // Arrange
        var source =
            "DCL 1 PARAME_LIST, " +
            "5 PARAM_LIST(2) CHAR(10);";

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
        Assert.Null(structureDeclaration.ArraySize);

        var member = Assert.Single(structureDeclaration.Members);

        Assert.Equal(5, member.Level);
        Assert.Equal("PARAM_LIST", member.Name);
        Assert.Equal(2, member.ArraySize);

        var memberType = Assert.IsType<Pl1CharacterType>(member.DataType);

        Assert.Equal(10, memberType.Length);
        Assert.Null(member.InitialValue);
    }

    /// <summary>
    /// PL/I structure içinde veri tipi olmayan nested group member yapısının
    /// child member listesiyle birlikte parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın `5 ADRES_BILGI, 10 IL_KOD..., 10 ILCE_KOD...` yapısını
    /// düz member listesi olarak değil, ADRES_BILGI altında child member'lar
    /// olacak şekilde modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL 1 PARAME_LIST,
    ///     5 ADRES_BILGI,
    ///         10 IL_KOD CHAR(02),
    ///         10 ILCE_KOD CHAR(03);
    ///
    /// Beklenen temel model:
    /// ----------------------
    /// - Root structure: PARAME_LIST
    /// - Root member: ADRES_BILGI
    /// - ADRES_BILGI.DataType: null
    /// - ADRES_BILGI.IsGroup: true
    /// - ADRES_BILGI.Members.Count: 2
    /// - Child members: IL_KOD, ILCE_KOD
    /// </summary>
    [Fact]
    public void Parse_WithNestedStructureMember_ShouldCreateGroupMemberWithChildren()
    {
        // Arrange
        var source =
            "DCL 1 PARAME_LIST, " +
            "5 ADRES_BILGI, " +
            "10 IL_KOD CHAR(02), " +
            "10 ILCE_KOD CHAR(03);";

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

        Assert.Equal("PARAME_LIST", structureDeclaration.Name);

        var groupMember = Assert.Single(structureDeclaration.Members);

        Assert.Equal(5, groupMember.Level);
        Assert.Equal("ADRES_BILGI", groupMember.Name);
        Assert.Null(groupMember.DataType);
        Assert.True(groupMember.IsGroup);
        Assert.Equal(2, groupMember.Members.Count);

        var firstChild = groupMember.Members[0];

        Assert.Equal(10, firstChild.Level);
        Assert.Equal("IL_KOD", firstChild.Name);
        Assert.False(firstChild.IsGroup);

        var firstChildType = Assert.IsType<Pl1CharacterType>(firstChild.DataType);

        Assert.Equal(2, firstChildType.Length);

        var secondChild = groupMember.Members[1];

        Assert.Equal(10, secondChild.Level);
        Assert.Equal("ILCE_KOD", secondChild.Name);
        Assert.False(secondChild.IsGroup);

        var secondChildType = Assert.IsType<Pl1CharacterType>(secondChild.DataType);

        Assert.Equal(3, secondChildType.Length);
    }

    /// <summary>
    /// PL/I structure içinde birden fazla nested group seviyesi bulunduğunda
    /// parser'ın level hiyerarşisini doğru kurduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın `5 GROUP1, 10 GROUP2, 15 FIELD1...` yapısını çok seviyeli
    /// nested member ağacı olarak modellediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL 1 PARAME_LIST,
    ///     5 GROUP1,
    ///         10 GROUP2,
    ///             15 FIELD1 CHAR(01);
    ///
    /// Beklenen temel model:
    /// ----------------------
    /// - GROUP1 root member olarak gelir.
    /// - GROUP1 altında GROUP2 vardır.
    /// - GROUP2 altında FIELD1 vardır.
    /// - GROUP1 ve GROUP2 group member'dır.
    /// - FIELD1 normal typed field'dır.
    /// </summary>
    [Fact]
    public void Parse_WithMultiLevelNestedStructureMember_ShouldCreateNestedGroupTree()
    {
        // Arrange
        var source =
            "DCL 1 PARAME_LIST, " +
            "5 GROUP1, " +
            "10 GROUP2, " +
            "15 FIELD1 CHAR(01);";

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

        Assert.Equal("PARAME_LIST", structureDeclaration.Name);

        var group1 = Assert.Single(structureDeclaration.Members);

        Assert.Equal(5, group1.Level);
        Assert.Equal("GROUP1", group1.Name);
        Assert.True(group1.IsGroup);
        Assert.Null(group1.DataType);

        var group2 = Assert.Single(group1.Members);

        Assert.Equal(10, group2.Level);
        Assert.Equal("GROUP2", group2.Name);
        Assert.True(group2.IsGroup);
        Assert.Null(group2.DataType);

        var field1 = Assert.Single(group2.Members);

        Assert.Equal(15, field1.Level);
        Assert.Equal("FIELD1", field1.Name);
        Assert.False(field1.IsGroup);

        var field1Type = Assert.IsType<Pl1CharacterType>(field1.DataType);

        Assert.Equal(1, field1Type.Length);
    }

    /// <summary>
    /// PL/I VARCHAR(n) tanımının Pl1VarcharType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın VARCHAR keyword'ünü tanıdığını ve parantez içindeki uzunluk
    /// bilgisini Pl1VarcharType.Length alanına taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL CUSTOMER_NAME VARCHAR(50);
    ///
    /// Beklenen temel model:
    /// ----------------------
    /// - Pl1VariableDeclaration
    /// - Name: CUSTOMER_NAME
    /// - DataType: Pl1VarcharType
    /// - Length: 50
    /// </summary>
    [Fact]
    public void Parse_WithVarcharDeclaration_ShouldCreateVarcharTypeDeclaration()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL CUSTOMER_NAME VARCHAR(50);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("CUSTOMER_NAME", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1VarcharType>(variableDeclaration.DataType);

        Assert.Equal(50, dataType.Length);
    }

    /// <summary>
    /// PL/I FIXED DECIMAL(p) tanımında scale verilmediğinde Scale bilgisinin
    /// null olarak taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın FIXED DECIMAL(15) ile FIXED DECIMAL(15,0) ifadelerini aynı
    /// model gibi üretmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT FIXED DECIMAL(15);
    ///
    /// Beklenen temel model:
    /// - Precision: 15
    /// - Scale: null
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalWithoutScale_ShouldSetScaleNull()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT FIXED DECIMAL(15);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED DECIMAL(p,0) tanımında scale açıkça 0 verildiğinde Scale
    /// bilgisinin 0 olarak taşındığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın kaynakta açıkça yazılan ,0 bilgisini kaybetmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT FIXED DECIMAL(15,0);
    ///
    /// Beklenen temel model:
    /// - Precision: 15
    /// - Scale: 0
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecimalExplicitZeroScale_ShouldSetScaleZero()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT FIXED DECIMAL(15,0);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED DEC(p,s) synonym kullanımının Pl1FixedDecimalType olarak
    /// parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın DEC kısaltmasını DECIMAL ile aynı semantic modele taşıdığını
    /// doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL AMOUNT FIXED DEC(17,2);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedDecimalType
    /// - Precision: 17
    /// - Scale: 2
    /// </summary>
    [Fact]
    public void Parse_WithFixedDecDeclaration_ShouldCreateFixedDecimalType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL AMOUNT FIXED DEC(17,2);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("AMOUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

        Assert.Equal(17, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// PL/I DEC FIXED(p,s) ters keyword sırasının Pl1FixedDecimalType olarak
    /// parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın DEC FIXED kullanımını FIXED DEC ile aynı semantic modele
    /// taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL AMOUNT DEC FIXED(17,2);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedDecimalType
    /// - Precision: 17
    /// - Scale: 2
    /// </summary>
    [Fact]
    public void Parse_WithDecFixedDeclaration_ShouldCreateFixedDecimalType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL AMOUNT DEC FIXED(17,2);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("AMOUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

        Assert.Equal(17, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
    }

    /// <summary>
    /// PL/I DECIMAL FIXED(p) ters keyword sırasının scale olmadan parse
    /// edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın DECIMAL FIXED(15) kullanımında scale bilgisini null olarak
    /// koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT DECIMAL FIXED(15);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedDecimalType
    /// - Precision: 15
    /// - Scale: null
    /// </summary>
    [Fact]
    public void Parse_WithDecimalFixedDeclarationWithoutScale_ShouldCreateFixedDecimalTypeWithNullScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT DECIMAL FIXED(15);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("COUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedDecimalType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED BIN(15) kullanımının Pl1FixedBinaryType olarak parse edildiğini
    /// doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın FIXED BIN synonym kullanımını binary fixed semantic modele
    /// taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT FIXED BIN(15);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedBinaryType
    /// - Precision: 15
    /// - Scale: null
    /// </summary>
    [Fact]
    public void Parse_WithFixedBinDeclaration_ShouldCreateFixedBinaryType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT FIXED BIN(15);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("COUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I BIN FIXED(31) ters keyword sırasının Pl1FixedBinaryType olarak
    /// parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın BIN FIXED kullanımını FIXED BIN ile aynı semantic modele
    /// taşıdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT BIN FIXED(31);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedBinaryType
    /// - Precision: 31
    /// - Scale: null
    /// </summary>
    [Fact]
    public void Parse_WithBinFixedDeclaration_ShouldCreateFixedBinaryType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT BIN FIXED(31);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("COUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);

        Assert.Equal(31, dataType.Precision);
        Assert.Null(dataType.Scale);
    }

    /// <summary>
    /// PL/I FIXED BINARY(15,0) kullanımında explicit zero scale bilgisinin
    /// Pl1FixedBinaryType üzerinde korunduğunu doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın binary fixed kullanımında açıkça yazılan ,0 bilgisini
    /// kaybetmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL COUNT FIXED BINARY(15,0);
    ///
    /// Beklenen temel model:
    /// - Pl1FixedBinaryType
    /// - Precision: 15
    /// - Scale: 0
    /// </summary>
    [Fact]
    public void Parse_WithFixedBinaryExplicitZeroScale_ShouldCreateFixedBinaryTypeWithZeroScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL COUNT FIXED BINARY(15,0);").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("COUNT", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1FixedBinaryType>(variableDeclaration.DataType);

        Assert.Equal(15, dataType.Precision);
        Assert.Equal(0, dataType.Scale);
    }

    /// <summary>
    /// PL/I PIC '999' numeric pattern bilgisinin Pl1PictureType olarak parse
    /// edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın PIC keyword'ünü tanıdığını ve numeric picture pattern için
    /// precision bilgisini hesapladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL PARAM1 PIC '999';
    ///
    /// Beklenen temel model:
    /// - RawPattern: 999
    /// - Precision: 3
    /// - Scale: null
    /// - IsNumeric: true
    /// - IsFormatted: false
    /// </summary>
    [Fact]
    public void Parse_WithNumericPicDeclaration_ShouldCreatePictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM1 PIC '999';").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM1", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("999", dataType.RawPattern);
        Assert.Equal(3, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.False(dataType.IsSigned);
        Assert.True(dataType.IsNumeric);
        Assert.False(dataType.IsFormatted);
    }

    /// <summary>
    /// PL/I PIC '999V99' numeric implied decimal pattern bilgisinin scale ile
    /// parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın V karakterini implied decimal point olarak yorumladığını ve
    /// V sonrasındaki digit sayısını scale olarak hesapladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL PARAM2 PIC '999V99';
    ///
    /// Beklenen temel model:
    /// - RawPattern: 999V99
    /// - Precision: 5
    /// - Scale: 2
    /// - IsNumeric: true
    /// </summary>
    [Fact]
    public void Parse_WithNumericPicHavingImpliedDecimal_ShouldCreatePictureTypeWithScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM2 PIC '999V99';").Tokenize();
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

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("999V99", dataType.RawPattern);
        Assert.Equal(5, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.False(dataType.IsSigned);
        Assert.True(dataType.IsNumeric);
        Assert.False(dataType.IsFormatted);
    }

    /// <summary>
    /// PL/I PIC 'S999' signed numeric pattern bilgisinin IsSigned ile parse
    /// edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın S prefix bilgisini sign metadata olarak koruduğunu doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL PARAM3 PIC 'S999';
    ///
    /// Beklenen temel model:
    /// - RawPattern: S999
    /// - Precision: 3
    /// - IsSigned: true
    /// </summary>
    [Fact]
    public void Parse_WithSignedNumericPicDeclaration_ShouldCreateSignedPictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM3 PIC 'S999';").Tokenize();
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

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("S999", dataType.RawPattern);
        Assert.Equal(3, dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.True(dataType.IsSigned);
        Assert.True(dataType.IsNumeric);
        Assert.False(dataType.IsFormatted);
    }

    /// <summary>
    /// PL/I PIC '(13)9V99' repeat count ve implied decimal içeren pattern
    /// bilgisinin doğru parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın `(n)9` tekrar söz dizimini precision hesabına dahil ettiğini
    /// ve V sonrasındaki digit sayısını scale olarak hesapladığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL PARAM4 PIC '(13)9V99';
    ///
    /// Beklenen temel model:
    /// - RawPattern: (13)9V99
    /// - Precision: 15
    /// - Scale: 2
    /// </summary>
    [Fact]
    public void Parse_WithRepeatedNumericPicHavingImpliedDecimal_ShouldCreatePictureTypeWithPrecisionAndScale()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM4 PIC '(13)9V99';").Tokenize();
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

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("(13)9V99", dataType.RawPattern);
        Assert.Equal(15, dataType.Precision);
        Assert.Equal(2, dataType.Scale);
        Assert.False(dataType.IsSigned);
        Assert.True(dataType.IsNumeric);
        Assert.False(dataType.IsFormatted);
    }

    /// <summary>
    /// PL/I PIC 'ZZ9' formatted pattern bilgisinin numeric olmayan formatted
    /// picture olarak işaretlendiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// ----------------------
    /// Parser'ın Z edit mask içeren picture pattern'i güvenli numeric pattern
    /// olarak kabul etmediğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// ----------------------
    /// DCL PARAM5 PIC 'ZZ9';
    ///
    /// Beklenen temel model:
    /// - RawPattern: ZZ9
    /// - IsNumeric: false
    /// - IsFormatted: true
    /// </summary>
    [Fact]
    public void Parse_WithFormattedPicDeclaration_ShouldCreateFormattedPictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM5 PIC 'ZZ9';").Tokenize();
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

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("ZZ9", dataType.RawPattern);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.False(dataType.IsSigned);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsFormatted);
    }

    /// <summary>
    /// PL/I PIC 'XXX' alphanumeric pattern bilgisinin Pl1PictureType olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın X karakterlerinden oluşan PIC pattern'ı alphanumeric olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM6 PIC 'XXX';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// RawPattern XXX, Category Alphanumeric, Length 3, IsAlphanumeric true.
    /// </summary>
    [Fact]
    public void Parse_WithAlphanumericPicDeclaration_ShouldCreateAlphanumericPictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM6 PIC 'XXX';").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM6", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("XXX", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(3, dataType.Length);
        Assert.False(dataType.IsSigned);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC '(20)X' alphanumeric repeat pattern bilgisinin length olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın (n)X tekrar söz dizimini alphanumeric length hesabına dahil ettiğini doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM7 PIC '(20)X';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// RawPattern (20)X, Category Alphanumeric, Length 20, IsAlphanumeric true.
    /// </summary>
    [Fact]
    public void Parse_WithRepeatedAlphanumericPicDeclaration_ShouldCreateAlphanumericPictureTypeWithLength()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM7 PIC '(20)X';").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM7", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("(20)X", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(20, dataType.Length);
        Assert.False(dataType.IsSigned);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC 'AAA' alphabetic pattern bilgisinin alphanumeric picture olarak parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın A karakterlerinden oluşan PIC pattern'ı karakter tabanlı alan olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM8 PIC 'AAA';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// RawPattern AAA, Category Alphanumeric, Length 3, IsAlphanumeric true.
    /// </summary>
    [Fact]
    public void Parse_WithAlphabeticPicDeclaration_ShouldCreateAlphanumericPictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM8 PIC 'AAA';").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM8", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("AAA", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(3, dataType.Length);
        Assert.False(dataType.IsSigned);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }

    /// <summary>
    /// PL/I PIC 'AXXAA' mixed alphanumeric pattern bilgisinin toplam length ile parse edildiğini doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Parser'ın A ve X karakterlerinden oluşan mixed alphanumeric PIC pattern'ı tek karakter alanı olarak sınıflandırdığını doğrular.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM9 PIC 'AXXAA';
    ///
    /// Beklenen temel model/çıktı nedir?
    /// RawPattern AXXAA, Category Alphanumeric, Length 5, IsAlphanumeric true.
    /// </summary>
    [Fact]
    public void Parse_WithMixedAlphanumericPicDeclaration_ShouldCreateAlphanumericPictureType()
    {
        // Arrange
        var tokens = new Pl1Lexer("DCL PARAM9 PIC 'AXXAA';").Tokenize();
        var parser = new Pl1Parser(tokens);

        // Act
        var result = parser.Parse();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        var declaration = Assert.Single(result.SyntaxTree!.Declarations);
        var variableDeclaration = Assert.IsType<Pl1VariableDeclaration>(declaration);

        Assert.Equal("PARAM9", variableDeclaration.Name);

        var dataType = Assert.IsType<Pl1PictureType>(variableDeclaration.DataType);

        Assert.Equal("AXXAA", dataType.RawPattern);
        Assert.Equal(Pl1PictureCategory.Alphanumeric, dataType.Category);
        Assert.Null(dataType.Precision);
        Assert.Null(dataType.Scale);
        Assert.Equal(5, dataType.Length);
        Assert.False(dataType.IsSigned);
        Assert.False(dataType.IsNumeric);
        Assert.True(dataType.IsAlphanumeric);
        Assert.False(dataType.IsFormatted);
        Assert.True(dataType.SupportsDirectEglMapping);
    }
}