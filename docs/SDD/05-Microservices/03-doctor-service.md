# Doctor Service - Detail

## 1. Overview
Menangani **profil dokter**, **spesialisasi**, dan **jadwal praktek**.

## 2. Bounded Context
- Profil dokter (demos, SIP, STR, lisensi)
- Data spesialisasi & subspecialty
- Jadwal praktek reguler per minggu
- Schedule exception (cuti, off, penggantian)
- Pencarian dokter by nama/spesialisasi

## 3. Domain Model
```
┌────────────────────────────┐
│    Doctor (AggregateRoot)  │
│  - EmployeeNumber          │
│  - LicenseNumber (STR)     │
│  - SIPNumber               │
│  - Specializations (list)  │
│  - Biografi                │
│  + SetSIP()                │
│  + Deactivate()            │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  DoctorSchedule (Entity)   │
│  - DayOfWeek               │
│  - StartTime, EndTime      │
│  - SlotDuration            │
│  - RoomNumber              │
│  + GenerateSlots()         │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  ScheduleException (Entity)│
│  - ExceptionDate           │
│  - IsCancelled             │
└────────────────────────────┘
```

## 4. CQRS

### Commands
| Command | Handler Responsibility |
|---|---|
| `CreateDoctorCommand` | Register dokter baru |
| `UpdateDoctorCommand` | Update profil |
| `SetScheduleCommand` | Tambah jadwal praktek |
| `UpdateScheduleCommand` | Update jadwal |
| `RemoveScheduleCommand` | Hapus jadwal |
| `CreateScheduleExceptionCommand` | Leave/cuti doctor |
| `SetSpecializationCommand` | Assign spesialisasi |

### Queries
| Query | Handler Responsibility |
|---|---|
| `GetDoctorsQuery` | Paginated list |
| `GetDoctorByIdQuery` | Set profile |
| `GetDoctorSchedulesQuery` | Weekly schedule |
| `GetAvailableDoctorsQuery` | Available doctors by date-time |
| `SearchDoctorsBySpecializationQuery` | Filter by spec |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `doctor.created` | Dokter baru | Notification |
| `doctor.updated` | Profil diubah | - |
| `doctor.schedule.changed` | Jadwal berubah | Appointment (cek conflict) |
| `doctor.leave.added` | Cuti/off | Appointment (notify affected) |

### Subscribes
| Event | Aksi |
|---|---|
| **none** | - |

## 6. Dependencies
**Outbound calls**: Identity (get user info), Appointment (update/manage schedule).
**Inbound calls**: Called by Appointment, Medical Record, Pharmacy service.

## 7. Design Decisions
- Doctor schedule adalah **master schedule**; Appointment service yang melakukan slot-booking
- SIP expiry check: sistem meng-cek nilai `SIPExpiryDate` secara berkala dan menonaktifkan jadwal bila expired
- DoctorService **publish** `doctor.schedule.changed` agar Appointment service bisa query ulang
