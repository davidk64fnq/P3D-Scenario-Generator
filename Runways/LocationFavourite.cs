namespace P3D_Scenario_Generator.Runways
{

    /// <summary>
    /// Stores the Country/State/City location filter values for a location favourite
    /// </summary>
    public class LocationFavourite() 
    {
        /// <summary>
        /// The name of the favourite
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The list of valid country strings for this favourite 
        /// </summary>
        public List<string> Countries { get; set; } = [];

        /// <summary>
        /// The list of valid state strings for this favourite 
        /// </summary>
        public List<string> States { get; set; } = [];

        /// <summary>
        /// The list of valid city strings for this favourite 
        /// </summary>
        public List<string> Cities { get; set; } = [];

        // Copy constructor 
        public LocationFavourite(LocationFavourite original) : this() // Calls the primary constructor for initialization
        {
            this.Name = original.Name;
            this.Countries = original.Countries?.ToList() ?? [];
            this.States = original.States?.ToList() ?? [];
            this.Cities = original.Cities?.ToList() ?? [];
        }
    }
}
