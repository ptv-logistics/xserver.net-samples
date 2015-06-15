## Tips to optimize xServer.NET for limited-memory scenarios

We got some requests for hints how to optimize the xServer .NET map control for scenarios where the memory for .NET is limited. This is mainly the case when the host application runs at 32-Bit, for example as Excel or Outlook plugin.

<img src="https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/XServerNetOffice.png"  alt="xServer.NET control running in as (32-Bit) Mircosoft Word Plugin" height="240"/>

Optimizing memory is a challenging task, especially in a managed (i.e. garbage-collected) runtime like .NET. These are the tips to optimize your application.

###1 Optimizations in the latest version
WPF has some memory issues for bitmap images, and the map control uses many of those. We applied some tweaks to release memory earlier for bitmaps. If you're interested in details about the optimization, read here http://code.logos.com/blog/2008/04/memory_leak_with_bitmapimage_and_memorystream.html. 

You can get the latest stable version of the control with this optimization here https://github.com/ptv-logistics/xservernet-bin/tree/master/Lib

###2 Tweak the control
WPF has a weird behavior for bitmap images that triggers the garbage collector very often. This causes a very annoying problem: An application that utilizes much memory (> 1GB) starts to stutter when scrolling in the map. For details read here http://stackoverflow.com/questions/7331735/gc-is-forced-when-working-with-small-images-4k-pixel-data. Because of this issue we bypass this behavior in the control. The latest stable version does this only for 64-Bit applications, for 32-Bit applications this may raise issues under memory pressure, so we use the standard behavior. You can override this automatic behavior with the global options property.
```
Ptv.XServer.Controls.Map.GlobalOptions.MemoryPressureMode = MemoryPressureMode.Enable;
```

Another option is to reduce the number of images that are kept in-memory. The default is 512.
```
Ptv.XServer.Controls.Map.GlobalOptions.TileCacheSize = 128;
```

###3 Set the LAA-Flag

32-Bit applications can use 2GB (31-Bit) of address space on a 32-Bit OS. The 32nd Bit is reserved for the Kernel Space. The 32nd Bit is also reserved on 64-Bit OS for compatibility reasons, even though the Kernel Space is outside this range. If your 32-Bit application doesn't abuse the 32nd-Bit for anything evil, you can set the LAA flag for the .exe-file to allow the process to use 4GB. This should be the case for any normal application. 

Here is a very good article on this issue (in german) http://www.3dcenter.org/artikel/das-large-address-aware-flag

<img src="https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/64bitnolaa.png" alt="3dcenter article about the LAA flag" height="240"/>

###4 Hunt down your memory leaks
We continously search for memory leaks in xServer .NET, and work around the memory leaks inside WPF for the map control. It's a common misconception that the garbage collector prevents .NET from having memeory leaks. Mainly UI applications are affected by this, as lapsed event listeners are a hideous source for leaks. 

Tools like the SciTech memory profiler can help to search for leaks in your .NET application http://memprofiler.com/?gclid=CIDHmZHxicYCFWXLtAodzzsAIQ

![3dcenter article about the LAA flag](https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/scitech.png)

###5 Testing under critical conditions
Our test project builds a 32-Bit application and allocates a large pile of memory to test the optimizations. Get the source code here https://github.com/ptv-logistics/xservernet-bin/tree/master/MemoryPressureTest

```
private static IntPtr largeUnmanagedHeap;

#region constructor
/// <summary> Constructor of the main window. </summary>
public MainWindow()
{
    // simulate a large pile (1.3 GB) of unmanaged memory
    // we're running in 32-Bit and only have 2GB per-process
    // so there isn't much left
    largeUnmanagedHeap = Marshal.AllocHGlobal(1300000000);

    // Enable the incrementation of memory pressure for bitmap images
    // This is default for 32-Bit but disabled for 64-Bit applications
    // Must be called before the first initialization of the map control!
    Ptv.XServer.Controls.Map.GlobalOptions.MemoryPressureMode = MemoryPressureMode.Enable;
            
    // decrease the tile cache, the default is 512
    // Must be called before the first initialization of the map control!
    Ptv.XServer.Controls.Map.GlobalOptions.TileCacheSize = 128;

    // now initialize the map
    InitializeComponent();
}          
```
