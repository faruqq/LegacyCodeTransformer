using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I IF THEN ELSE statement modelidir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// PL/I karar yapılarını syntax seviyesinde temsil etmek için vardır.
    ///
    /// Ne çözüyor?
    /// Tek satırlı THEN statement, DO block içeren THEN statement ve optional
    /// ELSE statement akışını aynı model altında taşır.
    ///
    /// Hangi örneği destekliyor?
    /// IF SQLCODE = 100 THEN DO;
    ///     A = 0;
    /// END;
    ///
    /// IF A > B THEN A = B;
    /// ELSE A = C;
    ///
    /// Nerede kullanılır?
    /// Statement parser IF ... THEN ... ELSE ... yapısını çözdüğünde bu modeli üretir.
    ///
    /// Gelecekte neye temel olur?
    /// EGL if/else üretimi, condition semantic analysis, nested IF traversal ve
    /// control-flow graph üretimi için temel olur.
    /// </remarks>
    public sealed class Pl1IfStatement : Pl1Statement
    {
        public Pl1Expression Condition { get; }

        public Pl1Statement ThenStatement { get; }

        public Pl1Statement? ElseStatement { get; }

        public Pl1IfStatement(
            Pl1Expression condition,
            Pl1Statement thenStatement,
            Pl1Statement? elseStatement,
            SourceLocation location)
            : base(location)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }
    }
}