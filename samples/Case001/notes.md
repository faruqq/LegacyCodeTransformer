\# Case001 İnceleme Notları



\## Amaç



Global declaration bilgileri ile procedure içinde kullanılan assignment

ve function çağrılarının mevcut PL/I → EGL pipeline davranışını

incelemek.



\## Input Kapsamı



\- FIXED DECIMAL declaration

\- CHAR declaration

\- PL/I procedure tanımı

\- Assignment statement

\- Argument içeren PL/I CALL statement

\- Global declaration bilgilerinin procedure içinde kullanılması



\## Mevcut Çıktı İncelemesi



CLI case modu başarıyla çalıştırıldı.



input.pl1 dosyası okundu ve actual.egl dosyası üretildi.



Üretilen mevcut çıktı:



&#x20;   MustNo decimal(8);

&#x20;   CustomerNo decimal(8);

&#x20;   CustomerName char(30);



\## Doğru Dönüşen Alanlar



\- MUST\_NO FIXED DECIMAL(8) declaration'ı MustNo decimal(8) olarak

&#x20; dönüştü.

\- CUSTOMER\_NO FIXED DECIMAL(8) declaration'ı CustomerNo decimal(8)

&#x20; olarak dönüştü.

\- CUSTOMER\_NAME CHAR(30) declaration'ı CustomerName char(30) olarak

&#x20; dönüştü.

\- CLI input.pl1 dosyasını okuyabildi.

\- CLI actual.egl dosyasını doğru case klasöründe oluşturabildi.

\- actual.egl Git tarafından takip edilmedi.



\## Eksik veya Hatalı Alanlar



\- CUSTOMER\_PROCESS PL/I procedure'ü EGL function olarak üretilmedi.

\- Procedure içindeki CUSTOMER\_NO = MUST\_NO assignment statement'ı

&#x20; actual.egl çıktısında yer almadı.

\- Procedure içindeki FETCH\_CUSTOMER çağrısı actual.egl çıktısında yer

&#x20; almadı.

\- Dönüştürülmeyen PL/I procedure için diagnostic üretilmedi.

\- Kaynak kodun procedure bölümü dönüştürülmediği halde conversion

&#x20; başarılı kabul edildi.



\## Dönüşüm Standardı Bulguları



PL/I CALL statement EGL tarafında `call` keyword'üyle üretilmemelidir.



PL/I:



&#x20;   CALL FETCH\_CUSTOMER(CUSTOMER\_NO, CUSTOMER\_NAME);



Doğru EGL:



&#x20;   FetchCustomer(CustomerNo, CustomerName);



Bu hata production generator ve regression testlerinde düzeltilmiştir.



\## EGL Function İncelemesi



PL/I procedure yapısının EGL hedef karşılığı function olacaktır.



İlk aday function biçimi:



&#x20;   function CustomerProcess()

&#x20;       CustomerNo = MustNo;

&#x20;       FetchCustomer(CustomerNo, CustomerName);

&#x20;   end



Bu çıktı henüz compiler tarafından üretilmemektedir.



PL/I procedure parametreleri ileride desteklendiğinde EGL function

parametrelerinin:



\- veri tipi

\- in

\- out

\- inOut



bilgileri gerçek PL/I declaration ve kullanım bilgisi üzerinden

belirlenmelidir.



Parametre yönü tahmin edilmeyecektir.



\## Uzman Geri Bildirimi



Henüz alınmadı.



EGL function gövdesi, global alan kullanımı ve parametre yönleri gerçek

EGL örnekleri üzerinden ayrıca doğrulanacaktır.



\## Sonraki İşlemler



1\. PL/I procedure → EGL function dönüşüm modeli tasarlanacak.

2\. Procedure body mevcut statement transpiler üzerinden dönüştürülecek.

3\. Dönüştürülmeyen procedure durumunda diagnostic politikası

&#x20;  belirlenecek.

4\. Case001 yeniden çalıştırılacak.

5\. Yeni actual.egl çıktısı incelenecek.

6\. Uzman geri bildirimi bu dosyaya eklenecek.

