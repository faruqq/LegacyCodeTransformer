# Case002 İnceleme Notları

## Amaç

Parametre alan PL/I procedure yapısının mevcut lexer, parser,
semantic analyzer, transpiler ve EGL generator davranışını incelemek.

## Kaynak Dayanağı

Paylaşılan PL/I eğitim dokümanlarında aşağıdaki yapıya sahip gerçekçi
procedure örnekleri bulunmaktadır:

    PROCEDURE_NAME: PROCEDURE(PARAMETER);
        DCL PARAMETER CHAR(50);
        ...
    END PROCEDURE_NAME;

Bu case aynı procedure biçimini anonimleştirilmiş ve küçük bir örnek
üzerinden temsil eder.

## Input Kapsamı

- Global CHAR declaration
- Global FIXED DECIMAL declaration
- Parameter listesi bulunan PL/I procedure
- Procedure içinde parameter declaration
- Assignment statement
- Argument içeren PL/I CALL statement
- Procedure dışından parameter ile CALL kullanımı
- String literal argument

## İncelenecek Parser Davranışı

- Procedure header içindeki PROCESS_TEXT parametresi parse ediliyor mu?
- Procedure içindeki DCL statement destekleniyor mu?
- Procedure body statement sırası korunuyor mu?
- Procedure sonrasındaki top-level CALL statement korunuyor mu?

## İncelenecek Semantic Davranış

- PROCESS_TEXT global symbol olarak mı değerlendiriliyor?
- Procedure parameter için ayrı scope ihtiyacı oluşuyor mu?
- Procedure içindeki declaration duplicate kabul ediliyor mu?
- ERROR_TEXT ve CUSTOMER_NO global symbol olarak korunuyor mu?
- PROCESS_TEXT reference kullanımı resolved kabul ediliyor mu?

## İncelenecek EGL Dönüşümü

PL/I procedure hedef dilde EGL function olarak değerlendirilmelidir.

Ancak EGL function parametresinin yalnızca adı yeterli değildir.

Her parameter için aşağıdaki bilgiler belirlenmelidir:

- EGL veri tipi
- in yönü
- out yönü
- inOut yönü

Parameter yönü tahmin edilmeyecektir.

PL/I procedure parametresi ile procedure içindeki declaration ve
kullanım şekli birlikte değerlendirilmelidir.

## Aday EGL Function

Aşağıdaki örnek yalnızca inceleme adayını gösterir ve henüz onaylanmış
compiler çıktısı değildir:

    function CustomerProcess(
        processText string in)

        ErrorText = processText;
        WriteError(ErrorText);
    end

Bu aday çıktıdaki aşağıdaki konular doğrulanmamıştır:

- CHAR(50) için function parameter EGL veri tipi
- Parameter declaration syntax'ı
- `in` yönünün doğruluğu
- Global alanların function içinden erişimi
- Function satır bölme ve formatting standardı

Bu nedenle bu örnek expected EGL olarak kullanılmayacaktır.

## Mevcut Çıktı İncelemesi

Case002 parametresiz EGL function desteği eklendikten sonra yeniden
çalıştırıldı.

Dönüşüm kontrollü olarak başarısız oldu.

Üretilen diagnostic:

    Parameter içeren PL/I procedure için EGL function mapping henüz
    desteklenmiyor: CUSTOMER_PROCESS.

actual.egl dosyası üretilmedi.

Bu davranış bilinçlidir.

Parameter ve direction bilgisi tamamlanmadan eksik EGL function üretmek
yerine conversion pipeline durdurulmaktadır.

## Doğru Çalışan Alanlar

- PROCEDURE(PROCESS_TEXT) header syntax'ı başarıyla parse edilmektedir.
- PROCESS_TEXT header parameter sırası korunmaktadır.
- DCL PROCESS_TEXT CHAR(50) procedure body declaration'ı parse
  edilmektedir.
- Header parameter ile body declaration semantic binding'i
  çözülmektedir.
- PROCESS_TEXT binding üzerinden Pl1CharacterType ve Length = 50
  bilgisine erişilebilmektedir.
- Procedure body assignment ve CALL statement modelleri syntax tree
  üzerinde korunmaktadır.
- Top-level CALL CUSTOMER_PROCESS statement'ı parse edilmektedir.
- Desteklenmeyen parameter mapping sessizce atlanmamaktadır.
- Conversion başarısız olduğunda EGL output üretilmemektedir.

## Eksik veya Hatalı Alanlar

- EGL function parameter syntax modeli bulunmamaktadır.
- PROCESS_TEXT için EGL parameter type henüz üretilmemektedir.
- Parameter direction henüz belirlenmemektedir.
- in, out veya inOut mapping yapılmamaktadır.
- Procedure body declaration'ı EGL function parameter declaration'a
  dönüştürülmemektedir.
- Parameterized CustomerProcess function henüz üretilmemektedir.
- Procedure dışındaki CustomerProcess invocation da output'a
  yazılmamaktadır; çünkü conversion diagnostic nedeniyle generator
  aşamasına geçilmemektedir.

## Semantic Değerlendirme

PROCESS_TEXT için semantic binding resolved durumdadır.

Binding bilgileri:

    ProcedureName = CUSTOMER_PROCESS
    ParameterName = PROCESS_TEXT
    Declaration.Name = PROCESS_TEXT
    Declaration.DataType = Pl1CharacterType
    Declaration.DataType.Length = 50
    IsResolved = true

Bu bilgi EGL parameter type mapping için yeterli bir foundation
sağlamaktadır.

Ancak direction bilgisi declaration tipinden tek başına çıkarılamaz.

Procedure body içindeki read/write kullanımları ayrıca analiz
edilmelidir.

## EGL Function Parameter İncelemesi

Aday hedef yapı:

    function CustomerProcess(
        processText char(50) in)

        ErrorText = processText;
        WriteError(ErrorText);
    end

    CustomerProcess("CUSTOMER NOT FOUND");

Bu örnek henüz onaylanmış EGL output değildir.

Aşağıdaki konular doğrulanmadan production mapping yapılmayacaktır:

- EGL function parameter declaration sırası
- Type ve direction keyword sırası
- CHAR(50) parameter syntax'ı
- `in` direction doğruluğu
- Function header satır bölme standardı
- Local declaration ve parameter ayrımı
- Function'ın EGL part içindeki yerleşimi

## Uzman Geri Bildirimi

Henüz alınmadı.

## Sonraki İşlemler

1. EGL function parameter syntax modeli oluşturulacak.
2. PL/I parameter declaration data type EGL type'a map edilecek.
3. Parameter read/write kullanım analizi tasarlanacak.
4. in / out / inOut direction politikası gerçek örneklerle
   doğrulanacak.
5. Parameterized Pl1Procedure → EglFunction dönüşümü eklenecek.
6. Case002 yeniden çalıştırılacak.
7. actual.egl çıktısı uzman incelemesine sunulacak.