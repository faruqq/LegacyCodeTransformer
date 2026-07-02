using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Core.Results
{
    /// <summary>
    /// Parser çalışmasının sonucunu temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser yalnızca başarılı bir SyntaxTree üretmez.
    /// Aynı zamanda hata, uyarı veya bilgilendirme mesajları da üretebilir.
    /// Bu sınıf, üretilen SyntaxTree ile Diagnostic listesini birlikte taşımak
    /// için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - Gelecekte eklenecek diğer dil parser'larında
    /// - Application pipeline içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// Parser sonucunun standart hale gelmesini sağlar.
    /// İleride farklı diller için parser yazıldığında aynı sonuç modeli
    /// kullanılabilir.
    ///
    /// Örneğin:
    /// - ParseResult<Pl1SyntaxTree>
    /// - ParseResult<EglSyntaxTree>
    /// - ParseResult<CSharpSyntaxTree>
    /// </summary>
    /// <typeparam name="TSyntaxTree">
    /// Parser tarafından üretilen dil bazlı SyntaxTree tipidir.
    /// </typeparam>
    public sealed class ParseResult<TSyntaxTree>
        where TSyntaxTree : SyntaxTree
    {
        /// <summary>
        /// Parser tarafından üretilen SyntaxTree modelidir.
        /// Hata durumunda null olabilir.
        /// </summary>
        public TSyntaxTree? SyntaxTree { get; }

        /// <summary>
        /// Parser tarafından üretilen hata, uyarı veya bilgi mesajlarıdır.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Parse işleminin başarılı olup olmadığını gösterir.
        /// SyntaxTree varsa ve Error seviyesinde diagnostic yoksa true döner.
        /// </summary>
        public bool Success => SyntaxTree is not null &&
                               !Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);

        public ParseResult(
            TSyntaxTree? syntaxTree,
            IEnumerable<Diagnostic>? diagnostics = null)
        {
            SyntaxTree = syntaxTree;
            Diagnostics = diagnostics?.ToList() ?? new List<Diagnostic>();
        }
    }
}
