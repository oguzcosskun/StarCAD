using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Project : SoftDeleteEntity
{
    public int ProjectFirmAuthorizationId { get; set; }
    public ProjectFirmAuthorization ProjectFirmAuthorization { get; set; } = null!;

    public int? OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }

    public int? BuildingId { get; set; }
    public Building? Building { get; set; }

    public int? StatusCodeId { get; set; }
    public Code? StatusCode { get; set; }

    public int? ProjectTypeCodeId { get; set; }
    public Code? ProjectTypeCode { get; set; }

    public int? HeatingTypeCodeId { get; set; }
    public Code? HeatingTypeCode { get; set; }

    public int? BuildingUsageTypeCodeId { get; set; }
    public Code? BuildingUsageTypeCode { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? TotalArea { get; set; }
    public int? Capacity { get; set; }
    public bool IsOfficial { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[]? RowVersion { get; set; }

    public bool IsPermitProject { get; set; }
    public int? ApartmentCount { get; set; }
    public int? WorkplaceCount { get; set; }
    public int? ServiceBoxPressureMbar { get; set; }

    public ICollection<ProjectVersion> ProjectVersions { get; set; } = new List<ProjectVersion>();
    public ICollection<ProjectUnit> ProjectUnits { get; set; } = new List<ProjectUnit>();
}