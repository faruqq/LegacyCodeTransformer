# Decisions.md

---

# Amaç

Bu doküman LegacyCodeTransformer projesinin mimari karar kayıtlarını
(Architecture Decision Records - ADR) içerir.

Bu doküman yalnızca "hangi karar alındığını" değil,
aynı zamanda "neden o kararın alındığını" da kayıt altına almayı amaçlar.

Bu sayede;

- Aylar sonra projeye geri dönüldüğünde
- Yeni geliştiriciler projeye dahil olduğunda
- Mimari değişiklikler yapılmak istendiğinde

kararların arkasındaki düşünce yapısı kolayca anlaşılabilecektir.

---

# Doküman Kullanım Kuralları

1.

Hiçbir Decision silinmez.

Yanlış olduğu düşünülse bile silinmez.

Gerekirse yerine geçen yeni Decision eklenir.

---

2.

Hiçbir Decision numarası değiştirilmez.

Numaralar proje boyunca kalıcıdır.

---

3.

Her Decision yalnızca tek bir konuyu kapsamalıdır.

Bir Decision içerisinde birden fazla mimari karar bulunmamalıdır.

---

4.

Önemli mimari değişikliklerde yeni Decision oluşturulur.

Küçük kod değişiklikleri Decision gerektirmez.

---

5.

Kod yazılmadan önce gerekiyorsa önce Decision oluşturulur.

Karar verilir.

Daha sonra kod yazılır.

---

6.

Bu doküman proje boyunca tek dosya olarak kullanılacaktır.

Decision sayısı arttıkça aynı dosyaya yeni kararlar eklenecektir.

Yeni dosya oluşturulmayacaktır.

---

# Durum Göstergeleri

✅ Aktif

🚧 Planlandı

❌ Kullanımdan Kaldırıldı

🔁 Yerine Yeni Decision Geldi

---

# Naming Decisions

---

## Decision 001 - Proje yalnızca PL/I → EGL dönüştürücüsü değildir.

### Karar

Proje yalnızca PL/I → EGL dönüştürücüsü olarak tasarlanmayacaktır.

Projenin adı:

LegacyCodeTransformer

olarak belirlenmiştir.

### Gerekçe

İlk hedef PL/I → EGL dönüşümüdür.

Ancak mimari;

- PL/I → C#
- EGL → C#
- COBOL → C#

gibi farklı dönüşümlere de açık olmalıdır.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

---

## Decision 002 - Metin tabanlı dönüşüm yerine Syntax Tree tabanlı dönüşüm kullanılacaktır.

### Karar

Dönüştürme işlemleri String.Replace(),
Regex veya benzeri metin tabanlı yöntemlerle yapılmayacaktır.

Dönüşüm süreci Syntax Tree modelleri üzerinden ilerleyecektir.

### Gerekçe

Legacy dillerde metin tabanlı dönüşümler kısa sürede yönetilemez hale gelir.

Syntax Tree yaklaşımı;

- daha güvenlidir,
- daha okunabilirdir,
- yeni hedef dillere dönüşümü kolaylaştırır.

### Etkilediği Modüller

- Parser
- Normalizer
- Transpiler
- Generator

### Durum

✅ Aktif

---

# Architecture Decisions

---

## Decision 003 - PL/I ve EGL modelleri birbirinden tamamen ayrılacaktır.

### Karar

PL/I ve EGL için ayrı Syntax Tree modelleri oluşturulacaktır.

Örneğin;

- Pl1SyntaxTree
- EglSyntaxTree

### Gerekçe

PL/I ve EGL aynı dil değildir.

Her iki dilin;

- sözdizimi,
- veri tipleri,
- ifadeleri,
- anahtar kelimeleri

birbirinden farklıdır.

Bu nedenle ortak model kullanılmayacaktır.

### Etkilediği Modüller

- Core
- PL1
- EGL
- Transpilers

### Durum

✅ Aktif

---

## Decision 004 - Parser hedef dili bilmeyecektir.

### Karar

PL/I Parser yalnızca PL/I dilini anlayacaktır.

Parser hiçbir zaman doğrudan EGL kodu üretmeyecektir.

### Gerekçe

Parser'ın tek sorumluluğu kaynak dili anlamaktır.

Hedef dile dönüştürme işi Transpiler katmanının sorumluluğudur.

Bu sayede Parser yeniden kullanılabilir hale gelir.

### Etkilediği Modüller

- PL1

### Durum

✅ Aktif

---

## Decision 005 - Generator kaynak dili bilmeyecektir.

### Karar

Generator yalnızca kendi hedef dilinin Syntax Tree modelini bilecektir.

Örneğin;

EglCodeGenerator yalnızca

EglSyntaxTree

ile çalışacaktır.

### Gerekçe

Generator'ın görevi yalnızca hedef kaynak kodu üretmektir.

Kaynak dile ait hiçbir bilgi Generator katmanına taşınmayacaktır.

### Etkilediği Modüller

- EGL

### Durum

✅ Aktif

---

## Decision 006 - Normalizer ilk sürümden itibaren projeye dahil edilecektir.

### Karar

Normalizer katmanı ilk sürümden itibaren mimaride yer alacaktır.

Ancak ilk sürümde yalnızca temel normalizasyon işlemleri yapılacaktır.

### Gerekçe

Legacy kodlarda aynı anlamı taşıyan farklı yazım biçimleri bulunabilir.

Normalizer;

- Transpiler'ın daha sade olmasını sağlar.
- Tekrarlayan dönüşüm kurallarını azaltır.
- Daha okunabilir Syntax Tree üretir.

### Etkilediği Modüller

- PL1

### Durum

✅ Aktif

---

## Decision 007 - Semantic Analyzer ilk sürümde geliştirilmeyecektir.

### Karar

Semantic Analyzer ilk sürüm kapsamına alınmayacaktır.

### Gerekçe

İlk hedef;

PL/I kodunu okuyup

EGL kodu üretebilmektir.

Anlamsal doğrulamalar;

- tip kontrolü
- prosedür doğrulaması
- parametre doğrulaması

ilerleyen sürümlerde eklenecektir.

### Etkilediği Modüller

- Gelecek geliştirmeler

### Durum

🚧 Planlandı

---

## Decision 008 - Over-engineering'den kaçınılacaktır.

### Karar

İhtiyaç oluşmadan soyutlama yapılmayacaktır.

Örneğin;

- Visitor
- SyntaxWalker
- Binder
- SemanticModel
- StatementSyntax
- ExpressionSyntax

gibi yapılar gerçek ihtiyaç oluşmadan projeye eklenmeyecektir.

### Gerekçe

Projenin ilk amacı çalışan ve sürdürülebilir bir transpiler altyapısı oluşturmaktır.

Gereksiz soyutlamalar öğrenme sürecini zorlaştırır
ve geliştirme hızını düşürür.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

# Development Standards

---

## Decision 009 - İlk Parser hedefi PL/I DCL deyimidir.

### Karar

İlk çalışan Parser hedefi aşağıdaki PL/I deyimini parse edebilmek olacaktır.

```pli
DCL MUST_NO FIXED DECIMAL(8);
```

### Gerekçe

Bu ifade;

- Declaration
- Identifier
- Data Type
- Numeric Precision

gibi Parser'ın temel yeteneklerini test etmek için yeterlidir.

Küçük fakat anlamlı ilk kilometre taşıdır.

### Etkilediği Modüller

- PL1
- Parser

### Durum

✅ Aktif

---

## Decision 010 - Test projeleri başlangıçta oluşturulacak ancak ihtiyaç oldukça geliştirilecektir.

### Karar

Test projeleri Solution içerisinde en baştan yer alacaktır.

Ancak bütün testler ilk günden yazılmayacaktır.

Her modül tamamlandıkça ilgili Unit Test'leri eklenecektir.

### Gerekçe

Parser ve Generator gibi katmanlarda
önce temel yapı oluşturulmalı,
ardından testleri yazılmalıdır.

Bu yaklaşım geliştirme hızını artıracaktır.

Tercih edilen test çatısı:

xUnit

### Etkilediği Modüller

- Tests

### Durum

✅ Aktif

---

## Decision 011 - SourceLocation bütün SyntaxNode'larda bulunacaktır.

### Karar

Her SyntaxNode kaynak kod içerisindeki konum bilgisini taşıyacaktır.

Bu amaçla SourceLocation modeli kullanılacaktır.

### Gerekçe

Parser, Normalizer, Transpiler ve Generator
oluşturdukları hata veya uyarıları
kaynak kod satırına bağlayabilmelidir.

Bu aynı zamanda ileride Source Mapping desteği için de temel oluşturacaktır.

### Etkilediği Modüller

- Core
- Parser
- Diagnostics

### Durum

✅ Aktif

---

## Decision 012 - Projedeki açıklamalar Türkçe yazılacaktır.

### Karar

Kod içerisindeki XML Documentation ve gerekli açıklama satırları Türkçe yazılacaktır.

Kod isimlendirmeleri ise İngilizce olacaktır.

### Gerekçe

Bu proje aynı zamanda öğrenme amacı da taşımaktadır.

Aylar sonra projeye geri dönüldüğünde
tasarım kararlarının kolay anlaşılması hedeflenmektedir.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

---

## Decision 013 - XML dokümantasyon standart hale getirilecektir.

### Karar

Public sınıflar,
interface'ler
ve önemli public methodlar
XML Documentation içerecektir.

XML açıklamaları yalnızca
"ne yaptığını"
anlatmayacaktır.

### Gerekçe

Kodun amacı kadar
neden oluşturulduğu da
projenin anlaşılması açısından önemlidir.

XML açıklamaları aşağıdaki sorulara mümkün olduğunca cevap vermelidir.

- Bu sınıf neyi temsil ediyor?
- Neden oluşturuldu?
- Pipeline'ın hangi aşamasında kullanılıyor?
- Gelecekte nasıl genişleyebilir?

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

---

## Decision 014 - SourceLocation ilk sürümde sade tutulacaktır.

### Karar

SourceLocation modeli ilk sürümde yalnızca aşağıdaki bilgileri taşıyacaktır.

- Line
- Column
- Position

### Gerekçe

Şimdilik bu bilgiler hata raporları için yeterlidir.

Aşağıdaki bilgiler ihtiyaç oluştuğunda eklenecektir.

- EndPosition
- Length
- SourceSpan
- FileName

### Etkilediği Modüller

- Core

### Durum

✅ Aktif

---

## Decision 015 - Proje .NET 8 üzerinde geliştirilecektir.

### Karar

Solution içerisindeki bütün projeler
.NET 8 hedef framework'ü kullanacaktır.

### Gerekçe

- LTS sürüm olması
- Güncel C# özelliklerinden faydalanabilmek
- Performans iyileştirmeleri
- Uzun süre desteklenecek olması

Framework sürümü ihtiyaç oluşmadığı sürece değiştirilmeyecektir.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

---

## Decision 016 - SyntaxNode minimal ve immutable olacaktır.

### Karar

SyntaxNode yalnızca gerçekten gerekli ortak özellikleri taşıyacaktır.

İlk sürümde yalnızca

SourceLocation

özelliği bulunacaktır.

SyntaxNode immutable olacaktır.

### Gerekçe

Şu anda ihtiyaç duyulmayan

- Guid
- Parent
- GetChildren()
- Visitor
- Annotation

gibi yapılar eklenmeyecektir.

İhtiyaç oluştuğunda genişletilecektir.

### Etkilediği Modüller

- Core

### Durum

✅ Aktif

---

## Decision 017 - Geliştirme sürecinde ADR Lite kullanılacaktır.

### Karar

Her önemli modül veya temel sınıf
kod yazılmadan önce ADR Lite formatında değerlendirilecektir.

### ADR Lite Adımları

1. Bu sınıf / modül neden var?

2. Alternatif tasarımlar nelerdir?

3. Neden bu tasarım seçildi?

4. Kod

5. Gelecekte değişebilir mi?

6. Dokümantasyon güncellemesi gerekiyor mu?

### Gerekçe

Bu proje yalnızca çalışan kod üretmeyi değil,

doğru mimari kararları kayıt altına almayı da hedeflemektedir.

Bu yaklaşım;

- öğrenmeyi kolaylaştıracaktır.
- gelecekte bakım maliyetini azaltacaktır.
- yeni geliştiricilerin projeyi anlamasını hızlandıracaktır.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

# Dokümantasyon Standartları

---

## Decision 018 - Proje dokümantasyonu yaşayan bir yapı olacaktır.

### Karar

Projedeki dokümanlar yalnızca proje başlangıcında oluşturulmayacaktır.

Proje geliştikçe güncellenecek ve yaşayan dokümanlar olarak kullanılacaktır.

### Gerekçe

LegacyCodeTransformer uzun soluklu bir projedir.

Dokümantasyonun koddan kopmaması,
mimari kararların zaman içerisinde kaybolmaması hedeflenmektedir.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 019 - Docs klasörü tek dokümantasyon kaynağı olacaktır.

### Karar

Projeye ait teknik dokümantasyon yalnızca docs klasörü altında tutulacaktır.

Başlangıçta aşağıdaki dosyalar kullanılacaktır.

- Architecture.md
- Decisions.md
- Roadmap.md
- ModuleSummaries.md
- Glossary.md

Yeni doküman ihtiyacı doğmadığı sürece farklı dosyalar oluşturulmayacaktır.

### Gerekçe

Dokümantasyonun tek merkezden yönetilmesi bakım kolaylığı sağlar.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 020 - Glossary.md proje sözlüğü olarak kullanılacaktır.

### Karar

Projede kullanılan teknik kavramlar Glossary.md içerisinde açıklanacaktır.

Örnekler;

- Syntax Tree
- Syntax Node
- Parser
- Lexer
- Normalizer
- Generator
- Diagnostic
- Token
- Transpiler

### Gerekçe

Bu proje compiler/transpiler mimarisi içerdiği için
alışılmış CRUD projelerinden farklı kavramlar kullanmaktadır.

Kavramların tek yerde açıklanması öğrenme sürecini kolaylaştıracaktır.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 021 - Her tamamlanan modül ModuleSummaries.md dosyasına eklenecektir.

### Karar

Anlamlı bir modül tamamlandığında
ModuleSummaries.md dosyasına kısa bir özet eklenecektir.

Özet aşağıdaki bilgileri içermelidir.

- Tamamlanan modül
- Yapılan çalışmalar
- Alınan önemli kararlar
- Sonraki hedef

### Gerekçe

Modül bazlı ilerleme geçmişinin takip edilebilmesi amaçlanmaktadır.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 022 - Roadmap.md geliştirme sürecini takip edecektir.

### Karar

Roadmap.md yalnızca yapılacak işleri ve mevcut ilerlemeyi takip edecektir.

Roadmap içerisinde mimari karar bulunmayacaktır.

### Gerekçe

Karar kayıtları ile geliştirme planının birbirinden ayrılması okunabilirliği artırmaktadır.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 023 - Doküman güncellemeleri geliştirmenin bir parçasıdır.

### Karar

Her modül sonunda gerekli doküman güncellemeleri değerlendirilecektir.

Güncelleme gerekiyorsa aşağıdaki format kullanılacaktır.

📄 Doküman Güncellemesi

- Decisions.md
- Architecture.md
- Roadmap.md
- ModuleSummaries.md
- Glossary.md

Her doküman için;

- Güncellenecek
- Değişiklik Yok

şeklinde bilgi verilecektir.

### Gerekçe

Hiçbir dokümanın unutulmaması hedeflenmektedir.

### Etkilediği Modüller

- docs

### Durum

✅ Aktif

---

## Decision 024 - Hiçbir Decision silinmeyecektir.

### Karar

Bir Decision geçerliliğini kaybetse bile silinmeyecektir.

Yerine yeni bir Decision oluşturulacaktır.

Eski Decision içerisinde gerekli yönlendirme yapılacaktır.

Örnek;

Durum

🔁 Yerine Decision 042 kullanılacaktır.

### Gerekçe

Projenin mimari evriminin takip edilebilmesi amaçlanmaktadır.

### Etkilediği Modüller

- Decisions.md

### Durum

✅ Aktif

---

## Decision 025 - Kod geliştirme süreci standart hale getirilmiştir.

### Karar

Projedeki her önemli geliştirme aşağıdaki sıraya göre yapılacaktır.

1.

ADR Lite

↓

2.

Gerekirse yeni Decision

↓

3.

Kod geliştirme

↓

4.

Unit Test

↓

5.

Module Summary güncellemesi

↓

6.

Roadmap güncellemesi

↓

7.

Gerekirse Glossary güncellemesi

### Gerekçe

Kod ile mimarinin birlikte ilerlemesi hedeflenmektedir.

Bu sayede;

- Dokümantasyon güncel kalacaktır.
- Kararlar kayıt altında olacaktır.
- Testler unutulmayacaktır.
- Yol haritası sürekli güncel olacaktır.

### Etkilediği Modüller

- Tüm Solution

### Durum

✅ Aktif

## Decision 026 - SyntaxTree minimal temel sınıf olarak tasarlanacaktır.

### Karar

SyntaxTree sınıfı, dil bazlı syntax tree modelleri için ortak temel sınıf olacaktır.

İlk sürümde yalnızca SourceLocation bilgisini taşıyacaktır.

### Gerekçe

Pl1SyntaxTree, EglSyntaxTree ve gelecekte eklenecek diğer syntax tree modellerinin ortak bir temel sınıfa sahip olması istenmektedir.

Ancak FilePath, SourceText, RootNode, Diagnostics gibi bilgiler gerçek ihtiyaç oluşmadan eklenmeyecektir.

### Etkilediği Modüller

- Core
- PL1
- EGL
- Transpilers

### Durum

✅ Aktif

## Decision 027 - DiagnosticSeverity enum olarak tanımlanacaktır.

### Karar

Diagnostic mesajlarının önem seviyesi enum ile temsil edilecektir.

İlk sürümde aşağıdaki seviyeler kullanılacaktır.

- Info
- Warning
- Error

### Gerekçe

String tabanlı severity kullanımı tutarsızlığa yol açabilir.

Boolean tabanlı hata kontrolü ise uyarı ve bilgilendirme mesajlarını ifade etmek için yetersizdir.

Enum kullanımı daha sade, güvenli ve okunabilir bir çözüm sağlar.

### Etkilediği Modüller

- Core
- Diagnostics
- Parser
- Normalizer
- Transpiler
- Generator
- Application
- CLI

### Durum

✅ Aktif

## Decision 028 - Bütün katmanlar ortak Diagnostic modeli kullanacaktır.

### Karar

Parser, Normalizer, Transpiler ve Generator ortak Diagnostic modeli kullanacaktır.

Diagnostic ilk sürümde aşağıdaki bilgileri taşıyacaktır.

- Severity
- Message
- Location

DiagnosticCode gibi alanlar ihtiyaç oluştuğunda eklenecektir.

### Gerekçe

Katmanlar arasında ortak hata ve uyarı modeli oluşturmak,
Application ve CLI katmanlarını sadeleştirir.

### Etkilediği Modüller

- Core
- Parser
- Normalizer
- Transpiler
- Generator
- Application
- CLI

### Durum

✅ Aktif

## Decision 029 - Diagnostic mesajları DiagnosticBag içinde toplanacaktır.

### Karar

Parser, Normalizer, Transpiler ve Generator aşamalarında oluşan Diagnostic mesajları DiagnosticBag içerisinde toplanacaktır.

### Gerekçe

Her katmanda doğrudan List<Diagnostic> kullanmak tekrar eden kodlara yol açar.

DiagnosticBag;

- AddError
- AddWarning
- AddInfo
- HasErrors
- HasWarnings

gibi ortak davranışları tek yerde toplar.

### Etkilediği Modüller

- Core
- Parser
- Normalizer
- Transpiler
- Generator
- Application

### Durum

✅ Aktif

## Decision 030 - Parser sonuçları ParseResult ile döndürülecektir.

### Karar

Parser çıktıları doğrudan SyntaxTree olarak dönmeyecektir.

Parser sonucunda aşağıdaki bilgiler birlikte döndürülecektir.

- SyntaxTree
- Diagnostics
- Success

Bu amaçla generic ParseResult<TSyntaxTree> modeli kullanılacaktır.

### Gerekçe

Parser işleminde yalnızca başarılı çıktı değil,
hata ve uyarılar da oluşabilir.

ParseResult modeli, parser sonucunu standart hale getirir
ve farklı diller için tekrar kullanılabilir.

### Etkilediği Modüller

- Core
- PL1
- Application

### Durum

✅ Aktif

## Decision 031 - Nihai dönüşüm çıktısı ConversionResult ile döndürülecektir.

### Karar

Application katmanı dönüşüm sonucunu doğrudan string olarak döndürmeyecektir.

Nihai dönüşüm sonucu aşağıdaki bilgileri içerecektir.

- Output
- Diagnostics
- Success

Bu amaçla ConversionResult modeli kullanılacaktır.

### Gerekçe

Dönüşüm süreci yalnızca hedef kod üretmez.

Aynı zamanda hata, uyarı ve bilgilendirme mesajları da üretir.

ConversionResult modeli, bu bilgileri standart ve tek bir sonuç nesnesinde toplar.

### Etkilediği Modüller

- Core
- Application
- CLI

### Durum

✅ Aktif

## Decision 032 - İlk PL/I modeli DCL FIXED DECIMAL hedefiyle sınırlı olacaktır.

### Karar

İlk PL/I syntax modeli yalnızca DCL değişken tanımı ve FIXED DECIMAL veri tipini destekleyecek şekilde oluşturulacaktır.

İlk model kapsamı:

- Pl1SyntaxTree
- Pl1VariableDeclaration
- Pl1DataType
- Pl1FixedDecimalType

### Gerekçe

İlk parser hedefi olan DCL MUST_NO FIXED DECIMAL(8); ifadesini modellemek için bu yapı yeterlidir.

Tüm PL/I dilini baştan modellemek over-engineering olacaktır.

### Etkilediği Modüller

- PL1
- Transpilers

### Durum

✅ Aktif

## Decision 033 - PL/I Lexer ayrı bir katman olarak geliştirilecektir.

### Karar

PL/I kaynak kodu Parser tarafından doğrudan karakter karakter okunmayacaktır.

Önce Lexer tarafından token listesine dönüştürülecektir.

İlk lexer kapsamı:

- DCL
- FIXED
- DECIMAL
- Identifier
- Number
- OpenParenthesis
- CloseParenthesis
- Semicolon
- Comma
- EndOfFile
- Unknown

### Gerekçe

Lexer, Parser'ın sorumluluğunu sadeleştirir.

Parser'ın ham metin yerine token listesi üzerinden çalışması,
daha okunabilir ve genişletilebilir bir yapı sağlar.

### Etkilediği Modüller

- PL1
- Parser
- Application

### Durum

✅ Aktif

## Decision 034 - PL/I Parser ilk sürümde recursive-descent yaklaşımıyla geliştirilecektir.

### Karar

PL/I Parser ilk sürümde token listesini okuyarak çalışan küçük ve kontrollü bir recursive-descent parser olarak geliştirilecektir.

İlk parser kapsamı:

- DCL
- Identifier
- FIXED DECIMAL
- Precision
- Opsiyonel Scale
- Semicolon

### Gerekçe

ANTLR gibi daha büyük parser altyapıları ilk sürüm için gereğinden fazla karmaşıktır.

İlk hedef yalnızca DCL MUST_NO FIXED DECIMAL(8); gibi basit declaration ifadelerini parse etmektir.

Recursive-descent parser bu hedef için yeterince sade, anlaşılır ve genişletilebilir bir çözümdür.

### Etkilediği Modüller

- PL1
- Parser
- Application

### Durum

✅ Aktif

## Decision 035 - PL/I Normalizer ilk sürümde minimal tutulacaktır.

### Karar

PL/I Normalizer katmanı mimaride yer alacaktır.

Ancak ilk sürümde yalnızca Pl1SyntaxTree modelini alıp doğrulayacak ve değiştirmeden döndürecektir.

### Gerekçe

Normalizer katmanının baştan bulunması, ileride Transpiler karmaşıklığını azaltacaktır.

Ancak ilk sürümde gereksiz normalizasyon kuralları eklemek over-engineering olacaktır.

### Etkilediği Modüller

- PL1
- Application
- Transpilers

### Durum

✅ Aktif

## Decision 036 - İlk EGL modeli decimal declaration hedefiyle sınırlı olacaktır.

### Karar

İlk EGL syntax modeli yalnızca değişken declaration ve decimal veri tipini destekleyecek şekilde oluşturulacaktır.

İlk model kapsamı:

- EglSyntaxTree
- EglVariableDeclaration
- EglDataType
- EglDecimalType

### Gerekçe

İlk dönüşüm hedefi olan PL/I FIXED DECIMAL tipini EGL decimal tipine çevirmek için bu yapı yeterlidir.

Tüm EGL dilini baştan modellemek over-engineering olacaktır.

### Etkilediği Modüller

- EGL
- Transpilers
- Generator

### Durum

✅ Aktif

## Decision 037 - PL/I → EGL dönüşümü ayrı Transpiler katmanında yapılacaktır.

### Karar

PL/I modeli doğrudan EGL string çıktısına dönüştürülmeyecektir.

Önce Pl1SyntaxTree modelinden EglSyntaxTree modeli üretilecektir.

İlk Transpiler kapsamı:

- Pl1VariableDeclaration → EglVariableDeclaration
- Pl1FixedDecimalType → EglDecimalType
- PL/I upper snake case isimlendirme → EGL lower camel case isimlendirme

### Gerekçe

Parser hedef dili bilmemelidir.

Generator kaynak dili bilmemelidir.

Bu iki katman arasındaki dönüşüm sorumluluğu Transpiler katmanında tutulacaktır.

### Etkilediği Modüller

- Transpilers
- PL1
- EGL
- Application

### Durum

✅ Aktif

## Decision 038 - EGL kaynak kodu EglCodeGenerator tarafından üretilecektir.

### Karar

EGL kaynak kodu doğrudan Transpiler tarafından üretilmeyecektir.

Transpiler yalnızca EglSyntaxTree üretecektir.

EglSyntaxTree modelinden gerçek EGL kaynak kodu üretme sorumluluğu EglCodeGenerator sınıfında olacaktır.

İlk Generator kapsamı:

- EglVariableDeclaration
- EglDecimalType

### Gerekçe

Transpiler ve Generator sorumluluklarının ayrılması mimarinin sürdürülebilirliğini artırır.

Generator yalnızca hedef dilin kod yazım formatını bilmelidir.

### Etkilediği Modüller

- EGL
- Transpilers
- Application
- CLI

### Durum

✅ Aktif

## Decision 039 - Dönüşüm pipeline'ı Application katmanında yönetilecektir.

### Karar

Lexer, Parser, Normalizer, Transpiler ve Generator doğrudan CLI tarafından yönetilmeyecektir.

PL/I → EGL dönüşüm akışı ConversionService üzerinden koordine edilecektir.

### Gerekçe

CLI, GUI veya IDE entegrasyonu gibi dış giriş noktalarının dönüşüm detaylarını bilmemesi gerekir.

Application katmanı pipeline orchestration sorumluluğunu üstlenir.

### Etkilediği Modüller

- Application
- CLI
- PL1
- EGL
- Transpilers

### Durum

✅ Aktif

## Decision 040 - İlk CLI sürümü basit args tabanlı olacaktır.

### Karar

İlk CLI sürümünde gelişmiş command-line parser paketi kullanılmayacaktır.

CLI, komut satırı argümanlarından PL/I source text alacak ve sonucu konsola yazacaktır.

Argüman verilmezse varsayılan örnek olarak aşağıdaki kaynak kod kullanılacaktır.

DCL MUST_NO FIXED DECIMAL(8);

### Gerekçe

İlk hedef pipeline'ın uçtan uca çalıştığını görmektir.

Gelişmiş CLI komut yapısı ve dosya okuma/yazma özellikleri daha sonra eklenebilir.

### Etkilediği Modüller

- CLI
- Application

### Durum

✅ Aktif

## Decision 041 - İlk uçtan uca testler Application katmanında yazılacaktır.

### Karar

İlk testler ConversionService üzerinden PL/I → EGL pipeline akışını doğrulayacaktır.

### Gerekçe

İlk hedef tek tek sınıfları değil, uçtan uca dönüşümün çalıştığını güvence altına almaktır.

### Etkilediği Modüller

- Application
- Tests

### Durum

✅ Aktif

## Decision 042 - PL/I CHAR ve CHARACTER tipleri EGL char tipine dönüştürülecektir.

### Karar

PL/I tarafındaki CHAR(n) ve CHARACTER(n) karakter veri tipi tanımları, EGL tarafında char(n) veri tipine dönüştürülecektir.

Örnek PL/I:

```pli
DCL PARAM CHAR(08);

---

## Decision 043 - PL/I → EGL identifier isimlendirme dönüşümü strategy tabanlı yapılacaktır.

### Karar

PL/I → EGL dönüşümünde değişken, parametre ve field isimleri hardcoded tek bir casing kuralına göre dönüştürülmeyecektir.

Identifier isim dönüşümü strategy tabanlı yapılacaktır.

İlk desteklenecek naming style değerleri:

- Preserve
- CamelCase
- PascalCase

Varsayılan PL/I → EGL dönüşüm kuralı PascalCase olacaktır.

Örnekler:

```text
PL/I adı        Preserve        CamelCase       PascalCase
MUST_NO         MUST_NO         mustNo          MustNo
CUSTOMER_NO     CUSTOMER_NO     customerNo      CustomerNo
DIZI_PARAM1     DIZI_PARAM1     diziParam1      DiziParam1
PARAM           PARAM           param           Param
PARAM2          PARAM2          param2          Param2