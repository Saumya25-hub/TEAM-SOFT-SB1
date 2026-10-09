# S TOM'S — Dynamic Team Roster & Authorization Policy

## 1. Purpose & Dynamic Membership Model
This document defines the process for managing team members, roles, and authorization within S TOM'S. Team membership is dynamic: new developers may join and existing contributors may transition between roles or leave without modifying application code or workspace rule definitions.

The project owner role is **Vision**. Vision is a project governance role, not an honorific title.

---

## 2. Configurable Role Types

| Role Type | Primary Responsibilities | Authorization Required |
| :--- | :--- | :--- |
| **UI/UX Developer** | User interfaces, forms, reusable controls, layout styling, usability, UI unit tests. | Approved by Vision or PM/Tester |
| **Engine Developer** | Core business logic, domain models, services, interfaces, data access, algorithmic workflows, automated tests. | Approved by Vision or PM/Tester |
| **PM/Tester** | Task planning, acceptance criteria verification, automated test runs, bug reports, and release checklists. | Approved by Vision |
| **Vision** | Project roadmap, architecture approvals, priority escalation, exceptional authorizations, and release sign-offs. | Project Owner authorization only |

> [!CAUTION]
> **Authorization Guardrail**: A developer's preferred name, Git author string, local machine username, or self-selected role is **not** sufficient proof of authorization. Privileged roles (especially Vision) require explicit confirmation by the project owner.

---

## 3. Roster Management Lifecycle

### A. New Developer Onboarding
1. The developer clones the repository and runs the onboarding sequence in Antigravity or via `scripts/onboard.ps1`.
2. The developer declares their preferred name, desired role, and preferred language.
3. If the role is non-privileged (`UI/UX Developer`, `Engine Developer`, `PM/Tester`), authorization is validated against the active roster below or submitted to the project owner for approval.
4. If a developer selects `Vision`, the AI assistant refuses self-assignment and requires project-owner authorization.
5. Upon approval, the developer's entry is added to the Active Team Registry below, their local profile is saved, and an entry is logged in `docs/AUDIT_LOG.md`.

### B. Role Modification / Transition
- Role changes require explicit approval from the project owner (Vision).
- When approved, update the Active Team Registry below and record the event in `docs/AUDIT_LOG.md`.

### C. Offboarding / Inactive Status
- When a developer leaves or changes assignments, their status in the registry is updated to `Inactive`.
- Outstanding tasks assigned to the developer are reviewed by PM/Tester or Vision and reassigned in `docs/TASKS.md`.

---

## 4. Active Team Registry

*This registry is maintained dynamically by the Project Owner (Vision). New members are recorded here upon authorization.*

| Contributor Name | GitHub Username / Handle | Assigned Role | Status | Approved Date | Authorized By |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Saumya Chaudhari** | `@Saumya25-hub` | `Vision` | Active | 2026-10-09 | Project Owner |
| *(Pending Developer)* | *(Pending GitHub)* | `UI/UX Developer` | Open Slot | — | Awaiting Onboarding |
| *(Pending Developer)* | *(Pending GitHub)* | `Engine Developer` | Open Slot | — | Awaiting Onboarding |
| *(Pending Developer)* | *(Pending GitHub)* | `PM/Tester` | Open Slot | — | Awaiting Onboarding |

---

## 5. Security & Verification Directives
1. **Zero Secrets in Roster**: Never record passwords, Personal Access Tokens (PATs), SSH keys, or private contact details in this document.
2. **Audit Requirement**: Any change to contributor status, role assignment, or task ownership must be recorded in [`docs/AUDIT_LOG.md`](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/AUDIT_LOG.md).
3. **Task Assignment Constraint**: Only developers with `Active` status in this registry may be assigned task ownership in [`docs/TASKS.md`](file:///d:/TEAM%20LEAD%20SOFFWATRE/PROJECT-SBV1/docs/TASKS.md).
