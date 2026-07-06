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
    ///     EglAssignmentStatement
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
        private readonly ExpressionTranspiler _expressionTranspiler;

        public StatementTranspiler(
            DiagnosticBag diagnostics,
            IdentifierNamingStyle namingStyle)
        {
            _diagnostics = diagnostics;
            _expressionTranspiler = new ExpressionTranspiler(
                diagnostics,
                namingStyle);
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
        /// Assignment target ve value expression modellerini ExpressionTranspiler üzerinden
        /// EGL expression modellerine dönüştürür ve EglAssignmentStatement üretir.
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
        /// Çoklu assignment veya gerçek expression tree desteği geldiğinde bu method
        /// genişletilecektir.
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

            var eglTarget = _expressionTranspiler.TranspileExpression(target);
            var eglValue = _expressionTranspiler.TranspileExpression(statement.Value);

            if (eglTarget is null || eglValue is null)
            {
                return null;
            }

            return new EglAssignmentStatement(
                eglTarget,
                eglValue,
                statement.Location);
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