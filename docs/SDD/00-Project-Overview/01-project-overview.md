# 00. Project Overview

## 1. Nama Proyek
**HMS - Hospital Management System**

## 2. Visi
Membangun platform manajemen rumah sakit yang **modular**, **scalable**, dan **dapat di-maintain** menggunakan arsitektur microservices dengan .NET 10.

## 3. Misi
- Memisahkan tanggung jawab bisnis ke dalam service yang independen
- Memungkinkan scale-out per service sesuai kebutuhan beban
- Menerapkan best practice (DDD, CQRS, Clean Architecture)
- Memastikan reliability dan availability tinggi
- Membangun sistem yang mudah di-develop, diuji, dan di-deploy secara independen

## 4. Tujuan Bisnis
1. **Digitalisasi proses rumah sakit** - dari registrasi pasien hingga billing
2. **Integrasi data antar departemen** - pasien, dokter, apotek, lab, keuangan
3. **Meningkatkan efisiensi operasional** - otomatisasi alur kerja
4. **Meningkatkan kualitas layanan pasien** - pengalaman pengguna yang baik
5. **Kepatuhan regulasi** - pelacakan rekam medis, resep, dan pembayaran

## 5. Scope Proyek

### In Scope
| Modul | Deskripsi |
|---|---|
| Patient Management | Registrasi, CRUD profil pasien |
| Doctor Management | Profil dokter, jadwal praktek, spesialisasi |
| Appointment Scheduling | Booking dan manajemen janji temu |
| Electronic Medical Records (EMR) | Rekam medis digital pasien |
| Pharmacy Management | Resep dan dispensing obat |
| Laboratory Management | Order dan hasil pemeriksaan lab |
| Billing & Payment | Tagihan dan pembayaran |
| Inventory Management | Stok obat dan alat medis |
| Notification System | Pengingat dan notifikasi |

### Out of Scope (Fase Awal)
- Integrasi mesin X-Ray / DICOM
- Sistem HR / Payroll internal
- Integrasi sistem asuransi pihak ketiga
- Modul akuntansi keuangan penuh (general ledger)

## 6. Stakeholder
| Role | Kebutuhan |
|---|---|
| Administrator | Kelola master data, user, dan role |
| Dokter | Melihat jadwal, rekam medis, menulis resep |
| Perawat | Registrasi pasien, triase, input data vital |
| Apoteker | Proses resep, dispense obat |
| Staf Lab | Menerima order lab, input hasil |
| Staf Kasir/Billing | Membuat tagihan, menerima pembayaran |
| Pasien | Melihat jadwal, pembayaran, riwayat |

## 7. NFR (Non-Functional Requirements)
| Aspek | Kebutuhan |
|---|---|
| Availability | 99.9% uptime |
| Scalability | Horizontal scaling per service |
| Security | Encryption at rest & in transit, RBAC |
| Performance | API response < 300ms (P95) |
| Auditability | Audit trail untuk perubahan data sensitif |
| Compliance | Perlindungan data pasien (HIS/EMR regulation) |

## 8. Asumsi & Kendala
- Semua service berjalan di environment containerized (Docker)
- SQL Server tersedia dan ter-manage
- Komunikasi antar service bersifat asynchronous (event-driven) untuk operasi non-kritikal
- Synchronous call tetap ada untuk operasi yang membutuhkan response langsung

## 9. Return on Investment Target
- Pengurangan waktu registrasi pasien: **60%**
- Pengurangan antrian billing: **50%**
- Digitalisasi rekam medis: **100%**
