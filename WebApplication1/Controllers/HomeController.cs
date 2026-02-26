using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class HomeController : ControllerBase
{
    private readonly RabbitMqProducerService _rabbitMqProducer;

    public HomeController(RabbitMqProducerService rabbitMqProducer)
    {
        _rabbitMqProducer = rabbitMqProducer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] ArticleDocument article)
    {
        var serializedArticle = JsonConvert.SerializeObject(article);
        
        await _rabbitMqProducer.SendAsync(serializedArticle);

        return Ok("Message sent to RabbitMQ");
    }
}
