namespace HMS.Shared.Messaging;

/// <summary>
/// Parameter konfigurasi koneksi RabbitMQ.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "HMS.Events";
    public string ExchangeType { get; set; } = "topic";

    public string AmqpUri => $"amqp://{UserName}:{Password}@{Host}:{Port}/{VirtualHost}";
}