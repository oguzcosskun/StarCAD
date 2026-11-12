using StarCAD.Core.Entities.Concrete;
using StarCAD.Core.Search;

namespace StarCAD.Business.Extensions;

public static class SearchQueryExtensions
{
    public static async Task<IQueryable<T>> ApplyTextSearchAsync<T>(
        this IQueryable<T> query,
        ISearchService searchService,
        string indexName,
        string? searchText,
        string? scopeField = null,
        string? scopeValue = null,
        CancellationToken cancellationToken = default)
        where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return query;

        var matchedIds = scopeField is not null && scopeValue is not null
            ? await searchService.SearchIdsAsync(indexName, searchText.Trim(), scopeField, scopeValue, cancellationToken)
            : await searchService.SearchIdsAsync(indexName, searchText.Trim(), cancellationToken);

        return query.Where(e => matchedIds.Contains(e.Id));
    }
}
