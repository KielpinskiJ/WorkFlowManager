<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-blue?style=for-the-badge&logo=dotnet" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/EF_Core-8.0-green?style=for-the-badge&logo=nuget" alt="Entity Framework Core" />
  <img src="https://img.shields.io/badge/Bootstrap-5.3-purple?style=for-the-badge&logo=bootstrap" alt="Bootstrap 5" />
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="MIT License" />
</p>

# 🏢 WorkFlowManager

> A comprehensive workforce management solution for employee scheduling, leave requests, and payroll processing.

**WorkFlowManager** is a modern web application built with ASP.NET Core MVC that streamlines HR operations including department management, work shift scheduling, leave request workflows, and automated payroll calculations.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Database Setup](#-database-setup)
- [Running the Application](#-running-the-application)
- [Default Accounts](#-default-accounts)
- [Project Structure](#-project-structure)
- [Contributors](#-contributors)
- [License](#-license)

---

## ✨ Features

| Module | Features |
|--------|----------|
| **👥 User Management** | Employee CRUD, role-based access (Admin/Employee), department assignment |
| **🏛️ Departments** | Department management with customizable hourly rates |
| **📅 Shift Scheduling** | Create and manage work shifts, filtering by date/employee |
| **📝 Leave Requests** | Vacation requests, department transfer requests, approval workflow |
| **💰 Payroll** | Automated salary calculation (hours × rate + bonuses), monthly reports |
| **🎁 Bonuses** | One-time bonus management with reason tracking |
| **📊 Statistics** | Interactive charts (Chart.js), department performance metrics |
| **🌍 Localization** | Full Polish and English language support |

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 8, ASP.NET Core MVC |
| **ORM** | Entity Framework Core 8 (Code-First) |
| **Database** | SQL Server (LocalDB / Express) |
| **Authentication** | ASP.NET Core Identity |
| **Frontend** | Razor Views, Bootstrap 5.3, Bootswatch Zephyr Theme |
| **Charts** | Chart.js 4.x |
| **Icons** | Bootstrap Icons |

---


## 📦 Prerequisites

**Required:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio)

**Recommended:**
- [Git](https://git-scm.com/) (for cloning the repository)

---

## 🚀 Installation

### 1. Clone the repository

```bash
git clone https://github.com/KielpinskiJ/WorkFlowManager.git
cd WorkFlowManager
```

---

## ⚙️ Configuration

### Admin Password (optional)

To set a custom admin password, create a `.env` file in the project root:

```bash
cp env.sample .env
```

Then edit `.env` and set your password:
```
ADMIN_PASSWORD=YourSecurePassword123!
```

> If no `.env` file is found, the application uses the default password: `Admin123!`

### Connection String

The default connection string in `appsettings.json` uses LocalDB (included with Visual Studio):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkFlowManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

## 🗄️ Database Setup

### Apply migrations

**.NET CLI:**
```bash
dotnet ef database update
```

**NuGet Package Manager Console (Visual Studio):**
```powershell
Update-Database
```

This will:
1. Create the database if it doesn't exist
2. Apply all migrations
3. Seed initial data (Admin user, sample departments, employees)

### Reset database (optional)

**.NET CLI:**
```bash
dotnet ef database drop --force
dotnet ef database update
```

**NuGet Package Manager Console:**
```powershell
Drop-Database
Update-Database
```

### Create new migration (development)

**.NET CLI:**
```bash
dotnet ef migrations add <MigrationName>
```

**NuGet Package Manager Console:**
```powershell
Add-Migration <MigrationName>
```

---

## ▶️ Running the Application

### Using .NET CLI

```bash
dotnet run
```

The application will start at:
- **HTTP:** `http://localhost:5193`
- **HTTPS:** `https://localhost:7113`

### Using Visual Studio

1. Open `WorkFlowManager.sln`
2. Press `F5` or click **Start Debugging**

### Using VS Code

1. Open the project folder
2. Press `F5` (select `.NET Core` if prompted)

---

## 🔐 Default Accounts

After initial seeding, the following accounts are available:

| Role | Email | Password |
|------|-------|----------|
| **Admin** | `admin@wsb.pl` | `Admin123!` ¹ |
| **Employee** | `anna.nowak@company.pl` | `Employee123!` |
| **Employee** | `piotr.kowalski@company.pl` | `Employee123!` |
| **Employee** | `...` _(X employees generated)_ | `Employee123!` |

> ¹ Default password. Can be changed by setting `ADMIN_PASSWORD` in `.env` file (see [Configuration](#%EF%B8%8F-configuration)).

> ⚠️ **Security Notice:** Change default passwords in production environments.

---

## 📁 Project Structure

```
WorkFlowManager/
├── Controllers/              # MVC Controllers
│   ├── HomeController.cs
│   ├── UsersController.cs
│   ├── DepartmentsController.cs
│   ├── ShiftsController.cs
│   ├── RequestsController.cs
│   ├── PayrollController.cs
│   ├── BonusesController.cs
│   └── StatsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs           # Initial data seeding
├── Models/                   # Entity models & enums
│   ├── ApplicationUser.cs
│   ├── Department.cs
│   ├── WorkShift.cs
│   ├── LeaveRequest.cs
│   ├── Bonus.cs
│   ├── RequestType.cs        # Enum: Vacation, DepartmentChange
│   ├── RequestStatus.cs      # Enum: Pending, Approved, Rejected
│   └── ErrorViewModel.cs
├── Services/                 # Business logic layer
│   ├── Interfaces/
│   │   ├── IDepartmentService.cs
│   │   ├── IPayrollService.cs
│   │   ├── IRequestService.cs
│   │   └── IShiftService.cs
│   ├── DepartmentService.cs
│   ├── PayrollService.cs
│   ├── RequestService.cs
│   └── ShiftService.cs
├── ViewModels/               # Form/display models
│   ├── CreateBonusViewModel.cs
│   ├── CreateRequestViewModel.cs
│   ├── CreateShiftViewModel.cs
│   ├── DepartmentStatsDto.cs
│   ├── PayrollViewModel.cs
│   ├── UserEditViewModel.cs
│   └── UserListViewModel.cs
├── Views/                    # Razor views
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _LoginPartial.cshtml
│   ├── Home/
│   ├── Users/
│   ├── Departments/
│   ├── Shifts/
│   ├── Requests/
│   ├── Payroll/
│   ├── Bonuses/
│   └── Stats/
├── Resources/                # Localization files
│   ├── SharedResource.cs
│   ├── SharedResource.resx       # English
│   └── SharedResource.pl.resx    # Polish
├── wwwroot/                  # Static files
│   ├── css/
│   └── js/
├── Migrations/               # EF Core migrations
├── Properties/
│   └── launchSettings.json
├── Program.cs                # Application entry point
├── appsettings.json          # Configuration
└── appsettings.Development.json
```

---

## 👥 Contributors

<table>
  <tr>
    <td align="center">
      <a href="https://github.com/KielpinskiJ">
        <img src="https://github.com/KielpinskiJ.png" width="100px;" alt=""/>
        <br />
        <sub><b>@KielpinskiJ</b></sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/DKowalec004">
        <img src="https://github.com/DKowalec004.png" width="100px;" alt=""/>
        <br />
        <sub><b>@DKowalec004</b></sub>
      </a>
    </td>
        <td align="center">
      <a href="https://github.com/KacKol">
        <img src="https://github.com/KacKol.png" width="100px;" alt=""/>
        <br />
        <sub><b>@KacKol</b></sub>
      </a>
    </td>
  </tr>
</table>

**Team JDK**

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Made with 🐟 by <strong>Team JDK</strong>
  <br />
</p>
