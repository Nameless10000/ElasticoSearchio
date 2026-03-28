using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WebApplication1.Models;

public class ArticleDocument
{
    [Key]
    public int Id { get; set; }
    
    [JsonPropertyName("Name")]
    [JsonProperty("Name")]
    public string Name { get; set; }
    
    [JsonPropertyName("Content")]
    [JsonProperty("Content")]
    public string Content { get; set; }
}