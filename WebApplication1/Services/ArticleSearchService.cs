using Elastic.Clients.Elasticsearch;
using WebApplication1.Database;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class ArticleSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly DataContext _dataContext;

    public ArticleSearchService(
        ElasticsearchClient client,
        DataContext dataContext)
    {
        _client = client;
        _dataContext = dataContext;
    }

    public async Task AddArticleAsync(ArticleDocument document)
    {
        await _dataContext.Articles.AddAsync(document);
        await _dataContext.SaveChangesAsync();
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
            .Indices("cdc.article.articles")
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