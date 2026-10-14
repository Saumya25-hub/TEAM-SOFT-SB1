using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Premium WinForms Splash Screen for SB1 (Simple Billing).
    /// Features pure white background, animate__zoomIn circular logo reveal,
    /// dynamic glowing aura, and smooth startup progress loading.
    /// </summary>
    public partial class SplashForm : Form
    {
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _animationProgress = 0f; // 0.0 to 1.0 (entry bounce phase)
        private float _zoomScale = 0f;         // Calculated scale
        private float _loadProgress = 0f;      // 0.0 to 100.0 (progress bar)
        private float _glowAngle = 0f;          // Continuous rotating subtle aura
        private bool _isExiting = false;        // Full-screen zoom burst trigger
        private float _exitProgress = 0f;      // 0.0 to 1.0 (burst phase)
        private float _textAlphaMultiplier = 1f;

        public event EventHandler? SplashCompleted;

        public SplashForm()
        {
            InitializeComponent();

            // Form properties
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(760, 460);
            BackColor = Color.White;
            DoubleBuffered = true;
            ShowInTaskbar = true;

            // 60 FPS animation timer (~16ms)
            _animTimer = new System.Windows.Forms.Timer
            {
                Interval = 16
            };
            _animTimer.Tick += OnAnimationTick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedRegion(18);
            _animTimer.Start();
        }

        private void ApplyRoundedRegion(int radius)
        {
            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, Width, Height, radius);
            Region = new Region(path);
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            // Exit Phase: Super Zoom-In Blast across the entire screen
            if (_isExiting)
            {
                _exitProgress += 0.045f;
                if (_exitProgress >= 1.0f)
                {
                    _exitProgress = 1.0f;
                    _animTimer.Stop();
                    SplashCompleted?.Invoke(this, EventArgs.Empty);
                    return;
                }

                // Dramatic cubic acceleration for full-screen zoom-in burst
                float t = _exitProgress;
                _zoomScale = 1.0f + 18.0f * (t * t * t);

                // Quick fade-out for text, progress bar & aura
                _textAlphaMultiplier = Math.Clamp(1.0f - (t * 2.8f), 0f, 1f);

                Invalidate();
                return;
            }

            // 1. Entry Zoom-In Animation with bounce overshoot
            if (_animationProgress < 1.0f)
            {
                _animationProgress += 0.035f;
                if (_animationProgress > 1.0f) _animationProgress = 1.0f;

                // Ease Out Back interpolation: f(t) = 1 + c3*(t-1)^3 + c1*(t-1)^2
                float t = _animationProgress;
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                _zoomScale = 1f + c3 * MathF.Pow(t - 1f, 3) + c1 * MathF.Pow(t - 1f, 2);
            }
            else
            {
                _zoomScale = 1.0f;
            }

            // 2. Continuous subtle aura rotation
            _glowAngle += 2.5f;
            if (_glowAngle >= 360f) _glowAngle -= 360f;

            // 3. Progress Bar Fill
            if (_loadProgress < 100f)
            {
                _loadProgress += 1.25f;
                if (_loadProgress >= 100f)
                {
                    _loadProgress = 100f;
                    _isExiting = true; // Trigger the super full-screen animate__zoomIn burst!
                }
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int centerX = ClientSize.Width / 2;
            int centerY = ClientSize.Height / 2 - 25;

            // Subtle minimal outer rounded border
            using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1.5f))
            {
                g.DrawRoundedRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1, 18);
            }

            // Draw animated Zoomed Center Component
            if (_zoomScale > 0.01f)
            {
                GraphicsState state = g.Save();

                // Translate to center, apply scale, then draw
                g.TranslateTransform(centerX, centerY);
                g.ScaleTransform(_zoomScale, _zoomScale);

                DrawCenterBadge(g);

                g.Restore(state);
            }

            // Draw Typography below badge
            DrawBrandingText(g, centerX, centerY);

            // Draw Bottom Progress Bar & Version Tag
            DrawProgressBar(g);
        }

        private void DrawCenterBadge(Graphics g)
        {
            int baseRadius = 60; // 120px diameter

            // 1. Soft Outer Glowing Rings
            for (int r = baseRadius + 22; r >= baseRadius + 4; r -= 4)
            {
                int alpha = Math.Clamp((int)(18 * (1.0f - (r - baseRadius) / 25f)), 0, 30);
                using var glowPen = new Pen(Color.FromArgb(alpha, 59, 130, 246), 3.5f);
                g.DrawEllipse(glowPen, -r, -r, r * 2, r * 2);
            }

            // 2. Dynamic Accent Gradient Ring
            int ringRadius = baseRadius + 4;
            using (var gradientBrush = new LinearGradientBrush(
                new Point(-ringRadius, -ringRadius),
                new Point(ringRadius, ringRadius),
                Color.FromArgb(59, 130, 246),   // Royal Blue
                Color.FromArgb(124, 58, 237)))  // Electric Violet
            {
                using var ringPen = new Pen(gradientBrush, 4f);
                g.DrawEllipse(ringPen, -ringRadius, -ringRadius, ringRadius * 2, ringRadius * 2);
            }

            // 3. Inner White Circle Badge
            using (var fillBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(fillBrush, -baseRadius, -baseRadius, baseRadius * 2, baseRadius * 2);
            }

            using (var innerBorderPen = new Pen(Color.FromArgb(241, 245, 249), 2f))
            {
                g.DrawEllipse(innerBorderPen, -baseRadius, -baseRadius, baseRadius * 2, baseRadius * 2);
            }

            // 4. Stylized Geometric "SB1" Logo Monogram in the Center
            using (var logoFont = new Font("Segoe UI", 26f, FontStyle.Bold))
            using (var logoBrush = new LinearGradientBrush(
                new Point(-45, -20),
                new Point(45, 20),
                Color.FromArgb(37, 99, 235),    // Rich Blue
                Color.FromArgb(109, 40, 217)))  // Deep Violet
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("SB1", logoFont, logoBrush, 0, 0, sf);
            }
        }

        private void DrawBrandingText(Graphics g, int centerX, int centerY)
        {
            if (_textAlphaMultiplier <= 0.01f) return;

            float textAlpha = Math.Clamp(_animationProgress * _textAlphaMultiplier, 0f, 1f);
            int primaryAlpha = (int)(255 * textAlpha);
            int secondaryAlpha = (int)(180 * textAlpha);

            int titleY = centerY + 85;

            // Brand Title: SB1
            using (var titleFont = new Font("Segoe UI", 20f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(primaryAlpha, 30, 41, 59)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };
                g.DrawString("SB1", titleFont, titleBrush, centerX, titleY, sf);
            }

            // Subtitle: Simple Billing
            int subtitleY = titleY + 34;
            using (var subFont = new Font("Segoe UI", 11.5f, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(secondaryAlpha, 100, 116, 139)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };
                g.DrawString("Simple Billing", subFont, subBrush, centerX, subtitleY, sf);
            }
        }

        private void DrawProgressBar(Graphics g)
        {
            if (_textAlphaMultiplier <= 0.01f) return;

            int barWidth = 260;
            int barHeight = 4;
            int barX = (ClientSize.Width - barWidth) / 2;
            int barY = ClientSize.Height - 65;

            int alpha = (int)(255 * _textAlphaMultiplier);

            // Background Track (Soft Gray)
            using (var trackBrush = new SolidBrush(Color.FromArgb(alpha, 241, 245, 249)))
            {
                g.FillRoundedRectangle(trackBrush, barX, barY, barWidth, barHeight, 2);
            }

            // Filled Progress (Linear Gradient)
            int filledWidth = (int)((_loadProgress / 100f) * barWidth);
            if (filledWidth > 4)
            {
                using (var fillBrush = new LinearGradientBrush(
                    new Rectangle(barX, barY, barWidth, barHeight),
                    Color.FromArgb(alpha, 59, 130, 246),
                    Color.FromArgb(alpha, 124, 58, 237),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRoundedRectangle(fillBrush, barX, barY, filledWidth, barHeight, 2);
                }
            }

            // Version Label
            int textY = barY + 14;
            using (var verFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            using (var verBrush = new SolidBrush(Color.FromArgb((int)(160 * _textAlphaMultiplier), 160, 174, 192)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };
                g.DrawString("v1.0", verFont, verBrush, ClientSize.Width / 2, textY, sf);
            }
        }
    }

    /// <summary>
    /// Helper extension method for drawing smooth rounded rectangles in WinForms GDI+.
    /// </summary>
    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, int x, int y, int width, int height, int radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, int x, int y, int width, int height, int radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.DrawPath(pen, path);
        }

        internal static GraphicsPath CreateRoundedRectanglePath(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;

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
}
