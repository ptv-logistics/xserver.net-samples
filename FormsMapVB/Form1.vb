Imports Ptv.XServer.Controls.Map.Layers.Shapes
Imports Ptv.XServer.Controls.Map.Symbols
Imports System.Windows.Media
Imports Point = System.Windows.Point

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' initialize base map (for xServer internet)
        FormsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap"
        FormsMap1.XMapCredentials = "xtok:561677741926322"

        ' go to Karlsruhe
        FormsMap1.SetMapLocation(New Point(8.4, 49), 16)

        ' add a new Shape Layer
        Dim layer = New ShapeLayer("MyShapes")
        FormsMap1.Layers.Add(layer)

        ' create  a truck marker
        Dim marker = New Truck With
            {
                .Color = Colors.Brown,
                .Width = 50,
                .ToolTip = "Hello Map"
            }

        ' set position and add to map
        ShapeCanvas.SetLocation(marker, New Point(8.4, 49))
        layer.Shapes.Add(marker)
    End Sub
End Class
