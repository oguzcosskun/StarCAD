namespace StarCAD.ViewModel.Codes;

public sealed class CodeCreateDto
{
    public int GroupId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CodeValue { get; init; } = string.Empty;
}

public sealed class CodeUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string CodeValue { get; init; } = string.Empty;
}

public sealed class CodeListItemDto
{
    public int Id { get; init; }
    public int GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CodeValue { get; init; } = string.Empty;
}