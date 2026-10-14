using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Premium WinForms Login Form for SB1 (Simple Billing).
    /// Features smooth fade-in entry animation, modern card styling,
    /// rounded gradient controls, and cohesive branding matching the Splash Screen.
    /// </summary>
    public partial class LoginForm : Form
    {
        private readonly System.Windows.Forms.Timer _entryFadeTimer;
        private float _fadeProgress = 0f;
        private int _slideOffsetY = 15; // Subtle slide-up entry effect

        // Dragging support for borderless window
        private bool _isDragging = false;
        private Point _dragCursorPoint;
        private Point _dragFormPoint;
        private bool _isEyeHovered = false;
        private bool _isAuthenticating = false;

        public event EventHandler? LoginSucceeded;

        /// <summary>
        /// Resets the login form state for subsequent logins after user logouts.
        /// </summary>
        public void ResetState()
        {
            _isAuthenticating = false;
            _fadeProgress = 0f;
            _slideOffsetY = 15;
            Opacity = 0.0;
            if (txtPass != null) txtPass.Text = string.Empty;
            if (btnSignIn != null) btnSignIn.Enabled = true;
            if (lblStatus != null) lblStatus.Text = string.Empty;
            _entryFadeTimer?.Start();
        }

        public LoginForm()
        {
            InitializeComponent();

            // Form properties
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(940, 640);
            BackColor = Color.White;
            DoubleBuffered = true;
            Opacity = 0.0; // Start hidden for smooth fade-in

            // Smooth entry animation (~16ms 60 FPS)
            _entryFadeTimer = new System.Windows.Forms.Timer
            {
                Interval = 16
            };
            _entryFadeTimer.Tick += OnEntryFadeTick;

            SetupWindowDrag();

            // Setup Password Eye Toggle Button
            btnTogglePassword.Paint += (s, e) =>
            {
                DrawEyeIcon(e.Graphics, btnTogglePassword.Width, btnTogglePassword.Height, !txtPass.UseSystemPasswordChar, _isEyeHovered);
            };

            btnTogglePassword.Click += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !txtPass.UseSystemPasswordChar;
                btnTogglePassword.Invalidate();
            };

            btnTogglePassword.MouseEnter += (s, e) =>
            {
                _isEyeHovered = true;
                btnTogglePassword.Invalidate();
            };

            btnTogglePassword.MouseLeave += (s, e) =>
            {
                _isEyeHovered = false;
                btnTogglePassword.Invalidate();
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyRoundedRegion(20);
            _entryFadeTimer.Start();
        }

        private void ApplyRoundedRegion(int radius)
        {
            using var path = GraphicsExtensions.CreateRoundedRectanglePath(0, 0, Width, Height, radius);
            Region = new Region(path);
        }

        private void OnEntryFadeTick(object? sender, EventArgs e)
        {
            _fadeProgress += 0.05f;
            if (_fadeProgress >= 1.0f)
            {
                _fadeProgress = 1.0f;
                _slideOffsetY = 0;
                Opacity = 1.0;
                _entryFadeTimer.Stop();
            }
            else
            {
                // Quadratic ease-out for slide
                float t = _fadeProgress;
                _slideOffsetY = (int)(15 * (1f - (t * (2f - t))));
                Opacity = _fadeProgress;
            }

            Invalidate();
        }

        private System.Windows.Forms.Timer? _exitFadeTimer;
        private float _exitFadeProgress = 1.0f;

        /// <summary>
        /// Executes a silky-smooth exit fade animation and closes the form upon completion.
        /// </summary>
        public void FadeOutAndClose(Action? onComplete = null)
        {
            _entryFadeTimer?.Stop();
            _exitFadeTimer?.Stop();
            _exitFadeTimer?.Dispose();

            _exitFadeProgress = 1.0f;
            _exitFadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _exitFadeTimer.Tick += (s, e) =>
            {
                _exitFadeProgress -= 0.08f;
                if (_exitFadeProgress <= 0f)
                {
                    _exitFadeProgress = 0f;
                    Opacity = 0.0;
                    _exitFadeTimer.Stop();
                    _exitFadeTimer.Dispose();
                    onComplete?.Invoke();
                    Close();
                }
                else
                {
                    // Smooth quadratic fade out
                    float t = _exitFadeProgress;
                    Opacity = Math.Clamp(t * t, 0.0, 1.0);
                }
            };
            _exitFadeTimer.Start();
        }

        private void SetupWindowDrag()
        {
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Y <= 40)
                {
                    _isDragging = true;
                    _dragCursorPoint = Cursor.Position;
                    _dragFormPoint = Location;
                }
            };

            MouseMove += (s, e) =>
            {
                if (_isDragging)
                {
                    Point diff = Point.Subtract(Cursor.Position, new Size(_dragCursorPoint));
                    Location = Point.Add(_dragFormPoint, new Size(diff));
                }
            };

            MouseUp += (s, e) => _isDragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 1. Subtle Minimal Outer Rounded Border
            using (var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1.5f))
            {
                g.DrawRoundedRectangle(borderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1, 20);
            }

            // 2. Central Layout Position (Enlarged breathing room)
            int cardWidth = 460;
            int cardHeight = 540;
            int cardX = (ClientSize.Width - cardWidth) / 2;
            int cardY = (ClientSize.Height - cardHeight) / 2 + _slideOffsetY;

            // 3. Central SB1 Emblem Badge (Enlarged)
            int emblemRadius = 38;
            int emblemCenterX = cardX + cardWidth / 2;
            int emblemCenterY = cardY + 48;

            // Outer subtle glow
            using (var glowPen = new Pen(Color.FromArgb(40, 59, 130, 246), 4.5f))
            {
                g.DrawEllipse(glowPen, emblemCenterX - emblemRadius - 3, emblemCenterY - emblemRadius - 3, (emblemRadius + 3) * 2, (emblemRadius + 3) * 2);
            }

            // Gradient Circle
            using (var gradientBrush = new LinearGradientBrush(
                new Point(emblemCenterX - emblemRadius, emblemCenterY - emblemRadius),
                new Point(emblemCenterX + emblemRadius, emblemCenterY + emblemRadius),
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(124, 58, 237)))
            {
                g.FillEllipse(gradientBrush, emblemCenterX - emblemRadius, emblemCenterY - emblemRadius, emblemRadius * 2, emblemRadius * 2);
            }

            // Monogram text "SB1" inside emblem
            using (var monogramFont = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("SB1", monogramFont, whiteBrush, emblemCenterX, emblemCenterY, sf);
            }

            // 4. Card Titles (Enlarged)
            int titleY = emblemCenterY + 48;
            using (var titleFont = new Font("Segoe UI", 21f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                g.DrawString("Welcome Back", titleFont, titleBrush, emblemCenterX, titleY, sf);
            }

            int subY = titleY + 35;
            using (var subFont = new Font("Segoe UI", 10.5f, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(100, 116, 139)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                g.DrawString("Sign in to your SB1 Simple Billing account", subFont, subBrush, emblemCenterX, subY, sf);
            }

            // Position input controls relative to card and animated slide
            UpdateControlPositions(cardX, cardY);
        }

        private void UpdateControlPositions(int cardX, int cardY)
        {
            if (pnlControls != null)
            {
                pnlControls.Location = new Point(cardX + 20, cardY + 175);
            }

            if (btnClose != null)
            {
                btnClose.Location = new Point(ClientSize.Width - 48, 12);
            }

            if (btnMinimize != null)
            {
                btnMinimize.Location = new Point(ClientSize.Width - 86, 12);
            }
        }

        /// <summary>
        /// Vector rendering of the Lucide / SVG Eye Open and Eye Closed icons with smooth anti-aliasing.
        /// </summary>
        private void DrawEyeIcon(Graphics g, int width, int height, bool isPasswordVisible, bool isHovered)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Center the 24x24 icon inside button
            float offsetX = (width - 24f) / 2f;
            float offsetY = (height - 24f) / 2f;

            GraphicsState state = g.Save();
            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(1.33333f, 1.33333f); // Match SVG transform="scale(1.33333)"

            Color iconColor = isHovered ? Color.FromArgb(59, 130, 246) : Color.FromArgb(148, 163, 184);
            using var pen = new Pen(iconColor, 1.5f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            if (isPasswordVisible)
            {
                // SVG 1: Eye Open (Password visible)
                using var path = new GraphicsPath();
                path.AddBezier(1.859f, 8f, 3.674f, 6.149f, 6.203f, 5f, 9f, 5f);
                path.AddBezier(9f, 5f, 11.797f, 5f, 14.326f, 6.148f, 16.141f, 8f);
                g.DrawPath(pen, path);

                // Center pupil circle: cx="9" cy="10.5" r="2.75"
                g.DrawEllipse(pen, 9f - 2.75f, 10.5f - 2.75f, 5.5f, 5.5f);

                // Eyelashes upward
                g.DrawLine(pen, 4.021f, 6.328f, 2.75f, 4.25f);
                g.DrawLine(pen, 7.3f, 5.144f, 6.823f, 2.769f);
                g.DrawLine(pen, 10.7f, 5.144f, 11.177f, 2.769f);
                g.DrawLine(pen, 13.979f, 6.328f, 15.25f, 4.25f);
            }
            else
            {
                // SVG 2: Eye Closed (Password hidden)
                using var path = new GraphicsPath();
                path.AddBezier(1.859f, 7.27f, 3.674f, 9.121f, 6.203f, 10.27f, 9f, 10.27f);
                path.AddBezier(9f, 10.27f, 11.797f, 10.27f, 14.326f, 9.122f, 16.141f, 7.27f);
                g.DrawPath(pen, path);

                // Eyelashes downward
                g.DrawLine(pen, 4.021f, 8.942f, 2.75f, 11.019f);
                g.DrawLine(pen, 7.3f, 10.126f, 6.823f, 12.5f);
                g.DrawLine(pen, 10.7f, 10.126f, 11.177f, 12.5f);
                g.DrawLine(pen, 13.979f, 8.942f, 15.25f, 11.019f);
            }

            g.Restore(state);
        }
    }
}
