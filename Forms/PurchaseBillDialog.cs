using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class PurchaseBillDialog : Form
    {
        private readonly PurchaseBill _bill;
        private readonly bool _isEdit;

        private readonly TextBox txtBillNum = new();
        private readonly ComboBox cmbSupplier = new();
        private readonly DateTimePicker dtpBillDate = new();
        private readonly DateTimePicker dtpDueDate = new();
        private readonly ComboBox cmbProduct = new();
        private readonly TextBox txtItemName = new();
        private readonly NumericUpDown nudQty = new();
        private readonly TextBox txtUnitCost = new();
        private readonly ComboBox cmbTaxRate = new();
        private readonly Label lblComputedTotal = new();
        private readonly TextBox txtAmountPaid = new();
        private readonly TextBox txtNotes = new();
        private readonly TeamPillButton btnSave = new();
        private readonly TeamPillButton btnCancel = new();

        private List<Supplier> _suppliers = new();
        private List<ProductItem> _products = new();

        public PurchaseBill SavedBill => _bill;

        public PurchaseBillDialog(PurchaseBill? existing = null)
        {
            _isEdit = existing != null;
            _bill = existing ?? new PurchaseBill
            {
                BillNumber = DatabaseService.GetNextPurchaseBillNumber()
            };

            InitializeForm();
            LoadData();
            ApplyTheme();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(580, 560);
            Text = _isEdit ? "Edit Purchase Bill" : "+ Create New Purchase Bill (Vendor Entry)";

            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };

            int y = 14;
            int labelW = 150;
            int inputW = 360;
            int inputH = 30;
            int gap = 42;

            void AddRow(string labelText, Control input)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Bounds = new Rectangle(24, y + 4, labelW, 24),
                    AutoSize = false
                };

                input.SetBounds(170, y, inputW, inputH);
                input.Font = new Font("Segoe UI", 9.5f);

                pnlContainer.Controls.Add(lbl);
                pnlContainer.Controls.Add(input);
                y += gap;
            }

            // 1. Bill Number
            txtBillNum.Text = _bill.BillNumber;
            AddRow("Purchase Bill # *", txtBillNum);

            // 2. Supplier
            cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            AddRow("Vendor / Supplier *", cmbSupplier);

            // 3. Bill Date
            dtpBillDate.Format = DateTimePickerFormat.Short;
            dtpBillDate.Value = _bill.BillDate;
            AddRow("Bill Date", dtpBillDate);

            // 4. Due Date
            dtpDueDate.Format = DateTimePickerFormat.Short;
            dtpDueDate.Value = _bill.DueDate;
            AddRow("Due Date", dtpDueDate);

            // 5. Product Catalog Select
            cmbProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProduct.SelectedIndexChanged += (s, e) =>
            {
                if (cmbProduct.SelectedItem is ProductItem p)
                {
                    txtItemName.Text = p.Name;
                    txtUnitCost.Text = p.CostPrice.ToString("F2");
                    Recalculate();
                }
            };
            AddRow("Select Catalog Item", cmbProduct);

            // 6. Item Name
            txtItemName.Text = _bill.Items.Count > 0 ? _bill.Items[0].ItemName : "";
            AddRow("Item / Description *", txtItemName);

            // 7. Quantity & Unit Cost (Side-by-Side)
            var pnlQtyCost = new Panel { Bounds = new Rectangle(170, y, inputW, inputH) };
            nudQty.Minimum = 1;
            nudQty.Maximum = 10000;
            nudQty.Value = 1;
            nudQty.SetBounds(0, 0, 100, inputH);
            nudQty.Font = new Font("Segoe UI", 9.5f);
            nudQty.ValueChanged += (s, e) => Recalculate();

            var lblCost = new Label { Text = "Cost (₹):", Font = new Font("Segoe UI", 9f, FontStyle.Bold), Bounds = new Rectangle(110, 4, 60, 24) };
            txtUnitCost.Text = "0.00";
            txtUnitCost.SetBounds(175, 0, 185, inputH);
            txtUnitCost.Font = new Font("Segoe UI", 9.5f);
            txtUnitCost.TextChanged += (s, e) => Recalculate();

            pnlQtyCost.Controls.AddRange(new Control[] { nudQty, lblCost, txtUnitCost });
            var lblQtyTitle = new Label
            {
                Text = "Quantity & Cost *",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Bounds = new Rectangle(24, y + 4, labelW, 24),
                AutoSize = false
            };
            pnlContainer.Controls.Add(lblQtyTitle);
            pnlContainer.Controls.Add(pnlQtyCost);
            y += gap;

            // 8. Tax Rate
            cmbTaxRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTaxRate.Items.AddRange(new object[] { "18% GST (Intra-State)", "18% IGST (Inter-State)", "12% GST", "5% GST", "0% (Exempt)" });
            cmbTaxRate.SelectedIndex = 0;
            cmbTaxRate.SelectedIndexChanged += (s, e) => Recalculate();
            AddRow("GST Tax Scheme", cmbTaxRate);

            // 9. Computed Total Badge
            lblComputedTotal.Text = "Total: ₹0.00 (Tax Included)";
            lblComputedTotal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblComputedTotal.ForeColor = ThemeManager.PrimaryAccent;
            AddRow("Computed Grand Total", lblComputedTotal);

            // 10. Initial Amount Paid
            txtAmountPaid.Text = _bill.AmountPaid.ToString("F2");
            txtAmountPaid.TextChanged += (s, e) => Recalculate();
            AddRow("Amount Paid (₹)", txtAmountPaid);

            // 11. Notes
            txtNotes.Text = _bill.Notes;
            AddRow("Notes / Remarks", txtNotes);

            y += 8;

            btnSave.Text = _isEdit ? "Update Bill" : "+ Save Purchase Bill";
            btnSave.Icon = ReiconRenderer.IconType.Plus;
            btnSave.FillColor = ThemeManager.PrimaryAccent;
            btnSave.HoverColor = ThemeManager.AccentHover;
            btnSave.SetBounds(270, y, 170, 38);
            btnSave.Click += (s, e) => SaveBill();

            btnCancel.Text = "Cancel";
            btnCancel.Icon = null;
            btnCancel.FillColor = Color.FromArgb(148, 163, 184);
            btnCancel.HoverColor = Color.FromArgb(100, 116, 139);
            btnCancel.SetBounds(450, y, 80, 38);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlContainer.Controls.Add(btnSave);
            pnlContainer.Controls.Add(btnCancel);
            Controls.Add(pnlContainer);
        }

        private void LoadData()
        {
            _suppliers = DatabaseService.GetSuppliers();
            cmbSupplier.Items.Clear();
            foreach (var s in _suppliers)
            {
                cmbSupplier.Items.Add(s);
            }
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;

            _products = DatabaseService.GetProducts();
            cmbProduct.Items.Clear();
            cmbProduct.Items.Add("-- Select Product from Catalog --");
            foreach (var p in _products)
            {
                cmbProduct.Items.Add(p);
            }
            cmbProduct.SelectedIndex = 0;

            Recalculate();
        }

        private void Recalculate()
        {
            decimal qty = nudQty.Value;
            decimal.TryParse(txtUnitCost.Text.Trim(), out decimal unitCost);
            decimal sub = qty * unitCost;

            decimal taxPct = cmbTaxRate.SelectedIndex switch
            {
                0 => 18m,
                1 => 18m,
                2 => 12m,
                3 => 5m,
                _ => 0m
            };

            decimal tax = Math.Round(sub * (taxPct / 100m), 2);
            decimal grand = sub + tax;

            decimal.TryParse(txtAmountPaid.Text.Trim(), out decimal paid);
            decimal bal = Math.Max(0m, grand - paid);

            lblComputedTotal.Text = $"Total: ₹{grand:N2} (Tax: ₹{tax:N2}) | Bal: ₹{bal:N2}";
        }

        private void SaveBill()
        {
            if (string.IsNullOrWhiteSpace(txtBillNum.Text))
            {
                MessageBox.Show("Please enter the purchase bill number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBillNum.Focus();
                return;
            }

            if (cmbSupplier.SelectedItem is not Supplier sup)
            {
                MessageBox.Show("Please select a vendor / supplier.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbSupplier.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Please enter an item name or select a product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtItemName.Focus();
                return;
            }

            decimal.TryParse(txtUnitCost.Text.Trim(), out decimal unitCost);
            if (unitCost <= 0)
            {
                MessageBox.Show("Please enter a valid unit cost greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnitCost.Focus();
                return;
            }

            decimal.TryParse(txtAmountPaid.Text.Trim(), out decimal paid);

            long prodId = 0;
            if (cmbProduct.SelectedItem is ProductItem p) prodId = p.Id;

            decimal taxPct = cmbTaxRate.SelectedIndex switch
            {
                0 => 18m,
                1 => 18m,
                2 => 12m,
                3 => 5m,
                _ => 0m
            };

            string taxType = cmbTaxRate.SelectedIndex == 1 ? "GST_INTER" : (taxPct > 0 ? "GST_INTRA" : "EXEMPT");

            _bill.BillNumber = txtBillNum.Text.Trim();
            _bill.SupplierId = sup.Id;
            _bill.SupplierName = sup.Name;
            _bill.SupplierCompany = sup.CompanyName;
            _bill.SupplierPhone = sup.Phone;
            _bill.SupplierGSTIN = sup.GSTIN;
            _bill.BillDate = dtpBillDate.Value.Date;
            _bill.DueDate = dtpDueDate.Value.Date;
            _bill.TaxType = taxType;
            _bill.AmountPaid = paid;
            _bill.Notes = txtNotes.Text.Trim();

            _bill.Items.Clear();
            _bill.Items.Add(new PurchaseBillItem
            {
                ProductId = prodId,
                ItemName = txtItemName.Text.Trim(),
                Quantity = nudQty.Value,
                UnitCost = unitCost,
                TaxPercent = taxPct
            });

            _bill.RecalculateTotals();

            try
            {
                DatabaseService.SavePurchaseBill(_bill);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save purchase bill: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = isDark ? Color.FromArgb(24, 28, 42) : Color.White;
            ForeColor = ThemeManager.TextPrimary;

            foreach (Control c in Controls)
            {
                ApplyThemeRecursive(c, isDark);
            }
        }

        private void ApplyThemeRecursive(Control parent, bool isDark)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox tb)
                {
                    tb.BackColor = isDark ? Color.FromArgb(32, 36, 52) : Color.White;
                    tb.ForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = isDark ? Color.FromArgb(32, 36, 52) : Color.White;
                    cb.ForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = isDark ? Color.FromArgb(32, 36, 52) : Color.White;
                    nud.ForeColor = isDark ? Color.White : Color.FromArgb(15, 23, 42);
                }
                else if (c is Label lbl && lbl != lblComputedTotal)
                {
                    lbl.ForeColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
                }

                if (c.HasChildren)
                    ApplyThemeRecursive(c, isDark);
            }
        }
    }
}
