using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace P3D_Scenario_Generator.CelestialScenario
{
    // This represents the raw data coming from bsc5p_extra.json
    public class Bsc5pJsonStar
    {
        [JsonPropertyName("lineNumber")]
        public string LineNumber { get; set; } = string.Empty; // This is the HR ID

        [JsonPropertyName("visualMagnitude")]
        public string VisualMagnitude { get; set; } = string.Empty;

        [JsonPropertyName("bayerAndOrFlamsteed")]
        public string Bayer { get; set; } = string.Empty;

        [JsonPropertyName("hoursRaJ2000")]
        public string RaH { get; set; } = string.Empty;

        [JsonPropertyName("minutesRaJ2000")]
        public string RaM { get; set; } = string.Empty;

        [JsonPropertyName("secondsRaJ2000")]
        public string RaS { get; set; } = string.Empty;

        [JsonPropertyName("signDecJ2000")]
        public string DecSign { get; set; } = string.Empty; // "+" or "-"

        [JsonPropertyName("degreesDecJ2000")]
        public string DecD { get; set; } = string.Empty;

        [JsonPropertyName("minutesDecJ2000")]
        public string DecM { get; set; } = string.Empty;

        [JsonPropertyName("secondsDecJ2000")]
        public string DecS { get; set; } = string.Empty;

        [JsonPropertyName("namesAlt")]
        public List<string> NamesAlt { get; set; } = [];
    }
}