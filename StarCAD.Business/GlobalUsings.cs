// StatusCodes.* (Status404NotFound vb.) Business genelinde kullanılır; Api katmanıyla aynı isimli
// sabitler. Business web SDK olmadığı için implicit gelmez. Tüm Microsoft.AspNetCore.Http namespace'i
// alınırsa onun IResult'ı bizim StarCAD.Core...IResult ile çakışır (CS0104) — bu yüzden yalnız
// StatusCodes tipi alias'la alınır.
global using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
