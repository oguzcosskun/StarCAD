# gotcha: Presigned URL host'u — konteynerde `minio:9000` tarayıcıdan çözülmez

**Sorun:** API konteynerde koşarken MinIO'ya compose servis adıyla ulaşır
(`MinioSettings:Endpoint = minio:9000`). `PresignedGetObjectAsync` da URL'i bu
host'a göre üretir → `http://minio:9000/starcad-drawings/...`. Bu adresi **tarayıcı
çözemez**; `GET /api/projectversions/{id}/get` 200 döner, çizim indirilemez. Hata
API loglarında görünmez — sorun istemci tarafında, uçta değil.

**Neden URL'i sonradan düzeltemeyiz:** S3 SigV4 imzası `Host` başlığını da
imzalar. `minio:9000` → `localhost:9000` diye string değiştirmek imzayı geçersiz
kılar, MinIO `SignatureDoesNotMatch` döner. Vekil (proxy) arkasına almak da aynı
nedenle çalışmaz, orijinal Host korunmadıkça.

**Doğru:** İmzalama için AYRI bir istemci. `MinioSettings.PublicEndpoint` =
tarayıcının ulaşabildiği adres; boşsa `Endpoint` kullanılır, yani yerelde
`dotnet run` ile koşarken hiçbir şey değişmez.

- `Endpoint` → API'nin MinIO'ya ulaştığı adres (konteynerde `minio:9000`)
- `PublicEndpoint` → presigned URL'e yazılacak adres (`${PUBLIC_HOST:-localhost}:29000`)

Sadece `CreatePresignedGetUrlAsync` `_presignClient`'ı kullanır; put/stat/remove
iç ağdan gider.

`PublicEndpoint` **şemasız** `host:port` olmalı — `http://` ekleme, şemayı
`WithSSL` belirler (Minio .NET `WithEndpoint(string)` şema kabul etmez).

**Prod'da da gerekli:** MinIO iç ağda, tarayıcı dışarıdaki bir alan adı görüyor.
Aynı ayır, aynı iki alan.

## Üç yerde birden host: `localhost` sunucuda çalışmaz

Aynı hatanın üç ayrı yüzü var; biri eksik kalırsa yerelde çalışan kurulum ortak
sunucuda çizimi açamaz. Hepsi tek `.env` değişkeninden (`PUBLIC_HOST`) beslenir:

1. `MinioSettings__PublicEndpoint` — presigned URL'in host'u. Sunucuda `localhost`
   tarayıcının KENDİ makinesi demek; kaydetme çalışır (API içeriden yazar),
   **açma** çöker. Semptom tam olarak budur: yazma tamam, okuma yok.
2. `MINIO_SERVER_URL` / `MINIO_BROWSER_REDIRECT_URL` — MinIO yayınlanan host
   portunu (29000/29001) **bilmez**, kendi iç portunu (9000/9001) bilir; bu ikisi
   verilmezse tarayıcıyı yayınlanmamış porta yönlendirir → "29000'e gidiyorum,
   9000'e atıyor".
3. `appsettings.json:MinioSettings:Endpoint` — API `dotnet run` ile yerelde
   koşarken kullanılır; compose host portu 9000→29000 olunca burası da değişmeli.

Compose'da host'u sabit yazma, `${PUBLIC_HOST:-localhost}` kullan — sabit yazılırsa
her ortam değişiminde dosya commit'lenir (bkz. 432ef5c…349d062 arası gidip gelme).

**Dosya:** StarCAD.DataAccess/Configurations/MinioSettings.cs,
StarCAD.DataAccess/Concrete/MinioStorageManager.cs, docker-compose.yml (`api` servisi).
