// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Ptv.XServer.Demo.XlocateService;
using Ptv.XServer.Demo.XrouteService;
using Ptv.XServer.Demo.XtourService;
using Ptv.XServer.Controls.Map.Tools;

namespace Ptv.XServer.Demo.Tools
{
    public class XServerClientFactory
    {
        /// <summary>
        /// Factory for the  of the xServer generated client class granting access to the xRoute server.
        /// If xServers are used on an Azure environment, authentication data has to be integrated when
        /// requests are made.
        /// </summary>
        /// <param name="xUrl">The xServer base url. </param>
        /// <returns></returns>
        public static XRouteWSClient CreateXRouteClient(string xUrl)
        {
            string completeXServerUrl = XServerUrl.Complete(xUrl, "XRoute");

            var binding = new BasicHttpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 0, 30),
                SendTimeout = new TimeSpan(0, 0, 30),
                OpenTimeout = new TimeSpan(0, 0, 30),
                CloseTimeout = new TimeSpan(0, 0, 30),
                MaxReceivedMessageSize = int.MaxValue
            };

            var endpoint = new EndpointAddress(completeXServerUrl);
            var client = new XRouteWSClient(binding, endpoint);

            if (!XServerUrl.IsXServerInternet(completeXServerUrl)) return client;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            client.ClientCredentials.SetConfiguredToken();

            return client;
        }

        /// <summary>
        /// Factory for the  of the xServer generated client class granting access to the xRoute server.
        /// If xServers are used on an Azure environment, authentication data has to be integrated when
        /// requests are made.
        /// </summary>
        /// <param name="xUrl">The xServer base url. </param>
        /// <returns></returns>
        public static XLocateWSClient CreateXLocateClient(string xUrl)
        {
            string completeXServerUrl = XServerUrl.Complete(xUrl, "XLocate");

            var binding = new BasicHttpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 0, 30),
                MaxReceivedMessageSize = int.MaxValue
            }; 
            
            var endpoint = new EndpointAddress(completeXServerUrl);
            var client = new XLocateWSClient(binding, endpoint);

            if (!XServerUrl.IsXServerInternet(completeXServerUrl)) return client;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            client.ClientCredentials.SetConfiguredToken();

            return client;
        }

            /// <summary>
        /// Factory for the  of the xServer generated client class granting access to the xRoute server.
        /// If xServers are used on an Azure environment, authentication data has to be integrated when
        /// requests are made.
        /// </summary>
        /// <param name="xUrl">The xServer base url. </param>
        /// <returns></returns>
        public static XTourWSClient CreateXTourClient(string xUrl)
        {
            string completeXServerUrl = XServerUrl.Complete(xUrl, "XTour");

            var binding = new BasicHttpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 0, 30),
                MaxReceivedMessageSize = int.MaxValue
            }; 
            
            var endpoint = new EndpointAddress(completeXServerUrl);
            var client = new XTourWSClient(binding, endpoint);

            if (!XServerUrl.IsXServerInternet(completeXServerUrl)) return client;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            client.ClientCredentials.SetConfiguredToken();

            return client;
        }
    }


    internal static class XServerWSClient
    {
        public static void SetConfiguredToken(this ClientCredentials cred)
        {
            string token = Properties.Settings.Default.XToken;
            if (String.IsNullOrEmpty(token))
                return;

            cred.UserName.UserName = "xtok";
            cred.UserName.Password = token;
        }
    }
}
