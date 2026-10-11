namespace PROJECT_SBV1.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Window chrome ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnMaximize;

        // ── Left Sidebar ───────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Button btnNavHome;
        private System.Windows.Forms.Button btnNavNewBill;
        private System.Windows.Forms.Button btnNavBills;
        private System.Windows.Forms.Button btnNavCustomers;
        private System.Windows.Forms.Button btnNavProducts;
        private System.Windows.Forms.Button btnNavReports;
        private System.Windows.Forms.Button btnNavSettings;
        private System.Windows.Forms.Label lblSidebarUser;
        private System.Windows.Forms.Label lblSidebarRole;

        // ── Main content host ──────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlContent;

        // ── Page: Home (Dashboard overview) ───────────────────────────────────────
        private System.Windows.Forms.Panel pnlHome;

        // KPI cards (4 summary cards)
        private System.Windows.Forms.Panel pnlCardTotalSales;
        private System.Windows.Forms.Panel pnlCardTodayRevenue;
        private System.Windows.Forms.Panel pnlCardPendingBills;
        private System.Windows.Forms.Panel pnlCardTotalCustomers;

        // Recent Bills DataGridView
        private System.Windows.Forms.DataGridView dgvRecentBills;

        // Quick-action "New Bill" button on home page
        private System.Windows.Forms.Button btnQuickNewBill;

        // ── Page: New Bill form ────────────────────────────────────────────────────
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

        // ── Page: Bills history ────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlBills;
        private System.Windows.Forms.DataGridView dgvAllBills;
        private System.Windows.Forms.TextBox txtBillSearch;

        // ── Page: Customers ────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlCustomers;
        private System.Windows.Forms.DataGridView dgvCustomers;
        private System.Windows.Forms.Button btnAddCustomer;
        private System.Windows.Forms.TextBox txtCustSearch;

        // ── Page: Products ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlProducts;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.Button btnAddProduct;
        private System.Windows.Forms.TextBox txtProdSearch;

        // ── Search bar (top bar) ───────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.Label lblPageTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ── Instantiate ────────────────────────────────────────────────────────
            btnClose           = new System.Windows.Forms.Button();
            btnMinimize        = new System.Windows.Forms.Button();
            btnMaximize        = new System.Windows.Forms.Button();

            pnlSidebar         = new System.Windows.Forms.Panel();
            btnNavHome         = new System.Windows.Forms.Button();
            btnNavNewBill      = new System.Windows.Forms.Button();
            btnNavBills        = new System.Windows.Forms.Button();
            btnNavCustomers    = new System.Windows.Forms.Button();
            btnNavProducts     = new System.Windows.Forms.Button();
            btnNavReports      = new System.Windows.Forms.Button();
            btnNavSettings     = new System.Windows.Forms.Button();
            lblSidebarUser     = new System.Windows.Forms.Label();
            lblSidebarRole     = new System.Windows.Forms.Label();

            pnlTopBar          = new System.Windows.Forms.Panel();
            lblPageTitle       = new System.Windows.Forms.Label();

            pnlContent         = new System.Windows.Forms.Panel();

            // Home page panels
            pnlHome            = new System.Windows.Forms.Panel();
            pnlCardTotalSales  = new System.Windows.Forms.Panel();
            pnlCardTodayRevenue= new System.Windows.Forms.Panel();
            pnlCardPendingBills= new System.Windows.Forms.Panel();
            pnlCardTotalCustomers = new System.Windows.Forms.Panel();
            dgvRecentBills     = new System.Windows.Forms.DataGridView();
            btnQuickNewBill    = new System.Windows.Forms.Button();

            // New Bill page
            pnlNewBill         = new System.Windows.Forms.Panel();
            txtBillCustomerName= new System.Windows.Forms.TextBox();
            txtBillPhone       = new System.Windows.Forms.TextBox();
            txtBillItemName    = new System.Windows.Forms.TextBox();
            txtBillQty         = new System.Windows.Forms.TextBox();
            txtBillPrice       = new System.Windows.Forms.TextBox();
            btnAddLineItem     = new System.Windows.Forms.Button();
            dgvBillItems       = new System.Windows.Forms.DataGridView();
            btnSaveBill        = new System.Windows.Forms.Button();
            btnClearBill       = new System.Windows.Forms.Button();
            lblBillTotal       = new System.Windows.Forms.Label();

            // Bills history
            pnlBills           = new System.Windows.Forms.Panel();
            dgvAllBills        = new System.Windows.Forms.DataGridView();
            txtBillSearch      = new System.Windows.Forms.TextBox();

            // Customers
            pnlCustomers       = new System.Windows.Forms.Panel();
            dgvCustomers       = new System.Windows.Forms.DataGridView();
            btnAddCustomer     = new System.Windows.Forms.Button();
            txtCustSearch      = new System.Windows.Forms.TextBox();

            // Products
            pnlProducts        = new System.Windows.Forms.Panel();
            dgvProducts        = new System.Windows.Forms.DataGridView();
            btnAddProduct      = new System.Windows.Forms.Button();
            txtProdSearch      = new System.Windows.Forms.TextBox();

            this.SuspendLayout();

            // ── Form ───────────────────────────────────────────────────────────────
            this.Text            = "SB1 — Simple Billing Dashboard";
            this.Name            = "DashboardForm";
            this.ClientSize      = new System.Drawing.Size(1200, 760);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor       = System.Drawing.Color.White;
            this.DoubleBuffered  = true;
            this.MinimumSize     = new System.Drawing.Size(1000, 660);

            // ── Window buttons ─────────────────────────────────────────────────────
            StyleChromeButton(btnClose,    "✕", System.Drawing.Color.FromArgb(254, 226, 226), System.Drawing.Color.FromArgb(252, 165, 165));
            StyleChromeButton(btnMinimize, "─", System.Drawing.Color.FromArgb(241, 245, 249), System.Drawing.Color.FromArgb(226, 232, 240));
            StyleChromeButton(btnMaximize, "□", System.Drawing.Color.FromArgb(241, 245, 249), System.Drawing.Color.FromArgb(226, 232, 240));
            btnClose.Click    += (s, e) => this.Close();
            btnMinimize.Click += (s, e) => this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            btnMaximize.Click += (s, e) =>
                this.WindowState = this.WindowState == System.Windows.Forms.FormWindowState.Maximized
                    ? System.Windows.Forms.FormWindowState.Normal
                    : System.Windows.Forms.FormWindowState.Maximized;

            // ── Sidebar ────────────────────────────────────────────────────────────
            pnlSidebar.Dock      = System.Windows.Forms.DockStyle.Left;
            pnlSidebar.Width     = 220;
            pnlSidebar.BackColor = System.Drawing.Color.FromArgb(15, 23, 42); // slate-900

            // Sidebar user info labels
            lblSidebarUser.AutoSize  = false;
            lblSidebarUser.Size      = new System.Drawing.Size(196, 22);
            lblSidebarUser.Location  = new System.Drawing.Point(12, 680);
            lblSidebarUser.Text      = "Jenil Rana";
            lblSidebarUser.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            lblSidebarUser.ForeColor = System.Drawing.Color.White;
            lblSidebarUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblSidebarRole.AutoSize  = false;
            lblSidebarRole.Size      = new System.Drawing.Size(196, 18);
            lblSidebarRole.Location  = new System.Drawing.Point(12, 700);
            lblSidebarRole.Text      = "UI/UX Developer";
            lblSidebarRole.Font      = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular);
            lblSidebarRole.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSidebarRole.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnNavHome, btnNavNewBill, btnNavBills,
                btnNavCustomers, btnNavProducts, btnNavReports, btnNavSettings,
                lblSidebarUser, lblSidebarRole
            });

            // ── Top bar ────────────────────────────────────────────────────────────
            pnlTopBar.Dock      = System.Windows.Forms.DockStyle.Top;
            pnlTopBar.Height    = 56;
            pnlTopBar.BackColor = System.Drawing.Color.White;

            lblPageTitle.AutoSize  = false;
            lblPageTitle.Size      = new System.Drawing.Size(400, 56);
            lblPageTitle.Location  = new System.Drawing.Point(16, 0);
            lblPageTitle.Text      = "Dashboard";
            lblPageTitle.Font      = new System.Drawing.Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
            lblPageTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlTopBar.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblPageTitle, btnClose, btnMinimize, btnMaximize
            });

            // ── Content area ───────────────────────────────────────────────────────
            pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlContent.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            pnlContent.Padding   = new System.Windows.Forms.Padding(24, 16, 24, 16);

            // ── Assemble form ──────────────────────────────────────────────────────
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlTopBar);
            this.Controls.Add(pnlSidebar);

            this.ResumeLayout(false);
        }

        /// <summary>Styles a borderless window chrome button (Close/Min/Max).</summary>
        private static void StyleChromeButton(
            System.Windows.Forms.Button btn,
            string symbol,
            System.Drawing.Color hoverBg,
            System.Drawing.Color downBg)
        {
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hoverBg;
            btn.FlatAppearance.MouseDownBackColor = downBg;
            btn.Font      = new System.Drawing.Font("Segoe UI", 10f);
            btn.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            btn.Text      = symbol;
            btn.Size      = new System.Drawing.Size(36, 30);
            btn.TabStop   = false;
            btn.Cursor    = System.Windows.Forms.Cursors.Hand;
            btn.BackColor = System.Drawing.Color.Transparent;
        }
    }
}
