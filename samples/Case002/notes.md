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

Case002 güncel parser ile CLI case modu üzerinden yeniden çalıştırıldı.

Dönüşüm başarıyla tamamlandı ve actual.egl dosyası üretildi.

Üretilen çıktı:

    ErrorText char(50);
    CustomerNo decimal(8);
    CustomerProcess("CUSTOMER NOT FOUND");

## Doğru Çalışan Alanlar

- PROCEDURE(PROCESS_TEXT) header syntax'ı başarıyla parse edildi.
- PROCESS_TEXT procedure parameter adı sırası korunarak procedure
  modeline eklendi.
- Procedure body içindeki DCL PROCESS_TEXT CHAR(50) declaration'ı
  başarıyla parse edildi.
- Procedure body declaration ve executable statement koleksiyonları
  ayrı biçimde korundu.
- Procedure sonrasındaki top-level CALL statement başarıyla parse edildi.
- PL/I CALL statement EGL tarafında call keyword'ü kullanılmadan doğrudan
  function invocation olarak üretildi.
- Global ERROR_TEXT declaration'ı ErrorText char(50) olarak dönüştürüldü.
- Global CUSTOMER_NO declaration'ı CustomerNo decimal(8) olarak
  dönüştürüldü.
- actual.egl dosyası başarıyla oluşturuldu.

## Eksik veya Hatalı Alanlar

- CUSTOMER_PROCESS PL/I procedure'ü henüz EGL function olarak
  üretilmiyor.
- Procedure içindeki PROCESS_TEXT declaration'ı EGL function
  parametresine dönüştürülmüyor.
- Procedure içindeki ERROR_TEXT = PROCESS_TEXT assignment statement'ı
  actual.egl çıktısında bulunmuyor.
- Procedure içindeki WRITE_ERROR çağrısı actual.egl çıktısında
  bulunmuyor.
- Procedure parameter veri tipi ile header parameter adı arasında
  semantic binding henüz yapılmıyor.
- EGL function parameter direction bilgisi henüz belirlenmiyor.
- Procedure dönüştürülmediği halde conversion başarılı kabul ediliyor.
- Dönüştürülmeyen procedure için diagnostic üretilmiyor.

## Semantic Değerlendirme

PROCESS_TEXT declaration'ı procedure içinde korunmaktadır.

Ancak mevcut semantic analyzer yalnızca global declaration symbol table
üretmektedir.

Procedure parameter ve local declaration için ayrı scope veya local
symbol table henüz bulunmamaktadır.

Header parameter adı ile procedure declaration bilgisinin eşleştirilmesi
sonraki semantic adımlardan biri olacaktır.

## EGL Function Parameter İncelemesi

CUSTOMER_PROCESS procedure'ü EGL tarafında function olarak
dönüştürülmelidir.

Aday hedef yapı:

    function CustomerProcess(
        processText char(50) in)

        ErrorText = processText;
        WriteError(ErrorText);
    end

    CustomerProcess("CUSTOMER NOT FOUND");

Bu örnek henüz onaylanmış EGL output değildir.

Özellikle aşağıdaki bilgiler gerçek EGL örnekleriyle doğrulanmalıdır:

- Function parameter declaration syntax'ı
- CHAR(50) için doğru EGL parameter type
- `in` yönünün doğru kullanımı
- Function header satır bölme standardı
- Global alanların function içinden erişim davranışı
- Function'ların EGL program part içindeki yerleşimi

Parameter direction tahmin edilmeyecektir.

PROCESS_TEXT yalnızca okunuyor göründüğü için `in` adayıdır; ancak bu
bilgi semantic kullanım analizi ve gerçek EGL standardıyla
doğrulanmadan production output'a taşınmayacaktır.

## Uzman Geri Bildirimi

Henüz alınmadı.

## Sonraki İşlemler

1. Procedure header parameter ile procedure declaration binding modeli
   belirlenecek.
2. Procedure local declaration scope ihtiyacı değerlendirilecek.
3. EGL function parameter syntax'ı doğrulanacak.
4. PL/I procedure → EGL function syntax modeli oluşturulacak.
5. Procedure body mevcut statement transpiler üzerinden dönüştürülecek.
6. Dönüştürülmeyen procedure için diagnostic politikası belirlenecek.
7. Case001 ve Case002 yeniden çalıştırılacak.
8. actual.egl çıktıları tekrar incelenecek.