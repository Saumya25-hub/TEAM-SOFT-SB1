# S TOM'S — Project SBV1

Welcome to **S TOM'S Project SBV1**, an AI-assisted, multi-developer Windows desktop application built with **.NET 8.0 Windows Forms**.

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
│   ├── TASKS.md                       # Structured task backlog & verification tracking
│   └── TEST_REPORT.md                 # Test execution logs & verification evidence
├── .team/
│   ├── profile.md                     # Current developer profile (Name, Role, Language, Task)
│   └── WORKLOG.md                     # Work session log (Git diffs, tests, progress)
├── .gitignore                         # Standard .NET / Visual Studio ignore rules
├── README.md                          # Repository overview & setup guide
├── TEAM_RULES.md                      # Team engineering standards, clean architecture, workflows
└── PROJECT-SBV1.slnx                  # Solution configuration
```

---

## 3. Quick Start & Build Instructions

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
