using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using StarCAD.Business.Abstract;
using StarCAD.DataAccess.Concrete.EntityFramework;
using StarCAD.Entity.Concrete;

namespace StarCAD.Business.Concrete;

public class UnitSyncManager : IUnitSyncService
{
    private readonly StarCADContext _db;

    public UnitSyncManager(StarCADContext db)
    {
        _db = db;
    }

    public async Task SyncAsync(int projectId, string rawJson, CancellationToken cancellationToken = default)
    {
        var incoming = ParseUnitReport(rawJson);

        // Tracked (AsNoTracking değil) — eşleşenler doğrudan bu entity'ler üzerinde güncellenecek.
        var existingUnits = await _db.ProjectUnits
            .Where(u => u.ProjectId == projectId && u.SourceMeterElementId != null)
            .ToListAsync(cancellationToken);

        var existingByMeterElementId = existingUnits.ToDictionary(u => u.SourceMeterElementId!.Value);
        var incomingMeterElementIds = incoming.Select(r => r.MeterElementId).ToHashSet();

        foreach (var row in incoming)
        {
            ProjectUnit unit;
            if (existingByMeterElementId.TryGetValue(row.MeterElementId, out var existingUnit))
            {
                // Id sabit kalır → bu birime bağlı Doc'lar kopmaz.
                ApplyRow(existingUnit, row);
                unit = existingUnit;

                // Cihazların stabil bir eşleştirme anahtarı yok — her senkronda o birimin tüm
                // cihazları silinip unitReport'taki güncel listeden sıfırdan yazılır
                await _db.Devices.Where(d => d.ProjectUnitId == unit.Id).ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                unit = new ProjectUnit { ProjectId = projectId, SourceMeterElementId = row.MeterElementId };
                ApplyRow(unit, row);
                _db.ProjectUnits.Add(unit);
            }
            
            foreach (var deviceRow in row.Devices)
            {
                _db.Devices.Add(new Device
                {
                    ProjectUnit = unit,
                    DeviceType = deviceRow.Type,
                    Brand = deviceRow.Brand,
                    Model = deviceRow.Model,
                    Capacity = deviceRow.Capacity,
                    FlowCubicMeterPerHour = deviceRow.FlowCubicMeterPerHour,
                    FlueLabel = deviceRow.FlueLabel
                });
            }
        }

        var removedUnitIds = existingUnits
            .Where(u => !incomingMeterElementIds.Contains(u.SourceMeterElementId!.Value))
            .Select(u => u.Id)
            .ToList();

        if (removedUnitIds.Count == 0)
            return;

        // Sayaç canvas'tan silindi: önce ona bağlı evrak bağlantılarını (DocProjectUnit) çöz,
        // sonra birimi GERÇEKTEN sil. Bilinçli soft-delete istisnası (bkz. knowledge/unit-sync-hard-delete.md) —
        // bu satır türetilmiş/geçici bir eşleştirme kaydı, kaynağı (sayaç) yoksa saklanacak iş değeri yok.
        var affectedDocIds = await _db.DocProjectUnits
            .Where(l => removedUnitIds.Contains(l.ProjectUnitId))
            .Select(l => l.DocId)
            .Distinct()
            .ToListAsync(cancellationToken);

        await _db.DocProjectUnits
            .Where(l => removedUnitIds.Contains(l.ProjectUnitId))
            .ExecuteDeleteAsync(cancellationToken);

        // UnlinkUnitAsync'le simetrik: silinen bağlantı bir evrağın SON birim bağlantısıysa
        // (başka birime bağlı değilse) evrak havuza döner. ProjectFirmId hiç dokunulmaz —
        // aksi halde evrak Admin dışında kimseye görünmez olurdu.
        if (affectedDocIds.Count > 0)
        {
            var docIdsWithRemainingLinks = await _db.DocProjectUnits
                .Where(l => affectedDocIds.Contains(l.DocId))
                .Select(l => l.DocId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var docIdsToReturnToPool = affectedDocIds.Except(docIdsWithRemainingLinks).ToList();
            if (docIdsToReturnToPool.Count > 0)
            {
                await _db.Docs
                    .Where(d => docIdsToReturnToPool.Contains(d.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(d => d.ProjectId, (int?)null), cancellationToken);
            }
        }

        // Silinecek birime bağlı poliçeler (Policy.ProjectUnitId, Restrict FK) varsa aşağıdaki
        // ExecuteDeleteAsync FK ihlaliyle reddedilir. Poliçe iş anlamı taşıyan bir kayıt (ProjectUnit
        // gibi türetilmiş değil) — o yüzden hard-delete edilmez: ProjectUnitId null'a çekilir,
        // Policy.ProjectId sayesinde proje bazında görünür kalır (bkz.
        // knowledge/unit-sync-hard-delete.md). IsActive'e BİLİNÇLİ OLARAK dokunulmaz: birim
        // silindiği için kapanan poliçe kullanıcıya görünür kalmalı (yalnızca IsUnitDeleted
        // bayrağıyla işaretlenir); IsActive yalnızca elle iptalde (PolicyManager.DeleteAsync)
        // false olur ve kullanıcıdan bir daha gizlenir. IgnoreQueryFilters: daha önce elle iptal
        // edilmiş (IsActive=false) ama hâlâ silinecek birime referans veren poliçelerin FK'si de
        // temizlenmeli, yoksa onlar da aynı ihlalle DELETE'i engeller — IsActive'leri false
        // kalmaya devam eder, tekrar görünür olmazlar.
        await _db.Policies
            .IgnoreQueryFilters()
            .Where(p => p.ProjectUnitId != null && removedUnitIds.Contains(p.ProjectUnitId.Value))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.ProjectUnitId, (int?)null)
                .SetProperty(p => p.IsUnitDeleted, true), cancellationToken);

        // Silinecek birime bağlı cihazlar da anlamsız kalır — ProjectUnit hard-delete edilmeden
        // önce temizlenir (Restrict FK).
        await _db.Devices
            .Where(d => removedUnitIds.Contains(d.ProjectUnitId))
            .ExecuteDeleteAsync(cancellationToken);

        await _db.ProjectUnits
            .Where(u => removedUnitIds.Contains(u.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static void ApplyRow(ProjectUnit unit, UnitReportRow row)
    {
        unit.UnitNumber = row.UnitNumber;
        unit.SubscriberName = row.SubscriberName;
        unit.SubscriberNo = row.SubscriberNo;
        unit.MeterClassLabel = row.ClassLabel;
        unit.Area = row.AreaSquareMeters is { } area ? (int)Math.Round(area) : null;
        unit.FlowCubicMeterPerHour = row.FlowCubicMeterPerHour;
        unit.PressureMbar = row.PressureMbar;
        unit.PipeTypeName = row.PipeTypeName;
    }

    private static List<UnitReportRow> ParseUnitReport(string rawJson)
    {
        var root = JsonNode.Parse(rawJson);
        var unitReport = root?["unitReport"]?.AsArray();
        if (unitReport is null)
            return [];

        var rows = new List<UnitReportRow>();
        foreach (var node in unitReport)
        {
            if (node is null)
                continue;

            // meterElementId eşleştirme anahtarı; okunamıyorsa satır atlanır (dış girdiye güvenilmez).
            if (GetInt(node["meterElementId"]) is not { } meterElementId)
                continue;

            rows.Add(new UnitReportRow(
                meterElementId,
                GetString(node["unitNumber"]),
                GetString(node["subscriberName"]),
                GetString(node["subscriberNo"]),
                GetString(node["classLabel"]),
                GetDecimal(node["areaSquareMeters"]),
                GetDecimal(node["flowCubicMeterPerHour"]),
                GetInt(node["pressureMbar"]),
                GetString(node["pipeTypeName"]),
                ParseDevices(node["devices"])));
        }

        return rows;
    }

    private static List<DeviceRow> ParseDevices(JsonNode? devicesNode)
    {
        var devicesArray = devicesNode?.AsArray();
        if (devicesArray is null)
            return [];

        var devices = new List<DeviceRow>();
        foreach (var node in devicesArray)
        {
            if (node is null)
                continue;

            devices.Add(new DeviceRow(
                GetString(node["type"]),
                GetString(node["brand"]),
                GetString(node["model"]),
                GetString(node["capacity"]),
                GetDecimal(node["flowCubicMeterPerHour"]),
                GetString(node["flueLabel"])));
        }

        return devices;
    }

    private static int? GetInt(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<double>(out var d) ? (int)d : null;

    private static decimal? GetDecimal(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<double>(out var d) ? (decimal)d : null;

    private static string? GetString(JsonNode? node)
    {
        if (node is not JsonValue value || !value.TryGetValue<string>(out var s))
            return null;

        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    private readonly record struct UnitReportRow(
        int MeterElementId,
        string? UnitNumber,
        string? SubscriberName,
        string? SubscriberNo,
        string? ClassLabel,
        decimal? AreaSquareMeters,
        decimal? FlowCubicMeterPerHour,
        int? PressureMbar,
        string? PipeTypeName,
        List<DeviceRow> Devices);

    private readonly record struct DeviceRow(
        string? Type,
        string? Brand,
        string? Model,
        string? Capacity,
        decimal? FlowCubicMeterPerHour,
        string? FlueLabel);
}
