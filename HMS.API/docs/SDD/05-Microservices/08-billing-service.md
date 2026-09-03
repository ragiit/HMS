# Billing Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · `Guid` · `/api/v1`).

## 1. Overview & Peran
Mengelola seluruh **penagihan & penerimaan bayaran** pasien: invoice (satu per kunjungan dengan banyak baris dari konsultasi/lab/obat/tindakan), pembayaran (penuh/parsial), refund, serta **master tarif jasa**. Billing seperti agregator beban keuangan ketika event dari unit klinis terjadi.

## 2. Fungsi utama
1. Buat/issue invoice (multi-line dari berbagai sumber: appointment completed, lab order, dispensing, treatment).
2. Baris item (subtotal, diskon, pajak item).
3. Pembayaran (cash/transfer/card/BPJS/QR), parsial.
4. Refund bila perlu.
5. Deteksi **overdue** & sisa tagihan pasien.
6. Master tarif / service catalog.
7. Opsional handling klaim BPJS/insurance (nomor klaim).

## 3. Bounded context
- Invoice & lifecycle
- Line item aggregation multi-source
- Payment & refund
- Service catalog (tarif, pajak, zona BPJS)
- Overdue & sisa saldo

## 4. Domain model (Guid)
```
Invoice (AggregateRoot<Guid>; IAuditable)
 ├─ InvoiceNumber, PatientId(Guid), MedicalRecord/Appointment ref (Guid opsional)
 ├─ InvoiceType(Konsultasi/Lab/Obat/Tindakan/…), Status
 ├─ InvoiceDate, DueDate, Currency
 ├─ Subtotal, Discount*, Tax*, TotalAmount, AmountPaid, AmountDue
 ├─ InsuranceClaimId?, Description
 ├─ IssuedBy, CancelledBy reason + IsVoided/VoidReason
 ├─ Items : ICollection<InvoiceItem>
 ├─ Payments: ICollection<Payment>
 ├─ Create(patient), AddItem(service…), RecomputeTotals(),
 ├─ Issue(by), RecordPayment(amt,meth,by)→ status paid/partial,
 ├─ Refund(amount,reason), Cancel(reason), MarkOverdue()
 └─ GenerateNumber INV-yyyymmdd-xxxx

InvoiceItem (child): ServiceCode/Name, Description, Quantity, UnitPrice,
   Discount%, TaxAmount, LineTotal, SourceType(FromService/Manual), SourceReferenceId

Payment (child): PaymentNumber, PaymentDate, Amount, PaymentMethod,
   ReferenceNumber, PaymentGateway, Status(Completed/Pending/Failed/Refunded), ReceivedBy

ServiceCatalog (Entity, master): ServiceCode/Name/Category/Description,
   UnitPrice, TaxRate, IsActive, IsCoveredByBPJS
```
> patient/medicalRecord/appointment external Guid. Id int pada doc lama → Guid saat implement.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreateInvoiceCommand(patientId, type, refs…)` |
| `AddInvoiceItemCommand(invoiceId, serviceDtos)` |
| `IssueInvoiceCommand(id)` |
| `RecordPaymentCommand(invoiceId, amount, method, by)` |
| `RefundCommand(paymentId, amount, reason)` |
| `CancelInvoiceCommand(id, reason)` |
| `MarkOverdueCommand(id)` |
| `AddServiceCatalogCommand(dto)` |

### Queries
| Query |
|---|
| `GetInvoicesQuery(patient/status/date/page)` |
| `GetInvoiceByIdQuery(id)` |
| `GetPatientBalanceQuery(patientId)` |
| `GetServiceCatalogQuery(category/search)` |
| `GetOverdueInvoicesQuery` |

## 6. Events
### Publishes
| Integration | Saat | Target |
|---|---|---|
| `InvoiceIssuedEvent` | issue | Notification |
| `PaymentReceivedEvent` | bayar | Notification / Accounting |
| `InvoiceOverdueEvent` | overdue | Notification reminder |
| `InvoiceCancelledEvent` / `RefundProcessedEvent` | - | - |

### Subscribes
| Event | Aksi |
|---|---|
| `AppointmentCompletedEvent` | buat invoice konsultasi |
| `LabOrderCreatedEvent` | tambah/ready invoice lab |
| `PrescriptionDispensedEvent` | invoice obat |
| `MedicalRecordTreatmentAddedEvent` | invoice tindakan |

## 7. API Endpoint preview (`/api/v1`)
### Invoice
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/billing/invoices` | buat invoice (sumber appt/lab/drug/…) | Billing/Admin |
| GET | `/billing/invoices` | list | " |
| GET | `/billing/invoices/{id}` | detail | owner/billing |
| GET | `/billing/invoices/patient/{patientId}` | invoices pasien | Patient/Billing/Admin |
| POST | `/billing/invoices/{id}/issue` | terbit | Billing |
| POST | `/billing/invoices/{id}/cancel` | batal | Billing |
| POST | `/billing/invoices/{id}/payment` | catat pembayaran | Billing/Admin |
### Master
| GET/POST | `/billing/services` | catalog tarif | GET-all/POST admin |

### Contoh request (draft)
```json
// POST /api/v1/billing/invoices
{ "patientId":"11..g", "invoiceType":"Consultation", "appointmentId":"33..g",
  "currency":"IDR",
  "lineItems":[ { "serviceCode":"CONSULT-GENERAL","quantity":1 } ] }
// Response 201 -> { "invoiceNumber":"INV-20260120-001","status":"Draft",
//                   "subtotal":150000,"taxAmount":0,"totalAmount":150000 }
```

## 8. Dependencies
- Out sync: Patient info, dsb (informasi) bila perlu.
- In (event): Appointment/Lab/Pharmacy/MedRecord membawa beban → invoice.
- Out async: issued/payment ke notification/accounting.

## 9. DB pointer
`04-Database-Design/08-…`: `Invoices, InvoiceItems, Payments, ServiceCatalogs, outbox/inbox`. Guid PK saat imp. Seed tarif (CONSULT-GENERAL, LAB-CBC, dsb).

## 10. Urutan implement ringkas
1. Scaffold; entity; migration; seed ServiceCatalog tarif.
2. GetPatientBalance + create invoice + payment (inti kasus).
3. Issue/Cancel + partialpayment recompute.
4. Event inbound datang (lab/appt/pharm) — buat agregate.
5. Overdue worker (opsional).

## 11. Catatan keputusan saat implement
- Pemicu invoice otomatis dari source event vs manual: auto lebih rapi — konfigurasi disable bila perlu.
- Tangani multiple invoice per hari? bisa gabung per kunjungan atau per jenis.
- BPJS/insurance clm — sedalam apa di MVP? (fields sudah ada: InsuranceClaimId, IsCoveredByBPJS).
