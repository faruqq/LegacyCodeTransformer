namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Helper parser sınıflarının parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// Parser helper sınıflarında CharacterTypeParseResult, NumericTypeParseResult,
/// StructureParseResult gibi çok sayıda küçük result modeli oluşmuştur.
/// Bu modellerin tamamı aynı iki bilgiyi taşır: parse edilen değer ve parse sonrası position.
///
/// Ne çözüyor?
/// ----------------------
/// Helper parser result modellerini tek generic yapı altında toplar.
/// Böylece gereksiz result class tekrarını azaltır ve parser altyapısını sadeleştirir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - HelperParseResult<Pl1DataType>
/// - HelperParseResult<Pl1InitialValue>
/// - HelperParseResult<int?>
/// - HelperParseResult<Pl1Declaration>
///
/// Nerede kullanılır?
/// ----------------------
/// - CharacterTypeParser
/// - BitTypeParser
/// - NumericTypeParser
/// - FloatingTypeParser
/// - DataTypeParser
/// - InitialValueParser
/// - DimensionParser
/// - DeclarationParser
/// - VariableDeclarationParser
/// - StructureParser
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement parser geliştirmelerinde statement, expression ve procedure parser
/// result modelleri aynı generic yapı ile temsil edilebilir.
/// </summary>
internal sealed class HelperParseResult<T>
{
    public T? Value { get; }

    public int Position { get; }

    public HelperParseResult(
        T? value,
        int position)
    {
        Value = value;
        Position = position;
    }
}