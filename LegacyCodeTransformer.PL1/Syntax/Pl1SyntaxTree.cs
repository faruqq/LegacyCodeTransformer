using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// Parser, PL/I kaynak kodunu okuduktan sonra string veya satır listesi yerine
    /// bu modeli üretir. Bu sınıf PL/I tarafındaki kök syntax tree modelidir.
    ///
    /// Ne çözüyor?
    /// Declaration, procedure ve executable statement modellerini tek bir root
    /// altında taşır.
    ///
    /// Hangi örneği destekliyor?
    /// DCL PARAM CHAR(08);
    /// PROCEDURE_NAME: PROCEDURE;
    ///     CALL FETCH_CURSOR;
    /// END PROCEDURE_NAME;
    ///
    /// Nerede kullanılır?
    /// Pl1Parser.Parse sonucunda, normalizer girişinde, transpiler girişinde,
    /// semantic analyzer ve visitor/walker traversal işlemlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Embedded SQL, INCLUDE, ON condition, SELECT, RETURN, STOP, READ/WRITE
    /// gibi yeni PL/I öğeleri eklendikçe root syntax tree bu modelleri taşımaya
    /// devam edecektir.
    /// </summary>
    public sealed class Pl1SyntaxTree : SyntaxTree
    {
        public IReadOnlyList<Pl1Declaration> Declarations { get; }

        public IReadOnlyList<Pl1Procedure> Procedures { get; }

        public IReadOnlyList<Pl1Statement> Statements { get; }

        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            SourceLocation location)
            : this(declarations, null, null, location)
        {
        }

        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            IEnumerable<Pl1Statement>? statements,
            SourceLocation location)
            : this(declarations, null, statements, location)
        {
        }

        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            IEnumerable<Pl1Procedure>? procedures,
            IEnumerable<Pl1Statement>? statements,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<Pl1Declaration>();
            Procedures = procedures?.ToList() ?? new List<Pl1Procedure>();
            Statements = statements?.ToList() ?? new List<Pl1Statement>();
        }
    }
}