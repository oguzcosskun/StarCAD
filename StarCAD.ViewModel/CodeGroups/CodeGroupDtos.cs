namespace StarCAD.ViewModel.CodeGroups;

public sealed class CodeGroupCreateDto
{
    public string GroupName { get; init; } = string.Empty;
}

public sealed class CodeGroupUpdateDto
{
    public string GroupName { get; init; } = string.Empty;
}

public sealed class CodeGroupListItemDto
{
    public int Id { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public int CodeCount { get; init; }
}