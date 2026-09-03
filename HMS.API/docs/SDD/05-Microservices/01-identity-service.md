# Identity Service - Detail

> **Status implementasi**: ✅ DI-IMPLEMENTASIKAN (sumber: `HMS.API/HMS/Services/Identity/`).
> Dokumen ini **disinkronkan 1:1 dengan kode yang ada** dan dijadikan **pola patokan** struktur CQRS + API untuk service HMS lain.

## 1. Overview & Peran (apa yang dikerjakan)

Bertanggung jawab atas **autentikasi & otorisasi** user, **manajemen user & role**, dan **siklus hidup token** (JWT access + refresh token berbasis database yang dapat di-rotate/revoke). Identity adalah **source of truth** untuk identitas seluruh **pegawai/system user** HMS (Admin, Doctor, Nurse, FrontDesk, Pharmacist, LabStaff, BillingStaff, Inventory). Pasien (end user publik) berada di Patient Service.

## 2. Tanggung Jawab / Fungsi Utama (list kerja)

1. **Login** — verifikasi username/email + password (PBKDF2), cek lockout/aktif.
2. **Issue token** — terbitkan access token (JWT) dan refresh token.
3. **Refresh token** — perpanjang akses tanpa login ulang; lakukan **rotation** (token lama di-revoke, token baru disimpan).
4. **Logout / Revoke** — matikan satu refresh token aktif.
5. **Ganti password** — verifikasi lama, buat baru, reset login state.
6. **Registrasi user** (oleh admin dgn izin) + **assign role**.
7. **Update profil** — ubah data non-kredensial + audit.
8. **Deactivate (soft-delete)** — nonaktifkan user & kirim event integrasi.
9. **List/kirim role** — master data role HMS.

## 3. Bounded Context
- Autentikasi user (login, refresh, revoke)
- Manajemen user & role (register, update, deactivate, assign)
- Manajemen kredensial & lockout
- Lifecycle token (issue, rotate, revoke)

## 4. Domain Model (real, mengikuti kode)

```
User (AggregateRoot<Guid>; IAuditableEntity)
 ├─ Username, Email, PasswordHash, PasswordSalt, FullName, PhoneNumber
 ├─ IsActive, IsLockedOut, LockoutEndDate, LastLoginDate, AccessFailedCount
 ├─ audit & soft-del: CreatedDate/CreatedBy, ModifiedDate/ModifiedBy,
 │      IsDeleted, DeletedDate
 ├─ Roles : ICollection<UserRole> ; RefreshTokens : ICollection<RefreshToken>
 ├─ SetPassword, UpdateProfile, Activate, Deactivate
 ├─ MarkDeleted()  → soft delete + event UserDeactivated
 ├─ RecordLogin / RecordFailedLogin(maxAttempts=5) / ResetFailedLogin
 ├─ AssignRole(Role)/RemoveRole(roleId) ; UpdateAudit/GetId

Role (Entity<Guid>)         -- master, bukan value object
 ├─ Name, NormalizedName, Description, IsActive, CreatedDate
 ├─ Update / Activate / Deactivate

UserRole (join; key komposit di User|Role)
 ├─ UserId, RoleId  + navigasi User & Role

RefreshToken (Entity<Guid>)
 ├─ UserId, Token, ExpiresOn(+7d), CreatedOn, RevokedOn,
 │      ReplacedByToken, ReasonRevoked, ClientId
 ├─ IsActive => RevokedOn is null && UtcNow < ExpiresOn
 └─ Revoke(reason, replacedBy)
```

> **Primary key convention HMS**: semua aggregate/entity memakai **`Guid`** (bukan `int`).
> `Id` pada `User/Role/RefreshToken` adalah Guid → semua endpoint `{id}` menerima GUID string.

## 5. CQRS — daftar real (selaras kode)

### Commands (MediatR)
| Command | Tanggung Jawab Handler |
|---|---|
| `LoginCommand(Username, Password, ClientId?)` | validasi kredensial; reset/record login; issue access + simpan refresh → `AuthResultDto` |
| `RegisterUserCommand(Username, Email, FullName, Password, PhoneNumber?, Roles?)` | cek duplikat, buat user, hash password, assign role, persist → `Guid` |
| `RefreshTokenCommand(RefreshToken, ClientId?)` | cek refresh valid & aktif, issue access baru + **rotate** → `AuthResultDto` |
| `RevokeTokenCommand(RefreshToken)` | logout: revoke refresh token ybs |
| `ChangePasswordCommand(UserId, CurrentPassword, NewPassword)` | ganti password + reset login state |
| `AssignRolesCommand(UserId, Roles)` | reset lalu assign role user |
| `DeactivateUserCommand(UserId)` | soft-delete (mark deleted) + audit |
| `UpdateUserProfileCommand(UserId, Email, FullName, PhoneNumber?)` | update profil non-kredensial + audit |

> **Tidak ada** `CreateUserCommand/LogoutCommand/DeleteUserCommand/GetUserRolesQuery/ValidateTokenQuery` seperti versi lama.
> Penggantinya: register → `RegisterUserCommand`; logout → `RevokeTokenCommand`; hapus → `DeactivateUserCommand`.

### Queries (MediatR)
| Query | Tanggung Jawab Handler |
|---|---|
| `GetUserByIdQuery(UserId)` | user + roles → `UserDto` |
| `GetUserByLoginQuery(Login, Password)` | cadangan validasi login (login utama via `LoginCommand`) |
| `GetAllUsersQuery(Page, PageSize, Search?)` | list user ter-paginasi (filter username/fullname/email) |
| `GetAllRolesQuery` | list role aktif |

### Cross-cutting
- `ValidationBehaviour` (pipeline) validasi command via `CommandValidators.cs` (FluentValidation) sebelum handler.
- UoW `SaveChangesAsync` menulis domain→integration event ke tabel outbox **dalam satu transaksi** (Transactional Outbox).

## 6. Events (real)

### Publishes (outbox → RabbitMQ)
| Domain Event | Integration Event | Dipicu | Target |
|---|---|---|---|
| `UserDeactivatedDomainEvent(UserId, Username)` | `UserDeactivatedEvent { UserId, Reason="deactivated" }` (namespace Shared.Contracts.Identity) | `User.MarkDeleted()` | Notification/Audit |

Hanya **1 integration event** yang terpetakan saat ini. Tidak ada `user.created/user.updated` integrasi pada kode ini (versi lama usang).

### Subscribes
Tidak berlangganan event eksternal. Identity adalah pemilik & sumber data identitas, sehingga tidak menyimpan data user di luar miliknya.

## 7. Dependencies & konsumen
- Keluar (HTTP/sync): dipanggil service lain utk cek user/otorisasi.
- Keluar (async): event integrasi ke RabbitMQ via outbox.
- Masuk: read config JWT/RabbitMQ; tidak men-depend service HMS lain.

## 8. Security & architecture decisions
- Password: PBKDF2 `HMACSHA256` 100k iterasi, salt 16B, hash 32B, verifikasi constant-time (`PasswordHasher`).
- JWT: HMAC-SHA256 dgn secret `JwtOptions`; claim `sub,nameid,name,jti,iat` + 1 `role` claim per role; expiry `AccessTokenExpiryMinutes`.
- Refresh token: random 64B → base64, **disimpan di DB** (bukan stateless) → bisa revoke & rotate.
- Validasi di req: `AddJwtBearer` di `Program.cs` (Identity.Api) dgn param & secret sama.
- Lockout: 5 gagal / 15 menit; default admin seeded `admin`/`Admin@12345`.
- RBAC: `[Authorize]` aktif; akan dikencangkan `[Authorize(Roles="Admin")]` (TODO).

> Renungan pengembangan berikutnya: tambahkan cleanup/expire lockout otomatis, cap sesi per user, & purge token usang bila diperlukan.

## 9. Project Structure (src, riil)
```
Identity.Api/                 # presentasi
├── Program.cs                # DI, JWT, swagger, middleware, seeding
├── Controllers/AuthController.cs
├── Controllers/UsersController.cs
└── Middleware/ExceptionHandlingMiddleware.cs

Identity.Application/         # use-cases/CQRS
├── Abstractions/ITokenService.cs, IUserRepository.cs
├── Behaviours/ValidationBehaviour.cs
├── Commands/UserCommands.cs
├── Queries/UserQueries.cs
├── Handlers/ ... # per command/query
├── DTOs/AuthDtos.cs
└── Validators/CommandValidators.cs

Identity.Domain/              # entities + events
├── Entities/User.cs, Role.cs, UserRole.cs, RefreshToken.cs
└── Events/UserDeactivatedDomainEvent.cs

Identity.Infrastructure/      # EF, repo, security, outbox
├── Persistence/IdentityDbContext.cs, EfUnitOfWork.cs,
│        IdentityDbSeeder.cs, IdentityDbContextFactory.cs, DomainEventMapper.cs
├── Persistence/Configurations/, Migrations/, Repositories/
├── Security/TokenService.cs, PasswordHasher.cs
```

## 10. Ringkasan Teknis Implementasi (highlight)
- Clean/layered: Api → Application → Domain / Infrastructure.
- MediatR CQRS + FluentValidation pipeline (lokal per service).
- Transactional Outbox Pattern untuk kirim event integrasi.
- JWT Bearer + role-claim RBAC.
- Refresh token rotation **berbasis DB**.
- EF Core code-first, migration per service DB.

## 11. Status & pekerjaan berjalan (kantor kerja)
| Bagian | Status |
|---|---|
| Domain | ✅ selesai |
| Command Login/Register/Refresh/Revoke/... | ✅ |
| Persistence refresh token disimpan saat login & rotate | ✅ (telah diperbaiki) |
| JWT + RBAC | ✅ (RBAC admin masih TODO) |
| Migrations DB | tersedia; naikkan sesuai env |
| Unlock manual (admin) | 🔲 belum (lihat bab endpoint) |
| Refresh token cleanup/cap | 🔲 belum |
