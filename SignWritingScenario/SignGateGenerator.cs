using P3D_Scenario_Generator.ConstantsEnums;
using System.Collections.Generic;

namespace P3D_Scenario_Generator.SignWritingScenario
{
    /// <summary>
    /// Defines a record to hold the parameters for a segment, used in generating signwriting messages.
    /// </summary>
    /// <param name="HeightFactor">How many grid lengths this segment is above letter bottom edge</param>
    /// <param name="HeightRadiusFactor">How many radius turns this segment is above letter bottom edge</param>
    /// <param name="WidthFactor">How many grid lengths this segment is right of letter lefthand edge</param>
    /// <param name="WidthRadiusFactor">How many radius turns this segment is right of letter lefthand edge</param>
    /// <param name="Orientation">The direction of front side of gate, e.g. 90 means the gate will be triggered when 
    /// flying through it from the west heading east</param>
    /// <param name="TopPixels">Top pixel reference for segment in letter</param>
    /// <param name="LeftPixels">Left pixel reference for segment in letter</param>
    public record SegmentSpecification(
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
    internal class SignGateGenerator
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
        /// segments used to represent a alphabet letter
        /// </summary>
        /// <returns>The list of gates</returns>
        internal static List<Gate> SetSignGatesMessage(ScenarioFormData formData)
        {
            List<Gate> gates = [];
            for (int index = 0; index < formData.SignMessage.Length; index++)
            {
                if (char.IsLetter(formData.SignMessage[index]))
                {
                    // Add the gates needed for current letter to List<Gate> gates
                    SetSignGatesLetter(gates, index, out int currentLetterNoOfGates, formData);

                    // Move gates just added from 0 lat 0 lon 0 asml reference point to the end of letters in message processed so far
                    // only longitude is changed
                    int startGateIndex = gates.Count - currentLetterNoOfGates;
                    double distanceTranslated = (formData.SignGridUnitSizeFeet * Constants.SignCharWidthGridUnits + Constants.SignCharPaddingGridUnits) * index;
                    TranslateGates(gates, startGateIndex, currentLetterNoOfGates, distanceTranslated, 0);
                }
            }
            TiltGates(gates, 0, gates.Count, formData);

            // Move gates to airport and correct height
            MoveGatesToAirport(gates, formData);
            return gates;
        }

        /// <summary>
        /// Moves a collection of <see cref="Gate"/> objects to positions relative to a specified airport location.
        /// Each gate's latitude and longitude are adjusted based on its original distance and bearing from the
        /// geographical origin (0,0) and then relocated to the corresponding position relative to the airport's
        /// starting runway coordinates. Additionally, the altitude of each gate is set by adding the airport's
        /// altitude to the gate's existing above mean sea level (AMSL) value.
        /// </summary>
        /// <param name="gates">A <see cref="List{T}"/> of <see cref="Gate"/> objects to be repositioned.</param>
        internal static void MoveGatesToAirport(List<Gate> gates, ScenarioFormData formData)
        {
            for (int index = 0; index < gates.Count; index++)
            {
                // Find out distance gate is from origin (0 lat, 0 long)
                double distanceMeters = MathRoutines.CalcDistance(0, 0, gates[index].lat, gates[index].lon) * Constants.MetresInNauticalMile;

                // Findout bearing of gate from origin (0 lat, 0 long)
                double bearing = MathRoutines.CalcBearing(0, 0, gates[index].lat, gates[index].lon);

                // Move gate to position relative to airport location
                MathRoutines.AdjCoords(Runway.startRwy.AirportLat, Runway.startRwy.AirportLon, bearing, distanceMeters,
                    ref gates[index].lat, ref gates[index].lon);

                // Set the altitude of the gate to the airport altitude plus the sign gate height
                gates[index].amsl += Runway.startRwy.Altitude + formData.SignGateHeightFeet;
            }
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
        internal static void SetSignGatesLetter(List<Gate> gates, int letterIndex, out int currentLetterNoOfGates, ScenarioFormData formData)
        {
            int currentLetterGateIndex = gates.Count - 1;
            int gateNo = 0; // Integer division by 2 gives the segment index

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
                               gateNo++ / 2, // This calculates the segmentIndex
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
        /// <param name="WidthRadiusFactor">How many radius turns this segment is right of letter lefthand edge</param>
        /// <param name="orientation">The true bearing (in degrees, 0-360) defining the direction an aircraft must fly through the gate to trigger it.
        /// For example, 90 degrees means flying from west to east.</param>
        /// <param name="topPixels">The pixel Y-coordinate of the segment's origin relative to the top edge of the letter's bounding box.</param>
        /// <param name="leftPixels">The pixel X-coordinate of the segment's origin relative to the left edge of the letter's bounding box.</param>
        /// <param name="segmentIndex">The zero-based index (0-21) identifying which of the 22 possible segments of a letter is being processed.</param>
        /// <param name="letterIndex">The zero-based index indicating the position of the current letter within the overall sign message.</param>
        /// <param name="formData">A <see cref="ScenarioFormData"/> object containing global scenario parameters, such as sign message text,
        /// grid unit size, segment radius, and tilt angle.</param>
        internal static void SetSegmentGate(List<Gate> gates, double HeightFactor, double HeightRadiusFactor, double WidthFactor, double WidthRadiusFactor,
            double orientation, double topPixels, double leftPixels, int segmentIndex, int letterIndex, ScenarioFormData formData)
        {
            // Check if the current segment is active for the specific letter in the sign message.
            // If not, this segment does not form part of the letter, and no gate is added.
            if (!SignCharacterMap.SegmentIsSet(formData.SignMessage[letterIndex], segmentIndex))
            {
                return;
            }

            // Calculate the segment's latitude and longitude relative to the letter's origin (bottom-left at 0,0).
            // The latitude calculation assumes a northerly bearing (0 degrees) for the distance.
            double finishLat = 0;
            double finishLon = 0;
            // Using a named discard variable as recommended for ref parameters, instead of directly using `_`.
            double ignoredLatLon = 0;

            double latitudeDistance = (formData.SignGridUnitSizeFeet * HeightFactor + formData.SignSegmentRadiusFeet * HeightRadiusFactor) * Constants.MetresInFoot;
            MathRoutines.AdjCoords(0, 0, 0, latitudeDistance, ref finishLat, ref ignoredLatLon);

            // The longitude calculation assumes an easterly bearing (90 degrees) for the distance.
            double longitudeDistance = (formData.SignGridUnitSizeFeet * WidthFactor + formData.SignSegmentRadiusFeet * WidthRadiusFactor) * Constants.MetresInFoot;
            MathRoutines.AdjCoords(0, 0, 90, longitudeDistance, ref ignoredLatLon, ref finishLon);

            // Initial altitude (AMSL) is set to zero; the entire sign will be translated to its final height later.
            double amsl = 0;

            // Determine the pitch angle for the gate based on its orientation and the sign's tilt angle.
            // Vertical segments (north/south orientation) may be pitched up or down according to the sign's tilt.
            // Horizontal segments (east/west orientation) remain level (0 pitch).
            double pitch;
            if (orientation == 90 || orientation == 270) // East or West orientation (horizontal segments)
            {
                pitch = 0;
            }
            else if (orientation == 180) // South orientation (descending vertical segments)
            {
                pitch = formData.SignTiltAngleDegrees;
            }
            else // North orientation (climbing vertical segments)
            {
                pitch = -formData.SignTiltAngleDegrees;
            }

            // Adjust the 'leftPixels' reference based on the letter's position within the overall sign message.
            // This accounts for character width and inter-character padding.
            leftPixels += letterIndex * (Constants.SignCharWidthPixels + Constants.SignCharPaddingInternalPixels);

            // Add the newly created gate with its calculated properties to the list.
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
                MathRoutines.AdjCoords(gates[index].lat, gates[index].lon, headingToMoveEast, distance, ref gates[index].lat, ref gates[index].lon); 
                gates[index].amsl += altAmt;
            }
        }

        /// <summary>
        /// Tilts a specified range of gates in a vertical segment plane relative to the equator.
        /// <para>
        /// This operation adjusts both the latitude and altitude of the gates based on a tilt angle.
        /// The tilt is conceptualized as rotating a segment that originates at the equator (latitude 0)
        /// on the gate's longitude and extends vertically (along the meridian) to the gate's original latitude.
        /// </para>
        /// <para>
        /// The <paramref name="formData.SignTiltAngle"/> represents the angle of tilt from the vertical.
        /// A 0-degree tilt means the gate remains at its original latitude and altitude (no change).
        /// A 90-degree tilt means the gate's latitude moves to the equator (latitude 0) and its altitude
        /// increases by its original meridian distance from the equator.
        /// </para>
        /// <para>
        /// This method assumes all gates are located in the Northern Hemisphere, meaning their latitudes are non-negative.
        /// </para>
        /// </summary>
        /// <param name="gates">The list of Gate objects to be modified.</param>
        /// <param name="startGateIndex">The zero-based index of the first gate in the list to be processed.</param>
        /// <param name="noGates">The number of gates, starting from <paramref name="startGateIndex"/>, to apply the tilt to.</param>
        /// <param name="formData">The scenario form data, containing the <see cref="ScenarioFormData.SignTiltAngleDegrees"/> in degrees.</param>
        internal static void TiltGates(List<Gate> gates, int startGateIndex, int noGates, ScenarioFormData formData)
        {
            for (int index = startGateIndex; index < startGateIndex + noGates; index++)
            {
                // Store original gate latitude and longitude for calculations.
                double originalLat = gates[index].lat;
                double originalLon = gates[index].lon;

                // Convert tilt angle from degrees to radians for trigonometric functions.
                double tiltAngleRadians = formData.SignTiltAngleDegrees * Math.PI / 180;

                // 1. Calculate the initial vertical offset (distance along the meridian)
                //    from the equator (latitude 0) at the gate's longitude to the gate's original latitude.
                //    This represents the "length" of the segment being tilted.
                //    MathRoutines.CalcDistance returns distance in nautical miles, so convert to meters.
                //    Since gates are always in the Northern Hemisphere, originalLat will be >= 0.
                double initialMeridianDistanceMeters = MathRoutines.CalcDistance(0, originalLon, originalLat, originalLon) * Constants.MetresInNauticalMile;

                // 2. Calculate the new horizontal projection of this segment after tilting.
                //    This will be the new distance from the equator along the meridian.
                //    This value determines the new latitude.
                double tiltedMeridianDistanceMeters = initialMeridianDistanceMeters * Math.Cos(tiltAngleRadians);

                // 3. Calculate the altitude adjustment (vertical projection) based on the tilt.
                //    This is the change in AMSL altitude.
                //    The altitude adjustment is always positive as it represents an upward shift due to tilt.
                double altitudeAdjustment = initialMeridianDistanceMeters * Math.Sin(tiltAngleRadians);

                // 4. Adjust the gate's latitude based on the tilted distance from the equator.
                //    The longitude remains constant as the tilt is strictly within the meridian plane.
                double newLat = 0; // Will hold the calculated new latitude
                double tempLon = originalLon; // Used by AdjCoords, should ideally remain originalLon

                // Since gates are always in the Northern Hemisphere, the heading to move from the
                // equator to the new latitude will always be 0 (North).
                const double headingToMoveNorth = 0;

                // Use AdjCoords to find the new latitude by starting from the equator at the
                // gate's longitude and moving the 'tiltedMeridianDistanceMeters' along the meridian.
                MathRoutines.AdjCoords(0, originalLon, headingToMoveNorth, tiltedMeridianDistanceMeters, ref newLat, ref tempLon);

                // Update the gate's latitude with the newly calculated value.
                gates[index].lat = newLat;
                // Explicitly preserve the original longitude, as the tilt is meridional and
                // AdjCoords might introduce minor longitude changes due to ellipsoid calculations
                // for very large distances or near poles, which is not desired here.
                gates[index].lon = originalLon;

                // 5. Adjust the gate's altitude (AMSL) by adding the calculated altitude adjustment.
                gates[index].amsl += altitudeAdjustment;
            }
        }
    }
}
