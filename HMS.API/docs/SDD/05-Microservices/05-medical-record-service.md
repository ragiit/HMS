# Medical Record Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid` · `/api/v1`). Data SIM/Dok.

## 1. Overview & Peran
Menyimpan **Electronic Medical Record (EMR)** per kunjungan: SOAP (Subjektif-Objektif-Assessment-Plan), tanda vital, diagnosis ICD-10, tindakan, dan perintah (resep/order lab). Data paling sensitif di rumah sakit ⇒ kontrol akses & audit yang ketat.

## 2. Tanggung jawab / fungsi utama
1. Membuat/update draf rekam per kunjungan.
2. Input tanda vital (time-series).
3. Diagnosis (ICD-10, primer/sekunder) & plan.
4. Tindakan/prosedur (treatment) + list order lab.
5. **Finalisasi** rekam (kunci; amend tercatat).
6. Enforce **access policy** per role & audit log.
7. Kirim order ke Pharmacy/Lab via event (untuk konsistensi lintas service).

## 3. Bounded context
- Rekam medis (SOAP & metadata)
- Vital signs (riwayat)
- Diagnosis (ICD-10) & ICD-10 master (seed/import)
- Rencana pengobatan/tindakan/follow-up
- Audit & kontrol akses (privacy)

## 4. Domain model (Guid)
```
MedicalRecord (AggregateRoot<Guid>; IAuditable)
 ├─ PatientId (Guid ref patient), DoctorId (Guid), AppointmentId?/VisitId
 ├─ RecordNumber (uniek), VisitDate, VisitType, Department
 ├─ SOAP: Subjective, Objective, Assessment, Plan, Summary
 ├─ Status (Draft/Finalized/Closed), IsConfidential
 ├─ FinalizedBy/At, FollowUpNeeded/Date
 ├─ VitalSigns : ICollection<VitalSign>   (BE)
 ├─ Diagnoses   : ICollection<Diagnosis>  (ICD10)
 ├─ Treatments  : ICollection<Treatment>
 ├─ PrescriptionOrders : ICollection<PrescriptionOrder> (forward pharmacy)
 ├─ LabOrdersRef : ICollection<LabOrderRef> (order lab)
 ├─ StartDraft(), Finalize(by), AddDiagnosis, AddVitalSign, AddTreatment
 ├─ AddPrescriptionOrder(), AddLabOrderRef(), Amend(note)

VitalSign  (time-series row): temp, BP sistol/diast, HR, RR, SpO2, weight, height, BMI, glucose, pain 0-10
Diagnosis : ICD10Code, Name, IsPrimary, Type(Working/Final/Rule-Out)
Treatment : code,name,desc,by,date,result,billingRef
ICD10Code : Code, Name (master; ter-seed)
```
> ⚠️ Dok db lama memakai `Bigint/Int`; konversi **Guid** utk consistency HMS. ID ekternal pasien/dokter Guid.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreateRecordCommand(patientId, doctorId, visit…)` → draft |
| `UpdateDraftCommand(id, soap…)` (hanya saat Draft) |
| `AddVitalSignsCommand(recordId, dto)` |
| `AddDiagnosisCommand(recordId, icd, name, isPrimary)` |
| `AddTreatmentCommand(recordId, dto)` |
| `FinalizeRecordCommand(recordId, by)` |
| `CreatePrescriptionOrderCommand(recordId, items)` → untuk Pharmacy |
| `CreateLabOrderCommand(recordId, tests)` → untuk Lab |
| `AmendRecordCommand(id, note)` (pasca-final, dgn jejak) |

### Queries
| Query |
|---|
| `GetPatientRecordsQuery(patientId, page)` |
| `GetRecordByIdQuery(id)` |
| `GetRecentVitalSignsQuery(patientId)` |
| `GetDiagnosisHistoryQuery(patientId)` |
| `GetPendingForFinalizeQuery` (opsional) |

## 6. Events
### Publishes
| Integration Event | Saat | Target |
|---|---|---|
| `MedicalRecordCreatedEvent` | draft dibuat | -/Notification |
| `MedicalRecordFinalizedEvent` | final | Notification (pasien), Pharmacy (jika resep), Lab |
| `PrescriptionOrderRequestedEvent` | resep dibuat | Pharmacy |
| `LabOrderRequestedEvent` | order lab | Laboratory |
| `VitalSignsUpdatedEvent` | vital baru | (opsional) |

### Subscribes
| Event | Aksi |
|---|---|
| `AppointmentCompletedEvent` | siapkan/buka draft rekam utk kunjungan itu |
| (results feedback dari Lab/Pharmacy bila perlu) | update status |

## 7. Access Control (role) — blueprint
| Role | Akses |
|---|---|
| Doctor | tulis&baca rekam ybs; baca pasien yang pernah ditangani |
| Nurse | tulis vital; baca rekam pasien bertugas |
| Patient (login) | baca sebagian (non-confidential) |
| Admin | read/audit |
| Lainnya | deny |

> Implementasikan lewat policy/claim + middleware di tiap controller & service method (sebaiknya data horizontaly-gated bila pasien/doctor id di klaim).

## 8. API Endpoint preview (`/api/v1`)
| Method | Path | Deskripsi |
|---|---|---|
| POST | `/medical-records` | buat draf |
| GET | `/medical-records/patient/{patientId}` | riwayat pasien |
| GET | `/medical-records/{id}` | detail |
| PUT | `/medical-records/{id}` | update draf |
| POST | `/medical-records/{id}/finalize` | finalisasi |
| POST | `/medical-records/{id}/vital-signs` | tambah vital |
| GET | `/medical-records/{patientId}/vital-signs/latest` | vital terakhir |
| POST | `/medical-records/{id}/diagnoses` | tambah diagnosis |
| GET | `/medical-records/{id}/diagnoses` | list diagnosis |
| POST | `/medical-records/{id}/treatments` | tambah tindakan |
| POST | `/medical-records/{id}/prescriptions` | buat order resep |
| POST | `/medical-records/{id}/lab-orders` | buat order lab |

## 9. Dependencies
- Out sync: Patient (info), Doctor (info), Pharmacy/Lab (order dibuat lewat event) — API bila diperlukan.
- In sync: dipanggil MedicalRecord oleh Lab/Pharmacy bila feed-back.
- Out async: finalisasi & order ke Rabbit.
- In async: appointment.completed memicu draf rekam.

## 10. DB pointer
`04-Database-Design/05-…` tabel `MedicalRecords, VitalSigns, Diagnoses, Treatments, PrescriptionOrders, ICD10(master), Outbox/Inbox`. Ubah ke Guid saat imp. Unique RecordNumber; index pasien/date & doctor/date; time-series vital.

## 11. Urutan implement
1. Scaffold.
2. Entity + config + migration (draft/final alur).
3. ICD-10 seed (atau import via migration).
4. Create draft + GetPatientRecords/GetRecordById.
5. Vital + diagnoses + treatments (sering dipakai dokter saat kunjungan).
6. Finalize & amend (untuk kunci).
7. Event order resep/lab via outbox.
8. Controller + access control role.

## 12. Catatan keputusan nnti implement
- Data SOAP: JSONB dense vs normalisasi? (dok normalisasi banyak tabel; mungkin campuran).
- Amend pasca-final: revisi vs lampiran append-only (tersedia security menyetujui).
- Who may create: bila appointment selesai otomatis draf dibuat operator? dan siapa final.
- Integrasi prescripsi/lab order/order di sini vs di service masing-masing yang mem-publish global order dgn referensi.
