using System;
using System.Collections.Generic;

namespace P3D_Scenario_Generator
{
    public struct LegParams
    {
        public double heading;
        public double distance;
        public double amsl;

        public LegParams(double d1, double d2, double d3)
        {
            heading = d1;
            distance = d2;
            amsl = d3;
        }
    }

    public struct Gate
    {
        public double lat;
        public double lon;
        public double amsl;

        public Gate(double d1, double d2, double d3)
        {
            lat = d1;
            lon = d2;
            amsl = d3;
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
            // Climb rate angle from gate 1 to 3 approx given by solving tan(angle) = opposite(height change) / adjacent(over ground distance)
            double gate1to3AngleRad = Math.Atan((Parameters.HeightDown - Parameters.HeightUpwind) / (turnDistance + Parameters.BaseLeg * Constants.feetInKnot));
            // Gate 1 height adj relative to gate 2 approx given by solving tan(gate2to3AngleDeg) = height adj / turnDistance
            double gate1to2heightDif = Math.Tan(gate1to3AngleRad) * turnDistance;
            // Descend rate angle from gate 6 to 8 approx given by solving tan(angle) = opposite(height change) / adjacent(over ground distance)
            double gate6to8AngleRad = Math.Atan((Parameters.HeightDown - Parameters.HeightBase) / (turnDistance + Parameters.BaseLeg * Constants.feetInKnot));
            // Gate 8 height adj relative to gate 7 approx given by solving tan(gate6to7AngleDeg) = height adj / turnDistance
            double gate7to8heightDif = Math.Tan(gate6to8AngleRad) * turnDistance;

            List<LegParams> legParams = new List<LegParams>();
            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar + 360) % 360, Runway.Len + (Parameters.UpwindLeg * Constants.feetInKnot), Runway.Altitude + Parameters.HeightUpwind));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 45 + 360) % 360, turnDistance, Runway.Altitude + Parameters.HeightUpwind + gate1to2heightDif));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 90 + 360) % 360, Parameters.BaseLeg * Constants.feetInKnot, Runway.Altitude + Parameters.HeightDown));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 135 + 360) % 360, turnDistance, Runway.Altitude + Parameters.HeightDown));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 180 + 360) % 360, (Parameters.FinalLeg * Constants.feetInKnot) + Runway.Len + (Parameters.UpwindLeg * Constants.feetInKnot), Runway.Altitude + Parameters.HeightDown));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 225 + 360) % 360, turnDistance, Runway.Altitude + Parameters.HeightDown));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 270 + 360) % 360, Parameters.BaseLeg * Constants.feetInKnot, Runway.Altitude + Parameters.HeightBase + gate7to8heightDif));
                    legParams.Add(new LegParams((Runway.Hdg + Runway.MagVar - 315 + 360) % 360, turnDistance, Runway.Altitude + Parameters.HeightBase));
                    break;
                default:
                    break;
            }

            double dStartLat = Runway.Lat;
            double dStartLon = Runway.Lon;
            double dFinishLat = 0;
            double dFinishLon = 0;
            gates.Clear();
            for (int index = 0; index < legParams.Count; index++)
            {
                MathRoutines.AdjCoords(dStartLat, dStartLon, legParams[index].heading, legParams[index].distance, ref dFinishLat, ref dFinishLon);
                gates.Add(new Gate(dFinishLat, dFinishLon, legParams[index].amsl));
                dStartLat = dFinishLat;
                dStartLon = dFinishLon;
            }
            GateCount = legParams.Count;
        }

        static internal Gate GetGate(int index)
        {
            return gates[index];
        }
    }
}
