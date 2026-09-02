namespace HMS.Shared.Contracts;

/// <summary>
/// Base class untuk integration event yang dikirim antar service
/// melalui message bus (RabbitMQ).
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Nama kelas event (misal AppointmentBookedEvent).</summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Routing key bertitik (misal "appointment.booked") hasil resolusi dari atribut
    /// [EventName]. Menggantikan EventType sebagai routing key pada publisher.
    /// </summary>
    public string RoutingKey => EventNameResolver.Resolve(GetType());
}