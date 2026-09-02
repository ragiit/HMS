# 01. API Gateway Design (YARP)

## 1. Overview
API Gateway menggunakan **YARP (Yet Another Reverse Proxy)** yang menjadi **entry point tunggal** untuk semua client (Web, Mobile).

## 2. Konfigurasi Routing YARP

### 2.1 appsettings.json Gateway

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/identity/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "patient": {
        "ClusterId": "patient-cluster",
        "Match": { "Path": "/api/patients/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "doctor": {
        "ClusterId": "doctor-cluster",
        "Match": { "Path": "/api/doctors/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "appointment": {
        "ClusterId": "appointment-cluster",
        "Match": { "Path": "/api/appointments/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "medical-record": {
        "ClusterId": "medical-record-cluster",
        "Match": { "Path": "/api/medical-records/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "pharmacy": {
        "ClusterId": "pharmacy-cluster",
        "Match": { "Path": "/api/pharmacy/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "laboratory": {
        "ClusterId": "laboratory-cluster",
        "Match": { "Path": "/api/laboratory/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "billing": {
        "ClusterId": "billing-cluster",
        "Match": { "Path": "/api/billing/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "inventory": {
        "ClusterId": "inventory-cluster",
        "Match": { "Path": "/api/inventory/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      },
      "notification": {
        "ClusterId": "notification-cluster",
        "Match": { "Path": "/api/notifications/{**catch-all}" },
        "Transforms": [{ "PathPattern": "/api/{catch-all}" }]
      }
    },
    "Clusters": {
      "identity-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "identity1": { "Address": "http://identity-service:8080" },
          "identity2": { "Address": "http://identity-service-2:8080" }
        }
      },
      "patient-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "patient1": { "Address": "http://patient-service:8080" },
          "patient2": { "Address": "http://patient-service-2:8080" }
        }
      },
      "doctor-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "doctor1": { "Address": "http://doctor-service:8080" }
        }
      },
      "appointment-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "appointment1": { "Address": "http://appointment-service:8080" },
          "appointment2": { "Address": "http://appointment-service-2:8080" }
        }
      },
      "medical-record-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "medrecord1": { "Address": "http://medical-record-service:8080" }
        }
      },
      "pharmacy-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "pharmacy1": { "Address": "http://pharmacy-service:8080" }
        }
      },
      "laboratory-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "laboratory1": { "Address": "http://laboratory-service:8080" }
        }
      },
      "billing-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "billing1": { "Address": "http://billing-service:8080" }
        }
      },
      "inventory-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "inventory1": { "Address": "http://inventory-service:8080" }
        }
      },
      "notification-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "notification1": { "Address": "http://notification-service:8080" }
        }
      }
    }
  }
}
```

## 3. URL Pattern Mapper (Public to Internal)

| Public URL (Gateway) | Internal Service URL |
|---|---|
| `/api/identity/*` | `http://identity-service:8080/api/*` |
| `/api/patients/*` | `http://patient-service:8080/api/*` |
| `/api/doctors/*` | `http://doctor-service:8080/api/*` |
| `/api/appointments/*` | `http://appointment-service:8080/api/*` |
| `/api/medical-records/*` | `http://medical-record-service:8080/api/*` |
| `/api/pharmacy/*` | `http://pharmacy-service:8080/api/*` |
| `/api/laboratory/*` | `http://laboratory-service:8080/api/*` |
| `/api/billing/*` | `http://billing-service:8080/api/*` |
| `/api/inventory/*` | `http://inventory-service:8080/api/*` |
| `/api/notifications/*` | `http://notification-service:8080/api/*` |

## 4. Authentication di API Gateway

```csharp
// Program.cs (Gateway)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://identity-service:8080";
        options.Audience = "hms-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "HMS.Identity",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

## 5. Rate Limiting di Gateway

```json
{
  "RateLimiting": {
    "GlobalLimiter": {
      "PermitLimit": 100,
      "Window": "00:01:00",
      "QueueLimit": 0,
      "QueueProcessingOrder": "OldestFirst"
    },
    "ScopedLimiter": {
      "IdentityService": { "PermitLimit": 50, "Window": "00:01:00" },
      "PatientService":  { "PermitLimit": 200, "Window": "00:01:00" },
      "BillingService":  { "PermitLimit": 100, "Window": "00:01:00" }
    }
  }
}
```

## 6. CORS Configuration

```json
{
  "Cors": {
    "AllowedOrigins": ["https://hms-web.example.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

## 7. Error Handling di Gateway

| Status Code | Makna | Response |
|---|---|---|
| 401 | Not Authenticated | `{ "error": "unauthorized", "message": "Invalid or expired token" }` |
| 403 | Not Authorized | `{ "error": "forbidden", "message": "Access denied" }` |
| 404 | Route not found | `{ "error": "not_found", "message": "Route not found" }` |
| 429 | Too Many Requests | `{ "error": "rate_limited", "message": "Request limit exceeded" }` |
| 502 | Service unavailable | `{ "error": "service_unavailable", "message": "Backend service unreachable" }` |

## 8. Request Flow Diagram

```
 Client (Web/Mobile)
       │
       ▼
 HTTPS /customers/123  →  API Gateway (YARP Port 8080)
       │
       ├─ Authentication Middleware (JWT check → 401 jika tidak valid)
       ├─ Rate Limiting Middleware
       ├─ Correlation ID Middleware
       │
       ▼
 Route Match: /api/patients/{id}
       │
       ▼
 Forward ke cluster "patient-cluster"
       │
       ▼
 Patient Service (Port 8080 internal)
```

## 9. Korrelation ID
Semua request melalui gateway akan diberi **Correlation ID** yang dibawa ke downstream service untuk **tracing**. Ini dikirimkan sebagai HTTP header `X-Correlation-ID`.

> **Implementasi**: YARP transform dapat menambahkan header ini, atau middleware custom di API Gateway.
