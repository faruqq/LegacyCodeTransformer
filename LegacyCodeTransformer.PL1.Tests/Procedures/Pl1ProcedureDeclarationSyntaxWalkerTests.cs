using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.PL1.Tests.Procedures
{
    /// <summary>
    /// Procedure içindeki declaration modellerinin syntax walker
    /// tarafından ziyaret edildiğini doğrular.
    ///
    /// Neden var?
    /// ----------------------
    /// Procedure local declaration modelleri parser çıktısına eklendi.
    /// Visitor tabanlı semantic analizlerin bu modelleri görebilmesi
    /// gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Pl1SyntaxWalker'ın procedure declaration ve statement
    /// koleksiyonlarını birlikte dolaştığını doğrular.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Procedure traversal regression testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Procedure local symbol table ve parameter binding analizlerinin
    /// declaration modellerine ulaşmasını güvence altına alır.
    /// </summary>
    public sealed class Pl1ProcedureDeclarationSyntaxWalkerTests
    {
        [Fact]
        public void Visit_ShouldVisitProcedureDeclarationAndStatement()
        {
            var declaration = new Pl1VariableDeclaration(
                "PROCESS_TEXT",
                new Pl1CharacterType(
                    50,
                    SourceLocation.Unknown),
                SourceLocation.Unknown,
                initialValue: null,
                arraySize: null);

            var statement = new Pl1CallStatement(
                "WRITE_ERROR",
                null,
                SourceLocation.Unknown);

            var procedure = new Pl1Procedure(
                "CUSTOMER_PROCESS",
                null,
                new[] { statement },
                SourceLocation.Unknown,
                new[] { "PROCESS_TEXT" },
                new[] { declaration });

            var syntaxTree = new Pl1SyntaxTree(
                null,
                new[] { procedure },
                null,
                SourceLocation.Unknown);

            var walker = new CountingWalker();

            walker.Visit(syntaxTree);

            Assert.Equal(
                1,
                walker.ProcedureCount);

            Assert.Equal(
                1,
                walker.VariableDeclarationCount);

            Assert.Equal(
                1,
                walker.CallStatementCount);
        }

        private sealed class CountingWalker : Pl1SyntaxWalker
        {
            public int ProcedureCount { get; private set; }

            public int VariableDeclarationCount { get; private set; }

            public int CallStatementCount { get; private set; }

            protected override void VisitProcedure(
                Pl1Procedure procedure)
            {
                ProcedureCount++;

                base.VisitProcedure(procedure);
            }

            protected override void VisitVariableDeclaration(
                Pl1VariableDeclaration declaration)
            {
                VariableDeclarationCount++;

                base.VisitVariableDeclaration(declaration);
            }

            protected override void VisitCallStatement(
                Pl1CallStatement statement)
            {
                CallStatementCount++;

                base.VisitCallStatement(statement);
            }
        }
    }
}