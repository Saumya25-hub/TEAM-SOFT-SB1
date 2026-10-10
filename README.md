# TEAM-SOFT-SB1 — S TOM'S Project SBV1

Welcome to **S TOM'S Project SBV1** (`TEAM-SOFT-SB1`), an AI-assisted, multi-developer Windows desktop application built with **.NET 8.0 Windows Forms**.

GitHub Repository: [https://github.com/Saumya25-hub/TEAM-SOFT-SB1](https://github.com/Saumya25-hub/TEAM-SOFT-SB1)

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
> **New Developer?** When you open this workspace in Antigravity IDE, press `Ctrl + L` to open the Agent sidebar and type `hello` (or any greeting). The S TOM'S AI Assistant will greet you with **"Welcome to S TOM'S!"** and guide you conversationally through your role, language, and initial task setup.

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

## 4. Team Workflow & Contribution Rules

All team members must follow the standards defined in [TEAM_RULES.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/TEAM_RULES.md):
- **Feature Branches**: Never commit directly to `main`. Create feature branches like `feature/ui-form1-redesign` or `feature/engine-calc`.
- **Clean Architecture**: Adhere strictly to layer separation (UI does not query databases; Engine does not manipulate UI components).
- **Session Logging**: Update [docs/TASKS.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TASKS.md) and [.team/WORKLOG.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/.team/WORKLOG.md) before concluding each work session.
- **Verification First**: Never mark a task `Done` without build/test verification proof in [docs/TEST_REPORT.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TEST_REPORT.md).

---

## 5. 🎯 Today's Sprint Assignment (Assigned by PM / Project Owner)

> [!IMPORTANT]
> **Active Sprint Goals**: Today's work focuses on the three foundational application screens:
> 1. **Splash Screen**
> 2. **Login Screen**
> 3. **Dashboard Screen**

### Role Responsibilities & Work Breakdown

- **UI/UX Developer**:
  - **Scope**: User interface presentation, layout, visual hierarchy, styling, controls, and screen responsiveness for **Splash**, **Login**, and **Dashboard** forms.
  - **Collaborative AI Directive**: When you open Antigravity on your PC, Antigravity will **NOT** dump code immediately. Antigravity will first ask you: *"Aapko Splash, Login, aur Dashboard ka visual theme, color palette, logo placement aur form layout kaisa chahiye?"* You guide the design, and Antigravity helps you implement it into WinForms controls and views cleanly.
  - **Branch**: Create and work on a feature branch (e.g. `feature/ui-splash-login-dashboard`).
  - **Deliverable**: Complete the presentation forms, test locally with `dotnet build`, and push to GitHub.

- **Engine Developer**:
  - **Scope**: Core business logic, authentication service, credentials/token validation, user session state, and navigation workflow between Splash ➔ Login ➔ Dashboard.
  - **Freedom of Implementation**: Write simple, robust C# logic with **no unnecessary restrictions** — follow Clean Architecture principles (Core models and interfaces separated from UI).
  - **Collaborative AI Directive**: When you open Antigravity on your PC, Antigravity will first ask you: *"Auth validation, session state management aur navigation transition logic ka architecture kaisa design karna hai?"* You direct the logic, and Antigravity assists you in structuring the services and models.
  - **Branch**: Create and work on a feature branch (e.g. `feature/engine-auth-dashboard`).
  - **Deliverable**: Implement core logic and unit tests, verify with `dotnet test`, and push to GitHub.

- **PM/Tester**:
  - **Scope**: Verify acceptance criteria for Splash-to-Login transition, Login validation (valid/invalid credentials), and Dashboard landing.
  - **Deliverable**: Record verification evidence in [docs/TEST_REPORT.md](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TEST_REPORT.md).

### 🛠️ How to Start Work with Antigravity
1. Open this repository in Antigravity IDE on your PC.
2. Press `Ctrl + L` to open the Assistant chat.
3. Type: *"Aaj ka task start karna hai"* or *"Mujhe Splash, Login aur Dashboard par kaam karna hai"*.
4. Antigravity will recognize today's assignment, ask for your design/architecture input, help you create your feature branch, write and refine the code with you, run builds/tests, and guide you to push your changes to GitHub!

