namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Parameters for a runway sourced from the runways.xml file
    /// </summary>
    public class RunwayParams : ICloneable
    {
        /// <summary>
        /// Four letter code known as ICAO airport code or location indicator
        /// </summary>
        public string IcaoId { get; set; }

        /// <summary>
        /// The name of the airport
        /// </summary>
        public string IcaoName { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        /// <summary>
        /// The longitude of the approximate center of the airport's useable runways
        /// </summary>
        public double AirportLon { get; set; }

        /// <summary>
        /// The latitude of the approximate center of the airport's useable runways
        /// </summary>
        public double AirportLat { get; set; }

        /// <summary>
        /// Airport altitude (AMSL)
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Airport magnetic variation
        /// </summary>
        public double MagVar { get; set; }

        /// <summary>
        /// The runway Id e.g. "05L", the two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N". The number is extracted and stored as "Number" field.
        /// The letter which distinguishes parallel runways is extracted and stored as "Designator" field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N".
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Runway length in feet
        /// </summary>
        public int Len { get; set; }

        /// <summary>
        /// Runway magnetic heading (add magVar for true)
        /// </summary>
        public double Hdg { get; set; }

        /// <summary>
        /// Runway surface material
        /// </summary>
        public string Def { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, one of "None", "Left", "Right", "Center", or "Water". Used in setting the airport landing trigger for a scenario
        /// </summary>
        public string Designator { get; set; }

        /// <summary>
        /// Helper property indicating whether this runway is a water surface/lane.
        /// Evaluates both Designator ("Water") and Surface Definition ("WATER").
        /// </summary>
        public bool IsWaterRunway =>
            string.Equals(Designator, "Water", StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(Def) && Def.Contains("WATER", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Runway threshold latitude
        /// </summary>
        public double ThresholdStartLat { get; set; }

        /// <summary>
        /// Runway threshold longitude
        /// </summary>
        public double ThresholdStartLon { get; set; }

        /// <summary>
        /// Index of runway in <see cref="RunwaySearcher._allRunways"></see>
        /// </summary>
        public int RunwaysIndex { get; set; }

        /// <summary>
        /// Indicates if the runway has any form of lighting (Edge, Center, Approach, or End).
        /// </summary>
        public bool HasLights { get; set; }

        /// <summary>
        /// Clones the airport level runway information prior to reading in each runway for the current airport
        /// </summary>
        /// <returns>Cloned version of <see cref="RunwayParams"/></returns>
        public object Clone()
        {
            var clonedRunwayParams = new RunwayParams
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
                HasLights = false // Default to false, will be set when parsing the specific runway
            };
            return clonedRunwayParams;
        }
    }
}
