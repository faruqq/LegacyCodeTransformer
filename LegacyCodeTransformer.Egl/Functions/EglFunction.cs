using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Statements;

namespace LegacyCodeTransformer.Egl.Functions
{
    /// <summary>
    /// EGL function syntax modelini temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I procedure modellerinin EGL hedef dilinde function olarak
    /// temsil edilmesi gerekir. Function bilgisi top-level executable
    /// statement gibi taşınmamalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL function adını, kaynak sırasıyla korunan parameter listesini
    /// ve function body statement listesini güçlü tipli bir syntax
    /// modelinde taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Parametresiz function:
    ///
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    /// end
    ///
    /// Parameter modeli taşıyan function:
    ///
    /// CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);
    ///     DCL PROCESS_TEXT CHAR(50);
    /// END CUSTOMER_PROCESS;
    ///
    /// Bu aşamada parameter bilgisi syntax modelinde korunur; generator
    /// output'u ve direction bilgisi henüz üretilmez.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Pl1ToEglTranspiler tarafından Pl1Procedure dönüşüm çıktısı olarak
    /// oluşturulur ve EglSyntaxTree.Functions koleksiyonunda taşınır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Parameter type mapping, parameter direction, local declaration,
    /// return type ve function-level metadata ihtiyaçlarına temel olur.
    /// </summary>
    public sealed class EglFunction : SyntaxNode
    {
        public string Name { get; }

        public IReadOnlyList<EglFunctionParameter> Parameters { get; }

        public IReadOnlyList<EglStatement> Statements { get; }

        public EglFunction(
            string name,
            IEnumerable<EglStatement>? statements,
            SourceLocation location,
            IEnumerable<EglFunctionParameter>? parameters = null)
            : base(location)
        {
            Name = name;

            Parameters = parameters?.ToList() ??
                new List<EglFunctionParameter>();

            Statements = statements?.ToList() ??
                new List<EglStatement>();
        }
    }
}