# Notification Service - Blueprint

> **Status**: 🔲 BELUM diimplementasikan (draft sesuai pola Identity · · /api/v1 (management endpoints) · konsumen event dari seluruh service).

## 1. Overview & Peran
Mengirim **notifikasi** lintas channel kepada user/system bagi banyak pemicu bisnis dari seluruh service. Ini cenderung **consumer-driven**: kebanyakan notifikasi dicetuskan dari event terbit subdomain (pesan) → service ini me-render dari *template* dan mengirim lewat Email/SMS/Push/SignalR (in-app), lalu mencatat status delivery & read/unread. Pada kondisi tertentu dapat juga disetujui via direct API (oleh sistem).

## 2. Fungsi utama
1. Konsumsi event domain lain → buat notifikasi.
2. Render template + substitusi placeholder.
3. Kirim lewat provider yang sesuai/template channel.
4. Lacak status pengiriman & berhasil retry (exponential backoff).
5. Kotak masuk tunggal (in-app), realtime via SignalR bila push.
6. Mark/read & unread-count (badge).
7. Template management.

## 3. Bounded context
- Notification records (inbox)
- Template & rendering
- Channel provider (Email/SMS/Push/InApp)
- Delivery status & retry
- Read/unread

## 4. Domain model
```
Notification (AggregateRoot<Guid>)
 ├─ RecipientUserId (Guid identity user) — atau RecipientContact bila belum akun
 ├─ TypeCode (nama template), Channel maybe opsy email+sms+p1
 ├─ Subject/Body (rendered), Priority(s) 
 ├─ Status(Pending/Sending/Sent/Delivered/Failed)
 ├─ delivery attempts : ICollection<DeliveryLog>
 ├─ Channel,
 ├─ CreatedDate, SentAt, ReadAt (null), ScheduledAt?
 ├─ Render(template,payload), MarkSent(providerRef), MarkRead(user)
 └─ UpdateDelivery(status, error)

DeliveryLog (child): Attempt, Provider, Action(Email/SMS/Push), At, Result(Success/Failed), Error

NotificationTemplate (master): Code, Channel, Subject + Body w/ placeholders,
   IsActive, LastUsed?
```
> Recipient user-Id dari Identity (Guid). Ada inbox `Notifications` bagi user. Penulisan id target contact jika belum full.

## 5. CQRS yang akan dibuat
### Commands
| Command |
|---|
| `CreateTemplateCommand(code, channel, subject, body)` |
| `CreateNotificationCommand(recipientId,type,channel,payload)` (dari API/system) |
| `MarkAsReadCommand(notificationId, userId)` |
| `MarkDeliveredCommand(notificationId, providerRef, ok, error)` |
| `RetryNotificationCommand(id)` |

### Queries
| Query |
|---|
| `GetUserNotificationsQuery(userId, page, unreadOnly?)` |
| `GetUnreadCountQuery(userId)` |
| `GetNotificationByIdQuery(id)` |
| `GetDeliveryStatusQuery(notificationId)` |
| (Admin) `GetTemplatesQuery` |

## 6. Events (consumer) — daftar sumber & template
| Sumber Event | Template default |
|---|---|
| `AppointmentBookedEvent` | APPOINTMENT_CONFIRMATION |
| `AppointmentRescheduledEvent` | APPOINTMENT_RESCHEDULED |
| `AppointmentCancelledEvent` | APPOINTMENT_CANCELLED |
| (worker) appointment reminder H-X | APPOINTMENT_REMINDER |
| `LabResultReadyEvent` | LAB_RESULT_READY |
| `PrescriptionDispensedEvent` | PRESCRIPTION_READY |
| `InvoiceIssuedEvent` | BILL_ISSUED |
| `PaymentReceivedEvent` | PAYMENT_RECEIVED |
| `InvoiceOverdueEvent` | PAYMENT_OVERDUE |
| `LowStockAlertEvent` / `ExpiringAlertEvent`(staff) | INVENTORY_LOW_STOCK |
| `PatientCreatedEvent` (welcome) | WELCOME_MESSAGE |
| (opsional) `MedicalRecordFinalizedEvent` | RECORD_READY |

**Service ini tidak mempublish event domain signifikan** untuk layanan lain (hanya admin/kirim error pun).

## 7. API Endpoint (management/inbox, `/api/v1`)— selaras versi lama doc
| Method | Path | Deskripsi | Auth |
|---|---|---|---|
| POST | `/api/v1/notifications` | kirim langsung (system/admin) | System/Admin |
| GET | `/api/v1/notifications/user/{userId}` | inbox | Owner |
| GET | `/api/v1/notifications/user/{userId}/unread-count` | unread badge | Owner |
| PUT | `/api/v1/notifications/{id}/read` | mark read | Owner |
| GET | `/api/v1/notifications/{id}` | detail | Owner/Admin |
| GET/POST | `/api/v1/notifications/templates` | master template | Admin |

> Kontrak request versi lama (definisi di §11 api-endpoint bagian Notification, menampilkan payload berupa patientName/ds) dapat dipakai sebagai referensi bentuk API-based notif. Hubungkan dengan inbox user Guid.

## 8. Dependencies
- In: Event dari seluruh service (Rabbit).
- Out: provider SMTP/Twilio/Firebase/APNs/SignalR.
- Resolve kontak — perlu memanggil Identity/Patient untuk contact (email/Hp) bila belum disediakan event; lebih hemat bila event sudah membawa contact snapshot.

## 9. DB pointer
`04-Database-Design/10-…`: Notifications, Templates, DeliveryLogs (+ possible InboxEventWorker). 

## 10. Urutan implement ringkas (terakhir, perlu banyak integrasi)
1. Konsumsi Event (wiring per subscription) + template catalog build.
2. Inbox persist + unread endpoint (untuk UI).
3. Render + dispatch Email (SMTP) inti.
4. SignalR hub realtime (opsional 2nd).
5. SMS/Push provider bila TTS menetapkan.
6. Retry & delivery-log.

## 11. Catatan keputusan saat implement
- Siapa mendefinisikan "channel target user/preferensi"? tambahkan preferensi di Notification bila perlu—simpan.
- Hinda rese-parse payload: pastikan event cukup data & template konsisten.
- Apakah SMS outbound (reminder/overdue) pada MVP — TTS/budget perlu keputusan.
- Deduplicate bila banyak event spam? settings.
