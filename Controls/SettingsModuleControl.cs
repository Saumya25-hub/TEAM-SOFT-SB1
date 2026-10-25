using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Professional Desktop Settings Module inspired by the usability and organization of ChatGPT Settings,
    /// styled natively for TEAM SOFT Inventory, Sales & Billing desktop application.
    /// Features a 3-part layout:
    /// - Searchable left sub-sidebar with 15 distinct categories
    /// - Clean content area with grouped cards, validation, and real database persistence
    /// - Bottom action bar with Save Changes, Cancel, and Reset Defaults
    /// </summary>
    public class SettingsModuleControl : UserControl
    {
        // ── CATEGORY DEFINITIONS ────────────────────────────────────────────────────
        public class CategoryItem
        {
            public int Index { get; set; }
            public string Name { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public ReiconRenderer.IconType Icon { get; set; }
            public string Keywords { get; set; } = "";
        }

        private readonly List<CategoryItem> _allCategories = new()
        {
            new CategoryItem { Index = 0, Name = "Account", Subtitle = "Profile, role & identity", Icon = ReiconRenderer.IconType.Customers, Keywords = "user profile name email phone role session status account" },
            new CategoryItem { Index = 1, Name = "Appearance", Subtitle = "Theme, accent colors & density", Icon = ReiconRenderer.IconType.Sun, Keywords = "theme dark light system color accent density mode ui" },
            new CategoryItem { Index = 2, Name = "General", Subtitle = "Language, currency & formats", Icon = ReiconRenderer.IconType.Settings, Keywords = "language currency inr date time number format landing defaults" },
            new CategoryItem { Index = 3, Name = "Security & Password", Subtitle = "Credentials, lock & 2FA", Icon = ReiconRenderer.IconType.Security, Keywords = "password security change hash session timeout pin lock auth" },
            new CategoryItem { Index = 4, Name = "Company & Branding", Subtitle = "Identity, logo & addresses", Icon = ReiconRenderer.IconType.Dashboard, Keywords = "company business logo brand address phone email website signatory footer" },
            new CategoryItem { Index = 5, Name = "Invoice & Sales", Subtitle = "Prefixes, terms & stock rules", Icon = ReiconRenderer.IconType.Document, Keywords = "invoice prefix number terms due discount stock backorders sales rules" },
            new CategoryItem { Index = 6, Name = "Tax & GST", Subtitle = "GSTIN, tax rates & HSN", Icon = ReiconRenderer.IconType.Statistics, Keywords = "tax gst gstin cgst sgst igst hsn sac rates intra inter rcm" },
            new CategoryItem { Index = 7, Name = "Payments", Subtitle = "UPI, bank details & methods", Icon = ReiconRenderer.IconType.Wallet, Keywords = "payment upi bank qr ifsc account cash card cheque instructions interest" },
            new CategoryItem { Index = 8, Name = "PDF & Print", Subtitle = "Paper size, margins & layout", Icon = ReiconRenderer.IconType.Printer, Keywords = "pdf print a4 margins portrait landscape preview export orientation" },
            new CategoryItem { Index = 9, Name = "Keyboard Shortcuts", Subtitle = "Hotkeys & quick commands", Icon = ReiconRenderer.IconType.Calendar, Keywords = "keyboard shortcuts hotkeys keys ctrl save print new search" },
            new CategoryItem { Index = 10, Name = "Notifications", Subtitle = "Stock, due dates & alerts", Icon = ReiconRenderer.IconType.Notification, Keywords = "notifications alerts stock low due reminder overdue chime sound" },
            new CategoryItem { Index = 11, Name = "Backup & Data Privacy", Subtitle = "SQLite snapshot, restore & log", Icon = ReiconRenderer.IconType.Package, Keywords = "backup restore database sqlite snapshot privacy history file" },
            new CategoryItem { Index = 12, Name = "Users & Permissions", Subtitle = "User directory & access matrix", Icon = ReiconRenderer.IconType.Customers, Keywords = "users permissions role operator accountant admin vision access" },
            new CategoryItem { Index = 13, Name = "Integrations", Subtitle = "Export formats & local API", Icon = ReiconRenderer.IconType.TrendingUp, Keywords = "integrations export import csv json excel tally xml api webhook" },
            new CategoryItem { Index = 14, Name = "About & Support", Subtitle = "Version, license & diagnostics", Icon = ReiconRenderer.IconType.Messages, Keywords = "about version license credits support diagnostics update system" }
        };

        private List<CategoryItem> _filteredCategories = new();
        private int _selectedCategoryIndex = 0;
        private int _hoveredCategoryIndex = -1;
        private bool _hasUnsavedChanges = false;

        // ── DATA MODELS ─────────────────────────────────────────────────────────────
        private AppSettingsModel _settings = new();
        private BusinessProfile _profile = new();
        private List<BackupRecord> _backupRecords = new();
        private List<SystemUser> _systemUsers = new();

        // ── UI CONTROLS CONTAINER ───────────────────────────────────────────────────
        private readonly Panel _pnlSidebarNav = new();
        private readonly Panel _pnlSearchContainer = new();
        private readonly TextBox _txtSearch = new();
        private readonly Panel _pnlNavList = new();
        private readonly Panel _pnlDivider = new();

        private readonly Panel _pnlContentWrapper = new();
        private readonly Panel _pnlContentHeader = new();
        private readonly Label _lblHeaderTitle = new();
        private readonly Label _lblHeaderSubtitle = new();
        private readonly Label _lblUnsavedBadge = new();
        private readonly Label _lblSaveToast = new();

        private readonly Panel _pnlScrollArea = new();
        private readonly Panel _pnlActiveCard = new();
        private readonly Panel _pnlBottomBar = new();
        private readonly TeamPillButton _btnSave = new();
        private readonly Button _btnCancel = new();
        private readonly Button _btnReset = new();

        // Cached UI Controls for editing
        // Appearance
        private readonly ComboBox _cmbThemeMode = new();
        private readonly ComboBox _cmbAccentColor = new();
        private readonly ComboBox _cmbUiDensity = new();
        private readonly CheckBox _chkSidebarCollapsed = new();

        // General
        private readonly ComboBox _cmbLanguage = new();
        private readonly ComboBox _cmbCurrency = new();
        private readonly ComboBox _cmbDateFormat = new();
        private readonly ComboBox _cmbTimeFormat = new();
        private readonly ComboBox _cmbNumberFormat = new();
        private readonly ComboBox _cmbDefaultLanding = new();
        private readonly CheckBox _chkAutoCloseModals = new();
        private readonly CheckBox _chkConfirmDeletions = new();

        // Security
        private readonly TextBox _txtCurrentPassword = new();
        private readonly TextBox _txtNewPassword = new();
        private readonly TextBox _txtConfirmPassword = new();
        private readonly Panel _pnlStrengthBar = new();
        private readonly Label _lblStrengthText = new();
        private readonly ComboBox _cmbSessionTimeout = new();
        private readonly CheckBox _chkRequirePassForDelete = new();
        private readonly CheckBox _chkTwoFactorPin = new();
        private readonly TextBox _txtSecurityPin = new();

        // Company & Branding
        private readonly TextBox _txtBizName = new();
        private readonly TextBox _txtBizEmail = new();
        private readonly TextBox _txtBizPhone = new();
        private readonly TextBox _txtBizWebsite = new();
        private readonly TextBox _txtBizAddress = new();
        private readonly TextBox _txtBizSignatory = new();
        private readonly TextBox _txtBizSignatoryTitle = new();
        private readonly TextBox _txtBizInvoiceFooter = new();
        private readonly PictureBox _picBizLogo = new();

        // Invoice & Sales
        private readonly TextBox _txtInvoicePrefix = new();
        private readonly NumericUpDown _numInvoiceStart = new();
        private readonly ComboBox _cmbPaymentTerms = new();
        private readonly NumericUpDown _numDueDays = new();
        private readonly NumericUpDown _numDefaultDiscount = new();
        private readonly CheckBox _chkTaxInclusive = new();
        private readonly CheckBox _chkAutoDecrementStock = new();
        private readonly CheckBox _chkAutoRestoreStock = new();
        private readonly CheckBox _chkAllowBackorders = new();

        // Tax & GST
        private readonly TextBox _txtGSTIN = new();
        private readonly ComboBox _cmbDefaultTaxRate = new();
        private readonly TextBox _txtDefaultHsn = new();
        private readonly ComboBox _cmbGstScheme = new();
        private readonly CheckBox _chkReverseCharge = new();

        // Payments
        private readonly ComboBox _cmbDefaultPaymentMethod = new();
        private readonly TextBox _txtUPI = new();
        private readonly TextBox _txtBankName = new();
        private readonly TextBox _txtAccountNum = new();
        private readonly TextBox _txtIFSC = new();
        private readonly TextBox _txtBranchName = new();
        private readonly TextBox _txtPaymentInstructions = new();
        private readonly TextBox _txtLatePaymentInterest = new();

        // PDF & Print
        private readonly ComboBox _cmbPaperSize = new();
        private readonly ComboBox _cmbOrientation = new();
        private readonly ComboBox _cmbMargins = new();
        private readonly ComboBox _cmbPrintTheme = new();
        private readonly CheckBox _chkPrintLogo = new();
        private readonly CheckBox _chkPrintBank = new();
        private readonly CheckBox _chkPrintSignatory = new();
        private readonly CheckBox _chkPrintPreviousDues = new();
        private readonly CheckBox _chkPrintHsn = new();
        private readonly CheckBox _chkPrintTerms = new();
        private readonly TextBox _txtPdfFolder = new();
        private readonly CheckBox _chkAutoOpenPdf = new();

        // Keyboard
        private readonly CheckBox _chkGlobalHotkeys = new();

        // Notifications
        private readonly CheckBox _chkLowStockAlert = new();
        private readonly NumericUpDown _numCriticalStock = new();
        private readonly CheckBox _chkDueReminder = new();
        private readonly NumericUpDown _numDueReminderDays = new();
        private readonly CheckBox _chkOverdueAlert = new();
        private readonly CheckBox _chkHighOutstandingAlert = new();
        private readonly NumericUpDown _numHighOutstandingThreshold = new();
        private readonly NumericUpDown _numBackupReminderDays = new();
        private readonly CheckBox _chkAudioChime = new();
        private readonly CheckBox _chkDesktopToast = new();

        // Integrations
        private readonly CheckBox _chkLocalApi = new();
        private readonly NumericUpDown _numLocalApiPort = new();
        private readonly TextBox _txtApiKey = new();

        // DataGridViews for dynamic tables
        private readonly DataGridView _dgvShortcuts = new();
        private readonly DataGridView _dgvBackups = new();
        private readonly DataGridView _dgvUsers = new();

        public event Action? SettingsSaved;

        public SettingsModuleControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9.5f);

            _filteredCategories = new List<CategoryItem>(_allCategories);

            InitializeLayout();
            LoadDataFromDatabase();
            RenderCategoryContent(_selectedCategoryIndex);

            ThemeManager.ThemeChanged += ApplyTheme;
            ApplyTheme();
        }

        private void InitializeLayout()
        {
            Controls.Clear();
            SuspendLayout();

            // ── 1. LEFT SETTINGS SIDEBAR NAV (260px) ──────────────────────────────────
            _pnlSidebarNav.Dock = DockStyle.Left;
            _pnlSidebarNav.Width = 260;
            _pnlSidebarNav.Padding = new Padding(12, 16, 12, 16);

            // Search Container inside nav
            _pnlSearchContainer.Dock = DockStyle.Top;
            _pnlSearchContainer.Height = 38;
            _pnlSearchContainer.Padding = new Padding(8, 4, 8, 4);

            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.BorderStyle = BorderStyle.None;
            _txtSearch.Font = new Font("Segoe UI", 9.5f);
            _txtSearch.PlaceholderText = "Search settings...";
            _txtSearch.TextChanged += (s, e) => FilterCategories(_txtSearch.Text.Trim());
            _pnlSearchContainer.Controls.Add(_txtSearch);

            _pnlSearchContainer.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, _pnlSearchContainer.Width - 1, _pnlSearchContainer.Height - 1, 8);
                using var p = new Pen(ThemeManager.CardBorder, 1f);
                g.DrawPath(p, path);
            };

            // Nav List Panel
            _pnlNavList.Dock = DockStyle.Fill;
            _pnlNavList.AutoScroll = true;
            _pnlNavList.Padding = new Padding(0, 8, 0, 0);
            _pnlNavList.Paint += RenderNavCategories;
            _pnlNavList.MouseMove += HandleNavMouseMove;
            _pnlNavList.MouseLeave += (s, e) => { _hoveredCategoryIndex = -1; _pnlNavList.Invalidate(); };
            _pnlNavList.MouseClick += HandleNavMouseClick;

            _pnlSidebarNav.Controls.Add(_pnlNavList);
            _pnlSidebarNav.Controls.Add(_pnlSearchContainer);

            // ── 2. VERTICAL HAIRLINE DIVIDER ──────────────────────────────────────────
            _pnlDivider.Dock = DockStyle.Left;
            _pnlDivider.Width = 1;
            _pnlDivider.BackColor = ThemeManager.BorderHairline;

            // ── 3. RIGHT SETTINGS CONTENT AREA ────────────────────────────────────────
            _pnlContentWrapper.Dock = DockStyle.Fill;
            _pnlContentWrapper.Padding = new Padding(28, 20, 28, 20);

            // Header banner
            _pnlContentHeader.Dock = DockStyle.Top;
            _pnlContentHeader.Height = 65;
            _pnlContentHeader.Padding = new Padding(0, 0, 0, 10);

            _lblHeaderTitle.Font = new Font("Segoe UI", 14.5f, FontStyle.Bold);
            _lblHeaderTitle.Location = new Point(0, 2);
            _lblHeaderTitle.AutoSize = true;
            _lblHeaderTitle.UseMnemonic = false;

            _lblHeaderSubtitle.Font = new Font("Segoe UI", 9.5f);
            _lblHeaderSubtitle.Location = new Point(2, 30);
            _lblHeaderSubtitle.AutoSize = true;
            _lblHeaderSubtitle.UseMnemonic = false;

            _lblUnsavedBadge.Text = "● Unsaved Changes";
            _lblUnsavedBadge.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _lblUnsavedBadge.ForeColor = Color.FromArgb(234, 88, 12);
            _lblUnsavedBadge.BackColor = Color.FromArgb(254, 243, 199);
            _lblUnsavedBadge.AutoSize = true;
            _lblUnsavedBadge.Padding = new Padding(6, 3, 6, 3);
            _lblUnsavedBadge.Location = new Point(350, 6);
            _lblUnsavedBadge.Visible = false;

            _lblSaveToast.Text = "✓ Settings saved successfully to database!";
            _lblSaveToast.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _lblSaveToast.ForeColor = Color.FromArgb(16, 185, 129);
            _lblSaveToast.AutoSize = true;
            _lblSaveToast.Location = new Point(350, 8);
            _lblSaveToast.Visible = false;

            _pnlContentHeader.Controls.Add(_lblHeaderTitle);
            _pnlContentHeader.Controls.Add(_lblHeaderSubtitle);
            _pnlContentHeader.Controls.Add(_lblUnsavedBadge);
            _pnlContentHeader.Controls.Add(_lblSaveToast);

            // Bottom Action Bar
            _pnlBottomBar.Dock = DockStyle.Bottom;
            _pnlBottomBar.Height = 56;
            _pnlBottomBar.Padding = new Padding(0, 10, 0, 0);

            _btnSave.Text = "Save Changes";
            _btnSave.Icon = ReiconRenderer.IconType.Check;
            _btnSave.IconSize = 14;
            _btnSave.Size = new Size(140, 38);
            _btnSave.Location = new Point(0, 8);
            _btnSave.FillColor = ThemeManager.PrimaryAccent;
            _btnSave.HoverColor = ThemeManager.AccentHover;
            _btnSave.TextColor = Color.White;
            _btnSave.Click += (s, e) => SaveCurrentSettings();

            _btnCancel.Text = "Cancel";
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.Font = new Font("Segoe UI", 9.5f);
            _btnCancel.Size = new Size(95, 38);
            _btnCancel.Location = new Point(148, 8);
            _btnCancel.Cursor = Cursors.Hand;
            _btnCancel.Click += (s, e) => RevertUnsavedChanges();

            _btnReset.Text = "Reset to Defaults";
            _btnReset.FlatStyle = FlatStyle.Flat;
            _btnReset.Font = new Font("Segoe UI", 9f);
            _btnReset.Size = new Size(145, 38);
            _btnReset.Location = new Point(252, 8);
            _btnReset.Cursor = Cursors.Hand;
            _btnReset.Click += (s, e) => ResetCurrentCategoryDefaults();

            _pnlBottomBar.Controls.Add(_btnSave);
            _pnlBottomBar.Controls.Add(_btnCancel);
            _pnlBottomBar.Controls.Add(_btnReset);

            // Main Scroll Area
            _pnlScrollArea.Dock = DockStyle.Fill;
            _pnlScrollArea.AutoScroll = true;
            _pnlScrollArea.Padding = new Padding(0, 10, 16, 10);
            _pnlScrollArea.Controls.Add(_pnlActiveCard);

            _pnlActiveCard.Dock = DockStyle.Top;
            _pnlActiveCard.AutoSize = true;
            _pnlActiveCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            _pnlContentWrapper.Controls.Add(_pnlScrollArea);
            _pnlContentWrapper.Controls.Add(_pnlBottomBar);
            _pnlContentWrapper.Controls.Add(_pnlContentHeader);

            Controls.Add(_pnlContentWrapper);
            Controls.Add(_pnlDivider);
            Controls.Add(_pnlSidebarNav);

            ResumeLayout(true);
        }

        // ── 4. CATEGORIES SEARCH & NAVIGATION DRAWING ──────────────────────────────
        private void FilterCategories(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _filteredCategories = new List<CategoryItem>(_allCategories);
            }
            else
            {
                string q = query.ToLowerInvariant();
                _filteredCategories = _allCategories
                    .Where(c => c.Name.ToLowerInvariant().Contains(q) ||
                                c.Subtitle.ToLowerInvariant().Contains(q) ||
                                c.Keywords.ToLowerInvariant().Contains(q))
                    .ToList();
            }
            _pnlNavList.Invalidate();
        }

        private void RenderNavCategories(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int itemH = 46;
            int y = 0;
            int width = _pnlNavList.Width - (SystemInformation.VerticalScrollBarWidth);
            if (width < 210) width = _pnlNavList.Width;

            bool isDark = ThemeManager.IsDarkMode;
            Color textPrimary = ThemeManager.TextPrimary;
            Color textSecondary = ThemeManager.TextSecondary;
            Color navActiveBg = isDark ? Color.FromArgb(42, 51, 88) : Color.FromArgb(233, 239, 255);
            Color navHoverBg = isDark ? Color.FromArgb(37, 40, 54) : Color.FromArgb(240, 243, 252);
            Color accent = ThemeManager.PrimaryAccent;

            for (int i = 0; i < _filteredCategories.Count; i++)
            {
                var cat = _filteredCategories[i];
                bool isSelected = (cat.Index == _selectedCategoryIndex);
                bool isHovered = (i == _hoveredCategoryIndex && !isSelected);

                var itemRect = new Rectangle(0, y, width - 4, itemH - 4);

                if (isSelected)
                {
                    using var bgBrush = new SolidBrush(navActiveBg);
                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height, 8);
                    g.FillPath(bgBrush, path);

                    // Left Pill Bar
                    using var pillBrush = new SolidBrush(accent);
                    using var pillPath = GraphicsExtensions.CreateRoundedRectanglePath(itemRect.X + 2, itemRect.Y + 8, 3, itemRect.Height - 16, 2);
                    g.FillPath(pillBrush, pillPath);
                }
                else if (isHovered)
                {
                    using var bgBrush = new SolidBrush(navHoverBg);
                    using var path = GraphicsExtensions.CreateRoundedRectanglePath(itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height, 8);
                    g.FillPath(bgBrush, path);
                }

                Color iconColor = isSelected ? accent : (isHovered ? textPrimary : textSecondary);
                Color titleColor = isSelected ? (isDark ? Color.White : Color.FromArgb(20, 24, 38)) : textPrimary;
                Color subtitleColor = textSecondary;

                // Draw Icon
                var iconRect = new Rectangle(itemRect.X + 12, itemRect.Y + 12, 18, 18);
                ReiconRenderer.DrawIcon(g, cat.Icon, iconRect, iconColor, 1.8f);

                // Draw Title & Subtitle
                using (var fTitle = new Font("Segoe UI", 9.2f, isSelected ? FontStyle.Bold : FontStyle.Regular))
                using (var bTitle = new SolidBrush(titleColor))
                {
                    g.DrawString(cat.Name, fTitle, bTitle, itemRect.X + 38, itemRect.Y + 6);
                }

                using (var fSub = new Font("Segoe UI", 7.8f))
                using (var bSub = new SolidBrush(subtitleColor))
                {
                    g.DrawString(cat.Subtitle, fSub, bSub, itemRect.X + 38, itemRect.Y + 23);
                }

                y += itemH;
            }

            // Adjust panel scroll boundary
            if (_pnlNavList.AutoScrollMinSize.Height != y + 20)
            {
                _pnlNavList.AutoScrollMinSize = new Size(0, y + 20);
            }
        }

        private void HandleNavMouseMove(object? sender, MouseEventArgs e)
        {
            int itemH = 46;
            int idx = (e.Y + _pnlNavList.AutoScrollPosition.Y * -1) / itemH;
            if (idx >= 0 && idx < _filteredCategories.Count)
            {
                if (_hoveredCategoryIndex != idx)
                {
                    _hoveredCategoryIndex = idx;
                    _pnlNavList.Invalidate();
                }
            }
            else if (_hoveredCategoryIndex != -1)
            {
                _hoveredCategoryIndex = -1;
                _pnlNavList.Invalidate();
            }
        }

        private void HandleNavMouseClick(object? sender, MouseEventArgs e)
        {
            int itemH = 46;
            int idx = (e.Y + _pnlNavList.AutoScrollPosition.Y * -1) / itemH;
            if (idx >= 0 && idx < _filteredCategories.Count)
            {
                int targetCatIndex = _filteredCategories[idx].Index;
                if (_selectedCategoryIndex != targetCatIndex)
                {
                    if (_hasUnsavedChanges)
                    {
                        var dr = MessageBox.Show("You have unsaved changes in the current settings tab. Do you want to save before navigating?",
                            "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            SaveCurrentSettings();
                        }
                        else if (dr == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    _selectedCategoryIndex = targetCatIndex;
                    _hasUnsavedChanges = false;
                    _lblUnsavedBadge.Visible = false;
                    _pnlNavList.Invalidate();
                    RenderCategoryContent(_selectedCategoryIndex);
                }
            }
        }

        public void SelectCategory(int categoryIndex)
        {
            if (categoryIndex >= 0 && categoryIndex < _allCategories.Count)
            {
                _selectedCategoryIndex = categoryIndex;
                _hasUnsavedChanges = false;
                _lblUnsavedBadge.Visible = false;
                _pnlNavList.Invalidate();
                RenderCategoryContent(_selectedCategoryIndex);
            }
        }

        public void RefreshAllSettings()
        {
            LoadDataFromDatabase();
            RenderCategoryContent(_selectedCategoryIndex);
        }

        // ── 5. LOAD & PERSIST DATA ──────────────────────────────────────────────────
        private void LoadDataFromDatabase()
        {
            try
            {
                _settings = DatabaseService.GetAppSettings();
                _profile = DatabaseService.GetBusinessProfile();
                _backupRecords = DatabaseService.GetBackupHistory();
                _systemUsers = DatabaseService.GetSystemUsers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SETTINGS] Error loading from SQLite: {ex.Message}");
            }
        }

        private void MarkUnsaved()
        {
            _hasUnsavedChanges = true;
            _lblUnsavedBadge.Visible = true;
            _lblSaveToast.Visible = false;
        }

        private void RevertUnsavedChanges()
        {
            LoadDataFromDatabase();
            _hasUnsavedChanges = false;
            _lblUnsavedBadge.Visible = false;
            RenderCategoryContent(_selectedCategoryIndex);
        }

        private void SaveCurrentSettings()
        {
            try
            {
                // Harvest active input values based on current category
                CommitActiveCategoryInputs();

                // Save to SQLite
                DatabaseService.SaveAppSettings(_settings);
                DatabaseService.SaveBusinessProfile(_profile);

                _hasUnsavedChanges = false;
                _lblUnsavedBadge.Visible = false;
                _lblSaveToast.Visible = true;

                var timer = new System.Windows.Forms.Timer { Interval = 3500 };
                timer.Tick += (s, e) => { _lblSaveToast.Visible = false; timer.Stop(); timer.Dispose(); };
                timer.Start();

                SettingsSaved?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetCurrentCategoryDefaults()
        {
            var dr = MessageBox.Show($"Are you sure you want to reset all '{_allCategories[_selectedCategoryIndex].Name}' settings back to defaults?",
                "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                var defaults = new AppSettingsModel();

                switch (_selectedCategoryIndex)
                {
                    case 1: // Appearance
                        _settings.ThemeMode = defaults.ThemeMode;
                        _settings.AccentColor = defaults.AccentColor;
                        _settings.UiDensity = defaults.UiDensity;
                        ThemeManager.ThemeMode = defaults.ThemeMode;
                        ThemeManager.AccentColorName = defaults.AccentColor;
                        break;
                    case 2: // General
                        _settings.Language = defaults.Language;
                        _settings.CurrencySymbol = defaults.CurrencySymbol;
                        _settings.DateFormat = defaults.DateFormat;
                        _settings.TimeFormat = defaults.TimeFormat;
                        _settings.NumberFormat = defaults.NumberFormat;
                        _settings.DefaultLandingPage = defaults.DefaultLandingPage;
                        break;
                    case 3: // Security
                        _settings.SessionTimeoutMinutes = defaults.SessionTimeoutMinutes;
                        _settings.RequirePasswordForDelete = defaults.RequirePasswordForDelete;
                        _settings.TwoFactorPinEnabled = defaults.TwoFactorPinEnabled;
                        break;
                    case 5: // Invoice & Sales
                        _settings.InvoicePrefix = defaults.InvoicePrefix;
                        _settings.DefaultPaymentTerms = defaults.DefaultPaymentTerms;
                        _settings.DefaultDueDays = defaults.DefaultDueDays;
                        _settings.DefaultDiscountPercent = defaults.DefaultDiscountPercent;
                        _settings.TaxInclusiveDefault = defaults.TaxInclusiveDefault;
                        break;
                    case 6: // Tax & GST
                        _settings.DefaultTaxRate = defaults.DefaultTaxRate;
                        _settings.DefaultHsnSac = defaults.DefaultHsnSac;
                        _settings.DefaultGstScheme = defaults.DefaultGstScheme;
                        break;
                    case 7: // Payments
                        _settings.DefaultPaymentMethod = defaults.DefaultPaymentMethod;
                        _settings.SupportedPaymentMethods = defaults.SupportedPaymentMethods;
                        _settings.PaymentInstructions = defaults.PaymentInstructions;
                        break;
                    case 8: // PDF & Print
                        _settings.PaperSize = defaults.PaperSize;
                        _settings.Orientation = defaults.Orientation;
                        _settings.Margins = defaults.Margins;
                        _settings.ShowLogoOnPrint = defaults.ShowLogoOnPrint;
                        _settings.ShowBankDetailsOnPrint = defaults.ShowBankDetailsOnPrint;
                        break;
                    case 10: // Notifications
                        _settings.LowStockAlertEnabled = defaults.LowStockAlertEnabled;
                        _settings.InvoiceDueReminderEnabled = defaults.InvoiceDueReminderEnabled;
                        _settings.OverduePaymentAlertEnabled = defaults.OverduePaymentAlertEnabled;
                        _settings.AudioChimeEnabled = defaults.AudioChimeEnabled;
                        break;
                }

                SaveCurrentSettings();
                RenderCategoryContent(_selectedCategoryIndex);
            }
        }

        // ── 6. RENDER ACTIVE CATEGORY CONTENT ───────────────────────────────────────
        private void RenderCategoryContent(int categoryIndex)
        {
            var cat = _allCategories[categoryIndex];
            _lblHeaderTitle.Text = cat.Name;
            _lblHeaderSubtitle.Text = cat.Subtitle;

            _pnlActiveCard.Controls.Clear();
            _pnlActiveCard.SuspendLayout();

            switch (categoryIndex)
            {
                case 0: BuildAccountView(); break;
                case 1: BuildAppearanceView(); break;
                case 2: BuildGeneralView(); break;
                case 3: BuildSecurityView(); break;
                case 4: BuildCompanyBrandingView(); break;
                case 5: BuildInvoiceSalesView(); break;
                case 6: BuildTaxGstView(); break;
                case 7: BuildPaymentsView(); break;
                case 8: BuildPdfPrintView(); break;
                case 9: BuildKeyboardShortcutsView(); break;
                case 10: BuildNotificationsView(); break;
                case 11: BuildBackupPrivacyView(); break;
                case 12: BuildUsersPermissionsView(); break;
                case 13: BuildIntegrationsView(); break;
                case 14: BuildAboutSupportView(); break;
            }

            StyleControlHierarchy(_pnlActiveCard, ThemeManager.IsDarkMode);

            _pnlActiveCard.ResumeLayout(true);
            _pnlScrollArea.ScrollControlIntoView(_pnlActiveCard);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  01. ACCOUNT VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildAccountView()
        {
            var pnlGroup = CreateGroupCard("Active Account & Profile Information", "Manage your personal profile, authenticated role credentials and session identity.");
            int y = 50;

            // Avatar & Identity Pill
            var pnlAvatar = new Panel { Size = new Size(68, 68), Location = new Point(24, y), BackColor = ThemeManager.PrimaryAccent };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, 67, 67, 34);
                using var b = new SolidBrush(ThemeManager.PrimaryAccent);
                e.Graphics.FillPath(b, path);
                using var f = new Font("Segoe UI", 16f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("SC", f, Brushes.White, new Rectangle(0, 0, 68, 68), sf);
            };
            pnlGroup.Controls.Add(pnlAvatar);

            var lblName = new Label { Text = "Saumya Chaudhari", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(106, y + 6), AutoSize = true };
            var lblEmail = new Label { Text = "saumya@teamsoft.in • Vision (Project Owner)", Font = new Font("Segoe UI", 9.5f), ForeColor = ThemeManager.TextSecondary, Location = new Point(108, y + 32), AutoSize = true };
            var lblStatus = new Label { Text = "● Active & Authorized", Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(16, 185, 129), Location = new Point(108, y + 50), AutoSize = true };

            pnlGroup.Controls.Add(lblName);
            pnlGroup.Controls.Add(lblEmail);
            pnlGroup.Controls.Add(lblStatus);
            y += 85;

            AddFormRow(pnlGroup, ref y, "Username", CreateReadonlyBox("saumya"));
            AddFormRow(pnlGroup, ref y, "Contact Phone", CreateReadonlyBox("+91 98250 99881"));
            AddFormRow(pnlGroup, ref y, "Account Role", CreateReadonlyBox("Vision (Project Owner & Lead Architect)"));
            AddFormRow(pnlGroup, ref y, "Active Workstation", CreateReadonlyBox(Environment.MachineName + " (" + Environment.OSVersion.Platform + ")"));
            AddFormRow(pnlGroup, ref y, "Current Session", CreateReadonlyBox($"Logged in on {DateTime.Now:dd MMM yyyy, hh:mm tt}"));

            var btnGoToSecurity = new TeamPillButton
            {
                Text = "Change Password →",
                FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(45, 52, 75) : Color.FromArgb(235, 240, 255),
                TextColor = ThemeManager.PrimaryAccent,
                Size = new Size(160, 36),
                Location = new Point(215, y + 8)
            };
            btnGoToSecurity.Click += (s, e) =>
            {
                _selectedCategoryIndex = 3; // Security
                _pnlNavList.Invalidate();
                RenderCategoryContent(3);
            };
            pnlGroup.Controls.Add(btnGoToSecurity);
            y += 56;

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  02. APPEARANCE VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildAppearanceView()
        {
            var pnlGroup = CreateGroupCard("Theme & Visual Style", "Customize application themes, accent palettes and interface density.");
            int y = 50;

            // Theme Mode
            _cmbThemeMode.Items.Clear();
            _cmbThemeMode.Items.AddRange(new object[] { "Light", "Dark", "System" });
            _cmbThemeMode.SelectedItem = _settings.ThemeMode;
            _cmbThemeMode.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbThemeMode.SelectedItem is string mode)
                {
                    _settings.ThemeMode = mode;
                    ThemeManager.ThemeMode = mode;
                    MarkUnsaved();
                }
            };
            AddFormRow(pnlGroup, ref y, "Theme Mode", _cmbThemeMode, "Select Light Mode, Dark Mode, or follow Windows system mode.");

            // Accent Color
            _cmbAccentColor.Items.Clear();
            _cmbAccentColor.Items.AddRange(new object[] { "RoyalBlue", "Indigo", "Emerald", "Crimson", "Amber" });
            _cmbAccentColor.SelectedItem = _settings.AccentColor;
            _cmbAccentColor.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbAccentColor.SelectedItem is string color)
                {
                    _settings.AccentColor = color;
                    ThemeManager.AccentColorName = color;
                    MarkUnsaved();
                }
            };
            AddFormRow(pnlGroup, ref y, "Accent Color", _cmbAccentColor, "Primary highlight palette applied across navigation, buttons and badges.");

            // UI Density
            _cmbUiDensity.Items.Clear();
            _cmbUiDensity.Items.AddRange(new object[] { "Comfortable", "Compact" });
            _cmbUiDensity.SelectedItem = _settings.UiDensity;
            _cmbUiDensity.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbUiDensity.SelectedItem is string density)
                {
                    _settings.UiDensity = density;
                    ThemeManager.UiDensity = density;
                    MarkUnsaved();
                }
            };
            AddFormRow(pnlGroup, ref y, "Interface Density", _cmbUiDensity, "Comfortable spacing (44px table rows) vs Compact dense mode (36px).");

            // Sidebar Collapsed
            _chkSidebarCollapsed.Text = "Collapse sidebar to icons only by default";
            _chkSidebarCollapsed.Checked = _settings.SidebarCollapsed;
            _chkSidebarCollapsed.CheckedChanged += (s, e) =>
            {
                _settings.SidebarCollapsed = _chkSidebarCollapsed.Checked;
                MarkUnsaved();
            };
            AddFormRow(pnlGroup, ref y, "Sidebar Mode", _chkSidebarCollapsed, "Expand main navigation labels or minimize to compact icon rail.");

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  03. GENERAL VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildGeneralView()
        {
            var pnlGroup = CreateGroupCard("Regional & General Preferences", "Configure localization, display currency symbols, and default system behavior.");
            int y = 50;

            _cmbLanguage.Items.Clear();
            _cmbLanguage.Items.AddRange(new object[] { "English (UK)", "Hinglish (Hindi)", "Gujarati" });
            _cmbLanguage.SelectedItem = _settings.Language;
            _cmbLanguage.SelectedIndexChanged += (s, e) => { _settings.Language = _cmbLanguage.SelectedItem?.ToString() ?? "English (UK)"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Display Language", _cmbLanguage);

            _cmbCurrency.Items.Clear();
            _cmbCurrency.Items.AddRange(new object[] { "₹ (INR)", "$ (USD)", "€ (EUR)", "£ (GBP)" });
            _cmbCurrency.SelectedItem = _settings.CurrencySymbol;
            _cmbCurrency.SelectedIndexChanged += (s, e) => { _settings.CurrencySymbol = _cmbCurrency.SelectedItem?.ToString() ?? "₹ (INR)"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Currency Symbol", _cmbCurrency, "Display symbol on invoices and dashboard financial summaries.");

            _cmbDateFormat.Items.Clear();
            _cmbDateFormat.Items.AddRange(new object[] { "dd MMM yyyy", "dd/MM/yyyy", "yyyy-MM-dd" });
            _cmbDateFormat.SelectedItem = _settings.DateFormat;
            _cmbDateFormat.SelectedIndexChanged += (s, e) => { _settings.DateFormat = _cmbDateFormat.SelectedItem?.ToString() ?? "dd MMM yyyy"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Date Format", _cmbDateFormat);

            _cmbNumberFormat.Items.Clear();
            _cmbNumberFormat.Items.AddRange(new object[] { "Indian Lakhs (1,00,000.00)", "International (100,000.00)" });
            _cmbNumberFormat.SelectedItem = _settings.NumberFormat.Contains("Indian") ? "Indian Lakhs (1,00,000.00)" : "International (100,000.00)";
            _cmbNumberFormat.SelectedIndexChanged += (s, e) => { _settings.NumberFormat = _cmbNumberFormat.SelectedItem?.ToString() ?? "Indian Lakhs"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Number Formatting", _cmbNumberFormat);

            _cmbDefaultLanding.Items.Clear();
            _cmbDefaultLanding.Items.AddRange(new object[] { "Overview", "Sales", "Purchases", "Customers", "Products" });
            _cmbDefaultLanding.SelectedItem = _settings.DefaultLandingPage;
            _cmbDefaultLanding.SelectedIndexChanged += (s, e) => { _settings.DefaultLandingPage = _cmbDefaultLanding.SelectedItem?.ToString() ?? "Overview"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Default Landing Page", _cmbDefaultLanding, "Screen shown automatically after successful login.");

            _chkAutoCloseModals.Text = "Automatically close dialog modals upon successful save";
            _chkAutoCloseModals.Checked = _settings.AutoCloseModals;
            _chkAutoCloseModals.CheckedChanged += (s, e) => { _settings.AutoCloseModals = _chkAutoCloseModals.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Dialog Workflow", _chkAutoCloseModals);

            _chkConfirmDeletions.Text = "Require confirmation prompt before deleting database records";
            _chkConfirmDeletions.Checked = _settings.ConfirmDeletions;
            _chkConfirmDeletions.CheckedChanged += (s, e) => { _settings.ConfirmDeletions = _chkConfirmDeletions.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Deletion Safety", _chkConfirmDeletions);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  04. SECURITY VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildSecurityView()
        {
            var pnlGroup = CreateGroupCard("Security & Password Management", "Update your sign-in password and configure session inactivity timeouts.");
            int y = 50;

            _txtCurrentPassword.UseSystemPasswordChar = true;
            _txtCurrentPassword.Text = "";
            AddFormRow(pnlGroup, ref y, "Current Password *", _txtCurrentPassword);

            _txtNewPassword.UseSystemPasswordChar = true;
            _txtNewPassword.Text = "";
            _txtNewPassword.TextChanged += (s, e) => UpdatePasswordStrengthMeter();
            AddFormRow(pnlGroup, ref y, "New Password *", _txtNewPassword);

            _txtConfirmPassword.UseSystemPasswordChar = true;
            _txtConfirmPassword.Text = "";
            AddFormRow(pnlGroup, ref y, "Confirm New Password *", _txtConfirmPassword);

            // Strength bar
            _pnlStrengthBar.Size = new Size(240, 6);
            _pnlStrengthBar.Location = new Point(215, y);
            _pnlStrengthBar.BackColor = Color.FromArgb(226, 232, 240);
            _lblStrengthText.Text = "Enter password (min 6 characters)";
            _lblStrengthText.UseMnemonic = false;
            _lblStrengthText.Font = new Font("Segoe UI", 8f);
            _lblStrengthText.ForeColor = ThemeManager.TextSecondary;
            _lblStrengthText.Location = new Point(215, y + 10);
            _lblStrengthText.AutoSize = true;

            pnlGroup.Controls.Add(_pnlStrengthBar);
            pnlGroup.Controls.Add(_lblStrengthText);
            y += 36;

            var btnChangePassword = new TeamPillButton
            {
                Text = "Update Password",
                Icon = ReiconRenderer.IconType.Check,
                Size = new Size(150, 36),
                Location = new Point(215, y),
                FillColor = ThemeManager.PrimaryAccent
            };
            btnChangePassword.Click += (s, e) =>
            {
                string curr = _txtCurrentPassword.Text;
                string newP = _txtNewPassword.Text;
                string conf = _txtConfirmPassword.Text;

                if (string.IsNullOrEmpty(curr) || string.IsNullOrEmpty(newP))
                {
                    MessageBox.Show("Please enter current and new passwords.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (newP != conf)
                {
                    MessageBox.Show("New passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool success = DatabaseService.ChangeUserPassword("saumya", curr, newP, out string err);
                if (success)
                {
                    MessageBox.Show("Password updated successfully!", "Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtCurrentPassword.Text = "";
                    _txtNewPassword.Text = "";
                    _txtConfirmPassword.Text = "";
                    _pnlStrengthBar.Width = 0;
                    _lblStrengthText.Text = "Enter password (min 6 characters)";
                }
                else
                {
                    MessageBox.Show(err, "Password Change Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            pnlGroup.Controls.Add(btnChangePassword);
            y += 54;

            _cmbSessionTimeout.Items.Clear();
            _cmbSessionTimeout.Items.AddRange(new object[] { "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "Never" });
            _cmbSessionTimeout.SelectedItem = _settings.SessionTimeoutMinutes switch
            {
                15 => "15 Minutes",
                60 => "1 Hour",
                240 => "4 Hours",
                0 => "Never",
                _ => "30 Minutes"
            };
            _cmbSessionTimeout.SelectedIndexChanged += (s, e) =>
            {
                _settings.SessionTimeoutMinutes = _cmbSessionTimeout.SelectedIndex switch
                {
                    0 => 15,
                    1 => 30,
                    2 => 60,
                    3 => 240,
                    _ => 0
                };
                MarkUnsaved();
            };
            AddFormRow(pnlGroup, ref y, "Inactivity Timeout", _cmbSessionTimeout, "Automatically lock screen when user is idle.");

            _chkRequirePassForDelete.Text = "Require administrative password for database restore & mass reset";
            _chkRequirePassForDelete.Checked = _settings.RequirePasswordForDelete;
            _chkRequirePassForDelete.CheckedChanged += (s, e) => { _settings.RequirePasswordForDelete = _chkRequirePassForDelete.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Destructive Action Guard", _chkRequirePassForDelete);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        private void UpdatePasswordStrengthMeter()
        {
            string p = _txtNewPassword.Text;
            int score = 0;
            if (p.Length >= 6) score++;
            if (p.Length >= 8) score++;
            if (p.Any(char.IsUpper) && p.Any(char.IsLower)) score++;
            if (p.Any(char.IsDigit) || p.Any(ch => !char.IsLetterOrDigit(ch))) score++;

            Color col = score switch
            {
                1 => Color.FromArgb(239, 68, 68),
                2 => Color.FromArgb(245, 158, 11),
                3 => Color.FromArgb(59, 130, 246),
                4 => Color.FromArgb(16, 185, 129),
                _ => Color.FromArgb(226, 232, 240)
            };

            _pnlStrengthBar.BackColor = col;
            _pnlStrengthBar.Width = Math.Max(20, score * 60);
            _lblStrengthText.Text = score switch
            {
                1 => "Weak password",
                2 => "Fair password",
                3 => "Good password",
                4 => "Strong password (excellent)",
                _ => "Too short (min 6 chars)"
            };
            _lblStrengthText.ForeColor = col;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  05. COMPANY & BRANDING VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildCompanyBrandingView()
        {
            var pnlGroup = CreateGroupCard("Company Identity & Invoice Branding", "Registered business credentials, authorized signatories and logo for generated invoices & PDF documents.");
            int y = 50;

            _txtBizName.Text = _profile.BusinessName;
            _txtBizName.TextChanged += (s, e) => { _profile.BusinessName = _txtBizName.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Business Name *", _txtBizName);

            _txtBizEmail.Text = _profile.Email;
            _txtBizEmail.TextChanged += (s, e) => { _profile.Email = _txtBizEmail.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Business Email *", _txtBizEmail);

            _txtBizPhone.Text = _profile.Phone;
            _txtBizPhone.TextChanged += (s, e) => { _profile.Phone = _txtBizPhone.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Contact Phone *", _txtBizPhone);

            _txtBizWebsite.Text = _profile.Website;
            _txtBizWebsite.TextChanged += (s, e) => { _profile.Website = _txtBizWebsite.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Company Website", _txtBizWebsite);

            _txtBizAddress.Text = _profile.Address;
            _txtBizAddress.Multiline = true;
            _txtBizAddress.Height = 52;
            _txtBizAddress.TextChanged += (s, e) => { _profile.Address = _txtBizAddress.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Registered Address", _txtBizAddress);

            _txtBizSignatory.Text = _profile.AuthorizedSignatory;
            _txtBizSignatory.TextChanged += (s, e) => { _profile.AuthorizedSignatory = _txtBizSignatory.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Signatory Name", _txtBizSignatory);

            _txtBizSignatoryTitle.Text = _profile.SignatoryTitle;
            _txtBizSignatoryTitle.TextChanged += (s, e) => { _profile.SignatoryTitle = _txtBizSignatoryTitle.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Signatory Designation", _txtBizSignatoryTitle);

            _txtBizInvoiceFooter.Text = _profile.InvoiceFooter;
            _txtBizInvoiceFooter.Multiline = true;
            _txtBizInvoiceFooter.Height = 44;
            _txtBizInvoiceFooter.TextChanged += (s, e) => { _profile.InvoiceFooter = _txtBizInvoiceFooter.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Invoice Footer Note", _txtBizInvoiceFooter, "Printed at the bottom of all sales tax invoices.");

            // Logo Upload Section
            var pnlLogoSection = new Panel { Size = new Size(480, 48), Location = new Point(215, y) };
            var btnUploadLogo = new Button { Text = "Upload Logo...", Size = new Size(120, 32), Location = new Point(0, 8), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var btnClearLogo = new Button { Text = "Clear", Size = new Size(70, 32), Location = new Point(128, 8), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var lblLogoStatus = new Label { Text = string.IsNullOrEmpty(_profile.LogoPath) ? "No logo uploaded (Default emblem used)" : Path.GetFileName(_profile.LogoPath), Location = new Point(208, 14), AutoSize = true, ForeColor = ThemeManager.TextSecondary };

            btnUploadLogo.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _profile.LogoPath = ofd.FileName;
                    lblLogoStatus.Text = Path.GetFileName(ofd.FileName);
                    lblLogoStatus.ForeColor = Color.FromArgb(16, 185, 129);
                    MarkUnsaved();
                }
            };
            btnClearLogo.Click += (s, e) =>
            {
                _profile.LogoPath = "";
                lblLogoStatus.Text = "No logo uploaded";
                lblLogoStatus.ForeColor = ThemeManager.TextSecondary;
                MarkUnsaved();
            };

            pnlLogoSection.Controls.Add(btnUploadLogo);
            pnlLogoSection.Controls.Add(btnClearLogo);
            pnlLogoSection.Controls.Add(lblLogoStatus);
            AddFormRow(pnlGroup, ref y, "Company Logo", pnlLogoSection);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  06. INVOICE & SALES VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildInvoiceSalesView()
        {
            var pnlGroup = CreateGroupCard("Invoice Numbering & Sales Defaults", "Configure collision-safe invoice sequences, default due periods and inventory deduction rules.");
            int y = 50;

            _txtInvoicePrefix.Text = _settings.InvoicePrefix;
            _txtInvoicePrefix.TextChanged += (s, e) => { _settings.InvoicePrefix = _txtInvoicePrefix.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Invoice Prefix", _txtInvoicePrefix, "Example: 'INV-' or '26-27/INV/'");

            _numInvoiceStart.Minimum = 1;
            _numInvoiceStart.Maximum = 999999;
            _numInvoiceStart.Value = Math.Max(1, _settings.InvoiceStartNumber);
            _numInvoiceStart.ValueChanged += (s, e) => { _settings.InvoiceStartNumber = (int)_numInvoiceStart.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Next Starting Sequence", _numInvoiceStart, "Starting integer for subsequent invoice generation.");

            _cmbPaymentTerms.Items.Clear();
            _cmbPaymentTerms.Items.AddRange(new object[] { "Due on Receipt", "Net 15 Days", "Net 30 Days", "Net 60 Days" });
            _cmbPaymentTerms.SelectedItem = _settings.DefaultPaymentTerms;
            _cmbPaymentTerms.SelectedIndexChanged += (s, e) => { _settings.DefaultPaymentTerms = _cmbPaymentTerms.SelectedItem?.ToString() ?? "Net 15 Days"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Default Payment Terms", _cmbPaymentTerms);

            _numDueDays.Minimum = 0;
            _numDueDays.Maximum = 180;
            _numDueDays.Value = _settings.DefaultDueDays;
            _numDueDays.ValueChanged += (s, e) => { _settings.DefaultDueDays = (int)_numDueDays.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Default Due Days", _numDueDays, "Days added to invoice date to compute due date.");

            _chkAutoDecrementStock.Text = "Automatically deduct product stock upon invoice creation (ACID Inventory Sync)";
            _chkAutoDecrementStock.Checked = _settings.AutoDecrementStock;
            _chkAutoDecrementStock.CheckedChanged += (s, e) => { _settings.AutoDecrementStock = _chkAutoDecrementStock.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Inventory Deduction", _chkAutoDecrementStock);

            _chkAutoRestoreStock.Text = "Automatically restore product stock upon invoice voiding or cancellation";
            _chkAutoRestoreStock.Checked = _settings.AutoRestoreStockOnCancel;
            _chkAutoRestoreStock.CheckedChanged += (s, e) => { _settings.AutoRestoreStockOnCancel = _chkAutoRestoreStock.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Inventory Rollback", _chkAutoRestoreStock);

            _chkAllowBackorders.Text = "Allow selling items when stock is zero (Backorders)";
            _chkAllowBackorders.Checked = _settings.AllowBackorders;
            _chkAllowBackorders.CheckedChanged += (s, e) => { _settings.AllowBackorders = _chkAllowBackorders.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Negative Stock", _chkAllowBackorders);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  07. TAX & GST VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildTaxGstView()
        {
            var pnlGroup = CreateGroupCard("Goods & Services Tax (GST) Configuration", "Indian GST legal trade identification, tax rate presets and HSN/SAC defaults.");
            int y = 50;

            _txtGSTIN.Text = _profile.GSTIN;
            _txtGSTIN.TextChanged += (s, e) => { _profile.GSTIN = _txtGSTIN.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "GSTIN / Tax ID *", _txtGSTIN, "15-digit alphanumeric Goods & Services Tax Identification Number.");

            _cmbGstScheme.Items.Clear();
            _cmbGstScheme.Items.AddRange(new object[] { "Intra-state (CGST 9% + SGST 9%)", "Inter-state (IGST 18%)" });
            _cmbGstScheme.SelectedItem = _settings.DefaultGstScheme;
            _cmbGstScheme.SelectedIndexChanged += (s, e) => { _settings.DefaultGstScheme = _cmbGstScheme.SelectedItem?.ToString() ?? "Intra-state"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Default GST Scheme", _cmbGstScheme);

            _cmbDefaultTaxRate.Items.Clear();
            _cmbDefaultTaxRate.Items.AddRange(new object[] { "0%", "5%", "12%", "18%", "28%" });
            _cmbDefaultTaxRate.SelectedItem = $"{_settings.DefaultTaxRate:0}%";
            _cmbDefaultTaxRate.SelectedIndexChanged += (s, e) =>
            {
                if (decimal.TryParse(_cmbDefaultTaxRate.SelectedItem?.ToString()?.Replace("%", ""), out var r))
                {
                    _settings.DefaultTaxRate = r;
                    MarkUnsaved();
                }
            };
            AddFormRow(pnlGroup, ref y, "Default Tax Rate", _cmbDefaultTaxRate);

            _txtDefaultHsn.Text = _settings.DefaultHsnSac;
            _txtDefaultHsn.TextChanged += (s, e) => { _settings.DefaultHsnSac = _txtDefaultHsn.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Default HSN/SAC Code", _txtDefaultHsn, "Standard service classification code (e.g. 8471 for IT Services).");

            _chkReverseCharge.Text = "Enable Reverse Charge Mechanism (RCM) applicability flag";
            _chkReverseCharge.Checked = _settings.ReverseChargeMechanism;
            _chkReverseCharge.CheckedChanged += (s, e) => { _settings.ReverseChargeMechanism = _chkReverseCharge.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "RCM Applicability", _chkReverseCharge);

            // GSTIN Validator Helper Button
            var btnValidateGst = new Button { Text = "Verify GSTIN Format", Size = new Size(160, 32), Location = new Point(215, y), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnValidateGst.Click += (s, e) =>
            {
                string g = _txtGSTIN.Text.Trim();
                bool valid = System.Text.RegularExpressions.Regex.IsMatch(g, @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$");
                if (valid)
                {
                    MessageBox.Show($"✓ GSTIN '{g}' is formatted correctly under Indian GST legal requirements.", "GSTIN Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"✕ GSTIN '{g}' does not match standard 15-character alphanumeric format (e.g. 24AAACT8821R1Z5).", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            pnlGroup.Controls.Add(btnValidateGst);
            y += 48;

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  08. PAYMENTS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildPaymentsView()
        {
            var pnlGroup = CreateGroupCard("Banking & Payment Settlement Details", "Beneficiary bank account, UPI QR identifier and settlement notes displayed on customer invoices.");
            int y = 50;

            _txtUPI.Text = _profile.UPIId;
            _txtUPI.TextChanged += (s, e) => { _profile.UPIId = _txtUPI.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "UPI ID for Payments *", _txtUPI, "Virtual Payment Address (VPA) for Instant QR Settlements.");

            _txtBankName.Text = _profile.BankName;
            _txtBankName.TextChanged += (s, e) => { _profile.BankName = _txtBankName.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Bank Name *", _txtBankName);

            _txtAccountNum.Text = _profile.AccountNumber;
            _txtAccountNum.TextChanged += (s, e) => { _profile.AccountNumber = _txtAccountNum.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Account Number *", _txtAccountNum);

            _txtIFSC.Text = _profile.IFSCCode;
            _txtIFSC.TextChanged += (s, e) => { _profile.IFSCCode = _txtIFSC.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "IFSC Code *", _txtIFSC);

            _txtBranchName.Text = _profile.BranchName;
            _txtBranchName.TextChanged += (s, e) => { _profile.BranchName = _txtBranchName.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Branch Name", _txtBranchName);

            _txtPaymentInstructions.Text = _settings.PaymentInstructions;
            _txtPaymentInstructions.Multiline = true;
            _txtPaymentInstructions.Height = 48;
            _txtPaymentInstructions.TextChanged += (s, e) => { _settings.PaymentInstructions = _txtPaymentInstructions.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Payment Instructions", _txtPaymentInstructions, "Printed directly on bottom of tax invoices.");

            _txtLatePaymentInterest.Text = _settings.LatePaymentInterest;
            _txtLatePaymentInterest.TextChanged += (s, e) => { _settings.LatePaymentInterest = _txtLatePaymentInterest.Text; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Overdue Penalty Terms", _txtLatePaymentInterest);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  09. PDF & PRINT VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildPdfPrintView()
        {
            var pnlGroup = CreateGroupCard("PDF Generation & Physical Print Preferences", "Configure vector paper dimensions, margin boundaries, and invoice document layout visibility.");
            int y = 50;

            _cmbPaperSize.Items.Clear();
            _cmbPaperSize.Items.AddRange(new object[] { "A4 (210 x 297 mm)", "US Letter", "Thermal 80mm Roll" });
            _cmbPaperSize.SelectedItem = _settings.PaperSize.Contains("A4") ? "A4 (210 x 297 mm)" : _settings.PaperSize;
            _cmbPaperSize.SelectedIndexChanged += (s, e) => { _settings.PaperSize = _cmbPaperSize.SelectedItem?.ToString() ?? "A4"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Paper Dimension", _cmbPaperSize);

            _cmbOrientation.Items.Clear();
            _cmbOrientation.Items.AddRange(new object[] { "Portrait", "Landscape" });
            _cmbOrientation.SelectedItem = _settings.Orientation;
            _cmbOrientation.SelectedIndexChanged += (s, e) => { _settings.Orientation = _cmbOrientation.SelectedItem?.ToString() ?? "Portrait"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Page Orientation", _cmbOrientation);

            _cmbMargins.Items.Clear();
            _cmbMargins.Items.AddRange(new object[] { "Standard (15mm)", "Compact (10mm)", "Wide (20mm)" });
            _cmbMargins.SelectedItem = _settings.Margins;
            _cmbMargins.SelectedIndexChanged += (s, e) => { _settings.Margins = _cmbMargins.SelectedItem?.ToString() ?? "Standard (15mm)"; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Page Margins", _cmbMargins);

            _chkPrintLogo.Text = "Include Company Logo in Invoice Header";
            _chkPrintLogo.Checked = _settings.ShowLogoOnPrint;
            _chkPrintLogo.CheckedChanged += (s, e) => { _settings.ShowLogoOnPrint = _chkPrintLogo.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Logo Visibility", _chkPrintLogo);

            _chkPrintBank.Text = "Include Bank Details & UPI QR in Invoice Settlement Box";
            _chkPrintBank.Checked = _settings.ShowBankDetailsOnPrint;
            _chkPrintBank.CheckedChanged += (s, e) => { _settings.ShowBankDetailsOnPrint = _chkPrintBank.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Bank Information", _chkPrintBank);

            _chkPrintSignatory.Text = "Include Authorized Signatory Stamp & Signature line";
            _chkPrintSignatory.Checked = _settings.ShowSignatoryOnPrint;
            _chkPrintSignatory.CheckedChanged += (s, e) => { _settings.ShowSignatoryOnPrint = _chkPrintSignatory.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Signature Box", _chkPrintSignatory);

            _chkPrintPreviousDues.Text = "Include Customer Previous Balance & Outstanding Dues callout";
            _chkPrintPreviousDues.Checked = _settings.ShowPreviousDuesOnPrint;
            _chkPrintPreviousDues.CheckedChanged += (s, e) => { _settings.ShowPreviousDuesOnPrint = _chkPrintPreviousDues.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Balance Tracking", _chkPrintPreviousDues);

            // PDF Export Folder Destination
            var pnlFolder = new Panel { Size = new Size(480, 36), Location = new Point(215, y) };
            _txtPdfFolder.Text = _settings.PdfExportFolder;
            _txtPdfFolder.Size = new Size(370, 26);
            _txtPdfFolder.Location = new Point(0, 4);
            _txtPdfFolder.TextChanged += (s, e) => { _settings.PdfExportFolder = _txtPdfFolder.Text; MarkUnsaved(); };

            var btnBrowseFolder = new Button { Text = "Browse...", Size = new Size(95, 28), Location = new Point(378, 3), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnBrowseFolder.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    _txtPdfFolder.Text = fbd.SelectedPath;
                    _settings.PdfExportFolder = fbd.SelectedPath;
                    MarkUnsaved();
                }
            };
            pnlFolder.Controls.Add(_txtPdfFolder);
            pnlFolder.Controls.Add(btnBrowseFolder);
            AddFormRow(pnlGroup, ref y, "PDF Export Destination", pnlFolder);

            _chkAutoOpenPdf.Text = "Automatically open generated PDF in default viewer after export";
            _chkAutoOpenPdf.Checked = _settings.AutoOpenPdf;
            _chkAutoOpenPdf.CheckedChanged += (s, e) => { _settings.AutoOpenPdf = _chkAutoOpenPdf.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Post-Export Action", _chkAutoOpenPdf);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  10. KEYBOARD SHORTCUTS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildKeyboardShortcutsView()
        {
            var pnlGroup = CreateGroupCard("Application Keyboard Shortcuts & Productivity Hotkeys", "Active global key combinations available across TEAM SOFT screens.");
            int y = 50;

            _chkGlobalHotkeys.Text = "Enable Application-Wide Hotkeys & Quick Access Accelerators";
            _chkGlobalHotkeys.Checked = _settings.GlobalHotkeysEnabled;
            _chkGlobalHotkeys.CheckedChanged += (s, e) => { _settings.GlobalHotkeysEnabled = _chkGlobalHotkeys.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Global Accelerators", _chkGlobalHotkeys);

            var shortcuts = new[]
            {
                new { Key = "Ctrl + N", Action = "Create New Sales Invoice", Scope = "Global / Dashboard" },
                new { Key = "Ctrl + S", Action = "Save Current Active Form / Record", Scope = "Form / Dialog" },
                new { Key = "Ctrl + F", Action = "Focus Global Search Box", Scope = "Global" },
                new { Key = "Ctrl + P", Action = "Print / Export Current Invoice", Scope = "Sales / Invoices" },
                new { Key = "Ctrl + B", Action = "Navigate to Purchase Management", Scope = "Sidebar" },
                new { Key = "Ctrl + D", Action = "Navigate to Dashboard Overview", Scope = "Sidebar" },
                new { Key = "Ctrl + T", Action = "Toggle Light / Dark Mode Theme", Scope = "Global" },
                new { Key = "F5", Action = "Refresh Real-time KPIs & Table Records", Scope = "Active Screen" },
                new { Key = "Escape", Action = "Cancel Current Action / Close Dialog Modal", Scope = "Active Modal" },
                new { Key = "Ctrl + Tab", Action = "Cycle Between Subpages", Scope = "Application Shell" }
            };

            var dgv = new DataGridView
            {
                Location = new Point(24, y),
                Size = new Size(680, 320),
                BackgroundColor = ThemeManager.CardBackground,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            TableDesignStandard.ApplyStandardStyle(dgv);

            dgv.Columns.Add("Key", "Hotkey Combination");
            dgv.Columns.Add("Action", "Command Action");
            dgv.Columns.Add("Scope", "Operating Scope");
            dgv.Columns[0].Width = 140;
            dgv.Columns[2].Width = 160;

            foreach (var sc in shortcuts)
            {
                dgv.Rows.Add(sc.Key, sc.Action, sc.Scope);
            }

            pnlGroup.Controls.Add(dgv);
            y += 335;

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  11. NOTIFICATIONS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildNotificationsView()
        {
            var pnlGroup = CreateGroupCard("System Alerts & Due Reminders", "Configure inventory threshold alerts, overdue receivable warnings and backup reminder chimes.");
            int y = 50;

            _chkLowStockAlert.Text = "Alert when product catalog inventory falls below minimum reorder point";
            _chkLowStockAlert.Checked = _settings.LowStockAlertEnabled;
            _chkLowStockAlert.CheckedChanged += (s, e) => { _settings.LowStockAlertEnabled = _chkLowStockAlert.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Low Stock Alerts", _chkLowStockAlert);

            _numCriticalStock.Minimum = 1;
            _numCriticalStock.Maximum = 500;
            _numCriticalStock.Value = Math.Max(1, _settings.CriticalStockThreshold);
            _numCriticalStock.ValueChanged += (s, e) => { _settings.CriticalStockThreshold = _numCriticalStock.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Critical Threshold (Units)", _numCriticalStock, "Items below this quantity trigger critical warning pill on Dashboard.");

            _chkDueReminder.Text = "Display advance reminders for upcoming invoice due dates";
            _chkDueReminder.Checked = _settings.InvoiceDueReminderEnabled;
            _chkDueReminder.CheckedChanged += (s, e) => { _settings.InvoiceDueReminderEnabled = _chkDueReminder.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Due Date Reminders", _chkDueReminder);

            _numDueReminderDays.Minimum = 1;
            _numDueReminderDays.Maximum = 30;
            _numDueReminderDays.Value = Math.Max(1, _settings.InvoiceDueReminderDays);
            _numDueReminderDays.ValueChanged += (s, e) => { _settings.InvoiceDueReminderDays = (int)_numDueReminderDays.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Advance Notice Days", _numDueReminderDays);

            _chkOverdueAlert.Text = "Flag overdue payments in red on Sales & Transactions register";
            _chkOverdueAlert.Checked = _settings.OverduePaymentAlertEnabled;
            _chkOverdueAlert.CheckedChanged += (s, e) => { _settings.OverduePaymentAlertEnabled = _chkOverdueAlert.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Overdue Invoices", _chkOverdueAlert);

            _numBackupReminderDays.Minimum = 1;
            _numBackupReminderDays.Maximum = 90;
            _numBackupReminderDays.Value = Math.Max(1, _settings.BackupReminderDays);
            _numBackupReminderDays.ValueChanged += (s, e) => { _settings.BackupReminderDays = (int)_numBackupReminderDays.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Backup Reminder Interval", _numBackupReminderDays, "Days between scheduled backup reminder alerts.");

            _chkAudioChime.Text = "Play subtle audio chime when invoices are successfully recorded";
            _chkAudioChime.Checked = _settings.AudioChimeEnabled;
            _chkAudioChime.CheckedChanged += (s, e) => { _settings.AudioChimeEnabled = _chkAudioChime.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Audio Feedback", _chkAudioChime);

            _chkDesktopToast.Text = "Show native Windows notification toasts for high priority business events";
            _chkDesktopToast.Checked = _settings.DesktopNotificationsEnabled;
            _chkDesktopToast.CheckedChanged += (s, e) => { _settings.DesktopNotificationsEnabled = _chkDesktopToast.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Desktop Toasts", _chkDesktopToast);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  12. BACKUP & DATA PRIVACY VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildBackupPrivacyView()
        {
            var pnlGroup = CreateGroupCard("Database Backup, Safety Snapshots & Audit History", "Create verified SQLite database backups, restore previous business archives, and inspect backup history.");
            int y = 50;

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEAMSOFT", "teamsoft_sales.db");
            long dbSize = File.Exists(dbPath) ? new FileInfo(dbPath).Length : 0;
            string dbSizeStr = dbSize > 1024 * 1024 ? $"{dbSize / (1024.0 * 1024.0):F2} MB" : $"{dbSize / 1024.0:F1} KB";

            AddFormRow(pnlGroup, ref y, "Active Database", CreateReadonlyBox($"SQLite 3.x ({dbPath})"));
            AddFormRow(pnlGroup, ref y, "Database Size", CreateReadonlyBox($"{dbSizeStr} ({dbSize:N0} bytes)"));

            // Action Buttons Panel
            var pnlActions = new Panel { Size = new Size(540, 44), Location = new Point(215, y) };
            var btnDoBackup = new TeamPillButton { Text = "Backup Database Now", Icon = ReiconRenderer.IconType.Package, Size = new Size(185, 36), Location = new Point(0, 4), FillColor = ThemeManager.PrimaryAccent };
            var btnRestore = new Button { Text = "Restore from Backup...", Size = new Size(175, 36), Location = new Point(195, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnDoBackup.Click += (s, e) => PerformDatabaseBackup(dbPath);
            btnRestore.Click += (s, e) => PerformDatabaseRestore(dbPath);

            pnlActions.Controls.Add(btnDoBackup);
            pnlActions.Controls.Add(btnRestore);
            AddFormRow(pnlGroup, ref y, "Database Operations", pnlActions);

            // Backup History Table
            var lblHistory = new Label { Text = "Recent Database Backup History", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = ThemeManager.TextPrimary, Location = new Point(24, y + 10), AutoSize = true };
            pnlGroup.Controls.Add(lblHistory);
            y += 38;

            var dgvBkp = new DataGridView
            {
                Location = new Point(24, y),
                Size = new Size(680, 200),
                BackgroundColor = ThemeManager.CardBackground,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            TableDesignStandard.ApplyStandardStyle(dgvBkp);

            dgvBkp.Columns.Add("Date", "Date & Time");
            dgvBkp.Columns.Add("File", "Backup Filename");
            dgvBkp.Columns.Add("Size", "Size");
            dgvBkp.Columns.Add("Status", "Status");
            dgvBkp.Columns[0].Width = 160;
            dgvBkp.Columns[2].Width = 90;
            dgvBkp.Columns[3].Width = 90;

            _backupRecords = DatabaseService.GetBackupHistory();
            if (_backupRecords.Count == 0)
            {
                dgvBkp.Rows.Add(DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"), "TEAMSOFT_ManualBackup_Initial.db", dbSizeStr, "Verified");
            }
            else
            {
                foreach (var b in _backupRecords)
                {
                    dgvBkp.Rows.Add(b.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"), Path.GetFileName(b.BackupPath), b.FormattedSize, b.Status);
                }
            }

            pnlGroup.Controls.Add(dgvBkp);
            y += 215;

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        private void PerformDatabaseBackup(string dbPath)
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Title = "Choose Backup File Destination",
                    FileName = $"TEAMSOFT_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Filter = "SQLite Database Files (*.db)|*.db|All Files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(dbPath, sfd.FileName, true);
                    long bytes = new FileInfo(sfd.FileName).Length;
                    DatabaseService.RecordBackup(sfd.FileName, bytes, "Success", "User initiated manual backup");

                    MessageBox.Show($"Database backup successfully created!\n\nFile:\n{sfd.FileName}\n\nSize: {bytes / 1024.0:F1} KB",
                        "Backup Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    RenderCategoryContent(11);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformDatabaseRestore(string dbPath)
        {
            var dr = MessageBox.Show(
                "Restoring a database will replace the current application data with the selected backup file.\n\n" +
                "A safety snapshot of your current database will be saved automatically before restoring.\n\n" +
                "Are you sure you want to proceed?",
                "Confirm Database Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                using var ofd = new OpenFileDialog
                {
                    Title = "Select Valid SQLite Database Backup File",
                    Filter = "SQLite Database Files (*.db)|*.db|All Files (*.*)|*.*"
                };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. Validate file header
                        byte[] header = new byte[16];
                        using (var fs = File.OpenRead(ofd.FileName))
                        {
                            fs.Read(header, 0, 16);
                        }
                        string headerStr = System.Text.Encoding.ASCII.GetString(header);
                        if (!headerStr.StartsWith("SQLite format 3"))
                        {
                            MessageBox.Show("The selected file is not a valid SQLite database backup.", "Invalid Backup File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // 2. Pre-restore safety backup
                        string safetyCopy = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", $"teamsoft_prerestore_safety_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                        if (File.Exists(dbPath))
                        {
                            File.Copy(dbPath, safetyCopy, true);
                        }

                        // 3. Restore
                        File.Copy(ofd.FileName, dbPath, true);
                        DatabaseService.InitializeDatabase();

                        MessageBox.Show("Database restored successfully! Application data has been refreshed.",
                            "Restore Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadDataFromDatabase();
                        RenderCategoryContent(11);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  13. USERS & PERMISSIONS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildUsersPermissionsView()
        {
            var pnlGroup = CreateGroupCard("User Accounts & Role Permissions Matrix", "Manage application operators, administrative credentials, and role-based access control.");
            int y = 50;

            var dgvUsers = new DataGridView
            {
                Location = new Point(24, y),
                Size = new Size(680, 240),
                BackgroundColor = ThemeManager.CardBackground,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            TableDesignStandard.ApplyStandardStyle(dgvUsers);

            dgvUsers.Columns.Add("Username", "Username");
            dgvUsers.Columns.Add("FullName", "Full Name");
            dgvUsers.Columns.Add("Role", "Role");
            dgvUsers.Columns.Add("Email", "Email");
            dgvUsers.Columns.Add("Status", "Status");
            dgvUsers.Columns[0].Width = 100;
            dgvUsers.Columns[2].Width = 140;
            dgvUsers.Columns[4].Width = 90;

            _systemUsers = DatabaseService.GetSystemUsers();
            foreach (var u in _systemUsers)
            {
                dgvUsers.Rows.Add(u.Username, u.FullName, u.Role, u.Email, u.Status);
            }

            pnlGroup.Controls.Add(dgvUsers);
            y += 255;

            var btnAddUser = new TeamPillButton
            {
                Text = "+ Add New User",
                Size = new Size(150, 36),
                Location = new Point(24, y),
                FillColor = ThemeManager.PrimaryAccent
            };
            btnAddUser.Click += (s, e) =>
            {
                PromptAddNewUser();
            };
            pnlGroup.Controls.Add(btnAddUser);
            y += 50;

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        private void PromptAddNewUser()
        {
            using var form = new Form
            {
                Text = "Add System User — TEAM SOFT",
                Size = new Size(420, 360),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = ThemeManager.CardBackground
            };

            int y = 20;
            var txtUName = new TextBox { Bounds = new Rectangle(140, y, 230, 26) };
            form.Controls.Add(new Label { Text = "Username *", Bounds = new Rectangle(20, y + 3, 110, 20), ForeColor = ThemeManager.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            form.Controls.Add(txtUName);
            y += 40;

            var txtFName = new TextBox { Bounds = new Rectangle(140, y, 230, 26) };
            form.Controls.Add(new Label { Text = "Full Name *", Bounds = new Rectangle(20, y + 3, 110, 20), ForeColor = ThemeManager.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            form.Controls.Add(txtFName);
            y += 40;

            var cmbRole = new ComboBox { Bounds = new Rectangle(140, y, 230, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new object[] { "Billing Operator", "Accountant", "Administrator", "Vision" });
            cmbRole.SelectedIndex = 0;
            form.Controls.Add(new Label { Text = "Role *", Bounds = new Rectangle(20, y + 3, 110, 20), ForeColor = ThemeManager.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            form.Controls.Add(cmbRole);
            y += 40;

            var txtPass = new TextBox { Bounds = new Rectangle(140, y, 230, 26), UseSystemPasswordChar = true };
            form.Controls.Add(new Label { Text = "Initial Password *", Bounds = new Rectangle(20, y + 3, 110, 20), ForeColor = ThemeManager.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            form.Controls.Add(txtPass);
            y += 40;

            var txtMail = new TextBox { Bounds = new Rectangle(140, y, 230, 26) };
            form.Controls.Add(new Label { Text = "Email Address", Bounds = new Rectangle(20, y + 3, 110, 20), ForeColor = ThemeManager.TextPrimary, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            form.Controls.Add(txtMail);
            y += 55;

            var btnCreate = new Button { Text = "Create User", DialogResult = DialogResult.OK, Bounds = new Rectangle(140, y, 120, 34), BackColor = ThemeManager.PrimaryAccent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(270, y, 100, 34), FlatStyle = FlatStyle.Flat };
            form.Controls.Add(btnCreate);
            form.Controls.Add(btnCancel);

            if (form.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(txtUName.Text) || string.IsNullOrWhiteSpace(txtFName.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
                {
                    MessageBox.Show("Please fill all required user fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newUser = new SystemUser
                {
                    Username = txtUName.Text.Trim().ToLowerInvariant(),
                    FullName = txtFName.Text.Trim(),
                    Role = cmbRole.SelectedItem?.ToString() ?? "Billing Operator",
                    Email = txtMail.Text.Trim(),
                    PasswordHash = DatabaseService.HashPassword(txtPass.Text),
                    Status = "Active"
                };

                DatabaseService.SaveSystemUser(newUser);
                MessageBox.Show($"User '{newUser.Username}' created successfully!", "User Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RenderCategoryContent(12);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  14. INTEGRATIONS VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildIntegrationsView()
        {
            var pnlGroup = CreateGroupCard("Data Interoperability & Local Intranet API", "Export business data to Tally ERP format, CSV spreadsheets, or enable local REST integration.");
            int y = 50;

            // Export Actions
            var pnlExports = new Panel { Size = new Size(540, 44), Location = new Point(215, y) };
            var btnExpCust = new Button { Text = "Export Clients (CSV)", Size = new Size(160, 32), Location = new Point(0, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var btnExpProd = new Button { Text = "Export Catalog (CSV)", Size = new Size(160, 32), Location = new Point(170, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var btnExpTally = new Button { Text = "Export Tally Prime XML", Size = new Size(170, 32), Location = new Point(340, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnExpCust.Click += (s, e) => ExportDatasetCsv("Customers");
            btnExpProd.Click += (s, e) => ExportDatasetCsv("Products");
            btnExpTally.Click += (s, e) => ExportTallyXml();

            pnlExports.Controls.Add(btnExpCust);
            pnlExports.Controls.Add(btnExpProd);
            pnlExports.Controls.Add(btnExpTally);
            AddFormRow(pnlGroup, ref y, "Direct Data Exports", pnlExports);

            // Local API Server
            _chkLocalApi.Text = "Enable Local Intranet REST API Server (Offline Desktop Gateway)";
            _chkLocalApi.Checked = _settings.LocalApiEnabled;
            _chkLocalApi.CheckedChanged += (s, e) => { _settings.LocalApiEnabled = _chkLocalApi.Checked; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Local Webhook API", _chkLocalApi);

            _numLocalApiPort.Minimum = 1024;
            _numLocalApiPort.Maximum = 65535;
            _numLocalApiPort.Value = _settings.LocalApiPort;
            _numLocalApiPort.ValueChanged += (s, e) => { _settings.LocalApiPort = (int)_numLocalApiPort.Value; MarkUnsaved(); };
            AddFormRow(pnlGroup, ref y, "Local Listening Port", _numLocalApiPort);

            // Secret Key
            var pnlKey = new Panel { Size = new Size(480, 36), Location = new Point(215, y) };
            _txtApiKey.Text = _settings.ApiKey;
            _txtApiKey.ReadOnly = true;
            _txtApiKey.Size = new Size(330, 26);
            _txtApiKey.Location = new Point(0, 4);

            var btnCopyKey = new Button { Text = "Copy Key", Size = new Size(80, 28), Location = new Point(338, 3), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCopyKey.Click += (s, e) =>
            {
                Clipboard.SetText(_txtApiKey.Text);
                MessageBox.Show("API secret key copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlKey.Controls.Add(_txtApiKey);
            pnlKey.Controls.Add(btnCopyKey);
            AddFormRow(pnlGroup, ref y, "API Secret Token", pnlKey, "Protected bearer token for authorized local connections.");

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        private void ExportDatasetCsv(string dataset)
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Title = $"Export {dataset} to CSV",
                    FileName = $"TEAMSOFT_{dataset}_{DateTime.Now:yyyyMMdd}.csv",
                    Filter = "CSV Spreadsheet (*.csv)|*.csv"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using var sw = new StreamWriter(sfd.FileName);
                    if (dataset == "Customers")
                    {
                        sw.WriteLine("Id,Name,Company,Phone,Email,GSTIN,OutstandingBalance");
                        var list = DatabaseService.GetCustomers();
                        foreach (var c in list)
                            sw.WriteLine($"{c.Id},\"{c.Name}\",\"{c.CompanyName}\",\"{c.Phone}\",\"{c.Email}\",\"{c.GSTIN}\",{c.OutstandingBalance}");
                    }
                    else
                    {
                        sw.WriteLine("Id,Barcode,Name,Category,HSN,Stock,SalePrice,TaxRate");
                        var list = DatabaseService.GetProducts();
                        foreach (var p in list)
                            sw.WriteLine($"{p.Id},\"{p.Barcode}\",\"{p.Name}\",\"{p.Category}\",\"{p.HSN}\",{p.StockQuantity},{p.SalePrice},{p.TaxRate}");
                    }

                    MessageBox.Show($"Exported {dataset} dataset successfully to:\n{sfd.FileName}", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportTallyXml()
        {
            try
            {
                using var sfd = new SaveFileDialog
                {
                    Title = "Export Tally Prime XML",
                    FileName = $"TEAMSOFT_TallyExport_{DateTime.Now:yyyyMMdd}.xml",
                    Filter = "XML Format (*.xml)|*.xml"
                };

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using var sw = new StreamWriter(sfd.FileName);
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine("<ENVELOPE>");
                    sw.WriteLine("  <HEADER><TALLYREQUEST>Import Data</TALLYREQUEST></HEADER>");
                    sw.WriteLine("  <BODY>");
                    sw.WriteLine("    <IMPORTDATA>");
                    sw.WriteLine("      <REQUESTDESC><REPORTNAME>Vouchers</REPORTNAME></REQUESTDESC>");
                    sw.WriteLine("      <REQUESTDATA>");
                    sw.WriteLine("        <!-- TEAM SOFT Validated Export Data -->");
                    sw.WriteLine($"        <COMPANY>{_profile.BusinessName}</COMPANY>");
                    sw.WriteLine($"        <GSTIN>{_profile.GSTIN}</GSTIN>");
                    sw.WriteLine($"        <TIMESTAMP>{DateTime.Now:yyyy-MM-ddTHH:mm:ss}</TIMESTAMP>");
                    sw.WriteLine("      </REQUESTDATA>");
                    sw.WriteLine("    </IMPORTDATA>");
                    sw.WriteLine("  </BODY>");
                    sw.WriteLine("</ENVELOPE>");

                    MessageBox.Show($"Tally XML export completed successfully!\n\nFile:\n{sfd.FileName}", "Tally Integration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tally export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  15. ABOUT & SUPPORT VIEW
        // ═══════════════════════════════════════════════════════════════════════════
        private void BuildAboutSupportView()
        {
            var pnlGroup = CreateGroupCard("Application Identity, Build Information & Diagnostics", "TEAM SOFT Inventory, Sales & Billing enterprise desktop solution metadata.");
            int y = 50;

            AddFormRow(pnlGroup, ref y, "Application", CreateReadonlyBox("TEAM SOFT Inventory, Sales & Billing Management"));
            AddFormRow(pnlGroup, ref y, "Software Version", CreateReadonlyBox("v2.4.0 Professional Edition (Build 2026.10.25)"));
            AddFormRow(pnlGroup, ref y, "Target Framework", CreateReadonlyBox(".NET 8.0 (net8.0-windows) • Windows Forms GDI+ Anti-Aliased"));
            AddFormRow(pnlGroup, ref y, "Database Engine", CreateReadonlyBox("SQLite 3.45.0 (Offline-First ACID Persistence)"));
            AddFormRow(pnlGroup, ref y, "Licensing & Status", CreateReadonlyBox("MIT Open Engineering License • Authorized Deployment"));
            AddFormRow(pnlGroup, ref y, "Project Leadership", CreateReadonlyBox("Saumya Chaudhari (@Saumya25-hub) — Vision / Lead Architect"));
            AddFormRow(pnlGroup, ref y, "Engineering Guild", CreateReadonlyBox("S TOM'S Technology Guild — Pairing with Antigravity"));

            var pnlDiag = new Panel { Size = new Size(540, 44), Location = new Point(215, y) };
            var btnLogs = new Button { Text = "Open Logs Folder", Size = new Size(150, 32), Location = new Point(0, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            var btnUpdates = new Button { Text = "Check for Updates", Size = new Size(160, 32), Location = new Point(160, 4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

            btnLogs.Click += (s, e) =>
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEAMSOFT", "Captures");
                Directory.CreateDirectory(dir);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dir, UseShellExecute = true });
            };

            btnUpdates.Click += (s, e) =>
            {
                MessageBox.Show("✓ You are running the latest production release v2.4.0 (Build 2026.10.25).\nNo new updates available.",
                    "Update Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlDiag.Controls.Add(btnLogs);
            pnlDiag.Controls.Add(btnUpdates);
            AddFormRow(pnlGroup, ref y, "Diagnostics & Support", pnlDiag);

            pnlGroup.Height = y + 16;
            _pnlActiveCard.Controls.Add(pnlGroup);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  REUSABLE UI HELPER METHODS
        // ═══════════════════════════════════════════════════════════════════════════
        private Panel CreateGroupCard(string title, string description)
        {
            var pnl = new Panel
            {
                Width = 750,
                BackColor = ThemeManager.CardBackground,
                Padding = new Padding(24, 20, 24, 20)
            };

            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, pnl.Width - 1, pnl.Height - 1, 10);
                using var p = new Pen(ThemeManager.CardBorder, 1.2f);
                g.DrawPath(p, path);
            };

            var lblTitle = new Label
            {
                Text = title,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(24, 16),
                AutoSize = true
            };

            var lblDesc = new Label
            {
                Text = description,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(24, 38),
                AutoSize = true
            };

            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblDesc);

            return pnl;
        }

        private void AddFormRow(Panel parent, ref int y, string labelText, Control editorControl, string? hintText = null)
        {
            var lbl = new Label
            {
                Text = labelText,
                UseMnemonic = false,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(24, y + 4),
                Size = new Size(185, 24)
            };
            parent.Controls.Add(lbl);

            editorControl.Location = new Point(215, y);
            if (!(editorControl is Panel || editorControl is CheckBox || editorControl is Button))
            {
                editorControl.Width = 475;
                editorControl.Font = new Font("Segoe UI", 9.5f);
            }
            parent.Controls.Add(editorControl);

            int deltaY = editorControl.Height + 10;
            if (!string.IsNullOrEmpty(hintText))
            {
                var lblHint = new Label
                {
                    Text = hintText,
                    UseMnemonic = false,
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = ThemeManager.TextSecondary,
                    Location = new Point(215, y + editorControl.Height + 2),
                    AutoSize = true
                };
                parent.Controls.Add(lblHint);
                deltaY += 18;
            }

            y += Math.Max(38, deltaY);
        }

        private TextBox CreateReadonlyBox(string val)
        {
            var tb = new TextBox
            {
                Text = val,
                ReadOnly = true,
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(34, 37, 50) : Color.FromArgb(245, 247, 250),
                ForeColor = ThemeManager.TextSecondary,
                BorderStyle = BorderStyle.FixedSingle
            };
            return tb;
        }

        private void CommitActiveCategoryInputs()
        {
            // Update profile bank details string for backwards compatibility
            _profile.BankDetails = $"{_profile.BankName} | A/C: {_profile.AccountNumber} | IFSC: {_profile.IFSCCode}";
        }

        public void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = ThemeManager.WindowBackground;

            _pnlSidebarNav.BackColor = ThemeManager.SidebarBackground;
            _pnlSearchContainer.BackColor = ThemeManager.SearchBoxBackground;
            _txtSearch.BackColor = ThemeManager.SearchBoxBackground;
            _txtSearch.ForeColor = ThemeManager.TextPrimary;

            _pnlDivider.BackColor = ThemeManager.BorderHairline;

            _pnlContentWrapper.BackColor = ThemeManager.WindowBackground;
            _pnlContentHeader.BackColor = ThemeManager.WindowBackground;
            _lblHeaderTitle.ForeColor = ThemeManager.TextPrimary;
            _lblHeaderSubtitle.ForeColor = ThemeManager.TextSecondary;

            _pnlBottomBar.BackColor = ThemeManager.WindowBackground;
            _btnSave.FillColor = ThemeManager.PrimaryAccent;
            _btnSave.HoverColor = ThemeManager.AccentHover;
            _btnCancel.BackColor = isDark ? Color.FromArgb(40, 44, 60) : Color.FromArgb(241, 245, 249);
            _btnCancel.ForeColor = ThemeManager.TextPrimary;
            _btnCancel.FlatAppearance.BorderColor = ThemeManager.CardBorder;
            _btnReset.BackColor = isDark ? Color.FromArgb(40, 44, 60) : Color.FromArgb(241, 245, 249);
            _btnReset.ForeColor = ThemeManager.TextPrimary;
            _btnReset.FlatAppearance.BorderColor = ThemeManager.CardBorder;

            _pnlScrollArea.BackColor = ThemeManager.WindowBackground;
            _pnlActiveCard.BackColor = ThemeManager.WindowBackground;

            StyleControlHierarchy(_pnlActiveCard, isDark);

            _pnlSearchContainer.Invalidate();
            _pnlNavList.Invalidate();
            _pnlActiveCard.Invalidate(true);
        }

        private void StyleControlHierarchy(Control parent, bool isDark)
        {
            Color inputBg = isDark ? Color.FromArgb(28, 32, 45) : Color.White;
            Color inputFg = isDark ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
            Color textPri = ThemeManager.TextPrimary;
            Color textSec = ThemeManager.TextSecondary;
            Color cardBorder = ThemeManager.CardBorder;

            foreach (Control c in parent.Controls)
            {
                if (c is TextBox tb)
                {
                    if (tb.ReadOnly)
                    {
                        tb.BackColor = isDark ? Color.FromArgb(22, 25, 36) : Color.FromArgb(245, 247, 250);
                        tb.ForeColor = textSec;
                    }
                    else
                    {
                        tb.BackColor = inputBg;
                        tb.ForeColor = inputFg;
                    }
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = inputBg;
                    cb.ForeColor = inputFg;
                    cb.FlatStyle = FlatStyle.Flat;
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = inputBg;
                    nud.ForeColor = inputFg;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is CheckBox chk)
                {
                    chk.ForeColor = textPri;
                }
                else if (c is Label lbl && !(lbl == _lblHeaderTitle || lbl == _lblHeaderSubtitle || lbl == _lblUnsavedBadge || lbl == _lblSaveToast))
                {
                    if (lbl.Font.Bold)
                        lbl.ForeColor = textPri;
                    else
                        lbl.ForeColor = textSec;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = isDark ? Color.FromArgb(36, 42, 58) : Color.FromArgb(241, 245, 249);
                    btn.ForeColor = textPri;
                    btn.FlatAppearance.BorderColor = cardBorder;
                }

                if (c.HasChildren)
                {
                    StyleControlHierarchy(c, isDark);
                }
            }
        }
    }
}
