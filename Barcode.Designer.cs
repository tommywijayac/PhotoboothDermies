namespace Erha
{
    partial class Barcode
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Barcode));
      this.tbBarcode = new System.Windows.Forms.TextBox();
      this.timerHold = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // tbBarcode
      // 
      this.tbBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbBarcode.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
      this.tbBarcode.Location = new System.Drawing.Point(73, 885);
      this.tbBarcode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tbBarcode.Name = "tbBarcode";
      this.tbBarcode.Size = new System.Drawing.Size(1781, 49);
      this.tbBarcode.TabIndex = 0;
      this.tbBarcode.Text = "Scan lembar antrian anda...";
      this.tbBarcode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.tbBarcode.TextChanged += new System.EventHandler(this.tbBarcode_TextChanged);
      this.tbBarcode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbBarcode_KeyDown);
      // 
      // timerHold
      // 
      this.timerHold.Interval = 2000;
      this.timerHold.Tick += new System.EventHandler(this.timerHold_Tick);
      // 
      // Barcode
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
      this.ClientSize = new System.Drawing.Size(1920, 1080);
      this.Controls.Add(this.tbBarcode);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.Name = "Barcode";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Barcode";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Barcode_FormClosed);
      this.Load += new System.EventHandler(this.Barcode_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timerHold;
        public System.Windows.Forms.TextBox tbBarcode;
    }
}
