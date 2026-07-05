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

``pli
DCL PARAM CHAR(08);

---

## Decision 043 - PL/I → EGL identifier isimlendirme dönüşümü strategy tabanlı yapılacaktır.

## Karar

PL/I → EGL dönüşümünde değişken, parametre ve field isimleri hardcoded tek bir casing kuralına göre dönüştürülmeyecektir.

Identifier isim dönüşümü strategy tabanlı yapılacaktır.

İlk desteklenecek naming style değerleri:

- Preserve
- CamelCase
- PascalCase

Varsayılan PL/I → EGL dönüşüm kuralı PascalCase olacaktır.

Örnek dönüşümler:

| PL/I Adı | Preserve | CamelCase | PascalCase |
|---|---|---|---|
| MUST_NO | MUST_NO | mustNo | MustNo |
| CUSTOMER_NO | CUSTOMER_NO | customerNo | CustomerNo |
| DIZI_PARAM1 | DIZI_PARAM1 | diziParam1 | DiziParam1 |
| PARAM | PARAM | param | Param |
| PARAM2 | PARAM2 | param2 | Param2 |

Parser, kaynak identifier adını değiştirmeden saklayacaktır.

Normalizer, ihtiyaç oluşmadıkça identifier casing değiştirmeyecektir.

PL/I → EGL Transpiler, seçilen naming strategy’ye göre EGL identifier adını üretecektir.

EGL Code Generator, kendisine gelen EGL modelindeki identifier adını olduğu gibi yazdıracaktır.

## Gerekçe

Identifier casing kuralı kaynak dilin syntax bilgisinden ziyade hedef dil, kurum standardı ve proje kullanım alışkanlığı ile ilgilidir.

Bu nedenle casing dönüşümünün Parser veya Generator içinde yapılması doğru değildir.

Parser yalnızca PL/I kodunu anlamalı ve kaynak adı korumalıdır.

Generator yalnızca EGL modelini kaynak koda yazdırmalıdır.

PL/I identifier adının EGL standardına göre dönüştürülmesi Transpiler sorumluluğunda kalmalıdır.

Firma EGL kodlarında çoğunlukla PascalCase kullanıldığı için varsayılan dönüşüm PascalCase olarak belirlenmiştir.

Ancak farklı projelerde veya ileride farklı hedef dil dönüşümlerinde camelCase ya da preserve davranışı gerekebileceği için yapı genişletilebilir tutulacaktır.

Bu karar, daha önceki hardcoded lower camel case davranışını strategy tabanlı hale getirir ve sonraki structure / record field dönüşümleri için merkezi isimlendirme altyapısı sağlar.

## Etkilediği Modüller

- Transpilers
- Application
- CLI
- Tests
- docs

## Durum

✅ Aktif

## Decision 044 - PL/I structure declaration ifadeleri EGL record olarak modellenecektir.

## Karar

PL/I tarafındaki seviye numaralı structure declaration ifadeleri, EGL tarafında record modeli olarak temsil edilecektir.

İlk desteklenecek PL/I structure kapsamı:

``pli
DCL 1 PARAME_LIST,
    5 PARAM CHAR(08) INIT(' '),
    5 PARAM2 CHAR(01) INIT(';');

---

## Decision 045 - PL/I structure array ifadeleri EGL basicRecord parent array field olarak üretilecektir.

## Karar

PL/I tarafında ana structure adı üzerinde dimension bilgisi varsa bu yapı EGL tarafında aynı record içinde parent array field olarak üretilecektir.

Örnek PL/I:

``pli
DCL 1 DIZI(6),
    3 DIZI_PARAM1 CHAR(01) INIT((*)' '),
    3 DIZI_PARAM2 CHAR(02) INIT((*)' '),
    3 DIZI_PARAM3 CHAR(02) INIT((*)' '),
    3 DIZI_PARAM4 CHAR(02) INIT((*)' '),
    3 DIZI_PARAM5 CHAR(08) INIT((*)' ');



## Decision 046 - EGL output casing ve indentation kuralları korunacaktır

### Karar

EGL kaynak kodu üretilirken keyword, type adı, casing, boşluk ve indentation kuralları dönüşümün parçası kabul edilecektir.

Özellikle aşağıdaki EGL keyword ve type değerleri birebir korunacaktır:

- `basicRecord`
- `sqlRecord`
- `char`
- `num`
- `smallint`
- `int`
- `decimal`

`basicRecord` değeri `BasicRecord` olarak üretilmeyecektir.

Record / structure level değerleri EGL output tarafında 5 ve 5'in katları olarak üretilecektir.

Bu standarda göre level değerleri aşağıdaki anlamda kullanılacaktır:

- `5`: parent / group field
- `10`: child field
- `15`: nested child field
- `20`: daha derin nested field

Record field çıktılarında level hiyerarşisi indentation ile görünür hale getirilecektir.

Indentation hesabı level standardına göre yapılacaktır:

- level `5` için 4 boşluk
- level `10` için 8 boşluk
- level `15` için 12 boşluk
- level `20` için 16 boşluk

Genel kural:

    indentationSpaceCount = (level / 5) * 4

Generator indentation hesabını bu standarda göre yapacaktır.

Örnek doğru çıktı:

    record RecordName type basicRecord
        5 Param1 char(80)[999];
            10 Param2 char(2);
            10 Param3 num(4);
            10 Param4 num(9,4);
    end

Aşağıdaki gibi field'ların aynı hizaya yazılması kabul edilmeyecektir:

    record RecordName type basicRecord
    5 Param1 char(80)[999];
    10 Param2 char(2);
    10 Param3 num(4);
    10 Param4 num(9,4);
    end

### Gerekçe

EGL record yapılarında level bilgisi field hiyerarşisini gösterir.

Bununla birlikte üretilen kodun okunabilir olması için bu hiyerarşinin indentation ile de görünür hale getirilmesi gerekir.

Casing, boşluk ve indentation kuralları yalnızca görsel tercih olarak ele alınmayacaktır. Bu kurallar kurum EGL kod standardının parçası kabul edilecektir.

Bu nedenle dönüşüm çıktısında `basicRecord` gibi keyword ve type adları birebir korunmalı, field hiyerarşisi de düzenli indentation ile üretilmelidir.

### Etkilediği Modüller

- EGL Code Generator
- Transpilers
- Application Tests
- Snapshot / output testleri
- Dokümantasyon

### Durum

✅ Aktif

## Decision 047 - Decision kayıtları standart başlık yapısıyla yazılacaktır.

### Karar

Tüm decision kayıtları aşağıdaki standart başlık yapısıyla yazılacaktır:

- `### Karar`
- `### Gerekçe`
- `### Etkilediği Modüller`
- `### Durum`

Yeni bir decision önerilirken veya mevcut decision düzenlenirken bu başlıkların tamamı bulunmalıdır.

### Gerekçe

Decision kayıtları proje boyunca mimari kararların izlenebilir olmasını sağlar.

Başlık yapısının her decision içinde aynı olması, dokümanın okunabilirliğini ve sürdürülebilirliğini artırır.

Eksik veya farklı formatta yazılan decision kayıtları zamanla dokümantasyonun dağınık hale gelmesine neden olur.

### Etkilediği Modüller

- Dokümantasyon
- Decisions.md
- Geliştirme süreci

### Durum

✅ Aktif

## Decision 048 - PL/I structure member array ifadeleri EGL field array olarak üretilecektir

### Karar

PL/I structure member üzerinde dimension bilgisi varsa bu bilgi EGL tarafında aynı record field üzerinde array suffix olarak üretilecektir.

Örnek PL/I:

    DCL 1 PARAME_LIST,
        5 PARAM_LIST(2) CHAR(10);

Beklenen EGL:

    record ParameList type basicRecord
            10 ParamList char(10)[2];
    end

Bu karar yalnızca structure member üzerinde bulunan field-level array dimension desteğini kapsar.

İlk kapsamda desteklenen syntax:

    5 FIELD_NAME(2) CHAR(10)
    5 FIELD_NAME(3) FIXED DECIMAL(9,4)

İlk kapsamda desteklenmeyecek syntax:

    5 FIELD_NAME CHAR(10) DIM(2)
    5 FIELD_NAME CHAR(10) DIMENSION(2)

Structure array ile member array birlikte kullanılırsa parent field length hesabında member array çarpanı dikkate alınacaktır.

Örnek PL/I:

    DCL 1 DIZI(6),
        3 DIZI_PARAM1(2) CHAR(10),
        3 DIZI_PARAM2 CHAR(5);

Beklenen EGL:

    record Dizi type basicRecord
        5 Dizi char(25)[6];
            10 DiziParam1 char(10)[2];
            10 DiziParam2 char(5);
    end

Parent field length hesabı:

    DIZI_PARAM1 => CHAR(10) * 2 = 20
    DIZI_PARAM2 => CHAR(5) = 5
    Toplam => 25

### Gerekçe

Gerçek PL/I structure tanımlarında yalnızca ana structure değil, structure member alanları da dimension bilgisi taşıyabilir.

Bu dimension bilgisinin parser aşamasında kaybedilmemesi ve EGL output tarafında field array suffix olarak üretilmesi gerekir.

EGL tarafında bu kullanım mevcut basicRecord layout standardıyla uyumludur.

Member array bilgisinin ayrı bir record olarak modellenmesi yerine aynı field üzerinde `[n]` suffix olarak üretilmesi, mevcut EGL output standardını ve field hiyerarşisini korur.

Parent structure array length hesabında member array çarpanının dikkate alınması gerekir. Aksi halde parent field `char(totalLength)[arraySize]` değeri eksik hesaplanır ve layout hatalı olur.

### Etkilediği Modüller

- PL1 Parser
- PL1 Syntax Model
- Transpilers
- EGL Record Field Model
- EGL Code Generator
- Application Tests
- Transpiler Tests
- Parser Tests
- Dokümantasyon

### Durum

✅ Aktif

## Decision 049 - PL/I nested structure ifadeleri EGL parent group field ve child field olarak üretilecektir

### Karar

PL/I structure içerisinde veri tipi olmayan ve altında daha alt seviyeli field'lar bulunan member ifadeleri nested structure / group field olarak kabul edilecektir.

Örnek PL/I:

    DCL 1 PARAME_LIST,
        5 ADRES_BILGI,
            10 IL_KOD CHAR(02),
            10 ILCE_KOD CHAR(03);

Bu yapı EGL tarafında aynı basicRecord içerisinde parent group field ve child field olarak üretilecektir.

Beklenen EGL:

    record ParameList type basicRecord
        5 AdresBilgi char(5);
            10 IlKod char(2);
            10 IlceKod char(3);
    end

Nested group field için veri tipi `char(totalLength)` olarak üretilecektir.

`totalLength`, group altındaki child field storage length toplamından hesaplanacaktır.

İlk kapsamda desteklenen length hesabı:

- `CHAR(n) => n`
- `FIXED DECIMAL(p,s) => p`
- Field-level array varsa `baseLength * arraySize`
- Nested group varsa altındaki child field toplamı

EGL level üretim standardı Decision 046 ile uyumlu olacaktır:

- Parent / group field level: `5`
- Child field level: `10`
- Daha derin nested child field level: `15`
- Daha derin seviyeler için level değeri 5 artarak devam eder

İlk kapsamda desteklenecek örnek:

    DCL 1 PARAME_LIST,
        5 ADRES_BILGI,
            10 IL_KOD CHAR(02),
            10 ILCE_KOD CHAR(03);

İlk kapsamda desteklenmeyecek konular:

- `UNION`
- `REDEFINES`
- `LIKE`
- `BASED`
- `REFER`
- Çok seviyeli karmaşık OCCURS / DIMENSION syntax
- sqlRecord mapping

### Gerekçe

Gerçek PL/I structure tanımlarında bazı member satırları doğrudan veri tipi taşımaz. Bu satırlar alt seviyeli field'ları gruplayan parent / group alanlardır.

Bu yapıların yok sayılması, alt field'ların record içerisinde bağlamını kaybetmesine neden olur.

EGL basicRecord tarafında bu group alanların aynı record içerisinde parent field olarak üretilmesi kurum standardıyla uyumludur.

Parent group field length değerinin child field toplamından hesaplanması, legacy layout bilgisinin korunmasını sağlar.

Bu karar, P04-D structure array ve P04-E member array kararlarıyla uyumludur. Aynı length hesaplama yaklaşımı nested group yapıları için de genişletilecektir.

### Etkilediği Modüller

- PL1 Parser
- PL1 Syntax Model
- Transpilers
- EGL Record Field Model
- EGL Code Generator
- Parser Tests
- Transpiler Tests
- Generator Tests
- Application Tests
- Dokümantasyon

### Durum

✅ Aktif

## Decision 050 - PL/I VARCHAR ifadeleri EGL char olarak üretilecektir

### Karar

PL/I tarafındaki `VARCHAR(n)` veri tipi parser tarafından ayrı bir PL/I veri tipi modeli olarak temsil edilecektir.

Örnek PL/I:

    DCL CUSTOMER_NAME VARCHAR(50);

Parser modeli:

    Pl1VarcharType
    - Length: 50

EGL tarafında ilk kapsamda `VARCHAR(n)` tipi `char(n)` olarak üretilecektir.

Beklenen EGL:

    CustomerName char(50);

Structure member içinde kullanım da aynı dönüşüm kuralını kullanacaktır.

Örnek PL/I:

    DCL 1 CUSTOMER_INFO,
        5 CUSTOMER_NAME VARCHAR(50);

Beklenen EGL:

    record CustomerInfo type basicRecord
            10 CustomerName char(50);
    end

Structure array ve nested group length hesabında `VARCHAR(n)` fixed length gibi ele alınacaktır.

Length hesabı:

    VARCHAR(n) => n

### Gerekçe

Kurum EGL kod standardında karakter alanlar ağırlıklı olarak `char(n)` formatında temsil edilmektedir.

Paylaşılan EGL örneklerinde `string` kullanımına rastlanmamıştır.

Bu nedenle PL/I `VARCHAR(n)` tipini EGL tarafında `string` olarak üretmek erken ve hatalı bir varsayım olur.

`VARCHAR(n)` içindeki uzunluk bilgisinin EGL `char(n)` çıktısına taşınması hem mevcut type standardıyla uyumludur hem de layout length hesabının korunmasını sağlar.

Bu karar, Decision 046 kapsamında belirtilen EGL output casing ve type koruma kurallarıyla uyumludur.

### Etkilediği Modüller

- PL1 Lexer
- PL1 Parser
- PL1 Type Model
- EGL Type Model
- EGL Code Generator
- Transpilers
- Parser Tests
- Transpiler Tests
- Generator Tests
- Application Tests
- Dokümantasyon

### Durum

✅ Aktif

## Decision 051 - PL/I numeric type mapping stratejisi aşamalı ve semantic korumalı yapılacaktır

### Karar

PL/I tarafındaki numeric veri tipleri tek seferde dar bir mapping ile ele alınmayacaktır.

Numeric type desteği aşamalı olarak geliştirilecektir.

İlk ana hedefler:

- FIXED DECIMAL / FIXED DEC
- DECIMAL FIXED / DEC FIXED
- FIXED BINARY / FIXED BIN
- BINARY FIXED / BIN FIXED
- PICTURE / PIC

PL/I tarafındaki synonym kullanımlar aynı semantic modele map edilecektir.

Decimal numeric tipler için kullanılacak PL/I model:

    Pl1FixedDecimalType
    - Precision: int
    - Scale: int?

`Scale` nullable olacaktır.

Bunun nedeni `FIXED DEC(15)` ile `FIXED DEC(15,0)` ifadelerinin aynı output'a üretilmemesi gerektiğidir.

EGL decimal output standardı:

- `FIXED DEC(15)` => `decimal(15)`
- `FIXED DEC(15,0)` => `decimal(15,0)`
- `FIXED DEC(17,2)` => `decimal(17,2)`
- `DEC FIXED(17,2)` => `decimal(17,2)`
- `FIXED DEC(09,4)` => `decimal(9,4)`

Binary fixed numeric tipler için kullanılacak PL/I model:

    Pl1FixedBinaryType
    - Precision: int
    - Scale: int?

İlk kapsamda yalnızca scale değeri olmayan veya scale değeri `0` olan binary integer mapping desteklenecektir.

EGL binary integer output standardı:

- `FIXED BIN(15)` => `smallint`
- `BIN FIXED(15)` => `smallint`
- `FIXED BINARY(15)` => `smallint`
- `BINARY FIXED(15)` => `smallint`
- `FIXED BIN(31)` => `int`
- `BIN FIXED(31)` => `int`
- `FIXED BINARY(31)` => `int`
- `BINARY FIXED(31)` => `int`

EGL tarafında küçük integer type için standart casing:

    smallint

`smallInt` kullanılmayacaktır.

Eğer mevcut kod veya testlerde `smallInt` varsa `smallint` olarak normalize edilecektir.

İlk kapsamda desteklenmeyen binary precision veya scale kullanımları diagnostic üretecektir.

Örnek desteklenmeyen kullanım:

    FIXED BIN(13,4)

Bu kullanım binary fractional alan olduğu için ilk kapsamda otomatik olarak `smallint`, `int`, `decimal` veya `num` tiplerinden birine çevrilmeyecektir.

PICTURE / PIC tipleri ayrı alt fazda ele alınacaktır.

PIC mapping için ilk taslak hedef:

- `PIC '999'` => numeric picture model
- `PIC '9'` => numeric picture model
- `PIC '(13)9V99'` => numeric picture model
- `PIC 'ZZ9'` => formatted numeric picture model

PIC tarafında `S`, `V`, `Z`, `9`, repeat count, sign ve formatting karakterleri ayrı parse edilmeden EGL output üretimi yapılmayacaktır.

### Gerekçe

PL/I numeric type sistemi birden fazla synonym, precision, scale ve picture format kombinasyonu içerir.

Dar bir mapping ile doğrudan kod üretmek yanlış EGL çıktısına neden olabilir.

Özellikle `decimal(15)` ve `decimal(15,0)` kurum standardında farklı kabul edilmektedir.

Bu nedenle decimal scale bilgisinin nullable olarak korunması gerekir.

Benzer şekilde `smallint` ve `smallInt` teknik olarak çalışsa bile dönüşüm aracının deterministik ve standart output üretmesi gerekir.

Proje standardı gereği EGL type casing değerleri birebir korunacaktır. Bu yüzden küçük integer output tipi `smallint` olarak sabitlenmiştir.

Binary fixed tarafında yaygın integer kullanımlar olan precision 15 ve 31 güvenli şekilde `smallint` ve `int` olarak üretilebilir.

Ancak scale içeren binary fixed ifadeler farklı semantic taşıdığı için ilk kapsamda diagnostic üretmek daha güvenlidir.

PIC ifadeleri ise yalnızca numeric alan değil, format bilgisi de taşıyabilir. Bu yüzden ayrı model ve ayrı mapping fazı gerektirir.

### Etkilediği Modüller

- PL1 Lexer
- PL1 Parser
- PL1 Type Model
- EGL Type Model
- EGL Code Generator
- Transpilers
- Diagnostic sistemi
- Parser Tests
- Transpiler Tests
- Generator Tests
- Application Tests
- Dokümantasyon

### Durum

✅ Aktif

## Decision 052 - PL/I PIC / PICTURE ifadeleri ayrı model ile parse edilecek ve aşamalı olarak EGL numeric type'lara dönüştürülecektir

### Karar

PL/I tarafındaki `PIC` / `PICTURE` ifadeleri ilk aşamada ayrı bir PL/I veri tipi modeli olarak temsil edilecektir.

Kullanılacak model:

    Pl1PictureType
    - RawPattern: string
    - Precision: int?
    - Scale: int?
    - IsSigned: bool
    - IsNumeric: bool
    - IsFormatted: bool

Örnek PL/I:

    DCL PARAM1 PIC '999';
    DCL PARAM2 PIC '9';
    DCL PARAM3 PIC '(13)9V99';
    DCL PARAM4 PIC 'ZZ9';
    DCL PARAM5 PIC 'S999';

İlk kapsamda yalnızca güvenli numeric picture pattern'leri EGL output'a dönüştürülecektir.

İlk desteklenecek pattern türleri:

- `PIC '9'`
- `PIC '999'`
- `PIC '(n)9'`
- `PIC '9V99'`
- `PIC '(n)9V99'`
- `PIC 'S999'`
- `PIC 'S(n)9'`
- `PIC 'S(13)9V99'`

İlk kapsamda desteklenmeyecek veya diagnostic üretilecek pattern türleri:

- `Z`
- `,`
- `.`
- `+`
- `-`
- `/`
- formatted picture pattern'leri
- alphanumeric picture pattern'leri
- currency / edit mask içeren pattern'ler

Numeric ve formatting içermeyen PIC pattern'leri EGL tarafında `num(p)` veya `num(p,s)` olarak üretilecektir.

Örnek mapping:

    PIC '9' => num(1)
    PIC '999' => num(3)
    PIC '(5)9' => num(5)
    PIC '9V99' => num(3,2)
    PIC '(13)9V99' => num(15,2)
    PIC 'S999' => num(3)
    PIC 'S(13)9V99' => num(15,2)

`S` sign bilgisini ifade eder. İlk kapsamda EGL output'a ayrıca sign suffix/prefix üretilmeyecektir. Ancak `IsSigned = true` olarak modelde korunacaktır.

`V` decimal point değildir; implied decimal point bilgisidir. Output tarafında scale hesabına dahil edilecektir.

Örnek:

    PIC '999V99'

Bu pattern toplam 5 digit taşır ve 2 digit scale içerir.

Beklenen EGL:

    num(5,2)

### Gerekçe

PL/I PIC / PICTURE ifadeleri yalnızca numeric uzunluk bilgisi taşımaz. Aynı zamanda sign, implied decimal, formatting ve edit mask bilgisi de taşıyabilir.

Bu nedenle PIC ifadelerini doğrudan `decimal`, `num`, `char` veya başka bir EGL tipe çevirmek risklidir.

İlk aşamada pattern bilgisinin kaybedilmeden `Pl1PictureType` modeli üzerinde saklanması gerekir.

Güvenli numeric alt küme desteklendikten sonra formatted picture pattern'leri ayrı karar ve ayrı mapping fazında ele alınacaktır.

Bu yaklaşım yanlış EGL output üretimini engeller ve PIC desteğini kontrollü şekilde büyütmemizi sağlar.

### Etkilediği Modüller

- PL1 Lexer
- PL1 Parser
- PL1 Type Model
- EGL Type Model
- EGL Code Generator
- Transpilers
- Diagnostic sistemi
- Parser Tests
- Transpiler Tests
- Generator Tests
- Application Tests
- Dokümantasyon

### Durum

✅ Aktif

## Decision 053 - Picture Pattern Semantic Classification

### Karar
PIC / PICTURE pattern bilgisi parser aşamasında tek sefer analiz edilecek ve semantic özellikleri Pl1PictureType modeli üzerinde saklanacaktır.

Pl1PictureType yalnızca raw pattern bilgisini taşıyan basit bir model olmayacaktır. Pattern analizinden çıkan kategori, uzunluk, precision, scale, sign ve format bilgileri de aynı model üzerinde tutulacaktır.

### Gerekçe
PIC / PICTURE syntax'ı PL/I tarafında hem numeric hem alphanumeric hem de formatted değerleri temsil edebilir.

Örnekler:

    PIC '999'
    PIC '999V99'
    PIC '(13)9V99'
    PIC 'XXX'
    PIC '(20)X'
    PIC 'ZZ9'
    PIC 'Z,ZZ9V.99'
    PIC 'S999'

Bu pattern'ların hepsini generator katmanında tekrar tekrar yorumlamak doğru değildir. Parser bu semantic analizi bir kez yapmalı, generator ise hazır semantic model üzerinden karar vermelidir.

Bu yaklaşım:
- EGL generator içinde tekrar regex/pattern çözümleme ihtiyacını azaltır.
- Gelecekte COBOL veya C# generator eklenirse aynı semantic modelin yeniden kullanılmasını sağlar.
- Diagnostic üretimini daha tutarlı hale getirir.
- Safe numeric, alphanumeric, signed numeric ve formatted PIC ayrımını merkezi hale getirir.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.PL1.Types
- LegacyCodeTransformer.PL1.Parsing
- LegacyCodeTransformer.Transpilers
- LegacyCodeTransformer.EGL
- LegacyCodeTransformer.Application
- Test projeleri

### Durum
Accepted

## Decision 054 - Assistant Output Delivery Standard

### Karar
Production code, parser, transpiler, generator, model, helper ve test önerileri verilirken her snippet kendi açıklama standardına uygun verilecektir.

Method veya test methodu önerilerinde açıklama şu bilgileri içerecektir:
- Neden var?
- Ne çözüyor?
- Hangi örneği destekliyor?
- Nerede kullanılır?
- Gelecekte neye temel olur?

Test methodlarında açıklama daha kısa olabilir ancak şu bilgiler bulunacaktır:
- Bu test neyi doğrular?
- Hangi input'u test eder?
- Beklenen temel model/çıktı nedir?

Bir class içindeki method değiştirilecekse mutlaka class path, method adı ve işlem tipi açıkça yazılacaktır.

Tek cevapta mümkün olduğunca anlamlı bir feature batch'i verilecektir. Gereksiz mikro adımlara bölünmeyecektir.

### Gerekçe
Parçalı, eksik açıklamalı veya class/method bağlamı belirtilmeden verilen öneriler uygulama hatasına neden olabilir. Projede kod standardı ve dokümantasyon standardı korunmalıdır.

### Etkilediği Modüller
- Decisions.md
- Roadmap.md
- ModuleSummaries.md
- Production code önerileri
- Unit test önerileri
- Parser / Transpiler / Generator değişiklikleri

### Durum
Accepted

## Decision 055 - EGL Record Type Selection

### Karar
PL/I structure declaration ifadeleri varsayılan olarak EGL basicRecord üretmeye devam edecektir.

sqlRecord üretimi otomatik yapılmayacaktır. İlk aşamada sqlRecord üretimi açık transpiler/application option ile seçilecektir.

### Gerekçe
PL/I structure declaration tek başına veritabanı tablosu semantiği taşımaz. Bu nedenle her structure declaration ifadesini otomatik sqlRecord yapmak semantic olarak risklidir.

basicRecord mevcut güvenli default davranıştır.

sqlRecord ise tablo metadata, column mapping, SQL context veya kullanıcı seçimi gerektirebilir. Bu metadata henüz parser modelinde bulunmadığı için sqlRecord üretimi opt-in olmalıdır.

### Etkilediği Modüller
- LegacyCodeTransformer.Transpilers
- LegacyCodeTransformer.Application
- LegacyCodeTransformer.EGL
- Test projeleri
- Roadmap.md
- ModuleSummaries.md

### Durum
Accepted

## Decision 056 - FLOAT / REAL / DOUBLE EGL Mapping

### Karar
PL/I floating point tipleri EGL tarafına aşamalı ve semantic korumalı şekilde map edilecektir.

İlk güvenli mapping kapsamı:

    REAL                  -> smallfloat
    DOUBLE                -> float
    DOUBLE PRECISION      -> float
    FLOAT                 -> float
    FLOAT BINARY          -> float
    FLOAT BIN(p)          -> float

FLOAT DECIMAL ve FLOAT DECIMAL(p) şimdilik otomatik EGL mapping kapsamı dışında bırakılacaktır.

### Gerekçe
EGL tarafında FLOAT ve SMALLFLOAT numeric primitive tipleri mevcuttur. REAL tipi single precision floating semantic taşıdığı için smallfloat ile, DOUBLE / DOUBLE PRECISION ise double precision floating semantic taşıdığı için float ile eşleştirilecektir.

FLOAT DECIMAL decimal floating semantic taşıyabileceğinden doğrudan EGL float mapping yapmak semantic kayıp riski oluşturabilir. Bu nedenle FLOAT DECIMAL için mapping kararı ayrıca değerlendirilecektir.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.Transpilers
- LegacyCodeTransformer.EGL
- LegacyCodeTransformer.Application
- Test projeleri
- Roadmap.md
- ModuleSummaries.md

### Durum
Accepted

## Decision 057 - Parser Responsibility Segregation

### Karar
PL/I parser davranışları tek sınıfta büyütülmeyecektir.

Büyük syntax aileleri kendi internal parser / helper sınıfları altında geliştirilecektir. Pl1Parser ana akışı yöneten sınıf olarak kalacak; veri tipi, initialization, dimension, structure ve ileride statement parser davranışları ayrı sınıflara taşınacaktır.

İlk refactor adımı PIC / PICTURE parsing sorumluluğunun PictureTypeParser sınıfına ayrılmasıdır.

### Gerekçe
P04 sonunda Pl1Parser declaration, data type, structure, dimension, init, picture, bit ve floating type parsing sorumluluklarını taşır hale gelmiştir.

P05 ile birlikte IF, DO, CALL, SELECT, WHEN, assignment ve expression parsing eklenecektir. Bu davranışlar aynı sınıfta büyütülürse Pl1Parser bakımı zor, test edilmesi güç ve değişiklik riski yüksek bir sınıfa dönüşür.

Parser sorumluluklarını ayırmak:
- SOLID prensiplerinden Single Responsibility Principle ile uyumludur.
- Yeni syntax ailelerinin izole geliştirilmesini sağlar.
- Unit test kapsamını daha hedefli hale getirir.
- P05 statement parser geliştirmesine temiz zemin hazırlar.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.PL1.Parsing
- LegacyCodeTransformer.PL1.Parsing.Helpers
- Test projeleri
- Roadmap.md
- ModuleSummaries.md

### Durum
Accepted