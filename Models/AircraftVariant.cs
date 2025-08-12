namespace P3D_Scenario_Generator.Models
{
    /// <summary>
    /// Stores information for a user selected aircraft variant; title, display name, cruisespeed, 
    /// thumbnail image full path, whether it has floats, and whether it has wheels equivalent
    /// </summary>
    public class AircraftVariant
    {
        /// <summary>
        /// The title of the aircraft variant as recorded in the relevant variant [fltsim.?] section of an aircraft.cfg file
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The user editable name of the aircraft variant for display purposes on General tab of form
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The cruise speed in knots of the aircraft variant as recorded in the aircraft.cfg file
        /// </summary>
        public double CruiseSpeed { get; set; }

        /// <summary>
        /// Full path including filename of selected thumbnail.jpg file or empty string
        /// </summary>
        public string ThumbnailImagePath { get; set; }

        /// <summary>
        /// Whether the aircraft has floats, used to exclude takeoff/landing for water runways if selected aircraft doesn't have floats
        /// </summary>
        public bool HasFloats { get; set; }

        /// <summary>
        /// Whether the aircraft has wheels/scrapes/skids/skis, used to exclude takeoff from land based (non water) runways if selected 
        /// aircraft doesn't have them. Note landing possible with straight floats.
        /// </summary>
        public bool HasWheelsOrEquiv { get; set; }
    }
}
