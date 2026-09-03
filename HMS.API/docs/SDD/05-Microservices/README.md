# 05. Microservices Detail

Dokumentasi detail setiap microservice meliputi:

> **Interpretasi status**:
> - ✅ **Implemented** = sudah dikoding; dokumen disinkron 1:1 dengan kode.
> - 🔲 **Blueprint** = belum dikoding; dokumen adalah rancangan implementable (siap dipakai sbg panduan) namun contract dapat berubah saat implement.

| # | Service | File | Status | Catatan |
|---|---|---|---|---|
| 1 | Identity Service | `01-identity-service.md` | ✅ Implemented | Satu-satunya service yang sudah dikoding (Contoh pola HMS) |
| 2 | Patient Service | `02-patient-service.md` | 🔲 Blueprint | |
| 3 | Doctor Service | `03-doctor-service.md` | 🔲 Blueprint | |
| 4 | Appointment Service | `04-appointment-service.md` | 🔲 Blueprint | |
| 5 | Medical Record Service | `05-medical-record-service.md` | 🔲 Blueprint | |
| 6 | Pharmacy Service | `06-pharmacy-service.md` | 🔲 Blueprint | |
| 7 | Laboratory Service | `07-laboratory-service.md` | 🔲 Blueprint | |
| 8 | Billing Service | `08-billing-service.md` | 🔲 Blueprint | |
| 9 | Inventory Service | `09-inventory-service.md` | 🔲 Blueprint | |
| 10 | Notification Service | `10-notification-service.md` | 🔲 Blueprint | |

## Template Struktur Document Per Service

Setiap dokumen service mengikuti template berikut:
1. **Overview** - Tanggung jawab service
2. **Bounded Context** - Batasan domain
3. **Domain Model** - Entitas, agregat, value objects
4. **API Endpoints** - Referensi ke API Design
5. **Events (Publish/Subscribe)** - Event yang dikirim/diterima
6. **CQRS Handlers** - Command & Query
7. **Database** - Referensi ke Database Design
8. **Dependencies** - Service lain yang di-call
9. **Message Contracts** - Event payload schema
