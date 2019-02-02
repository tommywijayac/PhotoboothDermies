namespace Erha
{
    partial class Review
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Review));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.ibFront = new Emgu.CV.UI.ImageBox();
      this.ibLeft = new Emgu.CV.UI.ImageBox();
      this.ibRight = new Emgu.CV.UI.ImageBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rb5 = new System.Windows.Forms.RadioButton();
      this.rb4 = new System.Windows.Forms.RadioButton();
      this.rb3 = new System.Windows.Forms.RadioButton();
      this.rb2 = new System.Windows.Forms.RadioButton();
      this.rb1 = new System.Windows.Forms.RadioButton();
      this.tbBarcodeR = new System.Windows.Forms.TextBox();
      this.timclose = new System.Windows.Forms.Timer(this.components);
      this.lbThanks = new System.Windows.Forms.Label();
      this.timerHold = new System.Windows.Forms.Timer(this.components);
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ibFront)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ibLeft)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ibRight)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
      this.tableLayoutPanel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tableLayoutPanel1.BackgroundImage")));
      this.tableLayoutPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
      this.tableLayoutPanel1.Controls.Add(this.ibFront, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.ibLeft, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.ibRight, 3, 1);
      this.tableLayoutPanel1.Controls.Add(this.panel1, 2, 3);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 4;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(1920, 1080);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // ibFront
      // 
      this.ibFront.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ibFront.Location = new System.Drawing.Point(675, 239);
      this.ibFront.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.ibFront.Name = "ibFront";
      this.ibFront.Size = new System.Drawing.Size(570, 492);
      this.ibFront.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.ibFront.TabIndex = 2;
      this.ibFront.TabStop = false;
      // 
      // ibLeft
      // 
      this.ibLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ibLeft.Location = new System.Drawing.Point(100, 241);
      this.ibLeft.Margin = new System.Windows.Forms.Padding(4);
      this.ibLeft.Name = "ibLeft";
      this.ibLeft.Size = new System.Drawing.Size(568, 488);
      this.ibLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.ibLeft.TabIndex = 2;
      this.ibLeft.TabStop = false;
      // 
      // ibRight
      // 
      this.ibRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ibRight.Location = new System.Drawing.Point(1252, 241);
      this.ibRight.Margin = new System.Windows.Forms.Padding(4);
      this.ibRight.Name = "ibRight";
      this.ibRight.Size = new System.Drawing.Size(568, 488);
      this.ibRight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.ibRight.TabIndex = 2;
      this.ibRight.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(143)))));
      this.panel1.Controls.Add(this.rb5);
      this.panel1.Controls.Add(this.rb4);
      this.panel1.Controls.Add(this.rb3);
      this.panel1.Controls.Add(this.rb2);
      this.panel1.Controls.Add(this.rb1);
      this.panel1.Controls.Add(this.tbBarcodeR);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(675, 854);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(570, 223);
      this.panel1.TabIndex = 3;
      // 
      // rb5
      // 
      this.rb5.AutoSize = true;
      this.rb5.FlatAppearance.BorderSize = 2;
      this.rb5.FlatAppearance.CheckedBackColor = System.Drawing.Color.Black;
      this.rb5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.rb5.Location = new System.Drawing.Point(437, 113);
      this.rb5.Name = "rb5";
      this.rb5.Size = new System.Drawing.Size(16, 15);
      this.rb5.TabIndex = 5;
      this.rb5.TabStop = true;
      this.rb5.UseVisualStyleBackColor = true;
      // 
      // rb4
      // 
      this.rb4.AutoSize = true;
      this.rb4.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(55)))), ((int)(((byte)(54)))));
      this.rb4.FlatAppearance.BorderSize = 3;
      this.rb4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.rb4.Location = new System.Drawing.Point(365, 113);
      this.rb4.Name = "rb4";
      this.rb4.Size = new System.Drawing.Size(16, 15);
      this.rb4.TabIndex = 4;
      this.rb4.TabStop = true;
      this.rb4.UseVisualStyleBackColor = true;
      // 
      // rb3
      // 
      this.rb3.AutoSize = true;
      this.rb3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.rb3.Location = new System.Drawing.Point(289, 113);
      this.rb3.Name = "rb3";
      this.rb3.Size = new System.Drawing.Size(16, 15);
      this.rb3.TabIndex = 3;
      this.rb3.TabStop = true;
      this.rb3.UseVisualStyleBackColor = true;
      // 
      // rb2
      // 
      this.rb2.AutoSize = true;
      this.rb2.Location = new System.Drawing.Point(209, 112);
      this.rb2.Name = "rb2";
      this.rb2.Size = new System.Drawing.Size(17, 16);
      this.rb2.TabIndex = 2;
      this.rb2.TabStop = true;
      this.rb2.UseVisualStyleBackColor = true;
      // 
      // rb1
      // 
      this.rb1.AutoSize = true;
      this.rb1.Location = new System.Drawing.Point(127, 112);
      this.rb1.Name = "rb1";
      this.rb1.Size = new System.Drawing.Size(17, 16);
      this.rb1.TabIndex = 1;
      this.rb1.TabStop = true;
      this.rb1.UseVisualStyleBackColor = true;
      // 
      // tbBarcodeR
      // 
      this.tbBarcodeR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBarcodeR.Font = new System.Drawing.Font("Microsoft Sans Serif", 25.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbBarcodeR.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
      this.tbBarcodeR.Location = new System.Drawing.Point(0, 1);
      this.tbBarcodeR.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tbBarcodeR.Name = "tbBarcodeR";
      this.tbBarcodeR.Size = new System.Drawing.Size(570, 56);
      this.tbBarcodeR.TabIndex = 0;
      this.tbBarcodeR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.tbBarcodeR.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbBarcodeR_KeyDown);
      // 
      // timclose
      // 
      this.timclose.Interval = 1000;
      this.timclose.Tick += new System.EventHandler(this.timclose_Tick);
      // 
      // lbThanks
      // 
      this.lbThanks.Anchor = System.Windows.Forms.AnchorStyles.None;
      this.lbThanks.AutoSize = true;
      this.lbThanks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(143)))));
      this.lbThanks.Font = new System.Drawing.Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbThanks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
      this.lbThanks.Location = new System.Drawing.Point(694, 413);
      this.lbThanks.Name = "lbThanks";
      this.lbThanks.Size = new System.Drawing.Size(651, 113);
      this.lbThanks.TabIndex = 1;
      this.lbThanks.Text = "Terima Kasih";
      this.lbThanks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // timerHold
      // 
      this.timerHold.Tick += new System.EventHandler(this.timerHold_Tick);
      // 
      // Review
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.ClientSize = new System.Drawing.Size(1920, 1080);
      this.Controls.Add(this.tableLayoutPanel1);
      this.Controls.Add(this.lbThanks);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "Review";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Review";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.Load += new System.EventHandler(this.Review_Load);
      this.tableLayoutPanel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.ibFront)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ibLeft)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ibRight)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Emgu.CV.UI.ImageBox ibFront;
        private Emgu.CV.UI.ImageBox ibLeft;
        private Emgu.CV.UI.ImageBox ibRight;
        private System.Windows.Forms.Timer timclose;
        public System.Windows.Forms.TextBox tbBarcodeR;
        private System.Windows.Forms.Label lbThanks;
        private System.Windows.Forms.Timer timerHold;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rb5;
    private System.Windows.Forms.RadioButton rb4;
    private System.Windows.Forms.RadioButton rb3;
    private System.Windows.Forms.RadioButton rb2;
    private System.Windows.Forms.RadioButton rb1;
  }
}