using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Results;
using LegacyCodeTransformer.Egl.Generation;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Normalization;
using LegacyCodeTransformer.Pl1.Parsing;
using LegacyCodeTransformer.Transpilers.Pl1ToEgl;
using LegacyCodeTransformer.Transpilers.Naming;

namespace LegacyCodeTransformer.Application.Services
{
    /// <summary>
    /// Kaynak kod dönüşüm pipeline'ını yöneten application servisidir.
    ///
    /// Neden var?
    /// ----------------------
    /// Lexer, Parser, Normalizer, Transpiler ve Generator sınıfları ayrı
    /// sorumluluklara sahiptir. Ancak dış dünyaya bu parçaları tek tek kullandırmak
    /// istemiyoruz.
    ///
    /// Bu servis, PL/I → EGL dönüşüm akışını tek noktadan yönetmek için
    /// oluşturulmuştur.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - CLI projesinde
    /// - Gelecekte GUI veya IDE entegrasyonlarında
    /// - Unit testlerde uçtan uca dönüşüm senaryolarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I → C#, EGL → C# gibi yeni dönüşümler eklendiğinde,
    /// ilgili pipeline metotları bu katmanda koordine edilebilir.
    /// </summary>
    public sealed class ConversionService
    {
        /// <summary>
        /// PL/I kaynak kodunu varsayılan dönüşüm ayarlarıyla EGL kaynak koduna dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Dış dünya için en basit kullanım bu method üzerinden sağlanır.
        /// Naming strategy verilmediğinde firma standardına uygun varsayılan
        /// PascalCase davranışı kullanılır.
        ///
        /// Örnek:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        ///
        /// Beklenen varsayılan EGL:
        ///
        /// MustNo decimal(8,0);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - CLI projesinde
        /// - Unit testlerde varsayılan uçtan uca dönüşüm senaryolarında
        /// - Gelecekte GUI veya IDE entegrasyonlarında
        /// </summary>
        public ConversionResult ConvertPl1ToEgl(string source)
        {
            return ConvertPl1ToEgl(
                source,
                IdentifierNamingOptions.Default);
        }

        /// <summary>
        /// PL/I kaynak kodunu verilen identifier naming ayarlarıyla EGL kaynak koduna dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I → EGL dönüşümünde identifier casing kuralı proje veya kurum
        /// standardına göre değişebilir.
        ///
        /// Bu overload, dönüşüm pipeline'ına naming strategy verilmesini sağlar.
        ///
        /// Örnek:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        ///
        /// PascalCase:
        /// MustNo decimal(8,0);
        ///
        /// CamelCase:
        /// mustNo decimal(8,0);
        ///
        /// Preserve:
        /// MUST_NO decimal(8,0);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application unit testlerinde farklı naming strategy doğrulamalarında
        /// - Gelecekte CLI parametresi veya UI seçimi ile dönüşüm yapılırken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Dönüşüm ayarları arttıkça bu method options tabanlı pipeline giriş noktası
        /// olarak genişletilebilir.
        /// </summary>
        public ConversionResult ConvertPl1ToEgl(
            string source,
            IdentifierNamingOptions namingOptions)
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

            var transpiler = new Pl1ToEglTranspiler(namingOptions);
            var transpilationResult = transpiler.Transpile(normalizationResult.SyntaxTree);

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
}
