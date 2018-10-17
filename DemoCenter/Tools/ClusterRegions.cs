// This source file is covered by the LICENSE.TXT file in the root folder of the SDK.

using Ptv.XServer.Controls.Map.Tools;
using System;
using System.Text.RegularExpressions;

namespace Ptv.XServer.Demo.Tools
{
    public static class ClusterRegions
    {
        /// <summary>
        /// Helper class that returns a region for an xServer meta info
        /// </summary>
        /// <param name="info">The meta info class</param>
        /// <returns>The region</returns>
        public static Region GetRegion(this XMapMetaInfo info)
        {
            var regex = new Regex(@"xmap-([a-z]+-[hnt](?:-test|-integration)?)\.cloud\.ptvgroup\.com", RegexOptions.IgnoreCase);
            var match = regex.Match(info.Url);
            var cluster = (match.Success) ? match.Groups[1].ToString() : String.Empty;

            if (string.IsNullOrEmpty(cluster))
                return Region.world;

            if (cluster.Contains("eu-n") || cluster.Contains("eu-t"))
                return Region.eu;
            else if (cluster.Contains("na-n") || cluster.Contains("na-t"))
                return Region.na;
            else if (cluster.Contains("au-n") || cluster.Contains("au-t"))
                return Region.au;
            else
                return Region.world;
        }
    }

    // The regions for Demo center data
    public enum Region
    {
        eu,
        na,
        au,
        world
    }
}
