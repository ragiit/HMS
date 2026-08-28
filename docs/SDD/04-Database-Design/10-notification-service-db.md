# 10. Database Design - Notification Service

**Database Name**: `HMS_Notification`

---

## ERD Overview

```
┌──────────────────┐     ┌──────────────────┐
│ NotificationTypes│     │ Notifications    │
└──────────────────┘     └──────────────────┘
                              │ 1
                              │ N
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ NotificationTemps│     │ NotificationLogs │     │ DeliveryAttempts │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

---

## Tabel 1: `Notifications`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `NotificationNumber` | `NVARCHAR(30)` | No | - | No. unique notifikasi |
| `TypeCode` | `NVARCHAR(30)` | No | - | Kode tipe (APPOINTMENT_REMINDER, etc) |
| `RecipientUserId` | `INT` | No | - | User penerima |
| `RecipientEmail` | `NVARCHAR(100)` | Yes | `NULL` | Email penerima |
| `RecipientPhone` | `NVARCHAR(20)` | Yes | `NULL` | Phone penerima |
| `Subject` | `NVARCHAR(200)` | No | - | Judul/subject |
| `Body` | `NVARCHAR(MAX)` | No | - | Isi pesan (personalized) |
| `TemplateCode` | `NVARCHAR(50)` | Yes | `NULL` | Template yang dipakai |
| `Payload` | `NVARCHAR(MAX)` | Yes | `NULL` | Data payload event |
| `Priority` | `NVARCHAR(20)` | No | `'Normal'` | Normal / High / Critical |
| `Channel` | `NVARCHAR(30)` | No | `'Email'` | Email / SMS / Push / InApp |
| `IsRead` | `BIT` | No | `0` | Sudah dibaca |
| `ReadAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dibaca |
| `ScheduledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Jadwal kirim |
| `ReceivedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diterima |
| `DeliveredAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu terkirim |
| `FailedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu gagal |
| `FailureReason` | `NVARCHAR(500)` | Yes | `NULL` | Alasan gagal |
| `DeliveryStatus` | `NVARCHAR(20)` | No | `'Pending'` | Pending / Sending / Delivered / Failed |
| `RetryCount` | `INT` | No | `0` | Jumlah retry |
| `SourceService` | `NVARCHAR(50)` | No | - | ID service pengirim |
| `CorrelationId` | `NVARCHAR(100)` | Yes | `NULL` | Tracing |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Notifications` | Clustered | `Id` | - | Primary key |
| `UX_Notifications_Number` | Unique Nonclustered | `NotificationNumber` | - | No. unik |
| `IX_Notif_Recipient` | Nonclustered | `RecipientUserId`, `CreatedDate` | - | Inbox user |
| `IX_Notif_Status` | Nonclustered | `DeliveryStatus`, `ScheduledAt` | - | Worker daemon polling |
| `IX_Notif_Read` | Nonclustered | `IsRead`, `RecipientUserId` | - | Unread count |

---

## Tabel 2: `NotificationTypes`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Code` | `NVARCHAR(30)` | No | - | Kode unik |
| `Name` | `NVARCHAR(100)` | No | - | Nama tipe |
| `Description` | `NVARCHAR(200)` | Yes | `NULL` | Deskripsi |
| `DefaultChannel` | `NVARCHAR(30)` | No | `'Email'` | Channel default |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Seed Data
| Code | Name | DefaultChannel |
|---|---|---|
| `APPOINTMENT_REMINDER` | Pengingat Appointment | SMS |
| `APPOINTMENT_CONFIRMATION` | Konfirmasi Booking | Email |
| `APPOINTMENT_CANCELLED` | Appointment Dibatalkan | Email |
| `LAB_RESULT_READY` | Hasil Lab Tersedia | Push |
| `PRESCRIPTION_READY` | Resep Siap Diambil | SMS |
| `BILL_ISSUED` | Tagihan Diterbitkan | Email |
| `PAYMENT_RECEIVED` | Pembayaran Diterima | Email |
| `PAYMENT_OVERDUE` | Tagihan Jatuh Tempo | SMS |
| `INVENTORY_LOW_STOCK` | Stok Menipis | InApp |
| `MEDICATION_REMINDER` | Pengingat Minum Obat | Push |
| `WELCOME_MESSAGE` | Selamat Datang Pasien | Email |
| `ACCOUNT_ACTIVITY` | Aktivitas Keamanan | Email |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_NotificationTypes` | Clustered | `Id` | - | Primary key |
| `UX_NotifTypes_Code` | Unique Nonclustered | `Code` | - | Code unik |

---

## Tabel 3: `NotificationTemplates`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `TypeCode` | `NVARCHAR(30)` | No | - | Referensi ke Type |
| `Channel` | `NVARCHAR(30)` | No | - | Email / SMS / Push / InApp |
| `SubjectTemplate` | `NVARCHAR(200)` | No | - | Template subject |
| `BodyTemplate` | `NVARCHAR(MAX)` | No | - | Template body (razor/simple substitution) |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `Language` | `NVARCHAR(5)` | No | `'id-ID'` | Bahasa |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_NotifTemplates` | Clustered | `Id` | - | Primary key |
| `UX_NotifTemplates_Type_Channel_Lang` | Unique Nonclustered | `TypeCode`, `Channel`, `Language` | - | Template unik |

---

## Tabel 4: `NotificationLogs`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `NotificationId` | `BIGINT` | No | - | FK -> Notifications.Id |
| `Provider` | `NVARCHAR(50)` | No | - | SMTP / Twilio / FCM / SignalR |
| `ProviderReferenceId` | `NVARCHAR(100)` | Yes | `NULL` | ID dari provider |
| `Status` | `NVARCHAR(20)` | No | - | Sending / Sent / Failed |
| `HttpStatusCode` | `INT` | Yes | `NULL` | HTTP status provider |
| `ErrorMessage` | `NVARCHAR(500)` | Yes | `NULL` | Error |
| `AttemptNumber` | `INT` | No | `1` | Attempt ke-berapa |
| `AttemptedAt` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu kirim |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_NotifLogs` | Clustered | `Id` | - | Primary key |
| `IX_NotifLogs_NotificationId` | Nonclustered | `NotificationId` | - | Logs per notification |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_NotifLogs_Notifications` | `NotificationId` | `Notifications.Id` |

---

## Tabel 5: `OutboxEvents` & `InboxEvents`
Sama seperti service lain.

## Tabel 6: `InboxEvents`
Sama seperti Patient Service.
