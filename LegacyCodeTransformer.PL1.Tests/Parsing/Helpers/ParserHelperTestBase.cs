using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Pl1.Lexing;
using LegacyCodeTransformer.Pl1.Parsing.Helpers;

namespace LegacyCodeTransformer.Pl1.Tests.Parsing.Helpers;

public abstract class ParserHelperTestBase
{
    protected static ParseContext CreateContext(string source)
    {
        var tokens = new Pl1Lexer(source).Tokenize();

        return new ParseContext(
            tokens,
            0,
            new DiagnosticBag());
    }

    protected static CharacterTypeParser CreateCharacterTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new CharacterTypeParser(context);
    }

    protected static BitTypeParser CreateBitTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new BitTypeParser(context);
    }

    protected static NumericTypeParser CreateNumericTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new NumericTypeParser(context);
    }

    protected static FloatingTypeParser CreateFloatingTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new FloatingTypeParser(context);
    }

    protected static DataTypeParser CreateDataTypeParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DataTypeParser(context);
    }

    protected static InitialValueParser CreateInitialValueParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new InitialValueParser(context);
    }

    protected static DimensionParser CreateDimensionParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DimensionParser(context);
    }

    protected static VariableDeclarationParser CreateVariableDeclarationParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new VariableDeclarationParser(context);
    }

    protected static StructureParser CreateStructureParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new StructureParser(context);
    }

    protected static DeclarationParser CreateDeclarationParser(
        string source,
        out ParseContext context)
    {
        context = CreateContext(source);

        return new DeclarationParser(context);
    }

    protected static IReadOnlyList<Diagnostic> GetDiagnostics(ParseContext context)
    {
        return context.Diagnostics.Diagnostics;
    }
}