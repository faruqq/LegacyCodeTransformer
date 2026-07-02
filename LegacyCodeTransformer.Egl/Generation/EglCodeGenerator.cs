using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using System.Text;

namespace LegacyCodeTransformer.Egl.Generation
{
    /// <summary>
    /// EglSyntaxTree modelinden EGL kaynak kodu üretir.
    ///
    /// Neden var?
    /// ----------------------
    /// Transpiler yalnızca hedef dil modeli olan EglSyntaxTree üretir.
    /// Gerçek .egl kaynak kodunu yazdırma sorumluluğu bu sınıfa aittir.
    ///
    /// Bu sayede Transpiler dönüşüm kurallarına odaklanır,
    /// Generator ise yalnızca EGL kod formatına odaklanır.
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application pipeline içerisinde
    /// - PL/I → EGL dönüşümünün son aşamasında
    /// - Gelecekte CLI, GUI veya IDE entegrasyonlarında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// EGL desteği genişledikçe variable declaration dışında function,
    /// record, service, statement ve expression üretimi de bu sınıfta
    /// geliştirilecektir.
    ///
    /// İlk sürümde yalnızca EGL variable declaration üretir.
    /// </summary>
    public sealed class EglCodeGenerator
    {
        /// <summary>
        /// Verilen EglSyntaxTree modelini EGL kaynak koduna dönüştürür.
        /// </summary>
        public string Generate(EglSyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            foreach (var declaration in syntaxTree.Declarations)
            {
                builder.AppendLine(GenerateVariableDeclaration(declaration));
            }

            return builder.ToString();
        }

        private static string GenerateVariableDeclaration(EglVariableDeclaration declaration)
        {
            return $"{declaration.Name} {GenerateDataType(declaration.DataType)};";
        }

        private static string GenerateDataType(EglDataType dataType)
        {
            return dataType switch
            {
                EglDecimalType decimalType => $"decimal({decimalType.Precision},{decimalType.Scale})",
                _ => "unknown"
            };
        }
    }
}
