# TEAM-SOFT-SB1 — S TOM'S Project SBV1

Welcome to **S TOM'S Project SBV1** (`TEAM-SOFT-SB1`), an AI-assisted, multi-developer Windows desktop application built with **.NET 8.0 Windows Forms**.

GitHub Repository: [https://github.com/Saumya25-hub/TEAM-SOFT-SB1](https://github.com/Saumya25-hub/TEAM-SOFT-SB1)

---

## 📢 🎯 TODAY'S ACTIVE SPRINT (Assigned by PM & Vision)

> [!IMPORTANT]
> ### 🚀 Active Sprint: Splash Screen, Login Screen, & Dashboard Screen
> **PM ne aaj ka task official assign kar diya hai!** 
> 
> All developers should check their role assignment below, start work on their feature branch, and push to GitHub!

### 📌 Role Assignments for Today

- **UI/UX Developer**:
  - **Task**: `TASK-SPRINT-001` — **Splash, Login & Dashboard UI Presentation**
  - **Scope**: User interface presentation, layout, visual hierarchy, styling, controls, and screen responsiveness for **Splash**, **Login**, and **Dashboard** WinForms.
  - **Collaborative AI Directive**: When you open Antigravity on your PC, Antigravity will **NOT** dump code immediately. Antigravity will first ask you: *"Aapko Splash, Login, aur Dashboard ka visual theme, color palette, logo placement aur form layout kaisa chahiye?"* You guide the design, and Antigravity helps you implement it into WinForms controls and views cleanly.
  - **Branch**: `feature/ui-splash-login-dashboard` ➔ push to GitHub!

- **Engine Developer**:
  - **Task**: `TASK-SPRINT-002` — **Auth, Session State & Navigation Engine Services**
  - **Scope**: Core business logic, authentication service, credentials/token validation, user session state, and navigation workflow between Splash ➔ Login ➔ Dashboard.
  - **Freedom of Implementation**: Write simple, robust C# logic with **no unnecessary restrictions** — follow Clean Architecture principles (Core models and interfaces separated from UI).
  - **Collaborative AI Directive**: When you open Antigravity on your PC, Antigravity will first ask you: *"Auth validation, session state management aur navigation transition logic ka architecture kaisa design karna hai?"* You direct the logic, and Antigravity assists you in structuring the services and models.
  - **Branch**: `feature/engine-auth-dashboard` ➔ verify with `dotnet test` ➔ push to GitHub!

- **PM/Tester**:
  - **Task**: `TASK-SPRINT-003` — **Splash, Login & Dashboard Verification**
  - **Scope**: Verify acceptance criteria for Splash-to-Login transition, Login validation (valid/invalid credentials), and Dashboard landing.
  - **Deliverable**: Record verification evidence in [docs/TEST_REPORT.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TEST_REPORT.md).

### 🛠️ How to Start Work with Antigravity
1. Open this repository in Antigravity IDE on your PC.
2. Press `Ctrl + L` to open the Assistant chat.
3. Type: *"Aaj ka task start karna hai"* or *"Mujhe Splash, Login aur Dashboard par kaam karna hai"*.
4. Antigravity will recognize today's assignment from PM, ask for your design/architecture input, help you create your feature branch, write and refine the code with you, run builds/tests, and guide you to push your changes to GitHub!

---

## 1. Technology Stack

- **Framework**: .NET 8.0 Windows Forms (`net8.0-windows`)
- **Language**: C# 12
- **IDE/SDK**: .NET SDK 8.0+ / 10.0+ (Visual Studio 2022 or VS Code)
- **Architecture**: Clean Architecture (UI / Presentation, Core Domain, Engine Services, Infrastructure)
- **Version Control**: Git & GitHub

---

## 2. Solution Structure

```text
PROJECT-SBV1/
├── src/
│   ├── PROJECT-SBV1.App/              # UI layer: WinForms, Controls, ViewModels, Resources
│   ├── PROJECT-SBV1.Core/             # Pure Domain Models, Interfaces, Validation logic
│   ├── PROJECT-SBV1.Engine/           # Business Workflows, Processing Engines, Service implementations
│   └── PROJECT-SBV1.Infrastructure/   # Data Access, Persistence, Hardware/IO configuration
├── tests/
│   └── PROJECT-SBV1.Core.Tests/       # Unit & Integration Tests
├── docs/
│   ├── TEAM_ROSTER.md                 # Dynamic team roster & authorization policy
│   ├── AUDIT_LOG.md                   # Project action, review, & security audit log
│   ├── BACKUP_GUIDE.md                # Backup, recovery, & machine replacement guide
│   ├── TASKS.md                       # Structured task backlog & verification tracking
│   └── TEST_REPORT.md                 # Test execution logs & verification evidence
├── scripts/
│   └── onboard.ps1                    # Local machine profile & Windows DPAPI setup tool
├── .team/
│   ├── profile.md                     # Host developer profile (Name, Role, Language, Task)
│   └── WORKLOG.md                     # Work session log (Git diffs, tests, progress)
├── .gitignore                         # Standard .NET / Visual Studio ignore rules
├── AGENTS.md                          # Antigravity agent workspace rules & onboarding protocol
├── README.md                          # Repository overview & setup guide
├── TEAM_RULES.md                      # Team engineering standards, clean architecture, workflows
└── PROJECT-SBV1.slnx                  # Solution configuration
```

---

## 3. Quick Start & Build Instructions

### 🚀 Developer Onboarding (First-Time Setup)
> [!TIP]
> **New Developer?** When you open this workspace in Antigravity IDE, press `Ctrl + L` to open the Agent sidebar and type `hello` (or any greeting). The S TOM'S AI Assistant will greet you with **"Welcome to S TOM'S Team!"** and guide you conversationally through your role, language, and initial task setup.

### Prerequisites
- Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher.
- Git for Windows.

### Build
From the repository root, run:
```powershell
dotnet build
```

### Run
Launch the application:
```powershell
dotnet run --project PROJECT-SBV1.csproj
```

### Run Tests
```powershell
dotnet test
```

---

---

## 4. Key Application Features

| Screen | Core Capabilities | Highlights |
| :--- | :--- | :--- |
| **Splash Screen** (`SplashForm.cs`) | Startup branding, progress initialization, smooth transition | Custom GDI+ anti-aliased rendering, `animate__zoomIn` bounce reveal, glowing aura, rounded border region. |
| **Login Screen** (`LoginForm.cs`) | Secure authentication gateway, user credential entry | Pure modern white theme, vector SVG eye toggle (show/hide password), error badges, default credentials. |
| **Dashboard Screen** (`DashboardForm.cs`) | Comprehensive POS billing and business management | Left sidebar navigation, 4 KPI summary cards (Sales, Revenue, Pending, Customers), New Bill line-item calculator, searchable Bills History, Customer & Product directories. |

### 🔑 Default Login Credentials
For testing and initial evaluation:
- **Username**: `admin`
- **Password**: `admin123`

---

## 5. Prerequisites & Environment Setup

- **Operating System**: Windows 10 (Build 19041+) or Windows 11 (x64)
- **.NET SDK**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher (`dotnet --version` should return `8.0.xxx` or `10.0.xxx`)
- **IDE** *(Optional)*: Visual Studio 2022 (with .NET desktop development workload), VS Code with C# Dev Kit, or Antigravity IDE
- **Version Control**: Git for Windows

---

## 6. Installation & How to Run

### Step 1: Clone the Repository
```powershell
git clone https://github.com/Saumya25-hub/TEAM-SOFT-SB1.git
cd TEAM-SOFT-SB1
```

### Step 2: Restore & Build
```powershell
dotnet restore
dotnet build
```
*(Verify clean compilation with `0 Warning(s)`, `0 Error(s)`).*

### Step 3: Run the Application
```powershell
dotnet run
```
The application will launch the Splash Screen, transition smoothly to the Login Screen, and open the SB1 Billing Dashboard upon authentication.

---

## 7. Troubleshooting

| Issue | Cause | Solution |
| :--- | :--- | :--- |
| `'dotnet' is not recognized` | .NET SDK is not in system PATH | Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and restart your terminal. |
| `Cannot find PROJECT-SBV1.csproj` | Command run outside repository root | Ensure current working directory is the project root (`cd C:\SBV1`). |
| `File locked / CS2012 error` | Previous app instance still running | Close the running app or run `Stop-Process -Name PROJECT-SBV1 -Force` in PowerShell. |
| Blurry fonts on High-DPI screens | Windows DPI virtualization | High DPI scaling is enabled via `ApplicationConfiguration.Initialize()` in `Program.cs`. Ensure display scaling is set to 100%, 125%, or 150%. |

---

---

## 9. Windows Desktop Inventory & Billing Suite (Electron + React + Node.js Backend)

This project has been expanded into a full-scale, multi-tenant Windows Desktop Inventory, Billing & Transaction Management Suite with 10 core business modules matching the reference dashboard UI:

### 🧩 10 Core Application Modules
1. **Overview Dashboard**: Metric KPI cards (Net Income, Sales, Returns), 7-month interactive revenue bar chart, category donut breakdown, recent transactions, low-stock warnings, and fast theme toggle.
2. **Statistics & Reports**: Financial intelligence, product margins, inventory valuation, and CSV export.
3. **Customers**: Customer database, contact cards, bill history, and credit balance tracking.
4. **Suppliers**: Wholesale supplier directory, procurement logs, and payable accounts.
5. **Products & Inventory**: SKU codes, barcode scanner integration, category organization, and manual stock adjustment modal with audit trail.
6. **Purchase Management**: Purchase order generation, receiving items, and automatic stock increment.
7. **Sales Billing & POS Invoices**: Rapid Point-of-Sale checkout, barcode lookup, line discounts, configurable taxes, Cash/Bank/UPI payment modes, and **Printable Tax Invoice Modal**.
8. **Messages & Notifications**: Operational warnings, threshold breaches, and backup notices.
9. **Transactions & Receipts**: Real-time financial ledger across Cash, Bank Transfer, UPI, and Card receipts.
10. **Settings & Security**: Multi-tenant business switcher, legal profile, tax rules, and immutable audit logs.

### 🚀 How to Run the Suite
1. **Start Central Backend API**:
   ```powershell
   cd backend
   npm run dev
   ```
   *(Server starts at `http://localhost:5000` with automatic database connection & mock seeding).*
2. **Start Desktop Client**:
   ```powershell
   cd desktop
   npm run dev
   # Or launch inside Electron:
   npm run electron:dev
   ```
3. **One-Click Master Launcher**:
   - Double click `start-all.bat` in the project root to launch both backend and desktop client concurrently.

### 📦 Building Windows x64 Installer
To generate a standalone branded Windows x64 NSIS installer (`release/SimpleBilling-Setup-1.0.0.exe`):
```powershell
cd desktop
npm run electron:build
```

---

## 10. Multi-Developer Contribution Guidelines

All team members must follow the engineering standards in [TEAM_RULES.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/TEAM_RULES.md):
- **Feature Branches**: Never commit directly to `main`. Create feature branches (`feature/<task-name>`).
- **Clean Architecture**: Keep UI presentation strictly separate from Core domain models and Engine services.
- **Session Logging**: Update [docs/TASKS.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TASKS.md) and [.team/WORKLOG.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/.team/WORKLOG.md) before concluding each work session.
- **Verification First**: Never mark a task `Done` without build/test verification proof in [docs/TEST_REPORT.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TEST_REPORT.md).

---

## 11. License

This project is licensed under the **MIT License**. See `LICENSE` for details.


