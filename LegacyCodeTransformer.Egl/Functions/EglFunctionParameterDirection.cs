namespace LegacyCodeTransformer.Egl.Functions
{
    /// <summary>
    /// EGL function parametresinin kullanım yönünü temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// EGL function parametreleri veri tipinin yanında in, out veya
    /// inOut yön bilgisi taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Semantic analyzer tarafından çözümlenen PL/I parameter yönünün
    /// EGL syntax modelinde güçlü tipli olarak korunmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// function CustomerProcess(
    ///     processText char(50) in)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// EglFunctionParameter modeli ve PL/I → EGL transpiler direction
    /// mapping davranışında kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL generator tarafından function parameter declaration
    /// üretilmesine temel olur.
    /// </summary>
    public enum EglFunctionParameterDirection
    {
        Unknown,
        In,
        Out,
        InOut
    }
}