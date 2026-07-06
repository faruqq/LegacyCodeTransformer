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

## Decision 058 - Parser Infrastructure Components

### Karar
Parser helper sınıflarında ortak token state, token tüketme ve diagnostic üretme davranışları tekrar edilmeyecektir.

Parser altyapısı aşağıdaki bileşenlerle standartlaştırılacaktır:
- ParseContext
- ParserBase
- ParserDiagnosticFactory
- Ortak parse result modeli

İlk aşamada ParseContext, ParserBase ve ParserDiagnosticFactory eklenecek; helper parser sınıfları kademeli olarak bu altyapıya taşınacaktır.

### Gerekçe
P04 refactor sürecinde çok sayıda helper parser sınıfı oluşmuştur. Bu sınıflarda Current, Advance, Consume, IsAtEnd ve diagnostic üretme davranışları tekrar etmektedir.

Ortak parser altyapısı:
- Kod tekrarını azaltır.
- Parser davranışını standartlaştırır.
- Yeni statement parser geliştirmelerini kolaylaştırır.
- SOLID prensiplerinden Single Responsibility ve Open/Closed prensiplerini destekler.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.PL1.Parsing
- LegacyCodeTransformer.PL1.Parsing.Helpers
- Parser helper testleri
- ModuleSummaries.md

### Durum
Accepted

## Decision 059 - Statement Parser P05 Kademeli Başlatılacaktır

### Karar
P05 kapsamında PL/I statement desteği kademeli olarak geliştirilecektir.

İlk aşamada parser, top-level DCL dışındaki tokenları doğrudan geniş statement parsing ile çözmeye çalışmayacaktır. Bunun yerine statement parser altyapısı ayrı modeller ve helper parser sınıflarıyla hazırlanacaktır.

İlk desteklenecek statement aileleri aşağıdaki sırayla ele alınacaktır:
- Assignment statement
- CALL statement
- IF statement
- DO / END statement blokları

### Gerekçe
Parser altyapısı P04 sonunda sadeleştirilmiş ve declaration parsing helper sınıflara ayrılmıştır.

Statement parsing declaration parsing’den daha karmaşıktır. Expression parsing, block scope, statement terminator ve nested statement davranışları içerir.

Bu nedenle P05 tek büyük değişiklik olarak değil, küçük ve test edilebilir parser adımlarıyla ilerletilecektir.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.PL1.Parsing
- LegacyCodeTransformer.PL1.Syntax
- LegacyCodeTransformer.PL1.Tests
- ConversionService
- Transpiler pipeline

### Durum
Accepted

## Decision 060 - Non-Invasive PL/I Syntax Visitor ve Walker Altyapısı

### Karar
PL/I syntax model traversal işlemleri için non-invasive visitor / walker altyapısı eklenecektir.

İlk aşamada mevcut syntax node modellerine Accept methodu eklenmeyecektir. Bunun yerine Pl1SyntaxVisitor ve Pl1SyntaxWalker sınıfları, mevcut modeller üzerinde dışarıdan traversal sağlayacaktır.

### Gerekçe
Mevcut parser ve transpiler davranışını değiştirmeden traversal altyapısı oluşturmak daha düşük risklidir.

Statement parser, semantic analysis, metrics, dependency analysis ve ileride visitor tabanlı transpiler refactor çalışmaları için ortak traversal standardı gereklidir.

Bu yaklaşım:
- Mevcut syntax modellerini bozmaz.
- Parser davranışını değiştirmez.
- Transpiler refactor için güvenli zemin hazırlar.
- P05 statement modellerinin aynı traversal standardına eklenmesini kolaylaştırır.

### Etkilediği Modüller
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.PL1.Syntax
- LegacyCodeTransformer.PL1.Tests

### Durum
Accepted

## Decision 061 - P05 Statement Syntax Model Foundation

### Karar

P05 kapsamında PL/I executable statement modelleri declaration modellerinden ayrı bir syntax model hiyerarşisi altında başlatılacaktır.

İlk statement model ailesi aşağıdaki sınıflarla kurulacaktır:

- Pl1Statement
- Pl1Expression
- Pl1RawExpression
- Pl1AssignmentStatement
- Pl1CallStatement
- Pl1IfStatement
- Pl1DoStatement
- Pl1DoStatementKind
- Pl1BlockStatement

Bu modeller ilk aşamada parser davranışını değiştirmeden syntax model foundation olarak eklenecektir.

Pl1SyntaxTree geriye dönük uyum korunarak hem declaration hem statement listesi taşıyacak şekilde genişletilecektir.

Pl1SyntaxVisitor ve Pl1SyntaxWalker statement ve expression node ailelerini dolaşacak şekilde genişletilecektir.

### Gerekçe

PL/I programları yalnızca declaration satırlarından oluşmaz. Assignment, CALL, IF, DO ve block yapıları executable statement seviyesinde modellenmelidir.

Statement parser’a geçmeden önce semantic olarak ayrılmış, visitor/walker tarafından dolaşılabilen ve ileride EGL generator tarafından tüketilebilecek bir model hiyerarşisi gerekir.

Expression parser henüz detaylandırılmadığı için ilk aşamada Pl1RawExpression güvenli ara model olarak kullanılacaktır. Bu sayede statement içindeki expression metni kaybolmadan syntax tree’ye taşınabilir.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Statements
- LegacyCodeTransformer.PL1/Syntax/Pl1SyntaxTree
- LegacyCodeTransformer.PL1/Syntax/Pl1SyntaxVisitor
- LegacyCodeTransformer.PL1/Syntax/Pl1SyntaxWalker
- LegacyCodeTransformer.PL1.Tests/Syntax/Pl1SyntaxWalkerTests

### Durum

Kabul edildi.

## Decision 063 - Faz ve Milestone Tamamlanma Kriteri

### Karar

Bir roadmap fazı veya milestone'u aşağıdaki adımlar tamamlanmadan "Tamamlandı" olarak işaretlenmeyecektir.

1. Production kodu tamamlanmış olmalıdır.
2. Unit testleri tamamlanmış olmalıdır.
3. Decisions.md güncellenmiş olmalıdır.
4. ModuleSummaries.md güncellenmiş olmalıdır.
5. Gerekliyse Roadmap.md güncellenmiş olmalıdır.
6. Commit kullanıcı tarafından atılmış olmalıdır.

Milestone kapatma sırası standart olarak aşağıdaki şekilde uygulanacaktır.

    Production Code
    Unit Tests
    Decisions.md
    ModuleSummaries.md
    Roadmap.md
    Commit

Bir milestone tamamen kapanmadan sonraki milestone'a geçilmeyecektir.

Örneğin P05.3 Assignment & CALL Parser tamamlanmadan P05.4 IF / DO Parser geliştirmesine başlanmayacaktır.

### Gerekçe

Kodun tamamlanması tek başına bir geliştirme adımının tamamlandığı anlamına gelmez.

Testlerin ve dokümantasyonun eksik bırakılması teknik borç oluşturur, sonraki geliştirmeleri zorlaştırır ve yeni sohbetlerde context kaybına neden olur.

Bu standart sayesinde her milestone tek seferde tamamen kapatılır.

Ayrıca commit geçmişi, dokümantasyon ve test kapsamı aynı geliştirme sınırını temsil eder.

Bu yaklaşım projeyi yıllar sonra tekrar açtığımızda hangi commit'te neyin tamamlandığını net biçimde göstermeyi sağlar.

### Etkilediği Modüller

- Documentation/Decisions.md
- Documentation/ModuleSummaries.md
- Documentation/Roadmap.md
- Test projeleri
- Geliştirme süreci
- Commit standardı

### Durum

Kabul edildi.

## Decision 064 - Test Delivery Standardı

### Karar

Unit test önerileri aşağıdaki standartlara göre sunulacaktır.

Production kodundan farklı olarak test sınıflarında yalnızca değişiklik yapılan
testler paylaşılacaktır.

Standart aşağıdaki şekilde uygulanacaktır.

#### Yeni test ekleniyorsa

Mevcut test sınıfı değiştirilmeden yalnızca eklenecek yeni test methodları
paylaşılır.

Test methodları tam haliyle verilir.

#### Mevcut test değiştiriliyorsa

Sadece değişen test methodlarının tamamı paylaşılır.

Test sınıfının tamamı yeniden verilmez.

#### Yeni test sınıfı oluşturuluyorsa

Test sınıfının tamamı paylaşılır.

#### Production kodu değiştiriliyorsa

Bu karar production kodunu kapsamaz.

Production class, parser, transpiler, generator, helper veya model değişikliklerinde
mevcut standart korunur ve ilgili class veya methodun tamamı paylaşılır.

### Gerekçe

Production kodunda tam method veya class paylaşılması bütünlüğü korurken,
test sınıfları zaman içerisinde yüzlerce satıra ulaşabilmektedir.

Her yeni testte bütün test sınıfının tekrar paylaşılması;

- gereksiz tekrar oluşturur,
- okunabilirliği azaltır,
- cevap boyutunu gereksiz büyütür,
- geliştirme hızını düşürür.

Bunun yerine yalnızca eklenen veya değiştirilen test methodlarının paylaşılması
aynı bütünlüğü korurken çok daha verimli bir çalışma sağlar.

### Etkilediği Modüller

- Tüm Test Projeleri
- Assistant çıktı standardı
- Geliştirme süreci

### Durum

Kabul edildi.

## Decision 065 - Statement Parser Foundation Mevcut Parser Altyapısını Kullanacaktır

### Karar

P05 statement parser foundation için ayrı bir StatementParseContext veya StatementParserBase sınıfı oluşturulmayacaktır.

Statement parser modülleri mevcut parser helper altyapısını kullanacaktır.

Kullanılacak mevcut altyapı:

- ParseContext
- ParserBase
- HelperParseResult<T>
- ParserDiagnosticFactory

Statement parsing orchestration için aşağıdaki sınıflar eklenecektir.

- StatementDispatcher
- StatementParser

StatementDispatcher, mevcut token'ın hangi statement ailesine ait olduğunu belirlemekten sorumlu olacaktır.

StatementParser, executable statement parse sürecinin orchestration sınıfı olacaktır.

Concrete statement parser modülleri P05.3 ve P05.4 içinde eklenecektir.

### Gerekçe

P04 sonunda parser altyapısı zaten modüler helper parser mimarisine taşınmıştır.

Yeni ve paralel bir StatementParseContext veya StatementParserBase oluşturmak gereksiz soyutlama üretir ve parser altyapısında iki farklı standart oluşmasına neden olur.

Mevcut ParseContext ve ParserBase sınıfları declaration parser için yeterli olduğu gibi statement parser için de yeterlidir.

Bu karar sayesinde parser altyapısı tek standartta kalır.

StatementParser yalnızca orchestration yapar. Gerçek parse davranışları ileride aşağıdaki concrete parser sınıflarına ayrılacaktır.

- AssignmentStatementParser
- CallStatementParser
- IfStatementParser
- DoStatementParser

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Parsing/Helpers
- LegacyCodeTransformer.PL1/Parsing/Pl1Parser
- LegacyCodeTransformer.PL1.Tests/Parsing/Helpers
- LegacyCodeTransformer.PL1.Tests/Parsing/Pl1ParserTests

### Durum

Kabul edildi.

## Decision 066 - Parser Helper Visibility ve Behavior Test Standardı

### Karar

Parser helper, dispatcher ve internal orchestration sınıfları mümkün olduğunca implementation detail olarak tutulacaktır.

Yeni parser/helper sınıfları için önce aşağıdaki sorular değerlendirilecektir:

1. Bu sınıf gerçekten dışarıdan görünmeli mi?
2. Bu sınıf implementation detayı mı?
3. Bu davranış test edilmeli mi, yoksa implementation mı test ediliyor?
4. Bu sınıf gelecekte başka parser'lar tarafından tekrar kullanılacak mı?

Internal enum, helper veya dispatcher kararları public test method signature içinde kullanılmayacaktır.

Testlerde implementation detail yerine observable behavior doğrulanacaktır.

### Gerekçe

Parser altyapısı büyüdükçe gereksiz public/internal detayların test API'sine sızması bakım maliyeti oluşturur.

GetParserKind gibi dispatcher iç kararları doğrudan test etmek yerine, bu kararın dışarıdan görülen etkisi test edilmelidir.

Örneğin Assignment, CALL, IF ve DO routing davranışı StatementParserTests üzerinden doğrulanmalıdır.

Böylece testler implementation detayına değil gerçek davranışa bağlı kalır.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Parsing/Helpers
- LegacyCodeTransformer.PL1.Tests/Parsing/Helpers
- Parser geliştirme standardı
- Test yazım standardı

### Durum

Kabul edildi.

## Decision 067 - P05 Assignment ve CALL Parser Foundation

### Karar

P05.3 kapsamında Assignment ve CALL statement parsing desteği eklenecektir.

Assignment statement parsing için:

- AssignmentStatementParser
- AssignmentRawExpressionBuilder
- ExpressionFactory
- DelimitedTokenReader
- StatementRecoveryHelper

kullanılacaktır.

CALL statement parsing için:

- CallStatementParser
- ExpressionFactory
- DelimitedTokenReader
- StatementRecoveryHelper

kullanılacaktır.

StatementParser, concrete parser seçiminde StatementDispatcher ve StatementParserKind üzerinden ilerleyecektir.

Assignment ve CALL parser ilk aşamada tam expression tree üretmeyecektir. Expression tarafı Pl1RawExpression olarak korunacaktır.

### Gerekçe

PL/I executable statement desteğinin ilk güvenli alt kümesi Assignment ve CALL statement türleridir.

Assignment parser aşağıdaki örnekleri destekler:

    PARAM = 'ABC';
    SQLCODE = 0;
    DCLGLAU.BRM_KOD = 888;

CALL parser aşağıdaki örnekleri destekler:

    CALL FETCH_CURSOR;
    CALL PROC1(A, 'ABC', B);
    CALL SQL_HATA_OLUSTUR('SELECT GLAU_HISTORY');

Expression parser henüz detaylandırılmadığı için expression içeriğini kaybetmeden Pl1RawExpression olarak taşımak en güvenli yaklaşımdır.

Parser utility refactor ile delimiter okuma, expression üretimi ve statement recovery davranışları tekrar eden parser kodlarından ayrılmıştır.

Bu yapı P05.4 IF / DO parser geliştirmeleri için de ortak foundation sağlar.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Parsing/Helpers/AssignmentStatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/CallStatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/AssignmentRawExpressionBuilder
- LegacyCodeTransformer.PL1/Parsing/Helpers/ExpressionFactory
- LegacyCodeTransformer.PL1/Parsing/Helpers/DelimitedTokenReader
- LegacyCodeTransformer.PL1/Parsing/Helpers/StatementRecoveryHelper
- LegacyCodeTransformer.PL1/Parsing/Helpers/StatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/StatementDispatcher
- LegacyCodeTransformer.PL1.Tests/Parsing/Helpers
- LegacyCodeTransformer.PL1.Tests/Parsing/Pl1ParserTests

### Durum

Kabul edildi.

## Decision 068 - P05 IF ve DO Parser Foundation

### Karar

P05.4 kapsamında IF / THEN / ELSE ve DO / DO WHILE / DO UNTIL statement parsing desteği eklenecektir.

IF parser ilk aşamada aşağıdaki yapıları destekleyecektir:

- IF condition THEN assignment;
- IF condition THEN CALL;
- IF condition THEN statement; ELSE statement;
- IF condition THEN DO; ... END;

DO parser ilk aşamada aşağıdaki yapıları destekleyecektir:

- DO; ... END;
- DO WHILE(condition); ... END;
- DO UNTIL(condition); ... END;

IF condition ve DO condition alanları P05.4 aşamasında Pl1RawExpression olarak taşınacaktır.

IF THEN ve ELSE kolları Pl1Statement olarak modellenecektir.

DO body, Pl1BlockStatement olarak modellenecektir.

### Gerekçe

Assignment ve CALL statement desteğinden sonra PL/I executable statement parsing için ilk control-flow yapıları IF ve DO statement türleridir.

Bu yapıların parser seviyesinde desteklenmesi, ileride EGL if/else ve loop/block output üretimi için temel oluşturur.

IF ve DO parser'ları recursive statement parsing gerektirir. Bu nedenle child statement parsing işlemi yine StatementParser üzerinden yürütülecektir.

Expression parser henüz tamamlanmadığı için condition expression içerikleri Pl1RawExpression olarak korunacaktır.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Parsing/Helpers/IfStatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/DoStatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/StatementParser
- LegacyCodeTransformer.PL1/Statements/Pl1IfStatement
- LegacyCodeTransformer.PL1/Statements/Pl1DoStatement
- LegacyCodeTransformer.PL1/Statements/Pl1BlockStatement
- LegacyCodeTransformer.PL1.Tests/Parsing/Helpers/StatementParserTests
- LegacyCodeTransformer.PL1.Tests/Parsing/Pl1ParserTests

### Durum

Kabul edildi.

## Decision 069 - Minimal Extensible Architecture Standard

### Karar

Proje mimarisi eklenebilir ve sürdürülebilir olacak şekilde tasarlanacaktır; ancak spekülatif veya erken soyutlama yapılmayacaktır.

Yeni abstraction, helper, factory veya dispatcher yalnızca aşağıdaki şartlardan en az biri sağlandığında eklenecektir:

1. Aynı davranış en az iki farklı production yerde tekrar etmeye başlamışsa.
2. Yeni özellik eklenirken mevcut sınıfın sorumluluğu belirgin şekilde bozuluyorsa.
3. Yakın roadmap milestone'unda doğrudan kullanılacak net bir ihtiyaç varsa.
4. Test edilebilirliği veya hata recovery davranışını anlamlı şekilde iyileştiriyorsa.

Aşağıdaki gerekçeler tek başına yeni abstraction eklemek için yeterli değildir:

- İleride belki lazım olur.
- Daha soyut görünür.
- Design pattern kullanmış oluruz.
- Her şeyi şimdiden genişletilebilir yapalım.

### Gerekçe

LegacyCodeTransformer uzun vadede büyüyecek bir dönüşüm platformudur. Bu nedenle mimarinin eklenebilir olması gerekir.

Ancak erken soyutlama ve gereksiz helper/factory katmanları kodun okunabilirliğini azaltır, geliştirme hızını düşürür ve bakım maliyetini artırır.

Bu yüzden proje, gerçek ihtiyaç çıktıkça genişleyen fakat gereksiz genelleştirme yapmayan bir mimari disiplinle ilerleyecektir.

### Etkilediği Modüller

- Tüm production kodları
- Parser helper mimarisi
- Transpiler helper mimarisi
- Generator helper mimarisi
- Test yazım standardı
- Geliştirme süreci

### Durum

Kabul edildi.

## Decision 070 - P05 Statement Integration ve Recursive Control Flow Test Standardı

### Karar

P05.5 kapsamında statement parser entegrasyonu, mixed statement parsing ve nested control-flow senaryoları testlerle güvence altına alınacaktır.

Bu milestone’da yeni production abstraction açılmayacaktır.

Odak noktası mevcut parser zincirinin aşağıdaki statement türlerini aynı syntax tree içinde doğru sırayla ve doğru hiyerarşiyle taşıdığını doğrulamaktır:

- Declaration
- Assignment
- CALL
- IF
- DO
- IF THEN DO
- IF THEN DO ELSE DO
- Nested DO
- DO WHILE
- DO UNTIL

### Gerekçe

Assignment, CALL, IF ve DO parser’ları ayrı ayrı çalışsa bile gerçek PL/I kaynaklarında bu statement türleri birlikte ve nested şekilde bulunur.

P05.5’in amacı yeni parser türü eklemek değil, mevcut statement parser altyapısının integration seviyesinde güvenilir olduğunu kanıtlamaktır.

Bu yaklaşım over engineering riskini azaltır ve mevcut davranışı testlerle sağlamlaştırır.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Parsing/Helpers/StatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/IfStatementParser
- LegacyCodeTransformer.PL1/Parsing/Helpers/DoStatementParser
- LegacyCodeTransformer.PL1.Tests/Parsing/Helpers/StatementParserTests
- LegacyCodeTransformer.PL1.Tests/Parsing/Pl1ParserTests

### Durum

Kabul edildi.

## Decision 071 - P05 Statement Pipeline Olarak Tamamlanacaktır

### Karar

P05 yalnızca PL/I executable statement parser fazı olarak kapatılmayacaktır.

P05, statement parser çıktılarının transpiler ve EGL generator katmanına kadar doğrulandığı bir Statement Pipeline fazı olarak tamamlanacaktır.

Bu nedenle P06 Procedure Desteği fazına geçmeden önce P05 aşağıdaki ek milestone’larla genişletilecektir.

- P05.6 — Statement Visitor / Walker Integration Verification
- P05.7 — Statement Transpiler Foundation
- P05.8 — Assignment EGL Generation
- P05.9 — CALL EGL Generation
- P05.10 — IF EGL Generation
- P05.11 — DO EGL Generation
- P05.12 — Statement End-to-End Tests

P05 tamamlandığında parser, transpiler ve EGL generator katmanları statement desteği için uçtan uca çalışır durumda olacaktır.

### Gerekçe

Parser desteğinin tek başına tamamlanması dönüşüm hattının tamamlandığı anlamına gelmez.

PL/I statement modelleri syntax tree üzerinde üretildikten sonra bu modellerin transpiler ve EGL output katmanında da kullanılabilir olduğu doğrulanmalıdır.

Parser bitiminden hemen sonra transpiler/generator katmanına geçmek, parser modelindeki eksikleri erken ortaya çıkarır.

Bu yaklaşım P06 Procedure Desteği’ne geçmeden önce statement pipeline’ın sağlamlaşmasını sağlar.

Böylece procedure parser eklendiğinde procedure body içinde bulunan executable statement’lar zaten hazır olan statement pipeline üzerinden işlenebilir.

### Etkilediği Modüller

- Documentation/Roadmap.md
- LegacyCodeTransformer.PL1
- LegacyCodeTransformer.Transpilers
- LegacyCodeTransformer.Egl
- LegacyCodeTransformer.Application
- Tüm ilgili test projeleri

### Durum

Kabul edildi.

## Decision 072 - Refactor Öncesi Mevcut Yapı Doğrulanacaktır

### Karar

Production refactor önerileri mevcut kod tabanı doğrulanmadan varsayımsal olarak önerilmeyecektir.

Bir refactor önerisi aşağıdaki kurallara uymalıdır:

- Mevcut sınıf gerçekten analiz edilmiş olmalıdır.
- Yeni helper method öneriliyorsa mevcut implementasyonda gerçekten tekrar eden kod olduğu doğrulanmalıdır.
- "Şöyle olabilir", "muhtemelen vardır", "eklenebilir" yaklaşımıyla production refactor önerilmeyecektir.
- Eğer bir sınıf değişecekse değişecek sınıfın tamamı verilecektir.
- Eğer yalnızca yeni test ekleniyorsa sadece yeni test methodları verilecektir.

### Gerekçe

LegacyCodeTransformer uzun ömürlü bir projedir.

Varsayımsal refactor önerileri gereksiz kod üretimine ve mevcut mimariden sapmalara neden olabilir.

Bu nedenle tüm production refactor kararları mevcut kod tabanı doğrulanarak alınacaktır.

### Etkilediği Modüller

- Tüm production kodları
- Refactor süreci
- Kod inceleme süreci

### Durum

Kabul edildi.

## Decision 072 - Refactor Öncesi Mevcut Yapı Doğrulanacaktır

### Karar

Production refactor önerileri mevcut kod tabanı doğrulanmadan varsayımsal olarak önerilmeyecektir.

Bir refactor önerisi aşağıdaki kurallara uymalıdır:

- Mevcut sınıf gerçekten analiz edilmiş olmalıdır.
- Yeni helper method öneriliyorsa mevcut implementasyonda gerçekten tekrar eden kod olduğu doğrulanmalıdır.
- "Şöyle olabilir", "muhtemelen vardır", "eklenebilir" yaklaşımıyla production refactor önerilmeyecektir.
- Eğer bir sınıf değişecekse değişecek sınıfın tamamı verilecektir.
- Eğer yalnızca yeni test ekleniyorsa sadece yeni test methodları verilecektir.

### Gerekçe

LegacyCodeTransformer uzun ömürlü bir projedir.

Varsayımsal refactor önerileri gereksiz kod üretimine ve mevcut mimariden sapmalara neden olabilir.

Bu nedenle tüm production refactor kararları mevcut kod tabanı doğrulanarak alınacaktır.

### Etkilediği Modüller

- Tüm production kodları
- Refactor süreci
- Kod inceleme süreci
- Assistant çıktı standardı

### Durum

Kabul edildi.

## Decision 073 - P05.6 Statement Visitor Walker Verification

### Karar

P05.6 kapsamında yeni production abstraction eklenmeyecektir.

Mevcut Pl1SyntaxVisitor ve Pl1SyntaxWalker altyapısının Assignment, CALL, IF, DO, Block ve RawExpression modellerini eksiksiz dolaştığı testlerle doğrulanacaktır.

Production refactor yalnızca mevcut traversal davranışında eksiklik veya gerçek tekrar tespit edilirse yapılacaktır.

Mevcut doğrulama sonucunda production refactor gerekmemektedir.

### Gerekçe

Statement transpiler foundation öncesinde visitor/walker altyapısının statement node ailesini doğru dolaştığı garanti altına alınmalıdır.

Ancak mevcut Pl1SyntaxVisitor ve Pl1SyntaxWalker yapısı statement dispatch ve recursive traversal için yeterlidir.

Bu nedenle yeni VisitorBase, StatementWalker veya ekstra abstraction eklemek over engineering olacaktır.

### Etkilediği Modüller

- LegacyCodeTransformer.PL1/Syntax/Pl1SyntaxVisitor
- LegacyCodeTransformer.PL1/Syntax/Pl1SyntaxWalker
- LegacyCodeTransformer.PL1.Tests/Syntax/Pl1SyntaxWalkerTests

### Durum

Kabul edildi.

## Decision 074 - Transpiler Katmanı EGL Syntax Modeli Üretecektir

### Karar

LegacyCodeTransformer içerisinde Transpiler katmanı hiçbir zaman doğrudan EGL kaynak kodu (string) üretmeyecektir.

Transpiler katmanının tek sorumluluğu PL/I syntax modellerini EGL syntax modellerine dönüştürmektir.

EGL kaynak kodunun üretilmesi yalnızca Generator katmanının sorumluluğunda olacaktır.

Statement pipeline aşağıdaki katmanlardan oluşacaktır.

    PL/I Source
        ↓
    PL/I Lexer
        ↓
    PL/I Parser
        ↓
    PL/I Syntax Tree
        ↓
    Transpiler
        ↓
    EGL Syntax Tree
        ↓
    EGL Generator
        ↓
    EGL Source Code

Bu mimari hem declaration hem de executable statement dönüşümleri için ortak standart olacaktır.

P05.7 ile başlayan statement transpiler çalışmaları ve ileride eklenecek procedure, expression ve program transpiler bileşenleri de aynı pipeline'ı kullanacaktır.

### Gerekçe

Parser, Transpiler ve Generator katmanlarının sorumlulukları birbirinden açık şekilde ayrılmalıdır.

Eğer Transpiler doğrudan string üretirse;

- dönüşüm mantısı ile formatlama davranışı aynı sınıfta toplanır,
- unit testler gereksiz şekilde string karşılaştırmalarına bağımlı hale gelir,
- EGL formatlama kurallarındaki değişiklikler transpiler kodunu etkiler,
- gelecekte yapılacak optimizasyon ve refactoring çalışmaları zorlaşır.

EGL Syntax Tree'nin ara model olarak kullanılması ise;

- dönüşüm mantısını formatlama mantısından tamamen ayırır,
- güçlü tipli (strongly typed) EGL modelleri üzerinden çalışılmasını sağlar,
- Generator katmanının tek sorumluluğunun kod üretmek olmasını sağlar,
- farklı generator stratejilerinin eklenmesini kolaylaştırır,
- syntax analizi, semantic analiz, optimizasyon ve code formatting gibi ileri seviye geliştirmelere altyapı hazırlar.

Bu nedenle Transpiler katmanı yalnızca EGL syntax modeli üretmeli, kaynak kod üretimi ise tamamen Generator katmanına bırakılmalıdır.

### Etkilediği Modüller

- LegacyCodeTransformer.Transpilers
- LegacyCodeTransformer.Egl
- LegacyCodeTransformer.Generators
- Declaration Transpiler
- Statement Transpiler
- Procedure Transpiler
- Expression Transpiler
- EGL Code Generator
- Roadmap.md
- Geliştirme standartları

### Durum

Kabul edildi.

## Decision 075 - Statement Transpiler Foundation

### Karar

P05.7 kapsamında statement transpiler foundation eklenecektir.

Transpiler katmanı PL/I statement modellerini doğrudan string'e çevirmeyecek, EGL statement syntax modelleri üretecektir.

Bu milestone’da concrete assignment, CALL, IF veya DO EGL mapping yapılmayacaktır.

İlk foundation bileşenleri:

- EglStatement
- EglSyntaxTree.Statements
- StatementTranspiler
- Pl1ToEglTranspiler statement routing entegrasyonu

### Gerekçe

P05 parser tarafında Assignment, CALL, IF ve DO statement modelleri üretilmiştir.

Bu modellerin dönüşüm pipeline içinde kullanılabilmesi için EGL syntax tree’nin statement listesi taşıması ve transpiler’ın PL/I statement listesini işlemeye başlaması gerekir.

Concrete mapping henüz eklenmediği için statement türleri diagnostic üretir.

Bu davranış P05.8 ve sonrası için güvenli foundation sağlar.

### Etkilediği Modüller

- LegacyCodeTransformer.Egl/Statements
- LegacyCodeTransformer.Egl/Syntax/EglSyntaxTree
- LegacyCodeTransformer.Transpilers/Pl1ToEgl
- LegacyCodeTransformer.Transpilers.Tests/Pl1ToEgl
- LegacyCodeTransformer.Egl.Tests/Syntax

### Durum

Kabul edildi.

## Decision 076 - Assignment Statement EGL Generation

### Karar

P05.8 kapsamında PL/I assignment statement modelleri EGL assignment statement modellerine dönüştürülecektir.

Transpiler doğrudan EGL string üretmeyecektir.

Assignment dönüşüm zinciri aşağıdaki gibi olacaktır.

    Pl1AssignmentStatement
        ↓
    EglAssignmentStatement
        ↓
    EglCodeGenerator
        ↓
    EGL assignment source line

Expression alanları P05.8 aşamasında raw expression fallback modeliyle taşınacaktır.

Kullanılan EGL expression modelleri:

- EglExpression
- EglRawExpression

Kullanılan EGL statement modeli:

- EglAssignmentStatement

### Gerekçe

P05.7 ile statement transpiler foundation kurulmuştur ancak concrete statement mapping yapılmamıştır.

P05.8 ile statement pipeline’ın ilk gerçek uçtan uca dönüşümü assignment statement üzerinden tamamlanır.

Bu yaklaşım parser, transpiler ve generator katmanları arasındaki sorumluluk ayrımını korur.

Desteklenen örnekler:

    PARAM = 'ABC';
    CUSTOMER_NO = MUST_NO;

EGL output örnekleri:

    Param = "ABC";
    CustomerNo = MustNo;

### Etkilediği Modüller

- LegacyCodeTransformer.Egl/Expressions
- LegacyCodeTransformer.Egl/Statements
- LegacyCodeTransformer.Egl/Generation/EglCodeGenerator
- LegacyCodeTransformer.Transpilers/Pl1ToEgl/StatementTranspiler
- LegacyCodeTransformer.Transpilers/Pl1ToEgl/ExpressionTranspiler
- LegacyCodeTransformer.Transpilers/Pl1ToEgl/EglRawExpressionTextTransformer
- LegacyCodeTransformer.Transpilers.Tests/Pl1ToEgl
- LegacyCodeTransformer.Egl.Tests/Generation

### Durum

Kabul edildi.

## Decision 077 - Tek Kullanım Noktalı Abstraction Ertelenecektir

### Karar

Yeni abstraction, helper, factory, mapper, transformer veya base model yalnızca mevcut milestone içinde gerçek ve somut bir ihtiyaç varsa eklenecektir.

Genel kural olarak bir abstraction aşağıdaki şartlardan en az birini sağlamalıdır:

1. Mevcut milestone içinde en az iki farklı production kullanım noktasına sahip olmalıdır.
2. Aynı davranış iki veya daha fazla production sınıfta tekrar etmeye başlamış olmalıdır.
3. Mevcut sınıfın sorumluluğu belirgin şekilde bozuluyorsa bu sorumluluğu ayırmalıdır.
4. Test edilebilirlik, diagnostic üretimi veya recovery davranışı açısından net fayda sağlamalıdır.

Tek kullanım noktası olan abstraction'lar, gerçek ikinci kullanım ihtiyacı oluşana kadar eklenmeyecektir.

Aşağıdaki gerekçeler tek başına yeni abstraction eklemek için yeterli değildir:

- İleride lazım olabilir.
- Daha temiz görünür.
- Design pattern uygulanmış olur.
- Gelecekte expression parser gelince kullanılır.
- Şimdiden genişletilebilir olsun.

### Gerekçe

LegacyCodeTransformer uzun vadeli ve büyüyen bir dönüşüm projesidir.

Mimari eklenebilir olmalıdır; ancak erken abstraction projeyi gereksiz büyütür, okunabilirliği azaltır ve bakım maliyetini artırır.

Özellikle parser, transpiler ve generator katmanlarında henüz tek kullanım noktası olan abstraction'lar ileride gerçek ihtiyaç şekillenmeden eklenirse yanlış yönde sabitlenmiş mimari kararlara dönüşebilir.

Bu nedenle abstraction kararları mevcut milestone ihtiyacına göre verilecek, gelecekteki belirsiz ihtiyaçlar için erken modelleme yapılmayacaktır.

### Etkilediği Modüller

- Tüm production kodları
- Parser helper mimarisi
- Transpiler helper mimarisi
- Generator helper mimarisi
- EGL syntax model tasarımı
- Test yazım standardı
- Geliştirme süreci

### Durum

Kabul edildi.