using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Egl.Expressions;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Transpilers.Naming;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
    /// PL/I expression modellerini EGL expression modellerine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// StatementTranspiler assignment, CALL, IF ve DO dönüşümlerinde expression alanlarıyla
    /// çalışacaktır. Bu expression dönüşüm kararı statement parser veya generator içine
    /// dağılmamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// P05.8 aşamasında Pl1RawExpression modellerini EglRawExpression modellerine dönüştürür.
    /// Identifier naming ve string literal dönüşümü bu katmanda yapılır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     Pl1RawExpression("PARAM") -> EglRawExpression("Param")
    ///     Pl1RawExpression("'ABC'") -> EglRawExpression("\"ABC\"")
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde assignment target/value expression dönüşümünde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Pl1IdentifierExpression, Pl1LiteralExpression, Pl1BinaryExpression gibi modeller
    /// eklendiğinde bu transpiler gerçek expression mapping davranışına genişletilecektir.
    /// </summary>
    internal sealed class ExpressionTranspiler
    {
        private readonly DiagnosticBag _diagnostics;
        private readonly EglRawExpressionTextTransformer _rawExpressionTextTransformer;

        public ExpressionTranspiler(
            DiagnosticBag diagnostics,
            IdentifierNamingStyle namingStyle)
        {
            _diagnostics = diagnostics;
            _rawExpressionTextTransformer = new EglRawExpressionTextTransformer(namingStyle);
        }

        /// <summary>
        /// PL/I expression modelini EGL expression modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Statement transpiler doğrudan expression concrete type detayını bilmemelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Desteklenen expression türlerini EGL expression modeline dönüştürür, desteklenmeyen
        /// expression türleri için diagnostic üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// P05.8 aşamasında Pl1RawExpression desteklenir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// StatementTranspiler.TranspileAssignmentStatement içinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// CALL argument, IF condition ve DO condition mapping bu method üzerinden
        /// ilerleyecektir.
        /// </summary>
        public EglExpression? TranspileExpression(Pl1Expression expression)
        {
            return expression switch
            {
                Pl1RawExpression rawExpression => TranspileRawExpression(rawExpression),
                _ => TranspileUnsupportedExpression(expression)
            };
        }

        private EglExpression TranspileRawExpression(Pl1RawExpression expression)
        {
            var text = _rawExpressionTextTransformer.Transform(expression.Text);

            return new EglRawExpression(
                text,
                expression.Location);
        }

        private EglExpression? TranspileUnsupportedExpression(Pl1Expression expression)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I expression türü: {expression.GetType().Name}",
                expression.Location));

            return null;
        }
    }
}