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

## 2. Identity Service Endpoints

### Authentication
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/auth/login` | Login, return JWT + refresh token | Public | - |
| POST | `/api/auth/refresh` | Refresh access token | Public | - |
| POST | `/api/auth/logout` | Revoke refresh token | Bearer | All |
| POST | `/api/auth/change-password` | Ganti password user | Bearer | All |

### User Management
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/users` | Buat user baru | Bearer | Admin |
| GET | `/api/users` | List semua users | Bearer | Admin |
| GET | `/api/users/{id}` | Detail user | Bearer | Admin |
| PUT | `/api/users/{id}` | Update user | Bearer | Admin |
| DELETE | `/api/users/{id}` | Hapus user (soft delete) | Bearer | Admin |
| PUT | `/api/users/{id}/roles` | Assign roles ke user | Bearer | Admin |
| GET | `/api/users/{id}/roles` | List roles user | Bearer | Admin |

### Roles
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| GET | `/api/roles` | List semua roles | Bearer | Admin |
| POST | `/api/roles` | Buat role baru | Bearer | Admin |

### Request/Response Contracts

#### POST /api/auth/login
```json
// Request
{
  "username": "dr.suparno",
  "password": "secret123"
}

// Response (200)
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1...",
    "refreshToken": "5f0b8c9d-...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": 123,
      "username": "dr.suparno",
      "fullName": "dr. Suparno, Sp.PD",
      "roles": ["Doctor"],
      "correlationId": "req-abc123"
    }
  }
}
```

---

## 3. Patient Service Endpoints

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

## 6. Medical Record Service Endpoints

### Medical Records
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/medical-records` | Buat rekam medis baru | Bearer | Doctor, Nurse |
| GET | `/api/medical-records/patient/{patientId}` | Semua rekam medis pasien | Bearer | Doctor, Nurse, Admin |
| GET | `/api/medical-records/{id}` | Detail rekam medis | Bearer | Doctor, Nurse, Admin |
| PUT | `/api/medical-records/{id}` | Update rekam medis | Bearer | Doctor, Admin |

### Vital Signs / Triage
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/medical-records/{recordId}/vital-signs` | Input tanda vital | Bearer | Doctor, Nurse |
| GET | `/api/medical-records/{patientId}/vital-signs/latest` | Vital signs terakhir | Bearer | Doctor, Nurse |

### Diagnosis
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/medical-records/{recordId}/diagnoses` | Tambah diagnosis | Bearer | Doctor |
| GET | `/api/medical-records/{recordId}/diagnoses` | List diagnosis | Bearer | Doctor, Admin |

### Request (POST /api/medical-records)
```json
{
  "appointmentId": 1,
  "patientId": 1,
  "doctorId": 5,
  "visitType": "Outpatient",
  "subjective": "Pasien mengeluh demam sejak 2 hari lalu, disertai batuk",
  "objective": "Suhu 38.5°C, tensi 120/80, nadi 90x/mnt",
  "assessment": "Febris, kemungkinan ISPA",
  "plan": "Resep antibiotik, paracetamol, kontrol 3 hari",
  "vitalSigns": {
    "temperature": 38.5,
    "systolic": 120,
    "diastolic": 80,
    "heartRate": 90,
    "respiratoryRate": 20,
    "oxygenSaturation": 98,
    "weightKg": 68,
    "heightCm": 170
  },
  "diagnosisCodes": ["J06.9", "R50.9"],
  "prescriptionNotes": "Amoxicillin 500mg 3x1, Paracetamol 500mg 3x1"
}
```

---

## 7. Pharmacy Service Endpoints

### Prescription
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/pharmacy/prescriptions` | Buat resep dari dokter | Bearer | Doctor |
| GET | `/api/pharmacy/prescriptions/{id}` | Detail resep | Bearer | Doctor, Pharmacist |
| GET | `/api/pharmacy/prescriptions/patient/{patientId}` | Resep pasien | Bearer | Doctor, Pharmacist |
| PUT | `/api/pharmacy/prescriptions/{id}` | Update resep | Bearer | Doctor (draft only) |

### Dispensing
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/pharmacy/prescriptions/{id}/dispense` | Dispense obat | Bearer | Pharmacist |
| POST | `/api/pharmacy/dispensing` | Proses dispensing (with inventory check) | Bearer | Pharmacist |
| GET | `/api/pharmacy/dispensing/{id}` | Status dispensing | Bearer | Pharmacist, Doctor |

### Request (POST /api/pharmacy/prescriptions)
```json
{
  "medicalRecordId": 100,
  "patientId": 1,
  "doctorId": 5,
  "appointmentId": 1,
  "date": "2026-01-20",
  "status": "Pending",
  "items": [
    {
      "inventoryItemId": 45,
      "medicationName": "Amoxicillin 500mg",
      "strength": "500mg",
      "dosage": "3x sehari",
      "durationDays": 7,
      "quantity": 21,
      "instructions": "Diminum setelah makan"
    },
    {
      "inventoryItemId": 78,
      "medicationName": "Paracetamol 500mg",
      "strength": "500mg",
      "dosage": "3x sehari",
      "durationDays": 5,
      "quantity": 15,
      "instructions": "Saat demam"
    }
  ]
}
```

---

## 8. Laboratory Service Endpoints

### Lab Orders
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/laboratory/orders` | Buat order pemeriksaan lab | Bearer | Doctor |
| GET | `/api/laboratory/orders` | List orders (filter by status/patient) | Bearer | LabStaff, Doctor |
| GET | `/api/laboratory/orders/{id}` | Detail order | Bearer | LabStaff, Doctor, Admin |
| POST | `/api/laboratory/orders/{id}/collect` | Sample collection | Bearer | LabStaff, Nurse |
| GET | `/api/laboratory/tests` | Master data tests | Bearer | LabStaff, Doctor, Admin |

### Lab Results
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/laboratory/orders/{id}/results` | Input hasil lab | Bearer | LabStaff |
| GET | `/api/laboratory/orders/{id}/results` | Lihat hasil lab | Bearer | Doctor, LabStaff, Admin |
| GET | `/api/laboratory/patients/{patientId}/results` | Semua hasil lab pasien | Bearer | Doctor, LabStaff |

### Request (POST /api/laboratory/orders)
```json
{
  "visitId": "APT-20260120-001",
  "patientId": 1,
  "doctorId": 5,
  "orderDate": "2026-01-20",
  "priority": "Routine",
  "tests": [
    {
      "testCode": "CBC",
      "testName": "Complete Blood Count",
      "specimenType": "Blood"
    },
    {
      "testCode": "GLU",
      "testName": "Fasting Blood Glucose",
      "specimenType": "Blood"
    }
  ],
  "billingReferenceId": "BL-20260120-010"
}
```

### Response (POST hasil lab)
```json
{
  "success": true,
  "data": {
    "testCode": "CBC",
    "results": [
      { "parameter": "WBC", "value": "8.5", "unit": "10^3/uL", "referenceRange": "4.5 - 11.0", "flag": "Normal" },
      { "parameter": "Hemoglobin", "value": "13.5", "unit": "g/dL", "referenceRange": "12.0 - 16.0", "flag": "Normal" },
      { "parameter": "Platelets", "value": "250", "unit": "10^3/uL", "referenceRange": "150 - 400", "flag": "Normal" }
    ],
    "status": "Completed",
    "completedBy": "aq_analis01",
    "resultDate": "2026-01-20T14:00:00Z"
  }
}
```

---

## 9. Billing Service Endpoints

### Invoice / Billing
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/billing/invoices` | Buat invoice (dari appointment/lab/pharmacy) | Bearer | BillingStaff, Admin |
| GET | `/api/billing/invoices` | List invoices | Bearer | BillingStaff, Admin |
| GET | `/api/billing/invoices/{id}` | Detail invoice | Bearer | All (owner) |
| GET | `/api/billing/invoices/patient/{patientId}` | Invoices pasien | Bearer | Patient, BillingStaff, Admin |
| POST | `/api/billing/invoices/{id}/payment` | Proses pembayaran | Bearer | BillingStaff, Admin |
| GET | `/api/billing/payments` | List payments | Bearer | BillingStaff, Admin |

### Pricing / Master
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| GET | `/api/billing/services` | Master data layanan & biaya | Bearer | BillingStaff, Admin |
| POST | `/api/billing/services` | Tambah master layanan | Bearer | Admin |

### Invoice Request (Create)
```json
{
  "patientId": 1,
  "appointmentId": 1,
  "invoiceType": "Consultation",
  "currency": "IDR",
  "lineItems": [
    {
      "serviceCode": "CONSULT-GENERAL",
      "description": "Konsultasi Dokter Umum",
      "quantity": 1,
      "unitPrice": 150000
    },
    {
      "serviceCode": "LAB-CBC",
      "description": "Complete Blood Count",
      "quantity": 1,
      "unitPrice": 85000
    }
  ],
  "discountAmount": 0,
  "taxPercent": 0,
  "paymentMethod": "Cash"
}
```

---

## 10. Inventory Service Endpoints

### Items Management
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/inventory/items` | Tambah item baru | Bearer | Pharmacist, Admin |
| GET | `/api/inventory/items` | List semua items | Bearer | Pharmacist, Admin, Doctor |
| GET | `/api/inventory/items/{id}` | Detail item | Bearer | Pharmacist, Admin |
| PUT | `/api/inventory/items/{id}` | Update item | Bearer | Pharmacist, Admin |
| GET | `/api/inventory/items/low-stock` | List stock menipis | Bearer | Pharmacist, Admin |
| POST | `/api/inventory/items/{id}/adjust` | Adjust stock (manual) | Bearer | Pharmacist, Admin |

### Stock Movements
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/inventory/receivings` | Terima barang masuk | Bearer | Pharmacist |
| GET | `/api/inventory/movements` | Riwayat stock movements | Bearer | Pharmacist, Admin |
| GET | `/api/inventory/movements/expiring` | Item mendekati expiration | Bearer | Pharmacist |

### Request (POST /api/inventory/receivings)
```json
{
  "supplierName": "PT. Farmasi Indonesia",
  "receiptNumber": "RCP-2026-001",
  "receivedDate": "2026-01-20",
  "items": [
    {
      "itemId": 45,
      "receivedQuantity": 500,
      "batchNumber": "AMX-2026-0145",
      "expirationDate": "2027-06-30"
    }
  ]
}
```

---

## 11. Notification Service Endpoints

### Notification
| Method | URL | Deskripsi | Auth | Role |
|---|---|---|---|---|
| POST | `/api/notifications` | Kirim notifikasi (API) | Bearer | System, Admin |
| GET | `/api/notifications/user/{userId}` | List notifikasi user | Bearer | User owner |
| GET | `/api/notifications/user/{userId}/unread-count` | Jumlah unread | Bearer | User owner |
| PUT | `/api/notifications/{id}/read` | Tandai dibaca | Bearer | User owner |
| PUT | `/api/notifications/{id}/delivered` | Update delivery status | Internally | System |

### Request (POST /api/notifications)
```json
{
  "recipientUserId": 123,
  "recipientContact": {
    "email": "budi@gmail.com",
    "phone": "081234567890"
  },
  "channels": ["Email", "SMS", "Push"],
  "templateCode": "APPOINTMENT_REMINDER",
  "payload": {
    "patientName": "Budi Santoso",
    "doctorName": "dr. Suparno, Sp.PD",
    "appointmentDate": "2026-01-20",
    "appointmentTime": "09:00",
    "location": "Poli Umum - R-101"
  },
  "priority": "Normal",
  "scheduledAt": "2026-01-20T08:00:00Z"
}
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
