# 🗺️ HMS Roadmap Pengembangan

Panduan **berurutan** untuk membangun HMS dari nol hingga integrasi penuh.
Urutan disusun berdasarkan **dependensi** — sebuah service dibangun setelah service/komponen yang ia butuhkan sudah ada.

---

## Fase 0 — Landasan (Foundation)

> Membangun fondasi yang dipakai semua service. **Tidak boleh dilewati.**

### 0.1 Solution & Tooling
- [ ] Buka solution `HMS.API/HMS.API.slnx`
- [ ] Siapkan struktur project .NET 10 (class library & web project)
- [ ] Set target framework `net10.0`, nullable enable, implicit usings
- [ ] Setup `Directory.Build.props` untuk standar kompilasi (opsional)

### 0.2 Shared Libraries (dibangun pertama, dipakai SEMUA service)
Buat 5 project class library di bawah `HMS.API/Shared/`:

| Project | Isi |
|---|---|
| `HMS.Shared.Abstractions` | Interfaces, base classes, ValueObject, `IGenericRepository`, `Result<T>` |
| `HMS.Shared.Contracts` | `IntegrationEvent`, event contracts (`AppointmentBookedEvent`, dst), shared DTOs |
| `HMS.Shared.Messaging` | RabbitMQ config, wrapper publisher/consumer, DI extensions |
| `HMS.Shared.Caching` | Redis extension `IDistributedCache`, serde helper, DI |
| `HMS.Shared.Outbox` | Model `OutboxMessage`, `IOutboxStore`, background processor |

> ✅ **Kriteria selesai**: semua 5 project bisa di-build, unit-test parser event & outbox dasar.

---

## Fase 1 — Service Template & Foundation Services

> Membangun service **satu per satu**, dimulai dari yang **paling tidak bergantung** (menjadi "base" bagi yang lain).

### 1.1 Buat Service Template (contoh: Patient)
Jadikan **Patient Service** sebagai template lengkap Clean Architecture, lalu replikasi ke service lain.
- [ ] `Patient.Api` (Minimal API/Controllers, DI, Swagger, health check)
- [ ] `Patient.Application` (MediatR Commands/Queries/Handlers, DTO)
- [ ] `Patient.Domain` (Entity, ValueObject, Aggregate, Domain Events)
- [ ] `Patient.Infrastructure` (EF Core DbContext, Repositories, Migrations)
- [ ] Template `Dockerfile`, `appsettings.json`
- [ ] Unit & Integration test skeleton

### 1.2 Identity Service (**WAJIB PERTAMA** — pusat auth)
Tidak bergantung service lain; semua service butuh auth darinya.
- [ ] Domain: `User`, `Role`, `UserRole`, `RefreshToken`
- [ ] `Application`: Login, Register, Refresh, ChangePassword, AssignRoles
- [ ] `Infrastructure`: EF Core + ASP.NET Identity / custom, JWT issuer, RefreshToken rotation
- [ ] Seed roles: `Admin`, `Doctor`, `Nurse`, `FrontDesk`, `Pharmacist`, `LabStaff`, `BillingStaff`, `Patient`, `Inventory`

### 1.3 Patient Service (data inti yang dirujuk banyak service)
- [ ] Domain: `Patient` (Aggregate), `Address`, `EmergencyContact`, `Insurance`
- [ ] `Application`: Register, Update, Search, AssignInsurance
- [ ] Publish `patient.created`, `patient.updated`, `patient.deactivated`
- [ ] Generasi `MedicalRecordNumber`

### 1.4 Doctor Service
- [ ] Domain: `Doctor`, `Specialization`, `DoctorSchedule`, `ScheduleException`
- [ ] `Application`: CRUD dokter, set/manage schedule, availability check
- [ ] Subscribes: `patient.deactivated` (utk cleanup) — opsional
- [ ] Publish `doctor.created`, `doctor.schedule.changed`

---

## Fase 2 — Core Clinical Services

> Bergantung pada Patient & Doctor (Fase 1). Ini alur bisnis utama.

### 2.1 Appointment Service
- [ ] Domain: `Appointment` + state machine (Scheduled → CheckedIn → InProgress → Completed/Cancelled/NoShow)
- [ ] Queue number management
- [ ] Anti double-booking (lock slot)
- [ ] Outbound call ke **Doctor** (get schedule) & **Patient** (get info)
- [ ] Publish: `appointment.booked`, `appointment.cancelled`, `appointment.completed`, `appointment.checked_in`, `appointment.no_show`

### 2.2 Medical Record Service
- [ ] Domain: `MedicalRecord`, `VitalSign`, `Diagnosis` (ICD-10), `Treatment`
- [ ] Access control (dokter/nurse/admin/patient)
- [ ] Subscribes: `appointment.completed` → auto-create record
- [ ] Publish: `medical_record.finalized`, `prescription.order_created`, `lab.order_created`

---

## Fase 3 — Supporting Transactional Services

> Melayani operasi pendukung setelah core clinical berjalan.

### 3.1 Inventory Service (dibuat dulu karena Pharmacy butuh stok)
- [ ] Domain: `InventoryItem`, `StockBatch`, `StockMovement`, `Supplier`
- [ ] FIFO batch dispensing, low-stock & expiring alert
- [ ] REST untuk stok queries (dipanggil Pharmacy)

### 3.2 Pharmacy Service
- [ ] Domain: `Prescription`, `PrescriptionItem`, `Dispensing`, `DrugFormulary`
- [ ] Outbound call ke **Inventory** (check/reduce stock)
- [ ] Subscribes: `prescription.order_created` (dari MedicalRecord)
- [ ] Publish: `prescription.dispensed`, `prescription.created`, `prescription.cancelled`

### 3.3 Laboratory Service
- [ ] Domain: `LabOrder`, `LabOrderTest`, `LabTestResult`, `LabTestCatalog`
- [ ] Subscribes: `lab.order_created` (dari MedicalRecord)
- [ ] Publish: `lab.result_ready`, `lab.result.critical`, `lab.order_created`

### 3.4 Billing Service (paling banyak konsumsi event)
- [ ] Domain: `Invoice`, `InvoiceItem`, `Payment`, `ServiceCatalog`
- [ ] Subscribes: `appointment.completed`, `lab.order_created`, `prescription.dispensed`, `medical_record.treatment_added`
- [ ] Generate & issue invoice, record payment, partial, overdue tracking
- [ ] Publish: `bill.issued`, `payment.received`, `payment.overdue`, `invoice.cancelled`

---

## Fase 4 — Notifikasi & API Gateway

### 4.1 Notification Service (konsumen hampir semua event)
- [ ] Domain: `Notification`, `NotificationType`, `NotificationTemplate`, `NotificationLog`
- [ ] Subscribes semua event penting → render template oleh channel (Email/SMS/Push/InApp)
- [ ] SignalR hub untuk real-time in-app notification
- [ ] Retry + delivery tracking

### 4.2 API Gateway (YARP)
- [ ] Setup route ke 10 service + auth middleware (JWT)
- [ ] Rate limiting, correlation ID
- [ ] CORS untuk frontend (`HMS.WEB`)
- [ ] Error handling terpusat

---

## Fase 5 — Integrasi & Hardening

- [ ] Buat `Dockerfile` di setiap `.Api` (sudah tersedia template dari Patient)
- [ ] Jalankan `docker compose up -d --build` dari `HMS.API/` — semua container hidup
- [ ] End-to-end event flow (contoh: book appointment → invoice → notification → stock)
- [ ] Outbox pattern aktif di semua service (publish)
- [ ] Inbox pattern (idempotency) di semua consumer
- [ ] Observability: Serilog + OpenTelemetry (trace), health endpoints
- [ ] Integration test antar service (Testcontainers)
- [ ] Seed data awal (roles, specializations, service catalog, lab catalog)
- [ ] Security audit (JWT, RBAC policy, data protection)

---

## Fase 6 (Nanti) — Frontend `HMS.WEB`
- [ ] Setup proyek frontend (React/Next/Vue - tentukan)
- [ ] Login & role-based UI (Admin, Doctor, Staff, Patient portal)
- [ ] Integrasi dengan API Gateway `localhost:8080`
- [ ] Real-time notification via SignalR

---

## 📌 Ringkasan Urutan Build (Quick Check)

```
Shared (5 lib)
   ↓
Identity  →  Patient  →  Doctor
   ↓              ↓        ↓
        Appointment
           ↓
        MedicalRecord
           ↓
   Inventory → Pharmacy
        ↓
   Laboratory  →  Billing
        ↓
   Notification
        ↓
   API Gateway (YARP)  →  Docker Compose (semua hidup)
```

> **Golden rule**: *Jangan build service sebelum dependency-nya siap.* Pola urutan di atas menjamin setiap service punya yang ia butuhkan (auth, data master, event, stok) saat diintegrasikan.
