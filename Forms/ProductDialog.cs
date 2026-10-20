using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class ProductDialog : Form
    {
        private readonly ProductItem _product;
        private readonly bool _isEdit;

        private readonly TextBox txtBarcode = new();
        private readonly TextBox txtName = new();
        private readonly ComboBox cboCategory = new();
        private readonly TextBox txtHSN = new();
        private readonly ComboBox cboUnit = new();
        private readonly NumericUpDown nudCost = new();
        private readonly NumericUpDown nudPrice = new();
        private readonly NumericUpDown nudStock = new();
        private readonly NumericUpDown nudMinAlert = new();
        private readonly ComboBox cboTaxRate = new();

        private readonly TeamPillButton btnSave = new();
        private readonly TeamPillButton btnCancel = new();

        public ProductItem SavedProduct => _product;

        public ProductDialog(ProductItem? existing = null)
        {
            _isEdit = existing != null;
            _product = existing ?? new ProductItem();

            InitializeForm();
            ApplyTheme();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(540, 560);
            Text = _isEdit ? $"Edit Product — {_product.Name}" : "Add New Inventory Product";

            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                AutoScroll = true
            };

            int y = 16;
            int labelW = 150;
            int inputW = 320;
            int inputH = 30;
            int gap = 44;

            void AddRow(string labelText, Control input)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Bounds = new Rectangle(24, y + 4, labelW, 24),
                    AutoSize = false
                };

                input.SetBounds(180, y, inputW, inputH);
                input.Font = new Font("Segoe UI", 9.5f);

                pnlContainer.Controls.Add(lbl);
                pnlContainer.Controls.Add(input);
                y += gap;
            }

            // 1. Barcode / SKU
            txtBarcode.Text = string.IsNullOrWhiteSpace(_product.Barcode) 
                ? $"890{DateTime.Now:yyMMddHHmmss}" 
                : _product.Barcode;
            AddRow("Barcode / SKU *", txtBarcode);

            // 2. Product Name
            txtName.Text = _product.Name;
            AddRow("Product Name *", txtName);

            // 3. Category
            cboCategory.DropDownStyle = ComboBoxStyle.DropDown;
            cboCategory.Items.AddRange(new object[] { "Electronics", "Groceries", "Clothing", "Hardware", "Networking", "Software", "Services", "Office Supplies", "General" });
            cboCategory.Text = string.IsNullOrWhiteSpace(_product.Category) ? "Electronics" : _product.Category;
            AddRow("Category", cboCategory);

            // 4. HSN Code
            txtHSN.Text = string.IsNullOrWhiteSpace(_product.HSN) ? "8471" : _product.HSN;
            AddRow("HSN Code", txtHSN);

            // 5. Unit
            cboUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            cboUnit.Items.AddRange(new object[] { "PCS", "Boxes", "Roll", "Kg", "Ltr", "Mtr", "Pkt", "Hours", "Service" });
            cboUnit.SelectedItem = string.IsNullOrWhiteSpace(_product.Unit) ? "PCS" : _product.Unit;
            if (cboUnit.SelectedIndex < 0) cboUnit.SelectedIndex = 0;
            AddRow("Unit of Measure", cboUnit);

            // 6. Cost Price
            nudCost.DecimalPlaces = 2;
            nudCost.Maximum = 10000000m;
            nudCost.Value = Math.Max(0m, _product.CostPrice);
            AddRow("Cost Price (₹)", nudCost);

            // 7. Selling Rate
            nudPrice.DecimalPlaces = 2;
            nudPrice.Maximum = 10000000m;
            nudPrice.Value = Math.Max(0m, _product.SalePrice);
            AddRow("Sale Price (₹) *", nudPrice);

            // 8. Available Quantity
            nudStock.DecimalPlaces = 2;
            nudStock.Maximum = 1000000m;
            nudStock.Value = Math.Max(0m, _product.StockQuantity);
            AddRow("Current Stock Qty", nudStock);

            // 9. Reorder Level (Min Alert)
            nudMinAlert.DecimalPlaces = 0;
            nudMinAlert.Maximum = 10000m;
            nudMinAlert.Value = _product.MinStockAlert > 0 ? _product.MinStockAlert : 10m;
            AddRow("Reorder Level Alert", nudMinAlert);

            // 10. Tax Rate
            cboTaxRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTaxRate.Items.AddRange(new object[] { "0% GST", "5% GST", "12% GST", "18% GST", "28% GST" });
            cboTaxRate.SelectedIndex = _product.TaxRate switch
            {
                0m => 0,
                5m => 1,
                12m => 2,
                18m => 3,
                28m => 4,
                _ => 3
            };
            AddRow("GST Tax Rate", cboTaxRate);

            y += 10;

            btnSave.Text = _isEdit ? "Update Product" : "Save Product";
            btnSave.Icon = ReiconRenderer.IconType.Plus;
            btnSave.FillColor = ThemeManager.PrimaryAccent;
            btnSave.HoverColor = ThemeManager.AccentHover;
            btnSave.SetBounds(260, y, 140, 38);
            btnSave.Click += (s, e) => SaveProduct();

            btnCancel.Text = "Cancel";
            btnCancel.Icon = null;
            btnCancel.FillColor = Color.FromArgb(148, 163, 184);
            btnCancel.HoverColor = Color.FromArgb(100, 116, 139);
            btnCancel.SetBounds(410, y, 90, 38);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlContainer.Controls.Add(btnSave);
            pnlContainer.Controls.Add(btnCancel);
            Controls.Add(pnlContainer);
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = ThemeManager.CardBackground;
            ForeColor = ThemeManager.TextPrimary;

            Color inputBg = isDark ? Color.FromArgb(30, 32, 48) : Color.White;
            Color inputFg = ThemeManager.TextPrimary;

            foreach (Control c in Controls[0].Controls)
            {
                if (c is Label lbl)
                {
                    lbl.ForeColor = ThemeManager.TextPrimary;
                }
                else if (c is TextBox tb)
                {
                    tb.BackColor = inputBg;
                    tb.ForeColor = inputFg;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = inputBg;
                    nud.ForeColor = inputFg;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = inputBg;
                    cb.ForeColor = inputFg;
                }
            }
        }

        private void SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the product name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (nudPrice.Value <= 0)
            {
                MessageBox.Show("Sale price must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudPrice.Focus();
                return;
            }

            _product.Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? $"890{DateTime.Now:yyMMddHHmmss}" : txtBarcode.Text.Trim();
            _product.Name = txtName.Text.Trim();
            _product.Category = string.IsNullOrWhiteSpace(cboCategory.Text) ? "General" : cboCategory.Text.Trim();
            _product.HSN = string.IsNullOrWhiteSpace(txtHSN.Text) ? "8471" : txtHSN.Text.Trim();
            _product.Unit = cboUnit.SelectedItem?.ToString() ?? "PCS";
            _product.CostPrice = nudCost.Value;
            _product.SalePrice = nudPrice.Value;
            _product.StockQuantity = nudStock.Value;
            _product.MinStockAlert = nudMinAlert.Value;

            _product.TaxRate = cboTaxRate.SelectedIndex switch
            {
                0 => 0m,
                1 => 5m,
                2 => 12m,
                3 => 18m,
                4 => 28m,
                _ => 18m
            };

            try
            {
                DatabaseService.SaveProduct(_product);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
