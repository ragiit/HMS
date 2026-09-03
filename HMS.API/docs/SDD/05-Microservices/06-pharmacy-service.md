# Pharmacy Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid` · `/api/v1`).

## 1. Overview & Peran
Menangani **resep** & **dispensing obat** serta **formularium obat** (master drug). Ber-integrasi erat dengan **Inventory Service** untuk cek/reserve/reduce stok obat, dan **Billing** saat obat didispense menghasilkan penagihan.

## 2. Tanggung jawab / fungsi utama
1. Buat/ubah/batalkan resep (dokter / dari event medical record atau UI).
2. Item resep (obat, aturan pakai, dosis, jumlah).
3. **Dispensing**: penuh / halve (partial) bila stok kurang.
4. Pelacakan status (Pending→Dispensed/Cancelled/Partially).
5. Formularium obat & lookup (sinkron dari Inventory / master sendiri).
6. Kirim event dispense→inventory(reserve/reduce) & billing.

## 3. Bounded context
- Prescription (resep) & item
- Dispensing (penuh/partial)
- Formularium obat (katalog internal, harga, generik)
- Tracking status resep
- Komunikasi dengan Inventory & Billing (async utk stock movement & invoice)

## 4. Domain model (Guid)
```
Prescription (AggregateRoot<Guid>; IAuditable)
 ├─ PrescriptionNumber, PatientId(Guid), DoctorId(Guid), MedicalRecordId/AppointmentId ref
 ├─ PrescriptionDate, Status(Pending/Dispensed/Cancelled/Partially)
 ├─ Priority, Notes, DispensingInstructions
 ├─ DispensedBy/At, CancelledBy/At/Reason (audit2)
 ├─ Items : ICollection<PrescriptionItem>
 ├─ Dispensing records
 ├─ Create(List<item>), AddItem, UpdateItem, Cancel(reason),
 ├─ Dispense(items+Dispenser, partial?: bool) -> memvalidasi stock
 └─ GenerateNumber RCP-yyyy-xxx

PrescriptionItem (child): MedicationName/GenericName/Strength/DosageForm/
   Dosage/Frequency/Route/DurationDays/Quantity/Unit/Instructions,
   IsDispensed, DispensedQuantity, SubstituteAllowed, LineNumber,
   InventoryItemId (Guid ref inventory)

Dispensing (child): DispensingNumber, DispensedBy(UserId Guid Identity),
   DispensedDate, Status(Completed/Partial), TotalQuantity, Notes

DrugFormulary (Entity<Guid>, master): Code,Name,GenericName,Category,Strength,
   DosageForm,UnitPrice,IsGenericAvailable,RequiresPrescription,StockAlertLevel
```
> ⚠️ `prescriptions/medical_record & inventory` meref Guid external, bukan int (db lama `BIGINT/INT` → diubah jadi Guid konsisten). `MedicalRecordId` external.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreatePrescriptionCommand(patientId, doctorId, items…)` |
| `UpdatePrescriptionItemsCommand(id, items)` (draf) |
| `DispensePrescriptionCommand(id, dispensedItemsIds, pharmacist)` |
| `PartialDispenseCommand(id, quantities)` |
| `CancelPrescriptionCommand(id, reason)` |
| `AddDrugFormularyCommand(dto)` |

### Queries
| Query |
|---|
| `GetPrescriptionsQuery(patient/status/date/page)` |
| `GetPrescriptionByIdQuery(id, include items)` |
| `GetPendingDispenseQueueQuery` |
| `SearchDrugFormularyQuery(term)` |

## 6. Events
### Publishes
| Integration | Saat | Target |
|---|---|---|
| `PrescriptionCreatedEvent` | resep dibuat | Notification, Inventory (reserve) |
| `PrescriptionDispensedEvent` | dispense sukses | Inventory (reduce), Billing, Notification |
| `PrescriptionCancelledEvent` | cancel | Inventory (release reserve) |

### Subscribes
| Event | Aksi |
|---|---|
| `MedicalRecordFinalizedEvent` (jika ada order resep) | buat resep dari plan |
| `InventoryStockUpdatedEvent` | update availability formularium (opsional) |

## 7. API Endpoint preview (`/api/v1`)
### Prescription
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/pharmacy/prescriptions` | buat resep | Doctor |
| GET | `/pharmacy/prescriptions/{id}` | detail+items | Doctor/Pharmacist |
| GET | `/pharmacy/prescriptions/patient/{patientId}` | riwayat pasien | Doctor/Pharmacist |
| PUT | `/pharmacy/prescriptions/{id}` | ubah (draf/diagnosis) | Doctor |
| POST | `/pharmacy/prescriptions/{id}/cancel` | batal | Doctor |
### Dispensing
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/pharmacy/prescriptions/{id}/dispense` | dispense penuh/partial | Pharmacist |
| GET | `/pharmacy/dispensings?status` | antrian dispensing | Pharmacist |

### Contoh request/response (draft) — lihat note pada API-design §7 (versi lama) untuk bentuk resep item (draft lama menampilkan contoh JSON). Ubah id jadi Guid saat implement.

## 8. Dependencies
- Out sync: Inventory (cek stok — bila sync API opsional), Billing (invoice via event).
- In: dipanggil doctor UI/medical record event.
- Out async: dispense & cancel ke Inventory/Billing.

## 9. DB pointer
`04-Database-Design/06-…`: `Prescriptions, PrescriptionItems, Dispensing, DrugFormularies, Outbox/Inbox`. Penyimpanan Guid PK; item col `InventoryItemId` Guid; const `PrescriptionNumber` unique.

## 10. Urutan implementasi (meniru Identity) — ringkas
1. Scaffold; entitas+kat; migration; seed beberapa formularium.
2. Buat resep + list/get resep (lookup drug → antrian dispensing milik yang paling esensial).
3. Tolak layanan: `Dispense` (sinkronisasi stock opsional tahap 1; simpul akhir publish event).
4. Event Pharmacy→Inventory/Billing (outbox).
5. Controller `/api/v1/pharmacy/prescriptions` + `/dispensings` — wrap ApiResponse.
6. Partial dispense & cancel path (release reserve bila ada).

## 11. Catatan keputusan saat implement
- Apakah cek dan reduce stok dilakukan **langsung (sync)** ke Inventory atau event async? (dok menyarankan async; tapi sync bisa menjamin lebih ketat pada tahap sederhana — putuskan dengan tradeoff konsistensi).
- DrugFormulary pemilik harga & katalog — koordinasi harga bila Inventory juga mencatat harga pembelian (harga jual ada di sini? pisahkan jelas).
- Partial dispense berapa banyak step dan pencatatan.
- Form: apakah resep dibuat secara langsung oleh MedicalRecord via event doctor save, atau manual dari pharmacist UI?
