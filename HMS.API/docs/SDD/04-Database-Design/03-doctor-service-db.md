# 03. Database Design - Doctor Service

**Database Name**: `HMS_Doctor`

---

## ERD Overview

```
┌──────────────────┐     ┌───────────────────────┐
│    Doctors       │     │  DoctorSpecializations │
└──────────────────┘     └───────────────────────┘
     │ 1                            │
     │ N                            │ N
┌──────────────────┐     ┌───────────────────────┐
│ DoctorSchedules  │     │  Specializations       │
└──────────────────┘     └───────────────────────┘
     │ N
     │
     │ N
┌──────────────────┐     ┌───────────────────────┐
│ ScheduleExceptions│    │   OutboxEvents        │
└──────────────────┘     └───────────────────────┘
```

---

## Tabel 1: `Doctors`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `UserId` | `INT` | No | - | FK referensi ke Identity Service (Users.Id) |
| `EmployeeNumber` | `NVARCHAR(20)` | No | - | NIP / Nomor pegawai |
| `FirstName` | `NVARCHAR(100)` | No | - | Nama depan |
| `LastName` | `NVARCHAR(100)` | Yes | `NULL` | Nama belakang |
| `Title` | `NVARCHAR(20)` | No | - | Gelar (dr., drg., prof., dll) |
| `Gender` | `NVARCHAR(10)` | No | - | Male / Female |
| `LicenseNumber` | `NVARCHAR(50)` | Yes | `NULL` | Nomor STR (Surat Tanda Registrasi) |
| `SIPNumber` | `NVARCHAR(50)` | Yes | `NULL` | Nomor SIP (Surat Ijin Praktek) |
| `SIPExpiryDate` | `DATE` | Yes | `NULL` | Berakhir SIP |
| `EducationBackground` | `NVARCHAR(500)` | Yes | `NULL` | Riwayat pendidikan |
| `Biography` | `NVARCHAR(MAX)` | Yes | `NULL` | Biografi profesional |
| `ProfilePhotoUrl` | `NVARCHAR(500)` | Yes | `NULL` | URL foto profil |
| `Email` | `NVARCHAR(100)` | No | - | Email dokter |
| `PhoneNumber` | `NVARCHAR(20)` | Yes | `NULL` | Nomor HP |
| `Status` | `NVARCHAR(20)` | No | `'Active'` | Active / Inactive / OnLeave |
| `IsDeleted` | `BIT` | No | `0` | Soft delete flag |
| `Rating` | `DECIMAL(3,2)` | No | `0.00` | Rata-rata rating |
| `TotalRatings` | `INT` | No | `0` | Jumlah rating |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `ModifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pengubah |
| `DeletedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dihapus |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Doctors` | Clustered | `Id` | - | Primary key |
| `UX_Doctors_UserId` | Unique Nonclustered | `UserId` | - | FK unik ke identity |
| `UX_Doctors_EmployeeNumber` | Unique Nonclustered | `EmployeeNumber` | - | NIP unik |
| `UX_Doctors_SIPNumber` | Unique Nonclustered | `SIPNumber` | `WHERE SIPNumber IS NOT NULL` | SIP unik |
| `IX_Doctors_LastName_FirstName` | Nonclustered | `LastName`, `FirstName` | - | Pencarian by nama |
| `IX_Doctors_Status` | Nonclustered | `Status`, `IsDeleted` | - | Filter active doctors |
| `IX_Doctors_Specialization` | Nonclustered | `Id` | INCLUDE | Optimasi join |

---

## Tabel 2: `Specializations`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Code` | `NVARCHAR(10)` | No | - | Kode unik (GENERAL, CARDIOLOGY) |
| `Name` | `NVARCHAR(100)` | No | - | Nama spesialisasi |
| `Description` | `NVARCHAR(500)` | Yes | `NULL` | Deskripsi spesialisasi |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Specializations` | Clustered | `Id` | - | Primary key |
| `UX_Specializations_Code` | Unique Nonclustered | `Code` | - | Kode unik |
| `UX_Specializations_Name` | Unique Nonclustered | `Name` | - | Nama unik |

### Seed Data (Spesialisasi)
| Code | Name | Description |
|---|---|---|
| `GENERAL` | Dokter Umum | Pelayanan medis umum |
| `PD` | Penyakit Dalam | Konsultasi penyakit dalam |
| `KANDUNG` | Kandungan (Obgyn) | Kesehatan ibu & anak |
| `ANAK` | Anak (Pediatric) | Kesehatan anak |
| `JANTUNG` | Jantung | Kardiologi |
| `SYARAF` | Saraf | Neurologi |
| `GIGI` | Gigi | Kesehatan gigi & mulut |
| `MATA` | Mata | Oftalmologi |
| `THT` | THT | Telinga hidung tenggorokan |
| `KULIT` | Kulit & Kelamin | Dermatologi |

---

## Tabel 3: `DoctorSpecializations` (Join Table)

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `DoctorId` | `INT` | No | - | FK -> Doctors.Id |
| `SpecializationId` | `INT` | No | - | FK -> Specializations.Id |
| `IsPrimary` | `BIT` | No | `0` | Spesialisasi utama |
| `IsBoardCertified` | `BIT` | No | `0` | Sertifikasi |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_DoctorSpecializations` | Clustered | `DoctorId`, `SpecializationId` | - | Composite PK |
| `IX_DoctorSpec_SpecializationId` | Nonclustered | `SpecializationId` | - | Cari per spesialisasi |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_DoctorSpec_Doctors` | `DoctorId` | `Doctors.Id` |
| `FK_DoctorSpec_Specs` | `SpecializationId` | `Specializations.Id` |

---

## Tabel 4: `DoctorSchedules`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `DoctorId` | `INT` | No | - | FK -> Doctors.Id |
| `DayOfWeek` | `TINYINT` | No | - | 1=Monday ... 7=Sunday |
| `StartTime` | `TIME(0)` | No | - | Jam mulai |
| `EndTime` | `TIME(0)` | No | - | Jam selesai |
| `SlotDurationMinutes` | `SMALLINT` | No | `30` | Durasi per slot (menit) |
| `MaxPatientsPerSlot` | `SMALLINT` | No | `1` | Max pasien per slot |
| `MaxDailyPatients` | `SMALLINT` | Yes | `NULL` | Max pasien per hari |
| `Location` | `NVARCHAR(100)` | Yes | `NULL` | Lokasi praktek |
| `RoomNumber` | `NVARCHAR(20)` | Yes | `NULL` | Nomor ruangan |
| `IsActive` | `BIT` | No | `1` | Jadwal aktif |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_DoctorSchedules` | Clustered | `Id` | - | Primary key |
| `IX_DoctorSched_DoctorId_Day` | Nonclustered | `DoctorId`, `DayOfWeek`, `IsActive` | - | Cari jadwal dokter per hari |
| `IX_DoctorSched_DayOfWeek` | Nonclustered | `DayOfWeek` | - | Filter per hari |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_DoctorSchedules_Doctors` | `DoctorId` | `Doctors.Id` |

### Contoh Data
| DoctorId | DayOfWeek | StartTime | EndTime | SlotDuration | Room |
|---|---|---|---|---|---|
| 1 | 1 (Senin) | 08:00 | 12:00 | 30 | R-101 |
| 1 | 1 (Senin) | 19:00 | 21:00 | 30 | R-102 |
| 1 | 3 (Rabu) | 08:00 | 12:00 | 30 | R-101 |

**Constraint Check**: `StartTime < EndTime`, `SlotDurationMinutes > 0`

---

## Tabel 5: `ScheduleExceptions`

Menyimpan jadwal yang di-cancel/ubah untuk tanggal tertentu.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `DoctorId` | `INT` | No | - | FK -> Doctors.Id |
| `ScheduleId` | `INT` | Yes | `NULL` | FK -> DoctorSchedules.Id (jika dari jadwal reguler) |
| `ExceptionDate` | `DATE` | No | - | Tanggal exception |
| `StartTime` | `TIME(0)` | Yes | `NULL` | Waktu mulai (jika berubah) |
| `EndTime` | `TIME(0)` | Yes | `NULL` | Waktu selesai |
| `IsCancelled` | `BIT` | No | `0` | Apakah dibatalkan seluruh |
| `Reason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | Yes | `NULL` | Yang membuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_ScheduleExceptions` | Clustered | `Id` | - | Primary key |
| `IX_Exceptions_DoctorDate` | Nonclustered | `DoctorId`, `ExceptionDate` | - | Cari by doctor & date |
| `IX_Exceptions_Date` | Nonclustered | `ExceptionDate` | - | Filter per tanggal |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Exceptions_Doctors` | `DoctorId` | `Doctors.Id` |

---

## Tabel 6: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 7: `InboxEvents`
Sama seperti Patient Service.
