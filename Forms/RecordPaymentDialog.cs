using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class RecordPaymentDialog : Form
    {
        private readonly Invoice _invoice;
        private readonly TextBox txtAmount = new();
        private readonly ComboBox cmbMethod = new();
        private readonly TextBox txtRef = new();
        private readonly TextBox txtNotes = new();
        private readonly DateTimePicker dtpDate = new();
        private readonly TeamPillButton btnSave = new();
        private readonly TeamPillButton btnCancel = new();

        public RecordPaymentDialog(Invoice invoice)
        {
            _invoice = invoice;
            InitializeForm();
            ApplyTheme();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 490);
            Text = $"Record Payment — {_invoice.InvoiceNumber}";

            var pnlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };

            int y = 16;

            // Invoice Summary Card
            var pnlSummary = new Panel
            {
                Bounds = new Rectangle(24, y, 470, 75),
                BackColor = Color.FromArgb(241, 245, 249)
            };
            var lblInfo = new Label
            {
                Text = $"Customer: {_invoice.CustomerName}\nGrand Total: ₹{_invoice.GrandTotal:N2}  |  Paid: ₹{_invoice.AmountPaid:N2}\nRemaining Balance Due: ₹{_invoice.BalanceDue:N2}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 12, 10)
            };
            pnlSummary.Controls.Add(lblInfo);
            pnlContainer.Controls.Add(pnlSummary);

            y += 90;

            int labelW = 140;
            int inputW = 310;
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

                input.SetBounds(160, y, inputW, inputH);
                input.Font = new Font("Segoe UI", 9.5f);

                pnlContainer.Controls.Add(lbl);
                pnlContainer.Controls.Add(input);
                y += gap;
            }

            // Default amount is remaining balance
            txtAmount.Text = _invoice.BalanceDue.ToString("F2");
            AddRow("Payment Amount *", txtAmount);

            cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMethod.Items.AddRange(new object[] { "UPI", "Bank Transfer", "Cash", "Card", "Other" });
            cmbMethod.SelectedIndex = 0;
            AddRow("Payment Method *", cmbMethod);

            dtpDate.Format = DateTimePickerFormat.Custom;
            dtpDate.CustomFormat = "dd MMM yyyy, hh:mm tt";
            dtpDate.Value = DateTime.Now;
            AddRow("Payment Date", dtpDate);

            txtRef.PlaceholderText = "e.g. UTR / UPI Ref / Cheque No.";
            AddRow("Reference / Txn ID", txtRef);

            txtNotes.PlaceholderText = "Optional notes or remark";
            AddRow("Remarks / Notes", txtNotes);

            y += 10;

            btnSave.Text = "Save Payment";
            btnSave.Icon = ReiconRenderer.IconType.Wallet;
            btnSave.FillColor = ThemeManager.StatusPositive;
            btnSave.HoverColor = Color.FromArgb(5, 150, 105);
            btnSave.SetBounds(230, y, 160, 38);
            btnSave.Click += (s, e) => SavePayment();

            btnCancel.Text = "Cancel";
            btnCancel.Icon = null;
            btnCancel.FillColor = Color.FromArgb(148, 163, 184);
            btnCancel.HoverColor = Color.FromArgb(100, 116, 139);
            btnCancel.SetBounds(400, y, 90, 38);
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
                else if (c is ComboBox cb)
                {
                    cb.BackColor = inputBg;
                    cb.ForeColor = inputFg;
                }
                else if (c is Panel pnl)
                {
                    pnl.BackColor = isDark ? Color.FromArgb(40, 44, 58) : Color.FromArgb(241, 245, 249);
                    foreach (Control sc in pnl.Controls)
                    {
                        if (sc is Label slbl) slbl.ForeColor = isDark ? Color.White : Color.FromArgb(17, 24, 39);
                    }
                }
            }
        }

        private void SavePayment()
        {
            if (!decimal.TryParse(txtAmount.Text, out decimal amt) || amt <= 0)
            {
                MessageBox.Show("Please enter a valid payment amount greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAmount.Focus();
                return;
            }

            if (amt > _invoice.BalanceDue)
            {
                var choice = MessageBox.Show(
                    $"Entered payment (₹{amt:N2}) exceeds remaining balance due (₹{_invoice.BalanceDue:N2}).\nDo you want to record this overpayment?",
                    "Overpayment Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choice != DialogResult.Yes) return;
            }

            var record = new PaymentRecord
            {
                InvoiceId = _invoice.Id,
                PaymentDate = dtpDate.Value,
                Amount = amt,
                PaymentMethod = cmbMethod.SelectedItem?.ToString() ?? "Cash",
                ReferenceNumber = txtRef.Text.Trim(),
                Notes = txtNotes.Text.Trim()
            };

            try
            {
                DatabaseService.RecordPayment(record);
                MessageBox.Show($"Payment of ₹{amt:N2} recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording payment: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
