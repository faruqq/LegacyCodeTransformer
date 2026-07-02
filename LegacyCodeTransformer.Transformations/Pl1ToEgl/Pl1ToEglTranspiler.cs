using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;

namespace LegacyCodeTransformer.Transpilers.Pl1ToEgl
{
    /// <summary>
/// PL/I syntax tree modelini EGL syntax tree modeline dönüştürür.
///
/// Neden var?
/// ----------------------
/// Parser yalnızca PL/I dilini anlar ve Pl1SyntaxTree üretir.
/// Generator ise yalnızca EGL dilini bilir ve EglSyntaxTree üzerinden
/// kaynak kod üretir.
///
/// Bu sınıf, bu iki dünya arasındaki dönüşüm sorumluluğunu taşır.
///
/// Nerede kullanılır?
/// ----------------------
/// - Application pipeline içerisinde
/// - PL/I → EGL dönüşüm sürecinde
///
/// Gelecekte ne işe yarayacak?
/// ----------------------
/// PL/I desteği büyüdükçe IF, CALL, DO/END, assignment, procedure,
/// embedded SQL gibi yapıların EGL karşılıkları bu katmanda üretilecektir.
///
/// İlk sürümde yalnızca:
/// - PL/I DCL FIXED DECIMAL
///
/// dönüşümünü destekler.
/// </summary>
public sealed class Pl1ToEglTranspiler
{
    private readonly DiagnosticBag _diagnostics = new();

    public Pl1ToEglTranspilationResult Transpile(Pl1SyntaxTree syntaxTree)
    {
        if (syntaxTree is null)
        {
            _diagnostics.AddError("Transpile edilecek Pl1SyntaxTree null olamaz.");

            return new Pl1ToEglTranspilationResult(
                null,
                _diagnostics.Diagnostics);
        }

        var declarations = new List<EglVariableDeclaration>();

        foreach (var declaration in syntaxTree.Declarations)
        {
            var eglDeclaration = TranspileDeclaration(declaration);

            if (eglDeclaration is not null)
            {
                declarations.Add(eglDeclaration);
            }
        }

        var eglSyntaxTree = new EglSyntaxTree(
            declarations,
            syntaxTree.Location);

        return new Pl1ToEglTranspilationResult(
            eglSyntaxTree,
            _diagnostics.Diagnostics);
    }

    private EglVariableDeclaration? TranspileDeclaration(
        Pl1VariableDeclaration declaration)
    {
        var dataType = TranspileDataType(declaration.DataType);

        if (dataType is null)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I veri tipi: {declaration.DataType.GetType().Name}",
                declaration.Location));

            return null;
        }

        return new EglVariableDeclaration(
            ToLowerCamelCase(declaration.Name),
            dataType,
            declaration.Location);
    }

    private EglDataType? TranspileDataType(Pl1DataType dataType)
    {
        return dataType switch
        {
            Pl1FixedDecimalType fixedDecimalType => new EglDecimalType(
                fixedDecimalType.Precision,
                fixedDecimalType.Scale,
                fixedDecimalType.Location),

            _ => null
        };
    }

    private static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value
            .Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLowerInvariant())
            .ToArray();

        if (parts.Length == 0)
        {
            return value.ToLowerInvariant();
        }

        return parts[0] + string.Concat(
            parts
                .Skip(1)
                .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
    }
}
}
