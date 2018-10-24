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
            this.featureLayerCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timeSpanTextBox = new System.Windows.Forms.TextBox();
            this.timeSpanRadioButton = new System.Windows.Forms.RadioButton();
            this.optimisticRadioButton = new System.Windows.Forms.RadioButton();
            this.snapshotRadioButton = new System.Windows.Forms.RadioButton();
            this.noneTimeConsiderationRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.timeZoneTextBox = new System.Windows.Forms.TextBox();
            this.referenceTimeTimePicker = new System.Windows.Forms.DateTimePicker();
            this.showOnlyRelevantCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.referenceTimeDatePicker = new System.Windows.Forms.DateTimePicker();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.mapLanguageTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.trafficIncidentsLanguageTextBox = new System.Windows.Forms.TextBox();
            this.formsMap = new Ptv.XServer.Controls.Map.FormsMap();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.mapStylesComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.contentSnapshotsComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.requestLoggingTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // featureLayerCheckedListBox
            // 
            this.featureLayerCheckedListBox.CheckOnClick = true;
            this.featureLayerCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.featureLayerCheckedListBox.FormattingEnabled = true;
            this.featureLayerCheckedListBox.Location = new System.Drawing.Point(7, 20);
            this.featureLayerCheckedListBox.Margin = new System.Windows.Forms.Padding(0);
            this.featureLayerCheckedListBox.Name = "featureLayerCheckedListBox";
            this.featureLayerCheckedListBox.Size = new System.Drawing.Size(189, 100);
            this.featureLayerCheckedListBox.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.featureLayerCheckedListBox);
            this.groupBox1.Location = new System.Drawing.Point(834, 153);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox1.Size = new System.Drawing.Size(203, 127);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Feature layers";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.timeSpanTextBox);
            this.groupBox2.Controls.Add(this.timeSpanRadioButton);
            this.groupBox2.Controls.Add(this.optimisticRadioButton);
            this.groupBox2.Controls.Add(this.snapshotRadioButton);
            this.groupBox2.Controls.Add(this.noneTimeConsiderationRadioButton);
            this.groupBox2.Location = new System.Drawing.Point(834, 287);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(203, 118);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Time consideration";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(136, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "h";
            // 
            // timeSpanTextBox
            // 
            this.timeSpanTextBox.Location = new System.Drawing.Point(84, 88);
            this.timeSpanTextBox.Name = "timeSpanTextBox";
            this.timeSpanTextBox.Size = new System.Drawing.Size(46, 20);
            this.timeSpanTextBox.TabIndex = 4;
            // 
            // timeSpanRadioButton
            // 
            this.timeSpanRadioButton.AutoSize = true;
            this.timeSpanRadioButton.Location = new System.Drawing.Point(7, 88);
            this.timeSpanRadioButton.Name = "timeSpanRadioButton";
            this.timeSpanRadioButton.Size = new System.Drawing.Size(71, 17);
            this.timeSpanRadioButton.TabIndex = 3;
            this.timeSpanRadioButton.Text = "Timespan";
            this.timeSpanRadioButton.UseVisualStyleBackColor = true;
            // 
            // optimisticRadioButton
            // 
            this.optimisticRadioButton.AutoSize = true;
            this.optimisticRadioButton.Location = new System.Drawing.Point(7, 46);
            this.optimisticRadioButton.Name = "optimisticRadioButton";
            this.optimisticRadioButton.Size = new System.Drawing.Size(70, 17);
            this.optimisticRadioButton.TabIndex = 1;
            this.optimisticRadioButton.Text = "Optimistic";
            this.optimisticRadioButton.UseVisualStyleBackColor = true;
            // 
            // snapshotRadioButton
            // 
            this.snapshotRadioButton.AutoSize = true;
            this.snapshotRadioButton.Location = new System.Drawing.Point(7, 67);
            this.snapshotRadioButton.Name = "snapshotRadioButton";
            this.snapshotRadioButton.Size = new System.Drawing.Size(70, 17);
            this.snapshotRadioButton.TabIndex = 2;
            this.snapshotRadioButton.Text = "Snapshot";
            this.snapshotRadioButton.UseVisualStyleBackColor = true;
            // 
            // noneTimeConsiderationRadioButton
            // 
            this.noneTimeConsiderationRadioButton.AutoSize = true;
            this.noneTimeConsiderationRadioButton.Location = new System.Drawing.Point(7, 25);
            this.noneTimeConsiderationRadioButton.Name = "noneTimeConsiderationRadioButton";
            this.noneTimeConsiderationRadioButton.Size = new System.Drawing.Size(51, 17);
            this.noneTimeConsiderationRadioButton.TabIndex = 0;
            this.noneTimeConsiderationRadioButton.Text = "None";
            this.noneTimeConsiderationRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.timeZoneTextBox);
            this.groupBox3.Controls.Add(this.referenceTimeTimePicker);
            this.groupBox3.Controls.Add(this.showOnlyRelevantCheckBox);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.referenceTimeDatePicker);
            this.groupBox3.Location = new System.Drawing.Point(834, 414);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(203, 108);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Reference time";
            // 
            // timeZoneTextBox
            // 
            this.timeZoneTextBox.Location = new System.Drawing.Point(107, 47);
            this.timeZoneTextBox.Name = "timeZoneTextBox";
            this.timeZoneTextBox.Size = new System.Drawing.Size(87, 20);
            this.timeZoneTextBox.TabIndex = 2;
            this.timeZoneTextBox.Text = "+02:00";
            // 
            // referenceTimeTimePicker
            // 
            this.referenceTimeTimePicker.CustomFormat = "";
            this.referenceTimeTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.referenceTimeTimePicker.Location = new System.Drawing.Point(107, 19);
            this.referenceTimeTimePicker.Name = "referenceTimeTimePicker";
            this.referenceTimeTimePicker.ShowUpDown = true;
            this.referenceTimeTimePicker.Size = new System.Drawing.Size(87, 20);
            this.referenceTimeTimePicker.TabIndex = 1;
            // 
            // showOnlyRelevantCheckBox
            // 
            this.showOnlyRelevantCheckBox.AutoSize = true;
            this.showOnlyRelevantCheckBox.Location = new System.Drawing.Point(7, 78);
            this.showOnlyRelevantCheckBox.Name = "showOnlyRelevantCheckBox";
            this.showOnlyRelevantCheckBox.Size = new System.Drawing.Size(158, 17);
            this.showOnlyRelevantCheckBox.TabIndex = 3;
            this.showOnlyRelevantCheckBox.Text = "Show only relevant (by time)";
            this.showOnlyRelevantCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Time zone (UTC)";
            // 
            // referenceTimeDatePicker
            // 
            this.referenceTimeDatePicker.CustomFormat = "";
            this.referenceTimeDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.referenceTimeDatePicker.Location = new System.Drawing.Point(6, 19);
            this.referenceTimeDatePicker.Name = "referenceTimeDatePicker";
            this.referenceTimeDatePicker.Size = new System.Drawing.Size(87, 20);
            this.referenceTimeDatePicker.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.mapLanguageTextBox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.trafficIncidentsLanguageTextBox);
            this.groupBox4.Location = new System.Drawing.Point(834, 71);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(203, 75);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Language settings for";
            // 
            // mapLanguageTextBox
            // 
            this.mapLanguageTextBox.Location = new System.Drawing.Point(139, 47);
            this.mapLanguageTextBox.Name = "mapLanguageTextBox";
            this.mapLanguageTextBox.Size = new System.Drawing.Size(57, 20);
            this.mapLanguageTextBox.TabIndex = 1;
            this.mapLanguageTextBox.Text = "x-ptv-DFT";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Towns and streets";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Traffic incidents messages";
            // 
            // trafficIncidentsLanguageTextBox
            // 
            this.trafficIncidentsLanguageTextBox.Location = new System.Drawing.Point(139, 23);
            this.trafficIncidentsLanguageTextBox.Name = "trafficIncidentsLanguageTextBox";
            this.trafficIncidentsLanguageTextBox.Size = new System.Drawing.Size(57, 20);
            this.trafficIncidentsLanguageTextBox.TabIndex = 0;
            this.trafficIncidentsLanguageTextBox.Text = "en";
            // 
            // formsMap
            // 
            this.formsMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.formsMap.Center = ((System.Windows.Point)(resources.GetObject("formsMap.Center")));
            this.formsMap.CoordinateDiplayFormat = Ptv.XServer.Controls.Map.CoordinateDiplayFormat.Degree;
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
            this.formsMap.Size = new System.Drawing.Size(829, 584);
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
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.mapStylesComboBox);
            this.groupBox5.Location = new System.Drawing.Point(834, 11);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(202, 54);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Map styles";
            // 
            // mapStylesComboBox
            // 
            this.mapStylesComboBox.FormattingEnabled = true;
            this.mapStylesComboBox.Location = new System.Drawing.Point(7, 20);
            this.mapStylesComboBox.Name = "mapStylesComboBox";
            this.mapStylesComboBox.Size = new System.Drawing.Size(187, 21);
            this.mapStylesComboBox.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.contentSnapshotsComboBox);
            this.groupBox6.Controls.Add(this.label5);
            this.groupBox6.Location = new System.Drawing.Point(834, 532);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(201, 52);
            this.groupBox6.TabIndex = 10;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Content snapshots";
            // 
            // contentSnapshotsComboBox
            // 
            this.contentSnapshotsComboBox.FormattingEnabled = true;
            this.contentSnapshotsComboBox.Location = new System.Drawing.Point(32, 21);
            this.contentSnapshotsComboBox.Name = "contentSnapshotsComboBox";
            this.contentSnapshotsComboBox.Size = new System.Drawing.Size(162, 21);
            this.contentSnapshotsComboBox.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "ID";
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.requestLoggingTextBox);
            this.groupBox7.Location = new System.Drawing.Point(0, 590);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(1035, 131);
            this.groupBox7.TabIndex = 11;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Request logging";
            // 
            // requestLoggingTextBox
            // 
            this.requestLoggingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.requestLoggingTextBox.Location = new System.Drawing.Point(6, 19);
            this.requestLoggingTextBox.Multiline = true;
            this.requestLoggingTextBox.Name = "requestLoggingTextBox";
            this.requestLoggingTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.requestLoggingTextBox.Size = new System.Drawing.Size(1022, 103);
            this.requestLoggingTextBox.TabIndex = 0;
            this.requestLoggingTextBox.TabStop = false;
            this.requestLoggingTextBox.Text = "Start logging:";
            this.requestLoggingTextBox.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 724);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.formsMap);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "XMap2 Layer Factory";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Ptv.XServer.Controls.Map.FormsMap formsMap;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        internal System.Windows.Forms.CheckedListBox featureLayerCheckedListBox;
        internal System.Windows.Forms.TextBox timeSpanTextBox;
        internal System.Windows.Forms.RadioButton timeSpanRadioButton;
        internal System.Windows.Forms.RadioButton optimisticRadioButton;
        internal System.Windows.Forms.RadioButton snapshotRadioButton;
        internal System.Windows.Forms.RadioButton noneTimeConsiderationRadioButton;
        internal System.Windows.Forms.CheckBox showOnlyRelevantCheckBox;
        internal System.Windows.Forms.DateTimePicker referenceTimeDatePicker;
        internal System.Windows.Forms.DateTimePicker referenceTimeTimePicker;
        internal System.Windows.Forms.TextBox timeZoneTextBox;
        internal System.Windows.Forms.TextBox trafficIncidentsLanguageTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        internal System.Windows.Forms.TextBox mapLanguageTextBox;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ComboBox mapStylesComboBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.ComboBox contentSnapshotsComboBox;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.TextBox requestLoggingTextBox;
    }
}

