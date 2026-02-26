using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using RabbitMQ.Client;
using WebApplication1.HostedServices;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .Authentication(new BasicAuthentication("elastic", "elastic_password"))
    .DefaultIndex("articles");

builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = "localhost",
    Port = 5672,
    UserName = "rabbituser",
    Password = "rabbitpassword"
});

builder.Services.AddSingleton<RabbitMqProducerService>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

builder.Services.AddSingleton(new ElasticsearchClient(settings));
builder.Services.AddScoped<ArticleSearchService>();

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
