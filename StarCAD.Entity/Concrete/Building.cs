using StarCAD.Core.Entities.Concrete;

namespace StarCAD.Entity.Concrete;

public class Building : SoftDeleteEntity
{
    public int? CityId { get; set; }
    public City? City { get; set; }
    public int? DistrictId { get; set; }
    public District? District { get; set; }
    public string? AddressLine { get; set; }
    public byte? FloorCount { get; set; }
    public byte? BasementCount { get; set; }
    public string? BlockLotParcel { get; set; }
    public string? Code { get; set; } 
}