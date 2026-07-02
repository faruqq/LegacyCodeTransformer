using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;

namespace LegacyCodeTransformer.Pl1.Syntax
{
    /// <summary>
    /// PL/I kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser, PL/I kaynak kodunu okuduktan sonra string veya satır listesi
    /// yerine bu modeli üretecektir.
    ///
    /// Bu sınıf PL/I tarafındaki kök modeldir.
    /// İçerisinde PL/I declaration, statement ve ileride eklenecek diğer
    /// dil öğeleri tutulacaktır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - PL/I Parser çıktısında
    /// - PL/I Normalizer giriş ve çıkışında
    /// - PL/I → EGL Transpiler girişinde
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// İlk sürümde yalnızca declaration listesi taşır.
    /// İleride statement, procedure, include, embedded SQL gibi PL/I öğeleri
    /// bu model üzerinden genişletilecektir.
    /// </summary>
    public sealed class Pl1SyntaxTree : SyntaxTree
    {
        /// <summary>
        /// PL/I kaynak kodunda tanımlanan değişken declaration listesidir.
        /// </summary>
        public IReadOnlyList<Pl1Declaration> Declarations { get; }

        public Pl1SyntaxTree(
            IEnumerable<Pl1VariableDeclaration>? declarations,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<Pl1VariableDeclaration>();
        }
    }
}
