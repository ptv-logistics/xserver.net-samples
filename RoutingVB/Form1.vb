Imports Ptv.XServer.Controls.Map.Layers.Shapes
Imports Ptv.XServer.Controls.Map.Symbols
Imports System.Windows.Media
Imports System.Linq
Imports FormsMapVB.XRouteServiceReference
Imports Ptv.XServer.Controls.Map
Imports Point = System.Windows.Point
Imports Ptv.XServer.Demo.UseCases.RoutingDragAndDrop

Public Class Form1
    Dim token = "9358789A-A8CF-4CA8-AC99-1C0C4AC07F1E"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Initialize()
    End Sub

    Private Async Sub Initialize()
        ' initialize base map (for xServer internet)
        FormsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap"
        FormsMap1.XMapCredentials = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9:" + token

        Dim startPoint = New Point(8.4, 49)
        Dim destPoint = New Point(8.4, 50)

        ' set map view
        FormsMap1.SetEnvelope(New MapRectangle({startPoint, destPoint}).Inflate(1.25))

        Dim dd = New RoutingDragAndDropUseCase(FormsMap1.WrappedMap, "https://xroute-eu-n-test.cloud.ptvgroup.com/xroute/ws/XRoute", "EBB3ABF6-C1FD-4B01-9D69-349332944AD9", token)
        dd.SetRoute(startPoint, destPoint)
        dd.Enable()

    End Sub
End Class
