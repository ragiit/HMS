# Inventory Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid` · `/api/v1` + append-only movements).

## 1. Overview & Peran
Mengelola penidi **stok farmasi & medis** (obat, alat medis, bahan habis pakai): katalog item, batch & tanggal kedaluwarsa, barang masuk (receiving), pengeluaran (dispensing), penyesuaian, **FIFO**, restocking, serta alert stok menipis/kadaluwarsa.

## 2. Fungsi utama
1. Item catalog & kategori (obat/alat/supply).
2. Stok batch (batch number, kedaluwarsa, harga unit).
3. **Barang masuk (receive)** — buat batch.
4. **Pengeluaran** stok (dari resep/pharmacy) FIFO & reduce current/fqty.
5. **Adjust manual** (dgn alasan; append-only log).
6. Peringatan stok rendah & barang mendekati kedaluwarsa.
7. Master supplier + harga (cost & cap dijual yang dipakai Drug catalog di Pharmacy).
8. Optimistic concurrency (RowVersion) & pengendalian batch legacy.

## 3. Bounded context
- Item & kategori
- Batch / expiration
- Receiving & dispatch trail
- Adjustment (audit)
- Supplier
- Low-stock & expiry alerting

## 4. Domain model (Guid)
```
InventoryItem (AggregateRoot<Guid>; IAuditable)
 ├─ Code, Name, Category(goods/pharmacy/devices)
 ├─ CurrentStockQty? (mirror QtyAvailable) — atau derived dr batches
 ├─ ReorderLevel, MaxLevel, Unit, UnitCost, DisplayName etc
 ├─ Batches : ICollection<StockBatch>
 ├─ Movements : ICollection<StockMovement> (append-only)
 ├─ ReceiveStock(supplier, batchNo, expiry, qty, cost)
 ├─ RemoveStockForDispense(qty)  -> FIFO deplete across batches
 ├─ Adjust(qty, reason, by)      -> crear movement
 └─ CalculateCurrentQty(), ThrowIfLockedLow()

StockBatch (Entity<Guid>): BatchNumber, ExpirationDate, InitialQty,
    RemainingQty, ReceivedDate, Status(Active/Expired/Depleted/Quarantine)

StockMovement (immutable record entity): MovementType(IN/OUT/ADJUST/RETURN),
   Quantity, BalanceAfter, ReferenceType/Id, Reason, OccurredAt, ByUser

Supplier (Entity/ref, master): Name, ContactPerson?, Phone, Notes/RegNo
```
> id obat dibuat inventory; `medicationName`/`inventoryItemId` yang dipakai Pharmacy langsung berdasarkan inventoryId (Guid). doc lama `int` — konversiGuid.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreateItemCommand(dto)` |
| `UpdateItemCommand(id, …)` |
| `ReceiveStockCommand(itemId, supplierId, batchDtos)` |
| `DispenseStock(items: qty per item)` (dipicu dari Pharmacy) |
| `AdjustStockCommand(itemId, qty, reason, by)` |
| `CreateSupplierCommand(dto)` |
| `MarkExpiredCommand(batchId)` / `Quarantine` |

### Queries
| Query |
|---|
| `GetItemsQuery(filters/page)` |
| `GetItemByIdQuery(id)` |
| `GetItemBatchesQuery(itemId)` |
| `GetLowStockQuery` |
| `GetExpiringItemsQuery(days)` |
| `GetStockMovementsQuery(itemId, from, to)` |
| `GetSuppliersQuery` |

## 6. Events
### Publishes
| Integration | Dipicu | Target |
|---|---|---|
| `InventoryReceivedEvent` | barang masuk | - |
| `StockAdjustedEvent` | adjust | - |
| `LowStockAlertEvent` | di bawah reorder | Notification |
| `ExpiringAlertEvent` | dekat expired | Notification |
| `OutOfStockEvent` | habis | Pharmacy (nonaktifk n dispensing) |
### Subscribes
| Event | Aksi |
|---|---|
| `PrescriptionDispensedEvent` | reduce stok (FIFO) |
| `PrescriptionCancelledEvent` | restock bila ada reserve |
| `PharmacyPrescriptionCreatedEvent` | (opsional) reserve stash |

## 7. API Endpoint preview (`/api/v1`)
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/inventory/items` | tambah item | Pharmacist/Admin |
| GET | `/inventory/items` | list+filter | Pharmacist/Admin/Doctor(lihat) |
| GET | `/inventory/items/{id}` | detail | " |
| PUT | `/inventory/items/{id}` | update | Pharmacist/Admin |
| GET | `/inventory/items/low-stock` | stok menipis | Pharmacist/Admin |
| GET | `/inventory/items/expiring` | mendekati expired | " |
| POST | `/inventory/items/{id}/adjust` | adjust manual | " |
| POST | `/inventory/receivings` | barang masuk+batch | Pharmacist/Admin |
| GET | `/inventory/movements` | history movements | " |
| GET/POST | `/inventory/suppliers` | master supplier | GET-all / POST admin |

### Contoh request (draft)
```json
// POST /api/v1/inventory/receivings
{ "supplierId":"55..g", "referenceNumber":"PO-2026-0001", "receivedDate":"2026-01-20",
  "items":[ { "itemId":"66..g", "receivedQuantity":500, "batchNumber":"AMX-2026-0145",
              "expirationDate":"2027-06-30", "unitCost":12500 } ] }
// -> batch dibuat, current stock bertambah, movement IN tersimpan
```

## 8. Dependencies
- Out async: alert/out-stock ke Notification & Pharmacy.
- In: Pharmacy (dispense/restock), Warehouse UI.
- Sync beberapa item info untuk UI.

## 9. DB pointer
`04-Database-Design/09-…` (Items, Batches, Movements, Suppliers, outbox/inbox). Guid PK.

## 10. Urutan implement ringkas
1. Scaffold; entitas (item/batch/movement)+migration; seeder beberapa kategori.
2. `ReceiveStock` (buat batch) + GetItems/GetItemBatches — inti.
3. FIFO `RemoveStockForDispense` + DispenseStock event caller.
4. Adjust (append-only) + movement query + low-stock/expiring query.
5. Alert worker & event out-of-stock.
6. Controller `/api/v1/inventory/*`.

## 11. Catatan keputusan saat implement
- Current stock dihitung dari batch (aggregate) atau kolom mirror — disarankan dapat kalkulasi dari batches saat item diaduk; mengunci race w/ RowVersion.
- FIFO vs expiry-FEFO (expiry first). Banyak farmasi pakai FEFO — konfirmasi.
- Reserve di staging utk pharmacy? atau dihitung terminal dispense. Tentukan konsistensi.
