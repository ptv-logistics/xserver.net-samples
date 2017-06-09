namespace Ptv.XServer.Controls
{
    partial class ActiveXSample
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActiveXSample));
            this.formsMap = new Ptv.XServer.Controls.Map.FormsMap();
            this.SuspendLayout();
            // 
            // formsMap
            // 
            this.formsMap.Center = ((System.Windows.Point)(resources.GetObject("formsMap.Center")));
            this.formsMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsMap.FitInWindow = false;
            this.formsMap.InvertMouseWheel = false;
            this.formsMap.Location = new System.Drawing.Point(0, 0);
            this.formsMap.MaxZoom = 19;
            this.formsMap.MinZoom = 0;
            this.formsMap.MouseWheelSpeed = 0.5D;
            this.formsMap.Name = "formsMap";
            this.formsMap.ShowCoordinates = true;
            this.formsMap.ShowLayers = true;
            this.formsMap.ShowMagnifier = true;
            this.formsMap.ShowNavigation = true;
            this.formsMap.ShowOverview = true;
            this.formsMap.ShowScale = true;
            this.formsMap.ShowZoomSlider = true;
            this.formsMap.Size = new System.Drawing.Size(674, 460);
            this.formsMap.TabIndex = 0;
            this.formsMap.UseAnimation = true;
            this.formsMap.UseDefaultTheme = true;
            this.formsMap.UseMiles = false;
            this.formsMap.XMapCopyright = "Please configure a valid copyright text!";
            this.formsMap.XMapUrl = "";
            this.formsMap.ZoomLevel = 1D;
            // 
            // ActiveXSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.formsMap);
            this.Name = "ActiveXSample";
            this.Size = new System.Drawing.Size(674, 460);
            this.ResumeLayout(false);

        }

        #endregion

        private Map.FormsMap formsMap;
    }
}
