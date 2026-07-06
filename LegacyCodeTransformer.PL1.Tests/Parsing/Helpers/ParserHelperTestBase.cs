using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public abstract class ParserHelperTestBase
{
    /// <summary>
    /// Parser helper testlerinde kullanılacak ortak ParseContext modelini oluşturur.
    ///
    /// Bu test altyapısı neyi çözer?
    /// Helper parser testlerinde tekrar eden lexer, token listesi ve DiagnosticBag oluşturma kodlarını tek noktada toplar.
    ///
    /// Hangi input'u test eder?
    /// Verilen source string değerini Pl1Lexer ile tokenize eder.
    ///
    /// Beklenen temel model/çıktı nedir?
    /// Position 0'dan başlayan ve boş DiagnosticBag içeren ParseContext üretilmelidir.
    /// </summary>
    private protected static ParseContext CreateContext(string source)
    {
        var tokens = new Pl1Lexer(source).Tokenize();

        return new ParseContext(
            tokens,
            0,
            new DiagnosticBag());
    }

    private protected static CharacterTypeParser CreateCharacterTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new CharacterTypeParser(context);
    }

    private protected static BitTypeParser CreateBitTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new BitTypeParser(context);
    }

    private protected static NumericTypeParser CreateNumericTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new NumericTypeParser(context);
    }

    private protected static FloatingTypeParser CreateFloatingTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new FloatingTypeParser(context);
    }

    private protected static DataTypeParser CreateDataTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DataTypeParser(context);
    }

    private protected static InitialValueParser CreateInitialValueParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new InitialValueParser(context);
    }

    private protected static DimensionParser CreateDimensionParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DimensionParser(context);
    }

    private protected static VariableDeclarationParser CreateVariableDeclarationParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new VariableDeclarationParser(context);
    }

    private protected static StructureParser CreateStructureParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new StructureParser(context);
    }

    private protected static DeclarationParser CreateDeclarationParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DeclarationParser(context);
    }

    private protected static IReadOnlyList<Diagnostic> GetDiagnostics(ParseContext context)
    {
        return context.Diagnostics.Diagnostics;
    }
}