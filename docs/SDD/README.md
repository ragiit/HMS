# HMS - Hospital Management System
## Software Design Document (SDD)

Dokumen ini berisi rancangan lengkap Hospital Management System (HMS) menggunakan arsitektur **Microservices** dengan **.NET 10**.

---

## 📂 Struktur Dokumentasi

| Folder/File | Deskripsi |
|---|---|
| `00-Project-Overview/` | Ringkasan proyek, tujuan, visi misi |
| `01-System-Architecture/` | Arsitektur sistem, pola desain, deployment |
| `02-Technology-Stack/` | Daftar teknologi dan framework yang digunakan |
| `03-API-Design/` | Rancangan API endpoints untuk semua service |
| `04-Database-Design/` | Design database per service (format tabel) |
| `05-Microservices/` | Dokumentasi detail setiap microservice |
| `06-Messaging/` | Desain message broker, event, integrasi antar service |
| `07-Cross-Cutting/` | Aspek lintas fungsi: security, logging, caching |

---

## 📑 Daftar Dokumen SDD

| # | Nama File | Lokasi |
|---|---|---|
| 1 | Project Overview | `00-Project-Overview/01-project-overview.md` |
| 2 | System Architecture | `01-System-Architecture/01-system-architecture.md` |
| 3 | Database Per Service Pattern | `01-System-Architecture/02-database-per-service.md` |
| 4 | Technology Stack | `02-Technology-Stack/01-technology-stack.md` |
| 5 | API Gateway Design | `03-API-Design/01-api-gateway-yarp.md` |
| 6 | API Endpoints | `03-API-Design/02-api-endpoints.md` |
| 7 | Identity Service DB | `04-Database-Design/01-identity-service-db.md` |
| 8 | Patient Service DB | `04-Database-Design/02-patient-service-db.md` |
| 9 | Doctor Service DB | `04-Database-Design/03-doctor-service-db.md` |
| 10 | Appointment Service DB | `04-Database-Design/04-appointment-service-db.md` |
| 11 | Medical Record Service DB | `04-Database-Design/05-medical-record-service-db.md` |
| 12 | Pharmacy Service DB | `04-Database-Design/06-pharmacy-service-db.md` |
| 13 | Laboratory Service DB | `04-Database-Design/07-laboratory-service-db.md` |
| 14 | Billing Service DB | `04-Database-Design/08-billing-service-db.md` |
| 15 | Inventory Service DB | `04-Database-Design/09-inventory-service-db.md` |
| 16 | Notification Service DB | `04-Database-Design/10-notification-service-db.md` |
| 17 | Microservices Detail | `05-Microservices/01-identity-service.md` (dst per service) |
| 18 | Event Messaging | `06-Messaging/01-event-messaging-design.md` |
| 19 | Cross-Cutting Concerns | `07-Cross-Cutting/01-security-logging-caching.md` |

---

## 📌 Cara Menggunakan

1. Mulai dari **`00-Project-Overview`** untuk memahami pembaruan project
2. Baca **`01-System-Architecture`** untuk arsitektur keseluruhan
3. Review **`02-Technology-Stack`** untuk stack teknologi
4. Lihat **`03-API-Design`** untuk rancangan API
5. Gunakan **`04-Database-Design`** sebagai referensi pembuatan EF Core migration
6. Dokumentasi setiap service ada di **`05-Microservices`**

---

## 🔧 Spesifikasi Teknis Singkat

| Kategori | Keputusan |
|---|---|
| Framework | .NET 10 |
| Database | SQL Server 2022+ |
| ORM | EF Core 10 (Code-First) |
| Arsitektur | Microservices + DDD + CQRS |
| API Gateway | YARP |
| Message Bus | RabbitMQ |
| Cache | Redis |
| Authentication | JWT (per service) + Refresh Token |
