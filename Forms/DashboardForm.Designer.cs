namespace PROJECT_SBV1.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        // Window Chrome
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnMaximize;

        // Master Layout Panels
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlBrandHeader;
        private System.Windows.Forms.Label lblSidebarBrand;
        private System.Windows.Forms.Label lblSidebarSub;
        private System.Windows.Forms.Panel pnlNavMenu;
        private System.Windows.Forms.Panel pnlSidebarFooter;
        private System.Windows.Forms.Button btnNavLogout;
        private System.Windows.Forms.Label lblSidebarUser;
        private System.Windows.Forms.Label lblSidebarRole;

        // Sidebar Navigation Buttons
        private System.Windows.Forms.Button btnNavOverview;
        private System.Windows.Forms.Button btnNavStatistics;
        private System.Windows.Forms.Button btnNavCustomers;
        private System.Windows.Forms.Button btnNavSuppliers;
        private System.Windows.Forms.Button btnNavProducts;
        private System.Windows.Forms.Button btnNavPurchases;
        private System.Windows.Forms.Button btnNavSales;
        private System.Windows.Forms.Button btnNavTransactions;
        private System.Windows.Forms.Button btnNavMessages;
        private System.Windows.Forms.Button btnNavUsers;
        private System.Windows.Forms.Button btnNavSettings;

        // Main Container: TableLayoutPanel (Row 0 = TopBar 68px, Row 1 = Content 100%)
        // Eliminates any possibility of header and content overlap!
        private System.Windows.Forms.TableLayoutPanel tblMainLayout;

        // Topbar
        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.Panel pnlTitleSection;
        private System.Windows.Forms.Label lblPageTitle;
        private System.Windows.Forms.Label lblPageSub;
        private System.Windows.Forms.Panel pnlWindowChrome;
        private System.Windows.Forms.FlowLayoutPanel flowHeaderActions;
        private System.Windows.Forms.Panel pnlSearchBox;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnNotifications;
        private System.Windows.Forms.Button btnThemeToggle;
        private System.Windows.Forms.Button btnTopAddProduct;

        // Content Area
        private System.Windows.Forms.Panel pnlContentHost;
        private System.Windows.Forms.Panel pnlOverview;

        // Layout Containers for Dashboard
        private System.Windows.Forms.TableLayoutPanel tlpCards;
        private System.Windows.Forms.Panel pnlCardSalesToday;
        private System.Windows.Forms.Panel pnlCardSalesMonth;
        private System.Windows.Forms.Panel pnlCardReceivables;
        private System.Windows.Forms.Panel pnlCardPayables;
        private System.Windows.Forms.Panel pnlCardTotalProducts;
        private System.Windows.Forms.Panel pnlCardLowStock;

        private System.Windows.Forms.TableLayoutPanel tlpCharts;
        private System.Windows.Forms.Panel pnlSalesDynamicsCard;
        private System.Windows.Forms.Panel pnlDynamicsChartCanvas;
        private System.Windows.Forms.Panel pnlCategoryDonutCard;
        private System.Windows.Forms.Panel pnlDonutChartCanvas;

        private System.Windows.Forms.TableLayoutPanel tlpTables;
        private System.Windows.Forms.Panel pnlRecentInvoicesCard;
        private System.Windows.Forms.DataGridView dgvInvoices;
        private System.Windows.Forms.Panel pnlLowStockCard;
        private System.Windows.Forms.DataGridView dgvLowStock;

        // Sub Pages
        private System.Windows.Forms.Panel pnlNewBill;
        private System.Windows.Forms.TextBox txtBillCustomerName;
        private System.Windows.Forms.TextBox txtBillPhone;
        private System.Windows.Forms.TextBox txtBillItemName;
        private System.Windows.Forms.TextBox txtBillQty;
        private System.Windows.Forms.TextBox txtBillPrice;
        private System.Windows.Forms.Button btnAddLineItem;
        private System.Windows.Forms.DataGridView dgvBillItems;
        private System.Windows.Forms.Button btnSaveBill;
        private System.Windows.Forms.Button btnClearBill;
        private System.Windows.Forms.Label lblBillTotal;

        private System.Windows.Forms.Panel pnlCustomers;
        private System.Windows.Forms.DataGridView dgvCustomers;
        private System.Windows.Forms.Button btnAddCustomer;
        private System.Windows.Forms.TextBox txtCustSearch;

        private System.Windows.Forms.Panel pnlProducts;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.Button btnAddProductPage;
        private System.Windows.Forms.TextBox txtProdSearch;

        private System.Windows.Forms.Panel pnlTransPage;
        private System.Windows.Forms.DataGridView dgvAllTransactions;
        private System.Windows.Forms.TextBox txtTransSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // Chrome Buttons
            btnClose = new System.Windows.Forms.Button();
            btnMinimize = new System.Windows.Forms.Button();
            btnMaximize = new System.Windows.Forms.Button();

            // Master Containers
            pnlSidebar = new System.Windows.Forms.Panel();
            pnlBrandHeader = new System.Windows.Forms.Panel();
            lblSidebarBrand = new System.Windows.Forms.Label();
            lblSidebarSub = new System.Windows.Forms.Label();
            pnlNavMenu = new System.Windows.Forms.Panel();
            pnlSidebarFooter = new System.Windows.Forms.Panel();
            btnNavLogout = new System.Windows.Forms.Button();
            lblSidebarUser = new System.Windows.Forms.Label();
            lblSidebarRole = new System.Windows.Forms.Label();

            // Navigation Buttons
            btnNavOverview = new System.Windows.Forms.Button();
            btnNavStatistics = new System.Windows.Forms.Button();
            btnNavCustomers = new System.Windows.Forms.Button();
            btnNavSuppliers = new System.Windows.Forms.Button();
            btnNavProducts = new System.Windows.Forms.Button();
            btnNavPurchases = new System.Windows.Forms.Button();
            btnNavSales = new System.Windows.Forms.Button();
            btnNavTransactions = new System.Windows.Forms.Button();
            btnNavMessages = new System.Windows.Forms.Button();
            btnNavUsers = new System.Windows.Forms.Button();
            btnNavSettings = new System.Windows.Forms.Button();

            // Main Layout: TableLayoutPanel (2 Rows: Row 0 = 68px, Row 1 = 100%)
            tblMainLayout = new System.Windows.Forms.TableLayoutPanel();

            // Topbar
            pnlTopBar = new System.Windows.Forms.Panel();
            pnlWindowChrome = new System.Windows.Forms.Panel();
            pnlTitleSection = new System.Windows.Forms.Panel();
            lblPageTitle = new System.Windows.Forms.Label();
            lblPageSub = new System.Windows.Forms.Label();
            flowHeaderActions = new System.Windows.Forms.FlowLayoutPanel();
            pnlSearchBox = new System.Windows.Forms.Panel();
            txtSearch = new System.Windows.Forms.TextBox();
            btnNotifications = new System.Windows.Forms.Button();
            btnThemeToggle = new System.Windows.Forms.Button();
            btnTopAddProduct = new System.Windows.Forms.Button();

            // Content Host
            pnlContentHost = new System.Windows.Forms.Panel();
            pnlOverview = new System.Windows.Forms.Panel();

            // Dashboard Grid Layouts
            tlpCards = new System.Windows.Forms.TableLayoutPanel();
            pnlCardSalesToday = new System.Windows.Forms.Panel();
            pnlCardSalesMonth = new System.Windows.Forms.Panel();
            pnlCardReceivables = new System.Windows.Forms.Panel();
            pnlCardPayables = new System.Windows.Forms.Panel();
            pnlCardTotalProducts = new System.Windows.Forms.Panel();
            pnlCardLowStock = new System.Windows.Forms.Panel();

            tlpCharts = new System.Windows.Forms.TableLayoutPanel();
            pnlSalesDynamicsCard = new System.Windows.Forms.Panel();
            pnlDynamicsChartCanvas = new System.Windows.Forms.Panel();
            pnlCategoryDonutCard = new System.Windows.Forms.Panel();
            pnlDonutChartCanvas = new System.Windows.Forms.Panel();

            tlpTables = new System.Windows.Forms.TableLayoutPanel();
            pnlRecentInvoicesCard = new System.Windows.Forms.Panel();
            dgvInvoices = new System.Windows.Forms.DataGridView();
            pnlLowStockCard = new System.Windows.Forms.Panel();
            dgvLowStock = new System.Windows.Forms.DataGridView();

            // Sub Pages
            pnlNewBill = new System.Windows.Forms.Panel();
            txtBillCustomerName = new System.Windows.Forms.TextBox();
            txtBillPhone = new System.Windows.Forms.TextBox();
            txtBillItemName = new System.Windows.Forms.TextBox();
            txtBillQty = new System.Windows.Forms.TextBox();
            txtBillPrice = new System.Windows.Forms.TextBox();
            btnAddLineItem = new System.Windows.Forms.Button();
            dgvBillItems = new System.Windows.Forms.DataGridView();
            btnSaveBill = new System.Windows.Forms.Button();
            btnClearBill = new System.Windows.Forms.Button();
            lblBillTotal = new System.Windows.Forms.Label();

            pnlCustomers = new System.Windows.Forms.Panel();
            dgvCustomers = new System.Windows.Forms.DataGridView();
            btnAddCustomer = new System.Windows.Forms.Button();
            txtCustSearch = new System.Windows.Forms.TextBox();

            pnlProducts = new System.Windows.Forms.Panel();
            dgvProducts = new System.Windows.Forms.DataGridView();
            btnAddProductPage = new System.Windows.Forms.Button();
            txtProdSearch = new System.Windows.Forms.TextBox();

            pnlTransPage = new System.Windows.Forms.Panel();
            dgvAllTransactions = new System.Windows.Forms.DataGridView();
            txtTransSearch = new System.Windows.Forms.TextBox();

            this.SuspendLayout();

            // ── Form Master Setup ──────────────────────────────────────────────────
            this.Text            = "TEAM SOFT — Inventory & Billing";
            this.Name            = "DashboardForm";
            this.ClientSize      = new System.Drawing.Size(1366, 820);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor       = System.Drawing.Color.FromArgb(17, 18, 24);
            this.DoubleBuffered  = true;
            this.MinimumSize     = new System.Drawing.Size(1100, 700);

            // ── Chrome Buttons ─────────────────────────────────────────────────────
            SetupChromeBtn(btnClose, "✕",
                System.Drawing.Color.FromArgb(239, 68, 68),
                System.Drawing.Color.FromArgb(220, 38, 38));
            SetupChromeBtn(btnMinimize, "─",
                System.Drawing.Color.FromArgb(51, 65, 85),
                System.Drawing.Color.FromArgb(71, 85, 105));
            SetupChromeBtn(btnMaximize, "□",
                System.Drawing.Color.FromArgb(51, 65, 85),
                System.Drawing.Color.FromArgb(71, 85, 105));

            btnClose.Click    += (s, e) => this.Close();
            btnMinimize.Click += (s, e) => this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            btnMaximize.Click += (s, e) =>
                this.WindowState = this.WindowState == System.Windows.Forms.FormWindowState.Maximized
                    ? System.Windows.Forms.FormWindowState.Normal
                    : System.Windows.Forms.FormWindowState.Maximized;

            // ═══════════════════════════════════════════════════════════════════════
            //  1. SIDEBAR (Dock = Left, Width = 230)
            // ═══════════════════════════════════════════════════════════════════════
            pnlSidebar.Dock      = System.Windows.Forms.DockStyle.Left;
            pnlSidebar.Width     = 230;
            pnlSidebar.BackColor = System.Drawing.Color.FromArgb(25, 26, 35);

            // Brand Header (Dock = Top)
            pnlBrandHeader.Dock   = System.Windows.Forms.DockStyle.Top;
            pnlBrandHeader.Height = 68;
            pnlBrandHeader.BackColor = System.Drawing.Color.Transparent;

            lblSidebarBrand.AutoSize  = false;
            lblSidebarBrand.Size      = new System.Drawing.Size(155, 24);
            lblSidebarBrand.Location  = new System.Drawing.Point(54, 16);
            lblSidebarBrand.Text      = "TEAM SOFT";
            lblSidebarBrand.Font      = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
            lblSidebarBrand.ForeColor = System.Drawing.Color.White;
            lblSidebarBrand.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblSidebarSub.AutoSize  = false;
            lblSidebarSub.Size      = new System.Drawing.Size(155, 16);
            lblSidebarSub.Location  = new System.Drawing.Point(54, 40);
            lblSidebarSub.Text      = "Inventory & Billing";
            lblSidebarSub.Font      = new System.Drawing.Font("Segoe UI", 7.5f);
            lblSidebarSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            lblSidebarSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlBrandHeader.Controls.AddRange(new System.Windows.Forms.Control[] { lblSidebarBrand, lblSidebarSub });

            // Sidebar Footer (Dock = Bottom)
            pnlSidebarFooter.Dock   = System.Windows.Forms.DockStyle.Bottom;
            pnlSidebarFooter.Height = 100;
            pnlSidebarFooter.BackColor = System.Drawing.Color.Transparent;

            btnNavLogout.Dock   = System.Windows.Forms.DockStyle.Top;
            btnNavLogout.Height = 38;
            btnNavLogout.Text   = string.Empty;

            lblSidebarUser.AutoSize  = false;
            lblSidebarUser.Size      = new System.Drawing.Size(190, 18);
            lblSidebarUser.Location  = new System.Drawing.Point(18, 44);
            lblSidebarUser.Text      = "Saumya Chaudhari";
            lblSidebarUser.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblSidebarUser.ForeColor = System.Drawing.Color.White;
            lblSidebarUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblSidebarRole.AutoSize  = false;
            lblSidebarRole.Size      = new System.Drawing.Size(190, 16);
            lblSidebarRole.Location  = new System.Drawing.Point(18, 64);
            lblSidebarRole.Text      = "Vision • Administrator";
            lblSidebarRole.Font      = new System.Drawing.Font("Segoe UI", 7.5f);
            lblSidebarRole.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            lblSidebarRole.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlSidebarFooter.Controls.AddRange(new System.Windows.Forms.Control[] { btnNavLogout, lblSidebarUser, lblSidebarRole });

            // Navigation Menu (Dock = Fill, Scrollable)
            pnlNavMenu.Dock       = System.Windows.Forms.DockStyle.Fill;
            pnlNavMenu.AutoScroll = true;
            pnlNavMenu.BackColor  = System.Drawing.Color.Transparent;

            pnlSidebar.Controls.Add(pnlNavMenu);
            pnlSidebar.Controls.Add(pnlSidebarFooter);
            pnlSidebar.Controls.Add(pnlBrandHeader);

            // ═══════════════════════════════════════════════════════════════════════
            //  2. MAIN LAYOUT: TableLayoutPanel (Dock = Fill, 2 Rows)
            //  Row 0 = TopBar (68px fixed)
            //  Row 1 = ContentHost (100% remaining)
            //  Guarantees 100% zero overlap between Header and Content!
            // ═══════════════════════════════════════════════════════════════════════
            tblMainLayout.Dock        = System.Windows.Forms.DockStyle.Fill;
            tblMainLayout.ColumnCount = 1;
            tblMainLayout.RowCount    = 2;
            tblMainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
            tblMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68f));
            tblMainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
            tblMainLayout.BackColor   = System.Drawing.Color.FromArgb(17, 18, 24);
            tblMainLayout.Margin      = System.Windows.Forms.Padding.Empty;
            tblMainLayout.Padding     = System.Windows.Forms.Padding.Empty;

            // Topbar Panel (Row 0)
            pnlTopBar.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlTopBar.BackColor = System.Drawing.Color.FromArgb(25, 26, 35);
            pnlTopBar.Padding   = new System.Windows.Forms.Padding(18, 0, 8, 0);
            pnlTopBar.Margin    = System.Windows.Forms.Padding.Empty;

            // Left Title Section (Dock = Left, Width = 285)
            pnlTitleSection.Dock      = System.Windows.Forms.DockStyle.Left;
            pnlTitleSection.Width     = 285;
            pnlTitleSection.BackColor = System.Drawing.Color.Transparent;

            lblPageTitle.AutoSize  = false;
            lblPageTitle.Size      = new System.Drawing.Size(280, 26);
            lblPageTitle.Location  = new System.Drawing.Point(0, 12);
            lblPageTitle.Text      = "Dashboard";
            lblPageTitle.Font      = new System.Drawing.Font("Segoe UI", 15f, System.Drawing.FontStyle.Bold);
            lblPageTitle.ForeColor = System.Drawing.Color.White;
            lblPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblPageSub.AutoSize  = false;
            lblPageSub.Size      = new System.Drawing.Size(280, 18);
            lblPageSub.Location  = new System.Drawing.Point(0, 38);
            lblPageSub.Text      = "Real-time business performance overview";
            lblPageSub.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            lblPageSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            lblPageSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlTitleSection.Controls.AddRange(new System.Windows.Forms.Control[] { lblPageTitle, lblPageSub });

            // Dedicated Window Chrome Panel (Dock = Right, Width = 116, safe from corner arc)
            pnlWindowChrome.Dock      = System.Windows.Forms.DockStyle.Right;
            pnlWindowChrome.Width     = 116;
            pnlWindowChrome.BackColor = System.Drawing.Color.Transparent;
            pnlWindowChrome.Margin    = System.Windows.Forms.Padding.Empty;
            pnlWindowChrome.Padding   = System.Windows.Forms.Padding.Empty;

            btnMinimize.Location = new System.Drawing.Point(4, 18);
            btnMinimize.Size     = new System.Drawing.Size(32, 32);
            btnMaximize.Location = new System.Drawing.Point(40, 18);
            btnMaximize.Size     = new System.Drawing.Size(32, 32);
            btnClose.Location    = new System.Drawing.Point(76, 18);
            btnClose.Size        = new System.Drawing.Size(32, 32);

            pnlWindowChrome.Controls.AddRange(new System.Windows.Forms.Control[] { btnMinimize, btnMaximize, btnClose });

            // Header Toolbar Actions (Dock = Right, adjacent to Chrome, RightToLeft order)
            flowHeaderActions.Dock          = System.Windows.Forms.DockStyle.Right;
            flowHeaderActions.AutoSize      = true;
            flowHeaderActions.AutoSizeMode  = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowHeaderActions.BackColor     = System.Drawing.Color.Transparent;
            flowHeaderActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowHeaderActions.WrapContents  = false;
            flowHeaderActions.Margin        = System.Windows.Forms.Padding.Empty;
            flowHeaderActions.Padding       = new System.Windows.Forms.Padding(0, 0, 8, 0);

            // Controls ordered Right-to-Left:
            // 1. New Invoice Button (Width = 136 guarantees full text visibility without clipping)
            btnTopAddProduct.Text      = string.Empty;
            btnTopAddProduct.Size      = new System.Drawing.Size(136, 36);
            btnTopAddProduct.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            btnTopAddProduct.ForeColor = System.Drawing.Color.White;
            btnTopAddProduct.BackColor = System.Drawing.Color.FromArgb(65, 105, 245);
            btnTopAddProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnTopAddProduct.FlatAppearance.BorderSize = 0;
            btnTopAddProduct.Cursor    = System.Windows.Forms.Cursors.Hand;
            btnTopAddProduct.Margin    = new System.Windows.Forms.Padding(6, 16, 8, 16);
            flowHeaderActions.Controls.Add(btnTopAddProduct);

            // 2. Theme Toggle Button
            btnThemeToggle.Size        = new System.Drawing.Size(62, 32);
            btnThemeToggle.FlatStyle   = System.Windows.Forms.FlatStyle.Flat;
            btnThemeToggle.FlatAppearance.BorderSize = 0;
            btnThemeToggle.BackColor   = System.Drawing.Color.Transparent;
            btnThemeToggle.Cursor      = System.Windows.Forms.Cursors.Hand;
            btnThemeToggle.TabStop     = false;
            btnThemeToggle.Margin      = new System.Windows.Forms.Padding(6, 18, 6, 18);
            flowHeaderActions.Controls.Add(btnThemeToggle);

            // 3. Notifications
            btnNotifications.Text      = string.Empty;
            btnNotifications.Size      = new System.Drawing.Size(36, 36);
            btnNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.Cursor    = System.Windows.Forms.Cursors.Hand;
            btnNotifications.Margin    = new System.Windows.Forms.Padding(4, 16, 4, 16);
            flowHeaderActions.Controls.Add(btnNotifications);

            // 4. Search Box
            pnlSearchBox.BackColor     = System.Drawing.Color.FromArgb(30, 32, 43);
            pnlSearchBox.Size          = new System.Drawing.Size(190, 36);
            pnlSearchBox.Margin        = new System.Windows.Forms.Padding(6, 16, 6, 16);
            txtSearch.BorderStyle      = System.Windows.Forms.BorderStyle.None;
            txtSearch.BackColor        = System.Drawing.Color.FromArgb(30, 32, 43);
            txtSearch.Font             = new System.Drawing.Font("Segoe UI", 9.5f);
            txtSearch.ForeColor        = System.Drawing.Color.FromArgb(203, 213, 225);
            txtSearch.PlaceholderText  = "Search bills, items...";
            txtSearch.Location         = new System.Drawing.Point(32, 8);
            txtSearch.Size             = new System.Drawing.Size(150, 20);
            pnlSearchBox.Controls.Add(txtSearch);
            flowHeaderActions.Controls.Add(pnlSearchBox);

            // Assemble TopBar with strict z-order:
            // pnlWindowChrome at bottom of z-order docks to outermost right.
            // flowHeaderActions at top of z-order docks to the left of pnlWindowChrome.
            // pnlTitleSection docks to the left.
            pnlTopBar.Controls.Add(flowHeaderActions);
            pnlTopBar.Controls.Add(pnlWindowChrome);
            pnlTopBar.Controls.Add(pnlTitleSection);
            pnlTitleSection.SendToBack();
            pnlWindowChrome.SendToBack();
            flowHeaderActions.BringToFront();

            // Content Host Panel (Row 1)
            pnlContentHost.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlContentHost.BackColor = System.Drawing.Color.FromArgb(17, 18, 24);
            pnlContentHost.Padding   = new System.Windows.Forms.Padding(16, 12, 16, 12);
            pnlContentHost.Margin    = System.Windows.Forms.Padding.Empty;

            // Overview Page (Dock = Fill, AutoScroll = false to eliminate right-side scrollbar!)
            pnlOverview.Dock       = System.Windows.Forms.DockStyle.Fill;
            pnlOverview.AutoScroll = false;
            pnlOverview.BackColor  = System.Drawing.Color.Transparent;

            // ── TableLayoutPanels for responsive, zero-overlap grid ────────────────
            tlpCards.Dock      = System.Windows.Forms.DockStyle.Top;
            tlpCards.Height    = 92;
            tlpCards.ColumnCount = 6;
            tlpCards.RowCount    = 1;
            tlpCards.BackColor = System.Drawing.Color.Transparent;
            tlpCards.Margin    = new System.Windows.Forms.Padding(0, 0, 0, 10);
            for (int i = 0; i < 6; i++)
                tlpCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.666f));
            tlpCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

            pnlCardSalesToday.Dock    = System.Windows.Forms.DockStyle.Fill;
            pnlCardSalesToday.Margin  = new System.Windows.Forms.Padding(4);
            pnlCardSalesMonth.Dock    = System.Windows.Forms.DockStyle.Fill;
            pnlCardSalesMonth.Margin  = new System.Windows.Forms.Padding(4);
            pnlCardReceivables.Dock   = System.Windows.Forms.DockStyle.Fill;
            pnlCardReceivables.Margin = new System.Windows.Forms.Padding(4);
            pnlCardPayables.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlCardPayables.Margin    = new System.Windows.Forms.Padding(4);
            pnlCardTotalProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlCardTotalProducts.Margin = new System.Windows.Forms.Padding(4);
            pnlCardLowStock.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlCardLowStock.Margin    = new System.Windows.Forms.Padding(4);

            tlpCards.Controls.Add(pnlCardSalesToday,    0, 0);
            tlpCards.Controls.Add(pnlCardSalesMonth,    1, 0);
            tlpCards.Controls.Add(pnlCardReceivables,   2, 0);
            tlpCards.Controls.Add(pnlCardPayables,      3, 0);
            tlpCards.Controls.Add(pnlCardTotalProducts, 4, 0);
            tlpCards.Controls.Add(pnlCardLowStock,      5, 0);

            // Row 2 Charts
            tlpCharts.Dock      = System.Windows.Forms.DockStyle.Top;
            tlpCharts.Height    = 250;
            tlpCharts.ColumnCount = 2;
            tlpCharts.RowCount    = 1;
            tlpCharts.BackColor = System.Drawing.Color.Transparent;
            tlpCharts.Margin    = new System.Windows.Forms.Padding(0, 10, 0, 10);
            tlpCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65f));
            tlpCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35f));
            tlpCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

            pnlSalesDynamicsCard.Dock   = System.Windows.Forms.DockStyle.Fill;
            pnlSalesDynamicsCard.Margin = new System.Windows.Forms.Padding(4);
            pnlCategoryDonutCard.Dock   = System.Windows.Forms.DockStyle.Fill;
            pnlCategoryDonutCard.Margin = new System.Windows.Forms.Padding(4);

            tlpCharts.Controls.Add(pnlSalesDynamicsCard, 0, 0);
            tlpCharts.Controls.Add(pnlCategoryDonutCard, 1, 0);

            // Row 3 Tables
            tlpTables.Dock      = System.Windows.Forms.DockStyle.Top;
            tlpTables.Height    = 260;
            tlpTables.ColumnCount = 2;
            tlpTables.RowCount    = 1;
            tlpTables.BackColor = System.Drawing.Color.Transparent;
            tlpTables.Margin    = new System.Windows.Forms.Padding(0, 10, 0, 0);
            tlpTables.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60f));
            tlpTables.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40f));
            tlpTables.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));

            pnlRecentInvoicesCard.Dock   = System.Windows.Forms.DockStyle.Fill;
            pnlRecentInvoicesCard.Margin = new System.Windows.Forms.Padding(4);
            pnlLowStockCard.Dock         = System.Windows.Forms.DockStyle.Fill;
            pnlLowStockCard.Margin       = new System.Windows.Forms.Padding(4);

            tlpTables.Controls.Add(pnlRecentInvoicesCard, 0, 0);
            tlpTables.Controls.Add(pnlLowStockCard,       1, 0);

            // Add in reverse top-dock order so Cards are at top, Charts in middle, Tables at bottom
            pnlOverview.Controls.Add(tlpTables);
            pnlOverview.Controls.Add(tlpCharts);
            pnlOverview.Controls.Add(tlpCards);

            pnlContentHost.Controls.Add(pnlOverview);

            // Add Topbar to Row 0 and Content to Row 1 of tblMainLayout
            tblMainLayout.Controls.Add(pnlTopBar, 0, 0);
            tblMainLayout.Controls.Add(pnlContentHost, 0, 1);

            // Add Master Panels to Form
            this.Controls.Add(tblMainLayout);
            this.Controls.Add(pnlSidebar);

            this.ResumeLayout(false);
        }

        private static void SetupChromeBtn(System.Windows.Forms.Button btn, string symbol,
            System.Drawing.Color hoverBg, System.Drawing.Color downBg)
        {
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor  = hoverBg;
            btn.FlatAppearance.MouseDownBackColor  = downBg;
            btn.Font      = new System.Drawing.Font("Segoe UI", 10f);
            btn.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            btn.Text      = symbol;
            btn.Size      = new System.Drawing.Size(34, 34);
            btn.Margin    = new System.Windows.Forms.Padding(2, 9, 2, 8);
            btn.TabStop   = false;
            btn.Cursor    = System.Windows.Forms.Cursors.Hand;
            btn.BackColor = System.Drawing.Color.Transparent;
        }
    }
}
