\# PL/I Conversion Cases



Bu klasör, LegacyCodeTransformer projesinin gerçek veya

anonimleştirilmiş PL/I kaynak örneklerini içerir.



Case dosyaları sentetik unit test örneklerinden farklıdır.



Amaç, gerçek PL/I kaynaklarında birlikte kullanılan declaration,

procedure, statement, embedded SQL ve compiler directive yapılarını

mevcut PL/I → EGL pipeline üzerinden incelemektir.



\## Case İsimlendirme Standardı



Case klasörleri nötr ve sıralı adlandırılır.



&#x20;   Case001

&#x20;   Case002

&#x20;   Case003



Case adı, örneğin yalnızca tek bir PL/I özelliğini içerdiği anlamına

gelmez.



Ortalama bir case birden fazla declaration, procedure ve statement

yapısını birlikte içerebilir.



\## Case Dosyaları



Her case klasöründe aşağıdaki dosyalar bulunur:



&#x20;   input.pl1

&#x20;   actual.egl

&#x20;   notes.md



Dosya sorumlulukları:



\- input.pl1 kalıcı PL/I kaynak örneğidir.

\- actual.egl CLI tarafından yeniden üretilen inceleme çıktısıdır.

\- notes.md dönüşüm bulgularını ve uzman geri bildirimlerini taşır.



actual.egl Git tarafından takip edilmez.



input.pl1 ve notes.md proje hafızasının kalıcı parçalarıdır.



\## PL/I Kaynak Standardı



input.pl1 içindeki her fiziksel PL/I kod satırının ilk karakteri bir

boşluk olmalıdır.



İlk boşluktan sonra en fazla 72 yazılabilir karakter bulunabilir.



Bir fiziksel satırın toplam uzunluğu en fazla 73 karakter olmalıdır.



Uzun statement'lar birden fazla fiziksel satıra bölünmelidir.



\## Case Yaşam Döngüsü



1\. PL/I örneği input.pl1 dosyasına eklenir.

2\. CLI case modu çalıştırılır.

3\. actual.egl dosyası üretilir.

4\. Çıktı insan tarafından incelenir.

5\. Bulgular notes.md dosyasına yazılır.

6\. Yeni parser, semantic, transpiler veya generator ihtiyaçları

&#x20;  belirlenir.

7\. Gerekli compiler desteği geliştirilir.

8\. Case yeniden çalıştırılır.

9\. Doğruluğu kesinleşen case ileride expected.egl regression

&#x20;  standardına alınabilir.



\## Çalıştırma



Repository kökünde:



&#x20;   dotnet run --project LegacyCodeTransformer.Cli -- \\

&#x20;       --case "samples/Case001"



Windows PowerShell veya CMD üzerinde tek satır kullanım:



&#x20;   dotnet run --project LegacyCodeTransformer.Cli -- --case "samples/Case001"



\## Mevcut Case'ler



\### Case001



Global declaration ve parametresiz PL/I procedure kullanımını içerir.



İlk bulgu:



\- Global declaration'lar EGL çıktısına üretilmektedir.

\- PL/I procedure henüz EGL function olarak üretilmemektedir.

\- Procedure body output'a taşınmamaktadır.



\### Case002



Procedure parameter listesi ve procedure içindeki parameter

declaration kullanımını içerir.



Temel inceleme konuları:



\- PL/I procedure parameter modelleme

\- EGL function parameter veri tipi

\- EGL function parameter yönü

\- Global declaration ve procedure parameter ayrımı

\- PL/I CALL argument ile function parameter eşleşmesi

