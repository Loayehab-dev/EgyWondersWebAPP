# 🏺 EgyWonders - Tourism & Rental Platform

![.NET](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-Hybrid%20Mode-blue)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-purple)
![jQuery](https://img.shields.io/badge/jQuery-AJAX-0769ad)

**EgyWonders** is a comprehensive full-stack booking platform connecting travelers with local apartment hosts and certified tour guides in Egypt. 

It features a robust **REST API** backend built with Clean Architecture patterns and a decoupled **ASP.NET Core MVC** frontend acting as a consumer client. The system implements advanced security, automated inventory management, and role-based access control.

---

## 🏗️ Architecture

The solution follows a strict **Service-Repository Pattern** to ensure separation of concerns, testability, and scalability.

### Backend (API)
* **Layered Architecture:** Controllers -> Services (Business Logic) -> Unit of Work -> Generic Repository -> Database.
* **Database Strategy:** **Hybrid Approach** (Started with Database-First Reverse Engineering, evolved with Code-First Migrations for schema updates).
* **Security:**
    * **ASP.NET Identity** for user management.
    * **JWT (JSON Web Tokens)** for stateless authentication.
    * **Google OAuth 2.0** for social login.
    * **Rate Limiting** middleware to prevent abuse.
* **Validation:** Comprehensive DTO validation with Data Annotations.

### Frontend (MVC Client)
* **Framework:** ASP.NET Core MVC 8 (Razor Views).
* **Integration:** Consumes the backend via a typed `HttpClient` factory (Backend-for-Frontend pattern).
* **Session:** Secure Cookie Authentication (Stores API JWTs in HttpOnly cookies).
* **Interactivity:** jQuery & AJAX for dynamic form handling and modal updates.
* **Styling:** Bootstrap 5 for responsive, mobile-first layout.

---

## 🚀 Key Features

### 👤 User Roles
* **Guest:** Search listings, book trips, manage bookings, leave reviews.
* **Host:** Create property listings, upload photos, track revenue.
* **Tour Guide:** Create tour packages, manage schedules/seats, upload professional certifications.
* **Admin:** User management, document verification, system-wide configuration.

### 🛠️ Core Modules

#### 1. Authentication & Security
* **Secure Login:** Standard Email/Password & Google Sign-In.
* **Verification:** Email confirmation workflow via SMTP.
* **Recovery:** Secure Password Reset flow using generated tokens.
* **Protection:** API Rate Limiting to block spam attacks.

#### 2. Listing Engine (Rentals)
* **Property Management:** CRUD operations for apartments/villas.
* **Media:** Multipart file uploading (`IFormFile`) for property galleries stored in `wwwroot`.
* **Smart Booking:** Date-range validation to prevent double-booking.

#### 3. Tour Management System
* **Product Catalog:** Create tours with geo-coordinates and rich descriptions.
* **Scheduling Engine:** Manage specific time slots and seat capacity.
* **Inventory Control:** Automated ticket counting (prevents overbooking).

#### 4. Compliance & Trust
* **Host Verification:** Upload national IDs/documents for admin approval.
* **Guide Certification:** Guides must upload licenses before publishing tours.
* **Reviews:** Verified feedback system linked to completed bookings.

---

## 🛠️ Technology Stack

| Category | Technology |
| :--- | :--- |
| **Framework** | .NET 8 (C#) |
| **Database** | Microsoft SQL Server 2022 |
| **ORM** | Entity Framework Core 9 |
| **Auth** | ASP.NET Identity, JWT Bearer, Google Auth Library |
| **File Storage** | Local Server Storage (`wwwroot` with static file serving) |
| **Email** | `System.Net.Mail` (SMTP) |
| **Frontend** | Razor Views, Bootstrap 5, jQuery, AJAX |

---

## ⚙️ Getting Started

### 1. Prerequisites
* Visual Studio 2022 or VS Code.
* .NET 8 SDK.
* SQL Server (LocalDB or Express).

### 2. Database Setup
This project uses a hybrid approach. The initial schema was reverse-engineered, but updates are handled via Migrations.

1.  Open **Package Manager Console**.
2.  Set **Default Project** to `EgyWonders.API`.
3.  Run the following command to sync your local DB:
    ```powershell
    Update-Database
    ```

### 3. Configuration (`appsettings.json`)
Create an `appsettings.json` file in the API project with your secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TravelDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_SUPER_SECURE_LONG_KEY_HERE_MUST_BE_32_CHARS",
    "Issuer": "EgyWonders",
    "Audience": "EgyWondersUsers",
    "AccessTokenExpirationMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "YOUR_GOOGLE_APP_PASSWORD",
    "EnableSsl": true
  },
  "GoogleAuthSettings": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  },
  "AppUrl": "https://localhost:7151"
}
