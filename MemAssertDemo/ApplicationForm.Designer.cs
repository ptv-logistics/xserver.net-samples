namespace MemoryDemo
{
    partial class ApplicationForm
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
            this.openMapWindow1 = new System.Windows.Forms.Button();
            this.AddTruckButton = new System.Windows.Forms.Button();
            this.openMapWindow2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openMapWindow1
            // 
            this.openMapWindow1.Location = new System.Drawing.Point(12, 12);
            this.openMapWindow1.Name = "openMapWindow1";
            this.openMapWindow1.Size = new System.Drawing.Size(123, 45);
            this.openMapWindow1.TabIndex = 0;
            this.openMapWindow1.Text = "Open MapWindow with Memory Leak";
            this.openMapWindow1.UseVisualStyleBackColor = true;
            this.openMapWindow1.Click += new System.EventHandler(this.openMapWindow1_Click);
            // 
            // AddTruckButton
            // 
            this.AddTruckButton.Enabled = false;
            this.AddTruckButton.Location = new System.Drawing.Point(12, 120);
            this.AddTruckButton.Name = "AddTruckButton";
            this.AddTruckButton.Size = new System.Drawing.Size(260, 23);
            this.AddTruckButton.TabIndex = 1;
            this.AddTruckButton.Text = "Add Truck";
            this.AddTruckButton.UseVisualStyleBackColor = true;
            // 
            // openMapWindow2
            // 
            this.openMapWindow2.Location = new System.Drawing.Point(149, 12);
            this.openMapWindow2.Name = "openMapWindow2";
            this.openMapWindow2.Size = new System.Drawing.Size(123, 45);
            this.openMapWindow2.TabIndex = 2;
            this.openMapWindow2.Text = "Open Map Window without Memory Leak";
            this.openMapWindow2.UseVisualStyleBackColor = true;
            this.openMapWindow2.Click += new System.EventHandler(this.openMapWindow2_Click);
            // 
            // ApplicationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.openMapWindow2);
            this.Controls.Add(this.AddTruckButton);
            this.Controls.Add(this.openMapWindow1);
            this.Name = "ApplicationForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button openMapWindow1;
        public System.Windows.Forms.Button AddTruckButton;
        private System.Windows.Forms.Button openMapWindow2;
    }
}

