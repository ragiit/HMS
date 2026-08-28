# 06. Database Design - Pharmacy Service

**Database Name**: `HMS_Pharmacy`

---

## ERD Overview

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│Prescriptions│─────│ PrescriptionItems│─────│ Dispensing  │
└─────────────┘     └──────────────────┘     └─────────────┘
      │ 1                      1
      │                        │ N
      ▼                        ▼
┌──────────────────────────────────────┐     ┌──────────────────┐
│  Drug Formularies                     │     │  OutboxEvents    │
└──────────────────────────────────────┘     └──────────────────┘
```

---

## Tabel 1: `Prescriptions`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PrescriptionNumber` | `NVARCHAR(20)` | No | - | No. resep (RCP-YYYY-XXXXX) |
| `MedicalRecordId` | `BIGINT` | No | - | FK referensi Medical Record Service |
| `AppointmentId` | `INT` | Yes | `NULL` | Reference Appointment |
| `PatientId` | `INT` | No | - | FK referensi Patient Service |
| `DoctorId` | `INT` | No | - | FK referensi Doctor Service |
| `PrescriptionDate` | `DATE` | No | - | Tanggal resep |
| `Status` | `NVARCHAR(20)` | No | `'Pending'` | Pending / Dispensed / Cancelled / PartiallyDispensed |
| `Priority` | `NVARCHAR(20)` | No | `'Normal'` | Normal / Urgent / Stat |
| `Notes` | `NVARCHAR(500)` | Yes | `NULL` | Catatan dokter |
| `DispensingInstructions` | `NVARCHAR(500)` | Yes | `NULL` | Instruksi dispensing |
| `DispensedBy` | `NVARCHAR(100)` | Yes | `NULL` | Apoteker yang dispense |
| `DispensedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dispense |
| `CancelledBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembatal |
| `CancelledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu batal |
| `CancelReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan batal |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | No | - | Dokter penulis |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Prescriptions` | Clustered | `Id` | - | Primary key |
| `UX_Prescriptions_Number` | Unique Nonclustered | `PrescriptionNumber` | - | No. resep unik |
| `IX_Presc_PatientId` | Nonclustered | `PatientId`, `PrescriptionDate` | - | Riwayat resep pasien |
| `IX_Presc_Status` | Nonclustered | `Status`, `PrescriptionDate` | - | Antrian dispensing |
| `IX_Presc_MedRecId` | Nonclustered | `MedicalRecordId` | - | Query per rekam medis |

---

## Tabel 2: `PrescriptionItems`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PrescriptionId` | `BIGINT` | No | - | FK -> Prescriptions.Id |
| `InventoryItemId` | `INT` | No | - | FK referensi Inventory Service |
| `MedicationName` | `NVARCHAR(200)` | No | - | Nama obat (denormalize) |
| `GenericName` | `NVARCHAR(200)` | Yes | `NULL` | Nama generik |
| `Strength` | `NVARCHAR(50)` | Yes | `NULL` | Dosis kekuatan (500mg) |
| `DosageForm` | `NVARCHAR(50)` | Yes | `NULL` | Tablet / Syrup / Injeksi |
| `Dosage` | `NVARCHAR(100)` | No | - | Aturan pakai (3x1) |
| `Frequency` | `NVARCHAR(50)` | Yes | `NULL` | Frekuensi |
| `Route` | `NVARCHAR(50)` | Yes | `NULL` | Oral / Topikal / IV |
| `DurationDays` | `SMALLINT` | No | - | Lama pemberian |
| `Quantity` | `DECIMAL(10,2)` | No | - | Jumlah obat |
| `Unit` | `NVARCHAR(20)` | No | `'pcs'` | Satuan |
| `Instructions` | `NVARCHAR(500)` | Yes | `NULL` | Instruksi minum |
| `IsDispensed` | `BIT` | No | `0` | Sudah dispense |
| `DispensedQuantity` | `DECIMAL(10,2)` | Yes | `NULL` | Jumlah dispense |
| `SubstituteAllowed` | `BIT` | No | `0` | Boleh ganti generic |
| `LineNumber` | `SMALLINT` | No | - | Nomor urut item |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PrescItems` | Clustered | `Id` | - | Primary key |
| `IX_PrescItems_PrescId` | Nonclustered | `PrescriptionId` | - | Items per prescription |
| `IX_PrescItems_InventoryId` | Nonclustered | `InventoryItemId` | - | Query per drug |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_PrescItems_Prescriptions` | `PrescriptionId` | `Prescriptions.Id` |

---

## Tabel 3: `Dispensing`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `PrescriptionId` | `BIGINT` | No | - | FK -> Prescriptions.Id |
| `DispensingNumber` | `NVARCHAR(20)` | No | - | No. dispensing |
| `DispensedByUserId` | `INT` | No | - | Apoteker (Identity user) |
| `DispensedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dispensing |
| `Status` | `NVARCHAR(20)` | No | `'Completed'` | Completed / Partial |
| `TotalQuantity` | `DECIMAL(10,2)` | No | - | Total obat |
| `Notes` | `NVARCHAR(500)` | Yes | `NULL` | Catatan apoteker |
| `BillingReferenceId` | `INT` | Yes | `NULL` | Reference ke Billing |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Dispensing` | Clustered | `Id` | - | Primary key |
| `UX_Dispensing_Number` | Unique Nonclustered | `DispensingNumber` | - | No. unik |
| `IX_Dispending_PrescId` | Nonclustered | `PrescriptionId` | - | Query per resep |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Dispensing_Prescriptions` | `PrescriptionId` | `Prescriptions.Id` |

---

## Tabel 4: `DrugFormularies`

Master data obat internal (untuk lookup saat dokter menulis resep).

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Code` | `NVARCHAR(20)` | No | - | Kode obat |
| `Name` | `NVARCHAR(200)` | No | - | Nama obat (merk) |
| `GenericName` | `NVARCHAR(200)` | No | - | Nama generik |
| `Category` | `NVARCHAR(50)` | Yes | `NULL` | Kategori (Antibiotik, Analgesik) |
| `Strength` | `NVARCHAR(50)` | Yes | `NULL` | Kekuatan |
| `DosageForm` | `NVARCHAR(50)` | Yes | `NULL` | Bentuk sediaan |
| `UnitPrice` | `DECIMAL(12,2)` | No | `0` | Harga satuan |
| `IsGenericAvailable` | `BIT` | No | `1` | Tersedia generik |
| `RequiresPrescription` | `BIT` | No | `1` | Obat keras/prescription |
| `StockAlertLevel` | `INT` | No | `10` | Level notifikasi stok |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_DrugFormularies` | Clustered | `Id` | - | Primary key |
| `UX_Drug_Code` | Unique Nonclustered | `Code` | - | Code unik |
| `IX_Drug_Name` | Nonclustered | `Name`, `GenericName` | - | Pencarian obat |
| `IX_Drug_Category` | Nonclustered | `Category` | - | Filter kategori |

---

## Tabel 5: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 6: `InboxEvents`
Sama seperti Patient Service.
