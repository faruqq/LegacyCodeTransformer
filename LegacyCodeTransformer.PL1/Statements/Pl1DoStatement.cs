using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I DO, DO WHILE ve DO UNTIL statement modelidir.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// PL/I DO ailesini executable statement olarak temsil etmek için vardır.
    ///
    /// Ne çözüyor?
    /// Basit DO block ile koşullu DO WHILE / DO UNTIL yapılarını aynı model
    /// altında ama Kind ayrımıyla taşır.
    ///
    /// Hangi örneği destekliyor?
    /// DO;
    ///     A = B;
    /// END;
    ///
    /// DO WHILE(SQLCODE = 0);
    ///     CALL FETCH_CURSOR;
    /// END;
    ///
    /// DO UNTIL(SAHA1 = 'ABC');
    ///     SAHA1 = 'ABC';
    /// END;
    ///
    /// Nerede kullanılır?
    /// Statement parser DO keyword’ünü gördüğünde, DO formuna göre bu modeli üretir.
    ///
    /// Gelecekte neye temel olur?
    /// EGL while/block üretimi, nested statement traversal, loop semantic analysis
    /// ve ileride sayaçlı DO desteğine temel olur.
    /// </remarks>
    public sealed class Pl1DoStatement : Pl1Statement
    {
        public Pl1DoStatementKind Kind { get; }

        public Pl1Expression? Condition { get; }

        public Pl1BlockStatement Body { get; }

        public Pl1DoStatement(
            Pl1DoStatementKind kind,
            Pl1Expression? condition,
            Pl1BlockStatement body,
            SourceLocation location)
            : base(location)
        {
            Kind = kind;
            Condition = condition;
            Body = body;
        }
    }
}