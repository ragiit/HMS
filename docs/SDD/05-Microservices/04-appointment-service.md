# Appointment Service - Detail

## 1. Overview
Menangani **booking** dan **manajemen janji temu** antara pasien dan dokter.

## 2. Bounded Context
- Booking appointment (pasien/dokter/tanggal/jam)
- Manajemen appointment lifecycle (Scheduled → CheckedIn → InProgress → Completed / Cancelled / NoShow)
- Slot availability checking
- Queue number management
- Reschedule & cancellation

## 3. Domain Model
```
┌────────────────────────────┐
│ Appointment (AggregateRoot)│
│  - PatientId               │
│  - DoctorId                │
│  - Date, StartTime, End    │
│  - Status (state machine)  │
│  - QueueNumber             │
│  + Book()                  │
│  + CheckIn()               │
│  + Start()                 │
│  + Complete()              │
│  + Cancel(reason)          │
│  + MarkNoShow()            │
│  + Reschedule()            │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│ AppointmentType (Enum)     │
│ AppointmentStatus (Enum)   │
│ QueueNumber (ValueObject)  │
└────────────────────────────┘
```

### Appointment Status State Machine
```
Scheduled ───CheckIn──▶ CheckedIn ──Start──▶ InProgress ──Complete──▶ Completed
    │                      │                     │
    └──────Cancel─────────▶Cancelled             └──NoShow──▶ NoShow
    └──────Reschedule────────▶ (ke Scheduled baru)
```

## 4. CQRS

### Commands
| Command | Handler Responsibility |
|---|---|
| `BookAppointmentCommand` | Validate availability, create appointment, generate queue number |
| `RescheduleAppointmentCommand` | Change date/time |
| `CancelAppointmentCommand` | Cancel with reason |
| `CheckInCommand` | Patient arrives |
| `StartAppointmentCommand` | Doctor starts consult |
| `CompleteAppointmentCommand` | Doctor completes |
| `MarkNoShowCommand` | Patient not come |

### Queries
| Query | Handler Responsibility |
|---|---|
| `GetAppointmentsQuery` | Filter by date/status/doctor |
| `GetAppointmentByIdQuery` | Detail |
| `GetDoctorAvailabilityQuery` | Check slots available |
| `GetPatientAppointmentsQuery` | History by patient |
| `GetDailyQueueQuery` | Today's queue list |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `appointment.booked` | Booking sukses | Notification (confirmation), Billing (generate invoice) |
| `appointment.rescheduled` | Reschedule | Notification |
| `appointment.cancelled` | Cancel | Notification, Billing |
| `appointment.completed` | Selesai | MedicalRecord, Billing (create invoice) |
| `appointment.checked_in` | Check-in | Queue display, Notification |
| `appointment.no_show` | No show | Billing (penalty), Notification |

### Subscribes
| Event | Aksi |
|---|---|
| `doctor.schedule.changed` | Refresh doctor availability |
| `patient.deactivated` | Cancel future appointments |

## 6. Dependencies
**Outbound calls**:
- Doctor Service (get schedule - detail)
- Patient Service (get patient info)
- Identity (user validation)

**Inbound calls**: Billing, Medical Record, Lab service (map appointment to visit).

## 7. Design Decisions
- **Slot availability check**: query di AppointmentService dengan transactional lock (UPDLOCK) untuk mencegah double-booking
- **Queue number**: di-generate counter per dokter & hari
- **Notification** trigger saat booking sukses / cancel (event-driven)
- **Billing trigger**: saat appointment completed, kirim event ke Billing untuk generate invoice
