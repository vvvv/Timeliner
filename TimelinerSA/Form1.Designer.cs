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
            this.panel1 = new System.Windows.Forms.Panel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PrefixTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TargetPortNumberBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.TargetIPTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ReceivePortNumberBox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPortNumberBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReceivePortNumberBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.webBrowser1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(876, 475);
            this.panel1.TabIndex = 3;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 20);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(876, 455);
            this.webBrowser1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.ReceivePortNumberBox);
            this.panel2.Controls.Add(this.PrefixTextBox);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.TargetPortNumberBox);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.TargetIPTextBox);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(876, 20);
            this.panel2.TabIndex = 1;
            // 
            // PrefixTextBox
            // 
            this.PrefixTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PrefixTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PrefixTextBox.Location = new System.Drawing.Point(272, 0);
            this.PrefixTextBox.Name = "PrefixTextBox";
            this.PrefixTextBox.Size = new System.Drawing.Size(100, 13);
            this.PrefixTextBox.TabIndex = 5;
            this.PrefixTextBox.Text = "/timeliner";
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(233, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Prefix";
            // 
            // TargetPortNumberBox
            // 
            this.TargetPortNumberBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TargetPortNumberBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TargetPortNumberBox.Location = new System.Drawing.Point(183, 0);
            this.TargetPortNumberBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.TargetPortNumberBox.Name = "TargetPortNumberBox";
            this.TargetPortNumberBox.Size = new System.Drawing.Size(50, 16);
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
            this.label2.Location = new System.Drawing.Point(152, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // TargetIPTextBox
            // 
            this.TargetIPTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TargetIPTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.TargetIPTextBox.Location = new System.Drawing.Point(58, 0);
            this.TargetIPTextBox.Name = "TargetIPTextBox";
            this.TargetIPTextBox.Size = new System.Drawing.Size(94, 13);
            this.TargetIPTextBox.TabIndex = 0;
            this.TargetIPTextBox.Text = "127.0.0.1";
            this.TargetIPTextBox.TextChanged += new System.EventHandler(this.TextBox1TextChanged);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Target IP";
            // 
            // timer1
            // 
            this.timer1.Interval = 1;
            // 
            // ReceivePortNumberBox
            // 
            this.ReceivePortNumberBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReceivePortNumberBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.ReceivePortNumberBox.Location = new System.Drawing.Point(825, 0);
            this.ReceivePortNumberBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.ReceivePortNumberBox.Name = "ReceivePortNumberBox";
            this.ReceivePortNumberBox.Size = new System.Drawing.Size(51, 16);
            this.ReceivePortNumberBox.TabIndex = 9;
            this.ReceivePortNumberBox.Value = new decimal(new int[] {
            5555,
            0,
            0,
            0});
            this.ReceivePortNumberBox.ValueChanged += new System.EventHandler(this.ReceivePortNumberBoxValueChanged);
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Right;
            this.label4.Location = new System.Drawing.Point(753, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Receive Port";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(876, 475);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "TimelinerSA";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TargetPortNumberBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReceivePortNumberBox)).EndInit();
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown ReceivePortNumberBox;
        private System.Windows.Forms.TextBox PrefixTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.NumericUpDown TargetPortNumberBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TargetIPTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel1;

        #endregion

    }
}

