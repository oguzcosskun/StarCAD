# decision: Project audit istisnaları (CreatedByUserId + UpdatedByUserId)

Genel kural: her tabloya blanket audit kolonu (`CreatedBy`/`UpdatedBy`/`DeletedAt`) **eklenmez**
(bilinçle reddedildi). Tek istisna **`project.Project`** — somut domain ihtiyacıyla:

- **`OwnerUserId`** — atanan yetkili (sahip).
- **`CreatedByUserId`** — fiilen oluşturan. Admin bir firma **adına** proje açıp yetkiliyi ayrı
  atayabildiği için "oluşturan ≠ sahip" gerçek bir senaryo.
- **`UpdatedByUserId`** — son güncelleyen. Aynı firmadan çok çalışan aynı projeye dokunabilir;
  `RowVersion` (optimistic concurrency) çakışmayı önler, bu kolon "kim"i tutar.

Üçü de nullable, User'a **NO ACTION / Restrict** FK (kullanıcı hard-delete edilmiyor, soft-delete
yeterli). EF config'inde üç ilişki ayrı ayrı Restrict — bkz. [[cascade-restrict]].

Bunlar `Project`'e özgü, bilinçli istisnalardır; **başka tabloya audit kolonu genişletme.**
