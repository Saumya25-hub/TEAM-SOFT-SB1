using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// SB1 — Simple Billing Dashboard.
    /// Reference-image layout: dark slate sidebar, white top-bar, stat row,
    /// transaction list + revenue bar chart, donut summary, sales-report bars.
    /// Color palette: original blue-500 / violet-600 / slate-900 (matches Splash & Login).
    /// </summary>
    public partial class DashboardForm : Form
    {
        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME STATE & ANIMATION
        // ═══════════════════════════════════════════════════════════════════════════
        private bool  _isDarkMode = false;
        private float _toggleAnimProgress = 0f; // 0.0f = Light, 1.0f = Dark
        private float _targetAnimProgress = 0f;
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 15 };

        // ═══════════════════════════════════════════════════════════════════════════
        //  COLOUR PALETTE  — dynamic getters blending smoothly with _toggleAnimProgress
        // ═══════════════════════════════════════════════════════════════════════════
        private static readonly Color C_NavActive  = Color.FromArgb(37,  99, 235);   // blue-600
        private static readonly Color C_Blue       = Color.FromArgb(59,  130, 246);  // blue-500
        private static readonly Color C_Violet     = Color.FromArgb(124,  58, 237);  // violet-600
        private static readonly Color C_Amber      = Color.FromArgb(245, 158,  11);  // amber-500
        private static readonly Color C_Red        = Color.FromArgb(239,  68,  68);  // red-500
        private static readonly Color C_BarIncome  = Color.FromArgb(59,  130, 246);  // blue-500

        private Color C_SidebarBg  => BlendColor(Color.FromArgb(15,  23,  42),  Color.FromArgb(14,  15,  22), _toggleAnimProgress);
        private Color C_NavHover   => BlendColor(Color.FromArgb(30,  41,  59),  Color.FromArgb(28,  30,  44), _toggleAnimProgress);
        private Color C_NavText    => BlendColor(Color.FromArgb(148, 163, 184), Color.FromArgb(148, 163, 184), _toggleAnimProgress);
        private Color C_NavTextOn  => Color.White;

        private Color C_PageBg     => BlendColor(Color.FromArgb(241, 245, 249), Color.FromArgb(15,  16,  22), _toggleAnimProgress);
        private Color C_CardBg     => BlendColor(Color.White,                   Color.FromArgb(22,  23,  34), _toggleAnimProgress);
        private Color C_Border     => BlendColor(Color.FromArgb(226, 232, 240), Color.FromArgb(35,  37,  54), _toggleAnimProgress);
        private Color C_TopBarBg   => BlendColor(Color.White,                   Color.FromArgb(18,  19,  28), _toggleAnimProgress);
        private Color C_SearchBg   => BlendColor(Color.FromArgb(248, 250, 252), Color.FromArgb(26,  28,  42), _toggleAnimProgress);
        private Color C_SearchBdr  => BlendColor(Color.FromArgb(203, 213, 225), Color.FromArgb(45,  48,  70), _toggleAnimProgress);

        private Color C_TextDark   => BlendColor(Color.FromArgb(15,  23,  42),  Color.FromArgb(248, 250, 252), _toggleAnimProgress);
        private Color C_TextMid    => BlendColor(Color.FromArgb(71,  85,  105), Color.FromArgb(148, 163, 184), _toggleAnimProgress);
        private Color C_TextLight  => BlendColor(Color.FromArgb(148, 163, 184), Color.FromArgb(100, 116, 139), _toggleAnimProgress);

        private Color C_HeroBg     => BlendColor(Color.FromArgb(15,  23,  42),  Color.FromArgb(18,  19,  30), _toggleAnimProgress);
        private Color C_BarExpense => BlendColor(Color.FromArgb(226, 232, 240), Color.FromArgb(38,  42,  60),  _toggleAnimProgress);

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
        //  STATE
        // ═══════════════════════════════════════════════════════════════════════════
        private string _activePage = "Overview";

        private readonly List<(string Item, int Qty, decimal Price)> _lineItems = new();
        private decimal _billTotal = 0m;

        private readonly List<string[]> _billsStore     = new();
        private readonly List<string[]> _customersStore = new();
        private readonly List<string[]> _productsStore  = new();

        private bool  _isDragging;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        // Chart bar heights (revenue, last 7 months)
        private static readonly int[]     _barIncome  = { 55, 80, 60, 95, 70, 115, 100 };
        private static readonly int[]     _barExpense = { 30, 45, 35, 50, 40,  65,  55 };
        private static readonly string[]  _barLabels  = { "Nov","Dec","Jan","Feb","Mar","Apr","May" };

        // ═══════════════════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════
        public DashboardForm()
        {
            InitializeComponent();
            InitThemeToggle();
            PopulateSampleData();
            BuildOverviewPage();
            BuildNewBillPage();
            BuildCustomersPage();
            BuildProductsPage();
            BuildTransactionsPage();
            SetupSidebarNav();
            SetupWindowDrag();
            ShowPage("Overview");
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  OVERRIDES
        // ═══════════════════════════════════════════════════════════════════════════
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedRegion(16);
            RepositionTopBarItems();
            ApplyTheme();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionTopBarItems();
            if (WindowState != FormWindowState.Normal) Region = null;
            else ApplyRoundedRegion(16);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // Outer rounded border
            using var pen = new Pen(_isDarkMode ? Color.FromArgb(40, 44, 64) : Color.FromArgb(30, 41, 59), 1.5f);
            g.DrawRoundedRectangle(pen, 0, 0, Width - 1, Height - 1, 16);
            // Sidebar logo area
            PaintSidebarLogo(g);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  WINDOW HELPERS
        // ═══════════════════════════════════════════════════════════════════════════
        private void ApplyRoundedRegion(int r)
        {
            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, Width, Height, r);
            Region = new Region(path);
        }

        private void RepositionTopBarItems()
        {
            int tbW   = pnlTopBar.Width;   // topbar width (excludes sidebar)
            int tbH   = pnlTopBar.Height;  // 68px

            int btnW  = 160;  // Add button
            int togW  = 68;   // Theme toggle button
            int srchW = 250;  // Search box
            int ctrlH = 38;   // common height
            int togH  = 34;   // toggle height
            int gap   = 10;   // gap between elements

            // Chrome buttons — right edge, vertically centred
            btnClose.Location    = new Point(tbW - 42,          (tbH - 30) / 2);
            btnMaximize.Location = new Point(tbW - 42 - 38,     (tbH - 30) / 2);
            btnMinimize.Location = new Point(tbW - 42 - 38*2,   (tbH - 30) / 2);

            int chromeEnd = 42 + 38*2 + 8; // total space used by chrome + small pad

            // Add button — left of chrome
            int btnX = tbW - chromeEnd - gap - btnW;
            btnTopAddProduct.Size     = new Size(btnW, ctrlH);
            btnTopAddProduct.Location = new Point(btnX, (tbH - ctrlH) / 2);

            // Theme toggle button — left of add button
            int togX = btnX - gap - togW;
            btnThemeToggle.Size     = new Size(togW, togH);
            btnThemeToggle.Location = new Point(togX, (tbH - togH) / 2);

            // Search box — left of toggle button (right beside search bar!)
            int srchX = togX - gap - srchW;
            pnlSearchBox.Size     = new Size(srchW, ctrlH);
            pnlSearchBox.Location = new Point(srchX, (tbH - ctrlH) / 2);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME TOGGLE SETUP & ANIMATION
        // ═══════════════════════════════════════════════════════════════════════════
        private void InitThemeToggle()
        {
            btnThemeToggle.Paint += (s, e) => PaintThemeToggle(e.Graphics);

            _animTimer.Tick += (s, e) =>
            {
                float diff = _targetAnimProgress - _toggleAnimProgress;
                if (Math.Abs(diff) < 0.02f)
                {
                    _toggleAnimProgress = _targetAnimProgress;
                    _animTimer.Stop();
                }
                else
                {
                    // Smooth ease-out interpolation for 60 FPS transition
                    _toggleAnimProgress += diff * 0.28f;
                }

                ApplyTheme();
            };

            btnThemeToggle.Click += (s, e) =>
            {
                _isDarkMode = !_isDarkMode;
                _targetAnimProgress = _isDarkMode ? 1.0f : 0.0f;
                _animTimer.Start();
            };

            // Custom search box painting with dynamic theme colors
            pnlSearchBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var rp = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnlSearchBox.Width - 1, pnlSearchBox.Height - 1, 8);
                using var bg = new SolidBrush(C_SearchBg);
                g.FillPath(bg, rp);
                using var bp = new Pen(C_SearchBdr, 1.2f);
                g.DrawPath(bp, rp);
                using var iconPen = new Pen(C_TextLight, 1.5f);
                g.DrawEllipse(iconPen, 10, 11, 13, 13);
                g.DrawLine(iconPen, 21, 22, 27, 28);
            };
        }

        private void PaintThemeToggle(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = btnThemeToggle.Width;
            int h = btnThemeToggle.Height;

            // 1. Capsule Track
            int radius = (h - 2) / 2;
            using var trackPath = GraphicsExtensions.CreateRoundedRectanglePath(1, 1, w - 3, h - 3, radius * 2);

            Color trackBg = BlendColor(Color.FromArgb(226, 232, 240), Color.FromArgb(30, 32, 48), _toggleAnimProgress);
            Color trackBorder = BlendColor(Color.FromArgb(203, 213, 225), Color.FromArgb(99, 102, 241), _toggleAnimProgress);

            using var trackBrush = new SolidBrush(trackBg);
            g.FillPath(trackBrush, trackPath);

            using var trackPen = new Pen(trackBorder, 1.2f);
            g.DrawPath(trackPen, trackPath);

            // 2. Background Sun Icon (Left side)
            Color sunColor = BlendColor(Color.FromArgb(245, 158, 11), Color.FromArgb(71, 85, 105), _toggleAnimProgress);
            DrawMiniSun(g, 18f, h / 2f, sunColor);

            // 3. Background Moon Icon (Right side)
            Color moonColor = BlendColor(Color.FromArgb(148, 163, 184), Color.FromArgb(165, 180, 252), _toggleAnimProgress);
            DrawMiniMoon(g, w - 18f, h / 2f, moonColor);

            // 4. Sliding Thumb Knob
            float knobD = 24f; // knob diameter
            float minX = 5f;
            float maxX = w - knobD - 5f;
            float knobX = minX + _toggleAnimProgress * (maxX - minX);
            float knobY = (h - knobD) / 2f;

            // Drop shadow under knob
            using var shadowPath = new GraphicsPath();
            shadowPath.AddEllipse(knobX, knobY + 1.2f, knobD, knobD);
            using var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0));
            g.FillPath(shadowBrush, shadowPath);

            // Knob body
            using var knobPath = new GraphicsPath();
            knobPath.AddEllipse(knobX, knobY, knobD, knobD);

            if (_toggleAnimProgress < 0.5f)
            {
                // Light mode knob: crisp white with subtle border and golden sun
                using var knobBrush = new LinearGradientBrush(
                    new PointF(knobX, knobY), new PointF(knobX + knobD, knobY + knobD),
                    Color.White, Color.FromArgb(248, 250, 252));
                g.FillPath(knobBrush, knobPath);

                using var knobPen = new Pen(Color.FromArgb(226, 232, 240), 1f);
                g.DrawPath(knobPen, knobPath);

                DrawMiniSun(g, knobX + knobD / 2f, knobY + knobD / 2f, Color.FromArgb(245, 158, 11));
            }
            else
            {
                // Dark mode knob: glowing indigo/violet gradient with white crescent moon
                using var knobBrush = new LinearGradientBrush(
                    new PointF(knobX, knobY), new PointF(knobX + knobD, knobY + knobD),
                    Color.FromArgb(59, 130, 246), Color.FromArgb(139, 92, 246));
                g.FillPath(knobBrush, knobPath);

                using var knobPen = new Pen(Color.FromArgb(165, 180, 252), 1f);
                g.DrawPath(knobPen, knobPath);

                DrawMiniMoon(g, knobX + knobD / 2f, knobY + knobD / 2f, Color.White);
            }
        }

        private static void DrawMiniSun(Graphics g, float cx, float cy, Color clr)
        {
            using var sunBrush = new SolidBrush(clr);
            using var rayPen = new Pen(clr, 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

            // Sun center disc
            g.FillEllipse(sunBrush, cx - 3.5f, cy - 3.5f, 7f, 7f);

            // 4 mini rays
            g.DrawLine(rayPen, cx, cy - 6f, cx, cy - 4.5f);
            g.DrawLine(rayPen, cx, cy + 4.5f, cx, cy + 6f);
            g.DrawLine(rayPen, cx - 6f, cy, cx - 4.5f, cy);
            g.DrawLine(rayPen, cx + 4.5f, cy, cx + 6f, cy);
        }

        private static void DrawMiniMoon(Graphics g, float cx, float cy, Color clr)
        {
            using var moonBrush = new SolidBrush(clr);
            using var moonPath = new GraphicsPath();
            moonPath.AddArc(cx - 5f, cy - 5f, 10f, 10f, 60f, 240f);
            moonPath.AddArc(cx - 3f, cy - 4f, 7.5f, 8f, 270f, -180f);
            moonPath.CloseFigure();
            g.FillPath(moonBrush, moonPath);
        }

        private void ApplyTheme()
        {
            // Form background
            this.BackColor = _isDarkMode ? Color.FromArgb(11, 12, 17) : Color.FromArgb(15, 23, 42);

            // Topbar & Sidebar
            pnlTopBar.BackColor  = C_TopBarBg;
            pnlSidebar.BackColor = C_SidebarBg;

            // Content host & pages
            pnlContent.BackColor   = C_PageBg;
            pnlOverview.BackColor  = C_PageBg;
            pnlNewBill.BackColor   = C_PageBg;
            pnlCustomers.BackColor = C_PageBg;
            pnlProducts.BackColor  = C_PageBg;
            pnlTransPage.BackColor = C_PageBg;

            // Topbar Labels
            lblPageTitle.ForeColor = C_TextDark;
            lblPageSub.ForeColor   = C_TextLight;

            // Search Box
            pnlSearchBox.BackColor = C_SearchBg;
            txtSearch.BackColor    = C_SearchBg;
            txtSearch.ForeColor    = C_TextDark;

            // Inputs
            ApplyTextBoxTheme(txtSearch);
            ApplyTextBoxTheme(txtBillCustomerName);
            ApplyTextBoxTheme(txtBillPhone);
            ApplyTextBoxTheme(txtBillItemName);
            ApplyTextBoxTheme(txtBillQty);
            ApplyTextBoxTheme(txtBillPrice);
            ApplyTextBoxTheme(txtCustSearch);
            ApplyTextBoxTheme(txtProdSearch);
            ApplyTextBoxTheme(txtTransSearch);

            // Grids
            ApplyGridTheme(dgvTransactions);
            ApplyGridTheme(dgvBillItems);
            ApplyGridTheme(dgvCustomers);
            ApplyGridTheme(dgvProducts);
            ApplyGridTheme(dgvAllTransactions);

            // Redraw panels
            pnlTopBar.Invalidate();
            pnlSearchBox.Invalidate();
            pnlSidebar.Invalidate();
            btnThemeToggle.Invalidate();
            pnlOverview.Invalidate(true);
            pnlNewBill.Invalidate(true);
            pnlCustomers.Invalidate(true);
            pnlProducts.Invalidate(true);
            pnlTransPage.Invalidate(true);
        }

        private void ApplyTextBoxTheme(TextBox? txt)
        {
            if (txt == null) return;
            txt.BackColor = C_SearchBg;
            txt.ForeColor = C_TextDark;
        }

        private void ApplyGridTheme(DataGridView? dgv)
        {
            if (dgv == null) return;
            dgv.BackgroundColor = C_CardBg;
            dgv.GridColor        = C_Border;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(28, 30, 44) : Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = _isDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

            dgv.DefaultCellStyle.BackColor          = C_CardBg;
            dgv.DefaultCellStyle.ForeColor          = C_TextDark;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(37, 99, 235);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(26, 27, 40) : Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = C_TextDark;

            dgv.Invalidate();
        }

        private void SetupWindowDrag()
        {
            pnlTopBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                { _isDragging = true; _dragCursorPoint = Cursor.Position; _dragFormPoint = Location; }
            };
            pnlTopBar.MouseMove += (s, e) =>
            {
                if (_isDragging)
                { var d = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint)); Location = Point.Add(_dragFormPoint, new Size(d)); }
            };
            pnlTopBar.MouseUp += (s, e) => _isDragging = false;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SIDEBAR — logo + nav  (reference image layout, blue palette)
        // ═══════════════════════════════════════════════════════════════════════════
        private void PaintSidebarLogo(Graphics g)
        {
            int sw = pnlSidebar.Width;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // ── Star/asterisk icon + "Simple Billing" (left-aligned like reference) ─
            int sx = 20, sy = 24, sr = 9;
            using var starPen = new Pen(C_Blue, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            for (int i = 0; i < 6; i++)
            {
                double angle = i * Math.PI / 3;
                float x1 = sx + (float)Math.Cos(angle) * (sr * 0.35f);
                float y1 = sy + (float)Math.Sin(angle) * (sr * 0.35f);
                float x2 = sx + (float)Math.Cos(angle) * sr;
                float y2 = sy + (float)Math.Sin(angle) * sr;
                g.DrawLine(starPen, x1, y1, x2, y2);
            }
            using var appF = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var appB = new SolidBrush(Color.White);
            g.DrawString("Simple Billing", appF, appB, sx + sr + 8, sy - 9);

            // ── Divider below logo ───────────────────────────────────────────────
            using var dp = new Pen(Color.FromArgb(30, 255, 255, 255), 1f);
            g.DrawLine(dp, 14, 48, sw - 14, 48);

            // ── "MENU" section label ─────────────────────────────────────────────
            using var secF = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            using var secB = new SolidBrush(Color.FromArgb(100, 116, 139));
            g.DrawString("MENU", secF, secB, 18f, 58f);

            // ── "GENERAL" section label (above Settings) ─────────────────────────
            g.DrawString("GENERAL", secF, secB, 18f, pnlSidebar.Height - 148f);

            // ── Divider above user row ────────────────────────────────────────────
            g.DrawLine(dp, 14, pnlSidebar.Height - 72, sw - 14, pnlSidebar.Height - 72);

            // ── User avatar + labels ─────────────────────────────────────────────
            int ay = pnlSidebar.Height - 42, ar = 16, ax = 22;
            var avSf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var ag = new LinearGradientBrush(
                new Point(ax - ar, ay - ar), new Point(ax + ar, ay + ar), C_Blue, C_Violet);
            g.FillEllipse(ag, ax - ar, ay - ar, ar * 2, ar * 2);
            using var avf = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var avb = new SolidBrush(Color.White);
            g.DrawString("J", avf, avb, ax, ay, avSf);

            lblSidebarUser.Location  = new Point(44, ay - 12);
            lblSidebarEmail.Location = new Point(44, ay + 6);
        }

        private void SetupSidebarNav()
        {
            int rowH = 42;

            // MENU items — matches reference photo order exactly
            var menuItems = new (string page, string label, Button btn, bool badge, string badgeText, bool chevron)[]
            {
                ("Overview",     "Overview",     btnNavOverview,     false, "",   false),
                ("Statistics",   "Statistics",   btnNavStatistics,   false, "",   false),
                ("Customers",    "Customers",    btnNavCustomers,    false, "",   false),
                ("Products",     "Product",      btnNavProducts,     false, "",   true),
                ("Messages",     "Messages",     btnNavMessages,     true,  "13", false),
                ("Transactions", "Transactions", btnNavTransactions, false, "",   false),
            };

            int startY = 76;
            for (int i = 0; i < menuItems.Length; i++)
            {
                var item = menuItems[i];
                ConfigNavBtn(item.btn, item.label, item.page, item.badge, item.badgeText, item.chevron);
                item.btn.Location = new Point(0, startY + i * rowH);
                item.btn.Size     = new Size(pnlSidebar.Width, rowH);
            }

            // GENERAL section
            int genStart = pnlSidebar.Height - 132;
            ConfigNavBtn(btnNavSettings, "Settings", "Settings", false, "", false);
            ConfigNavBtn(btnNavSecurity, "Security", "Security", false, "", false);
            btnNavSettings.Location = new Point(0, genStart);
            btnNavSecurity.Location = new Point(0, genStart + rowH);
            btnNavSettings.Size = btnNavSecurity.Size = new Size(pnlSidebar.Width, rowH);
            btnNavSecurity.Visible = true;

            // NewBill accessed via top button — hide from sidebar
            btnNavNewBill.Visible = false;
        }

        private void ConfigNavBtn(Button btn, string label, string page,
                                   bool hasBadge, string badgeText, bool hasChevron)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 59, 130, 246);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(35, 59, 130, 246);
            btn.BackColor = Color.Transparent;
            btn.ForeColor = C_NavText;
            btn.Font      = new Font("Segoe UI", 9.5f);
            btn.Text      = string.Empty;
            btn.Cursor    = Cursors.Hand;
            btn.Tag       = page;

            btn.Paint += (s, e) =>
            {
                var b2 = (Button)s!;
                var g2 = e.Graphics;
                g2.SmoothingMode = SmoothingMode.AntiAlias;
                g2.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bool isActive = _activePage == page;
                int  bw = b2.Width, bh = b2.Height;

                // Active tinted background
                if (isActive)
                {
                    using var bgBr = new SolidBrush(Color.FromArgb(30, 59, 130, 246));
                    g2.FillRectangle(bgBr, 0, 0, bw, bh);
                    // Left blue accent bar
                    using var barBr   = new SolidBrush(C_Blue);
                    using var barPath = GraphicsExtensions.CreateRoundedRectanglePath(0, 7, 4, bh - 14, 2);
                    g2.FillPath(barBr, barPath);
                }

                // Nav icon
                DrawNavIcon(g2, page, 20, bh / 2, isActive ? C_Blue : C_NavText);

                // Label text
                using var textBr = new SolidBrush(isActive ? Color.White : C_NavText);
                using var textF  = new Font("Segoe UI", 9.5f, isActive ? FontStyle.Bold : FontStyle.Regular);
                float textX = 38f;
                float textW = bw - textX - (hasChevron ? 22 : 6) - (hasBadge ? 38 : 0);
                var sf3 = new StringFormat { LineAlignment = StringAlignment.Center };
                g2.DrawString(label, textF, textBr, new RectangleF(textX, 0, textW, bh), sf3);

                // Notification badge
                if (hasBadge && !string.IsNullOrEmpty(badgeText))
                {
                    int bx = bw - 44, by = bh / 2 - 9;
                    using var badgeBr = new SolidBrush(C_Blue);
                    using var badgeP  = GraphicsExtensions.CreateRoundedRectanglePath(bx, by, 30, 18, 9);
                    g2.FillPath(badgeBr, badgeP);
                    using var badgeF  = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                    using var badgeTB = new SolidBrush(Color.White);
                    var bsf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g2.DrawString(badgeText, badgeF, badgeTB, new RectangleF(bx, by, 30, 18), bsf);
                }

                // Dropdown chevron (Product)
                if (hasChevron)
                {
                    int cx2 = bw - 18, cy2 = bh / 2;
                    using var chevPen = new Pen(C_NavText, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                    g2.DrawLine(chevPen, cx2 - 4, cy2 - 3, cx2, cy2 + 3);
                    g2.DrawLine(chevPen, cx2, cy2 + 3, cx2 + 4, cy2 - 3);
                }
            };

            btn.Click += (s, e) => { if (btn.Tag is string p) ShowPage(p); };
        }

        // Pixel-drawn nav icons, matching reference image shapes
        private static void DrawNavIcon(Graphics g, string page, int cx, int cy, Color clr)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var p  = new Pen(clr, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var br = new SolidBrush(clr);
            switch (page)
            {
                case "Overview":      // 2x2 grid
                    g.FillRectangle(br, cx - 6, cy - 6, 5, 5);
                    g.FillRectangle(br, cx + 2, cy - 6, 5, 5);
                    g.FillRectangle(br, cx - 6, cy + 2, 5, 5);
                    g.FillRectangle(br, cx + 2, cy + 2, 5, 5);
                    break;
                case "Statistics":    // bar chart
                    g.FillRectangle(br, cx - 6, cy + 2, 3, 4);
                    g.FillRectangle(br, cx - 2, cy - 1, 3, 7);
                    g.FillRectangle(br, cx + 2, cy - 5, 3, 11);
                    break;
                case "Customers":     // person silhouette
                    g.DrawEllipse(p, cx - 4, cy - 7, 7, 7);
                    g.DrawArc(p, cx - 6, cy + 1, 11, 6, 180, 180);
                    break;
                case "Products":      // box/product
                    g.DrawRectangle(p, cx - 5, cy - 4, 10, 9);
                    g.DrawLine(p, cx - 5, cy, cx + 5, cy);
                    break;
                case "Messages":      // envelope
                    g.DrawRectangle(p, cx - 6, cy - 4, 12, 9);
                    g.DrawLine(p, cx - 6, cy - 4, cx, cy + 1);
                    g.DrawLine(p, cx + 6, cy - 4, cx, cy + 1);
                    break;
                case "Transactions":  // swap arrows
                    g.DrawLine(p, cx - 5, cy - 2, cx + 5, cy - 2);
                    g.DrawLine(p, cx + 2, cy - 5, cx + 5, cy - 2);
                    g.DrawLine(p, cx - 5, cy + 2, cx + 5, cy + 2);
                    g.DrawLine(p, cx - 5, cy + 2, cx - 2, cy - 1);
                    break;
                case "Settings":      // gear
                    g.DrawEllipse(p, cx - 4, cy - 4, 8, 8);
                    for (int i = 0; i < 6; i++)
                    {
                        double a = i * Math.PI / 3;
                        g.DrawLine(p,
                            cx + (float)Math.Cos(a) * 5f, cy + (float)Math.Sin(a) * 5f,
                            cx + (float)Math.Cos(a) * 7f, cy + (float)Math.Sin(a) * 7f);
                    }
                    break;
                case "Security":      // shield
                    g.DrawArc(p, cx - 5, cy - 7, 10, 10, 180, 180);
                    g.DrawLine(p, cx - 5, cy - 2, cx - 5, cy + 3);
                    g.DrawLine(p, cx + 5, cy - 2, cx + 5, cy + 3);
                    g.DrawLine(p, cx - 5, cy + 3, cx, cy + 7);
                    g.DrawLine(p, cx + 5, cy + 3, cx, cy + 7);
                    break;
            }
        }

        // ─── ShowPage ──────────────────────────────────────────────────────────────
        private void ShowPage(string page)
        {
            _activePage = page;

            // Repaint all nav buttons (they self-draw active state)
            foreach (var b in new[] {
                btnNavOverview, btnNavStatistics, btnNavCustomers,
                btnNavProducts, btnNavTransactions, btnNavMessages,
                btnNavSettings, btnNavSecurity })
                b.Invalidate();

            lblPageTitle.Text = page switch
            {
                "Overview"     => "Dashboard",
                "Statistics"   => "Statistics",
                "NewBill"      => "New Bill",
                "Customers"    => "Customers",
                "Products"     => "Products",
                "Transactions" => "Transactions",
                "Messages"     => "Messages",
                "Settings"     => "Settings",
                "Security"     => "Security",
                _              => page
            };
            lblPageSub.Text = page switch
            {
                "Overview"     => "An easy way to manage your bills with care and precision.",
                "Statistics"   => "View your sales statistics and performance metrics.",
                "NewBill"      => "Create a new invoice for your customer.",
                "Customers"    => "Manage your customer database.",
                "Products"     => "Manage your product catalogue.",
                "Transactions" => "View and search all billing transactions.",
                "Messages"     => "View messages and notifications.",
                _              => ""
            };

            pnlOverview.Visible   = page is "Overview" or "Statistics" or "Messages";
            pnlNewBill.Visible    = page == "NewBill";
            pnlCustomers.Visible  = page == "Customers";
            pnlProducts.Visible   = page == "Products";
            pnlTransPage.Visible  = page == "Transactions";

            Invalidate();
        }


        // ═══════════════════════════════════════════════════════════════════════════
        //  PAGE: OVERVIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildOverviewPage()
        {
            pnlOverview.Dock      = DockStyle.Fill;
            pnlOverview.BackColor = C_PageBg;
            pnlOverview.Visible   = false;

            // ── ROW 1 — Stat cards ────────────────────────────────────────────────
            // Hero card  (dark, full gradient)
            BuildHeroCard();

            // Net Income card
            BuildSmallStatCard(pnlStatIncome,
                "Net Income", "193,000", "+35% from last month",
                C_Blue, true);

            // Total Return card
            BuildSmallStatCard(pnlStatReturn,
                "Total Return", "32,000", "-24% from last month",
                C_Red, false);

            // ── ROW 2 — Body columns ──────────────────────────────────────────────
            // LEFT: Transaction list
            BuildTransactionPanel();

            // CENTRE: Revenue bar chart  +  Sales report
            BuildRevenueChartPanel();
            BuildSalesReportPanel();

            // RIGHT: Total-view donut  +  Level-up promo
            BuildTotalViewPanel();
            BuildLevelUpPanel();

            // Add all to overview
            pnlOverview.Controls.AddRange(new Control[] {
                pnlStatHero, pnlStatIncome, pnlStatReturn,
                pnlTransactions, pnlRevenueCard, pnlSalesReport,
                pnlTotalView, pnlLevelUp
            });

            // Layout is done via Anchor + manual sizing triggered on resize
            pnlOverview.Resize += (s, e) => LayoutOverview();
            LayoutOverview();

            pnlContent.Controls.Add(pnlOverview);
        }

        // Runs on every resize of the overview panel to reposition all sub-panels
        private void LayoutOverview()
        {
            int W  = pnlOverview.ClientSize.Width;
            int gap = 12;

            // ── Stat row ─────────────────────────────────────────────────
            int statH   = 130;
            int heroW   = (int)(W * 0.33);
            int smallW  = (int)((W - heroW - gap * 2) / 2);

            pnlStatHero.SetBounds(0, 0, heroW, statH);
            pnlStatIncome.SetBounds(heroW + gap, 0, smallW, statH);
            pnlStatReturn.SetBounds(heroW + gap + smallW + gap, 0, smallW, statH);

            // ── Body row ─────────────────────────────────────────────────
            int bodyY   = statH + gap;
            int bodyH   = pnlOverview.ClientSize.Height - bodyY - gap;
            int rightW  = 220;
            int leftW   = (int)(W * 0.31);
            int midW    = W - leftW - rightW - gap * 2;

            // Left col: transactions
            pnlTransactions.SetBounds(0, bodyY, leftW, bodyH);

            // Mid col: split into revenue chart (60%) + sales report (38%)
            int midX    = leftW + gap;
            int revH    = (int)(bodyH * 0.56);
            int saleH   = bodyH - revH - gap;
            pnlRevenueCard.SetBounds(midX, bodyY, midW, revH);
            pnlSalesReport.SetBounds(midX, bodyY + revH + gap, midW, saleH);

            // Right col: donut (55%) + promo (43%)
            int rightX  = midX + midW + gap;
            int donutH  = (int)(bodyH * 0.53);
            int promoH  = bodyH - donutH - gap;
            pnlTotalView.SetBounds(rightX, bodyY, rightW, donutH);
            pnlLevelUp.SetBounds(rightX, bodyY + donutH + gap, rightW, promoH);
        }

        // ─── Hero stat card ────────────────────────────────────────────────────────
        private void BuildHeroCard()
        {
            pnlStatHero.BackColor = C_HeroBg;

            pnlStatHero.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var pnl = (Panel)s!;
                int w = pnl.Width, h = pnl.Height;

                // Rounded card
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w-1, h-1, 12);
                using var bg   = new LinearGradientBrush(new Point(0,0), new Point(w,h),
                    Color.FromArgb(30, 41, 59), Color.FromArgb(15, 23, 42));
                g.FillPath(bg, path);

                // Subtle purple glow blob
                using var glowBrush = new PathGradientBrush(path)
                {
                    CenterPoint = new PointF(w * 0.7f, h * 0.3f),
                    CenterColor = Color.FromArgb(40, 124, 58, 237),
                    SurroundColors = new[] { Color.Transparent }
                };
                g.FillPath(glowBrush, path);

                // "Update" pill
                using var pillBrush = new SolidBrush(Color.FromArgb(30, 59, 130, 246));
                var pillRect = new RectangleF(14, 14, 64, 22);
                using var pillPath = GraphicsExtensions.CreateRoundedRectanglePath((int)pillRect.X, (int)pillRect.Y, (int)pillRect.Width, (int)pillRect.Height, 11);
                g.FillPath(pillBrush, pillPath);
                using var pillPen = new Pen(C_Blue, 1f);
                g.DrawPath(pillPen, pillPath);

                using var pf  = new Font("Segoe UI", 8f, FontStyle.Bold);
                using var pb  = new SolidBrush(C_Blue);
                g.DrawString("  Update", pf, pb, 18f, 18f);

                // Date
                using var df  = new Font("Segoe UI", 7.5f);
                using var db  = new SolidBrush(C_NavText);
                g.DrawString("Oct 11th 2026", df, db, 14f, 42f);

                // Main text
                using var tf  = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var tb  = new SolidBrush(Color.White);
                g.DrawString("Sales revenue increased", tf, tb, 14f, 60f);
                using var tf2 = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                g.DrawString("40% in 1 week", tf2, tb, 14f, 80f);

                // "See Statistics" link
                using var lf  = new Font("Segoe UI", 8f);
                using var lb  = new SolidBrush(C_Blue);
                g.DrawString("See Statistics  >", lf, lb, 14f, h - 26f);
            };

            pnlStatHero.Cursor = Cursors.Hand;
            pnlStatHero.Click += (s, e) => ShowPage("Transactions");
        }

        // ─── Small stat card ───────────────────────────────────────────────────────
        private void BuildSmallStatCard(Panel pnl, string title, string value,
                                         string sub, Color accentColor, bool isPositive)
        {
            pnl.BackColor = Color.Transparent;

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                int w = p.Width, h = p.Height;

                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w-1, h-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var border = new Pen(C_Border, 1f);
                g.DrawPath(border, path);

                // Three-dot menu top-right
                using var dotB = new SolidBrush(C_TextLight);
                using var dotF = new Font("Segoe UI", 14f, FontStyle.Bold);
                g.DrawString("···", dotF, dotB, w - 36f, 6f);

                // Title
                using var titleF = new Font("Segoe UI", 8.5f);
                using var titleB = new SolidBrush(C_TextMid);
                g.DrawString(title, titleF, titleB, 16f, 16f);

                // Currency symbol + value
                using var symF = new Font("Segoe UI", 12f, FontStyle.Bold);
                using var symB = new SolidBrush(C_TextDark);
                g.DrawString("Rs", symF, symB, 14f, 38f);
                using var valF = new Font("Segoe UI", 24f, FontStyle.Bold);
                g.DrawString(value, valF, symB, 40f, 30f);

                // Trend arrow + sub text
                string arrow = isPositive ? "▲" : "▼";
                using var trendB = new SolidBrush(accentColor);
                using var trendF = new Font("Segoe UI", 8f, FontStyle.Bold);
                g.DrawString(arrow + " " + sub, trendF, trendB, 14f, h - 28f);
            };
        }

        // ─── Transaction list ──────────────────────────────────────────────────────
        private void BuildTransactionPanel()
        {
            pnlTransactions.BackColor = Color.Transparent;

            pnlTransactions.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, p.Width-1, p.Height-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var border = new Pen(C_Border, 1f);
                g.DrawPath(border, path);

                // Header
                using var hf = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var hb = new SolidBrush(C_TextDark);
                g.DrawString("Transaction", hf, hb, 14f, 14f);

                using var dotF = new Font("Segoe UI", 14f, FontStyle.Bold);
                using var dotB = new SolidBrush(C_TextLight);
                g.DrawString("···", dotF, dotB, p.Width - 36f, 8f);
            };

            // Grid inside
            StyleDataGrid(dgvTransactions);
            dgvTransactions.Dock = DockStyle.Fill;
            dgvTransactions.Margin = new Padding(12, 50, 12, 12);

            dgvTransactions.Columns.AddRange(
                MakeCol("Item",    120, false),
                MakeCol("Date",     80, false),
                MakeCol("Amount",   80, false),
                MakeCol("Status",   80, false)
            );
            LoadTransactionGrid();
            pnlTransactions.Controls.Add(dgvTransactions);
        }

        // ─── Revenue bar chart ─────────────────────────────────────────────────────
        private void BuildRevenueChartPanel()
        {
            pnlRevenueCard.BackColor = Color.Transparent;

            // Chart legend dots
            var legendIncome  = MakeLegendDot(C_BarIncome,  " Income");
            var legendExpense = MakeLegendDot(C_BarExpense, " Expenses");

            pnlRevenueCard.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, p.Width-1, p.Height-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var border = new Pen(C_Border, 1f);
                g.DrawPath(border, path);

                // Title
                using var hf = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var hb = new SolidBrush(C_TextDark);
                g.DrawString("Revenue", hf, hb, 14f, 14f);

                // Revenue big number
                using var vf = new Font("Segoe UI", 20f, FontStyle.Bold);
                g.DrawString("Rs.193,000", vf, hb, 14f, 38f);

                using var sf  = new Font("Segoe UI", 8f, FontStyle.Bold);
                using var sb2 = new SolidBrush(C_Blue);
                g.DrawString("  ▲ +35% from last month", sf, sb2, 180f, 50f);

                // Bars
                DrawBarChart(g, p.Width, p.Height);
            };

            pnlRevenueCard.Controls.AddRange(new Control[] { legendIncome, legendExpense });
            pnlRevenueCard.Resize += (s, e) =>
            {
                var p = (Panel)s!;
                legendIncome.Location  = new Point(p.Width - 140, 16);
                legendExpense.Location = new Point(p.Width - 80,  16);
                pnlRevenueCard.Invalidate();
            };
        }

        private void DrawBarChart(Graphics g, int panelW, int panelH)
        {
            int chartL = 14, chartR = panelW - 14;
            int chartT = 88, chartB = panelH - 30;
            int chartW = chartR - chartL, chartH = chartB - chartT;

            int n = _barIncome.Length;
            float groupW = (float)chartW / n;
            float barW   = groupW * 0.28f;
            float maxH   = 120f;

            using var incBrush = new SolidBrush(C_BarIncome);
            using var expBrush = new SolidBrush(C_BarExpense);
            using var lblFont  = new Font("Segoe UI", 7f);
            using var lblBrush = new SolidBrush(C_TextLight);

            for (int i = 0; i < n; i++)
            {
                float gx = chartL + i * groupW;
                float cx = gx + groupW / 2f;

                float ih = (_barIncome[i]  / maxH) * chartH;
                float eh = (_barExpense[i] / maxH) * chartH;

                // Expense bar (background, lighter)
                float ex = cx - barW - 1f;
                g.FillRectangle(expBrush, ex, chartB - eh, barW, eh);

                // Income bar (blue)
                float ix = cx + 1f;
                // Rounded top for income bar
                using var rp = GraphicsExtensions.CreateRoundedRectanglePath((int)ix, (int)(chartB - ih), (int)barW, (int)ih, 3);
                g.FillPath(incBrush, rp);

                // Month label
                g.DrawString(_barLabels[i], lblFont, lblBrush, cx - 8, chartB + 4);
            }

            // Horizontal grid lines
            using var gridPen = new Pen(_isDarkMode ? Color.FromArgb(30, 255, 255, 255) : Color.FromArgb(15, 0, 0, 0), 1f);
            for (int l = 0; l <= 4; l++)
            {
                float ly = chartB - l * (chartH / 4f);
                g.DrawLine(gridPen, chartL, ly, chartR, ly);
            }
        }

        private static Label MakeLegendDot(Color c, string text)
        {
            return new Label
            {
                AutoSize  = true,
                Text      = "\u25CF" + text,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = c,
                BackColor = Color.Transparent
            };
        }

        // ─── Sales Report panel ────────────────────────────────────────────────────
        private void BuildSalesReportPanel()
        {
            pnlSalesReport.BackColor = Color.Transparent;

            pnlSalesReport.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                int w = p.Width, h = p.Height;

                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w-1, h-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var border = new Pen(C_Border, 1f);
                g.DrawPath(border, path);

                using var hf = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var hb = new SolidBrush(C_TextDark);
                g.DrawString("Sales Report", hf, hb, 14f, 12f);

                // Three horizontal bars
                var bars = new[]
                {
                    ("Product Launched",  233, C_Blue,   (float)0.58),
                    ("Ongoing Product",   233, C_Violet, (float)0.58),
                    ("Product Sold",      482, C_Amber,  (float)1.0 ),
                };

                int barAreaL = 14, barAreaR = w - 14;
                int barAreaW = barAreaR - barAreaL;
                int barH2    = 14;
                int startY   = 40;
                int rowH     = (h - startY - 20) / bars.Length;

                using var lblF = new Font("Segoe UI", 8f);
                using var valF = new Font("Segoe UI", 8f, FontStyle.Bold);

                for (int i = 0; i < bars.Length; i++)
                {
                    var (label, val, clr, ratio) = bars[i];
                    int ry = startY + i * rowH;

                    // Label
                    using var lb2 = new SolidBrush(C_TextMid);
                    g.DrawString(label + "  (" + val + ")", lblF, lb2, barAreaL, ry);

                    // Track
                    int trackY = ry + 20;
                    Color trackClr = _isDarkMode ? Color.FromArgb(35, 38, 56) : Color.FromArgb(20, clr.R, clr.G, clr.B);
                    using var trackB    = new SolidBrush(trackClr);
                    using var trackPath = GraphicsExtensions.CreateRoundedRectanglePath(barAreaL, trackY, barAreaW, barH2, barH2 / 2);
                    g.FillPath(trackB, trackPath);

                    // Fill
                    int fillW = (int)(barAreaW * ratio);
                    using var fillPath = GraphicsExtensions.CreateRoundedRectanglePath(barAreaL, trackY, fillW, barH2, barH2/2);
                    using var fillB    = new LinearGradientBrush(
                        new Point(barAreaL, trackY), new Point(barAreaL + fillW, trackY),
                        clr, Color.FromArgb(200, clr.R, clr.G, clr.B));
                    g.FillPath(fillB, fillPath);
                }

                // X-axis numbers
                using var axF = new Font("Segoe UI", 7f);
                using var axB = new SolidBrush(C_TextLight);
                for (int n = 0; n <= 4; n++)
                {
                    int nx = barAreaL + n * barAreaW / 4;
                    g.DrawString((n * 100).ToString(), axF, axB, nx - 8, h - 20);
                }
            };
        }

        // ─── Total-view donut ──────────────────────────────────────────────────────
        private void BuildTotalViewPanel()
        {
            pnlTotalView.BackColor = Color.Transparent;

            pnlTotalView.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                int w = p.Width, h = p.Height;

                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w-1, h-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var border = new Pen(C_Border, 1f);
                g.DrawPath(border, path);

                using var hf = new Font("Segoe UI", 10f, FontStyle.Bold);
                using var hb = new SolidBrush(C_TextDark);
                g.DrawString("Total View Performance", hf, hb, 12f, 12f);

                // Donut chart
                int donutSize = Math.Min(w - 30, 120);
                int donutX = (w - donutSize) / 2;
                int donutY = 38;
                int thick  = 18;

                float[] angles  = { 57.6f, 82.8f, 244.8f };       // 16%, 23%, 68%
                Color[] clrs    = { C_Blue, C_Violet, C_Amber };
                float   startA  = -90f;

                for (int i = 0; i < angles.Length; i++)
                {
                    using var arcPen = new Pen(clrs[i], thick) { EndCap = LineCap.Round, StartCap = LineCap.Round };
                    g.DrawArc(arcPen, donutX + thick/2, donutY + thick/2,
                              donutSize - thick, donutSize - thick, startA, angles[i]);
                    startA += angles[i] + 4;
                }

                // Centre text
                int cx = w / 2, cy = donutY + donutSize / 2 + 2;
                using var ctF = new Font("Segoe UI", 7.5f);
                using var ctB = new SolidBrush(C_TextLight);
                g.DrawString("Total Count", ctF, ctB, cx - 30, cy - 18);
                using var cvF = new Font("Segoe UI", 13f, FontStyle.Bold);
                using var cvB = new SolidBrush(C_TextDark);
                g.DrawString("565K", cvF, cvB, cx - 20, cy - 4);

                // Legend
                string[] legends = { "16%", "23%", "68%" };
                string[] lnames  = { "View Count", "Percentage", "Sales" };
                int ly = donutY + donutSize + 10;
                using var legF = new Font("Segoe UI", 7.5f);
                for (int i = 0; i < 3; i++)
                {
                    int lx = 12 + i * (w / 3);
                    using var dotB = new SolidBrush(clrs[i]);
                    g.FillEllipse(dotB, lx, ly + 4, 7, 7);
                    using var legB = new SolidBrush(C_TextMid);
                    g.DrawString(lnames[i], legF, legB, lx + 10, ly);
                }

                // Helper text
                int tipY = donutY + donutSize + 36;
                if (tipY < h - 46)
                {
                    using var tipF = new Font("Segoe UI", 8f);
                    using var tipB = new SolidBrush(C_TextLight);
                    g.DrawString("Here are some tips on how to\nimprove your score.", tipF, tipB, 12f, tipY);
                }

                // Guide Views button (painted)
                int btnY = h - 44;
                using var btnRect = GraphicsExtensions.CreateRoundedRectanglePath(12, btnY, w - 24, 32, 8);
                using var btnBrush = new SolidBrush(_isDarkMode ? Color.FromArgb(30, 42, 68) : Color.FromArgb(239, 246, 255));
                g.FillPath(btnBrush, btnRect);
                using var btnPen = new Pen(C_Blue, 1f);
                g.DrawPath(btnPen, btnRect);
                using var btnF = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                using var btnB = new SolidBrush(_isDarkMode ? Color.FromArgb(147, 197, 253) : C_Blue);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Guide Views", btnF, btnB, new RectangleF(12, btnY, w - 24, 32), sf);
            };
        }

        // ─── Level-up promo ────────────────────────────────────────────────────────
        private void BuildLevelUpPanel()
        {
            pnlLevelUp.BackColor = Color.FromArgb(30, 41, 59);

            pnlLevelUp.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var p = (Panel)s!;
                int w = p.Width, h = p.Height;

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w-1, h-1, 12);
                using var bg   = new LinearGradientBrush(
                    new Point(0, 0), new Point(w, h),
                    Color.FromArgb(30, 41, 59), Color.FromArgb(15, 23, 42));
                g.FillPath(bg, path);

                // Violet glow spot
                using var glowPath = GraphicsExtensions.CreateRoundedRectanglePath(0,0,w-1,h-1,12);
                using var glow     = new PathGradientBrush(glowPath)
                {
                    CenterPoint    = new PointF(w * 0.8f, h * 0.2f),
                    CenterColor    = Color.FromArgb(55, C_Violet.R, C_Violet.G, C_Violet.B),
                    SurroundColors = new[] { Color.Transparent }
                };
                g.FillPath(glow, glowPath);

                using var wb = new SolidBrush(Color.White);

                // Headline
                using var h1f = new Font("Segoe UI", 10f, FontStyle.Bold);
                g.DrawString("Level up your sales", h1f, wb, 12f, 14f);
                using var h2f = new Font("Segoe UI", 10f, FontStyle.Bold);
                g.DrawString("managing to the", h2f, wb, 12f, 34f);
                g.DrawString("next level.", h2f, wb, 12f, 54f);

                using var sf = new Font("Segoe UI", 7.5f);
                using var sb2 = new SolidBrush(Color.FromArgb(148, 163, 184));
                g.DrawString("An easy way to manage sales with\ncare and precision.", sf, sb2, 12f, 78f);

                // CTA button
                int btnY = h - 44;
                using var btnRect  = GraphicsExtensions.CreateRoundedRectanglePath(12, btnY, w - 24, 32, 8);
                using var btnBrush = new LinearGradientBrush(
                    new Point(12, btnY), new Point(w - 12, btnY + 32), C_Blue, C_Violet);
                g.FillPath(btnBrush, btnRect);
                using var ctaF = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                var sff = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("Update to SB1+", ctaF, wb, new RectangleF(12, btnY, w - 24, 32), sff);
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PAGE: NEW BILL
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildNewBillPage()
        {
            pnlNewBill.Dock      = DockStyle.Fill;
            pnlNewBill.BackColor = C_PageBg;
            pnlNewBill.Visible   = false;

            // Wrap in a white card
            var card = MakeCard(24, 16, 700, 560);
            pnlNewBill.Controls.Add(card);
            pnlNewBill.Resize += (s, e) => { card.Width = Math.Min(700, pnlNewBill.Width - 48); };

            int lx = 0, ly = 0, lw = card.Width - 0, inputH = 38;

            AddSectionLabel(card, "Customer Info", lx, ly); ly += 32;
            StyleInput(txtBillCustomerName, "Customer Name",  lx,           ly, lw/2 - 8, inputH);
            StyleInput(txtBillPhone,        "Phone Number",   lx + lw/2 + 8, ly, lw/2 - 8, inputH);
            card.Controls.AddRange(new Control[] { txtBillCustomerName, txtBillPhone });
            ly += inputH + 20;

            AddSectionLabel(card, "Add Item", lx, ly); ly += 32;
            StyleInput(txtBillItemName, "Item Name",   lx,       ly, 190, inputH);
            StyleInput(txtBillQty,      "Qty",         lx + 198, ly, 80,  inputH);
            StyleInput(txtBillPrice,    "Unit Price",  lx + 286, ly, 120, inputH);
            card.Controls.AddRange(new Control[] { txtBillItemName, txtBillQty, txtBillPrice });

            StyleBtn(btnAddLineItem, "+ Add", C_Blue, lx + 414, ly, 90, inputH);
            btnAddLineItem.Click += BtnAddLine_Click;
            card.Controls.Add(btnAddLineItem);
            ly += inputH + 14;

            StyleDataGrid(dgvBillItems);
            dgvBillItems.Location = new Point(lx, ly);
            dgvBillItems.Size     = new Size(lw, 170);
            dgvBillItems.Anchor   = AnchorStyles.None;
            dgvBillItems.Columns.AddRange(
                MakeCol("Item",  200, false), MakeCol("Qty", 70, false),
                MakeCol("Unit",  100, false), MakeCol("Total",100, false));
            card.Controls.Add(dgvBillItems);
            ly += 184;

            lblBillTotal.AutoSize  = false;
            lblBillTotal.Size      = new Size(lw, 34);
            lblBillTotal.Location  = new Point(lx, ly);
            lblBillTotal.Text      = "Total:  Rs. 0.00";
            lblBillTotal.Font      = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblBillTotal.ForeColor = C_Blue;
            lblBillTotal.TextAlign = ContentAlignment.MiddleRight;
            card.Controls.Add(lblBillTotal);
            ly += 42;

            StyleBtn(btnSaveBill,  "Save Bill", C_Blue, lx,       ly, 130, 40);
            StyleBtn(btnClearBill, "Clear",     C_Red,  lx + 138, ly, 90,  40);
            btnSaveBill.Click  += BtnSaveBill_Click;
            btnClearBill.Click += BtnClearBill_Click;
            card.Controls.AddRange(new Control[] { btnSaveBill, btnClearBill });

            pnlContent.Controls.Add(pnlNewBill);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PAGE: CUSTOMERS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCustomersPage()
        {
            pnlCustomers.Dock      = DockStyle.Fill;
            pnlCustomers.BackColor = C_PageBg;
            pnlCustomers.Visible   = false;

            var card = MakeCard(24, 16, 800, 520);
            pnlCustomers.Controls.Add(card);
            pnlCustomers.Resize += (s,e) => { card.Width = pnlCustomers.Width - 48; card.Height = pnlCustomers.Height - 32; };

            StyleInput(txtCustSearch, "Search customers...", 0, 0, 300, 38);
            txtCustSearch.TextChanged += (s,e) => FilterCust();
            card.Controls.Add(txtCustSearch);

            StyleBtn(btnAddCustomer, "+ Add Customer", C_Blue, 316, 0, 150, 38);
            btnAddCustomer.Click += BtnAddCustomer_Click;
            card.Controls.Add(btnAddCustomer);

            StyleDataGrid(dgvCustomers);
            dgvCustomers.Location = new Point(0, 52);
            dgvCustomers.Anchor   = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom;
            dgvCustomers.Columns.AddRange(
                MakeCol("Name",200,false), MakeCol("Phone",130,false),
                MakeCol("Email",200,false), MakeCol("Bills",80,false), MakeCol("Total Spent",130,false));
            LoadCustGrid();
            card.Controls.Add(dgvCustomers);
            pnlContent.Controls.Add(pnlCustomers);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PAGE: PRODUCTS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildProductsPage()
        {
            pnlProducts.Dock      = DockStyle.Fill;
            pnlProducts.BackColor = C_PageBg;
            pnlProducts.Visible   = false;

            var card = MakeCard(24, 16, 800, 520);
            pnlProducts.Controls.Add(card);
            pnlProducts.Resize += (s,e) => { card.Width = pnlProducts.Width - 48; card.Height = pnlProducts.Height - 32; };

            StyleInput(txtProdSearch, "Search products...", 0, 0, 300, 38);
            txtProdSearch.TextChanged += (s,e) => FilterProd();
            card.Controls.Add(txtProdSearch);

            StyleBtn(btnAddProductPage, "+ Add Product", C_Blue, 316, 0, 150, 38);
            btnAddProductPage.Click += BtnAddProduct_Click;
            card.Controls.Add(btnAddProductPage);

            StyleDataGrid(dgvProducts);
            dgvProducts.Location = new Point(0, 52);
            dgvProducts.Anchor   = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom;
            dgvProducts.Columns.AddRange(
                MakeCol("Product Name",240,false), MakeCol("Category",140,false),
                MakeCol("Unit",80,false), MakeCol("Price",100,false), MakeCol("Stock",80,false));
            LoadProdGrid();
            card.Controls.Add(dgvProducts);
            pnlContent.Controls.Add(pnlProducts);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PAGE: TRANSACTIONS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildTransactionsPage()
        {
            pnlTransPage.Dock      = DockStyle.Fill;
            pnlTransPage.BackColor = C_PageBg;
            pnlTransPage.Visible   = false;

            var card = MakeCard(24, 16, 900, 520);
            pnlTransPage.Controls.Add(card);
            pnlTransPage.Resize += (s,e) => { card.Width = pnlTransPage.Width - 48; card.Height = pnlTransPage.Height - 32; };

            StyleInput(txtTransSearch, "Search transactions...", 0, 0, 360, 38);
            txtTransSearch.TextChanged += (s,e) => FilterTrans();
            card.Controls.Add(txtTransSearch);

            StyleDataGrid(dgvAllTransactions);
            dgvAllTransactions.Location = new Point(0, 52);
            dgvAllTransactions.Anchor   = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom;
            dgvAllTransactions.Columns.AddRange(
                MakeCol("Bill #",90,false), MakeCol("Customer",180,false),
                MakeCol("Phone",130,false),  MakeCol("Date",120,false),
                MakeCol("Items",60,false),   MakeCol("Amount",120,false),
                MakeCol("Status",100,false));
            LoadTransPage();
            card.Controls.Add(dgvAllTransactions);
            pnlContent.Controls.Add(pnlTransPage);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BILLING EVENTS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BtnAddLine_Click(object? sender, EventArgs e)
        {
            string item = txtBillItemName.Text.Trim();
            if (string.IsNullOrEmpty(item))                                    { Tip("Item name khali nahi ho sakta!"); return; }
            if (!int.TryParse(txtBillQty.Text.Trim(), out int qty) || qty<=0)  { Tip("Valid quantity daalo!");         return; }
            if (!decimal.TryParse(txtBillPrice.Text.Trim(), out decimal price)) { Tip("Valid price daalo!");           return; }

            _lineItems.Add((item, qty, price));
            decimal tot = qty * price;
            _billTotal += tot;
            dgvBillItems.Rows.Add(item, qty, $"Rs.{price:F2}", $"Rs.{tot:F2}");
            lblBillTotal.Text = $"Total:  Rs. {_billTotal:F2}";
            txtBillItemName.Clear(); txtBillQty.Clear(); txtBillPrice.Clear();
            txtBillItemName.Focus();
        }

        private void BtnSaveBill_Click(object? sender, EventArgs e)
        {
            string cust = txtBillCustomerName.Text.Trim();
            if (string.IsNullOrEmpty(cust))   { Tip("Customer name daalo!"); return; }
            if (_lineItems.Count == 0)         { Tip("Koi item add karo!");   return; }

            string no   = $"B-{DateTime.Now:yyMMddHHmm}";
            string date = DateTime.Now.ToString("dd MMM yyyy");
            string ph   = txtBillPhone.Text.Trim();

            _billsStore.Insert(0, new[] { no, cust, ph, date, _lineItems.Count.ToString(), $"Rs.{_billTotal:F2}", "Paid" });
            if (!_customersStore.Exists(c => c[0].Equals(cust, StringComparison.OrdinalIgnoreCase)))
                _customersStore.Add(new[] { cust, ph, "", "1", $"Rs.{_billTotal:F2}" });

            MessageBox.Show($"Bill {no} save ho gaya!\nCustomer: {cust}\nTotal: Rs.{_billTotal:F2}",
                "Bill Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

            BtnClearBill_Click(null, EventArgs.Empty);
            LoadTransactionGrid(); LoadTransPage(); LoadCustGrid();
            ShowPage("Transactions");
        }

        private void BtnClearBill_Click(object? sender, EventArgs e)
        {
            _lineItems.Clear(); _billTotal = 0m;
            dgvBillItems.Rows.Clear();
            lblBillTotal.Text = "Total:  Rs. 0.00";
            txtBillCustomerName.Clear(); txtBillPhone.Clear();
            txtBillItemName.Clear(); txtBillQty.Clear(); txtBillPrice.Clear();
        }

        private void BtnAddCustomer_Click(object? sender, EventArgs e)
        {
            ShowAddDialog("Add Customer",
                new[] { "Name", "Phone", "Email" },
                vals => { _customersStore.Add(new[] { vals[0], vals[1], vals[2], "0", "Rs.0.00" }); LoadCustGrid(); });
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            ShowAddDialog("Add Product",
                new[] { "Product Name", "Category", "Price", "Stock" },
                vals => { _productsStore.Add(new[] { vals[0], vals[1], "Pc", vals[2], vals[3] }); LoadProdGrid(); });
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  GRID LOADERS / FILTERS
        // ═══════════════════════════════════════════════════════════════════════════
        private void LoadTransactionGrid()
        {
            dgvTransactions.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src) dgvTransactions.Rows.Add(b[1], b[3], b[5], b[6]);
        }

        private void LoadTransPage()
        {
            dgvAllTransactions.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src) dgvAllTransactions.Rows.Add(b[0],b[1],b[2],b[3],b[4],b[5],b[6]);
        }

        private void LoadCustGrid()
        {
            dgvCustomers.Rows.Clear();
            var src = _customersStore.Count > 0 ? _customersStore : GetSampleCust();
            foreach (var c in src) dgvCustomers.Rows.Add(c);
        }

        private void LoadProdGrid()
        {
            dgvProducts.Rows.Clear();
            foreach (var p in _productsStore) dgvProducts.Rows.Add(p);
        }

        private void FilterTrans()
        {
            string q = txtTransSearch.Text.Trim().ToLower();
            dgvAllTransactions.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src)
                if (string.IsNullOrEmpty(q)||b[0].ToLower().Contains(q)||b[1].ToLower().Contains(q))
                    dgvAllTransactions.Rows.Add(b[0],b[1],b[2],b[3],b[4],b[5],b[6]);
        }

        private void FilterCust()
        {
            string q = txtCustSearch.Text.Trim().ToLower();
            dgvCustomers.Rows.Clear();
            var src = _customersStore.Count > 0 ? _customersStore : GetSampleCust();
            foreach (var c in src)
                if (string.IsNullOrEmpty(q)||c[0].ToLower().Contains(q)) dgvCustomers.Rows.Add(c);
        }

        private void FilterProd()
        {
            string q = txtProdSearch.Text.Trim().ToLower();
            dgvProducts.Rows.Clear();
            foreach (var p in _productsStore)
                if (string.IsNullOrEmpty(q)||p[0].ToLower().Contains(q)) dgvProducts.Rows.Add(p);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SAMPLE DATA
        // ═══════════════════════════════════════════════════════════════════════════
        private void PopulateSampleData()
        {
            _productsStore.AddRange(new[]
            {
                new[]{"A4 Paper","Stationery","Ream","220","85"},
                new[]{"Pen (Blue)","Stationery","Piece","10","500"},
                new[]{"Laptop Stand","Electronics","Piece","850","20"},
                new[]{"Mouse Pad","Electronics","Piece","150","60"},
                new[]{"Stapler","Stationery","Piece","180","35"},
            });
        }

        private static List<string[]> GetSampleBills() => new()
        {
            new[]{"B-2610110001","Amit Sharma",  "9876543210","11 Oct 2026","3","Rs.1,250","Completed"},
            new[]{"B-2610100002","Priya Patel",  "9123456780","10 Oct 2026","5","Rs.4,800","Completed"},
            new[]{"B-2610090003","Rahul Verma",  "9988776655","09 Oct 2026","2","Rs.620",  "Pending"},
            new[]{"B-2610090004","Sneha Gupta",  "9001122334","09 Oct 2026","7","Rs.9,100","Completed"},
            new[]{"B-2610080005","Kiran Joshi",  "9876001234","08 Oct 2026","1","Rs.350",  "Pending"},
            new[]{"B-2610080006","Vikram Singh", "9090909090","08 Oct 2026","4","Rs.2,200","Completed"},
        };

        private static List<string[]> GetSampleCust() => new()
        {
            new[]{"Amit Sharma", "9876543210","amit@email.com",  "8", "Rs.22,400"},
            new[]{"Priya Patel", "9123456780","priya@email.com","12","Rs.48,200"},
            new[]{"Rahul Verma", "9988776655","rahul@email.com", "5","Rs.11,300"},
            new[]{"Sneha Gupta", "9001122334","sneha@email.com","19","Rs.86,700"},
            new[]{"Kiran Joshi", "9876001234","kiran@email.com", "3", "Rs.4,200"},
        };

        // ═══════════════════════════════════════════════════════════════════════════
        //  UI STYLE HELPERS
        // ═══════════════════════════════════════════════════════════════════════════
        private Panel MakeCard(int x, int y, int w, int h)
        {
            var c = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                BackColor = Color.Transparent,
                Padding   = new Padding(16, 14, 16, 14),
            };
            c.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path   = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, c.Width-1, c.Height-1, 12);
                using var bg     = new SolidBrush(C_CardBg);
                g.FillPath(bg, path);
                using var pen    = new Pen(C_Border, 1f);
                g.DrawPath(pen, path);
            };
            return c;
        }

        private void StyleDataGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode              = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible                = false;
            dgv.AllowUserToAddRows               = false;
            dgv.ReadOnly                         = true;
            dgv.SelectionMode                    = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect                      = false;
            dgv.BorderStyle                      = BorderStyle.None;
            dgv.BackgroundColor                  = C_CardBg;
            dgv.GridColor                        = C_Border;
            dgv.CellBorderStyle                  = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersDefaultCellStyle    = new DataGridViewCellStyle
            {
                BackColor = _isDarkMode ? Color.FromArgb(28, 30, 44) : Color.FromArgb(248, 250, 252),
                ForeColor = _isDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Padding   = new Padding(8,0,0,0),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };
            dgv.ColumnHeadersHeight              = 34;
            dgv.ColumnHeadersBorderStyle         = DataGridViewHeaderBorderStyle.Single;
            dgv.EnableHeadersVisualStyles        = false;
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = C_CardBg,
                ForeColor          = C_TextDark,
                Font               = new Font("Segoe UI", 9f),
                Padding            = new Padding(8,0,0,0),
                SelectionBackColor = Color.FromArgb(37, 99, 235),
                SelectionForeColor = Color.White
            };
            dgv.RowTemplate.Height               = 36;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = _isDarkMode ? Color.FromArgb(26, 27, 40) : Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = C_TextDark;
        }

        private void StyleInput(TextBox txt, string ph, int x, int y, int w, int h)
        {
            txt.Location        = new Point(x, y);
            txt.Size            = new Size(w, h);
            txt.Font            = new Font("Segoe UI", 10f);
            txt.ForeColor       = C_TextDark;
            txt.BackColor       = C_SearchBg;
            txt.BorderStyle     = BorderStyle.FixedSingle;
            txt.PlaceholderText = ph;
        }

        private static void StyleBtn(Button btn, string text, Color bg, int x, int y, int w, int h)
        {
            btn.Text      = text;
            btn.Location  = new Point(x, y);
            btn.Size      = new Size(w, h);
            btn.Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.ForeColor = Color.White;
            btn.BackColor = bg;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor    = Cursors.Hand;
        }

        private static DataGridViewTextBoxColumn MakeCol(string h, int w, bool fill) =>
            new DataGridViewTextBoxColumn
            {
                HeaderText   = h,
                Width        = w,
                AutoSizeMode = fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = { Padding = new Padding(8,0,0,0) }
            };

        private void AddSectionLabel(Panel p, string text, int x, int y)
        {
            p.Controls.Add(new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = C_TextMid,
                AutoSize  = true,
                Location  = new Point(x, y)
            });
        }

        private static void Tip(string msg) =>
            MessageBox.Show(msg, "SB1", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static void ShowAddDialog(string title, string[] fields, Action<string[]> onOk)
        {
            using var f = new Form { Text = title, Size = new Size(340, fields.Length * 44 + 90),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog };
            var boxes = new TextBox[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                boxes[i] = new TextBox { PlaceholderText = fields[i],
                    Location = new Point(16, 14 + i * 44), Size = new Size(294, 32),
                    Font = new Font("Segoe UI", 10f) };
                f.Controls.Add(boxes[i]);
            }
            var ok  = new Button { Text="Add",    DialogResult=DialogResult.OK,    Location=new Point(90, fields.Length*44+18), Size=new Size(80,32), BackColor=Color.FromArgb(59,130,246), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            var can = new Button { Text="Cancel", DialogResult=DialogResult.Cancel, Location=new Point(180,fields.Length*44+18), Size=new Size(80,32) };
            ok.FlatAppearance.BorderSize = 0;
            f.Controls.AddRange(new Control[]{ok,can});
            f.AcceptButton=ok; f.CancelButton=can;
            if (f.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(boxes[0].Text.Trim()))
                onOk(Array.ConvertAll(boxes, b => b.Text.Trim()));
        }
    }
}
