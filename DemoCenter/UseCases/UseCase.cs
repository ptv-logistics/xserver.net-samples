// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo.UseCases
{
    /// <summary>
    /// Use cases are intended to show some functionality in combination with the PTV xServers. This functionality
    /// is disabled at first to avoid an overloading of information presented on the GUI. The instances of this class are
    /// commonly triggered by a check box, enabling and disabling the use case. This activation mode is managed by this
    /// base class.
    /// Additionally, an authentication framework is provided, but which is only needed for cloud-based xServers. 
    /// For on-premise configurations only the URL is taken into consideration. Mainly this authentication framework is
    /// used by derived classes to integrate layers into the map, for example. For simplicity reasons, all use case instances
    /// work in combination with exactly one WPF map, commonly to show some layer content in this map.
    /// </summary>
    public abstract class UseCase
    {
        private bool isEnabled;
        protected WpfMap wpfMap;

        /// <summary>
        /// Activation/deactivation of the use case specified according the value in parameter <paramref name="enable"/>.
        /// Due to the fact, that a use case can only be used for one WPF map, a deactivation is performed, when an
        /// eventually different WPF map was set previously.
        /// </summary>
        /// <remarks>
        /// Internally, the abstract methods <see cref="Enable"/> and <see cref="Disable"/> are called for the concrete functionality
        /// provided by the derived use case class.
        /// </remarks>
        /// <param name="enable"></param>
        /// <param name="usedWpfMap"></param>
        public void Activate(bool enable, WpfMap usedWpfMap)
        {
            if ((isEnabled && enable && Equals(usedWpfMap, wpfMap)) || (!isEnabled && !enable))
                return; // No changes --> nothing to do

            if (enable && !Equals(usedWpfMap, wpfMap))
              Activate(false, wpfMap); // Compensate a missing deactivation

            isEnabled = enable;
            wpfMap = usedWpfMap;

            if (enable) Enable(); else Disable();
        }

        /// <summary>
        /// Abstract method which can be overridden to implement the actual functionality of this use case.
        /// </summary>
        protected abstract void Enable();

        /// <summary>
        /// Abstract method which can be overridden to potentially remove resources allocated in method <see cref="Enable"/>.
        /// </summary>
        protected abstract void Disable();


        /// <summary>
        /// In comparison to the class <see cref="XMapMetaInfo"/>, this class encapsulates it and provides some additional 
        /// handling of the authentication's persistence data. After a successful configuration to a (maybe Cloud hosted)
        /// xServer, the URL and authentication data is stored in the application settings. 
        /// </summary>
        public static class ManagedAuthentication
        {
            /// <summary>
            /// For simplicity reasons, only a singleton object, responsible for the management of the authentication data,
            /// is implemented.
            /// The URL needed for XMap is predefined in the settings.settings file of the Demo Center project.
            /// If an Azure deployment of xServer is used, a user and a password have to be specified. By default,
            /// the xToken mechanism is used, whereby the constant user "xtok" is used in combination with the
            /// customer provided xToken.
            /// </summary>
            static ManagedAuthentication()
            {
                var args = ParseCommandLine();

                if (args.ContainsKey("xurl"))
                    Properties.Settings.Default.XUrl = args["xurl"];

                if (args.ContainsKey("xtoken"))
                    Properties.Settings.Default.XToken = args["xtoken"];

                Set(Properties.Settings.Default.XUrl, Properties.Settings.Default.XToken);
            }

            /// <summary>
            /// Parses the command line into a dictionary.
            /// </summary>
            /// <returns>Parsed command line.</returns>
            private static Dictionary<string, string> ParseCommandLine()
            {
                var argsDict = new Dictionary<string, string>();

                var args = Environment.GetCommandLineArgs()
                    .Select(arg => new {arg, match = Regex.Match(arg, @"^[/-]([^:]+)(?::(.+))?$") })
                    .ToArray();

                for (var i = 0; i < args.Length; ++i)
                    if (args[i].match.Success) argsDict.Add(
                        args[i].match.Groups[1].Value.ToLowerInvariant(),
                        args[i].match.Groups[2].Success
                            ? args[i].match.Groups[2].Value
                            : (i + 1) < args.Length && !args[i + 1].match.Success
                                ? args[++i].arg
                                : "true"
                    );

                return argsDict;
            }

            /// <summary>
            /// Property containing (among other things) the URL of the XServers. If a cloud deployment of PTV xServer is addressed,
            /// it contains the xToken too. This property is primary used to create layers provided by xServers hosted in the cloud.
            /// </summary>
            public static XMapMetaInfo XMapMetaInfo { get; private set; }

            /// <summary>
            /// Setting all relevant information for a PTV xServer, consisting of an URL and, in case of an on-premise xServer installation, 
            /// an customer-provided xToken. The internally available user-password-pair is not used due to its visibility during sending
            /// requests to the xServer.
            /// </summary>
            /// <param name="url">URL of the xServer installation.</param>
            /// <param name="xToken">Value provided to the customer, which grants access only to the xServer API, and no further
            /// rights in the context of the internal user management.</param>
            /// <returns>False if the settings denied access due to a wrong URL address or authentication data, otherwise true.</returns>
            public static bool Set(string url, string xToken)
            {
                try
                {
                    #region doc:CheckConnection
                    var newXMapMetaInfo = new XMapMetaInfo(url);
                    newXMapMetaInfo.SetCredentials("xtok", xToken);
                    newXMapMetaInfo.CheckCredentials("xtok", xToken); // causes an exception if something goes wrong
                    #endregion

                    XMapMetaInfo = newXMapMetaInfo;
                    IsOk = true;
                    ErrorMessage = string.Empty;

                    // Save settings for the next session
                    Properties.Settings.Default.XUrl = url;
                    Properties.Settings.Default.XToken = xToken;
                    Properties.Settings.Default.Save();
                }
                catch (Exception exception)
                {
                    IsOk = false;
                    ErrorMessage = exception.Message;
                }

                return IsOk;
            }

            /// <summary>
            /// Flag indicating if a previously set authentication was successful.
            /// </summary>
            public static bool IsOk { get; private set; }

            /// <summary>
            /// If authentication is not possible, this error message is available from the authentication check.
            /// </summary>
            public static string ErrorMessage { get; private set; }
        }
    }
}
