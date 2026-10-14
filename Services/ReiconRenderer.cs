using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Reicon — Open Source SVG Icon Library Renderer for Windows Forms (.NET 8).
    /// Official Source: github.com/dqev/reicon (MIT License)
    /// Renders pixel-perfect 24x24 vector outline icons directly using anti-aliased GDI+ vector paths.
    /// Fully resolution-independent and DPI-scalable (100%, 125%, 150%).
    /// </summary>
    public static class ReiconRenderer
    {
        public enum IconType
        {
            Dashboard,
            Statistics,
            Customers,
            Suppliers,
            Products,
            Purchases,
            Sales,
            Transactions,
            Messages,
            Security,
            Settings,
            Logout,
            Search,
            Calendar,
            Notification,
            Sun,
            Moon,
            Plus,
            TrendingUp,
            TrendingDown,
            Warning,
            Wallet,
            Coins,
            Package,
            Receipt,
            Close,
            Minimize,
            Maximize
        }

        /// <summary>
        /// Draws a Reicon vector icon cleanly inside the target rectangle with the specified color.
        /// </summary>
        public static void DrawIcon(Graphics g, IconType icon, Rectangle bounds, Color color, float strokeWidth = 1.6f)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            var prevSmoothing = g.SmoothingMode;
            var prevPixelOffset = g.PixelOffsetMode;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Normalize coordinate system to a 24x24 viewBox
            float scale = Math.Min(bounds.Width / 24f, bounds.Height / 24f);
            float offsetX = bounds.X + (bounds.Width - 24f * scale) / 2f;
            float offsetY = bounds.Y + (bounds.Height - 24f * scale) / 2f;

            var state = g.Save();
            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(scale, scale);

            using var pen = new Pen(color, strokeWidth / scale)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using var brush = new SolidBrush(color);

            switch (icon)
            {
                case IconType.Dashboard:
                    // 2x2 rounded squares
                    DrawRoundedRect(g, pen, 3, 3, 7.5f, 7.5f, 2f);
                    DrawRoundedRect(g, pen, 13.5f, 3, 7.5f, 7.5f, 2f);
                    DrawRoundedRect(g, pen, 3, 13.5f, 7.5f, 7.5f, 2f);
                    DrawRoundedRect(g, pen, 13.5f, 13.5f, 7.5f, 7.5f, 2f);
                    break;

                case IconType.Statistics:
                    // Analytics chart with trend line
                    g.DrawLine(pen, 3, 21, 21, 21);
                    g.DrawLine(pen, 6, 17, 6, 13);
                    g.DrawLine(pen, 11, 17, 11, 9);
                    g.DrawLine(pen, 16, 17, 16, 6);
                    // Trend line with arrow
                    g.DrawLine(pen, 5, 11, 10, 7);
                    g.DrawLine(pen, 10, 7, 15, 9);
                    g.DrawLine(pen, 15, 9, 20, 4);
                    g.DrawLine(pen, 17, 4, 20, 4);
                    g.DrawLine(pen, 20, 4, 20, 7);
                    break;

                case IconType.Customers:
                    // User silhouette
                    g.DrawEllipse(pen, 8, 3, 8, 8);
                    using (var p = new GraphicsPath())
                    {
                        p.AddArc(4, 14, 16, 12, 180, 180);
                        g.DrawPath(pen, p);
                    }
                    break;

                case IconType.Suppliers:
                    // Warehouse / Factory
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(3, 20, 3, 9);
                        p.AddLine(3, 9, 8, 5);
                        p.AddLine(8, 5, 13, 9);
                        p.AddLine(13, 9, 18, 5);
                        p.AddLine(18, 5, 21, 8);
                        p.AddLine(21, 8, 21, 20);
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, 8, 14, 8, 20);
                    g.DrawLine(pen, 16, 14, 16, 20);
                    break;

                case IconType.Products:
                case IconType.Package:
                    // 3D Box / Package
                    using (var p = new GraphicsPath())
                    {
                        p.AddPolygon(new[] {
                            new PointF(12, 2.5f),
                            new PointF(21, 7.5f),
                            new PointF(21, 16.5f),
                            new PointF(12, 21.5f),
                            new PointF(3, 16.5f),
                            new PointF(3, 7.5f)
                        });
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, 12, 2.5f, 12, 12);
                    g.DrawLine(pen, 12, 12, 21, 7.5f);
                    g.DrawLine(pen, 12, 12, 3, 7.5f);
                    g.DrawLine(pen, 12, 12, 12, 21.5f);
                    break;

                case IconType.Purchases:
                    // Tray with downward receiving arrow
                    g.DrawLine(pen, 4, 13, 8, 13);
                    g.DrawLine(pen, 8, 13, 10, 16);
                    g.DrawLine(pen, 10, 16, 14, 16);
                    g.DrawLine(pen, 14, 16, 16, 13);
                    g.DrawLine(pen, 16, 13, 20, 13);
                    g.DrawLine(pen, 20, 13, 20, 19);
                    g.DrawLine(pen, 20, 19, 4, 19);
                    g.DrawLine(pen, 4, 19, 4, 13);
                    // Arrow down
                    g.DrawLine(pen, 12, 4, 12, 12);
                    g.DrawLine(pen, 9, 9, 12, 12);
                    g.DrawLine(pen, 15, 9, 12, 12);
                    break;

                case IconType.Sales:
                case IconType.Receipt:
                    // Receipt invoice voucher
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(5, 3, 19, 3);
                        p.AddLine(19, 3, 19, 21);
                        p.AddLine(19, 21, 16.5f, 19);
                        p.AddLine(16.5f, 19, 14, 21);
                        p.AddLine(14, 21, 11.5f, 19);
                        p.AddLine(11.5f, 19, 9, 21);
                        p.AddLine(9, 21, 6.5f, 19);
                        p.AddLine(6.5f, 19, 5, 21);
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, 8, 8, 16, 8);
                    g.DrawLine(pen, 8, 12, 14, 12);
                    g.DrawLine(pen, 8, 15, 12, 15);
                    break;

                case IconType.Transactions:
                    // Credit card / transfer
                    DrawRoundedRect(g, pen, 3, 5, 18, 14, 2.5f);
                    g.DrawLine(pen, 3, 10, 21, 10);
                    g.DrawLine(pen, 6, 15, 10, 15);
                    break;

                case IconType.Messages:
                case IconType.Notification:
                    // Bell outline
                    using (var p = new GraphicsPath())
                    {
                        p.AddArc(7, 6, 10, 10, 180, 180);
                        p.AddLine(17, 11, 19, 16);
                        p.AddLine(19, 16, 5, 16);
                        p.AddLine(5, 16, 7, 11);
                        g.DrawPath(pen, p);
                    }
                    g.DrawArc(pen, 10, 17, 4, 3, 0, 180);
                    break;

                case IconType.Security:
                    // Shield
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(12, 3, 20, 6);
                        p.AddLine(20, 6, 20, 13);
                        p.AddBezier(20, 13, 18, 18, 12, 21, 12, 21);
                        p.AddBezier(12, 21, 6, 18, 4, 13, 4, 13);
                        p.AddLine(4, 13, 4, 6);
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    // Inner keyhole or check
                    g.DrawLine(pen, 9, 12, 11, 14);
                    g.DrawLine(pen, 11, 14, 15, 9);
                    break;

                case IconType.Settings:
                    // 6-tooth gear
                    g.DrawEllipse(pen, 9, 9, 6, 6);
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = i * 60f;
                        float rad = angle * (float)Math.PI / 180f;
                        float cos = (float)Math.Cos(rad);
                        float sin = (float)Math.Sin(rad);
                        g.DrawLine(pen, 12 + cos * 5.5f, 12 + sin * 5.5f, 12 + cos * 8.5f, 12 + sin * 8.5f);
                    }
                    break;

                case IconType.Logout:
                    // Door with exit arrow
                    g.DrawLine(pen, 9, 3, 17, 3);
                    g.DrawLine(pen, 17, 3, 17, 21);
                    g.DrawLine(pen, 17, 21, 9, 21);
                    // Arrow pointing out
                    g.DrawLine(pen, 4, 12, 13, 12);
                    g.DrawLine(pen, 10, 9, 13, 12);
                    g.DrawLine(pen, 10, 15, 13, 12);
                    break;

                case IconType.Search:
                    // Magnifying glass
                    g.DrawEllipse(pen, 4, 4, 11, 11);
                    g.DrawLine(pen, 13, 13, 19.5f, 19.5f);
                    break;

                case IconType.Calendar:
                    // Calendar
                    DrawRoundedRect(g, pen, 3, 5, 18, 15, 2.5f);
                    g.DrawLine(pen, 3, 9.5f, 21, 9.5f);
                    g.DrawLine(pen, 8, 3, 8, 6.5f);
                    g.DrawLine(pen, 16, 3, 16, 6.5f);
                    // Dot
                    g.FillEllipse(brush, 7, 13, 2, 2);
                    g.FillEllipse(brush, 12, 13, 2, 2);
                    g.FillEllipse(brush, 16, 13, 2, 2);
                    break;

                case IconType.Sun:
                    // Sun
                    g.DrawEllipse(pen, 7.5f, 7.5f, 9, 9);
                    g.DrawLine(pen, 12, 2, 12, 5);
                    g.DrawLine(pen, 12, 19, 12, 22);
                    g.DrawLine(pen, 2, 12, 5, 12);
                    g.DrawLine(pen, 19, 12, 22, 12);
                    g.DrawLine(pen, 5, 5, 7.2f, 7.2f);
                    g.DrawLine(pen, 16.8f, 16.8f, 19, 19);
                    g.DrawLine(pen, 19, 5, 16.8f, 7.2f);
                    g.DrawLine(pen, 7.2f, 16.8f, 5, 19);
                    break;

                case IconType.Moon:
                    // Crescent Moon
                    using (var p = new GraphicsPath())
                    {
                        p.AddArc(4, 4, 16, 16, 90, 270);
                        p.AddArc(8, 4, 12, 16, 270, -180);
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    break;

                case IconType.Plus:
                    g.DrawLine(pen, 12, 5, 12, 19);
                    g.DrawLine(pen, 5, 12, 19, 12);
                    break;

                case IconType.TrendingUp:
                    g.DrawLine(pen, 4, 17, 10, 11);
                    g.DrawLine(pen, 10, 11, 14, 14);
                    g.DrawLine(pen, 14, 14, 20, 6);
                    g.DrawLine(pen, 15, 6, 20, 6);
                    g.DrawLine(pen, 20, 6, 20, 11);
                    break;

                case IconType.TrendingDown:
                    g.DrawLine(pen, 4, 7, 10, 13);
                    g.DrawLine(pen, 10, 13, 14, 10);
                    g.DrawLine(pen, 14, 10, 20, 18);
                    g.DrawLine(pen, 15, 18, 20, 18);
                    g.DrawLine(pen, 20, 18, 20, 13);
                    break;

                case IconType.Warning:
                    // Triangle alert
                    using (var p = new GraphicsPath())
                    {
                        p.AddLine(12, 3, 21, 19);
                        p.AddLine(21, 19, 3, 19);
                        p.CloseFigure();
                        g.DrawPath(pen, p);
                    }
                    g.DrawLine(pen, 12, 8, 12, 13);
                    g.FillEllipse(brush, 11.2f, 15.5f, 1.6f, 1.6f);
                    break;

                case IconType.Wallet:
                case IconType.Coins:
                    DrawRoundedRect(g, pen, 3, 6, 18, 13, 2.5f);
                    g.DrawLine(pen, 3, 10, 21, 10);
                    DrawRoundedRect(g, pen, 14, 11.5f, 5, 4, 1.5f);
                    break;

                case IconType.Close:
                    g.DrawLine(pen, 6, 6, 18, 18);
                    g.DrawLine(pen, 18, 6, 6, 18);
                    break;

                case IconType.Minimize:
                    g.DrawLine(pen, 5, 12, 19, 12);
                    break;

                case IconType.Maximize:
                    DrawRoundedRect(g, pen, 5, 5, 14, 14, 2);
                    break;
            }

            g.Restore(state);
            g.SmoothingMode = prevSmoothing;
            g.PixelOffsetMode = prevPixelOffset;
        }

        private static void DrawRoundedRect(Graphics g, Pen pen, float x, float y, float w, float h, float r)
        {
            using var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}
