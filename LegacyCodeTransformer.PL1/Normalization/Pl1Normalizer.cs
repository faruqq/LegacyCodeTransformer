using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Normalization
{
    /// <summary>
    /// Parser tarafından üretilen Pl1SyntaxTree modelini standart hale getirir.
    ///
    /// Neden var?
    /// ----------------------
    /// Legacy PL/I kodlarında aynı anlama gelen farklı yazım biçimleri bulunabilir.
    /// Normalizer, bu farklılıkları Transpiler aşamasına gitmeden önce mümkün olduğunca
    /// ortak bir forma indirgemek için oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parser sonrasında
    /// - Transpiler öncesinde
    /// - Application pipeline içerisinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İleride aşağıdaki işlemler bu sınıf üzerinden yapılabilir:
    ///
    /// - Identifier standardizasyonu
    /// - Gereksiz parantez sadeleştirme
    /// - Varsayılan veri tipi değerlerini netleştirme
    /// - PL/I yazım varyasyonlarını tek forma indirme
    ///
    /// İlk sürümde bilinçli olarak sade tutulmuştur.
    /// Şimdilik gelen Pl1SyntaxTree modelini değiştirmeden döndürür.
    /// </summary>
    public sealed class Pl1Normalizer
    {
        private readonly DiagnosticBag _diagnostics = new();

        /// <summary>
        /// Verilen Pl1SyntaxTree modelini normalize eder.
        /// </summary>
        public Pl1NormalizationResult Normalize(Pl1SyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                _diagnostics.AddError("Normalize edilecek Pl1SyntaxTree null olamaz.");

                return new Pl1NormalizationResult(
                    null,
                    _diagnostics.Diagnostics);
            }

            return new Pl1NormalizationResult(
                syntaxTree,
                _diagnostics.Diagnostics);
        }
    }
}
