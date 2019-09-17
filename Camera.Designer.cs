namespace Erha
{
    partial class Camera
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Camera));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.ibFront = new Emgu.CV.UI.ImageBox();
      this.lbCountdown = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.timerResetTolerate = new System.Windows.Forms.Timer(this.components);
      this.timersave = new System.Windows.Forms.Timer(this.components);
      this.timerView = new System.Windows.Forms.Timer(this.components);
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ibFront)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
      this.tableLayoutPanel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tableLayoutPanel1.BackgroundImage")));
      this.tableLayoutPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
      this.tableLayoutPanel1.Controls.Add(this.ibFront, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.lbCountdown, 2, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(1920, 1080);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // ibFront
      // 
      this.ibFront.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ibFront.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
      this.ibFront.Location = new System.Drawing.Point(292, 274);
      this.ibFront.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.ibFront.Name = "ibFront";
      this.ibFront.Size = new System.Drawing.Size(1336, 802);
      this.ibFront.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.ibFront.TabIndex = 2;
      this.ibFront.TabStop = false;
      // 
      // lbCountdown
      // 
      this.lbCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lbCountdown.AutoSize = true;
      this.lbCountdown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(243)))), ((int)(((byte)(143)))));
      this.lbCountdown.Font = new System.Drawing.Font("Microsoft Sans Serif", 150F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lbCountdown.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
      this.lbCountdown.Location = new System.Drawing.Point(1636, 270);
      this.lbCountdown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lbCountdown.Name = "lbCountdown";
      this.lbCountdown.Size = new System.Drawing.Size(280, 810);
      this.lbCountdown.TabIndex = 3;
      this.lbCountdown.Text = "3";
      this.lbCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.lbCountdown.Click += new System.EventHandler(this.lbCountdown_Click);
      // 
      // timer
      // 
      this.timer.Interval = 1000;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // timerResetTolerate
      // 
      this.timerResetTolerate.Interval = 1000;
      this.timerResetTolerate.Tick += new System.EventHandler(this.timerResetTolerate_Tick);
      // 
      // timersave
      // 
      this.timersave.Interval = 1;
      this.timersave.Tick += new System.EventHandler(this.timersave_Tick);
      // 
      // timerView
      // 
      this.timerView.Interval = 1000;
      this.timerView.Tick += new System.EventHandler(this.timerView_Tick);
      // 
      // Camera
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.ClientSize = new System.Drawing.Size(1920, 1080);
      this.Controls.Add(this.tableLayoutPanel1);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "Camera";
      this.Text = "Camera";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Camera_FormClosing);
      this.Load += new System.EventHandler(this.Form1_Load);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.ibFront)).EndInit();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Emgu.CV.UI.ImageBox ibFront;
        private System.Windows.Forms.Label lbCountdown;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Timer timerResetTolerate;
        private System.Windows.Forms.Timer timersave;
        private System.Windows.Forms.Timer timerView;
    }
}
