using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

    public class Gate
    {
        public double lat;
        public double lon;
        public double amsl;
        public double pitch;
        public double orientation;

        public Gate(double d1, double d2, double d3, double d4, double d5)
        {
            lat = d1;
            lon = d2;
            amsl = d3;
            pitch = d4;
            orientation = d5;
        }
    }

    internal class Gates
    {
        private static readonly List<Gate> gates = new List<Gate>();
        private static readonly int signLetterNoGates = 32;
        internal static readonly double unitSegment = 40.0 / 3600;   // approx 4000ft or 40 secs of latitude

        internal static int GateCount { get; private set; }

        internal static void SetCircuitGates()
        {
            double[] circuitHeadingAdj = { 360, 90, 90, 180, 180, 270, 270, 360 };
            double orientation;

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
                orientation = (Runway.Hdg + Runway.MagVar + circuitHeadingAdj[index]) % 360;
                gates.Add(new Gate(dFinishLat, dFinishLon, legParams[index].amsl, 0, orientation));
                dStartLat = dFinishLat;
                dStartLon = dFinishLon;
            }
            GateCount = legParams.Count;
        }

        internal static void SetGates()
        {
            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Circuit))
            {
                SetCircuitGates();
            }
            else if (Parameters.SelectedScenario == nameof(ScenarioTypes.SignWriting))
            {
                SetSignGatesMessage();
            }
        }

        internal static void SetSignGatesMessage()
        {
            gates.Clear();
            for (int index = 0; index < Parameters.Message.Length; index++)
            {
                SetSignGatesSingleLetter();
                TranslateGates(GateCount - signLetterNoGates, signLetterNoGates, 0, unitSegment * 3 * index, 0);
            }
            TiltGates(0, GateCount);
            TranslateGates(0, GateCount, Runway.AirportLat, Runway.AirportLon, Runway.Altitude + 1000);
        }

        internal static void SetSignGatesSingleLetter()
        {
            double junctionRadius = 4.5 / 3600;     // approx 450ft or 4.5 seconds of latitude
            double diagonalAngle = Math.Atan(2.0) * 180 / Math.PI;
            double shortRadius = junctionRadius * 1.5 * Math.Cos(diagonalAngle * Math.PI / 180); // used to position start and finish of diagonal segments
            double longRadius = junctionRadius * 1.5 * Math.Sin(diagonalAngle * Math.PI / 180);

            gates.Add(new Gate(0, junctionRadius, 0, 0, 90));                                                                                   // Start segment 1
            gates.Add(new Gate(0, unitSegment - junctionRadius, 0, 0, 90));                                                                     // Finish segment 1
            gates.Add(new Gate(0, unitSegment + junctionRadius, 0, 0, 90));                                                                     // Start segment 2
            gates.Add(new Gate(0, unitSegment * 2 - junctionRadius, 0, 0, 90));                                                                 // Finish segment 2
            gates.Add(new Gate(unitSegment * 2, unitSegment * 2 - junctionRadius, 0, 0, 270));                                                  // Start segment 3
            gates.Add(new Gate(unitSegment * 2, unitSegment + junctionRadius, 0, 0, 270));                                                      // Finish segment 3
            gates.Add(new Gate(unitSegment * 2, unitSegment - junctionRadius, 0, 0, 270));                                                      // Start segment 4
            gates.Add(new Gate(unitSegment * 2, junctionRadius, 0, 0, 270));                                                                    // Finish segment 4
            gates.Add(new Gate(unitSegment * 4, junctionRadius, 0, 0, 90));                                                                     // Start segment 5
            gates.Add(new Gate(unitSegment * 4, unitSegment - junctionRadius, 0, 0, 90));                                                       // Finish segment 5
            gates.Add(new Gate(unitSegment * 4, unitSegment + junctionRadius, 0, 0, 90));                                                       // Start segment 6
            gates.Add(new Gate(unitSegment * 4, unitSegment * 2 - junctionRadius, 0, 0, 90));                                                   // Finish segment 6
            gates.Add(new Gate(unitSegment * 4 - longRadius, shortRadius, 0, Parameters.TiltAngle, 90 + diagonalAngle));                        // Start segment 7
            gates.Add(new Gate(unitSegment * 2 + longRadius, unitSegment - shortRadius, 0, Parameters.TiltAngle, 90 + diagonalAngle));          // Finish segment 7
            gates.Add(new Gate(unitSegment * 2 - longRadius, unitSegment + shortRadius, 0, Parameters.TiltAngle, 90 + diagonalAngle));          // Start segment 8
            gates.Add(new Gate(longRadius, unitSegment * 2 - shortRadius, 0, Parameters.TiltAngle, 90 + diagonalAngle));                        // Finish segment 8
            gates.Add(new Gate(longRadius, shortRadius, 0, -Parameters.TiltAngle, 90 - diagonalAngle));                                         // Start segment 9
            gates.Add(new Gate(unitSegment * 2 - longRadius, unitSegment - shortRadius, 0, -Parameters.TiltAngle, 90 - diagonalAngle));         // Finish segment 9
            gates.Add(new Gate(unitSegment * 2 + longRadius, unitSegment + shortRadius, 0, -Parameters.TiltAngle, 90 - diagonalAngle));         // Start segment 10
            gates.Add(new Gate(unitSegment * 4 - longRadius, unitSegment * 2 - shortRadius, 0, -Parameters.TiltAngle, 90 - diagonalAngle));     // Finish segment 10
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, 0, 0, Parameters.TiltAngle, 180));                                             // Start segment 11
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, 0, 0, Parameters.TiltAngle, 180));                                             // Finish segment 11
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, 0, 0, Parameters.TiltAngle, 180));                                             // Start segment 12
            gates.Add(new Gate(junctionRadius, 0, 0, Parameters.TiltAngle, 180));                                                               // Finish segment 12
            gates.Add(new Gate(junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0));                                                      // Start segment 13
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0));                                    // Finish segment 13
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0));                                    // Start segment 14
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0));                                    // Finish segment 14
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180));                               // Start segment 15
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180));                               // Finish segment 15
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180));                               // Start segment 16
            gates.Add(new Gate(junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180));                                                 // Finish segment 16
            GateCount += signLetterNoGates;
        }

        internal static void TiltGates(int startGateIndex, int noGates)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                // do altitude change first before adjusting latitude, tilt is on longitude axis
                gates[index].amsl += Math.Abs(gates[index].lat) * Math.Sin(Parameters.TiltAngle * Math.PI / 180) * Constants.degreeLatFeet;
                gates[index].lat = gates[index].lat * Math.Cos(Parameters.TiltAngle * Math.PI / 180);
            }
        }

        internal static void TranslateGates(int startGateIndex, int noGates, double latAmt, double longAmt, double altAmt)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                gates[index].lat += latAmt;
                gates[index].lon += longAmt;
                gates[index].amsl += altAmt;
            }
        }

        static internal Gate GetGate(int index)
        {
            return gates[index];
        }
    }
}
