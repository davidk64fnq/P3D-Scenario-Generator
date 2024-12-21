using CoordinateSharp;

namespace P3D_Scenario_Generator
{

    /// <summary>
    /// Used to store information needed for displaying a gate in a scenario
    /// </summary>
    /// <param name="lat">The latitude position for the gate</param>
    /// <param name="lon">The longitude position for the gate</param>
    /// <param name="amsl">The AMSL of the gate</param>
    /// <param name="pitch">Signwriting messages can be tilted in vertical plane of message letters</param>
    /// <param name="orientation">What direction must the gate be entered to trigger</param>
    /// <param name="topPixels">Used in signwriting scenario for displaying segment of letter on HTML canvas</param>
    /// <param name="leftPixels">Used in signwriting scenario for displaying segment of letter on HTML canvas</param>
    public class Gate(double lat, double lon, double amsl, double pitch, double orientation, double topPixels, double leftPixels)
    {
        public double lat = lat;
        public double lon = lon;
        public double amsl = amsl;
        public double pitch = pitch;
        public double orientation = orientation;
        public double topPixels = topPixels;
        public double leftPixels = leftPixels;
    }

    internal class Gates
    {
        #region Circuit

        /// <summary>
        /// Temporary data structure used for storing intermediate calculation parameters relating to each gate of the circuit,
        /// eight entries for the eight gates in the circuit. These give point to point path for the gates starting at takeoff threshold.
        /// </summary>
        /// <param name="heading">Magnetic bearing to travel to gate from previous gate</param>
        /// <param name="distance">Distance from previous gate in feet (start threshold for gate 1)</param>
        /// <param name="amsl">Height of the gate in feet</param>
        public struct LegParams(double heading, double distance, double amsl)
        {
            public double heading = heading;
            public double amsl = amsl;
            public Distance distance = new(distance, DistanceType.Feet);
        }

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

        /// <summary>
        /// Store intermediate calculation parameters relating to each gate of the circuit, eight entries for 
        /// the eight gates in the circuit. These give point to point path for the gates starting at takeoff threshold.
        /// </summary>
        /// <param name="turnRadius">The turn radius of plane based on it's cruise speed and turn rate</param>
        /// <param name="gate1to2heightDif">The absolute height difference between gates 1 and 2 in feet</param>
        /// <param name="gate7to8heightDif">The absolute height difference between gates 7 and 8 in feet</param>
        /// <returns>List of eight gate leg parameters</returns>
        internal static List<LegParams> SetLegParams(double turnRadius, double gate1to2heightDif, double gate7to8heightDif)
        {
            List<LegParams> legParams = [];
            double baseHeading = Runway.startRwy.Hdg + Runway.startRwy.MagVar + 360;
            double turnDistance = turnRadius * Math.Sqrt(2.0);
            // Start theshold to gate 1
            legParams.Add(new LegParams(baseHeading % 360, 
                Runway.startRwy.Len + (Parameters.UpwindLeg * Con.feetInNM), Runway.startRwy.Altitude + Parameters.HeightUpwind));
            // Gate 1 to gate 2
            legParams.Add(new LegParams((baseHeading - 45) % 360,
                turnDistance, Runway.startRwy.Altitude + Parameters.HeightUpwind + gate1to2heightDif));
            // Gate 2 to gate 3
            legParams.Add(new LegParams((baseHeading - 90) % 360, 
                Parameters.BaseLeg * Con.feetInNM, Runway.startRwy.Altitude + Parameters.HeightDown));
            // Gate 3 to gate 4
            legParams.Add(new LegParams((baseHeading - 135) % 360,
                turnDistance, Runway.startRwy.Altitude + Parameters.HeightDown));
            // Gate 4 to gate 5
            legParams.Add(new LegParams((baseHeading - 180) % 360, 
                (Parameters.FinalLeg * Con.feetInNM) + Runway.startRwy.Len + (Parameters.UpwindLeg * Con.feetInNM), 
                Runway.startRwy.Altitude + Parameters.HeightDown));
            // Gate 5 to gate 6
            legParams.Add(new LegParams((baseHeading - 225) % 360,
                turnDistance, Runway.startRwy.Altitude + Parameters.HeightDown));
            // Gate 6 to gate 7
            legParams.Add(new LegParams((baseHeading - 270) % 360, 
                Parameters.BaseLeg * Con.feetInNM, Runway.startRwy.Altitude + Parameters.HeightBase + gate7to8heightDif));
            // Gate 7 to gate 8
            legParams.Add(new LegParams((baseHeading - 315) % 360,
                turnDistance, Runway.startRwy.Altitude + Parameters.HeightBase));
            return legParams;
        }

        /// <summary>
        /// There are 8 gates, 2 for each right angle turn. Gates 1, 3 to 6, and 8 are at heights specified by three user parameters.
        /// The length of the upwind, base and final legs is user supplied. The program calculates the downwind based on runway
        /// length and user parameters. Program calculates height of gates 2 and 7 by interpolation between gates 1 and 3, and 
        /// 6 and 8 respectively taking into account user supplied turn rate and cruise speed of selected aircraft.
        /// </summary>
        /// <returns>List of eight gates with data needed to place gate objects into simulation</returns>
        internal static List<Gate> SetCircuitGates()
        {
            List<Gate> gates = [];
            // Amount each gate is rotated from initial runway heading, e.g. 1st gate not at all, 2nd and 3rd gates 90 degrees
            double[] circuitHeadingAdj = [360, 90, 90, 180, 180, 270, 270, 360];    
            double gateOrientation; 

            double turnRadius = CalcTurnRadius();
            double gate1to2heightDif = CalcGate1to2HeightDif(turnRadius);
            double gate7to8heightDif = CalcGate7to8HeightDif(turnRadius);
            List<LegParams> legParams = SetLegParams(turnRadius, gate1to2heightDif, gate7to8heightDif);

            Coordinate start = new(Runway.startRwy.ThresholdStartLat, Runway.startRwy.ThresholdStartLon);
            for (int index = 0; index < legParams.Count; index++)
            {
                start.Move(legParams[index].distance, legParams[index].heading, Shape.Ellipsoid);
                gateOrientation = (Runway.startRwy.Hdg + Runway.startRwy.MagVar + circuitHeadingAdj[index]) % 360;
                gates.Add(new Gate(start.Latitude.DecimalDegree, start.Longitude.DecimalDegree, legParams[index].amsl, 0, gateOrientation, 0, 0));
            }
            return gates;
        }

        #endregion

        #region Signwriting

        /// <summary>
        /// Create a set of gates for signwriting message. Start and finish gate for a subset of the 22 possible
        /// segments used to represent a alphabet letter
        /// </summary>
        /// <returns>The list of gates</returns>
        internal static List<Gate> SetSignGatesMessage()
        {
            List<Gate> gates = [];
            int currentLetterNoGates;
            for (int index = 0; index < Parameters.SignMessage.Length; index++)
            {
                if (char.IsLetter(Parameters.SignMessage[index]))
                {
                    // Add the gates needed for current letter to List<Gate> gates
                    currentLetterNoGates = SetSignGatesLetter(gates, index);

                    // Move gates just added from 0 lat 0 lon 0 asml reference point to the end of letters in message processed so far
                    // only longitude is changed
                    int startGateIndex = gates.Count - currentLetterNoGates;
                    TranslateGates(gates, startGateIndex, currentLetterNoGates, 0, Parameters.SignSegmentLengthDeg * 3 * index, 0);
                }
            }
            TiltGates(gates, 0, gates.Count);

            // Move gates to airport and correct height
            TranslateGates(gates, 0, gates.Count, Runway.startRwy.AirportLat, Runway.startRwy.AirportLon, Runway.startRwy.Altitude + Parameters.SignGateHeight);
            return gates;
        }

        /// <summary>
        /// A letter in the sign writing message is represented by a subset of 22 possible segments with each segment marked
        /// by a start and finish gate. So this method calls <see cref="SetSegmentGate"/> 44 times being twice for each
        /// of the possible 22 segments. Regardless of which character is being processed the order flown through
        /// those of the 22 segments "switched on" is always the same.
        /// </summary>
        /// <param name="gates">Where the gates are stored as they are created by <see cref="SetSegmentGate"/></param>
        /// <param name="letterIndex">Indicates which letter in the sign writing message is being processed</param>
        /// <returns>The number of gates created for the letter in the sign writing message</returns>
        internal static int SetSignGatesLetter(List<Gate> gates, int letterIndex)
        {
            int gateNo = 0; // Integer division by 2 gives the segment index
            int currentLetterGateIndex = gates.Count - 1;

            // Bottom 2 horizontal segments left to right
            SetSegmentGate(gates, 0, 0, 0, 1, 90, 140, 2, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 0, 0, 1, -1, 90, 140, 34, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 0, 0, 1, 1, 90, 140, 37, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 0, 0, 2, -1, 90, 140, 69, gateNo++/2, letterIndex);

            // Next 2 horizontal segments right to left
            SetSegmentGate(gates, 1, 0, 2, -1, 270,105, 69, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 0, 1, 1, 270, 105, 37, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 0, 1, -1, 270, 105, 34, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 0, 0, 1, 270, 105, 2, gateNo++/2, letterIndex);

            // Next 2 horizontal segments left to right
            SetSegmentGate(gates, 2, 0, 0, 1, 90, 70, 2, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 0, 1, -1, 90, 70, 34, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 0, 1, 1, 90, 70, 37, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 0, 2, -1, 90, 70, 69, gateNo++/2, letterIndex);

            // Next 2 horizontal segments right to left
            SetSegmentGate(gates, 3, 0, 2, -1, 270, 35, 69, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, 0, 1, 1, 270, 35, 37, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, 0, 1, -1, 270, 35, 34, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, 0, 0, 1, 270, 35, 2, gateNo++/2, letterIndex);

            // Top 2 horizontal segments left to right
            SetSegmentGate(gates, 4, 0, 0, 1, 90, 0, 2, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 4, 0, 1, -1, 90, 0, 34, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 4, 0, 1, 1, 90, 0, 37, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 4, 0, 2, -1, 90, 0, 69, gateNo++/2, letterIndex);

            // Lefthand edge 4 vertical segments top to bottom
            SetSegmentGate(gates, 4, -1, 0, 0, 180, 2, 0, gateNo++ / 2, letterIndex);
            SetSegmentGate(gates, 3, 1, 0, 0, 180, 34, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, -1, 0, 0, 180, 37, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 1, 0, 0, 180, 69, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, -1, 0, 0, 180, 72, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 1, 0, 0, 180, 104, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, -1, 0, 0, 180, 107, 0, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 0, 1, 0, 0, 180, 139, 0, gateNo++/2, letterIndex);

            // Next 4 vertical segments bottom to top
            SetSegmentGate(gates, 0, 1, 1, 0, 0, 139, 35, gateNo++ / 2, letterIndex);
            SetSegmentGate(gates, 1, -1, 1, 0, 0, 107, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 1, 1, 0, 0, 104, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, -1, 1, 0, 0, 72, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 1, 1, 0, 0, 69, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, -1, 1, 0, 0, 37, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, 1, 1, 0, 0, 34, 35, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 4, -1, 1, 0, 0, 2, 35, gateNo++/2, letterIndex);

            // Righthand edge 4 vertical segments top to bottom
            SetSegmentGate(gates, 4, -1, 2, 0, 180, 2, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, 1, 2, 0, 180, 34, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 3, -1, 2, 0, 180, 37, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, 1, 2, 0, 180, 69, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 2, -1, 2, 0, 180, 72, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, 1, 2, 0, 180, 104, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 1, -1, 2, 0, 180, 107, 70, gateNo++/2, letterIndex);
            SetSegmentGate(gates, 0, 1, 2, 0, 180, 139, 70, gateNo++/2, letterIndex);

            int noOfGatesForCurrentLetter = gates.Count - 1 - currentLetterGateIndex;
            return noOfGatesForCurrentLetter;
        }

        /// <summary>
        /// Adds a gate to list of gates for the current segment if it is part of the message letter being processed.
        /// </summary>
        /// <param name="gates">Where the gates are stored</param>
        /// <param name="latCoef">How many straight segment portions this segment is right of letter lefthand edge</param>
        /// <param name="latOffsetCoef">How many pointy cap segment portions this segment is right of letter lefthand edge</param>
        /// <param name="lonCoef">How many straight segment portions this segment is above letter bottom edge</param>
        /// <param name="lonOffsetCoef">How many pointy cap segment portions this segment is above letter bottom edge</param>
        /// <param name="orientation">The direction of front side of gate, e.g. 90 means the gate will be triggered when 
        /// flying through it from the west heading east</param>
        /// <param name="topPixels">Top pixel reference for segment in letter</param>
        /// <param name="leftPixels">Left pixel reference for segment in letter</param>
        /// <param name="segmentIndex">Which of the 22 possible segments is being processed</param>
        /// <param name="letterIndex">Indicates which letter in the sign writing message is being processed</param>
        internal static void SetSegmentGate(List<Gate> gates, double latCoef, double latOffsetCoef, double lonCoef, double lonOffsetCoef,
            double orientation, double topPixels, double leftPixels, int segmentIndex, int letterIndex)
        {
            // At this point the letter bottom lefthand edge is at 0 latitude and 0 longitude. Work out
            // from this referenc epoint the lat/lon of segment being processed in current message letter
            double lat = Parameters.SignSegmentLengthDeg * latCoef + Parameters.SignSegmentRadiusDeg * latOffsetCoef;
            double lon = Parameters.SignSegmentLengthDeg * lonCoef + Parameters.SignSegmentRadiusDeg * lonOffsetCoef;

            // Letter starts at 0 amsl and later gets translated to correct height.
            double amsl = 0;

            // Skip segments that don't form part of current letter in sign writing message
            if (!SignWriting.SegmentIsSet(Parameters.SignMessage[letterIndex], segmentIndex))
            {
                return;
            }

            // The tilt angle of message refers to the segments in the vertical plane e.g. the sides of letter 'A'
            // while the horizontal segments are unaffected by tilt angle e.g. the middle and top horizontal lines of letter 'A'
            // So for the gates marking start and finish of vertical segments in a letter the gates are pitched up or down
            // by tilt angle amount depending if the vertical sequence of gates is climbing or descending in altitude.
            double pitch;
            if ((orientation == 90) || (orientation == 270))
            {
                pitch = 0;
            }
            else if (orientation == 180)
            {
                pitch = Parameters.SignTiltAngle;
            }
            else
            {
                pitch = -Parameters.SignTiltAngle;
            }

            // Shift leftPixels based on which letter it is in message
            leftPixels += letterIndex * 105;

            gates.Add(new Gate(lat, lon, amsl, pitch, orientation, topPixels, leftPixels));
        }

        /// <summary>
        /// Translate a subset of gates in list of gates by specified latitude, longitude, and altitude amounts
        /// </summary>
        /// <param name="gates">Where the gates are stored</param>
        /// <param name="startGateIndex">Index of first gate in list of gates to be translated</param>
        /// <param name="noGates">The number of gates in list of gates to be translated </param>
        /// <param name="latAmt">Latitude translation amount</param>
        /// <param name="longAmt">Longitude translation amount</param>
        /// <param name="altAmt">Altitude translation amount</param>
        internal static void TranslateGates(List<Gate> gates, int startGateIndex, int noGates, double latAmt, double longAmt, double altAmt)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                gates[index].lat += latAmt;
                gates[index].lon += longAmt;
                gates[index].amsl += altAmt;
            }
        }

        /// <summary>
        /// Tilt gates in vertical segment plane
        /// </summary>
        /// <param name="gates">Where the gates are stored</param>
        /// <param name="startGateIndex">Index of first gate in list of gates to be translated</param>
        /// <param name="noGates">The number of gates in list of gates to be translated </param>
        internal static void TiltGates(List<Gate> gates, int startGateIndex, int noGates)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                // do altitude change first before adjusting latitude, tilt is on longitude axis, latitudes reduced as segment
                // length is constant but when tilted is shorter over ground
                gates[index].amsl += Math.Abs(gates[index].lat) * Math.Sin(Parameters.SignTiltAngle * Math.PI / 180) * Con.degreeLatFeet;
                gates[index].lat = gates[index].lat * Math.Cos(Parameters.SignTiltAngle * Math.PI / 180);
            }
        }

        #endregion
    }
}
