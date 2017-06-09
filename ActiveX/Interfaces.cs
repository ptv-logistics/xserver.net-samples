using System;
using System.Runtime.InteropServices;

namespace Ptv.XServer.Controls
{
    /// <summary>
    /// Defines the events fired by the ActiveX control.
    /// </summary>
    [Guid("C7E5BC04-0B71-488E-9990-567C059DC0D7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMapEvents
    {
        /// <summary>
        /// <see cref="OnShapeClicked"/> is fired when the user clicks on any 
        /// user-defined shape that was defined through <see cref="IShape"/>.
        /// </summary>
        /// <param name="id">The ID of the cliecked shape, as specified in the call 
        /// to the corresponding <see cref="IShape"/> method.</param>
        [DispId(1)]
        void OnShapeClicked(int id);
    }

    /// <summary>
    /// Groups the shape functionality in a single interface. The <see cref="IShape"/> 
    /// implementation can be accessed through <see cref="IMapControl"/>.
    /// </summary>
    [Guid("3CC9BD22-FA0F-4F93-87A2-E05BF67F3524")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IShapes
    {
        /// <summary>
        /// Returns the number of shapes currently defined.
        /// </summary>
        [DispId(1)]
        int Count { get; }

        /// <summary>
        /// Removes all shapes.
        /// </summary>
        [DispId(2)]
        void Clear();

        /// <summary>
        /// Adds a marker to the map.
        /// </summary>
        /// <param name="id">Unique shape ID.</param>
        /// <param name="x">x-coordinate of the marker.</param>
        /// <param name="y">y-coordiante of the marker.</param>
        /// <param name="size">Size of the marker, in pixels.</param>
        /// <param name="color">Color, specified in as HTML string. See remarks.</param>
        /// <param name="Symbol">The name of the marker to display. Defaults to "Pin" if empty. See remarks.</param>
        /// <param name="toolTip">An optional tool tip to be assigned with the marker.</param>
        /// <returns>True, if the marker was added to the shape layer. False otherwise.</returns>
        /// <remarks>Be sure to use unique IDs as <see cref="AddMarker"/> will fail if an element with the 
        /// specified ID already exists, may it be a line or a marker. The marker location is to be specified 
        /// using lat/lon co-ordinates (aka WGS84, EPSG:4326). Please note that drawing order equals the order 
        /// in which the shapes are defined through <see cref="IShape"/>. At the time of writing, the color 
        /// parameter is only applicable to a pin-type marker. Any other marker, named by the Symbol parameter, 
        /// uses its default color. Valid values for Symbol are the class names of the symbols defined in the 
        /// Ptv.XServer.Controls.Map.Symbols namespace - at the time of writing Ball, Crosshair, Cube, 
        /// Diamond, Hexagon, Pentagon, Pin, Pyramid, Star, TriangleDown and TriangleUp. Please refer to the
        /// PTV xServer .NET documentation for details.</remarks>
        [DispId(3)]
        bool AddMarker(int id, double x, double y, int size, string color, string Symbol, string toolTip);

        /// <summary>
        /// Adds a line to the map.
        /// </summary>.
        /// <param name="id">Unique shape ID.</param>
        /// <param name="coordinates">The line coordinates, specified as an array of 
        /// double values, representing the co-ordinates ([x0,y0,x1,y1,...,xn,yn]).</param>
        /// <param name="size">The size of the line, in pixels.</param>
        /// <param name="color">Color, in HTML format.</param>
        /// <param name="toolTip">Optional tool tip to be assigned with the line.</param>
        /// <returns>True, if the line was added to the shape layer. False otherwise.</returns>
        /// <remarks>Be sure to use unique IDs as <see cref="AddMarker"/> will fail if an element with the 
        /// specified ID already exists, may it be a line or a marker. The line's coordinates are to be 
        /// specified as lat/lon (aka WGS84, EPSG:4326). Please note that the drawing order equals the order 
        /// in which the shapes are defined through <see cref="IShape"/>.</remarks>
        [DispId(4)]
        bool AddLine(int id, object coordinates, int size, string color, string toolTip);

        /// <summary>
        /// Removes a specific shape, specified by its ID.
        /// </summary>
        /// <param name="id">The ID of the shape to be removed.</param>
        /// <returns></returns>
        [DispId(5)]
        bool Remove(int id);
    }

    /// <summary>
    /// This is the main interface of the ActiveX map control. 
    /// All functionality is accessed through this interfaces.
    /// </summary>
    [Guid("CFDB280A-8005-47AC-8229-0D988A35C36D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMapControl
    {
        /// <summary>
        /// Gets or sets the URLs of PTV's xMap Server.
        /// </summary>
        [DispId(1)]
        String XMapUrl { get; set; }

        /// <summary>
        /// Gets or sets the copyright string to be shown in the map.
        /// </summary>
        [DispId(2)]
        String XMapCopyright { get; set; }

        /// <summary>
        /// Sets the map's center and zoom level.
        /// </summary>
        /// <param name="x">X-coordinate of the map center.</param>
        /// <param name="y">Y-coordinate of the map center.</param>
        /// <param name="zoom">Zoom level to set.</param>
        /// <remarks><see cref="SetMapLocation"/> equals the core map control in terms of usage. 
        /// For the sake of simplicity, <see cref="SetMapLocation"/> uses lat/lon (or WGS84, 
        /// EPSG:4326) co-ordinates only. Please refer to the PTV xServer .NET documentation for 
        /// further details on co-ordinates, projections and zoom levels.</remarks>
        [DispId(3)]
        void SetMapLocation(double x, double y, double zoom);

        /// <summary>
        /// Returns the interface used to access the shape API.
        /// </summary>
        [DispId(4)]
        IShapes Shapes { get; }
    }
}
