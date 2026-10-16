using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Professional Three-Step Create Invoice Wizard (.NET 8 WinForms).
    /// Fully separates the invoice creation workflow from dashboard transaction cards.
    /// Features:
    /// - Step 1: Invoice Settings & Client Information (Auto unique number, database customer selection, client creation)
    /// - Step 2: Billable Services, Subscriptions & Products (Line item editor, real-time math, empty state)
    /// - Step 3: Payment Recording & Financial Summary (CGST/SGST vs IGST, balance due, payment status, A4 print/PDF)
    /// - State preservation across steps, Back/Next navigation, progress stepper indicator.
    /// - 100% theme reactive (Light & Dark modes).
    /// </summary>
    public class CreateInvoiceWizardControl : UserControl
    {
        // ═══════════════════════════════════════════════════════════════════════════
        //  EVENTS & PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════
        public event EventHandler? InvoiceSaved;
        public event EventHandler? CancelRequested;

        public Invoice? LastSavedInvoice { get; private set; }

        // ═══════════════════════════════════════════════════════════════════════════
        //  WIZARD STATE
        // ═══════════════════════════════════════════════════════════════════════════
        private int _currentStep = 1; // 1, 2, or 3
        private readonly List<InvoiceItem> _workingItems = new();
        private List<Customer> _allCustomers = new();
        private bool _isDirty = false;
        private bool _isSaving = false;

        // ═══════════════════════════════════════════════════════════════════════════
        //  LAYOUT CONTAINERS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly Panel pnlStepperHeader = new();
        private readonly Panel pnlStepContentHost = new();
        private readonly Panel pnlNavFooter = new();

        private readonly Panel pnlStep1 = new();
        private readonly Panel pnlStep2 = new();
        private readonly Panel pnlStep3 = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 1 CONTROLS: INVOICE SETTINGS & CLIENT DETAILS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TextBox txtInvNumber = new();
        private readonly DateTimePicker dtpInvDate = new();
        private readonly DateTimePicker dtpDueDate = new();
        private readonly ComboBox cmbPaymentTerms = new();
        private readonly ComboBox cmbTaxScheme = new();

        private readonly ComboBox cmbCustomerSelect = new();
        private readonly TeamPillButton btnAddNewCustomer = new();
        private readonly TextBox txtCustName = new();
        private readonly TextBox txtCustCompany = new();
        private readonly TextBox txtCustPhone = new();
        private readonly TextBox txtCustEmail = new();
        private readonly TextBox txtCustAddress = new();
        private readonly TextBox txtCustGSTIN = new();
        private readonly Label lblStep1Alert = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 2 CONTROLS: BILLABLE SERVICES & PRODUCTS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly ComboBox cmbServiceName = new();
        private readonly TextBox txtItemDesc = new();
        private readonly NumericUpDown numItemQty = new();
        private readonly TextBox txtItemPrice = new();
        private readonly TextBox txtItemDiscPct = new();
        private readonly ComboBox cmbItemTaxPct = new();
        private readonly TeamPillButton btnAddLineItem = new();
        private readonly TeamPillButton btnEditLineItem = new();
        private readonly TeamPillButton btnRemoveLineItem = new();
        private readonly DataGridView dgvInvoiceItems = new();
        private readonly Panel pnlEmptyState = new();
        private readonly Label lblStep2ItemsSummary = new();
        private readonly Label lblStep2Alert = new();
        private int _editingItemIndex = -1;

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 3 CONTROLS: PAYMENT RECORDING & FINANCIAL SUMMARY
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TextBox txtInitialPaymentAmt = new();
        private readonly ComboBox cmbInitialPaymentMode = new();
        private readonly TextBox txtInitialPaymentRef = new();
        private readonly Label lblPaymentStatusBadge = new();
        private readonly Label lblReviewClientDetails = new();
        private readonly Label lblReviewInvDetails = new();

        private readonly Label lblCalcSubtotal = new();
        private readonly Label lblCalcItemDisc = new();
        private readonly TextBox txtOverallDisc = new();
        private readonly Label lblCalcTaxable = new();
        private readonly Label lblCalcCgst = new();
        private readonly Label lblCalcSgst = new();
        private readonly Label lblCalcIgst = new();
        private readonly Label lblCalcGrandTotal = new();
        private readonly Label lblCalcPaid = new();
        private readonly Label lblCalcBalanceDue = new();

        // ═══════════════════════════════════════════════════════════════════════════
        //  NAVIGATION FOOTER BUTTONS
        // ═══════════════════════════════════════════════════════════════════════════
        private readonly TeamPillButton btnNavBack = new();
        private readonly TeamPillButton btnNavCancel = new();
        private readonly TeamPillButton btnNavClear = new();
        private readonly TeamPillButton btnNavNext = new();
        private readonly TeamPillButton btnPreviewInvoice = new();
        private readonly TeamPillButton btnExportPdf = new();
        private readonly TeamPillButton btnSaveAndPrint = new();
        private readonly TeamPillButton btnSaveInvoice = new();

        public CreateInvoiceWizardControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            Dock = DockStyle.Fill;

            BuildWizardLayout();
            ApplyTheme();
            ResetForm();

            ThemeManager.ThemeChanged += ApplyTheme;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BUILD MASTER WIZARD ARCHITECTURE
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildWizardLayout()
        {
            Controls.Clear();

            // 1. Top Stepper Header Panel (Height: 74px)
            pnlStepperHeader.Dock = DockStyle.Top;
            pnlStepperHeader.Height = 74;
            pnlStepperHeader.Padding = new Padding(24, 8, 24, 8);
            pnlStepperHeader.Paint += PaintStepperHeader;
            pnlStepperHeader.MouseClick += OnStepperHeaderClicked;
            pnlStepperHeader.Cursor = Cursors.Hand;

            // 2. Bottom Navigation Footer Panel (Height: 64px)
            pnlNavFooter.Dock = DockStyle.Bottom;
            pnlNavFooter.Height = 64;
            pnlNavFooter.Padding = new Padding(24, 12, 24, 12);
            pnlNavFooter.Paint += (s, e) =>
            {
                var g = e.Graphics;
                bool isDark = ThemeManager.IsDarkMode;
                using var p = new Pen(isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240), 1f);
                g.DrawLine(p, 0, 0, pnlNavFooter.Width, 0);
            };

            // 3. Central Step Content Host
            pnlStepContentHost.Dock = DockStyle.Fill;
            pnlStepContentHost.Padding = new Padding(24, 10, 24, 10);
            pnlStepContentHost.Controls.Add(pnlStep1);
            pnlStepContentHost.Controls.Add(pnlStep2);
            pnlStepContentHost.Controls.Add(pnlStep3);

            BuildStep1Panel();
            BuildStep2Panel();
            BuildStep3Panel();
            BuildFooterButtons();

            Controls.Add(pnlStepContentHost);
            Controls.Add(pnlNavFooter);
            Controls.Add(pnlStepperHeader);

            pnlNavFooter.Resize += (s, e) => LayoutFooterButtons();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 1: INVOICE SETTINGS & CLIENT INFORMATION
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildStep1Panel()
        {
            pnlStep1.Dock = DockStyle.Fill;
            pnlStep1.AutoScroll = true;
            pnlStep1.Controls.Clear();

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));

            // ── Card 1A: Invoice Settings ──
            var cardSettings = CreateSectionCard("Invoice Settings & Terms", ReiconRenderer.IconType.Receipt);
            cardSettings.Dock = DockStyle.Fill;
            cardSettings.Margin = new Padding(0, 0, 10, 0);

            int y = 46;
            int gap = 52;
            // labelW removed
            int fieldW = 280;

            AddFormField(cardSettings, "Invoice Number (Auto Unique)", txtInvNumber, 18, y, fieldW);
            txtInvNumber.ReadOnly = true;
            txtInvNumber.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            y += gap;
            AddFormField(cardSettings, "Invoice Date *", dtpInvDate, 18, y, fieldW);
            dtpInvDate.Format = DateTimePickerFormat.Short;
            dtpInvDate.ValueChanged += (s, e) => OnInvoiceDateOrTermsChanged();

            y += gap;
            cmbPaymentTerms.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentTerms.Items.Clear();
            cmbPaymentTerms.Items.AddRange(new object[] { "Due on Receipt", "Net 15", "Net 30", "Net 60" });
            cmbPaymentTerms.SelectedIndex = 1;
            cmbPaymentTerms.SelectedIndexChanged += (s, e) => OnInvoiceDateOrTermsChanged();
            AddFormField(cardSettings, "Payment Terms", cmbPaymentTerms, 18, y, fieldW);

            y += gap;
            AddFormField(cardSettings, "Payment Due Date", dtpDueDate, 18, y, fieldW);
            dtpDueDate.Format = DateTimePickerFormat.Short;

            y += gap;
            cmbTaxScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTaxScheme.Items.Clear();
            cmbTaxScheme.Items.AddRange(new object[] {
                "Intra-State GST (CGST 9% + SGST 9%)",
                "Inter-State GST (IGST 18%)",
                "Zero Tax / Export / SEZ"
            });
            cmbTaxScheme.SelectedIndex = 0;
            cmbTaxScheme.SelectedIndexChanged += (s, e) => RecalculateAll();
            AddFormField(cardSettings, "GST Tax Policy Scheme *", cmbTaxScheme, 18, y, fieldW + 60);

            // ── Card 1B: Client Information ──
            var cardClient = CreateSectionCard("Client & Billing Information", ReiconRenderer.IconType.Customers);
            cardClient.Dock = DockStyle.Fill;
            cardClient.Margin = new Padding(10, 0, 0, 0);

            int cy = 46;
            cmbCustomerSelect.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCustomerSelect.SelectedIndexChanged += (s, e) => OnCustomerDropdownSelected();
            AddFormField(cardClient, "Select Existing Customer", cmbCustomerSelect, 18, cy, 220);

            btnAddNewCustomer.Text = "+ New Client";
            btnAddNewCustomer.Icon = ReiconRenderer.IconType.Plus;
            btnAddNewCustomer.FillColor = ThemeManager.PrimaryAccent;
            btnAddNewCustomer.HoverColor = ThemeManager.AccentHover;
            btnAddNewCustomer.SetBounds(248, cy + 18, 120, 30);
            btnAddNewCustomer.Click += (s, e) => OpenAddNewClientDialog();
            cardClient.Controls.Add(btnAddNewCustomer);

            cy += gap;
            AddFormField(cardClient, "Customer / Client Name *", txtCustName, 18, cy, 350);
            txtCustName.TextChanged += (s, e) => _isDirty = true;

            cy += gap;
            AddFormField(cardClient, "Company / Org Name", txtCustCompany, 18, cy, 180);
            AddFormField(cardClient, "Phone Number *", txtCustPhone, 210, cy, 158);
            txtCustPhone.TextChanged += (s, e) => _isDirty = true;

            cy += gap;
            AddFormField(cardClient, "Email Address", txtCustEmail, 18, cy, 180);
            AddFormField(cardClient, "GSTIN (Optional)", txtCustGSTIN, 210, cy, 158);

            cy += gap;
            AddFormField(cardClient, "Billing Address / Location", txtCustAddress, 18, cy, 350);

            // Alert label at bottom of Step 1
            lblStep1Alert.Text = "";
            lblStep1Alert.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStep1Alert.ForeColor = Color.FromArgb(225, 29, 72);
            lblStep1Alert.AutoSize = true;
            lblStep1Alert.Location = new Point(18, cy + 54);
            cardClient.Controls.Add(lblStep1Alert);

            tlp.Controls.Add(cardSettings, 0, 0);
            tlp.Controls.Add(cardClient, 1, 0);
            pnlStep1.Controls.Add(tlp);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 2: SERVICES, SUBSCRIPTIONS & PRODUCTS (LINE ITEMS)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildStep2Panel()
        {
            pnlStep2.Dock = DockStyle.Fill;
            pnlStep2.AutoScroll = true;
            pnlStep2.Controls.Clear();

            var card = CreateSectionCard("Billable Services, Subscriptions & Products", ReiconRenderer.IconType.Package);
            card.Dock = DockStyle.Fill;

            int iy = 44;

            // Inputs Row
            cmbServiceName.DropDownStyle = ComboBoxStyle.DropDown;
            cmbServiceName.Items.Clear();
            cmbServiceName.Items.AddRange(new object[] {
                "Enterprise Software License (Annual)",
                "Cloud SaaS Subscription (Monthly)",
                "Custom Software Architecture & Dev",
                "Digital Signature Certificate (Class 3 DSC)",
                "Brand Identity & UI/UX Design System",
                "Cloud Hosting & Managed DevOps",
                "Database Migration & Security Audit",
                "Annual Maintenance Contract (AMC)"
            });
            cmbServiceName.SelectedIndexChanged += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtItemPrice.Text) || txtItemPrice.Text == "0.00")
                {
                    txtItemPrice.Text = cmbServiceName.SelectedIndex switch
                    {
                        0 => "85000.00",
                        1 => "15000.00",
                        2 => "60000.00",
                        3 => "4500.00",
                        4 => "35000.00",
                        5 => "25000.00",
                        6 => "40000.00",
                        7 => "20000.00",
                        _ => "0.00"
                    };
                }
            };
            AddFormField(card, "Service / Item Name *", cmbServiceName, 18, iy, 260);

            txtItemDesc.PlaceholderText = "Scope of work / deliverables";
            AddFormField(card, "Description / Scope", txtItemDesc, 288, iy, 210);

            numItemQty.Minimum = 1;
            numItemQty.Maximum = 100000;
            numItemQty.Value = 1;
            AddFormField(card, "Qty *", numItemQty, 508, iy, 65);

            txtItemPrice.PlaceholderText = "0.00";
            AddFormField(card, "Unit Rate (₹) *", txtItemPrice, 583, iy, 110);

            txtItemDiscPct.PlaceholderText = "0%";
            AddFormField(card, "Disc %", txtItemDiscPct, 703, iy, 65);

            cmbItemTaxPct.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbItemTaxPct.Items.Clear();
            cmbItemTaxPct.Items.AddRange(new object[] { "0%", "5%", "12%", "18%", "28%" });
            cmbItemTaxPct.SelectedItem = "18%";
            AddFormField(card, "GST %", cmbItemTaxPct, 778, iy, 75);

            btnAddLineItem.Text = "+ Add Line";
            btnAddLineItem.Icon = ReiconRenderer.IconType.Plus;
            btnAddLineItem.FillColor = Color.FromArgb(16, 185, 129);
            btnAddLineItem.HoverColor = Color.FromArgb(5, 150, 105);
            btnAddLineItem.SetBounds(863, iy + 18, 110, 30);
            btnAddLineItem.Click += (s, e) => AddOrUpdateLineItem();
            card.Controls.Add(btnAddLineItem);

            // Action row under inputs
            btnEditLineItem.Text = "Edit Selected";
            btnEditLineItem.Icon = ReiconRenderer.IconType.Document;
            btnEditLineItem.BorderColor = Color.FromArgb(203, 213, 225);
            btnEditLineItem.BorderWidth = 1.2f;
            btnEditLineItem.FillColor = Color.White;
            btnEditLineItem.TextColor = Color.FromArgb(51, 65, 85);
            btnEditLineItem.HoverColor = Color.FromArgb(241, 245, 249);
            btnEditLineItem.SetBounds(18, iy + 58, 120, 28);
            btnEditLineItem.Click += (s, e) => EditSelectedLineItem();

            btnRemoveLineItem.Text = "Remove Line";
            btnRemoveLineItem.Icon = ReiconRenderer.IconType.Close;
            btnRemoveLineItem.BorderColor = Color.FromArgb(244, 63, 94);
            btnRemoveLineItem.BorderWidth = 1.2f;
            btnRemoveLineItem.FillColor = Color.White;
            btnRemoveLineItem.TextColor = Color.FromArgb(225, 29, 72);
            btnRemoveLineItem.HoverColor = Color.FromArgb(255, 241, 242);
            btnRemoveLineItem.SetBounds(148, iy + 58, 120, 28);
            btnRemoveLineItem.Click += (s, e) => RemoveSelectedLineItem();

            lblStep2Alert.Text = "";
            lblStep2Alert.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStep2Alert.ForeColor = Color.FromArgb(225, 29, 72);
            lblStep2Alert.AutoSize = true;
            lblStep2Alert.Location = new Point(280, iy + 62);

            card.Controls.Add(btnEditLineItem);
            card.Controls.Add(btnRemoveLineItem);
            card.Controls.Add(lblStep2Alert);

            // Items DataGridView Table
            FormatTable(dgvInvoiceItems);
            dgvInvoiceItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvInvoiceItems.SetBounds(18, iy + 94, card.Width - 36, 260);

            dgvInvoiceItems.Columns.Add("Service", "SERVICE / PRODUCT");
            dgvInvoiceItems.Columns.Add("Desc", "SCOPE / DESCRIPTION");
            dgvInvoiceItems.Columns.Add("Qty", "QTY");
            dgvInvoiceItems.Columns.Add("Price", "RATE (₹)");
            dgvInvoiceItems.Columns.Add("Disc", "DISC (₹)");
            dgvInvoiceItems.Columns.Add("Tax", "GST %");
            dgvInvoiceItems.Columns.Add("Total", "LINE TOTAL (₹)");

            dgvInvoiceItems.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoiceItems.Columns["Disc"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvInvoiceItems.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Empty state overlay
            pnlEmptyState.Dock = DockStyle.Fill;
            pnlEmptyState.BackColor = Color.Transparent;
            var lblEmptyTitle = new Label
            {
                Text = "No billable services or products added yet.",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 36
            };
            var lblEmptySub = new Label
            {
                Text = "Select a service or type item details above, then click '+ Add Line'.",
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill
            };
            pnlEmptyState.Controls.Add(lblEmptySub);
            pnlEmptyState.Controls.Add(lblEmptyTitle);
            pnlEmptyState.Visible = true;
            dgvInvoiceItems.Controls.Add(pnlEmptyState);

            // Step 2 Summary Bar (Bottom of card)
            lblStep2ItemsSummary.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblStep2ItemsSummary.ForeColor = ThemeManager.PrimaryAccent;
            lblStep2ItemsSummary.Dock = DockStyle.Bottom;
            lblStep2ItemsSummary.Height = 30;
            lblStep2ItemsSummary.Padding = new Padding(18, 4, 18, 4);
            lblStep2ItemsSummary.Text = "Items Added: 0  |  Estimated Subtotal: ₹0.00";

            card.Controls.Add(dgvInvoiceItems);
            card.Controls.Add(lblStep2ItemsSummary);
            pnlStep2.Controls.Add(card);

            card.Resize += (s, e) =>
            {
                dgvInvoiceItems.SetBounds(18, iy + 94, card.Width - 36, Math.Max(120, card.Height - (iy + 134)));
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 3: PAYMENT RECORDING & FINANCIAL SUMMARY
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildStep3Panel()
        {
            pnlStep3.Dock = DockStyle.Fill;
            pnlStep3.AutoScroll = true;
            pnlStep3.Controls.Clear();

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            // ── Left Card: Payment Recording & Confirmation ──
            var cardPayment = CreateSectionCard("Payment Recording & Review", ReiconRenderer.IconType.Wallet);
            cardPayment.Dock = DockStyle.Fill;
            cardPayment.Margin = new Padding(0, 0, 10, 0);

            int py = 46;
            int gap = 50;

            AddFormField(cardPayment, "Amount Paid Now (₹)", txtInitialPaymentAmt, 18, py, 180);
            txtInitialPaymentAmt.Text = "0.00";
            txtInitialPaymentAmt.TextChanged += (s, e) => OnPaymentAmountChanged();

            // Quick payment pill buttons
            var btnPayFull = CreateMiniButton("100% Full", 210, py + 18, 80);
            btnPayFull.Click += (s, e) => SetPaymentPercentage(1.0m);
            var btnPayHalf = CreateMiniButton("50% Adv", 298, py + 18, 75);
            btnPayHalf.Click += (s, e) => SetPaymentPercentage(0.5m);
            var btnPayZero = CreateMiniButton("₹0 Clear", 380, py + 18, 70);
            btnPayZero.Click += (s, e) => SetPaymentPercentage(0.0m);
            cardPayment.Controls.AddRange(new Control[] { btnPayFull, btnPayHalf, btnPayZero });

            py += gap;
            cmbInitialPaymentMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbInitialPaymentMode.Items.Clear();
            cmbInitialPaymentMode.Items.AddRange(new object[] { "Bank Transfer", "UPI", "Cash", "Card", "Cheque", "Other" });
            cmbInitialPaymentMode.SelectedIndex = 0;
            AddFormField(cardPayment, "Payment Method", cmbInitialPaymentMode, 18, py, 180);

            txtInitialPaymentRef.PlaceholderText = "Cheque / UTR / Txn Ref";
            AddFormField(cardPayment, "Reference / Txn #", txtInitialPaymentRef, 210, py, 240);

            py += gap;
            // Payment status pill
            var lblStatusTitle = new Label
            {
                Text = "Payment Status:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Bounds = new Rectangle(18, py, 120, 20)
            };
            lblPaymentStatusBadge.SetBounds(140, py - 3, 130, 26);
            lblPaymentStatusBadge.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblPaymentStatusBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblPaymentStatusBadge.Paint += PaintPaymentStatusBadge;
            cardPayment.Controls.Add(lblStatusTitle);
            cardPayment.Controls.Add(lblPaymentStatusBadge);

            py += 36;
            // Review Snapshot Box
            var pnlReviewBox = new Panel
            {
                Bounds = new Rectangle(18, py, cardPayment.Width - 36, 170),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(12),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252)
            };
            pnlReviewBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using var p = new Pen(ThemeManager.IsDarkMode ? Color.FromArgb(48, 56, 80) : Color.FromArgb(226, 232, 240), 1f);
                g.DrawRectangle(p, 0, 0, pnlReviewBox.Width - 1, pnlReviewBox.Height - 1);
            };

            lblReviewClientDetails.Font = new Font("Segoe UI", 8.8f);
            lblReviewClientDetails.Dock = DockStyle.Left;
            lblReviewClientDetails.Width = 220;
            lblReviewClientDetails.Text = "Client: —\nCompany: —\nPhone: —";

            lblReviewInvDetails.Font = new Font("Segoe UI", 8.8f);
            lblReviewInvDetails.Dock = DockStyle.Fill;
            lblReviewInvDetails.Text = "Invoice #: —\nDate: —\nTax Scheme: —";

            pnlReviewBox.Controls.Add(lblReviewInvDetails);
            pnlReviewBox.Controls.Add(lblReviewClientDetails);
            cardPayment.Controls.Add(pnlReviewBox);

            // ── Right Card: Financial Calculation & Tax Ledger ──
            var cardSummary = CreateSectionCard("Financial Summary & Tax Ledger", ReiconRenderer.IconType.Receipt);
            cardSummary.Dock = DockStyle.Fill;
            cardSummary.Margin = new Padding(10, 0, 0, 0);

            int sy = 46;
            int rowH = 24;
            int rightValW = 160;

            void AddSummaryRow(string title, Label valLbl, bool bold = false)
            {
                var lblT = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 9.2f, bold ? FontStyle.Bold : FontStyle.Regular),
                    Bounds = new Rectangle(20, sy, 220, 20),
                    AutoSize = false,
                    UseMnemonic = false
                };
                valLbl.SetBounds(cardSummary.Width - rightValW - 20, sy, rightValW, 20);
                valLbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                valLbl.Font = new Font("Segoe UI", 9.5f, bold ? FontStyle.Bold : FontStyle.Regular);
                valLbl.TextAlign = ContentAlignment.TopRight;
                valLbl.Text = "₹0.00";
                cardSummary.Controls.Add(lblT);
                cardSummary.Controls.Add(valLbl);
                sy += rowH;
            }

            AddSummaryRow("Subtotal:", lblCalcSubtotal);
            AddSummaryRow("Item Discounts:", lblCalcItemDisc);

            // Overall Discount row with live input
            var lblOvr = new Label { Text = "Overall Discount (₹):", Font = new Font("Segoe UI", 9.2f), Bounds = new Rectangle(20, sy, 200, 20) };
            txtOverallDisc.SetBounds(cardSummary.Width - 140 - 20, sy - 2, 140, 24);
            txtOverallDisc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtOverallDisc.Text = "0.00";
            txtOverallDisc.TextAlign = HorizontalAlignment.Right;
            txtOverallDisc.TextChanged += (s, e) => RecalculateAll();
            cardSummary.Controls.Add(lblOvr);
            cardSummary.Controls.Add(txtOverallDisc);
            sy += rowH + 4;

            AddSummaryRow("Taxable Amount:", lblCalcTaxable, true);
            AddSummaryRow("CGST Amount (9%):", lblCalcCgst);
            AddSummaryRow("SGST Amount (9%):", lblCalcSgst);
            AddSummaryRow("IGST Amount (18%):", lblCalcIgst);

            // Hairline divider
            var sep = new Panel
            {
                Bounds = new Rectangle(20, sy + 4, cardSummary.Width - 40, 1),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(203, 213, 225)
            };
            cardSummary.Controls.Add(sep);
            sy += 12;

            // Grand Total
            var lblGtTitle = new Label { Text = "GRAND TOTAL:", Font = new Font("Segoe UI", 11.5f, FontStyle.Bold), Bounds = new Rectangle(20, sy, 160, 26) };
            lblCalcGrandTotal.Bounds = new Rectangle(cardSummary.Width - 220 - 20, sy - 2, 220, 28);
            lblCalcGrandTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCalcGrandTotal.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblCalcGrandTotal.ForeColor = Color.FromArgb(16, 185, 129);
            lblCalcGrandTotal.TextAlign = ContentAlignment.TopRight;
            lblCalcGrandTotal.Text = "₹0.00";
            cardSummary.Controls.Add(lblGtTitle);
            cardSummary.Controls.Add(lblCalcGrandTotal);
            sy += 34;

            AddSummaryRow("Amount Paid Now:", lblCalcPaid);

            // Balance Due
            var lblBalTitle = new Label { Text = "Balance Due:", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Bounds = new Rectangle(20, sy, 160, 24) };
            lblCalcBalanceDue.Bounds = new Rectangle(cardSummary.Width - 200 - 20, sy, 200, 24);
            lblCalcBalanceDue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCalcBalanceDue.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblCalcBalanceDue.ForeColor = Color.FromArgb(225, 29, 72);
            lblCalcBalanceDue.TextAlign = ContentAlignment.TopRight;
            lblCalcBalanceDue.Text = "₹0.00";
            cardSummary.Controls.Add(lblBalTitle);
            cardSummary.Controls.Add(lblCalcBalanceDue);

            tlp.Controls.Add(cardPayment, 0, 0);
            tlp.Controls.Add(cardSummary, 1, 0);
            pnlStep3.Controls.Add(tlp);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BOTTOM NAVIGATION FOOTER BUTTONS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildFooterButtons()
        {
            pnlNavFooter.Controls.Clear();

            // Left side buttons
            btnNavBack.Text = "← Back";
            btnNavBack.BorderColor = Color.FromArgb(203, 213, 225);
            btnNavBack.BorderWidth = 1.2f;
            btnNavBack.FillColor = Color.White;
            btnNavBack.TextColor = Color.FromArgb(51, 65, 85);
            btnNavBack.HoverColor = Color.FromArgb(241, 245, 249);
            btnNavBack.Click += (s, e) => NavigateBack();

            btnNavCancel.Text = "Cancel";
            btnNavCancel.BorderColor = Color.FromArgb(226, 232, 240);
            btnNavCancel.BorderWidth = 1f;
            btnNavCancel.FillColor = Color.Transparent;
            btnNavCancel.TextColor = Color.FromArgb(100, 116, 139);
            btnNavCancel.HoverColor = Color.FromArgb(254, 226, 226);
            btnNavCancel.Click += (s, e) => ConfirmAndCancel();

            btnNavClear.Text = "Clear Form";
            btnNavClear.BorderColor = Color.FromArgb(226, 232, 240);
            btnNavClear.BorderWidth = 1f;
            btnNavClear.FillColor = Color.Transparent;
            btnNavClear.TextColor = Color.FromArgb(100, 116, 139);
            btnNavClear.HoverColor = Color.FromArgb(241, 245, 249);
            btnNavClear.Click += (s, e) => ConfirmAndClear();

            // Right side buttons
            btnNavNext.Text = "Next: Add Billable Items →";
            btnNavNext.FillColor = ThemeManager.PrimaryAccent;
            btnNavNext.HoverColor = ThemeManager.AccentHover;
            btnNavNext.TextColor = Color.White;
            btnNavNext.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnNavNext.Click += (s, e) => NavigateNext();

            btnPreviewInvoice.Text = "Print Preview";
            btnPreviewInvoice.Icon = ReiconRenderer.IconType.Eye;
            btnPreviewInvoice.BorderColor = Color.FromArgb(203, 213, 225);
            btnPreviewInvoice.BorderWidth = 1.2f;
            btnPreviewInvoice.FillColor = Color.White;
            btnPreviewInvoice.TextColor = Color.FromArgb(30, 41, 59);
            btnPreviewInvoice.HoverColor = Color.FromArgb(241, 245, 249);
            btnPreviewInvoice.Click += (s, e) => PreviewInvoice();

            btnExportPdf.Text = "Export PDF";
            btnExportPdf.Icon = ReiconRenderer.IconType.Document;
            btnExportPdf.BorderColor = Color.FromArgb(203, 213, 225);
            btnExportPdf.BorderWidth = 1.2f;
            btnExportPdf.FillColor = Color.White;
            btnExportPdf.TextColor = Color.FromArgb(30, 41, 59);
            btnExportPdf.HoverColor = Color.FromArgb(241, 245, 249);
            btnExportPdf.Click += (s, e) => ExportInvoicePdf();

            btnSaveAndPrint.Text = "Save & Print (A4)";
            btnSaveAndPrint.Icon = ReiconRenderer.IconType.Printer;
            btnSaveAndPrint.FillColor = Color.FromArgb(16, 185, 129);
            btnSaveAndPrint.HoverColor = Color.FromArgb(5, 150, 105);
            btnSaveAndPrint.TextColor = Color.White;
            btnSaveAndPrint.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnSaveAndPrint.Click += (s, e) => SaveInvoice(andPrint: true);

            btnSaveInvoice.Text = "Save Invoice";
            btnSaveInvoice.Icon = ReiconRenderer.IconType.Check;
            btnSaveInvoice.FillColor = ThemeManager.PrimaryAccent;
            btnSaveInvoice.HoverColor = ThemeManager.AccentHover;
            btnSaveInvoice.TextColor = Color.White;
            btnSaveInvoice.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnSaveInvoice.Click += (s, e) => SaveInvoice(andPrint: false);

            pnlNavFooter.Controls.AddRange(new Control[] {
                btnNavBack, btnNavCancel, btnNavClear,
                btnNavNext, btnPreviewInvoice, btnExportPdf, btnSaveAndPrint, btnSaveInvoice
            });

            LayoutFooterButtons();
        }

        private void LayoutFooterButtons()
        {
            int h = pnlNavFooter.Height;
            int btnH = 38;
            int btnY = (h - btnH) / 2;

            // Left
            btnNavBack.SetBounds(24, btnY, 100, btnH);
            btnNavCancel.SetBounds(btnNavBack.Right + 10, btnY, 90, btnH);
            btnNavClear.SetBounds(btnNavCancel.Right + 10, btnY, 100, btnH);

            // Right
            int right = pnlNavFooter.ClientSize.Width - 24;
            int gap = 10;

            if (_currentStep == 1 || _currentStep == 2)
            {
                btnNavBack.Visible = (_currentStep == 2);
                btnNavClear.Visible = true;
                btnNavCancel.Visible = true;

                btnNavNext.Visible = true;
                btnPreviewInvoice.Visible = false;
                btnExportPdf.Visible = false;
                btnSaveAndPrint.Visible = false;
                btnSaveInvoice.Visible = false;

                btnNavNext.Text = _currentStep == 1 ? "Next: Add Billable Items →" : "Next: Payment & Review →";
                int nextW = 210;
                btnNavNext.SetBounds(right - nextW, btnY, nextW, btnH);
            }
            else // Step 3
            {
                btnNavBack.Visible = true;
                btnNavClear.Visible = false;
                btnNavCancel.Visible = true;
                btnNavNext.Visible = false;

                btnPreviewInvoice.Visible = true;
                btnExportPdf.Visible = true;
                btnSaveAndPrint.Visible = true;
                btnSaveInvoice.Visible = true;

                int saveW = 140;
                int printW = 165;
                int pdfW = 125;
                int prevW = 130;

                btnSaveInvoice.SetBounds(right - saveW, btnY, saveW, btnH);
                btnSaveAndPrint.SetBounds(btnSaveInvoice.Left - gap - printW, btnY, printW, btnH);
                btnExportPdf.SetBounds(btnSaveAndPrint.Left - gap - pdfW, btnY, pdfW, btnH);
                btnPreviewInvoice.SetBounds(btnExportPdf.Left - gap - prevW, btnY, prevW, btnH);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEPPER HEADER RENDERING & INTERACTION
        // ═══════════════════════════════════════════════════════════════════════════
        private void PaintStepperHeader(object? s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = pnlStepperHeader.Width;
            int h = pnlStepperHeader.Height;
            if (w <= 0 || h <= 0) return;

            bool isDark = ThemeManager.IsDarkMode;

            // Background & bottom hairline
            Color bg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
            using (var b = new SolidBrush(bg)) g.FillRectangle(b, 0, 0, w, h);
            using (var p = new Pen(isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240), 1f))
                g.DrawLine(p, 0, h - 1, w, h - 1);

            string[] stepTitles = { "1. Client & Invoice", "2. Services & Products", "3. Payment & Summary" };
            int stepCount = 3;

            int padX = 60;
            int availW = w - padX * 2;
            int spacing = availW / (stepCount - 1);
            int circleD = 28;
            int cy = 24;

            // Connecting lines
            for (int i = 0; i < stepCount - 1; i++)
            {
                int x1 = padX + i * spacing + circleD / 2;
                int x2 = padX + (i + 1) * spacing - circleD / 2;

                bool isPassed = (i + 1) < _currentStep;
                Color lineCol = isPassed
                    ? Color.FromArgb(16, 185, 129)
                    : (isDark ? Color.FromArgb(48, 56, 80) : Color.FromArgb(226, 232, 240));

                using var pen = new Pen(lineCol, 2.5f);
                g.DrawLine(pen, x1, cy, x2, cy);
            }

            // Step circles and labels
            for (int i = 0; i < stepCount; i++)
            {
                int stepNum = i + 1;
                int cx = padX + i * spacing;
                var circleRect = new Rectangle(cx - circleD / 2, cy - circleD / 2, circleD, circleD);

                bool isCurrent = (stepNum == _currentStep);
                bool isCompleted = (stepNum < _currentStep);

                Color circleBg;
                Color circleBdr;
                Color numColor;

                if (isCompleted)
                {
                    circleBg = Color.FromArgb(16, 185, 129);
                    circleBdr = Color.FromArgb(16, 185, 129);
                    numColor = Color.White;
                }
                else if (isCurrent)
                {
                    circleBg = ThemeManager.PrimaryAccent;
                    circleBdr = ThemeManager.PrimaryAccent;
                    numColor = Color.White;
                }
                else
                {
                    circleBg = isDark ? Color.FromArgb(32, 37, 54) : Color.FromArgb(241, 245, 249);
                    circleBdr = isDark ? Color.FromArgb(50, 58, 86) : Color.FromArgb(203, 213, 225);
                    numColor = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
                }

                using (var b = new SolidBrush(circleBg)) g.FillEllipse(b, circleRect);
                using (var p = new Pen(circleBdr, 1.5f)) g.DrawEllipse(p, circleRect);

                // Draw number or checkmark
                if (isCompleted)
                {
                    ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Check, new Rectangle(circleRect.X + 6, circleRect.Y + 6, 16, 16), Color.White, 2f);
                }
                else
                {
                    using var fNum = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                    using var bNum = new SolidBrush(numColor);
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(stepNum.ToString(), fNum, bNum, circleRect, sf);
                }

                // Step label below
                Color textCol = isCurrent
                    ? (isDark ? Color.White : Color.FromArgb(15, 23, 42))
                    : (isCompleted ? Color.FromArgb(16, 185, 129) : (isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139)));

                using var fTitle = new Font("Segoe UI", 8.8f, isCurrent ? FontStyle.Bold : FontStyle.Regular);
                using var bTitle = new SolidBrush(textCol);
                using var sfTitle = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(stepTitles[i], fTitle, bTitle, cx, cy + circleD / 2 + 6, sfTitle);
            }
        }

        private void OnStepperHeaderClicked(object? s, MouseEventArgs e)
        {
            int w = pnlStepperHeader.Width;
            int padX = 60;
            int spacing = (w - padX * 2) / 2;

            for (int i = 0; i < 3; i++)
            {
                int cx = padX + i * spacing;
                if (Math.Abs(e.X - cx) <= 50)
                {
                    int targetStep = i + 1;
                    if (targetStep < _currentStep)
                    {
                        SwitchToStep(targetStep);
                    }
                    else if (targetStep > _currentStep)
                    {
                        NavigateNext();
                    }
                    break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  NAVIGATION FLOW & STEP VALIDATION
        // ═══════════════════════════════════════════════════════════════════════════
        public void SwitchToStep(int step)
        {
            _currentStep = Math.Clamp(step, 1, 3);

            pnlStep1.Visible = (_currentStep == 1);
            pnlStep2.Visible = (_currentStep == 2);
            pnlStep3.Visible = (_currentStep == 3);

            pnlStepperHeader.Invalidate();
            LayoutFooterButtons();

            if (_currentStep == 3)
            {
                UpdateReviewSnapshot();
                RecalculateAll();
            }
        }

        private void NavigateNext()
        {
            lblStep1Alert.Text = "";
            lblStep2Alert.Text = "";

            if (_currentStep == 1)
            {
                // Validate Step 1
                string custName = txtCustName.Text.Trim();
                string custPhone = txtCustPhone.Text.Trim();

                if (string.IsNullOrWhiteSpace(custName))
                {
                    lblStep1Alert.Text = "● Customer / Client Name is required.";
                    txtCustName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(custPhone))
                {
                    lblStep1Alert.Text = "● Phone Number is required for GST billing.";
                    txtCustPhone.Focus();
                    return;
                }

                SwitchToStep(2);
            }
            else if (_currentStep == 2)
            {
                // Validate Step 2
                if (_workingItems.Count == 0)
                {
                    lblStep2Alert.Text = "● Please add at least one line item to the invoice.";
                    cmbServiceName.Focus();
                    return;
                }

                SwitchToStep(3);
            }
        }

        private void NavigateBack()
        {
            if (_currentStep > 1)
            {
                SwitchToStep(_currentStep - 1);
            }
        }

        private void ConfirmAndCancel()
        {
            if (_isDirty && _workingItems.Count > 0)
            {
                var res = MessageBox.Show(
                    "You have unsaved changes in this invoice. Are you sure you want to discard them and exit?",
                    "Discard Invoice?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes) return;
            }

            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ConfirmAndClear()
        {
            if (_isDirty || _workingItems.Count > 0)
            {
                var res = MessageBox.Show(
                    "Are you sure you want to clear all entered details and reset the invoice form?",
                    "Clear Invoice Form?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res != DialogResult.Yes) return;
            }

            ResetForm();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 1 LOGIC: CUSTOMERS & DATES
        // ═══════════════════════════════════════════════════════════════════════════
        public void ResetForm()
        {
            _currentStep = 1;
            _workingItems.Clear();
            _isDirty = false;
            _isSaving = false;
            _editingItemIndex = -1;

            txtInvNumber.Text = DatabaseService.GetNextInvoiceNumber();
            dtpInvDate.Value = DateTime.Today;
            cmbPaymentTerms.SelectedIndex = 1; // Net 15
            dtpDueDate.Value = DateTime.Today.AddDays(15);
            cmbTaxScheme.SelectedIndex = 0; // Intra-State

            LoadCustomersDropdown();

            txtCustName.Clear();
            txtCustCompany.Clear();
            txtCustPhone.Clear();
            txtCustEmail.Clear();
            txtCustAddress.Clear();
            txtCustGSTIN.Clear();

            // Clear Step 2
            cmbServiceName.SelectedIndex = -1;
            cmbServiceName.Text = "";
            txtItemDesc.Clear();
            numItemQty.Value = 1;
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
            cmbItemTaxPct.SelectedItem = "18%";
            dgvInvoiceItems.Rows.Clear();
            pnlEmptyState.Visible = true;
            lblStep2ItemsSummary.Text = "Items Added: 0  |  Estimated Subtotal: ₹0.00";

            // Clear Step 3
            txtInitialPaymentAmt.Text = "0.00";
            cmbInitialPaymentMode.SelectedIndex = 0;
            txtInitialPaymentRef.Clear();
            txtOverallDisc.Text = "0.00";

            lblStep1Alert.Text = "";
            lblStep2Alert.Text = "";

            SwitchToStep(1);
        }

        public void LoadCustomersDropdown(long selectCustomerId = 0)
        {
            _allCustomers = DatabaseService.GetCustomers();
            cmbCustomerSelect.Items.Clear();
            cmbCustomerSelect.Items.Add("-- Select Existing Customer --");

            int selectIdx = 0;
            for (int i = 0; i < _allCustomers.Count; i++)
            {
                var c = _allCustomers[i];
                cmbCustomerSelect.Items.Add(c.DisplayName);
                if (selectCustomerId > 0 && c.Id == selectCustomerId)
                {
                    selectIdx = i + 1;
                }
            }

            cmbCustomerSelect.SelectedIndex = selectIdx;
        }

        private void OnCustomerDropdownSelected()
        {
            if (cmbCustomerSelect.SelectedIndex > 0 && cmbCustomerSelect.SelectedIndex - 1 < _allCustomers.Count)
            {
                var c = _allCustomers[cmbCustomerSelect.SelectedIndex - 1];
                txtCustName.Text = c.Name;
                txtCustCompany.Text = c.CompanyName;
                txtCustPhone.Text = c.Phone;
                txtCustEmail.Text = c.Email;
                txtCustAddress.Text = c.BillingAddress;
                txtCustGSTIN.Text = c.GSTIN;
                _isDirty = true;
            }
        }

        private void OpenAddNewClientDialog()
        {
            using var dlg = new CustomerDialog();
            if (dlg.ShowDialog((IWin32Window?)this.FindForm() ?? this) == DialogResult.OK)
            {
                LoadCustomersDropdown(dlg.SavedCustomer.Id);
            }
        }

        private void OnInvoiceDateOrTermsChanged()
        {
            int days = cmbPaymentTerms.SelectedItem?.ToString() switch
            {
                "Due on Receipt" => 0,
                "Net 15" => 15,
                "Net 30" => 30,
                "Net 60" => 60,
                _ => 15
            };
            dtpDueDate.Value = dtpInvDate.Value.Date.AddDays(days);
            _isDirty = true;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 2 LOGIC: LINE ITEMS MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════════
        private void AddOrUpdateLineItem()
        {
            lblStep2Alert.Text = "";

            string name = cmbServiceName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                lblStep2Alert.Text = "● Service / Item name cannot be blank.";
                cmbServiceName.Focus();
                return;
            }

            if (!decimal.TryParse(txtItemPrice.Text.Trim(), out decimal rate) || rate <= 0)
            {
                lblStep2Alert.Text = "● Please enter a valid Unit Rate greater than ₹0.00.";
                txtItemPrice.Focus();
                return;
            }

            decimal qty = numItemQty.Value;
            string discStr = txtItemDiscPct.Text.Trim().Replace("%", "");
            decimal discPct = 0;
            if (!string.IsNullOrWhiteSpace(discStr) && decimal.TryParse(discStr, out decimal parsedDisc))
            {
                discPct = Math.Clamp(parsedDisc, 0m, 100m);
            }

            string taxStr = cmbItemTaxPct.SelectedItem?.ToString()?.Replace("%", "") ?? "18";
            decimal.TryParse(taxStr, out decimal taxPct);

            var item = new InvoiceItem
            {
                ServiceName = name,
                Description = txtItemDesc.Text.Trim(),
                Quantity = qty,
                UnitPrice = rate,
                DiscountPercent = discPct,
                TaxPercent = taxPct
            };
            item.CalculateLineTotal();

            if (_editingItemIndex >= 0 && _editingItemIndex < _workingItems.Count)
            {
                _workingItems[_editingItemIndex] = item;
                _editingItemIndex = -1;
                btnAddLineItem.Text = "+ Add Line";
                btnAddLineItem.FillColor = Color.FromArgb(16, 185, 129);
            }
            else
            {
                _workingItems.Add(item);
            }

            _isDirty = true;
            RefreshItemsGrid();

            // Clear inputs for next line
            cmbServiceName.SelectedIndex = -1;
            cmbServiceName.Text = "";
            txtItemDesc.Clear();
            numItemQty.Value = 1;
            txtItemPrice.Clear();
            txtItemDiscPct.Clear();
            cmbItemTaxPct.SelectedItem = "18%";
            cmbServiceName.Focus();
        }

        private void EditSelectedLineItem()
        {
            if (dgvInvoiceItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a line item from the table to edit.", "Select Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = dgvInvoiceItems.SelectedRows[0].Index;
            if (idx >= 0 && idx < _workingItems.Count)
            {
                var item = _workingItems[idx];
                cmbServiceName.Text = item.ServiceName;
                txtItemDesc.Text = item.Description;
                numItemQty.Value = Math.Clamp(item.Quantity, numItemQty.Minimum, numItemQty.Maximum);
                txtItemPrice.Text = item.UnitPrice.ToString("F2");
                txtItemDiscPct.Text = item.DiscountPercent > 0 ? $"{item.DiscountPercent}%" : "";
                cmbItemTaxPct.SelectedItem = $"{item.TaxPercent:0}%";

                _editingItemIndex = idx;
                btnAddLineItem.Text = "Update Line";
                btnAddLineItem.FillColor = Color.FromArgb(79, 70, 229);
            }
        }

        private void RemoveSelectedLineItem()
        {
            if (dgvInvoiceItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a line item from the table to remove.", "Select Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int idx = dgvInvoiceItems.SelectedRows[0].Index;
            if (idx >= 0 && idx < _workingItems.Count)
            {
                _workingItems.RemoveAt(idx);
                _isDirty = true;
                RefreshItemsGrid();
            }
        }

        private void RefreshItemsGrid()
        {
            dgvInvoiceItems.Rows.Clear();
            decimal subtotal = 0;

            foreach (var itm in _workingItems)
            {
                dgvInvoiceItems.Rows.Add(
                    itm.ServiceName,
                    itm.Description,
                    $"{itm.Quantity:N0}",
                    $"₹{itm.UnitPrice:N2}",
                    itm.DiscountAmount > 0 ? $"₹{itm.DiscountAmount:N2}" : "₹0.00",
                    $"{itm.TaxPercent:0}%",
                    $"₹{itm.LineTotal:N2}"
                );
                subtotal += (itm.Quantity * itm.UnitPrice);
            }

            pnlEmptyState.Visible = (_workingItems.Count == 0);
            lblStep2ItemsSummary.Text = $"Items Added: {_workingItems.Count}  |  Estimated Subtotal: ₹{subtotal:N2}";
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  STEP 3 LOGIC: FINANCIAL CALCULATIONS & GST
        // ═══════════════════════════════════════════════════════════════════════════
        private void UpdateReviewSnapshot()
        {
            lblReviewClientDetails.Text = $"Client: {txtCustName.Text.Trim()}\nCompany: {txtCustCompany.Text.Trim()}\nPhone: {txtCustPhone.Text.Trim()}\nAddress: {txtCustAddress.Text.Trim()}";
            lblReviewInvDetails.Text = $"Invoice #: {txtInvNumber.Text.Trim()}\nDate: {dtpInvDate.Value:dd MMM yyyy}\nDue: {dtpDueDate.Value:dd MMM yyyy}\nTerms: {cmbPaymentTerms.SelectedItem}\nScheme: {cmbTaxScheme.SelectedItem}";
        }

        public Invoice BuildInvoiceObject()
        {
            var inv = new Invoice
            {
                InvoiceNumber = txtInvNumber.Text.Trim(),
                CustomerName = txtCustName.Text.Trim(),
                CustomerCompany = txtCustCompany.Text.Trim(),
                CustomerPhone = txtCustPhone.Text.Trim(),
                CustomerEmail = txtCustEmail.Text.Trim(),
                CustomerAddress = txtCustAddress.Text.Trim(),
                CustomerGSTIN = txtCustGSTIN.Text.Trim(),
                InvoiceDate = dtpInvDate.Value.Date,
                DueDate = dtpDueDate.Value.Date,
                PaymentTerms = cmbPaymentTerms.SelectedItem?.ToString() ?? "Net 15",
                TaxType = cmbTaxScheme.SelectedIndex switch
                {
                    1 => "GST_INTER",
                    2 => "ZERO_TAX",
                    _ => "GST_INTRA"
                }
            };

            // Associate Customer ID if existing
            if (cmbCustomerSelect.SelectedIndex > 0 && cmbCustomerSelect.SelectedIndex - 1 < _allCustomers.Count)
            {
                inv.CustomerId = _allCustomers[cmbCustomerSelect.SelectedIndex - 1].Id;
            }

            // Copy items
            foreach (var itm in _workingItems)
            {
                inv.Items.Add(new InvoiceItem
                {
                    ServiceName = itm.ServiceName,
                    Description = itm.Description,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.UnitPrice,
                    DiscountPercent = itm.DiscountPercent,
                    DiscountAmount = itm.DiscountAmount,
                    TaxPercent = itm.TaxPercent,
                    LineTotal = itm.LineTotal
                });
            }

            // Overall discount
            decimal.TryParse(txtOverallDisc.Text.Trim(), out decimal ovrDisc);
            inv.OverallDiscount = Math.Max(0m, ovrDisc);

            // Paid amount
            decimal.TryParse(txtInitialPaymentAmt.Text.Trim(), out decimal paid);
            inv.AmountPaid = Math.Max(0m, paid);

            inv.RecalculateTotals();
            return inv;
        }

        private void RecalculateAll()
        {
            var inv = BuildInvoiceObject();

            lblCalcSubtotal.Text = $"₹{inv.Subtotal:N2}";
            lblCalcItemDisc.Text = inv.ItemDiscountTotal > 0 ? $"-₹{inv.ItemDiscountTotal:N2}" : "₹0.00";
            lblCalcTaxable.Text = $"₹{inv.TaxableAmount:N2}";

            bool isInter = (inv.TaxType == "GST_INTER");
            bool isZero = (inv.TaxType == "ZERO_TAX");

            if (isZero)
            {
                lblCalcCgst.Text = "₹0.00 (0%)";
                lblCalcSgst.Text = "₹0.00 (0%)";
                lblCalcIgst.Text = "₹0.00 (0%)";
            }
            else if (isInter)
            {
                lblCalcCgst.Text = "₹0.00 (N/A)";
                lblCalcSgst.Text = "₹0.00 (N/A)";
                lblCalcIgst.Text = $"₹{inv.IgstAmount:N2} ({inv.IgstRate:0}%)";
            }
            else
            {
                lblCalcCgst.Text = $"₹{inv.CgstAmount:N2} ({inv.CgstRate:0}%)";
                lblCalcSgst.Text = $"₹{inv.SgstAmount:N2} ({inv.SgstRate:0}%)";
                lblCalcIgst.Text = "₹0.00 (N/A)";
            }

            lblCalcGrandTotal.Text = $"₹{inv.GrandTotal:N2}";
            lblCalcPaid.Text = $"₹{inv.AmountPaid:N2}";
            lblCalcBalanceDue.Text = $"₹{inv.BalanceDue:N2}";

            // Update Payment Status Pill
            lblPaymentStatusBadge.Tag = inv.Status;
            lblPaymentStatusBadge.Invalidate();
        }

        private void OnPaymentAmountChanged()
        {
            decimal.TryParse(txtInitialPaymentAmt.Text.Trim(), out decimal paid);
            RecalculateAll();
        }

        private void SetPaymentPercentage(decimal pct)
        {
            var inv = BuildInvoiceObject();
            decimal amt = Math.Round(inv.GrandTotal * pct, 2);
            txtInitialPaymentAmt.Text = amt.ToString("F2");
        }

        private void PaintPaymentStatusBadge(object? s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = lblPaymentStatusBadge.Width;
            int h = lblPaymentStatusBadge.Height;
            if (w <= 0 || h <= 0) return;

            string status = lblPaymentStatusBadge.Tag?.ToString() ?? "Unpaid";
            Color bg, bdr, txt;

            switch (status)
            {
                case "Paid":
                    bg = Color.FromArgb(236, 253, 245);
                    bdr = Color.FromArgb(16, 185, 129);
                    txt = Color.FromArgb(5, 150, 105);
                    break;
                case "Partially Paid":
                    bg = Color.FromArgb(238, 242, 255);
                    bdr = Color.FromArgb(99, 102, 241);
                    txt = Color.FromArgb(79, 70, 229);
                    break;
                default:
                    bg = Color.FromArgb(254, 243, 199);
                    bdr = Color.FromArgb(245, 158, 11);
                    txt = Color.FromArgb(217, 119, 6);
                    break;
            }

            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 12);
            using var bBg = new SolidBrush(bg);
            using var pBdr = new Pen(bdr, 1f);
            using var bTxt = new SolidBrush(txt);
            using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.FillPath(bBg, path);
            g.DrawPath(pBdr, path);
            g.DrawString($"● {status}", font, bTxt, new RectangleF(0, 0, w, h), sf);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SAVE, PRINT & EXPORT ACTIONS
        // ═══════════════════════════════════════════════════════════════════════════
        private void SaveInvoice(bool andPrint)
        {
            if (_isSaving) return;

            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item to the invoice.", "Empty Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SwitchToStep(2);
                return;
            }

            _isSaving = true;
            btnSaveInvoice.Enabled = false;
            btnSaveAndPrint.Enabled = false;

            try
            {
                var inv = BuildInvoiceObject();

                PaymentRecord? initialPayment = null;
                if (inv.AmountPaid > 0)
                {
                    initialPayment = new PaymentRecord
                    {
                        Amount = inv.AmountPaid,
                        PaymentMethod = cmbInitialPaymentMode.SelectedItem?.ToString() ?? "Bank Transfer",
                        ReferenceNumber = txtInitialPaymentRef.Text.Trim(),
                        Notes = "Initial payment upon invoice creation"
                    };
                }

                var saved = DatabaseService.SaveInvoice(inv, initialPayment);
                LastSavedInvoice = saved;

                MessageBox.Show(
                    $"Invoice {saved.InvoiceNumber} saved successfully!\n" +
                    $"Customer: {saved.CustomerName}\n" +
                    $"Grand Total: ₹{saved.GrandTotal:N2} | Paid: ₹{saved.AmountPaid:N2} | Balance Due: ₹{saved.BalanceDue:N2}",
                    "Invoice Persisted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (andPrint)
                {
                    var business = DatabaseService.GetBusinessProfile();
                    InvoicePrintService.PrintDirect(saved, business, (IWin32Window?)this.FindForm() ?? this);
                }

                _isDirty = false;
                InvoiceSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save invoice to database: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                btnSaveInvoice.Enabled = true;
                btnSaveAndPrint.Enabled = true;
            }
        }

        private void PreviewInvoice()
        {
            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item to preview.", "Empty Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var inv = BuildInvoiceObject();
            var business = DatabaseService.GetBusinessProfile();
            InvoicePrintService.ShowPrintPreview(inv, business, (IWin32Window?)this.FindForm() ?? this);
        }

        private void ExportInvoicePdf()
        {
            if (_workingItems.Count == 0)
            {
                MessageBox.Show("Please add at least one line item to export.", "Empty Invoice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var inv = BuildInvoiceObject();
            var business = DatabaseService.GetBusinessProfile();
            InvoicePrintService.OpenOrShareInvoice(inv, business);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  THEME & UI HELPERS
        // ═══════════════════════════════════════════════════════════════════════════
        public void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            Color pageBg = isDark ? Color.FromArgb(17, 20, 29) : Color.FromArgb(248, 250, 252);
            Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
            Color textPri = isDark ? Color.White : Color.FromArgb(15, 23, 42);
            Color textSec = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
            Color inputBg = isDark ? Color.FromArgb(28, 33, 48) : Color.White;

            BackColor = pageBg;
            pnlStepContentHost.BackColor = pageBg;
            pnlNavFooter.BackColor = cardBg;

            pnlStepperHeader.Invalidate();
            pnlNavFooter.Invalidate();
            Invalidate(true);
        }

        private static Panel CreateSectionCard(string title, ReiconRenderer.IconType icon)
        {
            var pnl = new Panel
            {
                Padding = new Padding(18, 14, 18, 14)
            };

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = pnl.Width;
                int h = pnl.Height;
                if (w <= 0 || h <= 0) return;

                bool isDark = ThemeManager.IsDarkMode;
                Color cardBg = isDark ? Color.FromArgb(23, 26, 38) : Color.White;
                Color cardBdr = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);

                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 12);
                using var b = new SolidBrush(cardBg);
                using var p = new Pen(cardBdr, 1.2f);
                g.FillPath(b, path);
                g.DrawPath(p, path);

                // Icon & Title
                int iconD = 18;
                ReiconRenderer.DrawIcon(g, icon, new Rectangle(18, 14, iconD, iconD), ThemeManager.PrimaryAccent, 1.8f);

                using var font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var bText = new SolidBrush(isDark ? Color.White : Color.FromArgb(15, 23, 42));
                g.DrawString(title, font, bText, 18 + iconD + 8, 12);

                // Header hairline
                using var pLine = new Pen(isDark ? Color.FromArgb(36, 42, 62) : Color.FromArgb(241, 245, 249), 1f);
                g.DrawLine(pLine, 18, 38, w - 18, 38);
            };

            return pnl;
        }

        private static void AddFormField(Control parent, string labelText, Control input, int x, int y, int width)
        {
            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85),
                Bounds = new Rectangle(x, y, width, 18),
                AutoSize = false,
                UseMnemonic = false
            };

            input.SetBounds(x, y + 20, width, 28);
            input.Font = new Font("Segoe UI", 9.2f);

            parent.Controls.Add(lbl);
            parent.Controls.Add(input);
        }

        private static TeamPillButton CreateMiniButton(string text, int x, int y, int w)
        {
            var btn = new TeamPillButton
            {
                Text = text,
                BorderColor = Color.FromArgb(203, 213, 225),
                BorderWidth = 1f,
                FillColor = Color.FromArgb(241, 245, 249),
                TextColor = Color.FromArgb(71, 85, 105),
                HoverColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
            btn.SetBounds(x, y, w, 28);
            return btn;
        }

        private static void FormatTable(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(241, 245, 249);
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 36;
            dgv.Font = new Font("Segoe UI", 9f);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 34;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 242, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        }
    }
}
