using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I DCL / DECLARE declaration dispatch davranışını yönetir.
///
/// Neden var?
/// ----------------------
/// Pl1Parser içinde DCL sonrası token'a bakarak variable veya structure declaration
/// seçimi yapmak ana parser sınıfını declaration detaylarıyla büyütür.
///
/// Ne çözüyor?
/// ----------------------
/// DCL sonrasında Number görülürse StructureParser'a, Identifier görülürse
/// VariableDeclarationParser'a yönlendirir. Ortak token okuma davranışını
/// ParserBase üzerinden kullanır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL PARAM CHAR(08);
/// - DCL 1 REC, 5 PARAM CHAR(08);
///
/// Nerede kullanılır?
/// ----------------------
/// - Pl1Parser.ParseDeclaration içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Multiple declaration, factored declaration, procedure declaration ve statement parser
/// dispatch davranışlarına temiz zemin hazırlar.
/// </summary>
internal sealed class DeclarationParser : ParserBase
{
    public DeclarationParser(ParseContext context)
        : base(context)
    {
    }

    public DeclarationParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
        : this(new ParseContext(
            tokens,
            position,
            diagnostics))
    {
    }

    /// <summary>
    /// Mevcut token'dan başlayarak PL/I declaration parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I declaration syntax DCL / DECLARE ile başlar. DCL sonrasındaki token,
    /// declaration ailesini belirler.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Variable ve structure declaration parser seçimini tek noktada toplar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08); => Pl1VariableDeclaration
    /// - DCL 1 REC, 5 PARAM CHAR(08); => Pl1StructureDeclaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Pl1Parser.ParseDeclaration içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Yeni declaration türleri eklendiğinde Pl1Parser değişmeden bu dispatch methodu
    /// genişletilebilir.
    /// </summary>
    public DeclarationParseResult ParseDeclaration()
    {
        if (Current.Kind != Pl1TokenKind.DclKeyword)
        {
            Diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"DCL bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new DeclarationParseResult(
                null,
                Position);
        }

        var nextToken = Peek(1);

        if (nextToken.Kind == Pl1TokenKind.Number)
        {
            var parser = new StructureParser(Context);
            var result = parser.ParseStructureDeclaration();

            return new DeclarationParseResult(
                result.Declaration,
                Position);
        }

        if (nextToken.Kind == Pl1TokenKind.Identifier)
        {
            var parser = new VariableDeclarationParser(Context);
            var result = parser.ParseVariableDeclaration();

            return new DeclarationParseResult(
                result.Declaration,
                Position);
        }

        Diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"DCL sonrasında değişken adı veya structure seviye numarası bekleniyordu. Gelen token: {nextToken.Text}",
            nextToken.Location));

        return new DeclarationParseResult(
            null,
            Position);
    }
}

internal sealed class DeclarationParseResult
{
    public Pl1Declaration? Declaration { get; }

    public int Position { get; }

    public DeclarationParseResult(
        Pl1Declaration? declaration,
        int position)
    {
        Declaration = declaration;
        Position = position;
    }
}