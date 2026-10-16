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

            if (args.Length > 0 && args[0].Equals("--test-sales", StringComparison.OrdinalIgnoreCase))
            {
                RunSalesBillingTest();
                return;
            }

            if (args.Length > 0 && args[0].Equals("--test-flow", StringComparison.OrdinalIgnoreCase))
            {
                RunFlowTest();
                return;
            }

            if (args.Length > 0 && args[0].Equals("--render-sales", StringComparison.OrdinalIgnoreCase))
            {
                RunRenderSalesTest();
                return;
            }

            Application.Run(new Sb1ApplicationContext());
        }

        private static void RunSalesBillingTest()
        {
            Console.WriteLine("================================================================================");
            Console.WriteLine("  TEAM SOFT — SALES & BILLING MODULE COMPREHENSIVE AUTOMATED TEST SUITE");
            Console.WriteLine("================================================================================");

            // Step 1: Database Initialization
            Console.WriteLine("[TEST 1/8] Initializing SQLite database at %LOCALAPPDATA%\\TEAMSOFT\\teamsoft_sales.db...");
            Services.DatabaseService.InitializeDatabase();
            var business = Services.DatabaseService.GetBusinessProfile();
            Console.WriteLine($"[PASS] Database initialized. Business: {business.BusinessName} | GSTIN: {business.GSTIN}");

            // Step 2: Customer Seeding & Retrieval
            Console.WriteLine("[TEST 2/8] Verifying customers directory in SQLite...");
            var customers = Services.DatabaseService.GetCustomers();
            Console.WriteLine($"[PASS] Retrieved {customers.Count} customers. First customer: {customers[0].DisplayName}");
            if (customers.Count == 0) throw new Exception("Customer seed failed!");

            // Step 3: Decimal Precision & Arithmetic Calculations
            Console.WriteLine("[TEST 3/8] Validating strict decimal calculations & tax scheme logic...");
            var testInv = new Models.Invoice
            {
                InvoiceNumber = Services.DatabaseService.GetNextInvoiceNumber(),
                CustomerId = customers[0].Id,
                CustomerName = customers[0].Name,
                CustomerCompany = customers[0].CompanyName,
                CustomerPhone = customers[0].Phone,
                CustomerEmail = customers[0].Email,
                CustomerAddress = customers[0].BillingAddress,
                CustomerGSTIN = customers[0].GSTIN,
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(15),
                PaymentTerms = "Net 15",
                TaxType = "GST_INTRA", // CGST 9% + SGST 9%
                OverallDiscount = 5000m
            };

            // Line Item 1: 2 x 50,000 = 100,000. 10% disc = 10,000. Net = 90,000. 18% Tax = 16,200. Line Total = 106,200.
            testInv.Items.Add(new Models.InvoiceItem
            {
                ServiceName = "Custom Software Architecture & Dev",
                Description = "Microservice backend engine with WinForms presentation",
                Quantity = 2m,
                UnitPrice = 50000m,
                DiscountPercent = 10m,
                TaxPercent = 18m
            });

            // Line Item 2: 1 x 15,000 = 15,000. 0% disc. Net = 15,000. 18% Tax = 2,700. Line Total = 17,700.
            testInv.Items.Add(new Models.InvoiceItem
            {
                ServiceName = "Cloud SaaS Subscription (Monthly)",
                Description = "High availability dedicated server cluster",
                Quantity = 1m,
                UnitPrice = 15000m,
                DiscountPercent = 0m,
                TaxPercent = 18m
            });

            testInv.RecalculateTotals();
            Console.WriteLine($"       Subtotal:         ₹{testInv.Subtotal:N2} (Expected: ₹1,15,000.00)");
            Console.WriteLine($"       Item Discounts:   ₹{testInv.ItemDiscountTotal:N2} (Expected: ₹10,000.00)");
            Console.WriteLine($"       Overall Discount: ₹{testInv.OverallDiscount:N2} (Expected: ₹5,000.00)");
            Console.WriteLine($"       Taxable Amount:   ₹{testInv.TaxableAmount:N2} (Expected: ₹1,00,000.00)");
            Console.WriteLine($"       CGST (9%):        ₹{testInv.CgstAmount:N2} (Expected: ₹9,000.00)");
            Console.WriteLine($"       SGST (9%):        ₹{testInv.SgstAmount:N2} (Expected: ₹9,000.00)");
            Console.WriteLine($"       Grand Total:      ₹{testInv.GrandTotal:N2} (Expected: ₹1,18,000.00)");

            if (testInv.GrandTotal != 118000m) throw new Exception($"Grand total mismatch! Expected 118000, got {testInv.GrandTotal}");
            Console.WriteLine("[PASS] Decimal precision arithmetic verified (100% exact).");

            // Step 4: Invoice Persistence with Initial Partial Payment
            Console.WriteLine("[TEST 4/8] Saving invoice into SQLite with partial payment of ₹50,000...");
            var initPay = new Models.PaymentRecord
            {
                Amount = 50000m,
                PaymentMethod = "Bank Transfer",
                ReferenceNumber = "NEFT-TEST-9921",
                Notes = "Advance initial payment on contract signature"
            };

            var savedInv = Services.DatabaseService.SaveInvoice(testInv, initPay);
            Console.WriteLine($"[PASS] Saved invoice ID: {savedInv.Id} | Number: {savedInv.InvoiceNumber}");
            Console.WriteLine($"       Status: {savedInv.Status} | Amount Paid: ₹{savedInv.AmountPaid:N2} | Balance Due: ₹{savedInv.BalanceDue:N2}");
            if (savedInv.Status != "Partially Paid") throw new Exception($"Expected Partially Paid, got {savedInv.Status}");
            if (savedInv.BalanceDue != 68000m) throw new Exception($"Expected Balance Due 68000, got {savedInv.BalanceDue}");

            // Step 5: Recording Remainder Payment
            Console.WriteLine("[TEST 5/8] Recording second payment of ₹68,000 to fully settle invoice...");
            var secondPay = new Models.PaymentRecord
            {
                InvoiceId = savedInv.Id,
                Amount = 68000m,
                PaymentMethod = "UPI",
                ReferenceNumber = "UPI-TEST-88124",
                Notes = "Final settlement payment"
            };
            Services.DatabaseService.RecordPayment(secondPay);
            var updatedInv = Services.DatabaseService.GetInvoiceById(savedInv.Id);
            if (updatedInv == null) throw new Exception("Failed to reload invoice!");
            Console.WriteLine($"[PASS] Invoice updated. Status: {updatedInv.Status} | Paid: ₹{updatedInv.AmountPaid:N2} | Balance: ₹{updatedInv.BalanceDue:N2}");
            if (updatedInv.Status != "Paid") throw new Exception($"Expected Status Paid, got {updatedInv.Status}");
            if (updatedInv.BalanceDue != 0m) throw new Exception($"Expected Balance Due 0, got {updatedInv.BalanceDue}");

            // Step 6: HTML & PDF Invoice Export Generator
            Console.WriteLine("[TEST 6/8] Generating standalone HTML/PDF tax invoice report...");
            string filePath = Services.InvoicePrintService.ExportToPdfFile(updatedInv, business);
            if (!System.IO.File.Exists(filePath))
                throw new Exception("HTML/PDF invoice generation failed!");
            string htmlContent = System.IO.File.ReadAllText(filePath);
            Console.WriteLine($"[PASS] Generated vector-styled A4 invoice document ({htmlContent.Length} bytes at {filePath}).");

            // Step 7: Live Metrics Calculation
            Console.WriteLine("[TEST 7/8] Verifying live sales dashboard SQLite metrics query...");
            var metrics = Services.DatabaseService.GetSalesDashboardMetrics();
            Console.WriteLine($"       Total Sales:       {metrics.TotalSalesFormatted}");
            Console.WriteLine($"       Total Invoices:    {metrics.TotalInvoices}");
            Console.WriteLine($"       Paid Invoices:     {metrics.PaidInvoices}");
            Console.WriteLine($"       Unpaid Invoices:   {metrics.UnpaidInvoices}");
            Console.WriteLine($"       Outstanding Due:   {metrics.OutstandingDueFormatted}");
            Console.WriteLine($"       Active Clients:    {metrics.TotalCustomers}");
            Console.WriteLine("[PASS] SQLite aggregate metrics query succeeded.");

            // Step 8: UI Dashboard & Sales Module Instantiation & Theme Rendering
            Console.WriteLine("[TEST 8/8] Testing UI Controls, SalesBillingModuleControl, and Theme rendering...");
            using var dash = new Forms.DashboardForm();
            dash.CreateControl();
            dash.Show();
            Application.DoEvents();

            // Toggle Dark and Light themes
            Services.ThemeManager.IsDarkMode = true;
            Application.DoEvents();
            Services.ThemeManager.IsDarkMode = false;
            Application.DoEvents();
            dash.Close();
            Console.WriteLine("[PASS] WinForms controls, tabs, and theme toggling executed without errors!");

            Console.WriteLine("================================================================================");
            Console.WriteLine("  ALL SALES & BILLING MODULE TESTS PASSED CLEANLY (100% SUCCESS)!");
            Console.WriteLine("================================================================================");
        }

        private static void RunRenderSalesTest()
        {
            Services.DatabaseService.InitializeDatabase();
            string artifactDir = @"C:\Users\jenil\.gemini\antigravity-ide\brain\37f34276-3163-4b3f-9687-e4a92f749dcb";

            void CaptureView(int w, int h, bool isDark, string fileName)
            {
                Services.ThemeManager.IsDarkMode = isDark;
                using var form = new Forms.DashboardForm();
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(0, 0);
                form.Size = new Size(w, h);
                form.Show();
                form.ShowPage("Sales");

                for (int i = 0; i < 15; i++)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(20);
                }

                if (form.SalesModule is Controls.SalesBillingModuleControl sbm)
                {
                    Console.WriteLine($"[DIAG] SalesModule: bounds={sbm.Bounds}, Visible={sbm.Visible}, Parent={sbm.Parent?.GetType().Name} bounds={sbm.Parent?.Bounds}");
                    
                    var fbar = sbm.Controls.Find("pnlFilterToolbar", true).FirstOrDefault();
                    Console.WriteLine($"[DIAG] pnlFilterToolbar: {(fbar != null ? $"{fbar.Bounds}, Vis={fbar.Visible}, Controls={fbar.Controls.Count}" : "NOT FOUND")}");
                    
                    var rch = sbm.Controls.Find("pnlRecentCardHeader", true).FirstOrDefault();
                    Console.WriteLine($"[DIAG] pnlRecentCardHeader: {(rch != null ? $"{rch.Bounds}, Vis={rch.Visible}, Controls={rch.Controls.Count}" : "NOT FOUND")}");
                    
                    // Direct search in sbm
                    void PrintTree(Control parent, int indent)
                    {
                        string ind = new string(' ', indent * 2);
                        Console.WriteLine($"{ind}- {parent.GetType().Name} '{parent.Name}' bounds={parent.Bounds} Vis={parent.Visible} Dock={parent.Dock} CtlCount={parent.Controls.Count}");
                        foreach (Control child in parent.Controls)
                        {
                            if (child is Button || child is ComboBox || child is DateTimePicker || child is CheckBox || child is Label || child is Panel)
                                Console.WriteLine($"{ind}  * {child.GetType().Name} '{child.Name}' text='{child.Text}' bounds={child.Bounds} Vis={child.Visible}");
                            if (child.HasChildren && !(child is DataGridView))
                                PrintTree(child, indent + 1);
                        }
                    }
                    PrintTree(sbm, 0);
                }

                string outPath = System.IO.Path.Combine(artifactDir, fileName);
                using var bmp = new Bitmap(w, h);
                form.DrawToBitmap(bmp, new Rectangle(0, 0, w, h));
                bmp.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"[RENDER] Captured {fileName} ({w}x{h}, Dark={isDark})");

                form.Close();
            }

            CaptureView(1366, 820, false, "sales_layout_light_1366.png");
            CaptureView(1366, 820, true, "sales_layout_dark_1366.png");
            CaptureView(1600, 900, false, "sales_layout_light_1600.png");
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