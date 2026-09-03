# Laboratory Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid`/`/api/v1`).

## 1. Overview & Peran
Mengelola alur pemeriksaan laboratorium: **order test**, **koleksi sampel**, **input/verifikasi hasil**, **katalog pemeriksaan**, sampai notifikasi **nilai kritis** bila hasil di luar ambang.

## 2. Fungsi utama
1. Buat lab order multi-tes (oleh dokter/kejadian medical-record).
2. Tandai sampel diambil (koleksi) & QC.
3. Input hasil per parameter; tandai nilai kritis.
4. Verifikasi hasil oleh patolog (opsional) → rilis ke dokter/pasien.
5. Kelola katalog tes lab (code/name/metode/reference range).
6. Publikasi event (order→billing/notification; hasil siap/kritis→doctor).
7. Lacak turnaround time order.

## 3. Bounded context
- Lab Order (multi test)
- Sample collection & identitas sampel
- Hasil & verifikasi (parametrized)
- Nilai critical / aler
- Test catalog

## 4. Domain model (Guid)
```
LabOrder (AggregateRoot<Guid>; IAuditable)
 ├─ OrderNumber, PatientId(Guid), DoctorId(Guid), VisitId(AppointmentId?) ref
 ├─ OrderedAt, Priority(Routine/Urgent/Stat)
 ├─ Status(Ordered/Collected/InProgress/Completed/Cancelled)
 ├─ SampleCollectedBy/At, Cancelled reason/At, CompletedBy/At (audit)
 ├─ Tests : ICollection<LabOrderTest>
 ├─ Create(tests), CollectSample(by), Cancel(reason), StartTest, CompleteTest
 └─ GenerateNumber: LO-yyyy-xxx

LabOrderTest (child): TestCatalogId, TestCode/Name(snapshot), Status, SpecimenType,
    Results : ICollection<TestResult>, IsCritical

TestResult (parameter): ParameterName, Value, Unit, ReferenceRange, Flag(Normal/High/Low/Critical),
    VerifiedBy/At, Input timestamp

TestCatalog (Entity<Guid>, master): Code, Name, Category, SpecimenType, Method,
   ReferenceRange, IsActive
```
> catat: patient/doctor/appointment/billing id external Guid.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreateLabOrderCommand(patientId,doctorId,tests,…)` |
| `CancelLabOrderCommand(id, reason)` |
| `CollectSampleCommand(id, by)` |
| `InputResultsCommand(orderTestId, resultsParameter list)` |
| `VerifyResultsCommand(orderTestId, by)` |
| `FlagCriticalCommand(orderTestId)` (saat di luar range) |
| `AddTestCatalogCommand(dto)` |

### Queries
| Query |
|---|
| `GetLabOrdersQuery(status/date/patient/page)` |
| `GetLabOrderByIdQuery(id)` |
| `GetTestCatalogQuery(spec/cat)` |
| `GetPatientResultsQuery(patientId)` |

## 6. Events
### Publishes
| Integration | Saat | Target |
|---|---|---|
| `LabOrderCreatedEvent` | order dibuat | Billing (invoice), Notification |
| `LabResultReadyEvent` | hasil siap/belum | MedicalRecord (attach), Doctor, Notification |
| `LabCriticalResultEvent` | nilai kritis | Notification (urgent) |
| `LabOrderCancelledEvent` | dibatalkan | Billing |

### Subscribes
| Event | Aksi |
|---|---|
| `MedicalRecordFinalizedEvent` / `MedicalRecordLabOrderRequested` | buat order lab |
| `AppointmentCompletedEvent` | bila dibutuhkan |

## 7. API Endpoint preview (`/api/v1`)
### Orders
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/laboratory/orders` | buat order | Doctor |
| GET | `/laboratory/orders?status&patientId` | list | LabStaff/Doctor |
| GET | `/laboratory/orders/{id}` | detail+hasil | LabStaff/Doctor/Admin |
| POST | `/laboratory/orders/{id}/collect` | sampel diambil | LabStaff/Nurse |
| POST | `/laboratory/orders/{id}/cancel` | batal | Doctor/LabStaff |
### Results
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/laboratory/orders/{orderTestId}/results` | input hasil | LabStaff |
| POST | `/laboratory/orders/{orderTestId}/results/verify` | verifikasi | Pathologist |
| GET | `/laboratory/orders/{id}/results` | lihat hasil | Doctor/LabStaff/Admin |
| GET | `/laboratory/patients/{patientId}/results` | semua hasil pasien | Doctor/LabStaff |
### Catalog
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| GET | `/laboratory/tests` | katalog | Doctor/LabStaff/Admin |
| POST | `/laboratory/tests` | tambah/pantau katalog | Admin |

### Contoh Request (draft, Guid)
```json
// POST /api/v1/laboratory/orders
{
  "patientId": "11111111-...",
  "doctorId": "2222....",
  "visitId": "3333....",            // appointment/visit
  "priority": "Routine",
  "tests": [ { "catalogId":"4444...", "specimenType":"Blood" } ]
}
// Response 201 -> { "orderNumber":"LO-2026-0001","id":"..g","status":"Ordered" }
```

## 8. Dependencies
- Out sync: Patient/Doctor/Appointment (info bila tampilan), Billing bila invoice.
- Out async: events order/hasil/kritis.
- In: dipanggil MedicalRecord (sumber order), Lab UI.

## 9. DB pointer
`04-Database-Design/07-…`: `LabOrders`, `LabOrderTests`, `TestResults`, `TestCatalog`, outbox/inbox. Ubah ke Guid; index per order/patient & status.

## 10. Urutan implement ringkas
1. Scaffold; entity+cat; migrasi; seed test catalog.
2. Buat order + `GetLabOrders/getOrderById` (baca hasil) — paling vital.
3. Collect & input hasil (parameterisasi + flag normal vs critical) .
4. Verify (pathologist) opsional MVP.
5. Event hasil-kritis & order bila dipakai oleh billing di tahap itu.
6. Controller `/api/v1/laboratory/*` + wrap.

## 11. Catatan keputusan saat implement
- Verifikasi required atau opsional? (dok: opsional untuk pathologist)
- Nilai kritis dideteksi sisi mana (di service saat input / oleh rule engine)? dikarenakan flag.
- Siapkan integrasi hasil kembali ke MedicalRecord (attach hasil milestone).
- UI melihat hasil via lab service vs event ke medrecord — keputusan caching.
