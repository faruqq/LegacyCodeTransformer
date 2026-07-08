# Pl1CodingStandards.md

# Amaç

Bu doküman LegacyCodeTransformer projesinde PL/I kaynak kodlarý parse edilirken ve ileride source mapping / formatter davranýþlarý geliþtirilirken temel alýnacak gerçek kurum PL/I kodlama standartlarýný içerir.

Bu dosya mimari karar kaydý deðildir.

Mimari kararlar Decisions.md içinde tutulur.

Bu doküman yalnýzca PL/I kaynak kod formatý, yazým alýþkanlýklarý ve gerçek firma kodlarýndan çýkarýlan pratik kurallar için referans dokümandýr.

---

# Doküman Kullaným Kurallarý

1. Bu doküman PL/I kaynak kodlama standartlarýný toplar.

2. Mimari kararlar bu dosyaya yazýlmaz.

3. Yeni bir standardýn parser, lexer, generator veya test davranýþýný etkilediði durumlarda gerekirse ayrýca Decisions.md içinde karar alýnýr.

4. Gerçek firma PL/I kodlarýndan öðrenilen kurallar zamanla bu dokümana eklenir.

5. Bu dokümandaki örnekler gerçek PL/I kaynak kod formatýna uygun yazýlýr.

---

# Kaynak Kod Satýr Formatý

PL/I kaynak kodlarýnda her satýr 1 adet boþluk karakteri ile baþlar.

Bu boþluk karakterinden sonra kod alaný gelir.

Standart:

- Ýlk karakter boþluk olmalýdýr.
- Yazýlabilir kod alaný en fazla 72 karakterdir.
- Ýlk boþluk karakteri dahil fiziksel satýr uzunluðu en fazla 73 karakterdir.

Bu kural lexer ve parser geliþtirmelerinde temel kabul edilir.

---

# Procedure Yazým Standardý

Firma PL/I kodlarýnda procedure tanýmlarý temel olarak aþaðýdaki yapýdadýr.

    PROCEDURE_NAME: PROCEDURE;

        ...

    END PROCEDURE_NAME;

Procedure adý label olarak kullanýlýr.

END ifadesinde procedure adýnýn tekrar edilmesi beklenir.

---

# Procedure Ýçeriði

Firma PL/I kodlarýnda procedure'ler çoðunlukla business logic içerir.

Parametre ve deðiþken declaration bilgileri genellikle procedure içinde deðil, dosyanýn baþýndaki global declaration bölümünde yer alýr.

Bu nedenle P06 ilk kapsamýnda procedure modeli sade tutulacaktýr.

Ýlk kapsamda bilinçli olarak oluþturulmayacak modeller:

- Pl1ProcedureParameter
- Pl1ProcedureBody
- Local declaration scope modeli

---

# CALL Yazým Standardý

Procedure çaðrýlarý CALL statement ile yapýlýr.

Temel kullaným:

    CALL PROCEDURE_NAME;

Parametreli kullaným mevcut CALL statement parser kapsamý üzerinden deðerlendirilecektir.

Örnek:

    CALL PROCEDURE_NAME(PARAM1, PARAM2);

---

# Geniþletilecek Baþlýklar

Bu doküman ileride aþaðýdaki baþlýklarla geniþletilecektir.

- DCL yazým standardý
- INIT kullanýmý
- Level number kurallarý
- Structure declaration alýþkanlýklarý
- Array declaration alýþkanlýklarý
- Statement yazým standardý
- Comment yazým standardý
- SELECT kullanýmý
- DO kullanýmý
- EXEC SQL kullanýmý
- SQLCA kullanýmý
- Batch / CICS farklarý
- Firma kodlama alýþkanlýklarý
- Gerçek PL/I örneklerinden çýkarýlan parser kurallarý