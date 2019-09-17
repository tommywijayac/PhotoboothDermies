namespace SettingErha
{
    partial class Form3
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
            this.pathview = new System.Windows.Forms.TreeView();
            this.canc = new System.Windows.Forms.Button();
            this.sel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pathview
            // 
            this.pathview.Location = new System.Drawing.Point(12, 10);
            this.pathview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pathview.Name = "pathview";
            this.pathview.Size = new System.Drawing.Size(352, 253);
            this.pathview.TabIndex = 0;
            // 
            // canc
            // 
            this.canc.Location = new System.Drawing.Point(289, 268);
            this.canc.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.canc.Name = "canc";
            this.canc.Size = new System.Drawing.Size(74, 31);
            this.canc.TabIndex = 1;
            this.canc.Text = "Cancel";
            this.canc.UseVisualStyleBackColor = true;
            this.canc.Click += new System.EventHandler(this.canc_Click);
            // 
            // sel
            // 
            this.sel.Location = new System.Drawing.Point(210, 268);
            this.sel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sel.Name = "sel";
            this.sel.Size = new System.Drawing.Size(74, 31);
            this.sel.TabIndex = 2;
            this.sel.Text = "Select";
            this.sel.UseVisualStyleBackColor = true;
            this.sel.Click += new System.EventHandler(this.sel_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 309);
            this.Controls.Add(this.sel);
            this.Controls.Add(this.canc);
            this.Controls.Add(this.pathview);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "Form3";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select FTP Folder";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView pathview;
        private System.Windows.Forms.Button canc;
        private System.Windows.Forms.Button sel;
    }
}