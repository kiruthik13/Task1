# 🏥 MediCore Hospital Management System (HMS)

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Framework](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-PostgreSQL_14+-4169E1?style=flat-square&logo=postgresql)](https://www.postgresql.org/)
[![ORM](https://img.shields.io/badge/ORM-EF_Core_10-512BD4?style=flat-square&logo=dotnet)](https://learn.microsoft.com/en-us/ef/core/)
[![UI Theme](https://img.shields.io/badge/UI-Light_Glassmorphism-4F6FFF?style=flat-square&logo=css3)](wwwroot/css/site.css)

A modern, full-featured **Hospital Management System (HMS)** built with **ASP.NET Core MVC 10.0**, **Entity Framework Core**, **PostgreSQL**, **Bootstrap 5**, **BCrypt** password security, **Dual Cookie + JWT Authentication**, and a stunning **Apple VisionOS & Fluent Design Inspired Light Glassmorphism UI**.

Designed using clean architecture principles with distinct **Repository** and **Service** layers, MediCore provides an enterprise-ready platform for managing patients, doctors, medical appointments, user accounts, and real-time hospital analytics.

---

## 📋 Table of Contents

1. [Key Features](#-key-features)
2. [Tech Stack & Dependencies](#-tech-stack--dependencies)
3. [User Roles & RBAC Matrix](#-user-roles--rbac-matrix)
4. [Project Directory Structure](#-project-directory-structure)
5. [Prerequisites](#-prerequisites)
6. [Database & Configuration Setup](#-database--configuration-setup)
7. [How to Run the Project](#-how-to-run-the-project)
8. [Default Seeded Accounts](#-default-seeded-accounts)
9. [REST API & JWT Authentication](#-rest-api--jwt-authentication)
10. [Real-Time Analytics & Polling](#-real-time-analytics--polling)
11. [Troubleshooting & FAQs](#-troubleshooting--faqs)

---

## ✨ Key Features

- **🎨 Premium Light Glassmorphism UI**:
  - Inspired by **Apple VisionOS** and **Microsoft Fluent Design**.
  - Soft light background (`#EEF4FF`), periwinkle blue primary (`#4F6FFF`), royal purple secondary (`#7C6CFF`), and teal accents (`#37D5C8`).
  - Frosted glass cards (`rgba(255,255,255,0.82)` with `backdrop-filter: blur(18px)` and `24px` border radius).
  - Floating 28px glass sidebar with active gradient pill navigation items.
- **📊 Real-Time Database Dashboard & Analytics**:
  - Dynamic PostgreSQL querying for **Total Patients**, **Active Doctors**, **Appointments Today**, and **Completed Today**.
  - **Dynamic Trend Growth Calculations**: Calculates real percentage growth ($$+X\% \text{ growth}$$) comparing current vs previous month/day database records.
  - **ApexCharts Integration**: Multi-line curve area chart for weekly appointment activity and donut chart for doctor specialization breakdown.
  - **Live Auto-Refresh Endpoint (`/Home/GetDashboardMetrics`)**: Asynchronous JSON polling updates KPI stats and charts every 10 seconds without browser reloads.
- **🩺 Doctor Profile Management**:
  - Dedicated `DoctorController.MyProfile` action and `Doctor/MyProfile.cshtml` view for Doctor accounts.
  - Doctors can manage their medical specialization, contact info, qualification, biography, and real-time availability status (*Active & Available* vs *Currently Unavailable*).
- **👤 Patient Profile & Records**:
  - Comprehensive patient profile management, contact history, and medical records.
- **🔐 Dual Authentication & Authorization**:
  - **Cookie Authentication** for browser UI navigation.
  - **JWT Bearer Token Authentication** for API integrations and Postman testing.
  - **BCrypt.Net-Next** password hashing and salting.
  - Automatic Anti-Forgery Token protection across form submissions.
  - Conditional salutation prefixing restricting `"Dr."` exclusively to Doctor accounts.

---

## 🛠️ Tech Stack & Dependencies

| Layer | Technology | Details |
| :--- | :--- | :--- |
| **Framework** | ASP.NET Core MVC | .NET 10.0 Web Framework |
| **Database** | PostgreSQL | Relational Database Engine (v14+) |
| **ORM** | Entity Framework Core | Npgsql.EntityFrameworkCore.PostgreSQL 9.0+ / 10.0 |
| **Authentication** | Dual Auth | Cookie Authentication + JWT Bearer Tokens |
| **Password Security** | BCrypt.Net-Next | Salted Password Hashing |
| **Charts & Visuals** | ApexCharts | Interactive SVG Line & Donut Charts |
| **Icons** | Lucide & Bootstrap Icons | Outline SVG & Icon Fonts |
| **Frontend UI** | CSS3 / Light Glassmorphism | Custom Glassmorphic System (`site.css`) + Bootstrap 5 |

---

## 👥 User Roles & RBAC Matrix

The application implements strictly enforced Role-Based Access Control (RBAC):

| Permission / Operation | 👑 Admin (`RoleId: 1`) | 🩺 Doctor (`RoleId: 2`) | 👤 Patient (`RoleId: 3`) |
| :--- | :---: | :---: | :---: |
| **Manage System Users & Roles** | ✅ | ❌ | ❌ |
| **System Dashboard & Analytics** | ✅ | ✅ | ❌ |
| **Manage Doctor Profiles** | ✅ | ✅ (Own Doctor Profile) | ❌ |
| **View Doctors Directory** | ✅ | ✅ | ✅ |
| **Manage Patient Profiles** | ✅ | ✅ | ✅ (Own Patient Profile) |
| **Book & Schedule Appointments** | ✅ | ✅ | ✅ |
| **Update Appointment Status** | ✅ | ✅ | ❌ (Cancel Only) |
| **Access Real-time Auto-Refresh API** | ✅ | ✅ | ✅ |

---

## 📂 Project Directory Structure

```text
HospitalManagementSystem/
├── HospitalManagement.Web/
│   ├── Configuration/              # Service & Auth Registration Extensions
│   ├── Controllers/                # MVC & REST API Controllers
│   │   ├── AccountController.cs    # Authentication, Registration, Login, Logout
│   │   ├── AppointmentController.cs# Appointment CRUD, Scheduling & Status Updates
│   │   ├── DoctorController.cs     # Doctor Directory & Doctor Profile Management
│   │   ├── HomeController.cs       # Live Dashboard, Analytics & Auto-Refresh API
│   │   └── PatientController.cs    # Patient Records & Patient Profile Management
│   ├── Data/                       # Entity Framework Core Data Layer
│   │   ├── ApplicationDbContext.cs # DbContext, DbSet Configurations & Fluent API
│   │   └── SeedData.cs             # Automated Database Seeder & User Syncing
│   ├── DTOs/                       # Data Transfer Objects with DataAnnotations
│   │   ├── AppointmentDTO.cs
│   │   ├── DoctorDTO.cs
│   │   ├── LoginDTO.cs
│   │   ├── PatientDTO.cs
│   │   └── RegisterDTO.cs
│   ├── Helpers/                    # Security & JWT Token Utilities
│   │   └── JwtService.cs           # Token Generation & Validation Helpers
│   ├── Interfaces/                 # Repository & Service Interfaces
│   │   ├── IAppointmentRepository.cs / IAppointmentService.cs
│   │   ├── IDoctorRepository.cs      / IDoctorService.cs
│   │   ├── IGenericRepository.cs
│   │   ├── IPatientRepository.cs     / IPatientService.cs
│   │   └── IUserRepository.cs        / IUserService.cs
│   ├── Middleware/                 # Custom ASP.NET Core Middlewares
│   │   └── GlobalExceptionMiddleware.cs # Centralized Exception Handler
│   ├── Migrations/                 # EF Core Database Migration Files
│   ├── Models/                     # Database Domain Entities
│   │   ├── Appointment.cs
│   │   ├── BaseEntity.cs           # Shared Id, CreatedDate, UpdatedDate
│   │   ├── Bill.cs
│   │   ├── Doctor.cs
│   │   ├── ErrorViewModel.cs
│   │   ├── Patient.cs
│   │   ├── Role.cs
│   │   └── User.cs
│   ├── Repositories/               # Data Access Layer Implementations
│   │   ├── AppointmentRepository.cs
│   │   ├── DoctorRepository.cs
│   │   ├── GenericRepository.cs
│   │   ├── PatientRepository.cs
│   │   └── UserRepository.cs
│   ├── Services/                   # Business Logic Layer Implementations
│   │   ├── AppointmentService.cs
│   │   ├── DoctorService.cs
│   │   ├── PatientService.cs
│   │   └── UserService.cs
│   ├── Views/                      # Razor UI Templates
│   │   ├── Account/                # Login, Register, AccessDenied
│   │   ├── Appointment/            # Index, Create, Edit, Details, Delete
│   │   ├── Doctor/                 # Index, Create, Edit, Details, Delete, MyProfile
│   │   ├── Home/                   # Dashboard Overview Index with ApexCharts
│   │   ├── Patient/                # Index, Create, Edit, Details, Delete, MyProfile
│   │   └── Shared/                 # _Layout, _Sidebar, _ValidationScriptsPartial
│   ├── wwwroot/                    # Static Assets
│   │   ├── css/site.css            # Light Glassmorphism CSS System
│   │   └── js/site.js              # Client-Side Interactive Logic
│   ├── appsettings.json            # Application Settings & Database Connection
│   ├── HospitalManagement.Web.csproj # Project File & NuGet Package Dependencies
│   └── Program.cs                  # Startup Entry Point & Pipeline Setup
└── README.md                       # Comprehensive Project Documentation
```

---

## ⚡ Prerequisites

Make sure you have the following installed on your machine before running the project:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (or .NET 8.0 / 9.0+)
2. **[PostgreSQL Database Server](https://www.postgresql.org/download/)** (v14 or higher running on port `5432`)
3. **EF Core Global CLI Tool**:
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

---

## ⚙️ Database & Configuration Setup

1. **Configure Connection String**:
   Open `appsettings.json` and set your PostgreSQL credentials:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=HospitalDB;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
     },
     "JwtSettings": {
       "SecretKey": "HospitalManagement_SuperSecretKey_2024_DoNotShare_MinLength32Chars!",
       "Issuer": "HospitalManagement",
       "Audience": "HospitalManagementUsers",
       "ExpiryMinutes": "60"
     }
   }
   ```

2. **Apply Database Migrations**:
   Run the EF Core migration update command from this directory:

   ```powershell
   dotnet ef database update
   ```

---

## ▶️ How to Run the Project

1. **Start the Web Application**:
   ```powershell
   dotnet run
   ```

2. **Access via Browser**:
   Open your browser and navigate to:
   - **HTTPS**: `https://localhost:7196`
   - **HTTP**: `http://localhost:5068`

---

## 🔐 Default Seeded Accounts

Upon initial startup, `SeedData.cs` automatically creates database tables, seeds initial roles, creates an Admin account, and populates sample doctors and patients.

| User Role | Email | Default Password | Initial State |
| :--- | :--- | :--- | :--- |
| **👑 System Admin** | `admin@hospital.com` | `Admin@123` | Active Admin Account |
| **🩺 Sample Doctor** | `sarah.johnson@hospital.com` | Registered via App | Cardiology Specialist |
| **👤 Sample Patient** | `john.smith@email.com` | Registered via App | Sample Patient Profile |

*Note: You can log in as `admin@hospital.com` / `Admin@123` to access all system features immediately.*

---

## 🔑 REST API & JWT Authentication

In addition to browser-based Cookie login, the system supports JWT Bearer Tokens for Postman and API clients.

### 1. Authenticate & Obtain Token

Send a `POST` request to `/Account/Login` with JSON payload:

```http
POST /Account/Login HTTP/1.1
Content-Type: application/json

{
  "email": "admin@hospital.com",
  "password": "Admin@123"
}
```

### 2. Access Protected Endpoints

Include the returned token in the `Authorization` header for subsequent requests:

```http
Authorization: Bearer <your_jwt_token_here>
```

---

## 📈 Real-Time Analytics & Polling

The dashboard incorporates a background JSON API endpoint:

- **Endpoint**: `/Home/GetDashboardMetrics`
- **Method**: `GET`
- **Behavior**: Returns dynamic metrics for KPI counts, percentage growth trends, specialization distribution, weekly appointment line series, and recent appointment lists.
- **Client Polling**: `Home/Index.cshtml` polls this endpoint every 10 seconds to update chart series and stat cards dynamically without browser reloads.

---

## ❓ Troubleshooting & FAQs

- **Database Connection Failure**:
  Ensure PostgreSQL service is running on port `5432` and your username/password in `appsettings.json` matches your PostgreSQL configuration.
- **Locked Executable (`HospitalManagement.Web.exe`) during Build**:
  If `dotnet build` fails due to locked executable files, terminate the running process using `taskkill /F /IM HospitalManagement.Web.exe` and re-run `dotnet build`.
- **Npgsql Timestamp Exception (`DateTime` in UTC)**:
  `Program.cs` includes `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);` to manage PostgreSQL timestamp mapping automatically. Ensure entity dates are instantiated using `DateTime.UtcNow`.
