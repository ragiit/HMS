# 05. Database Design - Medical Record Service

**Database Name**: `HMS_MedicalRecord`

---

## ERD Overview

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│MedicalRecords│─────│ VitalSigns       │     │  Diagnoses   │
└──────────────┘     └──────────────────┘     └──────────────┘
      │ 1                1
      │ N                N
┌──────────────┐     ┌──────────────────┐
│ Treatments   │     │  Prescription    │
└──────────────┘     └──────────────────┘
      │ 1                    1
      │ N                    N
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│ LabOrdersRef │     │  ICD10Codes      │     │ DiagnosticImg│
└──────────────┘     └──────────────────┘     └──────────────┘
```

---

## Tabel 1: `MedicalRecords`

Rekam medis inti untuk setiap kunjungan pasien.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `RecordNumber` | `NVARCHAR(20)` | No | - | No. rekam (MR-YYYY-XXXXX) |
| `PatientId` | `INT` | No | - | FK referensi Patient Service |
| `DoctorId` | `INT` | No | - | FK referensi Doctor Service |
| `AppointmentId` | `INT` | Yes | `NULL` | FK referensi Appointment |
| `VisitDate` | `DATE` | No | - | Tanggal kunjungan |
| `VisitType` | `NVARCHAR(20)` | No | `'Outpatient'` | Outpatient / Inpatient / Emergency |
| `Department` | `NVARCHAR(50)` | Yes | `NULL` | Departemen (Poli Umum dll) |
| `Subjective` | `NVARCHAR(MAX)` | Yes | `NULL` | Anamnesa (keluhan pasien) |
| `Objective` | `NVARCHAR(MAX)` | Yes | `NULL` | Temuan pemeriksaan fisik |
| `Assessment` | `NVARCHAR(MAX)` | Yes | `NULL` | Analisis / diagnosis |
| `Plan` | `NVARCHAR(MAX)` | Yes | `NULL` | Rencana tindakan / terapi |
| `Summary` | `NVARCHAR(MAX)` | Yes | `NULL` | Ringkasan |
| `IsConfidential` | `BIT` | No | `0` | Rekam medis rahasia |
| `Status` | `NVARCHAR(20)` | No | `'Draft'` | Draft / Finalized / Closed |
| `FinalizedBy` | `NVARCHAR(100)` | Yes | `NULL` | Dokter yang finalize |
| `FinalizedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu finalize |
| `FollowUpNeeded` | `BIT` | No | `0` | Perlu follow-up |
| `FollowUpDate` | `DATE` | Yes | `NULL` | Tanggal follow-up |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | No | - | Pembuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `ModifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pengubah |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_MedicalRecords` | Clustered | `Id` | - | Primary key |
| `UX_MedRec_Number` | Unique Nonclustered | `RecordNumber` | - | No. rekam unik |
| `IX_MedRec_PatientId_Date` | Nonclustered | `PatientId`, `VisitDate` | - | Riwayat pasien |
| `IX_MedRec_DoctorId_Date` | Nonclustered | `DoctorId`, `VisitDate` | - | Kunjungan dokter |
| `IX_MedRec_AppointmentId` | Nonclustered | `AppointmentId` | `WHERE AppointmentId IS NOT NULL` | Map ke appointment |
| `IX_MedRec_Status` | Nonclustered | `Status`, `VisitDate` | - | Filter status |

---

## Tabel 2: `VitalSigns`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `MedicalRecordId` | `BIGINT` | No | - | FK -> MedicalRecords.Id |
| `PatientId` | `INT` | No | - | Patient reference |
| `RecordedAt` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu pengukuran |
| `Temperature` | `DECIMAL(4,1)` | Yes | `NULL` | Suhu (°C) |
| `Systolic` | `SMALLINT` | Yes | `NULL` | Tekanan sistolik (mmHg) |
| `Diastolic` | `SMALLINT` | Yes | `NULL` | Tekanan diastolik (mmHg) |
| `HeartRate` | `SMALLINT` | Yes | `NULL` | Denyut jantung (bpm) |
| `RespiratoryRate` | `SMALLINT` | Yes | `NULL` | Laju pernapasan |
| `OxygenSaturation` | `TINYINT` | Yes | `NULL` | SpO2 (%) |
| `WeightKg` | `DECIMAL(5,2)` | Yes | `NULL` | Berat badan (kg) |
| `HeightCm` | `DECIMAL(5,2)` | Yes | `NULL` | Tinggi badan (cm) |
| `BMI` | `DECIMAL(4,2)` | Yes | `NULL` | Body Mass Index |
| `BloodGlucose` | `DECIMAL(5,1)` | Yes | `NULL` | Gula darah (mg/dL) |
| `PainScale` | `TINYINT` | Yes | `NULL` | Skala nyeri 0-10 |
| `RecordedBy` | `NVARCHAR(100)` | No | - | Yang merekam |
| `Notes` | `NVARCHAR(255)` | Yes | `NULL` | Catatan |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_VitalSigns` | Clustered | `Id` | - | Primary key |
| `IX_VitalSigns_MedRecordId` | Nonclustered | `MedicalRecordId` | - | Query per rekam |
| `IX_VitalSigns_PatientId_Date` | Nonclustered | `PatientId`, `RecordedAt` | - | Trend vital pasien |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_VitalSigns_MedRec` | `MedicalRecordId` | `MedicalRecords.Id` |

---

## Tabel 3: `Diagnoses`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `MedicalRecordId` | `BIGINT` | No | - | FK -> MedicalRecords.Id |
| `ICD10Code` | `NVARCHAR(10)` | No | - | Kode diagnosis ICD-10 |
| `DiagnosisName` | `NVARCHAR(200)` | No | - | Nama diagnosis |
| `IsPrimary` | `BIT` | No | `0` | Diagnosis utama |
| `DiagnosisType` | `NVARCHAR(20)` | No | `'Working'` | Working / Final / Rule-Out |
| `CreatedBy` | `NVARCHAR(100)` | No | - | Dokter |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Diagnoses` | Clustered | `Id` | - | Primary key |
| `IX_Diag_MedRecordId` | Nonclustered | `MedicalRecordId` | - | Query per rekam |
| `IX_Diag_ICD10` | Nonclustered | `ICD10Code` | - | Statistik diagnosis |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Diag_MedRec` | `MedicalRecordId` | `MedicalRecords.Id` |

---

## Tabel 4: `Treatments`

Prosedur / tindakan yang dilakukan.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `MedicalRecordId` | `BIGINT` | No | - | FK -> MedicalRecords.Id |
| `TreatmentCode` | `NVARCHAR(20)` | No | - | Kode tindakan |
| `TreatmentName` | `NVARCHAR(200)` | No | - | Nama tindakan |
| `Description` | `NVARCHAR(500)` | Yes | `NULL` | Deskripsi |
| `PerformedBy` | `NVARCHAR(100)` | No | - | Dokter/perawat |
| `PerformedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu tindakan |
| `Result` | `NVARCHAR(500)` | Yes | `NULL` | Hasil tindakan |
| `BillingReferenceId` | `INT` | Yes | `NULL` | Reference ke Billing |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Treatments` | Clustered | `Id` | - | Primary key |
| `IX_Treatment_MedRecordId` | Nonclustered | `MedicalRecordId` | - | Query per rekam |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Treatment_MedRec` | `MedicalRecordId` | `MedicalRecords.Id` |

---

## Tabel 5: `PrescriptionOrders`

Order resep yang di-forward ke Pharmacy Service.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `MedicalRecordId` | `BIGINT` | No | - | FK -> MedicalRecords.Id |
| `PrescriptionNumber` | `NVARCHAR(20)` | No | - | No. resep |
| `PharmacyReferenceId` | `INT` | Yes | `NULL` | ID resep di Pharmacy Service |
| `DoctorId` | `INT` | No | - | Dokter penulis |
| `PatientId` | `INT` | No | - | Pasien |
| `Status` | `NVARCHAR(20)` | No | `'Pending'` | Pending / Submitted / Dispensed |
| `Notes` | `NVARCHAR(500)` | Yes | `NULL` | Catatan |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_PrescriptionOrders` | Clustered | `Id` | - | Primary key |
| `UX_Presc_Number` | Unique Nonclustered | `PrescriptionNumber` | - | Unik |
| `IX_Presc_MedRecId` | Nonclustered | `MedicalRecordId` | - | Query per rekam |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Presc_MedRec` | `MedicalRecordId` | `MedicalRecords.Id` |

---

## Tabel 6: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 7: `InboxEvents`
Sama seperti Patient Service.
