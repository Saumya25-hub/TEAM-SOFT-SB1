namespace PROJECT_SBV1.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Panel pnlUserBorder;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.Button btnTogglePassword;
        private System.Windows.Forms.Panel pnlPassBorder;
        private System.Windows.Forms.CheckBox chkRemember;
        private System.Windows.Forms.LinkLabel lnkForgot;
        private System.Windows.Forms.Button btnSignIn;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnMinimize;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _entryFadeTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlControls = new System.Windows.Forms.Panel();
            this.lblUser = new System.Windows.Forms.Label();
            this.pnlUserBorder = new System.Windows.Forms.Panel();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.lblPass = new System.Windows.Forms.Label();
            this.pnlPassBorder = new System.Windows.Forms.Panel();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.btnTogglePassword = new System.Windows.Forms.Button();
            this.chkRemember = new System.Windows.Forms.CheckBox();
            this.lnkForgot = new System.Windows.Forms.LinkLabel();
            this.btnSignIn = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.pnlControls.SuspendLayout();
            this.pnlUserBorder.SuspendLayout();
            this.pnlPassBorder.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(165)))), ((int)(((byte)(165)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.btnClose.Location = new System.Drawing.Point(894, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(34, 30);
            this.btnClose.TabIndex = 99;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += (s, e) => Application.Exit();
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.btnMinimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(232)))), ((int)(((byte)(240)))));
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.btnMinimize.Location = new System.Drawing.Point(856, 12);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(34, 30);
            this.btnMinimize.TabIndex = 98;
            this.btnMinimize.Text = "—";
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += (s, e) => this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            // 
            // pnlControls
            // 
            this.pnlControls.BackColor = System.Drawing.Color.White;
            this.pnlControls.Controls.Add(this.lblStatus);
            this.pnlControls.Controls.Add(this.btnSignIn);
            this.pnlControls.Controls.Add(this.lnkForgot);
            this.pnlControls.Controls.Add(this.chkRemember);
            this.pnlControls.Controls.Add(this.pnlPassBorder);
            this.pnlControls.Controls.Add(this.lblPass);
            this.pnlControls.Controls.Add(this.pnlUserBorder);
            this.pnlControls.Controls.Add(this.lblUser);
            this.pnlControls.Location = new System.Drawing.Point(260, 175);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(420, 350);
            this.pnlControls.TabIndex = 0;
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblUser.Location = new System.Drawing.Point(2, 4);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(126, 19);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "Username or Email";
            // 
            // pnlUserBorder
            // 
            this.pnlUserBorder.BackColor = System.Drawing.Color.White;
            this.pnlUserBorder.Controls.Add(this.txtUser);
            this.pnlUserBorder.Location = new System.Drawing.Point(0, 28);
            this.pnlUserBorder.Name = "pnlUserBorder";
            this.pnlUserBorder.Padding = new System.Windows.Forms.Padding(14, 11, 14, 11);
            this.pnlUserBorder.Size = new System.Drawing.Size(420, 48);
            this.pnlUserBorder.TabIndex = 1;
            this.pnlUserBorder.Paint += (s, e) =>
            {
                using var borderPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1.5f);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawRoundedRectangle(borderPen, 0, 0, this.pnlUserBorder.Width - 1, this.pnlUserBorder.Height - 1, 9);
            };
            // 
            // txtUser
            // 
            this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUser.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtUser.Location = new System.Drawing.Point(14, 11);
            this.txtUser.Name = "txtUser";
            this.txtUser.PlaceholderText = "admin";
            this.txtUser.Size = new System.Drawing.Size(392, 22);
            this.txtUser.TabIndex = 0;
            this.txtUser.Text = "admin";
            // 
            // lblPass
            // 
            this.lblPass.AutoSize = true;
            this.lblPass.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblPass.Location = new System.Drawing.Point(2, 90);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(68, 19);
            this.lblPass.TabIndex = 2;
            this.lblPass.Text = "Password";
            // 
            // pnlPassBorder
            // 
            this.pnlPassBorder.BackColor = System.Drawing.Color.White;
            this.pnlPassBorder.Controls.Add(this.btnTogglePassword);
            this.pnlPassBorder.Controls.Add(this.txtPass);
            this.pnlPassBorder.Location = new System.Drawing.Point(0, 114);
            this.pnlPassBorder.Name = "pnlPassBorder";
            this.pnlPassBorder.Padding = new System.Windows.Forms.Padding(14, 11, 14, 11);
            this.pnlPassBorder.Size = new System.Drawing.Size(420, 48);
            this.pnlPassBorder.TabIndex = 3;
            this.pnlPassBorder.Paint += (s, e) =>
            {
                using var borderPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(226, 232, 240), 1.5f);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawRoundedRectangle(borderPen, 0, 0, this.pnlPassBorder.Width - 1, this.pnlPassBorder.Height - 1, 9);
            };
            // 
            // txtPass
            // 
            this.txtPass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPass.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtPass.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtPass.Location = new System.Drawing.Point(14, 13);
            this.txtPass.Name = "txtPass";
            this.txtPass.PlaceholderText = "••••••••";
            this.txtPass.Size = new System.Drawing.Size(350, 22);
            this.txtPass.TabIndex = 0;
            this.txtPass.Text = "admin123";
            this.txtPass.UseSystemPasswordChar = true;
            // 
            // btnTogglePassword
            // 
            this.btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTogglePassword.FlatAppearance.BorderSize = 0;
            this.btnTogglePassword.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTogglePassword.Location = new System.Drawing.Point(372, 8);
            this.btnTogglePassword.Name = "btnTogglePassword";
            this.btnTogglePassword.Size = new System.Drawing.Size(36, 32);
            this.btnTogglePassword.TabIndex = 1;
            this.btnTogglePassword.UseVisualStyleBackColor = false;
            // 
            // chkRemember
            // 
            this.chkRemember.AutoSize = true;
            this.chkRemember.Checked = true;
            this.chkRemember.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemember.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkRemember.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.chkRemember.Location = new System.Drawing.Point(2, 175);
            this.chkRemember.Name = "chkRemember";
            this.chkRemember.Size = new System.Drawing.Size(113, 21);
            this.chkRemember.TabIndex = 4;
            this.chkRemember.Text = "Remember me";
            this.chkRemember.UseVisualStyleBackColor = true;
            // 
            // lnkForgot
            // 
            this.lnkForgot.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lnkForgot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkForgot.AutoSize = true;
            this.lnkForgot.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lnkForgot.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkForgot.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.lnkForgot.Location = new System.Drawing.Point(305, 176);
            this.lnkForgot.Name = "lnkForgot";
            this.lnkForgot.Size = new System.Drawing.Size(113, 17);
            this.lnkForgot.TabIndex = 5;
            this.lnkForgot.TabStop = true;
            this.lnkForgot.Text = "Forgot password?";
            // 
            // btnSignIn
            // 
            this.btnSignIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(130)))), ((int)(((byte)(246)))));
            this.btnSignIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSignIn.FlatAppearance.BorderSize = 0;
            this.btnSignIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSignIn.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSignIn.ForeColor = System.Drawing.Color.White;
            this.btnSignIn.Location = new System.Drawing.Point(0, 215);
            this.btnSignIn.Name = "btnSignIn";
            this.btnSignIn.Size = new System.Drawing.Size(420, 52);
            this.btnSignIn.TabIndex = 6;
            this.btnSignIn.Text = "Sign In";
            this.btnSignIn.UseVisualStyleBackColor = false;
            this.btnSignIn.Click += (s, e) =>
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Text;

                // Hardcoded default credentials verification
                bool isValid = (string.Equals(username, "admin", System.StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(username, "admin@sb1.com", System.StringComparison.OrdinalIgnoreCase)) &&
                               password == "admin123";

                if (isValid)
                {
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
                    lblStatus.Text = "✓ Login successful! Opening dashboard...";
                    btnSignIn.Enabled = false;

                    var successTimer = new System.Windows.Forms.Timer { Interval = 500 };
                    successTimer.Tick += (st, ev) =>
                    {
                        successTimer.Stop();
                        successTimer.Dispose();
                        LoginSucceeded?.Invoke(this, System.EventArgs.Empty);
                    };
                    successTimer.Start();
                }
                else
                {
                    lblStatus.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
                    lblStatus.Text = "✕ Invalid credentials. Default: admin / admin123";
                }
            };
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 280);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(420, 25);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 640);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pnlControls);
            this.Name = "LoginForm";
            this.Text = "SB1 - Login";

            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            this.pnlUserBorder.ResumeLayout(false);
            this.pnlUserBorder.PerformLayout();
            this.pnlPassBorder.ResumeLayout(false);
            this.pnlPassBorder.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
