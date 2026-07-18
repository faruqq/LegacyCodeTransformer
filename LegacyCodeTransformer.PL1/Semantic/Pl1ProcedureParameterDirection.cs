namespace LegacyCodeTransformer.Pl1.Semantic
{
    /// <summary>
    /// PL/I procedure parametresinin semantic kullanım yönünü temsil
    /// eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Procedure header ve parameter declaration bilgisi tek başına
    /// parameter yönünü belirlemek için yeterli değildir. Procedure body
    /// içindeki okuma ve yazma kullanımları da analiz edilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Güvenli biçimde analiz edilebilen parameter kullanımlarını In,
    /// Out veya InOut olarak sınıflandırır. Kesin karar verilemeyen
    /// kullanımları Unknown olarak korur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// In:
    ///
    /// ERROR_TEXT = PROCESS_TEXT;
    ///
    /// Out:
    ///
    /// PROCESS_TEXT = ERROR_TEXT;
    ///
    /// InOut:
    ///
    /// TARGET_TEXT = PROCESS_TEXT;
    /// PROCESS_TEXT = SOURCE_TEXT;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ProcedureParameterDirectionAnalyzer tarafından üretilir ve
    /// Pl1ProcedureParameterBinding üzerinde taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL function parameter in, out ve inOut direction mapping
    /// davranışına temel olur.
    /// </summary>
    public enum Pl1ProcedureParameterDirection
    {
        Unknown,
        In,
        Out,
        InOut
    }
}