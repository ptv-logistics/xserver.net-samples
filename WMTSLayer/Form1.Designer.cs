namespace XMap2FactoryTest
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
            this.formsMap = new Ptv.XServer.Controls.Map.FormsMap();
            this.SuspendLayout();
            // 
            // formsMap
            // 
            this.formsMap.Center = ((System.Windows.Point)(resources.GetObject("formsMap.Center")));
            this.formsMap.CoordinateDiplayFormat = Ptv.XServer.Controls.Map.CoordinateDiplayFormat.Degree;
            this.formsMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formsMap.FitInWindow = false;
            this.formsMap.InvertMouseWheel = false;
            this.formsMap.Location = new System.Drawing.Point(0, 0);
            this.formsMap.Margin = new System.Windows.Forms.Padding(2);
            this.formsMap.MaxZoom = 19;
            this.formsMap.MinZoom = 0;
            this.formsMap.MouseDoubleClickZoom = true;
            this.formsMap.MouseDragMode = Ptv.XServer.Controls.Map.Gadgets.DragMode.SelectOnShift;
            this.formsMap.MouseWheelSpeed = 0.5D;
            this.formsMap.Name = "formsMap";
            this.formsMap.ShowCoordinates = true;
            this.formsMap.ShowLayers = true;
            this.formsMap.ShowMagnifier = true;
            this.formsMap.ShowNavigation = true;
            this.formsMap.ShowOverview = true;
            this.formsMap.ShowScale = true;
            this.formsMap.ShowZoomSlider = true;
            this.formsMap.Size = new System.Drawing.Size(986, 555);
            this.formsMap.TabIndex = 3;
            this.formsMap.UseAnimation = true;
            this.formsMap.UseDefaultTheme = true;
            this.formsMap.UseMiles = false;
            this.formsMap.XMapCopyright = "Please configure a valid copyright text!";
            this.formsMap.XMapCredentials = "";
            this.formsMap.XMapStyle = "";
            this.formsMap.XMapUrl = "";
            this.formsMap.ZoomLevel = 1D;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(986, 555);
            this.Controls.Add(this.formsMap);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "XMap2 Layer Factory";
            this.ResumeLayout(false);

        }

        #endregion

        internal Ptv.XServer.Controls.Map.FormsMap formsMap;
    }
}

