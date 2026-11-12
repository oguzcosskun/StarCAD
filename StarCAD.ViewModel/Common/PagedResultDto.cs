namespace StarCAD.ViewModel.Common;

public sealed class PagedResultDto<TItem>
{
    public List<TItem> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}