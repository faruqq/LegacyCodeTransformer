using LegacyCodeTransformer.Core.Syntax;

namespace LegacyCodeTransformer.Pl1.Statements
{
    /// <summary>
    /// PL/I compiler directive statement modelidir.
    ///
    /// Neden var?
    /// ----------------------
    /// Gerçek PL/I kaynaklarında %INCLUDE, %PAGE, %EJECT gibi compiler directive
    /// satırları bulunur. Parser bu satırları kaybetmeden syntax tree üzerinde
    /// taşımalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Directive adını ve raw directive metnini korur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// %INCLUDE COPYLIB;
    /// %PAGE;
    /// %EJECT;
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// CompilerDirectiveStatementParser çıktısı olarak Pl1SyntaxTree.Statements
    /// veya Pl1Procedure.Statements listelerinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// INCLUDE resolution, listing directive handling ve compiler directive analysis
    /// davranışları için temel olur.
    /// </summary>
    public sealed class Pl1CompilerDirectiveStatement : Pl1Statement
    {
        public string DirectiveName { get; }

        public string RawDirectiveText { get; }

        public Pl1CompilerDirectiveStatement(
            string directiveName,
            string rawDirectiveText,
            SourceLocation location)
            : base(location)
        {
            DirectiveName = directiveName;
            RawDirectiveText = rawDirectiveText;
        }
    }
}