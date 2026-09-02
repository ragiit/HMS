# Medical Record Service - Detail

## 1. Overview
Menangani **Electronic Medical Records (EMR)** digital: rekam medis, tanda vital, diagnosis (ICD-10), dan rencana pengobatan.

## 2. Bounded Context
- Buat/read/update rekam medis per kunjungan
- Vital signs tracking
- Diagnosis (ICD-10 codes)
- Plan & summary
- Follow-up scheduling
- Confidentiality / access control pada rekam medis

## 3. Domain Model
```
┌────────────────────────────┐
│ MedicalRecord (Aggregate)  │
│  - PatientId               │
│  - DoctorId, AppointmentId │
│  - Subjective/Objective    │
│  - Assessment (diagnosis)  │
│  - Plan                    │
│  - VitalSigns              │
│  - Status (Draft/Final)    │
│  + Finalize(dc)            │
│  + AddDiagnosis()          │
│  + AddVitalSigns()         │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  Diagnosis (Entity)        │
│  - ICD10Code               │
│  - IsPrimary               │
│  VitalSign (ValueObject)   │
└────────────────────────────┘
```

## 4. Security / Access Control
| Role | Akses |
|---|---|
| Doctor | Full read/write pada rekam miliknya, read pada pasien yang pernah ditangani |
| Nurse | Write vital signs, read records pasien yang ditugaskan |
| Admin | Full read (audit) |
| Patient | Read own record (terbatas, tidak confidential) |
| Other | Denied |

## 5. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreateMedicalRecordCommand` | Create new record (draft) |
| `UpdateMedicalRecordCommand` | Update draft/record |
| `AddVitalSignsCommand` | Add vital signs |
| `AddDiagnosisCommand` | Add ICD-10 diagnosis |
| `FinalizeRecordCommand` | Finalize (kunci dari edit by same doctor? menggunakan workflow) |
| `AddTreatmentCommand` | Add procedure/action |
| `CreatePrescriptionOrderCommand` | Create prescription order (publikasi ke Pharmacy) |

### Queries
| Query | Handler |
|---|---|
| `GetPatientRecordsQuery` | All records by patient |
| `GetRecordByIdQuery` | Detail |
| `GetRecentVitalSignsQuery` | Latest vital signs |
| `GetDiagnosisHistoryQuery` | Diagnosis trends |

## 6. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `medical_record.created` | Rekam dibuat | - |
| `medical_record.finalized` | Rekam difinalisasi | Notification (patient can view), Pharmacy (if prescription) |
| `prescription.order_created` | Resep dibuat | Pharmacy (create prescription) |
| `lab.order_created` | Order lab dibuat | Laboratory (create lab order) |
| `vital_signs.updated` | Vital signs diubah | - |

### Subscribes
| Event | Aksi |
|---|---|
| `appointment.completed` | Create medical record for appointment |

## 7. Dependencies
**Outbound calls**:
- Patient Service (get info)
- Doctor Service (get doctor)
- Pharmacy (create prescription)
- Laboratory (create lab order)

**Inbound calls**: None major. (Lab hasil & pharmacy hasil feeding back via event)

## 8. Design Decisions
- Rekam medis sensitif; audit log lengkap
- Menggunakan **ICD-10** untuk diagnosis codes
- Data vital adalah separate tables untuk time-series
- Record **final** setelah doctor menandatangani; hanya bisa di-amend dengan catatan khusus
