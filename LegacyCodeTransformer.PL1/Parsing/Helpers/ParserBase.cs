using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Parser helper sınıfları için ortak token okuma davranışlarını sağlar.
///
/// Neden var?
/// ----------------------
/// Helper parser sınıflarında Current, Previous, Peek, Advance, Consume ve IsAtEnd
/// davranışları tekrar etmektedir.
///
/// Ne çözüyor?
/// ----------------------
/// Ortak token okuma ve diagnostic ekleme davranışını tek base sınıfta toplar.
/// Helper parser sınıfları kendi grammar logic'lerine odaklanır.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// CharacterTypeParser ve BitTypeParser gibi parserlar token tüketme davranışını
/// tekrar yazmadan ParserBase üzerinden kullanabilir.
///
/// Nerede kullanılır?
/// ----------------------
/// - Parser helper sınıflarının base class'ı olarak
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 statement parser helper sınıfları aynı base class üzerinden geliştirilecektir.
/// </summary>
internal abstract class ParserBase
{
    protected ParseContext Context { get; }

    protected DiagnosticBag Diagnostics => Context.Diagnostics;

    protected int Position
    {
        get => Context.Position;
        set => Context.Position = value;
    }

    protected ParserBase(ParseContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Beklenen token türünü tüketir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser grammar içinde belirli noktalarda belirli token türleri beklenir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Beklenen token gelirse token'ı tüketip döndürür. Gelmezse diagnostic üretir
    /// ve null döner.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// CHAR(08) içindeki OpenParenthesis, Number ve CloseParenthesis token'larını
    /// doğrular.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parser helper sınıflarında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Recovery mode veya expected token listesi gibi davranışlar bu method üzerinden
    /// geliştirilebilir.
    /// </summary>
    protected Pl1Token? Consume(
        Pl1TokenKind expectedKind,
        string errorMessage)
    {
        if (Current.Kind == expectedKind)
        {
            return Advance();
        }

        Diagnostics.Add(
            ParserDiagnosticFactory.ExpectedToken(
                errorMessage,
                Current));

        return null;
    }

    /// <summary>
    /// Mevcut token'ı tüketip bir sonraki token'a ilerler.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser token stream üzerinde ilerlemelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EOF değilse position değerini artırır ve tüketilen token'ı döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Current token CHAR iken Advance sonrası '(' token'ına geçilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Consume içinde
    /// - Özel grammar branch'lerinde doğrudan token atlamak gerektiğinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser recovery ve lookahead davranışları aynı token ilerleme
    /// standardını kullanır.
    /// </summary>
    protected Pl1Token Advance()
    {
        if (!IsAtEnd())
        {
            Position++;
        }

        return Previous;
    }

    /// <summary>
    /// Mevcut pozisyona göre ileri offset'teki token'ı döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Bazı parser branch kararları mevcut token dışındaki sonraki token'a göre verilir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token listesi dışına çıkıldığında güvenli şekilde son token'ı döndürür.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL sonrasında Identifier mı Number mı geldiğini kontrol etmek.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - DeclarationParser
    /// - Gelecekte expression parser lookahead işlemlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser ve expression parser lookahead davranışlarına temel olur.
    /// </summary>
    protected Pl1Token Peek(int offset)
    {
        var index = Position + offset;

        if (index >= Context.Tokens.Count)
        {
            return Context.Tokens[^1];
        }

        return Context.Tokens[index];
    }

    /// <summary>
    /// Parser'ın kaynak sonu token'ına gelip gelmediğini belirtir.
    /// </summary>
    protected bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    /// <summary>
    /// Mevcut parser pozisyonundaki token'ı döndürür.
    /// </summary>
    protected Pl1Token Current
    {
        get
        {
            if (Position >= Context.Tokens.Count)
            {
                return Context.Tokens[^1];
            }

            return Context.Tokens[Position];
        }
    }

    /// <summary>
    /// Bir önce tüketilen token'ı döndürür.
    /// </summary>
    protected Pl1Token Previous => Context.Tokens[Position - 1];
}