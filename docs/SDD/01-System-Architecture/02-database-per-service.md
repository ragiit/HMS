# 02. Database Per Service Pattern

## 1. Prinsip
Setiap microservice memiliki **database SQL Server sendiri** yang ownership-nya eksklusif dimiliki service tersebut.

## 2. Daftar Database per Service

| # | Service | Database Name | Lokasi Server |
|---|---|---|---|
| 1 | Identity Service | `HMS_Identity` | SQL Server Instance 1 |
| 2 | Patient Service | `HMS_Patient` | SQL Server Instance 1 |
| 3 | Doctor Service | `HMS_Doctor` | SQL Server Instance 1 |
| 4 | Appointment Service | `HMS_Appointment` | SQL Server Instance 2 |
| 5 | Medical Record Service | `HMS_MedicalRecord` | SQL Server Instance 2 |
| 6 | Pharmacy Service | `HMS_Pharmacy` | SQL Server Instance 2 |
| 7 | Laboratory Service | `HMS_Laboratory` | SQL Server Instance 3 |
| 8 | Billing Service | `HMS_Billing` | SQL Server Instance 3 |
| 9 | Inventory Service | `HMS_Inventory` | SQL Server Instance 3 |
| 10 | Notification Service | `HMS_Notification` | SQL Server Instance 3 |

> **Catatan**: Pembagian instance di atas adalah contoh distribusi untuk load spreading. Dalam environment production, semua database dapat berada di satu instance, namun tetap **terpisah secara logis** (separate schema/database). Untuk scale besar, gunakan separate instances.

## 3. Aturan / Policies

### 3.1 Ownership
- Setiap service **hanya** dapat membaca/menulis ke **database miliknya sendiri**
- Tidak ada service yang mengakses database service lain secara langsung
- Integrasi data antar service dilakukan via:
  - **Synchronous**: REST API Call (jika butuh data real-time)
  - **Asynchronous**: Event Bus / RabbitMQ (jika eventual consistency dapat diterima)

### 3.2 Konsistensi Data
- **Strong consistency** di dalam satu service (transactional)
- **Eventual consistency** antar service (event-driven)
- Menggunakan **Transactional Outbox Pattern** untuk memastikan event terkirim saat data berubah

### 3.3 Migration
- Migrasi database menggunakan **EF Core Migrations (Code-First)**
- Setiap service memiliki migrasi sendiri di folder `Migrations/`
- Database di-deploy via `dotnet ef database update` saat deployment

```
Command Example:
dotnet ef migrations add InitialCreate -p Patient.Infrastructure -s Patient.Api
dotnet ef database update -p Patient.Infrastructure -s Patient.Api
```

## 4. Diagram Database Ownership

```
┌──────────────────────┐    ┌──────────────────────┐    ┌──────────────────────┐
│  Identity Service    │    │  Patient Service     │    │  Doctor Service      │
│  ──────────────────  │    │  ──────────────────  │    │  ──────────────────  │
│  DB: HMS_Identity    │    │  DB: HMS_Patient     │    │  DB: HMS_Doctor      │
│  Tables:             │    │  Tables:             │    │  Tables:             │
│  - Users             │    │  - Patients          │    │  - Doctors           │
│  - Roles             │    │  - PatientAddresses  │    │  - Specializations   │
│  - UserRoles         │    │  - PatientContacts   │    │  - DoctorSchedules   │
│  - RefreshTokens     │    └──────────────────────┘    └──────────────────────┘
└──────────────────────┘
┌──────────────────────┐    ┌──────────────────────┐    ┌──────────────────────┐
│  Appointment Svc     │    │  MedicalRecord Svc   │    │  Pharmacy Service    │
│  ──────────────────  │    │  ──────────────────  │    │  ──────────────────  │
│  DB: HMS_Appointment │    │  DB: HMS_MedRecord   │    │  DB: HMS_Pharmacy    │
│  Tables:             │    │  Tables:             │    │  Tables:             │
│  - Appointments      │    │  - PatientMedicalRec │    │  - Prescriptions     │
│  - AppointmentStatus │    │  - VitalSigns        │    │  - PrescriptionItems │
│  - ApptHistory       │    │  - Diagnosis         │    │  - Dispensing        │
└──────────────────────┘    └──────────────────────┘    └──────────────────────┘
┌──────────────────────┐    ┌──────────────────────┐    ┌──────────────────────┐
│  Laboratory Svc      │    │  Billing Service     │    │  Inventory Svc       │
│  ──────────────────  │    │  ──────────────────  │    │  ──────────────────  │
│  DB: HMS_Laboratory  │    │  DB: HMS_Billing     │    │  DB: HMS_Inventory   │
│  Tables:             │    │  Tables:             │    │  Tables:             │
│  - LabOrders         │    │  - Invoices          │    │  - InventoryItems    │
│  - LabTests          │    │  - InvoiceItems      │    │  - StockMovements    │
│  - LabResults        │    │  - Payments          │    └──────────────────────┘
└──────────────────────┘    └──────────────────────┘
┌──────────────────────┐
│  Notification Svc    │
│  ──────────────────  │
│  DB: HMS_Notif       │
│  Tables:             │
│  - Notifications     │
│  - NotificationTypes │
└──────────────────────┘
```

## 5. Transaksi dan Konsistensi

### 5.1 Dalam Service (Strong Consistency)
```sql
BEGIN TRANSACTION
  -- Insert Prescription di Pharmacy Service
  -- Update Stock di Inventory Service (via event, async)
COMMIT
```

### 5.2 Antar Service (Eventual Consistency)
```
Pharmacy Service                Inventory Service
    │                              │
    │---- Event: prescription.filled ----▶
    │                              │
    │                              │  Update stock quantity
    │                              │  (eventual consistency)
```

## 6. Backups & Disaster Recovery
- Setiap database di-backup sesuai SLA masing-masing service
- Recovery Point Objective (RPO): maksimal 15 menit untuk data transaksional
- Point-In-Time Recovery (PITR) diaktifkan untuk semua database
