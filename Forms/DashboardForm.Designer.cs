namespace PROJECT_SBV1.Forms
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        // Window chrome
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnMaximize;

        // Sidebar
        private System.Windows.Forms.Panel  pnlSidebar;
        private System.Windows.Forms.Button btnNavOverview;
        private System.Windows.Forms.Button btnNavNewBill;
        private System.Windows.Forms.Button btnNavCustomers;
        private System.Windows.Forms.Button btnNavProducts;
        private System.Windows.Forms.Button btnNavTransactions;
        private System.Windows.Forms.Button btnNavMessages;
        private System.Windows.Forms.Button btnNavSettings;
        private System.Windows.Forms.Button btnNavSecurity;
        private System.Windows.Forms.Label  lblSidebarUser;
        private System.Windows.Forms.Label  lblSidebarEmail;

        // Top bar
        private System.Windows.Forms.Panel   pnlTopBar;
        private System.Windows.Forms.Label   lblPageTitle;
        private System.Windows.Forms.Label   lblPageSub;
        private System.Windows.Forms.Panel   pnlSearchBox;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button  btnTopAddProduct;

        // Content host
        private System.Windows.Forms.Panel pnlContent;

        // Overview page
        private System.Windows.Forms.Panel pnlOverview;
        private System.Windows.Forms.Panel pnlStatHero;
        private System.Windows.Forms.Panel pnlStatIncome;
        private System.Windows.Forms.Panel pnlStatReturn;
        private System.Windows.Forms.Panel pnlTransactions;
        private System.Windows.Forms.DataGridView dgvTransactions;
        private System.Windows.Forms.Panel pnlRevenueCard;
        private System.Windows.Forms.Panel pnlChartCanvas;
        private System.Windows.Forms.Panel pnlSalesReport;
        private System.Windows.Forms.Panel pnlTotalView;
        private System.Windows.Forms.Panel pnlLevelUp;

        // New Bill page
        private System.Windows.Forms.Panel    pnlNewBill;
        private System.Windows.Forms.TextBox  txtBillCustomerName;
        private System.Windows.Forms.TextBox  txtBillPhone;
        private System.Windows.Forms.TextBox  txtBillItemName;
        private System.Windows.Forms.TextBox  txtBillQty;
        private System.Windows.Forms.TextBox  txtBillPrice;
        private System.Windows.Forms.Button   btnAddLineItem;
        private System.Windows.Forms.DataGridView dgvBillItems;
        private System.Windows.Forms.Button   btnSaveBill;
        private System.Windows.Forms.Button   btnClearBill;
        private System.Windows.Forms.Label    lblBillTotal;

        // Customers page
        private System.Windows.Forms.Panel    pnlCustomers;
        private System.Windows.Forms.DataGridView dgvCustomers;
        private System.Windows.Forms.Button   btnAddCustomer;
        private System.Windows.Forms.TextBox  txtCustSearch;

        // Products page
        private System.Windows.Forms.Panel    pnlProducts;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.Button   btnAddProductPage;
        private System.Windows.Forms.TextBox  txtProdSearch;

        // Transactions page
        private System.Windows.Forms.Panel    pnlTransPage;
        private System.Windows.Forms.DataGridView dgvAllTransactions;
        private System.Windows.Forms.TextBox  txtTransSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnClose             = new System.Windows.Forms.Button();
            btnMinimize          = new System.Windows.Forms.Button();
            btnMaximize          = new System.Windows.Forms.Button();
            pnlSidebar           = new System.Windows.Forms.Panel();
            btnNavOverview       = new System.Windows.Forms.Button();
            btnNavNewBill        = new System.Windows.Forms.Button();
            btnNavCustomers      = new System.Windows.Forms.Button();
            btnNavProducts       = new System.Windows.Forms.Button();
            btnNavTransactions   = new System.Windows.Forms.Button();
            btnNavMessages       = new System.Windows.Forms.Button();
            btnNavSettings       = new System.Windows.Forms.Button();
            btnNavSecurity       = new System.Windows.Forms.Button();
            lblSidebarUser       = new System.Windows.Forms.Label();
            lblSidebarEmail      = new System.Windows.Forms.Label();
            pnlTopBar            = new System.Windows.Forms.Panel();
            lblPageTitle         = new System.Windows.Forms.Label();
            lblPageSub           = new System.Windows.Forms.Label();
            pnlSearchBox         = new System.Windows.Forms.Panel();
            txtSearch            = new System.Windows.Forms.TextBox();
            btnTopAddProduct     = new System.Windows.Forms.Button();
            pnlContent           = new System.Windows.Forms.Panel();
            pnlOverview          = new System.Windows.Forms.Panel();
            pnlStatHero          = new System.Windows.Forms.Panel();
            pnlStatIncome        = new System.Windows.Forms.Panel();
            pnlStatReturn        = new System.Windows.Forms.Panel();
            pnlTransactions      = new System.Windows.Forms.Panel();
            dgvTransactions      = new System.Windows.Forms.DataGridView();
            pnlRevenueCard       = new System.Windows.Forms.Panel();
            pnlChartCanvas       = new System.Windows.Forms.Panel();
            pnlSalesReport       = new System.Windows.Forms.Panel();
            pnlTotalView         = new System.Windows.Forms.Panel();
            pnlLevelUp           = new System.Windows.Forms.Panel();
            pnlNewBill           = new System.Windows.Forms.Panel();
            txtBillCustomerName  = new System.Windows.Forms.TextBox();
            txtBillPhone         = new System.Windows.Forms.TextBox();
            txtBillItemName      = new System.Windows.Forms.TextBox();
            txtBillQty           = new System.Windows.Forms.TextBox();
            txtBillPrice         = new System.Windows.Forms.TextBox();
            btnAddLineItem       = new System.Windows.Forms.Button();
            dgvBillItems         = new System.Windows.Forms.DataGridView();
            btnSaveBill          = new System.Windows.Forms.Button();
            btnClearBill         = new System.Windows.Forms.Button();
            lblBillTotal         = new System.Windows.Forms.Label();
            pnlCustomers         = new System.Windows.Forms.Panel();
            dgvCustomers         = new System.Windows.Forms.DataGridView();
            btnAddCustomer       = new System.Windows.Forms.Button();
            txtCustSearch        = new System.Windows.Forms.TextBox();
            pnlProducts          = new System.Windows.Forms.Panel();
            dgvProducts          = new System.Windows.Forms.DataGridView();
            btnAddProductPage    = new System.Windows.Forms.Button();
            txtProdSearch        = new System.Windows.Forms.TextBox();
            pnlTransPage         = new System.Windows.Forms.Panel();
            dgvAllTransactions   = new System.Windows.Forms.DataGridView();
            txtTransSearch       = new System.Windows.Forms.TextBox();

            this.SuspendLayout();

            // Form
            this.Text            = "SB1 — Simple Billing Dashboard";
            this.Name            = "DashboardForm";
            this.ClientSize      = new System.Drawing.Size(1280, 780);
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor       = System.Drawing.Color.FromArgb(15, 23, 42);
            this.DoubleBuffered  = true;
            this.MinimumSize     = new System.Drawing.Size(1100, 700);

            // Chrome buttons — positioned dynamically in code
            SetupChromeBtn(btnClose,    "✕",
                System.Drawing.Color.FromArgb(254, 202, 202),
                System.Drawing.Color.FromArgb(252, 165, 165));
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

            // Sidebar
            pnlSidebar.Dock      = System.Windows.Forms.DockStyle.Left;
            pnlSidebar.Width     = 210;
            pnlSidebar.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);

            lblSidebarUser.AutoSize  = false;
            lblSidebarUser.Size      = new System.Drawing.Size(182, 20);
            lblSidebarUser.Text      = "Jenil Rana";
            lblSidebarUser.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            lblSidebarUser.ForeColor = System.Drawing.Color.White;
            lblSidebarUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblSidebarEmail.AutoSize  = false;
            lblSidebarEmail.Size      = new System.Drawing.Size(182, 18);
            lblSidebarEmail.Text      = "jenil@sb1billing.com";
            lblSidebarEmail.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Regular);
            lblSidebarEmail.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSidebarEmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnNavOverview, btnNavNewBill, btnNavCustomers,
                btnNavProducts, btnNavTransactions, btnNavMessages,
                btnNavSettings, btnNavSecurity,
                lblSidebarUser, lblSidebarEmail
            });

            // Top bar
            pnlTopBar.Dock      = System.Windows.Forms.DockStyle.Top;
            pnlTopBar.Height    = 68;
            pnlTopBar.BackColor = System.Drawing.Color.White;

            lblPageTitle.AutoSize  = false;
            lblPageTitle.Size      = new System.Drawing.Size(280, 32);
            lblPageTitle.Location  = new System.Drawing.Point(20, 8);
            lblPageTitle.Text      = "Dashboard";
            lblPageTitle.Font      = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold);
            lblPageTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblPageTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            lblPageSub.AutoSize  = false;
            lblPageSub.Size      = new System.Drawing.Size(440, 18);
            lblPageSub.Location  = new System.Drawing.Point(22, 42);
            lblPageSub.Text      = "An easy way to manage your bills with care and precision.";
            lblPageSub.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            lblPageSub.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            lblPageSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // Search box panel
            pnlSearchBox.BackColor   = System.Drawing.Color.FromArgb(248, 250, 252);
            pnlSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlSearchBox.Size        = new System.Drawing.Size(240, 36);

            txtSearch.BorderStyle     = System.Windows.Forms.BorderStyle.None;
            txtSearch.BackColor       = System.Drawing.Color.FromArgb(248, 250, 252);
            txtSearch.Font            = new System.Drawing.Font("Segoe UI", 9.5f);
            txtSearch.ForeColor       = System.Drawing.Color.FromArgb(71, 85, 105);
            txtSearch.PlaceholderText = "Search anything in SB1...";
            txtSearch.Location        = new System.Drawing.Point(10, 8);
            txtSearch.Size            = new System.Drawing.Size(214, 20);
            pnlSearchBox.Controls.Add(txtSearch);

            // "Add new product" top-right button
            btnTopAddProduct.Text      = "+ Add new product";
            btnTopAddProduct.Size      = new System.Drawing.Size(152, 36);
            btnTopAddProduct.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            btnTopAddProduct.ForeColor = System.Drawing.Color.White;
            btnTopAddProduct.BackColor = System.Drawing.Color.FromArgb(59, 130, 246);
            btnTopAddProduct.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnTopAddProduct.FlatAppearance.BorderSize = 0;
            btnTopAddProduct.Cursor    = System.Windows.Forms.Cursors.Hand;
            btnTopAddProduct.Click    += (s, e) => ShowPage("Products");

            pnlTopBar.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblPageTitle, lblPageSub, pnlSearchBox, btnTopAddProduct,
                btnClose, btnMinimize, btnMaximize
            });

            // Content area
            pnlContent.Dock      = System.Windows.Forms.DockStyle.Fill;
            pnlContent.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            pnlContent.Padding   = new System.Windows.Forms.Padding(20, 16, 20, 16);

            // Assemble form
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlTopBar);
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
            btn.Size      = new System.Drawing.Size(36, 30);
            btn.TabStop   = false;
            btn.Cursor    = System.Windows.Forms.Cursors.Hand;
            btn.BackColor = System.Drawing.Color.Transparent;
        }
    }
}
