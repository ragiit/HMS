# Doctor Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity).
> Catat konvensi HMS: `Guid` PK & `UserId` berikut benar-benar mengacu `User.Id` (Guid) dari Identity — sehingga field `Doctors.UserId` perlu `Guid`, BUKAN `int` seperti di dok skema lama.

## 1. Overview & Peran
Data master **dokter** klinis HMS: profil (gelar, spesialisasi, izin STR/SIP), jadwal praktek reguler mingguan, serta pengecualian jadwal (cuti/off/penggantian). Appointment Service memakai jadwal ini (masternya) sebagai dasar slot booking.

## 2. Tanggung Jawab / Fungsi Utama (checklist kerja)
1. Profil dokter: buat/update/detail (identitas, lisensi, pendidikan/biografi, foto, kontak).
2. Master **spesialisasi** (seed) + assign ke dokter.
3. **Jadwal praktyk tetap** per hari-minggu (jam, slot, ruang, kuota).
4. **Jadwal pengecualian** (cuti/off/rumah/penggantian) per tanggal.
5. Cek & nonaktifkan otomatis saat SIP expired (pekerjaan background/event alarm).
6. Cari dokter (by nama/spesialisasi) & cek ketersediaan (available) oleh unit lain.

## 3. Bounded context
- Profil & lisensi dokter
- Spesialisasi & kompetensi (board cert)
- Jadwal praktek mingguan
- Siklus efisiensi: cancellations / exceptions
- Ketersediaan dokter (availability)

## 4. Domain model (Guid — koreksi penting)
```
Doctor (AggregateRoot<Guid>; IAuditableEntity)
 ├─ UserId? (Guid -> Identity.Users.Id)  — link akun login dokter  [tidak membuat akun sendiri]
 ├─ EmployeeNumber (NIP), Title, First/LastName, Gender
 ├─ Contact: Email, PhoneNumber
 ├─ LicenseNumber (STR), SIPNumber, SIPExpiryDate
 ├─ EducationBackground, Biography, ProfilePhotoUrl, Status(Active/Inactive/OnLeave)
 ├─ Rating, TotalRatings, IsDeleted + audit
 ├─ Specializations : List<DoctorSpecialization> (join ke Specialization)
 ├─ Schedules      : List<DoctorSchedule>   (mingguan tetap)
 ├─ Exceptions     : List<ScheduleException> (per tanggal)
 ├─ Create/Update/CreateException/Activate/Deactivate/MarkDeleted

Specialization (Entity<Guid>, master)
 ├─ Code, Name, Description, IsActive

DoctorSpecialization   -> DoctorId + SpecializationId, IsPrimary, IsBoardCertified

DoctorSchedule (Entity<Guid>)
 ├─ DayOfWeek(1-7), StartTime, EndTime, SlotDurationMinutes,
 ├─ MaxPatientsPerSlot, MaxDailyPatients, Location, RoomNumber, IsActive
 ├─ (validation Start<End, Slot>0)

ScheduleException (Entity<Guid>)
 ├─ ScheduleId?, ExceptionDate, StartTime?, EndTime?, IsCancelled, Reason
```
> ⚠️ Skema lama `04-Database-.../03-` menampilkan `Id/Int` dsb. KONVERSI ke `Guid` ketika implement.

## 5. CQRS — yang akan dibuat
### Commands
| Command | Tanggung jawab |
|---|---|
| `CreateDoctorCommand(...)` | buat profil+link UserId opsi → `Guid` |
| `UpdateDoctorCommand(Id,...)` | ubah profil/lisensi/kontak |
| `SetSpecializationCommand(Id, specIds)` | assign/ulas spealisasi |
| `SetScheduleCommand(Id, CreateScheduleDto)` | tambah jadwal mingguan |
| `UpdateScheduleCommand(Id, scheduleId, ...)` | ubah jadwal |
| `RemoveScheduleCommand(Id, scheduleId)` | hapus/aktif=false jadwal |
| `AddExceptionCommand(Id, date, ...)` | cuti/off/penggantian |
| `DeactivateDoctorCommand(Id)` | soft deactive |

### Queries
| Query | Tanggung jawab |
|---|---|
| `GetDoctorsQuery(page,filter)` | list + paging |
| `GetDoctorByIdQuery(Id)` | profil+spec+schedules |
| `GetDoctorScheduleQuery(Id, from,to)` | jadwal per periode |
| `GetAvailableDoctorsQuery(from,to)` | dokter available (tanpa tabrakan jadwal/cuti) |
| `SearchDoctorsBySpecQuery(spec)` | filter by spesialisasi |
| `GetExceptionsQuery(Id, range)` | list cuti/off |

## 6. Events (blueprint)
### Publishes
| Integration Event | Dipicu | Target utama |
|---|---|---|
| `DoctorCreatedEvent` | dr baru dibuat | Notification |
| `DoctorUpdatedEvent` | profil berubah | (caching denormanam) |
| `DoctorScheduleChangedEvent(doctorId, day…)` | jadwal berubah | Appointment (resync slot) |
| `DoctorLeaveCreatedEvent(doctorId,date)` | cuti di-tambah | Appointment (rekonsiliasi appointment terisi) |

### Subscribes
- (none utama semasa awal; Appointment boleh memberi tahu ke Doctor bila dibutuhkan agregat)

## 7. API Endpoint (preview, prefix `/api/v1`)
| Method | Path | Deskripsi | Auth rencana |
|---|---|---|---|
| POST | `/doctors` | buat dokter | Admin |
| GET | `/doctors` | list/paging/filter | Login (klinis) |
| GET | `/doctors/{id}` | detail | Login |
| PUT | `/doctors/{id}` | update profil | Admin/Dokter ybs |
| DELETE | `/doctors/{id}` | soft-delete | Admin |
| GET | `/doctors/{id}/schedules` | jadwal dr | FrontDesk/Doctor |
| POST | `/doctors/{id}/schedules` | tambah jadwal | Admin/Dr |
| PUT | `/doctors/{id}/schedules/{sid}` | update jadwal | " |
| POST | `/doctors/{id}/exceptions` | cuti/off | Admin/Dr |
| GET | `/doctors/available?from&to` | cek ketersediaan | FrontDesk/Dr |
| GET | `/doctors/search?spec=` | cari by spesialisasi | FrontDesk |
| (master) | `/specializations` GET/POST | daftar/update spesialisasi | GET-all / POST admin |

## 8. Dependencies
- Outbound sync: Identity (validasi user, list akun dr), Appointment opsional bila perlu informasi booking.
- Inbound sync: dipanggil Appointment, MedicalRecord, Pharmacy.
- Outbound async: event jadwal/cuti ke Rabbit.

## 9. DB pointer (draft schema)
`04-Database-Design/03-doctor-service-db.md`: tabel `Doctors, Specializations(+seed), DoctorSpecializations, DoctorSchedules, ScheduleExceptions, OutboxEvents, InboxEvents`. Ubah PK jadi Guid saat implement; tambahkan unique NIP/license/SIP; seeder spesialisasi.

## 10. Urutan implement (meniru Identity)
1. Scaffold Clean projects `Doctor.Api/Application/Domain/Infrastructure`.
2. Entity + value object + konfigurasi Guid & unique & FK.
3. Migration InitialCreate + seeder `Specializations`.
4. Query getDoctors/getById + command create/update first.
5. Controller `/api/v1/doctors`... wrap ApiResponse; JwtBearer Shared.
6. Jadwal + exception + availability (oleh dokter).
7. Outbox plugin utk event schedule.
8. Integrasi-hit dengan Identity untuk menautkan kesehatan (opsional).

## 11. Catatan keputusan yg perlu dinegosiasikan saat implement
- UserId doctor: apakah wajib di-buat akun Identity dulu? (flow: Identity buat user role=Doctor, lalu Doctor store link?).
- SIP auto-expire: mekanisme (hangtime check vs background).
- Slot-kuota & bed: apakah kuota ditentukan DoctorService atau di Appointment? (dok menyatakan master di Doctor, booking di Appointment).
