using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Core.Diagnostics
{
    /// <summary>
    /// Dönüştürme sürecinde oluşan bilgi, uyarı veya hata mesajını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// LegacyCodeTransformer içerisindeki bütün katmanlar
    /// (Parser, Normalizer, Transpiler, Generator vb.)
    /// kullanıcıya ortak bir model üzerinden bilgi aktarabilmelidir.
    ///
    /// Bu sınıf, üretilen mesajın önem seviyesini,
    /// açıklamasını ve kaynak kod içerisindeki konumunu birlikte taşır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parser
    /// - Normalizer
    /// - Transpiler
    /// - Generator
    /// - Application
    /// - CLI
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İhtiyaç oluşursa aşağıdaki bilgiler eklenebilir.
    ///
    /// - DiagnosticCode
    /// - HelpLink
    /// - SuggestedFix
    ///
    /// Ancak ilk sürümde yalnızca Severity, Message ve Location
    /// yeterli görülmüştür.
    /// </summary>
    public sealed class Diagnostic
    {
        /// <summary>
        /// Mesajın önem seviyesidir.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Kullanıcıya gösterilecek açıklamadır.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Mesajın kaynak kod içerisindeki konumudur.
        /// </summary>
        public SourceLocation Location { get; }

        public Diagnostic(
            DiagnosticSeverity severity,
            string message,
            SourceLocation? location = null)
        {
            Severity = severity;
            Message = message;
            Location = location ?? SourceLocation.Unknown;
        }

        public override string ToString()
        {
            return $"{Severity}: {Message} ({Location})";
        }
    }
}
