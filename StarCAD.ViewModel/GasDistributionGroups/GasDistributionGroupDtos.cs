namespace StarCAD.ViewModel.GasDistributionGroups;

public sealed class GasDistributionGroupCreateDto
{
    public string Name { get; init; } = string.Empty;
}

public sealed class GasDistributionGroupUpdateDto
{
    public string Name { get; init; } = string.Empty;
}

public sealed class GasDistributionGroupListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}