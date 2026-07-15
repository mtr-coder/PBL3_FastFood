<div align="center">

# PBL3 — Fast Food Management System

**A full-featured desktop application for managing fast food restaurant operations**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-Windows%20Forms-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)
[![BCrypt](https://img.shields.io/badge/BCrypt-Password%20Hashing-00C7B7?style=for-the-badge)](https://github.com/BcryptNet/bcrypt.net)

</div>

---

## 📌 Overview

**PBL3 Fast Food Management System** is a desktop application built with **C# .NET 10 Windows Forms** and **Microsoft SQL Server**, designed to digitize and streamline daily operations at a fast food restaurant. The system covers the full business cycle — from sales and inventory management to employee scheduling and revenue analytics — all within a role-based access control architecture.

---

## ✨ Key Features

### 🔐 Authentication & Security
- Phone number + password login with **BCrypt hashing** (industry-standard, no plain-text passwords)
- Role-based access control: **Admin** (full access) vs **Staff** (restricted access)
- Password recovery flow
- Session management throughout the application

### 🛒 Point of Sale (POS)
- Create and process sales invoices in real-time
- Support multiple serving unit sizes (e.g., Small / Medium / Large) with individual pricing
- **Customer loyalty points** system: auto-accumulate and redeem for discounts
- Invoice history with full detail lookup

### 📦 Inventory & Purchasing
- Manage raw ingredients with **low-stock threshold alerts**
- Create purchase orders from suppliers with itemized records
- Ingredient consumption tracking via dish recipes (`DINH_MUC_MON`)

### 🍽️ Menu Management
- Full CRUD for dishes, categories, and serving units
- Link ingredient requirements to each dish variant (recipe-level tracking)
- Toggle dish availability status

### 👨‍💼 Employee Management
- Complete employee records (CRUD) with role assignment
- **Shift scheduling**: assign shifts across date ranges
- **Leave request workflow**: staff submits → admin reviews & approves/rejects
- Monthly leave quota enforcement (max 3 days/month)
- Salary calculation based on base pay + shift coefficient

### 👥 Customer Management
- Customer profile management
- Loyalty point tracking with automatic discount calculation
  - Every **100,000 VND** spent → +10 points
  - Every **10 points** → 10,000 VND discount

### 🏭 Supplier Management
- Supplier contact management
- Link suppliers to purchase orders for traceability

### 📊 Statistics & Reporting
- Revenue dashboard with chart visualization (`DataVisualization`)
- Filter by custom date ranges
- Key metrics: total sales, best-selling dishes, shift staffing summaries

---

## 🏗️ Architecture

The project follows a clean **3-Layer Architecture** for maintainability and separation of concerns:

```
PBL3/
├── UI/                  # Presentation Layer — Windows Forms (Views)
│   ├── TrangDangNhap    # Login screen
│   ├── BanHang          # Point of Sale
│   ├── QuanLiNhanVien   # Employee management
│   ├── QuanLiMonAn      # Menu management
│   ├── ThongKe          # Statistics & reports
│   └── ...              # 20+ screens total
│
├── Business/            # Business Logic Layer — Services
│   ├── AuthService      # Authentication & session
│   ├── BanHangService   # Sales logic
│   ├── TrangNhanVienService  # Employee & shift logic
│   └── ...              # Domain services per feature
│
├── DataAccess/          # Data Access Layer — Repositories
│   ├── DbHelper         # Centralized SQL execution helper
│   ├── AuthRepository
│   ├── BanHangRepository
│   └── ...              # ADO.NET repositories with parameterized queries
│
├── Models/              # Data Transfer Objects
│   ├── NhanVien, KhachHang, MonBan, HoaDonBan ...
│   └── ...              # 23 model classes
│
└── DataBase/
    ├── QL_FASTFOOD.sql  # Full database schema (18 tables)
    └── App.config       # Connection string configuration
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 |
| Framework | .NET 10, Windows Forms |
| Database | Microsoft SQL Server |
| Data Access | ADO.NET (`System.Data.SqlClient`) |
| Security | BCrypt.Net-Next v4.1 |
| Charts | System.Windows.Forms.DataVisualization |
| Config | System.Configuration.ConfigurationManager |

---

## 🗄️ Database Schema

The database `QL_FASTFOOD` consists of **18 relational tables**:

| # | Table | Description |
|---|---|---|
| 1 | `CHUC_VU` | Job positions & base salary |
| 2 | `NHAN_VIEN` | Employee records |
| 3 | `CA_TRUC` | Shift definitions with time & pay coefficient |
| 4 | `PHAN_CONG_CA` | Shift assignments per employee per day |
| 5 | `LOAI_MON` | Dish categories |
| 6 | `DON_VI_TINH` | Units of measurement |
| 7 | `MON_BAN` | Menu items |
| 8 | `DON_VI_PHUC_VU` | Serving sizes (S/M/L) |
| 9 | `MON_DON_VI_PHUC_VU` | Dish × Size pricing |
| 10 | `NGUYEN_LIEU` | Raw ingredients with stock tracking |
| 11 | `DINH_MUC_MON` | Recipe: ingredient usage per dish variant |
| 12 | `KHACH_HANG` | Customer profiles & loyalty points |
| 13 | `HOA_DON_BAN` | Sales invoices |
| 14 | `CT_HOA_DON_BAN` | Sales invoice line items |
| 15 | `NHA_CUNG_CAP` | Supplier directory |
| 16 | `HOA_DON_NHAP` | Purchase orders |
| 17 | `CT_HOA_DON_NHAP` | Purchase order line items |
| 18 | `YEU_CAU` | Employee leave/request submissions |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (2019 or later recommended)
- Visual Studio 2022+

### Setup

**1. Clone the repository**
```bash
git clone https://github.com/mtr-coder/PBL3_FastFood.git
cd PBL3_FastFood
```

**2. Create the database**

Open SQL Server Management Studio and run:
```sql
-- Execute the full schema script
DataBase/QL_FASTFOOD.sql
```

**3. Configure connection string**

Edit `DataBase/App.config`:
```xml
<connectionStrings>
  <add name="QL_FASTFOOD"
       connectionString="Server=YOUR_SERVER;Database=QL_FASTFOOD;Integrated Security=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**4. Migrate existing passwords to BCrypt** *(if seeding legacy data)*
```sql
UPDATE dbo.NHAN_VIEN
SET MatKhau = '$2a$11$Gp2jEU5jKnlNfQmknP0Sm.G/PvZS1gF8Zu6gj7qS9RXlHBiVNBvQ2'
WHERE MatKhau = '123';
-- Hash above = BCrypt('123')
```

**5. Run the application**
```bash
dotnet run
# or open PBL3.slnx in Visual Studio and press F5
```

---

## 🔒 Security Highlights

- **No plain-text passwords** — all passwords are hashed with BCrypt (work factor 11)
- **Parameterized SQL queries** throughout all repositories — SQL Injection protected
- **Role-based session control** — UI elements dynamically shown/hidden based on user role
- **Input validation** on all forms before database operations

---

## 📈 Business Logic Highlights

```
Loyalty Points Calculation:
  Earn:   every 100,000 VND spent → +10 points
  Redeem: every 10 points → 10,000 VND discount

Shift Pay Calculation:
  Salary = BaseSalary + (ShiftsWorked × 176,000 VND × ShiftCoefficient)

Leave Policy:
  Max 3 approved leave days per month per employee
```

---

## 👨‍💻 Author

> Developed as **Project-Based Learning 3 (PBL3)** — Year 2, University of Science and Technology, Da Nang (DUT)

---

<div align="center">
  <sub>Built with ❤️ using C# · .NET 10 · SQL Server</sub>
</div>
