# 04. Database Design - Appointment Service

**Database Name**: `HMS_Appointment`

---

## ERD Overview

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ AppointmentTypes │     │  Appointments    │─────│ AppointmentHistory│
└──────────────────┘     └──────────────────┘     └──────────────────┘
                            │
                            │
                            ▼
                      ┌──────────────────┐     ┌──────────────────┐
                      │  QueueNumbers    │     │  OutboxEvents    │
                      └──────────────────┘     └──────────────────┘
```

---

## Tabel 1: `Appointments`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `AppointmentNumber` | `NVARCHAR(20)` | No | - | No. Appointment (APT-YYYYMMDD-XXX) |
| `PatientId` | `INT` | No | - | FK referensi Patient Service (Patients.Id) |
| `DoctorId` | `INT` | No | - | FK referensi Doctor Service (Doctors.Id) |
| `ScheduleId` | `INT` | Yes | `NULL` | FK referensi Doctor Schedule (opsional) |
| `AppointmentTypeId` | `INT` | No | - | FK -> AppointmentTypes.Id |
| `AppointmentDate` | `DATE` | No | - | Tanggal appointment |
| `StartTime` | `TIME(0)` | No | - | Jam mulai |
| `EndTime` | `TIME(0)` | No | - | Jam selesai |
| `Status` | `NVARCHAR(20)` | No | `'Scheduled'` | Scheduled / CheckedIn / InProgress / Completed / Cancelled / NoShow |
| `Reason` | `NVARCHAR(500)` | Yes | `NULL` | Keluhan / alasan |
| `Priority` | `NVARCHAR(20)` | No | `'Normal'` | Normal / Urgent / Emergency |
| `ReferralDoctorId` | `INT` | Yes | `NULL` | Dokter referal |
| `ReferralCode` | `NVARCHAR(20)` | Yes | `NULL` | Kode referal |
| `CheckInTime` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu check-in |
| `CheckInBy` | `NVARCHAR(100)` | Yes | `NULL` | User yang check-in |
| `StartedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Konsultasi mulai |
| `CompletedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Konsultasi selesai |
| `CancelledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu cancel |
| `CancelReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan cancel |
| `CancelledBy` | `NVARCHAR(100)` | Yes | `NULL` | Siapa cancel |
| `NoShowReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan no-show |
| `Note` | `NVARCHAR(MAX)` | Yes | `NULL` | Catatan umum |
| `BillingReferenceId` | `INT` | Yes | `NULL` | Reference invoice Billing Service |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembuat booking |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `ModifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pengubah |
| `DeletedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu hapus |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Appointments` | Clustered | `Id` | - | Primary key |
| `UX_Appointments_Number` | Unique Nonclustered | `AppointmentNumber` | - | No. unique |
| `IX_Appointments_PatientId_Date` | Nonclustered | `PatientId`, `AppointmentDate` | - | Riwayat pasien per tanggal |
| `IX_Appointments_DoctorId_Date` | Nonclustered | `DoctorId`, `AppointmentDate`, `Status` | - | Cek jadwal dokter (anti double booking) |
| `IX_Appointments_Date_Status` | Nonclustered | `AppointmentDate`, `Status` | - | Dashboard antrian per hari |
| `IX_Appointments_Status` | Nonclustered | `Status`, `AppointmentDate` | - | Filter status |
| `IX_Appointments_ScheduleId` | Nonclustered | `ScheduleId` | - | Query by jadwal |

---

## Tabel 2: `AppointmentTypes`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Code` | `NVARCHAR(20)` | No | - | Kode (REG, CONSULT, URGENT) |
| `Name` | `NVARCHAR(50)` | No | - | Nama tipe appointment |
| `Description` | `NVARCHAR(200)` | Yes | `NULL` | Deskripsi |
| `DefaultDurationMinutes` | `SMALLINT` | No | `15` | Durasi default |
| `IsActive` | `BIT` | No | `1` | Aktif |

### Seed Data
| Code | Name | DefaultDuration (min) |
|---|---|---|
| `REG` | Regular Check-up | 15 |
| `CONSULT` | Konsultasi | 30 |
| `FOLLOWUP` | Follow-up | 15 |
| `EMERGENCY` | Emergency | 10 |
| `VACCINE` | Vaksinasi | 10 |
| `MEDICAL_CHECK` | Medical Checkup | 60 |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_AppointmentTypes` | Clustered | `Id` | - | Primary key |
| `UX_AppointmentTypes_Code` | Unique Nonclustered | `Code` | - | Code unique |

---

## Tabel 3: `AppointmentHistory`

Menyimpan audit trail perubahan status appointment.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `AppointmentId` | `INT` | No | - | FK -> Appointments.Id |
| `OldStatus` | `NVARCHAR(20)` | Yes | `NULL` | Status sebelumnya |
| `NewStatus` | `NVARCHAR(20)` | No | - | Status baru |
| `ChangedBy` | `NVARCHAR(100)` | No | - | User yang mengubah |
| `ChangedAt` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu perubahan |
| `ChangeNote` | `NVARCHAR(500)` | Yes | `NULL` | Catatan perubahan |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_AppointmentHistory` | Clustered | `Id` | - | Primary key |
| `IX_ApptHistory_AppointmentId` | Nonclustered | `AppointmentId`, `ChangedAt` | - | Audit trail per appointment |
| `IX_ApptHistory_ChangedBy` | Nonclustered | `ChangedBy` | - | Tracking user actions |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_ApptHistory_Appointments` | `AppointmentId` | `Appointments.Id` |

---

## Tabel 4: `QueueNumbers`

Mengelola nomor antrian per hari per dokter.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `AppointmentId` | `INT` | No | - | FK -> Appointments.Id |
| `DoctorId` | `INT` | No | - | Doctor reference |
| `QueueDate` | `DATE` | No | - | Tanggal antrian |
| `QueueNumber` | `INT` | No | - | Nomor urut antrian |
| `Status` | `NVARCHAR(20)` | No | `'Waiting'` | Waiting / Called / Done |
| `CalledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dipanggil |
| `DoneAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu selesai |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_QueueNumbers` | Clustered | `Id` | - | Primary key |
| `UX_Queue_Date_Number` | Unique Nonclustered | `QueueDate`, `DoctorId`, `QueueNumber` | - | No. antrian unik per hari |
| `IX_Queue_Appt` | Nonclustered | `AppointmentId` | - | Query per appointment |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Queue_Appointments` | `AppointmentId` | `Appointments.Id` |

---

## Tabel 5: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 6: `InboxEvents`
Sama seperti Patient Service.
