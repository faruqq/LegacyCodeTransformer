using LegacyCodeTransformer.Application.Services;
using LegacyCodeTransformer.Cli;

var argumentParser = new CliArgumentParser();

if (!argumentParser.TryParse(
        args,
        out var options,
        out var argumentError))
{
    Console.Error.WriteLine("CLI argümanları geçersiz.");
    Console.Error.WriteLine(argumentError);

    return 1;
}

if (options is null)
{
    Console.Error.WriteLine(
        "CLI seçenekleri oluşturulamadı.");

    return 1;
}

if (!File.Exists(options.InputFilePath))
{
    Console.Error.WriteLine(
        $"PL/I giriş dosyası bulunamadı: {options.InputFilePath}");

    return 1;
}

string source;

try
{
    source = File.ReadAllText(options.InputFilePath);
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"PL/I giriş dosyası okunamadı: {options.InputFilePath}");

    Console.Error.WriteLine(exception.Message);

    return 1;
}

var conversionService = new ConversionService();
var result = conversionService.ConvertPl1ToEgl(source);

if (!result.Success || result.Output is null)
{
    Console.Error.WriteLine("Dönüşüm başarısız oldu.");

    WriteDiagnostics(result.Diagnostics);

    return 1;
}

if (string.IsNullOrWhiteSpace(options.OutputFilePath))
{
    Console.WriteLine(result.Output);
}
else
{
    try
    {
        var outputDirectory = Path.GetDirectoryName(
            options.OutputFilePath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllText(
            options.OutputFilePath,
            result.Output);

        Console.WriteLine("Dönüşüm başarılı.");
        Console.WriteLine(
            $"EGL çıktı dosyası: {options.OutputFilePath}");
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine(
            $"EGL çıktı dosyası yazılamadı: {options.OutputFilePath}");

        Console.Error.WriteLine(exception.Message);

        return 1;
    }
}

WriteDiagnostics(result.Diagnostics);

return 0;

/// <summary>
/// Conversion pipeline tarafından üretilen diagnostic mesajlarını konsola yazar.
///
/// Neden var?
/// ----------------------
/// Dosyaya başarılı EGL çıktısı yazılmış olsa bile warning veya bilgi seviyesindeki
/// diagnostic mesajlarının kullanıcı tarafından görülebilmesi gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Diagnostic konsol çıktısını başarı ve hata akışlarında aynı formatta üretir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// Parser, semantic analyzer veya transpiler tarafından üretilen diagnostic listesi.
///
/// Nerede kullanılır?
/// ----------------------
/// Program.cs başarılı ve başarısız dönüşüm akışlarında kullanılır.
///
/// Gelecekte neye temel olur?
/// ----------------------
/// Diagnostic dosyası veya verbose çıktı ihtiyacı oluştuğunda mevcut konsol
/// davranışının merkezi olarak geliştirilmesine temel olur.
/// </summary>
static void WriteDiagnostics(
    IReadOnlyList<LegacyCodeTransformer.Core.Diagnostics.Diagnostic>
        diagnostics)
{
    if (diagnostics.Count == 0)
    {
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Diagnostic Mesajları:");

    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine(diagnostic);
    }
}