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
    /// MustNo decimal(8);
    ///
    /// PL/I structure declaration:
    ///
    /// DCL 1 PARAME_LIST,
    ///     5 PARAM CHAR(08),
    ///     5 PARAM2 CHAR(01);
    ///
    /// EGL record declaration:
    ///
    /// record ParameList type basicRecord
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
        /// MustNo decimal(8);
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
        /// PL/I PIC / PICTURE veri tipini uygun EGL veri tipine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I PIC / PICTURE syntax'ı tek bir veri tipini temsil etmez.
        /// Aynı syntax numeric, alphanumeric, signed veya formatted alanları
        /// ifade edebilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Parser tarafından sınıflandırılmış Pl1PictureType modelini kullanarak
        /// güvenli PIC pattern'larını EGL veri tipine dönüştürür.
        ///
        /// Numeric PIC pattern'lar EGL num(p) veya num(p,s) olur.
        /// Alphanumeric PIC pattern'lar EGL char(n) olur.
        /// Formatted veya henüz desteklenmeyen PIC pattern'lar diagnostic üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - PIC '999'      => num(3)
        /// - PIC '999V99'   => num(5,2)
        /// - PIC '(13)9V99' => num(15,2)
        /// - PIC 'XXX'      => char(3)
        /// - PIC '(20)X'    => char(20)
        /// - PIC 'AAA'      => char(3)
        /// - PIC 'AXXAA'    => char(5)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// TranspileDataType içinde Pl1PictureType yakalandığında çağrılır.
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Signed numeric PIC, formatted PIC metadata ve display-format preserving
        /// mapping kararları için merkezi dönüşüm noktasıdır.
        /// </summary>
        private EglDataType? TranspilePictureType(
            Pl1PictureType pictureType)
        {
            if (pictureType.IsNumeric &&
                pictureType.Precision.HasValue)
            {
                return new EglNumType(
                    pictureType.Precision.Value,
                    pictureType.Scale,
                    pictureType.Location);
            }

            if (pictureType.IsAlphanumeric &&
                pictureType.Length.HasValue)
            {
                return new EglCharacterType(
                    pictureType.Length.Value,
                    pictureType.Location);
            }

            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen PIC pattern: {pictureType.RawPattern}",
                pictureType.Location));

            return null;
        }

        /// <summary>
        /// PL/I structure declaration modelini EGL record declaration modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I seviye numaralı structure yapıları EGL tarafında record olarak
        /// temsil edilir.
        ///
        /// P04-F ile birlikte structure içinde nested group field desteği de eklenmiştir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Structure member listesini EGL record field listesine dönüştürür.
        ///
        /// Normal typed field:
        ///
        /// 5 PARAM CHAR(08)
        ///
        /// EGL:
        ///
        /// 10 Param char(8);
        ///
        /// Nested group field:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02),
        ///     10 ILCE_KOD CHAR(03)
        ///
        /// EGL:
        ///
        /// 5 AdresBilgi char(5);
        ///     10 IlKod char(2);
        ///     10 IlceKod char(3);
        ///
        /// Structure array varsa önce parent array field üretilir:
        ///
        /// 5 Dizi char(totalLength)[arraySize];
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - Basic structure declaration
        /// - Structure array declaration
        /// - Structure member array declaration
        /// - Nested structure / group field declaration
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDeclaration dispatch methodu içerisinde
        /// - Structure declaration dönüşümünde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure, group array, sqlRecord mapping ve daha
        /// gelişmiş layout hesaplama kurallarına temel olur.
        /// </summary>
        private EglRecordDeclaration? TranspileStructureDeclaration(
            Pl1StructureDeclaration declaration)
        {
            var fields = new List<EglRecordFieldDeclaration>();

            if (declaration.ArraySize.HasValue)
            {
                var elementLength = CalculateStructureElementLength(declaration);

                if (elementLength is null)
                {
                    return null;
                }

                fields.Add(new EglRecordFieldDeclaration(
                    5,
                    IdentifierNameTransformer.Transform(
                        declaration.Name,
                        _namingOptions.Style),
                    new EglCharacterType(
                        elementLength.Value,
                        declaration.Location),
                    declaration.Location,
                    declaration.ArraySize.Value));
            }

            foreach (var member in declaration.Members)
            {
                var topLevel = declaration.ArraySize.HasValue
                    ? 10
                    : GetTopLevelEglLevel(member);

                var memberFields = TranspileStructureMember(
                    member,
                    topLevel);

                fields.AddRange(memberFields);
            }

            return new EglRecordDeclaration(
                IdentifierNameTransformer.Transform(
                    declaration.Name,
                    _namingOptions.Style),
                "basicRecord",
                fields,
                declaration.Location);
        }

        /// <summary>
        /// Root structure altındaki ilk seviye member için üretilecek EGL level değerini belirler.
        ///
        /// Neden var?
        /// ----------------------
        /// Mevcut kurum standardında normal typed structure field'lar EGL tarafında
        /// level 10 olarak üretilmektedir.
        ///
        /// Ancak nested group field parent alanları aynı record içinde group field
        /// olarak level 5 ile üretilmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Top-level normal field ile top-level group field arasındaki EGL level
        /// farkını merkezi hale getirir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Normal field:
        ///
        /// 5 PARAM CHAR(08)
        ///
        /// EGL:
        ///
        /// 10 Param char(8);
        ///
        /// Group field:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02)
        ///
        /// EGL:
        ///
        /// 5 AdresBilgi char(2);
        ///     10 IlKod char(2);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileStructureDeclaration içinde root member mapping işleminde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Farklı EGL level politikaları gerektiğinde tek noktadan yönetim sağlar.
        /// </summary>
        private static int GetTopLevelEglLevel(Pl1StructureMember member)
        {
            return member.IsGroup ? 5 : 10;
        }

        /// <summary>
        /// Structure array parent alanı için tek bir array elemanının toplam storage
        /// uzunluğunu hesaplar.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL basicRecord yapısında structure array parent field şu formatta üretilir:
        ///
        /// 5 Dizi char(15)[6];
        ///
        /// Buradaki `15` değeri, DIZI array'inin tek bir elemanını oluşturan child
        /// field uzunluklarının toplamıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Structure array parent field length değerini, typed field, field array ve
        /// nested group field bilgilerini dikkate alarak hesaplar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - CHAR(10) => 10
        /// - CHAR(10)[2] => 20
        /// - Nested group: child field toplamı
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Structure array dönüşümünde parent EGL field'ın char(n) uzunluğunu
        ///   hesaplamak için
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Nested structure, multi-level layout ve farklı numeric type storage
        /// hesaplarının merkezi olarak genişletilmesine temel olur.
        /// </summary>
        private int? CalculateStructureElementLength(
            Pl1StructureDeclaration declaration)
        {
            var totalLength = 0;

            foreach (var member in declaration.Members)
            {
                var memberLength = CalculateMemberLength(member);

                if (memberLength is null)
                {
                    return null;
                }

                totalLength += memberLength.Value;
            }

            return totalLength;
        }

        /// <summary>
        /// PL/I structure member alanının storage length değerini hesaplar.
        ///
        /// Neden var?
        /// ----------------------
        /// P04-F ile birlikte structure member yalnızca basit typed field değildir.
        ///
        /// Member:
        ///
        /// - Normal typed field olabilir.
        /// - Field array olabilir.
        /// - Nested group field olabilir.
        ///
        /// Bu nedenle length hesabı tek veri tipinden ibaret değildir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Typed field için data type length değerini hesaplar.
        ///
        /// Field array için array çarpanını uygular.
        ///
        /// Nested group için child member length toplamını hesaplar.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - CHAR(10) => 10
        /// - CHAR(10)[2] => 20
        /// - GROUP => child length toplamı
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - CalculateStructureElementLength içinde
        /// - TranspileStructureMember içinde group parent field char(n) üretirken
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure, group array ve farklı PL/I storage type
        /// hesaplarını merkezi hale getirir.
        /// </summary>
        private int? CalculateMemberLength(Pl1StructureMember member)
        {
            int? memberLength;

            if (member.IsGroup)
            {
                var totalChildLength = 0;

                foreach (var childMember in member.Members)
                {
                    var childLength = CalculateMemberLength(childMember);

                    if (childLength is null)
                    {
                        return null;
                    }

                    totalChildLength += childLength.Value;
                }

                memberLength = totalChildLength;
            }
            else
            {
                if (member.DataType is null)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        $"Structure member uzunluğu hesaplanamayan PL/I member veri tipi bulunamadı: {member.Name}",
                        member.Location));

                    return null;
                }

                memberLength = CalculateDataTypeLength(member.DataType);

                if (memberLength is null)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        $"Structure member uzunluğu hesaplanamayan PL/I member veri tipi: {member.DataType.GetType().Name}",
                        member.Location));

                    return null;
                }
            }

            var arrayMultiplier = member.ArraySize ?? 1;

            return memberLength.Value * arrayMultiplier;
        }

        /// <summary>
        /// PL/I veri tipinin fixed storage uzunluğunu hesaplar.
        ///
        /// Neden var?
        /// ----------------------
        /// Structure array parent field uzunluğu ve nested group field uzunluğu,
        /// child member veri tiplerinin storage uzunlukları üzerinden hesaplanır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Veri tipi bazlı temel storage length hesabını merkezi hale getirir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - CHAR(08) => 8
        /// - VARCHAR(50) => 50
        /// - FIXED DECIMAL(9,4) => 9
        /// - FIXED BIN(15) => 2
        /// - FIXED BIN(31) => 4
        /// - PIC '999' => 3
        /// - PIC '999V99' => 5
        /// - PIC 'XXX' => 3
        /// - PIC '(20)X' => 20
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - CalculateMemberLength içinde
        /// - Structure array parent length hesabında
        /// - Nested group field length hesabında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// BIT, formatted PIC ve farklı storage type hesapları geldikçe
        /// bu method genişletilecektir.
        /// </summary>
        private static int? CalculateDataTypeLength(Pl1DataType dataType)
        {
            return dataType switch
            {
                Pl1CharacterType characterType => characterType.Length,
                Pl1VarcharType varcharType => varcharType.Length,
                Pl1FixedDecimalType fixedDecimalType => fixedDecimalType.Precision,
                Pl1FixedBinaryType { Precision: 15, Scale: null } => 2,
                Pl1FixedBinaryType { Precision: 15, Scale: 0 } => 2,
                Pl1FixedBinaryType { Precision: 31, Scale: null } => 4,
                Pl1FixedBinaryType { Precision: 31, Scale: 0 } => 4,
                Pl1PictureType { IsNumeric: true, Precision: not null } pictureType =>
                    pictureType.Precision.Value,
                Pl1PictureType { IsAlphanumeric: true, Length: not null } pictureType =>
                    pictureType.Length.Value,
                _ => null
            };
        }

        /// <summary>
        /// PL/I structure member modelini bir veya daha fazla EGL record field modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure member alanları artık yalnızca tek typed field olmak
        /// zorunda değildir.
        ///
        /// Bir member:
        ///
        /// - Normal typed field olabilir.
        /// - Field array olabilir.
        /// - Nested group field olabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Normal field için tek EGL field üretir.
        ///
        /// Nested group için önce group parent field üretir, ardından child member
        /// alanlarını bir sonraki EGL level ile üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Normal field:
        ///
        /// 5 PARAM CHAR(08)
        ///
        /// Field array:
        ///
        /// 5 PARAM_LIST(2) CHAR(10)
        ///
        /// Nested group:
        ///
        /// 5 ADRES_BILGI,
        ///     10 IL_KOD CHAR(02),
        ///     10 ILCE_KOD CHAR(03)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileStructureDeclaration içerisinde
        /// - Nested group child field üretiminde recursive olarak
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure, group array ve layout hesaplama desteğini
        /// genişletmek için temel dönüşüm noktasıdır.
        /// </summary>
        private IReadOnlyList<EglRecordFieldDeclaration> TranspileStructureMember(
            Pl1StructureMember member,
            int eglLevel)
        {
            var fields = new List<EglRecordFieldDeclaration>();

            if (member.IsGroup)
            {
                var groupLength = CalculateMemberLength(member);

                if (groupLength is null)
                {
                    return fields;
                }

                fields.Add(new EglRecordFieldDeclaration(
                    eglLevel,
                    IdentifierNameTransformer.Transform(
                        member.Name,
                        _namingOptions.Style),
                    new EglCharacterType(
                        groupLength.Value,
                        member.Location),
                    member.Location,
                    member.ArraySize));

                foreach (var childMember in member.Members)
                {
                    var childFields = TranspileStructureMember(
                        childMember,
                        eglLevel + 5);

                    fields.AddRange(childFields);
                }

                return fields;
            }

            if (member.DataType is null)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"PL/I structure member veri tipi bulunamadı: {member.Name}",
                    member.Location));

                return fields;
            }

            var dataType = TranspileDataType(member.DataType);

            if (dataType is null)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"Desteklenmeyen PL/I structure member veri tipi: {member.DataType.GetType().Name}",
                    member.Location));

                return fields;
            }

            fields.Add(new EglRecordFieldDeclaration(
                eglLevel,
                IdentifierNameTransformer.Transform(
                    member.Name,
                    _namingOptions.Style),
                dataType,
                member.Location,
                member.ArraySize));

            return fields;
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
        /// Ne çözüyor?
        /// ----------------------
        /// PL/I veri tipi modellerini hedef EGL veri tipi modellerine dönüştürür.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - FIXED DECIMAL(15) => decimal(15)
        /// - FIXED DECIMAL(15,0) => decimal(15,0)
        /// - CHAR(08) => char(8)
        /// - VARCHAR(50) => char(50)
        /// - FIXED BIN(15) => smallint
        /// - FIXED BIN(31) => int
        /// - PIC '999' => num(3)
        /// - PIC '999V99' => num(5,2)
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I → EGL Transpiler içerisinde variable declaration dönüşümünde
        /// - PL/I → EGL Transpiler içerisinde record field dönüşümünde
        /// - Nested group child field dönüşümünde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// BIT, formatted PIC ve sqlRecord özel tip mapping kuralları eklendikçe
        /// bu method genişletilecektir.
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

                Pl1FixedBinaryType fixedBinaryType =>
                    TranspileFixedBinaryType(fixedBinaryType),

                Pl1CharacterType characterType =>
                    new EglCharacterType(
                        characterType.Length,
                        characterType.Location),

                Pl1VarcharType varcharType =>
                    new EglCharacterType(
                        varcharType.Length,
                        varcharType.Location),

                Pl1PictureType pictureType =>
                    TranspilePictureType(pictureType),

                _ => null
            };
        }

        /// <summary>
        /// PL/I FIXED BINARY / FIXED BIN veri tipini EGL integer tipine dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I binary fixed alanlar binary integer semantic taşır.
        ///
        /// İlk kapsamda yalnızca scale değeri olmayan veya scale değeri 0 olan
        /// integer kullanımlar desteklenecektir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Yaygın binary integer precision değerlerini EGL integer type karşılıklarına
        /// dönüştürür.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - FIXED BIN(15) => smallint
        /// - BIN FIXED(15) => smallint
        /// - FIXED BIN(31) => int
        /// - BIN FIXED(31) => int
        /// - FIXED BIN(15,0) => smallint
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDataType içinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Farklı binary precision değerleri veya binary fractional mapping
        /// kararları alındığında bu method genişletilecektir.
        /// </summary>
        private EglDataType? TranspileFixedBinaryType(
            Pl1FixedBinaryType fixedBinaryType)
        {
            if (fixedBinaryType.Scale.HasValue &&
                fixedBinaryType.Scale.Value != 0)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    $"FIXED BINARY scale değeri desteklenmiyor. Precision: {fixedBinaryType.Precision}, Scale: {fixedBinaryType.Scale.Value}",
                    fixedBinaryType.Location));

                return null;
            }

            return fixedBinaryType.Precision switch
            {
                15 => new EglSmallIntType(fixedBinaryType.Location),
                31 => new EglIntType(fixedBinaryType.Location),
                _ => CreateUnsupportedFixedBinaryPrecisionDiagnostic(fixedBinaryType)
            };
        }

        /// <summary>
        /// Desteklenmeyen FIXED BINARY precision değeri için diagnostic üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// Binary fixed precision değerleri yanlış EGL type'a sessizce çevrilmemelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Desteklenmeyen precision değerlerini görünür hata haline getirir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// FIXED BIN(13) gibi ilk kapsamda mapping kararı olmayan kullanımlar.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileFixedBinaryType içinde
        /// </summary>
        private EglDataType? CreateUnsupportedFixedBinaryPrecisionDiagnostic(
            Pl1FixedBinaryType fixedBinaryType)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"Desteklenmeyen FIXED BINARY precision değeri: {fixedBinaryType.Precision}",
                fixedBinaryType.Location));

            return null;
        }
    }
}