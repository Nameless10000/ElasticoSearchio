using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Models;
using WebApplication1.Services;

[ApiController]
[Route("api/[controller]/[action]")]
public class SearchController : ControllerBase
{
    private readonly ArticleSearchService _service;
    private KafkaProducerService _kafkaProducer;

    public SearchController(ArticleSearchService service, KafkaProducerService kafkaProducer)
    {
        _service = service;
        _kafkaProducer = kafkaProducer;
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
    
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ArticleDocument article)
    {
        var payload = JsonConvert.SerializeObject(article);
        await _kafkaProducer.ProduceAsync(
            topic: "demo-topic",
            message: payload
        );

        return Ok("Message sent to Kafka");
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var result = await _service.SearchAsync(q);
        return Ok(result);
    }
}
