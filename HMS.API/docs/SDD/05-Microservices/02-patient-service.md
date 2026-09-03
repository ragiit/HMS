# Patient Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft implementable sesuai pola `Identity` di 01-…).
> Saat mulai mengoding, ikuti pola Clean Layer + `Guid` PK + prefix `/api/v1` + `ApiResponse<T>` persis seperti Identity service.

## 1. Overview & Peran (apa yang dikerjakan)
Menangani **registrasi & manajemen profil pasien** (end-user klinis): data demografis, kontak, alamat, kontak darurat, dan informasi asuransi/BPJS. Pasien adalah **source of truth** data pasien; layanan lain (Appointment, MedicalRecord, Billing, Lab, Pharmacy) hanya menyimpan `PatientId` & memanggil/men-subscribe detail ketika butuh menampilkan nama.

Penting — di HMS ini **Patient Service TIDAK membuat akun login**. Pembuatan akun login selalu lewat Identity; di sini pasien "bisa dihubungkan" ke akun (identitas opsi) via userId (opsional).

## 2. Tanggung Jawab / Fungsi Utama (checklist kerja)
1. **Registrasi pasien** — buat profil + generate No Rekam Medis.
2. **Pencarian pasien** — by nama/NIK/no. HP/MRNumber (search).
3. **CRUD profil** — update demografis, alamat, kontak, emergency contact.
4. **Asuransi/BPJS** — tambah/kelola polis & status aktif.
5. **Deaktivasi / soft-delete** pasien.
6. **Riwayat kunjungan (view)** — menampilkan aggregate hasil-subscribe saat appointment completed (opsional).
7. Kelola status pasien: `Active / Inactive / Deceased`.

## 3. Bounded Context
- Registrasi pasien (Pendaftaran)
- Manajemen demografis, alamat, kontak, emergency contact
- Informasi asuransi (BPJS & swasta)
- Penomoran Medical Record Number (`MR-<yyyy>-<seq>`)
- Pencarian & status pasien

## 4. Domain Model (rancangan sesuai pola Identity, tipe Guid)
```
Patient (AggregateRoot<Guid>; IAuditableEntity)
 ├─ MedicalRecordNumber, FirstName, LastName, DateOfBirth
 ├─ Gender, BloodType, NationalIdNumber, PhoneNumber, SecondaryPhone, Email
 ├─ MaritalStatus, Nationality, Religion, Occupation, ProfilePhotoUrl
 ├─ Status (Active/Inactive/Deceased), IsDeleted
 ├─ audit fields + RowVersion (opsional concurrency)
 ├─ Addresses : ICollection<Address>
 ├─ Contacts  : ICollection<Contact>
 ├─ EmergencyContacts : ICollection<EmergencyContact>
 ├─ Insurances: ICollection<InsuranceCoverage>
 ├─ Register(), UpdateDemographics(), AddEmergencyContact(),
 ├─ UpdateInsurance(...), Activate(), Deactivate(), MarkDeleted()
 └─ (optional) UserId nullable -> link ke Identity user

Value Object (immutable, dipegang aggregate):
 ├─ Address        { AddressType, Street, City, Province, SubDistrict, PostalCode, Country, IsPrimary }
 ├─ EmergencyContact { Name, Relationship, PhoneNumber, Address }
 └─ InsuranceCoverage { ProviderName, PolicyNumber, CoverageType, ValidFrom, ValidTo, IsActive, IsPrimary }
```
> ⚠️ **Konvensi HMS (Guid):** skema DB lama di `04-Database-Design/02-…` menulis `Id INT IDENTITY`. Saat implement, ganti jadi **`Guid`** agar seragam & aman lintas DB/service (lihat catatan pada Identity).

## 5. CQRS — daftar yang akan dibuat

### Commands (MediatR)
| Command | Tanggung jawab handler |
|---|---|
| `RegisterPatientCommand(...)` | validasi, gen MR number, create+persist → `Guid` |
| `UpdateDemographicsCommand(PatientId, ...)` | ubah data demografis |
| `UpdateAddressCommand`/`RemoveAddress` | tambah/ubah alamat (primary flag) |
| `Add/UpdateEmergencyContactCommand` | kelola kontak darurat |
| `AssignInsuranceCommand(PatientId, Policy...)` | tambah/aktifkan asuransi/BPJS |
| `DeactivatePatientCommand(PatientId)` | set Status=Inactive / `MarkDeleted` |

### Queries
| Query | Tanggung jawab |
|---|---|
| `GetPatientByIdQuery(Id)` | profil + sub-koleksi → `PatientDto` |
| `GetPatientByMRNumberQuery(MR)` | lookup by MR |
| `SearchPatientsQuery(term/paging)` | nama/NIK/phone/MR |
| `GetPatientDetailQuery(Id)` | komplit (summary/emergency) |
| `GetPatientHistoryQuery(Id)` | riwayat kunjungan (dari event/agregasi) |

### Validasi
`ValidationBehaviour` + FluentValidation validator per command (mirip Identity).

## 6. Events (blueprint sesuai kebijakan sinkronisasi)

### Publishes
| Integration Event | Dipicu | Konsumen potensial |
|---|---|---|
| `PatientCreatedEvent` (id, MR, nama inti) | registrasi berhasil | Notification / Appointment |
| `PatientUpdatedEvent` | demografis/status | yang men-cache nama |
| `PatientDeactivatedEvent` | soft-delete | Semua yg punya ref pasien |

### Subscribes (opsional yang akan didukung di masa depan)
- `AppointmentCompletedEvent` → simpan "visit count / last visit" agregasi ringkas (denormalisasi diizinkan untuk view riwayat) — DEKAT jika perlu agregat read.

> Identitas asing: field `patientId` dipakai sebagai tidak terkunci referensi global; tetapi detail nama didapat via call/subscribe, tidak duplikasi disakralkan (data inti pasien HANYA di service ini).

## 7. API Endpoint (preview per rancangan; detail ada di 03-API-Design)
Base: `/api/v1` (contoh: `POST /api/v1/patients`).
| Method | Path | Deskripsi singkat | Auth (rencana) |
|---|---|---|---|
| POST | `/patients` | registrasi pasien | Bearer |
| GET | `/patients` | list/pencarian (paging) | Bearer (klinis) |
| GET | `/patients/search` | cari by term | Bearer |
| GET | `/patients/{id}` | detail profil | Bearer klinis |
| PUT | `/patients/{id}` | update demografis | Bearer frontdesk/admin |
| DELETE | `/patients/{id}` | soft-delete | Bearer admin |
| (sub) | `/patients/{id}/insurance` | kelola asuransi | Bearer frontdesk |
| (sub) | `/patients/{id}/emergency-contact` | kelola contact7 | Bearer |
| GET | `/patients/{id}/medical-history` | view riwayat | Bearer klinis |

> (Sebelum implementasikan endpoint baris-baris di atas, samakan dgn daftar utuh pada 03-API-Design bagian 3 Patient; pertimbangkan pemindahan koleksi & pagination ke sub-route yang konsisten.)

## 8. Dependencies & konsumen
- **Inbound (sync)**: Appointment, MedicalRecord, Billing, Pharmacy, Lab membutuhkan detail pasien.
- **Outbound (sync for data list)**: tidak ada (pemilik data).
- **Outbound (async)**: event ke Rabbit via outbox.

## 9. Database pointer
Skema lengkap (sementara memakai `int`) ada di `04-Database-Design/02-patient-service-db.md` (Tabel: `Patients`, `PatientAddresses`, `PatientContacts`, `PatientEmergencyContacts`, `PatientInsurances`, `OutboxEvents`, `InboxEvents`).
Saat implement: ubah `Id` jadi Guid, pertahankan unique constraint `MedicalRecordNumber` & `NationalIdNumber`, serta index pencarian.

## 10. Pekerjaan akan datang (urutan yang disarankan bila meniru Identity)
1. Scaffold sln project Patient.Api/Application/Domain/Infrastructure (Clean).
2. Entitas & konfigurasi (Guid, ValueObject address/insurance, unique MR/NIK).
3. Migration `InitialCreate`, seeder role bila perlu.
4. Command Register + query lookup/search (paling sering dipakai dulu).
5. Controller `/api/v1/patients`...; wrap ApiResponse; JwtBearer dari Shared.
6. Outbox+domain events pemetaan (pakai pattern Identity).
7. Test koneksi & seed contoh pasien.

## 11. Catatan produk & keputusan yang perlu dikonfirmasi saat implement
- Skema No RM `MR-{year}-{seq}`: sumber sequence (DB/DB-generated) — pastikan idempoten saat banyak node.
- Apakah NIK boleh invalid/decreed? validasi NIK 16 digit (opsional).
- Multi-channel kontak + verifikasi: apakah perlu `ContactVerified` endpoint? (untuk reminder mobilenotif).
- Link ke akun Identity (login pasien) — perlu flow terpisah yang belum di-design.
