namespace LegacyCodeTransformer.Pl1.Types;

/// <summary>
/// PL/I floating point tipinin decimal veya binary taban bilgisini temsil eder.
///
/// Neden var?
/// ----------------------
/// PL/I FLOAT tipi DECIMAL veya BINARY tabanla yazılabilir. Bazı örneklerde
/// taban açıkça belirtilmez.
///
/// Ne çözüyor?
/// ----------------------
/// FLOAT DECIMAL ve FLOAT BINARY ayrımını parser modelinde kaybetmeden
/// taşımayı sağlar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - FLOAT
/// - FLOAT DECIMAL
/// - FLOAT DECIMAL(16)
/// - FLOAT BINARY
/// - FLOAT BIN(53)
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1FloatingType modelinde
/// - ParseFloatingType methodunda
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Decimal floating ve binary floating tipler için farklı hedef dil mapping
/// kararları gerektiğinde semantic ayrım noktası olur.
/// </summary>
public enum Pl1FloatingBase
{
    Unspecified = 0,
    Decimal = 1,
    Binary = 2
}