using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class InvoiceDetailsForm : Form
    {
        private Invoice _invoice;
        private readonly BusinessProfile _business;

        private readonly Label lblHeader = new();
        private readonly Label lblCustInfo = new();
        private readonly Label lblSummary = new();
        private readonly DataGridView dgvItems = new();
        private readonly DataGridView dgvPayments = new();
        private readonly TeamPillButton btnPrint = new();
        private readonly TeamPillButton btnPreview = new();
        private readonly TeamPillButton btnPdf = new();
        private readonly TeamPillButton btnRecordPay = new();
        private readonly TeamPillButton btnCancelInv = new();
        private readonly TeamPillButton btnClose = new();

        public InvoiceDetailsForm(Invoice invoice)
        {
            _invoice = invoice;
            _business = DatabaseService.GetBusinessProfile();

            InitializeForm();
            LoadInvoiceData();
            ApplyTheme();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(880, 720);
            Text = $"Invoice View — {_invoice.InvoiceNumber}";

            var pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                AutoScroll = true
            };

            lblHeader.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblHeader.SetBounds(24, 16, 830, 36);

            lblCustInfo.Font = new Font("Segoe UI", 9.5f);
            lblCustInfo.SetBounds(24, 56, 830, 60);

            // Item Table Label
            var lblItemHeader = new Label
            {
                Text = "Billable Services & Products",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Bounds = new Rectangle(24, 124, 400, 26)
            };

            dgvItems.SetBounds(24, 154, 830, 160);
            dgvItems.Columns.Add("Service", "Service / Product");
            dgvItems.Columns.Add("Desc", "Description");
            dgvItems.Columns.Add("Qty", "Qty");
            dgvItems.Columns.Add("Price", "Unit Price (₹)");
            dgvItems.Columns.Add("Disc", "Disc (₹)");
            dgvItems.Columns.Add("Tax", "GST %");
            dgvItems.Columns.Add("Total", "Line Total (₹)");

            dgvItems.Columns["Qty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Qty"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Price"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Disc"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Disc"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Tax"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Tax"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns["Total"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            TableDesignStandard.ApplyStandardGrid(dgvItems, new TableStyleOptions { HeaderHeight = 32, RowHeight = 28, EmptyStateMessage = "No line items found" });

            // Financial Summary Block
            lblSummary.Font = new Font("Segoe UI", 10f);
            lblSummary.SetBounds(480, 324, 374, 130);

            // Payments Table Label
            var lblPayHeader = new Label
            {
                Text = "Payment History & Transactions",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Bounds = new Rectangle(24, 420, 400, 26)
            };

            dgvPayments.SetBounds(24, 450, 830, 140);
            dgvPayments.Columns.Add("Date", "Payment Date");
            dgvPayments.Columns.Add("Amount", "Amount (₹)");
            dgvPayments.Columns.Add("Method", "Mode");
            dgvPayments.Columns.Add("Ref", "Reference / UTR");
            dgvPayments.Columns.Add("Notes", "Notes");

            dgvPayments.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvPayments.Columns["Amount"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

            TableDesignStandard.ApplyStandardGrid(dgvPayments, new TableStyleOptions { HeaderHeight = 32, RowHeight = 28, EmptyStateMessage = "No payments recorded yet" });

            // Action Buttons
            int btnY = 610;
            btnPrint.Text = "Print";
            btnPrint.Icon = ReiconRenderer.IconType.Receipt;
            btnPrint.FillColor = ThemeManager.PrimaryAccent;
            btnPrint.SetBounds(24, btnY, 110, 38);
            btnPrint.Click += (s, e) => InvoicePrintService.PrintDirect(_invoice, _business, this);

            btnPreview.Text = "Preview";
            btnPreview.Icon = null;
            btnPreview.FillColor = Color.FromArgb(79, 70, 229);
            btnPreview.SetBounds(144, btnY, 110, 38);
            btnPreview.Click += (s, e) => InvoicePrintService.ShowPrintPreview(_invoice, _business, this);

            btnPdf.Text = "Export PDF";
            btnPdf.Icon = null;
            btnPdf.FillColor = Color.FromArgb(37, 99, 235);
            btnPdf.SetBounds(264, btnY, 130, 38);
            btnPdf.Click += (s, e) => InvoicePrintService.OpenOrShareInvoice(_invoice, _business);

            btnRecordPay.Text = "+ Add Payment";
            btnRecordPay.Icon = ReiconRenderer.IconType.Wallet;
            btnRecordPay.FillColor = ThemeManager.StatusPositive;
            btnRecordPay.SetBounds(404, btnY, 150, 38);
            btnRecordPay.Click += (s, e) =>
            {
                using var dlg = new RecordPaymentDialog(_invoice);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadInvoice();
                }
            };

            btnCancelInv.Text = "Cancel Invoice";
            btnCancelInv.Icon = ReiconRenderer.IconType.Close;
            btnCancelInv.FillColor = ThemeManager.StatusNegative;
            btnCancelInv.SetBounds(564, btnY, 140, 38);
            btnCancelInv.Click += (s, e) =>
            {
                if (MessageBox.Show($"Are you sure you want to cancel invoice {_invoice.InvoiceNumber}? This cannot be undone.",
                    "Cancel Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseService.CancelInvoice(_invoice.Id);
                    ReloadInvoice();
                }
            };

            btnClose.Text = "Close";
            btnClose.Icon = null;
            btnClose.FillColor = Color.FromArgb(148, 163, 184);
            btnClose.SetBounds(714, btnY, 90, 38);
            btnClose.Click += (s, e) => Close();

            pnlMain.Controls.AddRange(new Control[] {
                lblHeader, lblCustInfo, lblItemHeader, dgvItems, lblSummary,
                lblPayHeader, dgvPayments, btnPrint, btnPreview, btnPdf,
                btnRecordPay, btnCancelInv, btnClose
            });

            Controls.Add(pnlMain);
        }

        private void LoadInvoiceData()
        {
            lblHeader.Text = $"INVOICE {_invoice.InvoiceNumber} — Status: {_invoice.Status.ToUpper()}";
            lblCustInfo.Text = $"Customer: {_invoice.CustomerName} ({_invoice.CustomerCompany})\n" +
                               $"Billing Address: {_invoice.CustomerAddress}  |  GSTIN: {_invoice.CustomerGSTIN}\n" +
                               $"Invoice Date: {_invoice.InvoiceDate:dd MMM yyyy}  |  Due Date: {_invoice.DueDate:dd MMM yyyy}  |  Terms: {_invoice.PaymentTerms}";

            dgvItems.Rows.Clear();
            foreach (var it in _invoice.Items)
            {
                dgvItems.Rows.Add(it.ServiceName, it.Description, $"{it.Quantity:N0}", $"₹{it.UnitPrice:N2}", $"₹{it.DiscountAmount:N2}", $"{it.TaxPercent:N0}%", $"₹{it.LineTotal:N2}");
            }

            lblSummary.Text = $"Subtotal: ₹{_invoice.Subtotal:N2}\n" +
                              $"Discounts: -₹{(_invoice.ItemDiscountTotal + _invoice.OverallDiscount):N2}\n" +
                              $"Taxable: ₹{_invoice.TaxableAmount:N2}  |  GST: ₹{_invoice.TotalTax:N2}\n" +
                              $"Grand Total: ₹{_invoice.GrandTotal:N2}\n" +
                              $"Amount Paid: ₹{_invoice.AmountPaid:N2}\n" +
                              $"BALANCE DUE: ₹{_invoice.BalanceDue:N2}";

            dgvPayments.Rows.Clear();
            foreach (var p in _invoice.Payments)
            {
                dgvPayments.Rows.Add(p.PaymentDate.ToString("dd MMM yyyy, hh:mm tt"), $"₹{p.Amount:N2}", p.PaymentMethod, p.ReferenceNumber, p.Notes);
            }

            btnRecordPay.Enabled = (_invoice.Status != "Cancelled" && _invoice.BalanceDue > 0);
            btnCancelInv.Enabled = (_invoice.Status != "Cancelled");
        }

        private void ReloadInvoice()
        {
            var fresh = DatabaseService.GetInvoiceById(_invoice.Id);
            if (fresh != null)
            {
                _invoice = fresh;
                LoadInvoiceData();
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = ThemeManager.CardBackground;
            ForeColor = ThemeManager.TextPrimary;

            lblHeader.ForeColor = _invoice.Status == "Paid" ? ThemeManager.StatusPositive : (_invoice.Status == "Overdue" ? ThemeManager.StatusNegative : ThemeManager.PrimaryAccent);
            lblCustInfo.ForeColor = ThemeManager.TextSecondary;
            lblSummary.ForeColor = ThemeManager.TextPrimary;

            Color gridBg = isDark ? Color.FromArgb(24, 26, 38) : Color.White;
            Color gridFg = ThemeManager.TextPrimary;

            foreach (var dgv in new[] { dgvItems, dgvPayments })
            {
                TableDesignStandard.ApplyStandardGrid(dgv, new TableStyleOptions { HeaderHeight = 32, RowHeight = 28 });
            }
        }
    }
}
