namespace SwarmBot.UI
{
    partial class FormUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUI));
            this.SwConsole = new System.Windows.Forms.RichTextBox();
            this.restartButton = new System.Windows.Forms.Button();
            this.debugEnableButton = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIconMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // SwConsole
            // 
            this.SwConsole.BackColor = System.Drawing.Color.Black;
            this.SwConsole.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SwConsole.ForeColor = System.Drawing.Color.White;
            this.SwConsole.Location = new System.Drawing.Point(13, 13);
            this.SwConsole.Name = "SwConsole";
            this.SwConsole.ReadOnly = true;
            this.SwConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.SwConsole.Size = new System.Drawing.Size(560, 337);
            this.SwConsole.TabIndex = 0;
            this.SwConsole.TabStop = false;
            this.SwConsole.Text = "";
            // 
            // restartButton
            // 
            this.restartButton.Location = new System.Drawing.Point(13, 356);
            this.restartButton.Name = "restartButton";
            this.restartButton.Size = new System.Drawing.Size(116, 23);
            this.restartButton.TabIndex = 1;
            this.restartButton.Text = "Restart SwarmBot";
            this.restartButton.UseVisualStyleBackColor = true;
            this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // debugEnableButton
            // 
            this.debugEnableButton.Location = new System.Drawing.Point(438, 356);
            this.debugEnableButton.Name = "debugEnableButton";
            this.debugEnableButton.Size = new System.Drawing.Size(135, 23);
            this.debugEnableButton.TabIndex = 2;
            this.debugEnableButton.Text = "Enable Debug Mode";
            this.debugEnableButton.UseVisualStyleBackColor = true;
            this.debugEnableButton.Click += new System.EventHandler(this.debugEnableButton_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.notifyIconMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "SwarmBot";
            // 
            // notifyIconMenu
            // 
            this.notifyIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showMenuItem,
            this.exitMenuItem});
            this.notifyIconMenu.Name = "notifyIconMenu";
            this.notifyIconMenu.Size = new System.Drawing.Size(153, 70);
            // 
            // showMenuItem
            // 
            this.showMenuItem.Name = "showMenuItem";
            this.showMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showMenuItem.Text = "Show";
            this.showMenuItem.Click += new System.EventHandler(this.showMenuItem_Click);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // FormUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 387);
            this.Controls.Add(this.debugEnableButton);
            this.Controls.Add(this.restartButton);
            this.Controls.Add(this.SwConsole);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(601, 426);
            this.MinimumSize = new System.Drawing.Size(601, 426);
            this.Name = "FormUI";
            this.Text = "SwarmBot";
            this.Load += new System.EventHandler(this.FormUI_Load);
            this.Resize += new System.EventHandler(this.FormUI_Resize);
            this.notifyIconMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox SwConsole;
        private System.Windows.Forms.Button restartButton;
        private System.Windows.Forms.Button debugEnableButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyIconMenu;
        private System.Windows.Forms.ToolStripMenuItem showMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
    }
}

