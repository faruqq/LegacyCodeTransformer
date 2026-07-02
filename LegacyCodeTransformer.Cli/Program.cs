using LegacyCodeTransformer.Application.Services;

var source = args.Length > 0
    ? string.Join(' ', args)
    : "DCL MUST_NO FIXED DECIMAL(8);";

var conversionService = new ConversionService();

var result = conversionService.ConvertPl1ToEgl(source);

if (!result.Success)
{
    Console.WriteLine("Dönüşüm başarısız oldu.");
    Console.WriteLine();

    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine(diagnostic);
    }

    return;
}

Console.WriteLine("Dönüşüm başarılı.");
Console.WriteLine();
Console.WriteLine(result.Output);

if (result.Diagnostics.Any())
{
    Console.WriteLine();
    Console.WriteLine("Diagnostic Mesajları:");

    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine(diagnostic);
    }
}
