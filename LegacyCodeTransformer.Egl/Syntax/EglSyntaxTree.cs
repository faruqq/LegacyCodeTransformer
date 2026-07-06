using System.Collections.Generic;
using System.Linq;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Statements;

namespace LegacyCodeTransformer.Egl.Syntax
{
    /// <summary>
    /// EGL kaynak kodunun syntax tree karşılığını temsil eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I → EGL dönüşüm pipeline'ında Transpiler aşaması doğrudan string üretmez.
    /// Bunun yerine EGL diline ait güçlü tipli bir syntax tree üretir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EGL declaration ve executable statement modellerini tek root syntax tree altında
    /// taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Declaration örneği:
    ///
    ///     Param char(8);
    ///
    /// Statement örneği:
    ///
    ///     param = "ABC";
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// PL/I → EGL Transpiler çıktısında, EGL Code Generator girişinde ve unit testlerde
    /// kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// EGL program, service, library, function, record, variable ve statement modelleri
    /// aynı syntax tree kökü üzerinden taşınabilecektir.
    /// </summary>
    public sealed class EglSyntaxTree : SyntaxTree
    {
        public IReadOnlyList<EglDeclaration> Declarations { get; }

        public IReadOnlyList<EglStatement> Statements { get; }

        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            SourceLocation location)
            : this(declarations, null, location)
        {
        }

        public EglSyntaxTree(
            IEnumerable<EglDeclaration>? declarations,
            IEnumerable<EglStatement>? statements,
            SourceLocation location)
            : base(location)
        {
            Declarations = declarations?.ToList() ?? new List<EglDeclaration>();
            Statements = statements?.ToList() ?? new List<EglStatement>();
        }
    }
}