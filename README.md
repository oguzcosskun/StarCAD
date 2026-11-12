# StarCAD

> **About this repository.** Backend of a web-based CAD application for natural-gas indoor installation projects, built by the 4-person backend team of an 8-person intern group during my internship at Tekhnelogos (July–August 2026). Published with the company's permission; credentials, internal hosts and company infrastructure references have been removed, and the firm and user seed data is fictional.
>
> Project screenshots: [ProjeGörselleri.pdf](ProjeGörselleri.pdf)
>
> **My contributions**
> - Core layer: `BaseEntity` / `SoftDeleteEntity` split and the `Result` pattern; removed the generic repository layer in favour of using `DbContext` directly
> - `ProjectFirm` and `Project` CRUD APIs (EF Core configuration, manager services, controllers)
> - Encrypted national ID storage for individual firms (AES-GCM + HMAC for uniqueness), with fail-fast key configuration
> - Role-based authorization: firm reads scoped by role, writes restricted to admins; project firm authorization bound to gas distribution regions
> - Refactors: project status moved to the code-group model, Turkish identifiers renamed to English
> - README overhaul, Scalar auto-launch, and the `.claude/` AI-assistant configuration (CLAUDE.md, knowledge base, skills)

Doğalgaz iç tesisat projelendirme API'si — **.NET 10 · ASP.NET Core · EF Core (SQL Server) · MinIO ·
Redis · Elasticsearch · Scalar**. Geleneksel N-Layer mimari (8 proje).

| Proje | Rol |
|---|---|
| `StarCAD.Api` | Controller'lar, `Program.cs`, Scalar/OpenAPI |
| `StarCAD.Business` | İş servisleri (Manager), FluentValidation, JWT, MinIO, Redis cache, Outbox worker |
| `StarCAD.DataAccess` | EF Core (`StarCADContext`, Migrations), MinIO, Redis, Elasticsearch client |
| `StarCAD.Entity` / `StarCAD.Core` | POCO entity'ler / temel sınıflar (Result, arayüzler) |
| `StarCAD.ViewModel` / `StarCAD.Common` | DTO'lar / sabitler, mesajlar |
| `StarCAD.Tests` | xUnit + Moq |

---

## İçindekiler

1. [Gereksinimler](#1-gereksinimler)
2. [Hangi kurulum? (karar)](#2-hangi-kurulum-karar)
3. [Ön hazırlık (her iki yolda ortak)](#3-ön-hazırlık-her-iki-yolda-ortak)
4. [Kurulum A — Tamamen Docker](#4-kurulum-a--tamamen-docker)
5. [Kurulum B — API'yi yerelde koştur](#5-kurulum-b--apiyi-yerelde-koştur)
6. [Doğrulama](#6-doğrulama)
7. [Kimlik doğrulama & test kullanıcıları](#7-kimlik-doğrulama--test-kullanıcıları)
8. [Mimari: arama, önbellek, outbox worker](#8-mimari-arama-önbellek-outbox-worker)
9. [Günlük kullanım](#9-günlük-kullanım) · [9.1 Yerel veritabanını sıfırlama](#91-yerel-veritabanını-sıfırlama)
10. [Sorun giderme (SSS)](#10-sorun-giderme-sss)
11. [Yapılandırma notları](#11-yapılandırma-notları)

---

## 1. Gereksinimler

- **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** — SQL Server + MinIO + Redis +
  Elasticsearch/Kibana (ve istenirse API) için. Elasticsearch tek başına en az **512MB heap**
  ister (`ES_JAVA_OPTS`); Docker Desktop'a ayrılan toplam bellek **en az 4GB** olmalı, yoksa
  `elasticsearch` container'ı healthcheck'i geçemeden takılı kalabilir.
- **Git** — repoyu klonlamak için.
- **Yalnızca API'yi yerelde koşturacaksan:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  ve EF CLI: `dotnet tool install --global dotnet-ef`
- **openssl** — şifreleme anahtarı üretmek için. macOS/Linux'ta **yerleşik**; Windows'ta **Git Bash** ile gelir.

> **Kabuk / işletim sistemi notu.** Buradaki komutlar **bash** içindir.
> - **macOS / Linux:** varsayılan terminal (bash/zsh) yeter.
> - **Windows:** komutları **Git Bash**'te çalıştır (`openssl` ve `$(...)` orada çalışır). PowerShell
>   kullanıyorsan anahtar üretme için [alternatif](#32-şifreleme-anahtarları-zorunlu) verilmiştir.
> - `docker` ve `dotnet` komutları her OS/kabukta aynıdır.

---

## 2. Hangi kurulum? (karar)

| Senaryon | Seç | Neden |
|---|---|---|
| İlk kez kuruyorum / hızlıca deneyeceğim / demo | **A — Tamamen Docker** | Tek komut, local'e SQL/MinIO/Redis/Elastic kurmak yok |
| Kod geliştireceğim / debug (breakpoint) / günlük iş | **B — API'yi yerelde koştur** | IDE'de F5, hot reload, debugger |

İkisi de eşit meşru — altyapı (SQL Server + MinIO + Redis + Elasticsearch/Kibana) **her iki yolda da
Docker'da** çalışır; fark sadece **API'nin** konteynerde mi yoksa yerelde mi koştuğudur.

---

## 3. Ön hazırlık (her iki yolda ortak)

### 3.1 Repoyu klonla
```bash
git clone https://github.com/oguzcosskun/StarCAD.git
cd StarCAD
```

### 3.2 Şifreleme anahtarları (zorunlu)
Şahıs firma TC kimliği KVKK gereği **şifreli** (AES-GCM) saklanır; benzersizlik için **HMAC**'lenir.
Bu iki anahtar ortamdan gelir — **iki adet base64, 32-byte anahtar** gerekir:

| Çalıştırma | Anahtar nereye konur |
|---|---|
| **A — Docker** | `.env` dosyası (`ENCRYPTION_KEY`, `ENCRYPTION_HMAC_KEY`) — compose bunu okur |
| **B — Yerel** | **user-secrets** (`Encryption:Key`, `Encryption:HmacKey`) |

Anahtar üretme komutu:
- **Git Bash / macOS / Linux:** `openssl rand -base64 32`
- **PowerShell (openssl yoksa):**
  ```powershell
  [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
  ```

> ⚠️ **Anahtar sabit kalmalı.** Değişirse o anahtarla şifrelenmiş TC'ler bir daha **çözülemez**.
> Ortam başına (her dev makinesi / sunucu) bir kez üret, sabit tut. Anahtar eksik/geçersizse API
> **açılışta net hata verip durur** (ilk istekte gizemli 500 beklemezsin).

---

## 4. Kurulum A — Tamamen Docker

**1) `.env` hazırla** (repo kökünde). Yoksa şablondan kopyala ve doldur:
```bash
cp .env.example .env      # zaten bir .env varsa atla
```
```ini
# .env
ENCRYPTION_KEY=<openssl rand -base64 32 çıktısı>
ENCRYPTION_HMAC_KEY=<openssl rand -base64 32 çıktısı>
PUBLIC_HOST=              # YEREL: boş bırak (localhost). Ortak sunucuda: sunucu ip/hostname
```

**2) Ayağa kaldır:**
```bash
docker compose up -d --build
```
SQL Server, MinIO, Redis ve Elasticsearch sağlıklı olunca API kalkar (Kibana yalnızca Elasticsearch'e
bağımlı, healthcheck'i beklemez — API'nin ayağa kalkması için gerekli değildir). Migration'lar
açılışta otomatik uygulanır (ayrıca `database update` gerekmez).

**Servisler:**

| Servis | Adres | Kimlik |
|---|---|---|
| API | `http://localhost:5193` — Scalar: `/scalar` | — |
| SQL Server | `localhost:1433` | `sa` / `StarCAD_Local_123!` |
| MinIO S3 | `localhost:29000` | `minioadmin` / `minioadmin` |
| MinIO konsol | `http://localhost:29001` | `minioadmin` / `minioadmin` |
| Redis | `localhost:16379` | — (parolasız, yalnız yerel dev) |
| Elasticsearch | `http://localhost:9200` | — (güvenlik kapalı, yalnız yerel dev) |
| Kibana | `http://localhost:5601` | — |

> MinIO host portları **29000/29001** (konteyner içi 9000/9001), Redis host portu **16379**
> (konteyner içi 6379) — hepsi yerel makinede zaten kullanımda olabilecek varsayılan portlarla
> (1433/6379/9200) çakışmayı önlemek için kaydırıldı; SQL Server ve Elasticsearch varsayılan
> portlarında kaldı. Ortak bir sunucuda koşuyorsan `.env`'e `PUBLIC_HOST=<sunucu ip/hostname>` ekle
> — yoksa presigned URL `localhost` ile üretilir ve başka makinedeki tarayıcı dosyayı indiremez.

→ [6. Doğrulama](#6-doğrulama)'ya geç.

---

## 5. Kurulum B — API'yi yerelde koştur

Altyapı Docker'da, API yerelde (IDE'de debug için).

**1) Altyapıyı kaldır:**
```bash
docker compose up -d sqlserver minio redis elasticsearch
```

**2) Anahtarları user-secrets'a set et** (`.env` yerel `dotnet run`'a geçmez — bir kez):
```bash
# Git Bash / macOS / Linux:
dotnet user-secrets set "Encryption:Key"     "$(openssl rand -base64 32)" --project StarCAD.Api
dotnet user-secrets set "Encryption:HmacKey" "$(openssl rand -base64 32)" --project StarCAD.Api
```
```powershell
# PowerShell (openssl yoksa):
dotnet user-secrets set "Encryption:Key"     ([Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))) --project StarCAD.Api
dotnet user-secrets set "Encryption:HmacKey" ([Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))) --project StarCAD.Api
```

**3) Veritabanını hazırla:**
```bash
dotnet ef database update --project StarCAD.DataAccess --startup-project StarCAD.Api
```

**4) Çalıştır** (iki yoldan biri):
- **Komut satırı:** `dotnet run --project StarCAD.Api`
- **Visual Studio / Rider:** başlangıç projesi = **`StarCAD.Api`**, profil **`http`** (5193), **F5**.
  → Tarayıcı **otomatik `http://localhost:5193/scalar`'a açılır** (launchSettings ayarı).
  > VS profil menüsünden **`http`** seç; `https` seçersen 7144'te açılır (çalışır ama dev-sertifika uyarısı verebilir).

> **Port çakışması:** Docker'daki `api` ile yereldeki API **aynı anda** 5193'ü ister. Yerelde
> koşacaksan önce: `docker compose stop api`.
>
> **Yerel çalışırken bağlantı ayarları:** `appsettings.Development.json`'da `RedisSettings:Endpoint`
> ve `ElasticSettings:Endpoint` — Docker'daki gibi `redis:6379`/`http://elasticsearch:9200` değil,
> host'tan görünen adresler olmalı (`localhost:16379`, `http://localhost:9200`); container adları
> yalnızca konteynerler arası ağda çözülür.
>
> **Not:** `dotnet ef` komutları anahtar gerektirmez (design-time host açılışı tetiklemez); anahtar
> yalnız **uygulamayı çalıştırınca** lazım.

---

## 6. Doğrulama

```bash
curl http://localhost:5193/api/health          # 200 dönmeli
```
- Tarayıcıda **`http://localhost:5193/scalar`** — API dokümanı açılır (VS'te F5 ile otomatik gelir).
- Login testi:
  ```bash
  curl -X POST http://localhost:5193/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"admin","password":"Admin1234"}'      # token dönmeli
  ```

---

## 7. Kimlik doğrulama & test kullanıcıları

**API fail-closed'dır:** `[AllowAnonymous]` olmayan her uç token ister. Token'sız `401`, yetkisiz rol `403`.

`DevelopmentDataSeeder` (yalnız Development, DB boşsa) uçtan uca test edilebilir gerçekçi hacimli bir
fixture kurar: 3 rol, 7 dağıtım grubu, 12 dağıtım firması, 20 proje firması, ~40 kullanıcı ve durum
akışına (taslak/onay bekliyor/onaylandı/reddedildi) dağılmış ~80 proje — bkz.
[8. Mimari](#8-mimari-arama-önbellek-outbox-worker) ve `.claude/knowledge/seed-order-dependency.md`.
Rol bazında ortak şifre kullanılır (manuel test/demo girişini kolaylaştırmak için); hızlı giriş için:

| Kullanıcı | Şifre | Rol | Firma |
|---|---|---|---|
| `admin` | `Admin1234` | `Admin` | — |
| `pf01` | `Proje1234` | `ProjectFirmUser` | 1. proje firması (Anka Mühendislik Ltd. Şti.) |
| `gd01` | `Dagitim1234` | `GasDistributionUser` | 1. dağıtım firması (Aksa Çorum Doğalgaz Dağıtım A.Ş.) |

Diğer proje firması kullanıcıları `pf02`…`pf24`, dağıtım firması kullanıcıları `gd02`…`gd13` — hepsi
kendi rolünün ortak şifresiyle (`Proje1234` / `Dagitim1234`); ek yönetici hesapları `admin2`, `admin3`
(`Admin1234`). Tam kullanıcı/firma listesi ve üretim mantığı için
[`DevelopmentDataSeeder.cs`](StarCAD.DataAccess/Seed/DevelopmentDataSeeder.cs).

**Kullanım:**
1. `POST /api/auth/login` → `{ "username": "admin", "password": "Admin1234" }`
2. Dönen `token`'ı header'a ekle: `Authorization: Bearer <token>`
3. `GET /api/auth/me` ile doğrula.

Scalar'da: sağ üstteki **Authorize** panelinden `admin`/`Admin1234` ile giriş yap (OAuth2 password
flow ön-dolu) → token otomatik eklenir. Kullanıcı oluşturma (`POST /api/auth/register`) yalnız `Admin`.

---

## 8. Mimari: arama, önbellek, outbox worker

Kod tarafı üç ek altyapı bileşeni kullanır — hepsi `StarCAD.DataAccess/Extensions/ServiceRegistration.cs`
üzerinden DI'ya kaydedilir:

### 8.1 Elasticsearch — tam metin arama
- `ISearchService` (`StarCAD.Core.Search`) → `ElasticSearchManager` (`StarCAD.DataAccess`) somutlaştırması;
  Manager'lar somut Elastic SDK'sına değil bu arayüze bağımlıdır (DIP).
- Dört index: `projects`, `project-firms`, `gas-distribution-firms`, `users`. Şema
  `ElasticSearchManager.IndexMappings` içinde sabit (Türkçe analyzer'lı `text` alanlar + `keyword`
  filtre alanları); index yoksa ilk yazımda otomatik oluşturulur.
- Liste uçlarındaki serbest metin arama parametresi (`Project`, `ProjectFirm`, `GasDistributionFirm`,
  `User` listelerinde) önce Elasticsearch'te ID araması yapar, sonra o ID'lerle SQL'den asıl veriyi
  çeker — arama motoru **yalnızca ID döndürür**, kayıt içeriği hep SQL'den gelir (bkz.
  `StarCAD.Business.Extensions.SearchQueryExtensions`).
- Elasticsearch'e yazılan doküman şeması ilgili `*ListItemDto`'yla **birebir aynı alan seti** —
  kullanıcı listede neyi görüyorsa aramada da onu bulabilmeli. `User` dokümanında `PasswordHash`
  **bilerek** projekte edilmez.

### 8.2 Outbox worker — SQL ile arama index'ini senkron tutma
- `OutboxMessage` (append-only, soft-delete'e tabi değil) tablosu: `Create`/`Update`/`Delete`
  yapan Manager'lar aynı transaction'da (SQL yazımıyla birlikte) bir outbox satırı ekler.
- `OutboxProcessor` (`StarCAD.Business`, `BackgroundService`) her **5 saniyede** işlenmemiş
  (`ProcessedAt == null`) satırları **20'lik batch**'ler halinde okur, ilgili entity'yi SQL'den
  taze çeker, Elasticsearch'e `Index`/`Delete` eder, sonra `ProcessedAt` işaretler.
  - `Delete` operasyonu ya da SQL'de artık bulunamayan/soft-delete olmuş kayıt → index'ten silinir.
  - Bir mesaj işlenirken hata alırsa `ProcessedAt` **null kalır**, bir sonraki turda tekrar denenir
    (en-az-bir-kez teslim; idempotent — aynı ID'yi tekrar index'lemek zararsız).
  - Scoped `StarCADContext`'i tüketmek için her turda `IServiceScopeFactory` ile yeni scope açar
    (singleton `BackgroundService`'in scoped bağımlılık tüketmesinin tek doğru yolu).
- `DevelopmentDataSeeder` toplu eklediği kayıtlar için de aynı outbox kuyruğuna mesaj yazar
  (`EnqueueSearchBackfillAsync`) — seed verisi de normal bir `Create` ile aynı yoldan indekslenir,
  ayrı bir "seed'i indeksle" kodu yoktur.
- Yeni bir entity'yi aramaya dahil etmek için: `ISearchService`'e index eklenmez, sadece
  `ElasticSearchManager.IndexMappings`'e yeni index şeması + `OutboxProcessor.ProcessMessageAsync`'e
  yeni `case` eklenir; Manager tarafında ilgili outbox satırını yazma sorumluluğu değişmez.

### 8.3 Redis — salt okunur master veri önbelleği
- `IConnectionMultiplexer` (StackExchange.Redis) singleton olarak kaydedilir; kullanan Manager'lar
  (`CityManager`, `CodeManager`, `CodeGroupManager`, `RoleManager`, `GasDistributionGroupManager`)
  `_redis.GetDatabase()` ile doğrudan okur/yazar — ayrı bir cache soyutlama katmanı yok.
- Yalnızca **nadiren değişen, ID/tenant'a göre büyük ölçüde sabit master/lookup veri** cache'lenir
  (il/ilçe, kod grupları, roller, dağıtım grupları) — proje/kullanıcı gibi sık değişen veri Redis'e
  **girmez**, doğrudan SQL'den (gerekiyorsa Elasticsearch ID'siyle) okunur.
- Anahtarlar `StarCAD.Common.Constants.CacheKeys`'te merkezi tanımlı (`cities:list`,
  `cities:{id}:districts`, `codegroups:list`, `codes:group:{id}`, `gasdistributiongroups:list`);
  TTL genelde **24 saat**, ayrıca aktif bir invalidation/eviction stratejisi yok — veri güncellenince
  cache kendiliğinden temizlenmez, süresi dolana kadar bayat kalabilir (master veri için kabul
  edilebilir görülmüş bir trade-off).

---

## 9. Günlük kullanım

```bash
docker compose stop / start        # servisleri durdur / başlat (veri kalıcı)
docker compose logs -f api         # API loglarını izle
docker compose logs -f elasticsearch   # ES ayağa kalkmıyorsa ilk bakılacak yer
docker compose up -d --build api   # API kodu değişince yeniden derle
docker compose down -v             # servisleri VE verileri tamamen sil

# Migration:
dotnet ef migrations add <Ad> --project StarCAD.DataAccess --startup-project StarCAD.Api
dotnet ef database update      --project StarCAD.DataAccess --startup-project StarCAD.Api
```

> `docker compose down -v` üç adlandırılmış volume'u siler: `mssql-data`, `minio-data`, `es-data`.
> Redis **kalıcı volume kullanmaz** — `docker compose restart redis` (veya container'ın herhangi bir
> nedenle yeniden başlaması) cache'i sıfırlar; zararsızdır, sadece master veri sorguları bir sonraki
> istekte SQL'den tazelenip yeniden cache'lenir.

### 9.1 Yerel veritabanını sıfırlama

Migration geçmişi **squash'landığında** (repodaki migration'lar tek bir
`InitialCreate`'e toplandığında) mevcut yerel veritabanları EF'e "hiç migration
uygulanmamış" gibi görünür: EF zaten var olan tabloları yeniden oluşturmaya
çalışır ve API **açılışta düşer**.

Uygulama bunu saptayıp ne yapmanız gerektiğini logda yazar. Belirti frontend'de
`net::ERR_CONNECTION_RESET` olarak görünür — ağ sorunu değildir, arkada dinleyen
kimse kalmamıştır. ⚠️ `docker ps` yine de **"Up"** gösterir, çünkü süreç
`app.Run()`'a hiç gelmeden düşer; `docker compose logs api` bakılacak yerdir.

**Yalnız SQL verisini siler; MinIO (çizimler, evraklar), Elasticsearch index'lerine ve Redis'e dokunmaz:**

```bash
docker stop starcad-api
docker exec starcad-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
  -P 'StarCAD_Local_123!' -C -Q \
  "ALTER DATABASE [StarCAD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [StarCAD];"
docker start starcad-api
```

Açılışta migration'lar temiz uygulanır ve seeder örnek veriyi yeniden kurar
([7. bölümdeki test kullanıcıları](#7-kimlik-doğrulama--test-kullanıcıları) dahil); yeniden eklenen
kayıtlar outbox üzerinden birkaç saniye içinde Elasticsearch'e yeniden indekslenir (bkz.
[8.2](#82-outbox-worker--sql-ile-arama-indexini-senkron-tutma)). Elasticsearch'te eski çalıştırmadan
kalan dokümanlar (artık SQL'de karşılığı olmayan ID'ler) sahipsizleşir; yerelde zararsızdır, tamamen
temiz başlangıç isteyen `docker compose down -v` kullanabilir (o Elasticsearch/MinIO'yu DA siler).

> **Migration geçmişini yeniden yazarken:** bu, herkesin yerel ortamını bozan bir
> değişikliktir. Ayrı bir commit'te yap, mesaja sonucu ve bu bölümün bağlantısını
> yaz, ekibe haber ver — "squash'ladım" ifadesi tek başına okuyanı harekete
> geçirmiyor.

---

## 10. Sorun giderme (SSS)

| Belirti | Sebep / Çözüm |
|---|---|
| SQL bağlantısı reddedildi / *"Sunucu bulunamadı"* | Altyapı container'ları ayakta değil → `docker compose up -d sqlserver minio redis elasticsearch`. |
| `openssl: command not found` | Windows'ta **Git Bash** kullan; ya da PowerShell anahtar üretme alternatifini ([3.2](#32-şifreleme-anahtarları-zorunlu)) kullan. |
| Açılışta *"Encryption:Key ve Encryption:HmacKey ayarlanmalı"* | Anahtar eksik. Docker'da `.env`, yerelde user-secrets doldurulmalı ([3.2](#32-şifreleme-anahtarları-zorunlu)). |
| `Failed to bind... address already in use` (5193) | Docker `api` + yerel API aynı anda çalışıyor → `docker compose stop api`. |
| Scalar'da istek `401` alıyor (token'ım vardı) | Token'ın süresi doldu / bayat. Auth panelinden **yeniden Authorize** et, ya da Scalar'ı gizli pencerede aç. |
| `elasticsearch` container'ı healthcheck'i geçemiyor / sürekli restart | Genelde bellek: Docker Desktop'a ayrılan RAM'i artır (bkz. [1. Gereksinimler](#1-gereksinimler)). Linux host'ta ayrıca `vm.max_map_count` en az `262144` olmalı: `sudo sysctl -w vm.max_map_count=262144`. |
| Arama sonuçları boş / yeni eklenen kayıt aramada çıkmıyor | Outbox worker 5 saniyede bir çalışır — birkaç saniye bekle. Sürüyorsa `docker compose logs -f api` içinde `OutboxProcessor` uyarılarına bak; kalıcıysa index'i silip yeniden oluşturmak için ilgili kaydı tekrar kaydet (yeni outbox mesajı üretir). |
| Docker'a bağlanamıyorum | Docker Desktop açık mı? Kapalıysa aç, sonra `docker compose up -d`. |
| Açılışta *"Yerel veritabanınız bu koddaki migration setiyle UYUŞMUYOR"* | Migration geçmişi squash'lanmış; veritabanınız eski setle kurulmuş → [9.1 Yerel veritabanını sıfırlama](#91-yerel-veritabanını-sıfırlama). |
| **Frontend'de `net::ERR_CONNECTION_RESET`** (`localhost:5193`) | Ağ/CORS sorunu DEĞİL: API açılışta düşmüş, 8080'i dinleyen kimse yok. ⚠️ `docker ps` yine "Up" gösterir — `docker compose logs api` ile gerçek sebebe bak (çoğunlukla yukarıdaki migration uyuşmazlığı). |

---

## 11. Yapılandırma notları

- `MinioSettings:PublicEndpoint` — presigned URL'e yazılacak host. Konteynerde API MinIO'ya
  `minio:9000` ile ulaşır ama o adı tarayıcı çözemez; compose bu yüzden `${PUBLIC_HOST:-localhost}:29000`
  geçer (şema `http://` yazılmaz, sadece `host:port`). Bkz. `.claude/knowledge/minio-presign-host.md`.
- `PUBLIC_HOST` (`.env`) — tarayıcının gördüğü host. Hem `MinioSettings:PublicEndpoint`'i hem MinIO'nun
  `MINIO_SERVER_URL` / `MINIO_BROWSER_REDIRECT_URL` ayarlarını besler. Yerelde boş (localhost); ortak
  sunucuda sunucu ip/hostname.
- **MinIO host portları 29000/29001** (konteyner içi 9000/9001), **Redis host portu 16379**
  (konteyner içi 6379).
- `RedisSettings:Endpoint` / `ElasticSettings:Endpoint` — konteynerde (`api` servisi) sırasıyla
  `redis:6379` / `http://elasticsearch:9200`; yerelde çalışan API için host'tan görünen adresler
  kullanılmalı (bkz. [5. Kurulum B](#5-kurulum-b--apiyi-yerelde-koştur)).
- `ElasticSettings:MaxSearchResults` — arama sorgularının döndürebileceği azami ID sayısı (varsayılan
  1000); index mapping'i `ElasticSearchManager.IndexMappings` içinde koddadır, ayrı bir config dosyası
  yoktur.
- HTTPS yönlendirmesi yalnız Development **dışında** açıktır (dev'de CORS preflight sorununu önlemek için).
