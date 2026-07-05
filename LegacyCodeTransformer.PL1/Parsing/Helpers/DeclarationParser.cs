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
/// DCL sonrasında Number görülürse structure declaration parser'a, Identifier görülürse
/// variable declaration parser'a yönlendirir.
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
internal sealed class DeclarationParser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics;
    private int _position;

    public DeclarationParser(
        IReadOnlyList<Pl1Token> tokens,
        int position,
        DiagnosticBag diagnostics)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
        _position = position;
        _diagnostics = diagnostics;
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
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"DCL bekleniyordu. Gelen token: {Current.Text}",
                Current.Location));

            return new DeclarationParseResult(
                null,
                _position);
        }

        var nextToken = Peek(1);

        if (nextToken.Kind == Pl1TokenKind.Number)
        {
            var parser = new StructureParser(
                _tokens,
                _position,
                _diagnostics);

            var result = parser.ParseStructureDeclaration();

            return new DeclarationParseResult(
                result.Declaration,
                result.Position);
        }

        if (nextToken.Kind == Pl1TokenKind.Identifier)
        {
            var parser = new VariableDeclarationParser(
                _tokens,
                _position,
                _diagnostics);

            var result = parser.ParseVariableDeclaration();

            return new DeclarationParseResult(
                result.Declaration,
                result.Position);
        }

        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"DCL sonrasında değişken adı veya structure seviye numarası bekleniyordu. Gelen token: {nextToken.Text}",
            nextToken.Location));

        return new DeclarationParseResult(
            null,
            _position);
    }

    /// <summary>
    /// Mevcut pozisyona göre ileri offset'teki token'ı döndürür.
    /// </summary>
    private Pl1Token Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    /// <summary>
    /// Mevcut helper parser pozisyonundaki token'ı döndürür.
    /// </summary>
    private Pl1Token Current
    {
        get
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[^1];
            }

            return _tokens[_position];
        }
    }
}

/// <summary>
/// Declaration parse sonucunu ve parse sonrası token pozisyonunu taşır.
///
/// Neden var?
/// ----------------------
/// DeclarationParser ayrı token position state'i ile çalışır. Parse tamamlandığında
/// Pl1Parser'ın kendi pozisyonunu güncellemesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parse edilen Pl1Declaration modeli ile parse sonrası position değerini birlikte döndürür.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// DCL PARAM CHAR(08); parse edildiğinde variable declaration modeli ve semicolon sonrası
/// position birlikte taşınır.
///
/// Nerede kullanılır?
/// ----------------------
/// - DeclarationParser.ParseDeclaration dönüş değerinde
/// - Pl1Parser.ParseDeclaration içinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 ile statement / declaration dispatch ayrımı yapılırken aynı result pattern korunabilir.
/// </summary>
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