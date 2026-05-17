# GLMS — Global Logistics Management System
## Part 2: Core Prototype (ASP.NET Core MVC Monolith)

---

## About This Project

This is Part 2 of the GLMS project for TechMove Logistics, built as an ASP.NET Core MVC monolith using .NET 10. It implements the architecture designed in Part 1 (Zachman Framework + GoF design patterns) as a working web application.

The system allows TechMove staff to manage clients, freight contracts, and service requests — with PDF file handling, live currency conversion (USD to ZAR), and automated unit tests.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Database | SQL Server LocalDB |
| ORM | Entity Framework Core 9 |
| Unit Tests | xUnit |
| Currency API | open.er-api.com (free, no API key) |
| Frontend | Bootstrap 5 (CDN) |

---

## HOW TO RUN THIS PROJECT

### Step 1 — Prerequisites
Make sure you have installed:
- Visual Studio 2022 (or later)
- .NET 10 SDK
- SQL Server LocalDB (comes with Visual Studio)

### Step 2 — Open the Solution
- Extract the zip file
- Open Visual Studio
- File → Open → Project/Solution
- Select `GLMS.sln`

### Step 3 — Restore Packages
Open the **Package Manager Console**
(Tools → NuGet Package Manager → Package Manager Console) and run:

```
dotnet restore
```

### Step 4 — Create the Database
In Package Manager Console, make sure the **Default Project** dropdown is set to `GLMS.Web`, then run:

```
Add-Migration InitialCreate
Update-Database
```

This creates the `GLMSDb` database in SQL Server LocalDB and seeds it with test data.

### Step 5 — Run the Application
Press **F5** or click the green Run button.
The app opens at `https://localhost:xxxx`

### Step 6 — Run the Unit Tests
- Open Test Explorer: Test → Test Explorer
- Click **Run All Tests**
- All 10 tests should show green ticks

---

## PROJECT STRUCTURE

```
GLMS/
├── GLMS.sln
│
├── GLMS.Web/                            <- Main MVC web application
│   ├── Controllers/
│   │   ├── HomeController.cs            <- Dashboard home page
│   │   ├── ClientController.cs          <- Full CRUD for clients
│   │   ├── ContractController.cs        <- CRUD + PDF upload + LINQ search/filter
│   │   └── ServiceRequestController.cs  <- CRUD + currency API + workflow logic
│   │
│   ├── Models/
│   │   ├── Client.cs                    <- Name, ContactDetails, Region
│   │   ├── Contract.cs                  <- ContractStatus enum (Draft/Active/Expired/OnHold)
│   │   └── ServiceRequest.cs            <- CostUSD, CostZAR, ExchangeRateUsed
│   │
│   ├── Data/
│   │   └── GLMSDbContext.cs             <- EF Core DbContext with seed data
│   │
│   ├── Services/
│   │   ├── CurrencyService.cs           <- Calls ExchangeRate API (Strategy pattern)
│   │   └── FileService.cs               <- PDF validation and file saving
│   │
│   ├── Views/
│   │   ├── Client/      -> Index, Create, Edit, Details, Delete
│   │   ├── Contract/    -> Index, Create, Edit, Details, Delete
│   │   ├── ServiceRequest/ -> Index, Create, Details, Delete
│   │   └── Shared/      -> _Layout.cshtml (navigation + Bootstrap)
│   │
│   ├── wwwroot/uploads/                 <- Uploaded PDF files saved here
│   ├── Program.cs                       <- App setup, DI registration
│   └── appsettings.json                 <- DB connection string
│
└── GLMS.Tests/                          <- xUnit test project
    └── GLMSTests.cs                     <- 10 unit tests
```

---

## KEY FEATURES

### 1. Database & Models
- Three EF Core entities: **Client**, **Contract**, **ServiceRequest**
- Relationships: Client -> Contracts -> ServiceRequests
- Database migrations managed via EF Core tools

### 2. PDF File Upload
- Contracts support uploading a signed PDF agreement
- Saved to `wwwroot/uploads/` with a unique GUID filename
- Downloadable from the Contracts list
- Only `.pdf` files accepted — all other file types are rejected with a validation error

### 3. Workflow Logic
- A ServiceRequest **cannot** be created against a contract that is `Expired` or `On Hold`
- Only `Active` contracts appear in the ServiceRequest creation dropdown
- This enforces the business rule defined in the Part 1 Zachman Framework design

### 4. Currency API Integration
- The ServiceRequest creation page fetches the live USD to ZAR exchange rate from `open.er-api.com`
- As the user types a USD cost, the ZAR equivalent auto-calculates in real time via AJAX
- The exchange rate and ZAR amount are saved permanently with the service request

### 5. Search & Filter (LINQ)
- The Contracts page supports filtering by **Status** and **Start Date range**
- Implemented using LINQ `.Where()` queries in `ContractController`

### 6. Unit Tests
| Test Class | Tests | What It Checks |
|---|---|---|
| CurrencyServiceTests | 4 | USD x rate = correct ZAR amount |
| FileServiceTests | 6 | .pdf accepted, .exe/.docx rejected, null file, file too large |

---

## CONNECTION TO PART 1 ARCHITECTURE

This project directly implements the architecture designed in the Part 1 report:

| Part 1 Design | Part 2 Implementation |
|---|---|
| Zachman Data row — Contract, Client, ServiceRequest | EF Core models + SQL Server |
| Zachman Business Rule — Requests only on Active contracts | Workflow check in ServiceRequestController |
| Strategy Pattern — ICurrencyStrategy | ICurrencyService interface + CurrencyService class |
| FixedRateStrategy (for testing) | Unit tests pass a fixed rate — no live API call needed |
| Observer Pattern — status change notifications | ContractStatus enum drives workflow blocking logic |
| Factory Method — contract creation | Contract model + ContractStatus enum creation flow |

---

## Author
Student — TechMove GLMS Project
IIE Enterprise Development — Part 2 Submission
