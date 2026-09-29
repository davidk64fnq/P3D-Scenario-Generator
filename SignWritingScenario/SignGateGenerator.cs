using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Utilities;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Holds the geometric and orientational parameters for a sign segment used in generating signwriting messages.
    /// </summary>
    /// <param name="HeightFactor">How many grid lengths this segment is above the letter's bottom edge.</param>
    /// <param name="HeightRadiusFactor">How many radius turns this segment is above the letter's bottom edge.</param>
    /// <param name="WidthFactor">How many grid lengths this segment is right of the letter's left edge.</param>
    /// <param name="WidthRadiusFactor">How many radius turns this segment is right of the letter's left edge.</param>
    /// <param name="Orientation">The triggering orientation of the gate in degrees (e.g., 90 means west-to-east).</param>
    /// <param name="TopPixels">Top pixel reference for the segment in the letter display.</param>
    /// <param name="LeftPixels">Left pixel reference for the segment in the letter display.</param>
    internal record SegmentSpecification(
        double HeightFactor,
        double HeightRadiusFactor,
        double WidthFactor,
        double WidthRadiusFactor,
        double Orientation,
        double TopPixels,
        double LeftPixels
    );

    /// <summary>
    /// Provides methods for generating and manipulating gates to form signwriting messages in a simulated environment.
    /// This includes defining segment specifications, creating gates for individual letters,
    /// and applying transformations such as translation and tilting to the generated gates.
    /// </summary>
    internal static class SignGateGenerator
    {
        /// <summary>
        /// A static readonly list containing all 44 predefined <see cref="SegmentSpecification"/> instances.
        /// These specifications define the geometric and orientational properties for each of the
        /// possible segments that can be used to construct letters in the signwriting message.
        /// The segments are ordered to facilitate the sequential generation of gates for a letter.
        /// </summary>
        private static readonly List<SegmentSpecification> AllSegmentDefinitions =
        [
            // Bottom 2 horizontal segments left to right
            new SegmentSpecification(0, 0, 0, 1, 90, 140, 2),
            new SegmentSpecification(0, 0, 1, -1, 90, 140, 34),
            new SegmentSpecification(0, 0, 1, 1, 90, 140, 37),
            new SegmentSpecification(0, 0, 2, -1, 90, 140, 69),

            // Next 2 horizontal segments right to left
            new SegmentSpecification(1, 0, 2, -1, 270, 105, 69),
            new SegmentSpecification(1, 0, 1, 1, 270, 105, 37),
            new SegmentSpecification(1, 0, 1, -1, 270, 105, 34),
            new SegmentSpecification(1, 0, 0, 1, 270, 105, 2),

            // Next 2 horizontal segments left to right
            new SegmentSpecification(2, 0, 0, 1, 90, 70, 2),
            new SegmentSpecification(2, 0, 1, -1, 90, 70, 34),
            new SegmentSpecification(2, 0, 1, 1, 90, 70, 37),
            new SegmentSpecification(2, 0, 2, -1, 90, 70, 69),

            // Next 2 horizontal segments right to left
            new SegmentSpecification(3, 0, 2, -1, 270, 35, 69),
            new SegmentSpecification(3, 0, 1, 1, 270, 35, 37),
            new SegmentSpecification(3, 0, 1, -1, 270, 35, 34),
            new SegmentSpecification(3, 0, 0, 1, 270, 35, 2),

            // Top 2 horizontal segments left to right
            new SegmentSpecification(4, 0, 0, 1, 90, 0, 2),
            new SegmentSpecification(4, 0, 1, -1, 90, 0, 34),
            new SegmentSpecification(4, 0, 1, 1, 90, 0, 37),
            new SegmentSpecification(4, 0, 2, -1, 90, 0, 69),

            // Lefthand edge 4 vertical segments top to bottom
            new SegmentSpecification(4, -1, 0, 0, 180, 2, 0),
            new SegmentSpecification(3, 1, 0, 0, 180, 34, 0),
            new SegmentSpecification(3, -1, 0, 0, 180, 37, 0),
            new SegmentSpecification(2, 1, 0, 0, 180, 69, 0),
            new SegmentSpecification(2, -1, 0, 0, 180, 72, 0),
            new SegmentSpecification(1, 1, 0, 0, 180, 104, 0),
            new SegmentSpecification(1, -1, 0, 0, 180, 107, 0),
            new SegmentSpecification(0, 1, 0, 0, 180, 139, 0),

            // Next 4 vertical segments bottom to top
            new SegmentSpecification(0, 1, 1, 0, 0, 139, 35),
            new SegmentSpecification(1, -1, 1, 0, 0, 107, 35),
            new SegmentSpecification(1, 1, 1, 0, 0, 104, 35),
            new SegmentSpecification(2, -1, 1, 0, 0, 72, 35),
            new SegmentSpecification(2, 1, 1, 0, 0, 69, 35),
            new SegmentSpecification(3, -1, 1, 0, 0, 37, 35),
            new SegmentSpecification(3, 1, 1, 0, 0, 34, 35),
            new SegmentSpecification(4, -1, 1, 0, 0, 2, 35),

            // Righthand edge 4 vertical segments top to bottom
            new SegmentSpecification(4, -1, 2, 0, 180, 2, 70),
            new SegmentSpecification(3, 1, 2, 0, 180, 34, 70),
            new SegmentSpecification(3, -1, 2, 0, 180, 37, 70),
            new SegmentSpecification(2, 1, 2, 0, 180, 69, 70),
            new SegmentSpecification(2, -1, 2, 0, 180, 72, 70),
            new SegmentSpecification(1, 1, 2, 0, 180, 104, 70),
            new SegmentSpecification(1, -1, 2, 0, 180, 107, 70),
            new SegmentSpecification(0, 1, 2, 0, 180, 139, 70)
        ];

        /// <summary>
        /// Create a set of gates for signwriting message. Start and finish gate for a subset of the 22 possible
        /// segments used to represent an alphabet letter.
        /// </summary>
        /// <param name="gates">The collection to which generated gates are added.</param>
        /// <param name="formData">The scenario form data containing sign message configuration.</param>
        internal static void SetSignGatesMessage(List<Gate> gates, ScenarioFormData formData)
        {
            for (int index = 0; index < formData.SignMessage.Length; index++)
            {
                if (char.IsLetter(formData.SignMessage[index]))
                {
                    SetSignGatesLetter(gates, index, out int currentLetterNoOfGates, formData);

                    int startGateIndex = gates.Count - currentLetterNoOfGates;
                    double distanceTranslated = ((formData.SignGridUnitSizeFeet * Constants.SignCharWidthGridUnits) + Constants.SignCharPaddingGridUnits) * index;
                    TranslateGates(gates, startGateIndex, currentLetterNoOfGates, distanceTranslated, 0);
                }
            }
            TiltGates(gates, 0, gates.Count, formData);

            MoveGatesToAirport(gates, formData);
        }

        /// <summary>
        /// Moves a collection of <see cref="Gate"/> objects to positions relative to a specified airport location.
        /// Each gate's latitude and longitude are adjusted based on its original distance and bearing from the
        /// geographical origin (0,0) and then relocated to the corresponding position relative to the airport's
        /// starting runway coordinates. Additionally, the altitude of each gate is set by adding the airport's
        /// altitude to the gate's existing above mean sea level (AMSL) value.
        /// </summary>
        /// <param name="gates">A <see cref="List{T}"/> of <see cref="Gate"/> objects to be repositioned.</param>
        /// <param name="formData">The scenario form data containing runway and gate height configuration.</param>
        internal static void MoveGatesToAirport(List<Gate> gates, ScenarioFormData formData)
        {
            for (int index = 0; index < gates.Count; index++)
            {
                double distanceMeters = MathRoutines.CalcDistance(0, 0, gates[index].lat, gates[index].lon) * Constants.MetresInNauticalMile;
                double bearing = MathRoutines.CalcBearing(0, 0, gates[index].lat, gates[index].lon);

                MathRoutines.AdjCoords(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon, bearing, distanceMeters,
                    out gates[index].lat, out gates[index].lon);

                gates[index].amsl += formData.StartRunway.Altitude + formData.SignGateHeightFeet;
            }
        }

        /// <summary>
        /// Populates gates for a letter in the signwriting message across active segments.
        /// </summary>
        /// <param name="gates">Where the gates are stored as they are created.</param>
        /// <param name="letterIndex">Indicates which letter in the sign writing message is being processed.</param>
        /// <param name="currentLetterNoOfGates">When this method returns, contains the count of gates added for this letter.</param>
        /// <param name="formData">The scenario form data containing configuration parameters.</param>
        internal static void SetSignGatesLetter(List<Gate> gates, int letterIndex, out int currentLetterNoOfGates, ScenarioFormData formData)
        {
            int currentLetterGateIndex = gates.Count - 1;
            int gateNo = 0;

            foreach (var spec in AllSegmentDefinitions)
            {
                SetSegmentGate(gates,
                               spec.HeightFactor,
                               spec.HeightRadiusFactor,
                               spec.WidthFactor,
                               spec.WidthRadiusFactor,
                               spec.Orientation,
                               spec.TopPixels,
                               spec.LeftPixels,
                               gateNo++ / 2,
                               letterIndex,
                               formData);
            }

            currentLetterNoOfGates = gates.Count - 1 - currentLetterGateIndex;
        }

        /// <summary>
        /// Calculates the geographic position and attributes for a specific gate segment within a sign writing message
        /// and adds it to the provided list of gates, but only if the segment is part of the currently processed letter.
        /// </summary>
        /// <param name="gates">The list where the generated Gate object will be added.</param>
        /// <param name="HeightFactor">How many grid lengths this segment is above letter bottom edge.</param>
        /// <param name="HeightRadiusFactor">How many radius turns this segment is above letter bottom edge.</param>
        /// <param name="WidthFactor">How many grid lengths this segment is right of letter lefthand edge.</param>
        /// <param name="WidthRadiusFactor">How many radius turns this segment is right of letter lefthand edge.</param>
        /// <param name="orientation">The true bearing (in degrees, 0-360) defining the direction an aircraft must fly through the gate to trigger it.</param>
        /// <param name="topPixels">The pixel Y-coordinate of the segment's origin relative to the top edge of the letter's bounding box.</param>
        /// <param name="leftPixels">The pixel X-coordinate of the segment's origin relative to the left edge of the letter's bounding box.</param>
        /// <param name="segmentIndex">The zero-based index (0-21) identifying which of the 22 possible segments of a letter is being processed.</param>
        /// <param name="letterIndex">The zero-based index indicating the position of the current letter within the overall sign message.</param>
        /// <param name="formData">A <see cref="ScenarioFormData"/> object containing global scenario parameters.</param>
        internal static void SetSegmentGate(List<Gate> gates, double HeightFactor, double HeightRadiusFactor, double WidthFactor, double WidthRadiusFactor,
            double orientation, double topPixels, double leftPixels, int segmentIndex, int letterIndex, ScenarioFormData formData)
        {
            if (!SignCharacterMap.SegmentIsSet(formData.SignMessage[letterIndex], segmentIndex))
            {
                return;
            }

            double latitudeDistance = ((formData.SignGridUnitSizeFeet * HeightFactor) + (formData.SignSegmentRadiusFeet * HeightRadiusFactor)) * Constants.MetresInFoot;
            MathRoutines.AdjCoords(0, 0, 0, latitudeDistance, out double finishLat, out _);

            double longitudeDistance = ((formData.SignGridUnitSizeFeet * WidthFactor) + (formData.SignSegmentRadiusFeet * WidthRadiusFactor)) * Constants.MetresInFoot;
            MathRoutines.AdjCoords(0, 0, 90, longitudeDistance, out _, out double finishLon);

            const double amsl = 0;

            double pitch;
            if (orientation == 90 || orientation == 270)
            {
                pitch = 0;
            }
            else if (orientation == 180)
            {
                pitch = formData.SignTiltAngleDegrees;
            }
            else
            {
                pitch = -formData.SignTiltAngleDegrees;
            }

            leftPixels += letterIndex * (Constants.SignCharWidthPixels + Constants.SignCharPaddingInternalPixels);

            gates.Add(new Gate(finishLat, finishLon, amsl, pitch, orientation, topPixels, leftPixels));
        }

        /// <summary>
        /// Translates a specified subset of gates within a list by a given easterly distance and altitude amount.
        /// </summary>
        /// <param name="gates">The list containing the Gate objects to be translated.</param>
        /// <param name="startGateIndex">The zero-based index of the first gate in the list to begin translation from.</param>
        /// <param name="noGates">The total number of gates to translate, starting from <paramref name="startGateIndex"/>.</param>
        /// <param name="distance">The distance in meters by which to shift each selected gate eastward.</param>
        /// <param name="altAmt">The amount in meters to adjust the altitude (AMSL) of each selected gate.</param>
        internal static void TranslateGates(List<Gate> gates, int startGateIndex, int noGates, double distance, double altAmt)
        {
            const double headingToMoveEast = 90;
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                MathRoutines.AdjCoords(gates[index].lat, gates[index].lon, headingToMoveEast, distance, out gates[index].lat, out gates[index].lon);
                gates[index].amsl += altAmt;
            }
        }

        /// <summary>
        /// Tilts a specified range of gates in a vertical segment plane relative to the equator.
        /// </summary>
        /// <param name="gates">The list of Gate objects to be modified.</param>
        /// <param name="startGateIndex">The zero-based index of the first gate in the list to be processed.</param>
        /// <param name="noGates">The number of gates, starting from <paramref name="startGateIndex"/>, to apply the tilt to.</param>
        /// <param name="formData">The scenario form data, containing the tilt angle in degrees.</param>
        internal static void TiltGates(List<Gate> gates, int startGateIndex, int noGates, ScenarioFormData formData)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                double originalLat = gates[index].lat;
                double originalLon = gates[index].lon;

                double tiltAngleRadians = formData.SignTiltAngleDegrees * Math.PI / 180;
                double initialMeridianDistanceMeters = MathRoutines.CalcDistance(0, originalLon, originalLat, originalLon) * Constants.MetresInNauticalMile;
                double tiltedMeridianDistanceMeters = initialMeridianDistanceMeters * Math.Cos(tiltAngleRadians);
                double altitudeAdjustment = initialMeridianDistanceMeters * Math.Sin(tiltAngleRadians);

                const double headingToMoveNorth = 0;
                MathRoutines.AdjCoords(0, originalLon, headingToMoveNorth, tiltedMeridianDistanceMeters, out double newLat, out _);

                gates[index].lat = newLat;
                gates[index].lon = originalLon;
                gates[index].amsl += altitudeAdjustment;
            }
        }
    }
}