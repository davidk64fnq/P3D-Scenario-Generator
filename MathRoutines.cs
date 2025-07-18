﻿using P3D_Scenario_Generator.ConstantsEnums;

namespace P3D_Scenario_Generator
{
    class MathRoutines
    {
        static internal void AdjCoords(double dStartLat, double dStartLon, double dHeading, double dDist, ref double dFinishLat, ref double dFinishLon)
        {
            // http://www.movable-type.co.uk/scripts/latlong.html
            // Use this forumla to calculate lat2: =ASIN(SIN(lat1)*COS(d/R) + COS(lat1)*SIN(d/R)*COS(brng))
            double dBrng = dHeading * (Math.PI / 180);      // Convert degrees to radians
            double dLat1 = dStartLat * (Math.PI / 180);     // Convert threshold latitude from degrees to radians
            double dLat2 = Math.Asin(Math.Sin(dLat1) * Math.Cos(dDist / Constants.FeetInRadiusOfEarth) + Math.Cos(dLat1) * Math.Sin(dDist / Constants.FeetInRadiusOfEarth) * Math.Cos(dBrng));

            // Use this forumla to calculate lon2: =lon1 + ATAN2(SIN(brng)*SIN(d/R)*COS(lat1), COS(d/R)-SIN(lat1)*SIN(lat2))
            double dLon1 = dStartLon * (Math.PI / 180);     // Convert threshold longitude from degrees to radians
            double dLon2 = dLon1 + Math.Atan2(Math.Sin(dBrng) * Math.Sin(dDist / Constants.FeetInRadiusOfEarth) * Math.Cos(dLat1), Math.Cos(dDist / Constants.FeetInRadiusOfEarth) - (Math.Sin(dLat1) * Math.Sin(dLat2)));

            // Convert back to degrees
            dFinishLat = dLat2 * (180 / Math.PI);
            dFinishLon = dLon2 * (180 / Math.PI);
        }

        static internal double CalcBearing(double dStartLat, double dStartLon, double dFinishLat, double dFinishLon)
        {
            // http://www.movable-type.co.uk/scripts/latlong.html
            // Use this forumla to calculate bearing: = atan2( sin(lon2 - lon1) * cos(lat2) , cos(lat1) ⋅ sin(lat2) − sin(lat1) ⋅ cos(lat2) ⋅ cos(lon2 - lon1) )
            double dLat1 = dStartLat * (Math.PI / 180);     // Convert dStartLat from degrees to radians
            double dLat2 = dFinishLat * (Math.PI / 180);     // Convert dFinishLat from degrees to radians
            double dLon1 = dStartLon * (Math.PI / 180);     // Convert dStartLon from degrees to radians
            double dLon2 = dFinishLon * (Math.PI / 180);     // Convert dFinishLon from degrees to radians
            double bearingDegs = Math.Atan2(Math.Sin(dLon2 - dLon1) * Math.Cos(dLat2), Math.Cos(dLat1) * Math.Sin(dLat2) - Math.Sin(dLat1) * Math.Cos(dLat2) * Math.Cos(dLon2 - dLon1)) * (180 / Math.PI);
            return (bearingDegs + 180) % 360;
        }

        static internal double CalcDistance(double dStartLat, double dStartLon, double dFinishLat, double dFinishLon)
        {
            // http://www.movable-type.co.uk/scripts/latlong.html
            // Use this forumla to calculate distance: = acos( sin(lat1) ⋅ sin(lat2) + cos(lat1) ⋅ cos(lat2) ⋅ cos(lon2 - lon1) ) ⋅ R)
            double dLat1 = dStartLat * (Math.PI / 180);     // Convert dStartLat from degrees to radians
            double dLat2 = dFinishLat * (Math.PI / 180);     // Convert dFinishLat from degrees to radians
            double dLon1 = dStartLon * (Math.PI / 180);     // Convert dStartLon from degrees to radians
            double dLon2 = dFinishLon * (Math.PI / 180);     // Convert dFinishLon from degrees to radians
            return Math.Acos(Math.Sin(dLat1) * Math.Sin(dLat2) + Math.Cos(dLat1) * Math.Cos(dLat2) * Math.Cos(dLon2 - dLon1)) * Constants.FeetInRadiusOfEarth / Constants.FeetInNauticalMile;
        }

        static internal int CalcHeadingChange(double oldHeading, double newHeading)
        {
            int headingChange = Convert.ToInt32(oldHeading - newHeading);
            if (headingChange > 180) 
            {
                headingChange -= 360; 
            }
            if (headingChange < -180) 
            { 
                headingChange += 360;
            }
            return headingChange;
        }

        /// <summary>
        /// Round decimal degrees to nearest whole number of seconds and return as decimal degrees
        /// </summary>
        static internal double RoundDecDegreesToNearestSec(double dLocalCoord)
        {
            double dDecimalPart, dDegrees, dMinutes, dSeconds;

            // Degrees is integer part of raw coord
            dDegrees = (int)dLocalCoord;

            // Minutes is decimal part of raw coord times 60
            dDecimalPart = dLocalCoord - dDegrees;
            dMinutes = (int)(dDecimalPart * 60);

            // Seconds is decimal part of minutes times 60
            dDecimalPart = (dDecimalPart * 60) - dMinutes;
            dSeconds = dDecimalPart * 60;

            // Round seconds
            dSeconds = Math.Round(dSeconds);
            if (dSeconds > 60)
            {
                dSeconds = 0;
            }

            return dDegrees + (dMinutes / 60) + (dSeconds / 3600);
        }

        static internal double GetReciprocalHeading(double heading)
        {
            if (heading >= 180)
                return heading - 180;
            else 
                return heading + 180;
        }

        static internal double ConvertHeadingAbsoluteToRelative(double absoluteHdg)
        {
            if (absoluteHdg > 180)
            {
                absoluteHdg -= 360;
            }
            return absoluteHdg;
        }
    }
}
