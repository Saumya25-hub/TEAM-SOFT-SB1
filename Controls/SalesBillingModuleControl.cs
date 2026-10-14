using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Master Sales & Billing Module for TEAM SOFT Windows Desktop (.NET 8 WinForms).
    /// Houses Sales Dashboard, Create Invoice Engine, Invoices Directory, and Customer Management.
    /// Features:
    /// - 100% SQLite Database Driven (Zero fake/demo data)
    /// - Real-time Decimal Calculations for Subtotal, Discounts, CGST/SGST/IGST, and Balance
    /// - Vector GDI+ PrintPreview, Native Printing, and Standalone PDF Export
    /// - Partial & Full Payment Recording with Transaction Ledger
    /// - High-contrast Light/Dark mode responsiveness
    /// </summary>
    public class SalesBillingModuleControl : UserControl
    {
        // Navigation Sub-tabs
        private readonly Panel pnlNavHeader = new();
        private readonly Button btnTabDashboard = new();
        private readonly Button btnTabCreateInvoice = new();
        private readonly Button btnTabInvoices = new();
        private readonly Button btnTabCustomers = new();

        // Main View Deck
        private readonly Panel pnlViewDeck = new();
        private readonly Panel pnlDashboardView = new();
        private readonly Panel pnlCreateInvoiceView = new();
        private readonly Panel pnlInvoicesListView = new();
        private readonly Panel pnlCustomersView = new();

        private string _currentTab = "Dashboard";

        /// <summary>
        /// Fires when any invoice, payment, or customer record is created or modified.
        /// </summary>
        public event EventHandler? DataChanged;

        // ═══════════════════════════════════════════════════════════════════════════
        //  TAB 1: SALES DASHBOARD CONTROLS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly Panel pnlDashCardsHost = new();
        private readonly Panel pnlCardTotalSales = new();
        private readonly Panel pnlCardTotalInvoices = new();
        private readonly Panel pnlCardPaidInvoices = new();
        private readonly Panel pnlCardUnpaidInvoices = new();
        private readonly Panel pnlCardOutstanding = new();
        private readonly Panel pnlCardTodaySales = new();

        private readonly DataGridView dgvRecentSales = new();
        private readonly TeamPillButton btnQuickNewInv = new();
        private readonly TeamPillButton btnQuickViewInvs = new();
        private readonly TeamPillButton btnQuickCustomers = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  TAB 2: CREATE INVOICE CONTROLS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TextBox txtInvNumber = new();
        private readonly DateTimePicker dtpInvDate = new();
        private readonly DateTimePicker dtpDueDate = new();
        private readonly ComboBox cmbPaymentTerms = new();
        private readonly ComboBox cmbTaxScheme = new();

        private readonly ComboBox cmbCustomerSelect = new();
        private readonly TeamPillButton btnAddNewCustomer = new();
        private readonly TextBox txtCustCompany = new();
        private readonly TextBox txtCustPhone = new();
        private readonly TextBox txtCustEmail = new();
        private readonly TextBox txtCustAddress = new();
        private readonly TextBox txtCustGSTIN = new();

        // Line Item Inputs
        private readonly ComboBox cmbServiceName = new();
        private readonly TextBox txtItemDesc = new();
        private readonly NumericUpDown numItemQty = new();
        private readonly TextBox txtItemPrice = new();
        private readonly TextBox txtItemDiscPct = new();
        private readonly ComboBox cmbItemTaxPct = new();
        private readonly TeamPillButton btnAddLineItem = new();
        private readonly TeamPillButton btnRemoveLineItem = new();
        private readonly DataGridView dgvInvoiceItems = new();

        // Calculation Labels & Summary
        private readonly Label lblCalcSubtotal = new();
        private readonly Label lblCalcItemDisc = new();
        private readonly TextBox txtOverallDisc = new();
        private readonly Label lblCalcTaxable = new();
        private readonly Label lblCalcCgst = new();
        private readonly Label lblCalcSgst = new();
        private readonly Label lblCalcIgst = new();
        private readonly Label lblCalcGrandTotal = new();
        private readonly TextBox txtInitialPaymentAmt = new();
        private readonly ComboBox cmbInitialPaymentMode = new();
        private readonly TextBox txtInitialPaymentRef = new();
        private readonly Label lblCalcBalanceDue = new();

        private readonly TeamPillButton btnSaveInvoice = new();
        private readonly TeamPillButton btnSaveAndPrint = new();
        private readonly TeamPillButton btnPreviewInvoice = new();
        private readonly TeamPillButton btnExportPdf = new();
        private readonly TeamPillButton btnClearInvoice = new();

        private readonly List<InvoiceItem> _workingItems = new();
        private List<Customer> _allCustomers = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  TAB 3: INVOICES DIRECTORY CONTROLS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TextBox txtInvoiceSearch = new();
        private readonly ComboBox cmbStatusFilter = new();
        private readonly DateTimePicker dtpFilterFrom = new();
        private readonly DateTimePicker dtpFilterTo = new();
        private readonly CheckBox chkFilterDates = new();
        private readonly TeamPillButton btnFilterApply = new();
        private readonly TeamPillButton btnFilterReset = new();
        private readonly DataGridView dgvAllInvoices = new();
        private readonly TeamPillButton btnGridOpenInvoice = new();
        private readonly TeamPillButton btnGridRecordPay = new();
        private readonly TeamPillButton btnGridPrint = new();
        private readonly TeamPillButton btnGridExportPdf = new();
        private readonly TeamPillButton btnGridCancel = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  TAB 4: CUSTOMERS DIRECTORY CONTROLS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TextBox txtCustDirSearch = new();
        private readonly TeamPillButton btnCustDirAdd = new();
        private readonly TeamPillButton btnCustDirEdit = new();
        private readonly DataGridView dgvAllCustomers = new();

        public SalesBillingModuleControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            Dock = DockStyle.Fill;

            BuildModuleLayout();
            ApplyTheme();
            SwitchTab("Dashboard");

            ThemeManager.ThemeChanged += ApplyTheme;
        }

        private void BuildModuleLayout()
        {
            Controls.Clear();

            // 1. Top Sub-Navigation Bar
            pnlNavHeader.Dock = DockStyle.Top;
            pnlNavHeader.Height = 44;
            pnlNavHeader.Padding = new Padding(16, 4, 16, 4);

            SetupTabButton(btnTabDashboard, "📊 Sales Overview", "Dashboard");
            SetupTabButton(btnTabCreateInvoice, "➕ Create Invoice", "CreateInvoice");
            SetupTabButton(btnTabInvoices, "📋 Invoices Directory", "Invoices");
            SetupTabButton(btnTabCustomers, "👥 Customers", "Customers");

            pnlNavHeader.Controls.AddRange(new Control[] {
                btnTabCustomers, btnTabInvoices, btnTabCreateInvoice, btnTabDashboard
            });

            // 2. Main View Deck
            pnlViewDeck.Dock = DockStyle.Fill;
            pnlViewDeck.Controls.AddRange(new Control[] {
                pnlDashboardView, pnlCreateInvoiceView, pnlInvoicesListView, pnlCustomersView
            });

            Controls.Add(pnlViewDeck);
            Controls.Add(pnlNavHeader);

            BuildDashboardView();
            BuildCreateInvoiceView();
            BuildInvoicesListView();
            BuildCustomersView();
        }

        private void SetupTabButton(Button btn, string text, string tabKey)
        {
            btn.Text = text;
            btn.Dock = DockStyle.Left;
            btn.Width = 150;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Tag = tabKey;
            btn.Click += (s, e) => SwitchTab(tabKey);
        }

        public void SwitchTab(string tabKey)
        {
            _currentTab = tabKey;

            pnlDashboardView.Visible = (tabKey == "Dashboard");
            pnlCreateInvoiceView.Visible = (tabKey == "CreateInvoice");
            pnlInvoicesListView.Visible = (tabKey == "Invoices");
            pnlCustomersView.Visible = (tabKey == "Customers");

            UpdateTabButtonStyles();

            if (tabKey == "Dashboard") LoadDashboardData();
            else if (tabKey == "CreateInvoice") PrepareNewInvoiceForm();
            else if (tabKey == "Invoices") RefreshInvoicesGrid();
            else if (tabKey == "Customers") RefreshCustomersGrid();
        }

        private void UpdateTabButtonStyles()
        {
            bool isDark = ThemeManager.IsDarkMode;
            Color activeBg = ThemeManager.PrimaryAccent;
            Color inactiveBg = Color.Transparent;
            Color activeText = Color.White;
            Color inactiveText = ThemeManager.TextSecondary;

            foreach (Control c in pnlNavHeader.Controls)
            {
                if (c is Button btn && btn.Tag is string key)
                {
                    bool isActive = (key == _currentTab);
                    btn.BackColor = isActive ? activeBg : inactiveBg;
                    btn.ForeColor = isActive ? activeText : inactiveText;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 1: SALES DASHBOARD VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildDashboardView()
        {
            pnlDashboardView.Dock = DockStyle.Fill;
            pnlDashboardView.AutoScroll = true;
            pnlDashboardView.Padding = new Padding(16);

            // Row 1: KPI Metrics Cards
            pnlDashCardsHost.Dock = DockStyle.Top;
            pnlDashCardsHost.Height = 90;
            pnlDashCardsHost.Padding = new Padding(0, 0, 0, 10);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1
            };
            for (int i = 0; i < 6; i++)
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66f));

            tlp.Controls.Add(pnlCardTotalSales, 0, 0);
            tlp.Controls.Add(pnlCardTotalInvoices, 1, 0);
            tlp.Controls.Add(pnlCardPaidInvoices, 2, 0);
            tlp.Controls.Add(pnlCardUnpaidInvoices, 3, 0);
            tlp.Controls.Add(pnlCardOutstanding, 4, 0);
            tlp.Controls.Add(pnlCardTodaySales, 5, 0);

            pnlDashCardsHost.Controls.Add(tlp);

            // Row 2: Quick Action Bar
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 4, 0, 8)
            };

            btnQuickNewInv.Text = "Create New Invoice";
            btnQuickNewInv.Icon = ReiconRenderer.IconType.Plus;
            btnQuickNewInv.FillColor = ThemeManager.PrimaryAccent;
            btnQuickNewInv.SetBounds(0, 4, 180, 38);
            btnQuickNewInv.Click += (s, e) => SwitchTab("CreateInvoice");

            btnQuickViewInvs.Text = "View Invoices Directory";
            btnQuickViewInvs.Icon = ReiconRenderer.IconType.Receipt;
            btnQuickViewInvs.FillColor = Color.FromArgb(79, 70, 229);
            btnQuickViewInvs.SetBounds(190, 4, 190, 38);
            btnQuickViewInvs.Click += (s, e) => SwitchTab("Invoices");

            btnQuickCustomers.Text = "Manage Customers";
            btnQuickCustomers.Icon = ReiconRenderer.IconType.Customers;
            btnQuickCustomers.FillColor = Color.FromArgb(16, 185, 129);
            btnQuickCustomers.SetBounds(390, 4, 180, 38);
            btnQuickCustomers.Click += (s, e) => SwitchTab("Customers");

            pnlActions.Controls.AddRange(new Control[] { btnQuickNewInv, btnQuickViewInvs, btnQuickCustomers });

            // Row 3: Recent Sales Header & Grid
            var lblRecentHeader = new Label
            {
                Text = "Recent Sales Invoices (Live Database)",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 32
            };

            dgvRecentSales.Dock = DockStyle.Fill;
            dgvRecentSales.BorderStyle = BorderStyle.None;
            dgvRecentSales.RowHeadersVisible = false;
            dgvRecentSales.AllowUserToAddRows = false;
            dgvRecentSales.ReadOnly = true;
            dgvRecentSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecentSales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvRecentSales.Columns.Add("InvNum", "Invoice #");
            dgvRecentSales.Columns.Add("Customer", "Customer Name");
            dgvRecentSales.Columns.Add("Date", "Invoice Date");
            dgvRecentSales.Columns.Add("Due", "Due Date");
            dgvRecentSales.Columns.Add("Total", "Grand Total (₹)");
            dgvRecentSales.Columns.Add("Paid", "Amount Paid (₹)");
            dgvRecentSales.Columns.Add("Balance", "Balance Due (₹)");
            dgvRecentSales.Columns.Add("Status", "Status");

            dgvRecentSales.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvRecentSales.Rows[e.RowIndex].Tag is Invoice inv)
                {
                    using var view = new InvoiceDetailsForm(inv);
                    view.ShowDialog(this);
                    LoadDashboardData();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            };

            pnlDashboardView.Controls.Add(dgvRecentSales);
            pnlDashboardView.Controls.Add(lblRecentHeader);
            pnlDashboardView.Controls.Add(pnlActions);
            pnlDashboardView.Controls.Add(pnlDashCardsHost);
        }

        private void LoadDashboardData()
        {
            var m = DatabaseService.GetSalesDashboardMetrics();

            ConfigureKpiCard(pnlCardTotalSales, "Total Sales", $"₹{m.TotalSales:N2}", "Lifetime Revenue", ThemeManager.PrimaryAccent);
            ConfigureKpiCard(pnlCardTotalInvoices, "Total Invoices", $"{m.TotalInvoicesCount}", "Recorded Invoices", ThemeManager.TextPrimary);
            ConfigureKpiCard(pnlCardPaidInvoices, "Paid Invoices", $"{m.PaidInvoicesCount}", "Fully Cleared", ThemeManager.StatusPositive);
            ConfigureKpiCard(pnlCardUnpaidInvoices, "Unpaid Invoices", $"{m.UnpaidInvoicesCount}", $"{m.OverdueInvoicesCount} Overdue", ThemeManager.StatusWarning);
            ConfigureKpiCard(pnlCardOutstanding, "Outstanding Due", $"₹{m.OutstandingBalance:N2}", "Receivables", ThemeManager.StatusNegative);
            ConfigureKpiCard(pnlCardTodaySales, "Today's Sales", $"₹{m.TodaySales:N2}", $"Month: ₹{m.ThisMonthSales:N2}", ThemeManager.PrimaryAccent);

            dgvRecentSales.Rows.Clear();
            foreach (var inv in m.RecentInvoices)
            {
                int rIdx = dgvRecentSales.Rows.Add(
                    inv.InvoiceNumber, inv.CustomerName, inv.InvoiceDate.ToString("dd MMM yyyy"),
                    inv.DueDate.ToString("dd MMM yyyy"), $"₹{inv.GrandTotal:N2}", $"₹{inv.AmountPaid:N2}",
                    $"₹{inv.BalanceDue:N2}", inv.Status
                );
                dgvRecentSales.Rows[rIdx].Tag = inv;
            }
        }

        private void ConfigureKpiCard(Panel pnl, string title, string val, string sub, Color accent)
        {
            pnl.Controls.Clear();
            pnl.Dock = DockStyle.Fill;
            pnl.Margin = new Padding(3);
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                int w = pnl.Width;
                int h = pnl.Height;
                if (w <= 0 || h <= 0) return;

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
                using var bg = new SolidBrush(ThemeManager.CardBackground);
                g.FillPath(bg, path);

                using var pen = new Pen(ThemeManager.BorderHairline, 1.2f);
                g.DrawPath(pen, path);

                using var bTitle = new SolidBrush(ThemeManager.TextSecondary);
                using var bVal = new SolidBrush(ThemeManager.TextPrimary);
                using var bSub = new SolidBrush(accent);

                using var fTitle = new Font("Segoe UI", 8f);
                using var fVal = new Font("Segoe UI", 12f, FontStyle.Bold);
                using var fSub = new Font("Segoe UI", 7.5f);

                g.DrawString(title, fTitle, bTitle, 10, 8);
                g.DrawString(val, fVal, bVal, 10, 26);
                g.DrawString(sub, fSub, bSub, 10, 52);
            };
            pnl.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 2: CREATE INVOICE VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCreateInvoiceView()
        {
            pnlCreateInvoiceView.Dock = DockStyle.Fill;
            pnlCreateInvoiceView.AutoScroll = true;
            pnlCreateInvoiceView.Padding = new Padding(16);

            int y = 8;
            int col1 = 0;
            int col2 = 270;
            int col3 = 540;
            int col4 = 780;

            // Section 1: Top Invoice Meta
            var lblSec1 = new Label
            {
                Text = "1. INVOICE META & TAX SCHEME",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Bounds = new Rectangle(col1, y, 500, 24)
            };
            pnlCreateInvoiceView.Controls.Add(lblSec1);
            y += 28;

            AddLabelAndControl(pnlCreateInvoiceView, "Invoice Number (Auto Unique)", txtInvNumber, col1, y, 240);
            txtInvNumber.ReadOnly = true;

            AddLabelAndControl(pnlCreateInvoiceView, "Invoice Date", dtpInvDate, col2, y, 240);
            dtpInvDate.Format = DateTimePickerFormat.Short;

            AddLabelAndControl(pnlCreateInvoiceView, "Payment Due Date", dtpDueDate, col3, y, 210);
            dtpDueDate.Format = DateTimePickerFormat.Short;

            cmbPaymentTerms.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentTerms.Items.AddRange(new object[] { "Due on Receipt", "Net 15", "Net 30", "Net 60" });
            cmbPaymentTerms.SelectedIndex = 1;
            AddLabelAndControl(pnlCreateInvoiceView, "Payment Terms", cmbPaymentTerms, col4, y, 170);

            y += 56;

            cmbTaxScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTaxScheme.Items.AddRange(new object[] { "Intra-State GST (CGST + SGST)", "Inter-State GST (IGST)", "Zero Tax / SEZ / Export" });
            cmbTaxScheme.SelectedIndex = 0;
            cmbTaxScheme.SelectedIndexChanged += (s, e) => RecalculateBill();
            AddLabelAndControl(pnlCreateInvoiceView, "GST Tax Scheme", cmbTaxScheme, col1, y, 320);

            y += 64;

            // Section 2: Customer Details
            var lblSec2 = new Label
            {
                Text = "2. CUSTOMER & BILLING INFORMATION",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Bounds = new Rectangle(col1, y, 500, 24)
            };
            pnlCreateInvoiceView.Controls.Add(lblSec2);
            y += 28;

            cmbCustomerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCustomerSelect.SelectedIndexChanged += (s, e) => OnCustomerSelected();
            AddLabelAndControl(pnlCreateInvoiceView, "Select Existing Customer *", cmbCustomerSelect, col1, y, 320);

            btnAddNewCustomer.Text = "+ New Customer";
            btnAddNewCustomer.Icon = ReiconRenderer.IconType.Plus;
            btnAddNewCustomer.FillColor = ThemeManager.PrimaryAccent;
            btnAddNewCustomer.SetBounds(col1 + 330, y + 20, 140, 30);
            btnAddNewCustomer.Click += (s, e) =>
            {
                using var dlg = new CustomerDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadCustomersIntoDropdown(dlg.SavedCustomer.Id);
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            pnlCreateInvoiceView.Controls.Add(btnAddNewCustomer);

            AddLabelAndControl(pnlCreateInvoiceView, "Company / Org Name", txtCustCompany, col3, y, 210);
            AddLabelAndControl(pnlCreateInvoiceView, "Phone Number *", txtCustPhone, col4, y, 170);

            y += 56;
            AddLabelAndControl(pnlCreateInvoiceView, "Email Address", txtCustEmail, col1, y, 260);
            AddLabelAndControl(pnlCreateInvoiceView, "Billing Address", txtCustAddress, col2, y, 400);
            AddLabelAndControl(pnlCreateInvoiceView, "GSTIN Number", txtCustGSTIN, col4, y, 170);

            y += 66;

            // Section 3: Line Items (Services & Subscriptions)
            var lblSec3 = new Label
            {
                Text = "3. SERVICES, SUBSCRIPTIONS & LINE ITEMS",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Bounds = new Rectangle(col1, y, 500, 24)
            };
            pnlCreateInvoiceView.Controls.Add(lblSec3);
            y += 28;

            // Service Name Presets + Custom
            cmbServiceName.Items.AddRange(new object[] {
                "Enterprise Software License (Annual)",
                "Cloud SaaS Subscription (Monthly)",
                "Custom Software Architecture & Dev",
                "Digital Signature Certificate (Class 3 DSC)",
                "Brand Identity & UI/UX Design System",
                "Cloud Hosting & Managed DevOps",
                "Database Migration & Security Audit",
                "Annual Maintenance Contract (AMC)"
            });
            AddLabelAndControl(pnlCreateInvoiceView, "Service / Item Name *", cmbServiceName, col1, y, 260);

            txtItemDesc.PlaceholderText = "Scope of work / spec details";
            AddLabelAndControl(pnlCreateInvoiceView, "Description", txtItemDesc, col2, y, 220);

            numItemQty.Minimum = 1;
            numItemQty.Maximum = 10000;
            numItemQty.Value = 1;
            AddLabelAndControl(pnlCreateInvoiceView, "Qty *", numItemQty, 500, y, 60);

            txtItemPrice.PlaceholderText = "0.00";
            AddLabelAndControl(pnlCreateInvoiceView, "Unit Price (₹) *", txtItemPrice, 570, y, 100);

            txtItemDiscPct.PlaceholderText = "0%";
            AddLabelAndControl(pnlCreateInvoiceView, "Disc %", txtItemDiscPct, 680, y, 60);

            cmbItemTaxPct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbItemTaxPct.Items.AddRange(new object[] { "0%", "5%", "12%", "18%", "28%" });
            cmbItemTaxPct.SelectedItem = "18%";
            AddLabelAndControl(pnlCreateInvoiceView, "GST %", cmbItemTaxPct, 750, y, 70);

            btnAddLineItem.Text = "+ Add Item";
            btnAddLineItem.Icon = ReiconRenderer.IconType.Plus;
            btnAddLineItem.FillColor = ThemeManager.StatusPositive;
            btnAddLineItem.SetBounds(830, y + 20, 110, 30);
            btnAddLineItem.Click += (s, e) => AddItemToInvoice();
            pnlCreateInvoiceView.Controls.Add(btnAddLineItem);

            y += 60;

            // Invoice Items DataGridView
            dgvInvoiceItems.SetBounds(col1, y, 950, 160);
            dgvInvoiceItems.BorderStyle = BorderStyle.None;
            dgvInvoiceItems.RowHeadersVisible = false;
            dgvInvoiceItems.AllowUserToAddRows = false;
            dgvInvoiceItems.ReadOnly = true;
            dgvInvoiceItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoiceItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvInvoiceItems.Columns.Add("Service", "Service / Product");
            dgvInvoiceItems.Columns.Add("Desc", "Description");
            dgvInvoiceItems.Columns.Add("Qty", "Qty");
            dgvInvoiceItems.Columns.Add("Price", "Rate (₹)");
            dgvInvoiceItems.Columns.Add("Disc", "Disc (₹)");
            dgvInvoiceItems.Columns.Add("Tax", "GST %");
            dgvInvoiceItems.Columns.Add("Total", "Line Total (₹)");

            btnRemoveLineItem.Text = "Remove Selected Item";
            btnRemoveLineItem.Icon = ReiconRenderer.IconType.Close;
            btnRemoveLineItem.FillColor = ThemeManager.StatusNegative;
            btnRemoveLineItem.SetBounds(col1, y + 165, 160, 28);
            btnRemoveLineItem.Click += (s, e) => RemoveSelectedItem();

            pnlCreateInvoiceView.Controls.Add(dgvInvoiceItems);
            pnlCreateInvoiceView.Controls.Add(btnRemoveLineItem);

            y += 205;

            // Section 4: Live Arithmetic Summary & Initial Payment Block
            var pnlSummaryBox = new Panel
            {
                Bounds = new Rectangle(col1, y, 950, 170),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            int sumY = 12;
            void AddSummaryLabel(string title, Label valLbl, int rx, int ry, bool bold = false)
            {
                var lblT = new Label { Text = title, Font = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular), Bounds = new Rectangle(rx, ry, 130, 20), AutoSize = false };
                valLbl.SetBounds(rx + 130, ry, 120, 20);
                valLbl.Font = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular);
                valLbl.TextAlign = ContentAlignment.TopRight;
                valLbl.Text = "₹0.00";
                pnlSummaryBox.Controls.Add(lblT);
                pnlSummaryBox.Controls.Add(valLbl);
            }

            // Left side: Payment recording on save
            var lblPayTitle = new Label { Text = "RECORD PAYMENT ON INVOICE CREATION (OPTIONAL)", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Bounds = new Rectangle(16, sumY, 400, 20) };
            pnlSummaryBox.Controls.Add(lblPayTitle);

            AddLabelAndControl(pnlSummaryBox, "Amount Paid Now (₹)", txtInitialPaymentAmt, 16, sumY + 24, 150);
            txtInitialPaymentAmt.Text = "0.00";
            txtInitialPaymentAmt.TextChanged += (s, e) => RecalculateBill();

            cmbInitialPaymentMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInitialPaymentMode.Items.AddRange(new object[] { "Bank Transfer", "UPI", "Cash", "Card", "Other" });
            cmbInitialPaymentMode.SelectedIndex = 0;
            AddLabelAndControl(pnlSummaryBox, "Payment Mode", cmbInitialPaymentMode, 180, sumY + 24, 130);

            txtInitialPaymentRef.PlaceholderText = "Cheque / UTR / Txn #";
            AddLabelAndControl(pnlSummaryBox, "Reference Number", txtInitialPaymentRef, 324, sumY + 24, 150);

            // Right side: Financial breakdowns
            int rX = 660;
            AddSummaryLabel("Subtotal:", lblCalcSubtotal, rX, sumY);
            AddSummaryLabel("Item Discounts:", lblCalcItemDisc, rX, sumY + 20);

            var lblOvr = new Label { Text = "Overall Disc (₹):", Font = new Font("Segoe UI", 9f), Bounds = new Rectangle(rX, sumY + 42, 130, 20) };
            txtOverallDisc.SetBounds(rX + 130, sumY + 40, 110, 22);
            txtOverallDisc.Text = "0.00";
            txtOverallDisc.TextAlign = HorizontalAlignment.Right;
            txtOverallDisc.TextChanged += (s, e) => RecalculateBill();
            pnlSummaryBox.Controls.Add(lblOvr);
            pnlSummaryBox.Controls.Add(txtOverallDisc);

            AddSummaryLabel("Taxable Amount:", lblCalcTaxable, rX, sumY + 66);
            AddSummaryLabel("CGST Amount:", lblCalcCgst, rX, sumY + 86);
            AddSummaryLabel("SGST Amount:", lblCalcSgst, rX, sumY + 106);
            AddSummaryLabel("IGST Amount:", lblCalcIgst, rX, sumY + 106);
            AddSummaryLabel("GRAND TOTAL:", lblCalcGrandTotal, rX, sumY + 128, true);
            lblCalcGrandTotal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCalcGrandTotal.ForeColor = ThemeManager.PrimaryAccent;

            AddSummaryLabel("BALANCE DUE:", lblCalcBalanceDue, rX, sumY + 148, true);
            lblCalcBalanceDue.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCalcBalanceDue.ForeColor = ThemeManager.StatusNegative;

            pnlCreateInvoiceView.Controls.Add(pnlSummaryBox);

            y += 185;

            // Section 5: Bottom Working Buttons
            btnSaveInvoice.Text = "💾 Save Invoice";
            btnSaveInvoice.Icon = null;
            btnSaveInvoice.FillColor = ThemeManager.PrimaryAccent;
            btnSaveInvoice.SetBounds(col1, y, 140, 40);
            btnSaveInvoice.Click += (s, e) => SaveCurrentInvoice(andPrint: false);

            btnSaveAndPrint.Text = "🖨️ Save & Print";
            btnSaveAndPrint.Icon = null;
            btnSaveAndPrint.FillColor = ThemeManager.StatusPositive;
            btnSaveAndPrint.SetBounds(col1 + 150, y, 140, 40);
            btnSaveAndPrint.Click += (s, e) => SaveCurrentInvoice(andPrint: true);

            btnPreviewInvoice.Text = "👁️ Preview";
            btnPreviewInvoice.Icon = null;
            btnPreviewInvoice.FillColor = Color.FromArgb(79, 70, 229);
            btnPreviewInvoice.SetBounds(col1 + 300, y, 120, 40);
            btnPreviewInvoice.Click += (s, e) => PreviewCurrentInvoice();

            btnExportPdf.Text = "📄 Export PDF";
            btnExportPdf.Icon = null;
            btnExportPdf.FillColor = Color.FromArgb(37, 99, 235);
            btnExportPdf.SetBounds(col1 + 430, y, 130, 40);
            btnExportPdf.Click += (s, e) => ExportCurrentInvoicePdf();

            btnClearInvoice.Text = "Clear Form";
            btnClearInvoice.Icon = null;
            btnClearInvoice.FillColor = Color.FromArgb(148, 163, 184);
            btnClearInvoice.SetBounds(col1 + 570, y, 110, 40);
            btnClearInvoice.Click += (s, e) => PrepareNewInvoiceForm();

            pnlCreateInvoiceView.Controls.AddRange(new Control[] {
                btnSaveInvoice, btnSaveAndPrint, btnPreviewInvoice, btnExportPdf, btnClearInvoice
            });
        }

        private void AddLabelAndControl(Panel parent, string labelText, Control ctrl, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Bounds = new Rectangle(x, y, width, 18),
                ForeColor = ThemeManager.TextSecondary
            };
            ctrl.SetBounds(x, y + 20, width, 28);
            ctrl.Font = new Font("Segoe UI", 9.5f);
            parent.Controls.Add(lbl);
            parent.Controls.Add(ctrl);
        }

        private void PrepareNewInvoiceForm()
        {
            txtInvNumber.Text = DatabaseService.GetNextInvoiceNumber();
            dtpInvDate.Value = DateTime.Today;
            dtpDueDate.Value = DateTime.Today.AddDays(15);
            cmbPaymentTerms.SelectedIndex = 1;
            cmbTaxScheme.SelectedIndex = 0;

            LoadCustomersIntoDropdown();

            _workingItems.Clear();
            dgvInvoiceItems.Rows.Clear();

            txtItemDesc.Clear();
            numItemQty.Value = 1;
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
            cmbItemTaxPct.SelectedItem = "18%";

            txtOverallDisc.Text = "0.00";
            txtInitialPaymentAmt.Text = "0.00";
            txtInitialPaymentRef.Clear();

            RecalculateBill();
        }

        private void LoadCustomersIntoDropdown(long selectCustomerId = 0)
        {
            _allCustomers = DatabaseService.GetCustomers();
            cmbCustomerSelect.Items.Clear();
            cmbCustomerSelect.Items.Add("-- Select Customer --");

            int selectIndex = 0;
            for (int i = 0; i < _allCustomers.Count; i++)
            {
                var c = _allCustomers[i];
                cmbCustomerSelect.Items.Add(c.DisplayName);
                if (c.Id == selectCustomerId)
                {
                    selectIndex = i + 1;
                }
            }
            cmbCustomerSelect.SelectedIndex = selectIndex;
        }

        private void OnCustomerSelected()
        {
            int idx = cmbCustomerSelect.SelectedIndex;
            if (idx > 0 && idx <= _allCustomers.Count)
            {
                var c = _allCustomers[idx - 1];
                txtCustCompany.Text = c.CompanyName;
                txtCustPhone.Text = c.Phone;
                txtCustEmail.Text = c.Email;
                txtCustAddress.Text = c.BillingAddress;
                txtCustGSTIN.Text = c.GSTIN;
            }
            else
            {
                txtCustCompany.Clear();
                txtCustPhone.Clear();
                txtCustEmail.Clear();
                txtCustAddress.Clear();
                txtCustGSTIN.Clear();
            }
        }

        private void AddItemToInvoice()
        {
            string service = cmbServiceName.Text.Trim();
            if (string.IsNullOrWhiteSpace(service))
            {
                MessageBox.Show("Please enter or select a service name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbServiceName.Focus();
                return;
            }

            if (!decimal.TryParse(txtItemPrice.Text.Trim(), out decimal rate) || rate <= 0)
            {
                MessageBox.Show("Please enter a valid unit rate / price.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemPrice.Focus();
                return;
            }

            decimal discPct = 0m;
            if (!string.IsNullOrWhiteSpace(txtItemDiscPct.Text))
            {
                decimal.TryParse(txtItemDiscPct.Text.Replace("%", "").Trim(), out discPct);
            }

            decimal taxPct = 18m;
            if (cmbItemTaxPct.SelectedItem != null)
            {
                decimal.TryParse(cmbItemTaxPct.SelectedItem.ToString()!.Replace("%", ""), out taxPct);
            }

            var item = new InvoiceItem
            {
                ServiceName = service,
                Description = txtItemDesc.Text.Trim(),
                Quantity = numItemQty.Value,
                UnitPrice = rate,
                DiscountPercent = discPct,
                TaxPercent = taxPct
            };

            item.CalculateLineTotal();
            _workingItems.Add(item);

            RefreshLineItemsGrid();
            RecalculateBill();

            // Reset item inputs
            cmbServiceName.Text = string.Empty;
            txtItemDesc.Clear();
            numItemQty.Value = 1;
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
        }

        private void RemoveSelectedItem()
        {
            if (dgvInvoiceItems.SelectedRows.Count > 0)
            {
                int idx = dgvInvoiceItems.SelectedRows[0].Index;
                if (idx >= 0 && idx < _workingItems.Count)
                {
                    _workingItems.RemoveAt(idx);
                    RefreshLineItemsGrid();
                    RecalculateBill();
                }
            }
        }

        private void RefreshLineItemsGrid()
        {
            dgvInvoiceItems.Rows.Clear();
            foreach (var it in _workingItems)
            {
                dgvInvoiceItems.Rows.Add(
                    it.ServiceName, it.Description, $"{it.Quantity:N0}",
                    $"₹{it.UnitPrice:N2}", $"₹{it.DiscountAmount:N2}",
                    $"{it.TaxPercent:N0}%", $"₹{it.LineTotal:N2}"
                );
            }
        }

        private Invoice BuildCurrentInvoiceObject()
        {
            int custIdx = cmbCustomerSelect.SelectedIndex;
            long custId = 0;
            string custName = "Walk-in Customer";
            if (custIdx > 0 && custIdx <= _allCustomers.Count)
            {
                var c = _allCustomers[custIdx - 1];
                custId = c.Id;
                custName = c.Name;
            }

            string taxScheme = cmbTaxScheme.SelectedIndex switch
            {
                0 => "GST_INTRA",
                1 => "GST_INTER",
                _ => "NONE"
            };

            decimal.TryParse(txtOverallDisc.Text, out decimal ovrDisc);
            decimal.TryParse(txtInitialPaymentAmt.Text, out decimal initPay);

            var inv = new Invoice
            {
                InvoiceNumber = txtInvNumber.Text.Trim(),
                CustomerId = custId,
                CustomerName = custName,
                CustomerCompany = txtCustCompany.Text.Trim(),
                CustomerPhone = txtCustPhone.Text.Trim(),
                CustomerEmail = txtCustEmail.Text.Trim(),
                CustomerAddress = txtCustAddress.Text.Trim(),
                CustomerGSTIN = txtCustGSTIN.Text.Trim().ToUpperInvariant(),
                InvoiceDate = dtpInvDate.Value,
                DueDate = dtpDueDate.Value,
                PaymentTerms = cmbPaymentTerms.SelectedItem?.ToString() ?? "Net 15",
                TaxType = taxScheme,
                OverallDiscount = ovrDisc,
                AmountPaid = initPay
            };

            foreach (var it in _workingItems)
            {
                inv.Items.Add(new InvoiceItem
                {
                    ServiceName = it.ServiceName,
                    Description = it.Description,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice,
                    DiscountPercent = it.DiscountPercent,
                    DiscountAmount = it.DiscountAmount,
                    TaxPercent = it.TaxPercent,
                    LineTotal = it.LineTotal
                });
            }

            inv.RecalculateTotals();
            return inv;
        }

        private void RecalculateBill()
        {
            var inv = BuildCurrentInvoiceObject();

            lblCalcSubtotal.Text = $"₹{inv.Subtotal:N2}";
            lblCalcItemDisc.Text = $"-₹{inv.ItemDiscountTotal:N2}";
            lblCalcTaxable.Text = $"₹{inv.TaxableAmount:N2}";

            if (inv.TaxType == "GST_INTRA")
            {
                lblCalcCgst.Visible = true;
                lblCalcSgst.Visible = true;
                lblCalcIgst.Visible = false;
                lblCalcCgst.Text = $"₹{inv.CgstAmount:N2}";
                lblCalcSgst.Text = $"₹{inv.SgstAmount:N2}";
            }
            else if (inv.TaxType == "GST_INTER")
            {
                lblCalcCgst.Visible = false;
                lblCalcSgst.Visible = false;
                lblCalcIgst.Visible = true;
                lblCalcIgst.Text = $"₹{inv.IgstAmount:N2}";
            }
            else
            {
                lblCalcCgst.Visible = false;
                lblCalcSgst.Visible = false;
                lblCalcIgst.Visible = false;
            }

            lblCalcGrandTotal.Text = $"₹{inv.GrandTotal:N2}";
            lblCalcBalanceDue.Text = $"₹{inv.BalanceDue:N2}";
        }

        private void SaveCurrentInvoice(bool andPrint)
        {
            if (cmbCustomerSelect.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select or add a customer for this invoice.", "Customer Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCustomerSelect.Focus();
                return;
            }

            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item / service to the invoice.", "Empty Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbServiceName.Focus();
                return;
            }

            var inv = BuildCurrentInvoiceObject();
            PaymentRecord? initialPayment = null;

            if (decimal.TryParse(txtInitialPaymentAmt.Text, out decimal payAmt) && payAmt > 0)
            {
                initialPayment = new PaymentRecord
                {
                    Amount = payAmt,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = cmbInitialPaymentMode.SelectedItem?.ToString() ?? "Cash",
                    ReferenceNumber = txtInitialPaymentRef.Text.Trim(),
                    Notes = "Initial payment upon invoice creation"
                };
            }

            try
            {
                var saved = DatabaseService.SaveInvoice(inv, initialPayment);
                MessageBox.Show($"Invoice {saved.InvoiceNumber} saved successfully into database!\nTotal: ₹{saved.GrandTotal:N2} | Paid: ₹{saved.AmountPaid:N2} | Balance Due: ₹{saved.BalanceDue:N2}",
                    "Invoice Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (andPrint)
                {
                    var business = DatabaseService.GetBusinessProfile();
                    InvoicePrintService.PrintDirect(saved, business, this);
                }

                DataChanged?.Invoke(this, EventArgs.Empty);
                SwitchTab("Invoices");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save invoice: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewCurrentInvoice()
        {
            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item to preview the invoice.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var inv = BuildCurrentInvoiceObject();
            var business = DatabaseService.GetBusinessProfile();
            InvoicePrintService.ShowPrintPreview(inv, business, this);
        }

        private void ExportCurrentInvoicePdf()
        {
            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item to export the invoice.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var inv = BuildCurrentInvoiceObject();
            var business = DatabaseService.GetBusinessProfile();
            InvoicePrintService.OpenOrShareInvoice(inv, business);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 3: INVOICES DIRECTORY VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildInvoicesListView()
        {
            pnlInvoicesListView.Dock = DockStyle.Fill;
            pnlInvoicesListView.Padding = new Padding(16);

            // Filter Bar
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(0, 4, 0, 8)
            };

            txtInvoiceSearch.PlaceholderText = "Search by invoice # or customer name...";
            txtInvoiceSearch.SetBounds(0, 20, 260, 28);
            txtInvoiceSearch.TextChanged += (s, e) => RefreshInvoicesGrid();

            cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatusFilter.Items.AddRange(new object[] { "All", "Paid", "Partially Paid", "Unpaid", "Overdue", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SetBounds(270, 20, 130, 28);
            cmbStatusFilter.SelectedIndexChanged += (s, e) => RefreshInvoicesGrid();

            chkFilterDates.Text = "Filter Dates:";
            chkFilterDates.SetBounds(410, 22, 95, 24);
            chkFilterDates.CheckedChanged += (s, e) => { dtpFilterFrom.Enabled = dtpFilterTo.Enabled = chkFilterDates.Checked; RefreshInvoicesGrid(); };

            dtpFilterFrom.Format = DateTimePickerFormat.Short;
            dtpFilterFrom.Value = DateTime.Today.AddDays(-30);
            dtpFilterFrom.Enabled = false;
            dtpFilterFrom.SetBounds(510, 20, 100, 28);

            dtpFilterTo.Format = DateTimePickerFormat.Short;
            dtpFilterTo.Value = DateTime.Today;
            dtpFilterTo.Enabled = false;
            dtpFilterTo.SetBounds(620, 20, 100, 28);

            btnFilterReset.Text = "Reset Filters";
            btnFilterReset.Icon = null;
            btnFilterReset.FillColor = Color.FromArgb(148, 163, 184);
            btnFilterReset.SetBounds(730, 20, 100, 28);
            btnFilterReset.Click += (s, e) =>
            {
                txtInvoiceSearch.Clear();
                cmbStatusFilter.SelectedIndex = 0;
                chkFilterDates.Checked = false;
                RefreshInvoicesGrid();
            };

            pnlFilter.Controls.AddRange(new Control[] {
                txtInvoiceSearch, cmbStatusFilter, chkFilterDates, dtpFilterFrom, dtpFilterTo, btnFilterReset
            });

            // Action Bar for selected invoice
            var pnlGridActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 8, 0, 0)
            };

            btnGridOpenInvoice.Text = "Open / View";
            btnGridOpenInvoice.Icon = ReiconRenderer.IconType.Receipt;
            btnGridOpenInvoice.FillColor = ThemeManager.PrimaryAccent;
            btnGridOpenInvoice.SetBounds(0, 8, 130, 36);
            btnGridOpenInvoice.Click += (s, e) => OpenSelectedInvoice();

            btnGridRecordPay.Text = "+ Add Payment";
            btnGridRecordPay.Icon = ReiconRenderer.IconType.Wallet;
            btnGridRecordPay.FillColor = ThemeManager.StatusPositive;
            btnGridRecordPay.SetBounds(140, 8, 140, 36);
            btnGridRecordPay.Click += (s, e) => RecordPaymentForSelected();

            btnGridPrint.Text = "Print";
            btnGridPrint.Icon = null;
            btnGridPrint.FillColor = Color.FromArgb(79, 70, 229);
            btnGridPrint.SetBounds(290, 8, 90, 36);
            btnGridPrint.Click += (s, e) => PrintSelectedInvoice();

            btnGridExportPdf.Text = "Export PDF";
            btnGridExportPdf.Icon = null;
            btnGridExportPdf.FillColor = Color.FromArgb(37, 99, 235);
            btnGridExportPdf.SetBounds(390, 8, 110, 36);
            btnGridExportPdf.Click += (s, e) => ExportSelectedPdf();

            btnGridCancel.Text = "Cancel Invoice";
            btnGridCancel.Icon = ReiconRenderer.IconType.Close;
            btnGridCancel.FillColor = ThemeManager.StatusNegative;
            btnGridCancel.SetBounds(510, 8, 140, 36);
            btnGridCancel.Click += (s, e) => CancelSelectedInvoice();

            pnlGridActions.Controls.AddRange(new Control[] {
                btnGridOpenInvoice, btnGridRecordPay, btnGridPrint, btnGridExportPdf, btnGridCancel
            });

            // Invoices Grid
            dgvAllInvoices.Dock = DockStyle.Fill;
            dgvAllInvoices.BorderStyle = BorderStyle.None;
            dgvAllInvoices.RowHeadersVisible = false;
            dgvAllInvoices.AllowUserToAddRows = false;
            dgvAllInvoices.ReadOnly = true;
            dgvAllInvoices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAllInvoices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvAllInvoices.Columns.Add("InvNo", "Invoice #");
            dgvAllInvoices.Columns.Add("Customer", "Customer Name");
            dgvAllInvoices.Columns.Add("Date", "Invoice Date");
            dgvAllInvoices.Columns.Add("DueDate", "Due Date");
            dgvAllInvoices.Columns.Add("Total", "Grand Total (₹)");
            dgvAllInvoices.Columns.Add("Paid", "Paid (₹)");
            dgvAllInvoices.Columns.Add("Balance", "Balance Due (₹)");
            dgvAllInvoices.Columns.Add("Status", "Status");

            dgvAllInvoices.CellDoubleClick += (s, e) => OpenSelectedInvoice();

            pnlInvoicesListView.Controls.Add(dgvAllInvoices);
            pnlInvoicesListView.Controls.Add(pnlFilter);
            pnlInvoicesListView.Controls.Add(pnlGridActions);
        }

        private void RefreshInvoicesGrid()
        {
            string search = txtInvoiceSearch.Text.Trim();
            string status = cmbStatusFilter.SelectedItem?.ToString() ?? "All";
            DateTime? from = chkFilterDates.Checked ? dtpFilterFrom.Value.Date : null;
            DateTime? to = chkFilterDates.Checked ? dtpFilterTo.Value.Date : null;

            var list = DatabaseService.GetInvoices(search, status, from, to);
            dgvAllInvoices.Rows.Clear();

            foreach (var inv in list)
            {
                int r = dgvAllInvoices.Rows.Add(
                    inv.InvoiceNumber, inv.CustomerName, inv.InvoiceDate.ToString("dd MMM yyyy"),
                    inv.DueDate.ToString("dd MMM yyyy"), $"₹{inv.GrandTotal:N2}", $"₹{inv.AmountPaid:N2}",
                    $"₹{inv.BalanceDue:N2}", inv.Status
                );
                dgvAllInvoices.Rows[r].Tag = inv;
            }
        }

        private Invoice? GetSelectedGridInvoice()
        {
            if (dgvAllInvoices.SelectedRows.Count > 0 && dgvAllInvoices.SelectedRows[0].Tag is Invoice inv)
            {
                return DatabaseService.GetInvoiceById(inv.Id);
            }
            return null;
        }

        private void OpenSelectedInvoice()
        {
            var inv = GetSelectedGridInvoice();
            if (inv == null)
            {
                MessageBox.Show("Please select an invoice from the list.", "Select Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var view = new InvoiceDetailsForm(inv);
            view.ShowDialog(this);
            RefreshInvoicesGrid();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RecordPaymentForSelected()
        {
            var inv = GetSelectedGridInvoice();
            if (inv == null)
            {
                MessageBox.Show("Please select an invoice to record payment.", "Select Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (inv.Status == "Cancelled")
            {
                MessageBox.Show("Cannot record payment for a cancelled invoice.", "Invalid Action", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (inv.BalanceDue <= 0)
            {
                MessageBox.Show("This invoice is already fully paid.", "Fully Paid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new RecordPaymentDialog(inv);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                RefreshInvoicesGrid();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PrintSelectedInvoice()
        {
            var inv = GetSelectedGridInvoice();
            if (inv != null)
            {
                var b = DatabaseService.GetBusinessProfile();
                InvoicePrintService.PrintDirect(inv, b, this);
            }
        }

        private void ExportSelectedPdf()
        {
            var inv = GetSelectedGridInvoice();
            if (inv != null)
            {
                var b = DatabaseService.GetBusinessProfile();
                InvoicePrintService.OpenOrShareInvoice(inv, b);
            }
        }

        private void CancelSelectedInvoice()
        {
            var inv = GetSelectedGridInvoice();
            if (inv == null) return;

            if (inv.Status == "Cancelled")
            {
                MessageBox.Show("This invoice is already cancelled.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to cancel invoice {inv.InvoiceNumber}?", "Cancel Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DatabaseService.CancelInvoice(inv.Id);
                RefreshInvoicesGrid();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 4: CUSTOMERS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCustomersView()
        {
            pnlCustomersView.Dock = DockStyle.Fill;
            pnlCustomersView.Padding = new Padding(16);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 4, 0, 8)
            };

            txtCustDirSearch.PlaceholderText = "Search customers by name, company, phone, or email...";
            txtCustDirSearch.SetBounds(0, 8, 380, 28);
            txtCustDirSearch.TextChanged += (s, e) => RefreshCustomersGrid();

            btnCustDirAdd.Text = "+ Add Customer";
            btnCustDirAdd.Icon = ReiconRenderer.IconType.Plus;
            btnCustDirAdd.FillColor = ThemeManager.PrimaryAccent;
            btnCustDirAdd.SetBounds(390, 8, 150, 32);
            btnCustDirAdd.Click += (s, e) =>
            {
                using var dlg = new CustomerDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshCustomersGrid();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            };

            btnCustDirEdit.Text = "Edit Customer";
            btnCustDirEdit.Icon = null;
            btnCustDirEdit.FillColor = Color.FromArgb(79, 70, 229);
            btnCustDirEdit.SetBounds(550, 8, 130, 32);
            btnCustDirEdit.Click += (s, e) => EditSelectedCustomer();

            pnlTop.Controls.AddRange(new Control[] { txtCustDirSearch, btnCustDirAdd, btnCustDirEdit });

            dgvAllCustomers.Dock = DockStyle.Fill;
            dgvAllCustomers.BorderStyle = BorderStyle.None;
            dgvAllCustomers.RowHeadersVisible = false;
            dgvAllCustomers.AllowUserToAddRows = false;
            dgvAllCustomers.ReadOnly = true;
            dgvAllCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAllCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvAllCustomers.Columns.Add("Name", "Customer Name");
            dgvAllCustomers.Columns.Add("Company", "Company Name");
            dgvAllCustomers.Columns.Add("Phone", "Phone Number");
            dgvAllCustomers.Columns.Add("Email", "Email Address");
            dgvAllCustomers.Columns.Add("Balance", "Outstanding Balance (₹)");
            dgvAllCustomers.Columns.Add("GSTIN", "GSTIN");

            dgvAllCustomers.CellDoubleClick += (s, e) => EditSelectedCustomer();

            pnlCustomersView.Controls.Add(dgvAllCustomers);
            pnlCustomersView.Controls.Add(pnlTop);
        }

        private void RefreshCustomersGrid()
        {
            string q = txtCustDirSearch.Text.Trim();
            var list = DatabaseService.GetCustomers(q);
            dgvAllCustomers.Rows.Clear();

            foreach (var c in list)
            {
                int r = dgvAllCustomers.Rows.Add(c.Name, c.CompanyName, c.Phone, c.Email, $"₹{c.OutstandingBalance:N2}", c.GSTIN);
                dgvAllCustomers.Rows[r].Tag = c;
            }
        }

        private void EditSelectedCustomer()
        {
            if (dgvAllCustomers.SelectedRows.Count > 0 && dgvAllCustomers.SelectedRows[0].Tag is Customer c)
            {
                using var dlg = new CustomerDialog(c);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshCustomersGrid();
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.", "Select Customer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME APPLICATION (LIGHT / DARK MODES)
        // ═══════════════════════════════════════════════════════════════════════════
        public void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            Color pageBg = ThemeManager.PageBackground;
            Color cardBg = ThemeManager.CardBackground;
            Color textPrimary = ThemeManager.TextPrimary;
            Color textSecondary = ThemeManager.TextSecondary;
            Color borderCol = ThemeManager.BorderHairline;
            Color inputBg = isDark ? Color.FromArgb(30, 32, 48) : Color.White;

            BackColor = pageBg;
            pnlNavHeader.BackColor = cardBg;
            pnlViewDeck.BackColor = pageBg;

            pnlDashboardView.BackColor = pageBg;
            pnlCreateInvoiceView.BackColor = pageBg;
            pnlInvoicesListView.BackColor = pageBg;
            pnlCustomersView.BackColor = pageBg;

            UpdateTabButtonStyles();

            // Style all DataGridViews
            foreach (var dgv in new[] { dgvRecentSales, dgvInvoiceItems, dgvAllInvoices, dgvAllCustomers })
            {
                dgv.BackgroundColor = cardBg;
                dgv.GridColor = borderCol;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(32, 35, 50) : Color.FromArgb(241, 245, 249);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = textSecondary;
                dgv.DefaultCellStyle.BackColor = cardBg;
                dgv.DefaultCellStyle.ForeColor = textPrimary;
                dgv.DefaultCellStyle.SelectionBackColor = ThemeManager.NavHover;
                dgv.DefaultCellStyle.SelectionForeColor = textPrimary;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.GridRowAlternate;
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = textPrimary;
            }

            // Style inputs recursively
            ApplyInputsTheme(pnlCreateInvoiceView, inputBg, textPrimary, textSecondary);
            ApplyInputsTheme(pnlInvoicesListView, inputBg, textPrimary, textSecondary);
            ApplyInputsTheme(pnlCustomersView, inputBg, textPrimary, textSecondary);

            Invalidate(true);
        }

        private static void ApplyInputsTheme(Control parent, Color inputBg, Color textPrimary, Color textSecondary)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox tb)
                {
                    tb.BackColor = inputBg;
                    tb.ForeColor = textPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = inputBg;
                    cb.ForeColor = textPrimary;
                }
                else if (c is NumericUpDown num)
                {
                    num.BackColor = inputBg;
                    num.ForeColor = textPrimary;
                }
                else if (c is DateTimePicker dtp)
                {
                    dtp.CalendarMonthBackground = inputBg;
                    dtp.CalendarForeColor = textPrimary;
                }
                else if (c is Label lbl)
                {
                    lbl.ForeColor = textPrimary;
                }
                else if (c is Panel pnl)
                {
                    pnl.BackColor = ThemeManager.CardBackground;
                    ApplyInputsTheme(pnl, inputBg, textPrimary, textSecondary);
                }
            }
        }
    }
}
