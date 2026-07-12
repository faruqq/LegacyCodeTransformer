# Case001 İnceleme Notları

## Amaç

Global declaration bilgileri ile procedure içinde kullanılan assignment ve
CALL statement'larının mevcut PL/I → EGL pipeline davranışını incelemek.

## Input Kapsamı

* FIXED DECIMAL declaration
* CHAR declaration
* Procedure tanımı
* Assignment statement
* Parametreli CALL statement
* Global declaration bilgilerinin procedure içinde kullanılması
* ## \## Mevcut Çıktı İncelemesi
* ## 
* ## CLI case modu başarıyla çalıştırıldı ve actual.egl dosyası üretildi.
* ## 
* ## Global declaration alanları EGL çıktısına dönüştürüldü.
* ## 
* ## PL/I procedure modeli parser ve semantic analyzer aşamalarında korunmasına
* ## rağmen transpiler ve EGL generator tarafında procedure desteği bulunmadığı
* ## için CUSTOMER\_PROCESS procedure'ü actual.egl çıktısında yer almadı.
* ## 
* ## \## Doğru Dönüşen Alanlar
* ## 
* ## \- MUST\_NO FIXED DECIMAL(8) declaration'ı MustNo decimal(8) olarak dönüştü.
* ## \- CUSTOMER\_NO FIXED DECIMAL(8) declaration'ı CustomerNo decimal(8) olarak dönüştü.
* ## \- CUSTOMER\_NAME CHAR(30) declaration'ı CustomerName char(30) olarak dönüştü.
* ## \- CLI input.pl1 dosyasını okuyabildi.
* ## \- CLI actual.egl dosyasını oluşturabildi.
* ## 
* ## \## Eksik veya Hatalı Alanlar
* ## 
* ## \- CUSTOMER\_PROCESS PL/I procedure'ü EGL function olarak üretilmedi.
* ## \- Procedure içindeki CUSTOMER\_NO = MUST\_NO assignment statement'ı
* ## &#x20; çıktıda yer almadı.
* ## \- Procedure içindeki FETCH\_CUSTOMER CALL statement'ı çıktıda yer almadı.
* ## \- Dönüştürülmeyen PL/I procedure için diagnostic üretilmedi.
* ## \- ConversionResult, kaynak kodun bir bölümü dönüştürülmediği halde
* ## &#x20; başarılı döndü.

