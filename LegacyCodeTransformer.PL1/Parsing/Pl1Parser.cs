using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;
using LegacyCodeTransformer.Pl1.Syntax;

namespace LegacyCodeTransformer.Pl1.Parsing;

/// <summary>
/// PL/I token listesini Pl1SyntaxTree modeline dönüştüren ana parser sınıfıdır.
///
/// Neden var?
/// ----------------------
/// Lexer yalnızca kaynak PL/I metnini token listesine ayırır. Bu token listesinin
/// declaration, structure ve ileride statement modellerine dönüştürülmesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Parser ana orchestration katmanı olarak çalışır. Declaration detaylarını kendisi
/// parse etmez; DeclarationParser ve onun altındaki helper parser sınıflarına yönlendirir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL MUST_NO FIXED DECIMAL(8);
/// - DCL PARAM CHAR(08) INIT(' ');
/// - DCL 1 REC, 5 PARAM CHAR(08);
///
/// Nerede kullanılır?
/// ----------------------
/// - ConversionService pipeline içinde
/// - Parser unit testlerinde
/// - Normalizer ve Transpiler aşamalarından önce
///
/// Gelecekte neye temel olur?
/// ----------------------
/// P05 ile statement parser eklendiğinde Pl1Parser ana orchestration sınıfı olarak
/// kalacak; IF, DO, CALL, assignment gibi syntax aileleri ayrı helper parser sınıflarına
/// yönlendirilecektir.
/// </summary>
public sealed class Pl1Parser
{
    private readonly IReadOnlyList<Pl1Token> _tokens;
    private readonly DiagnosticBag _diagnostics = new();
    private int _position;

    /// <summary>
    /// PL/I parser instance'ını oluşturur.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser'ın lexer tarafından üretilen token listesini sırayla okuyabilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token listesini parser state'i olarak saklar. Null token listesi gelirse güvenli
    /// şekilde boş listeye düşer.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// new Pl1Parser(tokens)
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ConversionService içinde
    /// - Parser unit testlerinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser ve procedure parser eklendiğinde aynı token stream orchestration
    /// bu sınıf üzerinden devam eder.
    /// </summary>
    public Pl1Parser(IReadOnlyList<Pl1Token> tokens)
    {
        _tokens = tokens ?? Array.Empty<Pl1Token>();
    }

    /// <summary>
    /// Token listesini okuyarak Pl1SyntaxTree üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I kaynak kodundan gelen token listesi dönüşüm pipeline'ında kullanılabilecek
    /// güçlü tipli syntax tree modeline çevrilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Şu anda declaration odaklı parse yapar. DCL / DECLARE gördüğünde declaration
    /// parser'a yönlendirir; desteklenmeyen token gördüğünde diagnostic üretir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - Tekil variable declaration
    /// - Structure declaration
    /// - Nested structure declaration
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - ConversionService içinde
    /// - Parser unit testlerinde
    /// - Normalizer ve Transpiler öncesinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// PL/I program yapısı genişledikçe declaration dışındaki statement türleri de bu
    /// ana parse akışı üzerinden ilgili helper parser sınıflarına yönlendirilecektir.
    /// </summary>
    public ParseResult<Pl1SyntaxTree> Parse()
    {
        var declarations = new List<Pl1Declaration>();

        while (!IsAtEnd())
        {
            if (Current.Kind == Pl1TokenKind.DclKeyword)
            {
                var declaration = ParseDeclaration();

                if (declaration is not null)
                {
                    declarations.Add(declaration);
                }

                continue;
            }

            AddUnexpectedTokenDiagnostic(
                Current,
                "DCL");

            Advance();
        }

        var syntaxTree = new Pl1SyntaxTree(
            declarations,
            SourceLocation.Unknown);

        return new ParseResult<Pl1SyntaxTree>(
            syntaxTree,
            _diagnostics.Diagnostics);
    }

    /// <summary>
    /// PL/I declaration ifadesini parse eder.
    ///
    /// Neden var?
    /// ----------------------
    /// Parse ana döngüsü DCL / DECLARE token gördüğünde declaration parse davranışına
    /// yönlenmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Token akışının ana state'ini Pl1Parser üzerinde korur, fakat DCL sonrası variable /
    /// structure declaration seçimi ve detay parsing sorumluluğunu DeclarationParser helper
    /// sınıfına devreder.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// - DCL PARAM CHAR(08);
    /// - DCL 1 REC, 5 PARAM CHAR(08);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana döngüsü içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// P05 statement parser eklendiğinde Pl1Parser ana orchestration sınıfı olarak kalır;
    /// declaration detayları DeclarationParser içinde büyür.
    /// </summary>
    private Pl1Declaration? ParseDeclaration()
    {
        var parser = new DeclarationParser(
            _tokens,
            _position,
            _diagnostics);

        var result = parser.ParseDeclaration();

        _position = result.Position;

        return result.Value;
    }

    /// <summary>
    /// Beklenmeyen token için diagnostic üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser desteklenmeyen veya grammar dışı bir token gördüğünde kullanıcıya anlamlı
    /// hata bilgisi üretmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Hatalı token metniyle beklenen syntax bilgisini diagnostic listesine ekler.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL dışındaki token ile başlayan kaynaklarda beklenen token bilgisini üretir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser eklendiğinde top-level unsupported syntax diagnostic davranışı
    /// aynı standartta korunur.
    /// </summary>
    private void AddUnexpectedTokenDiagnostic(
        Pl1Token token,
        string expectedText)
    {
        _diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            $"Beklenmeyen token: {token.Text}. Beklenen: {expectedText}.",
            token.Location));
    }

    /// <summary>
    /// Mevcut token'ı tüketip bir sonraki token'a ilerler.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser token listesinde sırayla ilerlemelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Hata toparlama sırasında desteklenmeyen token'ı atlayarak parser'ın sonsuz
    /// döngüye girmesini engeller.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// DCL dışındaki beklenmeyen token görüldüğünde bir sonraki token'a geçilir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışındaki hata toparlama branch'inde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser eklendiğinde top-level recovery davranışı bu method üzerinden
    /// korunabilir.
    /// </summary>
    private void Advance()
    {
        if (!IsAtEnd())
        {
            _position++;
        }
    }

    /// <summary>
    /// Parser'ın kaynak sonu token'ına gelip gelmediğini belirtir.
    ///
    /// Neden var?
    /// ----------------------
    /// Parse ana döngüsünün token stream sonunda durması gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// EndOfFile token görüldüğünde parse işleminin tamamlandığını belirtir.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Tüm declaration'lar okunduktan sonra EOF token üzerinde parse döngüsünü bitirir.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana döngüsünde
    /// - Advance içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Statement parser eklendiğinde aynı EOF kontrolü ana orchestration seviyesinde
    /// kullanılmaya devam eder.
    /// </summary>
    private bool IsAtEnd()
    {
        return Current.Kind == Pl1TokenKind.EndOfFile;
    }

    /// <summary>
    /// Mevcut parser pozisyonundaki token'ı döndürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Parser'ın karar verebilmesi için mevcut token'a güvenli şekilde erişmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Position token listesinin dışına taşarsa son token'ı döndürerek out-of-range
    /// hatasını engeller.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// EOF sonrası güvenli Current erişimi sağlar.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Parse ana akışında
    /// - IsAtEnd içinde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Top-level parser orchestration aynı Current erişimiyle statement dispatch
    /// davranışını da yönetebilir.
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