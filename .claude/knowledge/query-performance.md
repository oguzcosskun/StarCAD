# convention: Sorgu performansı — Include yerine projeksiyon

İlişkili tablolardan gereksiz yere tüm entity/child'ı çekme. Varsayılan: **projeksiyon**, `Include` değil.

## Kurallar

- **Gerekmedikçe `Include` yapma.** Sadece ihtiyaç duyulan alanları **`.Select()` ile DTO'ya
  projekte et** — zaten çoğunlukla böyle yapıyoruz (ör. `ProjectManager` okuma sorguları).
  `.Select()` üretilen SQL'de yalnız o kolonları çeker; tracking/materialize maliyeti düşer.
- **`Include`'u bilinçli kullan:** ancak gerçekten tüm entity + child grafiği (yazma/güncelleme
  senaryosu ya da hepsini dönen bir uç) gerektiğinde.
- **Debug'da üretilen SQL'e bak** (dev SQL loglama açıkken) → **N+1** (döngüde lazy yükleme) ve
  gereksiz `JOIN`'leri yakala; yavaş yeri projeksiyona çevir.

## Neden

`Include` tüm satırı + ilişkiyi belleğe alır; liste uçlarında bu hem ağ hem materialize israfı,
N+1'e de açık. Projeksiyon veritabanı tarafında budar → daha az veri, daha az bellek, index dostu.
Okuma uçları salt-veri döndürdüğü için (bkz. [[generic-repo-removed]] — Manager `Result`/DTO döner)
tracking'e de gerek yoktur (`AsNoTracking` + `.Select()`).
