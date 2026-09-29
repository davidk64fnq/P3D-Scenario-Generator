namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Parameters for an airport runway sourced from runways data.
    /// </summary>
    internal class RunwayParams : ICloneable
    {
        /// <summary>
        /// Four-letter ICAO airport code or location indicator.
        /// </summary>
        internal string IcaoId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the airport.
        /// </summary>
        internal string IcaoName { get; set; } = string.Empty;

        /// <summary>
        /// The country where the airport is located.
        /// </summary>
        internal string Country { get; set; } = string.Empty;

        /// <summary>
        /// The state or province where the airport is located.
        /// </summary>
        internal string State { get; set; } = string.Empty;

        /// <summary>
        /// The city where the airport is located.
        /// </summary>
        internal string City { get; set; } = string.Empty;

        /// <summary>
        /// The longitude of the approximate center of the airport's usable runways.
        /// </summary>
        internal double AirportLon { get; set; }

        /// <summary>
        /// The latitude of the approximate center of the airport's usable runways.
        /// </summary>
        internal double AirportLat { get; set; }

        /// <summary>
        /// Airport altitude (AMSL).
        /// </summary>
        internal double Altitude { get; set; }

        /// <summary>
        /// Airport magnetic variation.
        /// </summary>
        internal double MagVar { get; set; }

        /// <summary>
        /// The runway identifier (e.g. "05L"). The two-digit number represents tens of degrees magnetic heading.
        /// </summary>
        internal string Id { get; set; } = string.Empty;

        /// <summary>
        /// The runway heading number (e.g. "05", or "37" for special designations).
        /// </summary>
        internal string Number { get; set; } = string.Empty;

        /// <summary>
        /// Runway length in feet.
        /// </summary>
        internal int Len { get; set; }

        /// <summary>
        /// Runway magnetic heading.
        /// </summary>
        internal double Hdg { get; set; }

        /// <summary>
        /// Runway surface material definition.
        /// </summary>
        internal string Def { get; set; } = string.Empty;

        /// <summary>
        /// Runway designator ("None", "Left", "Right", "Center", or "Water").
        /// </summary>
        internal string Designator { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether this runway has a water surface.
        /// </summary>
        internal bool IsWaterRunway =>
            string.Equals(Designator, "Water", StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(Def) && Def.Contains("WATER", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Runway threshold start latitude.
        /// </summary>
        internal double ThresholdStartLat { get; set; }

        /// <summary>
        /// Runway threshold start longitude.
        /// </summary>
        internal double ThresholdStartLon { get; set; }

        /// <summary>
        /// Index of the runway in the global runways collection.
        /// </summary>
        internal int RunwaysIndex { get; set; }

        /// <summary>
        /// Indicates if the runway has any form of lighting (Edge, Center, Approach, or End).
        /// </summary>
        internal bool HasLights { get; set; }

        /// <summary>
        /// Clones the airport-level runway information prior to reading in individual runways.
        /// </summary>
        /// <returns>A new <see cref="RunwayParams"/> instance copied from this instance.</returns>
        internal RunwayParams Clone()
        {
            return new RunwayParams
            {
                IcaoId = IcaoId,
                IcaoName = IcaoName,
                Country = Country,
                State = State,
                City = City,
                AirportLon = AirportLon,
                AirportLat = AirportLat,
                Altitude = Altitude,
                MagVar = MagVar,
                RunwaysIndex = RunwaysIndex,
                HasLights = false
            };
        }

        object ICloneable.Clone() => Clone();
    }
}