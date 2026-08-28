# Laboratory Service - Detail

## 1. Overview
Menangani **order pemeriksaan**, **sample collection**, **input hasil**, dan **catalog pemeriksaan lab**.

## 2. Bounded Context
- Create lab order (multi-test)
- Sample collection & QC
- Input/approve lab results
- Critical value alert
- Lab test catalog management

## 3. Domain Model
```
┌────────────────────────────┐
│ LabOrder (AggregateRoot)   │
│  - PatientId, DoctorId      │
│  - Tests (list)             │
│  - Status lifecycle         │
│  - SampleCollected          │
│  + Create()                 │
│  + CollectSample()          │
│  + Cancel()                 │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  LabOrderTest (Entity)     │
│  - TestCode, CatalogId     │
│  - Status                  │
│  - Results (list)          │
│  - IsCritical              │
└────────────────────────────┘
```

### Lab Order Status Lifecycle
```
Ordered ──Collect──▶ Collected ──Start──▶ InProgress ──Complete──▶ Completed
   │                                        │
   └────────Cancel──────────────────────────▶ Cancelled
```

## 4. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreateLabOrderCommand` | Create lab order |
| `CancelLabOrderCommand` | Cancel before sample |
| `CollectSampleCommand` | Mark sample collected |
| `InputResultsCommand` | Input results for any test |
| `VerifyResultCommand` | Verify by pathologist |
| `MarkCriticalResultCommand` | Handle critical value |
| `AddTestCatalogCommand` | Add new test catalog |

### Queries
| Query | Handler |
|---|---|
| `GetLabOrdersQuery` | List by status/date/patient |
| `GetLabOrderByIdQuery` | Detail with results |
| `GetTestCatalogQuery` | Master catalog |
| `GetPatientResultsQuery` | All results by patient |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `lab.order_created` | Order dibuat | Billing (create invoice), Notification |
| `lab.result_ready` | Hasil siap | MedicalRecord (attach), Notification (patient informed), Doctor (alert) |
| `lab.result.critical` | Hasil kritis | Notification (urgent) |
| `lab.order.cancelled` | Order dibatalkan | Billing |

### Subscribes
| Event | Aksi |
|---|---|
| `medical_record.lab_order_created` | Create lab order |
| `appointment.completed` | Trigger if lab needed |

## 6. Dependencies
**Outbound calls**:
- Patient, Doctor, Appointment (info)
- Billing (invoice)

**Inbound calls**: None major

## 7. Design Decisions
- **Critical value**: jika hasil di luar range critical, publish `lab.result.critical` segera
- **QC sample**: sample dikoleksi dulu, lalu bisa in-progress → completed
- Turnaround time tracking dibangun dalam status lifecycle
- Results disimpan per parameter; **verification** opsional (pathologist)
