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

            Application.Run(new Sb1ApplicationContext());
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

            // Instantiate dashboard with zero opacity for silky entrance
            _dashboardForm = new Forms.DashboardForm();
            _dashboardForm.FormClosed += (s, e) => ExitThread();

            // Smooth cross-fade: LoginForm fades out to 0 and closes, DashboardForm fades in to 1
            _dashboardForm.Show();
            _dashboardForm.FadeIn();

            _loginForm.FadeOutAndClose(() =>
            {
                _loginForm = null;
            });
        }
    }
}