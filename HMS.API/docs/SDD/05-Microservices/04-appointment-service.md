# Appointment Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid` · `/api/v1` · `ApiResponse<T>`).

## 1. Overview & Peran
Mengelola **janji temu (booking)** pasien↔dokter: pembuatan, state machine (Scheduled→…→Completed/Cancelled/NoShow), antrean, reschedule, pembatalan. Ini adalah jantung alur "pasien datang → pemeriksaan → rekam medis → billing".

Pasien & dokter memakai **id eksternal** (`patientId`, `doctorId`) — data detail diambil/subscribe; pembuatan booking rentan **double-booking** sehingga perlu lock.

## 2. Tanggung Jawab / Fungsi Utama
1. **Booking** appointment (validasi slot availability dgn lock, buat nomor antrean).
2. **Lifecycle**: Check-in, Start, Complete, Cancel (dgn alasan), NoShow.
3. **Reschedule** (pindah tanggal/jam, tetap dgn cek konflik).
4. **Antrean harian** per dokter/ruang.
5. Publikasi event bagi Billing/Notification/MedicalRecord.
6. Berlangganan perubahan jadwal doctor/status pasien utk cancel massal.

## 3. State Machine (lifecycle)
```
Scheduled ─CheckIn→ CheckedIn ─Start→ InProgress ─Complete→ Completed
   │   │                  │
   │   └────────────→ Cancelled (reason)
   │   └────────────→ NoShow
   └────────(Reschedule)→ Scheduled baru (date/jam bergeser)
```
Catatan : transisi yang "melompat mundur" dari Completed tidak diizinkan (kecuali admin revisi); NoShow vs Cancel berbeda utk billing (penalty).

## 4. Domain model (Guid)
```
Appointment (AggregateRoot<Guid>; IAuditableEntity)
 ├─ PatientId (Guid ref Patient service, non-FK lokal), DoctorId (Guid)
 ├─ ScheduleCourse basis: Date, StartTime, EndTime, Slot (derived)
 ├─ Status (enum), Priority, AppointmentType, Reason
 ├─ ReferralDoctorId? (rujukan)
 ├─ AppointmentNumber, QueueNumber
 ├─ CheckInTime?, StartTimeActual?, CompletedTime?, CancelReason?
 ├─ Book(), CheckIn(), Start(), Complete(cancel no), MarkNoShow(), Reschedule(newDetail)
 └─ (histori status rowset optional: AppointmentStatusHistory)

Enum: AppointmentType (Regular, FollowUp, Consultation, Referral, Emergency)
AppointmentStatus (Scheduled, CheckedIn, InProgress, Completed, Cancelled, NoShow)
```
> Catatan HMS: `patientId`,`doctorId` merupakan **external** Guid (bukan FK DB lokal). Sering kali simpan snapshot kecil (patientName/doctorName) utk daftar cepat — putuskan kebijakan (suggest snapshot kecil + publish).

## 5. CQRS yang akan dibuat
### Commands
| Command | Catatan |
|---|---|
| `BookAppointmentCommand(patientId, doctorId, date, slotStart, type, …)` | cek slot (transactional **UPDLOCK**), nomor antrean auto |
| `RescheduleAppointmentCommand(id, date, slot)` | cek konflik |
| `CancelAppointmentCommand(id, reason)` | →
| `CheckInCommand(id)` | |
| `StartAppointmentCommand(id)` | |
| `CompleteAppointmentCommand(id, notes?)` | trigger event completed |
| `MarkNoShowCommand(id)` | |

### Queries
| Query |
|---|
| `GetAppointmentsQuery(date,status,doctorId,page)` |
| `GetAppointmentByIdQuery(id)` |
| `GetDoctorAvailabilityQuery(doctorId, from,to)` |
| `GetPatientAppointmentsQuery(patientId)` |
| `GetDailyQueueQuery(date, doctorId)` |

## 6. Events
### Publishes
| Integration Event | Saat | Target |
|---|---|---|
| `AppointmentBookedEvent` | booking sukses | Notification confirm, Billing (perlu sebject invoice?), Doctor schedule |
| `AppointmentRescheduledEvent` | reschedule | Notification, Doctor |
| `AppointmentCancelledEvent` | cancel | Notification, Billing, libera slot |
| `AppointmentCheckedInEvent` | check-in | Queue monitor, Notification |
| `AppointmentCompletedEvent` | selesai | MedicalRecord (link visit), Billing (create invoice) |
| `AppointmentNoShowEvent` | no-show | Billing (penalty), Notification |

### Subscribes
| Event | Aksi |
|---|---|
| `DoctorScheduleChangedEvent` | refresh availability / mark affected |
| `DoctorLeaveCreatedEvent` | hadapi appointment hari cuti (auto reschedule/cancel) |
| `PatientDeactivatedEvent` | cancel appointment future pasien |

## 7. API Endpoint preview (`/api/v1`)
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/appointments` | booking | Patient/FrontDesk/Admin |
| GET | `/appointments?date&status&doctorId&page` | list/filter | Login klinis |
| GET | `/appointments/{id}` | detail | owner/admin |
| PUT | `/appointments/{id}` | reschedule | Patient/FrontDesk/Admin |
| DELETE | `/appointments/{id}` | cancel (dgn reason/message) | Patient/FrontDesk/Admin |
| POST | `/appointments/{id}/check-in` | pasien datang | FrontDesk |
| POST | `/appointments/{id}/complete` | selesai | Doctor |
| GET | `/appointments/doctor/{doctorId}?date` | Agenda dokter | Doctor/Admin |
| GET | `/appointments/patient/{patientId}` | riwayat pasien | Owner/Admin |

## 8. Dependencies
- Out sync: Doctor (availability/schedule), Patient (info), Identity (user valid/sesi).
- In sync: dipanggil MedicalRecord, Lab, Billing utk membuat visit/order/invoice.
- Out async: events booking→billing/notification.
- In async: jadwal dr & deactivate pasien.

## 9. DB pointer
`04-Database-Design/04-appointment-service-db.md` (Appointments, status history, queue counters, outbox/inbox). PK → Guid; index booking (doctorId,date,status) unik-effort double booking & query.

## 10. Urutan implement (meniru Identity)
1. Scaffold Projects.
2. Entity+konfigurasi + enum + snapshot.
3. Migration + seeder (kontraksi state) — optional.
4. BookAppointment + GetDoctorAvailability (paling penting), dsb.
5. Controller `/api/v1/appointments` + state transitions.
6. Anti-double-booking (UPDLOCK/optimistic) — verifikasi jelas.
7. Outbox utk events (Booked/Cancelled/Completed).

## 11. Catatan keputusan saat implement
- Apakah slot utk booking disimpan/di-generate dari schedule dr tiap tanggal, atau dihitung on-the-fly?
- Antrean di-reset harian? Nomor antrean generator (per doctor+day).
- Pelacakan "history" transisi status via tambahan tabel atau event store ringkas?
- Integrasi earliest (MVP): cukup Book→Completed + event Completed utk MedicalRecord & Billing.
