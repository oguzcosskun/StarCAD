# decision: `ProjectUnit` senkron-silmesi hard-delete — soft-delete'e bilinçli istisna

`UnitSyncManager.SyncAsync`, her "Kaydet"te canvas JSON'undaki `unitReport` bölümünü
`ProjectUnit` tablosuyla senkronize eder (bkz. [[minio-canvas-json]]). Bir sayaç canvas'tan
silinip artık `unitReport`'ta görünmüyorsa, ona karşılık gelen `ProjectUnit` satırı **§2'nin
genel soft-delete kuralına rağmen gerçekten (fiziksel) silinir.**

## Neden istisna

`ProjectUnit` burada iş anlamı taşıyan bir kayıt değil, canvas'taki bir `gasMeter` elemanının
**türetilmiş/önbelleklenmiş** bir yansıması — kaynağı (sayaç) yoksa saklanacak bağımsız bir
değeri yok. Soft-delete'in amacı olan "geri alınabilirlik/denetim izi" burada anlamsız: aynı
`meterElementId` geri gelirse zaten yeni bir satır olarak yeniden oluşur.

## Sıra — mutlaka bu sırayla

Doc↔ProjectUnit ilişkisi many-to-many'dir (`DocProjectUnit` junction tablosu — bkz.
[[doc-delete-hard-delete]]), `Doc`'ta doğrudan bir `ProjectUnitId` kolonu **yok**.

1. **Önce bağlantıları çöz:** silinen birimlere işaret eden `DocProjectUnit` satırları
   `ExecuteDeleteAsync` ile silinir.
2. **Son bağlantısı kalkan evrak havuza döner:** silme öncesi hangi `DocId`'lerin etkilendiği
   toplanır; silme sonrası bu `DocId`'lerden hangilerinin **hiç** `DocProjectUnit` satırı
   kalmadığı bulunur — o evrakların `Doc.ProjectId`'si `ExecuteUpdateAsync` ile `null`'a çekilir
   (`UnlinkUnitAsync`'teki "son bağlantı → havuza dön" mantığıyla simetrik, bkz.
   [[doc-delete-hard-delete]]). `ProjectFirmId`'ye hiç dokunulmaz.
3. **Poliçeleri kapat:** silinen birimlere bağlı `Policy` satırlarının `ProjectUnitId`'si
   `ExecuteUpdateAsync` ile `null`'a çekilir (bkz. aşağıdaki "Policy" bölümü).
4. **Sonra birimi sil:** `ProjectUnits.Where(...).ExecuteDeleteAsync(...)` — gerçek `DELETE`.

Sıra tersine çevrilirse `DocProjectUnit.ProjectUnitId` / `Policy.ProjectUnitId` FK'leri
(`Restrict`, bkz. [[cascade-restrict]]) silmeyi SQL seviyesinde reddeder.

## Mekanizma: `ExecuteUpdateAsync`/`ExecuteDeleteAsync`

`StarCADContext.SaveChanges` override'ı **her** `SoftDeleteEntity.Remove()` çağrısını otomatik
`IsActive=false`'a çevirir (§2, merkezî). Bu, `ChangeTracker`/`SaveChanges` üzerinden çalışır.
EF Core'un bulk `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (`SaveChanges`'i hiç görmeden anında
SQL'e giden) API'leri bu interceptor'ı **atlar** — gerçek hard-delete için kasıtlı olarak bunlar
kullanılır, `_db.ProjectUnits.Remove(unit)` + `SaveChangesAsync()` **değil**.

## Transaction

`ExecuteUpdateAsync`/`ExecuteDeleteAsync` anında commit edildiği için, aynı işlem biriminde
(`ProjectVersionManager.CreateAsync`) hâlâ bekleyen normal `SaveChangesAsync` (yeni
`ProjectVersion` satırı + eşleşen `ProjectUnit` upsert'leri) ile birlikte atomik kalması için
açık bir `_db.Database.BeginTransactionAsync()` sarmalayıcısı gerekir — aksi halde biri commit
olup diğeri başarısız olabilir.

## Policy: hard-delete öncesi FK temizliği, ama soft-delete DEĞİL

`Policy`, `DocProjectUnit`'ten farklı olarak `ProjectUnit`'in türetilmiş bir yansıması değil —
iş anlamı taşıyan gerçek bir kayıt (`StarCAD.Entity/Concrete/Policy.cs`). Bu yüzden birimi
hard-delete edilince poliçe **hard-delete edilmez**, ama Restrict FK yine de temizlenmesini
zorunlu kılar:

```csharp
await _db.Policies
    .IgnoreQueryFilters()
    .Where(p => p.ProjectUnitId != null && removedUnitIds.Contains(p.ProjectUnitId.Value))
    .ExecuteUpdateAsync(s => s
        .SetProperty(p => p.ProjectUnitId, (int?)null)
        .SetProperty(p => p.IsUnitDeleted, true), cancellationToken);
```

Üç bilinçli tasarım kararı:

1. **`Policy.ProjectId` ayrı, denormalize bir kolon** (`Doc.ProjectId` ile birebir aynı desen) —
   `ProjectUnitId` `null`'a çekilse de poliçe projeye bağlı kalır, proje bazlı listeden
   düşmez.
2. **`IsActive`'e BİLİNÇLİ OLARAK dokunulmaz.** Birim silindiği için kapanan poliçe kullanıcıya
   görünür kalmalı — yalnızca `IsUnitDeleted=true` ile işaretlenir. `IsActive=false` yalnızca
   elle iptalde (`PolicyManager.DeleteAsync` → standart `Remove()` → merkezî soft-delete
   interceptor) olur ve o zaman poliçe global query filtresiyle kullanıcıdan tamamen gizlenir,
   bir daha görünmez. Yani `IsActive` "kullanıcı görebilir mi", `IsUnitDeleted` ise "birim hâlâ
   var mı" sorusunu cevaplıyor — ikisi bağımsız eksenler.
3. **`IgnoreQueryFilters()` zorunlu.** Daha önce elle iptal edilmiş (`IsActive=false`) ama hâlâ
   silinecek birime referans veren bir poliçe varsa, global filtre onu bu sorgudan gizler ve
   `ProjectUnitId`'si temizlenmeden kalır — bir sonraki adımdaki `ProjectUnits`
   `ExecuteDeleteAsync`'i aynı FK ihlaliyle patlar. `IgnoreQueryFilters()` bu satırları da
   kapsama alır; `IsActive`'leri `false` kalmaya devam eder (madde 2 gereği), tekrar görünür
   olmazlar.

Ayrıca `PolicyManager.CreateAsync`, bir birimde zaten aktif bir poliçe varsa **otomatik
kapatmaz** — 400 döner, çağıranın önce mevcut poliçeyi `DeleteAsync` ile elle iptal etmesi
gerekir (bilinçli karar: yenileme akışı Manager içinde sessizce olmasın).

## Kapsam dışı (henüz)

`Device` senkronu henüz yazılmadı. Yazıldığında aynı problem orada da olacak: bir birim
hard-delete edilmeden önce ona bağlı `Device` satırları da (Restrict FK) aynı sırayla
temizlenmeli.
