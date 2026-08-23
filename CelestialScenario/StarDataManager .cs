using OfficeOpenXml;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Services;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P3D_Scenario_Generator.CelestialScenario
{
    #region External DTO Records

    /// <summary>
    /// Represents star record data used to map internal <see cref="Star"/> objects to the JSON structure 
    /// expected by client-side JavaScript (<c>starCatalog</c> array of objects).
    /// </summary>
    public record StarData(
        string ConstellationName,
        string CatalogID,
        string ShaIndex,
        string NavName,
        string BayerDesignation,
        double RaH,
        double RaM,
        double RaS,
        double DecD,
        double DecM,
        double DecS,
        double VisualMagnitude
    );

    #endregion

    /// <summary>
    /// Manages the loading, storage, and retrieval of star data from embedded JSON resources.
    /// It populates a list of all catalog stars, identifies and organizes navigational stars,
    /// and provides methods to access individual star properties and constellation line vectors.
    /// </summary>
    public sealed class StarDataManager(Logger logger, FileOps fileOps, FormProgressReporter progressReporter)
    {
        #region Private Fields & Dependencies

        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Collection of all loaded celestial stars parsed from the embedded JSON star catalog.
        /// </summary>
        private readonly List<Star> _stars = [];

        /// <summary>
        /// List of recognized navigational star names, used in scenario HTML and JavaScript files.
        /// </summary>
        private readonly List<string> _navStarNames = [];

        /// <summary>
        /// Flattened list of star ID pairs ([Id1, Id2, Id3, Id4, ...]) used to draw constellation lines.
        /// </summary>
        private readonly List<string> _starLineConnections = [];

        /// <summary>
        /// Stores the total count of stars successfully loaded from the JSON catalog.
        /// </summary>
        private int _noStars = 0;

        #endregion

        #region Static Lookup Tables

        /// <summary>
        /// Lookup table mapping catalog line numbers to standard navigational star metadata.
        /// </summary>
        private static readonly Dictionary<string, (int StarNumber, string StarName, string Constellation)> NavStarLookup = new()
        {
            { "871", (7, "Acamar", "Eridanus") },
            { "472", (5, "Achernar", "Eridanus") },
            { "4730", (30, "Acrux", "Crux") },
            { "2618", (19, "Adhara", "Canis Major") },
            { "8425", (55, "Al Na'ir", "Grus") },
            { "1457", (10, "Aldebaran", "Taurus") },
            { "4905", (32, "Alioth", "Ursa Major") },
            { "5191", (34, "Alkaid", "Ursa Major") },
            { "1903", (15, "Alnilam", "Orion") },
            { "3748", (25, "Alphard", "Hydra") },
            { "5793", (41, "Alphecca", "Corona Borealis") },
            { "15", (1, "Alpheratz", "Andromeda") },
            { "7557", (51, "Altair", "Aquila") },
            { "99", (2, "Ankaa", "Phoenix") },
            { "6134", (42, "Antares", "Scorpius") },
            { "5340", (37, "Arcturus", "Bootes") },
            { "6217", (43, "Atria", "Triangulum Australe") },
            { "3307", (22, "Avior", "Carina") },
            { "1790", (13, "Bellatrix", "Orion") },
            { "2061", (16, "Betelgeuse", "Orion") },
            { "2326", (17, "Canopus", "Carina") },
            { "1708", (12, "Capella", "Auriga") },
            { "7924", (53, "Deneb", "Cygnus") },
            { "4534", (28, "Denebola", "Leo") },
            { "188", (4, "Diphda", "Cetus") },
            { "4301", (27, "Dubhe", "Ursa Major") },
            { "1791", (14, "Elnath", "Taurus") },
            { "6705", (47, "Eltanin", "Draco") },
            { "8308", (54, "Enif", "Pegasus") },
            { "8728", (56, "Fomalhaut", "Piscis Austrinus") },
            { "4763", (31, "Gacrux", "Crux") },
            { "4662", (29, "Gienah", "Corvus") },
            { "5267", (35, "Hadar", "Centaurus") },
            { "617", (6, "Hamal", "Aries") },
            { "6879", (48, "Kaus Australis", "Sagittarius") },
            { "5563", (40, "Kochab", "Ursa Minor") },
            { "8781", (57, "Markab", "Pegasus") },
            { "911", (8, "Menkar", "Cetus") },
            { "5288", (36, "Menkent", "Centaurus") },
            { "3685", (24, "Miaplacidus", "Carina") },
            { "1017", (9, "Mirfak", "Perseus") },
            { "7121", (50, "Nunki", "Sagittarius") },
            { "7790", (52, "Peacock", "Pavo") },
            { "424", (0, "Polaris", "Ursa Minor") }, // 0 used as Polaris is unnumbered in the standard 1–57 sequence
            { "2990", (21, "Pollux", "Gemini") },
            { "2943", (20, "Procyon", "Canis Minor") },
            { "6406", (44, "Sabik", "Ophiuchus") },
            { "3982", (26, "Regulus", "Leo") },
            { "1713", (11, "Rigel", "Orion") },
            { "5459", (38, "Rigil Kentaurus", "Centaurus") },
            { "6378", (44, "Sabik", "Ophiuchus") },
            { "168", (3, "Schedar", "Cassiopeia") },
            { "6527", (45, "Shaula", "Scorpius") },
            { "2491", (18, "Sirius", "Canis Major") },
            { "5056", (33, "Spica", "Virgo") },
            { "3634", (23, "Suhail", "Vela") },
            { "7001", (49, "Vega", "Lyra") },
            { "5531", (39, "Zubenelgenubi", "Libra") }
        };

        /// <summary>
        /// The 38 IAU constellation abbreviations that contain standard Navigational Stars.
        /// </summary>
        private static readonly HashSet<string> NavConstellationAbbreviations =
        [
            with(StringComparer.OrdinalIgnoreCase),
            "And", "Aql", "Ari", "Aur", "Boo", "CMa", "CMi", "Car", "Cas",
            "Cen", "Cet", "CrB", "Crv", "Cru", "Cyg", "Dra", "Eri", "Gem",
            "Gru", "Hya", "Leo", "Lib", "Lyr", "Oph", "Ori", "Pav", "Peg",
            "Per", "Phe", "PsA", "Sgr", "Sco", "Tau", "TrA", "UMa", "UMi",
            "Vel", "Vir"
        ];

        /// <summary>
        /// Maps the BSC5P 3-letter abbreviations to Unicode Greek characters.
        /// </summary>
        private static readonly Dictionary<string, string> GreekAlphabet = new(StringComparer.OrdinalIgnoreCase)
        {
            {"alp", "α"}, {"bet", "β"}, {"gam", "γ"}, {"del", "δ"}, {"eps", "ε"},
            {"zet", "ζ"}, {"eta", "η"}, {"the", "θ"}, {"iot", "ι"}, {"kap", "κ"},
            {"lam", "λ"}, {"mu", "μ"}, {"nu", "ν"}, {"xi", "ξ"}, {"omi", "ο"},
            {"pi", "π"}, {"rho", "ρ"}, {"sig", "σ"}, {"tau", "τ"}, {"ups", "υ"},
            {"phi", "φ"}, {"chi", "χ"}, {"psi", "ψ"}, {"ome", "ω"}
        };

        #endregion

        #region Properties

        /// <summary>
        /// A read-only list of all loaded stars.
        /// </summary>
        public IReadOnlyList<Star> Stars => _stars.AsReadOnly();

        /// <summary>
        /// A read-only list of navigational star names.
        /// </summary>
        public IReadOnlyList<string> NavStarNames => _navStarNames.AsReadOnly();

        /// <summary>
        /// The total number of stars loaded into memory.
        /// </summary>
        public int NoStars => _noStars;

        /// <summary>
        /// Gets a flattened list of star ID pairs ([Id1, ConnectedId1, Id2, ConnectedId2, ...]) 
        /// used to draw lines between stars in constellations.
        /// </summary>
        public IReadOnlyList<string> StarLineConnections => _starLineConnections.AsReadOnly();

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously initializes the celestial star catalog by loading and parsing the expanded 
        /// BSC5P JSON data from the application's embedded resources. It populates internal collections 
        /// of stars and navigational names, parses precise astronomical coordinates, maps standard 
        /// navigational stars, applies clean Greek Bayer designations, and triggers loading 
        /// of Stellarium constellation line connections.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the JSON catalog and constellation lines were successfully loaded 
        /// and parsed; otherwise, <see langword="false"/>.
        /// </returns>
        public async Task<bool> InitStarsAsync()
        {
            await _logger.InfoAsync("Starting initialization of star data from JSON catalog.");
            _progressReporter.Report("Initializing star data...");

            _stars.Clear();
            _navStarNames.Clear();
            _noStars = 0;

            string resourceName = "JSON.bsc5p_extra.json";

            (bool success, Stream stream) = await _fileOps.TryGetResourceStreamAsync(resourceName, _progressReporter);
            if (!success) return false;

            try
            {
                List<Bsc5pJsonStar> rawStars;
                using (stream)
                {
                    rawStars = await JsonSerializer.DeserializeAsync<List<Bsc5pJsonStar>>(stream);
                }

                // Keep track of Nav Stars we have already processed to prevent duplicate entries for binary components
                HashSet<string> processedNavStars = [];

                // Map Hipparcos ID (e.g., "424") to Harvard Revised catalog LineNumber (e.g., "1")
                var hipToHrMap = new Dictionary<string, string>();

                foreach (var rawStar in rawStars)
                {
                    // 1. Parse JSON coordinate properties
                    _ = double.TryParse(rawStar.RaH, NumberStyles.Any, CultureInfo.InvariantCulture, out double raH);
                    _ = double.TryParse(rawStar.RaM, NumberStyles.Any, CultureInfo.InvariantCulture, out double raM);
                    _ = double.TryParse(rawStar.RaS, NumberStyles.Any, CultureInfo.InvariantCulture, out double raS);

                    _ = double.TryParse(rawStar.DecD, NumberStyles.Any, CultureInfo.InvariantCulture, out double decD);
                    _ = double.TryParse(rawStar.DecM, NumberStyles.Any, CultureInfo.InvariantCulture, out double decM);
                    _ = double.TryParse(rawStar.DecS, NumberStyles.Any, CultureInfo.InvariantCulture, out double decS);

                    // Parse Visual Magnitude; default to 6.0 (dim) if catalog value is omitted
                    if (!double.TryParse(rawStar.VisualMagnitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double visMag))
                    {
                        visMag = 6.0;
                    }

                    if (rawStar.DecSign == "-") decD *= -1;

                    // 2. Identify Navigational Stars and their primary Constellations
                    string starName = "";
                    string constellationName = "";
                    string starNumber = "";

                    if (NavStarLookup.TryGetValue(rawStar.LineNumber, out var navData))
                    {
                        // Ensure we only process the primary component of a binary system
                        if (!processedNavStars.Contains(navData.StarName))
                        {
                            starName = navData.StarName;
                            constellationName = navData.Constellation;
                            starNumber = navData.StarNumber.ToString();

                            _navStarNames.Add(starName);
                            processedNavStars.Add(starName); // Mark as processed
                        }
                    }

                    // 3. Extract UI-friendly Greek letter (or empty string if not applicable)
                    string cleanBayer = GetCleanBayerDesignation(rawStar.Bayer);

                    // 4. Construct internal Star object
                    _stars.Add(new Star(
                        Constellation: constellationName,
                        Id: rawStar.LineNumber,
                        ConnectedId: "", // Line pairings are populated separately via LoadStellariumLinesAsync
                        StarNumber: starNumber,
                        StarName: starName,
                        WikiLink: "",
                        Bayer: cleanBayer,
                        RaH: raH, RaM: raM, RaS: raS,
                        DecD: decD, DecM: decM, DecS: decS,
                        VisMag: visMag
                    ));

                    _noStars++;

                    // 5. Index Hipparcos designation mapping for constellation line resolution
                    if (rawStar.NamesAlt != null)
                    {
                        string hipEntry = rawStar.NamesAlt.FirstOrDefault(n => n.StartsWith("HIP ", StringComparison.OrdinalIgnoreCase));
                        if (hipEntry != null)
                        {
                            string hipId = hipEntry.Replace("HIP ", "", StringComparison.OrdinalIgnoreCase).Trim();
                            hipToHrMap[hipId] = rawStar.LineNumber;
                        }
                    }
                }

                _navStarNames.Sort();

                // 6. Load Constellation Line vectors from Stellarium catalog
                //   await LoadConstellationLinesAsync();
                await LoadStellariumLinesAsync(hipToHrMap);

                await _logger.InfoAsync($"Successfully initialized {_noStars} stars from JSON.");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to parse JSON star catalog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the entire star catalog as a consolidated list of StarData records, 
        /// optimized for a single JSON serialization to client-side JavaScript.
        /// </summary>
        /// <returns>A read-only list of StarData records.</returns>
        public IReadOnlyList<StarData> GetStarCatalog()
        {
            // The mapping ensures property names match the client-side JavaScript StarData typedef
            return _stars.Select(s => new StarData(
                ConstellationName: s.Constellation,
                CatalogID: s.Id,
                ShaIndex: s.StarNumber,
                NavName: s.StarName,
                BayerDesignation: s.Bayer,
                RaH: s.RaH,
                RaM: s.RaM,
                RaS: s.RaS,
                DecD: s.DecD,
                DecM: s.DecM,
                DecS: s.DecS,
                VisualMagnitude: s.VisMag
            )).ToList().AsReadOnly();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Parses the raw Bayer string from the star catalog entry.
        /// If the star belongs to a navigational constellation, it extracts and returns just the base 
        /// Greek Unicode character (e.g., "α", "β") to maintain a clean, uncluttered UI.
        /// </summary>
        /// <param name="rawBayer">The raw Bayer string from the star catalog entry.</param>
        /// <returns>The cleaned Greek character string, or an empty string if omitted or not applicable.</returns>
        private static string GetCleanBayerDesignation(string rawBayer)
        {
            if (string.IsNullOrWhiteSpace(rawBayer) || rawBayer.Length < 3) return "";

            string constAbbrev = rawBayer[^3..].Trim();

            if (!NavConstellationAbbreviations.Contains(constAbbrev)) return "";

            string leftPart = rawBayer[..^3].Trim().ToLowerInvariant();

            foreach (var kvp in GreekAlphabet)
            {
                if (leftPart.Contains(kvp.Key))
                {
                    // Return ONLY the base Greek letter (no Flamsteed numbers or superscripts)
                    return kvp.Value;
                }
            }

            return "";
        }

        /// <summary>
        /// Asynchronously loads and parses Stellarium constellation line definitions from JSON resources, 
        /// mapping Hipparcos (HIP) vertex IDs back to internal catalog LineNumber (HR) IDs.
        /// </summary>
        /// <param name="hipToHrMap">A dictionary mapping Hipparcos IDs to catalog LineNumber (HR) IDs.</param>
        /// <returns><see langword="true"/> if loaded and mapped successfully; otherwise, <see langword="false"/>.</returns>
        private async Task<bool> LoadStellariumLinesAsync(Dictionary<string, string> hipToHrMap)
        {
            // 1. Read Stellarium JSON stream
            (bool success, Stream stream) = await _fileOps.TryGetResourceStreamAsync("JSON.stellarium_modern_iau_lines.json", _progressReporter);
            if (!success) return false;

            using (stream)
            {
                var stellariumData = await JsonSerializer.DeserializeAsync<StellariumData>(stream);
                if (stellariumData?.Constellations == null) return false;

                foreach (var constel in stellariumData.Constellations)
                {
                    // Extract IAU 3-letter abbreviation from "CON modern_iau And" -> "And"
                    string abbrev = constel.Id.Split(' ').LastOrDefault() ?? "";

                    // Filter to Navigational Constellations only
                    if (!NavConstellationAbbreviations.Contains(abbrev)) continue;

                    foreach (var stroke in constel.Lines)
                    {
                        for (int i = 0; i < stroke.Count - 1; i++)
                        {
                            string hip1 = stroke[i].ToString();
                            string hip2 = stroke[i + 1].ToString();

                            // Map HIP IDs back to catalog LineNumbers (HR IDs)
                            if (hipToHrMap.TryGetValue(hip1, out string hr1) &&
                                hipToHrMap.TryGetValue(hip2, out string hr2))
                            {
                                _starLineConnections.Add(hr1);
                                _starLineConnections.Add(hr2);
                            }
                        }
                    }
                }
            }

            return true;
        }

        #endregion

        #region Internal DTO Records

        /// <summary>
        /// DTO representing the root container for Stellarium skyculture JSON data.
        /// </summary>
        public record StellariumData(
            [property: JsonPropertyName("constellations")] List<StellariumConstellation> Constellations
        );

        /// <summary>
        /// DTO representing individual constellation line definitions in Stellarium JSON format.
        /// </summary>
        public record StellariumConstellation(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("lines")] List<List<int>> Lines,
            [property: JsonPropertyName("common_name")] CommonName CommonName
        );

        /// <summary>
        /// DTO representing common naming attributes for Stellarium constellations.
        /// </summary>
        public record CommonName(
            [property: JsonPropertyName("english")] string English,
            [property: JsonPropertyName("native")] string Native
        );

        #endregion
    }
}