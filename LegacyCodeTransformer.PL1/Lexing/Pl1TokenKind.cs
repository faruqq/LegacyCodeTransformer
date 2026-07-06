namespace LegacyCodeTransformer.Pl1.Lexing;

/// <summary>
/// PL/I lexer tarafından üretilebilecek token türlerini temsil eder.
///
/// Neden var?
/// ----------------------
/// Lexer kaynak PL/I metnini anlamlı parçalara ayırır. Parser ise bu parçaların
/// türüne göre syntax model üretir.
///
/// Ne çözüyor?
/// ----------------------
/// Keyword, identifier, sayı, string literal, punctuation ve statement operator gibi
/// PL/I kaynak parçalarını güçlü tipli enum değerleriyle temsil eder.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL PARAM CHAR(08) INIT(' ');
/// - PARAM = 'ABC';
/// - CALL FETCH_CURSOR;
/// - IF SQLCODE = 0 THEN DO;
/// - DO WHILE(SQLCODE ^= 100);
///
/// Nerede kullanılır?
/// ----------------------
/// - PL/I Lexer token üretiminde
/// - PL/I Parser token dispatch işlemlerinde
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Statement parser, expression parser, procedure parser, embedded SQL ve
/// control-flow parser geliştirmelerinde lexer/parser sözleşmesinin merkezi enum
/// modeli olarak genişletilir.
/// </summary>
public enum Pl1TokenKind
{
    BadToken,
    EndOfFile,

    Identifier,
    Number,
    StringLiteral,

    DclKeyword,

    FixedKeyword,
    DecimalKeyword,
    DecKeyword,
    BinaryKeyword,
    BinKeyword,
    CharKeyword,
    CharacterKeyword,
    VarcharKeyword,
    PicKeyword,
    PictureKeyword,
    InitKeyword,
    InitialKeyword,
    BitKeyword,
    DimKeyword,
    DimensionKeyword,
    FloatKeyword,
    RealKeyword,
    DoubleKeyword,
    PrecisionKeyword,

    CallKeyword,
    IfKeyword,
    ThenKeyword,
    ElseKeyword,
    DoKeyword,
    EndKeyword,
    WhileKeyword,
    UntilKeyword,

    OpenParenthesis,
    CloseParenthesis,
    Comma,
    Semicolon,
    Dot,
    Colon,

    Equals,
    Plus,
    Minus,
    Asterisk,
    Slash,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    NotEqual,
    NotLessThan,
    NotGreaterThan,
    Ampersand,
    Exclamation,
    Caret
}