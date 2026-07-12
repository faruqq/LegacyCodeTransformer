\# Real-World PL/I Case Inventory



Bu dosya, paylaşılan PL/I dokümanları ve gerçek case incelemelerinden

çıkarılan destek ihtiyaçlarını kaydeder.



Bu liste doğrudan production backlog değildir.



Her madde gerçek case üzerinden doğrulanmadan compiler'a

eklenmeyecektir.



\## Öncelik Seviyeleri



\- P1: Mevcut gerçek case dönüşümünü doğrudan engelliyor.

\- P2: Gerçek örneklerde yaygın fakat mevcut case'i tamamen engellemiyor.

\- P3: İleri kapsam veya daha az doğrulanmış kullanım.



\## Procedure ve Blok Yapıları



\### P1 — Parametresiz PL/I procedure



Örnek:



&#x20;   PROCEDURE\_NAME: PROCEDURE;

&#x20;       ...

&#x20;   END PROCEDURE\_NAME;



Mevcut durum:



\- Parser desteği var.

\- Syntax tree modeli var.

\- Semantic traversal var.

\- EGL function transpiler desteği yok.

\- EGL function generator desteği yok.

\- Sessiz dönüşüm kaybı yaşanıyor.



İlgili case:



\- Case001



\### P1 — Parametreli PL/I procedure



Örnek:



&#x20;   PROCEDURE\_NAME: PROCEDURE(PARAMETER);

&#x20;       DCL PARAMETER CHAR(50);

&#x20;       ...

&#x20;   END PROCEDURE\_NAME;



Case002 ile doğrulanan mevcut durum:



\- Lexer parameter listesi tokenlarını üretebilmektedir.

\- ProcedureParser, PROCEDURE sonrasında parameter listesi

&#x20; desteklememektedir.

\- ProcedureParser doğrudan semicolon veya OPTIONS beklemektedir.

\- Procedure parameter syntax modeli bulunmamaktadır.

\- Procedure body içindeki DCL declaration desteklenmemektedir.

\- Procedure local declaration ve parameter scope modeli

&#x20; bulunmamaktadır.

\- Parse başarısız olduğu için EGL output üretilmemektedir.

\- İlk parse hatasından sonra zincirleme recovery diagnostic'leri

&#x20; oluşmaktadır.



Gözlenen diagnostic mesajları:



&#x20;   ';' bekleniyordu.

&#x20;   Beklenmeyen token: (

&#x20;   Beklenmeyen token: END.

&#x20;   '=' bekleniyordu.



İlk production kapsamı:



1\. Procedure parameter syntax modeli

2\. PROCEDURE(PARAMETER) parser desteği

3\. Birden fazla parameter için virgüllü liste desteği

4\. Header parameter sırasının korunması

5\. Parameter declaration ilişkisinin sonraki adıma hazırlanması

6\. Parser unit testleri

7\. Case002 yeniden çalıştırması



İlk kapsamda yapılmayacaklar:



\- EGL parameter yönü tahmini

\- Full local scope analysis

\- Procedure parameter type resolution

\- Procedure local variable semantic analysis

\- PL/I procedure → EGL function generation



İlgili case:



\- Case002

\### P1 — OPTIONS(MAIN) ana procedure



Örnek:



&#x20;   PROGRAM\_NAME: PROCEDURE OPTIONS(MAIN);

&#x20;       ...

&#x20;   END PROGRAM\_NAME;



İncelenecek konular:



\- EGL program part üretimi

\- function main() mapping

\- PL/I program adı ile EGL program adı eşleşmesi

\- Global declaration yerleşimi

\- Internal function yerleşimi



\### P2 — Internal procedure



Örnek:



&#x20;   PROGRAM\_NAME: PROCEDURE OPTIONS(MAIN);



&#x20;       INTERNAL\_PROCESS: PROCEDURE;

&#x20;           ...

&#x20;       END INTERNAL\_PROCESS;



&#x20;   END PROGRAM\_NAME;



İncelenecek konular:



\- Nested procedure parser desteği

\- EGL function yerleşimi

\- Scope ve symbol çözümleme

\- Internal function visibility



\### P2 — BEGIN block



Örnek:



&#x20;   BLOCK\_NAME: BEGIN;

&#x20;       ...

&#x20;   END BLOCK\_NAME;



Mevcut durum:



\- Ayrı syntax modeli bulunmuyor.

\- PL/I DO block ile karıştırılmamalı.



\## Control Flow Statement'ları



\### P1 — RETURN



Procedure yürütmesini çağıran noktaya döndürür.



İncelenecek konular:



\- Parametresiz return

\- Değer döndüren procedure/function ayrımı

\- EGL return statement mapping



\### P2 — LEAVE



DO block veya loop çıkışında kullanılır.



\### P2 — STOP



Program yürütmesini sonlandırır.



\### P2 — GO TO



Doğrudan dönüşüm yerine diagnostic veya kontrollü refactor stratejisi

gerekebilir.



\## Declaration Yapıları



\### P1 — Procedure parameter declaration



Procedure header parametresi ile body declaration ilişkisi

kurulmalıdır.



\### P2 — FILE declaration



Örnek aileleri:



\- FILE STREAM INPUT

\- FILE STREAM OUTPUT

\- ENV option



\### P2 — POINTER



CICS ve communication area örneklerinde görülmektedir.



\### P2 — Storage class



İncelenecek örnekler:



\- STATIC

\- AUTOMATIC

\- BASED



\## I/O Statement'ları



\### P2 — GET



\### P2 — PUT



\### P2 — READ



\### P2 — WRITE



\### P2 — OPEN



\### P2 — CLOSE



Bu statement'ların EGL karşılığı doğrulanmadan mapping

yapılmayacaktır.



\## Embedded SQL



\### P1 — EXEC SQL INCLUDE



Parser raw text olarak korumaktadır.



EGL output stratejisi henüz bulunmamaktadır.



\### P1 — EXEC SQL SELECT INTO



İncelenecek konular:



\- SQL text korunması

\- Host variable mapping

\- Qualified structure host variable

\- SQLCODE kullanımı

\- WITH UR



\### P2 — EXEC SQL ROLLBACK



\## Compiler Directive



\### P2 — PROCESS directive



Örnek:



&#x20;   %PROCESS GS,INCLUDE,NEST;



Mevcut compiler directive parser ile gerçek syntax uyumu ayrıca

doğrulanmalıdır.



\## İlk Uygulama Önceliği



Gerçek case'lere göre ilk conversion coverage sırası:



1\. Parametresiz PL/I procedure → EGL function

2\. Dönüştürülmeyen procedure için diagnostic

3\. Procedure body statement dönüşümü

4\. Parametreli PL/I procedure modeli

5\. EGL function parameter type ve direction çözümlemesi

6\. OPTIONS(MAIN) → EGL program/main mapping

7\. RETURN desteği

8\. Embedded SQL output stratejisi

