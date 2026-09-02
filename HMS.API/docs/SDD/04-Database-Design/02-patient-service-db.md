# 02. Database Design - Patient Service

**Database Name**: `HMS_Patient`

---

## ERD Overview

```
┌──────────────────┐        ┌─────────────────────┐
│    Patients      │        │  PatientAddresses   │
└──────────────────┘        └─────────────────────┘
     │ 1                          │ 1
     │                            └──────────
     │ 1                                    │ N
┌──────────────────┐        ┌─────────────────────┐
│ PatientContacts  │        │  PatientEmergency   │
└──────────────────┘        └─────────────────────┘
     │ 1
     │ N
┌──────────────────┐        ┌─────────────────────┐
│ PatientInsurance │        │  OutboxEvents       │
└──────────────────┘        └─────────────────────┘
```

---

## Tabel 1: `Patients`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `MedicalRecordNumber` | `NVARCHAR(20)` | No | - | No. Rekam Medis (MR-XXXXXX) |
| `FirstName` | `NVARCHAR(100)` | No | - | Nama depan |
| `LastName` | `NVARCHAR(100)` | Yes | `NULL` | Nama belakang |
| `DateOfBirth` | `DATE` | No | - | Tanggal lahir |
| `Gender` | `NVARCHAR(10)` | No | - | Male / Female / Other |
| `BloodType` | `NVARCHAR(3)` | Yes | `NULL` | A / B / AB / O / A+ dll |
| `NationalIdNumber` | `NVARCHAR(20)` | Yes | `NULL` | Nomor KTP / NIK |
| `PhoneNumber` | `NVARCHAR(20)` | No | - | Nomor HP utama |
| `SecondaryPhone` | `NVARCHAR(20)` | Yes | `NULL` | Nomor HP alternatif |
| `Email` | `NVARCHAR(100)` | Yes | `NULL` | Email pasien |
| `MaritalStatus` | `NVARCHAR(20)` | Yes | `NULL` | Single / Married / Divorced |
| `Nationality` | `NVARCHAR(50)` | No | `'Indonesia'` | Kewarganegaraan |
| `Religion` | `NVARCHAR(50)` | Yes | `NULL` | Agama |
| `Occupation` | `NVARCHAR(50)` | Yes | `NULL` | Pekerjaan |
| `ProfilePhotoUrl` | `NVARCHAR(500)` | Yes | `NULL` | URL foto |
| `Status` | `NVARCHAR(20)` | No | `'Active'` | Active / Inactive / Deceased |
| `IsDeleted` | `BIT` | No | `0` | Soft delete flag |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `ModifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pengubah |
| `DeletedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dihapus |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Patients` | Clustered | `Id` | - | Primary key |
| `UX_Patients_MRNumber` | Unique Nonclustered | `MedicalRecordNumber` | - | No. RM unik |
| `UX_Patients_NationalId` | Unique Nonclustered | `NationalIdNumber` | `WHERE NationalIdNumber IS NOT NULL` | NIK unik |
| `IX_Patients_LastName_FirstName` | Nonclustered | `LastName`, `FirstName` | - | Pencarian by nama |
| `IX_Patients_Phone` | Nonclustered | `PhoneNumber` | - | Pencarian by phone |
| `IX_Patients_DOB` | Nonclustered | `DateOfBirth` | - | Filter umur |
| `IX_Patients_Status` | Nonclustered | `Status`, `IsDeleted` | - | Query aktif |

---

## Tabel 2: `PatientAddresses`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PatientId` | `INT` | No | - | FK -> Patients.Id |
| `AddressType` | `NVARCHAR(20)` | No | `'Home'` | Home / Office / Other |
| `Street` | `NVARCHAR(255)` | No | - | Jalan |
| `City` | `NVARCHAR(100)` | No | - | Kota |
| `Province` | `NVARCHAR(100)` | No | - | Provinsi |
| `SubDistrict` | `NVARCHAR(100)` | Yes | `NULL` | Kelurahan/Kecamatan |
| `PostalCode` | `NVARCHAR(10)` | Yes | `NULL` | Kode pos |
| `Country` | `NVARCHAR(50)` | No | `'Indonesia'` | Negara |
| `IsPrimary` | `BIT` | No | `1` | Alamat utama |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PatientAddresses` | Clustered | `Id` | - | Primary key |
| `IX_PatientAddresses_PatientId` | Nonclustered | `PatientId` | - | Query by patient |
| `IX_PatientAddresses_City` | Nonclustered | `City` | - | Filter by kota |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_PatientAddresses_Patients` | `PatientId` | `Patients.Id` |

---

## Tabel 3: `PatientContacts`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PatientId` | `INT` | No | - | FK -> Patients.Id |
| `ContactType` | `NVARCHAR(20)` | No | - | Phone / Email / WhatsApp |
| `ContactValue` | `NVARCHAR(100)` | No | - | Nilai contact |
| `IsPrimary` | `BIT` | No | `0` | Contact utama |
| `IsVerified` | `BIT` | No | `0` | Sudah verifikasi |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PatientContacts` | Clustered | `Id` | - | Primary key |
| `IX_PatientContacts_PatientId` | Nonclustered | `PatientId` | - | Query by patient |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_PatientContacts_Patients` | `PatientId` | `Patients.Id` |

---

## Tabel 4: `PatientEmergencyContacts`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PatientId` | `INT` | No | - | FK -> Patients.Id |
| `Name` | `NVARCHAR(150)` | No | - | Nama contact darurat |
| `Relationship` | `NVARCHAR(50)` | No | - | Spouse / Parent / Sibling dll |
| `PhoneNumber` | `NVARCHAR(20)` | No | - | Nomor HP |
| `Address` | `NVARCHAR(255)` | Yes | `NULL` | Alamat |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PatientEmergency` | Clustered | `Id` | - | Primary key |
| `IX_PatientEmergency_PatientId` | Nonclustered | `PatientId` | - | Query by patient |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_PatientEmergency_Patients` | `PatientId` | `Patients.Id` |

---

## Tabel 5: `PatientInsurances`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PatientId` | `INT` | No | - | FK -> Patients.Id |
| `ProviderName` | `NVARCHAR(100)` | No | - | BPJS / Asuransi swasta |
| `PolicyNumber` | `NVARCHAR(50)` | No | - | Nomor polis / kartu |
| `CoverageType` | `NVARCHAR(50)` | Yes | `NULL` | Jenis coverage |
| `ValidFrom` | `DATE` | Yes | `NULL` | Berlaku sejak |
| `ValidTo` | `DATE` | Yes | `NULL` | Berlaku sampai |
| `IsActive` | `BIT` | No | `1` | Masih aktif |
| `IsPrimary` | `BIT` | No | `0` | Asuransi utama |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PatientInsurances` | Clustered | `Id` | - | Primary key |
| `IX_PatientInsurances_PatientId` | Nonclustered | `PatientId` | - | Query by patient |
| `IX_PatientInsurances_PolicyNumber` | Nonclustered | `PolicyNumber` | - | Query by polis |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_PatientInsurances_Patients` | `PatientId` | `Patients.Id` |

---

## Tabel 6: `OutboxEvents`

Sama seperti di Identity Service. Lihat `01-identity-service-db.md` untuk definisi.

## Tabel 7: `InboxEvents`

Untuk menghandle idempotency saat menerima event dari service lain.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `UNIQUEIDENTIFIER` | No | `NEWID()` | Primary Key |
| `EventId` | `UNIQUEIDENTIFIER` | No | - | ID event asli dari publisher |
| `EventType` | `NVARCHAR(150)` | No | - | Tipe event |
| `ProcessedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu diproses |
| `Result` | `NVARCHAR(50)` | No | - | Success / Failed |
| `ErrorDetail` | `NVARCHAR(500)` | Yes | `NULL` | Error message |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_InboxEvents` | Clustered | `Id` | - | Primary key |
| `UX_InboxEvents_EventId` | Unique Nonclustered | `EventId` | - | Menghindari duplicate processing |
