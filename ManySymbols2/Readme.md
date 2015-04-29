## ManySymbols2

This sample shows how to render serveral thousands of objects on the xServer .NET map control.

While the standard ShapeLayer can be tweaked to render several thousand objects (see sample "ManySymbols"), it is rather optimized 
for ease-of-use than for performance. If you want to display tens of thousands objects, it is more efficient to implement your own layer-rendering.

The sample implements a custom layer which displays a list of locations (25,000) on a map. The source for the locations is a .csv-file, 
but the practice can be applied to any other data source. The different locations can be styled with different colors, sizes and symbol-types.

The symbols are WPF objects and support the full WPF event-hanling (Mouse-Over, Click, Drag&Drop, ...). 

This practice is sufficient for up to about 100,000 locations. To display even more locations or other complex data, we recommend 
either use clustering (see Clustering-Demo in the SDK) or switch from WPF to GDI-based rendering (see Map&Market/Shape-File Demo in the SDK).