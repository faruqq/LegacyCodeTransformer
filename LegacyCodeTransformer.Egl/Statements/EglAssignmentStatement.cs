using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Expressions;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL assignment statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.8 kapsamında PL/I assignment statement modellerinin EGL syntax tree üzerinde
    /// güçlü tipli statement modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Assignment target ve value expression alanlarını final string output yerine EGL
    /// syntax modeli olarak temsil eder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     PARAM = 'ABC';
    ///
    /// EGL model karşılığı:
    ///
    ///     Target: Param
    ///     Value: "ABC"
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1AssignmentStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde assignment output üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Çoklu assignment, compound assignment veya expression tree tabanlı assignment
    /// üretimleri bu model üzerinden genişletilecektir.
    /// </summary>
    public sealed class EglAssignmentStatement : EglStatement
    {
        public EglExpression Target { get; }

        public EglExpression Value { get; }

        public EglAssignmentStatement(
            EglExpression target,
            EglExpression value,
            SourceLocation location)
            : base(location)
        {
            Target = target;
            Value = value;
        }
    }
}