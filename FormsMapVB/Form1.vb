Imports Ptv.XServer.Controls.Map.Layers.Shapes
Imports Ptv.XServer.Controls.Map.Symbols
Imports System.Windows.Media
Imports System.Linq
Imports FormsMapVB.XRouteServiceReference
Imports Ptv.XServer.Controls.Map
Imports Point = System.Windows.Point

Public Class Form1
    Dim token = "EBB3ABF6-C1FD-4B01-9D69-349332944AD9"

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Initialize()
    End Sub

    Private Async Sub Initialize()
        ' initialize base map (for xServer internet)
        FormsMap1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap"
        FormsMap1.XMapCredentials = "xtok:" + token

        ' add a new Shape Layer
        Dim layer = New ShapeLayer("MyShapes")
        FormsMap1.Layers.Add(layer)

        Dim startPoint = New Point(8.4, 49)
        Dim destPoint = New Point(8.4, 50)

        ' set map view
        FormsMap1.SetEnvelope(New MapRectangle({startPoint, destPoint}).Inflate(1.25))

        ' create start marker
        Dim startMarker = New Truck With
            {
                .Color = Colors.Blue,
                .Width = 50,
                .ToolTip = "Start"
            }

        ' set position and add to map
        ShapeCanvas.SetLocation(startMarker, startPoint)
        ShapeCanvas.SetZIndex(startMarker, 10)
        layer.Shapes.Add(startMarker)

        ' create destination marker
        Dim destMarker = New Pyramid With
            {
                .Color = Colors.Green,
                .Width = 50,
                .Height = 50,
                .ToolTip = "Destination"
            }

        ' set position and add to map
        ShapeCanvas.SetLocation(destMarker, destPoint)
        ShapeCanvas.SetZIndex(destMarker, 10)
        layer.Shapes.Add(destMarker)

        ' calculate route, non-blocking
        Dim route = Await Task.Run(Function()
                                       Return CalcRoute(startPoint.Y, startPoint.X, destPoint.Y, destPoint.X)
                                   End Function)

        ' display route
        SetRoute(route, layer, Colors.Blue, "Route")
    End Sub

    Private Function CalcRoute(lat1, lon1, lat2, lon2)
        Dim xRouteClient = New XRouteWSClient()

        xRouteClient.ClientCredentials.UserName.UserName = "xtok"
        xRouteClient.ClientCredentials.UserName.Password = token

        ' just need the distance
        Return xRouteClient.calculateRoute(New WaypointDesc() {New WaypointDesc() With {
             .wrappedCoords = New FormsMapVB.XRouteServiceReference.Point() {New FormsMapVB.XRouteServiceReference.Point() With {
                 .point = New PlainPoint() With {
                     .x = lon1,
                     .y = lat1
                }
            }}
        }, New WaypointDesc() With {
             .wrappedCoords = New FormsMapVB.XRouteServiceReference.Point() {New FormsMapVB.XRouteServiceReference.Point() With {
                 .point = New PlainPoint() With {
                     .x = lon2,
                     .y = lat2
                }
            }}
        }}, Nothing, Nothing, New ResultListOptions() With {.polygon = True}, New CallerContext() With {
             .wrappedProperties = New CallerContextProperty() {New CallerContextProperty() With {
                 .key = "CoordFormat",
                 .value = "OG_GEODECIMAL"
            }}
        })
    End Function

    Private Sub SetRoute(route As FormsMapVB.XRouteServiceReference.Route, layer As ShapeLayer, color As Color, toolTip As String)
        Dim r = route.polygon.lineString.wrappedPoints
        Dim pc = New PointCollection(From p In r Select New System.Windows.Point(p.x, p.y))

        Dim poly = New MapPolyline With
        {
            .Points = pc,
            .MapStrokeThickness = 40,
            .StrokeLineJoin = PenLineJoin.Round,
            .StrokeStartLineCap = PenLineCap.Flat,
            .StrokeEndLineCap = PenLineCap.Triangle,
            .Stroke = New SolidColorBrush(color),
            .ScaleFactor = 0.2,
            .ToolTip = toolTip
        }

        layer.Shapes.Add(poly)
    End Sub
End Class
