xserver.net-samples
===================

A collection of samples and demos for [PTV xServer .NET](https://xserverinternet.azurewebsites.net/xserver.net/).


### Resources

* [xserver.net](https://github.com/ptv-logistics/xserver.net) - Source code for PTV xServer.NET
* [xserver.net-docs](https://ptv-logistics.github.io/xserver.net-docs) - API documentation for xServer.NET
* [DemoCenter](https://xserverinternet.azurewebsites.net/xserver.net/) - PTV xServer.NET Demo Center
* [NuGet Package](https://www.nuget.org/packages/Ptv.XServer.Net) - Install-Package Ptv.XServer.Net


### Remarks
For xServer internet an additional authentication is needed compared to an on-premise solution. This authentication consists of a common user/password pair which has to be set at the corresponding property of the Map control. A special user <em>xtok</em> exists which can be used for unregistered clients. The corresponding password, called xToken, can be [obtained](https://www.ptvgroup.com/en/solutions/products/ptv-xserver/test-now) to grant full access of xServer functionality.

Due to this full access the xToken cannot be provided in the source code. Instead, place holders are used. They are replaced when the build process is started by a tool called <em>Insert-XS.Net-Token.exe</em>. When this tool finds an appropriate place holder, it shows a dialog to insert the xToken.

To access multiple sample projects without any tedious repetition of xToken insertion, <em>Insert-XS.Net-Token.exe</em> can be used to substitute the place holders in <strong>all</strong> projects. This tool is located in <em>libs</em> subfolder of the base directory. Via command line add the following parameters:

Insert-XS.Net-Token.exe -path "base directory of xserver.net-samples project"


### The samples

#### [ActiveX](ActiveX) \#interop
Shows how to use the map as ActiveX control

<img src="Screenshots/ActiveX.png" width="360" title="ActiveX">

#### [BasemapTooltips](BasemapTooltips) \#xserver-1
Shows how to display city/street information as tool tip by reverse locating

<img src="Screenshots/BasemapTooltips.png" width="360" title="BasemapTooltips">

#### [Circles](Circles) \#custom-data
Shows how to render circles with a geographic radius

<img src="Screenshots/Circles.png" width="360" title="Circles">

#### [CustomBgProfiles](CustomBgProfiles) \#xserver-1
Shows how to switch the basemap profile 

<img src="Screenshots/CustomBgProfiles.png" width="360" title="CustomBgProfiles">

#### [CustomInit](CustomInit) \#customize
Shows how to explicitly initialize the xMapServer base map

#### [CustomLayout](CustomLayout) \#customize
Shows how to set-up a map-layout with custom gadgets

<img src="Screenshots/CustomLayout.png" width="360" title="CustomLayout">

#### [CustomLocalizer](CustomLocalizer) \#customize
Shows how to use your own string resources to localize the texts of the control.

#### [CustomPanAndZoom](CustomPanAndZoom) \#customize
Shows how to change the default behavior for pan/zoom interaction.

<img src="Screenshots/CustomPanAndZoom.png" width="360" title="CustomPanAndZoom">

#### [DemoCenter](DemoCenter) \#xserver-1
Source code for xServer.NET demo center.

<img src="Screenshots/DemoCenter.png" width="360" title="DemoCenter">

#### [DrawMode](DrawMode) \#customize
The basic code to add an interactor for drawing custom polygons

<img src="Screenshots/DrawMode.png" width="360" title="DrawMode">

#### [DragAndDrop](DragAndDrop) \#customize
Shows how to implement drag&drop for elements on the map.

<img src="Screenshots/DragAndDrop.png" width="360" title="DragAndDrop">

#### [ExtensibilityTest](ExtensibilityTest) \#customize
Tests for customization and extensions of the map control 

<img src="Screenshots/ExtensibilityTest.png" width="360" title="ExtensibilityTest">

#### [FeatureLayers](FeatureLayers) \#xserver-1
Shows how to render Feature Layers on the control

<img src="Screenshots/FeatureLayers.png" width="360" title="FeatureLayers">

#### [FormsMapCS](FormsMapCS) \#hello-world
Shows how to add the map to a Windows Forms application in C#

#### [FormsMapVB](FormsMapVB) \#hello-world
Shows how to add the map to a Windows Forms application in VB.NET

<img src="Screenshots/FormsMapVB.png" width="360" title="FormsMapVB">

#### [leaflet-vb6](https://github.com/ptv-logistics/leaflet-vb6) \#featured \#interop \#vb6 \#javascript \#leaflet
Embed leaflet-based maps within classic desktop applications

<img src="https://github.com/ptv-logistics/leaflet-vb6/blob/master/screenshot.png" width="360" title="leaflet-vb6">

#### [Mandelbrot](Mandelbrot) \#customize
Shows how to implement a client-side tile provider and demonstrates the "Infinite Zoom" feature

<img src="Screenshots/Mandelbrot.png" width="360" title="Mandelbrot">

#### [ManySymbols](ManySymbols) \#customize
Shows practices to display many symbols with the ShapeLayer

<img src="Screenshots/ManySymbols.png" width="360" title="ManySymbols">

#### [ManySymbols2](ManySymbols2) \#customize
Shows how to display even more symbols by implementing a custom layer

<img src="Screenshots/ManySymbols2.png" width="360" title="ManySymbols2">

#### [MapArrowDemo](MapArrowDemo) \#customize
Shows how to build custom shapes for the shape layer

<img src="Screenshots/MapArrowDemo.png" width="360" title="MapArrowDemo">

#### [MFCMapDialog](MFCMapDialog) #\interop
Shows how to add the map to an MFC application

<img src="Screenshots/MFCMapDialog.png" width="360" title="MFCMapDialog">

#### [MultipleContainers](MultipleContainers) \#customize
Test for map controls in multiple tab-, split, and dock-containers

<img src="Screenshots/MultipleContainers.png" width="360" title="MultipleContainers">

#### [MemAssertDemo](MemAssertDemo) \#testing
A utility class + demo that helps to track-down memory-leaks in your code.

#### [MemoryPressureTest](MemoryPressureTest) \#testing
Tips to optimize xServer.NET for limited-memory scenarios

#### [PieChartsAndExport](PieChartsAndExport) \#user-data
Shows how to render arbitrary WPF elements and print/export the map content

<img src="Screenshots/PieChartsAndExport.png" width="360" title="PieChartsAndExport">

#### [Providers](Providers) \#interop \#3rd-party
Shows how to add 3rd-party basemap tiles

<img src="Screenshots/Providers.png" width="360" title="Provieders">

#### [RoutingVB](RoutingVB) \#interop
Shows how to add drag&drop routing to a Visual Basic project.

#### [SelectionDemo](SelectionDemo) \#customize
Shows how to render custom icons and select them by dragging a rubber band

<img src="Screenshots/SelectionDemo.png" width="360" title="SelectionDemo">

#### [ServerSideRendering](ServerSideRendering) \#xserver-1
Shows how to render additional xMap layers with tooltip interaction

<img src="Screenshots/ServerSideRendering.png" width="360" title="ServerSideRendering">

#### [SharpMap.Widgets](https://github.com/ptv-logistics/SharpMap.Widgets) #interop \#featured \#leaflet
Build responsive map applications for web and desktop

<img src="Screenshots/SharpMap.Win.png" width="360" title="SharpMap.Widgets">

#### [SimpleWms](SimpleWms) \#3rd-party
Shows how to add simple WMS (with "Google" EPSG:3857) layers to the control

<img src="Screenshots/SimpleWms.png" width="360" title="WmsTest">

#### [SymbolsAndLabels](SymbolsAndLabels) \#customize
Shows how to draw symbols with an attached label tag

<img src="Screenshots/SymbolsAndLabels.png" width="360" title="SymbolsAndLabels">

#### [TourPlanningDemo](TourPlanningDemo) \#xserver-1
Shows practices how to use the new xTour 1.18 job api @xServer internet

<img src="Screenshots/TourPlanningDemo.png" width="360" title="TourPlanningDemo">

#### [ToursAndStopsMultiCanvas](ToursAndStopsMultiCanvas) \#customize
Shows how to build one logical layer containing different canvases rendered before and after the xMap labels

<img src="Screenshots/ToursAndMultiCanvas.png" width="360" title="ToursAndMultiCanvas">

#### [VdiPerformance](VdiPerformance) \#testing
Benchmark for different settings that influence performance on Virtual Desktop Environments (VDI)VdiPerformance

<img src="Screenshots/VdiPerformance.png" width="360" title="VdiPerformance">

#### [WMTSLayer](WMTSLayer) \#new \#wms \#xserver-2 \#3rd-party
Demonstrates the integration and initialization of a layer showing Web Map Tile Service (WMTS) content.

<img src="Screenshots/WMTSLayer.png" width="360" title="WMTSLayer">

#### [Xmap1Rest](Xmap1Rest) \#xserver-1
Shows how to initialize the base map using the Xmap1 tile and wms APIs.

#### [Xmap2LayerFactoryTest](Xmap2LayerFactoryTest) \#xserver-2
Demonstrates the API for native Xmap2/FeatureLayer support.

<img src="Screenshots/Xmap2LayerFactoryTest.png" width="360" title="Xmap2LayerFactoryTest">

#### [XYNSegments](XYNSegments) \#xserver-1
Shows how to select and render xServer XYN-Segments

<img src="Screenshots/XYNSegments.png" width="360" title="XYNSegments">
