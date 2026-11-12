# convention: Tek tip NVARCHAR + her string'e uzunluk

Bütünlük kararı: **tüm metinsel alanlar tek tip `NVARCHAR`** (VARCHAR/NVARCHAR ayrımı yok).

## Kurallar

- Her string property config'te **`HasMaxLength(n)`** alır — `StarCAD_Database.sql`'de kolon kolon
  zaten tanımlı, mekanik olarak Fluent API'ye taşınır.
- **Asla `NVARCHAR(MAX)`.** EF, konfigüre edilmemiş bir `string`'i varsayılan olarak
  `nvarchar(max)` üretir; bu sınırsız uzunluk + o kolona UNIQUE/index atılamaması demektir
  (bkz. [[schema-config-gap]] — mevcut migration bu tuzağa düştü).
- Binary/şifreli veri string değildir: `ProjectFirm.NationalIdNumber` → `VARBINARY`.

## Neden

Türkçe serbest metin riski sıfırlanır, ileride çok dilli/emoji desteği açık kalır, iki string
tipi ile uğraşmak yerine tek kural. Index/performans için uzunluk şart.
