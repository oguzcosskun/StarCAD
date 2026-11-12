using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Doc : SoftDeleteEntity
{
    public int? DocTypeCodeId { get; set; }
    public Code? DocTypeCode { get; set; }

    public string FileName { get; set; } = null!;
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public int? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? ProjectFirmId { get; set; }
    public ProjectFirm? ProjectFirm { get; set; }
}
