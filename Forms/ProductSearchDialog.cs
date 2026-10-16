using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class ProductSearchDialog : Form
    {
        private readonly TextBox _txtSearch = new();
        private readonly DataGridView _dgvProducts = new();
        private readonly TeamPillButton _btnSelect = new();
        private readonly TeamPillButton _btnCancel = new();
        private readonly TeamPillButton _btnQuickAdd = new();
        private List<ProductItem> _products = new();

        public ProductItem? SelectedProduct { get; private set; }

        public ProductSearchDialog()
        {
            InitializeComponentCustom();
            LoadProducts();
        }

        private void InitializeComponentCustom()
        {
            Text = "Product Master Catalog & Search — S TOM'S";
            Size = new Size(880, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(20, 24, 36) : Color.FromArgb(248, 250, 252);

            // Header Panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 12, 20, 0),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(26, 31, 46) : Color.White
            };

            var lblTitle = new Label
            {
                Text = "Manual Product Search & Selection",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.IsDarkMode ? Color.White : Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            var lblSubtitle = new Label
            {
                Text = "Search by product name, barcode/SKU, category, or HSN code to add line items",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(20, 32)
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // Search Bar Panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(20, 12, 20, 12),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(24, 28, 42) : Color.White
            };

            _txtSearch.PlaceholderText = "Search products by name, barcode, SKU or HSN (Press Enter)...";
            _txtSearch.SetBounds(20, 12, 630, 32);
            _txtSearch.Font = new Font("Segoe UI", 10f);
            _txtSearch.TextChanged += (s, e) => FilterProducts();
            _txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    if (_dgvProducts.Rows.Count > 0)
                    {
                        SelectCurrentRow();
                    }
                }
            };

            _btnQuickAdd.Text = "+ Add New Product";
            _btnQuickAdd.SetBounds(660, 10, 180, 34);
            _btnQuickAdd.FillColor = Color.FromArgb(16, 185, 129);
            _btnQuickAdd.ForeColor = Color.White;
            _btnQuickAdd.Click += OnQuickAddProduct;

            pnlSearch.Controls.AddRange(new Control[] { _txtSearch, _btnQuickAdd });

            // Products Grid
            _dgvProducts.Dock = DockStyle.Fill;
            _dgvProducts.BackgroundColor = ThemeManager.IsDarkMode ? Color.FromArgb(20, 24, 36) : Color.White;
            _dgvProducts.BorderStyle = BorderStyle.None;
            _dgvProducts.RowHeadersVisible = false;
            _dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvProducts.MultiSelect = false;
            _dgvProducts.AllowUserToAddRows = false;
            _dgvProducts.ReadOnly = true;
            _dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvProducts.RowTemplate.Height = 36;
            _dgvProducts.CellDoubleClick += (s, e) => SelectCurrentRow();
            _dgvProducts.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SelectCurrentRow();
                }
            };

            _dgvProducts.Columns.Add("Barcode", "Barcode / SKU");
            _dgvProducts.Columns["Barcode"].FillWeight = 20;
            _dgvProducts.Columns.Add("Name", "Product Name");
            _dgvProducts.Columns["Name"].FillWeight = 32;
            _dgvProducts.Columns.Add("Category", "Category");
            _dgvProducts.Columns["Category"].FillWeight = 16;
            _dgvProducts.Columns.Add("HSN", "HSN");
            _dgvProducts.Columns["HSN"].FillWeight = 12;
            _dgvProducts.Columns.Add("Unit", "Unit");
            _dgvProducts.Columns["Unit"].FillWeight = 10;
            _dgvProducts.Columns.Add("Price", "Rate (₹)");
            _dgvProducts.Columns["Price"].FillWeight = 14;
            _dgvProducts.Columns.Add("Stock", "Stock");
            _dgvProducts.Columns["Stock"].FillWeight = 14;

            // Footer Panel
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20, 12, 20, 12),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(26, 31, 46) : Color.FromArgb(241, 245, 249)
            };

            _btnSelect.Text = "Add to Invoice";
            _btnSelect.SetBounds(710, 12, 130, 36);
            _btnSelect.FillColor = Color.FromArgb(37, 99, 235);
            _btnSelect.ForeColor = Color.White;
            _btnSelect.Click += (s, e) => SelectCurrentRow();

            _btnCancel.Text = "Cancel";
            _btnCancel.SetBounds(600, 12, 100, 36);
            _btnCancel.FillColor = Color.FromArgb(148, 163, 184);
            _btnCancel.ForeColor = Color.White;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlFooter.Controls.AddRange(new Control[] { _btnCancel, _btnSelect });

            Controls.Add(_dgvProducts);
            Controls.Add(pnlSearch);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);
        }

        private void LoadProducts()
        {
            _products = DatabaseService.GetProducts();
            PopulateGrid(_products);
        }

        private void FilterProducts()
        {
            string query = _txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                PopulateGrid(_products);
                return;
            }

            var filtered = DatabaseService.GetProducts(query);
            PopulateGrid(filtered);
        }

        private void PopulateGrid(List<ProductItem> items)
        {
            _dgvProducts.Rows.Clear();
            foreach (var p in items)
            {
                int rIdx = _dgvProducts.Rows.Add(
                    p.Barcode,
                    p.Name,
                    p.Category,
                    p.HSN,
                    p.Unit,
                    $"₹{p.SalePrice:N2}",
                    $"{p.StockQuantity} {p.Unit}"
                );
                _dgvProducts.Rows[rIdx].Tag = p;
            }
        }

        private void SelectCurrentRow()
        {
            if (_dgvProducts.CurrentRow?.Tag is ProductItem p)
            {
                SelectedProduct = p;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a product from the list.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnQuickAddProduct(object? sender, EventArgs e)
        {
            // Simple prompt to quickly register a new product
            using var quickForm = new Form
            {
                Text = "Quick Add Product",
                Size = new Size(420, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl1 = new Label { Text = "Barcode / SKU *", Location = new Point(20, 15), AutoSize = true };
            var txtBarcode = new TextBox { Text = $"890123400{_products.Count + 1}", Location = new Point(20, 35), Width = 360 };

            var lbl2 = new Label { Text = "Product Name *", Location = new Point(20, 70), AutoSize = true };
            var txtName = new TextBox { Location = new Point(20, 90), Width = 360 };

            var lbl3 = new Label { Text = "HSN Code", Location = new Point(20, 125), AutoSize = true };
            var txtHsn = new TextBox { Text = "8471", Location = new Point(20, 145), Width = 170 };

            var lbl4 = new Label { Text = "Unit", Location = new Point(210, 125), AutoSize = true };
            var txtUnit = new TextBox { Text = "PCS", Location = new Point(210, 145), Width = 170 };

            var lbl5 = new Label { Text = "Selling Rate (₹) *", Location = new Point(20, 180), AutoSize = true };
            var nudRate = new NumericUpDown { Location = new Point(20, 200), Width = 170, DecimalPlaces = 2, Maximum = 1000000, Value = 100 };

            var lbl6 = new Label { Text = "Initial Stock", Location = new Point(210, 180), AutoSize = true };
            var nudStock = new NumericUpDown { Location = new Point(210, 200), Width = 170, DecimalPlaces = 0, Maximum = 100000, Value = 50 };

            var btnSave = new Button { Text = "Save & Add", Location = new Point(240, 280), Width = 140, Height = 36, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, DialogResult = DialogResult.OK };
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Product name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    quickForm.DialogResult = DialogResult.None;
                    return;
                }

                var item = new ProductItem
                {
                    Barcode = txtBarcode.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    HSN = txtHsn.Text.Trim(),
                    Unit = txtUnit.Text.Trim(),
                    SalePrice = nudRate.Value,
                    StockQuantity = nudStock.Value,
                    TaxRate = 18m,
                    Category = "General"
                };
                DatabaseService.SaveProduct(item);
                SelectedProduct = item;
            };

            quickForm.Controls.AddRange(new Control[] { lbl1, txtBarcode, lbl2, txtName, lbl3, txtHsn, lbl4, txtUnit, lbl5, nudRate, lbl6, nudStock, btnSave });
            if (quickForm.ShowDialog(this) == DialogResult.OK && SelectedProduct != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                LoadProducts();
            }
        }
    }
}
