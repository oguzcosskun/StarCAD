using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class User : SoftDeleteEntity
{
    public int? ProjectFirmId { get; set; }
    public ProjectFirm? ProjectFirm { get; set; }

    public int? GasDistributionFirmId { get; set; }
    public GasDistributionFirm? GasDistributionFirm { get; set; }

    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public int TokenVersion { get; set; }
}