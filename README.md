# 🏥 HMS - Hospital Management System

Hospital Management System (HMS) menggunakan **Microservices Architecture** dengan **.NET 10**.

Monorepo ini dibagi menjadi beberapa proyek terpisah:

| Folder | Deskripsi | Status |
|---|---|---|
| `HMS.API/` | **Backend** — Microservices & API Gateway (.NET 10) | 🟢 Aktif |
| `HMS.WEB/` | **Frontend** — Web application (akan dibuat) | ⏳ Rencana |

---

## 📚 Documentation

- 🗺️ **Roadmap Pengembangan**: [`ROADMAP.md`](ROADMAP.md)
- 📄 **Software Design Document (SDD)**: [`HMS.API/docs/SDD`](HMS.API/docs/SDD)
- 📋 **Panduan Mulai SDD**: [`HMS.API/docs/SDD/README.md`](HMS.API/docs/SDD/README.md)
- 🐳 **Environment Setup**: [`HMS.API/docs/env-setup.md`](HMS.API/docs/env-setup.md)

---
## 🏗️ Architecture Overview (Backend)

- **Framework**: .NET 10
- **Database**: SQL Server 2022+ (Database per Service pattern)
- **ORM**: Entity Framework Core 10 (Code-First)
- **Architecture**: Microservices + DDD + CQRS + Clean Architecture
- **API Gateway**: YARP
- **Message Bus**: RabbitMQ (with Outbox Pattern)
- **Cache**: Redis

---

## 📂 Struktur Backend (`HMS.API/`)

