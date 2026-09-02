using HMS.Shared.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HMS.Shared.Messaging;

/// <summary>
/// Implementasi IMessageSubscriber berbasis RabbitMQ (topic exchange).
/// </summary>
public class RabbitMqSubscriber : IMessageSubscriber
{
    private readonly IConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqSubscriber> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqSubscriber(
        IConnection connection,
        RabbitMqOptions options,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqSubscriber> logger)
    {
        _connection = connection;
        _options = options;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Subscribe<TEvent>(string queueName, string routingKey, MessageHandler<TEvent> handler)
        where TEvent : IntegrationEvent
    {
        _ = Task.Run(async () =>
        {
            IChannel channel = await _connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: _options.ExchangeType,
                durable: true,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: _options.ExchangeName,
                routingKey: routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var @event = JsonSerializer.Deserialize<TEvent>(json, JsonOptions);
                    if (@event is null)
                    {
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                        return;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    await handler(@event, CancellationToken.None);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error memproses event {Type} pada queue {Queue}", ea.BasicProperties.Type, queueName);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Subscriber aktif untuk queue {Queue} dengan routing {Routing}", queueName, routingKey);
        });
    }
}