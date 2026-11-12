using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;

namespace StarCAD.DataAccess.Seed;

/// <summary>
/// Ortamdan bağımsız master veri: 81 il ve ilçeleri. Test fixture'ı DEĞİLDİR —
/// canlıda da gerekli olduğu için DevelopmentDataSeeder'dan ayrı tutulur.
/// </summary>
public static class MasterDataSeeder
{
    private const string CityResourceName = "StarCAD.DataAccess.Seed.Data.turkiye-il-ilce.json";

    public static async Task SeedAsync(StarCADContext db)
    {
        await SeedCitiesAsync(db);
    }

    private static async Task SeedCitiesAsync(StarCADContext db)
    {
        if (await db.Cities.AnyAsync())
            return;

        await using var stream = typeof(MasterDataSeeder).Assembly
            .GetManifestResourceStream(CityResourceName)
            ?? throw new InvalidOperationException($"Gömülü kaynak bulunamadı: {CityResourceName}");

        var items = await JsonSerializer.DeserializeAsync<List<CitySeedItem>>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("İl/ilçe verisi okunamadı.");

        db.Cities.AddRange(items.Select(item => new City
        {
            Name = item.Name,
            PlateCode = item.PlateCode,
            Districts = item.Districts.Select(name => new District { Name = name }).ToList()
        }));

        await db.SaveChangesAsync();
    }

    private sealed record CitySeedItem(string Name, string PlateCode, string[] Districts);
}
