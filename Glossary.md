Diagnostic

Parser, Normalizer, Transpiler ve Generator tarafýndan
üretilen bilgi, uyarý veya hata mesajýný temsil eden ortak modeldir.

## Pl1SyntaxTree

PL/I kaynak kodunun syntax tree karþýlýðýdýr.

Parser tarafýndan üretilir, Normalizer tarafýndan düzenlenir ve Transpiler tarafýndan hedef dile dönüþtürülür.

## Lexer

Kaynak kodu küçük ve anlamlý parçalara ayýran bileþendir.

Bu parçalar token olarak adlandýrýlýr.

Örneðin:

DCL MUST_NO FIXED DECIMAL(8);

ifadesi lexer tarafýndan DCL, MUST_NO, FIXED, DECIMAL, (, 8, ), ; token'larýna ayrýlýr.

## Parser

Lexer tarafýndan üretilen token listesini okuyarak anlamlý syntax tree modeli oluþturan bileþendir.

Bu projede PL/I Parser, token listesinden Pl1SyntaxTree üretir.

## Normalizer

Parser tarafýndan üretilen syntax tree modelini, Transpiler aþamasýna gitmeden önce daha standart ve sade hale getiren bileþendir.

Ýlk sürümde minimaldir. Ýleride PL/I yazým varyasyonlarýný tek forma indirmek için geniþletilecektir.

## EglSyntaxTree

Üretilecek EGL kodunun syntax tree karþýlýðýdýr.

Transpiler tarafýndan üretilir ve EGL Code Generator tarafýndan kaynak koda dönüþtürülür.

## Transpiler

Bir kaynak dilin syntax tree modelini hedef dilin syntax tree modeline dönüþtüren bileþendir.

Bu projede ilk Transpiler, Pl1SyntaxTree modelini EglSyntaxTree modeline dönüþtürür.

## Code Generator

Hedef dilin syntax tree modelini gerçek kaynak koda dönüþtüren bileþendir.

Bu projede EglCodeGenerator, EglSyntaxTree modelinden EGL kaynak kodu üretir.

## CHAR / CHARACTER

PL/I tarafýnda sabit uzunluklu karakter alanlarý tanýmlamak için kullanýlan veri tipidir.

Örnek:

``pli
DCL PARAM CHAR(08);
DCL PARAM2 CHARACTER(25);

## Structure Declaration

PL/I tarafýnda seviye numaralarýyla tanýmlanan veri grubu yapýsýdýr.

Örnek:

    DCL 1 PARAME_LIST,
        5 PARAM CHAR(08),
        5 PARAM2 CHAR(01);

LegacyCodeTransformer içinde bu yapý `Pl1StructureDeclaration` modeliyle temsil edilir.

Structure altýndaki alanlar `Pl1StructureMember` modeliyle taþýnýr.

---

## Structure Member

PL/I structure declaration altýnda yer alan field/member alanýdýr.

Örnek:

    5 PARAM CHAR(08)

Bu örnekte:

- Level: 5
- Name: PARAM
- DataType: CHAR(08)

bilgileri `Pl1StructureMember` modeli üzerinde tutulur.

---

## EGL Record Declaration

EGL tarafýnda record tanýmýný temsil eden declaration modelidir.

PL/I structure declaration ifadelerinin EGL karþýlýðý olarak kullanýlýr.

Örnek:

    record ParameList type basicRecord
        10 Param char(8);
        10 Param2 char(1);
    end

LegacyCodeTransformer içinde bu yapý `EglRecordDeclaration` modeliyle temsil edilir.

---

## EGL Record Field Declaration

EGL record içerisinde yer alan field declaration modelidir. 

Örnek:

    10 Param char(8);

LegacyCodeTransformer içinde bu yapý `EglRecordFieldDeclaration` modeliyle temsil edilir.