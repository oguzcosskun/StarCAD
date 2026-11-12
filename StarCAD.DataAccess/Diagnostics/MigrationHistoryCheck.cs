using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace StarCAD.DataAccess.Diagnostics;

/// <summary>
/// Yerel veritabanının, bu derlemedeki migration setiyle uyumlu olup olmadığını
/// söyler.
/// </summary>
public static class MigrationHistoryCheck
{
    /// <summary>
    /// `__EFMigrationsHistory`'de olup bu derlemede KARŞILIĞI OLMAYAN migration'lar.
    ///
    /// Boş değilse veritabanı, artık var olmayan bir migration setiyle kuruldu —
    /// pratikte migration geçmişinin squash'lanması (ya da başka bir dalda
    /// yeniden yazılması). EF bu durumda kendi migration'larını hiç uygulanmamış
    /// sayıp baştan CREATE TABLE üretir ve "There is already an object named ..."
    /// (SQL 2714) ile düşer.
    /// </summary>
    public static async Task<IReadOnlyList<string>> GetUnknownAppliedMigrationsAsync(
        DatabaseFacade database,
        CancellationToken cancellationToken = default)
    {
        // Veritabanı henüz yoksa geçmiş de yok: temiz kurulum, uyaracak bir şey yok.
        if (!await database.CanConnectAsync(cancellationToken))
        {
            return [];
        }

        var applied = await database.GetAppliedMigrationsAsync(cancellationToken);
        var known = database.GetMigrations().ToHashSet(StringComparer.Ordinal);

        return applied.Where(migrationId => !known.Contains(migrationId)).ToList();
    }

    /// <summary>
    /// Geliştiricinin ekranda göreceği açıklama. Ham yığın izi yerine bunu
    /// basıyoruz: SQL 2714'ten "yerel veritabanımı sıfırlamam gerekiyor"
    /// sonucuna kimse kendiliğinden varmıyor — sebep repoda, belirti ise
    /// frontend'de `ERR_CONNECTION_RESET` olarak görünüyor (API açılışta
    /// düştüğü için 8080'i dinleyen kimse kalmıyor, `docker ps` yine de
    /// "Up" diyor).
    /// </summary>
    public static string BuildMismatchMessage(IReadOnlyList<string> unknownAppliedMigrations)
    {
        const int sampleSize = 3;

        var sample = string.Join(", ", unknownAppliedMigrations.Take(sampleSize));
        var remaining = unknownAppliedMigrations.Count - sampleSize;
        if (remaining > 0)
        {
            sample += $" (+{remaining} tane daha)";
        }

        return $"""
            Yerel veritabanınız bu koddaki migration setiyle UYUŞMUYOR.

            Veritabanında uygulanmış görünen ama artık repoda bulunmayan migration'lar var:
              {sample}

            Sebebi genellikle migration geçmişinin squash'lanmasıdır: repodaki migration'lar
            yeniden yazıldığı için EF onları hiç uygulanmamış sayar ve zaten var olan
            tabloları yeniden oluşturmaya çalışır.

            ÇÖZÜM — yerel veritabanını sıfırlayın (MinIO ve Redis'e dokunmaz):

              docker stop starcad-api
              docker exec starcad-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa \
                -P 'StarCAD_Local_123!' -C -Q \
                "ALTER DATABASE [StarCAD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [StarCAD];"
              docker start starcad-api

            Açılışta migration'lar temiz uygulanır ve seeder örnek veriyi yeniden kurar
            (giriş: admin / Admin1234). Ayrıntı: README > "Yerel veritabanını sıfırlama".
            """;
    }
}
