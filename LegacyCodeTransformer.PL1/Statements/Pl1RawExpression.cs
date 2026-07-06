using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// Henüz detaylı expression tree'ye ayrıştırılmamış PL/I expression metnini temsil eder.
    /// </summary>
    /// <remarks>
    /// Neden var?
    /// P05 statement modelleme aşamasında assignment, IF condition, DO condition ve CALL
    /// argument gibi alanların expression taşıması gerekir. Ancak expression parser henüz
    /// binary expression, literal expression, function call expression gibi alt modellere
    /// ayrıştırılmadığı için ilk güvenli temsil raw expression modelidir.
    ///
    /// Ne çözüyor?
    /// Statement parser'ın expression içeriğini kaybetmeden syntax tree'ye taşımasını sağlar.
    /// Böylece statement desteği expression parser tamamlanmadan başlatılabilir.
    ///
    /// Hangi örneği destekliyor?
    /// SQLCODE = 0
    /// C + B
    /// 'SELECT GLAU_HISTORY'
    /// DCLGLAU.BRM_KOD
    ///
    /// Nerede kullanılır?
    /// Pl1AssignmentStatement.Value, Pl1AssignmentStatement.Targets,
    /// Pl1IfStatement.Condition, Pl1DoStatement.Condition ve Pl1CallStatement.Arguments
    /// içinde kullanılabilir.
    ///
    /// Gelecekte neye temel olur?
    /// İleride expression parser eklendiğinde Pl1RawExpression güvenli fallback model
    /// olarak kalabilir veya kademeli şekilde Pl1IdentifierExpression, Pl1LiteralExpression,
    /// Pl1BinaryExpression ve Pl1FunctionCallExpression gibi modellere dönüştürülebilir.
    /// </remarks>
    public sealed class Pl1RawExpression : Pl1Expression
    {
        public string Text { get; }

        public Pl1RawExpression(string text, SourceLocation location)
            : base(location)
        {
            Text = text;
        }
    }
}