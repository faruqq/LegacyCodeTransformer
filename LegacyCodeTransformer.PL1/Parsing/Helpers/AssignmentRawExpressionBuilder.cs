using System.Text;
using LegacyCodeTransformer.Pl1.Lexing;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// Assignment statement içinde target ve value tarafındaki token listesinden
/// Pl1RawExpression metni oluşturur.
///
/// Neden var?
/// ----------------------
/// P05.3 aşamasında tam expression parser henüz yoktur. Buna rağmen assignment
/// statement modellerinin üretilebilmesi için sol ve sağ taraftaki expression
/// metinlerinin güvenli biçimde korunması gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Token listesini raw expression text değerine dönüştürür. Identifier, number,
/// string literal, member access, array access ve basit operator tokenlarını ilk
/// aşama için okunabilir ve stabil bir metin formatıyla taşır.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     A
///     B + C
///     SQLCODE
///     DCLGLAU.BRM_KOD
///     DIZI(I)
///     'ABC'
///
/// Nerede kullanılır?
/// ----------------------
/// AssignmentStatementParser içinde target ve value expression üretiminde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// ExpressionParser geliştirildiğinde bu helper geçiş katmanı olarak kalabilir.
/// İleride raw expression yerine gerçek expression tree üretimine geçerken
/// statement parser davranışı minimum değişiklikle korunabilir.
/// </summary>
internal static class AssignmentRawExpressionBuilder
{
    /// <summary>
    /// Verilen token listesinden raw expression metni oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Assignment parser'ın expression tarafını kaybetmeden Pl1RawExpression
    /// modeline taşıması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token text değerlerini expression bağlamına uygun boşluklandırmayla birleştirir.
    /// Nokta ve parantez gibi member/argument/index access işaretlerinde gereksiz
    /// boşluk üretmez; operatorlarda okunabilir boşluk bırakır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCLGLAU . BRM_KOD tokenları DCLGLAU.BRM_KOD olarak üretilir.
    ///
    /// DIZI ( I ) tokenları DIZI(I) olarak üretilir.
    ///
    /// A + B tokenları A + B olarak üretilir.
    ///
    /// StringLiteral tokenı quote restore edilerek 'ABC' olarak üretilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// AssignmentStatementParser içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Function call expression, array access expression ve binary expression parse
    /// desteği eklendiğinde bu helper'ın davranışı kademeli olarak gerçek expression
    /// parser'a taşınabilir.
    /// </summary>
    public static string Build(IEnumerable<Pl1Token> tokens)
    {
        var builder = new StringBuilder();

        foreach (var token in tokens)
        {
            AppendToken(builder, token);
        }

        return builder.ToString();
    }

    private static void AppendToken(
        StringBuilder builder,
        Pl1Token token)
    {
        var text = GetTokenText(token);

        if (builder.Length == 0)
        {
            builder.Append(text);
            return;
        }

        if (ShouldAppendWithoutLeadingSpace(token.Kind))
        {
            TrimTrailingSpace(builder);
            builder.Append(text);
            return;
        }

        if (ShouldPreviousTokenAvoidTrailingSpace(builder))
        {
            builder.Append(text);
            return;
        }

        builder.Append(' ');
        builder.Append(text);
    }

    private static string GetTokenText(Pl1Token token)
    {
        return token.Kind == Pl1TokenKind.StringLiteral
            ? $"'{token.Text}'"
            : token.Text;
    }

    private static bool ShouldAppendWithoutLeadingSpace(Pl1TokenKind tokenKind)
    {
        return tokenKind switch
        {
            Pl1TokenKind.Dot => true,
            Pl1TokenKind.Comma => true,
            Pl1TokenKind.OpenParenthesis => true,
            Pl1TokenKind.CloseParenthesis => true,
            _ => false
        };
    }

    private static bool ShouldPreviousTokenAvoidTrailingSpace(StringBuilder builder)
    {
        if (builder.Length == 0)
        {
            return false;
        }

        var lastCharacter = builder[builder.Length - 1];

        return lastCharacter == '.'
            || lastCharacter == '(';
    }

    private static void TrimTrailingSpace(StringBuilder builder)
    {
        while (builder.Length > 0 && builder[builder.Length - 1] == ' ')
        {
            builder.Length--;
        }
    }
}