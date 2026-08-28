# Notification Service - Detail

## 1. Overview
Menangani **notifikasi** ke pengguna melalui berbagai channel: **Email**, **SMS**, **Push**, dan **In-App**.

## 2. Bounded Context
- Terima event dari semua service
- Generate notifikasi berdasarkan template
- Kirim melalui channel (SMTP, Twilio/Firebase, SignalR)
- Track delivery status
- Update read/unread state
- Retry mechanism

## 3. Domain Model
```
┌────────────────────────────┐
│ Notification (Aggregate)   │
│  - RecipientUser           │
│  - TypeCode                │
│  - Subject, Body           │
│  - Channel (Email/SMS/Push)│
│  - Status (Pending→Sent)   │
│  - DeliveryLogs (list)     │
│  + Render(template)        │
│  + MarkRead()              │
│  + UpdateDelivery()        │
└────────────────────────────┘
```

## 4. CQRS

### Commands
| Command | Handler |
|---|---|
| `CreateNotificationCommand` | Persist + queue for delivery |
| `SendNotificationCommand` | Dispatch via provider |
| `MarkAsReadCommand` | Mark user read |
| `RetryNotificationCommand` | Retry failed delivery |
| `CreateTemplateCommand` | Add/update template |

### Queries
| Query | Handler |
|---|---|
| `GetUserNotificationsQuery` | Inbox |
| `GetUnreadCountQuery` | Unread badge count |
| `GetNotificationByIdQuery` | Detail |
| `GetDeliveryStatusQuery` | Track delivery |

## 5. Events

### Subscribes (semua event dari service lain)
| Source Event | Template Used |
|---|---|
| `appointment.booked` | APPOINTMENT_CONFIRMATION |
| `appointment.cancelled` | APPOINTMENT_CANCELLED |
| `appointment.reminder` | APPOINTMENT_REMINDER |
| `lab.result.ready` | LAB_RESULT_READY |
| `prescription.dispensed` | PRESCRIPTION_READY |
| `bill.issued` | BILL_ISSUED |
| `payment.received` | PAYMENT_RECEIVED |
| `payment.overdue` | PAYMENT_OVERDUE |
| `inventory.low_stock` | INVENTORY_LOW_STOCK |
| `patient.created` | WELCOME_MESSAGE |

**Notification Service hanya ber-role sebagai consumer** (tidak publish event yang signifikan untuk domain lain).

## 6. Dependencies
**Outbound calls**:
- Patient/User (resolve contact - email/phone)
- External providers: SMTP, Twilio, FCM/APNs, SignalR Hub

## 7. Design Decisions
- **Single inbox** di database + **push via SignalR** untuk real-time
- **Template** menggunakan placeholder substitution: `{{patientName}}`, `{{appointmentDate}}`
- **Retry** dengan exponential backoff (maks 5 attempt)
- **Delivery tracking** di notification logs
- Menangani banyak channel; format berbeda per channel (email HTML, SMS pendek, push payload)
