# Patient Service - Detail

## 1. Overview
Menangani **registrasi** dan **manajemen data pasien** (demografis, kontak, alamat, asuransi).

## 2. Bounded Context
- Registrasi pasien baru
- Update data demografis pasien
- Contact & emergency contact management
- Insurance/BPJS information
- Pencarian pasien (by NIK, name, MR number)

## 3. Domain Model
```
┌────────────────────────────┐
│      Patient (AggregateRoot)│
│  - MedicalRecordNumber      │
│  - Demographics             │
│  - Addresses (list)         │
│  - Contacts (list)          │
│  - EmergencyContact        │
│  - Insurances (list)        │
│  + Register()               │
│  + UpdateDemographics()     │
│  + AddEmergencyContact()    │
│  + Deactivate()             │
└────────────────────────────┘
```

### Value Objects
| VO | Fields |
|---|---|
| `Address` | Street, City, Province, PostalCode, Country |
| `EmergencyContact` | Name, Relationship, Phone, Address |
| `InsuranceCoverage` | Provider, PolicyNumber, CoverageType, ValidFrom, ValidTo |

## 4. CQRS

### Commands
| Command | Handler Responsibility |
|---|---|
| `RegisterPatientCommand` | Create new patient with MR number |
| `UpdatePatientCommand` | Update patient demographics |
| `UpdateEmergencyContactCommand` | Update emergency contact |
| `AssignInsuranceCommand` | Assign/add insurance info |
| `DeactivatePatientCommand` | Soft-deactivate |
| `DeletePatientCommand` | Soft-delete |

### Queries
| Query | Handler Responsibility |
|---|---|
| `GetPatientByIdQuery` | Get by ID |
| `GetPatientByMRNumberQuery` | Get by medical record number |
| `SearchPatientsQuery` | Search by name/NIK/phone (full text) |
| `GetPatientHistoryQuery` | Get visit history (via event/aggregation) |
| `GetPatientAddressesQuery` | List addresses |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `patient.created` | Pasien baru ter-registrasi | Notification (welcome), Appt (possible) |
| `patient.updated` | Data demografis berubah | MedicalRecord, Billing |
| `patient.deactivated` | Pasien non-aktif | All services |

### Subscribes
| Event | Aksi |
|---|---|
| `appointment.completed` | Update visit count (opsional) |
| `user.password.set` | Link user account ke patient (opsional) |

## 6. Dependencies
**Inbound calls**: Called by Appointment, Medical Record, Billing, Pharmacy, Lab service (sync).
**Outbound calls**: **none** (source of truth pasien).

## 7. Design Decisions
- Generasi MR Number auto-increment di database: `MR-{year}-{sequence}`
- Data pasien adalah **source of truth**; service lain menyimpan hanya `PatientId` dan melakukan API call/DTO untuk detail
- Search menggunakan `ILIKE`/FTS pada kombinasi nama belakang + nama depan
- Tidak ada delete fisik; selalu soft-delete
