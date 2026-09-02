# Identity Service - Detail

## 1. Overview
Bertanggung jawab atas **autentikasi** dan **otorisasi** user, pengelolaan role, dan token management (± JWT & refresh token).

## 2. Bounded Context
- Autentikasi user (login, logout, refresh token)
- Manajemen user & role (CRUD)
- Password management
- Token lifecycle

## 3. Domain Model
```
┌───────────────────────┐
│       User (Aggregate)│
│  - Username           │
│  - Email              │
│  - PasswordHash       │
│  + AddRole()          │
│  + RemoveRole()       │
│  + Authenticate()     │
└───────────┬───────────┘
            │
┌───────────┴───────────┐
│  Role (Value Object)  │
│  RefreshToken (Entity)│
└───────────────────────┘
```

## 4. CQRS

### Commands
| Command | Handler Responsibility |
|---|---|
| `LoginCommand` | Validate credentials, generate JWT + refresh token |
| `RefreshTokenCommand` | Validate refresh token, issue new tokens |
| `LogoutCommand` | Revoke refresh token |
| `CreateUserCommand` | Register new user |
| `UpdateUserCommand` | Update user profile |
| `ChangePasswordCommand` | Change user password |
| `AssignRolesCommand` | Assign roles to user |
| `DeleteUserCommand` | Soft-delete user |

### Queries
| Query | Handler Responsibility |
|---|---|
| `GetUserByIdQuery` | Get user by ID |
| `GetAllUsersQuery` | Paginated list of users |
| `GetUserRolesQuery` | Get roles for user |
| `ValidateTokenQuery` | Validate JWT token (for Gateway) |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `user.created` | User baru dibuat | Notification Service (welcome email) |
| `user.updated` | Profil user diubah | - |
| `user.deactivated` | User dinonaktifkan | - |

### Subscribes
| Event | Aksi |
|---|---|
| (*none* - Identity adalah source of truth untuk users) | - |

## 6. Dependencies (Keluar)
Directo: **none** (identity berdiri sendiri). Sering di-call oleh service lain.

## 7. Design Decisions
- **Password hashing**: PBKDF2 via `PasswordHasher<TUser>` dari ASP.NET Core Identity, atau BCrypt
- **JWT Audience**: `hms-api`
- **Refresh token**: menggunakan `UNIQUEIDENTIFIER`, berbasis database (bukan stateless), sehingga bisa di-revoke
- **Lockout policy**: max 5 failed attempts dalam 15 menit
- **Password policy**: min 8 karakter, wajib 1 uppercase, 1 number, 1 symbol

## 8. Project Structure (src)
```
Identity.Api/
├── Program.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── UsersController.cs
│   └── RolesController.cs
├── Middleware/
├── Swagger/
```
```
Identity.Application/
├── Commands/
├── Queries/
├── Handlers/
├── DTOs/
├── Interfaces/
├── Behaviours/
└── Validators/
```
```
Identity.Domain/
├── Entities/
│   ├── User.cs
│   ├── Role.cs
│   └── UserRole.cs
├── Shared/
├── ValueObjects/
└── CommandsEvents/
```
```
Identity.Infrastructure/
├── Persistence/
│   ├── IdentityDbContext.cs
│   ├── Configurations/
│   └── Migrations/
├── Repositories/
├── Services/
├── Security/
└── External/
```

## 9. Teknis Implementasi Key
- **JWT Middleware** untuk validasi token di setiap request service
- **Authorization Policy** untuk RBAC
- **Refresh Token Rotation** (setiap refresh, token lama di-revoke)
