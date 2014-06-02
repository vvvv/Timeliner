namespace TimeLinerSA
{
    partial class Form1
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
        	System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        	this.OSCPanel = new System.Windows.Forms.Panel();
        	this.label4 = new System.Windows.Forms.Label();
        	this.ReceivePortNumberBox = new System.Windows.Forms.NumericUpDown();
        	this.PrefixTextBox = new System.Windows.Forms.TextBox();
        	this.label3 = new System.Windows.Forms.Label();
        	this.TargetPortNumberBox = new System.Windows.Forms.NumericUpDown();
        	this.label2 = new System.Windows.Forms.Label();
        	this.TargetIPTextBox = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.timer1 = new System.Windows.Forms.Timer(this.components);
        	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        	this.mainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.oSCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.FOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
        	this.FSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
        	this.webBrowser1 = new System.Windows.Forms.WebBrowser();
        	this.OSCPanel.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.ReceivePortNumberBox)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.TargetPortNumberBox)).BeginInit();
        	this.menuStrip1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// OSCPanel
        	// 
        	this.OSCPanel.Controls.Add(this.label4);
        	this.OSCPanel.Controls.Add(this.ReceivePortNumberBox);
        	this.OSCPanel.Controls.Add(this.PrefixTextBox);
        	this.OSCPanel.Controls.Add(this.label3);
        	this.OSCPanel.Controls.Add(this.TargetPortNumberBox);
        	this.OSCPanel.Controls.Add(this.label2);
        	this.OSCPanel.Controls.Add(this.TargetIPTextBox);
        	this.OSCPanel.Controls.Add(this.label1);
        	this.OSCPanel.Controls.Add(this.panel1);
        	this.OSCPanel.Dock = System.Windows.Forms.DockStyle.Top;
        	this.OSCPanel.Location = new System.Drawing.Point(0, 24);
        	this.OSCPanel.Name = "OSCPanel";
        	this.OSCPanel.Size = new System.Drawing.Size(876, 20);
        	this.OSCPanel.TabIndex = 1;
        	this.OSCPanel.Visible = false;
        	// 
        	// label4
        	// 
        	this.label4.Dock = System.Windows.Forms.DockStyle.Right;
        	this.label4.Location = new System.Drawing.Point(753, 0);
        	this.label4.Name = "label4";
        	this.label4.Size = new System.Drawing.Size(72, 20);
        	this.label4.TabIndex = 10;
        	this.label4.Text = "Receive Port";
        	this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// ReceivePortNumberBox
        	// 
        	this.ReceivePortNumberBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.ReceivePortNumberBox.Dock = System.Windows.Forms.DockStyle.Right;
        	this.ReceivePortNumberBox.Location = new System.Drawing.Point(825, 0);
        	this.ReceivePortNumberBox.Maximum = new decimal(new int[] {
        	        	        	65535,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.ReceivePortNumberBox.Name = "ReceivePortNumberBox";
        	this.ReceivePortNumberBox.Size = new System.Drawing.Size(51, 20);
        	this.ReceivePortNumberBox.TabIndex = 9;
        	this.ReceivePortNumberBox.Value = new decimal(new int[] {
        	        	        	5555,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.ReceivePortNumberBox.ValueChanged += new System.EventHandler(this.ReceivePortNumberBoxValueChanged);
        	// 
        	// PrefixTextBox
        	// 
        	this.PrefixTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.PrefixTextBox.Dock = System.Windows.Forms.DockStyle.Left;
        	this.PrefixTextBox.Location = new System.Drawing.Point(281, 0);
        	this.PrefixTextBox.Name = "PrefixTextBox";
        	this.PrefixTextBox.Size = new System.Drawing.Size(100, 20);
        	this.PrefixTextBox.TabIndex = 5;
        	this.PrefixTextBox.Text = "timeliner";
        	// 
        	// label3
        	// 
        	this.label3.Dock = System.Windows.Forms.DockStyle.Left;
        	this.label3.Location = new System.Drawing.Point(237, 0);
        	this.label3.Name = "label3";
        	this.label3.Size = new System.Drawing.Size(44, 20);
        	this.label3.TabIndex = 4;
        	this.label3.Text = "Prefix  /";
        	this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// TargetPortNumberBox
        	// 
        	this.TargetPortNumberBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.TargetPortNumberBox.Dock = System.Windows.Forms.DockStyle.Left;
        	this.TargetPortNumberBox.Location = new System.Drawing.Point(187, 0);
        	this.TargetPortNumberBox.Maximum = new decimal(new int[] {
        	        	        	65535,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.TargetPortNumberBox.Name = "TargetPortNumberBox";
        	this.TargetPortNumberBox.Size = new System.Drawing.Size(50, 20);
        	this.TargetPortNumberBox.TabIndex = 2;
        	this.TargetPortNumberBox.Value = new decimal(new int[] {
        	        	        	4444,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.TargetPortNumberBox.ValueChanged += new System.EventHandler(this.NumericUpDown1ValueChanged);
        	// 
        	// label2
        	// 
        	this.label2.Dock = System.Windows.Forms.DockStyle.Left;
        	this.label2.Location = new System.Drawing.Point(158, 0);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(29, 20);
        	this.label2.TabIndex = 3;
        	this.label2.Text = "Port";
        	this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// TargetIPTextBox
        	// 
        	this.TargetIPTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.TargetIPTextBox.Dock = System.Windows.Forms.DockStyle.Left;
        	this.TargetIPTextBox.Location = new System.Drawing.Point(64, 0);
        	this.TargetIPTextBox.MaxLength = 15;
        	this.TargetIPTextBox.Name = "TargetIPTextBox";
        	this.TargetIPTextBox.Size = new System.Drawing.Size(94, 20);
        	this.TargetIPTextBox.TabIndex = 0;
        	this.TargetIPTextBox.Text = "127.0.0.1";
        	this.TargetIPTextBox.TextChanged += new System.EventHandler(this.TextBox1TextChanged);
        	// 
        	// label1
        	// 
        	this.label1.Dock = System.Windows.Forms.DockStyle.Left;
        	this.label1.Location = new System.Drawing.Point(12, 0);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(52, 20);
        	this.label1.TabIndex = 1;
        	this.label1.Text = "Target IP";
        	this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        	// 
        	// panel1
        	// 
        	this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
        	this.panel1.Location = new System.Drawing.Point(0, 0);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(12, 20);
        	this.panel1.TabIndex = 11;
        	// 
        	// timer1
        	// 
        	this.timer1.Interval = 1;
        	// 
        	// menuStrip1
        	// 
        	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.mainToolStripMenuItem});
        	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        	this.menuStrip1.Name = "menuStrip1";
        	this.menuStrip1.Size = new System.Drawing.Size(876, 24);
        	this.menuStrip1.TabIndex = 4;
        	this.menuStrip1.Text = "menuStrip1";
        	// 
        	// mainToolStripMenuItem
        	// 
        	this.mainToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.newToolStripMenuItem,
        	        	        	this.loadToolStripMenuItem,
        	        	        	this.saveToolStripMenuItem,
        	        	        	this.saveAsToolStripMenuItem,
        	        	        	this.oSCToolStripMenuItem,
        	        	        	this.exitToolStripMenuItem});
        	this.mainToolStripMenuItem.Name = "mainToolStripMenuItem";
        	this.mainToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
        	this.mainToolStripMenuItem.Text = "Main Menu";
        	// 
        	// newToolStripMenuItem
        	// 
        	this.newToolStripMenuItem.Name = "newToolStripMenuItem";
        	this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
        	this.newToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.newToolStripMenuItem.Text = "New";
        	this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItemClick);
        	// 
        	// loadToolStripMenuItem
        	// 
        	this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
        	this.loadToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
        	this.loadToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.loadToolStripMenuItem.Text = "Open";
        	this.loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItemClick);
        	// 
        	// saveToolStripMenuItem
        	// 
        	this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        	this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
        	this.saveToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.saveToolStripMenuItem.Text = "Save";
        	this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItemClick);
        	// 
        	// saveAsToolStripMenuItem
        	// 
        	this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        	this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
        	        	        	| System.Windows.Forms.Keys.S)));
        	this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.saveAsToolStripMenuItem.Text = "Save As...";
        	this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.SaveAsToolStripMenuItemClick);
        	// 
        	// oSCToolStripMenuItem
        	// 
        	this.oSCToolStripMenuItem.Name = "oSCToolStripMenuItem";
        	this.oSCToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.oSCToolStripMenuItem.Text = "OSC";
        	this.oSCToolStripMenuItem.Click += new System.EventHandler(this.OSCToolStripMenuItemClick);
        	// 
        	// exitToolStripMenuItem
        	// 
        	this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        	this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
        	this.exitToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
        	this.exitToolStripMenuItem.Text = "Exit";
        	this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
        	// 
        	// FOpenFileDialog
        	// 
        	this.FOpenFileDialog.Filter = "*.xml|*.xml";
        	// 
        	// FSaveFileDialog
        	// 
        	this.FSaveFileDialog.Filter = "*.xml|*.xml";
        	// 
        	// webBrowser1
        	// 
        	this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.webBrowser1.Location = new System.Drawing.Point(0, 44);
        	this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
        	this.webBrowser1.Name = "webBrowser1";
        	this.webBrowser1.ScriptErrorsSuppressed = true;
        	this.webBrowser1.Size = new System.Drawing.Size(876, 431);
        	this.webBrowser1.TabIndex = 5;
        	// 
        	// Form1
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(876, 475);
        	this.Controls.Add(this.webBrowser1);
        	this.Controls.Add(this.OSCPanel);
        	this.Controls.Add(this.menuStrip1);
        	this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        	this.MainMenuStrip = this.menuStrip1;
        	this.Name = "Form1";
        	this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1FormClosed);
        	this.OSCPanel.ResumeLayout(false);
        	this.OSCPanel.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.ReceivePortNumberBox)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.TargetPortNumberBox)).EndInit();
        	this.menuStrip1.ResumeLayout(false);
        	this.menuStrip1.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SaveFileDialog FSaveFileDialog;
        private System.Windows.Forms.OpenFileDialog FOpenFileDialog;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem oSCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mainToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown ReceivePortNumberBox;
        private System.Windows.Forms.TextBox PrefixTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel OSCPanel;
        private System.Windows.Forms.NumericUpDown TargetPortNumberBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TargetIPTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.WebBrowser webBrowser1;

        #endregion

    }
}

