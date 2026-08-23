using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace P3D_Scenario_Generator.CelestialScenario
{
    // This represents the raw data coming from bsc5p_extra.json
    public class Bsc5pJsonStar
    {
        [JsonPropertyName("lineNumber")]
        public string LineNumber { get; set; } // This is the HR ID

        [JsonPropertyName("visualMagnitude")]
        public string VisualMagnitude { get; set; }

        [JsonPropertyName("bayerAndOrFlamsteed")]
        public string Bayer { get; set; }

        [JsonPropertyName("hoursRaJ2000")]
        public string RaH { get; set; }

        [JsonPropertyName("minutesRaJ2000")]
        public string RaM { get; set; }

        [JsonPropertyName("secondsRaJ2000")]
        public string RaS { get; set; }

        [JsonPropertyName("signDecJ2000")]
        public string DecSign { get; set; } // "+" or "-"

        [JsonPropertyName("degreesDecJ2000")]
        public string DecD { get; set; }

        [JsonPropertyName("minutesDecJ2000")]
        public string DecM { get; set; }

        [JsonPropertyName("secondsDecJ2000")]
        public string DecS { get; set; }

        [JsonPropertyName("namesAlt")]
        public List<string> NamesAlt { get; set; }
    }
}