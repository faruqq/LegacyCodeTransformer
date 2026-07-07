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
    /// P05.8 kapsamında assignment, P05.9 kapsamında CALL, P05.10 kapsamında IF,
    /// P05.11 kapsamında DO statement mapping desteği eklenmiştir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///     CALL FETCH_CURSOR;
    ///     IF A = B THEN CALL PROC1;
    ///     DO; CALL PROC2; END;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler.Transpile methodu içinde SyntaxTree.Statements listesi
    /// işlenirken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// SELECT, RETURN, EXEC SQL ve procedure body statement mapping çalışmaları bu
    /// sınıf üzerinden genişletilecektir.
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
        /// P05.11 kapsamında Pl1AssignmentStatement, Pl1CallStatement, Pl1IfStatement
        /// ve Pl1DoStatement desteklenir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        ///     PARAM = 'ABC';
        ///     CALL FETCH_CURSOR;
        ///     IF A = B THEN CALL PROC1;
        ///     DO; CALL PROC2; END;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// Pl1ToEglTranspiler.Transpile içerisinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// Yeni statement mapping türleri bu dispatch methodu üzerinden eklenecektir.
        /// </summary>
        public EglStatement? TranspileStatement(Pl1Statement statement)
        {
            return statement switch
            {
                Pl1AssignmentStatement assignmentStatement => TranspileAssignmentStatement(assignmentStatement),
                Pl1CallStatement callStatement => TranspileCallStatement(callStatement),
                Pl1IfStatement ifStatement => TranspileIfStatement(ifStatement),
                Pl1DoStatement doStatement => TranspileDoStatement(doStatement),
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

        private EglStatement? TranspileIfStatement(Pl1IfStatement statement)
        {
            var condition = TranspileExpressionText(statement.Condition);
            var thenStatement = TranspileStatement(statement.ThenStatement);

            if (thenStatement is null)
            {
                return null;
            }

            EglStatement? elseStatement = null;

            if (statement.ElseStatement is not null)
            {
                elseStatement = TranspileStatement(statement.ElseStatement);

                if (elseStatement is null)
                {
                    return null;
                }
            }

            return new EglIfStatement(
                condition,
                thenStatement,
                elseStatement,
                statement.Location);
        }

        /// <summary>
        /// PL/I DO statement modelini EGL DO statement modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// P05.11 kapsamında statement pipeline'ın dördüncü concrete EGL mapping
        /// davranışı DO statement için sağlanmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// DO kind bilgisini EGL DO kind modeline map eder, optional condition text
        /// değerini dönüştürür ve body içindeki child statement modellerini recursive
        /// olarak EGL statement modellerine dönüştürür.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// PL/I:
        ///
        ///     DO;
        ///         CALL PROC1;
        ///     END;
        ///
        ///     DO WHILE(SQLCODE = 0);
        ///         CALL FETCH_CURSOR;
        ///     END;
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// TranspileStatement dispatch methodu içinde kullanılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// IF THEN DO, ELSE DO ve nested DO output zinciri bu recursive mapping ile
        /// çalışacaktır.
        /// </summary>
        private EglStatement? TranspileDoStatement(Pl1DoStatement statement)
        {
            var kind = TranspileDoStatementKind(statement.Kind);
            var condition = statement.Condition is not null
                ? TranspileExpressionText(statement.Condition)
                : null;

            var statements = new List<EglStatement>();

            foreach (var childStatement in statement.Body.Statements)
            {
                var eglStatement = TranspileStatement(childStatement);

                if (eglStatement is null)
                {
                    return null;
                }

                statements.Add(eglStatement);
            }

            return new EglDoStatement(
                kind,
                condition,
                statements,
                statement.Location);
        }

        private static EglDoStatementKind TranspileDoStatementKind(Pl1DoStatementKind kind)
        {
            return kind switch
            {
                Pl1DoStatementKind.Block => EglDoStatementKind.Block,
                Pl1DoStatementKind.While => EglDoStatementKind.While,
                Pl1DoStatementKind.Until => EglDoStatementKind.Until,
                _ => EglDoStatementKind.Block
            };
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