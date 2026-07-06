using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Egl.Expressions
{
    /// <summary>
    /// Henüz detaylı expression tree'ye ayrıştırılmamış EGL expression metnini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.8 aşamasında PL/I expression parser henüz tam semantic expression tree
    /// üretmemektedir. Buna rağmen assignment, CALL, IF ve DO dönüşümlerinde expression
    /// içeriğinin kaybolmadan EGL syntax tree üzerinde taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Hedef EGL expression metnini geçici ama güçlü tipli bir syntax node olarak taşır.
    /// Transpiler doğrudan final EGL source string üretmez; raw expression bile olsa
    /// bunu EGL syntax modeli olarak taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     param
    ///     "ABC"
    ///     sqlCode == 0
    ///     dclglau.brmKod
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EglAssignmentStatement target/value alanlarında ve ileride CALL argument,
    /// IF condition ve DO condition alanlarında kullanılacaktır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek EGL expression modelleri eklendiğinde EglRawExpression güvenli fallback
    /// olarak kalabilir veya kademeli şekilde daha spesifik expression modelleriyle
    /// değiştirilebilir.
    /// </summary>
    public sealed class EglRawExpression : EglExpression
    {
        public string Text { get; }

        public EglRawExpression(
            string text,
            SourceLocation location)
            : base(location)
        {
            Text = text;
        }
    }
}