# decision: `Doc` silme hard-delete — soft-delete'e bilinçli istisna

`DocManager.DeleteAsync`, §2'nin genel soft-delete kuralına rağmen `Doc` satırını **gerçekten**
(fiziksel) siler; aynı çağrıda MinIO'daki asıl dosya da silinir (bkz. [[unit-sync-hard-delete]] —
aynı gerekçe kalıbı: kaynağı olmayan kayıt tutmanın değeri yok).

## Neden istisna

`Doc` diğer soft-delete'li tablolardan farklı: SQL satırının **tek işlevi** MinIO'daki dosyayı
işaret etmek. Dosyayı da sileceğimiz için `IsActive=false` bırakmak, artık hiçbir içeriğe
işaret etmeyen bir hayalet kayıt bırakmaktan başka bir şey yapmaz — `ProjectVersion` gibi bir
"geçmiş checkpoint" kavramı `Doc` için yok, retention politikası bu tabloya uygulanmıyor.

## Sıra — mutlaka bu sırayla

1. **Önce SQL, sonra MinIO.** `_db.Docs.Where(...).ExecuteDeleteAsync(...)` — `SaveChanges`
   override'ının soft-delete interceptor'ını atlayan bulk silme (mekanizma [[unit-sync-hard-delete]]
   ile aynı).
2. Ardından `IFileStorageService.RemoveObjectAsync(doc.FilePath, ...)`.

Bu sıra bilinçli: SQL silme başarısız olursa MinIO'ya hiç dokunulmamış olur (temiz rollback).
MinIO silme başarısız olursa geriye kullanıcıya hiç görünmeyen, zararsız bir artık (orphan)
nesne kalır — "hatanın yönü güvenli tarafa bakar" (bkz. `ProjectAccessExtensions`'taki aynı
ilke). Ters sırada (önce MinIO) bir SQL hatası, DB'de içeriksiz kalan bir `Doc` satırı bırakırdı
— kullanıcıya görünür ve kafa karıştırıcı.

## `Doc` ↔ `ProjectUnit`: many-to-many, junction tablo

`Doc.ProjectUnitId` diye bir kolon **yok** — tek bir evrak birden fazla bağımsız bölüme
bağlanabilir (ör. birkaç daireyi kapsayan tek bir sözleşme), o yüzden ilişki ayrı bir junction
tabloda: `DocProjectUnit` (`DocId`, `ProjectUnitId`, `ProjectId`, `ProjectFirmId`, `CreatedAt`).
`Doc.ProjectId`/`ProjectFirmId` hâlâ `Doc` üzerinde doğrudan kolon (onlar 1-e-1 kalıyor).
`ProjectId`/`ProjectFirmId` junction'da da **bilinçli denormalize** (mentörün referans
şemasındaki desen) — bağlantı hiç güncellenmez, sadece silinip yeniden kurulur (`AssignAsync`
proje değişince tüm bağlantıları siler), o yüzden bu kopyanın bayatlama riski yok.

## Havuzdan doğrudan birime bağlama

`LinkUnitAsync`, evrak henüz havuzdaysa (`Doc.ProjectId=null`) önce ayrıca `AssignAsync` ile
projeye atanmasını **şart koşmaz** — hedef birimden projeyi/firmayı kendisi çözer:
1. Birimin projesi (`ProjectUnit.ProjectId`) ve o projenin firma bağı bulunur.
2. `doc.ProjectId` doluysa hedef birimin projesiyle eşleşmeli, değilse 400.
3. `doc.ProjectFirmId` hedef projenin firmasıyla eşleşmeli, değilse 403 (başka firmaya bağlanamaz).
4. `doc.ProjectId` boşsa (havuzda) birimin projesiyle **geri doldurulur** (`doc.ProjectId ??= project.Id`).
5. `DocProjectUnit` satırı `ProjectId`/`ProjectFirmId` denormalize kopyalarıyla eklenir.

`UnlinkUnitAsync` bunun simetriği: kaldırılan bağlantı bir evrağın **son** birim bağlantısıysa
(`DocProjectUnits`'te başka satırı kalmadıysa) `doc.ProjectId` otomatik `null`'a döner — sadece
tek bir birim bağlantısıyla projeye giren evrak, o bağlantı kalkınca havuza geri düşer.
**`ProjectFirmId` bu akışta hiçbir zaman dokunulmaz/null'lanmaz** — `WhereVisibleTo(Doc)` firma
kullanıcısını `d.ProjectFirmId == currentUser.ProjectFirmId` ile filtreliyor, null olsa evrak
Admin dışında kimseye görünmez olurdu (bkz. [[cascade-restrict]]'teki "hatanın yönü güvenli
tarafa bakar" ilkesiyle aynı gerekçe).

`DocProjectUnit`, `SoftDeleteEntity` değil **`BaseEntity`**'den türer — saf bir ilişki kaydı,
bağımsız bir "silinmiş ama arşivlenmiş" değeri yok. Bu yüzden `_db.DocProjectUnits.Remove(link)`
zaten gerçek `DELETE`'tir, `unit-sync-hard-delete.md`'deki gibi `ExecuteDeleteAsync` gerekmez
(sadece bulk/toplu silmelerde — ör. birim senkron sırasında birden fazla satırı tek seferde
silerken — performans için `ExecuteDeleteAsync` kullanılır, tekil `UnlinkUnitAsync`'te gerekmez).

Bağlama/kaldırma `DocManager.LinkUnitAsync`/`UnlinkUnitAsync` ile ayrı endpoint'lerden yapılır
(`POST`/`DELETE /api/docs/{id}/units/{unitId}`) — `AssignAsync` sadece `Doc.ProjectId`'yi
değiştirir/havuza döndürür ve proje değişince mevcut tüm `DocProjectUnit` bağlantılarını temizler
(eski projenin birimi yeni bağlamda anlamsız kalır).
