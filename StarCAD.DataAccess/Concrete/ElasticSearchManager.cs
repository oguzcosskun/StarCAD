using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.Extensions.Options;
using StarCAD.Core.Search;
using StarCAD.DataAccess.Configurations;
using Elastic.Clients.Elasticsearch.Mapping;

namespace StarCAD.DataAccess.Concrete;

public class ElasticSearchManager : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly int _maxSearchResults;

    public ElasticSearchManager(IOptions<ElasticSettings> options)
    {
        _client = new ElasticsearchClient(new Uri(options.Value.Endpoint));
        _maxSearchResults = options.Value.MaxSearchResults;
    }
    private static readonly Dictionary<string, Properties> IndexMappings = new()
    {
        ["projects"] = new Properties
        {
            { "name", new TextProperty { Analyzer = "turkish" } },
            { "buildingCode", new KeywordProperty() },
            { "status", new KeywordProperty() },
            { "statusName", new TextProperty { Analyzer = "turkish" } },
            { "projectTypeName", new TextProperty { Analyzer = "turkish" } },
            { "heatingTypeName", new TextProperty { Analyzer = "turkish" } },
            { "firmName", new TextProperty { Analyzer = "turkish" } },
            { "gasDistributionFirmName", new TextProperty { Analyzer = "turkish" } },
            { "projectFirmId", new KeywordProperty() },
            { "gasDistributionFirmId", new KeywordProperty() }
        },
        ["project-firms"] = new Properties
        {
            { "title", new TextProperty { Analyzer = "turkish" } },
            { "contactPerson", new TextProperty { Analyzer = "turkish" } },
            { "phone", new KeywordProperty() },
            { "email", new KeywordProperty() },
            { "taxNumber", new KeywordProperty() }
        },
        ["gas-distribution-firms"] = new Properties
        {
            { "title", new TextProperty { Analyzer = "turkish" } },
            { "companyNumber", new KeywordProperty() },
            { "groupName", new TextProperty { Analyzer = "turkish" } }
        },
        ["users"] = new Properties
        {
            { "fullName", new TextProperty { Analyzer = "turkish" } },
            { "roleName", new TextProperty { Analyzer = "turkish" } },
            { "projectFirmName", new TextProperty { Analyzer = "turkish" } },
            { "gasDistributionFirmName", new TextProperty { Analyzer = "turkish" } },
            { "email", new KeywordProperty() },
            { "username", new KeywordProperty() },
            { "phone", new KeywordProperty() },
            { "projectFirmId", new KeywordProperty() },
            { "gasDistributionFirmId", new KeywordProperty() }
        }
    };
    public async Task IndexDocumentAsync(string indexName, string documentId, object document, CancellationToken ct = default)
    {
        await EnsureIndexAsync(indexName, ct);

        var response = await _client.IndexAsync(document, idx => idx.Index(indexName).Id(documentId), ct);
        if (!response.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch indexleme başarısız: {response.DebugInformation}");
    }

    private async Task EnsureIndexAsync(string indexName, CancellationToken ct)
    {
        var exists = await _client.Indices.ExistsAsync(indexName, ct);
        if (exists.Exists)
            return;

        if (!IndexMappings.TryGetValue(indexName, out var properties))
            return; // Bilinmeyen index adı — mapping tanımlı değil, ES'in varsayılanına bırakılır.

        await _client.Indices.CreateAsync(indexName, c => c.Mappings(m => m.Properties(properties)), ct);
    }
    
    public Task<IReadOnlyList<int>> SearchIdsAsync(string indexName, string queryText, CancellationToken ct = default) =>
        SearchIdsCoreAsync(indexName, queryText, null, null, ct);

    public Task<IReadOnlyList<int>> SearchIdsAsync(
        string indexName, string queryText, string filterField, string filterValue, CancellationToken ct = default) =>
        SearchIdsCoreAsync(indexName, queryText, filterField, filterValue, ct);

    private async Task<IReadOnlyList<int>> SearchIdsCoreAsync(
        string indexName, string queryText, string? filterField, string? filterValue, CancellationToken ct)
    {
        if (!IndexMappings.TryGetValue(indexName, out var properties))
            return [];

        var textFields = properties
            .Where(p => p.Value is TextProperty)
            .Select(p => p.Key.ToString()!)
            .ToArray();

        if (textFields.Length == 0)
            return [];

        var response = await _client.SearchAsync<object>(s => s
            .Indices(indexName)
            .Query(q =>
            {
                if (filterField is not null && filterValue is not null)
                {
                    q.Bool(b => b
                        .Must(m => m.MultiMatch(mm => mm
                            .Query(queryText)
                            .Fields(textFields)
                            .Fuzziness(new Fuzziness("AUTO"))))
                        .Filter(f => f.Term(t => t.Field(filterField).Value(filterValue))));
                }
                else
                {
                    q.MultiMatch(mm => mm
                        .Query(queryText)
                        .Fields(textFields)
                        .Fuzziness(new Fuzziness("AUTO")));
                }
            })
            .Source(new SourceConfig(false))
            .Size(_maxSearchResults), ct);

        if (!response.IsValidResponse)
        {
            // Index'e henüz hiç doküman yazılmadıysa ES onu hiç oluşturmaz — "index_not_found"
            // burada gerçek bir hata değil, sadece "eşleşen sonuç yok" demek.
            if (response.ElasticsearchServerError?.Status == 404)
                return [];

            throw new InvalidOperationException($"Elasticsearch arama başarısız: {response.DebugInformation}");
        }

        return response.Hits.Select(h => int.Parse(h.Id!)).ToList();
    }

    public async Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync(indexName, documentId, ct);
        if (!response.IsValidResponse && response.Result != Result.NotFound)
            throw new InvalidOperationException($"Elasticsearch silme başarısız: {response.DebugInformation}");
    }
}