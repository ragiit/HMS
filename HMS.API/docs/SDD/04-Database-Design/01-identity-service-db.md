# 01. Database Design - Identity Service

**Database Name**: `HMS_Identity`

---

## ERD (Entity Relationship Diagram) Overview

```
┌──────────┐     ┌───────────┐     ┌────────────┐
│  Users   │─────│  UserRoles │─────│  Roles     │
└──────────┘     └───────────┘     └────────────┘
     │
     │
     ▼
┌───────────┐     ┌─────────────┐
│RefreshTokens│    │ OutboxEvents │
└───────────┘     └─────────────┘
```

---

## Tabel 1: `Users`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Username` | `NVARCHAR(50)` | No | - | Username unik login |
| `Email` | `NVARCHAR(100)` | No | - | Email user |
| `PasswordHash` | `NVARCHAR(255)` | No | - | Hashed password |
| `PasswordSalt` | `NVARCHAR(255)` | No | - | Salt untuk hash |
| `FullName` | `NVARCHAR(150)` | No | - | Nama lengkap user |
| `PhoneNumber` | `NVARCHAR(20)` | Yes | `NULL` | Nomor HP |
| `IsActive` | `BIT` | No | `1` | Aktif / nonaktif |
| `IsLockedOut` | `BIT` | No | `0` | Status lockout |
| `LockoutEndDate` | `DATETIMEOFFSET` | Yes | `NULL` | Kapan lockout berakhir |
| `LastLoginDate` | `DATETIMEOFFSET` | Yes | `NULL` | Login terakhir |
| `AccessFailedCount` | `INT` | No | `0` | Jumlah gagal login |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `CreatedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pembuat (username) |
| `ModifiedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu diubah |
| `ModifiedBy` | `NVARCHAR(100)` | Yes | `NULL` | Pengubah (username) |
| `DeletedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Soft delete timestamp |
| `RowVersion` | `ROWVERSION` | No | - | Concurrency token |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Users` | Clustered | `Id` | - | Primary key |
| `UX_Users_Username` | Unique Nonclustered | `Username` | - | Username uniqueness |
| `UX_Users_Email` | Unique Nonclustered | `Email` | - | Email uniqueness |
| `IX_Users_IsActive` | Nonclustered | `IsActive` | - | Filter active users |
| `IX_Users_LastLogin` | Nonclustered | `LastLoginDate` | - | Sessions audit |

---

## Tabel 2: `Roles`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `INT (IDENTITY)` | No | `IDENTITY(1,1)` | Primary Key |
| `Name` | `NVARCHAR(50)` | No | - | Nama role (`Admin`, `Doctor`, `Nurse`, dll) |
| `NormalizedName` | `NVARCHAR(50)` | No | - | Uppercase normalized name |
| `Deskripsi` | `NVARCHAR(200)` | Yes | `NULL` | Deskripsi role |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `IsActive` | `BIT` | No | `1` | Aktif atau tidak |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_Roles` | Clustered | `Id` | - | Primary key |
| `UX_Roles_Name` | Unique Nonclustered | `Name` | - | Name uniqueness |

### Seed Data (Role)
| Id | Name | NormalizedName | Deskripsi |
|---|---|---|---|
| 1 | `Admin` | `ADMIN` | Administrator sistem |
| 2 | `Doctor` | `DOCTOR` | Dokter |
| 3 | `Nurse` | `NURSE` | Perawat |
| 4 | `FrontDesk` | `FRONTDESK` | Staf registrasi |
| 5 | `Pharmacist` | `PHARMACIST` | Apoteker |
| 6 | `LabStaff` | `LABSTAFF` | Staf laboratorium |
| 7 | `BillingStaff` | `BILLINGSTAFF` | Staf keuangan |
| 8 | `Patient` | `PATIENT` | Pasien (portal) |
| 9 | `Inventory` | `INVENTORY` | Staf inventory/gudang |

---

## Tabel 3: `UserRoles` (Join Table)

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `UserId` | `INT` | No | - | FK -> Users.Id |
| `RoleId` | `INT` | No | - | FK -> Roles.Id |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_UserRoles` | Clustered | `UserId`, `RoleId` | - | Composite primary key |
| `IX_UserRoles_RoleId` | Nonclustered | `RoleId` | - | Query by role |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_UserRoles_Users` | `UserId` | `Users.Id` |
| `FK_UserRoles_Roles` | `RoleId` | `Roles.Id` |

---

## Tabel 4: `RefreshTokens`

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `UNIQUEIDENTIFIER` | No | `NEWID()` | Primary Key |
| `UserId` | `INT` | No | - | FK -> Users.Id |
| `Token` | `NVARCHAR(500)` | No | - | Refresh token value |
| `ExpiresOn` | `DATETIMEOFFSET` | No | - | Kapan token expired |
| `CreatedOn` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu dibuat |
| `RevokedOn` | `DATETIMEOFFSET` | Yes | `NULL` | Kapan token revoked |
| `ReplacedByToken` | `NVARCHAR(500)` | Yes | `NULL` | Token pengganti |
| `ReasonRevoked` | `NVARCHAR(100)` | Yes | `NULL` | Alasan revoke |
| `ClientId` | `NVARCHAR(100)` | Yes | `NULL` | Identifikasi client |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_RefreshTokens` | Clustered | `Id` | - | Primary key |
| `UX_RefreshTokens_Token` | Unique Nonclustered | `Token` | - | Token unique |
| `IX_RefreshTokens_UserId` | Nonclustered | `UserId` | - | Query tokens by user |

### Foreign Keys
| Constraint | Columns | Reference |
|---|---|---|
| `FK_RefreshTokens_Users` | `UserId` | `Users.Id` |

---

## Tabel 5: `OutboxEvents` (Transactional Outbox)

| Column | Data Type | Nullable | Default | Description |
|---|---|---|---|---|
| `Id` | `UNIQUEIDENTIFIER` | No | `NEWID()` | Primary Key |
| `EventType` | `NVARCHAR(150)` | No | - | Nama event |
| `EventPayload` | `NVARCHAR(MAX)` | No | - | Serialized JSON payload |
| `CorrelationId` | `NVARCHAR(100)` | Yes | `NULL` | Correlation ID untuk tracing |
| `CreatedDate` | `DATETIMEOFFSET` | No | `SYSDATETIMEOFFSET()` | Waktu event dibuat |
| `ProcessedDate` | `DATETIMEOFFSET` | Yes | `NULL` | Waktu dipublish |
| `Status` | `NVARCHAR(20)` | No | `'Pending'` | Pending / Published / Failed |
| `RetryCount` | `INT` | No | `0` | Jumlah retry |
| `LastError` | `NVARCHAR(500)` | Yes | `NULL` | Pesan error terakhir |

### Indexes
| Index Name | Type | Columns | Filtered | Description |
|---|---|---|---|---|
| `PK_OutboxEvents` | Clustered | `Id` | - | Primary key |
| `IX_OutboxEvents_Status` | Nonclustered | `Status`, `CreatedDate` | `WHERE Status = 'Pending'` | Query pending events |
| `IX_OutboxEvents_CreatedDate` | Nonclustered | `CreatedDate` | - | Housekeeping query |
