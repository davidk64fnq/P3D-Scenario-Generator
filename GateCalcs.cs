using System;
using System.Collections.Generic;
using System.Text;

namespace P3D_Scenario_Generator
{
    public struct LegParams
    {
        public double heading;
        public double distance;

        public LegParams(double d1, double d2)
        {
            heading = d1;
            distance = d2;
        }
    }

    internal class GateCalcs
    {
        static public List<Gate> SetGateCoords(Runway runway, Params parameters)
        {
            List<Gate> gates = new List<Gate>();

            // Turn radius approx speed/180, then use pythagorus to get turn distance
            double turnDistance = parameters.speed / 180 / Math.Sin(45 * Math.PI / 180);

            List<LegParams> legParams = new List<LegParams>();
            switch (parameters.selectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    legParams.Add(new LegParams(runway.hdg + runway.magVar, runway.len + (parameters.upwindLeg / Constants.feetInKnot)));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 45) % 360, turnDistance));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 90) % 360, parameters.baseLeg / Constants.feetInKnot));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 135) % 360, turnDistance));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 180) % 360, (parameters.finalLeg / Constants.feetInKnot) + runway.len + (parameters.upwindLeg / Constants.feetInKnot)));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 225) % 360, turnDistance));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 270) % 360, parameters.baseLeg / Constants.feetInKnot));
                    legParams.Add(new LegParams((runway.hdg + runway.magVar + 315) % 360, turnDistance));
                    break;
                default:
                    break;
            }

            double dStartLat = runway.lat;
            double dStartLon = runway.lon;
            double dFinishLat = 0;
            double dFinishLon = 0;
            for (int index = 0; index < legParams.Count; index++)
            {
                AdjCoords(dStartLat, dStartLon, legParams[index].heading, legParams[index].distance, ref dFinishLat, ref dFinishLon);
                gates.Add(new Gate(dFinishLat, dFinishLon));
                dStartLat = dFinishLat;
                dStartLon = dFinishLon;
            }

            return gates;
        }

        static private void AdjCoords(double dStartLat, double dStartLon, double dHeading, double dDist, ref double dFinishLat, ref double dFinishLon)
        {
            // http://www.movable-type.co.uk/scripts/latlong.html
            // Use this forumla to calculate lat2: =ASIN(SIN(lat1)*COS(d/R) + COS(lat1)*SIN(d/R)*COS(brng))
            double dBrng = dHeading * (Math.PI / 180);      // Convert degrees to radians
            double dLat1 = dStartLat * (Math.PI / 180);     // Convert threshold latitude from degrees to radians
            double dLat2 = Math.Asin(Math.Sin(dLat1) * Math.Cos(dDist / Constants.radiusEarth) + Math.Cos(dLat1) * Math.Sin(dDist / Constants.radiusEarth) * Math.Cos(dBrng));

            // Use this forumla to calculate lon2: =lon1 + ATAN2(SIN(brng)*SIN(d/R)*COS(lat1), COS(d/R)-SIN(lat1)*SIN(lat2))
            double dLon1 = dStartLon * (Math.PI / 180);     // Convert threshold longitude from degrees to radians
            double dLon2 = dLon1 + Math.Atan2(Math.Sin(dBrng) * Math.Sin(dDist / Constants.radiusEarth) * Math.Cos(dLat1), Math.Cos(dDist / Constants.radiusEarth) - (Math.Sin(dLat1) * Math.Sin(dLat2)));
     
            // Convert back to degrees
            dFinishLat = dLat2 * (180 / Math.PI);
            dFinishLon = dLon2 * (180 / Math.PI);
        }
    }
}
