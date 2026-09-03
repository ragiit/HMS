# 02. API Endpoints Design

## 1. Konvensi Umum

### 1.1 Response Format
Semua API mengembalikan format JSON standar:

```json
{
  "success": true,
  "data": { ... },
  "message": "Success message",
  "errors": null
}
```

### 1.2 Error Format
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    { "field": "FirstName", "message": "First name is required" }
  ]
}
```

### 1.3 Pagination
Query parameter: `page` (default 1) dan `pageSize` (default 10, max 100)

Response pagination:
```json
{
  "page": 1,
  "pageSize": 10,
  "totalItems": 150,
  "totalPages": 15,
  "items": [ ... ]
}
```

### 1.4 Authentication
- Semua endpoint kecuali `/auth/login` dan `/auth/refresh` memerlukan `Authorization: Bearer <jwt>`
- Role-based access via claim `role`

---

## 2. Identity Service Endpoints (riil — `Identity.Api`, prefix `/api/v1`)

> Service ini **sudah diimplementasikan**. Kontrak di bawah disamakan persis dengan `AuthController.cs` & `UsersController.cs`.
> Semua id bertipe **Guid string**. Semua response dibungkus `ApiResponse<T>`:
> `{ success, message, errors, data }`. Pesan berbahasa Indonesia sesuai handler.

### Authentication
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/auth/login` | Login (username/email + password) → JWT + refresh token | AllowAnonymous |
| POST | `/api/v1/auth/refresh` | Perpanjang access token dgn refresh token (rotasi) | AllowAnonymous |
| POST | `/api/v1/auth/revoke` | Logout: cabut (revoke) 1 refresh token | Bearer |
| POST | `/api/v1/auth/change-password` | Ganti password user login | Bearer |

> Catatan: endpoint logout di kode bernama `revoke` (bukan `logout`, tidak ada `RolesController`, tidak ada endpoint `GET /users/{id}/roles`).

### User Management
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/users` | Registrasi user baru (body `RegisterUserCommand`), return id Guid | Bearer |
| GET | `/api/v1/users` | List user ter-paginasi (query page/pageSize/search) | Bearer |
| GET | `/api/v1/users/{id:guid}` | Detail user + roles → UserDto | Bearer |
| PUT | `/api/v1/users/{id:guid}` | Update profil (non-kredensial) | Bearer |
| DELETE | `/api/v1/users/{id:guid}` | Soft-delete user (set IsDeleted + event) | Bearer |
| POST | `/api/v1/users/{id:guid}/roles` | Assign roles (replace keseluruhan roles user) | Bearer |
| GET | `/api/v1/users/roles` | List master role aktif | Bearer |

> `UsersController` saat ini `[Authorize]` (belum per-role eksplisit) — **TODO** perketat `[Authorize(Roles="Admin")]` pada action sebagai berikutnya.

### Request/Response contracts (riil)

#### POST /api/v1/auth/login
```json
// Request body: LoginCommand  -> { username, password, clientId? }
{
  "username": "dr.suparno",
  "password": "secret123"
}

// Response 200: ApiResponse<AuthResultDto>
{
  "success": true,
  "message": "Login berhasil",
  "errors": null,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1...",
    "refreshToken": "d0FhYmNkZWYxMj...",            // random base64, DISIMPAN DB
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": "11111111-2222-3333-4444-555555555555",
      "username": "dr.suparno",
      "email": "suparno@hms.local",
      "fullName": "dr. Suparno, Sp.PD",
      "phoneNumber": null,
      "isActive": true,
      "roles": [ "Doctor" ]
    }
  }
}
```

#### POST /api/v1/auth/refresh
```json
// Request: RefreshTokenCommand
{
  "refreshToken": "d0FhYmNkZWYxMj...",
  "clientId": null
}
// Response 200 sama dgn login (access+refresh baru, refresh lama di-revoke & diganti)
```

#### POST /api/v1/auth/revoke  (logout)
```json
// Request: RevokeTokenCommand
{ "refreshToken": "d0FhYmNkZWYxMj..." }
// Response 200: { success:true, message:"Refresh token dicabut", data:null }
```

#### POST /api/v1/auth/change-password
```json
// Request: ChangePasswordCommand
{
  "userId": "11111111-2222-3333-4444-555555555555",
  "currentPassword": "oldP@ss",
  "newPassword": "NewP@ss123"
}
// 200 -> message "Password berhasil diubah"
```

#### POST /api/v1/users  (register)
```json
// Request: RegisterUserCommand
{
  "username": "nurse.anisa",
  "email": "anisa@hms.local",
  "fullName": "Anisa Putri",
  "phoneNumber": "081234567890",
  "password": "Secret@123",
  "roles": [ "Nurse" ]
}
// Response 201/200: data = { "id": "11..g" } ; message "User dibuat"
```

#### GET /api/v1/users  (list; query string)
`?page=1&pageSize=10&search=anisa`
```json
// Response 200 (PagedResult<UserDto>)
{
  "success": true, "message": "Success", "errors": null,
  "data": {
    "pageIndex": 1, "pageSize": 10, "totalCount": 1,
    "items": [
      { "id": "11..g", "username": "nurse.anisa", "email": "anisa@hms.local",
        "fullName": "Anisa Putri", "phoneNumber": "081234567890",
        "isActive": true, "roles": [ "Nurse" ] }
    ]
  }
}
```

#### POST /api/v1/users/{id}/roles  (assign/replace roles)
```json
// Request: AssignRolesCommand
{ "roles": [ "Admin", "Nurse" ] }
// Response 200 -> message "Roles diperbarui"
```

#### GET /api/v1/users/roles
```json
// Response 200: data = [ { "id":"..g", "name":"Doctor", "description":"..." }, ... ]
```

> Error mapping umum (oleh `ExceptionHandlingMiddleware`):
> Unauthorized → 401 "Invalid username or password" / "Account is locked...";
> NotFound → 404; Conflict → 409; BusinessRuleViolation/Validation → 400 dgn `errors[]`.

---

## 3. Patient Service Endpoints

> 🔲 Draft rancangan (di bawah masih ber-prefix tanpa `/api/v1` & `Id int`). Acuan kontrak terkini untuk service ini ada di `05-Microservices/02-patient-service.md` (preview endpoint berbasis `/api/v1` & Guid).

| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/patients` | Registrasi pasien baru | Bearer | Admin, FrontDesk, Nurse |
| GET | `/api/patients` | List pasien (dengan pagination & filter) | Bearer | Admin, Doctor, Nurse, Billing |
| GET | `/api/patients/{id}` | Detail pasien | Bearer | Admin, Doctor, Nurse |
| PUT | `/api/patients/{id}` | Update data pasien | Bearer | Admin, FrontDesk |
| DELETE | `/api/patients/{id}` | Soft-delete pasien | Bearer | Admin |
| GET | `/api/patients/search?term=` | Cari pasien (nama, NIK, MR) | Bearer | All klinis |
| GET | `/api/patients/{id}/medical-history` | Riwayat medis pasien | Bearer | Doctor, Nurse |
| GET | `/api/patients/{id}/emergency-contact` | Contact darurat pasien | Bearer | Admin, Nurse |

### Request/Response Contracts (POST /api/patients)
```json
// Request
{
  "medicalRecordNumber": "MR-000001",
  "firstName": "Budi",
  "lastName": "Santoso",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "bloodType": "O",
  "nationalIdNumber": "3175012345670001",
  "phone": "081234567890",
  "email": "budi@gmail.com",
  "address": {
    "street": "Jl. Merdeka No. 10",
    "city": "Jakarta",
    "province": "DKI Jakarta",
    "postalCode": "10110"
  },
  "emergencyContact": {
    "name": "Siti Rahayu",
    "relationship": "Spouse",
    "phone": "081298765432"
  },
  "insurance": {
    "provider": "BPJS",
    "policyNumber": "0001234567",
    "coverageType": "Kelas 3"
  }
}

// Response (201 Created)
{
  "success": true,
  "data": {
    "id": 1,
    "medicalRecordNumber": "MR-000001",
    "fullName": "Budi Santoso",
    "createdAt": "2026-01-15T10:30:00Z"
  }
}
```

---

## 4. Doctor Service Endpoints

> 🔲 Draft rancangan (di bawah : prefix tanpa `/api/v1`, `Id int`.) Acuan kontrak terkini: `05-Microservices/03-doctor-service.md` (blueprint `Guid`, `/api/v1`).

### Doctor Profile
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| GET | `/api/doctors` | List semua dokter | Bearer | Admin, Doctor, Nurse, FrontDesk |
| GET | `/api/doctors/{id}` | Detail profile dokter | Bearer | All logged users |
| POST | `/api/doctors` | Tambah dokter baru | Bearer | Admin |
| PUT | `/api/doctors/{id}` | Update profile dokter | Bearer | Admin, Doctor |
| DELETE | `/api/doctors/{id}` | Hapus dokter (soft delete) | Bearer | Admin |
| GET | `/api/doctors/{id}/schedules` | Jadwal praktek dokter | Bearer | Admin, Doctor, FrontDesk |
| GET | `/api/doctors/available?from=&to=` | Dokter yang available pada jam | Bearer | FrontDesk, Doctor |
| GET | `/api/doctors/search?specialization=` | Cari dokter by spesialisasi | Bearer | All |

### Spesialisasi (Master Data)
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| GET | `/api/specializations` | List all specializations | Bearer | All |
| POST | `/api/specializations` | Tambah specialisasi | Bearer | Admin |

### Schedule Management
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/doctors/{id}/schedules` | Set jadwal praktek dokter | Bearer | Admin, Doctor |
| PUT | `/api/doctors/{id}/schedules/{scheduleId}` | Update jadwal | Bearer | Admin, Doctor |
| DELETE | `/api/doctors/{id}/schedules/{scheduleId}` | Hapus jadwal | Bearer | Admin |
| GET | `/api/doctors/{id}/schedules/weekly?date=` | Jadwal mingguan | Bearer | All |

### Contoh Request (POST /api/doctors/{id}/schedules)
```json
{
  "dayOfWeek": "Monday",
  "startTime": "08:00",
  "endTime": "12:00",
  "slotDurationMinutes": 30,
  "maxPatientsPerSlot": 1,
  "location": "Poli Umum",
  "roomNumber": "R-101",
  "active": true
}
```

---

## 5. Appointment Service Endpoints

> 🔲 Draft rancangan (di bawah : prefix tanpa `/api/v1`, `Id int`.) Acuan kontrak terkini: `05-Microservices/04-appointment-service.md` (blueprint `Guid`, `/api/v1`).

### Appointment CRUD
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/appointments` | Booking appointment baru | Bearer | Patient, FrontDesk, Admin |
| GET | `/api/appointments` | List appointments (filter by date, status, doctor) | Bearer | All |
| GET | `/api/appointments/{id}` | Detail appointment | Bearer | Owner, Admin |
| PUT | `/api/appointments/{id}` | Reschedule/update appointment | Bearer | Patient, FrontDesk, Admin |
| DELETE | `/api/appointments/{id}` | Cancel appointment (soft) | Bearer | Patient, FrontDesk, Admin |
| GET | `/api/appointments/doctor/{doctorId}?date=` | Schedules appointments by doctor | Bearer | Doctor, Admin |
| GET | `/api/appointments/patient/{patientId}` | Appointments by patient | Bearer | Patient, Admin |
| POST | `/api/appointments/{id}/check-in` | Pasien check-in (datang) | Bearer | FrontDesk |
| POST | `/api/appointments/{id}/complete` | Tandai selesai | Bearer | Doctor |

### Request (POST /api/appointments)
```json
{
  "patientId": 1,
  "doctorId": 5,
  "scheduleId": 123,
  "date": "2026-01-20",
  "slotTime": "09:00",
  "reason": "Keluhan demam dan batuk",
  "appointmentType": "RegularCheckup",
  "priority": "Normal",
  "referralDoctorId": null
}
```

### Response (200 OK)
```json
{
  "success": true,
  "data": {
    "id": 1,
    "appointmentNumber": "APT-20260120-001",
    "status": "Scheduled",
    "date": "2026-01-20",
    "slotTime": "09:00",
    "doctor": { "id": 5, "fullName": "dr. Suparno, Sp.PD" },
    "patient": { "id": 1, "fullName": "Budi Santoso", "medicalRecordNumber": "MR-000001" },
    "queueNumber": 12,
    "createdAt": "2026-01-15T11:00:00Z"
  }
}
```

---

## 6. Medical Record Service Endpoints (blueprint)

> Service ini **belum dikoding** — kontrak di bawah bersifat *draft rencana* (prefix `/api/v1`). Selaraskan dgn `05-Microservices/05-…` + DTO saat implement.

### Rekam Medis
| Method | URL | Deskripsi | Auth (rencana) |
|---|---|---|---|
| POST | `/api/v1/medical-records` | Buat draft rekam utk kunjungan | Doctor |
| GET | `/api/v1/medical-records/patient/{patientId}` | Semua rekam pasien (page) | Doctor/Nurse/Admin; pasien = own |
| GET | `/api/v1/medical-records/{id}` | Detail rekam + subkoleksi | berdasar rule akses |
| PUT | `/api/v1/medical-records/{id}` | Update draf | Doctor |
| POST | `/api/v1/medical-records/{id}/finalize` | Finalize (kunci) | Doctor ybs |

### Vital Signs
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/medical-records/{id}/vital-signs` | Tambah vital | Doctor/Nurse |
| GET | `/api/v1/medical-records/patient/{patientId}/vital-signs/latest` | Vital terakhir | Doctor/Nurse |

### Diagnosis / Treatment / Orders
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/medical-records/{id}/diagnoses` | Tambah diagnosis (ICD-10) | Doctor |
| GET | `/api/v1/medical-records/{recordId}/diagnoses` | List diagnosis | Doctor/Admin |
| POST | `/api/v1/medical-records/{id}/treatments` | Tambah tindakan | Doctor/Nurse |
| POST | `/api/v1/medical-records/{id}/prescriptions` | Buat order resep → Pharmacy | Doctor |
| POST | `/api/v1/medical-records/{id}/lab-orders` | Buat order lab → Lab | Doctor |

### Contoh Request/Response (draft)

#### POST /api/v1/medical-records
```json
// Request
{
  "appointmentId": "11111111-...",
  "patientId": "aaaaaaaa-...",
  "doctorId": "bbbbbbbb-...",
  "visitType": "Outpatient",
  "department": "Poli Umum",
  "subjective": "Demam dan batuk sejak 2 hari",
  "objective": "T: 38.5 C, TD 120/80",
  "assessment": "Febris, suspek ISPA",
  "plan": "Terapi simptomatik",
  "vitalSigns": { "temperature": 38.5, "systolic":120, "diastolic":80, "heartRate":90, "oxygenSaturation":98 },
  "diagnosisCodes": ["J06.9"]
}
// Response 201: ApiResponse -> { "success":true, "data": { "id":"ff..0", "recordNumber":"MRR-2026-0001",
//    "status":"Draft" }, "message":"Rekam dibuat" }
```

#### POST /api/v1/medical-records/{id}/finalize
```json
// Response 200: { success:true, message:"Rekam difinalisasi", data:{ "id":"ff..", "status":"Finalized", "finalizedAt":"2026-01-20T11:00:00Z" } }
```
> Catatan pasca-final: update hanya melalui `amend` tercatat.

---

## 7. Pharmacy Service Endpoints (blueprint)

> 🔲 Belum dikoding — kontrak draft, prefix `/api/v1`, id Guid. Konten lama (versi `int`) tetap sebagai contoh bentuk payload item resep.

### Prescription
| Method | URL | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/api/v1/pharmacy/prescriptions` | Buat resep (dari dokter/medical record) | Doctor |
| GET | `/api/v1/pharmacy/prescriptions/{id}` | Detail + items | Doctor/Pharmacist |
| GET | `/api/v1/pharmacy/prescriptions/patient/{patientId}` | Riwayat resep pasien | Doctor/Pharmacist |
| PUT | `/api/v1/pharmacy/prescriptions/{id}` | Ubah (draft) | Doctor |
| POST | `/api/v1/pharmacy/prescriptions/{id}/cancel` | Batal | Doctor |

### Dispensing
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/pharmacy/prescriptions/{id}/dispense` | Dispense penuh/partial | Pharmacist |
| GET | `/api/v1/pharmacy/dispensings?status=` | Antrean/status dispensing | Pharmacist |

### Contoh payload resep (draft) — form GUID
```json
// POST /api/v1/pharmacy/prescriptions
{
  "patientId": "11111111-...",
  "doctorId": "22222222-...",
  "medicalRecordId": "33333333-...",
  "priority": "Normal",
  "notes": "Antibiotik 7 hari",
  "items": [
    {
      "inventoryItemId": "44444444-...",
      "medicationName": "Amoxicillin 500mg",
      "dosage": "3x sehari",
      "durationDays": 7,
      "quantity": 21,
      "unit": "pcs",
      "instructions": "Diminum setelah makan"
    }
  ]
}
// Response 201 -> data { "id":"..g","prescriptionNumber":"RCP-2026-0001","status":"Pending" }
```
> **Draft lengkap (versi int)** untuk melihat kemungkinan field obat tersimpan — tetap dipakai sebagai referensi skema; tuangkan ulang ke Guid saat implement.

---

## 8. Laboratory Service Endpoints (blueprint)

> 🔲 Draft — prefix `/api/v1`, id Guid. Versi lama (`/api/laboratory/...`) berfungsi sebagai referensi bentuk payload hasil.

### Lab Orders
| Method | URL | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/api/v1/laboratory/orders` | Buat order lab (multi test) | Doctor |
| GET | `/api/v1/laboratory/orders` | List (status/patient/date) | LabStaff/Doctor |
| GET | `/api/v1/laboratory/orders/{id}` | Detail + hasil | LabStaff/Doctor/Admin |
| POST | `/api/v1/laboratory/orders/{id}/collect` | Koleksi sampel | LabStaff/Nurse |
| POST | `/api/v1/laboratory/orders/{id}/cancel` | Batal | Doctor/LabStaff |

### Lab Results
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/laboratory/orders/{orderTestId}/results` | Input hasil | LabStaff |
| GET | `/api/v1/laboratory/orders/{id}/results` | Lihat hasil | Doctor/LabStaff/Admin |
| GET | `/api/v1/laboratory/patients/{patientId}/results` | Semua hasil pasien | Doctor/LabStaff |

### Catalog
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| GET | `/api/v1/laboratory/tests` | Master test | LabStaff/Doctor/Admin |
| POST | `/api/v1/laboratory/tests` | Tambah test | Admin |

### Contoh (draft, Guid)
```json
// POST /api/v1/laboratory/orders
{ "patientId":"11..g", "doctorId":"22..g", "visitId":"33..g",
  "priority":"Routine",
  "tests":[ { "catalogId":"44..g", "specimenType":"Blood" } ] }
// Response 201 -> { "success":true, "data": { "id":"..g","orderNumber":"LO-2026-0001","status":"Ordered" } }
```

---

## 9. Billing Service Endpoints (blueprint)

> 🔲 Draft — prefix `/api/v1`, id Guid.

### Invoice / Billing
| Method | URL | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/api/v1/billing/invoices` | Buat invoice (dari appointment/lab/pharmacy/tindakan) | Billing/Admin |
| GET | `/api/v1/billing/invoices` | List invoices (status/date/paging) | Billing/Admin |
| GET | `/api/v1/billing/invoices/{id}` | Detail invoice + line items | Owner/Billing/Admin |
| GET | `/api/v1/billing/invoices/patient/{patientId}` | Invoices pasien | Patient/Billing/Admin |
| POST | `/api/v1/billing/invoices/{id}/issue` | Terbitkan invoice | Billing/Admin |
| POST | `/api/v1/billing/invoices/{id}/payment` | Catat pembayaran | Billing/Admin |
| POST | `/api/v1/billing/invoices/{id}/cancel` | Batal | Billing/Admin |

### Pricing / Master
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| GET | `/api/v1/billing/services` | Master tarif & layanan | Billing/Admin |
| POST | `/api/v1/billing/services` | Tambah tarif | Admin |

### Contoh request/response (draft, Guid)
```json
// POST /api/v1/billing/invoices
{
  "patientId": "11111111-...",
  "invoiceType": "Consultation",
  "appointmentId": "33333333-...",
  "currency": "IDR",
  "lineItems": [ { "serviceCode": "CONSULT-GENERAL", "quantity": 1 } ]
}
// Response 201 -> data: { "id":"..g","invoiceNumber":"INV-20260120-0001",
//   "status":"Draft","subtotal":150000,"totalAmount":150000,"amountDue":150000 }
```

---

## 10. Inventory Service Endpoints (blueprint)

> 🔲 Draft — prefix `/api/v1`, id Guid.

### Items
| Method | URL | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/api/v1/inventory/items` | Tambah item | Pharmacist/Admin |
| GET | `/api/v1/inventory/items` | List + filter (category/search) | Pharmacist/Admin/Doctor(look) |
| GET | `/api/v1/inventory/items/{id}` | Detail item + batches | Pharmacist/Admin |
| PUT | `/api/v1/inventory/items/{id}` | Update item | Pharmacist/Admin |
| GET | `/api/v1/inventory/items/low-stock` | List stok menipis | Pharmacist/Admin |
| GET | `/api/v1/inventory/items/expiring` | Mendekati kedaluwarsa | Pharmacist |
| POST | `/api/v1/inventory/items/{id}/adjust` | Adjust manual | Pharmacist/Admin |

### Stock Movement & Supplier
| Method | URL | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/inventory/receivings` | Terima barang masuk (buat batch) | Pharmacist/Admin |
| GET | `/api/v1/inventory/movements` | Riwayat movements (item/date) | Pharmacist/Admin |
| GET/POST | `/api/v1/inventory/suppliers` | Master supplier | GET (login)/POST admin |

### Contoh request (draft)
```json
// POST /api/v1/inventory/items/{id}/adjust
{ "quantityChange": 15, "reason": "Stock opname", "by": "ph1" }
// POST /api/v1/inventory/receivings
{ "supplierId":"11..g", "receivedDate":"2026-01-20",
  "items":[{ "itemId":"22..g","receivedQuantity":500,"batchNumber":"AMX-2026-0145",
             "expirationDate":"2027-06-30","unitCost":12500.00 }] }
// Response 200 -> current stock bertambah, movement type=IN tercatat
```

---

## 11. Notification Service Endpoints (blueprint)

> 🔲 Draft — prefix `/api/v1`. Service ini kebanyakan **consumer event**; endpoint di bawah utk mengelola inbox/template/delivery via API.

### Notification (Inbox/Management)
| Method | URL | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/api/v1/notifications` | Kirim notifikasi (API/system) | System/Admin |
| GET | `/api/v1/notifications/user/{userId}` | Peroleh kotak masuk | Owner |
| GET | `/api/v1/notifications/user/{userId}/unread-count` | Jumlah unread (badge) | Owner |
| GET | `/api/v1/notifications/{id}` | Detail notif/status delivery | Owner/Admin |
| PUT | `/api/v1/notifications/{id}/read` | Tandai dibaca | Owner |
| GET/POST | `/api/v1/notifications/templates` | Master template | Admin |

### Contoh request (draft — versi lama § lampiran referensi bentuk payload)
```json
// POST /api/v1/notifications
{
  "recipientUserId": "11111111-...",
  "typeCode": "APPOINTMENT_REMINDER",
  "channels": ["Email","SMS"],
  "payload": {
    "patientName": "Budi Santoso",
    "doctorName": "dr. Suparno, Sp.PD",
    "appointmentDate": "2026-01-20",
    "appointmentTime": "09:00",
    "location": "Poli Umum - R-101"
  },
  "priority": "Normal"
}
// -> Template di-render dari typeCode; status Pending → provider dispatch → log
```

---

## 12. Event Contract (Public Transaksi Data Primary Keys)

Untuk sinkronisasi data between services, event menggunakan **foreign key yang disimpan sebagai external ID**.

| Service | External ID Field | Referensi |
|---|---|---|
| Patient | `patientId` (int) | Referensi ke Patient Service |
| Doctor | `doctorId` (int) | Referensi ke Doctor Service |
| Appointment | `appointmentId` (int) | Referensi ke Appointment Service |

> **Catatan**: Service tidak menyimpan field tambahan (seperti `patientName`) untuk menghindari denormalisasi yang tidak perlu. Melainkan melakukan API call ke service sumber saat perlu menampilkan nama.

Namun untuk **event payload**, data context dikirim bersama untuk menghindari roundtrip:
```json
{
  "eventId": "guid",
  "eventType": "appointment.booked",
  "timestamp": "2026-01-15T11:00:00Z",
  "data": {
    "appointmentId": 1,
    "patientId": 1,
    "doctorId": 5,
    "scheduledDate": "2026-01-20",
    "slotTime": "09:00",
    "status": "Scheduled"
  },
  "metadata": {
    "correlationId": "req-abc123"
  }
}
```
