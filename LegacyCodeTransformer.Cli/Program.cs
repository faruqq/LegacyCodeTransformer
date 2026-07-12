using LegacyCodeTransformer.Application.Services;
using LegacyCodeTransformer.Cli;
using LegacyCodeTransformer.Core.Diagnostics;

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

var inputFilePath = options.IsCaseMode
    ? Path.Combine(
        options.CaseDirectoryPath!,
        "input.pl1")
    : options.InputFilePath!;

var outputFilePath = options.IsCaseMode
    ? Path.Combine(
        options.CaseDirectoryPath!,
        "actual.egl")
    : options.OutputFilePath;

if (options.IsCaseMode &&
    !Directory.Exists(options.CaseDirectoryPath))
{
    Console.Error.WriteLine(
        $"Case klasörü bulunamadı: {options.CaseDirectoryPath}");

    return 1;
}

if (!File.Exists(inputFilePath))
{
    Console.Error.WriteLine(
        $"PL/I giriş dosyası bulunamadı: {inputFilePath}");

    return 1;
}

string source;

try
{
    source = File.ReadAllText(inputFilePath);
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"PL/I giriş dosyası okunamadı: {inputFilePath}");

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

if (string.IsNullOrWhiteSpace(outputFilePath))
{
    Console.WriteLine(result.Output);
}
else
{
    try
    {
        var outputDirectory = Path.GetDirectoryName(
            outputFilePath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllText(
            outputFilePath,
            result.Output);

        Console.WriteLine("Dönüşüm başarılı.");
        Console.WriteLine(
            $"PL/I giriş dosyası: {inputFilePath}");
        Console.WriteLine(
            $"EGL çıktı dosyası: {outputFilePath}");
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine(
            $"EGL çıktı dosyası yazılamadı: {outputFilePath}");

        Console.Error.WriteLine(exception.Message);

        return 1;
    }
}

WriteDiagnostics(result.Diagnostics);

return 0;

/// <summary>
/// Conversion pipeline tarafından üretilen diagnostic mesajlarını konsola
/// yazar.
///
/// Neden var?
/// ----------------------
/// Dosyaya başarılı EGL çıktısı yazılmış olsa bile warning veya bilgi
/// seviyesindeki diagnostic mesajlarının kullanıcı tarafından görülebilmesi
/// gerekir.
///
/// Ne çözüyor?
/// ----------------------
/// Diagnostic konsol çıktısını başarı ve hata akışlarında aynı formatta
/// üretir.
///
/// Hangi örneği destekliyor?
/// ----------------------
/// Parser, semantic analyzer veya transpiler tarafından üretilen diagnostic
/// listesi.
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
    IReadOnlyList<Diagnostic> diagnostics)
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