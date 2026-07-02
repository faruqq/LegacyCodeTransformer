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