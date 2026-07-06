using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Tests.Syntax;

public sealed class Pl1SyntaxWalkerTests
{
    /// <summary>
    /// Syntax walker'ın variable declaration altındaki data type ve initial value node'larını dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker root syntax tree'den başlayarak variable declaration, data type
    /// ve initial value node'larına recursive olarak ulaşmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM CHAR(08) INIT(' ');
    ///
    /// Beklenen temel model/çıktı nedir?
    /// VariableDeclarationCount 1, CharacterTypeCount 1 ve InitialValueCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithVariableDeclaration_ShouldVisitDataTypeAndInitialValue()
    {
        var syntaxTree = ParseSyntaxTree(
            "DCL PARAM CHAR(08) INIT(' ');");

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.VariableDeclarationCount);
        Assert.Equal(1, walker.CharacterTypeCount);
        Assert.Equal(1, walker.InitialValueCount);
        Assert.Equal(0, walker.StructureDeclarationCount);
    }

    /// <summary>
    /// Syntax walker'ın structure declaration ve member hiyerarşisini dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker structure declaration içindeki typed member ve nested group member
    /// hiyerarşisini recursive şekilde dolaşmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCL 1 REC, 5 GROUP1, 10 FIELD1 CHAR(01), 10 FIELD2 FIXED DECIMAL(5);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// StructureDeclarationCount 1, StructureMemberCount 3, CharacterTypeCount 1 ve
    /// FixedDecimalTypeCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithNestedStructureDeclaration_ShouldVisitAllMembersAndDataTypes()
    {
        var syntaxTree = ParseSyntaxTree(
            "DCL 1 REC, 5 GROUP1, 10 FIELD1 CHAR(01), 10 FIELD2 FIXED DECIMAL(5);");

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.StructureDeclarationCount);
        Assert.Equal(3, walker.StructureMemberCount);
        Assert.Equal(1, walker.CharacterTypeCount);
        Assert.Equal(1, walker.FixedDecimalTypeCount);
    }

    /// <summary>
    /// Syntax walker'ın aynı syntax tree içindeki birden fazla declaration'ı dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker syntax tree root declaration listesini sırayla dolaşmalıdır.
    ///
    /// Hangi input'u test eder?
    /// DCL PARAM1 CHAR(08); DCL PARAM2 FIXED BIN(31);
    ///
    /// Beklenen temel model/çıktı nedir?
    /// VariableDeclarationCount 2, CharacterTypeCount 1 ve FixedBinaryTypeCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithMultipleDeclarations_ShouldVisitAllDeclarations()
    {
        var syntaxTree = ParseSyntaxTree(
            "DCL PARAM1 CHAR(08); DCL PARAM2 FIXED BIN(31);");

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(2, walker.VariableDeclarationCount);
        Assert.Equal(1, walker.CharacterTypeCount);
        Assert.Equal(1, walker.FixedBinaryTypeCount);
    }

    private static Pl1SyntaxTree ParseSyntaxTree(string source)
    {
        var tokens = new Pl1Lexer(source).Tokenize();
        var parser = new Pl1Parser(tokens);
        var result = parser.Parse();

        Assert.True(result.Success);
        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.SyntaxTree);

        return result.SyntaxTree!;
    }

    private sealed class CountingSyntaxWalker : Pl1SyntaxWalker
    {
        public int VariableDeclarationCount { get; private set; }

        public int StructureDeclarationCount { get; private set; }

        public int StructureMemberCount { get; private set; }

        public int InitialValueCount { get; private set; }

        public int CharacterTypeCount { get; private set; }

        public int FixedDecimalTypeCount { get; private set; }

        public int FixedBinaryTypeCount { get; private set; }

        protected override void VisitVariableDeclaration(Pl1VariableDeclaration declaration)
        {
            VariableDeclarationCount++;

            base.VisitVariableDeclaration(declaration);
        }

        protected override void VisitStructureDeclaration(Pl1StructureDeclaration declaration)
        {
            StructureDeclarationCount++;

            base.VisitStructureDeclaration(declaration);
        }

        protected override void VisitStructureMember(Pl1StructureMember member)
        {
            StructureMemberCount++;

            base.VisitStructureMember(member);
        }

        protected override void VisitInitialValue(Pl1.InitialValues.Pl1InitialValue initialValue)
        {
            InitialValueCount++;

            base.VisitInitialValue(initialValue);
        }

        protected override void VisitCharacterType(Pl1CharacterType dataType)
        {
            CharacterTypeCount++;

            base.VisitCharacterType(dataType);
        }

        protected override void VisitFixedDecimalType(Pl1FixedDecimalType dataType)
        {
            FixedDecimalTypeCount++;

            base.VisitFixedDecimalType(dataType);
        }

        protected override void VisitFixedBinaryType(Pl1FixedBinaryType dataType)
        {
            FixedBinaryTypeCount++;

            base.VisitFixedBinaryType(dataType);
        }
    }
}