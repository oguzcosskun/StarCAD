namespace StarCAD.ViewModel.Cities;

public sealed class CityListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? PlateCode { get; init; }
}

public sealed class DistrictListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}