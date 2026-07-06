using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I kaynak kodunun syntax tree karşılığını temsil eder.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// Parser, PL/I kaynak kodunu okuduktan sonra string veya satır listesi yerine
    /// bu modeli üretir. Bu sınıf PL/I tarafındaki kök syntax tree modelidir.
    ///
    /// Ne çözüyor?
    /// Declaration ve executable statement modellerini tek bir root altında taşır.
    /// Önceki fazlarda yalnızca declaration listesi vardı. P05 ile birlikte declaration
    /// dışındaki executable statement modelleri de syntax tree üzerinde temsil edilmeye
    /// başlanır.
    ///
    /// Hangi örneği destekliyor?
    /// DCL PARAM CHAR(08);
    /// PARAM = 'ABC';
    /// CALL FETCH_CURSOR;
    /// IF SQLCODE = 0 THEN DO;
    ///     CALL FETCH_CURSOR;
    /// END;
    ///
    /// Nerede kullanılır?
    /// Pl1Parser.Parse sonucunda, normalizer girişinde, transpiler girişinde,
    /// semantic analyzer ve visitor/walker traversal işlemlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Procedure body, embedded SQL, INCLUDE, ON condition, SELECT, RETURN, STOP,
    /// READ/WRITE gibi yeni PL/I öğeleri eklendikçe root syntax tree bu modelleri
    /// taşımaya devam edecektir.
    /// </remarks>
    public sealed class Pl1SyntaxTree : SyntaxTree
    {
        public IReadOnlyList<Pl1Declaration> Declarations { get; }

        public IReadOnlyList<Pl1Statement> Statements { get; }

        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            SourceLocation location)
            : this(declarations, null, location)
        {
        }

        public Pl1SyntaxTree(
            IEnumerable<Pl1Declaration>? declarations,
            IEnumerable<Pl1Statement>? statements,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<Pl1Declaration>();
            Statements = statements?.ToList() ?? new List<Pl1Statement>();
        }
    }
}