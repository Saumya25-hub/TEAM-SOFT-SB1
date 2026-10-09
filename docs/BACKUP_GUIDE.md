# S TOM'S — Backup and Recovery Guide

This guide describes procedures for backing up and recovering repository assets, task tracking history, and developer workstation environments.

---

## 1. What Is Backed Up & Redundancy Strategy

| Component | Storage Location | Backup Mechanism | Redundancy Scope |
| :--- | :--- | :--- | :--- |
| **Source Code & Solution** | Git Repository | Git commits pushed to canonical GitHub remote (`origin/main`). | Distributed across developer clones + GitHub cloud. |
| **Tasks & Test Evidence** | `docs/TASKS.md`, `docs/TEST_REPORT.md` | Versioned Git Markdown files committed and pushed. | Redundant across all repository clones. |
| **Audit Logs** | `docs/AUDIT_LOG.md` | Versioned Git Markdown file. | Immutable commit history. |
| **Local Machine Profile** | `%LOCALAPPDATA%\STOMS\profile.json` | Machine-local only. Not in Git. | Recreated dynamically during developer onboarding. |

> [!CAUTION]
> **Zero Secrets in Backups**: Never copy passwords, GitHub Personal Access Tokens (PATs), SSH private keys, API keys, or machine-specific Windows DPAPI blobs into shared Git backups or external drives.

---

## 2. Disaster Recovery Scenarios

### Scenario A: Lost or Replaced Developer Workstation
When a developer receives a new computer or replaces their PC:
1. **Install Prerequisites**:
   - Install Git for Windows.
   - Install .NET 8.0 SDK or higher.
   - Install Antigravity IDE.
2. **Clone Canonical Repository**:
   ```powershell
   git clone https://github.com/Saumya25-hub/TEAM-SOFT-SB1.git
   cd TEAM-SOFT-SB1
   ```
3. **Re-Run Developer Onboarding**:
   - In Antigravity IDE, open the Agent sidebar (`Ctrl + L`) and type `hello`, or run `.\scripts\onboard.ps1`.
   - Re-enter preferred name, assigned role, language, and GitHub URL.
   - The onboarding engine will recreate `%LOCALAPPDATA%\STOMS\profile.json` on the new machine with appropriate Windows ACL permissions.
4. **Resume Task Work**:
   - Inspect `docs/TASKS.md` and check out the assigned feature branch (`git checkout feature/<assigned-task>`).

### Scenario B: Accidental Local Workspace Corruption
If uncommitted local files or build folders are corrupted:
1. **Inspect Status Without Overwriting**:
   ```powershell
   git status
   ```
2. **Clean Build Artifacts**:
   ```powershell
   dotnet clean
   git clean -ndX   # Dry-run to inspect ignored files to clean
   ```
3. **Restore Specific Files from Git (Requires Explicit User Approval)**:
   ```powershell
   git restore <specific-corrupted-file>
   ```
   *(Never run destructive commands like `git reset --hard` without verifying uncommitted work).*

---

## 3. Verifying a Successful Restore
After cloning or restoring from backup:
1. **Check Solution Build**:
   ```powershell
   dotnet build
   ```
   Must succeed with 0 errors and 0 warnings.
2. **Check Git Remote & Commit Head**:
   ```powershell
   git remote -v
   git log -n 1 --oneline
   ```
   Must match the latest authorized commit on GitHub `origin/main`.
3. **Verify Task Checklist Integrity**:
   Inspect `docs/TASKS.md` to confirm task statuses match the team's latest progress.
