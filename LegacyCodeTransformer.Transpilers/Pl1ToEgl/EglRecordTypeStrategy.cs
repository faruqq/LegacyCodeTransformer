namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl;

/// <summary>
/// PL/I structure declaration ifadelerinin EGL tarafında hangi record type ile üretileceğini belirler.
///
/// Neden var?
/// ----------------------
/// PL/I structure declaration her zaman veritabanı tablosu anlamına gelmez. Bu yüzden
/// basicRecord ve sqlRecord üretimi ayrı bir strateji ile yönetilmelidir.
///
/// Ne çözüyor?
/// ----------------------
/// Transpiler içinde record type değerinin hardcoded basicRecord kalmasını engeller
/// ve gerektiğinde sqlRecord üretimini kontrollü şekilde açar.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - BasicRecord strategy => record Musteri type basicRecord
/// - SqlRecord strategy => record Musteri type sqlRecord
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1ToEglTranspilerOptions içinde
/// - Pl1ToEglTranspiler.TranspileStructureDeclaration içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Tablo adı, column metadata, sqlDataCodeVariables veya farklı EGL record type
/// seçenekleri eklendiğinde record üretim stratejisinin merkezi enum modeli olur.
/// </summary>
public enum EglRecordTypeStrategy
{
    BasicRecord = 0,
    SqlRecord = 1
}