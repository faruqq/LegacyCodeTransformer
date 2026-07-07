using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Statements
{
    /// <summary>
    /// EGL IF statement modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.10 kapsamında PL/I IF statement modellerinin EGL syntax tree üzerinde
    /// güçlü tipli statement modeli olarak taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// IF condition, THEN statement ve optional ELSE statement alanlarını EGL
    /// statement modeli üzerinde taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    ///     IF A = B THEN CALL PROC1;
    ///     IF A = B THEN CALL PROC1; ELSE CALL PROC2;
    ///
    /// EGL model:
    ///
    ///     Condition: A = B
    ///     ThenStatement: EglCallStatement
    ///     ElseStatement: EglCallStatement?
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// StatementTranspiler içinde Pl1IfStatement dönüşüm çıktısı olarak ve
    /// EglCodeGenerator içinde IF output üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Full expression parser, condition operator mapping ve DO block output desteği
    /// geldiğinde bu model genişletilmeden kullanılmaya devam edebilir.
    /// </summary>
    public sealed class EglIfStatement : EglStatement
    {
        public string Condition { get; }

        public EglStatement ThenStatement { get; }

        public EglStatement? ElseStatement { get; }

        public EglIfStatement(
            string condition,
            EglStatement thenStatement,
            EglStatement? elseStatement,
            SourceLocation location)
            : base(location)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }
    }
}