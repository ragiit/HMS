using HMS.Shared.Contracts;

namespace HMS.Shared.Messaging;

/// <summary>
/// Meregistrasi cara konsumsi event pada queue tertentu.
/// </summary>
public delegate Task MessageHandler<TEvent>(TEvent @event, CancellationToken cancellationToken)
    where TEvent : IntegrationEvent;

/// <summary>
/// Mendaftarkan subscriber (consumer) terhadap queue & routing.
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// Mendaftarkan consumer untuk satu tipe event pada sebuah queue.
    /// </summary>
    /// <typeparam name="TEvent">Tipe integration event.</typeparam>
    /// <param name="queueName">Nama queue pada RabbitMQ.</param>
    /// <param name="routingKey">Routing key binding (wildcard * / # didukung).</param>
    /// <param name="handler">Handler yang dipanggil saat event diterima.</param>
    void Subscribe<TEvent>(string queueName, string routingKey, MessageHandler<TEvent> handler)
        where TEvent : IntegrationEvent;
}