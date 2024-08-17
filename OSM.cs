using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P3D_Scenario_Generator
{
    internal class OSM
    {

        internal static string GetOSMtileURL(double lon, double lat, int zoom, out int xOffset, out int yOffset)
        {
            // Convert the coordinate to the Web Mercator projection
            double xWM = lon;
            double yWM = Math.Asinh(Math.Tan(lat));

            // Transform the projected point onto the unit square
            double x = 0.5 + xWM / 360;
            double y = 0.5 - yWM / 2 * Math.PI;

            // Transform the projected point into tile space
            double N = Math.Pow(2, zoom);

            double xTileD = N * x;
            double xTileTruncated = Math.Truncate(xTileD);
            int xTileInt = Convert.ToInt32(xTileTruncated);
            double xTileFractional = xTileD - xTileInt;

            double yTileD = N * y;
            double yTileTruncated = Math.Truncate(yTileD);
            int yTileInt = Convert.ToInt32(Math.Abs(yTileTruncated));
            double yTileFractional = yTileD - yTileInt;

            // Calculate offsets
            xOffset = Convert.ToInt32(256 * xTileFractional);
            yOffset = Convert.ToInt32(256 * yTileFractional);

            return $"https://tile.openstreetmap.org/{zoom}/{xTileInt}/{yTileInt}.png";
        }
    }
}
