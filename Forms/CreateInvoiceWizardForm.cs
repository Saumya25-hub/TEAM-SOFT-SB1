using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Dedicated Modal Window hosting the Three-Step Create Invoice Wizard.
    /// Completely isolates the invoice creation workflow from dashboard transaction cards.
    /// Opens centered over the application window with smooth docking and responsive layout.
    /// </summary>
    public class CreateInvoiceWizardForm : Form
    {
        private readonly CreateInvoiceWizardControl _wizardControl = new();

        public Invoice? SavedInvoice => _wizardControl.LastSavedInvoice;

        public CreateInvoiceWizardForm()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            InitializeComponent();
            ApplyTheme();

            ThemeManager.ThemeChanged += ApplyTheme;
        }

        private void InitializeComponent()
        {
            Text = "New Sales Tax Invoice — Step-by-Step Creation Wizard";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 680);
            ClientSize = new Size(1080, 730);
            ShowIcon = false;
            ShowInTaskbar = false;

            _wizardControl.Dock = DockStyle.Fill;
            _wizardControl.InvoiceSaved += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            _wizardControl.CancelRequested += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(_wizardControl);
        }

        public void ResetWizard()
        {
            _wizardControl.ResetForm();
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode;
            BackColor = isDark ? Color.FromArgb(17, 20, 29) : Color.FromArgb(248, 250, 252);
            _wizardControl.ApplyTheme();
        }

        /// <summary>
        /// Helper to display the Create Invoice Wizard modal safely centered over any parent form.
        /// </summary>
        public static DialogResult ShowWizard(IWin32Window? owner, Action<Invoice?>? onInvoiceSaved = null)
        {
            using var wizardForm = new CreateInvoiceWizardForm();
            wizardForm.ResetWizard();
            var result = owner != null ? wizardForm.ShowDialog(owner) : wizardForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                onInvoiceSaved?.Invoke(wizardForm.SavedInvoice);
            }
            return result;
        }
    }
}
