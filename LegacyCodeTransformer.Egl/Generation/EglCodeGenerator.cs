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
                builder.Append(GenerateDeclaration(declaration));
            }

            return builder.ToString();
        }

        /// <summary>
        /// EGL declaration modelinden kaynak kod karşılığını üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL syntax tree artık yalnızca variable declaration taşımaz.
        /// Record declaration gibi farklı declaration türleri de desteklenir.
        ///
        /// Bu method declaration türüne göre doğru generator methoduna yönlendirir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Generate ana akışı içerisinde
        /// - EGL declaration listesinden kaynak kod üretirken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Function, service, library ve statement üretimleri eklendiğinde
        /// generator dispatch davranışı burada genişletilecektir.
        /// </summary>
        private static string GenerateDeclaration(EglDeclaration declaration)
        {
            return declaration switch
            {
                EglVariableDeclaration variableDeclaration =>
                    GenerateVariableDeclaration(variableDeclaration) + Environment.NewLine,

                EglRecordDeclaration recordDeclaration =>
                    GenerateRecordDeclaration(recordDeclaration),

                _ => string.Empty
            };
        }

        /// <summary>
        /// EGL record declaration modelinden record kaynak kodunu üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure declaration ifadeleri EGL tarafında record olarak
        /// üretilecektir.
        ///
        /// Örnek EGL:
        ///
        /// record ParameList type BasicRecord
        ///     10 Param char(8);
        ///     10 Param2 char(1);
        /// end
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - GenerateDeclaration içerisinde
        /// - EglRecordDeclaration kaynak kodu üretiminde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Record description metadata, field metadata ve annotation üretimi
        /// geldiğinde bu method genişletilecektir.
        /// </summary>
        private static string GenerateRecordDeclaration(
            EglRecordDeclaration declaration)
        {
            var builder = new StringBuilder();

            builder.AppendLine(
                $"record {declaration.Name} type {declaration.RecordType}");

            foreach (var field in declaration.Fields)
            {
                builder.AppendLine(
                    $"    {GenerateRecordFieldDeclaration(field)}");
            }

            builder.AppendLine("end");

            return builder.ToString();
        }

        /// <summary>
        /// EGL record field declaration modelinden field kaynak kodunu üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// Record içindeki field alanları variable declaration formatından farklı
        /// olarak level bilgisiyle birlikte yazılır.
        ///
        /// Örnek:
        ///
        /// 10 Param char(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - GenerateRecordDeclaration içerisinde
        /// - EglRecordFieldDeclaration kaynak kodu üretiminde
        /// </summary>
        private static string GenerateRecordFieldDeclaration(
            EglRecordFieldDeclaration field)
        {
            return $"{field.Level} {field.Name} {GenerateDataType(field.DataType)};";
        }

        private static string GenerateVariableDeclaration(EglVariableDeclaration declaration)
        {
            return $"{declaration.Name} {GenerateDataType(declaration.DataType)};";
        }

        /// <summary>
        /// EGL veri tipi modelinden kaynak kod karşılığını üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL syntax tree üzerindeki veri tipi modelleri doğrudan string değildir.
        /// Code generator aşamasında bu modellerin gerçek EGL kaynak koduna
        /// dönüştürülmesi gerekir.
        ///
        /// Örnek modeller:
        ///
        /// EglDecimalType
        /// - Precision: 8
        /// - Scale: 0
        ///
        /// Üretilen EGL:
        /// decimal(8,0)
        ///
        /// EglCharacterType
        /// - Length: 8
        ///
        /// Üretilen EGL:
        /// char(8)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - EGL variable declaration üretiminde
        /// - Application pipeline sonucunda hedef kaynak kod oluşturulurken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Record field üretimi, array tipleri, nullable/default value gibi
        /// EGL söz dizimleri desteklendiğinde data type üretimi merkezi olarak
        /// buradan yönetilecektir.
        /// </summary>
        private static string GenerateDataType(EglDataType dataType)
        {
            return dataType switch
            {
                EglDecimalType decimalType =>
                    $"decimal({decimalType.Precision},{decimalType.Scale})",

                EglCharacterType characterType =>
                    $"char({characterType.Length})",

                _ => "unknown"
            };
        }
    }
}
