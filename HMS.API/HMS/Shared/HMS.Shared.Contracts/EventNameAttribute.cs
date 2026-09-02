namespace HMS.Shared.Contracts;

/// <summary>
/// Menetapkan routing key / event name (format dot notation, misal "patient.created")
/// pada sebuah integration event. Publisher memakai ini sebagai routing key sehingga
/// konsisten dengan topology RabbitMQ (HMS.Events / topic) di SDD 06-Messaging.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EventNameAttribute : Attribute
{
    public string RoutingKey { get; }

    public EventNameAttribute(string routingKey)
    {
        RoutingKey = routingKey;
    }
}

/// <summary>
/// Helper untuk membaca routing key dari atribut [EventName] atau fallback ke nama class.
/// </summary>
public static class EventNameResolver
{
    /// <summary>
    /// Mengambil routing key sebuah tipe event.
    /// 1. Jika ada [EventName], gunakan nilainya.
    /// 2. Jika tipe mengimplementasikan IMappedEventName, gunakan GetEventName().
    /// 3. Fallback: nama class (dipisah menjadi kebab? tidak; gunakan GetType().Name).
    /// </summary>
    public static string Resolve(Type eventType)
    {
        var attribute = eventType.GetCustomAttributes(typeof(EventNameAttribute), false)
            .OfType<EventNameAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
            return attribute.RoutingKey;

        return eventType.Name;
    }

    public static string Resolve<TEvent>() where TEvent : IntegrationEvent =>
        Resolve(typeof(TEvent));
}