using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I expression modellerinin ortak temel sınıfıdır.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// Statement modellerinin koşul, hedef, değer ve argüman gibi expression
    /// parçalarını ortak bir model ailesi altında taşıması için vardır.
    ///
    /// Ne çözüyor?
    /// İlk P05 aşamasında expression parser henüz detaylandırılmadan,
    /// statement modellerinin expression bağımlılıklarını doğru bir syntax
    /// sınırıyla temsil etmesini sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// SQLCODE = 0
    /// B + C
    /// 'SELECT GLAU_HISTORY'
    /// DCLGLAU.BRM_KOD
    ///
    /// Nerede kullanılır?
    /// Assignment target/value, IF condition, DO WHILE/UNTIL condition ve CALL
    /// argument listelerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// Identifier expression, literal expression, binary expression, function call
    /// expression ve qualified member access modellerine temel olur.
    /// </remarks>
    public abstract class Pl1Expression : SyntaxNode
    {
        protected Pl1Expression(SourceLocation location)
            : base(location)
        {
        }
    }
}