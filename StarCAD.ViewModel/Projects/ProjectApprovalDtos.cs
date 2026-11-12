namespace StarCAD.ViewModel.Projects;

public sealed class ProjectRejectDto
{
    public string Description { get; init; } = string.Empty;
}

public sealed class OperationHistoryListItemDto
{
    public int Id { get; init; }
    public string OperationCode { get; init; } = string.Empty;
    public string OperationName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? RoleSnapshot { get; init; }
    public int UserId { get; init; }
    public string UserFullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}