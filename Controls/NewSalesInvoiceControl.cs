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
    /// Production-ready New Sales Invoice page matching the reference billing UI specification.
    /// Supports barcode scanning, live GST calculations, inventory management, customer selection,
    /// payment reconciliation, and atomic database persistence.
    /// </summary>
    public class NewSalesInvoiceControl : UserControl
    {
        // Events
        public event EventHandler? InvoiceSaved;
        public event EventHandler? BackRequested;
        public event EventHandler? CancelRequested;

        // Models & State
        private readonly Invoice _invoice = new();
        private List<Customer> _customers = new();
        private List<Broker> _brokers = new();
        private Customer? _selectedCustomer;
        private bool _isUpdatingUi = false;
        private bool _isSaving = false;

        // Colors (Light theme matching screenshot, adaptive to ThemeManager)
        private static Color C_PageBg => ThemeManager.IsDarkMode ? Color.FromArgb(17, 20, 29) : Color.FromArgb(248, 250, 252);
        private static Color C_CardBg => ThemeManager.IsDarkMode ? Color.FromArgb(24, 28, 42) : Color.White;
        private static Color C_Border => ThemeManager.IsDarkMode ? Color.FromArgb(45, 52, 75) : Color.FromArgb(226, 232, 240);
        private static Color C_Blue => Color.FromArgb(37, 99, 235);
        private static Color C_BlueHover => Color.FromArgb(29, 78, 216);
        private static Color C_TextDark => ThemeManager.IsDarkMode ? Color.White : Color.FromArgb(15, 23, 42);
        private static Color C_TextMid => ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
        private static Color C_LightBlueBg => ThemeManager.IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 246, 255);
        private static Color C_LightGreenBg => ThemeManager.IsDarkMode ? Color.FromArgb(20, 45, 35) : Color.FromArgb(240, 253, 244);
        private static Color C_GreenText => Color.FromArgb(22, 163, 74);

        // Header Controls
        private readonly Panel _pnlTopHeader = new();
        private readonly TeamPillButton _btnBack = new();
        private readonly Label _lblPageTitle = new();
        private readonly TeamPillButton _btnCancel = new();
        private readonly TeamPillButton _btnSaveHeader = new();

        // Section A: Invoice Info Card Controls
        private readonly Panel _pnlCardA = new();
        private readonly Label _lblHdrInvoiceNo = new();
        private readonly Label _lblHdrDate = new();
        private readonly Label _lblHdrDueDate = new();
        private readonly Label _lblHdrCustomer = new();
        private readonly Label _lblHdrBroker = new();
        private readonly TextBox _txtInvoiceNo = new();
        private readonly DateTimePicker _dtpDate = new();
        private readonly DateTimePicker _dtpDueDate = new();
        private readonly ComboBox _cmbCustomer = new();
        private readonly ComboBox _cmbBroker = new();
        private readonly LinkLabel _lnkManageBroker = new();
        private readonly Panel _pnlCustBanner = new();
        private readonly Label _lblCustAvatar = new();
        private readonly Label _lblCustName = new();
        private readonly Label _lblCustAddress = new();
        private readonly Label _lblSupplyBadge = new();

        // Section B: Barcode Toolbar Controls
        private readonly Panel _pnlCardB = new();
        private readonly TextBox _txtBarcode = new();
        private readonly TeamPillButton _btnScan = new();
        private readonly TeamPillButton _btnManualSearch = new();

        // Section C: Product Grid Controls
        private readonly Panel _pnlGridCard = new();
        private readonly DataGridView _dgvItems = new();
        private readonly Panel _pnlEmptyState = new();

        // Section D: Product Counters
        private readonly Panel _pnlCounters = new();
        private readonly Label _lblTotalProducts = new();
        private readonly Label _lblTotalQuantity = new();

        // Section E & F: Bottom Panel
        private readonly Panel _pnlMainContainer = new();
        private readonly Panel _pnlBottomDeck = new();
        private readonly Panel _pnlCardE = new(); // Payment & Notes
        private readonly Panel _pnlCardF = new(); // Invoice Summary

        // Section E Controls
        private readonly ComboBox _cmbPaymentStatus = new();
        private readonly TextBox _txtAmountReceived = new();
        private readonly TextBox _txtNotes = new();

        // Section F Controls
        private readonly Label _lblValSubtotal = new();
        private readonly Label _lblValDiscount = new();
        private readonly Label _lblValTaxable = new();
        private readonly Label _lblValCgst = new();
        private readonly Label _lblValSgst = new();
        private readonly Label _lblValIgst = new();
        private readonly Label _lblValRoundOff = new();
        private readonly Label _lblValGrandTotal = new();
        private readonly Label _lblValBalanceDue = new();
        private readonly Panel _pnlGrandTotalCallout = new();
        private readonly TeamPillButton _btnSaveSummary = new();

        public NewSalesInvoiceControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            InitializeComponentCustom();
            LoadMasterData();
            ResetForm();
            ApplyTheme();

            ThemeManager.ThemeChanged += () => ApplyTheme();
        }

        private void InitializeComponentCustom()
        {
            Dock = DockStyle.Fill;
            BackColor = C_PageBg;
            AutoScroll = true;

            // Master container with padding
            _pnlMainContainer.Dock = DockStyle.Top;
            _pnlMainContainer.AutoSize = true;
            _pnlMainContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _pnlMainContainer.Padding = new Padding(24, 16, 24, 24);
            _pnlMainContainer.BackColor = Color.Transparent;

            BuildHeader();
            BuildSectionA_InvoiceInfo();
            BuildSectionB_BarcodeToolbar();
            BuildSectionC_ProductGrid();
            BuildSectionD_Counters();
            BuildSectionE_and_F_Bottom();

            _pnlMainContainer.Controls.AddRange(new Control[] {
                _pnlBottomDeck,
                _pnlCounters,
                _pnlGridCard,
                _pnlCardB,
                _pnlCardA,
                _pnlTopHeader
            });

            Controls.Add(_pnlMainContainer);

            // Responsive layout adjustments
            Resize += (s, e) => HandleResponsiveLayout();
        }

        private void HandleResponsiveLayout()
        {
            int totalW = Width;
            int totalH = Height;
            if (totalW <= 0) return;

            // Responsive padding and max width for ultra-wide displays
            int clientW;
            if (totalW > 1750)
            {
                clientW = 1650;
                int padX = (totalW - clientW) / 2;
                _pnlMainContainer.Padding = new Padding(padX, 16, padX, 24);
            }
            else
            {
                _pnlMainContainer.Padding = new Padding(24, 16, 24, 24);
                clientW = totalW - 48;
                if (clientW < 800) clientW = 800;
            }

            // Dynamically calculate Grid Height to fill available vertical space on big screens
            int overhead = _pnlTopHeader.Height + 14 + _pnlCardA.Height + 14 + _pnlCardB.Height + 14 + _pnlCounters.Height + 14 + _pnlBottomDeck.Height + 70;
            int calcGridH = totalH - overhead;
            _pnlGridCard.Height = Math.Clamp(calcGridH, 280, 560);

            _pnlTopHeader.Width = clientW;
            _pnlCardA.Width = clientW;
            _pnlCardB.Width = clientW;
            _pnlGridCard.Width = clientW;
            _pnlCounters.Width = clientW;
            _pnlBottomDeck.Width = clientW;

            // Split bottom deck: Left Card (Payment & Notes) ~ 62%, Right Card (Summary) ~ 38%
            int gap = 16;
            int rightW = Math.Clamp((int)(clientW * 0.36f), 380, 480);
            int leftW = clientW - rightW - gap;

            _pnlCardE.SetBounds(0, 0, leftW, _pnlBottomDeck.Height);
            _pnlCardF.SetBounds(leftW + gap, 0, rightW, _pnlBottomDeck.Height);

            // Layout items inside Card A
            LayoutCardAFields();

            // Layout items inside Card B
            LayoutCardBFields();

            // Layout items inside Card E
            LayoutCardEFields();

            // Layout items inside Card F
            LayoutCardFFields();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  TOP HEADER
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildHeader()
        {
            _pnlTopHeader.Height = 46;
            _pnlTopHeader.Dock = DockStyle.Top;
            _pnlTopHeader.Margin = new Padding(0, 0, 0, 14);

            // Back button
            _btnBack.Text = "←";
            _btnBack.Icon = null;
            _btnBack.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            _btnBack.Size = new Size(38, 38);
            _btnBack.Location = new Point(0, 4);
            _btnBack.FillColor = Color.Transparent;
            _btnBack.HoverColor = ThemeManager.IsDarkMode ? Color.FromArgb(36, 42, 60) : Color.FromArgb(241, 245, 249);
            _btnBack.TextColor = C_TextDark;
            _btnBack.BorderColor = C_Border;
            _btnBack.BorderWidth = 1f;
            _btnBack.Click += (s, e) => BackRequested?.Invoke(this, EventArgs.Empty);

            // Title
            _lblPageTitle.Text = "New Sales Invoice";
            _lblPageTitle.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            _lblPageTitle.ForeColor = C_TextDark;
            _lblPageTitle.Location = new Point(48, 8);
            _lblPageTitle.AutoSize = true;

            // Cancel button
            _btnCancel.Text = "Cancel";
            _btnCancel.Icon = null;
            _btnCancel.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _btnCancel.Size = new Size(96, 38);
            _btnCancel.FillColor = C_CardBg;
            _btnCancel.HoverColor = ThemeManager.IsDarkMode ? Color.FromArgb(36, 42, 60) : Color.FromArgb(241, 245, 249);
            _btnCancel.TextColor = C_TextDark;
            _btnCancel.BorderColor = C_Border;
            _btnCancel.BorderWidth = 1f;
            _btnCancel.Click += OnCancelClicked;

            // Save Invoice button
            _btnSaveHeader.Text = "Save Invoice";
            _btnSaveHeader.Icon = null;
            _btnSaveHeader.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _btnSaveHeader.Size = new Size(135, 38);
            _btnSaveHeader.FillColor = C_Blue;
            _btnSaveHeader.HoverColor = C_BlueHover;
            _btnSaveHeader.TextColor = Color.White;
            _btnSaveHeader.Click += (s, e) => PerformSaveInvoice();

            _pnlTopHeader.Controls.AddRange(new Control[] {
                _btnBack, _lblPageTitle, _btnCancel, _btnSaveHeader
            });

            _pnlTopHeader.Resize += (s, e) =>
            {
                int r = _pnlTopHeader.Width;
                _btnSaveHeader.Location = new Point(r - _btnSaveHeader.Width, 4);
                _btnCancel.Location = new Point(_btnSaveHeader.Left - _btnCancel.Width - 10, 4);
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SECTION A: INVOICE INFO CARD
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSectionA_InvoiceInfo()
        {
            _pnlCardA.Height = 148;
            _pnlCardA.Dock = DockStyle.Top;
            _pnlCardA.Margin = new Padding(0, 0, 0, 14);
            _pnlCardA.Paint += (s, e) => PaintCardBackground(e.Graphics, _pnlCardA);

            // Labels above fields
            _lblHdrInvoiceNo.Text = "Invoice No.";
            _lblHdrInvoiceNo.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblHdrInvoiceNo.ForeColor = C_TextMid;
            _lblHdrInvoiceNo.AutoSize = true;

            _lblHdrDate.Text = "Date (DD-MM-YYYY)";
            _lblHdrDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblHdrDate.ForeColor = C_TextMid;
            _lblHdrDate.AutoSize = true;

            _lblHdrDueDate.Text = "Due Date (Optional)";
            _lblHdrDueDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblHdrDueDate.ForeColor = C_TextMid;
            _lblHdrDueDate.AutoSize = true;

            _lblHdrCustomer.Text = "Select Customer *";
            _lblHdrCustomer.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblHdrCustomer.ForeColor = C_TextMid;
            _lblHdrCustomer.AutoSize = true;

            _lblHdrBroker.Text = "Broker Name (Optional)";
            _lblHdrBroker.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblHdrBroker.ForeColor = C_TextMid;
            _lblHdrBroker.AutoSize = true;

            // Styling Controls
            _txtInvoiceNo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _txtInvoiceNo.ForeColor = C_Blue;
            _txtInvoiceNo.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(30, 36, 54) : Color.FromArgb(248, 250, 252);
            _txtInvoiceNo.BorderStyle = BorderStyle.FixedSingle;
            _txtInvoiceNo.ReadOnly = true;

            _dtpDate.Font = new Font("Segoe UI", 10f);
            _dtpDate.Format = DateTimePickerFormat.Custom;
            _dtpDate.CustomFormat = "dd-MM-yyyy";
            _dtpDate.ValueChanged += (s, e) => _invoice.InvoiceDate = _dtpDate.Value;

            _dtpDueDate.Font = new Font("Segoe UI", 10f);
            _dtpDueDate.Format = DateTimePickerFormat.Custom;
            _dtpDueDate.CustomFormat = "dd-MM-yyyy";
            _dtpDueDate.ValueChanged += (s, e) => _invoice.DueDate = _dtpDueDate.Value;

            _cmbCustomer.Font = new Font("Segoe UI", 10f);
            _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbCustomer.SelectedIndexChanged += OnCustomerSelectionChanged;
            SetupModernComboBox(_cmbCustomer);

            _cmbBroker.Font = new Font("Segoe UI", 10f);
            _cmbBroker.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBroker.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbBroker.SelectedItem is Broker b)
                    _invoice.BrokerName = b.Name;
                else
                    _invoice.BrokerName = string.Empty;
            };
            SetupModernComboBox(_cmbBroker);

            _lnkManageBroker.Text = "+ Manage";
            _lnkManageBroker.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lnkManageBroker.LinkColor = C_Blue;
            _lnkManageBroker.ActiveLinkColor = C_BlueHover;
            _lnkManageBroker.AutoSize = true;
            _lnkManageBroker.LinkClicked += (s, e) =>
            {
                using var dlg = new ManageBrokersDialog();
                if (dlg.ShowDialog((IWin32Window?)FindForm() ?? this) == DialogResult.OK)
                {
                    LoadBrokers();
                    if (dlg.LastAddedBroker != null)
                    {
                        for (int i = 0; i < _cmbBroker.Items.Count; i++)
                        {
                            if (_cmbBroker.Items[i] is Broker b && b.Id == dlg.LastAddedBroker.Id)
                            {
                                _cmbBroker.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            };

            // Customer details row 2 banner
            _pnlCustBanner.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _pnlCustBanner.Width - 1, _pnlCustBanner.Height - 1);
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, 8);
                using var bg = new SolidBrush(ThemeManager.IsDarkMode ? Color.FromArgb(20, 24, 38) : Color.FromArgb(248, 250, 252));
                using var pen = new Pen(C_Border, 1f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
            };

            _lblCustAvatar.Size = new Size(36, 36);
            _lblCustAvatar.Text = "👤";
            _lblCustAvatar.BackColor = Color.Transparent;
            _lblCustAvatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                path.AddEllipse(1, 1, 33, 33);
                using var bg = new SolidBrush(ThemeManager.IsDarkMode ? Color.FromArgb(30, 45, 65) : Color.FromArgb(239, 246, 255));
                using var pen = new Pen(Color.FromArgb(191, 219, 254), 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var f = new Font("Segoe UI", 11f, FontStyle.Bold);
                using var b = new SolidBrush(C_Blue);
                g.DrawString(_lblCustAvatar.Text, f, b, new RectangleF(0, 0, 36, 36), sf);
            };

            _lblCustName.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _lblCustName.ForeColor = C_TextDark;
            _lblCustName.BackColor = Color.Transparent;
            _lblCustName.Text = "No Customer Selected";
            _lblCustName.AutoSize = true;

            _lblCustAddress.Font = new Font("Segoe UI", 9f);
            _lblCustAddress.ForeColor = C_TextMid;
            _lblCustAddress.BackColor = Color.Transparent;
            _lblCustAddress.Text = "Address: Not available";
            _lblCustAddress.AutoSize = true;

            _lblSupplyBadge.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _lblSupplyBadge.Text = "Intra-State Supply";
            _lblSupplyBadge.AutoSize = false;
            _lblSupplyBadge.Size = new Size(160, 30);
            _lblSupplyBadge.TextAlign = ContentAlignment.MiddleCenter;
            _lblSupplyBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _lblSupplyBadge.Width - 1, _lblSupplyBadge.Height - 1);
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, 8);
                using var bg = new SolidBrush(ThemeManager.IsDarkMode ? Color.FromArgb(30, 45, 65) : Color.FromArgb(239, 246, 255));
                using var pen = new Pen(C_Blue, 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
                using var brush = new SolidBrush(C_Blue);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_lblSupplyBadge.Text, _lblSupplyBadge.Font, brush, rect, sf);
            };

            _pnlCustBanner.Controls.AddRange(new Control[] {
                _lblCustAvatar, _lblCustName, _lblCustAddress, _lblSupplyBadge
            });

            _pnlCardA.Controls.AddRange(new Control[] {
                _lblHdrInvoiceNo, _lblHdrDate, _lblHdrDueDate, _lblHdrCustomer, _lblHdrBroker,
                _txtInvoiceNo, _dtpDate, _dtpDueDate, _cmbCustomer, _cmbBroker,
                _lnkManageBroker, _pnlCustBanner
            });

            _pnlCardA.Resize += (s, e) => LayoutCardAFields();
        }

        private void LayoutCardAFields()
        {
            int w = _pnlCardA.Width;
            if (w <= 0) return;

            int pad = 18;
            int availW = w - pad * 2;
            int lblY = 12;
            int topY = 36;
            int fH = 34;

            // 5 columns with relative weights:
            // Invoice No (14%), Date (13%), Due Date (13%), Select Customer (36%), Broker (24%)
            int wInv = (int)(availW * 0.14f);
            int wDate = (int)(availW * 0.13f);
            int wDue = (int)(availW * 0.13f);
            int wCust = (int)(availW * 0.36f);
            int wBroker = availW - (wInv + wDate + wDue + wCust) - 40;

            int x = pad;
            _lblHdrInvoiceNo.Location = new Point(x, lblY);
            _txtInvoiceNo.SetBounds(x, topY, wInv, fH);

            x += wInv + 10;
            _lblHdrDate.Location = new Point(x, lblY);
            _dtpDate.SetBounds(x, topY, wDate, fH);

            x += wDate + 10;
            _lblHdrDueDate.Location = new Point(x, lblY);
            _dtpDueDate.SetBounds(x, topY, wDue, fH);

            x += wDue + 10;
            _lblHdrCustomer.Location = new Point(x, lblY);
            _cmbCustomer.SetBounds(x, topY, wCust, fH);

            x += wCust + 10;
            _lblHdrBroker.Location = new Point(x, lblY);
            _cmbBroker.SetBounds(x, topY, wBroker, fH);
            _lnkManageBroker.Location = new Point(x + wBroker - _lnkManageBroker.Width, lblY);

            // Row 2: Customer Details Banner
            int row2Y = topY + fH + 12;
            int bannerH = 50;
            _pnlCustBanner.SetBounds(pad, row2Y, availW, bannerH);
            _lblCustAvatar.Location = new Point(8, 7);
            _lblCustName.Location = new Point(52, 6);
            _lblCustAddress.Location = new Point(52, 26);
            _lblSupplyBadge.Location = new Point(_pnlCustBanner.Width - _lblSupplyBadge.Width - 10, 10);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SECTION B: BARCODE TOOLBAR
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSectionB_BarcodeToolbar()
        {
            _pnlCardB.Height = 64;
            _pnlCardB.Dock = DockStyle.Top;
            _pnlCardB.Margin = new Padding(0, 0, 0, 14);
            _pnlCardB.Paint += (s, e) =>
            {
                PaintCardBackground(e.Graphics, _pnlCardB);
                // Draw barcode visual icon badge on the left
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int iconX = 16;
                int iconY = 13;
                int iconSize = 38;
                using var pPath = GraphicsExtensions.CreateRoundedRectanglePath(iconX, iconY, iconSize, iconSize, 8);
                using var pBrush = new SolidBrush(C_Blue);
                g.FillPath(pBrush, pPath);

                // Draw barcode line pattern inside icon
                using var whitePen = new Pen(Color.White, 2.2f);
                g.DrawLine(whitePen, iconX + 9, iconY + 9, iconX + 9, iconY + 29);
                g.DrawLine(whitePen, iconX + 15, iconY + 9, iconX + 15, iconY + 29);
                g.DrawLine(whitePen, iconX + 20, iconY + 9, iconX + 20, iconY + 29);
                g.DrawLine(whitePen, iconX + 28, iconY + 9, iconX + 28, iconY + 29);
            };

            _txtBarcode.Font = new Font("Segoe UI", 10.5f);
            _txtBarcode.PlaceholderText = "Scan barcode or enter code (e.g. 8901234567890)...   [Press Enter ↵ to Add]";
            _txtBarcode.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ProcessBarcodeScan();
                }
            };

            _btnScan.Text = "Scan Barcode";
            _btnScan.Icon = null;
            _btnScan.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnScan.Size = new Size(145, 38);
            _btnScan.FillColor = C_Blue;
            _btnScan.HoverColor = C_BlueHover;
            _btnScan.TextColor = Color.White;
            _btnScan.Click += (s, e) => ProcessBarcodeScan();

            _btnManualSearch.Text = "+ Manual Search";
            _btnManualSearch.Icon = null;
            _btnManualSearch.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _btnManualSearch.Size = new Size(155, 38);
            _btnManualSearch.FillColor = C_CardBg;
            _btnManualSearch.HoverColor = ThemeManager.IsDarkMode ? Color.FromArgb(36, 42, 60) : Color.FromArgb(241, 245, 249);
            _btnManualSearch.TextColor = C_Blue;
            _btnManualSearch.BorderColor = C_Blue;
            _btnManualSearch.BorderWidth = 1f;
            _btnManualSearch.Click += OnManualSearchClicked;

            _pnlCardB.Controls.AddRange(new Control[] {
                _txtBarcode, _btnScan, _btnManualSearch
            });

            _pnlCardB.Resize += (s, e) => LayoutCardBFields();
        }

        private void LayoutCardBFields()
        {
            int w = _pnlCardB.Width;
            if (w <= 0) return;

            int pad = 16;
            int btnRight = w - pad;

            _btnManualSearch.Location = new Point(btnRight - _btnManualSearch.Width, 13);
            _btnScan.Location = new Point(_btnManualSearch.Left - _btnScan.Width - 10, 13);

            int barcodeLeft = 66;
            int barcodeWidth = _btnScan.Left - barcodeLeft - 14;
            if (barcodeWidth > 100)
            {
                _txtBarcode.SetBounds(barcodeLeft, 15, barcodeWidth, 34);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SECTION C: PRODUCT GRID
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSectionC_ProductGrid()
        {
            _pnlGridCard.Height = 320; // Default baseline, dynamically scaled in HandleResponsiveLayout
            _pnlGridCard.Dock = DockStyle.Top;
            _pnlGridCard.Margin = new Padding(0, 0, 0, 10);
            _pnlGridCard.Padding = new Padding(1);
            _pnlGridCard.Paint += (s, e) => PaintCardBackground(e.Graphics, _pnlGridCard);

            // DataGridView Configuration
            _dgvItems.Dock = DockStyle.Fill;
            _dgvItems.BackgroundColor = C_CardBg;
            _dgvItems.BorderStyle = BorderStyle.None;
            _dgvItems.RowHeadersVisible = false;
            _dgvItems.AllowUserToAddRows = false;
            _dgvItems.AllowUserToDeleteRows = false;
            _dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvItems.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _dgvItems.RowTemplate.Height = 42;
            _dgvItems.Font = new Font("Segoe UI", 9.75f);
            _dgvItems.GridColor = C_Border;
            _dgvItems.EnableHeadersVisualStyles = false;

            _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);
            _dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = C_TextMid;
            _dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgvItems.ColumnHeadersHeight = 40;

            _dgvItems.DefaultCellStyle.BackColor = C_CardBg;
            _dgvItems.DefaultCellStyle.ForeColor = C_TextDark;
            _dgvItems.DefaultCellStyle.SelectionBackColor = ThemeManager.IsDarkMode ? Color.FromArgb(36, 42, 60) : Color.FromArgb(239, 246, 255);
            _dgvItems.DefaultCellStyle.SelectionForeColor = C_Blue;
            _dgvItems.DefaultCellStyle.Padding = new Padding(6, 3, 6, 3);

            // Define Columns matching Screenshot
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Product", HeaderText = "Product", FillWeight = 26, ReadOnly = true });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "HSN", HeaderText = "HSN", FillWeight = 9, ReadOnly = true });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PCS", HeaderText = "PCS", FillWeight = 8, ReadOnly = true });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty", HeaderText = "Qty", FillWeight = 8, ReadOnly = false });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate", FillWeight = 11, ReadOnly = false });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Discount", HeaderText = "Discount", FillWeight = 9, ReadOnly = false });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "GST", HeaderText = "GST %", FillWeight = 8, ReadOnly = false });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Taxable", HeaderText = "Taxable", FillWeight = 11, ReadOnly = true });
            _dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", FillWeight = 12, ReadOnly = true });

            var colAction = new DataGridViewButtonColumn
            {
                Name = "Actions",
                HeaderText = "Actions",
                Text = "✕",
                UseColumnTextForButtonValue = true,
                FillWeight = 8,
                FlatStyle = FlatStyle.Flat
            };
            colAction.DefaultCellStyle.ForeColor = Color.FromArgb(239, 68, 68);
            _dgvItems.Columns.Add(colAction);

            // Alignments
            _dgvItems.Columns["Qty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["Rate"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["Discount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["GST"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["Taxable"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvItems.Columns["Actions"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Events
            _dgvItems.CellContentClick += OnGridCellContentClick;
            _dgvItems.CellValueChanged += OnGridCellValueChanged;
            _dgvItems.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_dgvItems.IsCurrentCellDirty)
                    _dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // Enhanced Empty state overlay
            _pnlEmptyState.Dock = DockStyle.Fill;
            _pnlEmptyState.BackColor = C_CardBg;
            _pnlEmptyState.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                int w = _pnlEmptyState.Width;
                int h = _pnlEmptyState.Height;
                if (w <= 0 || h <= 0) return;

                int centerY = h / 2;

                // Center icon badge
                int iconDiameter = 54;
                int iconX = (w - iconDiameter) / 2;
                int iconY = centerY - 58;
                using (var path = GraphicsExtensions.CreateRoundedRectanglePath(iconX, iconY, iconDiameter, iconDiameter, 14))
                using (var bgBrush = new SolidBrush(C_LightBlueBg))
                using (var borderPen = new Pen(Color.FromArgb(191, 219, 254), 1.2f))
                {
                    g.FillPath(bgBrush, path);
                    g.DrawPath(borderPen, path);
                }

                // Draw box icon inside
                using (var iconPen = new Pen(C_Blue, 2f))
                {
                    g.DrawRectangle(iconPen, iconX + 15, iconY + 16, 24, 22);
                    g.DrawLine(iconPen, iconX + 15, iconY + 22, iconX + 39, iconY + 22);
                    g.DrawLine(iconPen, iconX + 27, iconY + 16, iconX + 27, iconY + 22);
                }

                using var fTitle = new Font("Segoe UI", 12.5f, FontStyle.Bold);
                using var fSub = new Font("Segoe UI", 9.5f);
                using var bTitle = new SolidBrush(C_TextDark);
                using var bSub = new SolidBrush(C_TextMid);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                g.DrawString("No items added to Invoice", fTitle, bTitle, new RectangleF(0, centerY + 6, w, 26), sf);
                g.DrawString("Search and select a product above or scan a barcode to add line items.", fSub, bSub, new RectangleF(0, centerY + 34, w, 22), sf);
            };

            _pnlGridCard.Controls.Add(_pnlEmptyState);
            _pnlGridCard.Controls.Add(_dgvItems);
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            bool hasItems = _dgvItems.Rows.Count > 0;
            _pnlEmptyState.Visible = !hasItems;
            _dgvItems.Visible = hasItems;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SECTION D: PRODUCT COUNTERS
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSectionD_Counters()
        {
            _pnlCounters.Height = 38;
            _pnlCounters.Dock = DockStyle.Top;
            _pnlCounters.Margin = new Padding(0, 0, 0, 14);

            _lblTotalProducts.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblTotalProducts.Text = "Total Products: 0";
            _lblTotalProducts.AutoSize = false;
            _lblTotalProducts.Size = new Size(150, 30);
            _lblTotalProducts.TextAlign = ContentAlignment.MiddleCenter;
            _lblTotalProducts.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _lblTotalProducts.Width - 1, _lblTotalProducts.Height - 1);
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, 8);
                using var bg = new SolidBrush(C_LightBlueBg);
                using var pen = new Pen(Color.FromArgb(191, 219, 254), 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
                using var brush = new SolidBrush(C_Blue);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_lblTotalProducts.Text, _lblTotalProducts.Font, brush, rect, sf);
            };

            _lblTotalQuantity.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblTotalQuantity.Text = "Total Quantity: 0 units";
            _lblTotalQuantity.AutoSize = false;
            _lblTotalQuantity.Size = new Size(170, 30);
            _lblTotalQuantity.TextAlign = ContentAlignment.MiddleCenter;
            _lblTotalQuantity.Location = new Point(160, 0);
            _lblTotalQuantity.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _lblTotalQuantity.Width - 1, _lblTotalQuantity.Height - 1);
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, 8);
                using var bg = new SolidBrush(C_LightGreenBg);
                using var pen = new Pen(Color.FromArgb(187, 247, 208), 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
                using var brush = new SolidBrush(C_GreenText);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(_lblTotalQuantity.Text, _lblTotalQuantity.Font, brush, rect, sf);
            };

            _pnlCounters.Controls.AddRange(new Control[] {
                _lblTotalProducts, _lblTotalQuantity
            });
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SECTION E & F: BOTTOM DECK (PAYMENT & NOTES / INVOICE SUMMARY)
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSectionE_and_F_Bottom()
        {
            _pnlBottomDeck.Height = 325;
            _pnlBottomDeck.Dock = DockStyle.Top;
            _pnlBottomDeck.Margin = new Padding(0, 0, 0, 16);

            // Left Card (Section E)
            _pnlCardE.Paint += (s, e) =>
            {
                PaintCardBackground(e.Graphics, _pnlCardE);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var f = new Font("Segoe UI", 11.5f, FontStyle.Bold);
                using var b = new SolidBrush(C_TextDark);
                g.DrawString("Payment Received & Notes", f, b, 18, 14);

                using var fLbl = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var bLbl = new SolidBrush(C_TextMid);
                g.DrawString("Payment Status", fLbl, bLbl, 18, 46);
                g.DrawString("Amount Received (₹)", fLbl, bLbl, _txtAmountReceived.Left, 46);
                g.DrawString("Invoice Notes / Terms (Optional)", fLbl, bLbl, 18, 114);
            };

            _cmbPaymentStatus.Font = new Font("Segoe UI", 10f);
            _cmbPaymentStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbPaymentStatus.Items.AddRange(new object[] { "Unpaid", "Partially Paid", "Paid" });
            _cmbPaymentStatus.SelectedIndex = 0;
            _cmbPaymentStatus.SelectedIndexChanged += OnPaymentStatusChanged;
            SetupModernComboBox(_cmbPaymentStatus);

            _txtAmountReceived.Font = new Font("Segoe UI", 10f);
            _txtAmountReceived.Text = "0";
            _txtAmountReceived.TextChanged += (s, e) => OnAmountReceivedChanged();

            _txtNotes.Font = new Font("Segoe UI", 9.5f);
            _txtNotes.Multiline = true;
            _txtNotes.ScrollBars = ScrollBars.Vertical;
            _txtNotes.Text = "Thank you for your business!";
            _txtNotes.TextChanged += (s, e) => _invoice.Notes = _txtNotes.Text;

            _pnlCardE.Controls.AddRange(new Control[] {
                _cmbPaymentStatus, _txtAmountReceived, _txtNotes
            });

            // Right Card (Section F)
            _pnlCardF.Paint += (s, e) =>
            {
                PaintCardBackground(e.Graphics, _pnlCardF);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var f = new Font("Segoe UI", 11.5f, FontStyle.Bold);
                using var b = new SolidBrush(C_TextDark);
                g.DrawString("Invoice Summary", f, b, 18, 14);

                using var fLbl = new Font("Segoe UI", 9f);
                using var bLbl = new SolidBrush(C_TextMid);
                int rX = 18;
                g.DrawString("Subtotal (Before Disc/Tax)", fLbl, bLbl, rX, 44);
                g.DrawString("Total Discount", fLbl, bLbl, rX, 68);
                g.DrawString("Taxable Amount", fLbl, bLbl, rX, 92);
                g.DrawString("CGST", fLbl, bLbl, rX, 116);
                g.DrawString("SGST", fLbl, bLbl, rX, 140);
                g.DrawString("Round Off", fLbl, bLbl, rX, 164);
                g.DrawString("Balance Due", fLbl, bLbl, rX, 246);
            };

            // Value Labels in Card F
            SetupSummaryValueLabel(_lblValSubtotal, 44);
            SetupSummaryValueLabel(_lblValDiscount, 68);
            SetupSummaryValueLabel(_lblValTaxable, 92);
            SetupSummaryValueLabel(_lblValCgst, 116);
            SetupSummaryValueLabel(_lblValSgst, 140);
            SetupSummaryValueLabel(_lblValRoundOff, 164);
            SetupSummaryValueLabel(_lblValBalanceDue, 246);

            // Grand Total Callout Banner
            _pnlGrandTotalCallout.Height = 44;
            _pnlGrandTotalCallout.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _pnlGrandTotalCallout.Width - 1, _pnlGrandTotalCallout.Height - 1);
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, 8);
                using var bg = new SolidBrush(C_LightBlueBg);
                using var pen = new Pen(Color.FromArgb(191, 219, 254), 1.2f);
                g.FillPath(bg, path);
                g.DrawPath(pen, path);

                using var fTitle = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                using var bTitle = new SolidBrush(C_Blue);
                g.DrawString("GRAND TOTAL", fTitle, bTitle, 12, 13);
            };

            _lblValGrandTotal.Font = new Font("Segoe UI", 13.5f, FontStyle.Bold);
            _lblValGrandTotal.ForeColor = C_Blue;
            _lblValGrandTotal.TextAlign = ContentAlignment.MiddleRight;
            _lblValGrandTotal.Text = "₹0.00";
            _lblValGrandTotal.Dock = DockStyle.Right;
            _lblValGrandTotal.Width = 180;
            _pnlGrandTotalCallout.Controls.Add(_lblValGrandTotal);

            // Bottom Save button
            _btnSaveSummary.Text = "Save Sales Invoice";
            _btnSaveSummary.Icon = null;
            _btnSaveSummary.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            _btnSaveSummary.Height = 44;
            _btnSaveSummary.FillColor = C_Blue;
            _btnSaveSummary.HoverColor = C_BlueHover;
            _btnSaveSummary.TextColor = Color.White;
            _btnSaveSummary.Click += (s, e) => PerformSaveInvoice();

            _pnlCardF.Controls.AddRange(new Control[] {
                _lblValSubtotal, _lblValDiscount, _lblValTaxable,
                _lblValCgst, _lblValSgst, _lblValRoundOff,
                _pnlGrandTotalCallout, _lblValBalanceDue, _btnSaveSummary
            });

            _pnlBottomDeck.Controls.AddRange(new Control[] {
                _pnlCardE, _pnlCardF
            });
        }

        private void SetupSummaryValueLabel(Label lbl, int y)
        {
            lbl.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lbl.ForeColor = C_TextDark;
            lbl.TextAlign = ContentAlignment.MiddleRight;
            lbl.Text = "₹0.00";
            lbl.Tag = y;
        }

        private void LayoutCardEFields()
        {
            int w = _pnlCardE.Width;
            if (w <= 0) return;

            int pad = 18;
            int availW = w - pad * 2;

            int wStatus = Math.Clamp((int)(availW * 0.45f), 180, 280);
            int wAmt = Math.Clamp(availW - wStatus - 16, 180, 280);

            _cmbPaymentStatus.SetBounds(pad, 70, wStatus, 32);
            _txtAmountReceived.SetBounds(pad + wStatus + 16, 70, wAmt, 32);
            _txtNotes.SetBounds(pad, 138, availW, _pnlCardE.Height - 156);
        }

        private void LayoutCardFFields()
        {
            int w = _pnlCardF.Width;
            if (w <= 0) return;

            int pad = 18;
            int rightX = w - pad;

            int valW = 160;
            int valX = rightX - valW;

            PositionSummaryLabel(_lblValSubtotal, valX, valW);
            PositionSummaryLabel(_lblValDiscount, valX, valW);
            PositionSummaryLabel(_lblValTaxable, valX, valW);
            PositionSummaryLabel(_lblValCgst, valX, valW);
            PositionSummaryLabel(_lblValSgst, valX, valW);
            PositionSummaryLabel(_lblValRoundOff, valX, valW);
            PositionSummaryLabel(_lblValBalanceDue, valX, valW);

            _pnlGrandTotalCallout.SetBounds(pad, 190, w - pad * 2, 44);
            _btnSaveSummary.SetBounds(pad, 268, w - pad * 2, 44);
        }

        private void PositionSummaryLabel(Label lbl, int x, int width)
        {
            if (lbl.Tag is int y)
            {
                lbl.SetBounds(x, y, width, 20);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  PAINTING HELPERS
        // ═══════════════════════════════════════════════════════════════════════════
        private static void PaintCardBackground(Graphics g, Panel pnl)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnl.Width;
            int h = pnl.Height;
            if (w <= 0 || h <= 0) return;

            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, w - 1, h - 1, 10);
            using var bg = new SolidBrush(C_CardBg);
            using var pen = new Pen(C_Border, 1.2f);
            g.FillPath(bg, path);
            g.DrawPath(pen, path);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  MODERN CONTROL STYLING & THEME REFRESH
        // ═══════════════════════════════════════════════════════════════════════════
        private void SetupModernComboBox(ComboBox cmb)
        {
            cmb.DrawMode = DrawMode.OwnerDrawFixed;
            cmb.ItemHeight = 24;
            cmb.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                bool isDark = ThemeManager.IsDarkMode;
                Color bg = isSelected
                    ? (isDark ? Color.FromArgb(45, 55, 80) : Color.FromArgb(239, 246, 255))
                    : (isDark ? Color.FromArgb(30, 36, 54) : Color.White);
                Color textCol = isSelected
                    ? C_Blue
                    : (isDark ? Color.White : Color.FromArgb(15, 23, 42));

                using var bgBrush = new SolidBrush(bg);
                g.FillRectangle(bgBrush, e.Bounds);

                string text = cmb.Items[e.Index]?.ToString() ?? "";
                using var textBrush = new SolidBrush(textCol);
                using var f = new Font("Segoe UI", 9.5f);
                var textRect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
                using var sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(text, f, textBrush, textRect, sf);
            };
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = C_PageBg;

            Color inputBg = isDark ? Color.FromArgb(30, 36, 54) : Color.White;
            Color inputFg = isDark ? Color.White : Color.FromArgb(15, 23, 42);

            _txtBarcode.BackColor = inputBg;
            _txtBarcode.ForeColor = inputFg;
            _txtNotes.BackColor = inputBg;
            _txtNotes.ForeColor = inputFg;
            _txtAmountReceived.BackColor = inputBg;
            _txtAmountReceived.ForeColor = inputFg;
            _txtInvoiceNo.BackColor = isDark ? Color.FromArgb(24, 28, 42) : Color.FromArgb(248, 250, 252);

            _dgvItems.BackgroundColor = C_CardBg;
            _dgvItems.DefaultCellStyle.BackColor = C_CardBg;
            _dgvItems.DefaultCellStyle.ForeColor = C_TextDark;
            _dgvItems.DefaultCellStyle.SelectionBackColor = isDark ? Color.FromArgb(45, 55, 80) : Color.FromArgb(239, 246, 255);
            _dgvItems.DefaultCellStyle.SelectionForeColor = C_Blue;
            _dgvItems.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(250, 252, 255);
            _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);
            _dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = C_TextMid;
            _dgvItems.GridColor = C_Border;

            Invalidate(true);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  DATA MANAGEMENT & LOGIC
        // ═══════════════════════════════════════════════════════════════════════════
        public void LoadMasterData()
        {
            LoadCustomers();
            LoadBrokers();
        }

        private void LoadCustomers()
        {
            _customers = DatabaseService.GetCustomers();
            _cmbCustomer.Items.Clear();
            foreach (var c in _customers)
            {
                _cmbCustomer.Items.Add(c);
            }

            if (_cmbCustomer.Items.Count > 0 && _cmbCustomer.SelectedIndex < 0)
            {
                _cmbCustomer.SelectedIndex = 0;
            }
        }

        private void LoadBrokers()
        {
            _brokers = DatabaseService.GetBrokers();
            _cmbBroker.Items.Clear();
            _cmbBroker.Items.Add("None (Direct Sale)");
            foreach (var b in _brokers)
            {
                _cmbBroker.Items.Add(b);
            }

            if (_cmbBroker.Items.Count > 0 && _cmbBroker.SelectedIndex < 0)
            {
                _cmbBroker.SelectedIndex = 0;
            }
        }

        public void ResetForm()
        {
            _isUpdatingUi = true;
            try
            {
                _invoice.Items.Clear();
                _dgvItems.Rows.Clear();
                _txtInvoiceNo.Text = DatabaseService.GetNextSalesInvoiceNumber();
                _invoice.InvoiceNumber = _txtInvoiceNo.Text;
                _dtpDate.Value = DateTime.Today;
                _dtpDueDate.Value = DateTime.Today.AddDays(15);
                _invoice.InvoiceDate = DateTime.Today;
                _invoice.DueDate = DateTime.Today.AddDays(15);

                _cmbPaymentStatus.SelectedIndex = 0;
                _txtAmountReceived.Text = "0";
                _txtNotes.Text = "Thank you for your business!";
                _txtBarcode.Clear();

                if (_cmbCustomer.Items.Count > 0)
                    _cmbCustomer.SelectedIndex = 0;
                if (_cmbBroker.Items.Count > 0)
                    _cmbBroker.SelectedIndex = 0;

                _lblCustAvatar.Text = "👤";
                _lblCustAvatar.Invalidate();

                UpdateEmptyState();
                RecalculateAll();
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void OnCustomerSelectionChanged(object? sender, EventArgs e)
        {
            if (_cmbCustomer.SelectedItem is Customer c)
            {
                _selectedCustomer = c;
                _invoice.CustomerId = c.Id;
                _invoice.CustomerName = c.Name;
                _invoice.CustomerCompany = c.CompanyName;
                _invoice.CustomerAddress = c.BillingAddress;
                _invoice.CustomerPhone = c.Phone;
                _invoice.CustomerEmail = c.Email;
                _invoice.CustomerGSTIN = c.GSTIN;

                _lblCustName.Text = string.IsNullOrWhiteSpace(c.CompanyName) ? c.Name : $"{c.Name} ({c.CompanyName})";
                _lblCustAddress.Text = string.IsNullOrWhiteSpace(c.BillingAddress) ? "Address: Gujarat" : $"Address: {c.BillingAddress}";

                string initial = !string.IsNullOrWhiteSpace(c.Name) ? c.Name.Substring(0, 1).ToUpperInvariant() : "👤";
                _lblCustAvatar.Text = initial;
                _lblCustAvatar.Invalidate();

                // Determine Intra-State vs Inter-State supply based on state
                var biz = DatabaseService.GetBusinessProfile();
                bool isIntraState = true;
                if (!string.IsNullOrWhiteSpace(c.GSTIN) && c.GSTIN.Length >= 2 && !string.IsNullOrWhiteSpace(biz.GSTIN) && biz.GSTIN.Length >= 2)
                {
                    isIntraState = c.GSTIN.Substring(0, 2) == biz.GSTIN.Substring(0, 2);
                }
                else if (c.BillingAddress.IndexOf("Gujarat", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    isIntraState = true;
                }

                _invoice.TaxType = isIntraState ? "GST_INTRA" : "GST_INTER";
                _lblSupplyBadge.Text = isIntraState ? "Intra-State Supply" : "Inter-State Supply";
                _lblSupplyBadge.Invalidate();

                RecalculateAll();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  BARCODE & PRODUCT SCANNING
        // ═══════════════════════════════════════════════════════════════════════════
        private void ProcessBarcodeScan()
        {
            string code = _txtBarcode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                _txtBarcode.Focus();
                return;
            }

            var product = DatabaseService.GetProductByBarcode(code);
            if (product == null)
            {
                // Fallback: search by product code or name
                var matches = DatabaseService.GetProducts(code);
                if (matches.Count > 0)
                    product = matches[0];
            }

            if (product != null)
            {
                AddOrIncrementProduct(product);
                _txtBarcode.Clear();
                _txtBarcode.Focus();
            }
            else
            {
                var res = MessageBox.Show(
                    $"Product with barcode or code '{code}' was not found in catalog.\n\nWould you like to search manually or add a new product?",
                    "Product Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    OnManualSearchClicked(null, EventArgs.Empty);
                }
                else
                {
                    _txtBarcode.SelectAll();
                    _txtBarcode.Focus();
                }
            }
        }

        private void OnManualSearchClicked(object? sender, EventArgs e)
        {
            using var dlg = new ProductSearchDialog();
            if (dlg.ShowDialog((IWin32Window?)FindForm() ?? this) == DialogResult.OK && dlg.SelectedProduct != null)
            {
                AddOrIncrementProduct(dlg.SelectedProduct);
                _txtBarcode.Focus();
            }
        }

        public void AddOrIncrementProduct(ProductItem p)
        {
            // Check if product is already in the grid
            for (int i = 0; i < _dgvItems.Rows.Count; i++)
            {
                var row = _dgvItems.Rows[i];
                if (row.Tag is InvoiceItem item && item.ProductId == p.Id)
                {
                    item.Quantity += 1;
                    row.Cells["Qty"].Value = item.Quantity;
                    RecalculateRow(row);
                    RecalculateAll();
                    return;
                }
            }

            // New Item
            var newItem = new InvoiceItem
            {
                ProductId = p.Id,
                ServiceName = p.Name,
                HSN = p.HSN,
                Unit = p.Unit,
                Quantity = 1m,
                UnitPrice = p.SalePrice,
                DiscountPercent = 0m,
                TaxPercent = p.TaxRate
            };
            newItem.CalculateLineTotal();

            int rIdx = _dgvItems.Rows.Add(
                newItem.ServiceName,
                newItem.HSN,
                newItem.Unit,
                newItem.Quantity,
                newItem.UnitPrice,
                newItem.DiscountPercent,
                newItem.TaxPercent,
                $"₹{newItem.TaxableAmount:N2}",
                $"₹{newItem.LineTotal:N2}"
            );

            _dgvItems.Rows[rIdx].Tag = newItem;
            _invoice.Items.Add(newItem);

            UpdateEmptyState();
            RecalculateAll();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  GRID CALCULATIONS & EVENTS
        // ═══════════════════════════════════════════════════════════════════════════
        private void OnGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _dgvItems.Columns[e.ColumnIndex].Name == "Actions")
            {
                if (_dgvItems.Rows[e.RowIndex].Tag is InvoiceItem item)
                {
                    _invoice.Items.Remove(item);
                }
                _dgvItems.Rows.RemoveAt(e.RowIndex);
                UpdateEmptyState();
                RecalculateAll();
            }
        }

        private void OnGridCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdatingUi || e.RowIndex < 0) return;

            string colName = _dgvItems.Columns[e.ColumnIndex].Name;
            if (colName is "Qty" or "Rate" or "Discount" or "GST")
            {
                RecalculateRow(_dgvItems.Rows[e.RowIndex]);
                RecalculateAll();
            }
        }

        private void RecalculateRow(DataGridViewRow row)
        {
            if (row.Tag is not InvoiceItem item) return;

            decimal qty = 1m;
            decimal rate = 0m;
            decimal disc = 0m;
            decimal gst = 18m;

            if (decimal.TryParse(row.Cells["Qty"].Value?.ToString(), out var q) && q >= 0)
                qty = q;
            if (decimal.TryParse(row.Cells["Rate"].Value?.ToString(), out var r) && r >= 0)
                rate = r;
            if (decimal.TryParse(row.Cells["Discount"].Value?.ToString(), out var d) && d >= 0 && d <= 100)
                disc = d;
            if (decimal.TryParse(row.Cells["GST"].Value?.ToString(), out var g) && g >= 0)
                gst = g;

            item.Quantity = qty;
            item.UnitPrice = rate;
            item.DiscountPercent = disc;
            item.TaxPercent = gst;
            item.CalculateLineTotal();

            _isUpdatingUi = true;
            try
            {
                row.Cells["Taxable"].Value = $"₹{item.TaxableAmount:N2}";
                row.Cells["Amount"].Value = $"₹{item.LineTotal:N2}";
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void RecalculateAll()
        {
            _invoice.RecalculateTotals();

            // Update Counters
            _lblTotalProducts.Text = $"Total Products: {_invoice.Items.Count}";
            _lblTotalQuantity.Text = $"Total Quantity: {_invoice.TotalQuantity:0.##} units";
            _lblTotalProducts.Invalidate();
            _lblTotalQuantity.Invalidate();

            // Update Summary Card Values
            _lblValSubtotal.Text = $"₹{_invoice.Subtotal:N2}";
            _lblValDiscount.Text = $"-₹{_invoice.ItemDiscountTotal:N2}";
            _lblValTaxable.Text = $"₹{_invoice.TaxableAmount:N2}";
            _lblValCgst.Text = $"₹{_invoice.CgstAmount:N2}";
            _lblValSgst.Text = $"₹{_invoice.SgstAmount:N2}";
            _lblValRoundOff.Text = $"₹{_invoice.RoundOff:N2}";
            _lblValGrandTotal.Text = $"₹{_invoice.GrandTotal:N2}";

            // Reconcile Payment
            UpdatePaymentReconciliation();
        }

        private void OnPaymentStatusChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;

            string status = _cmbPaymentStatus.SelectedItem?.ToString() ?? "Unpaid";
            _isUpdatingUi = true;
            try
            {
                if (status == "Unpaid")
                {
                    _txtAmountReceived.Text = "0";
                    _txtAmountReceived.Enabled = false;
                    _invoice.AmountPaid = 0m;
                }
                else if (status == "Paid")
                {
                    _txtAmountReceived.Text = _invoice.GrandTotal.ToString("F2");
                    _txtAmountReceived.Enabled = false;
                    _invoice.AmountPaid = _invoice.GrandTotal;
                }
                else // Partially Paid
                {
                    _txtAmountReceived.Enabled = true;
                    if (decimal.TryParse(_txtAmountReceived.Text, out var amt))
                        _invoice.AmountPaid = amt;
                    else
                        _invoice.AmountPaid = 0m;
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }

            RecalculateBalance();
        }

        private void OnAmountReceivedChanged()
        {
            if (_isUpdatingUi) return;

            if (decimal.TryParse(_txtAmountReceived.Text, out var amt) && amt >= 0)
            {
                _invoice.AmountPaid = amt;
            }
            else
            {
                _invoice.AmountPaid = 0m;
            }

            RecalculateBalance();
        }

        private void UpdatePaymentReconciliation()
        {
            string status = _cmbPaymentStatus.SelectedItem?.ToString() ?? "Unpaid";
            if (status == "Paid")
            {
                _isUpdatingUi = true;
                _txtAmountReceived.Text = _invoice.GrandTotal.ToString("F2");
                _invoice.AmountPaid = _invoice.GrandTotal;
                _isUpdatingUi = false;
            }
            RecalculateBalance();
        }

        private void RecalculateBalance()
        {
            _invoice.BalanceDue = Math.Max(0m, _invoice.GrandTotal - _invoice.AmountPaid);
            _lblValBalanceDue.Text = $"₹{_invoice.BalanceDue:N2}";

            // Match status automatically if partially paid or paid
            if (!_isUpdatingUi)
            {
                _isUpdatingUi = true;
                try
                {
                    if (_invoice.AmountPaid <= 0)
                    {
                        _cmbPaymentStatus.SelectedItem = "Unpaid";
                        _invoice.Status = "Unpaid";
                    }
                    else if (_invoice.AmountPaid >= _invoice.GrandTotal && _invoice.GrandTotal > 0)
                    {
                        _cmbPaymentStatus.SelectedItem = "Paid";
                        _invoice.Status = "Paid";
                    }
                    else
                    {
                        _cmbPaymentStatus.SelectedItem = "Partially Paid";
                        _invoice.Status = "Partially Paid";
                    }
                }
                finally
                {
                    _isUpdatingUi = false;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  SAVE WORKFLOW & DATABASE PERSISTENCE
        // ═══════════════════════════════════════════════════════════════════════════
        public void PerformSaveInvoice()
        {
            if (_isSaving) return;

            // 1. Validation
            if (_selectedCustomer == null && _cmbCustomer.SelectedItem is Customer c)
            {
                _selectedCustomer = c;
            }

            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer for this sales invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbCustomer.Focus();
                return;
            }

            if (_invoice.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one product item to the invoice.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtBarcode.Focus();
                return;
            }

            foreach (var item in _invoice.Items)
            {
                if (item.Quantity <= 0)
                {
                    MessageBox.Show($"Product '{item.ServiceName}' has an invalid quantity ({item.Quantity}). Must be greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (_invoice.AmountPaid < 0)
            {
                MessageBox.Show("Amount received cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtAmountReceived.Focus();
                return;
            }

            _isSaving = true;
            _btnSaveHeader.Enabled = false;
            _btnSaveSummary.Enabled = false;

            try
            {
                // Ensure fresh invoice number
                _invoice.InvoiceNumber = DatabaseService.GetNextSalesInvoiceNumber();
                _invoice.CustomerId = _selectedCustomer.Id;
                _invoice.CustomerName = _selectedCustomer.Name;
                _invoice.CustomerCompany = _selectedCustomer.CompanyName;
                _invoice.CustomerAddress = _selectedCustomer.BillingAddress;
                _invoice.CustomerPhone = _selectedCustomer.Phone;
                _invoice.CustomerEmail = _selectedCustomer.Email;
                _invoice.CustomerGSTIN = _selectedCustomer.GSTIN;
                _invoice.BrokerName = _cmbBroker.SelectedItem is Broker b ? b.Name : "";
                _invoice.Notes = _txtNotes.Text;

                // Setup payment record if amount paid > 0
                _invoice.Payments.Clear();
                if (_invoice.AmountPaid > 0)
                {
                    _invoice.Payments.Add(new PaymentRecord
                    {
                        PaymentDate = DateTime.Now,
                        Amount = _invoice.AmountPaid,
                        PaymentMethod = "Cash",
                        ReferenceNumber = "REC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Notes = "Payment received at billing"
                    });
                }

                // Final calculation
                _invoice.RecalculateTotals();

                // Atomic save with transaction, stock decrement, and customer ledger update
                DatabaseService.SaveInvoice(_invoice);

                // Show success dialog with options
                ShowSaveSuccessDialog(_invoice);

                InvoiceSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save invoice:\n\n{ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                _btnSaveHeader.Enabled = true;
                _btnSaveSummary.Enabled = true;
            }
        }

        private void ShowSaveSuccessDialog(Invoice saved)
        {
            var res = MessageBox.Show(
                $"Sales Invoice '{saved.InvoiceNumber}' has been successfully created and recorded in the database!\n\n" +
                $"Grand Total: ₹{saved.GrandTotal:N2}\n" +
                $"Amount Received: ₹{saved.AmountPaid:N2}\n" +
                $"Balance Due: ₹{saved.BalanceDue:N2}\n\n" +
                "Would you like to print or view the invoice now?\n" +
                "(Click 'Yes' to Print, 'No' to Open PDF / View, or 'Cancel' to create another)",
                "Invoice Saved Successfully",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Information);

            var biz = DatabaseService.GetBusinessProfile();

            if (res == DialogResult.Yes)
            {
                InvoicePrintService.PrintDirect(saved, biz, this);
                ResetForm();
            }
            else if (res == DialogResult.No)
            {
                InvoicePrintService.OpenOrShareInvoice(saved, biz);
                ResetForm();
            }
            else
            {
                ResetForm();
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            if (_invoice.Items.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to discard this sales invoice? Any unsaved item entries will be lost.",
                    "Discard Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            ResetForm();
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
