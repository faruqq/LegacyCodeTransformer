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

```pli
DCL MUST_NO FIXED DECIMAL(8);

DCL CUSTOMER_NO FIXED DECIMAL(10,2);
```

↓

EGL

```egl
mustNo decimal(8);

customerNo decimal(10,2);
```

## Sonraki Faz

P04 — PL/I Veri Tiplerini Genişletme

---

# P04 — PL/I Veri Tiplerini Genişletme

## Durum
Aktif Olarak Geliştiriliyor

## Amaç
PL/I veri tipi, declaration ve structure desteğini gerçek projelerde kullanılabilecek seviyeye çıkarmak.

## Tamamlananlar
- ✅ CHAR / CHARACTER veri tipi desteği
- ✅ VARCHAR → EGL char dönüşümü
- ✅ INIT / INITIAL parse desteği
- ✅ Identifier naming strategy desteği
- ✅ PL/I basic structure declaration parse desteği
- ✅ PL/I structure declaration → EGL record dönüşümü
- ✅ PL/I structure array → EGL basicRecord parent array field dönüşümü
- ✅ PL/I structure member array → EGL field array dönüşümü
- ✅ PL/I nested structure → EGL parent group field dönüşümü
- ✅ Recursive nested structure mapping desteği
- ✅ EGL output casing ve indentation standardı
- ✅ Decimal scale bilgisinin nullable korunması
- ✅ FIXED DECIMAL / DEC FIXED / DECIMAL FIXED synonym desteği
- ✅ FIXED BIN / BIN FIXED numeric mapping desteği
- ✅ smallint / int casing standardı
- ✅ PIC / PICTURE ayrı model parse desteği
- ✅ Güvenli numeric PIC subset → EGL num mapping desteği
- ✅ Alphanumeric PIC subset → EGL char mapping desteği
- ✅ Signed PIC classification desteği
- ✅ Formatted PIC örnekleri için diagnostic üretimi

## Aktif Alt Hedef
- DIM / DIMENSION syntax desteği

## Sıradaki Alt Hedefler
- sqlRecord mapping desteği
- INIT değerlerinin EGL default value olarak üretilmesi

## Başarı Kriteri
Parser, Transpiler ve Generator katmanlarının yeni veri tiplerini ve declaration yapılarını uçtan uca desteklemesi.

## Sonraki Faz
P05 — PL/I Statement Desteği

---

# P05 — PL/I Statement Desteği

## Durum

⏳ Planlandı

## Amaç

PL/I içerisindeki temel işlem ifadelerini desteklemek.

## İlk Hedefler

* Assignment
* IF
* THEN
* ELSE
* DO
* END
* CALL

## Sonraki Faz

P06 — Procedure Desteği

---

# P06 — Procedure Desteği

## Durum

⏳ Planlandı

## Amaç

Procedure seviyesinde dönüşüm gerçekleştirebilmek.

## İlk Hedefler

* PROCEDURE
* Parametreler
* RETURN
* Local Variable Scope

## Sonraki Faz

P07 — Legacy PL/I Yapıları

---

# P07 — Legacy PL/I Yapıları

## Durum

⏳ Planlandı

## Amaç

Kurumsal PL/I projelerinde sık kullanılan yapıları desteklemek.

## İlk Hedefler

* INCLUDE
* Compiler Directives
* Embedded SQL
* CICS Çağrıları
* Makrolar

## Sonraki Faz

P08 — Dönüşüm Kalitesini Artırma

---

# P08 — Dönüşüm Kalitesini Artırma

## Durum

⏳ Planlandı

## Amaç

Üretilen kodun doğruluğunu ve okunabilirliğini artırmak.

## İlk Hedefler

* Gelişmiş Normalizer
* Kod formatlama
* Unsupported Syntax raporları
* Daha gelişmiş Diagnostic sistemi
* İsimlendirme kuralları

## Sonraki Faz

P09 — IDE Entegrasyonu

---

# P09 — IDE Entegrasyonu

## Durum

⏳ Planlandı

## Amaç

Dönüştürme aracını IDE içerisinden kullanılabilir hale getirmek.

## İlk Hedefler

* Eclipse tabanlı eklenti
* IBM RBD entegrasyonu
* Tek dosya dönüşümü
* Çoklu dosya dönüşümü

## Sonraki Faz

P10 — Yeni Hedef Diller

---

# P10 — Yeni Hedef Diller

## Durum

⏳ Planlandı

## Amaç

Mevcut parser altyapısını kullanarak farklı hedef dillere dönüşüm yapabilmek.

## İlk Hedefler

* PL/I → C#
* EGL → C#
* PL/I → Java

## Sonraki Faz

P11 — Kurumsal Özellikler

---

# P11 — Kurumsal Özellikler

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

## Sonraki Faz

Projenin ihtiyaçlarına göre yeni fazlar eklenecektir.

---

# Uzun Vadeli Hedef

LegacyCodeTransformer'ın yalnızca **PL/I → EGL** dönüşümü yapan bir araç olması hedeflenmemektedir.

Uzun vadeli hedef;

farklı legacy dilleri modern dillere dönüştürebilen,

* genişletilebilir,
* sürdürülebilir,
* test edilebilir,
* kurumsal ölçekte kullanılabilir

bir **Legacy Code Transformation Platformu** oluşturmaktır.
