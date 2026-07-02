# LegacyCodeTransformer

> Legacy programlama dillerini modern dillere dönüştürmek için geliştirilen modüler ve genişletilebilir bir kaynak kod dönüşüm framework'ü.

## Genel Bakış

LegacyCodeTransformer, eski (legacy) yazılım dillerinde geliştirilmiş uygulamaları analiz ederek modern programlama dillerine dönüştürmeyi amaçlayan bir dönüşüm altyapısıdır.

İlk hedefi;

* **Kaynak Dil:** IBM PL/I
* **Hedef Dil:** IBM EGL

olmakla birlikte, proje mimarisi ileride farklı kaynak ve hedef dilleri destekleyebilecek şekilde tasarlanmıştır.

Proje, modern derleyici (compiler) mimarilerinden ilham alınarak geliştirilmektedir ve aşağıdaki temel aşamalardan oluşmaktadır:

* Lexical Analysis (Lexer)
* Parsing
* Abstract Syntax Tree (AST)
* Semantic Analysis
* Transformation Engine
* Code Generation

---

# Projenin Amacı

LegacyCodeTransformer'ın temel hedefleri şunlardır:

* Legacy uygulamaların iş kurallarını koruyarak modern dillere dönüştürülmesini sağlamak
* Üretilen kodun okunabilir ve sürdürülebilir olmasını sağlamak
* Modüler ve genişletilebilir bir mimari sunmak
* Yeni kaynak ve hedef dillerin kolayca eklenebilmesini sağlamak
* Her dönüşüm adımının bağımsız olarak test edilebilmesini sağlamak

---

# Mimari

```
Legacy Kaynak Kod
        │
        ▼
      Lexer
        │
        ▼
      Parser
        │
        ▼
        AST
        │
        ▼
 Semantic Analysis
        │
        ▼
 Intermediate Model
        │
        ▼
 Transformation Engine
        │
        ▼
 Code Generator
        │
        ▼
 Modern Kaynak Kod
```

Her katman yalnızca kendi sorumluluğundan sorumludur ve diğer katmanlardan bağımsız çalışabilecek şekilde tasarlanmıştır.

---

# Mevcut Özellikler

* PL/I Lexer altyapısı
* Recursive Descent Parser mimarisi
* Güçlü tiplenmiş AST modeli
* Visitor Pattern desteği
* Modüler dönüşüm altyapısı
* Genişletilebilir dil soyutlama katmanı
* Kod üretim (Code Generation) altyapısı
* Unit Test desteği

---

# Proje Yapısı

```
src/

    Core/
        Common
        AST
        Interfaces
        Models

    Parser/
        Lexer
        Parser
        Grammar

    Transformer/
        Visitors
        Transformers
        Mapping

    Generator/
        EGL

    Tests/
```

Katmanlı yapı sayesinde parser, semantic analiz, dönüşüm ve kod üretim süreçleri birbirinden ayrılmıştır. Böylece bakım kolaylığı ve genişletilebilirlik sağlanmaktadır.

---

# Tasarım Prensipleri

Proje geliştirilirken aşağıdaki prensipler esas alınmaktadır:

* SOLID
* Clean Architecture
* Separation of Concerns
* Dependency Injection
* Visitor Pattern
* Strategy Pattern
* Factory Pattern (gerektiğinde)

---

# Yol Haritası

## Faz 1

* [x] Proje mimarisi
* [x] Dokümantasyon
* [x] Yol haritası
* [ ] Tokenizer
* [ ] Lexer
* [ ] Parser

## Faz 2

* [ ] AST oluşturulması
* [ ] Semantic Analysis
* [ ] Doğrulama (Validation)
* [ ] Intermediate Model

## Faz 3

* [ ] PL/I ifadelerinin dönüştürülmesi
* [ ] Expression dönüşümleri
* [ ] Procedure dönüşümleri

## Faz 4

* [ ] EGL kod üretimi
* [ ] Kod biçimlendirme (Formatting)
* [ ] Üretilen kodun doğrulanması

## Gelecek Planları

* COBOL desteği
* RPG desteği
* Java kod üretimi
* C# kod üretimi
* Plugin mimarisi
* CLI uygulaması
* Visual Studio Extension

---

# Kullanılan Teknolojiler

* .NET 9
* C#
* xUnit
* FluentAssertions
* GitHub Actions *(planlanıyor)*

---

# Neden Bu Proje?

Kurumsal yazılımların önemli bir kısmı hâlâ PL/I, COBOL ve RPG gibi legacy teknolojiler üzerinde çalışmaktadır.

Bu uygulamaların manuel olarak modern dillere dönüştürülmesi;

* yüksek maliyet,
* uzun geliştirme süresi,
* insan kaynaklı hata riski

gibi önemli problemler oluşturmaktadır.

LegacyCodeTransformer, bu süreci mümkün olduğunca otomatikleştirerek hem dönüşüm maliyetini azaltmayı hem de üretilen kodun geliştirilebilir olmasını hedeflemektedir.

---

# Katkıda Bulunma

Katkılar memnuniyetle karşılanmaktadır.

Katkı sağlayabileceğiniz alanlardan bazıları:

* Parser geliştirmeleri
* Yeni grammar kuralları
* Dönüştürücü (Transformer) modülleri
* Kod üreticileri (Code Generators)
* Dokümantasyon
* Unit Testler
* Performans iyileştirmeleri

Büyük değişiklikler planlıyorsanız öncelikle bir Issue oluşturmanız önerilir.

---

# Lisans

Bu proje **MIT License** altında lisanslanacaktır.

---

# Proje Durumu

🚧 **Aktif Geliştirme Aşamasında**

Projenin temel mimarisi oluşturulmuş olup parser ve dönüşüm motoru üzerinde geliştirme çalışmaları devam etmektedir.

---

# Uzun Vadeli Vizyon

LegacyCodeTransformer'ın uzun vadeli hedefi;

farklı legacy programlama dillerini destekleyen, farklı modern dillere dönüşüm yapabilen, modüler ve yeniden kullanılabilir bir **Kaynak Kod Modernizasyon Platformu (Source Code Modernization Platform)** oluşturmaktır.
