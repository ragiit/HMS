# Pharmacy Service - Detail

## 1. Overview
Menangani **resep** dan **dispensing** obat, serta berintegrasi dengan Inventory untuk stok.

## 2. Bounded Context
- Create/read prescription
- Prescription dispensing (multiple items)
- Drug formulary lookup
- Status tracking prescription
- Communication dengan Inventory (dari event `prescription.filled`)

## 3. Domain Model
```
┌────────────────────────────┐
│ Prescription (Aggregate)   │
│  - PatientId, DoctorId      │
│  - Items (list)             │
│  - Status (Pending→Dispensed)│
│  + AddItem()                │
│  + Dispense()               │
│  + Cancel()                 │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  PrescriptionItem (Entity) │
│  - InventoryItemId         │
│  - Medication, Strength    │
│  - Dosage, Quantity        │
│  - IsDispensed             │
└────────────────────────────┘
```

## 4. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreatePrescriptionCommand` | Create prescription (from doctor / via event) |
| `UpdatePrescriptionCommand` | Update draft items |
| `DispensePrescriptionCommand` | Full dispensing |
| `PartialDispenseCommand` | Partial dispensing (stok kurang) |
| `CancelPrescriptionCommand` | Cancel prescription |
| `AddDrugFormularyCommand` | Add formulary item |

### Queries
| Query | Handler |
|---|---|
| `GetPrescriptionsQuery` | List by patient/status/date |
| `GetPrescriptionByIdQuery` | Detail + items |
| `GetPendingDispenseQuery` | Antrian dispensing |
| `SearchDrugFormularyQuery` | Lookup drug |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `prescription.created` | Resep dibuat | Notification (pasien), Inventory (reserve stok) |
| `prescription.dispensed` | Resep didispense | Inventory (reduce stok), Billing (generate invoice), Notification |
| `prescription.cancelled` | Resep dibatalkan | Inventory (release reserve) |

### Subscribes
| Event | Aksi |
|---|---|
| `medical_record.finalized` | Jika ada plan prescription, create prescription |
| `inventory.stock.updated` | Update drug availability status |

## 6. Dependencies
**Outbound calls**:
- Inventory Service (check stock, reserve, reduce)
- Billing Service (create invoice)

**Inbound calls**: 
- Medical Record (via event)
- Direct API from doctor UI

## 7. Design Decisions
- Dispensing melibatkan check ke Inventory Service + **reduce stock**
- Interaksi antar service (Pharmacy↔Inventory) menggunakan **event-driven** (async) untuk stock movement
- Jika stok tidak cukup, gunakan `PartialDispense`
- Drug formulary disinkronkan dari Inventory Service
