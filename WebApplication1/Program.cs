using System.Diagnostics;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Database;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "elastic_password"))
    .DefaultIndex("cdc.article.articles");

builder.Services.AddSingleton(new ElasticsearchClient(settings));
builder.Services.AddScoped<ArticleSearchService>();

builder.Services.AddSingleton(new ProducerConfig
{
    BootstrapServers = "localhost:9094"
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion("11.2.6")));

builder.Services.AddSingleton<KafkaProducerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

try
{
    await using var scope = app.Services.CreateAsyncScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

    if (dataContext.Database.GetPendingMigrations().Any())
    {
        await dataContext.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    Debug.WriteLine(ex.Message);
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();