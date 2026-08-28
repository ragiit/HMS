# 09. Database Design - Inventory Service

**Database Name**: `HMS_Inventory`

---

## ERD Overview

```
┌──────────────┐     ┌──────────────────┐     ┌──────────────┐
│  Categories  │     │  InventoryItems  │─────│ StockBatches │
└──────────────┘     └──────────────────┘     └──────────────┘
                          │ 1                        │ 1
                          │                          │ N
                          ▼                          ▼
┌───────────────────────────────────────────┐  ┌──────────────┐
│           StockMovements (log)            │  │ StockBatches │
└───────────────────────────────────────────┘  └──────────────┘
```

---

## Tabel 1: `InventoryItems`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `ItemCode` | `NVARCHAR(20)` | No | - | Kode item unik |
| `Name` | `NVARCHAR(200)` | No | - | Nama obat/alat |
| `GenericName` | `NVARCHAR(200)` | Yes | `NULL` | Nama generik |
| `CategoryId` | `INT` | Yes | `NULL` | FK -> Categories.Id |
| `ItemType` | `NVARCHAR(20)` | No | `'Medicine'` | Medicine / MedicalDevice / Supply / Equipment |
| `SKU` | `NVARCHAR(50)` | Yes | `NULL` | SKU dari supplier |
| `Barcode` | `NVARCHAR(50)` | Yes | `NULL` | Barcode / EAN |
| `Unit` | `NVARCHAR(20)` | No | `'pcs'` | Satuan utama |
| `ReorderLevel` | `INT` | No | `10` | Batas reorder |
| `MaximumLevel` | `INT` | Yes | `NULL` | Batas maksimum |
| `CurrentStock` | `INT` | No | `0` | Stok saat ini |
| `TotalReceived` | `INT` | No | `0` | Total diterima |
| `UnitCost` | `DECIMAL(12,2)` | No | `0` | Harga beli |
| `SellingPrice` | `DECIMAL(12,2)` | No | `0` | Harga jual |
| `RequiresRefrigeration` | `BIT` | No | `0` | Perlu pendingin |
| `IsControlledSubstance` | `BIT` | No | `0` | Obat terlarang (narcotics) |
| `IsDeleted` | `BIT` | No | `0` | Soft delete |
| `IsActive` | `BIT` | No | `1` | Aktif |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu update |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_InventoryItems` | Clustered | `Id` | - | Primary key |
| `UX_InventoryItems_Code` | Unique Nonclustered | `ItemCode` | - | Code unik |
| `UX_InventoryItems_Barcode` | Unique Nonclustered | `Barcode` | `WHERE Barcode IS NOT NULL` | Barcode unik |
| `IX_InventoryItems_Name` | Nonclustered | `Name`, `GenericName` | - | Pencarian |
| `IX_InventoryItems_Category` | Nonclustered | `CategoryId` | - | Filter kategori |
| `IX_InventoryItems_Type` | Nonclustered | `ItemType`, `IsActive` | - | Filter tipe |

---

## Tabel 2: `Categories`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Code` | `NVARCHAR(10)` | No | - | Kode kategori |
| `Name` | `NVARCHAR(100)` | No | - | Nama kategori |
| `ParentId` | `INT` | Yes | `NULL` | Parent category |
| `Description` | `NVARCHAR(200)` | Yes | `NULL` | Deskripsi |
| `IsActive` | `BIT` | No | `1` | Aktif |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Categories` | Clustered | `Id` | - | Primary key |
| `UX_Categories_Code` | Unique Nonclustered | `Code` | - | Code unik |

### Seed Data
| Code | Name | Description |
|---|---|---|
| `ANTIBIOTIC` | Antibiotik | Obat antibiotik |
| `ANALGESIC` | Analgesik | Pereda nyeri |
| `CARDIO` | Kardiovaskular | Obat jantung |
| `SYRUP` | Sirup | Obat bentuk sirup |
| `WARD_SUPPLY` | Wards Supply | Alat perawatan |
| `SURGICAL` | Surgical | Alat bedah |
| `INJECTION` | Injeksi | Obat suntik |

---

## Tabel 3: `StockBatches`

Melacak batch kadaluwarsa.

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `ItemId` | `INT` | No | - | FK -> InventoryItems.Id |
| `BatchNumber` | `NVARCHAR(50)` | No | - | Nomor batch |
| `ReceivedDate` | `DATE` | No | - | Tanggal terima |
| `ExpirationDate` | `DATE` | No | - | Kadaluwarsa |
| `InitialQuantity` | `INT` | No | - | Qty awal |
| `CurrentQuantity` | `INT` | No | - | Qty tersisa |
| `UnitCost` | `DECIMAL(12,2)` | No | - | Harga beli batch |
| `SupplierId` | `INT` | Yes | `NULL` | Ref supplier |
| `IsConsumed` | `BIT` | No | `0` | Habis terpakai |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_StockBatches` | Clustered | `Id` | - | Primary key |
| `UX_StockBatches_Item_Batch` | Unique Nonclustered | `ItemId`, `BatchNumber` | - | Unique per item-batch |
| `IX_StockBatches_Expiry` | Nonclustered | `ExpirationDate` | `WHERE CurrentQuantity > 0` | Cek kadaluwarsa |
| `IX_StockBatches_ItemQty` | Nonclustered | `ItemId`, `CurrentQuantity` | - | Stok per item |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_StockBatches_Items` | `ItemId` | `InventoryItems.Id` |

---

## Tabel 4: `StockMovements`

Audit trail semua pergerakan stok (in/out/adjust).

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `BIGINT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `ItemId` | `INT` | No | - | FK -> InventoryItems.Id |
| `BatchId` | `BIGINT` | Yes | `NULL` | FK -> StockBatches.Id |
| `MovementType` | `NVARCHAR(20)` | No | - | Received / Dispensed / Adjusted / Returned / Expired |
| `Quantity` | `INT` | No | - | Jumlah (positif untuk in, negatif untuk out) |
| `BalanceAfter` | `INT` | No | - | Stock setelah movement |
| `ReferenceType` | `NVARCHAR(20)` | Yes | `NULL` | Prescription / Purchase / Adjustment |
| `ReferenceId` | `BIGINT` | Yes | `NULL` | Ref ke source document |
| `SourceLocation` | `NVARCHAR(50)` | Yes | `NULL` | Asal lokasi |
| `TargetLocation` | `NVARCHAR(50)` | Yes | `NULL` | Tujuan lokasi |
| `Reason` | `NVARCHAR(200)` | Yes | `NULL` | Alasan |
| `PerformedBy` | `NVARCHAR(100)` | No | - | User |
| `PerformedAt` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu |
| `UnitCostAtMovement` | `DECIMAL(12,2)` | Yes | `NULL` | Harga saat itu |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_StockMovements` | Clustered | `Id` | - | Primary key |
| `IX_StockMovements_ItemId` | Nonclustered | `ItemId`, `PerformedAt` | - | Riwayat per item |
| `IX_StockMovements_Type` | Nonclustered | `MovementType`, `PerformedAt` | - | Analisis type |
| `IX_StockMovements_Reference` | Nonclustered | `ReferenceType`, `ReferenceId` | - | Query by source |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_StockMovements_Items` | `ItemId` | `InventoryItems.Id` |

---

## Tabel 5: `Suppliers`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Name` | `NVARCHAR(150)` | No | - | Nama supplier |
| `ContactName` | `NVARCHAR(100)` | Yes | `NULL` | Kontak person |
| `Phone` | `NVARCHAR(20)` | Yes | `NULL` | Telepon |
| `Email` | `NVARCHAR(100)` | Yes | `NULL` | Email |
| `Address` | `NVARCHAR(255)` | Yes | `NULL` | Alamat |
| `IsActive` | `BIT` | No | `1` | Aktif |

---

## Tabel 6: `OutboxEvents`
Sama seperti Identity Service.

## Tabel 7: `InboxEvents`
Sama seperti Patient Service.
