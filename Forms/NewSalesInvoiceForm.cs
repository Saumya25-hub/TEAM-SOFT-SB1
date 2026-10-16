using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Dedicated Modal Window hosting the New Sales Invoice page.
    /// Provides responsive layout, clean typography, barcode scanning,
    /// and seamless financial calculations matching the reference billing UI.
    /// </summary>
    public class NewSalesInvoiceForm : Form
    {
        private readonly NewSalesInvoiceControl _invoiceControl = new();

        public NewSalesInvoiceForm()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            InitializeComponentCustom();
            ApplyTheme();

            ThemeManager.ThemeChanged += ApplyTheme;
        }

        private void InitializeComponentCustom()
        {
            Text = "New Sales Invoice — S TOM'S";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1080, 720);
            var screenArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            int targetW = Math.Min(1440, (int)(screenArea.Width * 0.90f));
            int targetH = Math.Min(920, (int)(screenArea.Height * 0.90f));
            if (targetW < 1180) targetW = 1180;
            if (targetH < 780) targetH = 780;
            ClientSize = new Size(targetW, targetH);
            ShowIcon = false;
            ShowInTaskbar = false;

            _invoiceControl.Dock = DockStyle.Fill;
            _invoiceControl.InvoiceSaved += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            _invoiceControl.BackRequested += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            _invoiceControl.CancelRequested += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(_invoiceControl);
        }

        public void ResetForm()
        {
            _invoiceControl.ResetForm();
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = isDark ? Color.FromArgb(17, 20, 29) : Color.FromArgb(248, 250, 252);
        }

        /// <summary>
        /// Displays the New Sales Invoice page modal safely centered over any parent form.
        /// </summary>
        public static DialogResult ShowInvoice(IWin32Window? owner, Action? onInvoiceSaved = null)
        {
            using var form = new NewSalesInvoiceForm();
            form.ResetForm();
            var result = owner != null ? form.ShowDialog(owner) : form.ShowDialog();
            if (result == DialogResult.OK)
            {
                onInvoiceSaved?.Invoke();
            }
            return result;
        }
    }
}
