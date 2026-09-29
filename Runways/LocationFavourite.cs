namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Stores Country, State, and City filter criteria for a saved location favourite.
    /// </summary>
    internal class LocationFavourite()
    {
        /// <summary>
        /// The name of the favourite.
        /// </summary>
        internal string Name { get; set; } = string.Empty;

        /// <summary>
        /// The list of valid country strings for this favourite.
        /// </summary>
        internal List<string> Countries { get; set; } = [];

        /// <summary>
        /// The list of valid state strings for this favourite.
        /// </summary>
        internal List<string> States { get; set; } = [];

        /// <summary>
        /// The list of valid city strings for this favourite.
        /// </summary>
        internal List<string> Cities { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationFavourite"/> class by copying another instance.
        /// </summary>
        /// <param name="original">The original instance to copy.</param>
        internal LocationFavourite(LocationFavourite original) : this()
        {
            ArgumentNullException.ThrowIfNull(original);

            Name = original.Name;
            Countries = original.Countries?.ToList() ?? [];
            States = original.States?.ToList() ?? [];
            Cities = original.Cities?.ToList() ?? [];
        }
    }
}