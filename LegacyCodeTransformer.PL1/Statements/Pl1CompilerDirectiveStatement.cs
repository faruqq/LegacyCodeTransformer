using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I compiler directive statement modelidir.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I kaynaklarında %INCLUDE, %PAGE, %EJECT, %PROCESS gibi compiler
    /// directive satırları bulunur. Parser bu satırları kaybetmeden syntax tree
    /// üzerinde taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Directive adını, directive argümanlarını ve raw directive metnini korur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// %INCLUDE COPYLIB;
    /// %PAGE;
    /// %PROCESS MACRO;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// CompilerDirectiveStatementParser çıktısı olarak Pl1SyntaxTree.Statements
    /// veya Pl1Procedure.Statements listelerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// INCLUDE resolution, listing directive handling, macro directive analysis
    /// ve compiler directive semantic analysis davranışları için temel olur.
    /// </summary>
    public sealed class Pl1CompilerDirectiveStatement : Pl1Statement
    {
        public string DirectiveName { get; }

        public IReadOnlyList<string> Arguments { get; }

        public string RawDirectiveText { get; }

        public Pl1CompilerDirectiveStatement(
            string directiveName,
            IEnumerable<string>? arguments,
            string rawDirectiveText,
            SourceLocation location)
            : base(location)
        {
            DirectiveName = directiveName;
            Arguments = arguments?.ToList() ?? new List<string>();
            RawDirectiveText = rawDirectiveText;
        }
    }
}