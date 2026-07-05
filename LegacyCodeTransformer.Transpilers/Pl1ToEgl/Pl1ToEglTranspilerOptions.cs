using LegacyCodeTransformer.Transpilers.Naming;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl;

/// <summary>
/// PL/I → EGL transpiler davranışını yöneten seçenekleri temsil eder.
///
/// Neden var?
/// ----------------------
/// Transpiler davranışı yalnızca identifier naming strategy ile sınırlı değildir.
/// Record type üretimi gibi hedef dile özgü kararlar da dışarıdan yönetilebilir
/// olmalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// NamingOptions ve RecordTypeStrategy ayarlarını tek options modeli altında toplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - Varsayılan: PascalCase + basicRecord
/// - Custom: CamelCase + basicRecord
/// - Custom: PascalCase + sqlRecord
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1ToEglTranspiler constructor içinde
/// - ConversionService overload içinde
/// - Unit testlerde record type davranışını doğrularken
///
/// Gelecekte neye temel olur?
/// ----------------------
/// sqlRecord metadata, default value üretimi, diagnostic seviyesi veya farklı EGL
/// generator ayarları gerektiğinde bu model genişletilecektir.
/// </summary>
public sealed class Pl1ToEglTranspilerOptions
{
    public static Pl1ToEglTranspilerOptions Default { get; } = new(
        IdentifierNamingOptions.Default,
        EglRecordTypeStrategy.BasicRecord);

    public IdentifierNamingOptions NamingOptions { get; }

    public EglRecordTypeStrategy RecordTypeStrategy { get; }

    public Pl1ToEglTranspilerOptions(
        IdentifierNamingOptions? namingOptions = null,
        EglRecordTypeStrategy recordTypeStrategy = EglRecordTypeStrategy.BasicRecord)
    {
        NamingOptions = namingOptions ?? IdentifierNamingOptions.Default;
        RecordTypeStrategy = recordTypeStrategy;
    }
}