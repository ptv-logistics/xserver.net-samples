<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.FormsMap1 = New Ptv.XServer.Controls.Map.FormsMap()
        Me.SuspendLayout()
        '
        'FormsMap1
        '
        Me.FormsMap1.Center = CType(resources.GetObject("FormsMap1.Center"), System.Windows.Point)
        Me.FormsMap1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.FormsMap1.FitInWindow = False
        Me.FormsMap1.InvertMouseWheel = False
        Me.FormsMap1.Location = New System.Drawing.Point(0, 0)
        Me.FormsMap1.MaxZoom = 19
        Me.FormsMap1.MinZoom = 0
        Me.FormsMap1.MouseWheelSpeed = 0.5R
        Me.FormsMap1.Name = "FormsMap1"
        Me.FormsMap1.ShowCoordinates = True
        Me.FormsMap1.ShowLayers = True
        Me.FormsMap1.ShowMagnifier = True
        Me.FormsMap1.ShowNavigation = True
        Me.FormsMap1.ShowOverview = True
        Me.FormsMap1.ShowScale = True
        Me.FormsMap1.ShowZoomSlider = True
        Me.FormsMap1.Size = New System.Drawing.Size(784, 562)
        Me.FormsMap1.TabIndex = 0
        Me.FormsMap1.UseAnimation = True
        Me.FormsMap1.UseDefaultTheme = True
        Me.FormsMap1.UseMiles = False
        Me.FormsMap1.XMapCopyright = "Please configure a valid copyright text!"
        Me.FormsMap1.XMapCredentials = ""
        Me.FormsMap1.XMapStyle = ""
        Me.FormsMap1.XMapUrl = ""
        Me.FormsMap1.ZoomLevel = 1.0R
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(784, 562)
        Me.Controls.Add(Me.FormsMap1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents FormsMap1 As Ptv.XServer.Controls.Map.FormsMap

End Class
