using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I executable statement modellerinin ortak temel sınıfıdır.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// Declaration dışındaki PL/I executable statement modellerini ortak bir
    /// syntax node ailesi altında toplamak için vardır.
    ///
    /// Ne çözüyor?
    /// Assignment, CALL, IF, DO ve block statement gibi farklı statement
    /// türlerinin parser, visitor, walker ve ileride transpiler katmanında
    /// ortak bir tip üzerinden taşınmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// A = B;
    /// CALL FETCH_CURSOR;
    /// IF SQLCODE = 0 THEN DO;
    ///     CALL FETCH_CURSOR;
    /// END;
    ///
    /// Nerede kullanılır?
    /// P05 statement parser çıktılarında, procedure body modelinde ve syntax
    /// tree içindeki executable statement listelerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Statement visitor/walker genişletmeleri, semantic statement analyzer,
    /// control-flow modeli ve EGL statement generator katmanları için temel olur.
    /// </remarks>
    public abstract class Pl1Statement : SyntaxNode
    {
        protected Pl1Statement(SourceLocation location)
            : base(location)
        {
        }
    }
}