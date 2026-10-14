# TEAM SOFT — Master UI/UX Specification & Architecture Blueprint
## C# Windows Forms | .NET 8 | Visual Studio | Standalone Windows EXE

> **Non-Negotiable Architecture Directive**: This application is a native C# Windows Forms desktop application built for `.NET 8` (`net8.0-windows`) producing a standalone Windows `.exe`. No web frameworks, HTML wrappers, Electron, or web containers are used for this core Windows desktop application.

---

## 1. Executive Summary & Design Vision

TEAM SOFT is a high-performance, offline-capable inventory and billing management desktop software engineered for Windows. The application features:
- **Clean Vertical Sidebar** with collapsible/expandable states, badge counters, and active route markers.
- **Top App Header** with search, date filter pill, live notifications, user profile menu, and animated capsule Light/Dark theme toggle.
- **Spacious Dashboard Grid** utilizing `TableLayoutPanel` and responsive layout containers.
- **6 Modern Metric/KPI Cards** with custom rounded corners, subtle GDI+ borders, hover elevation, and percentage trends.
- **GDI+ Smooth Vector Visualizations**: Dual-axis monthly Sales & Purchases bar/line dynamics and multi-segment Category revenue donut charts.
- **High-contrast, Accessible Light & Dark Themes** managed centrally through `ThemeManager.cs`.
- **Local SQLite Data Persistence** via `Microsoft.Data.Sqlite` for zero-configuration, offline operation (`teamsoft.db`).

---

## 2. Project Directory Tree Structure

```text
d:\sb1 (TEAM SOFT Solution Root)
├── .team/
│   ├── profile.md                      # Local developer configuration & active role
│   └── WORKLOG.md                      # Daily audit trail and engineering session log
├── docs/
│   ├── MASTER_UI_UX_SPEC.md            # This master blueprint document
│   ├── TASKS.md                        # Project task backlog & sprint tracker
│   ├── TEAM_ROSTER.md                  # Team permissions & contributor registry
│   └── TEST_REPORT.md                  # Test execution and verification evidence
├── Forms/
│   ├── SplashForm.cs                   # Smooth animated startup branding & loading
│   ├── LoginForm.cs                    # Secure credentials login & session starter
│   ├── DashboardForm.cs                # Main container form hosting sidebar, topbar, & sub-forms
│   ├── CompanyMasterForm.cs            # Company profile, GSTIN, tax, invoice prefix config
│   ├── ItemMasterForm.cs               # Products, barcodes, SKU, stock levels, pricing
│   ├── PartyMasterForm.cs              # Customers & Suppliers unified ledger master
│   ├── SalesForm.cs                    # Sales POS invoice billing, barcode scanner, receipt print
│   ├── PurchaseForm.cs                 # Purchase invoice entry & stock-in receipt
│   ├── SalesReturnForm.cs              # Credit note & sales return workflow
│   ├── PurchaseReturnForm.cs           # Debit note & vendor return workflow
│   ├── StockReportForm.cs              # Stock valuation, low-stock warnings, movement log
│   ├── LedgerForm.cs                   # Statement of account & running balances
│   └── PaymentReceiptForm.cs           # Cash/Bank/UPI payment and receipt voucher entry
├── Controls/
│   ├── StatCardControl.cs              # Reusable KPI metric card with icon & delta trend
│   ├── NavItemControl.cs               # Sidebar navigation item with hover & active pill
│   ├── ThemeToggleControl.cs           # Smooth animated sun/moon capsule toggle button
│   ├── SalesDynamicsChart.cs           # Custom GDI+ double bar chart with smooth gradients
│   └── DonutChartControl.cs            # Custom GDI+ anti-aliased segmented donut chart
├── Services/
│   ├── ThemeManager.cs                 # Centralized color palette, fonts, and theme event bus
│   ├── DatabaseService.cs              # SQLite initialization, schema migrations, backup/restore
│   ├── BillingService.cs               # Tax calculations (GST/CGST/SGST), discounts, invoice numbering
│   ├── InventoryService.cs             # Real-time stock reservation, low stock detection, FIFO batching
│   └── ReportService.cs                # Ledger generation, P&L calculations, export to CSV/PDF
├── Data/
│   ├── DbConnection.cs                 # Thread-safe SQLite connection factory (`teamsoft.db`)
│   └── DatabaseHelper.cs               # Parameterized query execution, transaction wrapper
├── Models/
│   ├── Item.cs                         # Product entity: Id, SKU, Barcode, Name, Cost, SalePrice, Stock
│   ├── Party.cs                        # Party entity: Id, Name, Type (Customer/Supplier), GSTIN, Balance
│   ├── Invoice.cs                      # Invoice header: Id, InvoiceNo, Date, PartyId, Total, Paid, Type
│   ├── InvoiceDetail.cs                # Line items: Id, InvoiceId, ItemId, Qty, Rate, Discount, Tax, Total
│   └── Payment.cs                      # Payment record: Id, PartyId, Date, Amount, Mode, RefNo, Notes
├── Reports/
│   ├── InvoicePrint.cs                 # Thermal (80mm) and A4 professional invoice generator
│   ├── StockReport.cs                  # Detailed inventory summary report model
│   └── LedgerReport.cs                 # Debit/Credit party account statement
├── App.config                          # Application settings, default theme, connection strings
├── Program.cs                          # Application entry point with High-DPI & Splash orchestration
├── PROJECT-SBV1.csproj                 # .NET 8 WinForms SDK-style project file
└── teamsoft.db                         # Local SQLite database (created on first run)
```

---

## 3. Navigation Hierarchy & Menu Tree

```text
TEAM SOFT
├── 1. Dashboard (Overview KPI, Sales/Purchase Charts, Low Stock, Recent Invoices)
├── 2. Masters
│   ├── Company Master        (GSTIN, Address, Bank details, Invoice numbering rules)
│   ├── Item / Product Master (SKU, Barcode, Category, Unit, HSN/SAC, Min Stock Alert)
│   ├── Customer Master       (Name, Phone, Credit Limit, State, Balance)
│   ├── Supplier Master       (Vendor name, Contact, GSTIN, Opening Balance)
│   └── Unit / Category Master(Pieces, Kg, Box, Category hierarchy)
├── 3. Transactions
│   ├── Sales / Tax Invoice   (POS billing, Barcode scan, Cash/Bank/UPI, Print receipt)
│   ├── Purchase Entry        (Vendor bill, Stock-in, Purchase rates, Due date)
│   ├── Sales Return          (Credit note against invoice, Stock reversal)
│   └── Purchase Return       (Debit note against vendor bill)
├── 4. Accounts
│   ├── Payment Receipt       (Cash/UPI received from Customer, auto ledger credit)
│   ├── Vendor Payment        (Payment made to Supplier, bank reference)
│   ├── Customer Ledger       (Running balance, Statement of Account)
│   └── Supplier Ledger       (Payables ledger, Purchase history)
├── 5. Inventory
│   ├── Current Stock         (Live quantities, Valuation at Cost and Sale price)
│   ├── Stock In / Out        (Adjustments, Wastage, Manual correction vouchers)
│   └── Low Stock Alert       (Items below Reorder Point with reorder action)
├── 6. Reports
│   ├── Sales Report          (Daily, Monthly, Category-wise, Tax liability summary)
│   ├── Purchase Report       (Supplier purchases, HSN summary, Tax input credit)
│   ├── Stock Report          (Fast-moving items, Dead stock, Valuation summary)
│   └── Profit & Loss         (Gross revenue, COGS, Net margin)
└── 7. Settings
    ├── Company Profile       (Logo, Contact, Thermal printer selection)
    ├── User Management       (Admin, Cashier, Manager roles, Pin passwords)
    ├── Backup / Restore      (1-Click SQLite DB backup to .bak file and restore)
    └── Printer Settings      (Default A4 / Thermal 80mm selection)
```

---

## 4. Theme System Design Tokens (`ThemeManager.cs`)

All application components consume centralized color tokens defined in `Services/ThemeManager.cs`:

| Token | Light Theme | Dark Theme | Purpose |
| :--- | :--- | :--- | :--- |
| `WindowBackground` | `#F5F6FA` | `#111218` | Master window canvas background |
| `SidebarBackground`| `#FFFFFF` | `#191A23` | Sidebar navigation rail |
| `CardBackground`   | `#FFFFFF` | `#1E202B` | Statistic cards, panels, and data surfaces |
| `CardBorder`       | `#E7E9EF` | `#30323E` | Subtle hairline borders on cards & dividers |
| `TextPrimary`      | `#20232B` | `#F4F5F8` | High-contrast headings and prominent numbers |
| `TextSecondary`    | `#7B8190` | `#A1A5B3` | Subtitles, labels, timestamps, units |
| `PrimaryAccent`    | `#4169F5` | `#6687FF` | Brand buttons, active nav pill, highlights |
| `AccentHover`      | `#3255D1` | `#7C9BFF` | Mouse hover feedback on primary action buttons |
| `NavHover`         | `#F0F3FC` | `#252836` | Sidebar button hover background |
| `NavActive`        | `#E9EFFF` | `#2A3358` | Sidebar button active background |
| `StatusPositive`   | `#24A36A` | `#35C987` | Sales increase, completed status, in-stock |
| `StatusWarning`    | `#E7A52A` | `#F0B64D` | Low stock alert, partially paid invoice |
| `StatusNegative`   | `#E05252` | `#F16B72` | Overdue invoices, out of stock, debit alerts |
| `GridRowAlternate` | `#FAFAFC` | `#161822` | DataGridView alternate row striping |

---

## 5. Main Dashboard Layout Specification

The main dashboard is arranged using an adaptive `TableLayoutPanel`:

### Row 1: KPI Statistic Cards (6 Columns)
1. **Today's Sales**: Real-time sales total from midnight to current time.
2. **Monthly Sales**: Cumulative sales total for current calendar month.
3. **Total Receivables**: Sum of outstanding customer balances.
4. **Total Payables**: Sum of pending vendor liabilities.
5. **Total Products**: Count of active items in catalog.
6. **Low Stock Items**: Count of items where `StockQty <= ReorderLevel`.

### Row 2: Analytics & Trends (2 Columns: 65% / 35%)
- **Left (65%)**: `SalesDynamicsChart` — 12-month dual-bar chart comparing Sales vs Purchases with GDI+ rounded gradient bars and tooltips.
- **Right (35%)**: `DonutChartControl` — Product category sales breakdown donut with percentage legend and total revenue center label.

### Row 3: Live Operational Tables (2 Columns: 60% / 40%)
- **Left (60%)**: Recent Invoices DataGridView (Invoice No, Customer, Date, Payment Mode, Amount, Status).
- **Right (40%)**: Low Stock & Urgent Reorders list (Item Name, Current Stock, Reorder Level, Action button).

---

## 6. SQLite Database Schema (`teamsoft.db`)

```sql
-- 1. Company Profile
CREATE TABLE IF NOT EXISTS Companies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Address TEXT,
    Phone TEXT,
    Email TEXT,
    GSTIN TEXT,
    StateCode TEXT,
    InvoicePrefix TEXT DEFAULT 'TS-',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Master Items / Products
CREATE TABLE IF NOT EXISTS Items (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Barcode TEXT UNIQUE,
    SKU TEXT UNIQUE,
    Name TEXT NOT NULL,
    Category TEXT,
    Unit TEXT DEFAULT 'PCS',
    CostPrice REAL NOT NULL DEFAULT 0.0,
    SalePrice REAL NOT NULL DEFAULT 0.0,
    TaxRate REAL NOT NULL DEFAULT 0.0,
    StockQty REAL NOT NULL DEFAULT 0.0,
    ReorderLevel REAL NOT NULL DEFAULT 5.0,
    IsActive INTEGER DEFAULT 1,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 3. Parties (Customers & Suppliers)
CREATE TABLE IF NOT EXISTS Parties (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Type TEXT NOT NULL CHECK (Type IN ('Customer', 'Supplier')),
    Phone TEXT,
    Email TEXT,
    Address TEXT,
    GSTIN TEXT,
    OpeningBalance REAL DEFAULT 0.0,
    CurrentBalance REAL DEFAULT 0.0,
    CreditLimit REAL DEFAULT 0.0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 4. Invoices (Sales & Purchase Header)
CREATE TABLE IF NOT EXISTS Invoices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceNo TEXT UNIQUE NOT NULL,
    InvoiceType TEXT NOT NULL CHECK (InvoiceType IN ('Sales', 'Purchase', 'SalesReturn', 'PurchaseReturn')),
    Date DATETIME NOT NULL,
    PartyId INTEGER,
    SubTotal REAL NOT NULL DEFAULT 0.0,
    TaxAmount REAL NOT NULL DEFAULT 0.0,
    DiscountAmount REAL NOT NULL DEFAULT 0.0,
    GrandTotal REAL NOT NULL DEFAULT 0.0,
    PaidAmount REAL NOT NULL DEFAULT 0.0,
    BalanceAmount REAL NOT NULL DEFAULT 0.0,
    PaymentMode TEXT DEFAULT 'Cash',
    Status TEXT DEFAULT 'Paid',
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PartyId) REFERENCES Parties(Id)
);

-- 5. Invoice Line Items
CREATE TABLE IF NOT EXISTS InvoiceDetails (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    InvoiceId INTEGER NOT NULL,
    ItemId INTEGER NOT NULL,
    ItemName TEXT NOT NULL,
    Quantity REAL NOT NULL,
    Rate REAL NOT NULL,
    DiscountPercent REAL DEFAULT 0.0,
    TaxPercent REAL DEFAULT 0.0,
    Total REAL NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    FOREIGN KEY (ItemId) REFERENCES Items(Id)
);

-- 6. Payments & Financial Transactions
CREATE TABLE IF NOT EXISTS Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReceiptNo TEXT UNIQUE NOT NULL,
    PartyId INTEGER NOT NULL,
    InvoiceId INTEGER,
    Date DATETIME NOT NULL,
    Type TEXT NOT NULL CHECK (Type IN ('Receipt', 'Payment')),
    Amount REAL NOT NULL,
    PaymentMode TEXT NOT NULL DEFAULT 'Cash',
    ReferenceNo TEXT,
    Notes TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PartyId) REFERENCES Parties(Id)
);
```

---

## 7. Implementation Checklist

- [x] **Project Alignment**: Verified Windows Forms (`net8.0-windows`) on .NET 8.
- [ ] **Package Installation**: Add `Microsoft.Data.Sqlite` to `PROJECT-SBV1.csproj`.
- [ ] **Data Layer**: Implement `Data/DbConnection.cs` and `Data/DatabaseHelper.cs`.
- [ ] **Theme Architecture**: Implement `Services/ThemeManager.cs` with full Light and Dark palettes.
- [ ] **Custom Controls**: Implement `StatCardControl`, `SalesDynamicsChart`, and `DonutChartControl`.
- [ ] **Main Form**: Build `DashboardForm.cs` with Collapsible Sidebar, Header with Sun/Moon toggle, and Dynamic Content Container.
- [ ] **Business Forms**: Implement `SalesForm.cs` (POS Billing), `ItemMasterForm.cs`, and `PartyMasterForm.cs`.
- [ ] **Build Verification**: Run `dotnet build` with 0 warnings, 0 errors.
- [ ] **Execution & Test**: Run executable, toggle themes, perform test transaction, and verify persistent SQLite storage.
