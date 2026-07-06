using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// Birden fazla PL/I statement içeren block statement modelidir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// DO ... END veya ileride procedure body gibi birden fazla executable
    /// statement içeren alanları tek statement modeli olarak temsil etmek için vardır.
    ///
    /// Ne çözüyor?
    /// IF THEN DO, ELSE DO, DO WHILE body ve nested block yapılarını ortak
    /// bir statement listesiyle taşır.
    ///
    /// Hangi örneği destekliyor?
    /// DO;
    ///     A = B;
    ///     B = D;
    /// END;
    ///
    /// IF SQLCODE = 100 THEN DO;
    ///     DCLGLAU.KUMULE_ALAC_TUT = 0;
    ///     DCLGLAU.KUMULE_BORC_TUT = 0;
    /// END;
    ///
    /// Nerede kullanılır?
    /// Pl1DoStatement.Body içinde, IF THEN/ELSE block temsilinde ve ileride
    /// procedure body modelinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Recursive syntax traversal, block scope analizi, nested control-flow modeli
    /// ve EGL block generation için temel olur.
    /// </remarks>
    public sealed class Pl1BlockStatement : Pl1Statement
    {
        public IReadOnlyList<Pl1Statement> Statements { get; }

        public Pl1BlockStatement(
            IEnumerable<Pl1Statement>? statements,
            SourceLocation location)
            : base(location)
        {
            Statements = statements?.ToList() ?? new List<Pl1Statement>();
        }
    }
}