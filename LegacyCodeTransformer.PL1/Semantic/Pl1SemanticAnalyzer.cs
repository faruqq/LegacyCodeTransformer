using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I syntax tree üzerinde semantic analysis yapacak analyzer sınıfıdır.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser yalnızca syntax model üretir. Duplicate declaration, undefined
    /// identifier, type mismatch veya scope gibi anlam kontrolleri parser'ın
    /// sorumluluğu değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// PL/I syntax tree ile transpiler arasına semantic analysis katmanını yerleştirir.
    /// P09.1 kapsamında henüz semantic kural çalıştırmaz; yalnızca pipeline
    /// foundation sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL CUSTOMER_NO FIXED DECIMAL(10);
    /// CUSTOMER_NO = 12345;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// ConversionService pipeline içinde normalizer sonrasında çağrılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Symbol table foundation, duplicate declaration diagnostics, basic reference
    /// analysis ve type resolution adımları bu sınıf üzerinden geliştirilecektir.
    /// </summary>
    public sealed class Pl1SemanticAnalyzer
    {
        public SemanticResult Analyze(Pl1SyntaxTree syntaxTree)
        {
            return new SemanticResult();
        }
    }
}