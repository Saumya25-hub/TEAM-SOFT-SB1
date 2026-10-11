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
    /// Premium WinForms Dashboard with Left Sidebar Navigation, KPI Summary Cards,
    /// New Bill Form, Bills History, Customers, and Products pages.
    /// Matches the white + blue-purple gradient theme of Splash &amp; Login screens.
    /// </summary>
    public partial class DashboardForm : Form
    {
        // ── Colour palette (matches Splash/Login) ──────────────────────────────────
        private static readonly Color C_Primary   = Color.FromArgb(59, 130, 246);
        private static readonly Color C_Secondary = Color.FromArgb(124, 58, 237);
        private static readonly Color C_Bg        = Color.FromArgb(248, 250, 252);
        private static readonly Color C_Surface   = Color.White;
        private static readonly Color C_Border    = Color.FromArgb(226, 232, 240);
        private static readonly Color C_TextDark  = Color.FromArgb(15, 23, 42);
        private static readonly Color C_TextMid   = Color.FromArgb(71, 85, 105);
        private static readonly Color C_TextLight = Color.FromArgb(148, 163, 184);
        private static readonly Color C_Sidebar   = Color.FromArgb(15, 23, 42);
        private static readonly Color C_NavActive = Color.FromArgb(30, 58, 138);
        private static readonly Color C_NavHover  = Color.FromArgb(23, 37, 84);

        private static readonly Color[] CardColors =
        {
            Color.FromArgb(59,  130, 246),
            Color.FromArgb(16,  185, 129),
            Color.FromArgb(245, 158,  11),
            Color.FromArgb(124,  58, 237),
        };

        // ── State ──────────────────────────────────────────────────────────────────
        private string _activePage = "Home";

        private readonly List<(string Item, int Qty, decimal Price)> _lineItems = new();
        private decimal _billTotal = 0m;

        private readonly List<string[]> _billsStore     = new();
        private readonly List<string[]> _customersStore = new();
        private readonly List<string[]> _productsStore  = new();

        private bool  _isDragging;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        // ── Constructor ────────────────────────────────────────────────────────────
        public DashboardForm()
        {
            InitializeComponent();
            PopulateSampleData();
            BuildHomePageUI();
            BuildNewBillPageUI();
            BuildBillsPageUI();
            BuildCustomersPageUI();
            BuildProductsPageUI();
            SetupSidebarNavButtons();
            SetupWindowDrag();
            ShowPage("Home");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Overrides
        // ══════════════════════════════════════════════════════════════════════════
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedRegion(18);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionChromeButtons();
            if (WindowState != FormWindowState.Normal)
                Region = null;
            else
                ApplyRoundedRegion(18);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var pen = new Pen(C_Border, 1.5f);
            g.DrawRoundedRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1, 18);

            DrawSidebarLogo(g);

            using var sepPen = new Pen(C_Border, 1f);
            int topBarH = pnlTopBar.Height;
            int sbW     = pnlSidebar.Width;
            g.DrawLine(sepPen, sbW, topBarH, ClientSize.Width, topBarH);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Setup helpers
        // ══════════════════════════════════════════════════════════════════════════
        private void ApplyRoundedRegion(int r)
        {
            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, Width, Height, r);
            Region = new Region(path);
        }

        private void RepositionChromeButtons()
        {
            int top = 13;
            btnClose.Location    = new Point(ClientSize.Width - 44,  top);
            btnMaximize.Location = new Point(ClientSize.Width - 82,  top);
            btnMinimize.Location = new Point(ClientSize.Width - 120, top);
        }

        private void SetupWindowDrag()
        {
            pnlTopBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging      = true;
                    _dragCursorPoint = Cursor.Position;
                    _dragFormPoint   = Location;
                }
            };
            pnlTopBar.MouseMove += (s, e) =>
            {
                if (_isDragging)
                {
                    var d = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
                    Location = Point.Add(_dragFormPoint, new Size(d));
                }
            };
            pnlTopBar.MouseUp += (s, e) => _isDragging = false;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Sidebar logo painter
        // ══════════════════════════════════════════════════════════════════════════
        private void DrawSidebarLogo(Graphics g)
        {
            int cx = pnlSidebar.Width / 2;
            int cy = 54;
            int r  = 26;

            using var grad = new LinearGradientBrush(
                new Point(cx - r, cy - r), new Point(cx + r, cy + r),
                C_Primary, C_Secondary);
            g.FillEllipse(grad, cx - r, cy - r, r * 2, r * 2);

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var monFont   = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var whiteBrush = new SolidBrush(Color.White);
            g.DrawString("SB1", monFont, whiteBrush, cx, cy, sf);

            var sf2 = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            using var nameFont  = new Font("Segoe UI", 11f, FontStyle.Bold);
            g.DrawString("Simple Billing", nameFont, whiteBrush, cx, 88f, sf2);

            using var tagFont  = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            using var tagBrush = new SolidBrush(C_TextLight);
            g.DrawString("SB1 · v1.0", tagFont, tagBrush, cx, 106f, sf2);

            using var divPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1f);
            g.DrawLine(divPen, 16, 128, pnlSidebar.Width - 16, 128);

            // User avatar at bottom-left of sidebar
            int ay = ClientSize.Height - 58;
            int ar = 18;
            int ax = 28;
            using var avatarGrad = new LinearGradientBrush(
                new Point(ax - ar, ay - ar), new Point(ax + ar, ay + ar),
                C_Primary, C_Secondary);
            g.FillEllipse(avatarGrad, ax - ar, ay - ar, ar * 2, ar * 2);
            using var avFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.DrawString("J", avFont, whiteBrush, ax, ay, sf);

            lblSidebarUser.Location = new Point(52, ay - 14);
            lblSidebarRole.Location = new Point(52, ay + 4);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Sidebar navigation
        // ══════════════════════════════════════════════════════════════════════════
        private void SetupSidebarNavButtons()
        {
            var navItems = new[]
            {
                ("Home",      "⌂  Dashboard",  btnNavHome),
                ("NewBill",   "+  New Bill",    btnNavNewBill),
                ("Bills",     "☰  Bills",       btnNavBills),
                ("Customers", "✦  Customers",   btnNavCustomers),
                ("Products",  "◈  Products",    btnNavProducts),
                ("Reports",   "▣  Reports",     btnNavReports),
                ("Settings",  "⚙  Settings",    btnNavSettings),
            };

            int startY = 140;
            int gap    = 4;
            int idx    = 0;

            foreach (var (page, label, btn) in navItems)
            {
                StyleNavButton(btn, label);
                btn.Location = new Point(8, startY + idx * (46 + gap));
                btn.Size     = new Size(204, 46);
                btn.Tag      = page;
                btn.Click   += NavButton_Click;
                idx++;
            }

            // Pin Settings to bottom
            btnNavSettings.Location = new Point(8, pnlSidebar.Height - 118);
        }

        private static void StyleNavButton(Button btn, string text)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor  = Color.FromArgb(23, 37, 84);
            btn.FlatAppearance.MouseDownBackColor  = Color.FromArgb(30, 58, 138);
            btn.BackColor  = Color.Transparent;
            btn.ForeColor  = Color.FromArgb(148, 163, 184);
            btn.Font       = new Font("Segoe UI", 10f, FontStyle.Regular);
            btn.Text       = text;
            btn.TextAlign  = ContentAlignment.MiddleLeft;
            btn.Padding    = new Padding(16, 0, 0, 0);
            btn.Cursor     = Cursors.Hand;
        }

        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string page)
                ShowPage(page);
        }

        private void ShowPage(string page)
        {
            _activePage = page;

            foreach (var btn in new[] { btnNavHome, btnNavNewBill, btnNavBills,
                btnNavCustomers, btnNavProducts, btnNavReports, btnNavSettings })
            {
                btn.ForeColor  = Color.FromArgb(148, 163, 184);
                btn.BackColor  = Color.Transparent;
                btn.Font       = new Font("Segoe UI", 10f, FontStyle.Regular);
            }

            Button? active = page switch
            {
                "Home"      => btnNavHome,
                "NewBill"   => btnNavNewBill,
                "Bills"     => btnNavBills,
                "Customers" => btnNavCustomers,
                "Products"  => btnNavProducts,
                "Reports"   => btnNavReports,
                "Settings"  => btnNavSettings,
                _           => null
            };
            if (active != null)
            {
                active.ForeColor = Color.White;
                active.BackColor = Color.FromArgb(30, 58, 138);
                active.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
            }

            lblPageTitle.Text = page switch
            {
                "Home"      => "Dashboard",
                "NewBill"   => "New Bill",
                "Bills"     => "Bills History",
                "Customers" => "Customers",
                "Products"  => "Products",
                "Reports"   => "Reports",
                "Settings"  => "Settings",
                _           => page
            };

            pnlHome.Visible      = page == "Home";
            pnlNewBill.Visible   = page == "NewBill";
            pnlBills.Visible     = page == "Bills";
            pnlCustomers.Visible = page == "Customers";
            pnlProducts.Visible  = page == "Products";

            Invalidate();
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Home page
        // ══════════════════════════════════════════════════════════════════════════
        private void BuildHomePageUI()
        {
            pnlHome.Dock      = DockStyle.Fill;
            pnlHome.BackColor = C_Bg;
            pnlHome.Padding   = new Padding(4);
            pnlHome.Visible   = false;

            var cardDefs = new[]
            {
                (pnlCardTotalSales,       "Total Sales",       "Rs. 1,24,800",  "+12% this month", 0),
                (pnlCardTodayRevenue,     "Today's Revenue",   "Rs. 8,450",     "+5 bills today",  1),
                (pnlCardPendingBills,     "Pending Bills",     "7",             "Rs.18,200 unpaid",2),
                (pnlCardTotalCustomers,   "Total Customers",   "143",           "+3 this week",    3),
            };

            int cardW = 240, cardH = 118, cardGap = 16;
            int cardX = 0;

            foreach (var (pnl, title, value, sub, colorIdx) in cardDefs)
            {
                BuildKpiCard(pnl, title, value, sub, CardColors[colorIdx], cardX, 0, cardW, cardH);
                pnlHome.Controls.Add(pnl);
                cardX += cardW + cardGap;
            }

            btnQuickNewBill.Text      = "+  Create New Bill";
            btnQuickNewBill.Size      = new Size(180, 42);
            btnQuickNewBill.Location  = new Point(0, cardH + 16);
            btnQuickNewBill.Font      = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnQuickNewBill.ForeColor = Color.White;
            btnQuickNewBill.BackColor = C_Primary;
            btnQuickNewBill.FlatStyle = FlatStyle.Flat;
            btnQuickNewBill.FlatAppearance.BorderSize = 0;
            btnQuickNewBill.Cursor    = Cursors.Hand;
            btnQuickNewBill.Click    += (s, e) => ShowPage("NewBill");
            pnlHome.Controls.Add(btnQuickNewBill);

            var lblRecent = new Label
            {
                Text      = "Recent Bills",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_TextDark,
                AutoSize  = true,
                Location  = new Point(0, cardH + 68),
            };
            pnlHome.Controls.Add(lblRecent);

            StyleDataGrid(dgvRecentBills);
            dgvRecentBills.Location = new Point(0, cardH + 96);
            dgvRecentBills.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvRecentBills.Columns.AddRange(
                MakeCol("Bill #",       80,  false),
                MakeCol("Customer",     170, false),
                MakeCol("Items",        80,  false),
                MakeCol("Date",         120, false),
                MakeCol("Amount",       120, false),
                MakeCol("Status",       90,  false)
            );
            LoadRecentBillsGrid();
            pnlHome.Controls.Add(dgvRecentBills);

            pnlContent.Controls.Add(pnlHome);
        }

        private void BuildKpiCard(Panel pnl, string title, string value, string sub,
                                   Color accent, int x, int y, int w, int h)
        {
            pnl.Location  = new Point(x, y);
            pnl.Size      = new Size(w, h);
            pnl.BackColor = C_Surface;

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using var cardPath  = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnl.Width - 1, pnl.Height - 1, 12);
                using var cardBrush = new SolidBrush(C_Surface);
                g.FillPath(cardBrush, cardPath);

                using var borderPen = new Pen(C_Border, 1f);
                g.DrawPath(borderPen, cardPath);

                using var stripPath  = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, 6, pnl.Height - 1, 3);
                using var stripBrush = new LinearGradientBrush(
                    new Point(0, 0), new Point(0, pnl.Height), accent,
                    Color.FromArgb(180, accent.R, accent.G, accent.B));
                g.FillPath(stripBrush, stripPath);

                int ic = 36, ix = pnl.Width - 58, iy = (pnl.Height - ic) / 2;
                using var iconBg  = new SolidBrush(Color.FromArgb(20, accent.R, accent.G, accent.B));
                using var iconPen = new Pen(accent, 2f);
                g.FillEllipse(iconBg,  ix, iy, ic, ic);
                g.DrawEllipse(iconPen, ix, iy, ic, ic);

                using var titleFont  = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                using var titleBrush = new SolidBrush(C_TextLight);
                g.DrawString(title, titleFont, titleBrush, 20f, 18f);

                using var valFont  = new Font("Segoe UI", 18f, FontStyle.Bold);
                using var valBrush = new SolidBrush(C_TextDark);
                g.DrawString(value, valFont, valBrush, 18f, 38f);

                using var subFont  = new Font("Segoe UI", 8f, FontStyle.Regular);
                using var subBrush = new SolidBrush(Color.FromArgb(180, accent.R, accent.G, accent.B));
                g.DrawString(sub, subFont, subBrush, 20f, 78f);
            };
        }

        // ══════════════════════════════════════════════════════════════════════════
        // New Bill page
        // ══════════════════════════════════════════════════════════════════════════
        private void BuildNewBillPageUI()
        {
            pnlNewBill.Dock      = DockStyle.Fill;
            pnlNewBill.BackColor = C_Bg;
            pnlNewBill.Visible   = false;

            int lx = 0, ly = 0, lw = 560, inputH = 38;

            AddSectionLabel(pnlNewBill, "Customer Info", lx, ly);
            ly += 32;

            StyleInputField(txtBillCustomerName, "Customer Name",  lx,            ly, lw / 2 - 8, inputH);
            StyleInputField(txtBillPhone,        "Phone Number",   lx + lw/2 + 8, ly, lw / 2 - 8, inputH);
            pnlNewBill.Controls.AddRange(new Control[] { txtBillCustomerName, txtBillPhone });
            ly += inputH + 20;

            AddSectionLabel(pnlNewBill, "Add Item to Bill", lx, ly);
            ly += 32;

            StyleInputField(txtBillItemName, "Item Name",    lx,       ly, 200, inputH);
            StyleInputField(txtBillQty,      "Qty",          lx + 208, ly, 80,  inputH);
            StyleInputField(txtBillPrice,    "Unit Price",   lx + 296, ly, 120, inputH);
            pnlNewBill.Controls.AddRange(new Control[] { txtBillItemName, txtBillQty, txtBillPrice });

            StyleActionButton(btnAddLineItem, "+ Add", C_Primary, lx + 424, ly, 90, inputH);
            btnAddLineItem.Click += BtnAddLineItem_Click;
            pnlNewBill.Controls.Add(btnAddLineItem);
            ly += inputH + 16;

            StyleDataGrid(dgvBillItems);
            dgvBillItems.Location = new Point(lx, ly);
            dgvBillItems.Size     = new Size(lw, 180);
            dgvBillItems.Anchor   = AnchorStyles.None;
            dgvBillItems.Columns.AddRange(
                MakeCol("Item",     220, false),
                MakeCol("Qty",       70, false),
                MakeCol("Unit",     110, false),
                MakeCol("Total",    110, false)
            );
            pnlNewBill.Controls.Add(dgvBillItems);
            ly += 196;

            lblBillTotal.AutoSize  = false;
            lblBillTotal.Size      = new Size(lw, 36);
            lblBillTotal.Location  = new Point(lx, ly);
            lblBillTotal.Text      = "Total:  Rs. 0.00";
            lblBillTotal.Font      = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblBillTotal.ForeColor = C_Primary;
            lblBillTotal.TextAlign = ContentAlignment.MiddleRight;
            pnlNewBill.Controls.Add(lblBillTotal);
            ly += 44;

            StyleActionButton(btnSaveBill,  "Save Bill", C_Primary,                   lx,       ly, 140, 40);
            StyleActionButton(btnClearBill, "Clear",     Color.FromArgb(239, 68, 68), lx + 148, ly, 100, 40);
            btnSaveBill.Click  += BtnSaveBill_Click;
            btnClearBill.Click += BtnClearBill_Click;
            pnlNewBill.Controls.AddRange(new Control[] { btnSaveBill, btnClearBill });

            pnlContent.Controls.Add(pnlNewBill);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Bills history page
        // ══════════════════════════════════════════════════════════════════════════
        private void BuildBillsPageUI()
        {
            pnlBills.Dock      = DockStyle.Fill;
            pnlBills.BackColor = C_Bg;
            pnlBills.Visible   = false;

            StyleInputField(txtBillSearch, "Search bills by customer, bill #...", 0, 0, 360, 38);
            txtBillSearch.TextChanged += (s, e) => FilterBillsGrid();
            pnlBills.Controls.Add(txtBillSearch);

            StyleDataGrid(dgvAllBills);
            dgvAllBills.Location = new Point(0, 52);
            dgvAllBills.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvAllBills.Columns.AddRange(
                MakeCol("Bill #",    80,  false),
                MakeCol("Customer", 180,  false),
                MakeCol("Phone",    130,  false),
                MakeCol("Date",     120,  false),
                MakeCol("Items",     60,  false),
                MakeCol("Amount",   130,  false),
                MakeCol("Status",   100,  false)
            );
            LoadAllBillsGrid();
            pnlBills.Controls.Add(dgvAllBills);

            pnlContent.Controls.Add(pnlBills);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Customers page
        // ══════════════════════════════════════════════════════════════════════════
        private void BuildCustomersPageUI()
        {
            pnlCustomers.Dock      = DockStyle.Fill;
            pnlCustomers.BackColor = C_Bg;
            pnlCustomers.Visible   = false;

            StyleInputField(txtCustSearch, "Search customers...", 0, 0, 300, 38);
            txtCustSearch.TextChanged += (s, e) => FilterCustomersGrid();
            pnlCustomers.Controls.Add(txtCustSearch);

            StyleActionButton(btnAddCustomer, "+ Add Customer", C_Primary, 316, 0, 150, 38);
            btnAddCustomer.Click += BtnAddCustomer_Click;
            pnlCustomers.Controls.Add(btnAddCustomer);

            StyleDataGrid(dgvCustomers);
            dgvCustomers.Location = new Point(0, 52);
            dgvCustomers.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvCustomers.Columns.AddRange(
                MakeCol("Name",          200, false),
                MakeCol("Phone",         140, false),
                MakeCol("Email",         200, false),
                MakeCol("Total Bills",   100, false),
                MakeCol("Total Spent",   130, false)
            );
            LoadCustomersGrid();
            pnlCustomers.Controls.Add(dgvCustomers);

            pnlContent.Controls.Add(pnlCustomers);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Products page
        // ══════════════════════════════════════════════════════════════════════════
        private void BuildProductsPageUI()
        {
            pnlProducts.Dock      = DockStyle.Fill;
            pnlProducts.BackColor = C_Bg;
            pnlProducts.Visible   = false;

            StyleInputField(txtProdSearch, "Search products...", 0, 0, 300, 38);
            txtProdSearch.TextChanged += (s, e) => FilterProductsGrid();
            pnlProducts.Controls.Add(txtProdSearch);

            StyleActionButton(btnAddProduct, "+ Add Product", C_Primary, 316, 0, 150, 38);
            btnAddProduct.Click += BtnAddProduct_Click;
            pnlProducts.Controls.Add(btnAddProduct);

            StyleDataGrid(dgvProducts);
            dgvProducts.Location = new Point(0, 52);
            dgvProducts.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvProducts.Columns.AddRange(
                MakeCol("Product Name", 240, false),
                MakeCol("Category",     140, false),
                MakeCol("Unit",          80, false),
                MakeCol("Price",        100, false),
                MakeCol("Stock",         80, false)
            );
            LoadProductsGrid();
            pnlProducts.Controls.Add(dgvProducts);

            pnlContent.Controls.Add(pnlProducts);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Billing logic
        // ══════════════════════════════════════════════════════════════════════════
        private void BtnAddLineItem_Click(object? sender, EventArgs e)
        {
            string item = txtBillItemName.Text.Trim();
            if (string.IsNullOrEmpty(item)) { ShowTip("Item name khali nahi ho sakta!"); return; }
            if (!int.TryParse(txtBillQty.Text.Trim(), out int qty) || qty <= 0) { ShowTip("Valid quantity daalo!"); return; }
            if (!decimal.TryParse(txtBillPrice.Text.Trim(), out decimal price) || price < 0) { ShowTip("Valid price daalo!"); return; }

            _lineItems.Add((item, qty, price));
            decimal lineTotal = qty * price;
            _billTotal += lineTotal;

            dgvBillItems.Rows.Add(item, qty.ToString(), $"Rs. {price:F2}", $"Rs. {lineTotal:F2}");
            lblBillTotal.Text = $"Total:  Rs. {_billTotal:F2}";

            txtBillItemName.Clear(); txtBillQty.Clear(); txtBillPrice.Clear();
            txtBillItemName.Focus();
        }

        private void BtnSaveBill_Click(object? sender, EventArgs e)
        {
            string customer = txtBillCustomerName.Text.Trim();
            if (string.IsNullOrEmpty(customer)) { ShowTip("Customer name daalo!"); return; }
            if (_lineItems.Count == 0)           { ShowTip("Koi item add karo!"); return; }

            string billNo  = $"B-{DateTime.Now:yyMMddHHmm}";
            string date    = DateTime.Now.ToString("dd MMM yyyy");
            string phone   = txtBillPhone.Text.Trim();

            _billsStore.Insert(0, new[] { billNo, customer, phone, date, _lineItems.Count.ToString(), $"Rs. {_billTotal:F2}", "Paid" });

            bool custExists = _customersStore.Exists(c => c[0].Equals(customer, StringComparison.OrdinalIgnoreCase));
            if (!custExists)
                _customersStore.Add(new[] { customer, phone, "", "1", $"Rs. {_billTotal:F2}" });

            MessageBox.Show($"Bill {billNo} save ho gaya!\nCustomer: {customer}\nTotal: Rs. {_billTotal:F2}",
                "Bill Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

            BtnClearBill_Click(null, EventArgs.Empty);
            LoadAllBillsGrid();
            LoadRecentBillsGrid();
            LoadCustomersGrid();
            ShowPage("Bills");
        }

        private void BtnClearBill_Click(object? sender, EventArgs e)
        {
            _lineItems.Clear();
            _billTotal = 0m;
            dgvBillItems.Rows.Clear();
            lblBillTotal.Text = "Total:  Rs. 0.00";
            txtBillCustomerName.Clear();
            txtBillPhone.Clear();
            txtBillItemName.Clear();
            txtBillQty.Clear();
            txtBillPrice.Clear();
        }

        private void BtnAddCustomer_Click(object? sender, EventArgs e)
        {
            using var form = new Form();
            form.Text = "Add Customer";
            form.Size = new Size(340, 220);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;

            var txtName  = new TextBox { PlaceholderText = "Customer Name",  Location = new Point(20, 20),  Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var txtPhone = new TextBox { PlaceholderText = "Phone Number",   Location = new Point(20, 60),  Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var txtEmail = new TextBox { PlaceholderText = "Email (Optional)",Location = new Point(20, 100), Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var btnOk    = new Button  { Text = "Add",    DialogResult = DialogResult.OK,     Location = new Point(100, 148), Size = new Size(90, 32), BackColor = C_Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel= new Button  { Text = "Cancel", DialogResult = DialogResult.Cancel,  Location = new Point(200, 148), Size = new Size(90, 32) };
            btnOk.FlatAppearance.BorderSize = 0;

            form.Controls.AddRange(new Control[] { txtName, txtPhone, txtEmail, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                _customersStore.Add(new[] { txtName.Text.Trim(), txtPhone.Text.Trim(), txtEmail.Text.Trim(), "0", "Rs. 0.00" });
                LoadCustomersGrid();
            }
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            using var form = new Form();
            form.Text = "Add Product";
            form.Size = new Size(340, 250);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;

            var txtName  = new TextBox { PlaceholderText = "Product Name",    Location = new Point(20, 20),  Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var txtCat   = new TextBox { PlaceholderText = "Category",         Location = new Point(20, 60),  Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var txtPrice = new TextBox { PlaceholderText = "Price",            Location = new Point(20, 100), Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var txtStock = new TextBox { PlaceholderText = "Stock Quantity",    Location = new Point(20, 140), Size = new Size(280, 30), Font = new Font("Segoe UI", 10f) };
            var btnOk    = new Button  { Text = "Add",    DialogResult = DialogResult.OK,    Location = new Point(100, 180), Size = new Size(90, 32), BackColor = C_Primary, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel= new Button  { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(200, 180), Size = new Size(90, 32) };
            btnOk.FlatAppearance.BorderSize = 0;

            form.Controls.AddRange(new Control[] { txtName, txtCat, txtPrice, txtStock, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                _productsStore.Add(new[] { txtName.Text.Trim(), txtCat.Text.Trim(), "Pc", txtPrice.Text.Trim(), txtStock.Text.Trim() });
                LoadProductsGrid();
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Grid loaders & filters
        // ══════════════════════════════════════════════════════════════════════════
        private void LoadRecentBillsGrid()
        {
            dgvRecentBills.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src)
                dgvRecentBills.Rows.Add(b[0], b[1], b[4], b[3], b[5], b[6]);
        }

        private void LoadAllBillsGrid()
        {
            dgvAllBills.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src)
                dgvAllBills.Rows.Add(b[0], b[1], b[2], b[3], b[4], b[5], b[6]);
        }

        private void LoadCustomersGrid()
        {
            dgvCustomers.Rows.Clear();
            var src = _customersStore.Count > 0 ? _customersStore : GetSampleCustomers();
            foreach (var c in src)
                dgvCustomers.Rows.Add(c);
        }

        private void LoadProductsGrid()
        {
            dgvProducts.Rows.Clear();
            foreach (var p in _productsStore)
                dgvProducts.Rows.Add(p);
        }

        private void FilterBillsGrid()
        {
            string q = txtBillSearch.Text.Trim().ToLower();
            dgvAllBills.Rows.Clear();
            var src = _billsStore.Count > 0 ? _billsStore : GetSampleBills();
            foreach (var b in src)
                if (string.IsNullOrEmpty(q) || b[0].ToLower().Contains(q) || b[1].ToLower().Contains(q))
                    dgvAllBills.Rows.Add(b[0], b[1], b[2], b[3], b[4], b[5], b[6]);
        }

        private void FilterCustomersGrid()
        {
            string q = txtCustSearch.Text.Trim().ToLower();
            dgvCustomers.Rows.Clear();
            var src = _customersStore.Count > 0 ? _customersStore : GetSampleCustomers();
            foreach (var c in src)
                if (string.IsNullOrEmpty(q) || c[0].ToLower().Contains(q))
                    dgvCustomers.Rows.Add(c);
        }

        private void FilterProductsGrid()
        {
            string q = txtProdSearch.Text.Trim().ToLower();
            dgvProducts.Rows.Clear();
            foreach (var p in _productsStore)
                if (string.IsNullOrEmpty(q) || p[0].ToLower().Contains(q))
                    dgvProducts.Rows.Add(p);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Sample data
        // ══════════════════════════════════════════════════════════════════════════
        private void PopulateSampleData()
        {
            _productsStore.AddRange(new[]
            {
                new[] { "A4 Paper (500 sheets)", "Stationery",  "Ream",  "220", "85"  },
                new[] { "Pen (Blue)",             "Stationery",  "Piece", "10",  "500" },
                new[] { "Laptop Stand",           "Electronics", "Piece", "850", "20"  },
                new[] { "Mouse Pad",              "Electronics", "Piece", "150", "60"  },
                new[] { "Stapler",                "Stationery",  "Piece", "180", "35"  },
            });
        }

        private static List<string[]> GetSampleBills() => new()
        {
            new[] { "B-2610110001", "Amit Sharma",  "9876543210", "11 Oct 2026", "3", "Rs. 1,250.00",  "Paid"    },
            new[] { "B-2610100002", "Priya Patel",  "9123456780", "10 Oct 2026", "5", "Rs. 4,800.00",  "Paid"    },
            new[] { "B-2610090003", "Rahul Verma",  "9988776655", "09 Oct 2026", "2", "Rs. 620.00",    "Pending" },
            new[] { "B-2610090004", "Sneha Gupta",  "9001122334", "09 Oct 2026", "7", "Rs. 9,100.00",  "Paid"    },
            new[] { "B-2610080005", "Kiran Joshi",  "9876001234", "08 Oct 2026", "1", "Rs. 350.00",    "Pending" },
        };

        private static List<string[]> GetSampleCustomers() => new()
        {
            new[] { "Amit Sharma", "9876543210", "amit@email.com",  "8",  "Rs. 22,400.00" },
            new[] { "Priya Patel", "9123456780", "priya@email.com", "12", "Rs. 48,200.00" },
            new[] { "Rahul Verma", "9988776655", "rahul@email.com", "5",  "Rs. 11,300.00" },
            new[] { "Sneha Gupta", "9001122334", "sneha@email.com", "19", "Rs. 86,700.00" },
            new[] { "Kiran Joshi", "9876001234", "kiran@email.com", "3",  "Rs. 4,200.00"  },
        };

        // ══════════════════════════════════════════════════════════════════════════
        // Styling helpers
        // ══════════════════════════════════════════════════════════════════════════
        private static void StyleDataGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible    = false;
            dgv.AllowUserToAddRows   = false;
            dgv.ReadOnly             = true;
            dgv.SelectionMode        = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect          = false;
            dgv.BorderStyle          = BorderStyle.None;
            dgv.BackgroundColor      = Color.White;
            dgv.GridColor            = Color.FromArgb(226, 232, 240);
            dgv.CellBorderStyle      = DataGridViewCellBorderStyle.SingleHorizontal;

            dgv.ColumnHeadersDefaultCellStyle.BackColor   = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor   = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding     = new Padding(8, 0, 0, 0);
            dgv.ColumnHeadersHeight        = 36;
            dgv.ColumnHeadersBorderStyle   = DataGridViewHeaderBorderStyle.Single;
            dgv.EnableHeadersVisualStyles  = false;

            dgv.DefaultCellStyle.BackColor          = Color.White;
            dgv.DefaultCellStyle.ForeColor          = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.Font               = new Font("Segoe UI", 9f, FontStyle.Regular);
            dgv.DefaultCellStyle.Padding            = new Padding(8, 0, 0, 0);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 58, 138);
            dgv.RowTemplate.Height                  = 36;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgv.Size   = new Size(800, 300);
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        }

        private static void StyleInputField(TextBox txt, string placeholder,
                                             int x, int y, int w, int h)
        {
            txt.Location        = new Point(x, y);
            txt.Size            = new Size(w, h);
            txt.Font            = new Font("Segoe UI", 10f);
            txt.ForeColor       = Color.FromArgb(30, 41, 59);
            txt.BackColor       = Color.White;
            txt.BorderStyle     = BorderStyle.FixedSingle;
            txt.PlaceholderText = placeholder;
        }

        private static void StyleActionButton(Button btn, string text, Color bg,
                                               int x, int y, int w, int h)
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

        private static DataGridViewTextBoxColumn MakeCol(string header, int w, bool fill)
        {
            return new DataGridViewTextBoxColumn
            {
                HeaderText   = header,
                Width        = w,
                AutoSizeMode = fill
                    ? DataGridViewAutoSizeColumnMode.Fill
                    : DataGridViewAutoSizeColumnMode.None,
                DefaultCellStyle = { Padding = new Padding(8, 0, 0, 0) }
            };
        }

        private static void AddSectionLabel(Panel parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                AutoSize  = true,
                Location  = new Point(x, y)
            });
        }

        private static void ShowTip(string msg) =>
            MessageBox.Show(msg, "SB1 Billing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
