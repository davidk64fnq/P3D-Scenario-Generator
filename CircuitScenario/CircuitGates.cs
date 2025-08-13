using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;

namespace P3D_Scenario_Generator.CircuitScenario
{
    /// <summary>
    /// Manages the calculation and definition of parameters for a flight circuit within the simulator.
    /// This includes determining turn radii, interpolating gate heights, and setting the
    /// geographical and altitude data for each of the eight circuit gates based on
    /// user-defined parameters and aircraft performance characteristics.
    /// </summary>
    internal class CircuitGates
    {
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
        internal static double CalcTurnRadius(ScenarioFormData formData)
        {
            // Calculate number of feet travelled by plane in an hour based on cruise speed
            double speedInFeetPerHour = formData.CircuitSpeed * Constants.FeetInNauticalMile;

            // Calculate number of feet travelled by plane to complete a circle in number of minutes given by CircuitTurnDuration360Degrees
            double turnCircumference = speedInFeetPerHour * (formData.CircuitTurnDuration360Degrees / 60);

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
        internal static double CalcGate1to2HeightDif(double turnRadius, ScenarioFormData formData)
        {
            // Firstly work out climb rate angle from gate 1 to 3. Approx given by solving
            // tan(angle) = opposite(height change) / adjacent (over ground distance)
            double heightChange = formData.CircuitHeightDown - formData.CircuitHeightUpwind;
            double overGroundDistance = turnRadius + formData.CircuitBaseLeg * Constants.FeetInNauticalMile;
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
        internal static double CalcGate7to8HeightDif(double turnRadius, ScenarioFormData formData)
        {
            // Firstly work out descent rate angle from gate 6 to 8. Approx given by solving
            // tan(angle) = opposite(height change) / adjacent (over ground distance)
            double heightChange = formData.CircuitHeightDown - formData.CircuitHeightBase;
            double overGroundDistance = turnRadius + formData.CircuitBaseLeg * Constants.FeetInNauticalMile;
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
        internal static List<LegParams> SetLegParams(double turnRadius, double gate1to2heightDif, double gate7to8heightDif, ScenarioFormData formData)
        {
            List<LegParams> legParams = [];
            double baseHeading = formData.StartRunway.Hdg + formData.StartRunway.MagVar + 360;
            double turnDistance = turnRadius * Math.Sqrt(2.0);
            // Start theshold to gate 1
            legParams.Add(new LegParams(baseHeading % 360,
                formData.StartRunway.Len + formData.CircuitUpwindLeg * Constants.FeetInNauticalMile, formData.StartRunway.Altitude + formData.CircuitHeightUpwind));
            // Gate 1 to gate 2
            legParams.Add(new LegParams((baseHeading - 45) % 360,
                turnDistance, formData.StartRunway.Altitude + formData.CircuitHeightUpwind + gate1to2heightDif));
            // Gate 2 to gate 3
            legParams.Add(new LegParams((baseHeading - 90) % 360,
                formData.CircuitBaseLeg * Constants.FeetInNauticalMile, formData.StartRunway.Altitude + formData.CircuitHeightDown));
            // Gate 3 to gate 4
            legParams.Add(new LegParams((baseHeading - 135) % 360,
                turnDistance, formData.StartRunway.Altitude + formData.CircuitHeightDown));
            // Gate 4 to gate 5
            legParams.Add(new LegParams((baseHeading - 180) % 360,
                formData.CircuitFinalLeg * Constants.FeetInNauticalMile + formData.StartRunway.Len + formData.CircuitUpwindLeg * Constants.FeetInNauticalMile,
                formData.StartRunway.Altitude + formData.CircuitHeightDown));
            // Gate 5 to gate 6
            legParams.Add(new LegParams((baseHeading - 225) % 360,
                turnDistance, formData.StartRunway.Altitude + formData.CircuitHeightDown));
            // Gate 6 to gate 7
            legParams.Add(new LegParams((baseHeading - 270) % 360,
                formData.CircuitBaseLeg * Constants.FeetInNauticalMile, formData.StartRunway.Altitude + formData.CircuitHeightBase + gate7to8heightDif));
            // Gate 7 to gate 8
            legParams.Add(new LegParams((baseHeading - 315) % 360,
                turnDistance, formData.StartRunway.Altitude + formData.CircuitHeightBase));
            return legParams;
        }

        /// <summary>
        /// There are 8 gates, 2 for each right angle turn. Gates 1, 3 to 6, and 8 are at heights specified by three user parameters.
        /// The length of the upwind, base and final legs is user supplied. The program calculates the downwind based on runway
        /// length and user parameters. Program calculates height of gates 2 and 7 by interpolation between gates 1 and 3, and 
        /// 6 and 8 respectively taking into account user supplied turn rate and cruise speed of selected aircraft.
        /// </summary>
        /// <returns>List of eight gates with data needed to place gate objects into simulation</returns>
        internal static bool SetCircuitGates(List<Gate> gates, ScenarioFormData formData)
        {
            gates.Clear();
            // Amount each gate is rotated from initial runway heading, e.g. 1st gate not at all, 2nd and 3rd gates 90 degrees
            double[] circuitHeadingAdj = [360, 90, 90, 180, 180, 270, 270, 360];
            double gateOrientation;

            double turnRadius = CalcTurnRadius(formData);
            double gate1to2heightDif = CalcGate1to2HeightDif(turnRadius, formData);
            double gate7to8heightDif = CalcGate7to8HeightDif(turnRadius, formData);
            List<LegParams> legParams = SetLegParams(turnRadius, gate1to2heightDif, gate7to8heightDif, formData);

            Coordinate start = new(formData.StartRunway.ThresholdStartLat, formData.StartRunway.ThresholdStartLon);
            for (int index = 0; index < legParams.Count; index++)
            {
                start.Move(legParams[index].distance, legParams[index].heading, Shape.Ellipsoid);
                gateOrientation = (formData.StartRunway.Hdg + formData.StartRunway.MagVar + circuitHeadingAdj[index]) % 360;
                gates.Add(new Gate(start.Latitude.DecimalDegree, start.Longitude.DecimalDegree, legParams[index].amsl, 0, gateOrientation, 0, 0));
            }
            return true;
        }
    }
}
