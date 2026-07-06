# Roadmap.md

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

## Sonraki Faz

P05 — PL/I Statement Desteği

---

# P05 — PL/I Statement Desteği

## Durum

🚧 Aktif olarak geliştiriliyor

## Amaç

PL/I declaration dışındaki executable statement modellerini oluşturmak, parser altyapısını statement desteği için genişletmek ve ilerleyen fazlarda transpiler katmanında kullanılacak semantic statement modelini hazırlamak.

## Kapsam

* Statement syntax modeli
* Statement parser altyapısı
* Statement dispatcher
* Assignment parser
* CALL parser
* IF parser
* DO / END parser
* Statement test altyapısı
* Expression parser hazırlığı

## İlk Teknik Adım

Statement model hiyerarşisi oluşturulacaktır.

* Pl1Statement
* Pl1Expression
* Pl1AssignmentStatement
* Pl1CallStatement
* Pl1IfStatement
* Pl1DoStatement
* Pl1BlockStatement

## İlk Desteklenecek Statement Türleri

1. Assignment
2. CALL
3. IF
4. DO / END

## Başarı Kriteri

Parser declaration dışındaki ilk executable statement türlerini syntax tree üzerinde semantic olarak temsil edebilmelidir.

İlk aşamada transpiler desteği zorunlu değildir.

## Sonraki Faz

P06 — Procedure Desteği

---

# P06 — Procedure Desteği

## Durum

⏳ Planlandı

## Amaç

PL/I procedure seviyesindeki syntax yapılarının parse edilmesini ve semantic model olarak temsil edilmesini sağlamak.

## Kapsam

* PROCEDURE parser
* ENTRY parser
* Parametre listeleri
* RETURN statement
* Local scope oluşturulması
* Procedure semantic modeli

## Başarı Kriteri

Bir PL/I source dosyası procedure seviyesinde eksiksiz parse edilebilmeli ve statement parser ile entegre çalışmalıdır.

## Sonraki Faz

P07 — Legacy PL/I Yapıları

---

# P07 — Legacy PL/I Yapıları

## Durum

⏳ Planlandı

## Amaç

Kurumsal PL/I projelerinde yaygın kullanılan legacy yapıların desteklenmesini sağlamak.

## Kapsam

* INCLUDE
* Compiler Directives
* EXEC SQL
* CICS çağrıları
* Makrolar
* COPY benzeri yapılar

## Başarı Kriteri

Gerçek kurumsal PL/I projelerinde kullanılan temel legacy yapıların parser tarafından okunabilmesi.

## Sonraki Faz

P08 — Dönüşüm Kalitesini Artırma

---

# P08 — Dönüşüm Kalitesini Artırma

## Durum

⏳ Planlandı

## Amaç

Üretilen EGL kodunun doğruluğunu, okunabilirliğini ve bakım kolaylığını artırmak.

## Kapsam

* Gelişmiş Normalizer
* Kod formatlama
* Unsupported Syntax Recovery
* Daha gelişmiş Diagnostic sistemi
* İsimlendirme kuralları
* Diagnostic iyileştirmeleri

## Başarı Kriteri

Desteklenmeyen syntax'lar mümkün olduğunca parse edilmeye devam edilmeli ve kullanıcıya anlaşılır diagnostic mesajları sunulmalıdır.

## Sonraki Faz

P09 — Semantic Analysis

---

# P09 — Semantic Analysis

## Durum

⏳ Planlandı

## Amaç

Parser tarafından oluşturulan syntax modelini analiz ederek semantic doğrulama yapmak ve transpiler katmanını beslemek.

## Kapsam

* Symbol Table
* Scope Analysis
* Duplicate Declaration Detection
* Type Resolution
* Constant Evaluation
* Semantic Diagnostic üretimi

## Başarı Kriteri

Syntax modeli semantic olarak doğrulanmalı ve transpiler yalnızca semantic olarak geçerli modeller üzerinde çalışmalıdır.

## Sonraki Faz

P10 — IDE Entegrasyonu

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

Mevcut parser ve semantic altyapısını kullanarak farklı hedef dillere dönüşüm yapabilmek.

## İlk Hedefler

* PL/I → C#
* EGL → C#
* PL/I → Java
* PL/I → Kotlin (opsiyonel)

## Başarı Kriteri

Yeni hedef dillere dönüşüm yalnızca yeni generator katmanları eklenerek gerçekleştirilebilmelidir.

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

Binlerce PL/I dosyası kurumsal ölçekte güvenilir ve raporlanabilir şekilde dönüştürülebilmelidir.

## Sonraki Faz

Projenin ihtiyaçlarına göre yeni fazlar eklenecektir.

---

# Uzun Vadeli Hedef

LegacyCodeTransformer'ın yalnızca **PL/I → EGL** dönüşümü yapan bir araç olması hedeflenmemektedir.

Uzun vadeli hedef;

Kaynak dili parse edebilen,

semantic model oluşturabilen,

farklı hedef dillere dönüştürebilen,

genişletilebilir,

sürdürülebilir,

test edilebilir,

kurumsal ölçekte kullanılabilir

bir **Legacy Code Transformation Platformu** oluşturmaktır.