namespace P3D_Scenario_Generator
{

    public struct LegParams(double d1, double d2, double d3)
    {
        public double heading = d1;
        public double distance = d2;
        public double amsl = d3;
    }

    public class Gate(double d1, double d2, double d3, double d4, double d5, double d6, double d7)
    {
        public double lat = d1;
        public double lon = d2;
        public double amsl = d3;
        public double pitch = d4;
        public double orientation = d5;
        public double topPixels = d6;
        public double leftPixels = d7;
    }

    internal class Gates
    {
        private static readonly List<Gate> gates = [];
        internal static readonly double cellPixels = 35;
        internal static readonly double cellCapExtraPixels = 5;

        internal static int GateCount { get; private set; }

        /// <summary>
        /// Circuit consists of four turns. Calculates turn radius.
        /// </summary>
        /// <returns>Turn radius in feet</returns>
        internal static double CalcTurnRadius()
        {
            // Calculate number of feet travelled by plane in an hour based on cruise speed
            double speedInFeetPerHour = Parameters.Speed * Con.feetInNM;

            // Calculate number of feet travelled by plane to complete a circle in number of minutes given by TurnRate
            double turnCircumference = speedInFeetPerHour * (Parameters.TurnRate / 60);

            // Calculate radius of turn, circumference = 2 * Pi * radius
            double turnRadius = turnCircumference / (2 * Math.PI);

            return turnRadius;
        }

        /// <summary>
        /// User specifies height of gates 1, 3 to 6, and 8. This method calculates the height difference between 
        /// gates 1 and 2 based on assumption plane maintains a constant climb rate from gates 1 to 3.
        /// </summary>
        /// <param name="turnRadius">The turn radius of plane based on it's cruise speed and turn rate</param>
        /// <returns>The absolute height difference between gates 1 and 2 in feet</returns>
        internal static double CalcGate1to2HeightDif(double turnRadius)
        {
            // Firstly work out climb rate angle from gate 1 to 3. Approx given by solving
            // tan(angle) = opposite(height change) / adjacent (over ground distance)
            double heightChange = Parameters.HeightDown - Parameters.HeightUpwind;
            double overGroundDistance = turnRadius + Parameters.BaseLeg * Con.feetInNM; 
            double gate1to3AngleRad = Math.Atan(heightChange / overGroundDistance);

            // Using climb rate angle calculated above and given adjacent (over ground distance) is turn radius we can calculate
            // gate 1 height adj relative to gate 2
            double gate1to2heightDif = Math.Tan(gate1to3AngleRad) * turnRadius;

            return gate1to2heightDif;
        }

        /// <summary>
        /// User specifies height of gates 1, 3 to 6, and 8. This method calculates the height difference between 
        /// gates 7 and 8 based on assumption plane maintains a constant descent rate from gates 6 to 8.
        /// </summary>
        /// <param name="turnRadius">The turn radius of plane based on it's cruise speed and turn rate</param>
        /// <returns>The absolute height difference between gates 7 and 8 in feet</returns>
        internal static double CalcGate7to8HeightDif(double turnRadius)
        {
            // Firstly work out descent rate angle from gate 6 to 8. Approx given by solving
            // tan(angle) = opposite(height change) / adjacent (over ground distance)
            double heightChange = Parameters.HeightDown - Parameters.HeightBase;
            double overGroundDistance = turnRadius + Parameters.BaseLeg * Con.feetInNM;
            double gate6to8AngleRad = Math.Atan(heightChange / overGroundDistance);

            // Using descent rate angle calculated above and given adjacent (over ground distance) is turn radius we can calculate
            // gate 7 height adj relative to gate 8
            double gate7to8heightDif = Math.Tan(gate6to8AngleRad) * turnRadius;

            return gate7to8heightDif;
        }

        internal static List<LegParams> SetLegParams(double turnRadius, double gate1to2heightDif, double gate7to8heightDif)
        {
            List<LegParams> legParams = [];
            double baseHeading = Runway.startRwy.Hdg + Runway.startRwy.MagVar + 360;
            legParams.Add(new LegParams(baseHeading % 360, 
                Runway.startRwy.Len + (Parameters.UpwindLeg * Con.feetInNM), Runway.startRwy.Altitude + Parameters.HeightUpwind));
            legParams.Add(new LegParams((baseHeading - 45) % 360, 
                turnRadius, Runway.startRwy.Altitude + Parameters.HeightUpwind + gate1to2heightDif));
            legParams.Add(new LegParams((baseHeading - 90) % 360, 
                Parameters.BaseLeg * Con.feetInNM, Runway.startRwy.Altitude + Parameters.HeightDown));
            legParams.Add(new LegParams((baseHeading - 135) % 360, 
                turnRadius, Runway.startRwy.Altitude + Parameters.HeightDown));
            legParams.Add(new LegParams((baseHeading - 180) % 360, 
                (Parameters.FinalLeg * Con.feetInNM) + Runway.startRwy.Len + (Parameters.UpwindLeg * Con.feetInNM), 
                Runway.startRwy.Altitude + Parameters.HeightDown));
            legParams.Add(new LegParams((baseHeading - 225) % 360, 
                turnRadius, Runway.startRwy.Altitude + Parameters.HeightDown));
            legParams.Add(new LegParams((baseHeading - 270) % 360, 
                Parameters.BaseLeg * Con.feetInNM, Runway.startRwy.Altitude + Parameters.HeightBase + gate7to8heightDif));
            legParams.Add(new LegParams((baseHeading - 315) % 360, 
                turnRadius, Runway.startRwy.Altitude + Parameters.HeightBase));
            return legParams;
        }

        internal static List<Gate> SetCircuitGates()
        {
            List<Gate> gates = [];
            // Amount each gate is rotated from initial runway heading, e.g. 1st gate not at all, 2nd and 3rd gates 90 degrees
            double[] circuitHeadingAdj = [360, 90, 90, 180, 180, 270, 270, 360];    
            double orientation; // Of each gate as a magnetic compass heading

            double turnRadius = CalcTurnRadius();
            double gate1to2heightDif = CalcGate1to2HeightDif(turnRadius);
            double gate7to8heightDif = CalcGate7to8HeightDif(turnRadius);
            List<LegParams> legParams = SetLegParams(turnRadius, gate1to2heightDif, gate7to8heightDif);

            double dStartLat = Runway.startRwy.ThresholdStartLat;
            double dStartLon = Runway.startRwy.ThresholdStartLon;
            double dFinishLat = 0;
            double dFinishLon = 0;
            for (int index = 0; index < legParams.Count; index++)
            {
                MathRoutines.AdjCoords(dStartLat, dStartLon, legParams[index].heading, legParams[index].distance, ref dFinishLat, ref dFinishLon);
                orientation = (Runway.startRwy.Hdg + Runway.startRwy.MagVar + circuitHeadingAdj[index]) % 360;
                gates.Add(new Gate(dFinishLat, dFinishLon, legParams[index].amsl, 0, orientation, 0, 0));
                dStartLat = dFinishLat;
                dStartLon = dFinishLon;
            }
            GateCount = legParams.Count;
            return gates;
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
            TranslateGates(0, GateCount, Runway.startRwy.AirportLat, Runway.startRwy.AirportLon, Runway.startRwy.Altitude + Parameters.GateHeight);
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
                gates[index].amsl += Math.Abs(gates[index].lat) * Math.Sin(Parameters.TiltAngle * Math.PI / 180) * Con.degreeLatFeet;
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
            return gates[index - 1];
        }
    }
}
