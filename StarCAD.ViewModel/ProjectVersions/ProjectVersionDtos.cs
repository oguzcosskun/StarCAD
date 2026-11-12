using System.Text.Json;

namespace StarCAD.ViewModel.ProjectVersions;

public sealed class ProjectVersionCreatedDto
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public string ObjectKey { get; init; } = string.Empty;
    public string? Label { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notice { get; init; }
}

public sealed class ProjectVersionListItemDto
{
    public int Id { get; init; }
    public string? Label { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class ProjectVersionDownloadDto
{
    public string Url { get; init; } = string.Empty;
}