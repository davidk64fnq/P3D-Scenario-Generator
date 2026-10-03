using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace P3D_Scenario_Generator.Models
{
    /// <summary>
    /// Represents a geo-located Wikipedia landmark or point of interest.
    /// </summary>
    internal class WikiPoi
    {
        [JsonPropertyName("pageid")]
        public long PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("dist")]
        public double DistanceMeters { get; set; }

        /// <summary>
        /// Distance from the query coordinate in Nautical Miles.
        /// </summary>
        [JsonIgnore]
        public double DistanceNM => DistanceMeters * 0.000539957;

        /// <summary>
        /// Plain-text summary extract of the article introduction.
        /// </summary>
        public string Extract { get; set; } = string.Empty;

        /// <summary>
        /// Direct URL to the article's lead thumbnail image, if available.
        /// </summary>
        public string? ThumbnailUrl { get; set; }
    }

    /// <summary>
    /// Root deserialization envelope for MediaWiki API responses.
    /// </summary>
    internal class WikiApiResponse
    {
        [JsonPropertyName("query")]
        public WikiQueryData? Query { get; set; }
    }

    internal class WikiQueryData
    {
        [JsonPropertyName("geosearch")]
        public List<WikiPoi>? GeoSearch { get; set; }

        [JsonPropertyName("pages")]
        public Dictionary<string, WikiPageDetail>? Pages { get; set; }
    }

    internal class WikiPageDetail
    {
        [JsonPropertyName("pageid")]
        public long PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("extract")]
        public string? Extract { get; set; }

        [JsonPropertyName("thumbnail")]
        public WikiThumbnailInfo? Thumbnail { get; set; }
    }

    internal class WikiThumbnailInfo
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
