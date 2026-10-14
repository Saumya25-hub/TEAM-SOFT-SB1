using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Controls
{
    /// <summary>
    /// Modern Anti-Aliased Pill & Rounded Button with Vector Icon support.
    /// Eliminates rectangular clipping and background bleed artifacts completely.
    /// </summary>
    public class TeamPillButton : Control
    {
        private bool _isHovered;
        private bool _isPressed;
        private int _borderRadius = 10;
        private Color _fillColor = Color.FromArgb(65, 105, 245);
        private Color _hoverColor = Color.FromArgb(50, 85, 220);
        private Color _pressColor = Color.FromArgb(40, 70, 190);
        private Color _textColor = Color.White;
        private Color _borderColor = Color.Transparent;
        private float _borderWidth = 0f;
        private ReiconRenderer.IconType? _icon = ReiconRenderer.IconType.Plus;
        private int _iconSize = 14;
        private int _iconSpacing = 8;
        private Color _iconColor = Color.White;

        public TeamPillButton()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Size = new Size(140, 36);
        }

        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; UpdateRegion(); Invalidate(); }
        }

        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; Invalidate(); }
        }

        public Color PressColor
        {
            get => _pressColor;
            set { _pressColor = value; Invalidate(); }
        }

        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; Invalidate(); }
        }

        public ReiconRenderer.IconType? Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        public int IconSize
        {
            get => _iconSize;
            set { _iconSize = value; Invalidate(); }
        }

        public int IconSpacing
        {
            get => _iconSpacing;
            set { _iconSpacing = value; Invalidate(); }
        }

        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (Width <= 0 || Height <= 0) return;
            int r = Math.Min(_borderRadius, Height / 2);
            using var path = CreateRoundedPath(0, 0, Width, Height, r);
            Region = new Region(path);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;
            if (w <= 0 || h <= 0) return;

            int r = Math.Min(_borderRadius, h / 2);
            using var path = CreateRoundedPath(0, 0, w - 1, h - 1, r);

            // Determine active background color
            Color currentBg = _isPressed ? _pressColor : (_isHovered ? _hoverColor : _fillColor);
            using (var brush = new SolidBrush(currentBg))
            {
                g.FillPath(brush, path);
            }

            // Draw border if configured
            if (_borderWidth > 0 && _borderColor != Color.Transparent)
            {
                using var pen = new Pen(_borderColor, _borderWidth);
                g.DrawPath(pen, path);
            }

            // Measure text & icon for exact combined center alignment
            SizeF textSize = SizeF.Empty;
            if (!string.IsNullOrEmpty(Text))
            {
                textSize = g.MeasureString(Text, Font);
            }

            int contentWidth = 0;
            if (_icon.HasValue)
            {
                contentWidth += _iconSize;
                if (!string.IsNullOrEmpty(Text))
                    contentWidth += _iconSpacing;
            }
            contentWidth += (int)Math.Ceiling(textSize.Width);

            int startX = Math.Max(4, (w - contentWidth) / 2);

            // Draw Icon
            if (_icon.HasValue)
            {
                int iconY = (h - _iconSize) / 2;
                var iconRect = new Rectangle(startX, iconY, _iconSize, _iconSize);
                ReiconRenderer.DrawIcon(g, _icon.Value, iconRect, _iconColor, 2.0f);
                startX += _iconSize + _iconSpacing;
            }

            // Draw Text
            if (!string.IsNullOrEmpty(Text))
            {
                int textY = (int)Math.Round((h - textSize.Height) / 2f);
                using var textBrush = new SolidBrush(_textColor);
                g.DrawString(Text, Font, textBrush, startX, textY);
            }
        }

        private static GraphicsPath CreateRoundedPath(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            if (diameter <= 0) diameter = 1;

            if (width < diameter) diameter = width;
            if (height < diameter) diameter = height;

            var bounds = new Rectangle(x, y, width, height);
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Dedicated Header Title & Subtitle control with guaranteed font metric calculations.
    /// Eliminates text clipping of descenders ('g', 'p', 'y', 'j', 'q') on any DPI.
    /// Also supports window dragging when clicked/dragged.
    /// </summary>
    public class TeamHeaderTitle : Control
    {
        private string _title = "Dashboard";
        private string _subtitle = "Real-time business performance";
        private Color _titleColor = Color.White;
        private Color _subtitleColor = Color.FromArgb(161, 165, 179);
        private Font _titleFont = new Font("Segoe UI", 15.5f, FontStyle.Bold);
        private Font _subtitleFont = new Font("Segoe UI", 9f, FontStyle.Regular);

        // Window drag integration
        private bool _isDragging;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;

        public TeamHeaderTitle()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Size = new Size(320, 52);
        }

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public string Subtitle
        {
            get => _subtitle;
            set { _subtitle = value; Invalidate(); }
        }

        public Color TitleColor
        {
            get => _titleColor;
            set { _titleColor = value; Invalidate(); }
        }

        public Color SubtitleColor
        {
            get => _subtitleColor;
            set { _subtitleColor = value; Invalidate(); }
        }

        public Font TitleFont
        {
            get => _titleFont;
            set { _titleFont = value; Invalidate(); }
        }

        public Font SubtitleFont
        {
            get => _subtitleFont;
            set { _subtitleFont = value; Invalidate(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && FindForm() is Form f)
            {
                _isDragging = true;
                _dragCursorPoint = Cursor.Position;
                _dragFormPoint = f.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && FindForm() is Form f)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
                f.Location = Point.Add(_dragFormPoint, new Size(diff));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isDragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Draw Title (Y = 2)
            using (var titleBrush = new SolidBrush(_titleColor))
            {
                g.DrawString(_title, _titleFont, titleBrush, 0, 2);
            }

            // Draw Subtitle (Y = 29, with ample room for full descenders)
            using (var subBrush = new SolidBrush(_subtitleColor))
            {
                g.DrawString(_subtitle, _subtitleFont, subBrush, 0, 29);
            }
        }
    }

    /// <summary>
    /// Subtle vertical hairline divider between action buttons and window controls.
    /// Provides visual structure and eliminates any accidental dead space.
    /// </summary>
    public class TeamVerticalDivider : Control
    {
        private Color _dividerColor = Color.FromArgb(48, 50, 62);

        public TeamVerticalDivider()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = Color.Transparent;
            Size = new Size(1, 26);
        }

        public Color DividerColor
        {
            get => _dividerColor;
            set { _dividerColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            using var pen = new Pen(_dividerColor, 1f);
            g.DrawLine(pen, 0, 0, 0, Height);
        }
    }

    /// <summary>
    /// Sleek, anti-aliased Window Chrome Control (Minimize, Maximize, Close).
    /// Perfectly aligned, zero corner clipping, modern hover animations.
    /// </summary>
    public class TeamWindowChrome : Control
    {
        private int _hoveredButton = -1; // 0 = Min, 1 = Max, 2 = Close
        private int _pressedButton = -1;
        private Color _iconColor = Color.FromArgb(161, 165, 179);
        private Color _hoverBg = Color.FromArgb(40, 43, 56);
        private Color _closeHoverBg = Color.FromArgb(239, 68, 68);

        public event EventHandler? MinimizeClicked;
        public event EventHandler? MaximizeClicked;
        public event EventHandler? CloseClicked;

        public TeamWindowChrome()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Size = new Size(108, 34);
            Cursor = Cursors.Hand;
        }

        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        public Color HoverBg
        {
            get => _hoverBg;
            set { _hoverBg = value; Invalidate(); }
        }

        public Color CloseHoverBg
        {
            get => _closeHoverBg;
            set { _closeHoverBg = value; Invalidate(); }
        }

        private Rectangle GetButtonRect(int index)
        {
            int btnW = 32;
            int btnH = 32;
            int gap = 4;
            int x = index * (btnW + gap);
            int y = (Height - btnH) / 2;
            return new Rectangle(x, y, btnW, btnH);
        }

        private int HitTest(Point pt)
        {
            for (int i = 0; i < 3; i++)
            {
                if (GetButtonRect(i).Contains(pt))
                    return i;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hit = HitTest(e.Location);
            if (hit != _hoveredButton)
            {
                _hoveredButton = hit;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredButton = -1;
            _pressedButton = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _pressedButton = HitTest(e.Location);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_pressedButton != -1 && _pressedButton == HitTest(e.Location))
            {
                switch (_pressedButton)
                {
                    case 0: MinimizeClicked?.Invoke(this, EventArgs.Empty); break;
                    case 1: MaximizeClicked?.Invoke(this, EventArgs.Empty); break;
                    case 2: CloseClicked?.Invoke(this, EventArgs.Empty); break;
                }
            }
            _pressedButton = -1;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            for (int i = 0; i < 3; i++)
            {
                var rect = GetButtonRect(i);
                bool isHovered = (_hoveredButton == i);
                bool isPressed = (_pressedButton == i);

                // Background hover pill
                if (isHovered || isPressed)
                {
                    Color bg = (i == 2) ? _closeHoverBg : _hoverBg;
                    if (isPressed && i != 2) bg = Color.FromArgb(55, 58, 73);
                    using var path = CreateRoundedPath(rect.X, rect.Y, rect.Width, rect.Height, 6);
                    using var brush = new SolidBrush(bg);
                    g.FillPath(brush, path);
                }

                // Icon color
                Color icColor = (isHovered && i == 2) ? Color.White : _iconColor;

                // Render vector icons
                var iconRect = new Rectangle(rect.X + (rect.Width - 14) / 2, rect.Y + (rect.Height - 14) / 2, 14, 14);
                switch (i)
                {
                    case 0:
                        ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Minimize, iconRect, icColor, 1.8f);
                        break;
                    case 1:
                        ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Maximize, iconRect, icColor, 1.6f);
                        break;
                    case 2:
                        ReiconRenderer.DrawIcon(g, ReiconRenderer.IconType.Close, iconRect, icColor, 1.8f);
                        break;
                }
            }
        }

        private static GraphicsPath CreateRoundedPath(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var bounds = new Rectangle(x, y, width, height);
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
