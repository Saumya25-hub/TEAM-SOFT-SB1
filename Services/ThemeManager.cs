using System;
using System.Drawing;
using System.IO;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Centralized Theme System for TEAM SOFT Windows Forms (.NET 8).
    /// Provides strict color tokens for Light (#F5F6FA) and Dark (#111218) modes.
    /// Supports dynamic theme switching and persistence.
    /// </summary>
    public static class ThemeManager
    {
        private static bool _isDarkMode = false;
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TEAMSOFT", "theme.config");

        public static event Action? ThemeChanged;

        static ThemeManager()
        {
            LoadSavedTheme();
        }

        public static bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    SaveTheme();
                    ThemeChanged?.Invoke();
                }
            }
        }

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        private static void LoadSavedTheme()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var text = File.ReadAllText(SettingsFilePath).Trim();
                    _isDarkMode = text.Equals("dark", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                _isDarkMode = false;
            }
        }

        private static void SaveTheme()
        {
            var isDark = _isDarkMode;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var dir = Path.GetDirectoryName(SettingsFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(SettingsFilePath, isDark ? "dark" : "light");
                }
                catch
                {
                    // Ignore background persistence errors
                }
            });
        }

        // ═══════════════════════════════════════════════════════════════════════════
        //  COLOR TOKENS (MASTER_UI_UX_SPEC.md)
        // ═══════════════════════════════════════════════════════════════════════════

        public static Color WindowBackground => _isDarkMode
            ? Color.FromArgb(17, 18, 24)
            : Color.FromArgb(245, 246, 250);

        public static Color SidebarBackground => _isDarkMode
            ? Color.FromArgb(25, 26, 35)
            : Color.FromArgb(255, 255, 255);

        public static Color CardBackground => _isDarkMode
            ? Color.FromArgb(30, 32, 43)
            : Color.FromArgb(255, 255, 255);

        public static Color CardBorder => _isDarkMode
            ? Color.FromArgb(48, 50, 62)
            : Color.FromArgb(231, 233, 239);

        public static Color TextPrimary => _isDarkMode
            ? Color.FromArgb(244, 245, 248)
            : Color.FromArgb(32, 35, 43);

        public static Color TextSecondary => _isDarkMode
            ? Color.FromArgb(161, 165, 179)
            : Color.FromArgb(123, 129, 144);

        public static Color PrimaryAccent => _isDarkMode
            ? Color.FromArgb(102, 135, 255)
            : Color.FromArgb(65, 105, 245);

        public static Color AccentHover => _isDarkMode
            ? Color.FromArgb(124, 155, 255)
            : Color.FromArgb(50, 85, 209);

        public static Color NavHover => _isDarkMode
            ? Color.FromArgb(37, 40, 54)
            : Color.FromArgb(240, 243, 252);

        public static Color NavActive => _isDarkMode
            ? Color.FromArgb(42, 51, 88)
            : Color.FromArgb(233, 239, 255);

        public static Color StatusPositive => _isDarkMode
            ? Color.FromArgb(53, 201, 135)
            : Color.FromArgb(36, 163, 106);

        public static Color StatusWarning => _isDarkMode
            ? Color.FromArgb(240, 182, 77)
            : Color.FromArgb(231, 165, 42);

        public static Color StatusNegative => _isDarkMode
            ? Color.FromArgb(241, 107, 114)
            : Color.FromArgb(224, 82, 82);

        public static Color GridRowAlternate => _isDarkMode
            ? Color.FromArgb(22, 24, 34)
            : Color.FromArgb(250, 250, 252);

        public static Color InputBackground => _isDarkMode
            ? Color.FromArgb(25, 27, 37)
            : Color.FromArgb(255, 255, 255);

        public static Color InputBorder => _isDarkMode
            ? Color.FromArgb(55, 58, 73)
            : Color.FromArgb(220, 224, 233);
    }
}
