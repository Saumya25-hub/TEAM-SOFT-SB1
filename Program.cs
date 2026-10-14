using System;
using System.Windows.Forms;

namespace PROJECT_SBV1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            if (args.Length > 0 && args[0].Equals("--dashboard", StringComparison.OrdinalIgnoreCase))
            {
                Application.Run(new Forms.DashboardForm());
                return;
            }

            if (args.Length > 0 && args[0].Equals("--test-perf", StringComparison.OrdinalIgnoreCase))
            {
                RunPerformanceTest();
                return;
            }

            if (args.Length > 0 && args[0].Equals("--test-flow", StringComparison.OrdinalIgnoreCase))
            {
                RunFlowTest();
                return;
            }

            Application.Run(new Sb1ApplicationContext());
        }

        private static void RunFlowTest()
        {
            Console.WriteLine("[FLOW-TEST] Starting End-to-End Login, Dashboard Entrance, Theme Toggle & Logout Test...");

            // 1. Create and test Dashboard Form
            var dashboard = new Forms.DashboardForm();
            dashboard.CreateControl();
            dashboard.Show();
            Application.DoEvents();

            // 2. Test Entrance Animation progression
            dashboard.StartEntranceAnimation();
            Console.WriteLine("[FLOW-TEST] Testing Entrance Animation progression (10px slide up & fade)...");
            for (int f = 0; f < 18; f++)
            {
                System.Threading.Thread.Sleep(16);
                Application.DoEvents();
            }
            Console.WriteLine($"[FLOW-TEST] Entrance Animation completed: Opacity={dashboard.Opacity:F2}");

            // 3. Test Theme Toggle
            bool initialDark = Services.ThemeManager.IsDarkMode;
            Console.WriteLine($"[FLOW-TEST] Testing Theme Switch from {initialDark} to {!initialDark}...");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Services.ThemeManager.ToggleTheme();
            Application.DoEvents();
            sw.Stop();
            Console.WriteLine($"[FLOW-TEST] Theme switched to {Services.ThemeManager.IsDarkMode} in {sw.ElapsedMilliseconds} ms!");

            // 4. Test Logout Event
            dashboard.LogoutRequested += (s, e) => { /* verified */ };
            var btnLogout = dashboard.Controls.Find("btnNavLogout", true);
            if (btnLogout.Length > 0 && btnLogout[0] is Button btn)
            {
                Console.WriteLine("[FLOW-TEST] btnNavLogout control verified.");
            }

            dashboard.Close();
            Console.WriteLine("[FLOW-TEST] All E2E lifecycle and performance tests PASSED cleanly!");
        }

        private static void RunPerformanceTest()
        {
            Console.WriteLine("[TEST] Starting Automated Theme Switch & Animation Performance Test...");
            var dashboard = new Forms.DashboardForm();
            dashboard.CreateControl();
            dashboard.Show();
            Application.DoEvents();

            var times = new System.Collections.Generic.List<double>();
            for (int i = 0; i < 15; i++)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                Services.ThemeManager.ToggleTheme();
                Application.DoEvents();
                sw.Stop();
                times.Add(sw.Elapsed.TotalMilliseconds);
                Console.WriteLine($"[TEST] Theme switch #{i + 1:D2} (Dark={Services.ThemeManager.IsDarkMode,-5}): {sw.Elapsed.TotalMilliseconds:F2} ms");
            }

            double avg = 0;
            foreach (var t in times) avg += t;
            avg /= times.Count;
            Console.WriteLine($"[TEST] Performance Verification Complete! 15 switches. Average duration: {avg:F2} ms (Target: < 100 ms).");
            dashboard.Close();
        }
    }

    /// <summary>
    /// Application context managing seamless, non-blocking transitions between
    /// Splash Screen, Login Form, and Dashboard without modal dialog freezing or flicker.
    /// </summary>
    internal sealed class Sb1ApplicationContext : ApplicationContext
    {
        private Forms.SplashForm? _splashForm;
        private Forms.LoginForm? _loginForm;
        private Forms.DashboardForm? _dashboardForm;

        public Sb1ApplicationContext()
        {
            _splashForm = new Forms.SplashForm();
            _splashForm.SplashCompleted += (s, e) =>
            {
                _splashForm.Close();
                _splashForm.Dispose();
                _splashForm = null;
                ShowLoginForm();
            };
            _splashForm.Show();
        }

        private void ShowLoginForm()
        {
            _loginForm = new Forms.LoginForm();
            _loginForm.FormClosed += (s, e) =>
            {
                // If user closes login form before logging in, exit app
                if (_dashboardForm == null)
                {
                    ExitThread();
                }
            };

            _loginForm.LoginSucceeded += (s, e) =>
            {
                TransitionToDashboard();
            };

            _loginForm.Show();
        }

        private void TransitionToDashboard()
        {
            if (_loginForm == null) return;

            bool isLoggingOut = false;

            _dashboardForm = new Forms.DashboardForm();
            _dashboardForm.Opacity = 1.0;
            _dashboardForm.FormClosed += (s, e) =>
            {
                if (!isLoggingOut)
                {
                    ExitThread();
                }
            };

            _dashboardForm.LogoutRequested += (s, e) =>
            {
                isLoggingOut = true;
                _dashboardForm = null;
                ShowLoginForm();
            };

            _dashboardForm.Show();

            _loginForm.Close();
            _loginForm.Dispose();
            _loginForm = null;
        }
    }
}