using System;
using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class ManageBrokersDialog : Form
    {
        private readonly TextBox _txtName = new();
        private readonly TextBox _txtPhone = new();
        private readonly NumericUpDown _nudCommission = new();
        private readonly TeamPillButton _btnAdd = new();
        private readonly DataGridView _dgvBrokers = new();
        private readonly TeamPillButton _btnClose = new();

        public Broker? LastAddedBroker { get; private set; }

        public ManageBrokersDialog()
        {
            InitializeComponentCustom();
            LoadBrokers();
        }

        private void InitializeComponentCustom()
        {
            Text = "Manage Brokers — S TOM'S";
            Size = new Size(650, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(20, 24, 36) : Color.FromArgb(248, 250, 252);

            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(20, 14, 20, 0),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(26, 31, 46) : Color.White
            };
            var lblTitle = new Label
            {
                Text = "Manage Commission Brokers",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ThemeManager.IsDarkMode ? Color.White : Color.FromArgb(15, 23, 42),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            // Add broker panel
            var pnlAdd = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(24, 28, 42) : Color.White
            };

            var lblName = new Label { Text = "Broker Name *", Bounds = new Rectangle(20, 12, 180, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
            _txtName.SetBounds(20, 32, 180, 28);
            _txtName.Font = new Font("Segoe UI", 9.5f);

            var lblPhone = new Label { Text = "Phone", Bounds = new Rectangle(210, 12, 130, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
            _txtPhone.SetBounds(210, 32, 130, 28);
            _txtPhone.Font = new Font("Segoe UI", 9.5f);

            var lblComm = new Label { Text = "Comm. %", Bounds = new Rectangle(350, 12, 90, 18), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
            _nudCommission.SetBounds(350, 32, 90, 28);
            _nudCommission.Font = new Font("Segoe UI", 9.5f);
            _nudCommission.DecimalPlaces = 2;
            _nudCommission.Maximum = 100m;

            _btnAdd.Text = "+ Add Broker";
            _btnAdd.SetBounds(455, 30, 150, 32);
            _btnAdd.FillColor = Color.FromArgb(37, 99, 235);
            _btnAdd.ForeColor = Color.White;
            _btnAdd.Click += OnAddBrokerClicked;

            pnlAdd.Controls.AddRange(new Control[] { lblName, _txtName, lblPhone, _txtPhone, lblComm, _nudCommission, _btnAdd });

            // Grid
            _dgvBrokers.Dock = DockStyle.Fill;
            _dgvBrokers.BackgroundColor = ThemeManager.IsDarkMode ? Color.FromArgb(20, 24, 36) : Color.White;
            _dgvBrokers.BorderStyle = BorderStyle.None;
            _dgvBrokers.RowHeadersVisible = false;
            _dgvBrokers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvBrokers.AllowUserToAddRows = false;
            _dgvBrokers.ReadOnly = true;
            _dgvBrokers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgvBrokers.RowTemplate.Height = 34;

            _dgvBrokers.Columns.Add("Id", "ID");
            _dgvBrokers.Columns["Id"].Width = 50;
            _dgvBrokers.Columns.Add("Name", "Broker Name");
            _dgvBrokers.Columns.Add("Phone", "Phone");
            _dgvBrokers.Columns.Add("Commission", "Commission %");
            _dgvBrokers.Columns.Add("CreatedAt", "Registered On");

            // Bottom panel
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(26, 31, 46) : Color.FromArgb(241, 245, 249)
            };
            _btnClose.Text = "Close";
            _btnClose.SetBounds(505, 12, 100, 32);
            _btnClose.FillColor = Color.FromArgb(100, 116, 139);
            _btnClose.ForeColor = Color.White;
            _btnClose.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            pnlBottom.Controls.Add(_btnClose);

            Controls.Add(_dgvBrokers);
            Controls.Add(pnlAdd);
            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
        }

        private void LoadBrokers()
        {
            _dgvBrokers.Rows.Clear();
            var brokers = DatabaseService.GetBrokers();
            foreach (var b in brokers)
            {
                _dgvBrokers.Rows.Add(b.Id, b.Name, b.Phone, $"{b.CommissionPercent:F2}%", b.CreatedAt.ToString("dd-MM-yyyy"));
            }
        }

        private void OnAddBrokerClicked(object? sender, EventArgs e)
        {
            string name = _txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid broker name.", "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return;
            }

            var broker = new Broker
            {
                Name = name,
                Phone = _txtPhone.Text.Trim(),
                CommissionPercent = _nudCommission.Value,
                CreatedAt = DateTime.Now
            };

            DatabaseService.SaveBroker(broker);
            LastAddedBroker = broker;
            _txtName.Clear();
            _txtPhone.Clear();
            _nudCommission.Value = 0m;
            LoadBrokers();
            MessageBox.Show($"Broker '{broker.Name}' registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
