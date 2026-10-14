# S TOM'S — Project Task Tracker

This file tracks all engineering and testing tasks. All team members must keep task entries up to date.

Allowed Statuses:
- `Not Started`
- `In Progress`
- `Blocked`
- `In Review`
- `Verified`
- `Done` *(Only marked after verified evidence is provided in TEST_REPORT.md)*

---

## Task Backlog

### [x] `TASK-BOOT-001`: Multi-Developer Bootstrap & Standards Setup
- **Owner**: Saumya Chaudhari (Vision & Project Owner) / AI Assistant
- **Role**: Vision & Workflow Setup
- **Description**: Initialize Git repository, configure .gitignore, establish team standards (`TEAM_RULES.md`), project documentation (`README.md`), task tracking (`docs/TASKS.md`), test logs (`docs/TEST_REPORT.md`), and developer workspace (`.team/`).
- **Acceptance Criteria**:
  1. `.git` initialized with `main` branch.
  2. `.gitignore` created covering .NET/Visual Studio artifacts.
  3. `TEAM_RULES.md` created with 9-step workflow, clean architecture, role boundaries, and security rules.
  4. Build succeeds with zero warnings and zero errors.
- **Status**: Verified
- **Verification Evidence**: `dotnet build` succeeded with 0 warnings, 0 errors. Git repository initialized. Verified via automated terminal checks. Approved by Vision.
- **Blockers**: None.

---

### [ ] `TASK-BOOT-002`: Clean Architecture Layering
- **Owner**: Engine Developer / UI/UX Developer
- **Role**: Engineering Architecture
- **Description**: Establish Clean Architecture folder structure (`src/`, `App`, `Core`, `Engine`, `Infrastructure`) separating presentation from engine logic without breaking existing WinForms initialization.
- **Acceptance Criteria**:
  1. UI views/controls separated from business logic.
  2. Core interfaces and models created without UI dependencies.
  3. Solution builds cleanly with 0 warnings and 0 errors.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Awaiting developer role onboarding & feature requirements.

---

### [ ] `TASK-BOOT-003`: Automated Testing Project Setup
- **Owner**: PM/Tester (Unassigned - Open for incoming PM/Tester)
- **Role**: PM/Tester
- **Description**: Set up an automated unit test project under `tests/` (e.g. xUnit) for validating domain validation and engine workflows.
- **Acceptance Criteria**:
  1. Test project created and referenced in solution.
  2. Sample domain tests run via `dotnet test` and pass.
  3. Test outputs logged in `docs/TEST_REPORT.md`.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Pending PM/Tester assignment and TASK-BOOT-002.

---

### [ ] `TASK-FEAT-001`: Initial Feature Development
- **Owner**: Assigned Developer
- **Role**: UI/UX Developer / Engine Developer
- **Description**: Implement the assigned project initial feature as specified in developer onboarding.
- **Acceptance Criteria**:
  1. Adheres to role boundaries (UI vs Engine).
  2. Follows 9-step daily workflow.
  3. Passes verification tests before review.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Awaiting developer onboarding task assignment.

---

### [ ] `TASK-TEST-001`: Temporary Onboarding Workflow Demonstration
- **Owner**: Temporary Developer (Test Role: PM/Tester)
- **Role**: PM/Tester
- **Description**: Verify feature branch isolation, task checklist update, and worklog synchronization on a test branch.
- **Acceptance Criteria**:
  1. Feature branch created and isolated from main without application code changes.
  2. Task checklist entry tracked with unique ID and In Progress status.
  3. Worklog entry synchronized in .team/WORKLOG.md for the onboarding session.
- **Status**: In Progress
- **Verification Evidence**: Branch test/onboarding-workflow verified isolated; task entry added to docs/TASKS.md; worklog entry synchronized in .team/WORKLOG.md.
- **Blockers**: None.

---

### [ ] `TASK-SPRINT-001`: UI Presentation — Splash, Login & Dashboard Forms
- **Owner**: Jenil Rana
- **Role**: UI/UX Developer
- **Description**: Design and implement WinForms presentation views, controls, and styling for Splash Screen, Login Screen, and Dashboard Screen. The developer guides the styling and Antigravity collaborates on the implementation without imposing direct code assumptions.
- **Acceptance Criteria**:
  1. Splash Form displays branding and initiates smooth startup transition.
  2. Login Form provides username/password controls, error labels, and submit buttons.
  3. Dashboard Form renders main application layout and placeholder panels.
  4. Completed on a feature branch (`feature/ui-splash-login-dashboard`), verified with `dotnet build`, and pushed to GitHub.
- **Status**: In Progress
- **Verification Evidence**: Feature branch `feature/ui-splash-login-dashboard` created and active.
- **Blockers**: None.

---

### [ ] `TASK-SPRINT-002`: Engine Logic — Auth, Session & Navigation Services
- **Owner**: Engine Developer
- **Role**: Engine Developer
- **Description**: Implement Clean Architecture domain models, authentication validation, user session state, and navigation workflow for Splash ➔ Login ➔ Dashboard. Developer has complete freedom to write simple, robust logic with no unnecessary restrictions.
- **Acceptance Criteria**:
  1. Core models/interfaces for User, Credentials, and SessionState created under Clean Architecture.
  2. Engine service validates credentials and manages user session.
  3. Navigation transition logic coordinates state between screens.
  4. Unit tests pass via `dotnet test`.
  5. Completed on a feature branch (e.g. `feature/engine-auth-dashboard`), verified with `dotnet build`, and pushed to GitHub.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: None.

---

### [ ] `TASK-SPRINT-003`: Verification & Test Cases — Splash, Login & Dashboard
- **Owner**: PM/Tester
- **Role**: PM/Tester
- **Description**: Define acceptance test scenarios and verify the integrated flow between Splash, Login, and Dashboard.
- **Acceptance Criteria**:
  1. Verify Splash-to-Login transition.
  2. Verify valid and invalid login handling.
  3. Verify Dashboard landing upon authentication.
  4. Test report logged in `docs/TEST_REPORT.md`.
- **Status**: Not Started
- **Verification Evidence**: None yet.
- **Blockers**: Pending completion of TASK-SPRINT-001 and TASK-SPRINT-002.

