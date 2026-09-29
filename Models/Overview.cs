namespace P3D_Scenario_Generator.Models
{
    /// <summary>
    /// Stores the descriptive metadata used to populate HTML scenario briefing files.
    /// </summary>
    internal class Overview
    {
        internal string Title { get; set; } = string.Empty;
        internal string Heading1 { get; set; } = string.Empty;
        internal string Location { get; set; } = string.Empty;
        internal string Difficulty { get; set; } = string.Empty;
        internal string Duration { get; set; } = string.Empty;
        internal string Aircraft { get; set; } = string.Empty;
        internal string Briefing { get; set; } = string.Empty;
        internal string Objective { get; set; } = string.Empty;
        internal string Tips { get; set; } = string.Empty;
    }
}