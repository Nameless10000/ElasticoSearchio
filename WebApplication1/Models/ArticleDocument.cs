using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class ArticleDocument
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Content { get; set; }
}