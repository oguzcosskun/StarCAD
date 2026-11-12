# decision: TC kimlik (KVKK) — AES-GCM şifreleme + HMAC hash + fail-fast anahtar

Şahıs firmanın (`ProjectFirm.CompanyType = 1`) TC kimlik numarası KVKK gereği düz saklanmaz.
`INationalIdProtector` (Core/Security arayüz) + `NationalIdProtector` (Business/Security) ile
korunur. Tüzel firma (CompanyType = 2) TC değil `TaxNumber` kullanır — şifreleme yok.

## İki kolon, iki amaç (ikisi birden gerekir)

- **`ProjectFirm.NationalIdNumber`** (`VARBINARY`) — AES-GCM ile **geri döndürülebilir** şifreli
  paket. Düzen: `[12 byte nonce][ciphertext][16 byte tag]`. Anahtar 32 byte (AES-256).
- **`ProjectFirm.NationalIdNumberHash`** — HMAC-SHA256 **deterministik** hash. Neden ayrı:
  şifreli paket her yazımda rastgele nonce içerir → aynı TC her seferinde **farklı** ciphertext
  üretir, üstüne UNIQUE index atılamaz. Benzersizlik (aynı TC iki kez girilemesin) hash kolonunda
  sağlanır: aynı TC → aynı hash → `UNIQUE`/`AnyAsync(hash == ...)` çalışır. Hash tek yön (geri
  çözülmez); şifre çözme yalnız AES kolonundan.

## Okuma — asla düz dönme

Detay/okuma yolunda TC **maskeli** döner: `Mask()` son 4 haneyi bırakır, gerisi `*`
(`*******0078`). Düz TC hiçbir response'ta dönmez; `Decrypt()` yalnız maskeleme öncesi iç adımda.

## Anahtarlar — config'ten, fail-fast

- `Encryption:Key` + `Encryption:HmacKey` (base64). Kaynak: **Docker → `.env`**, **yerel dev →
  user-secrets**, prod → env/CI variable. Koda **gömülmez**, commit'lenmez.
- `ServiceRegistration`'da `AddSingleton<INationalIdProtector>` factory'si anahtarlar boşsa
  `InvalidOperationException` fırlatır (README'ye yönlendiren mesajla). `NationalIdProtector` ctor'u
  ayrıca uzunluk doğrular (enc = 32, hmac ≥ 32 byte).
- **Fail-fast ama EF-safe:** `Program.cs`'te `var app = builder.Build();` **sonrasında**
  `_ = app.Services.GetRequiredService<INationalIdProtector>();` ile anahtar eager resolve edilir →
  anahtar eksikse uygulama açılışta net mesajla çöker. Bu satır **`Build()` sonrasında** olmalı:
  EF design-time (migration) `Build()`'e kadar çalışıp durur, oraya ulaşmaz → `dotnet ef` anahtar
  istemez. `AddBusinessServices` içine koyma (design-time patlar).

## Kurallar

- Yeni bir "hassas alan şifreleme" ihtiyacında bu deseni izle: AES-GCM (paket düzeni aynı) +
  gerekiyorsa benzersizlik için ayrı HMAC hash kolonu. Düz saklama / geri-çözülür "encoding" (base64
  vb.) şifreleme değildir.
- Anahtarı koda/appsettings'e gömme; config anahtarı (`Encryption:*`) üzerinden oku.
- Maskeleme dışında TC'yi response/log'a yazma.
