using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic diagnostic mesajlarını standartlaştıran factory sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Semantic analyzer büyüdükçe duplicate declaration, undefined identifier ve type
    /// mismatch gibi hata mesajlarının tek formatta üretilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Semantic diagnostic üretimini Pl1SemanticAnalyzer içine gömmek yerine ayrı ve
    /// test edilebilir bir yardımcı sınıfta toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL MUST_NO CHAR(8);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1SemanticAnalyzer duplicate declaration kontrolünde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Undefined identifier, type mismatch ve scope diagnostic'leri bu factory
    /// üzerinden standartlaştırılabilir.
    /// </summary>
    internal static class SemanticDiagnosticFactory
    {
        public static Diagnostic DuplicateDeclaration(
            string symbolName,
            SourceLocation location)
        {
            return new Diagnostic(
                DiagnosticSeverity.Error,
                $"Duplicate declaration bulundu: {symbolName}.",
                location);
        }
    }
}