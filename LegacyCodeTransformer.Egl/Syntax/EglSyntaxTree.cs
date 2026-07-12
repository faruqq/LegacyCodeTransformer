using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Functions;
using LegacyCodeTransformer.Egl.Statements;

namespace LegacyCodeTransformer.Egl.Syntax
{
    /// <summary>
    /// EGL kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I → EGL dönüşüm pipeline'ında transpiler doğrudan string
    /// üretmez. Bunun yerine EGL diline ait güçlü tipli bir syntax tree
    /// üretir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL declaration, function ve top-level executable statement
    /// modellerini tek root syntax tree altında taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Declaration:
    ///
    /// CustomerNo decimal(8);
    ///
    /// Function:
    ///
    /// function CustomerProcess()
    ///     CustomerNo = MustNo;
    /// end
    ///
    /// Top-level statement:
    ///
    /// CustomerProcess();
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// PL/I → EGL transpiler çıktısında, EglCodeGenerator girişinde ve
    /// EGL model testlerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL program, service, library, function, record, variable ve
    /// statement modelleri aynı syntax tree kökü üzerinden
    /// taşınabilecektir.
    /// </summary>
    public sealed class EglSyntaxTree : SyntaxTree
    {
        public IReadOnlyList<EglDeclaration> Declarations { get; }

        public IReadOnlyList<EglFunction> Functions { get; }

        public IReadOnlyList<EglStatement> Statements { get; }

        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            SourceLocation location)
            : this(
                declarations,
                functions: null,
                statements: null,
                location)
        {
        }

        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            IEnumerable<EglStatement>? statements,
            SourceLocation location)
            : this(
                declarations,
                functions: null,
                statements,
                location)
        {
        }

        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            IEnumerable<EglFunction>? functions,
            IEnumerable<EglStatement>? statements,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ??
                new List<EglDeclaration>();

            Functions = functions?.ToList() ??
                new List<EglFunction>();

            Statements = statements?.ToList() ??
                new List<EglStatement>();
        }
    }
}