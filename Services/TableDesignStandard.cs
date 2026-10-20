using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Configuration options for standardizing DataGridView controls across the project.
    /// </summary>
    public class TableStyleOptions
    {
        public int HeaderHeight { get; set; } = 32;
        public int RowHeight { get; set; } = 28;
        public bool EnableStatusPillOrColors { get; set; } = true;
        public bool AlternatingRows { get; set; } = true;
        public string EmptyStateMessage { get; set; } = "No records available";
        public string? EmptyStateSubtitle { get; set; } = null;
        public bool DrawOuterBorder { get; set; } = true;
    }

    /// <summary>
    /// Centralized Desktop Table Standardization Engine for TEAM SOFT / S TOM'S.
    /// Implements the exact layout and visual appearance from the reference design:
    /// - Clean rectangular headers with light gray background and crisp vertical column dividers
    /// - Compact, readable data rows (28-30px) with subtle hairline horizontal separators
    /// - Right-aligned currency and financial values with matching header alignment
    /// - Semantic status badges and alert indicators for Paid, Unpaid, Partially Paid, Cancelled, CRITICAL, LOW
    /// - Full Light and Dark mode synchronization with zero flickering
    /// - Automatic text truncation with ellipses and tooltip support
    /// - Graceful empty states when no records are present
    /// </summary>
    public static class TableDesignStandard
    {
        private static readonly ToolTip s_cellToolTip = new()
        {
            InitialDelay = 300,
            ReshowDelay = 100,
            AutoPopDelay = 5000
        };

        /// <summary>
        /// Enables hardware double-buffering on the specified DataGridView to prevent repaint flickering.
        /// </summary>
        public static void EnableDoubleBuffering(DataGridView dgv)
        {
            if (dgv == null) return;
            try
            {
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null, dgv, new object[] { true });
            }
            catch
            {
                // Fallback gracefully
            }
        }

        /// <summary>
        /// Applies the standardized table design to the specified DataGridView.
        /// </summary>
        public static void ApplyStandardGrid(DataGridView dgv, TableStyleOptions? options = null)
        {
            if (dgv == null) return;

            options ??= new TableStyleOptions();

            EnableDoubleBuffering(dgv);

            bool isDark = ThemeManager.IsDarkMode;

            // Structural configurations
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = options.HeaderHeight;
            dgv.RowTemplate.Height = options.RowHeight;

            // Palette tokens
            Color bgCard = ThemeManager.CardBackground;
            Color bgHeader = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);
            Color fgHeader = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);
            Color gridLineColor = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
            Color textPrimary = ThemeManager.TextPrimary;
            Color bgAltRow = isDark ? Color.FromArgb(22, 24, 34) : Color.FromArgb(250, 250, 252);
            Color selBg = isDark ? Color.FromArgb(38, 48, 72) : Color.FromArgb(239, 246, 255);
            Color selFg = isDark ? Color.FromArgb(147, 197, 253) : Color.FromArgb(30, 64, 175);

            dgv.BackgroundColor = bgCard;
            dgv.GridColor = gridLineColor;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = bgHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = fgHeader;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = bgHeader;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = fgHeader;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgv.DefaultCellStyle.BackColor = bgCard;
            dgv.DefaultCellStyle.ForeColor = textPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = selBg;
            dgv.DefaultCellStyle.SelectionForeColor = selFg;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.DefaultCellStyle.Padding = new Padding(6, 1, 6, 1);

            if (options.AlternatingRows)
            {
                dgv.AlternatingRowsDefaultCellStyle.BackColor = bgAltRow;
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = textPrimary;
                dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = selBg;
                dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = selFg;
            }

            // Tag options for reuse during event handling
            dgv.Tag = options;

            // Remove any existing standard handlers to avoid duplicates
            dgv.CellPainting -= OnStandardCellPainting;
            dgv.CellPainting += OnStandardCellPainting;

            dgv.CellFormatting -= OnStandardCellFormatting;
            dgv.CellFormatting += OnStandardCellFormatting;

            dgv.Paint -= OnStandardGridPaint;
            dgv.Paint += OnStandardGridPaint;

            dgv.CellMouseEnter -= OnStandardCellMouseEnter;
            dgv.CellMouseEnter += OnStandardCellMouseEnter;

            dgv.Invalidate();
        }

        private static void OnStandardCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView dgv || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            try
            {
                var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string val = cell.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(val) && val.Length > 20)
                {
                    cell.ToolTipText = val;
                }
            }
            catch
            {
            }
        }

        private static void OnStandardCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (sender is not DataGridView dgv || e.Graphics == null || e.ColumnIndex < 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            bool isDark = ThemeManager.IsDarkMode;
            Color bdrColor = isDark ? Color.FromArgb(48, 54, 76) : Color.FromArgb(203, 213, 225);
            Color headerBg = isDark ? Color.FromArgb(28, 33, 48) : Color.FromArgb(248, 250, 252);
            Color headerFg = isDark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);

            // ─────────────────────────────────────────────────────────────────
            // 1. COLUMN HEADER ROW (-1)
            // Draws clean background, subtle top/bottom borders, and VERTICAL DIVIDERS
            // ─────────────────────────────────────────────────────────────────
            if (e.RowIndex == -1)
            {
                // Background
                using (var bBg = new SolidBrush(headerBg))
                {
                    g.FillRectangle(bBg, e.CellBounds);
                }

                // Top hairline border
                using (var pBdr = new Pen(bdrColor, 1f))
                {
                    g.DrawLine(pBdr, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                    // Bottom border line separating headers from data rows
                    g.DrawLine(pBdr, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);

                    // VERTICAL SEPARATOR LINE (Crisp divider between columns)
                    g.DrawLine(pBdr, e.CellBounds.Right - 1, e.CellBounds.Top + 2, e.CellBounds.Right - 1, e.CellBounds.Bottom - 3);

                    // Outer left vertical divider if first column
                    if (e.ColumnIndex == 0)
                    {
                        g.DrawLine(pBdr, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom - 1);
                    }
                }

                // Text Alignment
                string colName = dgv.Columns[e.ColumnIndex].Name;
                string colHeader = e.Value?.ToString() ?? string.Empty;
                var align = dgv.Columns[e.ColumnIndex].DefaultCellStyle.Alignment;

                bool isRight = (align == DataGridViewContentAlignment.MiddleRight) ||
                               colName.Contains("Amount", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Total", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Paid", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Balance", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Price", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Cost", StringComparison.OrdinalIgnoreCase) ||
                               colName.Contains("Taxable", StringComparison.OrdinalIgnoreCase);

                bool isCenter = (align == DataGridViewContentAlignment.MiddleCenter) ||
                                colName.Contains("Status", StringComparison.OrdinalIgnoreCase) ||
                                colName.Equals("Action", StringComparison.OrdinalIgnoreCase) ||
                                colName.Equals("Actions", StringComparison.OrdinalIgnoreCase) ||
                                colName.Contains("Alert", StringComparison.OrdinalIgnoreCase);

                var sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = isRight ? StringAlignment.Far : (isCenter ? StringAlignment.Center : StringAlignment.Near),
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                int padX = 8;
                var textRect = new Rectangle(e.CellBounds.X + padX, e.CellBounds.Y, e.CellBounds.Width - (padX * 2), e.CellBounds.Height);

                using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (var bText = new SolidBrush(headerFg))
                {
                    g.DrawString(colHeader, f, bText, textRect, sf);
                }

                e.Handled = true;
                return;
            }

            // ─────────────────────────────────────────────────────────────────
            // 2. DATA ROW CELLS
            // Handled with standard painting + subtle hairline bottom separator
            // ─────────────────────────────────────────────────────────────────
            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

            // Draw crisp horizontal hairline row separator
            Color rowSepColor = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
            using (var pSep = new Pen(rowSepColor, 1f))
            {
                g.DrawLine(pSep, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
            }

            e.Handled = true;
        }

        private static void OnStandardCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (sender is not DataGridView dgv || e.RowIndex < 0 || e.ColumnIndex < 0 || e.CellStyle == null) return;

            string colName = dgv.Columns[e.ColumnIndex].Name;
            string val = e.Value?.ToString() ?? string.Empty;

            // Semantic Status Formatting
            if (colName.Contains("Status", StringComparison.OrdinalIgnoreCase))
            {
                if (val.Equals("Paid", StringComparison.OrdinalIgnoreCase) || val.Equals("Settled", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = ThemeManager.StatusPositive;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (val.Equals("Partially Paid", StringComparison.OrdinalIgnoreCase) || val.Equals("Pending", StringComparison.OrdinalIgnoreCase) || val.Contains("Due", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = ThemeManager.StatusWarning;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
                else if (val.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) || val.Equals("Void", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
                }
                else if (val.Equals("Unpaid", StringComparison.OrdinalIgnoreCase) || val.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = ThemeManager.StatusNegative;
                    e.CellStyle.Font = new Font(dgv.Font, FontStyle.Bold);
                }
            }

            // Inventory Alert Level Formatting (CRITICAL / LOW)
            if (colName.Contains("Alert", StringComparison.OrdinalIgnoreCase) || colName.Equals("Action", StringComparison.OrdinalIgnoreCase))
            {
                if (val.Equals("CRITICAL", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (val.Equals("LOW", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else if (val.Equals("NORMAL", StringComparison.OrdinalIgnoreCase) || val.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = ThemeManager.StatusPositive;
                }
            }
        }

        private static void OnStandardGridPaint(object? sender, PaintEventArgs e)
        {
            if (sender is not DataGridView dgv) return;

            // Empty State Painting when 0 rows are available
            if (dgv.RowCount == 0)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var opt = dgv.Tag as TableStyleOptions ?? new TableStyleOptions();

                int topOffset = dgv.ColumnHeadersVisible ? dgv.ColumnHeadersHeight : 0;
                int availH = dgv.Height - topOffset;
                if (availH <= 40) return;

                int centerY = topOffset + (availH / 2);
                int cx = dgv.Width / 2;

                bool isDark = ThemeManager.IsDarkMode;
                Color textMid = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
                Color textSub = isDark ? Color.FromArgb(100, 116, 139) : Color.FromArgb(148, 163, 184);

                using var fTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold);
                using var fSub = new Font("Segoe UI", 8.5f);
                using var bTitle = new SolidBrush(textMid);
                using var bSub = new SolidBrush(textSub);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                g.DrawString(opt.EmptyStateMessage, fTitle, bTitle, cx, centerY - 10, sf);
                if (!string.IsNullOrEmpty(opt.EmptyStateSubtitle))
                {
                    g.DrawString(opt.EmptyStateSubtitle, fSub, bSub, cx, centerY + 14, sf);
                }
            }
        }

        /// <summary>
        /// Paints a standardized outer border around a card or panel hosting a DataGridView.
        /// </summary>
        public static void PaintCardContainerBorder(Graphics g, Rectangle rect)
        {
            if (g == null || rect.Width <= 0 || rect.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bdrCol = ThemeManager.CardBorder;
            using var pen = new Pen(bdrCol, 1.2f);
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        }
    }
}
