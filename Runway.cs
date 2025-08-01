using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Represents a compass heading used as a special runway ID.
    /// </summary>
    /// <param name="Code">The unique code, a string from "37" to "52".</param>
    /// <param name="FullName">The full name, e.g., "Northwest-Southeast".</param>
    /// <param name="AbbrName">The abbreviated name, e.g., "NW-SE".</param>
    public record RunwayCompassId(string Code, string FullName, string AbbrName);

    /// <summary>
    /// Parameters for a runway sourced from the runways.xml file
    /// </summary>
    public class RunwayParams : ICloneable
    {
        /// <summary>
        /// Four letter code known as ICAO airport code or location indicator
        /// </summary>
        internal string IcaoId { get; set; }

        /// <summary>
        /// The name of the airport
        /// </summary>
        internal string IcaoName { get; set; }

        internal string Country { get; set; }

        internal string State { get; set; }

        internal string City { get; set; }

        /// <summary>
        /// The longitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLon { get; set; }

        /// <summary>
        /// The latitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLat { get; set; }

        /// <summary>
        /// Airport altitude (AMSL)
        /// </summary>
        internal double Altitude { get; set; }

        /// <summary>
        /// Airport magnetic variation
        /// </summary>
        internal double MagVar { get; set; }

        /// <summary>
        /// The runway Id e.g. "05L", the two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N". The number is extracted and stored as "Number" field.
        /// The letter which distinguishes parallel runways is extracted and stored as "Designator" field.
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N".
        /// </summary>
        internal string Number { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, one of "None", "Left", "Right", "Center", or "Water". Used in setting the airport landing trigger for a scenario
        /// </summary>
        internal string Designator { get; set; }

        /// <summary>
        /// Runway length in feet
        /// </summary>
        internal int Len { get; set; }

        /// <summary>
        /// Runway magnetic heading (add magVar for true)
        /// </summary>
        internal double Hdg { get; set; }

        /// <summary>
        /// Runway surface material
        /// </summary>
        internal string Def { get; set; }

        /// <summary>
        /// Runway threshold latitude
        /// </summary>
        internal double ThresholdStartLat { get; set; }

        /// <summary>
        /// Runway threshold longitude
        /// </summary>
        internal double ThresholdStartLon { get; set; }

        /// <summary>
        /// Index of runway in <see cref="Runway.Runways"></see>
        /// </summary>
        internal int RunwaysIndex { get; set; }

        /// <summary>
        /// Clones the airport level runway information prior to reading in each runway for the current airport
        /// </summary>
        /// <returns>Cloned version of <see cref="RunwayParams"/></returns>
        public object Clone()
        {
            var clonedRunwayParams = new RunwayParams
            {
                IcaoId = IcaoId,
                IcaoName = IcaoName,
                Country = Country,
                State = State,
                City = City,
                AirportLon = AirportLon,
                AirportLat = AirportLat,
                Altitude = Altitude,
                MagVar = MagVar,
                RunwaysIndex = RunwaysIndex
            };
            return clonedRunwayParams;
        }
    }

    /// <summary>
    /// A static utility class that provides a lookup map for the predefined runway compass IDs.
    /// </summary>
    internal static class RunwayCompassMap
    {
        private static readonly Dictionary<string, RunwayCompassId> _runwayCompassIds = new()
        {
            { "37", new("37", "North-South", "N-S") },
            { "38", new("38", "East-West", "E-W") },
            { "39", new("39", "Northwest-Southeast", "NW-SE") },
            { "40", new("40", "Southwest-Northeast", "SW-NE") },
            { "41", new("41", "South-North", "S-N") },
            { "42", new("42", "West-East", "W-E") },
            { "43", new("43", "Southeast-Northwest", "SE-NW") },
            { "44", new("44", "Northeast-Southwest", "NE-SW") },
            { "45", new("45", "North", "N") },
            { "46", new("46", "West", "W") },
            { "47", new("47", "Northwest", "NW") },
            { "48", new("48", "Southwest", "SW") },
            { "49", new("49", "South", "S") },
            { "50", new("50", "East", "E") },
            { "51", new("51", "Southeast", "SE") },
            { "52", new("52", "Northeast", "NE") },
        };

        /// <summary>
        /// Attempts to retrieve a RunwayCompassId from the map based on its code.
        /// </summary>
        /// <param name="code">The runway code to look up (e.g., "37").</param>
        /// <param name="compassId">When this method returns, contains the RunwayCompassId if the lookup was successful; otherwise, null.</param>
        /// <returns><c>true</c> if the compass ID was found; otherwise, <c>false</c>.</returns>
        public static bool TryGetCompassId(string code, out RunwayCompassId compassId)
        {
            return _runwayCompassIds.TryGetValue(code, out compassId);
        }
    }

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

    internal class Runway
    {
        /// <summary>
        /// The list of runways loaded from "runways.xml" file on application startup
        /// </summary>
        internal static List<RunwayParams> Runways { get; set; }

        /// <summary>
        /// The list of runways loaded from "runways.xml" file on application startup filtered by current 
        /// Country/State/Location strings on General tab
        /// </summary>
        internal static List<RunwayParams> RunwaysSubset { get; set; }

        /// <summary>
        /// The scenario start runway
        /// </summary>
        internal static RunwayParams startRwy = new();

        /// <summary>
        /// The scenario destination runway
        /// </summary>
        internal static RunwayParams destRwy = new();

        /// <summary>
        /// User created location favourites built from combinations of Country/State/City strings in "runways.xml" file
        /// </summary>
        internal static List<LocationFavourite> LocationFavourites = [];

        /// <summary>
        /// Currently selected location favourite displayed on form
        /// </summary>
        internal static int CurrentLocationFavouriteIndex;

        #region Load runways from "runways.xml" and build list for General tab region

        /// <summary>
        /// Asynchronously loads all runway data by offloading the synchronous parsing to a background thread.
        /// </summary>
        /// <param name="progressReporter">An object that can be used to report progress updates during the loading process.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task`1"/> representing the asynchronous operation, with a result of <c>true</c> if the runway data was loaded successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method uses <see cref="T:System.Threading.Tasks.Task.Run"/> to execute the CPU-intensive <c>GetRunways</c> method on a thread pool thread, ensuring the calling thread (e.g., the UI thread) remains responsive.
        /// </remarks>
        internal static async Task<bool> GetRunwaysAsync(IProgress<string> progressReporter)
        {
            // Task.Run offloads the synchronous work to a background thread.
            return await Task.Run(() => GetRunways(progressReporter));
        }

        /// <summary>
        /// Synchronously loads all runway data from an XML stream.
        /// </summary>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns>
        /// <c>true</c> if the runway data was loaded successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is a long-running, CPU-intensive operation that should not be called directly on the UI thread. It is designed to be executed in a background thread via <see cref="T:System.Threading.Tasks.Task.Run"/>. It reads runway data from an XML stream, populates a static collection, and reports progress.
        /// </remarks>
        internal static bool GetRunways(IProgress<string> progressReporter)
        {
            Runways = [];

            if (!TryGetRunwayXMLStream(out Stream stream, progressReporter))
            {
                return false;
            }

            try
            {
                using XmlReader reader = XmlReader.Create(stream);

                int curIndex = 0;
                while (reader.ReadToFollowing("ICAO"))
                {
                    RunwayParams curAirport = ReadAirport(reader);

                    progressReporter?.Report($"Loading runway data for airport: {curAirport.IcaoId}");

                    if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                    {
                        // The reader is positioned on the first runway.
                        do
                        {
                            RunwayParams newRunway = ReadRunway(reader, curAirport);
                            newRunway.RunwaysIndex = curIndex++;
                            Runways.Add(newRunway);
                            // Now, move to the next sibling named "Runway".
                        } while (reader.ReadToNextSibling("Runway"));
                    }
                }

                Log.Info($"Runways.GetRunways: Successfully loaded {Runways.Count} runways.");
                progressReporter?.Report($"Successfully loaded {Runways.Count} runways.");
                return true;
            }
            catch (XmlException ex)
            {
                Log.Error($"Runways.GetRunways: XML parsing error. {ex.Message}");
                progressReporter?.Report($"Error loading runway data: XML format is invalid.");
                return false;
            }
        }

        /// <summary>
        /// Helper method to read a single airport's general information from an XML reader.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> positioned at the "ICAO" element.</param>
        /// <returns>A <see cref="T:RunwayParams"/> object containing the airport's general data.</returns>
        /// <remarks>
        /// This method reads elements such as ICAOName, Country, State, City, and geographical coordinates. It returns when it encounters the first "Runway" element or the end of the "ICAO" element.
        /// </remarks>
        private static RunwayParams ReadAirport(XmlReader reader)
        {
            RunwayParams curAirport = new()
            {
                IcaoId = reader.GetAttribute("id")
            };

            if (reader.ReadToDescendant("ICAOName"))
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "ICAOName":
                                curAirport.IcaoName = reader.ReadElementContentAsString();
                                break;
                            case "Country":
                                curAirport.Country = reader.ReadElementContentAsString();
                                break;
                            case "State":
                                curAirport.State = reader.ReadElementContentAsString();
                                break;
                            case "City":
                                curAirport.City = reader.ReadElementContentAsString();
                                break;
                            case "Longitude":
                                curAirport.AirportLon = reader.ReadElementContentAsDouble();
                                break;
                            case "Latitude":
                                curAirport.AirportLat = reader.ReadElementContentAsDouble();
                                break;
                            case "Altitude":
                                curAirport.Altitude = reader.ReadElementContentAsDouble();
                                break;
                            case "MagVar":
                                curAirport.MagVar = reader.ReadElementContentAsDouble();
                                break;
                            case "Runway":
                                return curAirport;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                    // Advance the reader to the next node.
                    reader.Read();
                }
            }

            return curAirport;
        }

        /// <summary>
        /// Helper method to read a single runway's specific information from an XML reader.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> positioned at a "Runway" element.</param>
        /// <param name="airportData">The pre-existing <see cref="T:RunwayParams"/> object containing the parent airport's data.</param>
        /// <returns>A new <see cref="T:RunwayParams"/> object containing the combined airport and runway data.</returns>
        /// <remarks>
        /// This method clones the parent airport's data and then reads specific runway elements like length, heading, and threshold coordinates to create a new, distinct runway entry.
        /// </remarks>
        private static RunwayParams ReadRunway(XmlReader reader, RunwayParams airportData)
        {
            RunwayParams newRunway = (RunwayParams)airportData.Clone();
            SetRunwayId(newRunway, reader.GetAttribute("id"));

            if (reader.ReadToDescendant("Len"))
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Len":
                                newRunway.Len = reader.ReadElementContentAsInt();
                                break;
                            case "Hdg":
                                newRunway.Hdg = reader.ReadElementContentAsDouble();
                                break;
                            case "Def":
                                newRunway.Def = reader.ReadElementContentAsString();
                                break;
                            case "Lat":
                                newRunway.ThresholdStartLat = reader.ReadElementContentAsDouble();
                                break;
                            case "Lon":
                                newRunway.ThresholdStartLon = reader.ReadElementContentAsDouble();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                    // Advance the reader to the next node.
                    reader.Read();
                }
            }

            return newRunway;
        }

        /// <summary>
        /// Retrieves the XML stream for runway data. It first attempts to load "runways.xml" from the local application directory. 
        /// If not found, it falls back to loading "XML.runways.xml" from the embedded resources.
        /// </summary>
        /// <param name="stream">When this method returns, contains the loaded Stream if successful; otherwise, <see langword="null"/>.</param>
        /// <param name="progressReporter">Optional. Can be <see langword="null"/> if progress or error reporting to the UI is not required.</param>
        /// <returns><see langword="true"/> if the runway XML stream is successfully obtained; otherwise, <see langword="false"/>.</returns>
        static internal bool TryGetRunwayXMLStream(out Stream stream, IProgress<string> progressReporter)
        {
            string xmlFilename = "runways.xml"; // Local file name
            string embeddedResourceName = $"XML.{xmlFilename}"; // Embedded resource name

            string message = $"Runway.TryGetRunwayXMLStream: Attempting to retrieve runway XML stream for '{xmlFilename}' from local file.";
            Log.Info(message);
            progressReporter?.Report(message);

            // First, try to load from a local file
            string localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFilename); // Get path in application directory

            if (File.Exists(localFilePath))
            {
                Log.Info($"Runway.TryGetRunwayXMLStream: Local runway XML file found: '{localFilePath}'. Attempting to load.");
                if (FileOps.TryReadAllBytes(localFilePath, progressReporter, out byte[] fileBytes))
                {
                    stream = new MemoryStream(fileBytes);
                    Log.Info($"Runway.TryGetRunwayXMLStream: Successfully loaded runway XML from local file: '{localFilePath}'.");
                    return true;
                }
                else
                {
                    // Error already logged by TryReadAllBytes
                    Log.Error($"Runway.TryGetRunwayXMLStream: Failed to read local runway XML file: '{localFilePath}'. Falling back to embedded resource.");
                    // No progressReporter.Report here as TryReadAllBytes already did.
                    // Proceed to try embedded resource.
                }
            }
            else
            {
                Log.Info($"Runway.TryGetRunwayXMLStream: Local runway XML file not found at '{localFilePath}'. Attempting to load from embedded resource.");
            }

            // If local file doesn't exist or failed to load, fall back to embedded resource
            if (FileOps.TryGetResourceStream(embeddedResourceName, progressReporter, out stream))
            {
                Log.Info($"Runway.TryGetRunwayXMLStream: Successfully loaded runway XML from embedded resource: '{embeddedResourceName}'.");
                return true;
            }
            else
            {
                // Error already logged by FileOps.TryGetResourceStream
                message = $"Runway.TryGetRunwayXMLStream: Failed to load runway XML from embedded resource: '{embeddedResourceName}'. Runway data is unavailable.";
                Log.Error(message);
                progressReporter?.Report(message);
                return false;
            }
        }

        /// <summary>
        /// Takes a runway Id and extracts the alphabetic code letter if present and the runway number which is sometimes
        /// a string. E.g. "23L" is "Left" plus "23", "45" is "North", "32W" is "Water" plus "32"
        /// </summary>
        /// <param name="rwyParams">Where the runway Id, designator and number are stored</param>
        /// <param name="runwayId">The runway Id string to be processed</param>
        static private void SetRunwayId(RunwayParams rwyParams, string runwayId)
        {
            rwyParams.Id = runwayId;

            rwyParams.Designator = "None";
            if (runwayId.EndsWith('L'))
            {
                rwyParams.Designator = "Left";
                runwayId = runwayId.TrimEnd('L');
            }
            else if (runwayId.EndsWith('R'))
            {
                rwyParams.Designator = "Right";
                runwayId = runwayId.TrimEnd('R');
            }
            else if (runwayId.EndsWith('C'))
            {
                rwyParams.Designator = "Center";
                runwayId = runwayId.TrimEnd('C');
            }
            else if (runwayId.EndsWith('W'))
            {
                rwyParams.Designator = "Water";
                runwayId = runwayId.TrimEnd('W');
            }
            if (int.TryParse(runwayId, out int runwayNumber))
                if (runwayNumber <= 36)
                {
                    rwyParams.Number = runwayId.TrimStart('0');
                }
                else if (runwayNumber <= 52)
                {
                    if (RunwayCompassMap.TryGetCompassId(runwayId, out RunwayCompassId compassId))
                    {
                        rwyParams.Number = compassId.AbbrName;
                    }
                }
        }

        /// <summary>
        /// Builds list of strings in format "Airport ICAO (Runway Id)" e.g. "LFGO (14L)" used to populate
        /// available runway list in General tab of application form.
        /// </summary>
        /// <returns>The list of runway strings</returns>
        static internal List<string> GetICAOids()
        {
            List<string> icaoIDs = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (int.TryParse(Runways[i].Number, out int runwayNumber) && runwayNumber <= 36)
                {
                    icaoIDs.Add($"{Runways[i].IcaoId} ({Runways[i].Id})");
                }
                else
                {
                    string runwayId = $"{Runways[i].IcaoId} ({Runways[i].Number})";
                    if (Runways[i].Designator != null && Runways[i].Designator != "None")
                    {
                        // Append designator if not "None"
                        runwayId = $"{runwayId}[{Runways[i].Designator[0..1]}]";
                    }
                    icaoIDs.Add(runwayId);
                }
            }
            return icaoIDs;
        }

        /// <summary>
        /// Get a sorted list of the country strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the country strings in "runways.xml"</returns>
        static internal List<string> GetRunwayCountries()
        {
            List<string> countries = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].Country != "" && countries.IndexOf(Runways[i].Country) == -1)
                {
                    countries.Add(Runways[i].Country);
                }
            }
            countries.Sort();
            countries.Insert(0, "None");
            return countries;
        }

        /// <summary>
        /// Get a sorted list of the state strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the state strings in "runways.xml"</returns>
        static internal List<string> GetRunwayStates()
        {
            List<string> states = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].State != null && Runways[i].State != "" && states.IndexOf(Runways[i].State) == -1)
                {
                    states.Add(Runways[i].State);
                }
            }
            states.Sort();
            states.Insert(0, "None");
            return states;
        }

        /// <summary>
        /// Get a sorted list of the city strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the city strings in "runways.xml"</returns>
        static internal List<string> GetRunwayCities()
        {
            List<string> cities = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].City != "" && cities.IndexOf(Runways[i].City) == -1)
                {
                    cities.Add(Runways[i].City);
                }
            }
            cities.Sort();
            cities.Insert(0, "None");
            return cities;
        }

        #endregion

        #region Manage Location favourites region

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddFilterValueToLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                // Clear existing filter strings and add "None"
                ClearLocationFavouriteList(locationType);
                AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                // Add filter string
                AddToLocationFavouriteList(locationType, locationValue);
                // Handle case where filter list was "None"
                DeleteFromLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddToLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Countries = [.. LocationFavourites[CurrentLocationFavouriteIndex].Countries.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Sort();
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].States = [.. LocationFavourites[CurrentLocationFavouriteIndex].States.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].States.Sort();
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Cities = [.. LocationFavourites[CurrentLocationFavouriteIndex].Cities.Distinct()];
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Sort();
            }
        }

        /// <summary>
        /// Clears filter string list for one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which Country/State/City filter string list to clear </param>
        static internal void ClearLocationFavouriteList(string locationType)
        {
            if (locationType == "Country")
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Clear();
            else if (locationType == "State")
                LocationFavourites[CurrentLocationFavouriteIndex].States.Clear();
            else
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Clear();
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFromLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Countries.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].States.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Cities.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        ///  Gets a filter string to display in the Country/State/City fields on the General tab of form.
        ///  The current favourite may include more than one filter value for one or more of these fields.
        ///  Displays the first of the filter values as the list is maintained sorted.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City fields the display filter value is for</param>
        /// <returns>The Country/State/City field display filter value</returns>
        static internal string GetLocationFavouriteDisplayFilterValue(string locationType)
        {
            if (locationType == "Country")
                return LocationFavourites[CurrentLocationFavouriteIndex].Countries[0];
            else if (locationType == "State")
                return LocationFavourites[CurrentLocationFavouriteIndex].States[0];
            else
                return LocationFavourites[CurrentLocationFavouriteIndex].Cities[0];
        }

        /// <summary>
        /// Get a sorted list of the location favourite names
        /// </summary>
        /// <returns>Sorted list of the location favourite names</returns>
        static internal List<string> GetLocationFavouriteNames()
        {
            List<string> locationFavouriteNames = [];

            for (int i = 0; i < LocationFavourites.Count; i++)
            {
                locationFavouriteNames.Add(LocationFavourites[i].Name);
            }
            locationFavouriteNames.Sort();
            return locationFavouriteNames;
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFilterValueFromLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                return;
            }
            else
            {
                DeleteFromLocationFavouriteList(locationType, locationValue);
            }
        }

        /// <summary>
        /// Combines the Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <returns>Country/State/City location filters combined into a single string</returns>
        static internal string SetTextBoxGeneralLocationFilters()
        {
            string filters;

            filters = "Countries = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Countries);
            filters += "\" \nStates = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].States);
            filters += "\" \nCities = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Cities);
            filters += "\"";
            return filters;
        }

        /// <summary>
        /// Updates current location favourite with new name if that name has not already been used
        /// </summary>
        /// <param name="newLocationFavouriteName">The new location favourite name</param>
        /// <returns>The replaced current location favourite name</returns>
        static internal string UpdateLocationFavouriteName(string newLocationFavouriteName)
        {
            // Save name of current location favourite before changing
            string oldLocationFavouriteName = LocationFavourites[CurrentLocationFavouriteIndex].Name;

            // Make sure new name is not already in use and then change in current location favourite
            if (LocationFavourites.FindAll(favourite => favourite.Name == newLocationFavouriteName).Count == 0)
                LocationFavourites[CurrentLocationFavouriteIndex].Name = newLocationFavouriteName;

            // Return old favourite name so a new location favourite of that name can be created
            return oldLocationFavouriteName;
        }

        /// <summary>
        /// Add a location favourite to end of <see cref="LocationFavourites"/>, no need to
        /// adjust <see cref="CurrentLocationFavouriteIndex"/> as it is unaffected by adding to
        /// end of the list unless it's the first favourite to be added in which case set
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero.
        /// </summary>
        /// <param name="name">The name for the new <see cref="LocationFavourite"/> to be added</param>
        static internal void AddLocationFavourite(string name)
        {
            // Create a deep copy using the extension method
            LocationFavourite deepCopyFav = new(LocationFavourites[CurrentLocationFavouriteIndex])
            {
                Name = name
            };
            LocationFavourites.Add(deepCopyFav);
            if (LocationFavourites.Count == 1)
                CurrentLocationFavouriteIndex = 0;
        }

        /// <summary>
        /// Delete a location favourite from <see cref="LocationFavourites"/> and adjust 
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero
        /// </summary>
        /// <param name="deleteLocationFavouriteName">The name of the  <see cref="LocationFavourite"/> to be deleted</param>
        /// <returns>The name of the zero index <see cref="LocationFavourites"/> location favourite</returns>
        static internal string DeleteLocationFavourite(string deleteLocationFavouriteName)
        {
            if (LocationFavourites.Count > 1)
            {
                LocationFavourite deleteLocationFavourite = LocationFavourites.Find(favourite => favourite.Name == deleteLocationFavouriteName);
                if (deleteLocationFavourite != null)
                {
                    LocationFavourites.Remove(deleteLocationFavourite);
                    CurrentLocationFavouriteIndex = 0;
                }
            }
            return LocationFavourites[CurrentLocationFavouriteIndex].Name;
        }

        /// <summary>
        /// Combines one of Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <param name="locationFilterStrings">The list of location filter strings to be combined</param>
        /// <returns>One of Country/State/City location filters combined into a single string</returns>
        static private string SetTextBoxGeneralLocationFilter(List<string> locationFilterStrings)
        {
            string filters = "";

            foreach (string filterString in locationFilterStrings)
                filters += $"{filterString}, ";
            filters = filters.Trim();
            filters = filters.Trim(',');

            return filters;
        }

        /// <summary>
        /// Get the <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>
        /// </summary>
        /// <returns>The <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/></returns>
        static internal LocationFavourite GetCurrentLocationFavourite()
        {
            return LocationFavourites[CurrentLocationFavouriteIndex];
        }

        /// <summary>
        /// Reset <see cref="CurrentLocationFavouriteIndex"/> to the instance of <see cref="LocationFavourite"/>
        /// with newFavouriteName
        /// </summary>
        /// <param name="newFavouriteName">The name of the new instance to be set as <see cref="CurrentLocationFavouriteIndex"/></param>
        static internal void ChangeCurrentLocationFavouriteIndex(string newFavouriteName)
        {
            LocationFavourite locationFavourite = LocationFavourites.Find(favourite => favourite.Name == newFavouriteName);
            CurrentLocationFavouriteIndex = LocationFavourites.IndexOf(locationFavourite);
        }

        /// <summary>
        /// Save <see cref="LocationFavourites"/> to file in JSON format
        /// </summary>
        internal static void SaveLocationFavourites()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "LocationFavouritesJSON.txt");

            JsonSerializer serializer = new()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            using StreamWriter sw = new(directory);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, LocationFavourites);
        }

        /// <summary>
        /// Load <see cref="LocationFavourites"/> from file stored in JSON format
        /// </summary>
        internal static bool LoadLocationFavourites()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "LocationFavouritesJSON.txt");

            string input;
            if (!File.Exists(directory))
            {
                FileOps.TryGetResourceStream("Text.LocationFavouritesJSON.txt", null, out Stream stream);
                StreamReader reader = new(stream);
                input = reader.ReadToEnd();
            }
            else
            {
                input = File.ReadAllText(directory);
            }

            LocationFavourites = JsonConvert.DeserializeObject<List<LocationFavourite>>(input);

            return true;
        }

        /// <summary>
        /// Checks a location string (Country/State/City) against the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="location">The location string to be checked</param>
        /// <returns>True if the location string to be checked matches one of the Country/State/City filter values</returns>
        internal static bool CheckLocationFilters(string location)
        {
            // If Country/State/City filters all set to "None" return true as no filtering on location is required
            if (LocationFavourites[CurrentLocationFavouriteIndex].Countries[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].States[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].Cities[0] == "None")
            {
                return true;
            }

            // Check each of Country/State/City filter strings against location
            bool checkCountry = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].Countries);
            bool checkState = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].States);
            bool checkCity = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].Cities);
            if (checkCountry || checkState || checkCity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks a location string against one of Country/State/City in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="location">The location string to be checked</param>
        /// <param name="locationStrings">The strings to be checked against in <see cref="LocationFavourites"/></param>
        /// <returns>True if the location string to be checked matches one of the filter values in locationStrings</returns>
        internal static bool CheckLocationFilter(string location, List<string> locationStrings)
        {
            if (locationStrings[0] != "None")
            {
                return locationStrings.Contains(location);
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Search for a runway(s) that meets constraints region

        /// <summary>
        /// Selects and returns a random RunwayParams object from the predefined subset of runways.
        /// </summary>
        /// <returns>A randomly selected RunwayParams object.</returns>
        static internal RunwayParams GetRandomRunway()
        {
            Random random = new();
            int randomSubsetIndex = random.Next(0, Runway.RunwaysSubset.Count);
            return RunwaysSubset[randomSubsetIndex];
        }

        /// <summary>
        /// Search through the list of runways, starting from a random runway in list. Return the first runway
        /// located that is between a minimum and maximum distance from the provided reference coordinate
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <param name="minDist">The minimum distance runway can be from reference coordinate</param>
        /// <param name="maxDist">The maximum distance runway can be from reference coordinate</param>
        /// <returns>The runway that meets the minimum and maximum distance requirements</returns>
        static internal RunwayParams GetNearbyRunway(double queryLat, double queryLon, double minDist, double maxDist)
        {
            double curDistance;
            Random r = new();
            int curIndex = r.Next(0, RunwaysSubset.Count);
            for (int i = 0; i < Runways.Count; i++)
            {
                curDistance = MathRoutines.CalcDistance(RunwaysSubset[curIndex].AirportLat, RunwaysSubset[curIndex].AirportLon, queryLat, queryLon);
                if ((curDistance < maxDist) && (curDistance > minDist))
                {
                    return RunwaysSubset[curIndex];
                }
                curIndex++;
                if (curIndex == RunwaysSubset.Count)
                {
                    curIndex = 0;
                }
            }
            return null;
        }

        /// <summary>
        /// Search through the list of runways. Returns the nearest runway to the provided reference coordinate.
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <returns>The nearest runway to the provided reference cooordinate</returns>
        static internal RunwayParams GetNearestRunway(double queryLat, double queryLon)
        {
            RunwayParams minAirport = new();
            double minDistance = 9999;
            double curDistance;
            for (int curIndex = 0; curIndex < RunwaysSubset.Count; curIndex++)
            {
                curDistance = MathRoutines.CalcDistance(RunwaysSubset[curIndex].AirportLat, RunwaysSubset[curIndex].AirportLon, queryLat, queryLon);
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    minAirport = RunwaysSubset[curIndex];
                }
            }
            return minAirport;
        }

        /// <summary>
        /// Creates a subset of the <see cref="Runways"/> list of runways filtering on the Country/State/City
        /// location strings in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        static internal void SetRunwaysSubset()
        {
            // If Country/State/City filters all set to "None" just return full list of runways
            if (LocationFavourites[CurrentLocationFavouriteIndex].Countries[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].States[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].Cities[0] == "None")
            {
                RunwaysSubset = CloneRunwayParamsList(Runways);
                return;
            }

            // Get the subset of runways that match each of the Country/State/City filters and take the union of them - removes duplicates
            List<RunwayParams> runwaysCountrySubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].Countries, "Country");
            List<RunwayParams> runwaysStateSubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].States, "State");
            List<RunwayParams> runwaysCitySubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].Cities, "City");
            List<RunwayParams> runwaysSubset = [.. runwaysCountrySubset.Union(runwaysStateSubset)];
            runwaysSubset = [.. runwaysSubset.Union(runwaysCitySubset)];

            if (runwaysSubset.Count > 0)
            {
                RunwaysSubset = runwaysSubset;
            }
            else
            {
                MessageBox.Show("No runways match the location filters combination, using all runways instead", "Random Runway: Location filters",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                RunwaysSubset = CloneRunwayParamsList(Runways);
            }
        }

        /// <summary>
        /// Clones a list of <see cref="RunwayParams"/>
        /// </summary>
        /// <param name="sourceList">The list of <see cref="RunwayParams"/> to be cloned</param>
        /// <returns>The cloned list of <see cref="RunwayParams"/></returns>
        static internal List<RunwayParams> CloneRunwayParamsList(List<RunwayParams> sourceList)
        {
            List<RunwayParams> runwaysSubset = [];
            sourceList.ForEach((item) =>
            {
                runwaysSubset.Add((RunwayParams)item.Clone());
            });
            return runwaysSubset;
        }

        /// <summary>
        /// Gets the subset of runways from <see cref="Runways"/> filtered on one location string type
        /// in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationStrings">The collection of location strings</param>
        /// <param name="locationType">Which of Country/State/City filtering on</param>
        /// <returns>The subset of runways filtered from <see cref="Runways"/></returns>
        static internal List<RunwayParams> GetRunwaysLocationSubset(List<string> locationStrings, string locationType)
        {
            List<RunwayParams> runwaysSubset = [];
            if (locationStrings[0] != "None")
            {
                if (locationType == "Country")
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.Country));
                }
                else if (locationType == "State")
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.State));
                }
                else
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.City));
                }
            }
            return runwaysSubset;
        }

        #endregion region
    }
}