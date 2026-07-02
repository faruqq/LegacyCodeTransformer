using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.Syntax;
using LegacyCodeTransformer.Pl1.Types;
using LegacyCodeTransformer.Transpilers.Naming;

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
        private readonly IdentifierNamingOptions _namingOptions;

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Transpiler, PL/I syntax tree modelini EGL syntax tree modeline dönüştürür.
        /// Bu dönüşüm sırasında identifier isimleri de hedef dil standardına göre
        /// dönüştürülmelidir.
        ///
        /// Varsayılan olarak PascalCase naming strategy kullanılır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application pipeline içerisinde
        /// - Unit testlerde modelden modele dönüşüm doğrulamasında
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// CLI veya UI üzerinden farklı naming style seçimi geldiğinde bu constructor
        /// üzerinden Transpiler'a options verilebilecektir.
        /// </summary>
        public Pl1ToEglTranspiler()
            : this(IdentifierNamingOptions.Default)
        {
        }

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını verilen naming options ile oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I identifier adlarının EGL tarafında Preserve, CamelCase veya
        /// PascalCase gibi farklı stratejilerle üretilebilmesi gerekir.
        ///
        /// Örnek:
        ///
        /// MUST_NO
        ///
        /// PascalCase:
        /// MustNo
        ///
        /// CamelCase:
        /// mustNo
        ///
        /// Preserve:
        /// MUST_NO
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Unit testlerde farklı naming strategy davranışlarını doğrulamada
        /// - Application pipeline içerisinde kullanıcı seçimi geldiğinde
        /// </summary>
        public Pl1ToEglTranspiler(IdentifierNamingOptions namingOptions)
        {
            _namingOptions = namingOptions;
        }

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
                IdentifierNameTransformer.Transform(
                    declaration.Name,
                    _namingOptions.Style),
                dataType,
                declaration.Location);
        }

        /// <summary>
        /// PL/I veri tipini EGL veri tipine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser çıktısı PL/I diline ait syntax tree modellerini üretir.
        /// Code generator ise EGL diline ait modeller üzerinden çalışır.
        /// Bu nedenle PL/I data type modellerinin EGL data type modellerine
        /// çevrilmesi gerekir.
        ///
        /// Örnek dönüşümler:
        ///
        /// PL/I:
        /// FIXED DECIMAL(8)
        ///
        /// EGL:
        /// decimal(8,0)
        ///
        /// PL/I:
        /// CHAR(08)
        ///
        /// EGL:
        /// char(8)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I → EGL Transpiler içerisinde declaration dönüşümünde
        /// - Application pipeline içinde parser sonrası model dönüşümünde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// BIT, FIXED BINARY, PIC / PICTURE, VARYING ve structure field tipleri
        /// desteklendikçe bu method yeni mapping kurallarıyla genişletilecektir.
        /// </summary>
        private EglDataType? TranspileDataType(Pl1DataType dataType)
        {
            return dataType switch
            {
                Pl1FixedDecimalType fixedDecimalType =>
                    new EglDecimalType(
                        fixedDecimalType.Precision,
                        fixedDecimalType.Scale,
                        fixedDecimalType.Location),

                Pl1CharacterType characterType =>
                    new EglCharacterType(
                        characterType.Length,
                        characterType.Location),

                _ => null
            };
        }
    }
}
