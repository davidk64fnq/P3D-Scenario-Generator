using System;
using System.Collections.Generic;

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
    public struct Gate
    {
        public double lat;
        public double lon;

        public Gate(double d1, double d2)
        {
            lat = d1;
            lon = d2;
        }
    }

    internal class Gates
    {
        private static readonly List<Gate> gates = new List<Gate>();
        internal static int GateCount { get; private set; }

        internal static void SetGates()
        {
            // Turn radius approx speed/180, then use pythagorus to get turn distance
            double turnDistance = Parameters.Speed * Constants.feetInKnot / 180 / Math.Sin(45 * Math.PI / 180);

            List<LegParams> legParams = new List<LegParams>();
            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar + 360) % 360, Runway.Len + (Parameters.UpwindLeg * Constants.feetInKnot)));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 45 + 360) % 360, turnDistance));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 90 + 360) % 360, Parameters.BaseLeg * Constants.feetInKnot));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 135 + 360) % 360, turnDistance));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 180 + 360) % 360, (Parameters.FinalLeg * Constants.feetInKnot) + Runway.Len + (Parameters.UpwindLeg * Constants.feetInKnot)));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 225 + 360) % 360, turnDistance));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 270 + 360) % 360, Parameters.BaseLeg * Constants.feetInKnot));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 315 + 360) % 360, turnDistance));
                    break;
                default:
                    break;
            }

            double dStartLat = Runway.Lat;
            double dStartLon = Runway.Lon;
            double dFinishLat = 0;
            double dFinishLon = 0;
            for (int index = 0; index < legParams.Count; index++)
            {
                AdjCoords(dStartLat, dStartLon, legParams[index].heading, legParams[index].distance, ref dFinishLat, ref dFinishLon);
                gates.Add(new Gate(dFinishLat, dFinishLon));
                dStartLat = dFinishLat;
                dStartLon = dFinishLon;
            }
            GateCount = legParams.Count;
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

        static internal Gate GetGate(int index)
        {
            return gates[index];
        }
    }
}
