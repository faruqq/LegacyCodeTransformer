using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Types;

namespace LegacyCodeTransformer.Egl.Functions
{
    /// <summary>
    /// EGL function parametresini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure parametreleri EGL function modeline
    /// dönüştürülürken parameter adı, çözümlenen EGL veri tipi ve
    /// kullanım yönü birlikte korunmalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL function parameter adını, veri tipini ve in, out veya inOut
    /// direction bilgisini tek bir syntax modelinde toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// PL/I:
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    ///
    ///     ERROR_TEXT = PROCESS_TEXT;
    /// END CUSTOMER_PROCESS;
    ///
    /// EGL parameter modeli:
    ///
    /// Name = ProcessText
    /// DataType = EglCharacterType(50)
    /// Direction = In
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler tarafından oluşturulur ve
    /// EglFunction.Parameters koleksiyonunda taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL generator tarafından function parameter declaration
    /// üretilmesine temel olur.
    /// </summary>
    public sealed class EglFunctionParameter : SyntaxNode
    {
        public string Name { get; }

        public EglDataType DataType { get; }

        public EglFunctionParameterDirection Direction { get; }

        public EglFunctionParameter(
            string name,
            EglDataType dataType,
            SourceLocation location,
            EglFunctionParameterDirection direction =
                EglFunctionParameterDirection.Unknown)
            : base(location)
        {
            Name = name;
            DataType = dataType;
            Direction = direction;
        }
    }
}