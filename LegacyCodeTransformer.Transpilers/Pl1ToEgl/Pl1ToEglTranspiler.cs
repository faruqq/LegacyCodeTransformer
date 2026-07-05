using LegacyCodeTransformer.Core.Diagnostics;
using LegacyCodeTransformer.Core.Syntax;
using LegacyCodeTransformer.Egl.Declarations;
using LegacyCodeTransformer.Egl.InitialValues;
using LegacyCodeTransformer.Egl.Syntax;
using LegacyCodeTransformer.Egl.Types;
using LegacyCodeTransformer.Pl1.Declarations;
using LegacyCodeTransformer.Pl1.InitialValues;
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
        private readonly Pl1ToEglTranspilerOptions _options;

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını varsayılan ayarlarla oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// En basit kullanımda dışarıdan options verilmeden transpiler oluşturulabilmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Varsayılan olarak PascalCase identifier naming ve basicRecord record type üretimini kullanır.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// new Pl1ToEglTranspiler()
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Application pipeline içerisinde
        /// - Unit testlerde varsayılan dönüşüm davranışını doğrulamada
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Transpiler option sayısı artsa bile default constructor geriye uyumlu kalır.
        /// </summary>
        public Pl1ToEglTranspiler()
            : this(Pl1ToEglTranspilerOptions.Default)
        {
        }

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını verilen naming options ile oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Önceki API kullanımını bozmadan yalnızca identifier naming strategy verilmesini desteklemek gerekir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Eski constructor davranışını korur ve naming options bilgisini yeni Pl1ToEglTranspilerOptions modeline adapte eder.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// new Pl1ToEglTranspiler(new IdentifierNamingOptions(IdentifierNamingStyle.CamelCase))
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - Mevcut unit testlerde
        /// - ConversionService mevcut overload içinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Geriye uyumlu API korunurken yeni options modeli ana giriş noktası haline gelir.
        /// </summary>
        public Pl1ToEglTranspiler(IdentifierNamingOptions namingOptions)
            : this(new Pl1ToEglTranspilerOptions(namingOptions))
        {
        }

        /// <summary>
        /// PL/I → EGL Transpiler instance'ını verilen transpiler options ile oluşturur.
        ///
        /// Neden var?
        /// ----------------------
        /// Naming strategy dışında record type strategy gibi yeni dönüşüm ayarları da yönetilebilir olmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Transpiler davranışını tek options modeli üzerinden konfigüre eder.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// new Pl1ToEglTranspiler(new Pl1ToEglTranspilerOptions(
        ///     IdentifierNamingOptions.Default,
        ///     EglRecordTypeStrategy.SqlRecord))
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - sqlRecord mapping testlerinde
        /// - Application pipeline options overload içinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// sqlRecord metadata, default value üretimi ve farklı EGL output ayarları bu options modeli üzerinden genişletilebilir.
        /// </summary>
        public Pl1ToEglTranspiler(Pl1ToEglTranspilerOptions options)
        {
            _options = options ?? Pl1ToEglTranspilerOptions.Default;
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
        /// DCL ile tanımlanan basit değişkenler EGL tarafında doğrudan variable declaration olarak üretilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// PL/I scalar, array ve güvenli başlangıç değeri taşıyan variable declaration bilgisini EGL variable declaration modeline taşır.
        /// Veri tipi dönüşümü başarısız olursa diagnostic tekrarını engeller.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - DCL MUST_NO FIXED DECIMAL(8); => MustNo decimal(8);
        /// - DCL PARAM CHAR(10) DIM(2); => Param char(10)[2];
        /// - DCL PARAM CHAR(4) INIT('ABCD'); => Param char(4) = "ABCD";
        /// - DCL FLAG BIT(1); => tek BIT diagnostic üretir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDeclaration dispatch methodu içerisinde
        /// - Tekil variable declaration dönüşümlerinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Numeric default value, array initialization ve record field default value mapping kuralları eklendiğinde bu method genişletilecektir.
        /// </summary>
        private EglVariableDeclaration? TranspileVariableDeclaration(
            Pl1VariableDeclaration declaration)
        {
            var diagnosticCountBeforeDataType = _diagnostics.Diagnostics.Count;
            var dataType = TranspileDataType(declaration.DataType);

            if (dataType is null)
            {
                if (_diagnostics.Diagnostics.Count == diagnosticCountBeforeDataType)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        $"Desteklenmeyen PL/I veri tipi: {declaration.DataType.GetType().Name}",
                        declaration.Location));
                }

                return null;
            }

            var initialValue = TranspileInitialValue(
                declaration.InitialValue,
                declaration.Location);

            return new EglVariableDeclaration(
                IdentifierNameTransformer.Transform(
                    declaration.Name,
                    _options.NamingOptions.Style),
                dataType,
                declaration.Location,
                declaration.ArraySize,
                initialValue);
        }

        /// <summary>
        /// PL/I INIT / INITIAL bilgisini EGL initial value modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I parser başlangıç değerini Pl1InitialValue modeli üzerinde korur.
        /// EGL output üretilebilmesi için bu bilginin hedef dil modeli olan EglInitialValue
        /// modeline çevrilmesi gerekir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Güvenli scalar başlangıç değerlerini EGL syntax tree'ye taşır.
        /// RepeatCount veya AppliesToAllElements içeren initialization ifadelerini şimdilik output'a çevirmeden diagnostic üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Desteklenen:
        /// - INIT('ABCD')
        /// - INITIAL(';')
        ///
        /// Şimdilik diagnostic:
        /// - INIT((08)' ')
        /// - INIT((*)' ')
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileVariableDeclaration içinde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Array initialization, repeat factor expansion, record field default value ve numeric initialization mapping davranışlarına temel olur.
        /// </summary>
        private EglInitialValue? TranspileInitialValue(
            Pl1InitialValue? initialValue,
            SourceLocation location)
        {
            if (initialValue is null)
            {
                return null;
            }

            if (initialValue.RepeatCount.HasValue ||
                initialValue.AppliesToAllElements)
            {
                _diagnostics.Add(new Diagnostic(
                    DiagnosticSeverity.Error,
                    "INIT repeat factor veya (*) all-elements initialization için EGL default value mapping henüz desteklenmiyor.",
                    location));

                return null;
            }

            return new EglInitialValue(
                initialValue.Value,
                initialValue.Location);
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
        /// PL/I seviye numaralı structure yapıları EGL tarafında record olarak temsil edilir.
        /// Firmadaki DB2 erişim fonksiyonlarında sqlRecord kullanımı gerektiği için record type seçimi explicit strategy ile yönetilmelidir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Structure member listesini EGL record field listesine dönüştürür.
        /// Record type değerini hardcoded basicRecord yerine Pl1ToEglTranspilerOptions.RecordTypeStrategy üzerinden belirler.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// Default strategy:
        /// record CustomerInfo type basicRecord
        ///
        /// SqlRecord strategy:
        /// record CustomerInfo type sqlRecord
        ///
        /// Ayrıca şu PL/I yapılarını destekler:
        /// - Basic structure declaration
        /// - Structure array declaration
        /// - Structure member array declaration
        /// - Nested structure / group field declaration
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDeclaration dispatch methodu içerisinde
        /// - Structure declaration dönüşümünde
        /// - basicRecord / sqlRecord output üretiminde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// sqlRecord metadata, table name, column metadata, SQL annotation ve DCLGEN tabanlı mapping kuralları bu methodun oluşturduğu record model üzerine eklenecektir.
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
                        _options.NamingOptions.Style),
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
                    _options.NamingOptions.Style),
                GetEglRecordType(),
                fields,
                declaration.Location);
        }

        /// <summary>
        /// Transpiler options değerine göre EGL record type string karşılığını üretir.
        ///
        /// Neden var?
        /// ----------------------
        /// EGL tarafında record declaration type değeri basicRecord veya sqlRecord olabilir.
        /// Firma DB2 erişim fonksiyonlarında sqlRecord kullanımı gerektiği için bu değer kontrollü şekilde seçilebilir olmalıdır.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Record type strategy bilgisini EGL output'ta kullanılacak exact keyword değerine dönüştürür.
        /// basicRecord ve sqlRecord casing değerleri EGL output standardına uygun olarak birebir korunur.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - EglRecordTypeStrategy.BasicRecord => basicRecord
        /// - EglRecordTypeStrategy.SqlRecord => sqlRecord
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileStructureDeclaration içinde EglRecordDeclaration oluşturulurken
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// indexedRecord, serialRecord, table metadata veya DCLGEN tabanlı record type seçimi gerekirse merkezi mapping noktası olarak genişletilir.
        /// </summary>
        private string GetEglRecordType()
        {
            return _options.RecordTypeStrategy switch
            {
                EglRecordTypeStrategy.SqlRecord => "sqlRecord",
                _ => "basicRecord"
            };
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
        /// - BIT(8) => null
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - CalculateMemberLength içinde
        /// - Structure array parent length hesabında
        /// - Nested group field length hesabında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// BIT için storage-preserving mapping kararı alındığında bu method
        /// Pl1BitType branch'i ile genişletilecektir.
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
                Pl1BitType => null,
                _ => null
            };
        }

        /// <summary>
        /// PL/I structure member modelini bir veya daha fazla EGL record field modeline dönüştürür.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I structure member alanları normal typed field, field array veya nested group field olabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// Normal field için tek EGL field üretir.
        /// Nested group için parent group field ve child field listesini recursive olarak üretir.
        /// Veri tipi dönüşümü diagnostic ürettiyse ikinci generic diagnostic üretmez.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - 5 PARAM CHAR(08)
        /// - 5 PARAM_LIST(2) CHAR(10)
        /// - 5 PARAM_LIST CHAR(10) DIM(2)
        /// - 5 FLAG BIT(1) => tek BIT diagnostic üretir.
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileStructureDeclaration içerisinde
        /// - Nested group child field üretiminde recursive olarak
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// Çok seviyeli nested structure, sqlRecord field metadata ve unsupported type diagnostic yönetimi için temel dönüşüm noktasıdır.
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
                        _options.NamingOptions.Style),
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

            var diagnosticCountBeforeDataType = _diagnostics.Diagnostics.Count;
            var dataType = TranspileDataType(member.DataType);

            if (dataType is null)
            {
                if (_diagnostics.Diagnostics.Count == diagnosticCountBeforeDataType)
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        $"Desteklenmeyen PL/I structure member veri tipi: {member.DataType.GetType().Name}",
                        member.Location));
                }

                return fields;
            }

            fields.Add(new EglRecordFieldDeclaration(
                eglLevel,
                IdentifierNameTransformer.Transform(
                    member.Name,
                    _options.NamingOptions.Style),
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
        /// - BIT(1) => diagnostic
        /// - BIT(8) => diagnostic
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - PL/I → EGL Transpiler içerisinde variable declaration dönüşümünde
        /// - PL/I → EGL Transpiler içerisinde record field dönüşümünde
        /// - Nested group child field dönüşümünde
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// BIT için EGL tarafında kesin mapping kararı alındığında bu method
        /// Pl1BitType branch'i üzerinden genişletilecektir.
        /// </summary>
        private EglDataType? TranspileDataType(Pl1DataType dataType)
        {
            return dataType switch
            {
                Pl1FixedDecimalType fixedDecimalType => new EglDecimalType(
                    fixedDecimalType.Precision,
                    fixedDecimalType.Scale,
                    fixedDecimalType.Location),
                Pl1FixedBinaryType fixedBinaryType => TranspileFixedBinaryType(fixedBinaryType),
                Pl1CharacterType characterType => new EglCharacterType(
                    characterType.Length,
                    characterType.Location),
                Pl1VarcharType varcharType => new EglCharacterType(
                    varcharType.Length,
                    varcharType.Location),
                Pl1PictureType pictureType => TranspilePictureType(pictureType),
                Pl1BitType bitType => TranspileBitType(bitType),
                _ => null
            };
        }

        /// <summary>
        /// PL/I BIT(n) veri tipi için EGL mapping kararını yönetir.
        ///
        /// Neden var?
        /// ----------------------
        /// PL/I BIT(n) tipi bit string semantic taşır. Bu tipi doğrudan EGL char(n)
        /// veya numeric tipe çevirmek bit-level anlam kaybına neden olabilir.
        ///
        /// Ne çözüyor?
        /// ----------------------
        /// BIT tipi parser tarafından desteklense bile şimdilik otomatik EGL mapping
        /// yapılmasını engeller ve açık diagnostic üretir.
        ///
        /// Hangi örneği destekliyor?
        /// ----------------------
        /// - DCL FLAG BIT(1);
        /// - DCL MASK BIT(8);
        ///
        /// Nerede kullanılır?
        /// ----------------------
        /// - TranspileDataType içinde Pl1BitType yakalandığında
        ///
        /// Gelecekte neye temel olur?
        /// ----------------------
        /// BIT(1) boolean mapping, BIT(n) char/binary preserving mapping veya
        /// hedef dile özel bit string mapping kararları burada uygulanacaktır.
        /// </summary>
        private EglDataType? TranspileBitType(Pl1BitType bitType)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Error,
                $"BIT veri tipi için EGL mapping henüz desteklenmiyor. Length: {bitType.Length}",
                bitType.Location));

            return null;
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