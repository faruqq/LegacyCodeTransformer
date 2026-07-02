using System.Collections.Generic;
using LegacyCodeTransformer.Core.Diagnostics;
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
    /// PL/I tarafındaki modelleri EGL tarafındaki modellere dönüştürür.
    ///
    /// Örnek dönüşümler:
    ///
    /// PL/I variable declaration:
    ///
    /// DCL MUST_NO FIXED DECIMAL(8);
    ///
    /// EGL variable declaration:
    ///
    /// MustNo decimal(8,0);
    ///
    /// PL/I structure declaration:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08),
    ///     5 PARAM2 CHAR(01);
    ///
    /// EGL record declaration:
    ///
    /// record ParameList type BasicRecord
    ///     10 Param char(8);
    ///     10 Param2 char(1);
    /// end
    ///
    /// Nerede kullanılır?
    /// ----------------------
    /// - Application pipeline içerisinde
    /// - PL/I → EGL dönüşüm sürecinde
    /// - Parser ve Generator aşamaları arasında
    ///
    /// Gelecekte ne işe yarayacak?
    /// ----------------------
    /// PL/I desteği büyüdükçe IF, CALL, DO/END, assignment, procedure,
    /// embedded SQL, array declaration, nested structure gibi yapıların
    /// EGL karşılıkları bu katmanda üretilecektir.
    /// </summary>
    public sealed class Pl1ToEglTranspiler
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly IdentifierNamingOptions _namingOptions;

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını varsayılan ayarlarla oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// En basit kullanımda dışarıdan naming options verilmeden
        /// transpiler oluşturulabilmelidir.
        ///
        /// Varsayılan naming strategy IdentifierNamingOptions.Default
        /// üzerinden gelir.
        ///
        /// Mevcut kararımıza göre varsayılan identifier dönüşümü
        /// PascalCase davranışıdır.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application pipeline içerisinde
        /// - Unit testlerde varsayılan dönüşüm davranışını doğrulamada
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// CLI veya UI üzerinden farklı naming style seçimi geldiğinde
        /// diğer constructor kullanılacaktır.
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
        /// - Gelecekte CLI parametresi üzerinden naming style verildiğinde
        /// </summary>
        public Pl1ToEglTranspiler(IdentifierNamingOptions namingOptions)
        {
            _namingOptions = namingOptions ?? IdentifierNamingOptions.Default;
        }

        /// <summary>
        /// PL/I syntax tree modelini EGL syntax tree modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser çıktısı olan Pl1SyntaxTree doğrudan EGL kaynak koduna
        /// yazdırılamaz.
        ///
        /// Önce PL/I modellerinin EGL modellerine dönüştürülmesi gerekir.
        ///
        /// Bu method syntax tree içindeki declaration listesini gezer ve
        /// her declaration türünü uygun EGL declaration türüne dönüştürür.
        ///
        /// Desteklenen declaration dönüşümleri:
        ///
        /// - Pl1VariableDeclaration -> EglVariableDeclaration
        /// - Pl1StructureDeclaration -> EglRecordDeclaration
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - ConversionService içerisinde
        /// - Transpiler unit testlerinde
        /// - Application uçtan uca dönüşüm testlerinde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Yeni PL/I declaration türleri eklendikçe TranspileDeclaration
        /// dispatch methodu genişletilecektir.
        /// </summary>
        public Pl1ToEglTranspilationResult Transpile(Pl1SyntaxTree syntaxTree)
        {
            if (syntaxTree is null)
            {
                _diagnostics.AddError("Transpile edilecek Pl1SyntaxTree null olamaz.");

                return new Pl1ToEglTranspilationResult(
                    null,
                    _diagnostics.Diagnostics);
            }

            var declarations = new List<EglDeclaration>();

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

        /// <summary>
        /// PL/I declaration modelini uygun EGL declaration modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I syntax tree artık yalnızca tekil variable declaration taşımaz.
        /// Structure declaration gibi farklı declaration türleri de aynı
        /// declaration listesi içerisinde yer alabilir.
        ///
        /// Bu method declaration türüne göre doğru dönüşüm methoduna yönlendirir.
        ///
        /// Desteklenen dönüşümler:
        ///
        /// - Pl1VariableDeclaration -> EglVariableDeclaration
        /// - Pl1StructureDeclaration -> EglRecordDeclaration
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Transpile ana akışı içerisinde
        /// - PL/I declaration listesini EGL declaration listesine dönüştürürken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Procedure, file declaration, based declaration, factored declaration
        /// gibi yeni declaration türleri eklendiğinde dispatch davranışı
        /// burada genişletilecektir.
        /// </summary>
        private EglDeclaration? TranspileDeclaration(Pl1Declaration declaration)
        {
            return declaration switch
            {
                Pl1VariableDeclaration variableDeclaration =>
                    TranspileVariableDeclaration(variableDeclaration),

                Pl1StructureDeclaration structureDeclaration =>
                    TranspileStructureDeclaration(structureDeclaration),

                _ => CreateUnsupportedDeclarationDiagnostic(declaration)
            };
        }

        /// <summary>
        /// PL/I tekil değişken declaration modelini EGL variable declaration modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// DCL ile tanımlanan basit değişkenler EGL tarafında doğrudan
        /// variable declaration olarak üretilir.
        ///
        /// Örnek PL/I:
        ///
        /// DCL MUST_NO FIXED DECIMAL(8);
        ///
        /// Örnek EGL:
        ///
        /// MustNo decimal(8,0);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDeclaration dispatch methodu içerisinde
        /// - Tekil variable declaration dönüşümlerinde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Array dimension ve default value mapping kuralları eklendiğinde
        /// bu method genişletilecektir.
        /// </summary>
        private EglVariableDeclaration? TranspileVariableDeclaration(
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
        /// PL/I structure declaration modelini EGL record declaration modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I seviye numaralı structure yapıları EGL tarafında record olarak
        /// temsil edilecektir.
        ///
        /// Örnek PL/I:
        ///
        /// DCL 1 PARAME_LIST,
        ///     5 PARAM CHAR(08),
        ///     5 PARAM2 CHAR(01);
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
        /// - TranspileDeclaration dispatch methodu içerisinde
        /// - Structure declaration dönüşümünde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Structure array, nested structure, record metadata ve field
        /// description üretimi geldiğinde bu method genişletilecektir.
        /// </summary>
        private EglRecordDeclaration? TranspileStructureDeclaration(
            Pl1StructureDeclaration declaration)
        {
            var fields = new List<EglRecordFieldDeclaration>();

            foreach (var member in declaration.Members)
            {
                var field = TranspileStructureMember(member);

                if (field is not null)
                {
                    fields.Add(field);
                }
            }

            return new EglRecordDeclaration(
                IdentifierNameTransformer.Transform(
                    declaration.Name,
                    _namingOptions.Style),
                "BasicRecord",
                fields,
                declaration.Location);
        }

        /// <summary>
        /// PL/I structure member modelini EGL record field modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure içindeki her member, EGL record içerisinde field
        /// olarak üretilmelidir.
        ///
        /// Örnek PL/I member:
        ///
        /// 5 PARAM CHAR(08) INIT(' ')
        ///
        /// Örnek EGL field:
        ///
        /// 10 Param char(8);
        ///
        /// İlk kapsamda EGL field level değeri sabit 10 olarak üretilecektir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileStructureDeclaration içerisinde
        /// - Record field listesi oluşturulurken
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Nested structure ve farklı EGL level üretim kuralları geldiğinde
        /// member level bilgisi burada kullanılabilir.
        /// </summary>
        private EglRecordFieldDeclaration? TranspileStructureMember(
            Pl1StructureMember member)
        {
            var dataType = TranspileDataType(member.DataType);

            if (dataType is null)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"Desteklenmeyen PL/I structure member veri tipi: {member.DataType.GetType().Name}",
                    member.Location));

                return null;
            }

            return new EglRecordFieldDeclaration(
                10,
                IdentifierNameTransformer.Transform(
                    member.Name,
                    _namingOptions.Style),
                dataType,
                member.Location);
        }

        /// <summary>
        /// Desteklenmeyen PL/I declaration türü için diagnostic üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// Syntax tree ileride farklı declaration türleri taşıyabilir.
        /// Transpiler henüz desteklemediği bir declaration türü gördüğünde
        /// sessizce yok saymak yerine diagnostic üretmelidir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDeclaration switch expression default kolunda
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// Yeni declaration türleri eklendiğinde eksik mapping'leri erken
        /// fark etmeyi sağlar.
        /// </summary>
        private EglDeclaration? CreateUnsupportedDeclarationDiagnostic(
            Pl1Declaration declaration)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PL/I declaration türü: {declaration.GetType().Name}",
                declaration.Location));

            return null;
        }

        /// <summary>
        /// PL/I veri tipini EGL veri tipine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// Parser çıktısı PL/I diline ait syntax tree modellerini üretir.
        /// Code generator ise EGL diline ait modeller üzerinden çalışır.
        ///
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
        /// - PL/I → EGL Transpiler içerisinde variable declaration dönüşümünde
        /// - PL/I → EGL Transpiler içerisinde record field dönüşümünde
        /// - Application pipeline içinde parser sonrası model dönüşümünde
        ///
        /// Gelecekte ne işe yarayacak?
        /// ----------------------
        /// BIT, FIXED BINARY, PIC / PICTURE, VARYING ve structure field
        /// tipleri desteklendikçe bu method yeni mapping kurallarıyla
        /// genişletilecektir.
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