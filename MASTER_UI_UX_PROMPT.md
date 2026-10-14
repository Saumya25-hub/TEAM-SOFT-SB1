# TEAM SOFT — MASTER UI/UX SPECIFICATION & BLUEPRINT
## C# Windows Forms | .NET 8 | Visual Studio | Standalone Windows EXE

Detailed technical specifications, schema, and layout hierarchy have been created at:
👉 [`docs/MASTER_UI_UX_SPEC.md`](file:///d:/sb1/docs/MASTER_UI_UX_SPEC.md)

---

### Key Highlights

- **Target Architecture**: Real native C# Windows Forms on .NET 8 (`net8.0-windows`), compiling to a standalone native `.exe`.
- **Database**: Local SQLite database (`teamsoft.db`) using `Microsoft.Data.Sqlite` for 100% offline desktop speed and reliability.
- **Theme Engine**: Centralized `ThemeManager.cs` supporting instant Light Theme (`#F5F6FA`) and Dark Theme (`#111218`) switching with persistence.
- **Visual Presentation**:
  - Clean collapsible vertical sidebar with complete Menu Tree (Dashboard, Masters, Transactions, Accounts, Inventory, Reports, Settings).
  - Modern Top Header with search, date pill, notification counter, and animated Sun/Moon capsule theme switch.
  - 6 Responsive KPI Metric Cards with real database figures.
  - Anti-aliased GDI+ double-bar Sales Dynamics chart and category breakdown Donut chart.
  - Live DataGridView tables for Recent Invoices and Low Stock alerts with custom dark/light theme styling.

Refer to [`docs/MASTER_UI_UX_SPEC.md`](file:///d:/sb1/docs/MASTER_UI_UX_SPEC.md) for full database DDL, folder structure, and component design tokens.
