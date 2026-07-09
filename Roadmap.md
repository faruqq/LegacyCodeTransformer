# LegacyCodeTransformer Yol Haritası

Bu doküman, projenin uzun vadeli geliştirme planını içerir.

## Amaç

* Projenin geliştirme sırasını belirlemek.
* Tamamlanan fazları takip etmek.
* Aktif geliştirilen fazı göstermek.
* Bir sonraki geliştirme hedefini belirlemek.

Bu doküman teknik tasarım ayrıntıları içermez.

* Mimari kararlar **Decisions.md** dosyasında tutulur.
* Tamamlanan çalışmalar **ModuleSummaries.md** dosyasında özetlenir.
* Genel mimari **Architecture.md** dosyasında açıklanır.

---

# Faz Durumları

| Durum | Anlamı                      |
| ----- | --------------------------- |
| ✅     | Tamamlandı                  |
| 🚧    | Aktif olarak geliştiriliyor |
| ⏳     | Planlandı                   |
| ❌     | İptal edildi                |
| ⏸️    | Beklemede                   |

---

# P01 — Proje Kurulumu

## Durum

✅ Tamamlandı

## Amaç

Projeye ait temel çözüm yapısını ve geliştirme ortamını oluşturmak.

## Kapsam

* Solution oluşturulması
* Dokümantasyon klasörü
* Kaynak projeler
* Test projeleri
* İlk mimari planlama

## Tamamlananlar

### Solution

* LegacyCodeTransformer.sln

### Dokümantasyon

* Architecture.md
* Decisions.md
* Roadmap.md
* ModuleSummaries.md
* Glossary.md

### Kaynak Projeler

* LegacyCodeTransformer.Core
* LegacyCodeTransformer.Shared
* LegacyCodeTransformer.Pl1
* LegacyCodeTransformer.Egl
* LegacyCodeTransformer.Transpilers
* LegacyCodeTransformer.Application
* LegacyCodeTransformer.Cli

### Test Projeleri

* LegacyCodeTransformer.Pl1.Tests
* LegacyCodeTransformer.Egl.Tests
* LegacyCodeTransformer.Transpilers.Tests
* LegacyCodeTransformer.Application.Tests

## Sonraki Faz

P02 — Core Foundation

---

# P02 — Core Foundation

## Durum

✅ Tamamlandı

## Amaç

Dilden bağımsız ortak altyapıyı oluşturmak.

## Kapsam

* Syntax modelleri
* Diagnostic sistemi
* Ortak Result modelleri

## Tamamlananlar

* SourceLocation
* SyntaxNode
* SyntaxTree
* DiagnosticSeverity
* Diagnostic
* DiagnosticBag
* ParseResult
* ConversionResult

## Sonraki Faz

P03 — İlk PL/I → EGL Dönüşümü

---

# P03 — İlk PL/I → EGL Dönüşümü

## Durum

✅ Tamamlandı

## Amaç

İlk çalışan uçtan uca dönüşüm hattını oluşturmak.

## Kapsam

### PL/I

* Minimal Syntax Model
* Lexer
* Parser
* Normalizer

### EGL

* Minimal Syntax Model
* Code Generator

### Transpiler

* PL/I → EGL

### Application

* ConversionService

### CLI

* İlk çalışan Console uygulaması

### Testler

* ConversionService
* Lexer
* Parser
* Transpiler
* Generator

## Başarı Kriteri

Aşağıdaki dönüşüm başarıyla gerçekleştirilmektedir.

PL/I

    DCL MUST_NO FIXED DECIMAL(8);

    DCL CUSTOMER_NO FIXED DECIMAL(10,2);

EGL

    mustNo decimal(8);

    customerNo decimal(10,2);

## Sonraki Faz

P04 — PL/I Veri Tiplerini Genişletme

---

# P04 — PL/I Veri Tiplerini Genişletme

## Durum

✅ Tamamlandı

## Amaç

PL/I veri tipi, declaration, structure ve parser altyapısını gerçek projelerde kullanılabilecek seviyeye çıkarmak.

## Kapsam

* PL/I scalar veri tipleri
* PL/I declaration parsing
* INIT / INITIAL başlangıç değeri parsing
* Structure declaration parsing
* Structure array desteği
* Structure member array desteği
* Nested structure parsing
* PL/I → EGL record dönüşümü
* EGL output casing ve indentation standardı
* Parser helper mimarisi
* Parser test altyapısı

## Tamamlananlar

### Character ve Initialization

* ✅ CHAR / CHARACTER veri tipi desteği
* ✅ VARCHAR → EGL char dönüşümü
* ✅ INIT / INITIAL parse desteği
* ✅ INIT / INITIAL güvenli scalar subset → EGL default value dönüşümü
* ✅ INIT repeat factor bilgisinin syntax model üzerinde korunması
* ✅ INIT all-elements bilgisinin syntax model üzerinde korunması

### Identifier ve EGL Output Standardı

* ✅ Identifier naming strategy desteği
* ✅ EGL output casing standardı
* ✅ EGL indentation standardı
* ✅ basicRecord casing standardı
* ✅ sqlRecord opt-in record type strategy desteği
* ✅ char, num, smallint, int, decimal type casing standardı

### Structure Declaration

* ✅ PL/I basic structure declaration parse desteği
* ✅ PL/I structure declaration → EGL record dönüşümü
* ✅ PL/I structure array → EGL parent array field dönüşümü
* ✅ PL/I structure member array → EGL field array dönüşümü
* ✅ DIM / DIMENSION syntax desteği
* ✅ PL/I nested structure parse desteği
* ✅ PL/I nested structure → EGL parent group field dönüşümü
* ✅ Recursive nested structure mapping desteği
* ✅ Record / structure level değerlerinin 5 ve 5’in katları olarak üretilmesi
* ✅ Level bazlı indentation standardı

### Numeric Types

* ✅ Decimal scale bilgisinin nullable korunması
* ✅ FIXED DECIMAL(p) desteği
* ✅ FIXED DECIMAL(p,0) explicit zero scale desteği
* ✅ FIXED DECIMAL(p,s) scale desteği
* ✅ FIXED DEC / DEC FIXED / DECIMAL FIXED synonym desteği
* ✅ FIXED BIN / BIN FIXED numeric mapping desteği
* ✅ FIXED BIN(15) → EGL smallint mapping desteği
* ✅ FIXED BIN(31) → EGL int mapping desteği
* ✅ smallint / int casing standardı

### PIC / PICTURE

* ✅ PIC / PICTURE ayrı model parse desteği
* ✅ Numeric PIC güvenli subset parse desteği
* ✅ Güvenli numeric PIC subset → EGL num mapping desteği
* ✅ Alphanumeric PIC subset parse desteği
* ✅ Alphanumeric PIC subset → EGL char mapping desteği
* ✅ PIC repeat notation length / precision hesabı
* ✅ PIC implied decimal scale hesabı
* ✅ Signed PIC classification desteği
* ✅ Formatted PIC örnekleri için diagnostic üretimi

### BIT ve Floating Types

* ✅ BIT(n) parser desteği
* ✅ BIT(n) semantic model desteği
* ✅ BIT(n) için EGL diagnostic davranışı
* ✅ FLOAT parser desteği
* ✅ REAL parser desteği
* ✅ DOUBLE PRECISION parser desteği
* ✅ FLOAT DECIMAL desteği
* ✅ FLOAT BIN desteği
* ✅ FLOAT semantic model desteği
* ✅ Floating precision bilgisinin korunması
* ✅ FLOAT / REAL / DOUBLE güvenli EGL mapping subset desteği

### Parser Infrastructure

* ✅ ParseContext ortak parser state modeli
* ✅ ParserBase ortak parser altyapısı
* ✅ ParserDiagnosticFactory ortak diagnostic üretimi
* ✅ HelperParseResult<T> generic helper parse modeli
* ✅ DeclarationParser ayrıştırması
* ✅ VariableDeclarationParser ayrıştırması
* ✅ StructureParser ayrıştırması
* ✅ DataTypeParser ayrıştırması
* ✅ CharacterTypeParser ayrıştırması
* ✅ NumericTypeParser ayrıştırması
* ✅ FloatingTypeParser ayrıştırması
* ✅ PictureTypeParser ayrıştırması
* ✅ PicturePatternAnalyzer ayrıştırması
* ✅ BitTypeParser ayrıştırması
* ✅ InitialValueParser ayrıştırması
* ✅ DimensionParser ayrıştırması
* ✅ Parser helper test altyapısının ortaklaştırılması
* ✅ Parser test tekrarlarının kaldırılması
* ✅ Pl1Parser sınıfının orchestration seviyesine indirgenmesi

### Visitor / Walker Foundation

* ✅ Pl1SyntaxVisitor
* ✅ Pl1SyntaxWalker
* ✅ Visitor / Walker test altyapısı
* ✅ Non-invasive Visitor tasarımı
* ✅ Accept() gerektirmeyen traversal yaklaşımı

## Mimari Sonuç

P04 sonunda parser aşağıdaki sorumluluk dağılımına sahiptir.

    Pl1Parser
        DeclarationParser
            VariableDeclarationParser
                DataTypeParser
                DimensionParser
                InitialValueParser

            StructureParser
                DataTypeParser
                DimensionParser
                InitialValueParser

        StatementParser (P05)

    DataTypeParser
        CharacterTypeParser
        NumericTypeParser
        PictureTypeParser
        BitTypeParser
        FloatingTypeParser

## Kapsam Dışı Bırakılanlar

* FLOAT DECIMAL için kesin EGL mapping
* BIT için kesin EGL mapping
* Formatted PIC için display metadata üretimi
* INIT repeat factor expansion
* Structure member default value output
* Çok boyutlu DIMENSION desteği
* Gelişmiş sqlRecord metadata üretimi
* Statement parsing
* Procedure parsing
* Expression parsing

## Başarı Kriteri

Parser, Transpiler ve Generator katmanları gerçek PL/I declaration yapılarını uçtan uca destekleyecek seviyeye ulaşmıştır.

Parser mimarisi helper parser yaklaşımı ile sadeleştirilmiş ve yeni parser modüllerinin eklenmesine uygun hale getirilmiştir.

Visitor / Walker altyapısı parser ve transpiler katmanlarının gelecekte genişletilmesine uygun şekilde tamamlanmıştır.

## Sonraki Faz

P05 — PL/I Statement Desteği

---

# P05 — PL/I Statement Pipeline

## Durum

🚧 Aktif olarak geliştiriliyor

## Amaç

PL/I declaration dışındaki executable statement modellerini oluşturmak, parser altyapısını statement desteği için genişletmek ve statement modellerini transpiler ile EGL generator katmanına kadar uçtan uca çalışır hale getirmek.

P05 yalnızca parser geliştirme fazı değildir.

Bu faz sonunda Assignment, CALL, IF ve DO statement türleri aşağıdaki dönüşüm hattı boyunca doğrulanmış olacaktır.

    PL/I Source
        ↓
    Lexer
        ↓
    Statement Parser
        ↓
    PL/I Syntax Tree
        ↓
    Statement Transpiler
        ↓
    EGL Syntax Tree
        ↓
    EGL Generator

P05, declaration odaklı parser mimarisinden gerçek PL/I executable statement pipeline mimarisine geçiş fazıdır.

---

## P05.1 — Statement Model Foundation

### Durum

✅ Tamamlandı

### Amaç

Statement modellerinin parser davranışından bağımsız olarak oluşturulması ve syntax modelinin executable statement desteğine hazırlanması.

### Tamamlananlar

#### Statement Modeli

* ✅ Pl1Statement
* ✅ Pl1Expression
* ✅ Pl1RawExpression
* ✅ Pl1AssignmentStatement
* ✅ Pl1CallStatement
* ✅ Pl1IfStatement
* ✅ Pl1DoStatement
* ✅ Pl1DoStatementKind
* ✅ Pl1BlockStatement

#### Syntax Tree

* ✅ Pl1SyntaxTree statement listesi desteği

#### Visitor / Walker

* ✅ Statement visitor dispatch desteği
* ✅ Statement walker recursive traversal desteği
* ✅ Statement traversal testleri

### Başarı Kriteri

Parser henüz gerçek statement parse etmese bile syntax tree executable statement modellerini taşıyabilir duruma gelmiştir.

---

## P05.2 — Statement Parser Foundation

### Durum

✅ Tamamlandı

### Amaç

Statement parser altyapısını declaration parser mimarisine benzer şekilde modüler hale getirmek.

### Tamamlananlar

* ✅ StatementDispatcher
* ✅ StatementParser
* ✅ Pl1Parser statement routing entegrasyonu
* ✅ Statement başlangıç token tespiti
* ✅ Statement family name mapping
* ✅ Statement parser recovery davranışı
* ✅ Unsupported concrete parser diagnostic davranışı
* ✅ StatementDispatcher unit testleri
* ✅ StatementParser unit testleri
* ✅ Pl1Parser statement routing testleri

### Bilinçli Olarak Eklenmeyenler

* StatementParseContext
* StatementParserBase

Bu iki sınıf eklenmemiştir çünkü mevcut parser altyapısındaki ParseContext ve ParserBase statement parser için de yeterlidir.

### Başarı Kriteri

Parser declaration ve statement parse işlemlerini birbirinden bağımsız yönetebilir hale gelmiştir.

Pl1Parser, DCL / DECLARE tokenlarını DeclarationParser'a, statement başlangıç tokenlarını StatementParser'a yönlendirebilmektedir.

StatementParser, concrete parser modülleri henüz eklenmeden güvenli diagnostic ve recovery davranışı sağlamaktadır.

### Sonraki Milestone

P05.3 — Assignment & CALL Parser

---

## P05.3 — Assignment & CALL Parser

### Durum

✅ Tamamlandı

### Amaç

Statement parser altyapısı üzerine ilk executable statement parser modüllerini eklemek ve gerçek PL/I kaynak kodundan Assignment ile CALL statement modellerini üretmek.

### Tamamlananlar

#### Assignment Parser

* ✅ AssignmentStatementParser
* ✅ Identifier assignment desteği
* ✅ String literal assignment desteği
* ✅ Numeric assignment desteği
* ✅ Qualified member assignment desteği
* ✅ Raw expression target/value taşıma

#### CALL Parser

* ✅ CallStatementParser
* ✅ Parametresiz CALL desteği
* ✅ Parametreli CALL desteği
* ✅ Identifier argument desteği
* ✅ String literal argument desteği
* ✅ Qualified member argument desteği

#### Parser Utility Refactor

* ✅ AssignmentRawExpressionBuilder
* ✅ ExpressionFactory
* ✅ DelimitedTokenReader
* ✅ StatementRecoveryHelper
* ✅ StatementParserKind
* ✅ StatementDispatcher parser kind mapping
* ✅ StatementParser concrete parser routing

#### Testler

* ✅ AssignmentRawExpressionBuilderTests
* ✅ ExpressionFactoryTests
* ✅ DelimitedTokenReaderTests
* ✅ StatementRecoveryHelperTests
* ✅ StatementParser assignment testleri
* ✅ StatementParser CALL testleri
* ✅ Pl1Parser assignment integration testleri
* ✅ Pl1Parser CALL integration testleri

### Desteklenen Örnekler

    PARAM = 'ABC';
    SQLCODE = 0;
    DCLGLAU.BRM_KOD = 888;
    CALL FETCH_CURSOR;
    CALL PROC1(A, 'ABC', B);
    CALL SQL_HATA_OLUSTUR('SELECT GLAU_HISTORY');

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* Çoklu assignment
* Tam expression parser
* Operator precedence
* Nested function call argument parsing
* IF parser
* DO parser
* EGL statement output

### Başarı Kriteri

Parser gerçek PL/I Assignment ve CALL statement'larını syntax tree üzerinde semantic olarak temsil edebilir hale gelmiştir.

Pl1Parser declaration, assignment ve CALL statement modellerini aynı Pl1SyntaxTree içinde taşıyabilmektedir.

### Sonraki Milestone

P05.4 — IF / DO Parser

---

## P05.4 — IF / DO Parser

### Durum

✅ Tamamlandı

### Amaç

Kontrol akışı oluşturan statement yapılarını parse ederek syntax tree üzerinde temsil etmek.

### Tamamlananlar

#### IF Parser

* ✅ IfStatementParser
* ✅ IF condition THEN CALL desteği
* ✅ IF condition THEN assignment desteği
* ✅ IF condition THEN statement ELSE statement desteği
* ✅ IF condition THEN DO block desteği
* ✅ Condition alanının Pl1RawExpression olarak taşınması
* ✅ THEN / ELSE kollarının Pl1Statement olarak modellenmesi

#### DO Parser

* ✅ DoStatementParser
* ✅ DO block desteği
* ✅ DO WHILE(condition) desteği
* ✅ DO UNTIL(condition) desteği
* ✅ DO body içinde recursive statement parsing
* ✅ DO body'nin Pl1BlockStatement olarak modellenmesi

#### Testler

* ✅ StatementParser IF parser testleri
* ✅ StatementParser IF ELSE parser testleri
* ✅ StatementParser IF THEN DO parser testleri
* ✅ StatementParser DO block parser testleri
* ✅ StatementParser DO WHILE parser testleri
* ✅ StatementParser DO UNTIL parser testleri
* ✅ Pl1Parser IF integration testleri
* ✅ Pl1Parser DO integration testleri

### Desteklenen Örnekler

    IF SQLCODE = 0 THEN CALL FETCH_CURSOR;
    IF A = B THEN PARAM = 'ABC';
    IF A = B THEN CALL PROC1; ELSE CALL PROC2;
    DO; PARAM = 'ABC'; CALL PROC1; END;
    DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END;
    DO UNTIL(EOF); PARAM = 'ABC'; END;
    IF A = B THEN DO; CALL PROC1; END;

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* Tam expression parser
* Operator precedence
* ELSE DO için ayrı ek test seti
* Nested function call condition parsing
* Sayaçlı DO parsing
* SELECT / WHEN parser
* EGL statement output

### Başarı Kriteri

Parser IF, IF ELSE, DO block, DO WHILE ve DO UNTIL statement modellerini syntax tree üzerinde semantic olarak temsil edebilir hale gelmiştir.

Pl1Parser declaration, assignment, CALL, IF ve DO statement modellerini aynı Pl1SyntaxTree içinde taşıyabilmektedir.

### Sonraki Milestone

P05.5 — Statement Integration & Tests

---

## P05.5 — Statement Integration & Tests

### Durum

✅ Tamamlandı

### Amaç

Statement parser'ın declaration parser ile tamamen entegre edilmesi ve gerçek PL/I programlarında birlikte görülen executable statement türlerinin aynı syntax tree içinde doğrulanması.

### Tamamlananlar

#### Integration

* ✅ Declaration + Statement birlikte parse
* ✅ Mixed source parsing
* ✅ Assignment + CALL + IF + DO birlikte parse
* ✅ Recursive DO block parsing
* ✅ IF THEN DO parsing
* ✅ IF THEN DO ELSE DO parsing
* ✅ Nested control-flow hierarchy parsing
* ✅ Statement sırası koruma testleri

#### Testler

* ✅ StatementParser nested DO block testleri
* ✅ StatementParser IF THEN DO ELSE DO testleri
* ✅ Pl1Parser mixed declaration + statement integration testleri
* ✅ Pl1Parser nested control-flow integration testleri

### Desteklenen Entegre Örnekler

    DCL PARAM CHAR(08);
    PARAM = 'ABC';
    CALL PROC1;
    IF A = B THEN CALL PROC2;
    DO;
        CALL PROC3;
    END;

    IF A = B THEN DO;
        DO WHILE(SQLCODE = 0);
            CALL FETCH_CURSOR;
        END;
    END;

    IF A = B THEN DO;
        CALL PROC1;
    END;
    ELSE DO;
        CALL PROC2;
    END;

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* Procedure parser
* EXEC SQL parser
* SELECT / WHEN parser
* RETURN parser
* GOTO parser
* Full expression parser
* EGL statement output

### Başarı Kriteri

Parser declaration ve executable statement'ları tek syntax tree içerisinde eksiksiz temsil edebilir hale gelmiştir.

Nested IF / DO control-flow yapıları syntax tree üzerinde hiyerarşik olarak korunmaktadır.

P05 sonunda transpiler katmanı statement desteğini kullanabilecek seviyeye hazırlanmıştır.

### Sonraki Milestone

P05.6 — Statement Visitor / Walker Integration Verification

---

## P05.6 — Statement Visitor / Walker Integration Verification

### Durum

✅ Tamamlandı

### Amaç

Statement transpiler foundation öncesinde mevcut Pl1SyntaxVisitor ve Pl1SyntaxWalker altyapısının statement node ailesini eksiksiz dolaştığını doğrulamak.

### Tamamlananlar

#### Production Verification

* ✅ Pl1SyntaxVisitor statement dispatch desteği doğrulandı.
* ✅ Pl1SyntaxVisitor expression dispatch desteği doğrulandı.
* ✅ Pl1SyntaxWalker SyntaxTree.Statements traversal desteği doğrulandı.
* ✅ Pl1SyntaxWalker Assignment target/value traversal desteği doğrulandı.
* ✅ Pl1SyntaxWalker CALL argument traversal desteği doğrulandı.
* ✅ Pl1SyntaxWalker IF condition / THEN / ELSE traversal desteği doğrulandı.
* ✅ Pl1SyntaxWalker DO condition / body traversal desteği doğrulandı.
* ✅ Pl1SyntaxWalker nested block recursive traversal desteği doğrulandı.

#### Production Refactor

* ✅ Production refactor gerekmediği doğrulandı.
* ✅ Yeni VisitorBase / StatementWalker abstraction eklenmedi.
* ✅ Mevcut Pl1SyntaxVisitor ve Pl1SyntaxWalker altyapısı korundu.

#### Testler

* ✅ IF THEN ELSE traversal testleri
* ✅ DO WHILE traversal testleri
* ✅ Nested DO block traversal testleri
* ✅ CALL argument expression traversal testleri

### Başarı Kriteri

Visitor / Walker altyapısı Assignment, CALL, IF, DO, Block ve RawExpression modellerini eksiksiz dolaşabilmektedir.

Statement transpiler foundation için mevcut traversal altyapısının yeterli olduğu doğrulanmıştır.

### Sonraki Milestone

P05.7 — Statement Transpiler Foundation

---

## P05.7 — Statement Transpiler Foundation

### Durum

✅ Tamamlandı

### Amaç

PL/I statement modellerinin transpiler pipeline içinde işlenmeye başlamasını ve EGL syntax tree tarafında statement modellerinin taşınabilmesini sağlamak.

### Tamamlananlar

#### EGL Syntax Foundation

* ✅ EglStatement base modeli eklendi.
* ✅ EglSyntaxTree statement listesi taşıyacak şekilde genişletildi.
* ✅ Declaration + statement root model yapısı oluşturuldu.

#### Transpiler Foundation

* ✅ StatementTranspiler eklendi.
* ✅ Pl1ToEglTranspiler statement routing entegrasyonu eklendi.
* ✅ Unsupported statement diagnostic davranışı eklendi.
* ✅ Concrete statement mapping sonraki milestone’lara bırakıldı.

#### Testler

* ✅ EglSyntaxTree statement listesi testleri
* ✅ Assignment statement routing testleri
* ✅ Declaration + statement transpiler pipeline testleri
* ✅ Unsupported statement diagnostic testleri

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* Assignment EGL mapping
* CALL EGL mapping
* IF EGL mapping
* DO EGL mapping
* EGL statement generator output
* End-to-end statement output testleri

### Başarı Kriteri

Transpiler pipeline PL/I statement listesini görüp statement transpiler’a yönlendirebilmektedir.

EGL syntax tree statement listesi taşıyabilir hale gelmiştir.

### Sonraki Milestone

P05.8 — Assignment EGL Generation

---

## P05.8 — Assignment EGL Generation

### Durum

✅ Tamamlandı

### Amaç

PL/I assignment statement modellerini EGL assignment syntax modeline dönüştürmek ve EGL generator üzerinden kaynak kod çıktısı üretebilmek.

### Tamamlananlar

#### EGL Assignment Statement

* ✅ EglAssignmentStatement modeli eklendi.
* ✅ Pl1AssignmentStatement → EglAssignmentStatement mapping eklendi.
* ✅ Assignment target text dönüşümü eklendi.
* ✅ Assignment value text dönüşümü eklendi.
* ✅ PL/I string literal → EGL string literal quote dönüşümü eklendi.
* ✅ Identifier casing dönüşümü eklendi.

#### Generator

* ✅ EglCodeGenerator statement listesi output desteği eklendi.
* ✅ EglAssignmentStatement output desteği eklendi.
* ✅ Declaration + statement output sırası korundu.

#### Testler

* ✅ Assignment statement transpiler testleri
* ✅ Declaration + assignment transpiler testleri
* ✅ Identifier expression dönüşüm testleri
* ✅ Assignment generator testleri
* ✅ Declaration + assignment generator testleri
* ✅ Assignment transpile + generate end-to-end testleri

### Desteklenen Örnekler

    PARAM = 'ABC';
    CUSTOMER_NO = MUST_NO;

EGL output:

    Param = "ABC";
    CustomerNo = MustNo;

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* EglExpression abstraction
* EglRawExpression abstraction
* ExpressionTranspiler
* CALL EGL generation
* IF EGL generation
* DO EGL generation
* Full expression parser
* Operator mapping
* Operator precedence
* Multi-target assignment

### Başarı Kriteri

Assignment statement için parser → transpiler → EGL generator zinciri uçtan uca çalışır hale gelmiştir.

### Sonraki Milestone

P05.9 — CALL EGL Generation

---

## P05.9 — CALL EGL Generation

### Durum

✅ Tamamlandı

### Amaç

PL/I CALL statement modellerini EGL CALL syntax modeline dönüştürmek ve EGL generator üzerinden kaynak kod çıktısı üretebilmek.

### Tamamlananlar

#### EGL CALL Statement

* ✅ EglCallStatement modeli eklendi.
* ✅ Pl1CallStatement → EglCallStatement mapping eklendi.
* ✅ ProcedureName casing dönüşümü eklendi.
* ✅ CALL argument text dönüşümü eklendi.
* ✅ PL/I string literal → EGL string literal quote dönüşümü korundu.
* ✅ Identifier casing dönüşümü argument tarafında korundu.

#### Generator

* ✅ EglCodeGenerator EglCallStatement output desteği eklendi.
* ✅ Parametresiz CALL output desteği eklendi.
* ✅ Parametreli CALL output desteği eklendi.
* ✅ Declaration + assignment + CALL output sırası korundu.

#### Testler

* ✅ CALL statement transpiler testleri
* ✅ CALL argument transpiler testleri
* ✅ CALL generator testleri
* ✅ CALL transpile + generate end-to-end testleri
* ✅ Declaration + assignment + CALL output sırası testleri

### Desteklenen Örnekler

    CALL FETCH_CURSOR;
    CALL PROC1(CUSTOMER_NO, 'ABC');

EGL output:

    call FetchCursor();
    call Proc1(CustomerNo, "ABC");

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* IF EGL generation
* DO EGL generation
* Full expression parser
* Named argument mapping
* OUT / INOUT parameter semantic analysis
* Service invocation mapping

### Başarı Kriteri

CALL statement için parser → transpiler → EGL generator zinciri uçtan uca çalışır hale gelmiştir.

### Sonraki Milestone

P05.10 — IF EGL Generation

---

## P05.10 — IF EGL Generation

### Durum

✅ Tamamlandı

### Amaç

PL/I IF statement modellerini EGL IF syntax modeline dönüştürmek ve EGL generator üzerinden kaynak kod çıktısı üretebilmek.

### Tamamlananlar

#### EGL IF Statement

* ✅ EglIfStatement modeli eklendi.
* ✅ Pl1IfStatement → EglIfStatement mapping eklendi.
* ✅ Condition text dönüşümü eklendi.
* ✅ THEN branch recursive statement dönüşümü eklendi.
* ✅ ELSE branch recursive statement dönüşümü eklendi.
* ✅ Identifier casing dönüşümü condition tarafında korundu.

#### Generator

* ✅ EglCodeGenerator EglIfStatement output desteği eklendi.
* ✅ IF THEN output desteği eklendi.
* ✅ IF THEN ELSE output desteği eklendi.
* ✅ Nested child statement indentation standardı eklendi.

#### Testler

* ✅ IF statement transpiler testleri
* ✅ IF THEN ELSE transpiler testleri
* ✅ IF generator testleri
* ✅ IF ELSE generator testleri
* ✅ IF transpile + generate end-to-end testleri

### Desteklenen Örnekler

    IF CUSTOMER_NO = MUST_NO THEN CALL FETCH_CURSOR;
    IF A = B THEN CALL PROC1; ELSE CALL PROC2;

EGL output:

    if (CustomerNo = MustNo)
        call FetchCursor();
    end

    if (A = B)
        call Proc1();
    else
        call Proc2();
    end

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* DO EGL generation
* IF THEN DO output
* ELSE DO output
* Full expression parser
* Condition operator semantic mapping
* Complex boolean expression mapping

### Başarı Kriteri

IF statement için parser → transpiler → EGL generator zinciri uçtan uca çalışır hale gelmiştir.

### Sonraki Milestone

P05.11 — DO EGL Generation

---

## P05.11 — DO EGL Generation

### Durum

✅ Tamamlandı

### Amaç

PL/I DO, DO WHILE ve DO UNTIL statement modellerini EGL DO / loop syntax modeline dönüştürmek ve EGL generator üzerinden kaynak kod çıktısı üretebilmek.

### Tamamlananlar

#### EGL DO Statement

* ✅ EglDoStatement modeli eklendi.
* ✅ EglDoStatementKind enum modeli eklendi.
* ✅ Pl1DoStatement → EglDoStatement mapping eklendi.
* ✅ DO kind mapping eklendi.
* ✅ DO condition text dönüşümü eklendi.
* ✅ DO body recursive statement dönüşümü eklendi.
* ✅ Identifier casing dönüşümü condition tarafında korundu.

#### Generator

* ✅ EglCodeGenerator EglDoStatement output desteği eklendi.
* ✅ Block DO output desteği eklendi.
* ✅ DO WHILE output desteği eklendi.
* ✅ DO UNTIL → negated while output desteği eklendi.
* ✅ Nested child statement indentation standardı korundu.

#### Testler

* ✅ Block DO transpiler testleri
* ✅ DO WHILE transpiler testleri
* ✅ Block DO generator testleri
* ✅ DO WHILE generator testleri
* ✅ DO UNTIL generator testleri
* ✅ DO WHILE transpile + generate end-to-end testleri
* ✅ IF THEN DO nested output testleri

### Desteklenen Örnekler

    DO; CALL PROC1; END;
    DO WHILE(SQLCODE = 0); CALL FETCH_CURSOR; END;
    DO UNTIL(EOF); CALL CLOSE_CURSOR; END;
    IF A = B THEN DO; CALL PROC1; END;

EGL output:

    do
        call Proc1();
    end

    while (Sqlcode = 0)
        call FetchCursor();
    end

    while (!(Eof))
        call CloseCursor();
    end

### Bilinçli Olarak Kapsam Dışı Bırakılanlar

* Sayaçlı DO generation
* SELECT / WHEN generation
* Full expression parser
* Boolean condition semantic mapping
* Complex loop optimization

### Başarı Kriteri

DO statement için parser → transpiler → EGL generator zinciri uçtan uca çalışır hale gelmiştir.

### Sonraki Milestone

P05.12 — Statement End-to-End Tests

---

## P05.12 — Statement End-to-End Tests

### Durum

✅ Tamamlandı

### Amaç

P05 kapsamında eklenen declaration, assignment, CALL, IF ve DO statement pipeline davranışını gerçek PL/I source input üzerinden uçtan uca doğrulamak.

### Tamamlananlar

#### End-to-End Pipeline

* ✅ PL/I source → Lexer entegrasyonu doğrulandı.
* ✅ Lexer → Parser entegrasyonu doğrulandı.
* ✅ Parser → Pl1SyntaxTree entegrasyonu doğrulandı.
* ✅ Pl1SyntaxTree → Pl1ToEglTranspiler entegrasyonu doğrulandı.
* ✅ EglSyntaxTree → EglCodeGenerator entegrasyonu doğrulandı.
* ✅ Final EGL source output doğrulandı.

#### Testler

* ✅ Declaration + assignment end-to-end testleri
* ✅ Declaration + CALL end-to-end testleri
* ✅ IF THEN CALL end-to-end testleri
* ✅ DO WHILE CALL end-to-end testleri
* ✅ IF THEN DO nested output testleri
* ✅ Mini program end-to-end output testleri

### Desteklenen Entegre Örnekler

    DCL PARAM CHAR(10);
    PARAM = 'ABC';

    DCL CUSTOMER_NO CHAR(10);
    CALL FETCH_CUSTOMER(CUSTOMER_NO);

    DCL SQLCODE FIXED DECIMAL(5);
    IF SQLCODE = 0 THEN CALL FETCH_CURSOR;

    DO WHILE(SQLCODE = 0);
        CALL FETCH_CURSOR;
    END;

    IF A = B THEN DO;
        CALL PROC1;
    END;

### Başarı Kriteri

PL/I source input, declaration, assignment, CALL, IF ve DO statement türleri için EGL source output’a kadar başarıyla dönüştürülebilmektedir.

P05 statement pipeline parser, transpiler ve generator katmanlarında uçtan uca doğrulanmıştır.

### Sonraki Faz

P06 — Procedure Desteği

---

# P06 — Procedure Desteği

## Durum ✅ Tamamlandı

## Amaç

PL/I procedure seviyesindeki syntax yapılarının parse edilmesini ve statement pipeline ile entegre şekilde temsil edilmesini sağlamak.

Firma PL/I kodlarında procedure yapıları çoğunlukla business logic içerir. Parametre ve değişken declaration bilgileri genellikle procedure içinde değil, dosyanın başındaki global declaration bölümünde yer alır.

Bu nedenle P06 ilk kapsamı sade procedure modelleme ve parser entegrasyonu üzerine kurulacaktır.

## Milestone Durumu

* ✅ P06.1 — Procedure Syntax Model Foundation
* ✅ P06.2 — Procedure Parser Foundation
* ✅ P06.3 — Procedure Parser Statement Integration
* ✅ P06.4 — Procedure End-to-End Tests

## İlk Kapsam

* PL/I procedure syntax model foundation
* PROCEDURE_NAME: PROCEDURE; parse desteği
* END PROCEDURE_NAME; parse desteği
* Procedure içindeki executable statement listesinin korunması
* Global declaration + procedure statement ayrımının yapılması
* CALL PROCEDURE_NAME; çağrılarının mevcut CALL statement pipeline ile uyumlu çalışması

## Bilinçli Olarak İlk Kapsam Dışı Bırakılanlar

* Procedure parameter modeli
* Procedure body için ayrı model
* Local declaration scope modeli
* ENTRY parser
* RETURN statement
* Parametre semantic analysis

## Başarı Kriteri

Bir PL/I source dosyası procedure seviyesinde parse edilebilmeli, procedure içindeki assignment, CALL, IF, DO, DO WHILE ve DO UNTIL statement modelleri mevcut statement parser pipeline ile korunmalıdır.

## Sonraki Milestone

P07 — Legacy PL/I Yapıları

---

# P07 — Legacy PL/I Yapıları

## Durum ✅ Tamamlandı

## Amaç

Kurumsal PL/I projelerinde yaygın kullanılan legacy yapıların parser tarafından kaybedilmeden okunmasını ve syntax tree üzerinde taşınmasını sağlamak.

Bu fazda ilk hedef, legacy yapıların tamamını detaylı semantic modele dönüştürmek değildir.

Öncelik, gerçek PL/I kaynak kodlarında sık görülen legacy satırların parser tarafından güvenli şekilde tanınması, statement pipeline içinde taşınması ve ileride semantic / generator adımlarına temel oluşturmasıdır.

## Milestone Durumu

* ✅ P07.1 — Embedded SQL Statement Foundation
* ✅ P07.2 — Compiler Directive Statement Foundation
* ✅ P07.3 — Legacy Statement Expansion
* ✅ P07.4 — Legacy Statement End-to-End Tests
* ✅ P07.5 — Legacy Statement Documentation Closure

## İlk Kapsam

* EXEC SQL statement model foundation
* EXEC SQL statement parser foundation
* %INCLUDE, %PAGE, %EJECT gibi compiler directive satırlarının statement pipeline içinde taşınması
* Compiler directive argument bilgisinin korunması
* Procedure içinde legacy statement parse desteği
* Syntax visitor / walker traversal desteği
* Parser test coverage

## Bilinçli Olarak İlk Kapsam Dışı Bırakılanlar

* SQL grammar parser
* SQL statement semantic classification
* SQL host variable analysis
* INCLUDE dosya çözümleme
* COPYLIB fiziksel dosya okuma
* Macro expansion
* CICS semantic modelleme
* EGL SQL generation
* EGL CICS generation

## Başarı Kriteri

Gerçek kurumsal PL/I projelerinde sık görülen EXEC SQL ve compiler directive benzeri legacy yapılar parser tarafından kaybedilmeden syntax tree üzerinde taşınabilmelidir.

Bu statement'lar top-level ve procedure body içinde mevcut statement pipeline üzerinden parse edilebilmelidir.

## Sonraki Faz

P08 — Dönüşüm Kalitesini Artırma

---

# P08 — Dönüşüm Kalitesini Artırma

## Durum ✅ Tamamlandı

## Amaç

Parser, syntax tree, transpiler ve generator tarafında dönüşüm kalitesini artırmak.

Bu fazda öncelik yeni PL/I syntax eklemek değildir.

Öncelik mevcut parser modeli, syntax tree yapısı, diagnostic davranışı ve EGL output kalitesinin stabil hale getirilmesidir.

## Milestone Durumu

* ✅ P08.1 — Parser Model Stabilization
* ✅ P08.2 — Generator Quality Improvements
* ✅ P08.3 — Diagnostic Improvements
* ✅ P08.4 — Regression Test Suite
* ✅ P08.5 — Performance Improvements

## İlk Kapsam

* Syntax tree model ailesinin gözden geçirilmesi
* Parser modelinin yeni statement türlerine hazır olduğunun doğrulanması
* Visitor / Walker coverage kontrolü
* EGL generator output tutarlılığının iyileştirilmesi
* Diagnostic mesajlarının standardize edilmesi
* Regression test setinin güçlendirilmesi

## Bilinçli Olarak İlk Kapsam Dışı Bırakılanlar

* Full expression parser
* SQL grammar parser
* Semantic analysis
* Symbol table
* Scope analysis
* Yeni hedef dil generatorları
* Macro expansion
* INCLUDE dosya çözümleme

## Başarı Kriteri

Parser ve syntax tree modeli stabil kabul edilebilmelidir.

Yeni PL/I statement türleri eklendiğinde mevcut çekirdek mimaride değişiklik gerekmemeli; yalnızca ilgili statement modeli, parser, dispatcher bağlantısı ve testleri eklenerek ilerlenebilmelidir.

EGL output okunabilir, tutarlı ve mevcut naming / indentation standartlarıyla uyumlu olmalıdır.

## Sonraki Milestone

P09 — Semantic Analysis

---

# P09 — Semantic Analysis

# P09 — Semantic Analysis

## Durum ⏳ Devam Ediyor

## Amaç

Parser tarafından oluşturulan PL/I syntax modelini semantic olarak analiz etmek ve transpiler katmanını daha güvenli hale getirmek.

Bu fazın ilk hedefi tam kapsamlı compiler semantic engine oluşturmak değildir.

Öncelik, declaration ve statement modelleri üzerinden temel sembol bilgisi üretmek, duplicate declaration gibi açık hataları yakalamak ve ileride type resolution / scope analysis adımlarına temel oluşturmaktır.

## Milestone Durumu

* ✅ P09.1 — Semantic Model Foundation
* ✅ P09.2 — Symbol Table Foundation
* ⏳ P09.3 — Duplicate Declaration Diagnostics
* ⏳ P09.4 — Basic Reference Analysis
* ⏳ P09.5 — Semantic Analysis Regression Tests

## İlk Kapsam

* Semantic analysis result modeli
* Symbol model foundation
* Global declaration symbol table
* Duplicate declaration detection
* Basit identifier reference analizi
* Semantic diagnostic üretimi
* Parser output'unun semantic analyzer giriş modeli olarak kullanılması

## Bilinçli Olarak İlk Kapsam Dışı Bırakılanlar

* Full type resolution
* Full expression semantic analysis
* Scope stack
* Procedure parameter binding
* SQL host variable semantic analysis
* INCLUDE resolution
* Macro expansion
* Data flow analysis
* Control flow graph

## Başarı Kriteri

Syntax tree semantic analyzer tarafından okunabilmeli, global declaration sembolleri çıkarılabilmeli ve duplicate declaration gibi temel semantic hatalar diagnostic olarak raporlanabilmelidir.

Semantic analyzer mevcut parser/transpiler mimarisini bozmadan ayrı bir analiz katmanı olarak eklenmelidir.

## Sonraki Milestone

P09.3 — Duplicate Declaration Diagnostics

---

# P10 — IDE Entegrasyonu

## Durum

⏳ Planlandı

## Amaç

LegacyCodeTransformer'ın IDE içerisinden kullanılabilir hale getirilmesi.

## Kapsam

* Eclipse tabanlı eklenti
* IBM RBD entegrasyonu
* Tek dosya dönüşümü
* Çoklu dosya dönüşümü
* Progress gösterimi

## Başarı Kriteri

Kullanıcı IDE üzerinden PL/I dosyalarını doğrudan dönüştürebilmelidir.

## Sonraki Faz

P11 — Yeni Hedef Diller

---

# P11 — Yeni Hedef Diller

## Durum

⏳ Planlandı

## Amaç

Mevcut parser ve semantic altyapısını kullanarak farklı hedef dillere dönüşüm
yapabilmek.

## İlk Hedefler

* PL/I → C#
* EGL → C#
* PL/I → Java
* PL/I → Kotlin (Opsiyonel)

## Başarı Kriteri

Yeni hedef dillere dönüşüm yalnızca yeni generator katmanları eklenerek
gerçekleştirilebilmelidir.

## Sonraki Faz

P12 — Kurumsal Özellikler

---

# P12 — Kurumsal Özellikler

## Durum

⏳ Planlandı

## Amaç

Kurumsal projelerde ihtiyaç duyulacak yardımcı araçları geliştirmek.

## İlk Hedefler

* Batch Conversion
* Klasör bazlı dönüşüm
* Dönüşüm raporları
* Satır eşleme (Line Mapping)
* Performans ölçümleri
* Loglama
* Konfigürasyon profilleri

## Başarı Kriteri

Binlerce PL/I dosyası kurumsal ölçekte güvenilir ve raporlanabilir şekilde
dönüştürülebilmelidir.

## Sonraki Faz

Projenin ihtiyaçlarına göre yeni fazlar eklenecektir.

---

# Uzun Vadeli Hedef

LegacyCodeTransformer'ın yalnızca **PL/I → EGL** dönüşümü yapan bir araç
olması hedeflenmemektedir.

Uzun vadeli hedef;

* Kaynak dili parse edebilen,
* Semantic model oluşturabilen,
* Farklı hedef dillere dönüştürebilen,
* Genişletilebilir,
* Sürdürülebilir,
* Test edilebilir,
* Kurumsal ölçekte kullanılabilir

bir **Legacy Code Transformation Platformu** oluşturmaktır.

---

# Mevcut Geliştirme Durumu

## Tamamlanan Fazlar

* ✅ P01 — Proje Kurulumu
* ✅ P02 — Core Foundation
* ✅ P03 — İlk PL/I → EGL Dönüşümü
* ✅ P04 — PL/I Veri Tipleri ve Parser Foundation
* ✅ P05 — PL/I Statement Pipeline
* ✅ P06 — Procedure Desteği
* ✅ P07 — Legacy PL/I Yapıları
* ✅ P08 — Dönüşüm Kalitesini Artırma

## Aktif Faz

* P09 — Semantic Analysis

## Sonraki Büyük Faz

* ⏳ P10 — IDE Entegrasyonu

---

# Yol Haritası Güncelleme Kuralları

Roadmap.md yaşayan bir dokümandır.

Aşağıdaki durumlarda güncellenmelidir:

* Yeni bir faz başlatıldığında.
* Bir milestone tamamlandığında.
* Büyük mimari kararlar roadmap'i etkilediğinde.
* Faz kapsamı değiştiğinde.
* Başarı kriterleri güncellendiğinde.

Teknik implementasyon ayrıntıları bu dokümanda tutulmaz.

Mimari kararlar Decisions.md içerisinde dokümante edilir.

Tamamlanan geliştirmeler ModuleSummaries.md içerisinde kronolojik olarak özetlenir.

Roadmap.md yalnızca projenin geliştirme yönünü ve planlanan fazlarını gösteren üst seviye referans dokümandır.