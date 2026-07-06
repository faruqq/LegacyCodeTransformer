using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Statements;

namespace LegacyCodeTransformer.Pl1.Parsing.Helpers;

/// <summary>
/// PL/I expression modellerinin oluşturulmasını merkezi hale getirir.
///
/// Neden var?
/// ----------------------
/// Statement parser sınıfları expression modelinin concrete türünü bilmemelidir.
/// P05.3 aşamasında expression parser henüz detaylandırılmadığı için raw expression
/// üretilir, ancak bu karar parser sınıflarına dağılmamalıdır.
///
/// Ne çözüyor?
/// ----------------------
/// AssignmentStatementParser ve CallStatementParser gibi statement parser'ların
/// doğrudan new Pl1RawExpression(...) üretmesini engeller. Expression üretim
/// kararını tek noktada toplar.
///
/// Hangi örneği destekliyor?
/// ----------------------
///     PARAM
///     'ABC'
///     DCLGLAU.BRM_KOD
///     PRICE + TAX
///
/// Nerede kullanılır?
/// ----------------------
/// Assignment target/value expression üretiminde ve CALL argument expression
/// üretiminde kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Pl1IdentifierExpression, Pl1LiteralExpression, Pl1BinaryExpression ve
/// Pl1FunctionCallExpression gibi gerçek expression modelleri eklendiğinde
/// statement parser'lar değiştirilmeden bu factory genişletilecektir.
/// </summary>
internal static class ExpressionFactory
{
    /// <summary>
    /// Token listesinden uygun PL/I expression modelini oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Statement parser'ların expression üretiminde tek ve stabil bir entrypoint
    /// kullanması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Bugün için güvenli fallback olarak Pl1RawExpression üretir. İleride gerçek
    /// expression parser eklendiğinde bu method daha spesifik expression modelleri
    /// üretmek üzere genişletilecektir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     PARAM
    ///     'ABC'
    ///     PRICE + TAX
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// AssignmentStatementParser ve CallStatementParser içinde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser'lar değişmeden expression model üretimini geliştirme imkanı
    /// sağlar.
    /// </summary>
    public static Pl1Expression Create(
        IEnumerable<Pl1Token> tokens,
        SourceLocation location)
    {
        return CreateRaw(
            tokens,
            location);
    }

    /// <summary>
    /// Token listesinden Pl1RawExpression üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// P05.3 aşamasında expression parser henüz detaylı değildir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Expression içeriğini kaybetmeden syntax tree üzerinde taşır.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    ///     DCLGLAU.BRM_KOD
    ///     'SELECT GLAU_HISTORY'
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// Create methodunun P05.3 fallback üretiminde kullanılır.
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Gerçek expression modelleri geldiğinde raw fallback davranışı korunabilir.
    /// </summary>
    public static Pl1Expression CreateRaw(
        IEnumerable<Pl1Token> tokens,
        SourceLocation location)
    {
        var text = AssignmentRawExpressionBuilder.Build(tokens);

        return new Pl1RawExpression(
            text,
            location);
    }
}