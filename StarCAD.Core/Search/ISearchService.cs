namespace StarCAD.Core.Search;

public interface ISearchService
{
    Task IndexDocumentAsync(string indexName, string documentId, object document, CancellationToken ct = default);
    Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> SearchIdsAsync(string indexName, string queryText, CancellationToken ct = default);
    Task<IReadOnlyList<int>> SearchIdsAsync(string indexName, string queryText, string filterField, string filterValue, CancellationToken ct = default);
}