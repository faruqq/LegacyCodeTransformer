using System.Text;
using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Egl.Statements;
using LegacyCodeTransformer.Pl1.Statements;
using LegacyCodeTransformer.Transpilers.Naming;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
    /// PL/I executable statement modellerini EGL statement modellerine dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Pl1ToEglTranspiler ana sınıfı declaration dönüşümlerini yönetmektedir.
    /// Statement dönüşümleri büyüdükçe bu sorumluluğun ayrı bir bileşene ayrılması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I statement modellerini hedef EGL statement modellerine dönüştürür.
    /// P05.8 kapsamında assignment statement mapping desteği eklenmiştir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///
    /// EGL syntax model:
    ///
    ///     EglAssignmentStatement(Target: Param, Value: "ABC")
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler.Transpile methodu içinde SyntaxTree.Statements listesi
    /// işlenirken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// CALL, IF ve DO EGL generation milestone'larında concrete EGL statement
    /// modelleri bu sınıf üzerinden üretilecektir.
    /// </summary>
    internal sealed class StatementTranspiler
    {
        private readonly DiagnosticBag _diagnostics;
        private readonly IdentifierNamingStyle _namingStyle;

        public StatementTranspiler(
            DiagnosticBag diagnostics,
            IdentifierNamingStyle namingStyle)
        {
            _diagnostics = diagnostics;
            _namingStyle = namingStyle;
        }

        /// <summary>
        /// PL/I statement modelini EGL statement modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler katmanının statement dönüşümü için tek bir entrypoint'e ihtiyacı vardır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Desteklenen PL/I statement türlerini EGL statement modeline dönüştürür.
        /// P05.8 kapsamında Pl1AssignmentStatement desteklenir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        ///     PARAM = 'ABC';
        ///
        /// Bu input EglAssignmentStatement modeline dönüşür.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ToEglTranspiler.Transpile içerisinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// P05.9 ile CALL, P05.10 ile IF ve P05.11 ile DO mapping bu method üzerinden
        /// genişletilecektir.
        /// </summary>
        public EglStatement? TranspileStatement(Pl1Statement statement)
        {
            return statement switch
            {
                Pl1AssignmentStatement assignmentStatement => TranspileAssignmentStatement(assignmentStatement),
                _ => TranspileUnsupportedStatement(statement)
            };
        }

        /// <summary>
        /// PL/I assignment statement modelini EGL assignment statement modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// P05.8 kapsamında statement pipeline'ın ilk concrete EGL mapping davranışı
        /// assignment statement için sağlanmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Assignment target ve value raw expression metinlerini EGL output standardına
        /// uygun hale getirir ve EglAssignmentStatement üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// PL/I:
        ///
        ///     PARAM = 'ABC';
        ///
        /// EGL model:
        ///
        ///     Target: Param
        ///     Value: "ABC"
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// TranspileStatement dispatch methodu içinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Full expression parser geldiğinde bu method expression model mapping'e
        /// taşınabilir.
        /// </summary>
        private EglStatement? TranspileAssignmentStatement(Pl1AssignmentStatement statement)
        {
            var target = statement.Targets.FirstOrDefault();

            if (target is null)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "PL/I assignment target bulunamadı.",
                    statement.Location));

                return null;
            }

            var targetText = TranspileExpressionText(target);
            var valueText = TranspileExpressionText(statement.Value);

            return new EglAssignmentStatement(
                targetText,
                valueText,
                statement.Location);
        }

        private string TranspileExpressionText(Pl1Expression expression)
        {
            return expression switch
            {
                Pl1RawExpression rawExpression => TransformRawExpressionText(rawExpression.Text),
                _ => CreateUnsupportedExpressionDiagnostic(expression)
            };
        }

        private string CreateUnsupportedExpressionDiagnostic(Pl1Expression expression)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I expression türü: {expression.GetType().Name}",
                expression.Location));

            return string.Empty;
        }

        private string TransformRawExpressionText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var index = 0;

            while (index < text.Length)
            {
                var current = text[index];

                if (current == '\'')
                {
                    index = AppendStringLiteral(text, index, builder);
                    continue;
                }

                if (IsIdentifierStart(current))
                {
                    index = AppendIdentifier(text, index, builder);
                    continue;
                }

                builder.Append(current);
                index++;
            }

            return builder.ToString();
        }

        private static int AppendStringLiteral(
            string text,
            int startIndex,
            StringBuilder builder)
        {
            builder.Append('"');

            var index = startIndex + 1;

            while (index < text.Length && text[index] != '\'')
            {
                var current = text[index];

                if (current == '"' || current == '\\')
                {
                    builder.Append('\\');
                }

                builder.Append(current);
                index++;
            }

            builder.Append('"');

            return index < text.Length
                ? index + 1
                : index;
        }

        private int AppendIdentifier(
            string text,
            int startIndex,
            StringBuilder builder)
        {
            var index = startIndex;

            while (index < text.Length && IsIdentifierPart(text[index]))
            {
                index++;
            }

            var identifier = text.Substring(
                startIndex,
                index - startIndex);

            builder.Append(
                IdentifierNameTransformer.Transform(
                    identifier,
                    _namingStyle));

            return index;
        }

        private static bool IsIdentifierStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        private static bool IsIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private EglStatement? TranspileUnsupportedStatement(Pl1Statement statement)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I statement türü: {statement.GetType().Name}",
                statement.Location));

            return null;
        }
    }
}