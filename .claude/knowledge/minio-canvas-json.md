# decision: Çizim JSON'u MinIO'da — SQL'e girmez

Yalnızca kimlik/sahiplik/ilişkisel veri SQL'de. **Çizimin kendisi değil.**

Bir projenin tüm çizimi (kat/duvar/oda/cihaz — ocak/kombi/sayaç —, boru, topoloji düğümleri)
**tek bir JSON nesnesi** olarak MinIO'da tutulur; SQL'den `project.ProjectVersion.JsonToken` ile
referanslanır.

## Kurallar

- Çizim varlıkları için SQL'e **granüler ilişkisel tablo ekleme** — FK/cascade her save'de
  MinIO ile senkron tutmayı zorlar, MinIO'nun tüm amacını bozar.
- Yeni tablo eklemeden önce sor: (a) yapısal/sorgulanabilir ilişkisel veri mi → SQL doğru;
  (b) sürekli düzenlenen çizimin parçası mı → canvas JSON'a ait.

## Kaydet akışı (autosave YOK)

- Yalnızca kullanıcı "Kaydet"e basınca yazılır.
- İstemci tam canvas JSON'u gönderir → API yeni **değişmez** MinIO nesnesi yazar → `ProjectVersion`
  satırı eklenir → `Project.UpdatedAt`/`RowVersion` güncellenir.
- MinIO ↔ SQL ortak transaction yok: MinIO'ya yaz → token al → SQL satırı ekle; SQL başarısızsa
  MinIO nesnesi temizlenir.

## Retention

Son 20 sıradan checkpoint (FIFO) + **kalıcı olarak** Onaylandı checkpoint'i ve en son Reddedildi
checkpoint'i (pencerenin neresinde olursa olsun silinmez).
