using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class SupplierDialog : Form
    {
        private readonly Supplier _supplier;
        private readonly bool _isEdit;

        private readonly TextBox txtName = new();
        private readonly TextBox txtCompany = new();
        private readonly TextBox txtPhone = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtAddress = new();
        private readonly TextBox txtGSTIN = new();
        private readonly TeamPillButton btnSave = new();
        private readonly TeamPillButton btnCancel = new();

        public Supplier SavedSupplier => _supplier;

        public SupplierDialog(Supplier? existing = null)
        {
            _isEdit = existing != null;
            _supplier = existing ?? new Supplier();

            InitializeForm();
            ApplyTheme();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 480);
            Text = _isEdit ? "Edit Supplier / Vendor" : "Add New Supplier / Vendor";

            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };

            int y = 16;
            int labelW = 140;
            int inputW = 320;
            int inputH = 30;
            int gap = 44;

            void AddRow(string labelText, Control input, string initialVal = "")
            {
                var lbl = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Bounds = new Rectangle(24, y + 4, labelW, 24),
                    AutoSize = false
                };

                input.SetBounds(160, y, inputW, inputH);
                input.Font = new Font("Segoe UI", 9.5f);
                if (input is TextBox tb) tb.Text = initialVal;

                pnlContainer.Controls.Add(lbl);
                pnlContainer.Controls.Add(input);
                y += gap;
            }

            AddRow("Vendor Name *", txtName, _supplier.Name);
            AddRow("Company / Firm", txtCompany, _supplier.CompanyName);
            AddRow("Phone Number *", txtPhone, _supplier.Phone);
            AddRow("Email Address", txtEmail, _supplier.Email);
            AddRow("Address", txtAddress, _supplier.Address);
            AddRow("GSTIN (Optional)", txtGSTIN, _supplier.GSTIN);

            y += 10;

            btnSave.Text = _isEdit ? "Save Changes" : "Create Supplier";
            btnSave.Icon = ReiconRenderer.IconType.Plus;
            btnSave.FillColor = ThemeManager.PrimaryAccent;
            btnSave.HoverColor = ThemeManager.AccentHover;
            btnSave.SetBounds(250, y, 150, 38);
            btnSave.Click += (s, e) => SaveSupplier();

            btnCancel.Text = "Cancel";
            btnCancel.Icon = null;
            btnCancel.FillColor = Color.FromArgb(148, 163, 184);
            btnCancel.HoverColor = Color.FromArgb(100, 116, 139);
            btnCancel.SetBounds(410, y, 80, 38);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            pnlContainer.Controls.Add(btnSave);
            pnlContainer.Controls.Add(btnCancel);
            Controls.Add(pnlContainer);
        }

        private void SaveSupplier()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the supplier / vendor name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter the contact phone number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            _supplier.Name = txtName.Text.Trim();
            _supplier.CompanyName = txtCompany.Text.Trim();
            _supplier.Phone = txtPhone.Text.Trim();
            _supplier.Email = txtEmail.Text.Trim();
            _supplier.Address = txtAddress.Text.Trim();
            _supplier.GSTIN = txtGSTIN.Text.Trim();

            try
            {
                DatabaseService.SaveSupplier(_supplier);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save supplier: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                else if (c is Label lbl)
                {
                    lbl.ForeColor = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
                }

                if (c.HasChildren)
                    ApplyThemeRecursive(c, isDark);
            }
        }
    }
}
