# 02. Technology Stack

## 1. Framework & Runtime

| Komponen | Teknologi | Version | Keterangan |
|---|---|---|---|
| Runtime | .NET | 10.0 | LTS (Long-Term Support) |
| Web Framework | ASP.NET Core | 10.0 | Minimal API & Controllers |
| Language | C# | 13 / 14 | Terbaru untuk .NET 10 |
| IDE | Visual Studio 2026 / JetBrains Rider | - | Atau VS Code |

## 2. Data & ORM

| Komponen | Teknologi | Version | Keterangan |
|---|---|---|---|
| Database | Microsoft SQL Server | 2022+ | Relational OLTP |
| ORM | Entity Framework Core | 10.0 | Code-First migrations |
| Migration Tool | dotnet-ef CLI | 10.0 | `dotnet ef` |
| Database CI/CD | EfCore migration bundle | 10.0 | Untuk automated deployment |

## 3. Infrastructure & Middleware

| Komponen | Teknologi | Version | Keterangan |
|---|---|---|---|
| API Gateway | YARP (Yet Another Reverse Proxy) | 2.x | Microsoft's reverse proxy |
| Message Broker | RabbitMQ | 3.13+ | Event bus / messaging |
| Cache | Redis | 7.x | Distributed cache |
| Container | Docker / Docker Compose | - | Containerization |
| Orchestration | Kubernetes (k8s) atau Docker Swarm | - | Opsional untuk scale besar |
| Secrets Mgmt | Azure Key Vault / Docker Secrets / .NET User Secrets | - | Config di development/prod |

## 4. Authentication & Authorization

| Komponen | Teknologi | Keterangan |
|---|---|---|
| Token | JWT (JSON Web Tokens) | OpenID Connect / OAuth2 |
| Password Hashing | BCrypt / PBKDF2 | ASP.NET Core Identity |
| Multi-factor | ASP.NET Core Identity 2FA | Opsional |
| Policy-based Auth | ASP.NET Core Policy | Claims & roles based |

## 5. NuGet Packages Utama

### Per Service (umum)
| Package | Version | Keterangan |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 10.x | EF Core runtime |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.x | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Design` | 10.x | Design-time stuff |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.x | JWT auth |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 10.x | Redis client |
| `RabbitMQ.Client` | 6.8.x | RabbitMQ client |

### CQRS / DDD
| Package | Keterangan |
|---|---|
| `MediatR` | Command/Query dispatch |
| `FluentValidation` | Validation pipeline |
| `AutoMapper` | DTO mapping |
| `Microsoft.Extensions.Hosting.BackgroundService` | Background/consumer workers |

### Observability
| Package | Keterangan |
|---|---|
| `OpenTelemetry` | Distributed tracing (OTel) |
| `Serilog.AspNetCore` | Structured logging |
| `Serilog.Sinks.Console` / `Serilog.Sinks.File` | Logging destinations |
| `Serilog.Sinks.Seq` | Centralized log (opsional) |

### Cross-cutting
| Package | Keterangan |
|---|---|
| `Swashbuckle.AspNetCore` | OpenAPI/Swagger generation |
| `DotNetEnv` | Env file loader |
| `Polly` | Resilience / retry patterns |

## 6. Testing Stack

| Kategori | Teknologi |
|---|---|
| Unit Testing | xUnit (tambahkan FluentAssertions, Moq/NSubstitute) |
| Integration Testing | xUnit + WebApplicationFactory + Testcontainers (SQL Server) |
| Contract Testing | Pact (opsional) |
| Load Testing | k6 / JMeter |
| Code Coverage | coverlet (integrated in xUnit) |

## 7. DevOps & CI/CD

| Komponen | Teknologi |
|---|---|
| Version Control | Git + GitHub / Azure DevOps |
| CI Pipeline | GitHub Actions / Azure Pipelines |
| Container Registry | Docker Hub / ACR / GHCR |
| Config Management | .NET Configuration (appsettings + env vars) |
| Service Discovery | Static (docker-compose) / K8s Service |

## 8. Environment Matrix

### Development
```
OS: Windows 11 / macOS / Linux
Runtime: .NET 10 SDK
Database: SQL Server via Docker | LocalDB
RabbitMQ: Docker container
Redis: Docker container
```

### Staging / Production
```
OS: Linux (Ubuntu 22.04 LTS+) dalam container
Database: SQL Server 2022 (managed / AKS)
RabbitMQ: Clustered
Redis: Redis Enterprise / managed cache
```

## 9. Version Control Strategy
- **Branching**: Git Flow (main, develop, feature/*, release/*)
- **Conventional Commits**: Untuk auto-changelog
- **EF Migration**: Disimpan sebagai committed files di repo service

## 10. Docker Images (Ubuntu-based)

```
mcr.microsoft.com/dotnet/aspnet:10.0
mcr.microsoft.com/dotnet/sdk:10.0 (build stage)
mcr.microsoft.com/mssql/server:2022-latest
rabbitmq:3.13-management
redis:7-alpine
```
