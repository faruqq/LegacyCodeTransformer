using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Normalization;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Transpilers.Naming;
using LegacyCodeTransformer.Transpilers.Pl1ToEgl;

namespace LegacyCodeTransformer.Application.Services;

/// <summary>
/// Kaynak kod dönüşüm pipeline'ını yöneten application servisidir.
///
/// Neden var?
/// ----------------------
/// Lexer, Parser, Normalizer, Transpiler ve Generator sınıfları ayrı
/// sorumluluklara sahiptir. Ancak dış dünyaya bu parçaları tek tek kullandırmak
/// istemiyoruz.
///
/// Ne çözüyor?
/// ----------------------
/// PL/I → EGL dönüşüm akışını tek noktadan yönetir.
/// Diagnostic toplama, başarısız aşamada dönüşümü durdurma ve başarılı
/// durumda EGL source output üretme sorumluluğunu merkezi hale getirir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// - DCL MUST_NO FIXED DECIMAL(8);
/// - DCL PARAM CHAR(10) DIM(2);
/// - DCL 1 CUSTOMER_INFO, 5 CUSTOMER_NAME CHAR(20);
///
/// Nerede kullanılır?
/// ----------------------
/// - CLI projesinde
/// - Unit testlerde uçtan uca dönüşüm senaryolarında
/// - Gelecekte GUI veya IDE entegrasyonlarında
///
/// Gelecekte neye temel olur?
/// ----------------------
/// PL/I → C#, EGL → C#, sqlRecord metadata veya farklı output options
/// eklendiğinde ilgili pipeline metotları bu katmanda koordine edilebilir.
/// </summary>
public sealed class ConversionService
{
    /// <summary>
    /// PL/I kaynak kodunu varsayılan dönüşüm ayarlarıyla EGL kaynak koduna dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Dış dünya için en basit kullanım bu method üzerinden sağlanır.
    /// Caller'ın options veya naming strategy bilmeden PL/I → EGL dönüşümü
    /// yapabilmesi gerekir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Varsayılan naming strategy ve varsayılan record type strategy ile dönüşüm
    /// yapılmasını sağlar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Input:
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// Beklenen varsayılan EGL:
    /// MustNo decimal(8);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - CLI projesinde
    /// - Unit testlerde varsayılan uçtan uca dönüşüm senaryolarında
    /// - Gelecekte GUI veya IDE entegrasyonlarında
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Varsayılan conversion behavior korunarak yeni options değerleri
    /// Pl1ToEglTranspilerOptions.Default üzerinden merkezi yönetilebilir.
    /// </summary>
    public ConversionResult ConvertPl1ToEgl(string source)
    {
        return ConvertPl1ToEgl(
            source,
            Pl1ToEglTranspilerOptions.Default);
    }

    /// <summary>
    /// PL/I kaynak kodunu verilen identifier naming ayarlarıyla EGL kaynak koduna dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// PL/I → EGL dönüşümünde identifier casing kuralı proje veya kurum
    /// standardına göre değişebilir. Bu overload daha önce var olan public API
    /// davranışını korumalıdır.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Sadece naming strategy verilerek dönüşüm yapılmasını sağlar.
    /// Record type strategy verilmediği için default basicRecord davranışı korunur.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Input:
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// PascalCase:
    /// MustNo decimal(8);
    ///
    /// CamelCase:
    /// mustNo decimal(8);
    ///
    /// Preserve:
    /// MUST_NO decimal(8);
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application unit testlerinde farklı naming strategy doğrulamalarında
    /// - Gelecekte CLI parametresi veya UI seçimi ile dönüşüm yapılırken
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// Naming strategy dışındaki ayarlar gerektiğinde bu overload yeni options
    /// overload'una adaptör olarak kalır.
    /// </summary>
    public ConversionResult ConvertPl1ToEgl(
        string source,
        IdentifierNamingOptions namingOptions)
    {
        return ConvertPl1ToEgl(
            source,
            new Pl1ToEglTranspilerOptions(namingOptions));
    }

    /// <summary>
    /// PL/I kaynak kodunu verilen transpiler options ile EGL kaynak koduna dönüştürür.
    ///
    /// Neden var?
    /// ----------------------
    /// Dönüşüm davranışı yalnızca naming strategy ile sınırlı değildir.
    /// EGL record type strategy gibi hedef dile özgü üretim kararları da
    /// application pipeline'a verilebilmelidir.
    ///
    /// Ne çözüyor?
    /// ----------------------
    /// Lexer, Parser, Normalizer, Transpiler ve Generator aşamalarını tek
    /// pipeline olarak çalıştırır. Transpiler aşamasına Pl1ToEglTranspilerOptions
    /// vererek basicRecord / sqlRecord seçimini mümkün kılar.
    ///
    /// Hangi örneği destekliyor?
    /// ----------------------
    /// Default options:
    /// record CustomerInfo type basicRecord
    ///
    /// SqlRecord strategy:
    /// record CustomerInfo type sqlRecord
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - sqlRecord uçtan uca application testlerinde
    /// - Gelecekte CLI veya UI tarafında dönüşüm ayarı verilirken
    /// - DB2 erişim fonksiyonları için sqlRecord output üretiminde
    ///
    /// Gelecekte neye temel olur?
    /// ----------------------
    /// INIT output, SQL metadata, table name mapping veya farklı EGL output
    /// stratejileri bu options modeli üzerinden application pipeline'a taşınabilir.
    /// </summary>
    public ConversionResult ConvertPl1ToEgl(
        string source,
        Pl1ToEglTranspilerOptions options)
    {
        var diagnostics = new List<Diagnostic>();

        var lexer = new Pl1Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Pl1Parser(tokens);
        var parseResult = parser.Parse();

        diagnostics.AddRange(parseResult.Diagnostics);

        if (!parseResult.Success || parseResult.SyntaxTree is null)
        {
            return new ConversionResult(null, diagnostics);
        }

        var normalizer = new Pl1Normalizer();
        var normalizationResult = normalizer.Normalize(parseResult.SyntaxTree);

        diagnostics.AddRange(normalizationResult.Diagnostics);

        if (!normalizationResult.Success || normalizationResult.SyntaxTree is null)
        {
            return new ConversionResult(null, diagnostics);
        }

        var transpiler = new Pl1ToEglTranspiler(
            options ?? Pl1ToEglTranspilerOptions.Default);

        var transpilationResult = transpiler.Transpile(
            normalizationResult.SyntaxTree);

        diagnostics.AddRange(transpilationResult.Diagnostics);

        if (!transpilationResult.Success || transpilationResult.SyntaxTree is null)
        {
            return new ConversionResult(null, diagnostics);
        }

        var generator = new EglCodeGenerator();
        var output = generator.Generate(transpilationResult.SyntaxTree);

        return new ConversionResult(output, diagnostics);
    }
}