using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using WebApplication1.Services;
using WebApplication1.Services.KafkaConsumerService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "elastic_password"))
    .DefaultIndex("articles");

builder.Services.AddSingleton(new ElasticsearchClient(settings));
builder.Services.AddScoped<ArticleSearchService>();

builder.Services.AddSingleton(new ProducerConfig
{
    BootstrapServers = "localhost:9094"
});

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddSingleton<IConsumer<string, string>>(_ =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "localhost:9094",
        GroupId = "article-indexer",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };

    return new ConsumerBuilder<string, string>(config)
        .Build();
});
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
