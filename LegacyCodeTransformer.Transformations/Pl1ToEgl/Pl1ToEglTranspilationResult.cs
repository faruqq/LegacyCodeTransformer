using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Egl.Syntax;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
    /// PL/I → EGL transpilation işleminin sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler yalnızca EglSyntaxTree üretmez.
    /// Aynı zamanda desteklenmeyen PL/I yapıları için hata veya uyarı da
    /// üretebilir.
    ///
    /// Bu nedenle EglSyntaxTree ile Diagnostic listesini birlikte taşımak için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - Application pipeline içerisinde
    /// - Generator öncesi kontrol aşamasında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I kapsamı genişledikçe desteklenmeyen yapıların raporlanması,
    /// kısmi dönüşüm ve dönüşüm uyarıları bu sonuç modeli üzerinden yönetilecektir.
    /// </summary>
    public sealed class Pl1ToEglTranspilationResult
    {
        /// <summary>
        /// Transpiler tarafından üretilen EGL syntax tree modelidir.
        /// Hata durumunda null olabilir.
        /// </summary>
        public EglSyntaxTree? SyntaxTree { get; }

        /// <summary>
        /// Transpilation sırasında oluşan hata, uyarı veya bilgi mesajlarıdır.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Transpilation işleminin başarılı olup olmadığını gösterir.
        /// </summary>
        public bool Success => SyntaxTree is not null &&
                               !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public Pl1ToEglTranspilationResult(
            EglSyntaxTree? syntaxTree,
            IEnumerable<Diagnostic>? diagnostics = null)
        {
            SyntaxTree = syntaxTree;
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
        }
    }
}
