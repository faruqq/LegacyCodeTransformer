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
    /// P05.8 kapsamında assignment, P05.9 kapsamında CALL statement mapping desteği
    /// eklenmiştir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///     CALL FETCH_CURSOR;
    ///     CALL PROC1(A, B);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler.Transpile methodu içinde SyntaxTree.Statements listesi
    /// işlenirken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// IF ve DO EGL generation milestone'larında concrete EGL statement modelleri
    /// bu sınıf üzerinden üretilecektir.
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
        /// P05.9 kapsamında Pl1AssignmentStatement ve Pl1CallStatement desteklenir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        ///     PARAM = 'ABC';
        ///     CALL FETCH_CURSOR;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ToEglTranspiler.Transpile içerisinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// P05.10 ile IF ve P05.11 ile DO mapping bu method üzerinden genişletilecektir.
        /// </summary>
        public EglStatement? TranspileStatement(Pl1Statement statement)
        {
            return statement switch
            {
                Pl1AssignmentStatement assignmentStatement => TranspileAssignmentStatement(assignmentStatement),
                Pl1CallStatement callStatement => TranspileCallStatement(callStatement),
                _ => TranspileUnsupportedStatement(statement)
            };
        }

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

        /// <summary>
        /// PL/I CALL statement modelini EGL CALL statement modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// P05.9 kapsamında statement pipeline'ın ikinci concrete EGL mapping davranışı
        /// CALL statement için sağlanmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Procedure adını identifier naming standardına göre dönüştürür, argument
        /// expression metinlerini EGL text standardına çevirir ve EglCallStatement üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// PL/I:
        ///
        ///     CALL FETCH_CURSOR;
        ///     CALL PROC1(A, 'ABC');
        ///
        /// EGL model:
        ///
        ///     ProcedureName: FetchCursor
        ///     Arguments: []
        ///
        ///     ProcedureName: Proc1
        ///     Arguments: [A, "ABC"]
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// TranspileStatement dispatch methodu içinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Full expression parser geldiğinde argument mapping expression model mapping'e
        /// taşınabilir.
        /// </summary>
        private EglStatement TranspileCallStatement(Pl1CallStatement statement)
        {
            var procedureName = IdentifierNameTransformer.Transform(
                statement.ProcedureName,
                _namingStyle);

            var arguments = statement.Arguments
                .Select(TranspileExpressionText)
                .ToList();

            return new EglCallStatement(
                procedureName,
                arguments,
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