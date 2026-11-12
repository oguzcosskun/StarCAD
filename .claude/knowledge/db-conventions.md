# decision: dbo + çoğul tablo adı; elle mapping (şema/AutoMapper benimsenmedi)

## Tablo adlandırma — dbo + çoğul

- **`dbo` şeması + çoğul tablo adı** (EF varsayılanı). `builder.ToTable("ProjectFirms")` — şema
  argümanı **YOK**.
- Modül şeması (`common`/`user`/`company`/`project`/`document`) **benimsenmedi.**
- **Neden:** ≤30 tablo (küçük DB) + **tek uygulama DB hesabı** → şemanın gerçek getirisi
  (şema-bazlı `GRANT`, dev DB'de gruplama) yok. Ayrıca `main` zaten dbo/çoğul (merge olmuş
  ProjectVersion/Policy config'leri + fiili DB böyle kuruldu). Şemaya geçmek retrofit + koordinasyon
  maliyeti getirir, karşılığı yok.
- **Mantıksal gruplama kodda yaşar** (DB'de şema olarak değil): namespace'ler, `StarCADContext`'teki
  DbSet yorum grupları (`// Auth`, `// Lookup`, `// Firm`…), ViewModel feature klasörleri.
- Tasarım docs/`sema.dbml` şema + tekil ad gösterir — o **yalnızca diyagram/doküman düzeni**;
  koda taşınmaz. Config yazarken **şema EKLEME**, tekil ada çevirme.

## Mapping — elle

- **AutoMapper kullanılmıyor** (paket referanslı olsa da). Mapping **elle**: okumada `.Select()`
  projeksiyon, yazmada `new Entity{}` / `new Dto{}`. Bkz. [[manager-service]].

## Config dosya yeri — düz

- `StarCAD.DataAccess/Concrete/EntityFramework/Configurations/` — **düz** (modül alt-klasörü yok),
  mentör + ekip deseniyle ayni. Bkz. [[ef-core-config]].

**How to apply:** Yeni config: `ToTable("Xxxs")` (dbo/çoğul, şema yok). Yeni Manager: elle mapping,
AutoMapper enjekte etme. Tüm ekip bu konvansiyona uyar (yarısı şema/yarısı dbo = çorba).
