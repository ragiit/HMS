# 01. System Architecture

## 1. Arsitektur Mikroservices - Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          CLIENT (Web/Mobile)                          │
└──────────────────────────────────┬──────────────────────────────────────┘
                                     │ HTTPS
                                     ▼
                    ┌─────────────────────────────────────┐
                    │      API GATEWAY (YARP)             │
                    │  - Route management                 │
                    │  - Authentication (JWT Token)       │
                    │  - Rate limiting                    │
                    │  - Load balancing                   │
                    └─────────────────────────────────────┘
                                     │
   ┌───────────┬──────────┬──────────┼──────────┬──────────┬───────────┐
   ▼           ▼          ▼          ▼          ▼          ▼           ▼
┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐
│ Identity│ │ Patient │ │ Doctor  │ │Appointm.│ │ Med.    │ │ Pharmacy│ │ Lab     │
│ Service │ │ Service │ │ Service │ │ Service │ │ Record  │ │ Service │ │ Service │
└────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘ │ Service │ └────┬────┘ └────┬────┘
     │          │          │          │    └────┬────┘     │          │
     │          │          │          │         │          │          │
   ┌─┴──────────┴──────────┴──────────┴─────────┴──────────┴──────────┴───────┐
   │                        RABBITMQ (Event Bus)                              │
   └──────────────────────────────────────────────────────────────────────────┘
                                     │
   ┌─────────────────────────────────┼──────────────────────────────────────┐
   ▼                                 ▼                                      ▼
┌─────────┐                    ┌─────────────┐                       ┌─────────┐
│ Billing │                    │ Inventory   │                       │Notificat│
│ Service │                    │ Service     │                       │ Service │
└────┬────┘                    └──────┬──────┘                       └────┬────┘
     │                                │                                     │
     ▼                                ▼                                     ▼
┌──────────────────┐   ┌───────────────────┐  ┌───────────────────────────┐
│ Cache (Redis)    │   │ Database (SQL Svr)│  │ Email/SMS/Push Provider   │
│ - Session State  │   │ - Per Service DB  │  └───────────────────────────┘
│ - Rate Limiting  │   └───────────────────┘
│ - Hot Data       │
└──────────────────┘
```

## 2. Pola Arsitektur per Service

```
┌───────────────────────────────────────────────────────────────┐
│                    CLEAN ARCHITECTURE                         │
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  PRESENTATION / API Layer                               │  │
│  │  - Controllers / Minimal APIs                           │  │
│  │  - Middleware (Auth, Validation, Error Handling)        │  │
│  ├─────────────────────────────────────────────────────────┤  │
│  │  APPLICATION Layer (CQRS)                               │  │
│  │  - Commands (Write Model)                               │  │
│  │  - Queries (Read Model)                                 │  │
│  │  - Handlers, Behaviors (Pipeline)                       │  │
│  │  - DTOs, Mappers                                        │  │
│  ├─────────────────────────────────────────────────────────┤  │
│  │  DOMAIN Layer (DDD)                                     │  │
│  │  - Entities                                            │  │
│  │  - Value Objects                                       │  │
│  │  - Aggregates & Roots                                  │  │
│  │  - Domain Events                                       │  │
│  │  - Domain Services, Exceptions                         │  │
│  ├─────────────────────────────────────────────────────────┤  │
│  │  INFRASTRUCTURE Layer                                   │  │
│  │  - EF Core DbContext                                   │  │
│  │  - Repositories (read/write)                           │  │
│  │  - Message Bus Publisher/Consumer                      │  │
│  │  - Redis Cache, External Clients                       │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
```

## 3. Komponen Infrastruktur

### 3.1 API Gateway (YARP)
- **Peran**: Reverse proxy, entry point tunggal untuk client
- **Fitur**:
  - Routing - menentukan backend service berdasarkan URL pattern
  - Authentication - validasi JWT token flow
  - Rate Limiting - batasi request per client/IP
  - Load Balancing - distribusi request ke multiple instance service

### 3.2 Message Bus (RabbitMQ)
- **Peran**: Komunikasi asynchronous antar service
- **Pola**: Publish/Subscribe + Event-Driven (Outbox Pattern)
- **Topik**: 
  - `patient.created`, `patient.updated`
  - `doctor.schedule.changed`
  - `appointment.booked`, `appointment.cancelled`
  - `lab.order.created`, `lab.result.ready`
  - `prescription.created`, `prescription.dispensed`
  - `bill.generated`, `payment.received`
  - `inventory.low`, `inventory.received`
  - `notification.*` (email, sms, push)

### 3.3 Cache (Redis)
- **Peran**: Cache data yang sering diakses (read-heavy)
- **Penggunaan**:
  - Cache profil pasien yang sering dibuka
  - Cache daftar jadwal dokter (dengan TTL)
  - Session management
  - Distributed rate limiting

### 3.4 Database per Service
Setiap service memiliki **database SQL Server sendiri** (Database per Service pattern). Tidak ada cross-database query. Integrasi data antar service dilakukan via **API call** atau **Event**.

## 4. Authentication Flow

```
Client ── POST /api/auth/login ──► API Gateway ──► Identity Service
Client ◄───── JWT Token + Refresh ── Identity Service

Request berikutnya:
Client ── GET /api/patients/123 (dengan Bearer Token) ──► API Gateway
         └── Validasi token (JWT middleware) └── Forward ke Patient Service
```

## 5. Event-Driven Flow (Contoh: Booking Appointment)

```
┌────────────┐   Book Appointment    ┌──────────────────┐
│  Client    │ ─────────────────────▶ │ Appointment Svc  │
└────────────┘                       └────────┬─────────┘
                                              │ Persist event ke Outbox
                                              ▼
                                       RabbitMQ
                                    ┌──────────┴──────────┐
                                    ▼                     ▼
                    ┌──────────────────────┐   ┌────────────────────┐
                    │ Notification Service │   │ Patient Service    │
                    │ (send reminder)      │   │ (update record)    │
                    └──────────────────────┘   └────────────────────┘
```

## 6. Struktur Folder Solusi (.NET 10)

```
HMS/
├── ApiGateway/                      # YARP Reverse Proxy
├── Services/
│   ├── Identity/
│   ├── Patient/
│   ├── Doctor/
│   ├── Appointment/
│   ├── MedicalRecord/
│   ├── Pharmacy/
│   ├── Laboratory/
│   ├── Billing/
│   ├── Inventory/
│   └── Notification/
├── Shared/
│   ├── HMS.Shared.Abstractions/     # Interfaces, base classes
│   ├── HMS.Shared.Contracts/        # Event contracts, DTOs bersama
│   ├── HMS.Shared.Messaging/        # RabbitMQ configuration
│   ├── HMS.Shared.Caching/          # Redis extension
│   └── HMS.Shared.Outbox/           # Transactional Outbox implementasi
├── docker-compose.yml
├── docker-compose.override.yml
└── .env
```

## 7. Pola Per Service (Struktur Folder)

```
Patient.Service/
├── src/
│   ├── Patient.Api/                          # Presentation Layer
│   ├── Patient.Application/                  # Application Layer (CQRS)
│   ├── Patient.Domain/                       # Domain Layer (DDD)
│   └── Patient.Infrastructure/               # Infrastructure Layer
└── tests/
    ├── Patient.UnitTests/
    ├── Patient.IntegrationTests/
    └── Patient.FunctionalTests/
```

## 8. Arquitectural Decisions (ADR)

| Keputusan | Alasan |
|---|---|
| YARP sebagai API Gateway | Performa tinggi (native .NET), middleware pipeline native |
| RabbitMQ untuk message bus | Mature, reliable, banyak dukungan |
| Redis untuk cache | In-memory, high performance, easy to scale |
| EF Core + Code-First | Development speed, migration versioned di git |
| CQRS untuk setiap service | Memisahkan read vs write, optimasi terpisah |
| Outbox Pattern | Memastikan at-least-once delivery, tidak ada inbox issue |
| JWT untuk auth | Stateless, scalable, standar industri |
