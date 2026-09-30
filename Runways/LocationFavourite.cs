using System.Text.Json.Serialization;

namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Stores Country, State, and City filter criteria for a saved location favourite.
    /// </summary>
    internal class LocationFavourite()
    {
        [JsonInclude]
        public string Name { get; set; } = string.Empty;

        [JsonInclude]
        public List<string> Countries { get; set; } = ["None"];

        [JsonInclude]
        public List<string> States { get; set; } = ["None"];

        [JsonInclude]
        public List<string> Cities { get; set; } = ["None"];

        internal LocationFavourite(LocationFavourite original) : this()
        {
            ArgumentNullException.ThrowIfNull(original);

            Name = original.Name;
            Countries = original.Countries?.ToList() ?? ["None"];
            States = original.States?.ToList() ?? ["None"];
            Cities = original.Cities?.ToList() ?? ["None"];
        }
    }
}