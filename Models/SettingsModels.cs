using System;
using System.Collections.Generic;

namespace PROJECT_SBV1.Models
{
    /// <summary>
    /// Comprehensive strongly-typed settings configuration model for TEAM SOFT.
    /// Maps to persistent SQLite storage (AppSettings and BusinessProfile tables).
    /// </summary>
    public class AppSettingsModel
    {
        // ── 02. Appearance ──────────────────────────────────────────────────────────
        public string ThemeMode { get; set; } = "Light"; // "Light", "Dark", "System"
        public string AccentColor { get; set; } = "RoyalBlue"; // "RoyalBlue", "Indigo", "Emerald", "Crimson", "Amber"
        public string AccentColorName { get => AccentColor; set => AccentColor = value; }
        public string UiDensity { get; set; } = "Comfortable"; // "Comfortable", "Compact"
        public bool SidebarCollapsed { get; set; } = false;

        // ── 03. General ─────────────────────────────────────────────────────────────
        public string Language { get; set; } = "English (UK)"; // "English (UK)", "Hinglish (Hindi)", "Gujarati"
        public string CurrencySymbol { get; set; } = "₹ (INR)"; // "₹ (INR)", "$ (USD)", "€ (EUR)", "£ (GBP)"
        public string Currency { get => CurrencySymbol; set => CurrencySymbol = value; }
        public string DateFormat { get; set; } = "dd MMM yyyy"; // "dd MMM yyyy", "dd/MM/yyyy", "yyyy-MM-dd"
        public string TimeFormat { get; set; } = "12-Hour"; // "12-Hour", "24-Hour"
        public string NumberFormat { get; set; } = "Indian Lakhs"; // "Indian Lakhs", "International"
        public string DefaultLandingPage { get; set; } = "Overview"; // "Overview", "Sales", "Purchases", "Customers", "Products"
        public bool AutoCloseModals { get; set; } = true;
        public bool ConfirmDeletions { get; set; } = true;

        // ── 04. Security & Access ───────────────────────────────────────────────────
        public int SessionTimeoutMinutes { get; set; } = 30; // 15, 30, 60, 240, 0 (Never)
        public bool RequirePasswordForDelete { get; set; } = true;
        public bool TwoFactorPinEnabled { get; set; } = false;
        public string SecurityPin { get; set; } = "";

        // ── 06. Invoice & Sales ─────────────────────────────────────────────────────
        public string InvoicePrefix { get; set; } = "INV-";
        public int InvoiceStartNumber { get; set; } = 1001;
        public int NextInvoiceNumber { get => InvoiceStartNumber; set => InvoiceStartNumber = value; }
        public string DefaultPaymentTerms { get; set; } = "Net 15 Days";
        public int DefaultDueDays { get; set; } = 15;
        public decimal DefaultDiscountPercent { get; set; } = 0m;
        public bool TaxInclusiveDefault { get; set; } = false;
        public bool AutoDecrementStock { get; set; } = true;
        public bool AutoRestoreStockOnCancel { get; set; } = true;
        public bool AllowBackorders { get; set; } = false;

        // ── 07. Tax & GST ───────────────────────────────────────────────────────────
        public decimal DefaultTaxRate { get; set; } = 18m;
        public string DefaultHsnSac { get; set; } = "8471";
        public string DefaultGstScheme { get; set; } = "Intra-state (CGST 9% + SGST 9%)";
        public bool ReverseChargeMechanism { get; set; } = false;

        // ── 08. Payments ────────────────────────────────────────────────────────────
        public string DefaultPaymentMethod { get; set; } = "UPI";
        public string SupportedPaymentMethods { get; set; } = "Cash,UPI,Bank Transfer,Card,Cheque";
        public string PaymentInstructions { get; set; } = "Scan UPI QR code or transfer directly to HDFC Bank A/c 50200088912344 (IFSC: HDFC0001025). Mention invoice # in transaction notes.";
        public string LatePaymentInterest { get; set; } = "1.5% per month on overdue balances.";

        // ── 09. PDF & Print ─────────────────────────────────────────────────────────
        public string PaperSize { get; set; } = "A4"; // "A4", "Letter", "Thermal 80mm"
        public string Orientation { get; set; } = "Portrait"; // "Portrait", "Landscape"
        public string Margins { get; set; } = "Standard (15mm)";
        public string PrintTheme { get; set; } = "Professional Blue";
        public bool ShowLogoOnPrint { get; set; } = true;
        public bool ShowBankDetailsOnPrint { get; set; } = true;
        public bool ShowSignatoryOnPrint { get; set; } = true;
        public bool ShowPreviousDuesOnPrint { get; set; } = true;
        public bool ShowHsnColumn { get; set; } = true;
        public bool ShowTermsInFooter { get; set; } = true;
        public string PdfExportFolder { get; set; } = "";
        public bool AutoOpenPdf { get; set; } = true;

        // ── 10. Keyboard Shortcuts ──────────────────────────────────────────────────
        public bool GlobalHotkeysEnabled { get; set; } = true;

        // ── 11. Notifications ───────────────────────────────────────────────────────
        public bool LowStockAlertEnabled { get; set; } = true;
        public decimal CriticalStockThreshold { get; set; } = 5m;
        public bool InvoiceDueReminderEnabled { get; set; } = true;
        public int InvoiceDueReminderDays { get; set; } = 3;
        public bool OverduePaymentAlertEnabled { get; set; } = true;
        public bool HighOutstandingAlertEnabled { get; set; } = true;
        public decimal HighOutstandingThreshold { get; set; } = 50000m;
        public int BackupReminderDays { get; set; } = 7;
        public bool AudioChimeEnabled { get; set; } = true;
        public bool DesktopNotificationsEnabled { get; set; } = true;

        // ── 14. Integrations ────────────────────────────────────────────────────────
        public bool LocalApiEnabled { get; set; } = false;
        public int LocalApiPort { get; set; } = 8088;
        public string ApiKey { get; set; } = "ts_live_98a72b4c10ef82bc34d5";
    }

    /// <summary>
    /// Database backup history log item.
    /// </summary>
    public class BackupRecord
    {
        public long Id { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public string FileName => System.IO.Path.GetFileName(BackupPath);
        public long FileSizeBytes { get; set; } = 0;
        public string Status { get; set; } = "Success";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FormattedSize => FileSizeBytes switch
        {
            < 1024 => $"{FileSizeBytes} B",
            < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
            _ => $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB"
        };
    }

    /// <summary>
    /// System User model for Users & Permissions module.
    /// </summary>
    public class SystemUser
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Billing Operator"; // Vision, Administrator, Billing Operator, Accountant
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLoginAt { get; set; } = DateTime.Now;
    }
}
