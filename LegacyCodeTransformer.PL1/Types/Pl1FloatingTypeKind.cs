namespace LegacyCodeTransformer.Pl1.Types;

/// <summary>
/// PL/I floating point tip ailesindeki ana tip kategorisini temsil eder.
///
/// Neden var?
/// ----------------------
/// FLOAT, REAL ve DOUBLE / DOUBLE PRECISION ifadeleri aynı genel floating point
/// ailesine ait olsa da semantic olarak ayrı tip niyetleri taşır.
///
/// Ne çözüyor?
/// ----------------------
/// Floating point declaration bilgisini yalnızca raw keyword olarak değil,
/// güçlü tipli semantic kategori olarak modellemeyi sağlar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - FLOAT
/// - REAL
/// - DOUBLE
/// - DOUBLE PRECISION
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1FloatingType modelinde
/// - Parser floating type üretiminde
/// - Transpiler diagnostic üretiminde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// FLOAT / REAL / DOUBLE için EGL veya farklı hedef dil mapping kararları
/// bu enum üzerinden ayrıştırılabilir.
/// </summary>
public enum Pl1FloatingTypeKind
{
    Float = 0,
    Real = 1,
    DoublePrecision = 2
}