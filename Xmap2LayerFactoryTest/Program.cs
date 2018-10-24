using System;
using System.Windows.Forms;

namespace XMap2FactoryTest
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        // https://xserver.ptvgroup.com/forum/viewtopic.php?f=14&t=735
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
