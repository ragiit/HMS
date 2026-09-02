# 07. Database Design - Laboratory Service

**Database Name**: `HMS_Laboratory`

---

## ERD Overview

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│  LabOrders   │─────│  LabOrderTests   │─────│ LabTestResults│
└──────────────┘     └──────────────────┘     └──────────────┘
      │ 1                    1
      │                       │ N
      ▼                       ▼
┌──────────────────────────────────────┐
│    LabTestCatalog (Master)           │
└──────────────────────────────────────┘
```

---

## Tabel 1: `LabOrders`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `OrderNumber` | `NVARCHAR(20)` | No | - | No. order lab (LAB-YYYY-XXXXX) |
| `PatientId` | `INT` | No | - | FK referensi Patient Service |
| `DoctorId` | `INT` | No | - | FK referensi Doctor Service |
| `MedicalRecordId` | `BIGINT` | Yes | `NULL` | Reference Medical Record |
| `AppointmentId` | `INT` | Yes | `NULL` | Reference Appointment |
| `OrderDate` | `DATE` | No | - | Tanggal order |
| `Priority` | `NVARCHAR(20)` | No | `'Routine'` | Routine / Urgent / Stat |
| `Status` | `NVARCHAR(20)` | No | `'Ordered'` | Ordered / Collected / InProgress / Completed / Cancelled |
| `SampleCollectedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu sampling |
| `CollectedBy` | `NVARCHAR(100)` | Yes | `NULL` | Petugas sampling |
| `SampleQcResult` | `NVARCHAR(20)` | Yes | `NULL` | Pass / Reject |
| `ClinicalNote` | `NVARCHAR(500)` | Yes | `NULL` | Catatan klinis |
| `DoctorNote` | `NVARCHAR(500)` | Yes | `NULL` | Catatan dokter |
| `BillingReferenceId` | `INT` | Yes | `NULL` | Reference Billing |
| `CancelledBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembatal |
| `CancelledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu batal |
| `CancelledReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan batal |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | No | - | Dokter |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu update |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_LabOrders` | Clustered | `Id` | - | Primary key |
| `UX_LabOrders_Number` | Unique Nonclustered | `OrderNumber` | - | No. unik |
| `IX_LabOrders_PatientId` | Nonclustered | `PatientId`, `OrderDate` | - | Riwayat lab pasien |
| `IX_LabOrders_Status_Date` | Nonclustered | `Status`, `OrderDate` | - | Antrian lab |
| `IX_LabOrders_DoctorId` | Nonclustered | `DoctorId`, `OrderDate` | - | Order per dokter |

---

## Tabel 2: `LabOrderTests`

Detail test yang dipesan per order.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `LabOrderId` | `BIGINT` | No | - | FK -> LabOrders.Id |
| `TestCatalogId` | `INT` | No | - | FK -> LabTestCatalog.Id |
| `TestCode` | `NVARCHAR(20)` | No | - | Copy dari catalog |
| `TestName` | `NVARCHAR(100)` | No | - | Nama test |
| `SpecimenType` | `NVARCHAR(50)` | No | - | Blood / Urine / Stool etc |
| `SpecimenCollectionContainer` | `NVARCHAR(50)` | Yes | `NULL` | Tabung merah / ungu |
| `SpecimenLabel` | `NVARCHAR(20)` | Yes | `NULL` | No. label specimen |
| `Status` | `NVARCHAR(20)` | No | `'Ordered'` | Ordered / Collected / InProgress / Completed |
| `ResultSummary` | `NVARCHAR(MAX)` | Yes | `NULL` | Ringkasan hasil |
| `IsCriticalResult` | `BIT` | No | `0` | Hasil kritis |
| `CriticalValueNote` | `NVARCHAR(200)` | Yes | `NULL` | Catatan hasil kritis |
| `CompletedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu selesai |
| `CompletedBy` | `NVARCHAR(100)` | Yes | `NULL` | Analis lab |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_LabOrderTests` | Clustered | `Id` | - | Primary key |
| `IX_LabOrderTests_OrderId` | Nonclustered | `LabOrderId` | - | Tests per order |
| `IX_LabOrderTests_Status` | Nonclustered | `Status`, `LabOrderId` | - | Tracking per test |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_LabOrderTests_LabOrders` | `LabOrderId` | `LabOrders.Id` |
| `FK_LabOrderTests_Catalog` | `TestCatalogId` | `LabTestCatalog.Id` |

---

## Tabel 3: `LabTestResults`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `LabOrderTestId` | `BIGINT` | No | - | FK -> LabOrderTests.Id |
| `ParameterName` | `NVARCHAR(100)` | No | - | Nama parameter (WBC, Hb, dll) |
| `ParameterCode` | `NVARCHAR(20)` | No | - | Kode parameter |
| `ResultValue` | `NVARCHAR(50)` | No | - | Nilai hasil |
| `Unit` | `NVARCHAR(20)` | Yes | `NULL` | Satuan |
| `ReferenceLow` | `NVARCHAR(20)` | Yes | `NULL` | Batas bawah normal |
| `ReferenceHigh` | `NVARCHAR(20)` | Yes | `NULL` | Batas atas normal |
| `Flag` | `NVARCHAR(10)` | Yes | `NULL` | Normal / High / Low / Critical |
| `Comments` | `NVARCHAR(255)` | Yes | `NULL` | Komentar analis |
| `ResultDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu hasil |
| `ResultBy` | `NVARCHAR(100)` | Yes | `NULL` | Analis |
| `VerifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Verifikator (dokter patologi) |
| `VerifiedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu verifikasi |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_LabTestResults` | Clustered | `Id` | - | Primary key |
| `IX_LabResults_TestId` | Nonclustered | `LabOrderTestId` | - | Results per order test |
| `IX_Results_Parameter` | Nonclustered | `ParameterCode`, `ResultDate` | - | Query historis parameter |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_LabResults_OrderTest` | `LabOrderTestId` | `LabOrderTests.Id` |

---

## Tabel 4: `LabTestCatalog`

Master data pemeriksaan lab.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `TestCode` | `NVARCHAR(20)` | No | - | Kode unik (CBC, GLU, TFT) |
| `TestName` | `NVARCHAR(100)` | No | - | Nama pemeriksaan |
| `Category` | `NVARCHAR(50)` | No | - | Hematologi / Kimia / Urinalysis |
| `SpecimenType` | `NVARCHAR(50)` | No | - | Blood / Urine / Sputum |
| `DefaultUnit` | `NVARCHAR(20)` | Yes | `NULL` | Satuan default |
| `TurnaroundTimeMinutes` | `SMALLINT` | No | `60` | Waktu selesai |
| `Price` | `DECIMAL(12,2)` | No | `0` | Tarif |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_LabTestCatalog` | Clustered | `Id` | - | Primary key |
| `UX_LabTestCatalog_Code` | Unique Nonclustered | `TestCode` | - | Code unik |
| `IX_LabTestCatalog_Category` | Nonclustered | `Category` | - | Filter kategori |

### Seed Data (contoh)
| TestCode | TestName | Category | Specimen | Price |
|---|---|---|---|---|
| `CBC` | Complete Blood Count | Hematologi | Blood | 85,000 |
| `GLU` | Fasting Blood Glucose | Kimia | Blood | 50,000 |
| `UR` | Urinalysis | Urinalysis | Urine | 40,000 |
| `LFT` | Liver Function Test | Kimia | Blood | 200,000 |
| `RFT` | Renal Function Test | Kimia | Blood | 180,000 |
| `TFT` | Thyroid Function Test | Kimia | Blood | 250,000 |

---

## Tabel 5: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 6: `InboxEvents`
Sama seperti Patient Service.
