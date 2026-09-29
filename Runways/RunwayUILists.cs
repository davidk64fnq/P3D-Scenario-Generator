namespace P3D_Scenario_Generator.Runways
{
    /// <summary>
    /// Holds collections of distinct Countries, States, Cities, and Runway identifiers used to populate UI selection dropdowns.
    /// </summary>
    internal class RunwayUILists
    {
        internal List<string> Countries { get; set; } = [];
        internal List<string> States { get; set; } = [];
        internal List<string> Cities { get; set; } = [];
        internal List<string> IcaoRunwayNumbers { get; set; } = [];
    }
}