# LegacyCodeTransformer - Project State

Bu dosya LegacyCodeTransformer projesinin güncel geliştirme durumunu,
çalışma kurallarını ve bir sonraki teknik hedefini özetler.

Bu dosyanın amacı yeni bir geliştirme oturumunda projeye önceki oturum
kesilmemiş gibi devam edilebilmesini sağlamaktır.

Bu dosya ayrıntılı teknik kararların yerine geçmez.

Ayrıntılı ve bağlayıcı bilgiler için aşağıdaki dosyalar source of truth
olarak kabul edilir:

- Decisions.md
- Roadmap.md
- ModuleSummaries.md
- docs/Architecture.md
- docs/Pl1CodingStandards.md

------------------------------------------------------------
PROJE
------------------------------------------------------------

Repository:

    https://github.com/faruqq/LegacyCodeTransformer

Hedef:

    Gerçek PL/I kaynak kodlarını lexer, parser, semantic analyzer,
    transpiler ve generator aşamalarından geçirerek EGL kaynak koduna
    dönüştüren compiler/transpiler altyapısı geliştirmek.

Ana pipeline:

    PL/I Source
        ↓
    Lexer
        ↓
    Parser
        ↓
    PL/I Syntax Tree
        ↓
    Semantic Analyzer
        ↓
    PL/I → EGL Transpiler
        ↓
    EGL Syntax Tree
        ↓
    EGL Code Generator
        ↓
    EGL Source

Hedef framework:

    .NET 8

------------------------------------------------------------
AKTİF DURUM
------------------------------------------------------------

Aktif faz:

    P10 — Statement & Declaration Coverage Expansion

Aktif milestone:

    P10.3 — Procedure & Statement Coverage Expansion

Milestone durumu:

    Devam ediyor.

Son tamamlanan production kapsamı:

- Procedure header parameter modelinin korunması
- Procedure body declaration bilgilerinin korunması
- Header parameter ile body declaration arasında semantic binding
- Parametresiz PL/I procedure → EGL function dönüşümü
- Procedure body statement'larının EGL function body içinde üretilmesi
- EglSyntaxTree üzerinde Declarations, Functions ve Statements
  koleksiyonlarının ayrı korunması
- EGL generator tarafından function block üretilmesi
- Desteklenmeyen procedure dönüşümlerinde kontrollü diagnostic

Son tamamlanan doğrulamalar:

- Case001 başarılı şekilde EGL output üretmektedir.
- Case002 parameterized procedure mapping henüz desteklenmediği için
  kontrollü diagnostic üretmektedir.
- Desteklenmeyen procedure business logic'i sessizce kaybolmamaktadır.
- Conversion başarısız olduğunda actual.egl üretilmemektedir.

------------------------------------------------------------
SAMPLE DURUMU
------------------------------------------------------------

samples klasörü gerçek PL/I örnekleri üzerinden çalışan regression
laboratuvarıdır.

Sample isimleri aşağıdaki formatta tutulur:

    Case001
    Case002
    Case003
    ...

Feature adları sample klasör adı olarak kullanılmaz.

Her sample temel olarak aşağıdaki dosyaları içerir:

    input.pl1
    notes.md

actual.egl generator tarafından oluşturulan build artifact'tır.

Repository tarafından takip edilmez.

.gitignore kuralı:

    samples/**/actual.egl

CLI sample çalıştırma formatı:

    dotnet run --project LegacyCodeTransformer.Cli -- --case "samples/Case001"

Case001:

    Parametresiz PL/I procedure içerir.

    Güncel durum:

        Başarılı.

    Desteklenen temel dönüşümler:

    - Global declaration
    - Assignment statement
    - CALL statement
    - Parametresiz procedure
    - EGL function generation

Case002:

    Parametreli PL/I procedure içerir.

    Güncel durum:

        Kontrollü başarısızlık.

    Güncel diagnostic:

        Parameter içeren PL/I procedure için EGL function mapping henüz
        desteklenmiyor: CUSTOMER_PROCESS.

    Bu davranış bilinçlidir.

    Parameter type ve direction bilgisi tamamlanmadan eksik EGL output
    üretilmez.

------------------------------------------------------------
BİR SONRAKİ HEDEF
------------------------------------------------------------

Ana hedef:

    Parameterized PL/I Procedure → EGL Function

Bu hedefe geçmeden önce çözülmesi gereken konular:

1. EGL function parameter syntax modeli
2. Semantic binding üzerinden parameter data type erişimi
3. PL/I parameter type → EGL parameter type mapping
4. Parameter direction analizi
5. in / out / inOut politikası
6. Procedure parameter declaration ile local declaration ayrımı
7. Generator parameter output formatı
8. Case002 end-to-end dönüşümü

İlk production adımı, mevcut repository durumu incelendikten sonra
minimum extensible architecture prensibine göre belirlenmelidir.

------------------------------------------------------------
ŞU ANDA BİLİNÇLİ OLARAK YAPILMAYANLAR
------------------------------------------------------------

- EGL function parameter generation
- in direction mapping
- out direction mapping
- inOut direction mapping
- Procedure local declaration output
- Procedure local scope
- OPTIONS(MAIN) mapping
- EGL program, library veya service part wrapper
- RETURN statement mapping
- Tahmine dayalı EGL syntax generation
- Eksik dönüşümün diagnostic olmadan atlanması

Bu konuların hiçbiri gerçek örnek, semantic bilgi veya doğrulanmış EGL
syntax olmadan production'a eklenmemelidir.

------------------------------------------------------------
MİMARİ PRENSİPLER
------------------------------------------------------------

Minimal Extensible Architecture:

    Yalnızca mevcut gerçek ihtiyaç için gerekli abstraction eklenir.

    Gelecekte kullanılabilir düşüncesiyle gereksiz model, interface,
    factory veya katman eklenmez.

Real-world first:

    Yeni parser, semantic veya transpiler özelliği gerçek bir PL/I
    örneğinden doğmalıdır.

Sample-driven development:

    Yeni bir yetenek mümkün olduğunda önce veya en geç production
    değişikliğiyle birlikte bir sample case üzerinden ele alınmalıdır.

Separation of concerns:

    Lexer yalnızca token üretir.

    Parser syntax modelini oluşturur.

    Semantic analyzer anlam ve binding bilgilerini çözer.

    Transpiler PL/I modelini EGL modeline dönüştürür.

    Generator yalnızca EGL modelinden kaynak kod üretir.

No silent data loss:

    Desteklenmeyen PL/I syntax veya semantic durum sessizce atlanmaz.

    Açık diagnostic üretilir.

No semantic guessing:

    Parameter direction, scope, type veya target-language syntax tahmin
    edilmez.

Identifier policy:

    Parser orijinal PL/I identifier adını korur.

    EGL naming dönüşümü transpiler katmanında uygulanır.

    Varsayılan naming strategy PascalCase'tir.

------------------------------------------------------------
GELİŞTİRME AKIŞI
------------------------------------------------------------

Her milestone aşağıdaki sırayla yürütülür:

1. Production code
2. Unit tests
3. Decisions.md
4. ModuleSummaries.md
5. Roadmap.md
6. Commit

Bir aşama tamamlanmadan sonraki aşamaya geçilmez.

Kullanıcı commit hash'ini paylaşmadan commit edilmemiş değişiklikler
repository'de varmış gibi kabul edilmez.

Her batch sonunda uygun bir commit adı önerilir.

Commit zamanı geldiğinde açıkça:

    Commit bekliyorum.

ifadesi kullanılır.

------------------------------------------------------------
REPOSITORY KULLANIM KURALI
------------------------------------------------------------

Öneriler her zaman kullanıcı tarafından verilen son commit'e göre
hazırlanır.

Kod önermeden önce repository'nin ilgili production, test ve
dokümantasyon dosyaları incelenir.

Repository'de olmayan sınıf, constructor, namespace, method veya model
hakkında varsayım yapılmaz.

Öneri commit edilmemiş yerel bir değişikliğe bağlıysa aşağıdaki şekilde
ilerlenir:

    Bu değişiklik son commit'te görünmüyor. Commit edilmemiş yerel
    değişiklik varsa ilgili kodu veya yeni commit hash'ini paylaş.

Repository incelemeden kesin dosya yolu, constructor veya method
önerilmez.

------------------------------------------------------------
KOD TESLİM STANDARDI
------------------------------------------------------------

Production code:

- Tam class verilir.
- Veya tam method verilir.
- Method body parçası verilmez.
- Önemli helper sınıflarında tercihen tüm dosya verilir.

Unit test:

- Yeni test dosyasında tam class verilir.
- Mevcut test değişiyorsa tam değişen test methodu verilir.
- Zorunlu olmadıkça tüm mevcut test class'ı yeniden verilmez.

Her değişiklikte aşağıdakiler açıkça belirtilir:

- Tam dosya yolu
- Yeni dosya mı mevcut dosya mı olduğu
- Ekleme veya değiştirme noktası
- Hangi method veya class'ın değişeceği

Kod yorumları Türkçe yazılır.

Önemli class ve test açıklamalarında mevcut proje standardı kullanılır:

- Neden var?
- Ne çözüyor?
- Hangi örneği destekliyor?
- Nerede kullanılır?
- Gelecekte neye temel olur?

------------------------------------------------------------
DOKÜMANTASYON STANDARDI
------------------------------------------------------------

Dokümantasyon değişikliklerinde:

- Tam dosya yolu belirtilir.
- Kesin ekleme veya değiştirme noktası belirtilir.
- "Uygun yere ekle" veya "varsa bu bölümün altına ekle" gibi belirsiz
  yönlendirmeler kullanılmaz.
- Decisions.md içindeki son decision numarası repository'den doğrulanır.
- Decision numarası tahmin edilmez.
- Roadmap milestone durumları gerçek implementation durumuna göre
  değiştirilir.
- Markdown doküman snippet'lerinde triple backtick kullanılmaz.
- Plain veya indented block kullanılır.

------------------------------------------------------------
TEST STANDARDI
------------------------------------------------------------

Her yeni test şu soruya cevap vermelidir:

    Gerçek bir PL/I programcısı bunu yazabilir mi?

Gerçek kullanım değeri bulunmayan yapay parser kombinasyonları yalnızca
test sayısını artırmak amacıyla eklenmez.

Testler aşağıdaki seviyelerde dengeli tutulur:

- Syntax model tests
- Parser tests
- Semantic tests
- Transpiler tests
- Generator tests
- Application end-to-end tests
- Sample regression checks

Production code ile test code dengeli ilerlemelidir.

Testler production tasarımını gereksiz abstraction'a zorlamamalıdır.

------------------------------------------------------------
REFACTOR KURALI
------------------------------------------------------------

"Daha temiz görünür" veya "ileride lazım olabilir" gerekçesi tek başına
refactor sebebi değildir.

Refactor yalnızca aşağıdaki durumlardan en az birini sağlıyorsa önerilir:

- Mimari sınırları gerçekten netleştiriyorsa
- Anlamlı kod tekrarını azaltıyorsa
- Aktif feature geliştirmesini kolaylaştırıyorsa
- Bakım maliyetini ölçülebilir biçimde düşürüyorsa
- Hatalı sorumluluk dağılımını düzeltiyorsa

Büyük refactor ile feature development aynı batch içinde karıştırılmaz.

------------------------------------------------------------
PL/I KAYNAK STANDARDI
------------------------------------------------------------

Projede kullanılan kritik PL/I kaynak kuralları:

1. Her PL/I kaynak satırının ilk karakteri boşluktur.
2. Yazılabilir kaynak alanı 72 karakterdir.
3. İlk boşluk ile birlikte fiziksel satır uzunluğu 73 karakterdir.
4. Procedure formatı:

       PROCEDURE_NAME: PROCEDURE;
           ...
       END PROCEDURE_NAME;

5. Procedure invocation formatı:

       CALL PROCEDURE_NAME;

Ayrıntılı kurallar için:

    docs/Pl1CodingStandards.md

------------------------------------------------------------
OTURUM DEVAM KURALI
------------------------------------------------------------

Yeni geliştirme oturumunda aşağıdaki sıra izlenmelidir:

1. Kullanıcının verdiği son commit doğrulanır.
2. Bu ProjectState.md dosyası okunur.
3. Roadmap.md üzerinden aktif milestone doğrulanır.
4. Decisions.md üzerinden son kararlar doğrulanır.
5. ModuleSummaries.md üzerinden tamamlanan batch'ler doğrulanır.
6. Aktif milestone ile ilgili production ve test dosyaları incelenir.
7. Sonraki minimum production batch önerilir.

Bu dosya repository'nin mevcut durumuyla çelişirse repository ve
bağlayıcı dokümantasyon dosyaları esas alınır.