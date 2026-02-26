using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.HostedServices;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly ConnectionFactory _connectionFactory;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly string _queueName = "demo-queue";

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        ConnectionFactory connectionFactory,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _connection ??= await _connectionFactory.CreateConnectionAsync(stoppingToken);
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var searchService = scope.ServiceProvider.GetRequiredService<ArticleSearchService>();

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            var articleDoc = JsonConvert.DeserializeObject<ArticleDocument>(message);

            if (articleDoc != null)
            {
                await searchService.IndexAsync([articleDoc]);
            }
            
            try
            {
                _logger.LogInformation("RabbitMQ message received: {Message}", message);
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message processing failed. DeliveryTag: {DeliveryTag}", eventArgs.DeliveryTag);
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // graceful shutdown
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
