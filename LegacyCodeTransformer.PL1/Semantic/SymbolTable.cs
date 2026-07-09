namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I semantic symbol table modelidir.
    ///
    /// Neden var?
    /// ----------------------
    /// Semantic analyzer tarafından bulunan sembollerin tek bir sonuç modeliyle
    /// taşınması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Global declaration sembollerini isim bazlı, okunabilir bir dictionary olarak
    /// saklar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL MUST_NO FIXED DECIMAL(8);
    /// DCL CUSTOMER_NO FIXED DECIMAL(8);
    ///
    /// Bu input sonunda MUST_NO ve CUSTOMER_NO sembolleri SymbolTable içinde yer alır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// SemanticResult içinde analyzer çıktısı olarak kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Duplicate declaration, undefined identifier ve reference analysis adımları bu
    /// sembol tablosu üzerinden ilerleyecektir.
    /// </summary>
    public sealed class SymbolTable
    {
        public IReadOnlyDictionary<string, Symbol> Symbols { get; }

        public SymbolTable(IEnumerable<Symbol>? symbols = null)
        {
            Symbols = (symbols ?? Array.Empty<Symbol>())
                .ToDictionary(
                    symbol => symbol.Name,
                    symbol => symbol,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}