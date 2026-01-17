using P3D_Scenario_Generator.Services;
using System.Xml;

namespace P3D_Scenario_Generator.Runways
{
    public class RunwayLoader(FileOps fileOps, CacheManager cacheManager, Logger log)
    {
        // Parameter validation for the primary constructor.
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly CacheManager _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        private readonly Logger _log = log ?? throw new ArgumentNullException(nameof(log));

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

                    try
                    {
                        KDNode treeRoot = await Task.Run(() => BuildKDTree(runways, progressReporter));

                        // Create the full RunwayData object to be cached.
                        runwayData = new RunwayData { Runways = runways, RunwayTreeRoot = treeRoot };
                        await SaveToCacheAsync(runwayData, progressReporter);
                        progressReporter.Report($"INFO: Successfully loaded {runways.Count} runways from XML and cached.");
                        return runwayData;
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle the cancellation gracefully
                        await _log.InfoAsync("Runway loading and KD-tree building was canceled.");
                        progressReporter.Report("NOTICE: Runway data loading canceled.");
                        return null;
                    }
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
            var (isSuccess, loadedRunways) = await TryGetRunwayXMLDataAsync(progressReporter);

            if (isSuccess)
            {
                return loadedRunways;
            }
            else
            {
                await _log.ErrorAsync("Runway data is unavailable after attempting to load from XML.");
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
            var runways = new List<RunwayParams>();

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
                    int airportCount = 0;

                    while (reader.ReadToFollowing("ICAO"))
                    {
                        airportCount++;
                        RunwayParams curAirport = ReadAirport(reader);

                        // YIELD THE THREAD every 500 airports to allow UI to update
                        if (airportCount % 500 == 0)
                        {
                            progressReporter?.Report($"INFO: Processed {airportCount} airports...");
                            await Task.Yield(); // This is the "Magic" line
                        }

                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            do
                            {
                                RunwayParams newRunway = ReadRunway(reader, curAirport);
                                newRunway.RunwaysIndex = curIndex++;
                                runways.Add(newRunway);
                            } while (reader.ReadToNextSibling("Runway"));
                        }
                    }

                    string message = $"Successfully loaded {runways.Count} runways.";
                    await _log.InfoAsync(message);
                    progressReporter?.Report($"INFO: {message}");

                    return (true, runways);
                }
            }
            catch (XmlException ex)
            {
                await _log.ErrorAsync($"XML parsing error.", ex);
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

            string dataDirectory = await FileOps.GetApplicationDataDirectoryAsync();
            string localFilePath = Path.Combine(dataDirectory, xmlFilename);

            string message = $"Attempting to retrieve runway XML stream for '{xmlFilename}' from local file.";
            await _log.InfoAsync(message);
            progressReporter?.Report($"INFO: {message}");

            if (File.Exists(localFilePath))
            {
                await _log.InfoAsync($"Local runway XML file found: '{localFilePath}'. Attempting to load.");
                // The IFileOps.TryReadAllBytesAsync method does not accept a CancellationToken, so it's removed.
                // It does, however, accept a progress reporter.
                var (localFileSuccess, localFileData) = await _fileOps.TryReadAllBytesAsync(localFilePath, progressReporter);
                if (localFileSuccess)
                {
                    Stream stream = new MemoryStream(localFileData);
                    await _log.InfoAsync($"Successfully loaded runway XML from local file: '{localFilePath}'.");
                    progressReporter?.Report("INFO: Successfully loaded runway XML.");
                    return (true, stream);
                }
                else
                {
                    await _log.ErrorAsync($"Failed to read local runway XML file: '{localFilePath}'. Falling back to embedded resource.");
                }
            }
            else
            {
                await _log.InfoAsync($"Local runway XML file not found at '{localFilePath}'. Attempting to load from embedded resource.");
                progressReporter?.Report("INFO: Local runways XML not found. Attempting to load from embedded resource.");
            }

            // The IFileOps.TryGetResourceStreamAsync method does not accept a CancellationToken.
            var (embeddedFileSuccess, embeddedFileStream) = await _fileOps.TryGetResourceStreamAsync(embeddedResourceName, progressReporter);
            if (embeddedFileSuccess)
            {
                await _log.InfoAsync($"Successfully loaded runway XML from embedded resource: '{embeddedResourceName}'.");
                progressReporter?.Report("INFO: Successfully loaded runway XML from embedded resource.");
                return (true, embeddedFileStream);
            }
            else
            {
                message = $"Failed to load runway XML from embedded resource: '{embeddedResourceName}'. Runway data is unavailable.";
                await _log.ErrorAsync(message);
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
            string dataDirectory = await FileOps.GetApplicationDataDirectoryAsync();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");
            string xmlFilePath = Path.Combine(dataDirectory, "runways.xml");

            bool isCacheOutOfDate = false;
            DateTime cacheLastModified = FileOps.GetFileLastWriteTime(cacheFilePath) ?? DateTime.MinValue;
            DateTime xmlLastModified = FileOps.GetFileLastWriteTime(xmlFilePath) ?? DateTime.MinValue;

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
                    await _log.InfoAsync("Successfully loaded runways and KD-tree from cache.");
                    return runwayData;
                }
                else
                {
                    string message = "Failed to load from cache. Falling back to XML.";
                    progressReporter.Report($"ERROR: {message}");
                    await _log.ErrorAsync(message);
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
            string dataDirectory = await FileOps.GetApplicationDataDirectoryAsync();
            string cacheFilePath = Path.Combine(dataDirectory, "runways.cache");

            // Serialize the entire RunwayData object, which includes the KD-tree.
            await _cacheManager.TrySerializeToFileAsync(runwayData, cacheFilePath);

            const string message = "Runway data (including KD-tree) cached to binary file.";
            await _log.InfoAsync(message);
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
        /// Builds a KD-tree from a list of runway parameters using index-based boundaries
        /// to prevent excessive memory allocation.
        /// </summary>
        private static KDNode BuildKDTree(List<RunwayParams> runways, FormProgressReporter progressReporter)
        {
            if (runways == null || runways.Count == 0) return null;

            progressReporter.Report("INFO: Building KD-tree for spatial indexing...");

            // Pass the entire list with the full range (0 to Count - 1)
            return BuildKDTreeRecursive(runways, 0, runways.Count - 1, 0);
        }

        /// <summary>
        /// A recursive helper function that operates on a single list using start and end pointers.
        /// </summary>
        private static KDNode BuildKDTreeRecursive(List<RunwayParams> runways, int start, int end, int axis)
        {
            // Base case: if the segment is empty
            if (start > end)
            {
                return null;
            }

            // 1. Sort only the specific segment of the list we are interested in.
            // This is much faster than copying the list first.
            int length = end - start + 1;
            if (axis == 0) // Latitude
            {
                runways.Sort(start, length, Comparer<RunwayParams>.Create((a, b) => a.AirportLat.CompareTo(b.AirportLat)));
            }
            else // Longitude
            {
                runways.Sort(start, length, Comparer<RunwayParams>.Create((a, b) => a.AirportLon.CompareTo(b.AirportLon)));
            }

            // 2. Find the median index of the current segment
            int medianIndex = start + (end - start) / 2;
            RunwayParams medianRunway = runways[medianIndex];

            // 3. Create the node and recursively build subtrees using index boundaries
            KDNode node = new()
            {
                Runway = medianRunway,
                Axis = axis,
                // Left subtree is from start to the item before the median
                Left = BuildKDTreeRecursive(runways, start, medianIndex - 1, (axis + 1) % 2),
                // Right subtree is from the item after the median to the end
                Right = BuildKDTreeRecursive(runways, medianIndex + 1, end, (axis + 1) % 2)
            };

            return node;
        }
    }
}
