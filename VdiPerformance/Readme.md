# VdiPerformance

VdiPerfomance is a tool to test the xServer.NET map control when running in a VDI (= Virtual Desktop Infrastructure) environment.

Various factors have an impact to the user experience when the application runs remotely: For example network latency, bandwidth and the performance of server and client. For the map control several options can be activated to reduce the traffic, and thereby increace the responsiveness of the map.

This tool lets you play live with these options. You can start it on your remote host and check the behaviour if options are activated or deacitavted.

### The information panel
- **fps:** Displays the estimated frames per second on the host system
- **Hardware Level:** Displays the hardware acceleration level on the host system. 0: No hardware acceleration, 1: DirectX >= 7.0, 2: DirectX >= 9.0

### Generic Options
- **Software Rendering:** Disables the hardware rendering, if available
- **Nearest Neightbour Filter:** This changes the fitering mode for bitmaps from default (bilinear filter) to nearest-neightour. This will create less interpolated pixels, and thereby reduces the sizes of the transferred images.

### Base Map Options
These options use different initializations for the xMapServer layers to reduce the count and content for dynamic updates, so that less images have to be transferred.
- **Flat Profile:** Uses the *gravelpit*-profile instead of the *ajax*-profile for the xServer map. *gravelpit* has no textured forests, and therefore less pixels.
- **Single Tile Background:** Renders the background in a single image (= non-tiled) instead of multiple tiles. This also reduces the traffic.
- **Single Layer BaseMap:** Render the whole xServer map (-bg + -fg) as one single image. **Note:** With this option it is not possible to display client objects under the xMap-labels.

### Navigation Options
These are options to change the behaviour when navigating (zooming, panning) on the map. **Note:** The first two options are implemented by exchanging the xServer.NET "PanAndZoom gadget" with a modified one.
- **Move While Dragging:** When deactivated the map is only dragged after the mouse button is released.
- **Use Filled Pan Shape:** When deactived the rectangle for panning an selecting is not rendered with a semi-opaque filling.
- **Use Animation:** When deactivated transitions from one map viewport to an other vieweport are not animated. Moreover there's no fade-in effect for tiles.

### Symbol Options
These are some generic options for the xServer.NET Shape Layer. These option generally improve the performance, not only for VDI.
- **Lazy Update:** Symbols are only updated (re-scaled) after a map transition
- **Use Bitmap Caching:** The symbols aren't rendered directly as WPF shape, but cached using *RenderTargetBitmap();*

![VdiPerformance](https://github.com/ptv-logistics/xservernet-bin/blob/master/Screenshots/VdiPerformance.png "VdiPerformance")
