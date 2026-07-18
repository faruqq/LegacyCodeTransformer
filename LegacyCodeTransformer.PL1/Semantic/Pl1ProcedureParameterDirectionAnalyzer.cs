using LegacyCodeTransformer.Pl1.Procedures;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I procedure body içindeki parameter kullanım yönünü analiz
    /// eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parameter direction yalnızca header veya declaration üzerinden
    /// belirlenemez. Assignment target, assignment value, condition ve
    /// nested statement kullanımları birlikte değerlendirilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Güvenli simple identifier expression kullanımlarında read ve
    /// write bilgisini toplar.
    ///
    /// Yalnızca read varsa In, yalnızca write varsa Out, ikisi birlikte
    /// varsa InOut üretir.
    ///
    /// Raw expression veya CALL argument kullanımı güvenli biçimde
    /// sınıflandırılamıyorsa Unknown üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// In:
    ///
    /// ERROR_TEXT = PROCESS_TEXT;
    ///
    /// Out:
    ///
    /// PROCESS_TEXT = ERROR_TEXT;
    ///
    /// InOut:
    ///
    /// TARGET_TEXT = PROCESS_TEXT;
    /// PROCESS_TEXT = SOURCE_TEXT;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer tarafından procedure parameter binding
    /// oluşturulurken kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Expression syntax modeli genişlediğinde binary expression,
    /// function invocation, array access ve qualified reference
    /// direction analizlerine temel olur.
    /// </summary>
    internal sealed class Pl1ProcedureParameterDirectionAnalyzer
    {
        public Pl1ProcedureParameterDirection Analyze(
            Pl1Procedure procedure,
            string parameterName)
        {
            var usage = new ParameterUsage();

            foreach (var statement in procedure.Statements)
            {
                AnalyzeStatement(
                    statement,
                    parameterName,
                    usage);
            }

            if (usage.IsUncertain)
            {
                return Pl1ProcedureParameterDirection.Unknown;
            }

            if (usage.IsRead && usage.IsWritten)
            {
                return Pl1ProcedureParameterDirection.InOut;
            }

            if (usage.IsRead)
            {
                return Pl1ProcedureParameterDirection.In;
            }

            if (usage.IsWritten)
            {
                return Pl1ProcedureParameterDirection.Out;
            }

            return Pl1ProcedureParameterDirection.Unknown;
        }

        private static void AnalyzeStatement(
            Pl1Statement statement,
            string parameterName,
            ParameterUsage usage)
        {
            switch (statement)
            {
                case Pl1AssignmentStatement assignmentStatement:
                    AnalyzeAssignment(
                        assignmentStatement,
                        parameterName,
                        usage);
                    break;

                case Pl1IfStatement ifStatement:
                    AnalyzeReadExpression(
                        ifStatement.Condition,
                        parameterName,
                        usage);

                    AnalyzeStatement(
                        ifStatement.ThenStatement,
                        parameterName,
                        usage);

                    if (ifStatement.ElseStatement is not null)
                    {
                        AnalyzeStatement(
                            ifStatement.ElseStatement,
                            parameterName,
                            usage);
                    }

                    break;

                case Pl1DoStatement doStatement:
                    if (doStatement.Condition is not null)
                    {
                        AnalyzeReadExpression(
                            doStatement.Condition,
                            parameterName,
                            usage);
                    }

                    AnalyzeStatement(
                        doStatement.Body,
                        parameterName,
                        usage);
                    break;

                case Pl1BlockStatement blockStatement:
                    foreach (var childStatement in
                        blockStatement.Statements)
                    {
                        AnalyzeStatement(
                            childStatement,
                            parameterName,
                            usage);
                    }

                    break;

                case Pl1CallStatement callStatement:
                    AnalyzeCall(
                        callStatement,
                        parameterName,
                        usage);
                    break;
            }
        }

        private static void AnalyzeAssignment(
            Pl1AssignmentStatement statement,
            string parameterName,
            ParameterUsage usage)
        {
            foreach (var target in statement.Targets)
            {
                AnalyzeWriteExpression(
                    target,
                    parameterName,
                    usage);
            }

            AnalyzeReadExpression(
                statement.Value,
                parameterName,
                usage);
        }

        private static void AnalyzeCall(
            Pl1CallStatement statement,
            string parameterName,
            ParameterUsage usage)
        {
            foreach (var argument in statement.Arguments)
            {
                if (ExpressionReferencesParameter(
                        argument,
                        parameterName))
                {
                    usage.IsUncertain = true;
                }
            }
        }

        private static void AnalyzeReadExpression(
            Pl1Expression expression,
            string parameterName,
            ParameterUsage usage)
        {
            if (IsExactParameterReference(
                    expression,
                    parameterName))
            {
                usage.IsRead = true;
                return;
            }

            if (ExpressionReferencesParameter(
                    expression,
                    parameterName))
            {
                usage.IsUncertain = true;
            }
        }

        private static void AnalyzeWriteExpression(
            Pl1Expression expression,
            string parameterName,
            ParameterUsage usage)
        {
            if (IsExactParameterReference(
                    expression,
                    parameterName))
            {
                usage.IsWritten = true;
                return;
            }

            if (ExpressionReferencesParameter(
                    expression,
                    parameterName))
            {
                usage.IsUncertain = true;
            }
        }

        private static bool IsExactParameterReference(
            Pl1Expression expression,
            string parameterName)
        {
            return expression is Pl1RawExpression rawExpression &&
                string.Equals(
                    rawExpression.Text.Trim(),
                    parameterName,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool ExpressionReferencesParameter(
            Pl1Expression expression,
            string parameterName)
        {
            if (expression is not Pl1RawExpression rawExpression)
            {
                return false;
            }

            var identifier = new List<char>();

            foreach (var character in rawExpression.Text)
            {
                if (char.IsLetterOrDigit(character) ||
                    character == '_')
                {
                    identifier.Add(character);
                    continue;
                }

                if (IdentifierMatches(
                        identifier,
                        parameterName))
                {
                    return true;
                }

                identifier.Clear();
            }

            return IdentifierMatches(
                identifier,
                parameterName);
        }

        private static bool IdentifierMatches(
            IReadOnlyCollection<char> identifier,
            string parameterName)
        {
            if (identifier.Count == 0)
            {
                return false;
            }

            return string.Equals(
                new string(identifier.ToArray()),
                parameterName,
                StringComparison.OrdinalIgnoreCase);
        }

        private sealed class ParameterUsage
        {
            public bool IsRead { get; set; }

            public bool IsWritten { get; set; }

            public bool IsUncertain { get; set; }
        }
    }
}