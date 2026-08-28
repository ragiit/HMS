# 06. Event Messaging Design

## 1. Overview
Menggunakan **RabbitMQ** sebagai message bus dan mengimplementasikan **Transactional Outbox Pattern** untuk menjamin keandalan event delivery.

## 2. Arsitektur Messaging

```
┌─────────────────┐     ┌──────────────────────┐
│  Service A      │     │  RabbitMQ Broker     │
│  ─────────      │     │  ───────────────────  │
│  Domain Change  │     │  Exchange: HMS.Events │
│       │         │     │  ├─ patient.*        │
│  ┌────v────┐    │     │  ├─ appointment.*    │
│  │DbContext│    │     │  ├─ prescription.*   │
│  └────┬────┘    │     │  ├─ lab.*            │
│       │         │     │  ├─ bill.*           │
│  ┌────v────┐    │     │  ├─ inventory.*      │
│  │ Outbox  │    │     │  └─ notification.*   │
│  │ Table   │    │     └──────────┬───────────┘
│  └────┬────┘    │                │
│       │         │                │ (Topic routing)
│  ┌────v────┐    │     ┌──────────┴───────────┐
│  │ Outbox  │    │     │  Service B (Consumer)│
│  │ Worker  │────┼────▶│  + Inbox table       │
│  │ (bg)    │    │     └──────────────────────┘
└─────────────────┘
```

## 3. Exchange & Routing Key Design

### Exchange
- **Name**: `HMS.Events` (topic exchange)
- **Type**: Topic

### Routing Key struktur
`{service}.{entity}.{action}`

### Tabel Routing Keys

| Routing Key | Event | Publisher | Subscribers |
|---|---|---|---|
| `patient.created` | Patient created | Patient | Notification |
| `patient.updated` | Patient updated | Patient | MedicalRecord, Billing |
| `patient.deactivated` | Patient deactivated | Patient | All |
| `doctor.created` | Doctor created | Doctor | Notification |
| `doctor.schedule.changed` | Schedule changed | Doctor | Appointment |
| `appointment.booked` | Appointment booked | Appointment | Notification, Billing |
| `appointment.cancelled` | Appointment cancelled | Appointment | Notification, Billing |
| `appointment.completed` | Appointment completed | Appointment | MedicalRecord, Billing |
| `appointment.reminder_due` | Reminder due | Appointment | Notification |
| `medical_record.created` | MR created | MedicalRecord | - |
| `medical_record.finalized` | MR finalized | MedicalRecord | Notification, Pharmacy |
| `prescription.order_created` | Prescription order | MedicalRecord | Pharmacy |
| `prescription.created` | Prescription created | Pharmacy | Notification, Inventory |
| `prescription.dispensed` | Prescription dispensed | Pharmacy | Inventory, Billing, Notification |
| `lab.order_created` | Lab order created | Laboratory | Billing |
| `lab.result_ready` | Lab result ready | Laboratory | MedicalRecord, Notification |
| `lab.result.critical` | Critical lab result | Laboratory | Notification |
| `bill.issued` | Invoice issued | Billing | Notification |
| `payment.received` | Payment received | Billing | Notification |
| `payment.overdue` | Payment overdue | Billing | Notification |
| `inventory.low_stock` | Low stock | Inventory | Notification |
| `inventory.out_of_stock` | Out of stock | Inventory | Pharmacy |

## 4. Transactional Outbox Pattern

### Flow
```
1. Application Handler (Command Handler) memproses business logic
2. Dalam SATU database transaction:
   - Persist domain entity changes
   - Insert event ke OutboxTable
3. Commit transaction
4. Background Worker (Outbox Processor) membaca OutboxTable rows:
   - SELECT events WHERE Status='Pending' ORDER BY CreatedDate
   - Publish ke RabbitMQ
   - Mark processed = 'Published'
5. Jika RabbitMQ down, event tetap di Outbox dan di-retry
```

### Benefit
- **Atomicity**: Perubahan data + event dalam satu transaction
- **At-least-once delivery**: Consumer harus idempotent (via Inbox table)
- **Resilience**: RabbitMQ down tidak mempengaruhi business commit

### Consume Side (Idempotency)
```
1. Consumer terima event dari RabbitMQ
2. Cek Inbox table: apakah EventId sudah pernah diproses?
3. Jika belum: proses + insert ke Inbox
4. Jika sudah: skip (idempotent)
5. Acknowledge message
```

## 5. Consumer Pattern (Background Service)

```csharp
public class AppointmentBookedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IModel _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Setup consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var message = JsonSerializer.Deserialize<AppointmentBookedEvent>(ea.Body);
            await HandleEvent(message);
            _channel.BasicAck(ea.DeliveryTag, false);
        };
        _channel.BasicConsume("appointment.queue", false, consumer);
    }

    private async Task HandleEvent(AppointmentBookedEvent evt)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<INotificationDbContext>();
        
        // Idempotency check
        if (await db.InboxEvents.AnyAsync(x => x.EventId == evt.EventId))
            return;
            
        // Process...
        await db.Notifications.AddAsync(new NotificationEntity { ... });
        
        // Save inbox
        await db.InboxEvents.AddAsync(new InboxEvent { EventId = evt.EventId, ... });
        await db.SaveChangesAsync();
    }
}
```

## 6. Event Contract (Base)

```csharp
public abstract class IntegrationEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = null!;
    public string SourceService { get; set; } = null!;
}
```

### Konkret contoh AppEvent
```csharp
public class AppointmentBookedEvent : IntegrationEvent
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string SlotTime { get; set; } = null!;
}
```

## 7. Topology RabbitMQ

```
Exchange: HMS.Events (topic)
├── Queue: patient-queue       ┌── bind: patient.*
├── Queue: notification-queue  ┌── bind: patient.*, appointment.*, lab.*, ...
├── Queue: billing-queue       ┌── bind: appointment.*, prescription.dispensed, lab.order_created
├── Queue: appointment-queue   ┌── bind: doctor.schedule.changed, patient.deactivated
├── Queue: medical-record-queue┌── bind: appointment.completed, lab.result_ready
├── Queue: pharmacy-queue      ┌── bind: prescription.order_created, inventory.out_of_stock
├── Queue: inventory-queue     ┌── bind: prescription.dispensed, prescription.cancelled
├── Queue: laboratory-queue    ┌── bind: prescription.order_created (await)
├── Queue: dead-letter-queue   ┌── bind: (dead letter untuk failed message)
```

## 8. Retry & Dead Letter

```
Main Queue
    │ (fail processing)
    ▼
Retry Queue (delay 30s)
    │ (retry 1, 2, 3)
    ▼
Dead Letter Queue (max retries exceeded)
    │
    ▼
(Scheduled job → report/requeue manually)
```

## 9. Correlation ID
- Ditambahkan sekali di API Gateway (untuk HTTP request)
- Event membawa `CorrelationId` yang sama sepanjang alur async
- Menggunakan `Serilog` Enricher untuk menambahkan ke log

## 10. Consumer Config Notes
- `PrefetchCount`: 10 agar tidak overload
- `false` manual ack (selalu biarkan consumer acknowledge sendiri)
- Manually ack setelah persist ke Inbox + business logic sukses
