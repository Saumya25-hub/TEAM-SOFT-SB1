using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Master Sales & Billing Module for TEAM SOFT Windows Desktop (.NET 8 WinForms).
    /// Modern, high-end SaaS aesthetic (Linear / Stripe / Windows 11 Fluent 2.0).
    /// Features:
    /// - Sleek Segmented Capsule Navigation Tab Bar (Zero Emojis, pure SVG vector icons)
    /// - High-End Elevated KPI Cards with Icon Badges, Trend Tags & Hover Elevation
    /// - Custom DataGridView Table Rendering with Rounded Status Pill Badges
    /// - Multi-Card Structured Create Invoice Form with Clean Grouping & Zero Underscores
    /// - 100% SQLite Database Driven with Parameterized Queries & ACID Transactions
    /// - Strict Decimal Precision Math for Subtotal, Discounts, CGST/SGST/IGST & Balance Due
    /// - Vector GDI+ PrintPreview, Native Printing, and Standalone PDF Export
    /// </summary>
    public class SalesBillingModuleControl : UserControl
    {
        // ═══════════════════════════════════════════════════════════════════════════
        //  NAVIGATION SUB-TABS (SEGMENTED CAPSULE TRACK)
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly Panel pnlNavHeader = new();
        private readonly Panel pnlNavPillTrack = new();
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

        private struct MetricCardData
        {
            public string Title;
            public string Value;
            public string Badge;
            public Color AccentColor;
            public ReiconRenderer.IconType Icon;
            public bool IsHovered;
        }

        private readonly MetricCardData[] _cardsData = new MetricCardData[6];

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

            InitMetricCardsData();
            BuildModuleLayout();
            ApplyTheme();
            SwitchTab("Dashboard");

            ThemeManager.ThemeChanged += ApplyTheme;
        }

        private void InitMetricCardsData()
        {
            _cardsData[0] = new MetricCardData { Title = "TOTAL SALES", Value = "₹0.00", Badge = "● All-Time", AccentColor = Color.FromArgb(79, 70, 229), Icon = ReiconRenderer.IconType.TrendingUp };
            _cardsData[1] = new MetricCardData { Title = "TOTAL INVOICES", Value = "0", Badge = "● Recorded", AccentColor = Color.FromArgb(2, 132, 199), Icon = ReiconRenderer.IconType.Receipt };
            _cardsData[2] = new MetricCardData { Title = "PAID INVOICES", Value = "0", Badge = "● Settled", AccentColor = Color.FromArgb(16, 185, 129), Icon = ReiconRenderer.IconType.Check };
            _cardsData[3] = new MetricCardData { Title = "UNPAID INVOICES", Value = "0", Badge = "● Due", AccentColor = Color.FromArgb(245, 158, 11), Icon = ReiconRenderer.IconType.Warning };
            _cardsData[4] = new MetricCardData { Title = "OUTSTANDING DUE", Value = "₹0.00", Badge = "● Receivables", AccentColor = Color.FromArgb(225, 29, 72), Icon = ReiconRenderer.IconType.Wallet };
            _cardsData[5] = new MetricCardData { Title = "TODAY'S BILLINGS", Value = "₹0.00", Badge = "● Live Volume", AccentColor = Color.FromArgb(139, 92, 246), Icon = ReiconRenderer.IconType.Coins };
        }

        private void BuildModuleLayout()
        {
            Controls.Clear();

            // 1. Top Sub-Navigation Bar (Segmented Capsule)
            pnlNavHeader.Dock = DockStyle.Top;
            pnlNavHeader.Height = 54;
            pnlNavHeader.Padding = new Padding(18, 6, 18, 6);

            pnlNavPillTrack.Dock = DockStyle.Left;
            pnlNavPillTrack.Width = 660;
            pnlNavPillTrack.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = pnlNavPillTrack.Width;
                int h = pnlNavPillTrack.Height;
                if (w <= 0 || h <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;
                Color trackBg = isDark ? Color.FromArgb(26, 29, 43) : Color.FromArgb(241, 245, 249);
                Color trackBdr = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
                using var b = new SolidBrush(trackBg);
                using var p = new Pen(trackBdr, 1.2f);
                g.FillPath(b, path);
                g.DrawPath(p, path);
            };

            SetupTabButton(btnTabDashboard, "Sales Overview", "Dashboard", ReiconRenderer.IconType.Dashboard, 155);
            SetupTabButton(btnTabCreateInvoice, "Create Invoice", "CreateInvoice", ReiconRenderer.IconType.Plus, 155);
            SetupTabButton(btnTabInvoices, "Invoices Directory", "Invoices", ReiconRenderer.IconType.Receipt, 175);
            SetupTabButton(btnTabCustomers, "Customers Directory", "Customers", ReiconRenderer.IconType.Customers, 175);

            pnlNavPillTrack.Controls.AddRange(new Control[] {
                btnTabCustomers, btnTabInvoices, btnTabCreateInvoice, btnTabDashboard
            });

            pnlNavHeader.Controls.Add(pnlNavPillTrack);

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

        private void SetupTabButton(Button btn, string text, string tabKey, ReiconRenderer.IconType icon, int width)
        {
            btn.Text = string.Empty;
            btn.Dock = DockStyle.Left;
            btn.Width = width;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;
            btn.Tag = tabKey;

            bool isHovered = false;
            btn.MouseEnter += (s, e) => { isHovered = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { isHovered = false; btn.Invalidate(); };
            btn.Click += (s, e) => SwitchTab(tabKey);

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = btn.Width;
                int h = btn.Height;
                if (w <= 0 || h <= 0) return;

                bool isActive = (_currentTab == tabKey);
                bool isDark = ThemeManager.IsDarkMode;

                int pad = 3;
                var pillRect = new Rectangle(pad, pad, w - pad * 2, h - pad * 2);

                if (isActive)
                {
                    Color activeBg = isDark ? Color.FromArgb(36, 42, 64) : Color.White;
                    Color activeBdr = isDark ? Color.FromArgb(79, 70, 229) : Color.FromArgb(203, 213, 225);

                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(pillRect.X, pillRect.Y, pillRect.Width, pillRect.Height, 8);
                    using var b = new SolidBrush(activeBg);
                    using var p = new Pen(activeBdr, 1.2f);
                    g.FillPath(b, path);
                    g.DrawPath(p, path);
                }
                else if (isHovered)
                {
                    Color hoverBg = isDark ? Color.FromArgb(32, 37, 54) : Color.FromArgb(226, 232, 240);
                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(pillRect.X, pillRect.Y, pillRect.Width, pillRect.Height, 8);
                    using var b = new SolidBrush(hoverBg);
                    g.FillPath(b, path);
                }

                Color iconColor = isActive
                    ? (isDark ? Color.FromArgb(129, 140, 248) : Color.FromArgb(79, 70, 229))
                    : (isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139));

                Color textColor = isActive
                    ? (isDark ? Color.White : Color.FromArgb(15, 23, 42))
                    : (isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105));

                // Draw Icon
                int iconD = 16;
                int iconX = pad + 14;
                int iconY = (h - iconD) / 2;
                ReiconRenderer.DrawIcon(g, icon, new Rectangle(iconX, iconY, iconD, iconD), iconColor, 1.6f);

                // Draw Text
                using var font = new Font("Segoe UI", 9f, isActive ? FontStyle.Bold : FontStyle.Regular);
                using var textBrush = new SolidBrush(textColor);
                using var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                g.DrawString(text, font, textBrush, new RectangleF(iconX + iconD + 8, 0, w - (iconX + iconD + 8), h), sf);
            };
        }

        public void SwitchTab(string tabKey)
        {
            _currentTab = tabKey;

            pnlDashboardView.Visible = (tabKey == "Dashboard");
            pnlCreateInvoiceView.Visible = (tabKey == "CreateInvoice");
            pnlInvoicesListView.Visible = (tabKey == "Invoices");
            pnlCustomersView.Visible = (tabKey == "Customers");

            pnlNavPillTrack.Invalidate(true);

            if (tabKey == "Dashboard") LoadDashboardData();
            else if (tabKey == "CreateInvoice") PrepareNewInvoiceForm();
            else if (tabKey == "Invoices") RefreshInvoicesGrid();
            else if (tabKey == "Customers") RefreshCustomersGrid();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 1: SALES DASHBOARD VIEW (PRO AESTHETICS)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildDashboardView()
        {
            pnlDashboardView.Dock = DockStyle.Fill;
            pnlDashboardView.AutoScroll = true;
            pnlDashboardView.Padding = new Padding(18);

            // Row 1: KPI Metrics Cards
            pnlDashCardsHost.Dock = DockStyle.Top;
            pnlDashCardsHost.Height = 114;
            pnlDashCardsHost.Padding = new Padding(0, 0, 0, 12);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1
            };
            for (int i = 0; i < 6; i++)
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));

            Panel[] cardPanels = { pnlCardTotalSales, pnlCardTotalInvoices, pnlCardPaidInvoices, pnlCardUnpaidInvoices, pnlCardOutstanding, pnlCardTodaySales };
            for (int i = 0; i < 6; i++)
            {
                int cardIdx = i;
                var pnl = cardPanels[i];
                pnl.Dock = DockStyle.Fill;
                pnl.Margin = new Padding(4);
                pnl.MouseEnter += (s, e) => { _cardsData[cardIdx].IsHovered = true; pnl.Invalidate(); };
                pnl.MouseLeave += (s, e) => { _cardsData[cardIdx].IsHovered = false; pnl.Invalidate(); };
                pnl.Paint += (s, e) => PaintMetricCard(e.Graphics, pnl, _cardsData[cardIdx]);
                tlp.Controls.Add(pnl, i, 0);
            }

            pnlDashCardsHost.Controls.Add(tlp);

            // Row 2: Recent Invoices Action Bar
            var pnlActionBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(0, 8, 0, 8)
            };

            var lblRecentTitle = new Label
            {
                Text = "Recent Sales Invoices & Transactions",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 6, 0, 0),
                UseMnemonic = false
            };

            var pnlBtnsRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 500
            };

            btnQuickCustomers.Text = "Manage Clients";
            btnQuickCustomers.Icon = ReiconRenderer.IconType.Customers;
            btnQuickCustomers.BorderColor = Color.FromArgb(203, 213, 225);
            btnQuickCustomers.BorderWidth = 1.2f;
            btnQuickCustomers.FillColor = Color.White;
            btnQuickCustomers.TextColor = Color.FromArgb(30, 41, 59);
            btnQuickCustomers.HoverColor = Color.FromArgb(241, 245, 249);
            btnQuickCustomers.SetBounds(350, 0, 145, 36);
            btnQuickCustomers.Click += (s, e) => SwitchTab("Customers");

            btnQuickViewInvs.Text = "All Invoices";
            btnQuickViewInvs.Icon = ReiconRenderer.IconType.Receipt;
            btnQuickViewInvs.BorderColor = Color.FromArgb(203, 213, 225);
            btnQuickViewInvs.BorderWidth = 1.2f;
            btnQuickViewInvs.FillColor = Color.White;
            btnQuickViewInvs.TextColor = Color.FromArgb(30, 41, 59);
            btnQuickViewInvs.HoverColor = Color.FromArgb(241, 245, 249);
            btnQuickViewInvs.SetBounds(210, 0, 130, 36);
            btnQuickViewInvs.Click += (s, e) => SwitchTab("Invoices");

            btnQuickNewInv.Text = "+ New Invoice";
            btnQuickNewInv.Icon = ReiconRenderer.IconType.Plus;
            btnQuickNewInv.FillColor = Color.FromArgb(79, 70, 229);
            btnQuickNewInv.HoverColor = Color.FromArgb(67, 56, 202);
            btnQuickNewInv.TextColor = Color.White;
            btnQuickNewInv.SetBounds(60, 0, 140, 36);
            btnQuickNewInv.Click += (s, e) => SwitchTab("CreateInvoice");

            pnlBtnsRight.Controls.AddRange(new Control[] { btnQuickNewInv, btnQuickViewInvs, btnQuickCustomers });
            pnlActionBar.Controls.Add(lblRecentTitle);
            pnlActionBar.Controls.Add(pnlBtnsRight);

            // Row 3: Modern Recent Sales Grid
            FormatModernTable(dgvRecentSales);
            dgvRecentSales.Dock = DockStyle.Fill;

            dgvRecentSales.Columns.Add("InvNum", "INVOICE #");
            dgvRecentSales.Columns.Add("Customer", "CUSTOMER / CLIENT");
            dgvRecentSales.Columns.Add("Date", "INVOICE DATE");
            dgvRecentSales.Columns.Add("Due", "DUE DATE");
            dgvRecentSales.Columns.Add("Total", "GRAND TOTAL (₹)");
            dgvRecentSales.Columns.Add("Paid", "AMOUNT PAID (₹)");
            dgvRecentSales.Columns.Add("Balance", "BALANCE DUE (₹)");
            dgvRecentSales.Columns.Add("Status", "STATUS");

            dgvRecentSales.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvRecentSales.Columns["Paid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvRecentSales.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

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
            pnlDashboardView.Controls.Add(pnlActionBar);
            pnlDashboardView.Controls.Add(pnlDashCardsHost);
        }

        private static void PaintMetricCard(Graphics g, Panel pnl, MetricCardData data)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = pnl.Width;
            int h = pnl.Height;
            if (w <= 0 || h <= 0) return;

            bool isDark = ThemeManager.IsDarkMode;
            Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
            Color borderCol = data.IsHovered ? data.AccentColor : (isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240));

            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 12);
            using var bg = new SolidBrush(cardBg);
            using var pen = new Pen(borderCol, data.IsHovered ? 1.5f : 1.2f);
            g.FillPath(bg, path);
            g.DrawPath(pen, path);

            // Icon Badge (30x30 with 15% opacity tint)
            int badgeSize = 30;
            int badgeX = 14;
            int badgeY = 12;
            Color badgeBg = Color.FromArgb(30, data.AccentColor);
            using var badgePath = GraphicsExtensions.CreateRoundedRectanglePath(badgeX, badgeY, badgeSize, badgeSize, 8);
            using var badgeBrush = new SolidBrush(badgeBg);
            g.FillPath(badgeBrush, badgePath);

            ReiconRenderer.DrawIcon(g, data.Icon, new Rectangle(badgeX + 6, badgeY + 6, 18, 18), data.AccentColor, 1.8f);

            // Pill tag on top right
            using var fBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            using var bBadge = new SolidBrush(data.AccentColor);
            using var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            g.DrawString(data.Badge, fBadge, bBadge, new RectangleF(0, badgeY, w - 14, badgeSize), sfRight);

            // Title
            using var fTitle = new Font("Segoe UI", 8.2f, FontStyle.Bold);
            Color textSec = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
            using var bTitle = new SolidBrush(textSec);
            g.DrawString(data.Title, fTitle, bTitle, 14, 52);

            // Main Value
            using var fVal = new Font("Segoe UI", 14f, FontStyle.Bold);
            Color textPri = isDark ? Color.White : Color.FromArgb(15, 23, 42);
            using var bVal = new SolidBrush(textPri);
            g.DrawString(data.Value, fVal, bVal, 12, 72);
        }

        private void LoadDashboardData()
        {
            var m = DatabaseService.GetSalesDashboardMetrics();

            _cardsData[0].Value = $"₹{m.TotalSales:N2}";
            _cardsData[1].Value = $"{m.TotalInvoicesCount}";
            _cardsData[2].Value = $"{m.PaidInvoicesCount}";
            _cardsData[3].Value = $"{m.UnpaidInvoicesCount}";
            _cardsData[3].Badge = m.OverdueInvoicesCount > 0 ? $"● {m.OverdueInvoicesCount} Overdue" : "● 0 Overdue";
            _cardsData[4].Value = $"₹{m.OutstandingBalance:N2}";
            _cardsData[5].Value = $"₹{m.TodaySales:N2}";

            pnlDashCardsHost.Invalidate(true);

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

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD TAB 2: CREATE INVOICE VIEW (STRUCTURED CARDS, PRO FINISH)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCreateInvoiceView()
        {
            pnlCreateInvoiceView.Dock = DockStyle.Fill;
            pnlCreateInvoiceView.AutoScroll = true;
            pnlCreateInvoiceView.Padding = new Padding(18);

            int y = 6;
            int cardW = 980;

            // ─────────────────────────────────────────────────────────────────────────
            // CARD 1: INVOICE SETTINGS & CLIENT DETAILS
            // ─────────────────────────────────────────────────────────────────────────
            var pnlCard1 = CreateStyledCardPanel(0, y, cardW, 195, "1. Invoice Settings & Client Information", ReiconRenderer.IconType.Receipt);
            pnlCreateInvoiceView.Controls.Add(pnlCard1);

            int subCol1 = 20;
            int subCol2 = 250;
            int subCol3 = 510;
            int subCol4 = 750;
            int row1Y = 46;

            // Left: Invoice Metadata
            AddModernField(pnlCard1, "Invoice Number (Auto Unique)", txtInvNumber, subCol1, row1Y, 210);
            txtInvNumber.ReadOnly = true;
            txtInvNumber.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            AddModernField(pnlCard1, "Invoice Date", dtpInvDate, subCol2, row1Y, 230);
            dtpInvDate.Format = DateTimePickerFormat.Short;

            int row2Y = row1Y + 54;
            AddModernField(pnlCard1, "Payment Due Date", dtpDueDate, subCol1, row2Y, 210);
            dtpDueDate.Format = DateTimePickerFormat.Short;

            cmbPaymentTerms.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentTerms.Items.Clear();
            cmbPaymentTerms.Items.AddRange(new object[] { "Due on Receipt", "Net 15", "Net 30", "Net 60" });
            cmbPaymentTerms.SelectedIndex = 1;
            AddModernField(pnlCard1, "Payment Terms", cmbPaymentTerms, subCol2, row2Y, 230);

            int row3Y = row2Y + 54;
            cmbTaxScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTaxScheme.Items.Clear();
            cmbTaxScheme.Items.AddRange(new object[] { "Intra-State GST (CGST 9% + SGST 9%)", "Inter-State GST (IGST 18%)", "Zero Tax / Export / SEZ" });
            cmbTaxScheme.SelectedIndex = 0;
            cmbTaxScheme.SelectedIndexChanged += (s, e) => RecalculateBill();
            AddModernField(pnlCard1, "GST Tax Policy Scheme", cmbTaxScheme, subCol1, row3Y, 460);

            // Right: Customer Selection & Details
            cmbCustomerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCustomerSelect.SelectedIndexChanged += (s, e) => OnCustomerSelected();
            AddModernField(pnlCard1, "Select Existing Customer *", cmbCustomerSelect, subCol3, row1Y, 220);

            btnAddNewCustomer.Text = "+ New Client";
            btnAddNewCustomer.Icon = ReiconRenderer.IconType.Plus;
            btnAddNewCustomer.FillColor = Color.FromArgb(79, 70, 229);
            btnAddNewCustomer.SetBounds(subCol3 + 230, row1Y + 20, 130, 28);
            btnAddNewCustomer.Click += (s, e) =>
            {
                using var dlg = new CustomerDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadCustomersIntoDropdown(dlg.SavedCustomer.Id);
                    DataChanged?.Invoke(this, EventArgs.Empty);
                }
            };
            pnlCard1.Controls.Add(btnAddNewCustomer);

            AddModernField(pnlCard1, "Company / Org Name", txtCustCompany, subCol3, row2Y, 220);
            AddModernField(pnlCard1, "Phone Number *", txtCustPhone, subCol4 + 30, row2Y, 170);

            AddModernField(pnlCard1, "Email Address", txtCustEmail, subCol3, row3Y, 220);
            AddModernField(pnlCard1, "Billing Address / Location", txtCustAddress, subCol4 + 30, row3Y, 170);

            y += 210;

            // ─────────────────────────────────────────────────────────────────────────
            // CARD 2: SERVICES & BILLABLE LINE ITEMS
            // ─────────────────────────────────────────────────────────────────────────
            var pnlCard2 = CreateStyledCardPanel(0, y, cardW, 290, "2. Billable Services, Subscriptions & Products", ReiconRenderer.IconType.Package);
            pnlCreateInvoiceView.Controls.Add(pnlCard2);

            int itemY = 46;
            cmbServiceName.Items.Clear();
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
            AddModernField(pnlCard2, "Service / Item Name *", cmbServiceName, 20, itemY, 260);

            txtItemDesc.PlaceholderText = "Scope of work / deliverables";
            AddModernField(pnlCard2, "Description / Scope", txtItemDesc, 290, itemY, 210);

            numItemQty.Minimum = 1;
            numItemQty.Maximum = 10000;
            numItemQty.Value = 1;
            AddModernField(pnlCard2, "Qty *", numItemQty, 510, itemY, 60);

            txtItemPrice.PlaceholderText = "0.00";
            AddModernField(pnlCard2, "Unit Rate (₹) *", txtItemPrice, 580, itemY, 100);

            txtItemDiscPct.PlaceholderText = "0%";
            AddModernField(pnlCard2, "Disc %", txtItemDiscPct, 690, itemY, 60);

            cmbItemTaxPct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbItemTaxPct.Items.Clear();
            cmbItemTaxPct.Items.AddRange(new object[] { "0%", "5%", "12%", "18%", "28%" });
            cmbItemTaxPct.SelectedItem = "18%";
            AddModernField(pnlCard2, "GST %", cmbItemTaxPct, 760, itemY, 70);

            btnAddLineItem.Text = "+ Add Line";
            btnAddLineItem.Icon = ReiconRenderer.IconType.Plus;
            btnAddLineItem.FillColor = Color.FromArgb(16, 185, 129);
            btnAddLineItem.HoverColor = Color.FromArgb(5, 150, 105);
            btnAddLineItem.SetBounds(840, itemY + 20, 115, 28);
            btnAddLineItem.Click += (s, e) => AddItemToInvoice();
            pnlCard2.Controls.Add(btnAddLineItem);

            // Line items grid
            FormatModernTable(dgvInvoiceItems);
            dgvInvoiceItems.SetBounds(20, itemY + 58, 935, 140);
            dgvInvoiceItems.Columns.Add("Service", "SERVICE / PRODUCT");
            dgvInvoiceItems.Columns.Add("Desc", "SCOPE / DESCRIPTION");
            dgvInvoiceItems.Columns.Add("Qty", "QTY");
            dgvInvoiceItems.Columns.Add("Price", "RATE (₹)");
            dgvInvoiceItems.Columns.Add("Disc", "DISC (₹)");
            dgvInvoiceItems.Columns.Add("Tax", "GST %");
            dgvInvoiceItems.Columns.Add("Total", "LINE TOTAL (₹)");

            dgvInvoiceItems.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoiceItems.Columns["Disc"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoiceItems.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            btnRemoveLineItem.Text = "Remove Item";
            btnRemoveLineItem.Icon = ReiconRenderer.IconType.Close;
            btnRemoveLineItem.BorderColor = Color.FromArgb(244, 63, 94);
            btnRemoveLineItem.BorderWidth = 1.2f;
            btnRemoveLineItem.FillColor = Color.White;
            btnRemoveLineItem.TextColor = Color.FromArgb(225, 29, 72);
            btnRemoveLineItem.HoverColor = Color.FromArgb(255, 241, 242);
            btnRemoveLineItem.SetBounds(20, itemY + 204, 130, 28);
            btnRemoveLineItem.Click += (s, e) => RemoveSelectedItem();

            pnlCard2.Controls.Add(dgvInvoiceItems);
            pnlCard2.Controls.Add(btnRemoveLineItem);

            y += 305;

            // ─────────────────────────────────────────────────────────────────────────
            // CARD 3: PAYMENT LEDGER & FINAL CALCULATION SUMMARY
            // ─────────────────────────────────────────────────────────────────────────
            var pnlCard3 = CreateStyledCardPanel(0, y, cardW, 215, "3. Payment Recording & Financial Summary", ReiconRenderer.IconType.Wallet);
            pnlCreateInvoiceView.Controls.Add(pnlCard3);

            // Left side: Payment recording on save
            int payY = 46;
            AddModernField(pnlCard3, "Amount Paid Now (₹)", txtInitialPaymentAmt, 20, payY, 150);
            txtInitialPaymentAmt.Text = "0.00";
            txtInitialPaymentAmt.TextChanged += (s, e) => RecalculateBill();

            cmbInitialPaymentMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInitialPaymentMode.Items.Clear();
            cmbInitialPaymentMode.Items.AddRange(new object[] { "Bank Transfer", "UPI", "Cash", "Card", "Other" });
            cmbInitialPaymentMode.SelectedIndex = 0;
            AddModernField(pnlCard3, "Payment Method", cmbInitialPaymentMode, 185, payY, 140);

            txtInitialPaymentRef.PlaceholderText = "Cheque / UTR / Txn Ref";
            AddModernField(pnlCard3, "Reference Number", txtInitialPaymentRef, 340, payY, 150);

            // Right side: Financial receipt card
            int sumRx = 650;
            int sumY = 44;

            void AddReceiptRow(string title, Label valLbl, int ry, bool bold = false)
            {
                var lblT = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
                    Bounds = new Rectangle(sumRx, ry, 140, 20),
                    AutoSize = false,
                    UseMnemonic = false
                };
                valLbl.SetBounds(sumRx + 140, ry, 160, 20);
                valLbl.Font = new Font("Segoe UI", 9.5f, bold ? FontStyle.Bold : FontStyle.Regular);
                valLbl.TextAlign = ContentAlignment.TopRight;
                valLbl.Text = "₹0.00";
                pnlCard3.Controls.Add(lblT);
                pnlCard3.Controls.Add(valLbl);
            }

            AddReceiptRow("Subtotal:", lblCalcSubtotal, sumY);
            AddReceiptRow("Item Discounts:", lblCalcItemDisc, sumY + 22);

            var lblOvr = new Label { Text = "Overall Discount (₹):", Font = new Font("Segoe UI", 9f), Bounds = new Rectangle(sumRx, sumY + 44, 140, 20), UseMnemonic = false };
            txtOverallDisc.SetBounds(sumRx + 180, sumY + 43, 120, 22);
            txtOverallDisc.Text = "0.00";
            txtOverallDisc.TextAlign = HorizontalAlignment.Right;
            txtOverallDisc.TextChanged += (s, e) => RecalculateBill();
            pnlCard3.Controls.Add(lblOvr);
            pnlCard3.Controls.Add(txtOverallDisc);

            AddReceiptRow("Taxable Amount:", lblCalcTaxable, sumY + 68);
            AddReceiptRow("CGST Amount:", lblCalcCgst, sumY + 88);
            AddReceiptRow("SGST Amount:", lblCalcSgst, sumY + 108);
            AddReceiptRow("IGST Amount:", lblCalcIgst, sumY + 108);

            // Hairline separator
            var sep = new Panel { Bounds = new Rectangle(sumRx, sumY + 130, 305, 1), BackColor = Color.FromArgb(203, 213, 225) };
            pnlCard3.Controls.Add(sep);

            // Grand Total (Large Bold Callout)
            var lblGtTitle = new Label { Text = "GRAND TOTAL:", Font = new Font("Segoe UI", 11f, FontStyle.Bold), Bounds = new Rectangle(sumRx, sumY + 136, 120, 24), UseMnemonic = false };
            lblCalcGrandTotal.Bounds = new Rectangle(sumRx + 120, sumY + 134, 185, 26);
            lblCalcGrandTotal.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblCalcGrandTotal.ForeColor = Color.FromArgb(16, 185, 129);
            lblCalcGrandTotal.TextAlign = ContentAlignment.TopRight;
            lblCalcGrandTotal.Text = "₹0.00";
            pnlCard3.Controls.Add(lblGtTitle);
            pnlCard3.Controls.Add(lblCalcGrandTotal);

            // Balance Due
            var lblBdTitle = new Label { Text = "Balance Due:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Bounds = new Rectangle(sumRx, sumY + 162, 120, 22), UseMnemonic = false };
            lblCalcBalanceDue.Bounds = new Rectangle(sumRx + 120, sumY + 162, 185, 22);
            lblCalcBalanceDue.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCalcBalanceDue.ForeColor = Color.FromArgb(225, 29, 72);
            lblCalcBalanceDue.TextAlign = ContentAlignment.TopRight;
            lblCalcBalanceDue.Text = "₹0.00";
            pnlCard3.Controls.Add(lblBdTitle);
            pnlCard3.Controls.Add(lblCalcBalanceDue);

            y += 230;

            // ─────────────────────────────────────────────────────────────────────────
            // DOCKED ACTION FOOTER BAR
            // ─────────────────────────────────────────────────────────────────────────
            var pnlFooter = new Panel
            {
                Bounds = new Rectangle(0, y, cardW, 56),
                Padding = new Padding(0, 8, 0, 8)
            };

            btnSaveInvoice.Text = "Save Invoice";
            btnSaveInvoice.Icon = ReiconRenderer.IconType.Check;
            btnSaveInvoice.FillColor = Color.FromArgb(79, 70, 229);
            btnSaveInvoice.HoverColor = Color.FromArgb(67, 56, 202);
            btnSaveInvoice.SetBounds(0, 4, 150, 40);
            btnSaveInvoice.Click += (s, e) => SaveCurrentInvoice(andPrint: false);

            btnSaveAndPrint.Text = "Save & Print (A4)";
            btnSaveAndPrint.Icon = ReiconRenderer.IconType.Printer;
            btnSaveAndPrint.FillColor = Color.FromArgb(16, 185, 129);
            btnSaveAndPrint.HoverColor = Color.FromArgb(5, 150, 105);
            btnSaveAndPrint.SetBounds(160, 4, 165, 40);
            btnSaveAndPrint.Click += (s, e) => SaveCurrentInvoice(andPrint: true);

            btnPreviewInvoice.Text = "Print Preview";
            btnPreviewInvoice.Icon = ReiconRenderer.IconType.Eye;
            btnPreviewInvoice.BorderColor = Color.FromArgb(203, 213, 225);
            btnPreviewInvoice.BorderWidth = 1.2f;
            btnPreviewInvoice.FillColor = Color.White;
            btnPreviewInvoice.TextColor = Color.FromArgb(30, 41, 59);
            btnPreviewInvoice.HoverColor = Color.FromArgb(241, 245, 249);
            btnPreviewInvoice.SetBounds(335, 4, 140, 40);
            btnPreviewInvoice.Click += (s, e) => PreviewCurrentInvoice();

            btnExportPdf.Text = "Export PDF";
            btnExportPdf.Icon = ReiconRenderer.IconType.Document;
            btnExportPdf.BorderColor = Color.FromArgb(203, 213, 225);
            btnExportPdf.BorderWidth = 1.2f;
            btnExportPdf.FillColor = Color.White;
            btnExportPdf.TextColor = Color.FromArgb(30, 41, 59);
            btnExportPdf.HoverColor = Color.FromArgb(241, 245, 249);
            btnExportPdf.SetBounds(485, 4, 135, 40);
            btnExportPdf.Click += (s, e) => ExportCurrentInvoicePdf();

            btnClearInvoice.Text = "Clear Form";
            btnClearInvoice.Icon = ReiconRenderer.IconType.Close;
            btnClearInvoice.BorderColor = Color.FromArgb(226, 232, 240);
            btnClearInvoice.BorderWidth = 1f;
            btnClearInvoice.FillColor = Color.Transparent;
            btnClearInvoice.TextColor = Color.FromArgb(100, 116, 139);
            btnClearInvoice.HoverColor = Color.FromArgb(254, 226, 226);
            btnClearInvoice.SetBounds(630, 4, 120, 40);
            btnClearInvoice.Click += (s, e) => PrepareNewInvoiceForm();

            pnlFooter.Controls.AddRange(new Control[] {
                btnSaveInvoice, btnSaveAndPrint, btnPreviewInvoice, btnExportPdf, btnClearInvoice
            });

            pnlCreateInvoiceView.Controls.Add(pnlFooter);
        }

        private static Panel CreateStyledCardPanel(int x, int y, int w, int h, string sectionTitle, ReiconRenderer.IconType icon)
        {
            var pnl = new Panel
            {
                Bounds = new Rectangle(x, y, w, h),
                Padding = new Padding(16)
            };

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int pw = pnl.Width;
                int ph = pnl.Height;
                if (pw <= 0 || ph <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;
                Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
                Color bdr = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pw - 1, ph - 1, 12);
                using var bg = new SolidBrush(cardBg);
                using var p = new Pen(bdr, 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(p, path);

                // Section Title & Icon
                ReiconRenderer.DrawIcon(g, icon, new Rectangle(18, 14, 18, 18), Color.FromArgb(79, 70, 229), 1.8f);

                using var font = new Font("Segoe UI", 10f, FontStyle.Bold);
                using var brush = new SolidBrush(isDark ? Color.White : Color.FromArgb(15, 23, 42));
                g.DrawString(sectionTitle, font, brush, 42, 13);

                // Thin divider line under title
                using var penDiv = new Pen(bdr, 1f);
                g.DrawLine(penDiv, 16, 38, pw - 16, 38);
            };

            return pnl;
        }

        private static void AddModernField(Panel parent, string labelText, Control ctrl, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                Bounds = new Rectangle(x, y, width, 18),
                ForeColor = ThemeManager.TextSecondary,
                UseMnemonic = false
            };
            ctrl.SetBounds(x, y + 20, width, 26);
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

            _workingItems.Clear();
            dgvInvoiceItems.Rows.Clear();
            txtItemDesc.Clear();
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
            txtOverallDisc.Text = "0.00";
            txtInitialPaymentAmt.Text = "0.00";
            txtInitialPaymentRef.Clear();

            LoadCustomersIntoDropdown();
            RecalculateBill();
        }

        private void LoadCustomersIntoDropdown(long selectCustomerId = 0)
        {
            _allCustomers = DatabaseService.GetCustomers();
            cmbCustomerSelect.Items.Clear();
            cmbCustomerSelect.Items.Add("-- Select Existing Customer --");

            int targetIndex = 0;
            for (int i = 0; i < _allCustomers.Count; i++)
            {
                var c = _allCustomers[i];
                cmbCustomerSelect.Items.Add(c.DisplayName);
                if (c.Id == selectCustomerId)
                {
                    targetIndex = i + 1;
                }
            }
            cmbCustomerSelect.SelectedIndex = targetIndex;
            if (targetIndex == 0) ClearCustomerFields();
        }

        private void OnCustomerSelected()
        {
            int idx = cmbCustomerSelect.SelectedIndex;
            if (idx > 0 && idx - 1 < _allCustomers.Count)
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
                ClearCustomerFields();
            }
        }

        private void ClearCustomerFields()
        {
            txtCustCompany.Clear();
            txtCustPhone.Clear();
            txtCustEmail.Clear();
            txtCustAddress.Clear();
            txtCustGSTIN.Clear();
        }

        private void AddItemToInvoice()
        {
            string sName = cmbServiceName.Text.Trim();
            if (string.IsNullOrWhiteSpace(sName))
            {
                MessageBox.Show("Please enter or select a Service / Item name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbServiceName.Focus();
                return;
            }

            if (!decimal.TryParse(txtItemPrice.Text.Replace("₹", "").Trim(), out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid unit rate.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemPrice.Focus();
                return;
            }

            decimal qty = numItemQty.Value;
            decimal.TryParse(txtItemDiscPct.Text.Replace("%", "").Trim(), out decimal discPct);

            string taxStr = cmbItemTaxPct.SelectedItem?.ToString()?.Replace("%", "") ?? "18";
            decimal.TryParse(taxStr, out decimal taxPct);

            var item = new InvoiceItem
            {
                ServiceName = sName,
                Description = txtItemDesc.Text.Trim(),
                Quantity = qty,
                UnitPrice = price,
                DiscountPercent = discPct,
                TaxPercent = taxPct
            };
            item.CalculateLineTotal();

            _workingItems.Add(item);

            dgvInvoiceItems.Rows.Add(
                item.ServiceName, item.Description, item.Quantity,
                $"₹{item.UnitPrice:N2}", $"₹{item.DiscountAmount:N2}",
                $"{item.TaxPercent}%", $"₹{item.LineTotal:N2}"
            );

            cmbServiceName.Text = string.Empty;
            txtItemDesc.Clear();
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
            numItemQty.Value = 1;

            RecalculateBill();
        }

        private void RemoveSelectedItem()
        {
            if (dgvInvoiceItems.SelectedRows.Count > 0)
            {
                int idx = dgvInvoiceItems.SelectedRows[0].Index;
                if (idx >= 0 && idx < _workingItems.Count)
                {
                    _workingItems.RemoveAt(idx);
                    dgvInvoiceItems.Rows.RemoveAt(idx);
                    RecalculateBill();
                }
            }
        }

        private void RecalculateBill()
        {
            var inv = BuildCurrentInvoiceObject();
            inv.RecalculateTotals();

            lblCalcSubtotal.Text = $"₹{inv.Subtotal:N2}";
            lblCalcItemDisc.Text = $"-₹{inv.ItemDiscountTotal:N2}";
            lblCalcTaxable.Text = $"₹{inv.TaxableAmount:N2}";

            if (inv.TaxType == "GST_INTRA")
            {
                lblCalcCgst.Visible = true;
                lblCalcSgst.Visible = true;
                lblCalcIgst.Visible = false;
                lblCalcCgst.Text = $"₹{inv.CgstAmount:N2} (9%)";
                lblCalcSgst.Text = $"₹{inv.SgstAmount:N2} (9%)";
            }
            else if (inv.TaxType == "GST_INTER")
            {
                lblCalcCgst.Visible = false;
                lblCalcSgst.Visible = false;
                lblCalcIgst.Visible = true;
                lblCalcIgst.Text = $"₹{inv.IgstAmount:N2} (18%)";
            }
            else
            {
                lblCalcCgst.Visible = false;
                lblCalcSgst.Visible = false;
                lblCalcIgst.Visible = false;
            }

            lblCalcGrandTotal.Text = $"₹{inv.GrandTotal:N2}";

            decimal.TryParse(txtInitialPaymentAmt.Text.Replace("₹", "").Trim(), out decimal initialPay);
            inv.AmountPaid = Math.Min(inv.GrandTotal, Math.Max(0m, initialPay));
            inv.BalanceDue = Math.Max(0m, inv.GrandTotal - inv.AmountPaid);

            lblCalcBalanceDue.Text = $"₹{inv.BalanceDue:N2}";
            lblCalcBalanceDue.ForeColor = inv.BalanceDue > 0 ? Color.FromArgb(225, 29, 72) : Color.FromArgb(16, 185, 129);
        }

        private Invoice BuildCurrentInvoiceObject()
        {
            var inv = new Invoice
            {
                InvoiceNumber = txtInvNumber.Text.Trim(),
                InvoiceDate = dtpInvDate.Value.Date,
                DueDate = dtpDueDate.Value.Date,
                PaymentTerms = cmbPaymentTerms.SelectedItem?.ToString() ?? "Net 15",
                TaxType = cmbTaxScheme.SelectedIndex switch
                {
                    0 => "GST_INTRA",
                    1 => "GST_INTER",
                    _ => "NONE"
                }
            };

            int custIdx = cmbCustomerSelect.SelectedIndex;
            if (custIdx > 0 && custIdx - 1 < _allCustomers.Count)
            {
                var c = _allCustomers[custIdx - 1];
                inv.CustomerId = c.Id;
                inv.CustomerName = c.Name;
                inv.CustomerCompany = c.CompanyName;
                inv.CustomerPhone = c.Phone;
                inv.CustomerEmail = c.Email;
                inv.CustomerAddress = c.BillingAddress;
                inv.CustomerGSTIN = c.GSTIN;
            }
            else
            {
                inv.CustomerName = string.IsNullOrWhiteSpace(txtCustCompany.Text) ? "Walk-in Customer" : txtCustCompany.Text.Trim();
                inv.CustomerCompany = txtCustCompany.Text.Trim();
                inv.CustomerPhone = txtCustPhone.Text.Trim();
                inv.CustomerEmail = txtCustEmail.Text.Trim();
                inv.CustomerAddress = txtCustAddress.Text.Trim();
                inv.CustomerGSTIN = txtCustGSTIN.Text.Trim();
            }

            decimal.TryParse(txtOverallDisc.Text.Replace("₹", "").Trim(), out decimal overallDisc);
            inv.OverallDiscount = Math.Max(0m, overallDisc);

            foreach (var item in _workingItems)
            {
                inv.Items.Add(item);
            }

            decimal.TryParse(txtInitialPaymentAmt.Text.Replace("₹", "").Trim(), out decimal initPaid);
            inv.AmountPaid = Math.Max(0m, initPaid);

            inv.RecalculateTotals();
            return inv;
        }

        private void SaveCurrentInvoice(bool andPrint)
        {
            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one billable service or item to the invoice.", "Empty Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var inv = BuildCurrentInvoiceObject();

            PaymentRecord? initialPayment = null;
            if (inv.AmountPaid > 0)
            {
                initialPayment = new PaymentRecord
                {
                    Amount = inv.AmountPaid,
                    PaymentMethod = cmbInitialPaymentMode.SelectedItem?.ToString() ?? "Bank Transfer",
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
        //  BUILD TAB 3: INVOICES DIRECTORY VIEW (PRO TABLE & FILTERS)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildInvoicesListView()
        {
            pnlInvoicesListView.Dock = DockStyle.Fill;
            pnlInvoicesListView.Padding = new Padding(18);

            // Filter Header Card
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 4, 0, 10)
            };

            txtInvoiceSearch.PlaceholderText = "Search by Invoice #, client name, company, or GSTIN...";
            txtInvoiceSearch.SetBounds(0, 6, 320, 28);
            txtInvoiceSearch.TextChanged += (s, e) => RefreshInvoicesGrid();

            cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatusFilter.Items.AddRange(new object[] { "All Statuses", "Paid", "Partially Paid", "Unpaid", "Overdue", "Cancelled" });
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SetBounds(330, 6, 140, 28);
            cmbStatusFilter.SelectedIndexChanged += (s, e) => RefreshInvoicesGrid();

            chkFilterDates.Text = "Filter Dates:";
            chkFilterDates.SetBounds(480, 8, 95, 24);
            chkFilterDates.CheckedChanged += (s, e) => RefreshInvoicesGrid();

            dtpFilterFrom.Format = DateTimePickerFormat.Short;
            dtpFilterFrom.Value = DateTime.Today.AddDays(-30);
            dtpFilterFrom.SetBounds(580, 6, 105, 28);
            dtpFilterFrom.ValueChanged += (s, e) => { if (chkFilterDates.Checked) RefreshInvoicesGrid(); };

            dtpFilterTo.Format = DateTimePickerFormat.Short;
            dtpFilterTo.Value = DateTime.Today;
            dtpFilterTo.SetBounds(695, 6, 105, 28);
            dtpFilterTo.ValueChanged += (s, e) => { if (chkFilterDates.Checked) RefreshInvoicesGrid(); };

            btnFilterReset.Text = "Reset Filters";
            btnFilterReset.Icon = ReiconRenderer.IconType.Close;
            btnFilterReset.FillColor = Color.FromArgb(241, 245, 249);
            btnFilterReset.TextColor = Color.FromArgb(71, 85, 105);
            btnFilterReset.BorderColor = Color.FromArgb(203, 213, 225);
            btnFilterReset.BorderWidth = 1f;
            btnFilterReset.SetBounds(810, 6, 120, 28);
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

            // Action Bar for Table
            var pnlGridActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(0, 0, 0, 8)
            };

            btnGridOpenInvoice.Text = "View Details";
            btnGridOpenInvoice.Icon = ReiconRenderer.IconType.Eye;
            btnGridOpenInvoice.FillColor = Color.FromArgb(79, 70, 229);
            btnGridOpenInvoice.SetBounds(0, 2, 125, 34);
            btnGridOpenInvoice.Click += (s, e) => OpenSelectedInvoice();

            btnGridRecordPay.Text = "Record Payment";
            btnGridRecordPay.Icon = ReiconRenderer.IconType.Wallet;
            btnGridRecordPay.FillColor = Color.FromArgb(16, 185, 129);
            btnGridRecordPay.SetBounds(135, 2, 145, 34);
            btnGridRecordPay.Click += (s, e) => RecordPaymentForSelected();

            btnGridPrint.Text = "Print Direct";
            btnGridPrint.Icon = ReiconRenderer.IconType.Printer;
            btnGridPrint.BorderColor = Color.FromArgb(203, 213, 225);
            btnGridPrint.BorderWidth = 1.2f;
            btnGridPrint.FillColor = Color.White;
            btnGridPrint.TextColor = Color.FromArgb(30, 41, 59);
            btnGridPrint.HoverColor = Color.FromArgb(241, 245, 249);
            btnGridPrint.SetBounds(290, 2, 125, 34);
            btnGridPrint.Click += (s, e) => PrintSelectedInvoice();

            btnGridExportPdf.Text = "Export PDF";
            btnGridExportPdf.Icon = ReiconRenderer.IconType.Document;
            btnGridExportPdf.BorderColor = Color.FromArgb(203, 213, 225);
            btnGridExportPdf.BorderWidth = 1.2f;
            btnGridExportPdf.FillColor = Color.White;
            btnGridExportPdf.TextColor = Color.FromArgb(30, 41, 59);
            btnGridExportPdf.HoverColor = Color.FromArgb(241, 245, 249);
            btnGridExportPdf.SetBounds(425, 2, 120, 34);
            btnGridExportPdf.Click += (s, e) => ExportSelectedPdf();

            btnGridCancel.Text = "Cancel Invoice";
            btnGridCancel.Icon = ReiconRenderer.IconType.Close;
            btnGridCancel.BorderColor = Color.FromArgb(244, 63, 94);
            btnGridCancel.BorderWidth = 1.2f;
            btnGridCancel.FillColor = Color.White;
            btnGridCancel.TextColor = Color.FromArgb(225, 29, 72);
            btnGridCancel.HoverColor = Color.FromArgb(255, 241, 242);
            btnGridCancel.SetBounds(555, 2, 135, 34);
            btnGridCancel.Click += (s, e) => CancelSelectedInvoice();

            pnlGridActions.Controls.AddRange(new Control[] {
                btnGridOpenInvoice, btnGridRecordPay, btnGridPrint, btnGridExportPdf, btnGridCancel
            });

            // Modern Invoices Grid
            FormatModernTable(dgvAllInvoices);
            dgvAllInvoices.Dock = DockStyle.Fill;

            dgvAllInvoices.Columns.Add("InvNo", "INVOICE #");
            dgvAllInvoices.Columns.Add("Customer", "CUSTOMER / CLIENT");
            dgvAllInvoices.Columns.Add("Date", "INVOICE DATE");
            dgvAllInvoices.Columns.Add("DueDate", "DUE DATE");
            dgvAllInvoices.Columns.Add("Total", "GRAND TOTAL (₹)");
            dgvAllInvoices.Columns.Add("Paid", "PAID (₹)");
            dgvAllInvoices.Columns.Add("Balance", "BALANCE DUE (₹)");
            dgvAllInvoices.Columns.Add("Status", "STATUS");

            dgvAllInvoices.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvAllInvoices.Columns["Paid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvAllInvoices.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvAllInvoices.CellDoubleClick += (s, e) => OpenSelectedInvoice();

            pnlInvoicesListView.Controls.Add(dgvAllInvoices);
            pnlInvoicesListView.Controls.Add(pnlGridActions);
            pnlInvoicesListView.Controls.Add(pnlFilter);
        }

        private void RefreshInvoicesGrid()
        {
            string search = txtInvoiceSearch.Text.Trim();
            string status = cmbStatusFilter.SelectedItem?.ToString() ?? "All Statuses";
            if (status.StartsWith("All")) status = "All";

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
                MessageBox.Show("This invoice is already fully settled and paid.", "Fully Paid", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        //  BUILD TAB 4: CUSTOMERS VIEW (PRO CLIENT LEDGER)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCustomersView()
        {
            pnlCustomersView.Dock = DockStyle.Fill;
            pnlCustomersView.Padding = new Padding(18);

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(0, 4, 0, 8)
            };

            txtCustDirSearch.PlaceholderText = "Search customers by name, company, phone, email, or GSTIN...";
            txtCustDirSearch.SetBounds(0, 6, 380, 28);
            txtCustDirSearch.TextChanged += (s, e) => RefreshCustomersGrid();

            btnCustDirAdd.Text = "+ Add Customer";
            btnCustDirAdd.Icon = ReiconRenderer.IconType.Plus;
            btnCustDirAdd.FillColor = Color.FromArgb(79, 70, 229);
            btnCustDirAdd.SetBounds(390, 4, 150, 34);
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
            btnCustDirEdit.Icon = ReiconRenderer.IconType.Customers;
            btnCustDirEdit.BorderColor = Color.FromArgb(203, 213, 225);
            btnCustDirEdit.BorderWidth = 1.2f;
            btnCustDirEdit.FillColor = Color.White;
            btnCustDirEdit.TextColor = Color.FromArgb(30, 41, 59);
            btnCustDirEdit.HoverColor = Color.FromArgb(241, 245, 249);
            btnCustDirEdit.SetBounds(550, 4, 140, 34);
            btnCustDirEdit.Click += (s, e) => EditSelectedCustomer();

            pnlTop.Controls.AddRange(new Control[] { txtCustDirSearch, btnCustDirAdd, btnCustDirEdit });

            // Customers Table
            FormatModernTable(dgvAllCustomers);
            dgvAllCustomers.Dock = DockStyle.Fill;

            dgvAllCustomers.Columns.Add("Name", "CLIENT / PERSON NAME");
            dgvAllCustomers.Columns.Add("Company", "COMPANY / ORGANIZATION");
            dgvAllCustomers.Columns.Add("Phone", "PHONE NUMBER");
            dgvAllCustomers.Columns.Add("Email", "EMAIL ADDRESS");
            dgvAllCustomers.Columns.Add("Balance", "OUTSTANDING DUE (₹)");
            dgvAllCustomers.Columns.Add("GSTIN", "GSTIN NUMBER");

            dgvAllCustomers.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
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

                if (c.OutstandingBalance > 0)
                {
                    dgvAllCustomers.Rows[r].Cells["Balance"].Style.ForeColor = Color.FromArgb(225, 29, 72);
                }
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
        //  MODERN TABLE FORMATTING & CUSTOM STATUS PILL BADGE RENDERING
        // ═══════════════════════════════════════════════════════════════════════════
        private void FormatModernTable(DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 40;
            dgv.Cursor = Cursors.Hand;

            dgv.CellPainting += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgv.Columns[e.ColumnIndex].Name.Contains("Status", StringComparison.OrdinalIgnoreCase))
                {
                    PaintStatusCellBadge(e, ThemeManager.IsDarkMode);
                }
            };
        }

        private static void PaintStatusCellBadge(DataGridViewCellPaintingEventArgs e, bool isDark)
        {
            if (e.RowIndex < 0 || e.Value == null || e.Graphics == null) return;
            e.PaintBackground(e.ClipBounds, true);

            string status = e.Value.ToString() ?? "";
            Color bgCol;
            Color textCol;
            Color dotCol;

            switch (status)
            {
                case "Paid":
                    bgCol = isDark ? Color.FromArgb(6, 78, 59) : Color.FromArgb(236, 253, 245);
                    textCol = isDark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(4, 120, 87);
                    dotCol = Color.FromArgb(16, 185, 129);
                    break;
                case "Partially Paid":
                    bgCol = isDark ? Color.FromArgb(30, 58, 138) : Color.FromArgb(239, 246, 255);
                    textCol = isDark ? Color.FromArgb(96, 165, 250) : Color.FromArgb(29, 78, 216);
                    dotCol = Color.FromArgb(59, 130, 246);
                    break;
                case "Unpaid":
                    bgCol = isDark ? Color.FromArgb(120, 53, 15) : Color.FromArgb(254, 243, 199);
                    textCol = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(180, 83, 9);
                    dotCol = Color.FromArgb(245, 158, 11);
                    break;
                case "Overdue":
                    bgCol = isDark ? Color.FromArgb(136, 19, 55) : Color.FromArgb(255, 241, 242);
                    textCol = isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(190, 18, 60);
                    dotCol = Color.FromArgb(244, 63, 94);
                    break;
                default: // Cancelled / Draft
                    bgCol = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                    textCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
                    dotCol = Color.FromArgb(148, 163, 184);
                    break;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int pillH = 24;
            int pillW = Math.Min(e.CellBounds.Width - 20, 110);
            int pillX = e.CellBounds.X + (e.CellBounds.Width - pillW) / 2;
            int pillY = e.CellBounds.Y + (e.CellBounds.Height - pillH) / 2;

            using (var path = GraphicsExtensions.CreateRoundedRectanglePath(pillX, pillY, pillW, pillH, pillH / 2))
            using (var brush = new SolidBrush(bgCol))
            using (var borderPen = new Pen(dotCol, 0.8f))
            {
                g.FillPath(brush, path);
                g.DrawPath(borderPen, path);
            }

            // Dot indicator
            using (var dotBrush = new SolidBrush(dotCol))
            {
                g.FillEllipse(dotBrush, pillX + 9, pillY + (pillH - 6) / 2, 6, 6);
            }

            // Text
            using (var font = new Font("Segoe UI", 8.2f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(textCol))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var textRect = new Rectangle(pillX + 16, pillY, pillW - 18, pillH);
                g.DrawString(status, font, textBrush, textRect, sf);
            }

            e.Handled = true;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME APPLICATION (LIGHT / DARK MODES)
        // ═══════════════════════════════════════════════════════════════════════════
        public void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            Color pageBg = isDark ? Color.FromArgb(15, 17, 26) : Color.FromArgb(248, 250, 252);
            Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
            Color textPrimary = isDark ? Color.White : Color.FromArgb(15, 23, 42);
            Color textSecondary = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
            Color borderCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
            Color inputBg = isDark ? Color.FromArgb(30, 35, 50) : Color.White;

            BackColor = pageBg;
            pnlNavHeader.BackColor = isDark ? Color.FromArgb(18, 20, 30) : Color.White;
            pnlViewDeck.BackColor = pageBg;

            pnlDashboardView.BackColor = pageBg;
            pnlCreateInvoiceView.BackColor = pageBg;
            pnlInvoicesListView.BackColor = pageBg;
            pnlCustomersView.BackColor = pageBg;

            pnlNavPillTrack.Invalidate(true);
            pnlDashCardsHost.Invalidate(true);

            // Style all DataGridViews
            foreach (var dgv in new[] { dgvRecentSales, dgvInvoiceItems, dgvAllInvoices, dgvAllCustomers })
            {
                dgv.BackgroundColor = cardBg;
                dgv.GridColor = borderCol;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(26, 29, 43) : Color.FromArgb(248, 250, 252);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = textSecondary;
                dgv.DefaultCellStyle.BackColor = cardBg;
                dgv.DefaultCellStyle.ForeColor = textPrimary;
                dgv.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(38, 46, 72) : Color.FromArgb(238, 242, 255);
                dgv.DefaultCellStyle.SelectionForeColor = textPrimary;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(20, 23, 33) : Color.FromArgb(250, 250, 252);
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = textPrimary;
            }

            // Style inputs recursively
            ApplyInputsTheme(pnlCreateInvoiceView, inputBg, textPrimary, textSecondary, borderCol);
            ApplyInputsTheme(pnlInvoicesListView, inputBg, textPrimary, textSecondary, borderCol);
            ApplyInputsTheme(pnlCustomersView, inputBg, textPrimary, textSecondary, borderCol);

            Invalidate(true);
        }

        private static void ApplyInputsTheme(Control parent, Color inputBg, Color textPrimary, Color textSecondary, Color borderCol)
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
                    if (lbl.Font.Bold && lbl.Font.Size > 10f)
                    {
                        // Headers
                    }
                    else if (lbl.ForeColor != Color.FromArgb(16, 185, 129) && lbl.ForeColor != Color.FromArgb(225, 29, 72))
                    {
                        lbl.ForeColor = textPrimary;
                    }
                }
                else if (c.HasChildren)
                {
                    ApplyInputsTheme(c, inputBg, textPrimary, textSecondary, borderCol);
                }
            }
        }
    }
}
