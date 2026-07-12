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

Case002 CLI case modu ile çalıştırıldı.

Dönüşüm başarısız olduğu için actual.egl dosyası üretilmedi.

Üretilen diagnostic mesajları:

    Error: ';' bekleniyordu.
    Error: Beklenmeyen token: (.
    Error: Beklenmeyen token: END.
    Error: '=' bekleniyordu.

Diagnostic zincirinin temel nedeni procedure parameter listesinin henüz
parser tarafından desteklenmemesidir.

Aşağıdaki procedure header mevcut parser kapsamı dışındadır:

    CUSTOMER_PROCESS: PROCEDURE(PROCESS_TEXT);

Mevcut ProcedureParser, PROCEDURE keyword'ünden sonra yalnızca:

    PROCEDURE;
    PROCEDURE OPTIONS(...);

biçimlerini desteklemektedir.

PROCEDURE sonrasında gelen açık parantez tanınmadığı için procedure
modeli oluşturulamamakta ve sonraki diagnostic mesajları recovery
sürecinde zincirleme olarak oluşmaktadır.

## Doğru Çalışan Alanlar

- CLI Case002 klasörünü bulabildi.
- CLI input.pl1 dosyasını okuyabildi.
- Parser hatalı veya desteklenmeyen input için diagnostic üretti.
- Dönüşüm başarısız olduğunda actual.egl oluşturulmadı.
- Başarısız parse sonucu transpiler ve generator aşamalarına taşınmadı.

## Tespit Edilen Eksikler

- PROCEDURE(PARAMETER) header syntax'ı desteklenmiyor.
- Procedure parameter syntax modeli bulunmuyor.
- Procedure parameter listesi ile CALL argument listesi arasında model
  ilişkisi bulunmuyor.
- Procedure body içindeki parameter declaration desteklenmiyor.
- Procedure body yalnızca executable statement parser üzerinden
  işleniyor.
- Procedure local declaration modeli bulunmuyor.
- Procedure parameter scope modeli bulunmuyor.
- Parameter veri tipi henüz procedure header parametresiyle
  ilişkilendirilemiyor.
- EGL function parameter yönü belirlenemiyor.

## Diagnostic Değerlendirmesi

İlk diagnostic gerçek kök hatadır:

    ';' bekleniyordu.

Aşağıdaki diagnostic mesajları ilk parse hatasından sonra oluşan
zincirleme recovery sonuçlarıdır:

    Beklenmeyen token: (
    Beklenmeyen token: END
    '=' bekleniyordu.

Procedure parameter desteği eklendikten sonra diagnostic recovery
davranışı yeniden değerlendirilmelidir.

Aynı kaynak için yalnızca gerçekten bağımsız hataların raporlanması
hedeflenmelidir.

## EGL Function Parameter İncelemesi

PL/I procedure parametresinin EGL function parametresine dönüşebilmesi
için yalnızca parameter adı yeterli değildir.

Her parameter için aşağıdaki bilgiler gereklidir:

- Parameter adı
- PL/I declaration
- PL/I veri tipi
- EGL veri tipi
- in yönü
- out yönü
- inOut yönü
- Procedure body içindeki okuma ve yazma kullanımları

Parameter yönü tahmin edilmeyecektir.

PROCESS_TEXT yalnızca okunuyorsa EGL yönü için `in` adayı olabilir.

Ancak bu davranış gerçek EGL function örnekleri ve PL/I kullanım
analizleriyle doğrulanmadan production mapping olarak kabul
edilmeyecektir.

## Uzman Geri Bildirimi

Henüz alınmadı.

EGL function parameter declaration ve direction standardı gerçek EGL
örnekleri üzerinden doğrulanmalıdır.

## Sonraki İşlemler

1. Pl1Procedure parameter modelinin ilk kapsamı belirlenecek.
2. PROCEDURE(PARAMETER) parser desteği eklenecek.
3. Procedure body declaration parsing ihtiyacı değerlendirilecek.
4. Parameter declaration ile header parameter adı ilişkilendirilecek.
5. Procedure parameter scope semantic modeline hazırlanacak.
6. Case002 yeniden çalıştırılacak.
7. Diagnostic zincirinin sadeleştiği doğrulanacak.
8. EGL parameter type ve direction mapping ayrıca ele alınacak.