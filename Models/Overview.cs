namespace P3D_Scenario_Generator.Models
{
    /// <summary>Used to store information that populates HTML scenario files.</summary>
    public class Overview
    {
        public string Title { get; set; } = string.Empty;
        public string Heading1 { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Aircraft { get; set; } = string.Empty;
        public string Briefing { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string Tips { get; set; } = string.Empty;
    }
}