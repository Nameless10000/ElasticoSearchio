using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ArticleSearchService _service;

    public SearchController(ArticleSearchService service)
    {
        _service = service;
    }

    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var documents = new[]
        {
            new ArticleDocument { Id = 1, Name = "abc", Content = "Моя первая статья по ASP.NET Core и Elasticsearch" },
            new ArticleDocument { Id = 2, Name = "def", Content = "Полнотекстовый поиск в .NET" },
            new ArticleDocument { Id = 3, Name = "ghi", Content = "Работа с Elasticsearch 9 - быстрый старт" }
        };

        await _service.IndexAsync(documents);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var result = await _service.SearchAsync(q);
        return Ok(result);
    }
}