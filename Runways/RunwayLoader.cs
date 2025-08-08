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
        /// Asynchronously loads runway data, first attempting to retrieve it and the KD-tree from a binary cache.
        /// If the cache is unavailable or outdated, it falls back to parsing an XML file and then builds and caches the KD-tree.
        /// </summary>
        /// <param name="progressReporter">The object for reporting progress and status updates to the UI.</param>
        /// <returns>A <see cref="RunwayData"/> object containing the loaded runway information; otherwise, <see langword="null"/> if the data could not be loaded.</returns>
        public async Task<RunwayData> LoadRunwaysAsync(FormProgressReporter progressReporter)
        {
            // Attempt to load the entire RunwayData object from cache first.
            RunwayData runwayData = await TryLoadFromCacheAsync(progressReporter);

            if (runwayData != null)
            {
                progressReporter.Report($"INFO: Successfully loaded {runwayData.Runways.Count} runways from cache.");
                // No need to build the KD-tree; it was deserialized with the data.
                return runwayData;
            }
            else
            {
                // Fallback to XML
                progressReporter.Report("INFO: Cache not found or invalid. Parsing XML...");
                List<RunwayParams> runways = await LoadFromXmlAsync(progressReporter);

                if (runways != null)
                {
                    progressReporter.Report("INFO: Building KD-tree for spatial indexing...");
                    KDNode treeRoot = await Task.Run(() => BuildKDTree(runways, progressReporter));

                    // Create the full RunwayData object to be cached.
                    runwayData = new RunwayData { Runways = runways, RunwayTreeRoot = treeRoot };
                    await SaveToCacheAsync(runwayData, progressReporter);
                    progressReporter.Report($"INFO: Successfully loaded {runways.Count} runways from XML and cached.");
                    return runwayData;
                }
                else
                {
                    progressReporter.Report("ERROR: Failed to load runway data from XML.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Asynchronously loads runway data from an XML source, either a local file or an embedded resource.
        /// </summary>
        /// <param name="progressReporter">The progress reporter for UI updates.</param>
        /// <returns>A list of <see cref="RunwayParams"/> if successful; otherwise, <see langword="null"/>.</returns>
        private async Task<List<RunwayParams>> LoadFromXmlAsync(FormProgressReporter progressReporter)
        {
            // Await the asynchronous TryGet method and deconstruct the returned tuple.
            var (success, runways) = await TryGetRunwayXMLDataAsync(progressReporter);

            if (success)
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

        /// <summary>
        /// Asynchronously attempts to get a list of runways by parsing XML data.
        /// </summary>
        /// <param name="progressReporter">The object for reporting progress and status updates to the UI.</param>
        /// <returns>
        /// A tuple containing a boolean indicating success and the loaded list of runways.
        /// The list will be <see langword="null"/> if the operation failed.
        /// </returns>
        private async Task<(bool success, List<RunwayParams> runways)> TryGetRunwayXMLDataAsync(FormProgressReporter progressReporter)
        {
            // The method now returns a tuple, so we initialize the list here.
            var runways = new List<RunwayParams>();

            // Await the asynchronous method call and deconstruct the returned tuple.
            var (streamSuccess, stream) = await TryGetRunwayXMLStreamAsync(progressReporter);

            if (!streamSuccess)
            {
                return (false, null);
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

                    return (true, runways);
                }
            }
            catch (XmlException ex)
            {
                _log.Error($"XML parsing error. {ex.Message}");
                progressReporter?.Report($"ERROR: Error loading runway data: XML format is invalid.");
                return (false, null);
            }
        }

        /// <summary>
        /// Asynchronously attempts to get a stream for the runway XML data,
        /// first from a local file and then from an embedded resource.
        /// </summary>
        /// <param name="progressReporter">The object for reporting progress and status updates to the UI.</param>
        /// <returns>
        /// A tuple containing a boolean indicating success and the loaded Stream.
        /// The stream will be <see langword="null"/> if the operation failed.
        /// </returns>
        private async Task<(bool success, Stream stream)> TryGetRunwayXMLStreamAsync(FormProgressReporter progressReporter)
        {
            const string xmlFilename = "runways.xml";
            const string embeddedResourceName = $"XML.{xmlFilename}";

            string dataDirectory = await _fileOps.GetApplicationDataDirectoryAsync();
            string localFilePath = Path.Combine(dataDirectory, xmlFilename);

            string message = $"Attempting to retrieve runway XML stream for '{xmlFilename}' from local file.";
            _log.Info(message);
            progressReporter?.Report($"INFO: {message}");

            if (File.Exists(localFilePath))
            {
                _log.Info($"Local runway XML file found: '{localFilePath}'. Attempting to load.");
                var (success, fileBytes) = await _fileOps.TryReadAllBytesAsync(localFilePath, progressReporter);
                if (success)
                {
                    Stream stream = new MemoryStream(fileBytes);
                    _log.Info($"Successfully loaded runway XML from local file: '{localFilePath}'.");
                    progressReporter?.Report($"INFO: Successfully loaded runway XML.");
                    return (true, stream);
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

            var (embeddedSuccess, embeddedStream) = await _fileOps.TryGetResourceStreamAsync(embeddedResourceName, progressReporter);
            if (embeddedSuccess)
            {
                _log.Info($"Successfully loaded runway XML from embedded resource: '{embeddedResourceName}'.");
                progressReporter?.Report("INFO: Successfully loaded runway XML from embedded resource.");
                return (true, embeddedStream);
            }
            else
            {
                message = $"Failed to load runway XML from embedded resource: '{embeddedResourceName}'. Runway data is unavailable.";
                _log.Error(message);
                progressReporter?.Report($"ERROR: {message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Attempts to load the entire RunwayData object, including the KD-tree, from a binary cache file.
        /// </summary>
        /// <param name="progressReporter">The progress reporter for UI updates.</param>
        /// <returns>The <see cref="RunwayData"/> object if successful; otherwise, <see langword="null"/>.</returns>
        private async Task<RunwayData> TryLoadFromCacheAsync(FormProgressReporter progressReporter)
        {
            string dataDirectory = await _fileOps.GetApplicationDataDirectoryAsync();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");
            string xmlFilePath = Path.Combine(dataDirectory, "runways.xml");

            bool isCacheOutOfDate = false;
            DateTime cacheLastModified = _fileOps.GetFileLastWriteTime(cacheFilePath) ?? DateTime.MinValue;
            DateTime xmlLastModified = _fileOps.GetFileLastWriteTime(xmlFilePath) ?? DateTime.MinValue;

            if (File.Exists(cacheFilePath) && File.Exists(xmlFilePath))
            {
                if (xmlLastModified > cacheLastModified)
                {
                    isCacheOutOfDate = true;
                    progressReporter.Report("NOTICE: Runways XML file is newer than the cache. Parsing XML...");
                }
            }

            if (File.Exists(cacheFilePath) && !isCacheOutOfDate)
            {
                progressReporter.Report("INFO: Loading runways from binary cache...");
                // Deserialize the entire RunwayData object, including the KD-tree.
                var (success, runwayData) = await _cacheManager.TryDeserializeFromFileAsync<RunwayData>(cacheFilePath);

                if (success)
                {
                    _log.Info("Successfully loaded runways and KD-tree from cache.");
                    return runwayData;
                }
                else
                {
                    string message = "Failed to load from cache. Falling back to XML.";
                    progressReporter.Report($"ERROR: {message}");
                    _log.Error(message);
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Asynchronously serializes and saves the entire RunwayData object, including the KD-tree, to a binary cache file.
        /// </summary>
        /// <param name="runwayData">The RunwayData object to be cached.</param>
        /// <param name="progressReporter">The object for reporting progress and status updates to the UI.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
        private async Task SaveToCacheAsync(RunwayData runwayData, FormProgressReporter progressReporter)
        {
            string dataDirectory = await _fileOps.GetApplicationDataDirectoryAsync();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");

            // Serialize the entire RunwayData object, which includes the KD-tree.
            await _cacheManager.TrySerializeToFileAsync(runwayData, cacheFilePath);

            const string message = "Runway data (including KD-tree) cached to binary file.";
            _log.Info(message);
            progressReporter.Report($"INFO: {message}");
        }

        /// <summary>
        /// Parses a single airport's data from the XML reader.
        /// </summary>
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
        /// Parses a single runway's data from the XML reader.
        /// </summary>
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
        /// Parses and sets the runway ID and designator.
        /// </summary>
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

        /// <summary>
        /// Builds a KD-tree from a list of runway parameters.
        /// </summary>
        /// <param name="runways">The list of runway parameters to build the tree from.</param>
        /// <param name="progressReporter">The progress reporter for UI updates.</param>
        /// <returns>The root node of the constructed KD-tree.</returns>
        private static KDNode BuildKDTree(List<RunwayParams> runways, FormProgressReporter progressReporter)
        {
            progressReporter.Report("INFO: Building KD-tree for spatial indexing...");
            // Start the recursive build process.
            return BuildKDTreeRecursive(runways, 0);
        }

        /// <summary>
        /// A recursive helper function to build the KD-tree.
        /// </summary>
        /// <param name="runways">The list of runways for the current subtree.</param>
        /// <param name="axis">The current splitting axis (0 for Latitude, 1 for Longitude).</param>
        /// <returns>The root node of the current subtree.</returns>
        private static KDNode BuildKDTreeRecursive(List<RunwayParams> runways, int axis)
        {
            if (runways == null || runways.Count == 0)
            {
                return null;
            }

            // Sort the list of runways based on the current axis.
            if (axis == 0) // Latitude
            {
                runways.Sort((a, b) => a.AirportLat.CompareTo(b.AirportLat));
            }
            else // Longitude
            {
                runways.Sort((a, b) => a.AirportLon.CompareTo(b.AirportLon));
            }

            // Find the median and split the list.
            int medianIndex = runways.Count / 2;
            RunwayParams medianRunway = runways[medianIndex];

            // Create the node for the median runway.
            KDNode node = new()
            {
                Runway = medianRunway,
                Axis = axis,
                // Recursively build the left and right subtrees.
                Left = BuildKDTreeRecursive([.. runways.Take(medianIndex)], (axis + 1) % 2),
                Right = BuildKDTreeRecursive([.. runways.Skip(medianIndex + 1)], (axis + 1) % 2)
            };

            return node;
        }
    }
}