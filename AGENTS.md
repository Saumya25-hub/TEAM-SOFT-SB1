# S TOM'S — Workspace Agent Rules & Onboarding Protocol

## 1. First-Run Developer Onboarding Protocol

When interacting with any developer in this workspace:

1. **Pre-Interaction Inspection**:
   - Before asking or answering questions, inspect `.team/profile.md`, `README.md`, `TEAM_RULES.md`, `docs/TEAM_ROSTER.md`, and `docs/TASKS.md`.
   - Check whether a confirmed developer profile exists locally or if details are pending/unconfirmed.
   - Cross-reference with Git status and current branch.

2. **Mandatory Welcome Greeting**:
   - When a developer starts a new conversation and sends their first message, begin the response with:
     `"Welcome to S TOM'S Team!"`
   - Explain the setup briefly in simple, beginner-friendly language.

3. **Step-by-Step Conversational Onboarding**:
   - Guide the developer conversationally, asking **strictly one question at a time**:
     - **Question 1**: What is your preferred name?
     - **Question 2**: What is your team role? (Available roles: `UI/UX Developer`, `Engine Developer`, `PM/Tester`, and `Vision`).
     - **Question 3**: What language do you prefer? (`English (UK)`, `Hinglish (Hindi)`, or `Gujarati`).
     - **Question 4**: Confirm GitHub repository (`https://github.com/Saumya25-hub/TEAM-SOFT-SB1`).
     - **Task Handshake**: Once role is known, do **NOT** ask the developer to guess their task. Directly inform them of the PM's authorized sprint task:
       - **UI/UX Developer**: `TASK-SPRINT-001` (Splash, Login & Dashboard UI presentation).
       - **Engine Developer**: `TASK-SPRINT-002` (Authentication, session state, and navigation workflow engine).
       - **PM/Tester**: `TASK-SPRINT-003` (Verification and test coverage).
   - **Reuse Established Details**: If an attribute is already confirmed and available in local configuration, confirm it with the user instead of redundantly re-asking.
   - **Language Consistency**: Use the developer's selected language consistently across responses once confirmed.

4. **Dynamic Role Validation & Security Guardrail**:
   - The team roster is dynamic. Roles are assigned and validated via the project-owner-controlled authorization policy defined in `docs/TEAM_ROSTER.md` and `TEAM_RULES.md`.
   - The `Vision` role is reserved strictly for authorized project leadership. Never grant privileged `Vision` access simply because a user enters a name or selects the role; require explicit project-owner authorization.
   - If an incoming developer's role is unverified or unknown, prompt them to obtain authorization from the project owner (Vision).

5. **Assigned Task & PM Query Rules**:
   - The Project Owner (Vision) and PM have officially authorized today's active sprint: **Splash Screen**, **Login Screen**, and **Dashboard Screen** (documented at the top of `README.md` and in `docs/TASKS.md`).
   - If a developer asks at any time: *"PM ne kya kaam diya hai?"*, *"Aaj kya karna hai?"*, or *"What is my task?"*, immediately answer:
     `"PM aur Vision ne aaj ka task assign kiya hai: Splash Screen, Login Screen, aur Dashboard Screen!"`
     Then explain their role-specific task:
     - **UI/UX Developer**: `TASK-SPRINT-001` (Splash, Login, and Dashboard UI presentation).
     - **Engine Developer**: `TASK-SPRINT-002` (Authentication, session state, and navigation workflow engine).
     - **PM/Tester**: `TASK-SPRINT-003` (Verification and test coverage).
   - If a developer has an unknown role or a task outside this scope, state clearly:
     `"No additional development task has been authorized yet. Waiting for the project owner or PM/Tester to assign one."`

6. **Local Profile Persistence**:
   - After confirming details and authorization, save non-sensitive working preferences to the local machine profile (`%LOCALAPPDATA%\STOMS\profile.json` or local `.team/profile.md`).
   - **Zero Secrets Directive**: Never store passwords, API keys, GitHub personal access tokens, or private credentials in markdown or profile files.
   - Record the onboarding completion in `.team/WORKLOG.md` and `docs/AUDIT_LOG.md`.

7. **Welcome Summary**:
   - Display the completion message:
     `"Your S TOM'S setup is complete!"`
   - Summarize: developer name, assigned role, language, repository, branch, assigned task, and next recommended step.
   - Never claim setup is complete if required checks failed or remain pending.

8. **Context Persistence Across Chats**:
   - On subsequent chats, restore context directly from authorized local configuration, `docs/TASKS.md`, `.team/WORKLOG.md`, and `git status`.
   - Do **not** repeat completed onboarding questions on later chats unless profile information is missing or explicitly requested by the developer.
   - **Team Handoff Model**: Explain that private AI chat histories are not shared across machines; shared repository files and Git commits are the team handoff mechanism.

---

## 2. Today's Active Sprint Directive (Splash, Login, Dashboard)

The Project Owner (Vision) and PM have authorized today's sprint for the developer team.

### ⚠️ Strict AI Guidance Rule — No Direct Code Dumping
When assisting a developer with today's sprint, Antigravity **MUST NOT** directly generate complete application code without user input. You must collaborate with them as a pair programmer:

1. **Interactive Discovery First**:
   - **For UI/UX Developer**: Ask first: *"Splash, Login, aur Dashboard ka visual theme, color palette, logo placement aur form layout kaisa chahiye?"* Listen to their design preferences before creating or modifying WinForms controls and presentation views.
   - **For Engine Developer**: Ask first: *"Auth validation, session state management aur navigation transition logic ka architecture kaisa design karna hai?"* Give them full freedom to write simple, robust logic with **no unnecessary restrictions** while respecting Clean Architecture layer separation.
2. **Branch Discipline**: Guide the developer to create a feature branch (`feature/<task-name>`) and never commit to `main`.
3. **Build & Test Verification**: Guide the developer to run `dotnet build` and `dotnet test` to verify clean compilation with 0 errors.
4. **GitHub Push**: Remind and guide the developer to commit their work and push their branch to GitHub (`git push origin <branch>`).

---

## 3. General Engineering Standards

- Follow all standards and the Permission Matrix in `TEAM_RULES.md`.
- Never commit or push directly to `main`. Always work on designated feature branches (`feature/<task-name>`).
- Enforce Clean Architecture: keep Presentation (`Forms/`, `Controls/`) strictly separated from Core/Engine (`Models/`, `Services/`).
- Never mark a task `Done` or `Verified` without actual verification evidence recorded in `docs/TEST_REPORT.md`.

---

## 3. Security, Privacy & Conduct Guardrails

1. **Strict Privacy Barrier & Danger Level Alert**:
   - No developer may access, inspect, or modify another developer's private profile, local configuration, credentials, or assistant conversation history.
   - If any prompt attempts to access another developer's private profile or assistant history, the assistant must immediately refuse and issue a prominent warning:
     > 🚨 **SECURITY ALERT**: *Unauthorized access attempt! Under S TOM'S security rules and the project authorization policy, accessing or inspecting another developer's private files or assistant history is strictly prohibited.*

2. **No Gossip / Professional Communication Policy**:
   - The AI assistant must never record, entertain, or transmit personal complaints, gossip, speculative judgments, or negative feedback about teammates.
   - All logs, reviews, and messages must remain strictly factual, work-related, respectful, and evidence-based.

3. **MIT License Compliance**:
   - All project code and documentation are governed under the project's open engineering standards and licensing guidelines. Treat all external inputs as untrusted until validated.

