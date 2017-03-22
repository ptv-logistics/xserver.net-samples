namespace XSTwo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.formsMap2 = new Ptv.XServer.Controls.Map.FormsMap();
            this.formsMap1 = new Ptv.XServer.Controls.Map.FormsMap();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // formsMap2
            // 
            this.formsMap2.Center = ((System.Windows.Point)(resources.GetObject("formsMap2.Center")));
            this.formsMap2.CoordinateDiplayFormat = Ptv.XServer.Controls.Map.CoordinateDiplayFormat.Degree;
            this.formsMap2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsMap2.FitInWindow = false;
            this.formsMap2.InvertMouseWheel = false;
            this.formsMap2.Location = new System.Drawing.Point(0, 0);
            this.formsMap2.MaxZoom = 19;
            this.formsMap2.MinZoom = 0;
            this.formsMap2.MouseDoubleClickZoom = true;
            this.formsMap2.MouseDragMode = Ptv.XServer.Controls.Map.Gadgets.DragMode.SelectOnShift;
            this.formsMap2.MouseWheelSpeed = 0.5D;
            this.formsMap2.Name = "formsMap2";
            this.formsMap2.ShowCoordinates = true;
            this.formsMap2.ShowLayers = true;
            this.formsMap2.ShowMagnifier = true;
            this.formsMap2.ShowNavigation = true;
            this.formsMap2.ShowOverview = true;
            this.formsMap2.ShowScale = true;
            this.formsMap2.ShowZoomSlider = true;
            this.formsMap2.Size = new System.Drawing.Size(626, 654);
            this.formsMap2.TabIndex = 2;
            this.formsMap2.UseAnimation = true;
            this.formsMap2.UseDefaultTheme = true;
            this.formsMap2.UseMiles = false;
            this.formsMap2.XMapCopyright = "Please configure a valid copyright text!";
            this.formsMap2.XMapCredentials = "";
            this.formsMap2.XMapStyle = "";
            this.formsMap2.XMapUrl = "";
            this.formsMap2.ZoomLevel = 1D;
            // 
            // formsMap1
            // 
            this.formsMap1.Center = ((System.Windows.Point)(resources.GetObject("formsMap1.Center")));
            this.formsMap1.CoordinateDiplayFormat = Ptv.XServer.Controls.Map.CoordinateDiplayFormat.Degree;
            this.formsMap1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsMap1.FitInWindow = false;
            this.formsMap1.InvertMouseWheel = false;
            this.formsMap1.Location = new System.Drawing.Point(0, 0);
            this.formsMap1.MaxZoom = 19;
            this.formsMap1.MinZoom = 0;
            this.formsMap1.MouseDoubleClickZoom = true;
            this.formsMap1.MouseDragMode = Ptv.XServer.Controls.Map.Gadgets.DragMode.SelectOnShift;
            this.formsMap1.MouseWheelSpeed = 0.5D;
            this.formsMap1.Name = "formsMap1";
            this.formsMap1.ShowCoordinates = true;
            this.formsMap1.ShowLayers = true;
            this.formsMap1.ShowMagnifier = true;
            this.formsMap1.ShowNavigation = true;
            this.formsMap1.ShowOverview = true;
            this.formsMap1.ShowScale = true;
            this.formsMap1.ShowZoomSlider = true;
            this.formsMap1.Size = new System.Drawing.Size(574, 654);
            this.formsMap1.TabIndex = 0;
            this.formsMap1.UseAnimation = true;
            this.formsMap1.UseDefaultTheme = true;
            this.formsMap1.UseMiles = false;
            this.formsMap1.XMapCopyright = "Please configure a valid copyright text!";
            this.formsMap1.XMapCredentials = "";
            this.formsMap1.XMapStyle = "";
            this.formsMap1.XMapUrl = "";
            this.formsMap1.ZoomLevel = 1D;
            this.formsMap1.Load += new System.EventHandler(this.formsMap1_Load);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.formsMap2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.formsMap1);
            this.splitContainer1.Size = new System.Drawing.Size(1204, 654);
            this.splitContainer1.SplitterDistance = 626;
            this.splitContainer1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1204, 654);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "XMap-2 Demo";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Ptv.XServer.Controls.Map.FormsMap formsMap2;
        private Ptv.XServer.Controls.Map.FormsMap formsMap1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}

