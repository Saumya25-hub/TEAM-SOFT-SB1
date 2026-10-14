using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class CustomerDialog : Form
    {
        private readonly Customer _customer;
        private readonly bool _isEdit;

        private readonly TextBox txtName = new();
        private readonly TextBox txtCompany = new();
        private readonly TextBox txtPhone = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtAddress = new();
        private readonly TextBox txtGSTIN = new();
        private readonly TeamPillButton btnSave = new();
        private readonly TeamPillButton btnCancel = new();

        public Customer SavedCustomer => _customer;

        public CustomerDialog(Customer? existing = null)
        {
            _isEdit = existing != null;
            _customer = existing ?? new Customer();

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
            Text = _isEdit ? "Edit Customer Details" : "Add New Customer";

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

            AddRow("Customer Name *", txtName, _customer.Name);
            AddRow("Company Name", txtCompany, _customer.CompanyName);
            AddRow("Phone Number *", txtPhone, _customer.Phone);
            AddRow("Email Address", txtEmail, _customer.Email);
            AddRow("Billing Address", txtAddress, _customer.BillingAddress);
            AddRow("GSTIN (Optional)", txtGSTIN, _customer.GSTIN);

            y += 10;

            btnSave.Text = _isEdit ? "Save Changes" : "Create Customer";
            btnSave.Icon = ReiconRenderer.IconType.Plus;
            btnSave.FillColor = ThemeManager.PrimaryAccent;
            btnSave.HoverColor = ThemeManager.AccentHover;
            btnSave.SetBounds(250, y, 150, 38);
            btnSave.Click += (s, e) => SaveCustomer();

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
            }
        }

        private void SaveCustomer()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the customer name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please enter a valid phone number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return;
            }

            _customer.Name = txtName.Text.Trim();
            _customer.CompanyName = txtCompany.Text.Trim();
            _customer.Phone = txtPhone.Text.Trim();
            _customer.Email = txtEmail.Text.Trim();
            _customer.BillingAddress = txtAddress.Text.Trim();
            _customer.GSTIN = txtGSTIN.Text.Trim().ToUpperInvariant();

            try
            {
                DatabaseService.SaveCustomer(_customer);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save customer: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
