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
        public double topPixels;
        public double leftPixels;

        public Gate(double d1, double d2, double d3, double d4, double d5, double d6, double d7)
        {
            lat = d1;
            lon = d2;
            amsl = d3;
            pitch = d4;
            orientation = d5;
            topPixels = d6;
            leftPixels = d7;
        }
    }

    internal class Gates
    {
        private static readonly List<Gate> gates = new List<Gate>();
        private static readonly int signLetterNoGates = 44;
        internal static readonly double unitSegment = 36.0 / 3600;   // approx 3600ft or 36 secs of latitude
        internal static readonly double unitPixels = 48;
        internal static readonly double unitHalfLinePixels = 6;

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
                gates.Add(new Gate(dFinishLat, dFinishLon, legParams[index].amsl, 0, orientation, 0, 0));
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
                SetSignGatesLetter();
                TranslateGates(GateCount - signLetterNoGates, signLetterNoGates, 0, unitSegment * 3 * index, 0);
            }
            TiltGates(0, GateCount);
            TranslateGates(0, GateCount, Runway.AirportLat, Runway.AirportLon, Runway.Altitude + 1000);
        }

        internal static void SetSignGatesLetter()
        {
            double junctionRadius = 2.0 / 3600;
            double cellPixels = 48;
            double cellInset = 2;

            gates.Add(new Gate(0, junctionRadius, 0, 0, 90, cellPixels * 4, cellInset));                                                       // Start segment 1
            gates.Add(new Gate(0, unitSegment - junctionRadius, 0, 0, 90, cellPixels * 4, cellPixels - cellInset));                                         // Finish segment 1
            gates.Add(new Gate(0, unitSegment + junctionRadius, 0, 0, 90, cellPixels * 4, cellPixels + cellInset));                                         // Start segment 2
            gates.Add(new Gate(0, unitSegment * 2 - junctionRadius, 0, 0, 90, cellPixels * 4, cellPixels * 2 - cellInset));                                     // Finish segment 2
            gates.Add(new Gate(unitSegment, unitSegment * 2 - junctionRadius, 0, 0, 270, cellPixels * 3, cellPixels * 2 - cellInset));                          // Start segment 3
            gates.Add(new Gate(unitSegment, unitSegment + junctionRadius, 0, 0, 270, cellPixels * 3, cellPixels + cellInset));                              // Finish segment 3
            gates.Add(new Gate(unitSegment, unitSegment - junctionRadius, 0, 0, 270, cellPixels * 3, cellPixels - cellInset));                              // Start segment 4
            gates.Add(new Gate(unitSegment, junctionRadius, 0, 0, 270, cellPixels * 3, cellInset));                                            // Finish segment 4
            gates.Add(new Gate(unitSegment * 2, junctionRadius, 0, 0, 90, cellPixels * 2, cellInset));                                         // Start segment 5
            gates.Add(new Gate(unitSegment * 2, unitSegment - junctionRadius, 0, 0, 90, cellPixels * 2, cellPixels - cellInset));                           // Finish segment 5
            gates.Add(new Gate(unitSegment * 2, unitSegment + junctionRadius, 0, 0, 90, cellPixels * 2, cellPixels + cellInset));                           // Start segment 6
            gates.Add(new Gate(unitSegment * 2, unitSegment * 2 - junctionRadius, 0, 0, 90, cellPixels * 2, cellPixels * 2 - cellInset));                       // Finish segment 6
            gates.Add(new Gate(unitSegment * 3, unitSegment * 2 - junctionRadius, 0, 0, 270, cellPixels, cellPixels * 2 - cellInset));                      // Start segment 7
            gates.Add(new Gate(unitSegment * 3, unitSegment + junctionRadius, 0, 0, 270, cellPixels, cellPixels + cellInset));                          // Finish segment 7
            gates.Add(new Gate(unitSegment * 3, unitSegment - junctionRadius, 0, 0, 270, cellPixels, cellPixels - cellInset));                          // Start segment 8
            gates.Add(new Gate(unitSegment * 3, junctionRadius, 0, 0, 270, cellPixels, cellInset));                                        // Finish segment 8
            gates.Add(new Gate(unitSegment * 4, junctionRadius, 0, 0, 90, 0, cellInset));                                         // Start segment 9
            gates.Add(new Gate(unitSegment * 4, unitSegment - junctionRadius, 0, 0, 90, 0, cellPixels - cellInset));                           // Finish segment 9
            gates.Add(new Gate(unitSegment * 4, unitSegment + junctionRadius, 0, 0, 90, 0, cellPixels + cellInset));                           // Start segment 10
            gates.Add(new Gate(unitSegment * 4, unitSegment * 2 - junctionRadius, 0, 0, 90, 0, cellPixels * 2 - cellInset));                       // Finish segment 10
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellInset, 0));                 // Start segment 11
            gates.Add(new Gate(unitSegment * 3 + junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels - cellInset, 0));                 // Finish segment 11
            gates.Add(new Gate(unitSegment * 3 - junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels + cellInset, 0));                 // Start segment 12
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels * 2 - cellInset, 0));                 // Finish segment 12
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels * 2 + cellInset, 0));                 // Start segment 13
            gates.Add(new Gate(unitSegment * 1 + junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels * 3 - cellInset, 0));                 // Finish segment 13
            gates.Add(new Gate(unitSegment * 1 - junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels * 3 + cellInset, 0));                 // Start segment 14
            gates.Add(new Gate(junctionRadius, 0, 0, Parameters.TiltAngle, 180, cellPixels * 4 - cellInset, 0));                                   // Finish segment 14
            gates.Add(new Gate(junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels * 4 - cellInset, cellPixels));                          // Start segment 15
            gates.Add(new Gate(unitSegment * 1 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels * 3 + cellInset, cellPixels));        // Finish segment 15
            gates.Add(new Gate(unitSegment * 1 + junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels * 3 - cellInset, cellPixels));        // Start segment 16
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels * 2 + cellInset, cellPixels));        // Finish segment 16
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels * 2 - cellInset, cellPixels));        // Start segment 17
            gates.Add(new Gate(unitSegment * 3 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels + cellInset, cellPixels));        // Finish segment 17
            gates.Add(new Gate(unitSegment * 3 + junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellPixels - cellInset, cellPixels));        // Start segment 18
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, unitSegment, 0, -Parameters.TiltAngle, 0, cellInset, cellPixels));        // Finish segment 18
            gates.Add(new Gate(unitSegment * 4 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellInset, cellPixels * 2));   // Start segment 19
            gates.Add(new Gate(unitSegment * 3 + junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels - cellInset, cellPixels * 2));   // Finish segment 19
            gates.Add(new Gate(unitSegment * 3 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels + cellInset, cellPixels * 2));   // Start segment 20
            gates.Add(new Gate(unitSegment * 2 + junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels * 2 - cellInset, cellPixels * 2));   // Finish segment 20
            gates.Add(new Gate(unitSegment * 2 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels * 2 + cellInset, cellPixels * 2));   // Start segment 21
            gates.Add(new Gate(unitSegment * 1 + junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels * 3 - cellInset, cellPixels * 2));   // Finish segment 21
            gates.Add(new Gate(unitSegment * 1 - junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels * 3 + cellInset, cellPixels * 2));   // Start segment 22
            gates.Add(new Gate(junctionRadius, unitSegment * 2, 0, Parameters.TiltAngle, 180, cellPixels * 4 - cellInset, cellPixels * 2));                     // Finish segment 22
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
