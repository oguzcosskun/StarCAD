# gotcha: Her FK'ye açık Restrict — yoksa "multiple cascade paths"

EF Core, **zorunlu** (non-nullable) bir FK için varsayılan olarak `DeleteBehavior.Cascade`
üretir. Şemamız neredeyse tümüyle NO ACTION olduğundan, birden fazla cascade yolu SQL Server'da
**"Introducing FOREIGN KEY constraint ... may cause cycles or multiple cascade paths"** hatasıyla
migration'ı reddettirir.

## Kural

- Her ilişkide **açıkça** `.OnDelete(DeleteBehavior.Restrict)` yaz.
- `StarCADContext.OnModelCreating` şu an tüm FK'leri döngüyle Restrict'e çekiyor
  (`GetForeignKeys()` üzerinde) — bu default'ı korumak için config'lerde ters bir davranış
  yazma.
- **Tek SetNull istisnası:** `document.Doc.UploadedByUserId → user.User` (atıf: evrak kaydı
  yükleyeninden bağımsız değerli). Bunu config'te açıkça `.OnDelete(DeleteBehavior.SetNull)` yaz.

## Cascade üç kategori (özet)

- **Sahiplik** (parent'sız anlamsız) → CASCADE. Şu an şemada örnek yok.
- **Referans** (master/lookup'a işaret) → NO ACTION / Restrict. Neredeyse tümü.
- **Atıf** ("kim yaptı/yükledi") → SET NULL. Tek örnek `Doc.UploadedByUserId`.

## Özel dikkat: Project → User üç FK

`project.Project`'in User'a **üç** FK'si var: `OwnerUserId`, `CreatedByUserId`, `UpdatedByUserId`.
Üçü de ayrı ayrı `.WithMany().HasForeignKey(...).OnDelete(DeleteBehavior.Restrict)` ile tanımlanır
(hepsi Restrict olduğu için multiple cascade paths sorunu çıkmaz). Bkz. [[project-audit-exceptions]].
