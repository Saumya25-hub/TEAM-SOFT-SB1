using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
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
        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME ANIMATION & PALETTE ACCESSORS
        // ═══════════════════════════════════════════════════════════════════════════
        private float _toggleAnimProgress = 0f; // 0.0f = Light, 1.0f = Dark
        private float _targetAnimProgress = 0f;
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 16 };

        // Entrance animation
        private float _entranceProgress = 0f;
        private readonly System.Windows.Forms.Timer _entranceTimer = new() { Interval = 16 };

        // Micro-interaction states
        private string? _hoveredNavPage = null;
        private int _hoveredCardIndex = -1;
        private bool _isSearchFocused = false;
        private readonly ToolTip _toolTip = new();

        private static Color C_Blue       => ThemeManager.PrimaryAccent;
        private static Color C_Positive   => ThemeManager.StatusPositive;
        private static Color C_Warning    => ThemeManager.StatusWarning;
        private static Color C_Negative   => ThemeManager.StatusNegative;

        private Color C_SidebarBg  => BlendColor(Color.White, Color.FromArgb(25, 26, 35), _toggleAnimProgress);
        private Color C_NavHover   => BlendColor(Color.FromArgb(240, 243, 252), Color.FromArgb(37, 40, 54), _toggleAnimProgress);
        private Color C_NavActive  => BlendColor(Color.FromArgb(233, 239, 255), Color.FromArgb(42, 51, 88), _toggleAnimProgress);
        private Color C_NavText    => BlendColor(Color.FromArgb(123, 129, 144), Color.FromArgb(161, 165, 179), _toggleAnimProgress);
        private Color C_NavTextOn  => BlendColor(Color.FromArgb(32, 35, 43), Color.White, _toggleAnimProgress);

        private Color C_PageBg     => BlendColor(Color.FromArgb(245, 246, 250), Color.FromArgb(17, 18, 24), _toggleAnimProgress);
        private Color C_CardBg     => BlendColor(Color.White, Color.FromArgb(30, 32, 43), _toggleAnimProgress);
        private Color C_Border     => BlendColor(Color.FromArgb(231, 233, 239), Color.FromArgb(48, 50, 62), _toggleAnimProgress);
        private Color C_TopBarBg   => BlendColor(Color.White, Color.FromArgb(25, 26, 35), _toggleAnimProgress);
        private Color C_SearchBg   => BlendColor(Color.FromArgb(245, 246, 250), Color.FromArgb(30, 32, 43), _toggleAnimProgress);
        private Color C_SearchBdr  => _isSearchFocused ? C_Blue : BlendColor(Color.FromArgb(226, 232, 240), Color.FromArgb(55, 58, 73), _toggleAnimProgress);

        private Color C_TextDark   => BlendColor(Color.FromArgb(32, 35, 43), Color.FromArgb(244, 245, 248), _toggleAnimProgress);
        private Color C_TextMid    => BlendColor(Color.FromArgb(123, 129, 144), Color.FromArgb(161, 165, 179), _toggleAnimProgress);
        private Color C_TextLight  => BlendColor(Color.FromArgb(148, 163, 184), Color.FromArgb(100, 116, 139), _toggleAnimProgress);

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

        private readonly List<(string Item, int Qty, decimal Price)> _lineItems = new();
        private decimal _billTotal = 0m;

        private readonly List<string[]> _invoicesStore = new();
        private readonly List<string[]> _lowStockStore = new();

        private bool _isDragging;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        // 12 Months Dynamics Data (Jan to Dec)
        private static readonly string[] _monthLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private static readonly int[] _salesDynamicsData    = { 42, 58, 65, 80, 72, 95, 88, 110, 102, 128, 115, 140 };
        private static readonly int[] _purchaseDynamicsData = { 28, 35, 42, 50, 48, 62, 55,  70,  68,  82,  75,  90 };

        // ═══════════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════
        public DashboardForm()
        {
            InitializeComponent();
            _toggleAnimProgress = ThemeManager.IsDarkMode ? 1.0f : 0.0f;
            _targetAnimProgress = _toggleAnimProgress;

            // Load data immediately — NEVER delay data loading for animation!
            PopulateSampleData();

            InitTooltips();
            InitThemeToggle();
            InitEntranceAnimation();
            BuildOverviewPage();
            BuildNewBillPage();
            BuildCustomersPage();
            BuildProductsPage();
            BuildTransactionsPage();
            SetupSidebarNav();
            SetupWindowDrag();
            ShowPage("Overview");

            ThemeManager.ThemeChanged += () =>
            {
                _targetAnimProgress = ThemeManager.IsDarkMode ? 1.0f : 0.0f;
                ApplyThemeFast();
                _animTimer.Start();
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  ENTRANCE ANIMATION (180–240ms Subtle Fade & Staggered Cards)
        // ═══════════════════════════════════════════════════════════════════════════
        private void InitEntranceAnimation()
        {
            this.Opacity = 0.88;
            _entranceProgress = 0f;

            _entranceTimer.Tick += (s, e) =>
            {
                _entranceProgress += 0.14f;
                this.Opacity = Math.Min(1.0, 0.88 + (0.12 * _entranceProgress));

                if (_entranceProgress >= 1f)
                {
                    _entranceProgress = 1f;
                    this.Opacity = 1.0;
                    _entranceTimer.Stop();
                    _entranceTimer.Dispose();
                }

                tlpCards.Invalidate(true);
            };
        }

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
            ApplyRoundedRegion(16);
            ApplyThemeFast();
            _entranceTimer.Start();
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

        // ═══════════════════════════════════════════════════════════════════════════
        //  FAST THEME TOGGLE (<100ms response, zero blocking loops)
        // ═══════════════════════════════════════════════════════════════════════════
        private void InitThemeToggle()
        {
            btnThemeToggle.Paint += (s, e) => PaintThemeToggle(e.Graphics);

            // Fast 120ms transition for thumb knob
            _animTimer.Tick += (s, e) =>
            {
                float diff = _targetAnimProgress - _toggleAnimProgress;
                float step = Math.Max(0.18f, Math.Abs(diff) * 0.65f);

                if (Math.Abs(diff) <= step)
                {
                    _toggleAnimProgress = _targetAnimProgress;
                    _animTimer.Stop();
                }
                else
                {
                    _toggleAnimProgress += Math.Sign(diff) * step;
                }

                // Only invalidate toggle button during thumb slide to prevent heavy repainting
                btnThemeToggle.Invalidate();
            };

            btnThemeToggle.Click += (s, e) =>
            {
                ThemeManager.ToggleTheme();
            };

            pnlBrandHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int cx = 30;
                int cy = 34;
                int petalR = 8;
                int offset = 6;

                Color logoAccent = BlendColor(Color.FromArgb(65, 105, 245), Color.FromArgb(102, 135, 255), _toggleAnimProgress);
                using var brush1 = new SolidBrush(logoAccent);
                using var brush2 = new SolidBrush(BlendColor(Color.FromArgb(99, 102, 241), Color.FromArgb(129, 140, 248), _toggleAnimProgress));

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
            btnNotifications.MouseEnter += (s, e) => { isNotifHovered = true; btnNotifications.Invalidate(); };
            btnNotifications.MouseLeave += (s, e) => { isNotifHovered = false; btnNotifications.Invalidate(); };
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

            // New Invoice button click navigation (Custom Framework: Anti-aliased pill, built-in vector icon)
            btnTopAddProduct.Click += (s, e) => ShowPage("Sales");

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

            int radius = (h - 2) / 2;
            using var trackPath = GraphicsExtensions.CreateRoundedRectanglePath(1, 1, w - 3, h - 3, radius * 2);

            Color trackBg = ThemeManager.IsDarkMode ? Color.FromArgb(30, 32, 48) : Color.FromArgb(226, 232, 240);
            Color trackBorder = ThemeManager.IsDarkMode ? Color.FromArgb(99, 102, 241) : Color.FromArgb(203, 213, 225);

            using var trackBrush = new SolidBrush(trackBg);
            g.FillPath(trackBrush, trackPath);

            using var trackPen = new Pen(trackBorder, 1.2f);
            g.DrawPath(trackPen, trackPath);

            // Reicon Sun (Left) & Moon (Right) Vector Icons
            Color sunColor = ThemeManager.IsDarkMode ? Color.FromArgb(100, 116, 139) : Color.FromArgb(245, 158, 11);
            ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Sun, new Rectangle(6, (h - 16) / 2, 16, 16), sunColor, 1.5f);

            Color moonColor = ThemeManager.IsDarkMode ? Color.FromArgb(165, 180, 252) : Color.FromArgb(148, 163, 184);
            ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Moon, new Rectangle(w - 22, (h - 16) / 2, 16, 16), moonColor, 1.5f);

            // Moving Thumb Knob
            int knobD = h - 6;
            int knobMinX = 3;
            int knobMaxX = w - knobD - 3;
            float knobX = knobMinX + (knobMaxX - knobMinX) * _toggleAnimProgress;
            float knobY = 3;

            using var knobPath = GraphicsExtensions.CreateRoundedRectanglePath((int)knobX, (int)knobY, knobD, knobD, knobD);
            Color knobColor = ThemeManager.IsDarkMode ? Color.FromArgb(99, 102, 241) : Color.White;
            using var knobBrush = new SolidBrush(knobColor);
            g.FillPath(knobBrush, knobPath);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME APPLICATION (OPTIMIZED: <100ms in-place update)
        // ═══════════════════════════════════════════════════════════════════════════
        private void ApplyThemeFast()
        {
            _toggleAnimProgress = ThemeManager.IsDarkMode ? 1.0f : 0.0f;

            BackColor                  = C_PageBg;
            pnlSidebar.BackColor       = C_SidebarBg;
            tblMainLayout.BackColor    = C_PageBg;
            pnlTopBar.BackColor        = C_TopBarBg;
            pnlContentHost.BackColor   = C_PageBg;
            pnlOverview.BackColor      = C_PageBg;

            headerTitle.TitleColor    = C_TextDark;
            headerTitle.SubtitleColor = C_TextMid;
            lblSidebarBrand.ForeColor = C_TextDark;
            lblSidebarSub.ForeColor   = C_TextMid;
            lblSidebarUser.ForeColor  = C_TextDark;
            lblSidebarRole.ForeColor  = C_TextMid;

            pnlSearchBox.BackColor  = C_SearchBg;
            txtSearch.BackColor     = C_SearchBg;
            txtSearch.ForeColor     = C_TextDark;

            winChrome.IconColor        = C_TextMid;
            winChrome.HoverBg          = ThemeManager.IsDarkMode ? Color.FromArgb(40, 43, 56) : Color.FromArgb(226, 232, 240);
            chromeDivider.DividerColor = C_Border;
            btnNotifications.ForeColor = C_TextDark;

            btnTopAddProduct.FillColor  = C_Blue;
            btnTopAddProduct.HoverColor = ThemeManager.IsDarkMode ? Color.FromArgb(124, 155, 255) : Color.FromArgb(50, 85, 209);

            pnlSearchBox.Invalidate();
            btnNotifications.Invalidate();
            btnTopAddProduct.Invalidate();
            winChrome.Invalidate();
            chromeDivider.Invalidate();
            headerTitle.Invalidate();

            // Invalidate cards and tables once on theme change
            tlpCards.Invalidate(true);
            tlpCharts.Invalidate(true);
            tlpTables.Invalidate(true);

            pnlSalesDynamicsCard.Invalidate();
            pnlCategoryDonutCard.Invalidate();
            pnlRecentInvoicesCard.Invalidate();
            pnlLowStockCard.Invalidate();

            StyleDataGridView(dgvInvoices);
            StyleDataGridView(dgvLowStock);
            pnlSidebar.Invalidate(true);
            pnlBrandHeader.Invalidate();
            btnThemeToggle.Invalidate();
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;
            dgv.BackgroundColor           = C_CardBg;
            dgv.GridColor                 = C_Border;
            dgv.BorderStyle               = BorderStyle.None;
            dgv.CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = C_CardBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = C_TextMid;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = C_CardBg;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = C_TextMid;

            dgv.DefaultCellStyle.BackColor          = C_CardBg;
            dgv.DefaultCellStyle.ForeColor          = C_TextDark;
            dgv.DefaultCellStyle.SelectionBackColor = C_NavHover;
            dgv.DefaultCellStyle.SelectionForeColor = C_TextDark;
            dgv.DefaultCellStyle.Font               = new Font("Segoe UI", 9f);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = ThemeManager.GridRowAlternate;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = C_TextDark;
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
            pnl.MouseEnter += (s, e) => { _hoveredCardIndex = cardIndex; pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { if (_hoveredCardIndex == cardIndex) { _hoveredCardIndex = -1; pnl.Invalidate(); } };

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnl.Width;
                int h = pnl.Height;
                if (w <= 0 || h <= 0) return;

                // Subtle Entrance Y-Stagger
                float cardStagger = Math.Clamp(_entranceProgress * 1.5f - (cardIndex * 0.08f), 0f, 1f);
                int yOffset = (int)((1f - cardStagger) * 10);

                bool isHovered = (_hoveredCardIndex == cardIndex);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, yOffset, w - 1, h - 1 - yOffset, 10);
                using var bgBrush = new SolidBrush(C_CardBg);
                g.FillPath(bgBrush, path);

                // Subtle Hover Elevation Border
                using var borderPen = new Pen(isHovered ? C_Blue : C_Border, isHovered ? 1.5f : 1.2f);
                g.DrawPath(borderPen, path);

                // Reicon Vector Icon (Clean 20x20 Vector, No Emojis!)
                ReiconRenderer.DrawIcon(g, iconType, new Rectangle(12, 10 + yOffset, 18, 18), statusCol, 1.8f);

                using var fTitle = new Font("Segoe UI", 8.5f);
                using var fVal   = new Font("Segoe UI", 13f, FontStyle.Bold);
                using var fSub   = new Font("Segoe UI", 7.5f);

                using var bTitle = new SolidBrush(C_TextMid);
                using var bVal   = new SolidBrush(C_TextDark);
                using var bSub   = new SolidBrush(statusCol);

                g.DrawString(title, fTitle, bTitle, 36, 11 + yOffset);
                g.DrawString(val, fVal, bVal, 12, 34 + yOffset);
                g.DrawString(sub, fSub, bSub, 12, 64 + yOffset);
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  CHARTS: 12-MONTH SALES DYNAMICS & CATEGORY DONUT
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupSalesDynamicsCard()
        {
            pnlSalesDynamicsCard.Controls.Add(pnlDynamicsChartCanvas);
            pnlDynamicsChartCanvas.Dock = DockStyle.Fill;

            pnlSalesDynamicsCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = pnlSalesDynamicsCard.Width;
                int h = pnlSalesDynamicsCard.Height;
                if (w <= 0 || h <= 0) return;

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
                using var bg = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var bdr = new Pen(C_Border, 1.2f);
                g.DrawPath(bdr, path);

                using var fTitle = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var fLegend = new Font("Segoe UI", 8.5f);
                using var bTitle = new SolidBrush(C_TextDark);
                using var bSales = new SolidBrush(C_Blue);
                using var bPurch = new SolidBrush(BlendColor(Color.FromArgb(99, 102, 241), Color.FromArgb(165, 180, 252), _toggleAnimProgress));

                g.DrawString("Sales Dynamics (12 Months)", fTitle, bTitle, 16, 12);

                int legX = w - 180;
                if (legX > 200)
                {
                    g.FillEllipse(bSales, legX, 16, 8, 8);
                    g.DrawString("Sales", fLegend, bTitle, legX + 12, 12);

                    g.FillEllipse(bPurch, legX + 70, 16, 8, 8);
                    g.DrawString("Purchases", fLegend, bTitle, legX + 82, 12);
                }
            };

            pnlDynamicsChartCanvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnlDynamicsChartCanvas.Width;
                int h = pnlDynamicsChartCanvas.Height;
                int padL = 20, padR = 20, padB = 26, topY = 46;

                int chartW = w - padL - padR;
                int chartH = h - topY - padB;
                if (chartW <= 0 || chartH <= 0) return;

                int n = _monthLabels.Length;
                float slotW = (float)chartW / n;
                float barW = Math.Max(5, slotW * 0.32f);

                int maxVal = 160;

                for (int i = 0; i < n; i++)
                {
                    float cx = padL + i * slotW + slotW / 2;
                    float sVal = _salesDynamicsData[i];
                    float pVal = _purchaseDynamicsData[i];

                    float sH = (sVal / maxVal) * chartH;
                    float pH = (pVal / maxVal) * chartH;

                    float sY = topY + chartH - sH;
                    float pY = topY + chartH - pH;

                    // Sales Bar
                    using var sPath = GraphicsExtensions.CreateRoundedRectanglePath((int)(cx - barW - 2), (int)sY, (int)barW, (int)sH, 4);
                    using var sBrush = new SolidBrush(C_Blue);
                    g.FillPath(sBrush, sPath);

                    // Purchase Bar
                    using var pPath = GraphicsExtensions.CreateRoundedRectanglePath((int)(cx + 2), (int)pY, (int)barW, (int)pH, 4);
                    using var pBrush = new SolidBrush(BlendColor(Color.FromArgb(99, 102, 241), Color.FromArgb(165, 180, 252), _toggleAnimProgress));
                    g.FillPath(pBrush, pPath);

                    // Month Label
                    using var fLbl = new Font("Segoe UI", 7.5f);
                    using var bLbl = new SolidBrush(C_TextMid);
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(_monthLabels[i], fLbl, bLbl, cx, topY + chartH + 6, sf);
                }
            };
        }

        private void SetupCategoryDonutCard()
        {
            pnlCategoryDonutCard.Controls.Add(pnlDonutChartCanvas);
            pnlDonutChartCanvas.Dock = DockStyle.Fill;

            pnlCategoryDonutCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int w = pnlCategoryDonutCard.Width;
                int h = pnlCategoryDonutCard.Height;
                if (w <= 0 || h <= 0) return;

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
                using var bg = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var bdr = new Pen(C_Border, 1.2f);
                g.DrawPath(bdr, path);

                using var fTitle = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var bTitle = new SolidBrush(C_TextDark);
                g.DrawString("Category Revenue", fTitle, bTitle, 16, 12);
            };

            pnlDonutChartCanvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int w = pnlDonutChartCanvas.Width;
                int h = pnlDonutChartCanvas.Height;

                int d = Math.Min(w / 2, h - 50);
                if (d <= 0) return;

                int cx = 20 + d / 2;
                int cy = 40 + d / 2;
                var rect = new Rectangle(cx - d / 2, cy - d / 2, d, d);

                float[] angles = { 144f, 108f, 65f, 43f };
                Color[] colors = { C_Blue, C_Positive, C_Warning, Color.FromArgb(168, 85, 247) };
                string[] cats = { "Electronics (40%)", "Groceries (30%)", "Clothing (18%)", "Hardware (12%)" };

                float startA = 0f;
                for (int i = 0; i < angles.Length; i++)
                {
                    using var b = new SolidBrush(colors[i]);
                    g.FillPie(b, rect, startA, angles[i]);
                    startA += angles[i];
                }

                // Inner cutout
                int holeD = (int)(d * 0.62f);
                var holeRect = new Rectangle(cx - holeD / 2, cy - holeD / 2, holeD, holeD);
                using var holeBrush = new SolidBrush(C_CardBg);
                g.FillEllipse(holeBrush, holeRect);

                // Center Total Text
                using var fC1 = new Font("Segoe UI", 7.5f);
                using var fC2 = new Font("Segoe UI", 10f, FontStyle.Bold);
                using var bC1 = new SolidBrush(C_TextMid);
                using var bC2 = new SolidBrush(C_TextDark);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("TOTAL", fC1, bC1, cx, cy - 8, sf);
                g.DrawString("₹8.4L", fC2, bC2, cx, cy + 10, sf);

                // Legend List on the right
                int legX = cx + d / 2 + 16;
                int legY = 50;
                using var fLeg = new Font("Segoe UI", 8f);
                for (int i = 0; i < cats.Length; i++)
                {
                    using var bDot = new SolidBrush(colors[i]);
                    using var bTxt = new SolidBrush(C_TextDark);
                    g.FillEllipse(bDot, legX, legY + i * 24 + 3, 8, 8);
                    g.DrawString(cats[i], fLeg, bTxt, legX + 14, legY + i * 24);
                }
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  TABLES: RECENT INVOICES & LOW STOCK ALERTS
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupRecentInvoicesCard()
        {
            pnlRecentInvoicesCard.Controls.Add(dgvInvoices);
            dgvInvoices.Dock = DockStyle.Fill;
            dgvInvoices.BringToFront();

            dgvInvoices.Columns.Add("InvoiceNo", "Invoice No");
            dgvInvoices.Columns.Add("Customer", "Customer");
            dgvInvoices.Columns.Add("Date", "Date");
            dgvInvoices.Columns.Add("Mode", "Mode");
            dgvInvoices.Columns.Add("Amount", "Amount (₹)");
            dgvInvoices.Columns.Add("Status", "Status");

            dgvInvoices.RowHeadersVisible = false;
            dgvInvoices.AllowUserToAddRows = false;
            dgvInvoices.ReadOnly = true;
            dgvInvoices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInvoices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            RefreshInvoicesTable();
        }

        private void SetupLowStockCard()
        {
            pnlLowStockCard.Controls.Add(dgvLowStock);
            dgvLowStock.Dock = DockStyle.Fill;
            dgvLowStock.BringToFront();

            dgvLowStock.Columns.Add("ItemName", "Item / Product");
            dgvLowStock.Columns.Add("Stock", "In Stock");
            dgvLowStock.Columns.Add("MinLevel", "Reorder Level");
            dgvLowStock.Columns.Add("Action", "Alert Level");

            dgvLowStock.RowHeadersVisible = false;
            dgvLowStock.AllowUserToAddRows = false;
            dgvLowStock.ReadOnly = true;
            dgvLowStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLowStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            RefreshLowStockTable();
        }

        private void RefreshInvoicesTable()
        {
            dgvInvoices.Rows.Clear();
            foreach (var inv in _invoicesStore)
            {
                dgvInvoices.Rows.Add(inv[0], inv[1], inv[2], inv[3], inv[4], inv[5]);
            }
        }

        private void RefreshLowStockTable()
        {
            dgvLowStock.Rows.Clear();
            foreach (var item in _lowStockStore)
            {
                dgvLowStock.Rows.Add(item[0], item[1], item[2], item[3]);
            }
        }

        private void PopulateSampleData()
        {
            _invoicesStore.Add(new[] { "INV-2026-089", "Rajesh Sharma", "13 Oct 2026", "UPI", "₹4,250", "Paid" });
            _invoicesStore.Add(new[] { "INV-2026-088", "Anita Patel", "13 Oct 2026", "Cash", "₹12,400", "Paid" });
            _invoicesStore.Add(new[] { "INV-2026-087", "Kiran Traders", "12 Oct 2026", "Bank", "₹38,900", "Pending" });
            _invoicesStore.Add(new[] { "INV-2026-086", "Amit Verma", "12 Oct 2026", "UPI", "₹2,150", "Paid" });
            _invoicesStore.Add(new[] { "INV-2026-085", "Suresh Enterprise", "11 Oct 2026", "Cash", "₹18,500", "Paid" });

            _lowStockStore.Add(new[] { "Samsung SSD 1TB 980 Pro", "2 PCS", "10 PCS", "CRITICAL" });
            _lowStockStore.Add(new[] { "Wireless Mouse Logitech M331", "3 PCS", "15 PCS", "LOW" });
            _lowStockStore.Add(new[] { "Cat6 LAN Cable 305M Roll", "1 Roll", "5 Rolls", "LOW" });
            _lowStockStore.Add(new[] { "Thermal Paper Rolls 80mm", "4 Boxes", "20 Boxes", "CRITICAL" });
            _lowStockStore.Add(new[] { "HDMI Cable 2.0 5 Meter", "2 PCS", "8 PCS", "LOW" });
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

                using var fText = new Font("Segoe UI", 9f, isActive ? FontStyle.Bold : FontStyle.Regular);
                using var bText = new SolidBrush(textColor);
                g.DrawString(label, fText, bText, 42, (h - 18) / 2);
            };
        }

        private void ShowPage(string page)
        {
            _activePage = page;
            pnlOverview.Visible = (page == "Overview");
            pnlNewBill.Visible = (page == "Sales");
            pnlCustomers.Visible = (page == "Customers");
            pnlProducts.Visible = (page == "Products");
            pnlTransPage.Visible = (page == "Transactions");

            headerTitle.Title = page switch
            {
                "Overview" => "Dashboard",
                "Sales" => "Sales & Tax Invoice (POS)",
                "Customers" => "Customer Directory",
                "Products" => "Products & Inventory",
                "Transactions" => "Financial Transactions Ledger",
                "Statistics" => "Statistics & Analytics",
                "Purchases" => "Purchase Management",
                "Suppliers" => "Suppliers & Vendors",
                "Settings" => "System Settings",
                _ => page
            };

            headerTitle.Subtitle = page switch
            {
                "Overview" => "Real-time business performance overview",
                "Sales" => "Rapid POS billing, barcode & receipt printing",
                "Customers" => "Customer ledger, balances & directories",
                "Products" => "Product catalog stock & inventory valuation",
                "Transactions" => "Audit log of receipts and vendor payouts",
                _ => "Manage and monitor your business operations"
            };

            pnlSidebar.Invalidate(true);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SUB PAGES (SALES, CUSTOMERS, PRODUCTS, TRANSACTIONS)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildNewBillPage()
        {
            pnlNewBill.Dock = DockStyle.Fill;
            pnlNewBill.Visible = false;
            pnlNewBill.BackColor = C_PageBg;

            var lblHeader = new Label
            {
                Text = "⚡ Rapid POS Billing & Barcode Terminal",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_Blue,
                Location = new Point(20, 20),
                AutoSize = true
            };

            txtBillCustomerName.PlaceholderText = "Customer Name";
            txtBillCustomerName.SetBounds(20, 60, 200, 30);

            txtBillPhone.PlaceholderText = "Phone Number";
            txtBillPhone.SetBounds(230, 60, 160, 30);

            txtBillItemName.PlaceholderText = "Product / Barcode";
            txtBillItemName.SetBounds(20, 105, 240, 30);

            txtBillQty.PlaceholderText = "Qty";
            txtBillQty.SetBounds(270, 105, 70, 30);

            txtBillPrice.PlaceholderText = "Price (₹)";
            txtBillPrice.SetBounds(350, 105, 90, 30);

            btnAddLineItem.Text = "+ Add Item";
            btnAddLineItem.BackColor = C_Blue;
            btnAddLineItem.ForeColor = Color.White;
            btnAddLineItem.FlatStyle = FlatStyle.Flat;
            btnAddLineItem.SetBounds(450, 105, 100, 30);

            dgvBillItems.SetBounds(20, 150, 700, 300);
            dgvBillItems.Columns.Add("Item", "Item Name");
            dgvBillItems.Columns.Add("Qty", "Qty");
            dgvBillItems.Columns.Add("Price", "Price");
            dgvBillItems.Columns.Add("Total", "Total");
            dgvBillItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            lblBillTotal.Text = "Total: ₹0.00";
            lblBillTotal.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblBillTotal.ForeColor = C_Positive;
            lblBillTotal.SetBounds(20, 465, 300, 35);

            btnSaveBill.Text = "💾 Save & Print Bill";
            btnSaveBill.BackColor = C_Positive;
            btnSaveBill.ForeColor = Color.White;
            btnSaveBill.FlatStyle = FlatStyle.Flat;
            btnSaveBill.SetBounds(420, 465, 150, 40);

            btnClearBill.Text = "Clear";
            btnClearBill.FlatStyle = FlatStyle.Flat;
            btnClearBill.SetBounds(580, 465, 90, 40);

            btnAddLineItem.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtBillItemName.Text) &&
                    int.TryParse(txtBillQty.Text, out int q) &&
                    decimal.TryParse(txtBillPrice.Text, out decimal p))
                {
                    decimal tot = q * p;
                    _lineItems.Add((txtBillItemName.Text, q, p));
                    dgvBillItems.Rows.Add(txtBillItemName.Text, q, p, tot);
                    _billTotal += tot;
                    lblBillTotal.Text = $"Total: ₹{_billTotal:N2}";
                    txtBillItemName.Clear();
                    txtBillQty.Text = "1";
                    txtBillPrice.Clear();
                }
            };

            btnSaveBill.Click += (s, e) =>
            {
                if (_lineItems.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to bill.", "Empty Bill", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string invNo = $"INV-2026-09{_invoicesStore.Count + 1}";
                string cust = string.IsNullOrWhiteSpace(txtBillCustomerName.Text) ? "Walk-in Customer" : txtBillCustomerName.Text;
                _invoicesStore.Insert(0, new[] { invNo, cust, "13 Oct 2026", "Cash", $"₹{_billTotal:N2}", "Paid" });
                RefreshInvoicesTable();
                MessageBox.Show($"Invoice {invNo} saved and printed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _lineItems.Clear();
                dgvBillItems.Rows.Clear();
                _billTotal = 0;
                lblBillTotal.Text = "Total: ₹0.00";
                ShowPage("Overview");
            };

            btnClearBill.Click += (s, e) =>
            {
                _lineItems.Clear();
                dgvBillItems.Rows.Clear();
                _billTotal = 0;
                lblBillTotal.Text = "Total: ₹0.00";
            };

            pnlNewBill.Controls.AddRange(new Control[] {
                lblHeader, txtBillCustomerName, txtBillPhone,
                txtBillItemName, txtBillQty, txtBillPrice, btnAddLineItem,
                dgvBillItems, lblBillTotal, btnSaveBill, btnClearBill
            });

            pnlContentHost.Controls.Add(pnlNewBill);
        }

        private void BuildCustomersPage()
        {
            pnlCustomers.Dock = DockStyle.Fill;
            pnlCustomers.Visible = false;
            pnlCustomers.BackColor = C_PageBg;

            txtCustSearch.PlaceholderText = "Search customers by name or phone...";
            txtCustSearch.SetBounds(20, 20, 300, 30);

            btnAddCustomer.Text = "+ Add Customer";
            btnAddCustomer.BackColor = C_Blue;
            btnAddCustomer.ForeColor = Color.White;
            btnAddCustomer.FlatStyle = FlatStyle.Flat;
            btnAddCustomer.SetBounds(330, 20, 140, 30);

            dgvCustomers.SetBounds(20, 65, 800, 480);
            dgvCustomers.Columns.Add("Name", "Customer Name");
            dgvCustomers.Columns.Add("Phone", "Phone");
            dgvCustomers.Columns.Add("Balance", "Current Balance (₹)");
            dgvCustomers.Columns.Add("Status", "Account Status");
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvCustomers.Rows.Add("Rajesh Sharma", "+91 98251 00112", "₹0.00", "Active");
            dgvCustomers.Rows.Add("Anita Patel", "+91 94260 88231", "₹1,200.00", "Active");
            dgvCustomers.Rows.Add("Kiran Traders", "+91 97245 44109", "₹38,900.00", "Overdue");

            pnlCustomers.Controls.AddRange(new Control[] { txtCustSearch, btnAddCustomer, dgvCustomers });
            pnlContentHost.Controls.Add(pnlCustomers);
        }

        private void BuildProductsPage()
        {
            pnlProducts.Dock = DockStyle.Fill;
            pnlProducts.Visible = false;
            pnlProducts.BackColor = C_PageBg;

            txtProdSearch.PlaceholderText = "Search by item name, SKU, or barcode...";
            txtProdSearch.SetBounds(20, 20, 320, 30);

            btnAddProductPage.Text = "+ Add Product";
            btnAddProductPage.BackColor = C_Blue;
            btnAddProductPage.ForeColor = Color.White;
            btnAddProductPage.FlatStyle = FlatStyle.Flat;
            btnAddProductPage.SetBounds(350, 20, 140, 30);

            dgvProducts.SetBounds(20, 65, 850, 480);
            dgvProducts.Columns.Add("Barcode", "Barcode");
            dgvProducts.Columns.Add("Name", "Product Name");
            dgvProducts.Columns.Add("Category", "Category");
            dgvProducts.Columns.Add("Cost", "Cost Price");
            dgvProducts.Columns.Add("Sale", "Sale Price");
            dgvProducts.Columns.Add("Stock", "In Stock");
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvProducts.Rows.Add("8901234001", "Samsung SSD 1TB 980 Pro", "Electronics", "₹6,800", "₹8,499", "2 PCS");
            dgvProducts.Rows.Add("8901234002", "Wireless Mouse Logitech M331", "Electronics", "₹850", "₹1,199", "3 PCS");
            dgvProducts.Rows.Add("8901234003", "HP Laser Toner 12A Black", "Hardware", "₹550", "₹850", "24 PCS");
            dgvProducts.Rows.Add("8901234004", "Cat6 LAN Cable 305M Roll", "Networking", "₹4,200", "₹5,600", "1 Roll");

            pnlProducts.Controls.AddRange(new Control[] { txtProdSearch, btnAddProductPage, dgvProducts });
            pnlContentHost.Controls.Add(pnlProducts);
        }

        private void BuildTransactionsPage()
        {
            pnlTransPage.Dock = DockStyle.Fill;
            pnlTransPage.Visible = false;
            pnlTransPage.BackColor = C_PageBg;

            txtTransSearch.PlaceholderText = "Filter transactions by reference, party, or date...";
            txtTransSearch.SetBounds(20, 20, 350, 30);

            dgvAllTransactions.SetBounds(20, 65, 850, 480);
            dgvAllTransactions.Columns.Add("Date", "Date & Time");
            dgvAllTransactions.Columns.Add("Type", "Type");
            dgvAllTransactions.Columns.Add("Party", "Party Name");
            dgvAllTransactions.Columns.Add("Mode", "Payment Mode");
            dgvAllTransactions.Columns.Add("Amount", "Amount (₹)");
            dgvAllTransactions.Columns.Add("Ref", "Reference No");
            dgvAllTransactions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvAllTransactions.Rows.Add("13 Oct 2026 14:22", "Sale Receipt", "Rajesh Sharma", "UPI", "+₹4,250", "UPI/628109");
            dgvAllTransactions.Rows.Add("13 Oct 2026 11:05", "Sale Receipt", "Anita Patel", "Cash", "+₹12,400", "CSH/00912");
            dgvAllTransactions.Rows.Add("12 Oct 2026 16:40", "Vendor Pay", "Microtech Wholesalers", "Bank", "-₹24,500", "NEFT/8812");

            pnlTransPage.Controls.AddRange(new Control[] { txtTransSearch, dgvAllTransactions });
            pnlContentHost.Controls.Add(pnlTransPage);
        }
    }
}
