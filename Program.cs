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

            using var splash = new Forms.SplashForm();
            Forms.LoginForm? loginForm = null;

            splash.SplashCompleted += (s, e) =>
            {
                loginForm = new Forms.LoginForm();
                splash.Close();
            };

            Application.Run(splash);

            if (loginForm != null)
            {
                loginForm.LoginSucceeded += (s, e) =>
                {
                    // Transition to SB1 Billing Dashboard
                    var dashboard = new Forms.DashboardForm();
                    loginForm.Hide();
                    dashboard.ShowDialog();
                    loginForm.Close();
                };

                Application.Run(loginForm);
            }
        }
    }
}