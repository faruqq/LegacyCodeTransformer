using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Pl1.Declarations
{
    /// <summary>
    /// PL/I değişken tanımını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kodunda DCL ifadesiyle tanımlanan değişkenlerin syntax tree
    /// içerisinde tip güvenli şekilde tutulması için oluşturulmuştur.
    ///
    /// Örnek PL/I kodu:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Bu modelde:
    /// - Name: MUST_NO
    /// - DataType: Pl1FixedDecimalType
    ///
    /// olarak temsil edilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer içerisinde
    /// - PL/I → EGL Transpiler dönüşüm kurallarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İleride CHAR, BIT, BINARY, STRUCTURE, LIKE gibi PL/I declaration
    /// tipleri de aynı model üzerinden taşınabilir.
    /// </summary>
    public sealed class Pl1VariableDeclaration : SyntaxNode
    {
        /// <summary>
        /// PL/I değişken adıdır.
        /// Örneğin: MUST_NO
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// PL/I değişken tipidir.
        /// Örneğin: FIXED DECIMAL(8)
        /// </summary>
        public Pl1DataType DataType { get; }

        public Pl1VariableDeclaration(
            string name,
            Pl1DataType dataType,
            SourceLocation location)
            : base(location)
        {
            Name = name;
            DataType = dataType;
        }
    }
}
