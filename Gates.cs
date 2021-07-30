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
        internal static readonly double cellPixels = 35;
        internal static readonly double cellCapExtraPixels = 5;

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
            int signLetterNoGates;
            gates.Clear();
            for (int index = 0; index < Parameters.Message.Length; index++)
            {
                if (Parameters.Message[index] != ' ')
                {
                    signLetterNoGates = SetSignGatesLetter(index);
                    TranslateGates(GateCount - signLetterNoGates, signLetterNoGates, 0, Parameters.SegmentLengthDeg * 3 * index, 0);
                }
            }
            TiltGates(0, GateCount);
            TranslateGates(0, GateCount, Runway.AirportLat, Runway.AirportLon, Runway.Altitude + Parameters.GateHeight);
        }

        internal static void SetSegmentGate(double latCoef, double latOffsetCoef, double lonCoef, double lonOffsetCoef, double orientation, double topPixels, double leftPixels, int gateNo, int letterIndex)
        {
            double lat = Parameters.SegmentLengthDeg * latCoef + Parameters.SegmentRadiusDeg * latOffsetCoef;
            double lon = Parameters.SegmentLengthDeg * lonCoef + Parameters.SegmentRadiusDeg * lonOffsetCoef;
            double amsl = 0;
            double pitch;

            // convert gateNo to segment position
            int segmentIndex = gateNo / 2;
            if (!SignWriting.SegmentIsSet(Parameters.Message[letterIndex], segmentIndex))
            {
                return;
            }

            if ((orientation == 90) || (orientation == 270))
            {
                pitch = 0;
            }
            else if (orientation == 180)
            {
                pitch = Parameters.TiltAngle;
            }
            else
            {
                pitch = -Parameters.TiltAngle;
            }

            // shift left pixels based on which letter it is in message
            leftPixels += letterIndex * 105;

            GateCount++;
            gates.Add(new Gate(lat, lon, amsl, pitch, orientation, topPixels, leftPixels));
        }

        internal static int SetSignGatesLetter(int letterIndex)
        {
            int gateNo = 0;
            int startNoGates = GateCount;

            SetSegmentGate(0, 0, 0, 1, 90, 140, 2, gateNo++, letterIndex);
            SetSegmentGate(0, 0, 1, -1, 90, 140, 34, gateNo++, letterIndex);
            SetSegmentGate(0, 0, 1, 1, 90, 140, 37, gateNo++, letterIndex);
            SetSegmentGate(0, 0, 2, -1, 90, 140, 69, gateNo++, letterIndex);
            SetSegmentGate(1, 0, 2, -1, 270,105, 69, gateNo++, letterIndex);
            SetSegmentGate(1, 0, 1, 1, 270, 105, 37, gateNo++, letterIndex);
            SetSegmentGate(1, 0, 1, -1, 270, 105, 34, gateNo++, letterIndex);
            SetSegmentGate(1, 0, 0, 1, 270, 105, 2, gateNo++, letterIndex);
            SetSegmentGate(2, 0, 0, 1, 90, 70, 2, gateNo++, letterIndex);
            SetSegmentGate(2, 0, 1, -1, 90, 70, 34, gateNo++, letterIndex);
            SetSegmentGate(2, 0, 1, 1, 90, 70, 37, gateNo++, letterIndex);
            SetSegmentGate(2, 0, 2, -1, 90, 70, 69, gateNo++, letterIndex);
            SetSegmentGate(3, 0, 2, -1, 270, 35, 69, gateNo++, letterIndex);
            SetSegmentGate(3, 0, 1, 1, 270, 35, 37, gateNo++, letterIndex);
            SetSegmentGate(3, 0, 1, -1, 270, 35, 34, gateNo++, letterIndex);
            SetSegmentGate(3, 0, 0, 1, 270, 35, 2, gateNo++, letterIndex);
            SetSegmentGate(4, 0, 0, 1, 90, 0, 2, gateNo++, letterIndex);
            SetSegmentGate(4, 0, 1, -1, 90, 0, 34, gateNo++, letterIndex);
            SetSegmentGate(4, 0, 1, 1, 90, 0, 37, gateNo++, letterIndex);
            SetSegmentGate(4, 0, 2, -1, 90, 0, 69, gateNo++, letterIndex);
            SetSegmentGate(4, -1, 0, 0, 180, 2, 0, gateNo++, letterIndex);
            SetSegmentGate(3, 1, 0, 0, 180, 34, 0, gateNo++, letterIndex);
            SetSegmentGate(3, -1, 0, 0, 180, 37, 0, gateNo++, letterIndex);
            SetSegmentGate(2, 1, 0, 0, 180, 69, 0, gateNo++, letterIndex);
            SetSegmentGate(2, -1, 0, 0, 180, 72, 0, gateNo++, letterIndex);
            SetSegmentGate(1, 1, 0, 0, 180, 104, 0, gateNo++, letterIndex);
            SetSegmentGate(1, -1, 0, 0, 180, 107, 0, gateNo++, letterIndex);
            SetSegmentGate(0, 1, 0, 0, 180, 139, 0, gateNo++, letterIndex);
            SetSegmentGate(0, 1, 1, 0, 0, 139, 35, gateNo++, letterIndex);
            SetSegmentGate(1, -1, 1, 0, 0, 107, 35, gateNo++, letterIndex);
            SetSegmentGate(1, 1, 1, 0, 0, 104, 35, gateNo++, letterIndex);
            SetSegmentGate(2, -1, 1, 0, 0, 72, 35, gateNo++, letterIndex);
            SetSegmentGate(2, 1, 1, 0, 0, 69, 35, gateNo++, letterIndex);
            SetSegmentGate(3, -1, 1, 0, 0, 37, 35, gateNo++, letterIndex);
            SetSegmentGate(3, 1, 1, 0, 0, 34, 35, gateNo++, letterIndex);
            SetSegmentGate(4, -1, 1, 0, 0, 2, 35, gateNo++, letterIndex);
            SetSegmentGate(4, -1, 2, 0, 180, 2, 70, gateNo++, letterIndex);
            SetSegmentGate(3, 1, 2, 0, 180, 34, 70, gateNo++, letterIndex);
            SetSegmentGate(3, -1, 2, 0, 180, 37, 70, gateNo++, letterIndex);
            SetSegmentGate(2, 1, 2, 0, 180, 69, 70, gateNo++, letterIndex);
            SetSegmentGate(2, -1, 2, 0, 180, 72, 70, gateNo++, letterIndex);
            SetSegmentGate(1, 1, 2, 0, 180, 104, 70, gateNo++, letterIndex);
            SetSegmentGate(1, -1, 2, 0, 180, 107, 70, gateNo++, letterIndex);
            SetSegmentGate(0, 1, 2, 0, 180, 139, 70, gateNo++, letterIndex);

            return GateCount - startNoGates;
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
