# Architecture.md

# LegacyCodeTransformer Mimarisi

## Projenin Amacı

LegacyCodeTransformer, eski (legacy) yazılım dillerini modern dillere dönüştürmek amacıyla geliştirilen, uzun ömürlü ve genişletilebilir bir dönüşüm platformudur.

İlk somut hedef:

```text
PL/I → EGL
```

Ancak mimari yalnızca bu dönüşüm için tasarlanmamıştır.

İlerleyen aşamalarda aşağıdaki dönüşümler de aynı altyapı kullanılarak geliştirilebilir.

* PL/I → C#
* EGL → C#
* COBOL → C#
* Diğer legacy diller → Modern diller

Bu nedenle proje, belirli bir dönüşüme değil, dönüşüm altyapısına odaklanmaktadır.

---

# Temel Mimari Yaklaşım

LegacyCodeTransformer bir **metin dönüştürme (text replacement)** aracı değildir.

Regex veya String.Replace tabanlı dönüşümler yerine Compiler / Transpiler mimarisi kullanılacaktır.

Genel dönüşüm hattı aşağıdaki gibidir.

```text
Kaynak Kod
    │
    ▼
Lexer
    │
    ▼
Parser
    │
    ▼
Kaynak Syntax Tree
    │
    ▼
Normalizer
    │
    ▼
Hedef Syntax Tree
    │
    ▼
Code Generator
    │
    ▼
Hedef Kaynak Kod
```

---

# İlk Sürüm Mimarisi

İlk sürümde gerçekleştirilecek dönüşüm aşağıdaki pipeline üzerinden ilerleyecektir.

```text
PL/I Source
    │
    ▼
PL/I Lexer
    │
    ▼
PL/I Parser
    │
    ▼
Pl1SyntaxTree
    │
    ▼
PL/I Normalizer
    │
    ▼
Pl1SyntaxTree
    │
    ▼
PL/I → EGL Transpiler
    │
    ▼
EglSyntaxTree
    │
    ▼
EglCodeGenerator
    │
    ▼
EGL Source
```

Bu mimari sayesinde her katman yalnızca kendi sorumluluğunu yerine getirir.

---

# Katman Sorumlulukları

## Lexer

Kaynak kodu karakter karakter okuyarak anlamlı token'lara ayırır.

Örnek:

```pli
DCL MUST_NO FIXED DECIMAL(8);
```

↓

```text
DCL
MUST_NO
FIXED
DECIMAL
(
8
)
;
EOF
```

Lexer yalnızca token üretir.

Programın anlamını çözmez.

---

## Parser

Lexer tarafından üretilen token listesini okuyarak kaynak dile ait Syntax Tree modelini oluşturur.

Örneğin:

```text
Token Listesi
        │
        ▼
Pl1SyntaxTree
```

Parser hedef dili bilmez.

---

## Syntax Tree

Syntax Tree, kaynak kodun nesne modelidir.

Örneğin;

```pli
DCL MUST_NO FIXED DECIMAL(8);
```

şu modele dönüşebilir.

```text
Pl1VariableDeclaration
│
├── Name
└── DataType
```

Parser bu modeli üretir.

Transpiler bu modeli kullanır.

---

## Normalizer

Normalizer, parser tarafından oluşturulan Syntax Tree modelini daha standart hale getirir.

İlk sürümde minimal tutulacaktır.

İlerleyen sürümlerde;

* Yazım farklılıklarının giderilmesi
* Varsayılan değerlerin belirlenmesi
* Gereksiz syntax farklılıklarının sadeleştirilmesi

gibi işlemler burada yapılacaktır.

---

## Transpiler

Transpiler, kaynak dil modelini hedef dil modeline dönüştürür.

Örneğin;

```text
Pl1SyntaxTree
        │
        ▼
EglSyntaxTree
```

Transpiler kaynak dili ve hedef dili bilir.

Kod üretmez.

---

## Code Generator

Generator yalnızca hedef dili bilir.

Görevi hedef Syntax Tree modelini gerçek kaynak koda dönüştürmektir.

Örneğin;

```text
EglSyntaxTree
        │
        ▼
EGL Source
```

Generator kaynak dili bilmez.

---

# Solution Yapısı

```text
LegacyCodeTransformer.sln

docs
│
├── Architecture.md
├── Decisions.md
├── Roadmap.md
├── ModuleSummaries.md
└── Glossary.md

src
│
├── LegacyCodeTransformer.Core
├── LegacyCodeTransformer.Shared
├── LegacyCodeTransformer.Pl1
├── LegacyCodeTransformer.Egl
├── LegacyCodeTransformer.Transpilers
├── LegacyCodeTransformer.Application
└── LegacyCodeTransformer.Cli

tests
│
├── LegacyCodeTransformer.Pl1.Tests
├── LegacyCodeTransformer.Egl.Tests
├── LegacyCodeTransformer.Transpilers.Tests
└── LegacyCodeTransformer.Application.Tests
```

> Fiziksel klasör yapısı zaman içerisinde değişebilir. Buradaki yapı mimari katmanları temsil etmektedir.

---

# Proje Sorumlulukları

## LegacyCodeTransformer.Core

Projedeki bütün diller tarafından ortak kullanılan temel modeller bulunur.

İlk sürüm kapsamı:

* SourceLocation
* SyntaxNode
* SyntaxTree
* DiagnosticSeverity
* Diagnostic
* DiagnosticBag
* ParseResult
* ConversionResult

Bu proje hiçbir dile özel kod içermez.

---

## LegacyCodeTransformer.Shared

Genel yardımcı sınıfları içerir.

Örnekler:

* Extension Method'lar
* Guard sınıfları
* Ortak yardımcı metodlar
* Genel sabitler

Dönüştürme mantığı bu projede bulunmaz.

---

## LegacyCodeTransformer.Pl1

PL/I diline ait bütün yapıları içerir.

İlk sürüm kapsamı:

* PL/I Lexer
* PL/I Parser
* Pl1SyntaxTree
* Declaration modelleri
* DataType modelleri
* PL/I Normalizer

Bu proje EGL hakkında hiçbir bilgi içermez.

---

## LegacyCodeTransformer.Egl

EGL diline ait bütün yapıları içerir.

İlk sürüm kapsamı:

* EglSyntaxTree
* Declaration modelleri
* DataType modelleri
* EGL Code Generator

Bu proje PL/I hakkında hiçbir bilgi içermez.

---

## LegacyCodeTransformer.Transpilers

Kaynak dil modelini hedef dil modeline dönüştürür.

İlk sorumluluğu:

```text
Pl1SyntaxTree
        │
        ▼
EglSyntaxTree
```

İlerleyen sürümlerde yeni dönüşümler aynı proje altında geliştirilecektir.

---

## LegacyCodeTransformer.Application

Pipeline'ın koordinasyonunu sağlar.

İlk sürüm akışı:

```text
PL/I Source
    │
    ▼
Lexer
    ▼
Parser
    ▼
Normalizer
    ▼
Transpiler
    ▼
Generator
    ▼
ConversionResult
```

Bu proje iş akışını yönetir.

Lexer, Parser veya Generator mantığını içermez.

---

## LegacyCodeTransformer.Cli

Komut satırı giriş noktasıdır.

İlk sürümde;

* PL/I kaynak kodunu alır.
* Application katmanını çağırır.
* Sonucu ekrana yazdırır.

İlerleyen sürümlerde;

* Dosya okuma
* Dosyaya yazma
* Parametreli çalışma
* Toplu dönüşüm

gibi özellikler eklenecektir.

---

# İlk Sürüm Kapsamı

İlk sürümün amacı tam bir PL/I dönüştürücüsü geliştirmek değildir.

İlk hedef başarıyla aşağıdaki dönüşümü gerçekleştirebilmektir.

PL/I

```pli
DCL MUST_NO FIXED DECIMAL(8);

DCL CUSTOMER_NO FIXED DECIMAL(10,2);
```

↓

EGL

```egl
mustNo decimal(8,0);

customerNo decimal(10,2);
```

Bu hedef doğrulandıktan sonra yeni PL/I yapıları adım adım eklenecektir.

---

# Mimari Prensipler

Bu proje boyunca aşağıdaki prensipler uygulanacaktır.

* Her katmanın tek bir sorumluluğu vardır.
* Parser hedef dili bilmez.
* Generator kaynak dili bilmez.
* Transpiler yalnızca model dönüşümü yapar.
* Syntax Tree tabanlı mimari kullanılır.
* Gereksiz soyutlamalardan kaçınılır.
* İhtiyaç oluşmadan yeni katman eklenmez.
* Genişletilebilirlik, gereksiz karmaşıklığın önüne geçirilmez.

Bu prensipler sayesinde proje hem öğrenilebilir hem de uzun yıllar geliştirilebilir bir yapıda kalacaktır.
