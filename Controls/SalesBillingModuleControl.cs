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

        //  MASTER INVOICES / SALES BILLING CONTROLS (SECTIONS A, B, C, D)

        // ═══════════════════════════════════════════════════════════════════════════

        // Section A: Page Header

        private readonly Panel pnlPageHeader = new();

        private readonly Label lblPageTitle = new();

        private readonly Label lblPageSubtitle = new();



        // Section B: 6 Aligned Summary Cards

        private readonly Panel pnlDashCardsHost = new();

        private readonly Panel pnlCardTotalSales = new();

        private readonly Panel pnlCardTotalInvoices = new();

        private readonly Panel pnlCardPaidInvoices = new();

        private readonly Panel pnlCardUnpaidInvoices = new();

        private readonly Panel pnlCardOutstandingDue = new();

        private readonly Panel pnlCardTodayBillings = new();



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



        // Section C: Search & Filter Toolbar

        private readonly Panel pnlFilterToolbar = new();

        private readonly Panel pnlSearchBox = new();

        private readonly TextBox txtRecentSearch = new();

        private readonly Label lblDateToSep = new();

        private readonly TeamPillButton btnFilterAllInvoices = new();



        // Section D: All Invoices Master Card

        private readonly Panel pnlRecentSalesCard = new();

        private readonly Panel pnlRecentCardHeader = new();

        private readonly Panel pnlRecentGridHost = new();

        private readonly Panel pnlRecentCardFooter = new();

        private readonly Label lblRecentTitle = new();

        private readonly Label lblRecentBadge = new();

        private readonly Label lblRecentFooterHint = new();

        private readonly Label lblRecentFooterStats = new();

        private List<Invoice> _cachedRecentInvoices = new();



        private readonly DataGridView dgvRecentSales = new();

        private readonly TeamPillButton btnQuickNewInv = new();

        private readonly TeamPillButton btnQuickViewInvs = new();

        private readonly TeamPillButton btnQuickCustomers = new();



        // Dedicated New Sales Invoice Page (Matching Reference Screenshot)
        private readonly NewSalesInvoiceControl _inlineNewInvoice = new();



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

            _cardsData[0] = new MetricCardData { Title = "TOTAL SALES", Value = "₹0.00", Badge = "● All-Time", AccentColor = Color.FromArgb(139, 92, 246), Icon = ReiconRenderer.IconType.TrendingUp };

            _cardsData[1] = new MetricCardData { Title = "TOTAL INVOICES", Value = "0", Badge = "● Recorded", AccentColor = Color.FromArgb(79, 70, 229), Icon = ReiconRenderer.IconType.Receipt };

            _cardsData[2] = new MetricCardData { Title = "PAID INVOICES", Value = "0", Badge = "● Settled", AccentColor = Color.FromArgb(16, 185, 129), Icon = ReiconRenderer.IconType.Check };

            _cardsData[3] = new MetricCardData { Title = "PENDING INVOICES", Value = "0", Badge = "● Due", AccentColor = Color.FromArgb(245, 158, 11), Icon = ReiconRenderer.IconType.Warning };

            _cardsData[4] = new MetricCardData { Title = "OUTSTANDING DUE", Value = "₹0.00", Badge = "● Receivables", AccentColor = Color.FromArgb(244, 63, 94), Icon = ReiconRenderer.IconType.TrendingDown };

            _cardsData[5] = new MetricCardData { Title = "TODAY'S BILLINGS", Value = "₹0.00", Badge = "● Today", AccentColor = Color.FromArgb(14, 165, 233), Icon = ReiconRenderer.IconType.Wallet };

        }



        private void BuildModuleLayout()

        {

            Controls.Clear();



            // 1. Top Sub-Navigation Bar (Segmented Capsule)

            pnlNavHeader.Dock = DockStyle.Fill;

            pnlNavHeader.Margin = Padding.Empty;

            pnlNavHeader.Padding = new Padding(18, 5, 18, 5);



            pnlNavPillTrack.Dock = DockStyle.Left;

            pnlNavPillTrack.Width = 510;

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



            SetupTabButton(btnTabDashboard, "Invoices", "Dashboard", ReiconRenderer.IconType.Receipt, 140);

            SetupTabButton(btnTabCreateInvoice, "Create Invoice", "CreateInvoice", ReiconRenderer.IconType.Plus, 155);

            SetupTabButton(btnTabCustomers, "Customers Directory", "Customers", ReiconRenderer.IconType.Customers, 175);



            pnlNavPillTrack.Controls.Clear();

            pnlNavPillTrack.Controls.AddRange(new Control[] {

                btnTabCustomers, btnTabCreateInvoice, btnTabDashboard

            });



            pnlNavHeader.Controls.Clear();

            pnlNavHeader.Controls.Add(pnlNavPillTrack);



            // 2. Main View Deck

            pnlViewDeck.Dock = DockStyle.Fill;

            pnlViewDeck.Margin = Padding.Empty;

            pnlViewDeck.Padding = Padding.Empty;

            pnlViewDeck.Controls.Clear();

            pnlViewDeck.Controls.AddRange(new Control[] {

                pnlDashboardView, pnlCreateInvoiceView, pnlInvoicesListView, pnlCustomersView

            });



            var tlpModuleMaster = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                Margin = Padding.Empty,

                Padding = Padding.Empty

            };

            tlpModuleMaster.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            tlpModuleMaster.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));  // Row 0: Sub-navigation bar

            tlpModuleMaster.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Row 1: Active View Deck



            tlpModuleMaster.Controls.Add(pnlNavHeader, 0, 0);

            tlpModuleMaster.Controls.Add(pnlViewDeck, 0, 1);

            Controls.Add(tlpModuleMaster);



            BuildDashboardView();

            BuildCreateInvoiceView();

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



                bool isActive = (_currentTab == tabKey || (_currentTab == "Invoices" && tabKey == "Dashboard") || (_currentTab == "Dashboard" && tabKey == "Invoices"));

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



            pnlDashboardView.Visible = (tabKey == "Dashboard" || tabKey == "Invoices");

            pnlCreateInvoiceView.Visible = (tabKey == "CreateInvoice");

            pnlInvoicesListView.Visible = false;

            pnlCustomersView.Visible = (tabKey == "Customers");



            pnlNavPillTrack.Invalidate(true);



            if (tabKey == "Dashboard" || tabKey == "Invoices") LoadDashboardData();

            else if (tabKey == "CreateInvoice") PrepareNewInvoiceForm();

            else if (tabKey == "Customers") RefreshCustomersGrid();

        }

        public void FilterInvoicesByStatus(string status)
        {
            SwitchTab("Dashboard");
            if (cmbStatusFilter != null)
            {
                for (int i = 0; i < cmbStatusFilter.Items.Count; i++)
                {
                    if (cmbStatusFilter.Items[i]?.ToString()?.IndexOf(status, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        cmbStatusFilter.SelectedIndex = i;
                        break;
                    }
                }
            }
        }



        // ═══════════════════════════════════════════════════════════════════════════

        //  BUILD TAB 1: SALES DASHBOARD VIEW (PRO AESTHETICS & ELEVATED SAAS CARD)

        // ═══════════════════════════════════════════════════════════════════════════

        private void BuildDashboardView()

        {

            pnlDashboardView.Dock = DockStyle.Fill;

            pnlDashboardView.AutoScroll = false;

            pnlDashboardView.Padding = new Padding(18, 12, 18, 14);

            pnlDashboardView.Controls.Clear();



            var tlpInvoiceMaster = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 4,

                Padding = Padding.Empty,

                Margin = Padding.Empty

            };

            tlpInvoiceMaster.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            tlpInvoiceMaster.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));  // Row 0: Section A (Page Header)

            tlpInvoiceMaster.RowStyles.Add(new RowStyle(SizeType.Absolute, 118f)); // Row 1: Section B (Summary Cards)

            tlpInvoiceMaster.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));  // Row 2: Section C (Filter Toolbar)

            tlpInvoiceMaster.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // Row 3: Section D (Invoice List Card)



            // ─────────────────────────────────────────────────────────────────────────

            // SECTION A: PAGE HEADER

            // ─────────────────────────────────────────────────────────────────────────

            pnlPageHeader.Dock = DockStyle.Fill;

            pnlPageHeader.Margin = new Padding(0, 0, 0, 6);

            pnlPageHeader.Controls.Clear();



            lblPageTitle.Text = "Invoices";

            lblPageTitle.Font = new Font("Segoe UI", 15.5f, FontStyle.Bold);

            lblPageTitle.AutoSize = true;

            lblPageTitle.Location = new Point(0, 0);



            lblPageSubtitle.Text = "Manage sales, billing and customer invoices";

            lblPageSubtitle.Font = new Font("Segoe UI", 8.8f, FontStyle.Regular);

            lblPageSubtitle.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

            lblPageSubtitle.AutoSize = true;

            lblPageSubtitle.Location = new Point(0, 27);



            var pnlHeaderRightActions = new Panel

            {

                Dock = DockStyle.Right,

                Width = 310

            };



            btnQuickNewInv.Text = "+ New Invoice";

            btnQuickNewInv.Icon = ReiconRenderer.IconType.Plus;

            btnQuickNewInv.IconColor = Color.White;

            btnQuickNewInv.FillColor = Color.FromArgb(79, 70, 229);

            btnQuickNewInv.HoverColor = Color.FromArgb(67, 56, 202);

            btnQuickNewInv.TextColor = Color.White;

            btnQuickNewInv.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);

            btnQuickNewInv.Click -= OnNewInvoiceClicked;

            btnQuickNewInv.Click += OnNewInvoiceClicked;



            btnQuickCustomers.Text = "Manage Clients";

            btnQuickCustomers.Icon = ReiconRenderer.IconType.Customers;

            btnQuickCustomers.IconColor = ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

            btnQuickCustomers.BorderColor = ThemeManager.IsDarkMode ? Color.FromArgb(50, 58, 86) : Color.FromArgb(203, 213, 225);

            btnQuickCustomers.BorderWidth = 1.2f;

            btnQuickCustomers.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(28, 33, 48) : Color.White;

            btnQuickCustomers.TextColor = ThemeManager.IsDarkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59);

            btnQuickCustomers.HoverColor = ThemeManager.IsDarkMode ? Color.FromArgb(38, 44, 64) : Color.FromArgb(241, 245, 249);

            btnQuickCustomers.Font = new Font("Segoe UI", 9f);

            btnQuickCustomers.Click -= OnCustomersClicked;

            btnQuickCustomers.Click += OnCustomersClicked;



            void LayoutPageHeaderActions()

            {

                int btnH = 36;

                int btnY = (pnlHeaderRightActions.Height - btnH) / 2;

                int newInvW = 142;

                int custW = 142;

                btnQuickNewInv.SetBounds(pnlHeaderRightActions.Width - newInvW, btnY, newInvW, btnH);

                btnQuickCustomers.SetBounds(btnQuickNewInv.Left - 10 - custW, btnY, custW, btnH);

            }

            pnlHeaderRightActions.Resize += (s, e) => LayoutPageHeaderActions();

            LayoutPageHeaderActions();



            pnlHeaderRightActions.Controls.Clear();

            pnlHeaderRightActions.Controls.AddRange(new Control[] { btnQuickCustomers, btnQuickNewInv });

            pnlPageHeader.Controls.AddRange(new Control[] { lblPageTitle, lblPageSubtitle, pnlHeaderRightActions });



            // ─────────────────────────────────────────────────────────────────────────

            // SECTION B: SUMMARY CARDS (6 ALIGNED CARDS)

            // ─────────────────────────────────────────────────────────────────────────

            pnlDashCardsHost.Dock = DockStyle.Fill;

            pnlDashCardsHost.Margin = new Padding(0, 0, 0, 8);

            pnlDashCardsHost.Padding = Padding.Empty;

            pnlDashCardsHost.Controls.Clear();



            var tlpCards = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 6,

                RowCount = 1,

                Margin = Padding.Empty,

                Padding = Padding.Empty

            };

            for (int i = 0; i < 6; i++)

                tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));



            Panel[] cardPanels = { pnlCardTotalSales, pnlCardTotalInvoices, pnlCardPaidInvoices, pnlCardUnpaidInvoices, pnlCardOutstandingDue, pnlCardTodayBillings };

            for (int i = 0; i < 6; i++)

            {

                int cardIdx = i;

                var pnl = cardPanels[i];

                pnl.Dock = DockStyle.Fill;

                pnl.Margin = new Padding(i == 0 ? 0 : 4, 0, i == 5 ? 0 : 4, 0);

                pnl.MouseEnter -= (s, e) => OnCardHover(cardIdx, true, pnl);

                pnl.MouseEnter += (s, e) => OnCardHover(cardIdx, true, pnl);

                pnl.MouseLeave -= (s, e) => OnCardHover(cardIdx, false, pnl);

                pnl.MouseLeave += (s, e) => OnCardHover(cardIdx, false, pnl);

                pnl.Paint -= (s, e) => PaintMetricCard(e.Graphics, pnl, _cardsData[cardIdx]);

                pnl.Paint += (s, e) => PaintMetricCard(e.Graphics, pnl, _cardsData[cardIdx]);

                tlpCards.Controls.Add(pnl, i, 0);

            }

            pnlDashCardsHost.Controls.Add(tlpCards);



            // ─────────────────────────────────────────────────────────────────────────

            // SECTION C: SEARCH AND FILTER TOOLBAR

            // ─────────────────────────────────────────────────────────────────────────

            pnlFilterToolbar.Dock = DockStyle.Fill;

            pnlFilterToolbar.Margin = new Padding(0, 0, 0, 8);

            pnlFilterToolbar.Padding = Padding.Empty;

            pnlFilterToolbar.Controls.Clear();



            pnlSearchBox.Paint -= OnSearchBoxPaint;

            pnlSearchBox.Paint += OnSearchBoxPaint;



            txtRecentSearch.PlaceholderText = "Search invoice, customer...";

            txtRecentSearch.Font = new Font("Segoe UI", 9.2f);

            txtRecentSearch.BorderStyle = BorderStyle.None;

            txtRecentSearch.BackColor = Color.FromArgb(248, 250, 252);

            txtRecentSearch.TextChanged -= OnSearchFilterChanged;

            txtRecentSearch.TextChanged += OnSearchFilterChanged;



            pnlSearchBox.Controls.Clear();

            pnlSearchBox.Controls.Add(txtRecentSearch);



            cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbStatusFilter.Font = new Font("Segoe UI", 9.2f);

            cmbStatusFilter.Items.Clear();

            cmbStatusFilter.Items.AddRange(new object[] { "All Statuses", "Paid", "Partially Paid", "Pending", "Overdue", "Cancelled" });

            cmbStatusFilter.SelectedIndex = 0;

            cmbStatusFilter.SelectedIndexChanged -= OnSearchFilterChanged;

            cmbStatusFilter.SelectedIndexChanged += OnSearchFilterChanged;



            chkFilterDates.Text = "Filter Dates:";

            chkFilterDates.Font = new Font("Segoe UI", 9f);

            chkFilterDates.AutoSize = false;

            chkFilterDates.CheckedChanged -= OnDateFilterCheckChanged;

            chkFilterDates.CheckedChanged += OnDateFilterCheckChanged;



            dtpFilterFrom.Format = DateTimePickerFormat.Short;

            dtpFilterFrom.Font = new Font("Segoe UI", 9.2f);

            dtpFilterFrom.Value = DateTime.Today.AddDays(-30);

            dtpFilterFrom.Enabled = false;

            dtpFilterFrom.ValueChanged -= OnDateFilterValChanged;

            dtpFilterFrom.ValueChanged += OnDateFilterValChanged;



            lblDateToSep.Text = "to";

            lblDateToSep.Font = new Font("Segoe UI", 8.8f);

            lblDateToSep.TextAlign = ContentAlignment.MiddleCenter;

            lblDateToSep.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);



            dtpFilterTo.Format = DateTimePickerFormat.Short;

            dtpFilterTo.Font = new Font("Segoe UI", 9.2f);

            dtpFilterTo.Value = DateTime.Today;

            dtpFilterTo.Enabled = false;

            dtpFilterTo.ValueChanged -= OnDateFilterValChanged;

            dtpFilterTo.ValueChanged += OnDateFilterValChanged;



            btnFilterReset.Text = "Reset";

            btnFilterReset.Icon = ReiconRenderer.IconType.Close;

            btnFilterReset.FillColor = Color.FromArgb(241, 245, 249);

            btnFilterReset.TextColor = Color.FromArgb(71, 85, 105);

            btnFilterReset.BorderColor = Color.FromArgb(203, 213, 225);

            btnFilterReset.BorderWidth = 1f;

            btnFilterReset.Font = new Font("Segoe UI", 8.8f);

            btnFilterReset.Click -= OnFilterResetClicked;

            btnFilterReset.Click += OnFilterResetClicked;



            btnFilterAllInvoices.Text = "All Invoices";

            btnFilterAllInvoices.Icon = ReiconRenderer.IconType.Receipt;

            btnFilterAllInvoices.FillColor = ThemeManager.PrimaryAccent;

            btnFilterAllInvoices.HoverColor = ThemeManager.AccentHover;

            btnFilterAllInvoices.TextColor = Color.White;

            btnFilterAllInvoices.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);

            btnFilterAllInvoices.Click -= OnFilterAllInvoicesClicked;

            btnFilterAllInvoices.Click += OnFilterAllInvoicesClicked;



            void LayoutFilterToolbar()

            {

                int h = pnlFilterToolbar.Height;

                int searchH = 34;

                int searchY = (h - searchH) / 2;

                pnlSearchBox.SetBounds(0, searchY, 240, searchH);

                txtRecentSearch.SetBounds(34, (searchH - txtRecentSearch.Height) / 2, pnlSearchBox.Width - 44, txtRecentSearch.Height);



                int comboW = 135;

                int comboH = cmbStatusFilter.Height;

                int comboY = (h - comboH) / 2;

                cmbStatusFilter.SetBounds(pnlSearchBox.Right + 10, comboY, comboW, comboH);



                chkFilterDates.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59);

                chkFilterDates.Location = new Point(cmbStatusFilter.Right + 10, (h - chkFilterDates.Height) / 2);



                int dtpW = 105;

                int dtpY = (h - dtpFilterFrom.Height) / 2;

                dtpFilterFrom.SetBounds(chkFilterDates.Right + 4, dtpY, dtpW, dtpFilterFrom.Height);



                lblDateToSep.SetBounds(dtpFilterFrom.Right + 2, (h - 20) / 2, 18, 20);

                dtpFilterTo.SetBounds(lblDateToSep.Right + 2, dtpY, dtpW, dtpFilterTo.Height);



                int resetW = 80;

                int allW = 105;

                int resetH = 32;

                btnFilterReset.SetBounds(dtpFilterTo.Right + 10, (h - resetH) / 2, resetW, resetH);

                btnFilterAllInvoices.SetBounds(btnFilterReset.Right + 8, (h - resetH) / 2, allW, resetH);

            }

            pnlFilterToolbar.Resize += (s, e) => LayoutFilterToolbar();

            LayoutFilterToolbar();



            pnlFilterToolbar.Controls.Clear();

            pnlFilterToolbar.Controls.AddRange(new Control[] {

                pnlSearchBox, cmbStatusFilter, chkFilterDates, dtpFilterFrom, lblDateToSep, dtpFilterTo, btnFilterReset, btnFilterAllInvoices

            });



            // ─────────────────────────────────────────────────────────────────────────

            // SECTION D: INVOICE LIST SECTION ("ALL INVOICES" CONTAINER CARD)

            // ─────────────────────────────────────────────────────────────────────────

            pnlRecentSalesCard.Dock = DockStyle.Fill;

            pnlRecentSalesCard.Margin = Padding.Empty;

            pnlRecentSalesCard.Padding = new Padding(1);

            pnlRecentSalesCard.Controls.Clear();



            void UpdateCardRegion()

            {

                if (pnlRecentSalesCard.Width > 20 && pnlRecentSalesCard.Height > 20)

                {

                    using var p = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnlRecentSalesCard.Width, pnlRecentSalesCard.Height, 14);

                    pnlRecentSalesCard.Region = new Region(p);

                }

            }

            pnlRecentSalesCard.Resize += (s, e) => UpdateCardRegion();

            pnlRecentSalesCard.HandleCreated += (s, e) => UpdateCardRegion();

            pnlRecentSalesCard.Paint += (s, e) =>

            {

                var g = e.Graphics;

                g.SmoothingMode = SmoothingMode.AntiAlias;

                int cw = pnlRecentSalesCard.Width;

                int ch = pnlRecentSalesCard.Height;

                if (cw <= 0 || ch <= 0) return;



                bool isDark = ThemeManager.IsDarkMode;

                Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;

                Color cardBdr = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);



                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, cw - 1, ch - 1, 14);

                using var bBg = new SolidBrush(cardBg);

                using var pBdr = new Pen(cardBdr, 1.5f);

                g.FillPath(bBg, path);

                g.DrawPath(pBdr, path);

            };



            // Header of Table Card

            pnlRecentCardHeader.Dock = DockStyle.Top;

            pnlRecentCardHeader.Height = 52;

            pnlRecentCardHeader.Padding = new Padding(18, 9, 18, 9);

            pnlRecentCardHeader.Controls.Clear();

            pnlRecentCardHeader.Paint += (s, e) =>

            {

                var g = e.Graphics;

                int hw = pnlRecentCardHeader.Width;

                int hh = pnlRecentCardHeader.Height;

                if (hw <= 0 || hh <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;

                Color lineCol = isDark ? Color.FromArgb(36, 42, 62) : Color.FromArgb(241, 245, 249);

                using var pen = new Pen(lineCol, 1f);

                g.DrawLine(pen, 0, hh - 1, hw, hh - 1);

            };



            lblRecentTitle.Text = "Recent Sales Invoices & Transactions";

            lblRecentTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);

            lblRecentTitle.AutoSize = true;

            lblRecentTitle.Location = new Point(18, 15);

            lblRecentTitle.UseMnemonic = false;



            lblRecentBadge.Text = string.Empty;

            lblRecentBadge.Tag = "● 0 Invoices";

            lblRecentBadge.Font = new Font("Segoe UI", 7.8f, FontStyle.Bold);

            lblRecentBadge.Size = new Size(100, 24);

            lblRecentBadge.TextAlign = ContentAlignment.MiddleCenter;

            lblRecentBadge.Paint += (s, e) =>

            {

                var g = e.Graphics;

                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool isDark = ThemeManager.IsDarkMode;

                Color bg = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(238, 242, 255);

                Color bdr = isDark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(199, 210, 254);

                Color txt = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(79, 70, 229);



                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, lblRecentBadge.Width - 1, lblRecentBadge.Height - 1, 12);

                using var b = new SolidBrush(bg);

                using var p = new Pen(bdr, 1f);

                g.FillPath(b, path);

                g.DrawPath(p, path);



                using var f = new Font("Segoe UI", 7.5f, FontStyle.Bold);

                using var tb = new SolidBrush(txt);

                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                string badgeText = lblRecentBadge.Tag?.ToString() ?? "● 0 Invoices";

                g.DrawString(badgeText, f, tb, new RectangleF(0, 0, lblRecentBadge.Width, lblRecentBadge.Height), sf);

            };



            btnGridOpenInvoice.Text = "View Details";

            btnGridOpenInvoice.Icon = ReiconRenderer.IconType.Eye;

            btnGridOpenInvoice.FillColor = Color.FromArgb(79, 70, 229);

            btnGridOpenInvoice.HoverColor = Color.FromArgb(67, 56, 202);

            btnGridOpenInvoice.TextColor = Color.White;

            btnGridOpenInvoice.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);

            btnGridOpenInvoice.Click -= OnGridOpenInvoiceClicked;

            btnGridOpenInvoice.Click += OnGridOpenInvoiceClicked;



            btnGridRecordPay.Text = "Record Payment";

            btnGridRecordPay.Icon = ReiconRenderer.IconType.Wallet;

            btnGridRecordPay.FillColor = Color.FromArgb(16, 185, 129);

            btnGridRecordPay.HoverColor = Color.FromArgb(5, 150, 105);

            btnGridRecordPay.TextColor = Color.White;

            btnGridRecordPay.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);

            btnGridRecordPay.Click -= OnGridRecordPayClicked;

            btnGridRecordPay.Click += OnGridRecordPayClicked;



            btnGridPrint.Text = "Print";

            btnGridPrint.Icon = ReiconRenderer.IconType.Printer;

            btnGridPrint.BorderColor = Color.FromArgb(203, 213, 225);

            btnGridPrint.BorderWidth = 1.2f;

            btnGridPrint.FillColor = Color.White;

            btnGridPrint.TextColor = Color.FromArgb(30, 41, 59);

            btnGridPrint.HoverColor = Color.FromArgb(241, 245, 249);

            btnGridPrint.Font = new Font("Segoe UI", 8.8f);

            btnGridPrint.Click -= OnGridPrintClicked;

            btnGridPrint.Click += OnGridPrintClicked;



            btnGridExportPdf.Text = "Export PDF";

            btnGridExportPdf.Icon = ReiconRenderer.IconType.Document;

            btnGridExportPdf.BorderColor = Color.FromArgb(203, 213, 225);

            btnGridExportPdf.BorderWidth = 1.2f;

            btnGridExportPdf.FillColor = Color.White;

            btnGridExportPdf.TextColor = Color.FromArgb(30, 41, 59);

            btnGridExportPdf.HoverColor = Color.FromArgb(241, 245, 249);

            btnGridExportPdf.Font = new Font("Segoe UI", 8.8f);

            btnGridExportPdf.Click -= OnGridExportPdfClicked;

            btnGridExportPdf.Click += OnGridExportPdfClicked;



            btnGridCancel.Text = "Cancel";

            btnGridCancel.Icon = ReiconRenderer.IconType.Close;

            btnGridCancel.BorderColor = Color.FromArgb(244, 63, 94);

            btnGridCancel.BorderWidth = 1.2f;

            btnGridCancel.FillColor = Color.White;

            btnGridCancel.TextColor = Color.FromArgb(225, 29, 72);

            btnGridCancel.HoverColor = Color.FromArgb(255, 241, 242);

            btnGridCancel.Font = new Font("Segoe UI", 8.8f);

            btnGridCancel.Click -= OnGridCancelClicked;

            btnGridCancel.Click += OnGridCancelClicked;



            void LayoutTableActions()

            {

                lblRecentTitle.Location = new Point(18, (pnlRecentCardHeader.Height - lblRecentTitle.Height) / 2);

                lblRecentBadge.Location = new Point(lblRecentTitle.Right + 12, (pnlRecentCardHeader.Height - lblRecentBadge.Height) / 2);



                int rightMargin = 16;

                int gap = 8;

                int btnH = 32;

                int btnY = (pnlRecentCardHeader.Height - btnH) / 2;

                int right = pnlRecentCardHeader.ClientSize.Width - rightMargin;



                int cancelW = 85;

                btnGridCancel.SetBounds(right - cancelW, btnY, cancelW, btnH);



                int pdfW = 100;

                btnGridExportPdf.SetBounds(btnGridCancel.Left - gap - pdfW, btnY, pdfW, btnH);



                int printW = 80;

                btnGridPrint.SetBounds(btnGridExportPdf.Left - gap - printW, btnY, printW, btnH);



                int payW = 130;

                btnGridRecordPay.SetBounds(btnGridPrint.Left - gap - payW, btnY, payW, btnH);



                int openW = 110;

                btnGridOpenInvoice.SetBounds(btnGridRecordPay.Left - gap - openW, btnY, openW, btnH);

            }

            pnlRecentCardHeader.Resize += (s, e) => LayoutTableActions();

            LayoutTableActions();



            pnlRecentCardHeader.Controls.Clear();

            pnlRecentCardHeader.Controls.AddRange(new Control[] {

                lblRecentTitle, lblRecentBadge,

                btnGridOpenInvoice, btnGridRecordPay, btnGridPrint, btnGridExportPdf, btnGridCancel

            });



            // Body: DataGridView inside Card Host

            pnlRecentGridHost.Dock = DockStyle.Fill;

            pnlRecentGridHost.Padding = new Padding(1, 0, 1, 0);

            pnlRecentGridHost.Controls.Clear();



            FormatModernTable(dgvRecentSales);

            dgvRecentSales.Dock = DockStyle.Fill;

            dgvRecentSales.Columns.Clear();



            dgvRecentSales.Columns.Add("InvNum", "INVOICE #");

            dgvRecentSales.Columns.Add("Customer", "CUSTOMER / CLIENT");

            dgvRecentSales.Columns.Add("Date", "INVOICE DATE");

            dgvRecentSales.Columns.Add("Due", "DUE DATE");

            dgvRecentSales.Columns.Add("Total", "AMOUNT (₹)");

            dgvRecentSales.Columns.Add("Paid", "PAID (₹)");

            dgvRecentSales.Columns.Add("Balance", "BALANCE DUE (₹)");

            dgvRecentSales.Columns.Add("Status", "PAYMENT STATUS");

            dgvRecentSales.Columns.Add("Action", "ACTIONS");



            dgvRecentSales.Columns["InvNum"].FillWeight = 110;

            dgvRecentSales.Columns["Customer"].FillWeight = 170;

            dgvRecentSales.Columns["Date"].FillWeight = 95;

            dgvRecentSales.Columns["Due"].FillWeight = 95;

            dgvRecentSales.Columns["Total"].FillWeight = 115;

            dgvRecentSales.Columns["Paid"].FillWeight = 105;

            dgvRecentSales.Columns["Balance"].FillWeight = 105;

            dgvRecentSales.Columns["Status"].FillWeight = 100;

            dgvRecentSales.Columns["Action"].FillWeight = 85;



            dgvRecentSales.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Paid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvRecentSales.Columns["Action"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            dgvRecentSales.Columns["Total"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Paid"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Balance"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvRecentSales.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvRecentSales.Columns["Action"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;



            dgvRecentSales.CellContentClick -= OnRecentGridCellContentClick;

            dgvRecentSales.CellContentClick += OnRecentGridCellContentClick;



            dgvRecentSales.CellDoubleClick -= OnRecentGridCellDoubleClick;

            dgvRecentSales.CellDoubleClick += OnRecentGridCellDoubleClick;



            pnlRecentGridHost.Controls.Add(dgvRecentSales);



            // Footer of Table Card

            pnlRecentCardFooter.Dock = DockStyle.Bottom;

            pnlRecentCardFooter.Height = 38;

            pnlRecentCardFooter.Padding = new Padding(18, 0, 18, 0);

            pnlRecentCardFooter.Controls.Clear();

            pnlRecentCardFooter.Paint += (s, e) =>

            {

                var g = e.Graphics;

                int fw = pnlRecentCardFooter.Width;

                if (fw <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;

                Color lineCol = isDark ? Color.FromArgb(36, 42, 62) : Color.FromArgb(241, 245, 249);

                using var pen = new Pen(lineCol, 1f);

                g.DrawLine(pen, 0, 0, fw, 0);

            };



            lblRecentFooterHint.Text = "Double-click row or click 'View →' to open invoice details & print";

            lblRecentFooterHint.UseMnemonic = false;

            lblRecentFooterHint.Font = new Font("Segoe UI", 8.2f);

            lblRecentFooterHint.ForeColor = Color.FromArgb(100, 116, 139);

            lblRecentFooterHint.Dock = DockStyle.Left;

            lblRecentFooterHint.AutoSize = true;

            lblRecentFooterHint.Padding = new Padding(0, 10, 0, 0);



            lblRecentFooterStats.Text = "Total Volume: ₹0.00  |  Receivables Due: ₹0.00";

            lblRecentFooterStats.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            lblRecentFooterStats.ForeColor = Color.FromArgb(71, 85, 105);

            lblRecentFooterStats.Dock = DockStyle.Right;

            lblRecentFooterStats.AutoSize = true;

            lblRecentFooterStats.Padding = new Padding(0, 10, 0, 0);



            pnlRecentCardFooter.Controls.AddRange(new Control[] { lblRecentFooterHint, lblRecentFooterStats });



            // Assemble Table Card

            pnlRecentSalesCard.Controls.Add(pnlRecentGridHost);

            pnlRecentSalesCard.Controls.Add(pnlRecentCardFooter);

            pnlRecentSalesCard.Controls.Add(pnlRecentCardHeader);



            // ─────────────────────────────────────────────────────────────────────────

            // ASSEMBLE MASTER TABLE LAYOUT PANEL (SECTIONS A, B, C, D)

            // ─────────────────────────────────────────────────────────────────────────

            tlpInvoiceMaster.Controls.Add(pnlPageHeader, 0, 0);

            tlpInvoiceMaster.Controls.Add(pnlDashCardsHost, 0, 1);

            tlpInvoiceMaster.Controls.Add(pnlFilterToolbar, 0, 2);

            tlpInvoiceMaster.Controls.Add(pnlRecentSalesCard, 0, 3);



            pnlDashboardView.Controls.Add(tlpInvoiceMaster);

        }



        private void OnNewInvoiceClicked(object? s, EventArgs e)
        {
            NewSalesInvoiceForm.ShowInvoice((IWin32Window?)this.FindForm() ?? this, () =>
            {
                LoadDashboardData();
                DataChanged?.Invoke(this, EventArgs.Empty);
            });
        }



        private void OnFilterAllInvoicesClicked(object? s, EventArgs e)

        {

            txtRecentSearch.Clear();

            cmbStatusFilter.SelectedIndex = 0;

            chkFilterDates.Checked = false;

            RefreshUnifiedInvoices();

        }

        private void OnCustomersClicked(object? s, EventArgs e) => SwitchTab("Customers");

        private void OnCardHover(int cardIdx, bool hovered, Panel pnl) { _cardsData[cardIdx].IsHovered = hovered; pnl.Invalidate(); }

        private void OnSearchFilterChanged(object? s, EventArgs e) => RefreshUnifiedInvoices();

        private void OnDateFilterCheckChanged(object? s, EventArgs e)

        {

            dtpFilterFrom.Enabled = dtpFilterTo.Enabled = chkFilterDates.Checked;

            RefreshUnifiedInvoices();

        }

        private void OnDateFilterValChanged(object? s, EventArgs e)

        {

            if (chkFilterDates.Checked) RefreshUnifiedInvoices();

        }

        private void OnFilterResetClicked(object? s, EventArgs e)

        {

            txtRecentSearch.Clear();

            cmbStatusFilter.SelectedIndex = 0;

            chkFilterDates.Checked = false;

            RefreshUnifiedInvoices();

        }

        private void OnGridOpenInvoiceClicked(object? s, EventArgs e) => OpenSelectedInvoice();

        private void OnGridRecordPayClicked(object? s, EventArgs e) => RecordPaymentForSelected();

        private void OnGridPrintClicked(object? s, EventArgs e) => PrintSelectedInvoice();

        private void OnGridExportPdfClicked(object? s, EventArgs e) => ExportSelectedPdf();

        private void OnGridCancelClicked(object? s, EventArgs e) => CancelSelectedInvoice();



        private void OnRecentGridCellContentClick(object? s, DataGridViewCellEventArgs e)

        {

            if (e.RowIndex >= 0 && dgvRecentSales.Columns[e.ColumnIndex].Name == "Action" && dgvRecentSales.Rows[e.RowIndex].Tag is Invoice inv)

            {

                OpenInvoiceDetails(inv);

            }

        }



        private void OnRecentGridCellDoubleClick(object? s, DataGridViewCellEventArgs e)

        {

            if (e.RowIndex >= 0 && dgvRecentSales.Rows[e.RowIndex].Tag is Invoice inv)

            {

                OpenInvoiceDetails(inv);

            }

        }



        private void OnSearchBoxPaint(object? s, PaintEventArgs e)

        {

            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isDark = ThemeManager.IsDarkMode;

            Color boxBg = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);

            Color boxBdr = isDark ? Color.FromArgb(48, 56, 80) : Color.FromArgb(203, 213, 225);



            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnlSearchBox.Width - 1, pnlSearchBox.Height - 1, 8);

            using var bBg = new SolidBrush(boxBg);

            using var pBdr = new Pen(boxBdr, 1.2f);

            g.FillPath(bBg, path);

            g.DrawPath(pBdr, path);



            ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Search, new Rectangle(10, 8, 16, 16), isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139), 1.6f);

        }



        private void OpenInvoiceDetails(Invoice inv)

        {

            using var view = new InvoiceDetailsForm(inv);

            view.ShowDialog(this);

            LoadDashboardData();

            DataChanged?.Invoke(this, EventArgs.Empty);

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



            // Elevated Card background & border with 14px rounded corners

            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 14);

            using var bg = new SolidBrush(cardBg);

            using var pen = new Pen(borderCol, data.IsHovered ? 1.6f : 1.2f);

            g.FillPath(bg, path);

            g.DrawPath(pen, path);



            // 1. Top Section (Golden Section 38.2%): 34x34 squircle icon badge

            int badgeSize = 34;

            int badgeX = 16;

            int badgeY = 14;

            Color badgeBg = Color.FromArgb(isDark ? 36 : 24, data.AccentColor);

            using var badgePath = GraphicsExtensions.CreateRoundedRectanglePath(badgeX, badgeY, badgeSize, badgeSize, 10);

            using var badgeBrush = new SolidBrush(badgeBg);

            g.FillPath(badgeBrush, badgePath);



            ReiconRenderer.DrawIcon(g, data.Icon, new Rectangle(badgeX + 8, badgeY + 8, 18, 18), data.AccentColor, 1.8f);



            // Pill tag on top right (Golden Ratio capsule badge)

            int tagH = 22;

            using var fBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold);

            var tagSize = g.MeasureString(data.Badge, fBadge);

            int tagW = (int)tagSize.Width + 16;

            int tagX = w - 16 - tagW;

            int tagY = badgeY + (badgeSize - tagH) / 2;



            if (tagX > badgeX + badgeSize + 8)

            {

                var tagRect = new Rectangle(tagX, tagY, tagW, tagH);

                using var tagPath = GraphicsExtensions.CreateRoundedRectanglePath(tagRect.X, tagRect.Y, tagRect.Width, tagRect.Height, 11);

                using var bTagBg = new SolidBrush(Color.FromArgb(isDark ? 35 : 18, data.AccentColor));

                using var pTagBdr = new Pen(Color.FromArgb(isDark ? 70 : 45, data.AccentColor), 1f);

                g.FillPath(bTagBg, tagPath);

                g.DrawPath(pTagBdr, tagPath);



                using var bBadge = new SolidBrush(data.AccentColor);

                using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                g.DrawString(data.Badge, fBadge, bBadge, tagRect, sfCenter);

            }



            // 2. Middle Section: Metric Title

            using var fTitle = new Font("Segoe UI", 8.2f, FontStyle.Bold);

            Color textSec = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

            using var bTitle = new SolidBrush(textSec);

            g.DrawString(data.Title, fTitle, bTitle, 16, 56);



            // 3. Lower Section: Main Value (Golden Ratio Scaled 17.5pt)

            string val = data.Value;

            float valFontSize = val.Length > 11 ? 13.5f : (val.Length > 9 ? 15f : 17.5f);

            using var fVal = new Font("Segoe UI", valFontSize, FontStyle.Bold);

            Color textPri = isDark ? Color.White : Color.FromArgb(15, 23, 42);

            using var bVal = new SolidBrush(textPri);

            g.DrawString(val, fVal, bVal, 15, 77);



            // 4. Golden Ratio Foundation Micro-Bar at bottom edge (3px)

            int barY = h - 5;

            int barW = w - 32;

            if (barW > 10)

            {

                Color barCol = data.IsHovered ? data.AccentColor : Color.FromArgb(isDark ? 65 : 35, data.AccentColor);

                using var barPath = GraphicsExtensions.CreateRoundedRectanglePath(16, barY, barW, 3, 2);

                using var bBar = new SolidBrush(barCol);

                g.FillPath(bBar, barPath);

            }

        }



        public void LoadDashboardData()

        {

            var m = DatabaseService.GetSalesDashboardMetrics();



            _cardsData[0].Value = $"₹{m.TotalSales:N2}";

            _cardsData[1].Value = $"{m.TotalInvoicesCount}";

            _cardsData[2].Value = $"{m.PaidInvoicesCount}";

            _cardsData[3].Value = $"{m.UnpaidInvoicesCount}";

            _cardsData[3].Badge = m.OverdueInvoicesCount > 0 ? $"● {m.OverdueInvoicesCount} Overdue" : "● Due";

            _cardsData[4].Value = $"₹{m.OutstandingBalance:N2}";

            _cardsData[5].Value = $"₹{m.TodaySales:N2}";



            pnlDashCardsHost.Invalidate(true);

            RefreshUnifiedInvoices();

        }



        private void FilterRecentInvoices() => RefreshUnifiedInvoices();



        private void RefreshUnifiedInvoices()

        {

            string search = txtRecentSearch.Text.Trim();

            string status = cmbStatusFilter.SelectedItem?.ToString() ?? "All Statuses";

            if (status.StartsWith("All", StringComparison.OrdinalIgnoreCase)) status = "All";



            DateTime? from = chkFilterDates.Checked ? dtpFilterFrom.Value.Date : null;

            DateTime? to = chkFilterDates.Checked ? dtpFilterTo.Value.Date : null;



            List<Invoice> list;

            if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))

            {

                list = DatabaseService.GetInvoices(search, "All", from, to)

                    .Where(i => i.Status == "Unpaid" || i.Status == "Pending" || i.Status == "Draft")

                    .ToList();

            }

            else

            {

                list = DatabaseService.GetInvoices(search, status, from, to);

            }



            dgvRecentSales.Rows.Clear();

            decimal totalVol = 0;

            decimal totalBal = 0;



            foreach (var inv in list)

            {

                int rIdx = dgvRecentSales.Rows.Add(

                    inv.InvoiceNumber,

                    inv.CustomerName,

                    inv.InvoiceDate.ToString("dd MMM yyyy"),

                    inv.DueDate.ToString("dd MMM yyyy"),

                    $"₹{inv.GrandTotal:N2}",

                    $"₹{inv.AmountPaid:N2}",

                    $"₹{inv.BalanceDue:N2}",

                    inv.Status,

                    "View"

                );

                dgvRecentSales.Rows[rIdx].Tag = inv;

                totalVol += inv.GrandTotal;

                totalBal += inv.BalanceDue;

            }



            lblRecentBadge.Text = string.Empty;

            lblRecentBadge.Tag = $"● {list.Count} Invoices";

            lblRecentBadge.Invalidate();

            lblRecentFooterStats.Text = $"Total Volume: ₹{totalVol:N2}  |  Receivables Due: ₹{totalBal:N2}";

        }



        // ═══════════════════════════════════════════════════════════════════════════

        //  BUILD TAB 2: CREATE INVOICE VIEW (NEW SALES INVOICE PAGE)

        // ═══════════════════════════════════════════════════════════════════════════

        private void BuildCreateInvoiceView()

        {

            pnlCreateInvoiceView.Dock = DockStyle.Fill;

            pnlCreateInvoiceView.Controls.Clear();



            _inlineNewInvoice.Dock = DockStyle.Fill;

            _inlineNewInvoice.InvoiceSaved -= OnInlineNewInvoiceSaved;

            _inlineNewInvoice.InvoiceSaved += OnInlineNewInvoiceSaved;

            _inlineNewInvoice.CancelRequested -= OnInlineNewInvoiceCancelRequested;

            _inlineNewInvoice.CancelRequested += OnInlineNewInvoiceCancelRequested;

            _inlineNewInvoice.BackRequested -= OnInlineNewInvoiceCancelRequested;

            _inlineNewInvoice.BackRequested += OnInlineNewInvoiceCancelRequested;



            pnlCreateInvoiceView.Controls.Add(_inlineNewInvoice);

        }



        private void OnInlineNewInvoiceSaved(object? s, EventArgs e)

        {

            SwitchTab("Dashboard");

            LoadDashboardData();

            DataChanged?.Invoke(this, EventArgs.Empty);

        }



        private void OnInlineNewInvoiceCancelRequested(object? s, EventArgs e)

        {

            SwitchTab("Dashboard");

        }



        private void PrepareNewInvoiceForm()

        {

            _inlineNewInvoice.ResetForm();

        }



        // ═══════════════════════════════════════════════════════════════════════════

        //  BUILD TAB 3: INVOICES DIRECTORY VIEW (PRO TABLE & FILTERS)

        // ═══════════════════════════════════════════════════════════════════════════

        private void BuildInvoicesListView()

        {

            // Deprecated: Unified Invoices presentation is now centrally managed in BuildDashboardView().

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

            if (dgvRecentSales.SelectedRows.Count > 0 && dgvRecentSales.SelectedRows[0].Tag is Invoice invRecent)

            {

                return DatabaseService.GetInvoiceById(invRecent.Id) ?? invRecent;

            }

            if (dgvAllInvoices.SelectedRows.Count > 0 && dgvAllInvoices.SelectedRows[0].Tag is Invoice inv)

            {

                return DatabaseService.GetInvoiceById(inv.Id) ?? inv;

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

            LoadDashboardData();

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

                LoadDashboardData();

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

            else

            {

                MessageBox.Show("Please select an invoice to print.", "Select Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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

            else

            {

                MessageBox.Show("Please select an invoice to export as PDF.", "Select Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }



        private void CancelSelectedInvoice()

        {

            var inv = GetSelectedGridInvoice();

            if (inv == null)

            {

                MessageBox.Show("Please select an invoice to cancel.", "Select Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            if (inv.Status == "Cancelled")

            {

                MessageBox.Show("This invoice is already cancelled.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;

            }



            if (MessageBox.Show($"Are you sure you want to cancel invoice {inv.InvoiceNumber}?", "Cancel Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)

            {

                DatabaseService.CancelInvoice(inv.Id);

                LoadDashboardData();

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

        //  MODERN TABLE FORMATTING & CUSTOM PIXEL-PERFECT SAAS CELL RENDERING

        // ═══════════════════════════════════════════════════════════════════════════

        private void FormatModernTable(DataGridView dgv)

        {

            dgv.BorderStyle = BorderStyle.None;

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;

            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgv.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;

            dgv.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;

            dgv.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            dgv.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;



            dgv.EnableHeadersVisualStyles = false;

            dgv.RowHeadersVisible = false;

            dgv.AllowUserToAddRows = false;

            dgv.AllowUserToDeleteRows = false;

            dgv.AllowUserToResizeRows = false;

            dgv.AllowUserToResizeColumns = false;

            dgv.ReadOnly = true;

            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgv.MultiSelect = false;

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.RowTemplate.Height = 30;

            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.ColumnHeadersHeight = 32;

            dgv.StandardTab = true;

            dgv.BackgroundColor = ThemeManager.IsDarkMode ? Color.FromArgb(23, 26, 38) : Color.White;

            dgv.GridColor = dgv.BackgroundColor;



            int hoveredRow = -1;



            dgv.CellMouseMove += (s, e) =>

            {

                if (e.RowIndex >= 0)

                {

                    dgv.Cursor = Cursors.Hand;

                    if (hoveredRow != e.RowIndex)

                    {

                        int old = hoveredRow;

                        hoveredRow = e.RowIndex;

                        if (old >= 0 && old < dgv.RowCount) dgv.InvalidateRow(old);

                        dgv.InvalidateRow(e.RowIndex);

                    }

                }

                else

                {

                    dgv.Cursor = Cursors.Default;

                }

            };



            dgv.MouseLeave += (s, e) =>

            {

                if (hoveredRow >= 0)

                {

                    int old = hoveredRow;

                    hoveredRow = -1;

                    if (old < dgv.RowCount) dgv.InvalidateRow(old);

                }

            };



            dgv.CellPainting += (s, e) =>

            {

                PaintModernCell(dgv, e, hoveredRow);

            };

        }



        private static void PaintModernCell(DataGridView dgv, DataGridViewCellPaintingEventArgs e, int hoveredRowIndex)

        {

            if (e.Graphics == null || e.ColumnIndex < 0) return;



            var g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            bool isDark = ThemeManager.IsDarkMode;



            // ─────────────────────────────────────────────────────────────────

            // 1. COLUMN HEADER ROW (ZERO VERTICAL BORDERS, CRISP TYPOGRAPHY)

            // ─────────────────────────────────────────────────────────────────

            if (e.RowIndex == -1)

            {

                Color headerBg = isDark ? Color.FromArgb(24, 28, 42) : Color.FromArgb(248, 250, 252);

                Color borderBottom = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);

                Color headerText = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);



                var fillRect = new Rectangle(e.CellBounds.X - 1, e.CellBounds.Y, e.CellBounds.Width + 2, e.CellBounds.Height);

                using (var bBg = new SolidBrush(headerBg))

                    g.FillRectangle(bBg, fillRect);



                // Feather-soft 1px top & bottom border line and vertical column separator matching reference screenshot
                using (var pBorder = new Pen(borderBottom, 1f))
                {
                    g.DrawLine(pBorder, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                    g.DrawLine(pBorder, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    g.DrawLine(pBorder, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                    if (e.ColumnIndex == 0)
                        g.DrawLine(pBorder, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom - 1);
                }



                string title = e.Value?.ToString() ?? "";

                using var f = new Font("Segoe UI", 8.2f, FontStyle.Bold);

                using var bText = new SolidBrush(headerText);



                string colName = dgv.Columns[e.ColumnIndex].Name;

                var align = dgv.Columns[e.ColumnIndex].DefaultCellStyle.Alignment;

                bool isRight = (align == DataGridViewContentAlignment.MiddleRight) ||

                               colName.Contains("Total", StringComparison.OrdinalIgnoreCase) ||

                               colName.Contains("Paid", StringComparison.OrdinalIgnoreCase) ||

                               colName.Contains("Balance", StringComparison.OrdinalIgnoreCase) ||

                               colName.Contains("Price", StringComparison.OrdinalIgnoreCase) ||

                               colName.Contains("Disc", StringComparison.OrdinalIgnoreCase);



                bool isCenter = (align == DataGridViewContentAlignment.MiddleCenter) ||

                                colName.Contains("Status", StringComparison.OrdinalIgnoreCase) ||

                                colName.Equals("Action", StringComparison.OrdinalIgnoreCase);



                var sf = new StringFormat

                {

                    LineAlignment = StringAlignment.Center,

                    Alignment = isRight ? StringAlignment.Far : (isCenter ? StringAlignment.Center : StringAlignment.Near)

                };



                int padX = 16;

                var textRect = new Rectangle(e.CellBounds.X + padX, e.CellBounds.Y, e.CellBounds.Width - (padX * 2), e.CellBounds.Height);

                g.DrawString(title, f, bText, textRect, sf);



                e.Handled = true;

                return;

            }



            // ─────────────────────────────────────────────────────────────────

            // 2. DATA ROW CELLS (SUBTLE HOVER, ROW ACCENTS & RICH CONTENT)

            // ─────────────────────────────────────────────────────────────────

            if (e.RowIndex >= 0)

            {

                bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

                bool isHovered = (e.RowIndex == hoveredRowIndex);



                Color rowBg = isSelected

                    ? (isDark ? Color.FromArgb(34, 42, 68) : Color.FromArgb(238, 242, 255))

                    : (isHovered

                        ? (isDark ? Color.FromArgb(28, 33, 50) : Color.FromArgb(248, 250, 252))

                        : ((e.RowIndex % 2 == 1)

                            ? (isDark ? Color.FromArgb(21, 24, 35) : Color.FromArgb(252, 253, 255))

                            : (isDark ? Color.FromArgb(23, 26, 38) : Color.White)));



                Color gridLineColor = isDark ? Color.FromArgb(36, 42, 62) : Color.FromArgb(241, 245, 249);



                var fillRect = new Rectangle(e.CellBounds.X - 1, e.CellBounds.Y, e.CellBounds.Width + 2, e.CellBounds.Height);

                using (var bBg = new SolidBrush(rowBg))

                    g.FillRectangle(bBg, fillRect);



                // 1px horizontal bottom divider line (NO vertical lines!)

                using (var pBorder = new Pen(gridLineColor, 1f))

                    g.DrawLine(pBorder, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);



                // Left accent bar on selected row (first column)

                if (isSelected && e.ColumnIndex == 0)

                {

                    using var bAccent = new SolidBrush(Color.FromArgb(79, 70, 229));

                    g.FillRectangle(bAccent, e.CellBounds.Left, e.CellBounds.Top, 3, e.CellBounds.Height);

                }



                string colName = dgv.Columns[e.ColumnIndex].Name;

                string val = e.Value?.ToString() ?? "";

                var align = dgv.Columns[e.ColumnIndex].DefaultCellStyle.Alignment;



                // A. STATUS PILL BADGE

                if (colName.Contains("Status", StringComparison.OrdinalIgnoreCase))

                {

                    PaintStatusCellBadgeInternal(g, e.CellBounds, val, isDark);

                    e.Handled = true;

                    return;

                }



                // B. ACTION BUTTON PILL

                if (colName.Equals("Action", StringComparison.OrdinalIgnoreCase))

                {

                    int btnW = 60;

                    int btnH = 22;

                    int btnX = e.CellBounds.X + (e.CellBounds.Width - btnW) / 2;

                    int btnY = e.CellBounds.Y + (e.CellBounds.Height - btnH) / 2;

                    var btnRect = new Rectangle(btnX, btnY, btnW, btnH);



                    Color btnFill = isHovered ? (isDark ? Color.FromArgb(49, 46, 129) : Color.FromArgb(238, 242, 255))

                                              : (isDark ? Color.FromArgb(30, 36, 56) : Color.FromArgb(248, 250, 252));

                    Color btnBdr = isHovered ? Color.FromArgb(99, 102, 241)

                                             : (isDark ? Color.FromArgb(50, 58, 86) : Color.FromArgb(203, 213, 225));

                    Color btnText = isDark ? Color.FromArgb(165, 180, 252) : Color.FromArgb(79, 70, 229);



                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height, 6);

                    using (var bFill = new SolidBrush(btnFill)) g.FillPath(bFill, path);

                    using (var pBdr = new Pen(btnBdr, 1f)) g.DrawPath(pBdr, path);



                    using var fBtn = new Font("Segoe UI", 8.2f, FontStyle.Bold);

                    using var bTxt = new SolidBrush(btnText);

                    using var sfBtn = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    g.DrawString("View →", fBtn, bTxt, btnRect, sfBtn);



                    e.Handled = true;

                    return;

                }



                // C. CUSTOMER AVATAR + NAME

                if (colName.Equals("Customer", StringComparison.OrdinalIgnoreCase) ||

                    (colName.Equals("Name", StringComparison.OrdinalIgnoreCase) && dgv.Name.Contains("Customer", StringComparison.OrdinalIgnoreCase)))

                {

                    string initials = GetInitials(val);

                    int avaSize = 20;

                    int avaX = e.CellBounds.X + 8;

                    int avaY = e.CellBounds.Y + (e.CellBounds.Height - avaSize) / 2;



                    Color avaBg = GetAvatarBgColor(initials, isDark);

                    Color avaText = GetAvatarTextColor(initials, isDark);



                    using (var path = GraphicsExtensions.CreateRoundedRectanglePath(avaX, avaY, avaSize, avaSize, avaSize / 2))

                    using (var bAva = new SolidBrush(avaBg))

                    {

                        g.FillPath(bAva, path);

                    }

                    using (var fAva = new Font("Segoe UI", 7.5f, FontStyle.Bold))

                    using (var bAvaTxt = new SolidBrush(avaText))

                    using (var sfAva = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })

                    {

                        g.DrawString(initials, fAva, bAvaTxt, new RectangleF(avaX, avaY, avaSize, avaSize), sfAva);

                    }



                    int textX = avaX + avaSize + 10;

                    using var fName = new Font("Segoe UI", 9.2f, FontStyle.Bold);

                    Color nameCol = isDark ? Color.White : Color.FromArgb(15, 23, 42);

                    using var bName = new SolidBrush(nameCol);

                    using var sfName = new StringFormat { LineAlignment = StringAlignment.Center };

                    g.DrawString(val, fName, bName, new RectangleF(textX, e.CellBounds.Y, Math.Max(10, e.CellBounds.Width - (textX - e.CellBounds.X) - 8), e.CellBounds.Height), sfName);



                    e.Handled = true;

                    return;

                }



                // D. INVOICE NUMBER (Brand Accent Link)

                if (colName.Contains("InvNum", StringComparison.OrdinalIgnoreCase) || colName.Contains("InvNo", StringComparison.OrdinalIgnoreCase))

                {

                    using var fInv = new Font("Segoe UI", 9.2f, FontStyle.Bold);

                    Color invCol = isDark ? Color.FromArgb(129, 140, 248) : Color.FromArgb(79, 70, 229);

                    using var bInv = new SolidBrush(invCol);

                    using var sfInv = new StringFormat { LineAlignment = StringAlignment.Center };

                    g.DrawString(val, fInv, bInv, new RectangleF(e.CellBounds.X + 16, e.CellBounds.Y, Math.Max(10, e.CellBounds.Width - 20), e.CellBounds.Height), sfInv);



                    e.Handled = true;

                    return;

                }



                // E. AMOUNTS (Total, Paid, Balance, Rates, Right-Aligned Columns)

                if (colName.Equals("Total", StringComparison.OrdinalIgnoreCase) ||

                    colName.Equals("Paid", StringComparison.OrdinalIgnoreCase) ||

                    colName.Equals("Balance", StringComparison.OrdinalIgnoreCase) ||

                    colName.Equals("Price", StringComparison.OrdinalIgnoreCase) ||

                    colName.Equals("Disc", StringComparison.OrdinalIgnoreCase) ||

                    align == DataGridViewContentAlignment.MiddleRight)

                {

                    Color amtCol;

                    Font fAmt;



                    if (colName.Equals("Total", StringComparison.OrdinalIgnoreCase) || colName.Equals("Price", StringComparison.OrdinalIgnoreCase))

                    {

                        amtCol = isDark ? Color.White : Color.FromArgb(15, 23, 42);

                        fAmt = new Font("Segoe UI", 9.2f, FontStyle.Bold);

                    }

                    else if (colName.Equals("Paid", StringComparison.OrdinalIgnoreCase))

                    {

                        amtCol = isDark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(5, 150, 105);

                        fAmt = new Font("Segoe UI", 9f, FontStyle.Regular);

                    }

                    else if (colName.Equals("Balance", StringComparison.OrdinalIgnoreCase))

                    {

                        bool isZero = val.Contains("0.00");

                        amtCol = isZero

                            ? (isDark ? Color.FromArgb(100, 116, 139) : Color.FromArgb(148, 163, 184))

                            : (isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(225, 29, 72));

                        fAmt = new Font("Segoe UI", 9.2f, isZero ? FontStyle.Regular : FontStyle.Bold);

                    }

                    else

                    {

                        amtCol = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(30, 41, 59);

                        fAmt = new Font("Segoe UI", 9f, FontStyle.Regular);

                    }



                    using (fAmt)

                    using (var bAmt = new SolidBrush(amtCol))

                    using (var sfAmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })

                    {

                        g.DrawString(val, fAmt, bAmt, new RectangleF(e.CellBounds.X, e.CellBounds.Y, Math.Max(10, e.CellBounds.Width - 16), e.CellBounds.Height), sfAmt);

                    }



                    e.Handled = true;

                    return;

                }



                // F. GENERAL / DATE CELLS

                {

                    using var fGen = new Font("Segoe UI", 9f, FontStyle.Regular);

                    Color genCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

                    using var bGen = new SolidBrush(genCol);

                    var sfGen = new StringFormat

                    {

                        LineAlignment = StringAlignment.Center,

                        Alignment = (align == DataGridViewContentAlignment.MiddleCenter) ? StringAlignment.Center : StringAlignment.Near

                    };

                    g.DrawString(val, fGen, bGen, new RectangleF(e.CellBounds.X + 16, e.CellBounds.Y, Math.Max(10, e.CellBounds.Width - 20), e.CellBounds.Height), sfGen);



                    e.Handled = true;

                }

            }

        }



        private static void PaintStatusCellBadgeInternal(Graphics g, Rectangle bounds, string status, bool isDark)

        {

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



            int pillH = 20;

            int pillW = status == "Partially Paid" ? 104 : (status == "Overdue" ? 82 : (status == "Unpaid" ? 78 : 72));

            int pillX = bounds.X + (bounds.Width - pillW) / 2;

            int pillY = bounds.Y + (bounds.Height - pillH) / 2;



            using (var path = GraphicsExtensions.CreateRoundedRectanglePath(pillX, pillY, pillW, pillH, 12))

            using (var brush = new SolidBrush(bgCol))

            using (var borderPen = new Pen(Color.FromArgb(isDark ? 90 : 50, dotCol), 1f))

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

        }



        private static string GetInitials(string name)

        {

            if (string.IsNullOrWhiteSpace(name)) return "CL";

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();

            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";

        }



        private static Color GetAvatarBgColor(string initials, bool isDark)

        {

            int hash = Math.Abs(initials.GetHashCode());

            Color[] lightColors = {

                Color.FromArgb(238, 242, 255), // Indigo tint

                Color.FromArgb(236, 253, 245), // Emerald tint

                Color.FromArgb(240, 249, 255), // Sky tint

                Color.FromArgb(254, 243, 199), // Amber tint

                Color.FromArgb(250, 245, 255), // Purple tint

                Color.FromArgb(255, 241, 242)  // Rose tint

            };

            Color[] darkColors = {

                Color.FromArgb(49, 46, 129),

                Color.FromArgb(6, 78, 59),

                Color.FromArgb(12, 74, 110),

                Color.FromArgb(120, 53, 15),

                Color.FromArgb(88, 28, 135),

                Color.FromArgb(136, 19, 55)

            };

            return isDark ? darkColors[hash % darkColors.Length] : lightColors[hash % lightColors.Length];

        }



        private static Color GetAvatarTextColor(string initials, bool isDark)

        {

            int hash = Math.Abs(initials.GetHashCode());

            Color[] lightTexts = {

                Color.FromArgb(79, 70, 229),

                Color.FromArgb(5, 150, 105),

                Color.FromArgb(2, 132, 199),

                Color.FromArgb(217, 119, 6),

                Color.FromArgb(147, 51, 234),

                Color.FromArgb(225, 29, 72)

            };

            Color[] darkTexts = {

                Color.FromArgb(199, 210, 254),

                Color.FromArgb(167, 243, 208),

                Color.FromArgb(186, 230, 253),

                Color.FromArgb(253, 230, 138),

                Color.FromArgb(233, 213, 255),

                Color.FromArgb(254, 205, 211)

            };

            return isDark ? darkTexts[hash % darkTexts.Length] : lightTexts[hash % lightTexts.Length];

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

            Color gridLineCol = isDark ? Color.FromArgb(36, 42, 62) : Color.FromArgb(241, 245, 249);

            Color inputBg = isDark ? Color.FromArgb(30, 35, 50) : Color.White;



            BackColor = pageBg;

            pnlNavHeader.BackColor = isDark ? Color.FromArgb(18, 20, 30) : Color.White;

            pnlViewDeck.BackColor = pageBg;



            pnlDashboardView.BackColor = pageBg;

            pnlCreateInvoiceView.BackColor = pageBg;

            pnlInvoicesListView.BackColor = pageBg;

            pnlCustomersView.BackColor = pageBg;



            pnlNavPillTrack.Invalidate(true);

            pnlPageHeader.Invalidate(true);

            lblPageTitle.ForeColor = textPrimary;

            lblPageSubtitle.ForeColor = textSecondary;

            lblDateToSep.ForeColor = textSecondary;

            pnlFilterToolbar.Invalidate(true);

            pnlDashCardsHost.Invalidate(true);

            pnlRecentSalesCard.Invalidate(true);

            pnlRecentCardHeader.Invalidate(true);

            pnlRecentCardFooter.Invalidate(true);

            lblRecentTitle.ForeColor = textPrimary;

            lblRecentBadge.Invalidate();



            // Style all DataGridViews

            foreach (var dgv in new[] { dgvRecentSales, dgvAllInvoices, dgvAllCustomers })

            {

                dgv.BackgroundColor = cardBg;

                dgv.GridColor = cardBg;

                dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;

                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(24, 28, 42) : Color.FromArgb(248, 250, 252);

                dgv.ColumnHeadersDefaultCellStyle.ForeColor = textSecondary;

                dgv.DefaultCellStyle.BackColor = cardBg;

                dgv.DefaultCellStyle.ForeColor = textPrimary;

                dgv.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(34, 42, 68) : Color.FromArgb(238, 242, 255);

                dgv.DefaultCellStyle.SelectionForeColor = textPrimary;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(21, 24, 35) : Color.FromArgb(252, 253, 255);

                dgv.AlternatingRowsDefaultCellStyle.ForeColor = textPrimary;

            }



            // Style Header Quick Action buttons

            Color outlineBtnBg = isDark ? Color.FromArgb(28, 33, 48) : Color.White;

            Color outlineBtnBdr = isDark ? Color.FromArgb(50, 58, 86) : Color.FromArgb(203, 213, 225);

            Color outlineBtnText = isDark ? Color.FromArgb(226, 232, 240) : Color.FromArgb(30, 41, 59);

            Color outlineBtnIcon = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

            Color outlineBtnHover = isDark ? Color.FromArgb(38, 44, 64) : Color.FromArgb(241, 245, 249);



            btnQuickViewInvs.FillColor = outlineBtnBg;

            btnQuickViewInvs.BorderColor = outlineBtnBdr;

            btnQuickViewInvs.TextColor = outlineBtnText;

            btnQuickViewInvs.IconColor = outlineBtnIcon;

            btnQuickViewInvs.HoverColor = outlineBtnHover;



            btnQuickCustomers.FillColor = outlineBtnBg;

            btnQuickCustomers.BorderColor = outlineBtnBdr;

            btnQuickCustomers.TextColor = outlineBtnText;

            btnQuickCustomers.IconColor = outlineBtnIcon;

            btnQuickCustomers.HoverColor = outlineBtnHover;



            btnGridPrint.FillColor = outlineBtnBg;

            btnGridPrint.BorderColor = outlineBtnBdr;

            btnGridPrint.TextColor = outlineBtnText;

            btnGridPrint.IconColor = outlineBtnIcon;

            btnGridPrint.HoverColor = outlineBtnHover;



            btnGridExportPdf.FillColor = outlineBtnBg;

            btnGridExportPdf.BorderColor = outlineBtnBdr;

            btnGridExportPdf.TextColor = outlineBtnText;

            btnGridExportPdf.IconColor = outlineBtnIcon;

            btnGridExportPdf.HoverColor = outlineBtnHover;



            btnFilterReset.FillColor = isDark ? Color.FromArgb(32, 38, 56) : Color.FromArgb(241, 245, 249);

            btnFilterReset.BorderColor = outlineBtnBdr;

            btnFilterReset.TextColor = outlineBtnIcon;

            btnFilterReset.IconColor = outlineBtnIcon;

            btnFilterReset.HoverColor = outlineBtnHover;



            btnGridCancel.FillColor = isDark ? Color.FromArgb(38, 24, 30) : Color.White;

            btnGridCancel.BorderColor = Color.FromArgb(244, 63, 94);

            btnGridCancel.TextColor = isDark ? Color.FromArgb(251, 113, 133) : Color.FromArgb(225, 29, 72);

            btnGridCancel.IconColor = btnGridCancel.TextColor;

            btnGridCancel.HoverColor = isDark ? Color.FromArgb(56, 28, 36) : Color.FromArgb(255, 241, 242);



            btnGridRecordPay.FillColor = Color.FromArgb(16, 185, 129);

            btnGridRecordPay.HoverColor = Color.FromArgb(5, 150, 105);

            btnGridRecordPay.TextColor = Color.White;

            btnGridRecordPay.IconColor = Color.White;



            btnGridOpenInvoice.FillColor = Color.FromArgb(79, 70, 229);

            btnGridOpenInvoice.HoverColor = Color.FromArgb(67, 56, 202);

            btnGridOpenInvoice.TextColor = Color.White;

            btnGridOpenInvoice.IconColor = Color.White;



            chkFilterDates.ForeColor = textPrimary;

            lblDateToSep.ForeColor = textSecondary;



            // Style inputs recursively

            ApplyInputsTheme(pnlDashboardView, inputBg, textPrimary, textSecondary, borderCol);

            ApplyInputsTheme(pnlCreateInvoiceView, inputBg, textPrimary, textSecondary, borderCol);

            ApplyInputsTheme(pnlInvoicesListView, inputBg, textPrimary, textSecondary, borderCol);

            ApplyInputsTheme(pnlCustomersView, inputBg, textPrimary, textSecondary, borderCol);

            _inlineNewInvoice.Invalidate(true);



            Invalidate(true);

        }



        private void ApplyInputsTheme(Control parent, Color inputBg, Color textPrimary, Color textSecondary, Color borderCol)

        {

            bool isDark = ThemeManager.IsDarkMode;

            foreach (Control c in parent.Controls)

            {

                if (c is TextBox tb)

                {

                    if (tb == txtRecentSearch)

                    {

                        tb.BackColor = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);

                        tb.ForeColor = textPrimary;

                        tb.BorderStyle = BorderStyle.None;

                    }

                    else

                    {

                        tb.BackColor = inputBg;

                        tb.ForeColor = textPrimary;

                        tb.BorderStyle = BorderStyle.FixedSingle;

                    }

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

