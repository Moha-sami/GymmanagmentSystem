# 🏋️ Gym Management System

A full-featured gym management web application built with **ASP.NET Core MVC** using a clean **3-Tier Architecture**. Designed to streamline gym operations including member registration, health tracking, and membership management.

---

## 📸 Screenshots

> _Coming soon_

---

## 🏗️ Architecture

This project follows the **3-Tier Architecture** pattern, separating concerns across three distinct layers:

```
GymmanagmentSystem/
├── GymManagment.DAL/        # Data Access Layer  — Models, DbContext, Repositories, UnitOfWork
├── GymMangment.BLL/         # Business Logic Layer — Services, ViewModels, Mapping, Result Pattern
└── GymmanagmentSystem/      # Presentation Layer  — Controllers, Views, wwwroot
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **DAL** | `GymManagment.DAL` | Database models, EF Core, Generic Repository, Unit of Work |
| **BLL** | `GymMangment.BLL` | Business logic, service interfaces, ViewModels, AutoMapper profiles |
| **PL**  | `GymmanagmentSystem` | MVC Controllers, Razor Views, UI |

---

## ✨ Features

### Members
- ✅ List all members
- ✅ Create member (with health record)
- ✅ Member details
- ✅ Health record details
- ✅ Edit member profile
- ✅ Delete member (with confirmation page)

### Trainers
- 🔲 CRUD _(planned)_

### Plans & Memberships
- 🔲 CRUD plans _(planned)_
- 🔲 Assign member to plan _(planned)_

### Sessions & Bookings
- 🔲 CRUD sessions _(planned)_
- 🔲 Book a session _(planned)_

### Authentication
- 🔲 Login / Register _(planned)_
- 🔲 Roles (Admin, Trainer, Member) _(planned)_

---

## 🛠️ Tech Stack

| Technology | Usage |
|------------|-------|
| ASP.NET Core MVC (.NET 9) | Web framework |
| Entity Framework Core 9 | ORM / Database access |
| SQL Server | Database |
| AutoMapper | Object mapping (ViewModel ↔ Entity) |
| Bootstrap 5 | UI styling |
| Bootstrap Icons | Icon set |
| C# | Primary language |

---

## 🧱 Design Patterns

| Pattern | Where Used |
|---------|-----------|
| **3-Tier Architecture** | Full project structure |
| **Generic Repository** | `IGenericRepository<T>` in DAL |
| **Unit of Work** | `IUnitOfWork` wrapping all repositories |
| **Result Pattern** | `Result<T>` returned from all service methods |
| **AutoMapper** | `MappingProfile` in BLL |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- Visual Studio 2022+ or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Moha-sami/GymmanagmentSystem.git
   cd GymmanagmentSystem
   ```

2. **Set up the connection string**

   In `GymmanagmentSystem/appsettings.json`, update:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=GymDB;Trusted_Connection=True;"
   }
   ```

3. **Apply migrations**
   ```bash
   dotnet ef database update --project GymManagment.DAL --startup-project GymmanagmentSystem
   ```

4. **Run the application**
   ```bash
   dotnet run --project GymmanagmentSystem
   ```

5. Open your browser at `https://localhost:PORT`

---

## 📁 Project Structure

```
GymManagment.DAL/
├── Models/
│   ├── BaseEntity.cs
│   ├── GymUser.cs (abstract)
│   ├── Member.cs
│   ├── HealthRecord.cs
│   ├── Trainer.cs
│   ├── Plans.cs
│   ├── Membership.cs
│   ├── Session.cs
│   ├── Booking.cs
│   └── Enum/Gender.cs
├── DbContext/
│   └── GymDbcontext.cs
└── Repositories/
    ├── Interfaces/
    │   ├── IGenericRepository.cs
    │   └── IUnitOfWork.cs
    └── Class/
        ├── GenericRepository.cs
        └── UnitOfWork.cs

GymMangment.BLL/
├── Common/
│   └── Result.cs
├── Mapping/
│   └── MappingProfile.cs
├── Services/
│   ├── Interfaces/ImemberService.cs
│   └── Class/MemberService.cs
└── ViewModels/
    ├── MemberViewModels/
    │   ├── MemberViewModel.cs
    │   ├── CreateMemberViewModel.cs
    │   └── MemberToUpdateViewModel.cs
    └── HealthRecordsViewModels/
        └── HealthRecordViewModel.cs

GymmanagmentSystem/
├── Controllers/
│   └── MembersController.cs
├── Views/
│   └── Members/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── MemberDetails.cshtml
│       ├── HealthRecordDetails.cshtml
│       ├── EditMember.cshtml
│       └── Delete.cshtml
└── wwwroot/
```

---

## 👨‍💻 Author

**Moha-sami** — [@Moha-sami](https://github.com/Moha-sami)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
