# Bilgi Tabanı (Knowledge Index)

Her mimari karar ve tuzak bir satır. Yeni oturumda ilgili olanları oku.
Yeni karar/tuzak çıkınca buraya satır ekle + dosyasını yaz.

Tür: `decision` (neden öyle) · `gotcha` (sessizce bozan tuzak) ·
`convention` (uyum kuralı) · `open-question` (henüz karar verilmedi — varsayarak kod yazma)

| Tarih | Tür | Konu | Özet |
|-------|-----|------|------|
| 2026-07 | decision | [generic-repo-removed](./generic-repo-removed.md) | Generic repo kaldırıldı; Manager → `StarCADContext` doğrudan, `Result` döner |
| 2026-07 | gotcha | [cascade-restrict](./cascade-restrict.md) | EF zorunlu FK'yi Cascade üretir → her FK'ye açık `Restrict`; SetNull istisnası `Doc.UploadedByUserId` |
| 2026-07 | convention | [nvarchar-length-caps](./nvarchar-length-caps.md) | Tek tip NVARCHAR + her string'e `HasMaxLength`; asla `NVARCHAR(MAX)`; binary → VARBINARY |
| 2026-07 | decision | [single-role](./single-role.md) | `User.RoleId` = tek rol; UserRole N:N kaldırıldı |
| 2026-07 | decision | [minio-canvas-json](./minio-canvas-json.md) | Çizim tek JSON nesnesi MinIO'da; SQL sadece `JsonToken`; autosave yok + retention |
| 2026-07 | decision | [project-audit-exceptions](./project-audit-exceptions.md) | `Project.CreatedByUserId`/`UpdatedByUserId` = §8'in bilinçli audit istisnaları |
| 2026-07 | decision | [db-conventions](./db-conventions.md) | `dbo` + çoğul tablo adı (modül şeması YOK); config düz klasörde; elle mapping (AutoMapper yok) |
| 2026-08 | gotcha | [minio-presign-host](./minio-presign-host.md) | Presigned URL host'u imzaya dahil: `minio:9000` tarayıcıdan çözülmez → ayrı `PublicEndpoint`; sunucuda host `${PUBLIC_HOST}` + `MINIO_SERVER_URL`/`BROWSER_REDIRECT_URL` şart |
| 2026-07 | convention | [schema-config-gap](./schema-config-gap.md) | Config kapatma büyük ölçüde bitti (tüm tablolar config'li); yeni tablo eklenince config'i de aynı feature'da yazılır |
| 2026-08 | convention | [query-performance](./query-performance.md) | Include yerine `.Select()` projeksiyon; Include bilinçli; debug'da SQL'e bakıp N+1 yakala |
| 2026-08 | decision | [national-id-encryption](./national-id-encryption.md) | Şahıs TC: AES-GCM şifreli `VARBINARY` + ayrı HMAC hash (benzersizlik); maskeli okuma; anahtar config'ten, `Build()` sonrası fail-fast (EF-safe) |
| 2026-08 | gotcha | [seed-order-dependency](./seed-order-dependency.md) | `Program.cs` seed sırası: Migrate → MasterDataSeeder (il/ilçe) → DevelopmentDataSeeder; ters sırada Building.City boş kalır |
| 2026-08 | decision | [unit-sync-hard-delete](./unit-sync-hard-delete.md) | `UnitSyncManager`: canvas'tan silinen sayaç → `ProjectUnit` §2'ye rağmen hard-delete (`ExecuteDeleteAsync`); önce `DocProjectUnit` bağlantıları + `Policy.ProjectUnitId` çözülür, transaction'la sarmalanır |
| 2026-08 | decision | [unit-sync-hard-delete § Policy](./unit-sync-hard-delete.md#policy-hard-delete-öncesi-fk-temizliği-ama-soft-delete-değil) | `Policy`: birim silinince `ProjectUnitId=null` + `IsUnitDeleted=true`, ama `IsActive`'e DOKUNULMAZ — kullanıcıya görünür kalır; `IsActive=false` yalnızca elle iptalde (o zaman bir daha görünmez). `CreateAsync` aktif poliçe varsa otomatik kapatmaz, 400 döner |
| 2026-08 | decision | [doc-delete-hard-delete](./doc-delete-hard-delete.md) | `DocManager.DeleteAsync`: §2'ye rağmen hard-delete (önce SQL sonra MinIO); `Doc`↔`ProjectUnit` many-to-many, `DocProjectUnit` junction (`BaseEntity`, `Doc.ProjectUnitId` kolonu yok) |
| 2026-08 | gotcha | [migration-history-rewrite](./migration-history-rewrite.md) | Migration squash'ı (a75c7e8) mevcut HER yerel veritabanını kırar: `__EFMigrationsHistory` eski kimlikleri taşır, EF yeni `InitialCreate`'i hiç uygulanmamış sayıp SQL 2714 ile düşer. ⚠️ Belirti frontend'de `ERR_CONNECTION_RESET` ve `docker ps` "Up" gösterir — teşhis yeri `docker compose logs api`. `MigrationHistoryCheck` artık sebebi + çözümü basıyor (önden bloklamaz: dal değiştirende yanlış alarm olurdu). Geçmişi yeniden yazan değişiklik AYRI commit + sonucu yazan mesaj + ekibe haber; sıfırlama README > 8.1 |
