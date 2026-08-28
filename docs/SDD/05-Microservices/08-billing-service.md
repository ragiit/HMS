# Billing Service - Detail

## 1. Overview
Menangani **invoice**, **pembayaran**, **refund**, dan **master data tarif**.

## 2. Bounded Context
- Generate invoice (dari appointment, lab, pharmacy, treatment)
- Line items management
- Payment processing (cash/transfer/card/BPJS)
- Partial payment
- Refund
- Overdue tracking
- Service catalog (tarif)

## 3. Domain Model
```
┌────────────────────────────┐
│ Invoice (AggregateRoot)    │
│  - PatientId               │
│  - Status (Draft→Paid...)  │
│  - LineItems (list)        │
│  - Subtotal, Tax, Total    │
│  - Payments (list)         │
│  + Issue()                 │
│  + AddItem()               │
│  + RecordPayment()         │
│  + Refund()                │
│  + Cancel()                │
└─────────────┬──────────────┘
              │
┌─────────────┴──────────────┐
│  InvoiceItem (Entity)      │
│  - ServiceCode, Quantity   │
│  - UnitPrice, Tax          │
│  Payment (Entity)          │
└────────────────────────────┘
```

### Invoice Status Lifecycle
```
Draft ─Issue──▶ Issued ──Payment──▶ Paid
   │               │
   └──Cancel────▶Cancelled   ──Partial──▶ PartiallyPaid
```

## 4. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreateInvoiceCommand` | Create invoice from appointment/lab |
| `AddInvoiceItemCommand` | Add line item |
| `IssueInvoiceCommand` | Change to Issued |
| `RecordPaymentCommand` | Record payment |
| `RefundPaymentCommand` | Process refund |
| `CancelInvoiceCommand` | Cancel invoice |
| `AddServiceCatalogCommand` | Add tarif |
| `ApplyInsuranceCommand` | Adjust for BPJS |

### Queries
| Query | Handler |
|---|---|
| `GetInvoicesQuery` | List by patient/status/date |
| `GetInvoiceByIdQuery` | Detail |
| `GetPatientBalanceQuery` | Outstanding balance |
| `GetServiceCatalogQuery` | Master tarif |
| `GetOverdueInvoicesQuery` | Overdue list |

## 5. Events

### Publishes
| Event | Ketika | Konsumen |
|---|---|---|
| `invoice.issued` | Invoice diterbitkan | Notification (patient) |
| `payment.received` | Pembayaran diterima | Notification, Accounting |
| `payment.overdue` | Melewati due date | Notification (SMS reminder) |
| `invoice.cancelled` | Invoice dibatalkan | - |
| `refund.processed` | Refund | - |

### Subscribes
| Event | Aksi |
|---|---|
| `appointment.completed` | Create invoice for consultation |
| `lab.order_created` | Create invoice for lab services |
| `prescription.dispensed` | Create invoice for pharmacy items |
| `medical_record.treatment_added` | Create invoice for procedures |

## 6. Dependencies
**Outbound calls**:
- Patient, Appointment (info)
- Notification (event)

**Inbound calls**: Pharmacy, Lab, Medical Record services (via events).

## 7. Design Decisions
- Billing mengumpulkan **multi-source** line items: consultation, lab, obat, tindakan
- **Partial payment** didukung
- **Overdue** detection via background worker (cron job)
- Tax per line item + subtotal
- BPJS coverage logic di tarif level
