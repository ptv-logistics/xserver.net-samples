## xServer .NET under memory pressure

We got some request for hints how to optimize the xServer .NET Control for scenarios where the memory for .NET is limited. This is mainly the case when the host applications runs at 32-Bit,for as Excel or Outlook plugin

![xServer.NET control running in as (32-Bit) Mircosoft Word Plugin](https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/XServerNetOffice.png =250x)

###1 Optimizations in the latest version
WPF has some memory issues for bitmap images, and the map control uses many of these. We we applied some tweaks to release memory earlier for bitmaps. if you're interested in details about the optimization, read here http://code.logos.com/blog/2008/04/memory_leak_with_bitmapimage_and_memorystream.html. 

You can get the latest stable version of the control with this optimization here https://github.com/ptv-logistics/xservernet-bin/tree/master/Lib

###2 Tweak the control
WPF has a weird behavior for bitmap images that triggers the garbage collector very often. This leads to a very annoying problem that an application that utilized much memory (> 1GB) starts to stutter when scrolling in the map. For details see here http://code.logos.com/blog/2008/04/memory_leak_with_bitmapimage_and_memorystream.html for dedails. For this reason we disabled this behavior. The latest stable version does this only for 64-Bit applications, for 32-Bit applications this may raise issues under memory pressure. You can override the default behavior with the global options property.
```
Ptv.XServer.Controls.Map.GlobalOptions.MemoryPressureMode = MemoryPressureMode.Enable;
```

Another option is to reduce the number of images that are kept in-memory. The default is 512.
```
Ptv.XServer.Controls.Map.GlobalOptions.TileCacheSize = 128;
```

###3 Set the LAA-Flag

32-Bit applications can use 2GB (31-Bit) of address space on a 32-Bit OS. The 32nd Bit is reserved by the Kernel Space. The 32nd Bit is also reserved on 64-Bit OS for compatibility reasons, even though the Kernel Space is outside this range. If your 32-Bit application doesn't abuse the 32nd-Bit for anything evil, you can set the LAA flag for the .exe-file to allow the process to use 4GB. This should be the case for normal application. Here is a very good article on this issue (in german) http://www.3dcenter.org/artikel/das-large-address-aware-flag

![3dcenter article about the LAA flag](https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/64bitnolaa.png)

###4 Hunt down your memory leaks
We continously search for memory leaks, and workaround the memory leaks inside WPF for the map control. It's a common misconception that the garbage collector prevents .NET from having memeory leaks. Especially UI applications are affected by this, as lapsed event listener are a hideous source for leaks. Tool like the SciTech memory profiler can help to search for leaks http://memprofiler.com/?gclid=CIDHmZHxicYCFWXLtAodzzsAIQ

![3dcenter article about the LAA flag](https://github.com/ptv-logistics/xservernet-bin/blob/master/MemoryPressureTest/screenshots/scitech.png)

###5 Testing under critical contditions
The sample project builds a 32-Bit applications and allocates a large pile of memory to test the optimizations. Get the source code here https://github.com/ptv-logistics/xservernet-bin/tree/master/MemoryPressureTest

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
