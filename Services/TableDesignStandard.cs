using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Configuration options for standardizing DataGridView controls across the project.
    /// </summary>
    public class TableStyleOptions
    {
        public int HeaderHeight { get; set; } = 34;
        public int RowHeight { get; set; } = 32;
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
    /// - Compact, readable data rows (32px) with subtle hairline horizontal separators
    /// - Right-aligned currency and financial values with matching header alignment
    /// - Anti-aliased semantic status pill badges with indicator dots for Paid, Unpaid, Partially Paid, Cancelled, CRITICAL, LOW
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
            Color selBg = isDark ? Color.FromArgb(38, 48, 76) : Color.FromArgb(239, 246, 255);
            Color selFg = isDark ? Color.White : Color.FromArgb(15, 23, 42);

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

                    // VERTICAL SEPARATOR LINE (Clean, padded divider between columns)
                    g.DrawLine(pBdr, e.CellBounds.Right - 1, e.CellBounds.Top + 6, e.CellBounds.Right - 1, e.CellBounds.Bottom - 6);

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

                int padX = 10;
                var textRect = new Rectangle(e.CellBounds.X + padX, e.CellBounds.Y, e.CellBounds.Width - (padX * 2), e.CellBounds.Height);

                using (var f = new Font("Segoe UI", 8.8f, FontStyle.Bold))
                using (var bText = new SolidBrush(headerFg))
                {
                    g.DrawString(colHeader, f, bText, textRect, sf);
                }

                e.Handled = true;
                return;
            }

            // ─────────────────────────────────────────────────────────────────
            // 2. DATA ROW CELLS
            // Handled with full-row selection, capsule status pill badges, and ClearType text
            // ─────────────────────────────────────────────────────────────────
            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

            Color rowBg = isSelected
                ? (isDark ? Color.FromArgb(36, 46, 74) : Color.FromArgb(239, 246, 255))
                : ((e.RowIndex % 2 == 1)
                    ? (isDark ? Color.FromArgb(22, 25, 36) : Color.FromArgb(250, 250, 253))
                    : (isDark ? Color.FromArgb(28, 32, 44) : Color.White));

            // Background Fill
            using (var bBg = new SolidBrush(rowBg))
            {
                g.FillRectangle(bBg, e.CellBounds);
            }

            // Selected Row: 3.5px Left Accent Bar on Column 0
            if (isSelected && e.ColumnIndex == 0)
            {
                using var bAccent = new SolidBrush(ThemeManager.PrimaryAccent);
                g.FillRectangle(bAccent, e.CellBounds.Left, e.CellBounds.Top, 3, e.CellBounds.Height);
            }

            // Crisp horizontal hairline bottom row separator
            Color rowSepColor = isDark ? Color.FromArgb(40, 46, 68) : Color.FromArgb(226, 232, 240);
            using (var pSep = new Pen(rowSepColor, 1f))
            {
                g.DrawLine(pSep, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
            }

            string cellColName = dgv.Columns[e.ColumnIndex].Name;
            string rawVal = e.Value?.ToString() ?? string.Empty;

            // A. STATUS CAPSULE PILL BADGE
            if (cellColName.Contains("Status", StringComparison.OrdinalIgnoreCase))
            {
                PaintStatusPill(g, e.CellBounds, rawVal, isDark);
                e.Handled = true;
                return;
            }

            // B. ALERT LEVEL CAPSULE PILL BADGE (CRITICAL / LOW / NORMAL)
            if (cellColName.Contains("Alert", StringComparison.OrdinalIgnoreCase) || cellColName.Equals("Action", StringComparison.OrdinalIgnoreCase))
            {
                PaintAlertPill(g, e.CellBounds, rawVal, isDark);
                e.Handled = true;
                return;
            }

            // C. STANDARD DATA TEXT / FINANCIALS
            var cellAlign = dgv.Columns[e.ColumnIndex].DefaultCellStyle.Alignment;
            bool isRightAligned = (cellAlign == DataGridViewContentAlignment.MiddleRight) ||
                                  cellColName.Contains("Amount", StringComparison.OrdinalIgnoreCase) ||
                                  cellColName.Contains("Total", StringComparison.OrdinalIgnoreCase) ||
                                  cellColName.Contains("Paid", StringComparison.OrdinalIgnoreCase) ||
                                  cellColName.Contains("Balance", StringComparison.OrdinalIgnoreCase) ||
                                  cellColName.Contains("Price", StringComparison.OrdinalIgnoreCase) ||
                                  cellColName.Contains("Cost", StringComparison.OrdinalIgnoreCase);

            bool isCenterAligned = cellAlign == DataGridViewContentAlignment.MiddleCenter;

            var cellSf = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = isRightAligned ? StringAlignment.Far : (isCenterAligned ? StringAlignment.Center : StringAlignment.Near),
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            int cellPadX = 10;
            var cellTextRect = new Rectangle(e.CellBounds.X + cellPadX, e.CellBounds.Y, e.CellBounds.Width - (cellPadX * 2), e.CellBounds.Height);

            Color textCol = isSelected
                ? (isDark ? Color.White : Color.FromArgb(15, 23, 42))
                : ThemeManager.TextPrimary;

            bool isBold = isRightAligned && (cellColName.Contains("Amount", StringComparison.OrdinalIgnoreCase) || cellColName.Contains("Total", StringComparison.OrdinalIgnoreCase));
            using (var fText = new Font("Segoe UI", 9f, isBold ? FontStyle.Bold : FontStyle.Regular))
            using (var bText = new SolidBrush(textCol))
            {
                g.DrawString(rawVal, fText, bText, cellTextRect, cellSf);
            }

            e.Handled = true;
        }

        public static void PaintStatusPill(Graphics g, Rectangle cellBounds, string status, bool isDark)
        {
            if (string.IsNullOrWhiteSpace(status)) return;

            Color bgCol, borderCol, dotCol, textCol;
            string s = status.Trim();

            if (s.Equals("Paid", StringComparison.OrdinalIgnoreCase) || s.Equals("Settled", StringComparison.OrdinalIgnoreCase))
            {
                bgCol = isDark ? Color.FromArgb(6, 78, 59) : Color.FromArgb(236, 253, 245);
                borderCol = isDark ? Color.FromArgb(4, 120, 87) : Color.FromArgb(167, 243, 208);
                dotCol = Color.FromArgb(16, 185, 129);
                textCol = isDark ? Color.FromArgb(110, 231, 183) : Color.FromArgb(4, 120, 87);
            }
            else if (s.Equals("Partially Paid", StringComparison.OrdinalIgnoreCase) || s.Equals("Pending", StringComparison.OrdinalIgnoreCase) || s.Contains("Due", StringComparison.OrdinalIgnoreCase))
            {
                bgCol = isDark ? Color.FromArgb(120, 53, 15) : Color.FromArgb(254, 243, 199);
                borderCol = isDark ? Color.FromArgb(180, 83, 9) : Color.FromArgb(253, 230, 138);
                dotCol = Color.FromArgb(245, 158, 11);
                textCol = isDark ? Color.FromArgb(253, 230, 138) : Color.FromArgb(180, 83, 9);
            }
            else if (s.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) || s.Equals("Void", StringComparison.OrdinalIgnoreCase))
            {
                bgCol = isDark ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                borderCol = isDark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225);
                dotCol = Color.FromArgb(148, 163, 184);
                textCol = isDark ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);
            }
            else // Unpaid / Overdue
            {
                bgCol = isDark ? Color.FromArgb(127, 29, 29) : Color.FromArgb(254, 242, 242);
                borderCol = isDark ? Color.FromArgb(185, 28, 28) : Color.FromArgb(254, 202, 202);
                dotCol = Color.FromArgb(239, 68, 68);
                textCol = isDark ? Color.FromArgb(254, 202, 202) : Color.FromArgb(185, 28, 28);
            }

            int pillH = 20;
            int pillW = s.Equals("Partially Paid", StringComparison.OrdinalIgnoreCase) ? 96 : (s.Length > 7 ? 80 : 70);
            int pillX = cellBounds.X + (cellBounds.Width - pillW) / 2;
            int pillY = cellBounds.Y + (cellBounds.Height - pillH) / 2;

            using (var path = GraphicsExtensions.CreateRoundedRectanglePath(pillX, pillY, pillW, pillH, 10))
            using (var bBg = new SolidBrush(bgCol))
            using (var pBdr = new Pen(borderCol, 1f))
            {
                g.FillPath(bBg, path);
                g.DrawPath(pBdr, path);
            }

            // Dot indicator
            using (var bDot = new SolidBrush(dotCol))
            {
                g.FillEllipse(bDot, pillX + 8, pillY + (pillH - 5) / 2, 5, 5);
            }

            // Text
            using (var f = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var bText = new SolidBrush(textCol))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var textRect = new Rectangle(pillX + 13, pillY, pillW - 14, pillH);
                g.DrawString(s, f, bText, textRect, sf);
            }
        }

        public static void PaintAlertPill(Graphics g, Rectangle cellBounds, string alertLevel, bool isDark)
        {
            if (string.IsNullOrWhiteSpace(alertLevel)) return;

            Color bgCol, borderCol, dotCol, textCol;
            string s = alertLevel.Trim().ToUpperInvariant();

            if (s == "CRITICAL" || s == "OUT OF STOCK")
            {
                bgCol = isDark ? Color.FromArgb(69, 10, 10) : Color.FromArgb(254, 226, 226);
                borderCol = isDark ? Color.FromArgb(153, 27, 27) : Color.FromArgb(252, 165, 165);
                dotCol = Color.FromArgb(220, 38, 38);
                textCol = isDark ? Color.FromArgb(252, 165, 165) : Color.FromArgb(153, 27, 27);
            }
            else if (s == "LOW" || s == "REORDER")
            {
                bgCol = isDark ? Color.FromArgb(69, 26, 3) : Color.FromArgb(254, 243, 199);
                borderCol = isDark ? Color.FromArgb(180, 83, 9) : Color.FromArgb(253, 230, 138);
                dotCol = Color.FromArgb(217, 119, 6);
                textCol = isDark ? Color.FromArgb(253, 230, 138) : Color.FromArgb(146, 64, 14);
            }
            else // NORMAL / HEALTHY / OK
            {
                bgCol = isDark ? Color.FromArgb(6, 78, 59) : Color.FromArgb(236, 253, 245);
                borderCol = isDark ? Color.FromArgb(4, 120, 87) : Color.FromArgb(167, 243, 208);
                dotCol = Color.FromArgb(16, 185, 129);
                textCol = isDark ? Color.FromArgb(110, 231, 183) : Color.FromArgb(4, 120, 87);
            }

            int pillH = 20;
            int pillW = s == "CRITICAL" ? 76 : (s == "LOW" ? 64 : 74);
            int pillX = cellBounds.X + (cellBounds.Width - pillW) / 2;
            int pillY = cellBounds.Y + (cellBounds.Height - pillH) / 2;

            using (var path = GraphicsExtensions.CreateRoundedRectanglePath(pillX, pillY, pillW, pillH, 10))
            using (var bBg = new SolidBrush(bgCol))
            using (var pBdr = new Pen(borderCol, 1f))
            {
                g.FillPath(bBg, path);
                g.DrawPath(pBdr, path);
            }

            // Dot indicator
            using (var bDot = new SolidBrush(dotCol))
            {
                g.FillEllipse(bDot, pillX + 7, pillY + (pillH - 5) / 2, 5, 5);
            }

            // Text
            using (var f = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var bText = new SolidBrush(textCol))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                var textRect = new Rectangle(pillX + 12, pillY, pillW - 13, pillH);
                g.DrawString(s, f, bText, textRect, sf);
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
