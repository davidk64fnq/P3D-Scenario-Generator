using P3D_Scenario_Generator.Interfaces;
using System.Xml;

namespace P3D_Scenario_Generator.Runways
{
    public class RunwayLoader(IFileOps fileOps, ICacheManager cacheManager, ILog log)
    {
        // Parameter validation for the primary constructor.
        private readonly IFileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly ICacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        private readonly ILog _log = log ?? throw new ArgumentNullException(nameof(log));

        /// <summary>
        /// Asynchronously loads runway data, first attempting to retrieve it from a binary cache.
        /// If the cache is unavailable or outdated, it falls back to parsing an XML file.
        /// </summary>
        /// <param name="progressReporter">The object for reporting progress and status updates to the UI.</param>
        /// <returns>A <see cref="RunwayData"/> object containing the loaded runway information; otherwise, <see langword="null"/> if the data could not be loaded.</returns>
        public Task<RunwayData> LoadRunwaysAsync(FormProgressReporter progressReporter)
        {
            // Use Task.Run for a CPU-bound operation, which avoids blocking the UI thread.
            return Task.Run(() =>
            {
                // Attempt to load from cache
                List<RunwayParams> runways;
                if (TryLoadFromCache(out runways, progressReporter))
                {
                    progressReporter.Report($"INFO: Successfully loaded {runways.Count} runways from cache.");
                }
                else
                {
                    // Fallback to XML
                    progressReporter.Report("INFO: Cache not found or invalid. Parsing XML...");
                    runways = LoadFromXml(progressReporter);

                    if (runways != null)
                    {
                        // Cache the data after a successful XML load
                        SaveToCache(runways, progressReporter);
                        progressReporter.Report($"INFO: Successfully loaded {runways.Count} runways from XML.");
                    }
                    else
                    {
                        progressReporter.Report("ERROR: Failed to load runway data from XML.");
                        return null; // Return null on failure
                    }
                }

                // Build the KD tree and return the complete data object
                KDNode treeRoot = BuildKDTree(runways);
                return new RunwayData { Runways = runways, RunwayTreeRoot = treeRoot };
            });
        }

        private bool TryLoadFromCache(out List<RunwayParams> runways, FormProgressReporter progressReporter)
        {
            runways = null;
            string dataDirectory = _fileOps.GetApplicationDataDirectory();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");
            string xmlFilePath = Path.Combine(dataDirectory, "runways.xml");

            bool isCacheOutOfDate = false;
            if (File.Exists(cacheFilePath) && File.Exists(xmlFilePath))
            {
                DateTime cacheLastModified = File.GetLastWriteTime(cacheFilePath);
                DateTime xmlLastModified = File.GetLastWriteTime(xmlFilePath);
                if (xmlLastModified > cacheLastModified)
                {
                    isCacheOutOfDate = true;
                    progressReporter.Report("NOTICE: Runways XML file is newer than the cache. Parsing XML...");
                }
            }

            if (File.Exists(cacheFilePath) && !isCacheOutOfDate)
            {
                progressReporter.Report("INFO: Loading runways from binary cache...");
                try
                {
                    runways = _cacheManager.DeserializeFromFile<List<RunwayParams>>(cacheFilePath);
                    return true;
                }
                catch (Exception ex)
                {
                    string message = $"Failed to load from cache: {ex.Message}. Falling back to XML.";
                    _log.Error(message);
                    progressReporter.Report($"ERROR: {message}");
                    return false;
                }
            }
            return false;
        }

        private void SaveToCache(List<RunwayParams> runways, FormProgressReporter progressReporter)
        {
            string dataDirectory = _fileOps.GetApplicationDataDirectory();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");
            _cacheManager.SerializeToFile(runways, cacheFilePath);
            const string message = "Runway data cached to binary file.";
            _log.Info(message);
            progressReporter.Report($"INFO: {message}");
        }

        private List<RunwayParams> LoadFromXml(FormProgressReporter progressReporter)
        {
            // Use a TryGet method that returns the list on success
            if (TryGetRunwayXMLData(out List<RunwayParams> runways, progressReporter))
            {
                return runways;
            }
            else
            {
                _log.Error("Runway data is unavailable after attempting to load from XML.");
                progressReporter?.Report("ERROR: Failed to load runway data from XML.");
                return null;
            }
        }

        private bool TryGetRunwayXMLData(out List<RunwayParams> runways, FormProgressReporter progressReporter)
        {
            runways = [];

            if (!TryGetRunwayXMLStream(out Stream stream, progressReporter))
            {
                return false;
            }

            try
            {
                using (stream)
                using (XmlReader reader = XmlReader.Create(stream))
                {

                    int curIndex = 0;
                    while (reader.ReadToFollowing("ICAO"))
                    {
                        RunwayParams curAirport = ReadAirport(reader);

                        progressReporter?.Report($"INFO: Loading runway data for airport: {curAirport.IcaoId}");

                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            // The reader is positioned on the first runway.
                            do
                            {
                                RunwayParams newRunway = ReadRunway(reader, curAirport);
                                newRunway.RunwaysIndex = curIndex++;
                                runways.Add(newRunway);
                                // Now, move to the next sibling named "Runway".
                            } while (reader.ReadToNextSibling("Runway"));
                        }
                    }

                    string message = $"Successfully loaded {runways.Count} runways.";
                    _log.Info(message);
                    progressReporter?.Report($"INFO: {message}");

                    return true;
                }
            }
            catch (XmlException ex)
            {
                _log.Error($"XML parsing error. {ex.Message}");
                progressReporter?.Report($"ERROR: Error loading runway data: XML format is invalid.");
                return false;
            }
        }

        private bool TryGetRunwayXMLStream(out Stream stream, FormProgressReporter progressReporter)
        {
            const string xmlFilename = "runways.xml"; // Local file name
            const string embeddedResourceName = $"XML.{xmlFilename}"; // Embedded resource name

            string dataDirectory = _fileOps.GetApplicationDataDirectory();
            string localFilePath = Path.Combine(dataDirectory, xmlFilename);

            string message = $"Attempting to retrieve runway XML stream for '{xmlFilename}' from local file.";
            _log.Info(message);
            progressReporter?.Report($"INFO: {message}");

            if (File.Exists(localFilePath))
            {
                _log.Info($"Local runway XML file found: '{localFilePath}'. Attempting to load.");
                if (FileOps.TryReadAllBytes(localFilePath, progressReporter, out byte[] fileBytes))
                {
                    stream = new MemoryStream(fileBytes);
                    _log.Info($"Successfully loaded runway XML from local file: '{localFilePath}'.");
                    progressReporter?.Report($"INFO: Successfully loaded runway XML.");
                    return true;
                }
                else
                {
                    _log.Error($"Failed to read local runway XML file: '{localFilePath}'. Falling back to embedded resource.");
                }
            }
            else
            {
                _log.Info($"Local runway XML file not found at '{localFilePath}'. Attempting to load from embedded resource.");
                progressReporter?.Report("INFO: Local runways XML not found. Attempting to load from embedded resource.");
            }

            if (FileOps.TryGetResourceStream(embeddedResourceName, progressReporter, out stream))
            {
                _log.Info($"Successfully loaded runway XML from embedded resource: '{embeddedResourceName}'.");
                progressReporter?.Report("INFO: Successfully loaded runway XML from embedded resource.");
                return true;
            }
            else
            {
                message = $"Failed to load runway XML from embedded resource: '{embeddedResourceName}'. Runway data is unavailable.";
                _log.Error(message);
                progressReporter?.Report($"ERROR: {message}");
                return false;
            }
        }

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

        private static void SetRunwayId(RunwayParams rwyParams, string runwayId)
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

        private KDNode BuildKDTree(List<RunwayParams> runways)
        {
            // ... Your KD-tree building logic here, returning the root node.
            return new KDNode(); // Placeholder
        }
    }
}