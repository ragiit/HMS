# 08. Database Design - Billing Service

**Database Name**: `HMS_Billing`

---

## ERD Overview

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ ServiceCatalogs  │     │   Invoices       │─────│ InvoiceItems     │
└──────────────────┘     └──────────────────┘     └──────────────────┘
                              │ 1
                              │ N
┌──────────────────┐     ┌──────────────────┐
│  ServicePrices   │     │   Payments       │
└──────────────────┘     └──────────────────┘
```

---

## Tabel 1: `Invoices`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `InvoiceNumber` | `NVARCHAR(20)` | No | - | No. invoice (INV-YYYYMMDD-XXXX) |
| `PatientId` | `INT` | No | - | FK referensi Patient Service |
| `InvoiceType` | `NVARCHAR(30)` | No | - | Consultation / Lab / Pharmacy / Admission / Procedure |
| `AppointmentId` | `INT` | Yes | `NULL` | Reference Appointment (optional) |
| `MedicalRecordId` | `BIGINT` | Yes | `NULL` | Reference Medical Record |
| `Status` | `NVARCHAR(20)` | No | `'Draft'` | Draft / Issued / Paid / PartiallyPaid / Cancelled / Overdue |
| `InvoiceDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `DueDate` | `DATE` | No | - | Batas pembayaran |
| `Currency` | `NVARCHAR(3)` | No | `'IDR'` | Mata uang |
| `Subtotal` | `DECIMAL(14,2)` | No | `0` | Total sebelum diskon |
| `DiscountAmount` | `DECIMAL(12,2)` | No | `0` | Diskon |
| `DiscountReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan diskon |
| `TaxPercent` | `DECIMAL(5,2)` | No | `0` | Persen pajak |
| `TaxAmount` | `DECIMAL(12,2)` | No | `0` | Jumlah pajak |
| `TotalAmount` | `DECIMAL(14,2)` | No | - | Total akhir |
| `AmountPaid` | `DECIMAL(14,2)` | No | `0` | Sudah dibayar |
| `AmountDue` | `DECIMAL(14,2)` | No | - | Sisa tagihan |
| `InsuranceClaimId` | `NVARCHAR(50)` | Yes | `NULL` | Klaim asuransi |
| `Description` | `NVARCHAR(500)` | Yes | `NULL` | Deskripsi invoice |
| `IssuedBy` | `NVARCHAR(100)` | No | - | Pembuat invoice |
| `CancelledBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembatal |
| `CancelledAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu batal |
| `CancelReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan batal |
| `IsVoided` | `BIT` | No | `0` | Void flag |
| `VoidReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan void |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu update |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Invoices` | Clustered | `Id` | - | Primary key |
| `UX_Invoices_Number` | Unique Nonclustered | `InvoiceNumber` | - | No. invoice unik |
| `IX_Invoices_PatientId` | Nonclustered | `PatientId`, `InvoiceDate` | - | Riwayat tagihan pasien |
| `IX_Invoices_Status` | Nonclustered | `Status`, `DueDate` | - | Cek overdue |
| `IX_Invoices_Type` | Nonclustered | `InvoiceType`, `InvoiceDate` | - | Aggregasi jenis |

---

## Tabel 2: `InvoiceItems`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `InvoiceId` | `BIGINT` | No | - | FK -> Invoices.Id |
| `ServiceCatalogId` | `INT` | Yes | `NULL` | FK -> ServiceCatalogs.Id |
| `ServiceCode` | `NVARCHAR(20)` | No | - | Kode layanan |
| `ServiceName` | `NVARCHAR(200)` | No | - | Nama layanan |
| `Description` | `NVARCHAR(500)` | Yes | `NULL` | Deskripsi item |
| `Quantity` | `DECIMAL(10,2)` | No | `1` | Jumlah |
| `UnitPrice` | `DECIMAL(12,2)` | No | - | Harga per unit |
| `DiscountPercent` | `DECIMAL(5,2)` | No | `0` | Diskon % |
| `DiscountAmount` | `DECIMAL(12,2)` | No | `0` | Diskon jumlah |
| `TaxAmount` | `DECIMAL(12,2)` | No | `0` | Pajak item |
| `LineTotal` | `DECIMAL(12,2)` | No | - | Total per item |
| `SourceType` | `NVARCHAR(20)` | No | - | FromService / Manual |
| `SourceReferenceId` | `BIGINT` | Yes | `NULL` | Ref ke source item |
| `LineNumber` | `SMALLINT` | No | - | No. urut |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_InvoiceItems` | Clustered | `Id` | - | Primary key |
| `IX_InvoiceItems_InvoiceId` | Nonclustered | `InvoiceId` | - | Items per invoice |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_InvoiceItems_Invoices` | `InvoiceId` | `Invoices.Id` |

---

## Tabel 3: `Payments`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `InvoiceId` | `BIGINT` | No | - | FK -> Invoices.Id |
| `PaymentNumber` | `NVARCHAR(20)` | No | - | No. pembayaran |
| `PaymentDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu bayar |
| `Amount` | `DECIMAL(14,2)` | No | - | Jumlah bayar |
| `PaymentMethod` | `NVARCHAR(30)` | No | - | Cash / Transfer / Card / BPJS / QR |
| `ReferenceNumber` | `NVARCHAR(100)` | Yes | `NULL` | No. referensi (trx bank) |
| `PaymentGateway` | `NVARCHAR(50)` | Yes | `NULL` | Midtrans / Xendit dll |
| `Status` | `NVARCHAR(20)` | No | `'Completed'` | Completed / Pending / Failed / Refunded |
| `ReceivedBy` | `NVARCHAR(100)` | No | - | Kasir |
| `RefundAmount` | `DECIMAL(14,2)` | Yes | `NULL` | Jumlah refund |
| `RefundReason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan refund |
| `RefundedAt` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu refund |
| `Currency` | `NVARCHAR(3)` | No | `'IDR'` | Mata uang |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Payments` | Clustered | `Id` | - | Primary key |
| `UX_Payments_Number` | Unique Nonclustered | `PaymentNumber` | - | No. unik |
| `IX_Payments_InvoiceId` | Nonclustered | `InvoiceId`, `PaymentDate` | - | Payments per invoice |
| `IX_Payments_Method` | Nonclustered | `PaymentMethod`, `PaymentDate` | - | Analisis pembayaran |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_Payments_Invoices` | `InvoiceId` | `Invoices.Id` |

---

## Tabel 4: `ServiceCatalogs`

Master data layanan & tarif.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `ServiceCode` | `NVARCHAR(20)` | No | - | Kode layanan unik |
| `ServiceName` | `NVARCHAR(200)` | No | - | Nama layanan |
| `Category` | `NVARCHAR(50)` | No | - | Konstultasi / Lab / Obat / Tindakan |
| `Description` | `NVARCHAR(500)` | Yes | `NULL` | Deskripsi |
| `UnitPrice` | `DECIMAL(12,2)` | No | - | Harga standar |
| `TaxRate` | `DECIMAL(5,2)` | No | `0` | Pajak |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `IsCoveredByBPJS` | `BIT` | No | `0` | Dicover BPJS |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_ServiceCatalogs` | Clustered | `Id` | - | Primary key |
| `UX_ServiceCatalog_Code` | Unique Nonclustered | `ServiceCode` | - | Code unik |
| `IX_ServiceCatalog_Category` | Nonclustered | `Category` | - | Filter kategori |

### Seed Data
| ServiceCode | ServiceName | Category | UnitPrice |
|---|---|---|---|
| `CONSULT-GENERAL` | Konsultasi Dokter Umum | Konsultasi | 150,000 |
| `CONSULT-SPECIALIST` | Konsultasi Dokter Spesialis | Konsultasi | 250,000 |
| `LAB-CBC` | Complete Blood Count | Laboratorium | 85,000 |
| `PHARM-DISPENSE` | Biaya Layanan Apotek | Obat | 5,000 |
| `ADMISSION-IPD` | Biaya Rawat Inap (per hari) | Tindakan | 500,000 |
| `EMERGENCY` | Tindakan Emergency | Tindakan | 200,000 |

---

## Tabel 5: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 6: `InboxEvents`
Sama seperti Patient Service.
