# 07. Cross-Cutting Concerns

## 1. Security

### 1.1 Authentication
- **JWT** (Bearer) digunakan untuk semua request ke service
- Token mengandung **claims**: `sub` (userId), `role`, `permission`, `iss` (Identity), `exp`
- Validasi di API Gateway (authorization) dan secara opsional di service (specific permission)
- **Refresh Token**: disimpan di Identity Service, rotated each refresh

### 1.2 Authorization - RBAC
| Role | Deskripsi |
|---|---|
| `Admin` | Full access, manage users, konfigurasi |
| `Doctor` | Akses medical record, prescription, appointment |
| `Nurse` | Akses medical record (views), vital signs, patient |
| `FrontDesk` | Registrasi pasien, booking appointment |
| `Pharmacist` | Dispense, inventory, prescription |
| `LabStaff` | Lab order, results |
| `BillingStaff` | Invoice, payment |
| `Inventory` | Stock management |
| `Patient` | Portal: lihat schedule, result, billing |

### 1.3 Policy-based Authorization
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PatientView", p => p.RequireClaim("permission", "patient.view"));
    options.AddPolicy("MedicalRecordWrite", p => p.RequireClaim("permission", "medical_record.write"));
    options.AddPolicy("BillingProcess", p => p.RequireClaim("permission", "billing.process"));
});
```

### 1.4 Data Protection
- **At rest**: SQL Server TDE, dan sensitive columns encrypted (AES) untuk NIK, nomor polis
- **In transit**: HTTPS/Automated TLS 
- **Secrets**: stored in environment variables / Azure Key Vault / Docker secrets

### 1.5 Audit Trail
- Setiap perubahan pada: `Patient`, `MedicalRecord`, `Prescription`, `Payment` di-log dengan:
  - User yang melakukan
  - Timestamp
  - Old & New value (untuk field sensitif)

## 2. Logging & Observability

### Stack
- **Serilog** untuk structured logging
- **OpenTelemetry** untuk distributed tracing
- Sink: Console (dev), Seq (centralized)

### Log Structure (Standardized JSON)
```json
{
  "@timestamp": "2026-01-15T10:30:00+07:00",
  "level": "Information",
  "service": "appointment-service",
  "traceId": "fe013...",
  "spanId": "8c01f...",
  "correlationId": "req-abc123",
  "message": "Appointment booked successfully",
  "appointmentId": 1,
  "patientId": 1,
  "durationMs": 45,
  "exception": null
}
```

### Observability Metrics
| Metric | Deskripsi |
|---|---|
| `request_rate` | RPS per service |
| `request_latency` | P50/P95/P99 |
| `error_rate` | % error |
| `queue_depth` | RabbitMQ queue depth |
| `db_connection_pool` | DB pool utilization |
| `cache_hit_ratio` | Redis hit ratio |

## 3. Caching (Redis)

### Strategi
| Data | Caching Strategy | TTL |
|---|---|---|
| Patient profile (detail) | Cache-aside | 5 min |
| Doctor list & schedule | Cache-aside | 10 min |
| Medical record list | Cache-aside | 1 min (sensitive, short) |
| Service catalog (tarif) | Cache-aside | 1 hour |
| Inventory item list | Cache-aside | 5 min |
| Appointment slots | Cache-around | 15 min |

### Pattern Implementasi
```csharp
var cached = await _cache.GetStringAsync($"patient:{patientId}");
if (cached != null) return JsonSerializer.Deserialize<PatientDto>(cached);

var patient = await _patientRepo.GetByIdAsync(patientId);
await _cache.SetStringAsync($"patient:{patientId}", JsonSerializer.Serialize(patient), 
    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
return patient;
```

### Invalidation
- **Event-driven invalidation**: saat `patient.updated` dipublish, subscriber menghapus cache `patient:{id}`
- Atau menggunakan **short TTL** untuk data yang sering berubah

## 4. Resilience Pattern (Polly)

| Pattern | Konfigurasi |
|---|---|
| Retry | 3 attempts, exponential backoff |
| Circuit breaker | 5 failures → open 30s |
| Timeout | Per operation 30s |
| Fallback | Return cached data |

```csharp
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
```

## 5. Configuration Management

### Approach
- `appsettings.json` untuk default
- **Environment variables** untuk environment-specific (database conn, token keys)
- **.env** file di development (dot-env loader)
- Production: injected via container secrets / K8s secrets

### Connection String example
```json
{
  "ConnectionStrings": {
    "Default": "Server=sqlserver;Database=HMS_Appointment;User Id=sa;Password=***;TrustServerCertificate=True",
    "Redis": "redis:6379",
    "RabbitMQ": "amqp://guest:guest@rabbitmq:5672"
  }
}
```

## 6. Health Checks

Setiap service expose `/health` endpoint:
- Liveness: `GET /health` → 200 jika berjalan
- Readiness: `GET /health/ready` → cek DB, RabbitMQ, Redis connectivity

API Gateway juga aggregate health check dari semua downstream service.

## 7. Error Handling

### Global Exception Middleware (di setiap API service)
| Exception | HTTP Status | Response |
|---|---|---|
| `ValidationException` | 400 | Field errors |
| `NotFoundException` | 404 | `{message}` |
| `BusinessRuleViolation` | 409 | `{message}` |
| `UnauthorizedException` | 401 | Auth error |
| `PermissionDeniedException` | 403 | Access denied |
| `ConcurrencyException` | 409 | Optimistic lock |
| Any other | 500 | Generic | 

## 8. Deployment (Docker Compose)

```yaml
version: '3.9'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "Strong_Pa55word"
    ports:
      - "1433:1433"
  rabbitmq:
    image: rabbitmq:3.13-management
    ports:
      - "5672:5672"
      - "15672:15672"
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
  api-gateway:
    build: ./ApiGateway
    ports:
      - "8080:8080"
  identity-service: { build: ./Services/Identity }
  patient-service: { build: ./Services/Patient }
  doctor-service: { build: ./Services/Doctor }
  # ... dst untuk 10 services
  notification-worker:
    build: ./Services/Notification
```
