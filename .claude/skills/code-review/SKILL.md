---
name: code-review
description: Commit/MR öncesi backend değişikliğini gözden geçirirken oku. Şema, cascade, Result deseni, soft-delete, güvenlik ve commit mesajı kontrolü için checklist. "Çalışıyor" ≠ "bitti" — bu listeden geçmeden MR açma.
---

# Backend Code Review (commit öncesi)

"Çalışıyor" ≠ "bitti": testler geçiyor + temiz kod + kurallara uygun = bitti. Kendi
değişikliğini de bu listeden geçir.

## Şema / DataAccess

- [ ] Yeni/değişen string alanların `HasMaxLength(n)` var mı — **hiçbir yerde `nvarchar(max)` yok**.
- [ ] Gereken UNIQUE index'ler tanımlı mı (Email/Username, PolicyNumber, bileşikler)?
- [ ] Gereken CHECK'ler var mı (firma-exclusivity, CompanyType, tarih kısıtları)?
- [ ] Tablolar `dbo` + **çoğul** adla mı (`ToTable("Xxxs")`, şema argümanı YOK — modül şeması
      benimsenmedi, bkz. knowledge/db-conventions.md)?
- [ ] FK cascade doğru mu — `OnModelCreating`'deki **global Restrict döngüsü** korunuyor (FK başına
      config'te `OnDelete` **yazılmaz**); tek açık istisna `Doc.UploadedByUserId` → SetNull.
      Multiple cascade paths riski yok mu?
- [ ] Migration üretildiyse `Down()` geri-alınabilir mi; üretilen SQL `StarCAD_Database.sql` ile
      tutarlı mı?

## Business / mimari

- [ ] Manager `StarCADContext`'i doğrudan mı kullanıyor — **generic repo/DAL eklenmemiş**.
- [ ] Metotlar `Result`/`DataResult<T>` dönüyor mu; entity controller'a sızmıyor (`.Select()`
      projeksiyon)?
- [ ] Soft-delete/audit Manager'da elle tekrar edilmemiş (DbContext override yapıyor)?
- [ ] Rol kontrolü `Role.Code` üzerinden mi (Id/Name string'i değil); tek-rol modeli korunuyor mu?
- [ ] Concurrency `DbUpdateConcurrencyException` → 409'a map'leniyor mu?

## Dil / isimlendirme

- [ ] Kod tümüyle İngilizce (sınıf/metot/değişken/dosya)?
- [ ] Lookup anahtarları İngilizce (`CodeGroup.GroupName`, `Code.CodeValue`); yalnız `Code.Name`
      (ekran metni) Türkçe? (Anahtar alanlar tek dil — İngilizce.)
- [ ] Kullanıcıya görünen metin Türkçe ve `Messages.cs`'te mi (koda gömülü string değil)?
- [ ] Yorum sadece "neden" için mi (bariz şeye yorum yok); sahipsiz TODO yok?

## Güvenlik / hijyen

- [ ] Sır yok: connection string / MinIO key / JWT secret koda gömülmemiş (appsettings/secrets/env)?
- [ ] `Console.WriteLine`/debug logu, yorum satırı çöpü kaldırılmış?
- [ ] Parola düz metin tutulmuyor (hash); hata response'unda stack trace/sır sızmıyor?
- [ ] `SELECT *` benzeri gereksiz geniş çekim yok (projeksiyonla sadece gereken kolonlar)?
- [ ] Yıkıcı işlem (DROP/DELETE/TRUNCATE, database drop) içeren bir şey varsa onay alındı mı?

## Commit / MR

- [ ] Conventional commit: `<type>(<scope>): <subject>`, iki nokta sonrası boşluk, İngilizce
      imperative, ≤72; issue sonda `(#n)` / gövdede `Closes #n`; `EA/` gibi prefix yok.
- [ ] Branch `<type>/<kebab>`, `main` değil; `bin/`/`obj/` gibi build çıktısı stage'e girmemiş.
- [ ] Değişiklik tek konulu ve küçük mü; refactor ayrı commit mi?
- [ ] Yeni bir mimari karar/tuzak çıktıysa `knowledge/` + INDEX güncellendi mi?
