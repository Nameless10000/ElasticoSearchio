using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Database;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<ArticleDocument> Articles { get; set; }
}