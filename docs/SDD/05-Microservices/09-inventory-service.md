# Inventory Service - Detail

## 1. Overview
Menangani **stok obat**, **alat medis**, **supplies**, **batch & kadaluwarsa**, dan **stock movements**.

## 2. Bounded Context
- Item catalog & kategori
- Stock batch & expiration management
- Stock in (receive) / outflow (dispense) / adjustments
- Low stock alert
- Unit cost & pricing
- Supplier management

## 3. Domain Model
```
┌────────────────────────────┐
│  InventoryItem (Aggregate) │
│  - Code, Name, Category    │
│  - CurrentStock            │
│  - ReorderLevel, MaxLevel  │
│  - UnitCost, Price         │
│  - Batches (list)          │
│  + ReceiveStock(batch)     │
│  + RemoveStock(qty)        │
│  + Adjust()                │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  StockBatch (Entity)       │
│  - BatchNumber, Expiry     │
│  - InitialQty, CurrentQty  │
│  StockMovement (Record)    │
│  Supplier (Reference)      │
└────────────────────────────┘
```

## 4. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreateItemCommand` | Add new inventory item |
| `UpdateItemCommand` | Update item info |
| `ReceiveStockCommand` | Receive goods in (create batch) |
| `DispenseStockCommand` | Reduce stock (from pharmacy) |
| `AdjustStockCommand` | Manual adjustment (with reason) |
| `CreateSupplierCommand` | Add supplier |
| `MarkExpiredCommand` | Batch kadaluwarsa |

### Queries
| Query | Handler |
|---|---|
| `GetItemsQuery` | List with filters |
| `GetItemByIdQuery` | Detail |
| `GetLowStockQuery` | Items below reorder level |
| `GetExpiringItemsQuery` | Expiration alert |
| `GetStockMovementsQuery` | Movement history |
| `GetItemBatchesQuery` | Batches by item |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `inventory.received` | Barang masuk | - |
| `inventory.stock.adjusted` | Manual adjust | - |
| `inventory.low_stock` | Stok dibawah reorder | Notification (inventory staff) |
| `inventory.expiring` | Mendekati expired | Notification |
| `inventory.out_of_stock` | Stok habis | Pharmacy (disable dispensing) |

### Subscribes
| Event | Aksi |
|---|---|
| `prescription.dispensed` | Reduce stock (items) |
| `prescription.cancelled` | Restock jika sudah reserved |
| `pharmacy.prescription_created` | Optionally reserve stock |

## 6. Dependencies
**Outbound calls**:
- Notification (low stock, expiring)
- Pharmacy (stock queries)

**Inbound calls**: Pharmacy, Warehouse.

## 7. Design Decisions
- **FIFO (First In First Out)** untuk batch dispensing
- **Optimistic concurrency** (RowVersion) pada item & batch untuk mencegah race
- Low stock & expiring alert dikirim via Notification service
- Movement selalu **append-only** (immutable log audit)
- Stock batch tracking penting untuk recall/kadaluwarsa
