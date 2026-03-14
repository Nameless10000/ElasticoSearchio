using Elastic.Clients.Elasticsearch;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class ArticleSearchService
{
    private readonly ElasticsearchClient _client;

    public ArticleSearchService(ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task UpsertAsync(ArticleDocument document)
    {
        var response = await  _client.IndexAsync(document, i =>
        {
            i.Id(document.Id);
            i.Index("articles");
        });

        if (!response.IsValidResponse && response.TryGetOriginalException(out var exception))
        {
            throw new InvalidOperationException(exception.Message);
        }
    }

    public async Task IndexAsync(IEnumerable<ArticleDocument> documents)
    {
        // Bulk-индексация - предпочтительный способ записи
        var response = await _client.BulkAsync(b => b
            .Index("articles")
            .IndexMany(documents)
        );

        if (response.Errors)
        {
            throw new InvalidOperationException("Ошибка при индексации документов");
        }
    }

    public async Task<IReadOnlyCollection<ArticleDocument>> SearchAsync(string query)
    {
        var response = await _client.SearchAsync<ArticleDocument>(s => s
            .Indices("articles")
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f.Name, f => f.Content)
                    .Query(query)
                )
            )
        );

        return response.Documents;
    }
}