# Case001 İnceleme Notları

## Amaç

Global declaration bilgileri ile parametresiz PL/I procedure içinde
kullanılan assignment ve function invocation dönüşümünü incelemek.

## Input Kapsamı

- FIXED DECIMAL declaration
- CHAR declaration
- Parametresiz PL/I procedure
- Assignment statement
- Argument içeren PL/I CALL statement
- Global declaration bilgilerinin procedure içinde kullanılması

## Mevcut Çıktı İncelemesi

Case001 güncel compiler pipeline ile yeniden çalıştırıldı.

Dönüşüm başarıyla tamamlandı ve actual.egl dosyası üretildi.

Üretilen EGL:

    MustNo decimal(8);
    CustomerNo decimal(8);
    CustomerName char(30);

    function CustomerProcess()
        CustomerNo = MustNo;
        FetchCustomer(CustomerNo, CustomerName);
    end

## Doğru Dönüşen Alanlar

- MUST_NO FIXED DECIMAL(8) declaration'ı MustNo decimal(8) olarak
  dönüştü.
- CUSTOMER_NO FIXED DECIMAL(8) declaration'ı CustomerNo decimal(8)
  olarak dönüştü.
- CUSTOMER_NAME CHAR(30) declaration'ı CustomerName char(30) olarak
  dönüştü.
- CUSTOMER_PROCESS PL/I procedure'ü CustomerProcess EGL function
  olarak üretildi.
- Procedure içindeki CUSTOMER_NO = MUST_NO assignment statement'ı
  function body içinde üretildi.
- Procedure içindeki FETCH_CUSTOMER çağrısı doğrudan EGL function
  invocation olarak üretildi.
- Function body indentation standardı korundu.
- Global declaration'lar function bloğundan önce üretildi.
- actual.egl doğru case klasöründe oluşturuldu.
- actual.egl Git tarafından takip edilmedi.

## Eksik veya Hatalı Alanlar

Case001 kapsamındaki parametresiz procedure dönüşümünde gözlenen açık bir
dönüşüm kaybı bulunmamaktadır.

Ancak aşağıdaki hedef dil konuları uzman incelemesi gerektirir:

- Global alanların EGL function içinden kullanım standardı
- Function'ın bağımsız EGL source içinde mi yoksa program/library part
  içinde mi bulunması gerektiği
- EGL part wrapper ihtiyacı
- Function ve global declaration yerleşim sırası
- Function visibility standardı

## Dönüşüm Standardı Bulguları

PL/I procedure, parameter ve local declaration taşımadığı durumda EGL
function olarak üretilebilir.

PL/I:

    CUSTOMER_PROCESS: PROCEDURE;
        CUSTOMER_NO = MUST_NO;
        CALL FETCH_CUSTOMER(CUSTOMER_NO, CUSTOMER_NAME);
    END CUSTOMER_PROCESS;

EGL:

    function CustomerProcess()
        CustomerNo = MustNo;
        FetchCustomer(CustomerNo, CustomerName);
    end

PL/I CALL statement EGL tarafında call keyword'ü kullanılmadan doğrudan
function invocation olarak üretilir.

## Uzman Geri Bildirimi

Henüz alınmadı.

Özellikle EGL function'ın hangi program, library veya service part
içinde bulunması gerektiği gerçek EGL proje örnekleri üzerinden
doğrulanmalıdır.

## Sonraki İşlemler

1. Case001 EGL çıktısı uzman incelemesine sunulacak.
2. EGL part wrapper standardı doğrulanacak.
3. Function ve global alan yerleşimi doğrulanacak.
4. Geri bildirimler bu dosyaya eklenecek.
5. Doğruluğu kesinleştiğinde expected.egl adayı değerlendirilecek.