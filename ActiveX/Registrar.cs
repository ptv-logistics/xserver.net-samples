using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Ptv.XServer.Controls
{
    /// <summary>
    /// The Registrar provides the necessary extension methods to 
    /// register .NET based ActiveX controls.
    /// </summary>
    internal static class Registrar
    {
        /// <summary>
        /// Writes additional information to the registry, registering 
        /// the specified type (expected to be a control) with the specified 
        /// key as an ActiveX control.
        /// </summary>
        /// <param name="t">The type of the control to register</param>
        /// <param name="key">The key of the control to register</param>
        public static void RegisterClass(this Type t, string key)
        {
            // Strip off HKEY_CLASSES_ROOT\ from the passed key as I don't need it
            var sb = new StringBuilder(key);
            sb.Replace(@"HKEY_CLASSES_ROOT\", "");

            // Open the CLSID\{guid} key for write access
            using (var registryKey = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true))
            {
                // change title to ProgID
                object progIdAttribute = t.GetCustomAttributes(typeof(ProgIdAttribute), false)[0];
                registryKey.SetValue(null, ((ProgIdAttribute)progIdAttribute).Value);

                // add version information
                using (var versionKey = registryKey.CreateSubKey("Version"))
                    versionKey.SetValue(null, t.Assembly.GetName().Version);

                // And create the 'Control' key - this allows it to show up in 
                // the ActiveX control container 
                using (registryKey.CreateSubKey("Control"))
                {}

                // Add TypeLib key - this is not done by regasm
                object guidAttribute = t.Assembly.GetCustomAttributes(typeof(GuidAttribute), false)[0];
                using (var typeLibKey = registryKey.CreateSubKey("TypeLib"))
                    typeLibKey.SetValue(null, "{" + ((GuidAttribute) guidAttribute).Value + "}");
            }
        }

        /// <summary>
        /// Removes the additional information written by <see cref="RegisterClass"/> 
        /// from the registry, fully unregister the specified type as an ActiveX control.
        /// </summary>
        /// <param name="t">The type of the control to unregister</param>
        /// <param name="key">The key of the control to unregister</param>
        public static void UnregisterClass(this Type t, string key)
        {
            var stringBuilder = new StringBuilder(key);
            stringBuilder.Replace(@"HKEY_CLASSES_ROOT\", "");

            // Open HKCR\CLSID\{guid} for write access
            using (var registryKey = Registry.ClassesRoot.OpenSubKey(stringBuilder.ToString(), true))
            {
                // do not fail if key does not exist
                if (registryKey == null) return;

                // Delete the 'Control' key, but don't throw an exception if it does not exist
                registryKey.DeleteSubKey("Control", false);

                // Delete TypeLib key
                registryKey.DeleteSubKey("TypeLib", false);

                // And delete the CodeBase key, again not throwing if missing 
                registryKey.DeleteSubKey("CodeBase", false);

                // Delete Version key
                registryKey.DeleteSubKey("Version", false);
            }
        }
    }
}
