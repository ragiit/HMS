# HMS - Hospital Management System

Hospital Management System (HMS) menggunakan **Microservices Architecture** dengan **.NET 10**.

## 📚 Documentation

- 📄 **Software Design Document (SDD)**: [`/docs/SDD`](/docs/SDD)
- 📋 **Panduan Mulai**: Mulai dari [`docs/SDD/README.md`](/docs/SDD/README.md)

## 🏗️ Architecture Overview
- **Framework**: .NET 10
- **Database**: SQL Server 2022+ (Database per Service pattern)
- **ORM**: Entity Framework Core 10 (Code-First)
- **Architecture**: Microservices + DDD + CQRS + Clean Architecture
- **API Gateway**: YARP
- **Message Bus**: RabbitMQ (with Outbox Pattern)
- **Cache**: Redis

## 📦 Services (10 total)
| # | Service | Database |
|---|---|---|
| 1 | Identity Service | `HMS_Identity` |
| 2 | Patient Service | `HMS_Patient` |
| 3 | Doctor Service | `HMS_Doctor` |
| 4 | Appointment Service | `HMS_Appointment` |
| 5 | Medical Record Service | `HMS_MedicalRecord` |
| 6 | Pharmacy Service | `HMS_Pharmacy` |
| 7 | Laboratory Service | `HMS_Laboratory` |
| 8 | Billing Service | `HMS_Billing` |
| 9 | Inventory Service | `HMS_Inventory` |
| 10 | Notification Service | `HMS_Notification` |

## 📁 Steps
1. Baca **SDD** secara menyeluruh
2. Buat solution structure untuk setiap service
3. Implementasi Domain Layer (DDD)
4. Implementasi Infrastructure Layer (EF Core)
5. Implementasi Application Layer (CQRS)
6. Implementasi API Layer
7. Setup RabbitMQ, Redis, SQL Server (Docker Compose)
8. Integrate API Gateway (YARP)

