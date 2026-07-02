
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;

namespace LegacyCodeTransformer.Egl.Syntax
{
    /// <summary>
    /// EGL kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler, PL/I modelini doğrudan string EGL koduna çevirmeyecektir.
    /// Bunun yerine önce EglSyntaxTree üretecektir.
    ///
    /// Bu sınıf, EGL tarafındaki kök modeldir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I → EGL Transpiler çıktısında
    /// - EGL Code Generator girişinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İlk sürümde yalnızca değişken declaration listesini taşır.
    /// İleride function, record, service, statement ve expression yapıları
    /// bu model üzerinden genişletilecektir.
    /// </summary>
    public sealed class EglSyntaxTree : SyntaxTree
    {
        /// <summary>
        /// EGL kaynak kodunda üretilecek değişken declaration listesidir.
        /// </summary>
        public IReadOnlyList<EglDeclaration> Declarations { get; }

        public EglSyntaxTree(
            IEnumerable<EglVariableDeclaration>? declarations,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<EglVariableDeclaration>();
        }
    }
}
