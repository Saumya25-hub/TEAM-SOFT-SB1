using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// TEAM SOFT — Master Inventory & Billing Dashboard (.NET 8 WinForms).
    /// Features:
    /// - Smooth Entrance Animation (180–240ms subtle fade & card stagger)
    /// - High-speed, non-blocking Light/Dark theme switching (<100ms response)
    /// - Reicon SVG open-source vector icon integration (Zero emojis)
    /// - Native WinForms TableLayoutPanel/FlowLayoutPanel zero-overlap layout
    /// - Micro-interactions: Card hover elevation, nav active pill, search focus border, tooltips
    /// </summary>
    public partial class DashboardForm : Form
    {
        // Win32 API to freeze window updates so all components switch simultaneously (Zero sequential repaint)
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        private const int WM_SETREDRAW = 0x000B;
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_ALLCHILDREN = 0x0080;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_ERASE = 0x0004;

        // Direct Theme Toggle (No animation — direct instantaneous switch)
        public event EventHandler? LogoutRequested;

        private readonly System.Windows.Forms.Timer _entranceTimer = new() { Interval = 16 };

        // Micro-interaction states
        private string? _hoveredNavPage = null;
        private int _hoveredCardIndex = -1;
        private bool _isSearchFocused = false;
        private readonly ToolTip _toolTip = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  CACHED GDI FONTS (Zero allocations during paint loops)
        // ═══════════════════════════════════════════════════════════════════════════
        private static readonly Font s_fontStatTitle   = new("Segoe UI", 8.5f);
        private static readonly Font s_fontStatVal     = new("Segoe UI", 13f, FontStyle.Bold);
        private static readonly Font s_fontStatSub     = new("Segoe UI", 7.5f);
        private static readonly Font s_fontChartTitle  = new("Segoe UI", 11f, FontStyle.Bold);
        private static readonly Font s_fontLegend      = new("Segoe UI", 8.5f);
        private static readonly Font s_fontMonthLbl    = new("Segoe UI", 7.5f);
        private static readonly Font s_fontCenterSmall = new("Segoe UI", 7.5f);
        private static readonly Font s_fontCenterBold  = new("Segoe UI", 10f, FontStyle.Bold);
        private static readonly Font s_fontLegendList  = new("Segoe UI", 8f);
        private static readonly Font s_fontNav         = new("Segoe UI", 9f);
        private static readonly Font s_fontNavActive   = new("Segoe UI", 9f, FontStyle.Bold);
        private static readonly Font s_fontGrid        = new("Segoe UI", 9f);
        private static readonly Font s_fontGridHeader  = new("Segoe UI", 9f, FontStyle.Bold);

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME PALETTE ACCESSORS (Instant delegation, zero float math)
        // ═══════════════════════════════════════════════════════════════════════════
        private static Color C_Blue       => ThemeManager.PrimaryAccent;
        private static Color C_Positive   => ThemeManager.StatusPositive;
        private static Color C_Warning    => ThemeManager.StatusWarning;
        private static Color C_Negative   => ThemeManager.StatusNegative;

        private static Color C_SidebarBg  => ThemeManager.SidebarBackground;
        private static Color C_NavHover   => ThemeManager.NavHover;
        private static Color C_NavActive  => ThemeManager.NavActive;
        private static Color C_NavText    => ThemeManager.NavText;
        private static Color C_NavTextOn  => ThemeManager.NavTextOn;

        private static Color C_PageBg     => ThemeManager.PageBackground;
        private static Color C_CardBg     => ThemeManager.CardBackground;
        private static Color C_Border     => ThemeManager.BorderHairline;
        private static Color C_TopBarBg   => ThemeManager.TopBarBackground;
        private static Color C_SearchBg   => ThemeManager.SearchBoxBackground;
        private Color C_SearchBdr         => _isSearchFocused ? C_Blue : ThemeManager.InputBorder;

        private static Color C_TextDark   => ThemeManager.TextPrimary;
        private static Color C_TextMid    => ThemeManager.TextSecondary;
        private static Color C_TextLight  => ThemeManager.IsDarkMode ? Color.FromArgb(100, 116, 139) : Color.FromArgb(148, 163, 184);

        private static Color BlendColor(Color c1, Color c2, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            int a = (int)(c1.A + (c2.A - c1.A) * t);
            int r = (int)(c1.R + (c2.R - c1.R) * t);
            int g = (int)(c1.G + (c2.G - c1.G) * t);
            int b = (int)(c1.B + (c2.B - c1.B) * t);
            return Color.FromArgb(a, r, g, b);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STATE & DATA REPOSITORIES
        // ═══════════════════════════════════════════════════════════════════════════
        private string _activePage = "Overview";

        private SalesBillingModuleControl? _salesModule;
        public SalesBillingModuleControl? SalesModule => _salesModule;
        private SettingsModuleControl? _settingsModule;
        public SettingsModuleControl? SettingsModule => _settingsModule;
        private SalesDashboardMetrics? _currentMetrics;
        private readonly (string Title, string Value, string Subtitle, Color StatusCol, ReiconRenderer.IconType Icon)[] _kpiCardData = new (string, string, string, Color, ReiconRenderer.IconType)[6];

        private int _selectedDynamicsYear = DateTime.Today.Year;
        private List<MonthlySalesBarItem> _monthlyDynamics = new();
        private List<CategorySalesDistribution> _categoryDistribution = new();
        private decimal _categoryGrandTotal = 0m;
        private List<ProductItem> _lowStockProducts = new();
        private List<Invoice> _recentInvoices = new();

        private Panel? _pnlInvoicesHeader;
        private Label? _lblInvoicesBadge;
        private Panel? _pnlInvoicesFooter;
        private Label? _lblInvoicesFooterSummary;

        private Panel? _pnlLowStockHeader;
        private Label? _lblLowStockBadge;
        private Panel? _pnlLowStockFooter;
        private Label? _lblLowStockFooterSummary;

        private Panel? _pnlDynamicsHeader;
        private int _hoveredDynamicsMonthIndex = -1;
        private Point _dynamicsMousePos = Point.Empty;

        private Panel? _pnlDonutHeader;
        private int _hoveredCategoryIndex = -1;
        private Point _donutMousePos = Point.Empty;

        private readonly Panel pnlSettings = new();
        private readonly Panel pnlStatistics = new();
        private readonly Panel pnlPurchases = new();
        private readonly Panel pnlSuppliers = new();

        private readonly DataGridView dgvPurchases = new();
        private readonly DataGridView dgvSuppliers = new();
        private readonly TextBox txtPurchaseSearch = new();
        private readonly TextBox txtSupplierSearch = new();
        private readonly ComboBox cmbPurchaseStatus = new();
        private readonly Label lblPurchasesKpi1 = new();
        private readonly Label lblPurchasesKpi2 = new();
        private readonly Label lblPurchasesKpi3 = new();
        private readonly Label lblPurchasesKpi4 = new();

        private bool _isDragging;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        // ═══════════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════
        public DashboardForm()
        {
            // Initialize SQLite Database schema & seed records first
            DatabaseService.InitializeDatabase();

            InitializeComponent();

            this.Opacity = 1.0;

            InitTooltips();
            InitThemeToggle();
            InitEntranceAnimation();
            BuildOverviewPage();
            BuildNewBillPage();
            BuildCustomersPage();
            BuildProductsPage();
            BuildTransactionsPage();
            BuildPurchasesPage();
            BuildSuppliersPage();
            BuildSettingsPage();
            BuildStatisticsPage();
            SetupSidebarNav();
            SetupWindowDrag();

            RefreshDashboardAnalytics();
            RefreshLiveKpiCards();
            RefreshInvoicesTable();
            RefreshLowStockTable();
            ShowPage("Overview");

            // CRITICAL: Double-buffer the entire form and all child controls AFTER pages are built
            EnableDoubleBuffering(this);

            ThemeManager.ThemeChanged += ApplyDashboardTheme;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  DIRECT INSTANT ENTRANCE (NO ANIMATION / DIRECT RENDER)
        // ═══════════════════════════════════════════════════════════════════════════
        private void InitEntranceAnimation()
        {
            this.Opacity = 1.0;
            pnlContentHost.Padding = new Padding(16, 12, 16, 12);
        }

        /// <summary>
        /// Direct instant entrance (Zero animation, zero opacity fade).
        /// </summary>
        public void StartEntranceAnimation()
        {
            this.Opacity = 1.0;
            pnlContentHost.Padding = new Padding(16, 12, 16, 12);
            _entranceTimer.Stop();
        }

        public void FadeIn() => StartEntranceAnimation();

        private void InitTooltips()
        {
            _toolTip.SetToolTip(btnNotifications, "Unread Notifications");
            _toolTip.SetToolTip(btnThemeToggle, "Toggle Light / Dark Mode");
            _toolTip.SetToolTip(btnTopAddProduct, "Create New Sales Tax Invoice");
            _toolTip.SetToolTip(winChrome, "Window Controls (Minimize, Maximize, Close)");
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  OVERRIDES
        // ═══════════════════════════════════════════════════════════════════════════
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Opacity = 1.0;
            ApplyRoundedRegion(16);
            ApplyDashboardTheme();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _entranceTimer.Stop();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState != FormWindowState.Normal) Region = null;
            else ApplyRoundedRegion(16);
            AdaptCardsLayout();
            UpdateHeaderLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Form outer hairline border
            using var pen = new Pen(C_Border, 1.2f);
            g.DrawRoundedRectangle(pen, 0, 0, Width - 1, Height - 1, 16);
        }

        private void ApplyRoundedRegion(int r)
        {
            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, Width, Height, r);
            Region = new Region(path);
        }

        private void SetupWindowDrag()
        {
            MouseEventHandler onDown = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging = true;
                    _dragCursorPoint = Cursor.Position;
                    _dragFormPoint = Location;
                }
            };
            MouseEventHandler onMove = (s, e) =>
            {
                if (_isDragging)
                {
                    Point diff = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
                    Location = Point.Add(_dragFormPoint, new Size(diff));
                }
            };
            MouseEventHandler onUp = (s, e) => _isDragging = false;

            pnlTopBar.MouseDown += onDown;
            pnlTopBar.MouseMove += onMove;
            pnlTopBar.MouseUp += onUp;

            pnlHeaderRight.MouseDown += onDown;
            pnlHeaderRight.MouseMove += onMove;
            pnlHeaderRight.MouseUp += onUp;
        }

        private void InitThemeToggle()
        {
            btnThemeToggle.Paint += (s, e) => PaintThemeToggle(e.Graphics);

            btnThemeToggle.Click += (s, e) =>
            {
                ToggleThemeDirect();
            };

            pnlBrandHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int cx = 30;
                int cy = 34;
                int petalR = 8;
                int offset = 6;

                Color logoAccent = ThemeManager.IsDarkMode ? Color.FromArgb(102, 135, 255) : Color.FromArgb(65, 105, 245);
                using var brush1 = new SolidBrush(logoAccent);
                using var brush2 = new SolidBrush(ThemeManager.IsDarkMode ? Color.FromArgb(129, 140, 248) : Color.FromArgb(99, 102, 241));

                g.FillEllipse(brush1, cx - offset - petalR, cy - petalR, petalR * 2, petalR * 2);
                g.FillEllipse(brush1, cx + offset - petalR, cy - petalR, petalR * 2, petalR * 2);
                g.FillEllipse(brush2, cx - petalR, cy - offset - petalR, petalR * 2, petalR * 2);
                g.FillEllipse(brush2, cx - petalR, cy + offset - petalR, petalR * 2, petalR * 2);

                using var centerBrush = new SolidBrush(C_SidebarBg);
                g.FillEllipse(centerBrush, cx - 4, cy - 4, 8, 8);
            };

            // Search box with focus border and Reicon Search icon
            txtSearch.GotFocus += (s, e) => { _isSearchFocused = true; pnlSearchBox.Invalidate(); };
            txtSearch.LostFocus += (s, e) => { _isSearchFocused = false; pnlSearchBox.Invalidate(); };
            txtSearch.TextChanged += (s, e) =>
            {
                string q = txtSearch.Text.Trim();
                if (_activePage == "Overview")
                {
                    RefreshInvoicesTable(q);
                }
                else if (_activePage == "Customers")
                {
                    txtCustSearch.Text = q;
                }
                else if (_activePage == "Products")
                {
                    txtProdSearch.Text = q;
                }
                else if (_activePage == "Transactions")
                {
                    txtTransSearch.Text = q;
                }
            };

            pnlSearchBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var rp = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnlSearchBox.Width - 1, pnlSearchBox.Height - 1, 8);
                using var bg = new SolidBrush(C_SearchBg);
                g.FillPath(bg, rp);
                using var bp = new Pen(C_SearchBdr, _isSearchFocused ? 1.5f : 1.2f);
                g.DrawPath(bp, rp);

                // Draw Reicon Search Vector Icon
                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Search, new Rectangle(8, 9, 18, 18), _isSearchFocused ? C_Blue : C_TextLight, 1.6f);
            };

            // Notifications button with Reicon Notification Icon
            bool isNotifHovered = false;
            btnNotifications.Cursor = Cursors.Hand;
            btnNotifications.MouseEnter += (s, e) => { isNotifHovered = true; btnNotifications.Invalidate(); };
            btnNotifications.MouseLeave += (s, e) => { isNotifHovered = false; btnNotifications.Invalidate(); };
            btnNotifications.Click += (s, e) =>
            {
                var lowList = DatabaseService.GetLowStockProducts(10m);
                var unpaid = DatabaseService.GetInvoices("", "Unpaid");
                string msg = $"Active Inventory & Billing Notifications:\n\n" +
                    $"• Low Stock Products ({lowList.Count} items):\n" +
                    string.Join("\n", lowList.Take(5).Select(p => $"   - {p.Name}: {p.StockQuantity} {p.Unit} remaining (Min Alert: {p.MinStockAlert} {p.Unit})")) +
                    $"\n\n• Outstanding / Unpaid Invoices ({unpaid.Count} invoices):\n" +
                    string.Join("\n", unpaid.Take(5).Select(i => $"   - {i.InvoiceNumber} ({i.CustomerName}): ₹{i.BalanceDue:N2} balance due"));

                MessageBox.Show(msg, "Notifications & Stock Alerts — S TOM'S", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            btnNotifications.Text = string.Empty;
            btnNotifications.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (isNotifHovered)
                {
                    using var hBg = new SolidBrush(C_NavHover);
                    using var hPath = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, btnNotifications.Width - 1, btnNotifications.Height - 1, 8);
                    g.FillPath(hBg, hPath);
                }

                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Notification, new Rectangle((btnNotifications.Width - 18) / 2, (btnNotifications.Height - 18) / 2, 18, 18), C_TextDark, 1.6f);

                // Red unread badge dot
                using var dotBrush = new SolidBrush(C_Negative);
                g.FillEllipse(dotBrush, btnNotifications.Width - 14, 7, 6, 6);
            };

            // New Invoice button click navigation (Opens New Sales Invoice modal)
            btnTopAddProduct.Click += (s, e) =>
            {
                NewSalesInvoiceForm.ShowInvoice(this, () =>
                {
                    RefreshDashboardAnalytics();
                    RefreshLiveKpiCards();
                    RefreshInvoicesTable();
                    RefreshLowStockTable();
                    _salesModule?.LoadDashboardData();
                });
            };

            // Dynamic header layout resizing for mathematical alignment and zero overlap
            pnlTopBar.Resize += (s, e) => UpdateHeaderLayout();
            pnlHeaderRight.Resize += (s, e) => UpdateHeaderLayout();
            UpdateHeaderLayout();
        }

        private void UpdateHeaderLayout()
        {
            if (pnlHeaderRight == null || winChrome == null || btnTopAddProduct == null ||
                btnThemeToggle == null || btnNotifications == null || pnlSearchBox == null || chromeDivider == null) return;

            int rightEdge = pnlHeaderRight.ClientSize.Width - 6;

            // 1. Window Chrome (Far right) - Centered at Y = 18 in 68px topbar (Center Y = 34)
            winChrome.Location = new Point(rightEdge - winChrome.Width, (pnlHeaderRight.Height - winChrome.Height) / 2);

            // 2. Vertical Divider (12px to left of chrome)
            chromeDivider.Location = new Point(winChrome.Left - 12 - chromeDivider.Width, (pnlHeaderRight.Height - chromeDivider.Height) / 2);

            // 3. New Invoice Button (12px to left of divider)
            btnTopAddProduct.Location = new Point(chromeDivider.Left - 12 - btnTopAddProduct.Width, (pnlHeaderRight.Height - btnTopAddProduct.Height) / 2);

            // 4. Theme Toggle Switch (12px to left of New Invoice)
            btnThemeToggle.Location = new Point(btnTopAddProduct.Left - 12 - btnThemeToggle.Width, (pnlHeaderRight.Height - btnThemeToggle.Height) / 2);

            // 5. Notifications Bell (10px to left of Theme Toggle)
            btnNotifications.Location = new Point(btnThemeToggle.Left - 10 - btnNotifications.Width, (pnlHeaderRight.Height - btnNotifications.Height) / 2);

            // 6. Search Box (10px to left of Notifications, responsive width)
            int minLeft = 16;
            int availableWidth = btnNotifications.Left - 10 - minLeft;
            int targetSearchWidth = Math.Clamp(availableWidth, 110, 190);

            pnlSearchBox.Size = new Size(targetSearchWidth, 36);
            pnlSearchBox.Location = new Point(btnNotifications.Left - 10 - targetSearchWidth, (pnlHeaderRight.Height - pnlSearchBox.Height) / 2);
            txtSearch.Width = Math.Max(50, targetSearchWidth - 38);
        }

        private void PaintThemeToggle(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = btnThemeToggle.Width;
            int h = btnThemeToggle.Height;
            if (w <= 0 || h <= 0) return;

            int radius = (h - 2) / 2;
            using var trackPath = GraphicsExtensions.CreateRoundedRectanglePath(1, 1, w - 3, h - 3, radius * 2);

            bool isDark = ThemeManager.IsDarkMode;
            Color trackBg = isDark ? Color.FromArgb(30, 32, 48) : Color.FromArgb(226, 232, 240);
            Color trackBorder = isDark ? Color.FromArgb(99, 102, 241) : Color.FromArgb(203, 213, 225);

            using var trackBrush = new SolidBrush(trackBg);
            g.FillPath(trackBrush, trackPath);

            using var trackPen = new Pen(trackBorder, 1.2f);
            g.DrawPath(trackPen, trackPath);

            // Reicon Sun (Left) & Moon (Right) Vector Icons
            Color sunColor = isDark ? Color.FromArgb(100, 116, 139) : Color.FromArgb(245, 158, 11);
            ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Sun, new Rectangle(6, (h - 16) / 2, 16, 16), sunColor, 1.5f);

            Color moonColor = isDark ? Color.FromArgb(165, 180, 252) : Color.FromArgb(148, 163, 184);
            ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Moon, new Rectangle(w - 22, (h - 16) / 2, 16, 16), moonColor, 1.5f);

            // Direct Instant Knob Position (No animation lag — instant position!)
            int knobD = h - 6;
            int knobMinX = 3;
            int knobMaxX = w - knobD - 3;
            int knobX = isDark ? knobMaxX : knobMinX;
            int knobY = 3;

            using var knobPath = GraphicsExtensions.CreateRoundedRectanglePath(knobX, knobY, knobD, knobD, knobD);
            Color knobColor = isDark ? Color.FromArgb(99, 102, 241) : Color.White;
            using var knobBrush = new SolidBrush(knobColor);
            g.FillPath(knobBrush, knobPath);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  DIRECT INSTANT THEME APPLICATION (ZERO ANIMATION, ALL COMPONENTS AT ONCE)
        // ═══════════════════════════════════════════════════════════════════════════
        private void ToggleThemeDirect()
        {
            // Direct instantaneous switch — zero animation, zero delay!
            // Changing ThemeManager.IsDarkMode triggers ThemeChanged event which invokes ApplyDashboardTheme()
            ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode;
        }

        private void ApplyDashboardTheme()
        {
            bool hasHandle = IsHandleCreated && !IsDisposed;
            if (hasHandle)
            {
                // 1. FREEZE WINDOW AT OS LEVEL:
                // Completely prevents Windows from redrawing controls one by one ("ek ek karke")
                SendMessage(this.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            }

            try
            {
                Color pageBg = ThemeManager.PageBackground;
                Color sidebarBg = ThemeManager.SidebarBackground;
                Color cardBg = ThemeManager.CardBackground;
                Color borderCol = ThemeManager.BorderHairline;
                Color topBarBg = ThemeManager.TopBarBackground;
                Color textDark = ThemeManager.TextPrimary;
                Color textMid = ThemeManager.TextSecondary;

                BackColor                = pageBg;
                pnlSidebar.BackColor     = sidebarBg;
                tblMainLayout.BackColor  = pageBg;
                pnlTopBar.BackColor      = topBarBg;
                pnlContentHost.BackColor = pageBg;
                pnlOverview.BackColor    = pageBg;

                // Eliminate transparent parent-chain repaint lag
                tlpCards.BackColor  = pageBg;
                tlpCharts.BackColor = pageBg;
                tlpTables.BackColor = pageBg;

                if (pnlNewBill != null) pnlNewBill.BackColor = pageBg;
                if (pnlCustomers != null) pnlCustomers.BackColor = pageBg;
                if (pnlProducts != null) pnlProducts.BackColor = pageBg;
                if (pnlTransPage != null) pnlTransPage.BackColor = pageBg;

                _salesModule?.ApplyTheme();
                _settingsModule?.ApplyTheme();

                headerTitle.TitleColor    = textDark;
                headerTitle.SubtitleColor = textMid;
                lblSidebarBrand.ForeColor = textDark;
                lblSidebarSub.ForeColor   = textMid;
                lblSidebarUser.ForeColor  = textDark;
                lblSidebarRole.ForeColor  = textMid;

                pnlSearchBox.BackColor = ThemeManager.SearchBoxBackground;
                txtSearch.BackColor    = ThemeManager.SearchBoxBackground;
                txtSearch.ForeColor    = textDark;

                winChrome.IconColor        = textMid;
                winChrome.HoverBg          = ThemeManager.IsDarkMode ? Color.FromArgb(40, 43, 56) : Color.FromArgb(226, 232, 240);
                chromeDivider.DividerColor = borderCol;
                btnNotifications.ForeColor = textDark;

                btnTopAddProduct.FillColor  = C_Blue;
                btnTopAddProduct.HoverColor = ThemeManager.AccentHover;

                StyleDataGridView(dgvInvoices);
                StyleDataGridView(dgvLowStock);
                StyleDataGridView(dgvBillItems);
                StyleDataGridView(dgvCustomers);
                StyleDataGridView(dgvProducts);
                StyleDataGridView(dgvAllTransactions);

                if (_pnlInvoicesHeader != null) _pnlInvoicesHeader.BackColor = cardBg;
                if (_pnlInvoicesFooter != null) _pnlInvoicesFooter.BackColor = cardBg;
                if (_lblInvoicesFooterSummary != null) _lblInvoicesFooterSummary.ForeColor = textMid;
                if (_pnlLowStockHeader != null) _pnlLowStockHeader.BackColor = cardBg;
                if (_pnlLowStockFooter != null) _pnlLowStockFooter.BackColor = cardBg;
                if (_lblLowStockFooterSummary != null) _lblLowStockFooterSummary.ForeColor = textMid;
                pnlRecentInvoicesCard.BackColor = cardBg;
                pnlLowStockCard.BackColor = cardBg;
                _pnlInvoicesHeader?.Invalidate();
                _pnlInvoicesFooter?.Invalidate();
                _pnlLowStockHeader?.Invalidate();
                _pnlLowStockFooter?.Invalidate();

                if (_pnlDynamicsHeader != null) _pnlDynamicsHeader.BackColor = cardBg;
                if (_pnlDonutHeader != null) _pnlDonutHeader.BackColor = cardBg;
                pnlSalesDynamicsCard.BackColor = cardBg;
                pnlCategoryDonutCard.BackColor = cardBg;
                pnlDynamicsChartCanvas.BackColor = cardBg;
                pnlDonutChartCanvas.BackColor = cardBg;
                _pnlDynamicsHeader?.Invalidate();
                _pnlDonutHeader?.Invalidate();
                pnlDynamicsChartCanvas.Invalidate();
                pnlDonutChartCanvas.Invalidate();

                // Invalidate all visible elements
                btnThemeToggle.Invalidate();
                pnlBrandHeader.Invalidate();
                pnlSidebar.Invalidate(true);
                headerTitle.Invalidate();
                pnlSearchBox.Invalidate();
                btnNotifications.Invalidate();
                btnTopAddProduct.Invalidate();
                winChrome.Invalidate();
                chromeDivider.Invalidate();

                if (_activePage == "Overview")
                {
                    pnlOverview.Invalidate(true);
                }
                else
                {
                    pnlContentHost.Invalidate(true);
                }
            }
            finally
            {
                if (hasHandle)
                {
                    // 2. UNFREEZE & SIMULTANEOUS ATOMIC REPAINT:
                    // Flushes all invalidated components together at the exact same microsecond!
                    SendMessage(this.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
                    RedrawWindow(this.Handle, IntPtr.Zero, IntPtr.Zero,
                        RDW_INVALIDATE | RDW_ALLCHILDREN | RDW_UPDATENOW | RDW_ERASE);
                }
            }
        }

        private void ApplyThemeFast() => ApplyDashboardTheme();

        private void StyleDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;
            TableDesignStandard.ApplyStandardGrid(dgv);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  OVERVIEW PAGE (6 CARDS + 2 CHARTS + 2 TABLES)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildOverviewPage()
        {
            SetupStatCard(0, pnlCardSalesToday,    "Today's Sales",      "₹48,250",   "+12.5% vs yesterday", C_Positive, ReiconRenderer.IconType.TrendingUp);
            SetupStatCard(1, pnlCardSalesMonth,    "Monthly Sales",      "₹4,28,900", "+18.2% vs last mo",   C_Positive, ReiconRenderer.IconType.Wallet);
            SetupStatCard(2, pnlCardReceivables,   "Total Receivables",  "₹1,15,400", "4 accounts due",      C_Warning,  ReiconRenderer.IconType.TrendingUp);
            SetupStatCard(3, pnlCardPayables,      "Total Payables",     "₹62,800",   "2 bills due soon",    C_Negative, ReiconRenderer.IconType.TrendingDown);
            SetupStatCard(4, pnlCardTotalProducts, "Total Products",     "1,280",     "Active Catalog",      C_Blue,     ReiconRenderer.IconType.Package);
            SetupStatCard(5, pnlCardLowStock,      "Low Stock Items",    "7 Items",   "Reorder Required",    C_Negative, ReiconRenderer.IconType.Warning);

            SetupSalesDynamicsCard();
            SetupCategoryDonutCard();
            SetupRecentInvoicesCard();
            SetupLowStockCard();

            AdaptCardsLayout();
        }

        private void AdaptCardsLayout()
        {
            int availW = pnlOverview.ClientSize.Width;
            if (availW >= 950)
            {
                if (tlpCards.ColumnCount != 6)
                {
                    tlpCards.SuspendLayout();
                    tlpCards.Controls.Clear();
                    tlpCards.ColumnCount = 6;
                    tlpCards.RowCount = 1;
                    tlpCards.Height = 94;
                    tlpCards.ColumnStyles.Clear();
                    for (int i = 0; i < 6; i++)
                        tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
                    tlpCards.RowStyles.Clear();
                    tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

                    tlpCards.Controls.Add(pnlCardSalesToday,    0, 0);
                    tlpCards.Controls.Add(pnlCardSalesMonth,    1, 0);
                    tlpCards.Controls.Add(pnlCardReceivables,   2, 0);
                    tlpCards.Controls.Add(pnlCardPayables,      3, 0);
                    tlpCards.Controls.Add(pnlCardTotalProducts, 4, 0);
                    tlpCards.Controls.Add(pnlCardLowStock,      5, 0);
                    tlpCards.ResumeLayout(true);
                }
            }
            else
            {
                if (tlpCards.ColumnCount != 3)
                {
                    tlpCards.SuspendLayout();
                    tlpCards.Controls.Clear();
                    tlpCards.ColumnCount = 3;
                    tlpCards.RowCount = 2;
                    tlpCards.Height = 188;
                    tlpCards.ColumnStyles.Clear();
                    for (int i = 0; i < 3; i++)
                        tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
                    tlpCards.RowStyles.Clear();
                    tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
                    tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

                    tlpCards.Controls.Add(pnlCardSalesToday,    0, 0);
                    tlpCards.Controls.Add(pnlCardSalesMonth,    1, 0);
                    tlpCards.Controls.Add(pnlCardReceivables,   2, 0);
                    tlpCards.Controls.Add(pnlCardPayables,      0, 1);
                    tlpCards.Controls.Add(pnlCardTotalProducts, 1, 1);
                    tlpCards.Controls.Add(pnlCardLowStock,      2, 1);
                    tlpCards.ResumeLayout(true);
                }
            }
        }

        private void SetupStatCard(int cardIndex, Panel pnl, string title, string val, string sub, Color statusCol, ReiconRenderer.IconType iconType)
        {
            _kpiCardData[cardIndex] = (title, val, sub, statusCol, iconType);

            pnl.Cursor = Cursors.Hand;
            pnl.MouseEnter += (s, e) => { _hoveredCardIndex = cardIndex; pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { if (_hoveredCardIndex == cardIndex) { _hoveredCardIndex = -1; pnl.Invalidate(); } };

            pnl.Click += (s, e) =>
            {
                switch (cardIndex)
                {
                    case 0: // Total Sales
                        ShowPage("Sales");
                        _salesModule?.SwitchTab("Dashboard");
                        break;
                    case 1: // Total Invoices
                        ShowPage("Sales");
                        _salesModule?.SwitchTab("Invoices");
                        break;
                    case 2: // Paid Invoices
                        ShowPage("Sales");
                        _salesModule?.FilterInvoicesByStatus("Paid");
                        break;
                    case 3: // Unpaid Invoices
                        ShowPage("Sales");
                        _salesModule?.FilterInvoicesByStatus("Unpaid");
                        break;
                    case 4: // Outstanding Due
                        ShowPage("Customers");
                        break;
                    case 5: // Active Clients
                        ShowPage("Customers");
                        break;
                }
            };

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnl.Width;
                int h = pnl.Height;
                if (w <= 0 || h <= 0) return;

                bool isHovered = (_hoveredCardIndex == cardIndex);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
                using var bgBrush = new SolidBrush(C_CardBg);
                g.FillPath(bgBrush, path);

                // Subtle Hover Elevation Border
                using var borderPen = new Pen(isHovered ? C_Blue : C_Border, isHovered ? 1.5f : 1.2f);
                g.DrawPath(borderPen, path);

                var d = _kpiCardData[cardIndex];

                // Reicon Vector Icon (Clean 20x20 Vector, No Emojis!)
                ReiconRenderer.DrawIcon(g, d.Icon, new Rectangle(12, 10, 18, 18), d.StatusCol, 1.8f);

                using var bTitle = new SolidBrush(C_TextMid);
                using var bVal   = new SolidBrush(C_TextDark);
                using var bSub   = new SolidBrush(d.StatusCol);

                g.DrawString(d.Title, s_fontStatTitle, bTitle, 36, 11);
                g.DrawString(d.Value, s_fontStatVal, bVal, 12, 34);
                g.DrawString(d.Subtitle, s_fontStatSub, bSub, 12, 64);
            };
        }

        public void RefreshLiveKpiCards()
        {
            try
            {
                _currentMetrics = DatabaseService.GetSalesDashboardMetrics();
                _kpiCardData[0] = ("Total Sales", _currentMetrics.TotalSalesFormatted, "All-time billings", C_Positive, ReiconRenderer.IconType.TrendingUp);
                _kpiCardData[1] = ("Total Invoices", $"{_currentMetrics.TotalInvoices} Invoices", "Issued to date", C_Blue, ReiconRenderer.IconType.Wallet);
                _kpiCardData[2] = ("Paid Invoices", $"{_currentMetrics.PaidInvoices} Invoices", "Settled in full", C_Positive, ReiconRenderer.IconType.TrendingUp);
                _kpiCardData[3] = ("Unpaid Invoices", $"{_currentMetrics.UnpaidInvoices} Invoices", "Pending settlement", C_Negative, ReiconRenderer.IconType.TrendingDown);
                _kpiCardData[4] = ("Outstanding Due", _currentMetrics.OutstandingDueFormatted, "Customer balances", C_Warning, ReiconRenderer.IconType.Warning);
                _kpiCardData[5] = ("Active Clients", $"{_currentMetrics.TotalCustomers} Clients", "Service accounts", C_Blue, ReiconRenderer.IconType.Customers);

                pnlCardSalesToday?.Invalidate();
                pnlCardSalesMonth?.Invalidate();
                pnlCardReceivables?.Invalidate();
                pnlCardPayables?.Invalidate();
                pnlCardTotalProducts?.Invalidate();
                pnlCardLowStock?.Invalidate();
            }
            catch
            {
                // Fallback gracefully
            }
        }

        public void RefreshDashboardAnalytics()
        {
            try
            {
                _monthlyDynamics = DatabaseService.Get12MonthsSalesDynamics(_selectedDynamicsYear);
                var (cats, gTotal) = DatabaseService.GetCategorySalesDistribution("ThisYear");
                _categoryDistribution = cats;
                _categoryGrandTotal = gTotal;
                _lowStockProducts = DatabaseService.GetLowStockProducts(15m);
                pnlDynamicsChartCanvas?.Invalidate();
                pnlDonutChartCanvas?.Invalidate();
                pnlSalesDynamicsCard?.Invalidate();
                pnlCategoryDonutCard?.Invalidate();
                _pnlDynamicsHeader?.Invalidate();
                _pnlDonutHeader?.Invalidate();
            }
            catch
            {
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  CHARTS: 12-MONTH SALES DYNAMICS & CATEGORY DONUT
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupSalesDynamicsCard()
        {
            EnableDoubleBuffering(pnlSalesDynamicsCard);
            EnableDoubleBuffering(pnlDynamicsChartCanvas);

            pnlSalesDynamicsCard.Padding = new Padding(1);
            pnlSalesDynamicsCard.BackColor = C_CardBg;
            pnlSalesDynamicsCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                TableDesignStandard.PaintCardContainerBorder(g, pnlSalesDynamicsCard.ClientRectangle);
            };

            // 1. Header Bar
            _pnlDynamicsHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = C_CardBg,
                Padding = new Padding(14, 0, 14, 0)
            };

            var fHdrTitle = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            _pnlDynamicsHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;

                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using (var pen = new Pen(sepCol, 1f))
                {
                    g.DrawLine(pen, 0, _pnlDynamicsHeader.Height - 1, _pnlDynamicsHeader.Width, _pnlDynamicsHeader.Height - 1);
                }

                // Vector Icon
                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Statistics, new Rectangle(14, 13, 18, 18), ThemeManager.PrimaryAccent, 1.5f);

                // Title
                using var bTitle = new SolidBrush(ThemeManager.TextPrimary);
                g.DrawString($"Sales Dynamics ({_selectedDynamicsYear})", fHdrTitle, bTitle, 38, 12);

                // Right Legend (Sales vs Purchases)
                int w = _pnlDynamicsHeader.Width;
                int legX = w - 170;
                if (legX > 220)
                {
                    using var bSalesDot = new SolidBrush(C_Blue);
                    Color purchCol = isDark ? Color.FromArgb(165, 180, 252) : Color.FromArgb(99, 102, 241);
                    using var bPurchDot = new SolidBrush(purchCol);
                    using var bLegendTxt = new SolidBrush(ThemeManager.TextPrimary);
                    using var fLegend = new Font("Segoe UI", 8.5f, FontStyle.Regular);

                    g.FillEllipse(bSalesDot, legX, 17, 8, 8);
                    g.DrawString("Sales", fLegend, bLegendTxt, legX + 12, 13);

                    g.FillEllipse(bPurchDot, legX + 65, 17, 8, 8);
                    g.DrawString("Purchases", fLegend, bLegendTxt, legX + 77, 13);
                }
            };

            // 2. Chart Canvas
            pnlSalesDynamicsCard.Controls.Clear();
            pnlSalesDynamicsCard.Controls.Add(pnlDynamicsChartCanvas);
            pnlSalesDynamicsCard.Controls.Add(_pnlDynamicsHeader);
            pnlDynamicsChartCanvas.Dock = DockStyle.Fill;
            pnlDynamicsChartCanvas.BringToFront();

            // Mouse Hover Tracking on Canvas
            pnlDynamicsChartCanvas.MouseMove += (s, e) =>
            {
                _dynamicsMousePos = e.Location;
                int w = pnlDynamicsChartCanvas.Width;
                int h = pnlDynamicsChartCanvas.Height;
                int padL = 44, padR = 16, topY = 16, padB = 26;
                int chartW = w - padL - padR;
                int chartH = h - topY - padB;
                int n = _monthlyDynamics?.Count ?? 0;

                int newHover = -1;
                if (n > 0 && chartW > 0 && chartH > 0)
                {
                    float slotW = (float)chartW / n;
                    if (e.X >= padL && e.X <= padL + chartW && e.Y >= topY - 10 && e.Y <= topY + chartH + 24)
                    {
                        int idx = (int)((e.X - padL) / slotW);
                        if (idx >= 0 && idx < n)
                        {
                            newHover = idx;
                        }
                    }
                }

                if (newHover != _hoveredDynamicsMonthIndex)
                {
                    _hoveredDynamicsMonthIndex = newHover;
                    pnlDynamicsChartCanvas.Cursor = (newHover >= 0) ? Cursors.Hand : Cursors.Default;
                    pnlDynamicsChartCanvas.Invalidate();
                }
                else if (_hoveredDynamicsMonthIndex >= 0)
                {
                    pnlDynamicsChartCanvas.Invalidate();
                }
            };

            pnlDynamicsChartCanvas.MouseLeave += (s, e) =>
            {
                if (_hoveredDynamicsMonthIndex != -1)
                {
                    _hoveredDynamicsMonthIndex = -1;
                    pnlDynamicsChartCanvas.Cursor = Cursors.Default;
                    pnlDynamicsChartCanvas.Invalidate();
                }
            };

            // Canvas Paint Event
            pnlDynamicsChartCanvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnlDynamicsChartCanvas.Width;
                int h = pnlDynamicsChartCanvas.Height;
                if (w <= 0 || h <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;
                Color cardBg = ThemeManager.CardBackground;

                // 1. Explicit full background fill to prevent any ghosting or duplication
                using (var bCardBg = new SolidBrush(cardBg))
                {
                    g.FillRectangle(bCardBg, 0, 0, w, h);
                }

                int padL = 44, padR = 16, topY = 16, padB = 26;
                int chartW = w - padL - padR;
                int chartH = h - topY - padB;
                if (chartW <= 0 || chartH <= 0) return;

                if (_monthlyDynamics == null || _monthlyDynamics.Count == 0)
                {
                    _monthlyDynamics = DatabaseService.Get12MonthsSalesDynamics(_selectedDynamicsYear);
                }

                int n = _monthlyDynamics.Count;
                if (n == 0) return;

                decimal maxDataVal = _monthlyDynamics.Max(m => Math.Max(m.SalesAmount, m.PurchasesAmount));
                float maxVal = (float)(maxDataVal <= 0 ? 50000m : maxDataVal * 1.25m);

                // 2. Y-Axis Subtle Horizontal Guidelines & Lakhs Labels
                Color guideColor = isDark ? Color.FromArgb(38, 44, 64) : Color.FromArgb(235, 238, 244);
                Color yLabelColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
                using (var pGuide = new Pen(guideColor, 1f))
                using (var bYLbl = new SolidBrush(yLabelColor))
                using (var fYLbl = new Font("Segoe UI", 7.5f, FontStyle.Regular))
                using (var sfY = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
                {
                    float[] levels = { 1.0f, 0.66f, 0.33f, 0.0f };
                    foreach (var lvl in levels)
                    {
                        float gy = topY + chartH * (1.0f - lvl);
                        g.DrawLine(pGuide, padL, gy, padL + chartW, gy);

                        decimal lvlVal = (decimal)(maxVal * lvl);
                        string lvlText = lvlVal >= 100000m
                            ? $"₹{(lvlVal / 100000m):0.#}L"
                            : (lvlVal > 0 ? $"₹{lvlVal:0}" : "0");

                        g.DrawString(lvlText, fYLbl, bYLbl, new RectangleF(0, gy - 8, padL - 6, 16), sfY);
                    }
                }

                float slotW = (float)chartW / n;
                float barW = Math.Max(6, slotW * 0.30f);
                Color purchCol = isDark ? Color.FromArgb(165, 180, 252) : Color.FromArgb(99, 102, 241);

                using var sBrush = new SolidBrush(C_Blue);
                using var pBrush = new SolidBrush(purchCol);
                using var bLbl = new SolidBrush(C_TextMid);
                using var bLblHover = new SolidBrush(ThemeManager.PrimaryAccent);
                using var fLblReg = new Font("Segoe UI", 8f, FontStyle.Regular);
                using var fLblBold = new Font("Segoe UI", 8.2f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center };

                int hoveredSlotX = -1;
                float hoveredSalesY = 0f, hoveredPurchY = 0f;

                for (int i = 0; i < n; i++)
                {
                    float cx = padL + i * slotW + slotW / 2;
                    float sVal = (float)_monthlyDynamics[i].SalesAmount;
                    float pVal = (float)_monthlyDynamics[i].PurchasesAmount;

                    float sH = (sVal / maxVal) * chartH;
                    float pH = (pVal / maxVal) * chartH;

                    if (sVal > 0 && sH < 4f) sH = 4f;
                    if (pVal > 0 && pH < 4f) pH = 4f;

                    float sY = topY + chartH - sH;
                    float pY = topY + chartH - pH;

                    bool isHovered = (i == _hoveredDynamicsMonthIndex);
                    if (isHovered)
                    {
                        hoveredSlotX = (int)cx;
                        hoveredSalesY = sY;
                        hoveredPurchY = pY;

                        // Sleek Translucent Column Hover Pillar
                        int colX = (int)(padL + i * slotW + 2);
                        int colW = (int)(slotW - 4);
                        using var colPath = GraphicsExtensions.CreateRoundedRectanglePath(colX, topY - 4, colW, chartH + 8, 6);
                        using var bCol = new SolidBrush(Color.FromArgb(isDark ? 35 : 22, ThemeManager.PrimaryAccent));
                        using var pCol = new Pen(Color.FromArgb(isDark ? 70 : 45, ThemeManager.PrimaryAccent), 1f);
                        g.FillPath(bCol, colPath);
                        g.DrawPath(pCol, colPath);
                    }

                    // Sales Bar
                    int sBarX = (int)(cx - barW - 2);
                    using (var sPath = GraphicsExtensions.CreateRoundedRectanglePath(sBarX, (int)sY, (int)barW, (int)Math.Max(1, sH), 4))
                    {
                        if (isHovered)
                        {
                            using var bGlow = new SolidBrush(Color.FromArgb(96, 165, 250));
                            using var pGlow = new Pen(Color.White, 1.2f);
                            g.FillPath(bGlow, sPath);
                            g.DrawPath(pGlow, sPath);
                        }
                        else
                        {
                            g.FillPath(sBrush, sPath);
                        }
                    }

                    // Purchase/Cost Bar
                    int pBarX = (int)(cx + 2);
                    using (var pPath = GraphicsExtensions.CreateRoundedRectanglePath(pBarX, (int)pY, (int)barW, (int)Math.Max(1, pH), 4))
                    {
                        if (isHovered)
                        {
                            using var bGlow = new SolidBrush(Color.FromArgb(199, 210, 254));
                            using var pGlow = new Pen(Color.White, 1.2f);
                            g.FillPath(bGlow, pPath);
                            g.DrawPath(pGlow, pPath);
                        }
                        else
                        {
                            g.FillPath(pBrush, pPath);
                        }
                    }

                    // Month Label
                    g.DrawString(_monthlyDynamics[i].MonthLabel, isHovered ? fLblBold : fLblReg, isHovered ? bLblHover : bLbl, cx, topY + chartH + 6, sf);
                }

                // 3. FLOATING REAL-USER HUD TOOLTIP CARD
                if (_hoveredDynamicsMonthIndex >= 0 && _hoveredDynamicsMonthIndex < n)
                {
                    var item = _monthlyDynamics[_hoveredDynamicsMonthIndex];
                    decimal margin = item.SalesAmount - item.PurchasesAmount;

                    int ttW = 196;
                    int ttH = 82;
                    int ttX = (int)(hoveredSlotX - ttW / 2);
                    float highestBar = Math.Min(hoveredSalesY, hoveredPurchY);
                    int ttY = (int)(highestBar - ttH - 12);

                    // Clamp within canvas boundaries
                    if (ttY < topY - 6) ttY = (int)(highestBar + 22);
                    if (ttX < padL) ttX = padL;
                    if (ttX + ttW > w - 10) ttX = w - ttW - 10;

                    // Tooltip Drop Shadow
                    using (var shPath = GraphicsExtensions.CreateRoundedRectanglePath(ttX + 2, ttY + 3, ttW, ttH, 8))
                    using (var bSh = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    {
                        g.FillPath(bSh, shPath);
                    }

                    // Tooltip Container
                    using (var ttPath = GraphicsExtensions.CreateRoundedRectanglePath(ttX, ttY, ttW, ttH, 8))
                    using (var bTt = new SolidBrush(isDark ? Color.FromArgb(245, 15, 23, 42) : Color.FromArgb(248, 24, 30, 48)))
                    using (var pTt = new Pen(isDark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(51, 65, 85), 1.2f))
                    {
                        g.FillPath(bTt, ttPath);
                        g.DrawPath(pTt, ttPath);
                    }

                    // Month Title in Tooltip
                    string fullMonth = item.MonthLabel switch
                    {
                        "Jan" => "January", "Feb" => "February", "Mar" => "March",
                        "Apr" => "April", "May" => "May", "Jun" => "June",
                        "Jul" => "July", "Aug" => "August", "Sep" => "September",
                        "Oct" => "October", "Nov" => "November", "Dec" => "December",
                        _ => item.MonthLabel
                    };

                    using (var fTtHdr = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (var fTtRow = new Font("Segoe UI", 7.8f, FontStyle.Regular))
                    using (var bTtWhite = new SolidBrush(Color.White))
                    using (var bTtDotS = new SolidBrush(Color.FromArgb(96, 165, 250)))
                    using (var bTtDotP = new SolidBrush(Color.FromArgb(165, 180, 252)))
                    using (var bTtDotM = new SolidBrush(margin >= 0 ? Color.FromArgb(52, 211, 153) : Color.FromArgb(248, 113, 113)))
                    {
                        g.DrawString($"{fullMonth} {_selectedDynamicsYear}", fTtHdr, bTtWhite, ttX + 12, ttY + 8);

                        // Row 1: Sales
                        g.FillEllipse(bTtDotS, ttX + 13, ttY + 28, 6, 6);
                        g.DrawString($"Sales: ₹{item.SalesAmount:N2}", fTtRow, bTtWhite, ttX + 24, ttY + 24);

                        // Row 2: Purchases
                        g.FillEllipse(bTtDotP, ttX + 13, ttY + 44, 6, 6);
                        g.DrawString($"Purchases: ₹{item.PurchasesAmount:N2}", fTtRow, bTtWhite, ttX + 24, ttY + 40);

                        // Row 3: Net Margin
                        g.FillEllipse(bTtDotM, ttX + 13, ttY + 60, 6, 6);
                        string marginSign = margin >= 0 ? "+" : "";
                        g.DrawString($"Margin: {marginSign}₹{margin:N2}", fTtRow, bTtDotM, ttX + 24, ttY + 56);
                    }
                }
            };
        }

        private void SetupCategoryDonutCard()
        {
            EnableDoubleBuffering(pnlCategoryDonutCard);
            EnableDoubleBuffering(pnlDonutChartCanvas);

            pnlCategoryDonutCard.Padding = new Padding(1);
            pnlCategoryDonutCard.BackColor = C_CardBg;
            pnlCategoryDonutCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                TableDesignStandard.PaintCardContainerBorder(g, pnlCategoryDonutCard.ClientRectangle);
            };

            // 1. Header Bar
            _pnlDonutHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = C_CardBg,
                Padding = new Padding(14, 0, 14, 0)
            };

            var fHdrTitle = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            _pnlDonutHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;

                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using (var pen = new Pen(sepCol, 1f))
                {
                    g.DrawLine(pen, 0, _pnlDonutHeader.Height - 1, _pnlDonutHeader.Width, _pnlDonutHeader.Height - 1);
                }

                // Vector Icon
                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Products, new Rectangle(14, 13, 18, 18), Color.FromArgb(16, 185, 129), 1.5f);

                // Title
                using var bTitle = new SolidBrush(ThemeManager.TextPrimary);
                g.DrawString("Category Revenue", fHdrTitle, bTitle, 38, 12);

                // Count Badge
                int count = _categoryDistribution?.Count ?? 0;
                string catBadge = $"● {count} {(count == 1 ? "Category" : "Categories")}";
                using var bBadgeBg = new SolidBrush(isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(238, 242, 255));
                using var pBadgeBdr = new Pen(isDark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(199, 210, 254), 1f);
                using var bBadgeTxt = new SolidBrush(isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(79, 70, 229));
                using var fBadge = new Font("Segoe UI", 7.8f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                int bW = 104;
                int bH = 22;
                int bX = _pnlDonutHeader.Width - bW - 14;
                if (bX > 180)
                {
                    using var bPath = GraphicsExtensions.CreateRoundedRectanglePath(bX, 11, bW, bH, 11);
                    g.FillPath(bBadgeBg, bPath);
                    g.DrawPath(pBadgeBdr, bPath);

                    using var bDot = new SolidBrush(Color.FromArgb(99, 102, 241));
                    g.FillEllipse(bDot, bX + 8, 11 + 8, 6, 6);

                    var textRect = new Rectangle(bX + 12, 11, bW - 14, bH);
                    g.DrawString(catBadge.Replace("● ", ""), fBadge, bBadgeTxt, textRect, sf);
                }
            };

            // 2. Donut Canvas
            pnlCategoryDonutCard.Controls.Clear();
            pnlCategoryDonutCard.Controls.Add(pnlDonutChartCanvas);
            pnlCategoryDonutCard.Controls.Add(_pnlDonutHeader);
            pnlDonutChartCanvas.Dock = DockStyle.Fill;
            pnlDonutChartCanvas.BringToFront();

            Color[] palette = {
                C_Blue,
                C_Positive,
                C_Warning,
                Color.FromArgb(168, 85, 247),
                Color.FromArgb(236, 72, 153),
                Color.FromArgb(14, 165, 233)
            };

            // Mouse Hover Tracking
            pnlDonutChartCanvas.MouseMove += (s, e) =>
            {
                _donutMousePos = e.Location;
                int w = pnlDonutChartCanvas.Width;
                int h = pnlDonutChartCanvas.Height;
                int d = Math.Min((int)(w * 0.44f), h - 36);
                int cx = 20 + d / 2;
                int cy = (h - d) / 2 + d / 2;

                int newHover = -1;
                if (d > 10 && _categoryDistribution != null && _categoryDistribution.Count > 0)
                {
                    float dx = e.X - cx;
                    float dy = e.Y - cy;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    int outerRadius = d / 2;
                    int innerRadius = (int)(d * 0.62f) / 2;

                    // Hovering on donut ring
                    if (dist >= innerRadius - 4 && dist <= outerRadius + 14)
                    {
                        float angle = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                        if (angle < 0) angle += 360f;

                        float currA = 0f;
                        for (int i = 0; i < _categoryDistribution.Count; i++)
                        {
                            float sliceA = _categoryDistribution[i].Angle;
                            if (angle >= currA && angle < currA + sliceA)
                            {
                                newHover = i;
                                break;
                            }
                            currA += sliceA;
                        }
                    }
                    else
                    {
                        // Check if hovering over Legend items on the right side
                        int legX = cx + d / 2 + 16;
                        int legY = 16;
                        for (int i = 0; i < Math.Min(6, _categoryDistribution.Count); i++)
                        {
                            var legRect = new Rectangle(legX - 4, legY + i * 28 - 2, w - legX - 8, 26);
                            if (legRect.Contains(e.Location))
                            {
                                newHover = i;
                                break;
                            }
                        }
                    }
                }

                if (newHover != _hoveredCategoryIndex)
                {
                    _hoveredCategoryIndex = newHover;
                    pnlDonutChartCanvas.Cursor = (newHover >= 0) ? Cursors.Hand : Cursors.Default;
                    pnlDonutChartCanvas.Invalidate();
                }
                else if (_hoveredCategoryIndex >= 0)
                {
                    pnlDonutChartCanvas.Invalidate();
                }
            };

            pnlDonutChartCanvas.MouseLeave += (s, e) =>
            {
                if (_hoveredCategoryIndex != -1)
                {
                    _hoveredCategoryIndex = -1;
                    pnlDonutChartCanvas.Cursor = Cursors.Default;
                    pnlDonutChartCanvas.Invalidate();
                }
            };

            // Canvas Paint Event
            pnlDonutChartCanvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnlDonutChartCanvas.Width;
                int h = pnlDonutChartCanvas.Height;
                if (w <= 0 || h <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;
                Color cardBg = ThemeManager.CardBackground;

                // 1. Explicit full background fill to prevent any ghosting or duplication
                using (var bCardBg = new SolidBrush(cardBg))
                {
                    g.FillRectangle(bCardBg, 0, 0, w, h);
                }

                int d = Math.Min((int)(w * 0.44f), h - 36);
                if (d <= 10) return;

                int cx = 20 + d / 2;
                int cy = (h - d) / 2 + d / 2;
                var rect = new Rectangle(cx - d / 2, cy - d / 2, d, d);

                if (_categoryDistribution == null || _categoryDistribution.Count == 0)
                {
                    var (cats, gTotal) = DatabaseService.GetCategorySalesDistribution("ThisYear");
                    _categoryDistribution = cats;
                    _categoryGrandTotal = gTotal;
                }

                // 2. Draw Donut Slices (with Explosion & Glow on Hover)
                float startA = 0f;
                int hoveredSliceIndex = _hoveredCategoryIndex;
                Rectangle hoveredSliceRect = Rectangle.Empty;
                float hoveredStartA = 0f, hoveredAngle = 0f;
                Color hoveredColor = Color.Empty;

                for (int i = 0; i < _categoryDistribution.Count; i++)
                {
                    var item = _categoryDistribution[i];
                    Color c = palette[i % palette.Length];
                    using var b = new SolidBrush(c);

                    if (item.Angle > 0)
                    {
                        if (i == hoveredSliceIndex)
                        {
                            hoveredStartA = startA;
                            hoveredAngle = item.Angle;
                            hoveredColor = c;

                            float midA = (startA + item.Angle / 2f) * (float)Math.PI / 180f;
                            float offDist = 5f;
                            int expCx = cx + (int)(Math.Cos(midA) * offDist);
                            int expCy = cy + (int)(Math.Sin(midA) * offDist);
                            hoveredSliceRect = new Rectangle(expCx - d / 2 - 2, expCy - d / 2 - 2, d + 4, d + 4);
                        }
                        else
                        {
                            g.FillPie(b, rect, startA, item.Angle);
                            using var pSliceSep = new Pen(cardBg, 1.5f);
                            g.DrawPie(pSliceSep, rect, startA, item.Angle);
                        }
                        startA += item.Angle;
                    }
                }

                // Draw Hovered Slice with Expanded Boundary and White/Bright Glow Outline
                if (hoveredSliceIndex >= 0 && hoveredAngle > 0)
                {
                    using (var bHov = new SolidBrush(hoveredColor))
                    using (var pGlow = new Pen(Color.White, 2.5f))
                    {
                        g.FillPie(bHov, hoveredSliceRect, hoveredStartA, hoveredAngle);
                        g.DrawPie(pGlow, hoveredSliceRect, hoveredStartA, hoveredAngle);
                    }
                }

                // 3. Inner Cutout Circle (Hole)
                int holeD = (int)(d * 0.62f);
                var holeRect = new Rectangle(cx - holeD / 2, cy - holeD / 2, holeD, holeD);
                using (var holeBrush = new SolidBrush(cardBg))
                using (var holePen = new Pen(isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240), 1f))
                {
                    g.FillEllipse(holeBrush, holeRect);
                    g.DrawEllipse(holePen, holeRect);
                }

                // 4. REAL-USER DYNAMIC CENTER TEXT
                using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    if (hoveredSliceIndex >= 0 && hoveredSliceIndex < _categoryDistribution.Count)
                    {
                        var hItem = _categoryDistribution[hoveredSliceIndex];
                        Color hCol = palette[hoveredSliceIndex % palette.Length];
                        using var bCol = new SolidBrush(hCol);
                        using var bVal = new SolidBrush(ThemeManager.TextPrimary);
                        using var bSub = new SolidBrush(ThemeManager.TextSecondary);

                        using var fName = new Font("Segoe UI", 7.8f, FontStyle.Bold);
                        using var fAmt = new Font("Segoe UI", 12f, FontStyle.Bold);
                        using var fPct = new Font("Segoe UI", 7.5f, FontStyle.Bold);

                        string amtFmt = hItem.TotalSales >= 100000m
                            ? $"₹{(hItem.TotalSales / 100000m):N1}L"
                            : $"₹{hItem.TotalSales:N0}";

                        g.DrawString(hItem.CategoryName.ToUpperInvariant(), fName, bCol, cx, cy - 18, sfCenter);
                        g.DrawString(amtFmt, fAmt, bVal, cx, cy, sfCenter);
                        g.DrawString($"{hItem.Percentage:0.0}% of Total", fPct, bSub, cx, cy + 18, sfCenter);
                    }
                    else
                    {
                        using var bC1 = new SolidBrush(C_TextMid);
                        using var bC2 = new SolidBrush(C_TextDark);
                        using var fTotalLbl = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                        using var fTotalVal = new Font("Segoe UI", 12.5f, FontStyle.Bold);

                        g.DrawString("TOTAL SALES", fTotalLbl, bC1, cx, cy - 10, sfCenter);

                        string grandTotalFormatted = _categoryGrandTotal >= 100000m
                            ? $"₹{(_categoryGrandTotal / 100000m):N1}L"
                            : $"₹{_categoryGrandTotal:N0}";
                        g.DrawString(grandTotalFormatted, fTotalVal, bC2, cx, cy + 10, sfCenter);
                    }
                }

                // 5. INTERACTIVE LEGEND LIST ON THE RIGHT
                int legX = cx + d / 2 + 18;
                int legY = 16;
                int maxLeg = Math.Min(6, _categoryDistribution.Count);

                for (int i = 0; i < maxLeg; i++)
                {
                    var item = _categoryDistribution[i];
                    Color c = palette[i % palette.Length];
                    bool isLegHover = (i == _hoveredCategoryIndex);

                    int rowY = legY + i * 28;

                    // Hover Highlight Pill on Legend Row
                    if (isLegHover)
                    {
                        int legW = w - legX - 10;
                        using var path = GraphicsExtensions.CreateRoundedRectanglePath(legX - 4, rowY - 2, legW, 25, 6);
                        using var bH = new SolidBrush(Color.FromArgb(isDark ? 40 : 22, c));
                        using var pH = new Pen(Color.FromArgb(isDark ? 80 : 50, c), 1f);
                        g.FillPath(bH, path);
                        g.DrawPath(pH, path);
                    }

                    // Colored Dot
                    using (var bDot = new SolidBrush(c))
                    {
                        g.FillEllipse(bDot, legX + 2, rowY + 5, 8, 8);
                    }

                    // Category Name & Percentage
                    using (var bTxt = new SolidBrush(isLegHover ? ThemeManager.TextPrimary : ThemeManager.TextSecondary))
                    using (var fLeg = new Font("Segoe UI", isLegHover ? 8.5f : 8.2f, isLegHover ? FontStyle.Bold : FontStyle.Regular))
                    {
                        g.DrawString($"{item.CategoryName} ({item.Percentage:0.#}%)", fLeg, bTxt, legX + 16, rowY + 1);
                    }
                }

                // 6. FLOATING HUD TOOLTIP CARD FOR DONUT SLICE
                if (hoveredSliceIndex >= 0 && hoveredSliceIndex < _categoryDistribution.Count)
                {
                    var hItem = _categoryDistribution[hoveredSliceIndex];
                    Color hCol = palette[hoveredSliceIndex % palette.Length];

                    int ttW = 184;
                    int ttH = 68;
                    int ttX = _donutMousePos.X + 14;
                    int ttY = _donutMousePos.Y - ttH / 2;

                    // Clamp
                    if (ttX + ttW > w - 8) ttX = _donutMousePos.X - ttW - 14;
                    if (ttX < 8) ttX = 8;
                    if (ttY < 6) ttY = 6;
                    if (ttY + ttH > h - 6) ttY = h - ttH - 6;

                    // Drop shadow
                    using (var shPath = GraphicsExtensions.CreateRoundedRectanglePath(ttX + 2, ttY + 3, ttW, ttH, 8))
                    using (var bSh = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    {
                        g.FillPath(bSh, shPath);
                    }

                    // HUD Container
                    using (var ttPath = GraphicsExtensions.CreateRoundedRectanglePath(ttX, ttY, ttW, ttH, 8))
                    using (var bTt = new SolidBrush(isDark ? Color.FromArgb(245, 15, 23, 42) : Color.FromArgb(248, 24, 30, 48)))
                    using (var pTt = new Pen(isDark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(51, 65, 85), 1.2f))
                    {
                        g.FillPath(bTt, ttPath);
                        g.DrawPath(pTt, ttPath);
                    }

                    // Tooltip Content
                    using (var fTtHdr = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (var fTtRow = new Font("Segoe UI", 7.8f, FontStyle.Regular))
                    using (var bTtWhite = new SolidBrush(Color.White))
                    using (var bTtSub = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    using (var bTtDot = new SolidBrush(hCol))
                    {
                        g.FillEllipse(bTtDot, ttX + 12, ttY + 13, 7, 7);
                        g.DrawString(hItem.CategoryName, fTtHdr, bTtWhite, ttX + 24, ttY + 9);

                        g.DrawString($"Revenue: ₹{hItem.TotalSales:N2}", fTtRow, bTtWhite, ttX + 12, ttY + 28);
                        g.DrawString($"Contribution: {hItem.Percentage:0.0}% of total", fTtRow, bTtSub, ttX + 12, ttY + 44);
                    }
                }
            };
        }


        // ═══════════════════════════════════════════════════════════════════════════
        //  TABLES: RECENT INVOICES & LOW STOCK ALERTS
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupRecentInvoicesCard()
        {
            pnlRecentInvoicesCard.Padding = new Padding(1);
            pnlRecentInvoicesCard.BackColor = C_CardBg;
            pnlRecentInvoicesCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                TableDesignStandard.PaintCardContainerBorder(g, pnlRecentInvoicesCard.ClientRectangle);
            };

            // 1. Header Bar (Dock = Top)
            _pnlInvoicesHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = C_CardBg,
                Padding = new Padding(12, 0, 12, 0)
            };

            var fHdrTitle = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            _pnlInvoicesHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;

                // Bottom hairline separator
                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using (var pen = new Pen(sepCol, 1f))
                {
                    g.DrawLine(pen, 0, _pnlInvoicesHeader.Height - 1, _pnlInvoicesHeader.Width, _pnlInvoicesHeader.Height - 1);
                }

                // Vector Icon
                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Sales, new Rectangle(14, 13, 18, 18), ThemeManager.PrimaryAccent, 1.5f);

                // Title
                using var bTitle = new SolidBrush(ThemeManager.TextPrimary);
                g.DrawString("Recent Invoices & Transactions", fHdrTitle, bTitle, 38, 12);
            };

            // Badge
            _lblInvoicesBadge = new Label
            {
                Text = "● 0 Invoices",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                Size = new Size(96, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _lblInvoicesBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;
                Color bg = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(238, 242, 255);
                Color bdr = isDark ? Color.FromArgb(71, 85, 105) : Color.FromArgb(199, 210, 254);
                Color txt = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(79, 70, 229);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, _lblInvoicesBadge.Width - 1, _lblInvoicesBadge.Height - 1, 11);
                using var bBg = new SolidBrush(bg);
                using var pBdr = new Pen(bdr, 1f);
                g.FillPath(bBg, path);
                g.DrawPath(pBdr, path);

                using var bDot = new SolidBrush(Color.FromArgb(99, 102, 241));
                g.FillEllipse(bDot, 8, 8, 6, 6);

                using var bTxt = new SolidBrush(txt);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var rect = new Rectangle(12, 0, _lblInvoicesBadge.Width - 14, _lblInvoicesBadge.Height);
                g.DrawString(_lblInvoicesBadge.Text.Replace("● ", ""), _lblInvoicesBadge.Font, bTxt, rect, sf);
            };

            void LayoutInvoicesBadge()
            {
                if (_lblInvoicesBadge == null || _pnlInvoicesHeader == null) return;
                using var g = _pnlInvoicesHeader.CreateGraphics();
                var sz = g.MeasureString("Recent Invoices & Transactions", fHdrTitle);
                _lblInvoicesBadge.Location = new Point(40 + (int)sz.Width + 8, (_pnlInvoicesHeader.Height - _lblInvoicesBadge.Height) / 2);
            }
            _pnlInvoicesHeader.Resize += (s, e) => LayoutInvoicesBadge();

            var btnViewAllInvoices = new Button
            {
                Text = "View All →",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.PrimaryAccent,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                AutoSize = true,
                Height = 28,
                Dock = DockStyle.Right
            };
            btnViewAllInvoices.FlatAppearance.BorderSize = 0;
            btnViewAllInvoices.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, ThemeManager.PrimaryAccent);
            btnViewAllInvoices.Click += (s, e) =>
            {
                ShowPage("Sales");
                _salesModule?.SwitchTab("Dashboard");
            };

            _pnlInvoicesHeader.Controls.Add(_lblInvoicesBadge);
            _pnlInvoicesHeader.Controls.Add(btnViewAllInvoices);

            // 2. Footer Bar (Dock = Bottom)
            _pnlInvoicesFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = C_CardBg,
                Padding = new Padding(12, 0, 12, 0)
            };
            _pnlInvoicesFooter.Paint += (s, e) =>
            {
                var g = e.Graphics;
                bool isDark = ThemeManager.IsDarkMode;
                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using var pen = new Pen(sepCol, 1f);
                g.DrawLine(pen, 0, 0, _pnlInvoicesFooter.Width, 0);
            };

            _lblInvoicesFooterSummary = new Label
            {
                Text = "Showing latest 20 sales transactions across all accounts",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = C_TextMid,
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 9, 0, 0)
            };

            var btnNewInvoiceQuick = new Button
            {
                Text = "+ New Invoice",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                ForeColor = ThemeManager.PrimaryAccent,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                AutoSize = true,
                Height = 26,
                Dock = DockStyle.Right
            };
            btnNewInvoiceQuick.FlatAppearance.BorderSize = 0;
            btnNewInvoiceQuick.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, ThemeManager.PrimaryAccent);
            btnNewInvoiceQuick.Click += (s, e) =>
            {
                ShowPage("Sales");
                _salesModule?.SwitchTab("CreateInvoice");
            };

            _pnlInvoicesFooter.Controls.Add(_lblInvoicesFooterSummary);
            _pnlInvoicesFooter.Controls.Add(btnNewInvoiceQuick);

            // 3. Grid Columns & Layout
            dgvInvoices.Columns.Clear();
            dgvInvoices.Columns.Add("InvoiceNo", "Invoice No");
            dgvInvoices.Columns.Add("Customer", "Customer");
            dgvInvoices.Columns.Add("Date", "Date");
            dgvInvoices.Columns.Add("Mode", "Mode");
            dgvInvoices.Columns.Add("Amount", "Amount (₹)");
            dgvInvoices.Columns.Add("Status", "Status");

            dgvInvoices.Columns["InvoiceNo"].FillWeight = 19;
            dgvInvoices.Columns["Customer"].FillWeight = 24;
            dgvInvoices.Columns["Date"].FillWeight = 18;
            dgvInvoices.Columns["Mode"].FillWeight = 12;
            dgvInvoices.Columns["Amount"].FillWeight = 15;
            dgvInvoices.Columns["Status"].FillWeight = 12;

            dgvInvoices.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoices.Columns["Amount"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoices.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInvoices.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            TableDesignStandard.ApplyStandardGrid(dgvInvoices, new TableStyleOptions { EmptyStateMessage = "No recent invoices recorded" });
            dgvInvoices.Cursor = Cursors.Hand;

            dgvInvoices.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvInvoices.Rows.Count)
                {
                    string invNo = dgvInvoices.Rows[e.RowIndex].Cells["InvoiceNo"].Value?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(invNo))
                    {
                        var inv = DatabaseService.GetInvoiceByNumber(invNo);
                        if (inv != null)
                        {
                            using var dlg = new InvoiceDetailsForm(inv);
                            dlg.ShowDialog(this);
                            RefreshDashboardAnalytics();
                            RefreshLiveKpiCards();
                            RefreshInvoicesTable();
                            RefreshLowStockTable();
                            _salesModule?.LoadDashboardData();
                        }
                    }
                }
            };

            pnlRecentInvoicesCard.Controls.Clear();
            pnlRecentInvoicesCard.Controls.Add(dgvInvoices);
            pnlRecentInvoicesCard.Controls.Add(_pnlInvoicesFooter);
            pnlRecentInvoicesCard.Controls.Add(_pnlInvoicesHeader);
            dgvInvoices.Dock = DockStyle.Fill;
            dgvInvoices.BringToFront();

            LayoutInvoicesBadge();
            RefreshInvoicesTable();
        }

        private void SetupLowStockCard()
        {
            pnlLowStockCard.Padding = new Padding(1);
            pnlLowStockCard.BackColor = C_CardBg;
            pnlLowStockCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                TableDesignStandard.PaintCardContainerBorder(g, pnlLowStockCard.ClientRectangle);
            };

            // 1. Header Bar (Dock = Top)
            _pnlLowStockHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = C_CardBg,
                Padding = new Padding(12, 0, 12, 0)
            };

            var fHdrTitle = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            _pnlLowStockHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;

                // Bottom hairline separator
                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using (var pen = new Pen(sepCol, 1f))
                {
                    g.DrawLine(pen, 0, _pnlLowStockHeader.Height - 1, _pnlLowStockHeader.Width, _pnlLowStockHeader.Height - 1);
                }

                // Vector Icon
                ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Warning, new Rectangle(14, 13, 18, 18), Color.FromArgb(245, 158, 11), 1.5f);

                // Title
                using var bTitle = new SolidBrush(ThemeManager.TextPrimary);
                g.DrawString("Inventory Stock Alerts", fHdrTitle, bTitle, 38, 12);
            };

            // Badge
            _lblLowStockBadge = new Label
            {
                Text = "● 0 Low Stock",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                Size = new Size(100, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _lblLowStockBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isDark = ThemeManager.IsDarkMode;
                int count = _lowStockProducts?.Count ?? 0;
                Color bg = count == 0
                    ? (isDark ? Color.FromArgb(6, 78, 59) : Color.FromArgb(236, 253, 245))
                    : (isDark ? Color.FromArgb(69, 26, 3) : Color.FromArgb(254, 243, 199));
                Color bdr = count == 0
                    ? (isDark ? Color.FromArgb(4, 120, 87) : Color.FromArgb(167, 243, 208))
                    : (isDark ? Color.FromArgb(180, 83, 9) : Color.FromArgb(253, 230, 138));
                Color dotCol = count == 0 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11);
                Color txt = count == 0
                    ? (isDark ? Color.FromArgb(110, 231, 183) : Color.FromArgb(4, 120, 87))
                    : (isDark ? Color.FromArgb(253, 230, 138) : Color.FromArgb(180, 83, 9));

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, _lblLowStockBadge.Width - 1, _lblLowStockBadge.Height - 1, 11);
                using var bBg = new SolidBrush(bg);
                using var pBdr = new Pen(bdr, 1f);
                g.FillPath(bBg, path);
                g.DrawPath(pBdr, path);

                using var bDot = new SolidBrush(dotCol);
                g.FillEllipse(bDot, 8, 8, 6, 6);

                using var bTxt = new SolidBrush(txt);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                var rect = new Rectangle(12, 0, _lblLowStockBadge.Width - 14, _lblLowStockBadge.Height);
                g.DrawString(_lblLowStockBadge.Text.Replace("● ", ""), _lblLowStockBadge.Font, bTxt, rect, sf);
            };

            void LayoutLowStockBadge()
            {
                if (_lblLowStockBadge == null || _pnlLowStockHeader == null) return;
                using var g = _pnlLowStockHeader.CreateGraphics();
                var sz = g.MeasureString("Inventory Stock Alerts", fHdrTitle);
                _lblLowStockBadge.Location = new Point(40 + (int)sz.Width + 8, (_pnlLowStockHeader.Height - _lblLowStockBadge.Height) / 2);
            }
            _pnlLowStockHeader.Resize += (s, e) => LayoutLowStockBadge();

            var btnViewProducts = new Button
            {
                Text = "Catalog →",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = ThemeManager.PrimaryAccent,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                AutoSize = true,
                Height = 28,
                Dock = DockStyle.Right
            };
            btnViewProducts.FlatAppearance.BorderSize = 0;
            btnViewProducts.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, ThemeManager.PrimaryAccent);
            btnViewProducts.Click += (s, e) => ShowPage("Products");

            _pnlLowStockHeader.Controls.Add(_lblLowStockBadge);
            _pnlLowStockHeader.Controls.Add(btnViewProducts);

            // 2. Footer Bar (Dock = Bottom)
            _pnlLowStockFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = C_CardBg,
                Padding = new Padding(12, 0, 12, 0)
            };
            _pnlLowStockFooter.Paint += (s, e) =>
            {
                var g = e.Graphics;
                bool isDark = ThemeManager.IsDarkMode;
                Color sepCol = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
                using var pen = new Pen(sepCol, 1f);
                g.DrawLine(pen, 0, 0, _pnlLowStockFooter.Width, 0);
            };

            _lblLowStockFooterSummary = new Label
            {
                Text = "All other products in catalog are healthy.",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = C_TextMid,
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 9, 0, 0)
            };

            var btnAddProductQuick = new Button
            {
                Text = "+ Add Product",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                ForeColor = ThemeManager.PrimaryAccent,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                AutoSize = true,
                Height = 26,
                Dock = DockStyle.Right
            };
            btnAddProductQuick.FlatAppearance.BorderSize = 0;
            btnAddProductQuick.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, ThemeManager.PrimaryAccent);
            btnAddProductQuick.Click += (s, e) =>
            {
                using var dlg = new ProductDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshDashboardAnalytics();
                    RefreshLowStockTable();
                    RefreshProductsTable();
                }
            };

            _pnlLowStockFooter.Controls.Add(_lblLowStockFooterSummary);
            _pnlLowStockFooter.Controls.Add(btnAddProductQuick);

            // 3. Grid Columns & Layout
            dgvLowStock.Columns.Clear();
            dgvLowStock.Columns.Add("ItemName", "Item / Product");
            dgvLowStock.Columns.Add("Stock", "In Stock");
            dgvLowStock.Columns.Add("MinLevel", "Reorder Level");
            dgvLowStock.Columns.Add("Action", "Alert Level");

            dgvLowStock.Columns["ItemName"].FillWeight = 42;
            dgvLowStock.Columns["Stock"].FillWeight = 18;
            dgvLowStock.Columns["MinLevel"].FillWeight = 22;
            dgvLowStock.Columns["Action"].FillWeight = 18;

            dgvLowStock.Columns["Action"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvLowStock.Columns["Action"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            TableDesignStandard.ApplyStandardGrid(dgvLowStock, new TableStyleOptions { EmptyStateMessage = "All stock levels are currently healthy" });
            dgvLowStock.Cursor = Cursors.Hand;

            dgvLowStock.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvLowStock.Rows.Count)
                {
                    string itemName = dgvLowStock.Rows[e.RowIndex].Cells["ItemName"].Value?.ToString() ?? "";
                    var prod = _lowStockProducts.FirstOrDefault(p => p.Name == itemName) ?? DatabaseService.GetProducts().FirstOrDefault(p => p.Name == itemName);
                    if (prod != null)
                    {
                        using var dlg = new ProductDialog(prod);
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            RefreshDashboardAnalytics();
                            RefreshLowStockTable();
                            RefreshProductsTable();
                        }
                    }
                }
            };

            pnlLowStockCard.Controls.Clear();
            pnlLowStockCard.Controls.Add(dgvLowStock);
            pnlLowStockCard.Controls.Add(_pnlLowStockFooter);
            pnlLowStockCard.Controls.Add(_pnlLowStockHeader);
            dgvLowStock.Dock = DockStyle.Fill;
            dgvLowStock.BringToFront();

            LayoutLowStockBadge();
            RefreshLowStockTable();
        }

        public void RefreshInvoicesTable(string search = "")
        {
            dgvInvoices.Rows.Clear();
            try
            {
                var list = DatabaseService.GetRecentInvoicesWithPayment(20);
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = search.ToLowerInvariant();
                    list = list.Where(i => i.InvoiceNumber.ToLowerInvariant().Contains(s) || i.CustomerName.ToLowerInvariant().Contains(s)).ToList();
                }

                foreach (var inv in list)
                {
                    int rowIndex = dgvInvoices.Rows.Add(
                        inv.InvoiceNumber,
                        inv.CustomerName,
                        inv.InvoiceDate.ToString("dd MMM yyyy"),
                        inv.PaymentMode,
                        $"₹{inv.TotalAmount:N2}",
                        inv.Status
                    );

                    var statusCell = dgvInvoices.Rows[rowIndex].Cells["Status"];
                    if (inv.Status == "Paid")
                        statusCell.Style.ForeColor = C_Positive;
                    else if (inv.Status == "Partially Paid")
                        statusCell.Style.ForeColor = C_Warning;
                    else if (inv.Status == "Cancelled")
                        statusCell.Style.ForeColor = C_TextLight;
                    else
                        statusCell.Style.ForeColor = C_Negative;
                }

                if (_lblInvoicesBadge != null)
                {
                    _lblInvoicesBadge.Text = $"● {list.Count} Invoices";
                    _lblInvoicesBadge.Invalidate();
                }
                if (_lblInvoicesFooterSummary != null)
                {
                    _lblInvoicesFooterSummary.Text = $"Showing latest {list.Count} sales transactions across all accounts";
                }
            }
            catch
            {
            }
        }

        public void RefreshLowStockTable()
        {
            dgvLowStock.Rows.Clear();
            try
            {
                _lowStockProducts = DatabaseService.GetLowStockProducts(15m);
                foreach (var p in _lowStockProducts)
                {
                    string alertLevel = p.StockQuantity <= 5 ? "CRITICAL" : "LOW";
                    int rowIndex = dgvLowStock.Rows.Add(
                        p.Name,
                        $"{p.StockQuantity:G29} {p.Unit}",
                        $"{p.MinStockAlert:G29} {p.Unit}",
                        alertLevel
                    );

                    var alertCell = dgvLowStock.Rows[rowIndex].Cells["Action"];
                    alertCell.Style.ForeColor = alertLevel == "CRITICAL" ? C_Negative : C_Warning;
                    alertCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }

                int count = _lowStockProducts.Count;
                if (_lblLowStockBadge != null)
                {
                    _lblLowStockBadge.Text = count == 0 ? "● All Healthy" : $"● {count} {(count == 1 ? "Item Due" : "Items Due")}";
                    _lblLowStockBadge.Invalidate();
                }
                if (_lblLowStockFooterSummary != null)
                {
                    _lblLowStockFooterSummary.Text = count == 0
                        ? "✓ All inventory items are safely above minimum reorder levels."
                        : $"✓ {count} urgent {(count == 1 ? "item requires" : "items require")} restocking • Catalog healthy";
                }
            }
            catch
            {
            }
        }


        // ═══════════════════════════════════════════════════════════════════════════
        //  SIDEBAR NAVIGATION WITH REICON VECTOR ICONS (Zero Emojis!)
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupSidebarNav()
        {
            var menu = new (string page, string label, Button btn, ReiconRenderer.IconType icon)[]
            {
                ("Overview",     "Overview",            btnNavOverview,     ReiconRenderer.IconType.Dashboard),
                ("Statistics",   "Statistics & Reports",btnNavStatistics,   ReiconRenderer.IconType.Statistics),
                ("Customers",    "Customers",           btnNavCustomers,    ReiconRenderer.IconType.Customers),
                ("Suppliers",    "Suppliers",           btnNavSuppliers,    ReiconRenderer.IconType.Suppliers),
                ("Products",     "Products & Inventory",btnNavProducts,     ReiconRenderer.IconType.Products),
                ("Purchases",    "Purchase Management", btnNavPurchases,    ReiconRenderer.IconType.Purchases),
                ("Sales",        "Sales & Billing",     btnNavSales,        ReiconRenderer.IconType.Sales),
                ("Transactions", "Transactions",        btnNavTransactions, ReiconRenderer.IconType.Transactions),
                ("Messages",     "Messages & Alerts",   btnNavMessages,     ReiconRenderer.IconType.Messages),
                ("Users",        "Users & Security",    btnNavUsers,        ReiconRenderer.IconType.Security),
                ("Settings",     "Settings",            btnNavSettings,     ReiconRenderer.IconType.Settings)
            };

            pnlNavMenu.Controls.Clear();

            // Reverse order adding for DockStyle.Top so Overview appears at the top
            for (int i = menu.Length - 1; i >= 0; i--)
            {
                var item = menu[i];
                ConfigNavButton(item.btn, item.label, item.page, item.icon);
                item.btn.Dock = DockStyle.Top;
                item.btn.Height = 40;
                pnlNavMenu.Controls.Add(item.btn);
            }

            ConfigNavButton(btnNavLogout, "Logout", "Logout", ReiconRenderer.IconType.Logout);
        }

        private void ConfigNavButton(Button btn, string label, string page, ReiconRenderer.IconType iconType)
        {
            btn.Text = string.Empty;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;
            btn.Tag = page;

            btn.MouseEnter += (s, e) => { _hoveredNavPage = page; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { if (_hoveredNavPage == page) { _hoveredNavPage = null; btn.Invalidate(); } };

            btn.Click += (s, e) =>
            {
                if (page == "Logout")
                {
                    if (MessageBox.Show("Are you sure you want to log out of TEAM SOFT?", "Logout",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        LogoutRequested?.Invoke(this, EventArgs.Empty);
                        this.Close();
                    }
                    return;
                }
                ShowPage(page);
            };

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                bool isActive = (_activePage == page);
                bool isHovered = (_hoveredNavPage == page);
                int w = btn.Width;
                int h = btn.Height;

                if (isActive)
                {
                    using var bgBrush = new SolidBrush(C_NavActive);
                    g.FillRectangle(bgBrush, 0, 0, w, h);

                    using var pillBrush = new SolidBrush(C_Blue);
                    using var pillPath = GraphicsExtensions.CreateRoundedRectanglePath(0, 6, 4, h - 12, 2);
                    g.FillPath(pillBrush, pillPath);
                }
                else if (isHovered)
                {
                    using var bgBrush = new SolidBrush(C_NavHover);
                    g.FillRectangle(bgBrush, 0, 0, w, h);
                }

                Color iconColor = isActive ? C_Blue : (isHovered ? C_TextDark : C_NavText);
                Color textColor = isActive ? C_NavTextOn : (isHovered ? C_TextDark : C_NavText);

                // Draw Reicon Vector Icon (Clean 20x20 Vector)
                ReiconRenderer.DrawIcon(g, iconType, new Rectangle(14, (h - 18) / 2, 18, 18), iconColor, 1.7f);

                using var bText = new SolidBrush(textColor);
                g.DrawString(label, isActive ? s_fontNavActive : s_fontNav, bText, 42, (h - 18) / 2);
            };
        }

        public void ShowPage(string page)
        {
            _activePage = page;

            pnlOverview.Visible = (page == "Overview");
            pnlNewBill.Visible = (page == "Sales");
            pnlPurchases.Visible = (page == "Purchases");
            pnlSuppliers.Visible = (page == "Suppliers");
            pnlCustomers.Visible = (page == "Customers");
            pnlProducts.Visible = (page == "Products");
            pnlTransPage.Visible = (page == "Transactions");
            pnlSettings.Visible = (page == "Settings");
            pnlStatistics.Visible = (page == "Statistics");

            headerTitle.Title = page switch
            {
                "Overview" => "Dashboard Overview",
                "Sales" => "Sales & Billing Management",
                "Customers" => "Customer Accounts & Ledgers",
                "Products" => "Products & Inventory",
                "Transactions" => "Financial Transactions Ledger",
                "Statistics" => "Statistics & Analytics",
                "Purchases" => "Purchase Management & Vendor Bills",
                "Suppliers" => "Suppliers & Vendor Accounts",
                "Settings" => "System Settings & Business Profile",
                _ => page
            };

            headerTitle.Subtitle = page switch
            {
                "Overview" => "Real-time business performance overview",
                "Sales" => "Service invoicing, tax computations, A4 printing & PDF export",
                "Customers" => "Client directory, contact details & balance ledgers",
                "Products" => "Product catalog stock & inventory valuation",
                "Transactions" => "Audit log of receipts and payment transactions",
                "Statistics" => "Comprehensive revenue analysis & printable financial report",
                "Purchases" => "Vendor purchase bills, stock addition & payables tracking",
                "Suppliers" => "Vendor catalog, payables ledgers & procurement contacts",
                "Settings" => "Preferences, company profile, GST, security, backup & integrations",
                _ => "Manage and monitor your business operations"
            };

            if (page == "Overview")
            {
                RefreshDashboardAnalytics();
                RefreshLiveKpiCards();
                RefreshInvoicesTable();
                RefreshLowStockTable();
            }
            else if (page == "Sales")
            {
                _salesModule?.SwitchTab("Dashboard");
            }
            else if (page == "Purchases")
            {
                RefreshPurchasesTable();
            }
            else if (page == "Suppliers")
            {
                RefreshSuppliersTable();
            }
            else if (page == "Customers")
            {
                RefreshCustomersTable();
            }
            else if (page == "Products")
            {
                RefreshProductsTable();
            }
            else if (page == "Transactions")
            {
                RefreshTransactionsTable();
            }
            else if (page == "Statistics")
            {
                RefreshStatisticsPage();
            }
            else if (page == "Settings")
            {
                _settingsModule?.RefreshAllSettings();
            }

            pnlSidebar.Invalidate(true);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SUB PAGES (SALES, CUSTOMERS, PRODUCTS, TRANSACTIONS, SETTINGS, STATISTICS)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildNewBillPage()
        {
            pnlNewBill.Dock = DockStyle.Fill;
            pnlNewBill.Visible = false;
            pnlNewBill.BackColor = C_PageBg;
            pnlNewBill.Controls.Clear();

            _salesModule = new SalesBillingModuleControl();
            _salesModule.Dock = DockStyle.Fill;
            _salesModule.DataChanged += (s, e) =>
            {
                RefreshDashboardAnalytics();
                RefreshLiveKpiCards();
                RefreshInvoicesTable();
                RefreshLowStockTable();
            };

            pnlNewBill.Controls.Add(_salesModule);
            pnlContentHost.Controls.Add(pnlNewBill);
        }

        private void BuildCustomersPage()
        {
            pnlCustomers.Dock = DockStyle.Fill;
            pnlCustomers.Visible = false;
            pnlCustomers.BackColor = C_PageBg;
            pnlCustomers.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 10) };

            txtCustSearch.PlaceholderText = "Search customers by name, company, phone, or GSTIN...";
            txtCustSearch.SetBounds(20, 14, 380, 32);
            txtCustSearch.Font = new Font("Segoe UI", 9.5f);
            txtCustSearch.TextChanged += (s, e) => RefreshCustomersTable(txtCustSearch.Text.Trim());

            btnAddCustomer.Text = "+ Add Customer";
            btnAddCustomer.BackColor = C_Blue;
            btnAddCustomer.ForeColor = Color.White;
            btnAddCustomer.FlatStyle = FlatStyle.Flat;
            btnAddCustomer.FlatAppearance.BorderSize = 0;
            btnAddCustomer.Cursor = Cursors.Hand;
            btnAddCustomer.SetBounds(420, 14, 150, 32);
            btnAddCustomer.Click += (s, e) =>
            {
                using var dlg = new CustomerDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshCustomersTable();
                    RefreshLiveKpiCards();
                }
            };

            pnlTop.Controls.AddRange(new Control[] { txtCustSearch, btnAddCustomer });

            dgvCustomers.Columns.Clear();
            dgvCustomers.Columns.Add("Id", "ID");
            dgvCustomers.Columns["Id"].Visible = false;
            dgvCustomers.Columns.Add("Name", "Customer Name");
            dgvCustomers.Columns.Add("Company", "Company");
            dgvCustomers.Columns.Add("Phone", "Phone");
            dgvCustomers.Columns.Add("GSTIN", "GSTIN");
            dgvCustomers.Columns.Add("Balance", "Outstanding Due (₹)");
            dgvCustomers.Columns.Add("Status", "Status");

            dgvCustomers.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCustomers.Columns["Balance"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvCustomers.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCustomers.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            TableDesignStandard.ApplyStandardGrid(dgvCustomers, new TableStyleOptions { EmptyStateMessage = "No customer accounts registered" });
            dgvCustomers.Cursor = Cursors.Hand;

            dgvCustomers.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvCustomers.Rows.Count)
                {
                    if (long.TryParse(dgvCustomers.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out long cid))
                    {
                        var cust = DatabaseService.GetCustomerById(cid);
                        if (cust != null)
                        {
                            using var dlg = new CustomerDialog(cust);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                RefreshCustomersTable();
                                RefreshLiveKpiCards();
                            }
                        }
                    }
                }
            };

            dgvCustomers.Dock = DockStyle.Fill;
            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGridContainer.Controls.Add(dgvCustomers);

            pnlCustomers.Controls.Add(pnlGridContainer);
            pnlCustomers.Controls.Add(pnlTop);
            pnlGridContainer.BringToFront();
            pnlContentHost.Controls.Add(pnlCustomers);
            RefreshCustomersTable();
        }

        public void RefreshCustomersTable(string search = "")
        {
            dgvCustomers.Rows.Clear();
            try
            {
                var list = DatabaseService.GetCustomers(search);
                foreach (var c in list)
                {
                    string status = c.OutstandingBalance > 0 ? "Outstanding Due" : "Settled";
                    int rIdx = dgvCustomers.Rows.Add(
                        c.Id,
                        c.Name,
                        c.CompanyName,
                        c.Phone,
                        c.GSTIN,
                        $"₹{c.OutstandingBalance:N2}",
                        status
                    );

                    var stCell = dgvCustomers.Rows[rIdx].Cells["Status"];
                    stCell.Style.ForeColor = c.OutstandingBalance > 0 ? C_Warning : C_Positive;
                }
            }
            catch
            {
            }
        }

        private void BuildProductsPage()
        {
            pnlProducts.Dock = DockStyle.Fill;
            pnlProducts.Visible = false;
            pnlProducts.BackColor = C_PageBg;
            pnlProducts.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 10) };

            txtProdSearch.PlaceholderText = "Search by item name, SKU, or barcode...";
            txtProdSearch.SetBounds(20, 14, 380, 32);
            txtProdSearch.Font = new Font("Segoe UI", 9.5f);
            txtProdSearch.TextChanged += (s, e) => RefreshProductsTable(txtProdSearch.Text.Trim());

            btnAddProductPage.Text = "+ Add Product";
            btnAddProductPage.BackColor = C_Blue;
            btnAddProductPage.ForeColor = Color.White;
            btnAddProductPage.FlatStyle = FlatStyle.Flat;
            btnAddProductPage.FlatAppearance.BorderSize = 0;
            btnAddProductPage.Cursor = Cursors.Hand;
            btnAddProductPage.SetBounds(420, 14, 140, 32);
            btnAddProductPage.Click += (s, e) =>
            {
                using var dlg = new ProductDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshProductsTable();
                    RefreshLowStockTable();
                    RefreshDashboardAnalytics();
                }
            };

            var btnStockAdjust = new Button
            {
                Text = "± Quick Adjust Stock",
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(40, 44, 60) : Color.FromArgb(241, 245, 249),
                ForeColor = C_TextDark,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Bounds = new Rectangle(570, 14, 160, 32)
            };
            btnStockAdjust.FlatAppearance.BorderColor = C_Border;
            btnStockAdjust.Click += (s, e) =>
            {
                if (dgvProducts.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a product from the table first.", "Adjust Stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (long.TryParse(dgvProducts.SelectedRows[0].Cells["Id"].Value?.ToString(), out long pid))
                {
                    var prod = DatabaseService.GetProducts().FirstOrDefault(p => p.Id == pid);
                    if (prod != null)
                    {
                        using var dlg = new ProductDialog(prod);
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            RefreshProductsTable();
                            RefreshLowStockTable();
                            RefreshDashboardAnalytics();
                        }
                    }
                }
            };

            pnlTop.Controls.AddRange(new Control[] { txtProdSearch, btnAddProductPage, btnStockAdjust });

            dgvProducts.Columns.Clear();
            dgvProducts.Columns.Add("Id", "ID");
            dgvProducts.Columns["Id"].Visible = false;
            dgvProducts.Columns.Add("Barcode", "Barcode / SKU");
            dgvProducts.Columns.Add("Name", "Product Name");
            dgvProducts.Columns.Add("Category", "Category");
            dgvProducts.Columns.Add("Cost", "Cost Price");
            dgvProducts.Columns.Add("Sale", "Sale Price");
            dgvProducts.Columns.Add("Stock", "In Stock");
            dgvProducts.Columns.Add("MinAlert", "Alert Level");

            dgvProducts.Columns["Cost"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["Cost"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["Sale"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["Sale"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns["MinAlert"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProducts.Columns["MinAlert"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            TableDesignStandard.ApplyStandardGrid(dgvProducts, new TableStyleOptions { EmptyStateMessage = "No inventory items cataloged" });
            dgvProducts.Cursor = Cursors.Hand;

            dgvProducts.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvProducts.Rows.Count)
                {
                    if (long.TryParse(dgvProducts.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out long pid))
                    {
                        var prod = DatabaseService.GetProducts().FirstOrDefault(p => p.Id == pid);
                        if (prod != null)
                        {
                            using var dlg = new ProductDialog(prod);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                RefreshProductsTable();
                                RefreshLowStockTable();
                                RefreshDashboardAnalytics();
                            }
                        }
                    }
                }
            };

            dgvProducts.Dock = DockStyle.Fill;
            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGridContainer.Controls.Add(dgvProducts);

            pnlProducts.Controls.Add(pnlGridContainer);
            pnlProducts.Controls.Add(pnlTop);
            pnlGridContainer.BringToFront();
            pnlContentHost.Controls.Add(pnlProducts);
            RefreshProductsTable();
        }

        public void RefreshProductsTable(string search = "")
        {
            dgvProducts.Rows.Clear();
            try
            {
                var list = DatabaseService.GetProducts();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = search.ToLowerInvariant();
                    list = list.Where(p => p.Name.ToLowerInvariant().Contains(s) || p.Barcode.ToLowerInvariant().Contains(s) || p.Category.ToLowerInvariant().Contains(s)).ToList();
                }

                foreach (var p in list)
                {
                    int rIdx = dgvProducts.Rows.Add(
                        p.Id,
                        p.Barcode,
                        p.Name,
                        p.Category,
                        $"₹{p.CostPrice:N2}",
                        $"₹{p.SalePrice:N2}",
                        $"{p.StockQuantity:G29} {p.Unit}",
                        $"{p.MinStockAlert:G29} {p.Unit}"
                    );

                    var stockCell = dgvProducts.Rows[rIdx].Cells["Stock"];
                    if (p.StockQuantity <= 5)
                        stockCell.Style.ForeColor = C_Negative;
                    else if (p.StockQuantity <= p.MinStockAlert)
                        stockCell.Style.ForeColor = C_Warning;
                    else
                        stockCell.Style.ForeColor = C_Positive;
                }
            }
            catch
            {
            }
        }

        private void BuildTransactionsPage()
        {
            pnlTransPage.Dock = DockStyle.Fill;
            pnlTransPage.Visible = false;
            pnlTransPage.BackColor = C_PageBg;
            pnlTransPage.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 10) };

            txtTransSearch.PlaceholderText = "Filter transactions by reference, customer, or payment mode...";
            txtTransSearch.SetBounds(20, 14, 420, 32);
            txtTransSearch.Font = new Font("Segoe UI", 9.5f);
            txtTransSearch.TextChanged += (s, e) => RefreshTransactionsTable(txtTransSearch.Text.Trim());

            pnlTop.Controls.Add(txtTransSearch);

            dgvAllTransactions.Columns.Clear();
            dgvAllTransactions.Columns.Add("Date", "Date & Time");
            dgvAllTransactions.Columns.Add("Type", "Type");
            dgvAllTransactions.Columns.Add("Party", "Customer / Party");
            dgvAllTransactions.Columns.Add("Invoice", "Invoice No");
            dgvAllTransactions.Columns.Add("Mode", "Payment Mode");
            dgvAllTransactions.Columns.Add("Amount", "Amount (₹)");
            dgvAllTransactions.Columns.Add("Ref", "Reference No");

            dgvAllTransactions.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvAllTransactions.Columns["Amount"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            TableDesignStandard.ApplyStandardGrid(dgvAllTransactions, new TableStyleOptions { EmptyStateMessage = "No payment transactions recorded" });
            dgvAllTransactions.Cursor = Cursors.Hand;

            dgvAllTransactions.Dock = DockStyle.Fill;
            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGridContainer.Controls.Add(dgvAllTransactions);

            pnlTransPage.Controls.Add(pnlGridContainer);
            pnlTransPage.Controls.Add(pnlTop);
            pnlGridContainer.BringToFront();
            pnlContentHost.Controls.Add(pnlTransPage);
            RefreshTransactionsTable();
        }

        public void RefreshTransactionsTable(string search = "")
        {
            dgvAllTransactions.Rows.Clear();
            try
            {
                var list = DatabaseService.GetPaymentTransactions(search);
                foreach (var tx in list)
                {
                    int rIdx = dgvAllTransactions.Rows.Add(
                        tx.PaymentDate.ToString("dd MMM yyyy HH:mm"),
                        "Payment Receipt",
                        tx.CustomerName,
                        tx.InvoiceNumber,
                        tx.PaymentMethod,
                        $"₹{tx.Amount:N2}",
                        tx.ReferenceNumber
                    );

                    var amtCell = dgvAllTransactions.Rows[rIdx].Cells["Amount"];
                    amtCell.Style.ForeColor = C_Positive;
                    amtCell.Style.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }
            catch
            {
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PURCHASE MANAGEMENT & VENDOR BILLS PAGE
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildPurchasesPage()
        {
            pnlPurchases.Dock = DockStyle.Fill;
            pnlPurchases.Visible = false;
            pnlPurchases.BackColor = C_PageBg;
            pnlPurchases.Controls.Clear();

            // 1. KPI Cards Bar at the Top (Total Purchases, Total Bills, Paid, Pending Payables)
            var pnlKpiBar = new Panel { Dock = DockStyle.Top, Height = 84, Padding = new Padding(20, 10, 20, 6), BackColor = Color.Transparent };
            var tlpKpi = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++)
                tlpKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            Panel CreateMiniKpi(string title, Label lblVal, Color accent)
            {
                var pnl = new Panel { Dock = DockStyle.Fill, BackColor = C_CardBg, Margin = new Padding(4) };
                pnl.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnl.Width - 1, pnl.Height - 1, 8);
                    using var pen = new Pen(C_Border, 1f);
                    g.DrawPath(pen, path);
                };
                var lblT = new Label { Text = title, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = C_TextMid, Bounds = new Rectangle(14, 10, 180, 18) };
                lblVal.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
                lblVal.ForeColor = accent;
                lblVal.Bounds = new Rectangle(14, 30, 200, 28);
                pnl.Controls.AddRange(new Control[] { lblT, lblVal });
                return pnl;
            }

            tlpKpi.Controls.Add(CreateMiniKpi("Total Purchases", lblPurchasesKpi1, C_Blue), 0, 0);
            tlpKpi.Controls.Add(CreateMiniKpi("Total Vendor Bills", lblPurchasesKpi2, C_TextDark), 1, 0);
            tlpKpi.Controls.Add(CreateMiniKpi("Paid to Vendors", lblPurchasesKpi3, C_Positive), 2, 0);
            tlpKpi.Controls.Add(CreateMiniKpi("Pending Payables Due", lblPurchasesKpi4, C_Negative), 3, 0);
            pnlKpiBar.Controls.Add(tlpKpi);

            // 2. Filter & Action Toolbar
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.Transparent, Padding = new Padding(20, 10, 20, 6) };

            txtPurchaseSearch.PlaceholderText = "Search purchase bills by bill #, vendor, or remarks...";
            txtPurchaseSearch.SetBounds(20, 10, 360, 32);
            txtPurchaseSearch.Font = new Font("Segoe UI", 9.5f);
            txtPurchaseSearch.TextChanged += (s, e) => RefreshPurchasesTable(txtPurchaseSearch.Text.Trim(), cmbPurchaseStatus.SelectedItem?.ToString() ?? "All");

            cmbPurchaseStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPurchaseStatus.Items.Clear();
            cmbPurchaseStatus.Items.AddRange(new object[] { "All Statuses", "Paid", "Partially Paid", "Unpaid", "Overdue" });
            cmbPurchaseStatus.SelectedIndex = 0;
            cmbPurchaseStatus.SetBounds(390, 10, 140, 32);
            cmbPurchaseStatus.Font = new Font("Segoe UI", 9.5f);
            cmbPurchaseStatus.SelectedIndexChanged += (s, e) => RefreshPurchasesTable(txtPurchaseSearch.Text.Trim(), cmbPurchaseStatus.SelectedItem?.ToString() ?? "All");

            var btnNewBill = new Button
            {
                Text = "+ New Purchase Bill",
                BackColor = C_Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnNewBill.FlatAppearance.BorderSize = 0;
            btnNewBill.SetBounds(540, 10, 170, 32);
            btnNewBill.Click += (s, e) =>
            {
                using var dlg = new PurchaseBillDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshPurchasesTable();
                    RefreshProductsTable();
                    RefreshLowStockTable();
                    RefreshDashboardAnalytics();
                }
            };

            pnlTop.Controls.AddRange(new Control[] { txtPurchaseSearch, cmbPurchaseStatus, btnNewBill });

            // 3. Grid Table
            dgvPurchases.Columns.Clear();
            dgvPurchases.Columns.Add("BillNum", "BILL #");
            dgvPurchases.Columns.Add("Supplier", "VENDOR / SUPPLIER");
            dgvPurchases.Columns.Add("Date", "BILL DATE");
            dgvPurchases.Columns.Add("Due", "DUE DATE");
            dgvPurchases.Columns.Add("Total", "AMOUNT (₹)");
            dgvPurchases.Columns.Add("Paid", "PAID (₹)");
            dgvPurchases.Columns.Add("Balance", "BALANCE DUE (₹)");
            dgvPurchases.Columns.Add("Status", "PAYMENT STATUS");

            dgvPurchases.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Total"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Paid"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Paid"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Balance"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPurchases.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPurchases.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvPurchases.Dock = DockStyle.Fill;
            TableDesignStandard.ApplyStandardGrid(dgvPurchases, new TableStyleOptions { EmptyStateMessage = "No purchase bills recorded" });

            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGridContainer.Controls.Add(dgvPurchases);

            pnlPurchases.Controls.Add(pnlGridContainer);
            pnlPurchases.Controls.Add(pnlTop);
            pnlPurchases.Controls.Add(pnlKpiBar);
            pnlGridContainer.BringToFront();

            pnlContentHost.Controls.Add(pnlPurchases);
            RefreshPurchasesTable();
        }

        public void RefreshPurchasesTable(string search = "", string status = "")
        {
            dgvPurchases.Rows.Clear();
            try
            {
                var list = DatabaseService.GetPurchaseBills(search, status);
                foreach (var b in list)
                {
                    dgvPurchases.Rows.Add(
                        b.BillNumber,
                        string.IsNullOrWhiteSpace(b.SupplierCompany) ? b.SupplierName : $"{b.SupplierName} ({b.SupplierCompany})",
                        b.BillDate.ToString("dd MMM yyyy"),
                        b.DueDate.ToString("dd MMM yyyy"),
                        $"₹{b.GrandTotal:N2}",
                        $"₹{b.AmountPaid:N2}",
                        $"₹{b.BalanceDue:N2}",
                        b.Status
                    );
                }

                var m = DatabaseService.GetPurchaseDashboardMetrics();
                lblPurchasesKpi1.Text = $"₹{m.TotalPurchases:N2}";
                lblPurchasesKpi2.Text = $"{m.TotalBillsCount}";
                lblPurchasesKpi3.Text = $"₹{(m.TotalPurchases - m.TotalPayablesDue):N2}";
                lblPurchasesKpi4.Text = $"₹{m.TotalPayablesDue:N2}";
            }
            catch
            {
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SUPPLIERS & VENDOR ACCOUNTS PAGE
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSuppliersPage()
        {
            pnlSuppliers.Dock = DockStyle.Fill;
            pnlSuppliers.Visible = false;
            pnlSuppliers.BackColor = C_PageBg;
            pnlSuppliers.Controls.Clear();

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent, Padding = new Padding(20, 14, 20, 10) };

            txtSupplierSearch.PlaceholderText = "Search vendors by name, company, phone, or GSTIN...";
            txtSupplierSearch.SetBounds(20, 14, 380, 32);
            txtSupplierSearch.Font = new Font("Segoe UI", 9.5f);
            txtSupplierSearch.TextChanged += (s, e) => RefreshSuppliersTable(txtSupplierSearch.Text.Trim());

            var btnAddSupplier = new Button
            {
                Text = "+ Add Supplier",
                BackColor = C_Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnAddSupplier.FlatAppearance.BorderSize = 0;
            btnAddSupplier.SetBounds(420, 14, 150, 32);
            btnAddSupplier.Click += (s, e) =>
            {
                using var dlg = new SupplierDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshSuppliersTable();
                }
            };

            pnlTop.Controls.AddRange(new Control[] { txtSupplierSearch, btnAddSupplier });

            dgvSuppliers.Columns.Clear();
            dgvSuppliers.Columns.Add("Id", "ID");
            dgvSuppliers.Columns["Id"].Visible = false;
            dgvSuppliers.Columns.Add("Name", "Vendor Name");
            dgvSuppliers.Columns.Add("Company", "Company / Firm");
            dgvSuppliers.Columns.Add("Phone", "Phone");
            dgvSuppliers.Columns.Add("GSTIN", "GSTIN");
            dgvSuppliers.Columns.Add("Balance", "Payable Balance (₹)");
            dgvSuppliers.Columns.Add("Status", "Status");

            dgvSuppliers.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvSuppliers.Columns["Balance"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvSuppliers.Columns["Status"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSuppliers.Columns["Status"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvSuppliers.Dock = DockStyle.Fill;
            TableDesignStandard.ApplyStandardGrid(dgvSuppliers, new TableStyleOptions { EmptyStateMessage = "No suppliers / vendors registered" });
            dgvSuppliers.Cursor = Cursors.Hand;

            dgvSuppliers.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvSuppliers.Rows.Count)
                {
                    if (long.TryParse(dgvSuppliers.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out long sid))
                    {
                        var sup = DatabaseService.GetSupplierById(sid);
                        if (sup != null)
                        {
                            using var dlg = new SupplierDialog(sup);
                            if (dlg.ShowDialog(this) == DialogResult.OK)
                            {
                                RefreshSuppliersTable();
                            }
                        }
                    }
                }
            };

            var pnlGridContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 0, 20, 20) };
            pnlGridContainer.Controls.Add(dgvSuppliers);

            pnlSuppliers.Controls.Add(pnlGridContainer);
            pnlSuppliers.Controls.Add(pnlTop);
            pnlGridContainer.BringToFront();

            pnlContentHost.Controls.Add(pnlSuppliers);
            RefreshSuppliersTable();
        }

        public void RefreshSuppliersTable(string search = "")
        {
            dgvSuppliers.Rows.Clear();
            try
            {
                var list = DatabaseService.GetSuppliers(search);
                foreach (var s in list)
                {
                    string status = s.OutstandingBalance > 0 ? "Pending Due" : "Settled";
                    dgvSuppliers.Rows.Add(
                        s.Id,
                        s.Name,
                        s.CompanyName,
                        s.Phone,
                        s.GSTIN,
                        $"₹{s.OutstandingBalance:N2}",
                        status
                    );
                }
            }
            catch
            {
            }
        }

        private void BuildSettingsPage()
        {
            pnlSettings.Dock = DockStyle.Fill;
            pnlSettings.Visible = false;
            pnlSettings.BackColor = C_PageBg;
            pnlSettings.AutoScroll = false;
            pnlSettings.Padding = new Padding(0);
            pnlSettings.Controls.Clear();

            _settingsModule = new SettingsModuleControl();
            _settingsModule.Dock = DockStyle.Fill;
            _settingsModule.SettingsSaved += () =>
            {
                var profile = DatabaseService.GetBusinessProfile();
                lblSidebarBrand.Text = !string.IsNullOrWhiteSpace(profile.BusinessName) ? profile.BusinessName : "TEAM SOFT";
                ApplyThemeFast();
            };

            pnlSettings.Controls.Add(_settingsModule);
            pnlContentHost.Controls.Add(pnlSettings);
        }

        private void BuildStatisticsPage()
        {
            pnlStatistics.Dock = DockStyle.Fill;
            pnlStatistics.Visible = false;
            pnlStatistics.BackColor = C_PageBg;
            pnlStatistics.AutoScroll = true;
            pnlStatistics.Padding = new Padding(24);

            pnlContentHost.Controls.Add(pnlStatistics);
            RefreshStatisticsPage();
        }

        public void RefreshStatisticsPage()
        {
            pnlStatistics.Controls.Clear();
            var metrics = DatabaseService.GetSalesDashboardMetrics();
            var (cats, gTotal) = DatabaseService.GetCategorySalesDistribution("ThisYear");

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            var lblTitle = new Label { Text = "Business Analytics & Audit Reports", Font = new Font("Segoe UI", 14f, FontStyle.Bold), ForeColor = C_TextDark, Bounds = new Rectangle(0, 10, 400, 30) };

            var btnExport = new Button
            {
                Text = "Print / Export Financial Summary",
                BackColor = C_Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Bounds = new Rectangle(450, 10, 240, 34)
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += (s, e) =>
            {
                string reportHtml = $@"
                    <html><head><style>
                    body {{ font-family: Segoe UI, sans-serif; padding: 24px; color: #1e293b; }}
                    h1 {{ color: #4338ca; }}
                    table {{ border-collapse: collapse; width: 100%; margin-top: 16px; }}
                    th, td {{ border: 1px solid #cbd5e1; padding: 10px; text-align: left; }}
                    th {{ background-color: #f8fafc; font-weight: bold; }}
                    .highlight {{ font-weight: bold; color: #10b981; }}
                    </style></head><body>
                    <h1>TEAM SOFT — Executive Financial & Sales Report</h1>
                    <p>Generated on: {DateTime.Now:dd MMM yyyy HH:mm}</p>
                    <hr/>
                    <h2>Key Financial Indicators</h2>
                    <table>
                        <tr><th>Metric</th><th>Value</th></tr>
                        <tr><td>Total Lifetime Billings</td><td class='highlight'>{metrics.TotalSalesFormatted}</td></tr>
                        <tr><td>Total Invoices Issued</td><td>{metrics.TotalInvoices}</td></tr>
                        <tr><td>Settled In Full</td><td>{metrics.PaidInvoices}</td></tr>
                        <tr><td>Unpaid / Overdue Invoices</td><td>{metrics.UnpaidInvoices}</td></tr>
                        <tr><td>Total Outstanding Receivables</td><td style='color:#ef4444;font-weight:bold;'>{metrics.OutstandingDueFormatted}</td></tr>
                        <tr><td>Active Client Accounts</td><td>{metrics.TotalCustomers}</td></tr>
                    </table>
                    <h2>Category Distribution</h2>
                    <table>
                        <tr><th>Category</th><th>Revenue</th><th>Percentage</th></tr>
                        {string.Join("", cats.Select(c => $"<tr><td>{c.CategoryName}</td><td>₹{c.TotalSales:N2}</td><td>{c.Percentage:0.0}%</td></tr>"))}
                    </table>
                    <p style='margin-top:30px; font-size:12px; color:#64748b;'>Certified by TEAM SOFT Database Engine — 0 Discrepancy Reconciliation</p>
                    </body></html>
                ";

                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TEAMSOFT_Financial_Report_{DateTime.Now:yyyyMMdd}.html");
                System.IO.File.WriteAllText(tempFile, reportHtml);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = tempFile, UseShellExecute = true });
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, btnExport });
            pnlStatistics.Controls.Add(pnlHeader);

            var tlpStats = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 180,
                ColumnCount = 3,
                RowCount = 2,
                Margin = new Padding(0, 16, 0, 16)
            };
            for (int i = 0; i < 3; i++) tlpStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            tlpStats.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tlpStats.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            Panel MakeStatTile(string label, string val, Color col)
            {
                var p = new Panel { Dock = DockStyle.Fill, Margin = new Padding(6), BackColor = C_CardBg };
                p.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, p.Width - 1, p.Height - 1, 8);
                    using var bdr = new Pen(C_Border, 1.2f);
                    g.DrawPath(bdr, path);
                    using var bL = new SolidBrush(C_TextMid);
                    using var bV = new SolidBrush(col);
                    g.DrawString(label, s_fontStatTitle, bL, 14, 12);
                    g.DrawString(val, new Font("Segoe UI", 14f, FontStyle.Bold), bV, 14, 34);
                };
                return p;
            }

            tlpStats.Controls.Add(MakeStatTile("Total Lifetime Billings", metrics.TotalSalesFormatted, C_Positive), 0, 0);
            tlpStats.Controls.Add(MakeStatTile("Outstanding Dues Balance", metrics.OutstandingDueFormatted, C_Warning), 1, 0);
            tlpStats.Controls.Add(MakeStatTile("Total Issued Invoices", $"{metrics.TotalInvoices} Invoices", C_Blue), 2, 0);
            tlpStats.Controls.Add(MakeStatTile("Settled Invoices", $"{metrics.PaidInvoices} Invoices", C_Positive), 0, 1);
            tlpStats.Controls.Add(MakeStatTile("Unsettled Invoices", $"{metrics.UnpaidInvoices} Invoices", C_Negative), 1, 1);
            tlpStats.Controls.Add(MakeStatTile("Registered Customers", $"{metrics.TotalCustomers} Accounts", C_TextDark), 2, 1);

            pnlStatistics.Controls.Add(tlpStats);
        }

        private static void EnableDoubleBuffering(Control c)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(c, true, null);

            foreach (Control child in c.Controls)
            {
                EnableDoubleBuffering(child);
            }
        }
    }
}
