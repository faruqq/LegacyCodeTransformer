using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Pl1.Statements;
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

    /// <summary>
    /// Syntax walker'ın assignment statement altındaki target ve value expression node'larını dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker, Pl1AssignmentStatement modeline ulaştığında hem sol taraf target
    /// expression listesini hem de sağ taraf value expression modelini ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde PARAM = 'ABC'; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// AssignmentStatementCount 1 ve RawExpressionCount 2 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithAssignmentStatement_ShouldVisitTargetsAndValue()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
                new Pl1AssignmentStatement(
                    targets: new[]
                    {
                        new Pl1RawExpression("PARAM", SourceLocation.Unknown)
                    },
                    value: new Pl1RawExpression("'ABC'", SourceLocation.Unknown),
                    location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.AssignmentStatementCount);
        Assert.Equal(2, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın CALL statement altındaki argument expression node'larını dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker, Pl1CallStatement modeline ulaştığında CALL argument listesindeki
    /// expression modellerini recursive olarak ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde CALL SQL_HATA_OLUSTUR('SELECT GLAU_HISTORY'); karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// CallStatementCount 1 ve RawExpressionCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithCallStatement_ShouldVisitArguments()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
                new Pl1CallStatement(
                    procedureName: "SQL_HATA_OLUSTUR",
                    arguments: new[]
                    {
                        new Pl1RawExpression("'SELECT GLAU_HISTORY'", SourceLocation.Unknown)
                    },
                    location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.CallStatementCount);
        Assert.Equal(1, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın IF statement içindeki condition, THEN ve ELSE statement modellerini dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker, Pl1IfStatement modelinde condition expression, then statement
    /// ve optional else statement alanlarını ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde IF SQLCODE = 100 THEN A = 0; ELSE A = 1; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// IfStatementCount 1, AssignmentStatementCount 2 ve RawExpressionCount 5 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithIfStatement_ShouldVisitConditionThenAndElseStatements()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
                new Pl1IfStatement(
                    condition: new Pl1RawExpression("SQLCODE = 100", SourceLocation.Unknown),
                    thenStatement: CreateAssignmentStatement("A", "0"),
                    elseStatement: CreateAssignmentStatement("A", "1"),
                    location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.IfStatementCount);
        Assert.Equal(2, walker.AssignmentStatementCount);
        Assert.Equal(5, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın DO statement condition ve body modellerini dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Pl1SyntaxWalker, Pl1DoStatement modelinde condition varsa condition expression'ı
    /// ve body block içindeki child statement listesini ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DoStatementCount 1, BlockStatementCount 1, CallStatementCount 1 ve RawExpressionCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithDoWhileStatement_ShouldVisitConditionAndBody()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
                new Pl1DoStatement(
                    kind: Pl1DoStatementKind.While,
                    condition: new Pl1RawExpression("SQLCODE = 0", SourceLocation.Unknown),
                    body: new Pl1BlockStatement(
                        statements: new[]
                        {
                            new Pl1CallStatement(
                                procedureName: "FETCH_CURSOR",
                                arguments: null,
                                location: SourceLocation.Unknown)
                        },
                        location: SourceLocation.Unknown),
                    location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.DoStatementCount);
        Assert.Equal(1, walker.BlockStatementCount);
        Assert.Equal(1, walker.CallStatementCount);
        Assert.Equal(1, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın IF THEN ELSE statement hiyerarşisini eksiksiz dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Walker, IF condition expression'ı, THEN statement'ı ve ELSE statement'ı recursive
    /// olarak ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde IF A = B THEN CALL PROC1; ELSE CALL PROC2; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// IfStatementCount 1, CallStatementCount 2 ve RawExpressionCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithIfThenElseStatement_ShouldVisitConditionThenAndElse()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
            new Pl1IfStatement(
                condition: new Pl1RawExpression("A = B", SourceLocation.Unknown),
                thenStatement: new Pl1CallStatement(
                    procedureName: "PROC1",
                    arguments: null,
                    location: SourceLocation.Unknown),
                elseStatement: new Pl1CallStatement(
                    procedureName: "PROC2",
                    arguments: null,
                    location: SourceLocation.Unknown),
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.IfStatementCount);
        Assert.Equal(2, walker.CallStatementCount);
        Assert.Equal(1, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın DO body içindeki nested statement hiyerarşisini dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Walker, DO statement condition alanını, body block'unu ve block içindeki child
    /// statement listesini recursive olarak ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DoStatementCount 1, BlockStatementCount 1, CallStatementCount 1 ve RawExpressionCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithDoWhileStatement_ShouldVisitConditionBodyAndChildStatement()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
            new Pl1DoStatement(
                kind: Pl1DoStatementKind.While,
                condition: new Pl1RawExpression("SQLCODE = 0", SourceLocation.Unknown),
                body: new Pl1BlockStatement(
                    statements: new[]
                    {
                        new Pl1CallStatement(
                            procedureName: "FETCH_CURSOR",
                            arguments: null,
                            location: SourceLocation.Unknown)
                    },
                    location: SourceLocation.Unknown),
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.DoStatementCount);
        Assert.Equal(1, walker.BlockStatementCount);
        Assert.Equal(1, walker.CallStatementCount);
        Assert.Equal(1, walker.RawExpressionCount);
    }

    /// <summary>
    /// Syntax walker'ın nested DO block hiyerarşisini recursive dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Walker, DO body içinde tekrar DO statement bulunduğunda iç DO block'a kadar
    /// recursive olarak inmeli ve child statement'ları ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde DO; DO; CALL PROC1; END; END; karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// DoStatementCount 2, BlockStatementCount 2 ve CallStatementCount 1 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithNestedDoBlock_ShouldVisitNestedStatementHierarchy()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
            new Pl1DoStatement(
                kind: Pl1DoStatementKind.Block,
                condition: null,
                body: new Pl1BlockStatement(
                    statements: new[]
                    {
                        new Pl1DoStatement(
                            kind: Pl1DoStatementKind.Block,
                            condition: null,
                            body: new Pl1BlockStatement(
                                statements: new[]
                                {
                                    new Pl1CallStatement(
                                        procedureName: "PROC1",
                                        arguments: null,
                                        location: SourceLocation.Unknown)
                                },
                                location: SourceLocation.Unknown),
                            location: SourceLocation.Unknown)
                    },
                    location: SourceLocation.Unknown),
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(2, walker.DoStatementCount);
        Assert.Equal(2, walker.BlockStatementCount);
        Assert.Equal(1, walker.CallStatementCount);
    }

    /// <summary>
    /// Syntax walker'ın CALL argument expression listesini dolaştığını doğrular.
    ///
    /// Bu test neyi doğrular?
    /// Walker, Pl1CallStatement argument listesindeki her expression node'unu ziyaret etmelidir.
    ///
    /// Hangi input'u test eder?
    /// Model seviyesinde CALL PROC1(A, 'ABC', B); karşılığı oluşturulur.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// CallStatementCount 1 ve RawExpressionCount 3 olmalıdır.
    /// </summary>
    [Fact]
    public void Visit_WithCallArguments_ShouldVisitAllArgumentExpressions()
    {
        var syntaxTree = new Pl1SyntaxTree(
            declarations: null,
            statements: new[]
            {
            new Pl1CallStatement(
                procedureName: "PROC1",
                arguments: new[]
                {
                    new Pl1RawExpression("A", SourceLocation.Unknown),
                    new Pl1RawExpression("'ABC'", SourceLocation.Unknown),
                    new Pl1RawExpression("B", SourceLocation.Unknown)
                },
                location: SourceLocation.Unknown)
            },
            location: SourceLocation.Unknown);

        var walker = new CountingSyntaxWalker();

        walker.Visit(syntaxTree);

        Assert.Equal(1, walker.CallStatementCount);
        Assert.Equal(3, walker.RawExpressionCount);
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

    private static Pl1AssignmentStatement CreateAssignmentStatement(string target, string value)
    {
        return new Pl1AssignmentStatement(
            targets: new[]
            {
                new Pl1RawExpression(target, SourceLocation.Unknown)
            },
            value: new Pl1RawExpression(value, SourceLocation.Unknown),
            location: SourceLocation.Unknown);
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

        public int AssignmentStatementCount { get; private set; }

        public int CallStatementCount { get; private set; }

        public int IfStatementCount { get; private set; }

        public int DoStatementCount { get; private set; }

        public int BlockStatementCount { get; private set; }

        public int RawExpressionCount { get; private set; }

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

        protected override void VisitAssignmentStatement(Pl1AssignmentStatement statement)
        {
            AssignmentStatementCount++;

            base.VisitAssignmentStatement(statement);
        }

        protected override void VisitCallStatement(Pl1CallStatement statement)
        {
            CallStatementCount++;

            base.VisitCallStatement(statement);
        }

        protected override void VisitIfStatement(Pl1IfStatement statement)
        {
            IfStatementCount++;

            base.VisitIfStatement(statement);
        }

        protected override void VisitDoStatement(Pl1DoStatement statement)
        {
            DoStatementCount++;

            base.VisitDoStatement(statement);
        }

        protected override void VisitBlockStatement(Pl1BlockStatement statement)
        {
            BlockStatementCount++;

            base.VisitBlockStatement(statement);
        }

        protected override void VisitRawExpression(Pl1RawExpression expression)
        {
            RawExpressionCount++;

            base.VisitRawExpression(expression);
        }
    }
}