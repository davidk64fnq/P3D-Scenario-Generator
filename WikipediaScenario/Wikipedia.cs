using CoordinateSharp;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.WikipediaScenario
{

    /// <summary>
    /// Provides routines for the Wikipedia scenario type
    /// </summary>
    public class Wikipedia(Logger logger, FileOps fileOps, FormProgressReporter progressReporter, HttpRoutines httpRoutines)
    {
        // Guard clauses to validate the constructor parameters.
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly HttpRoutines _httpRoutines = httpRoutines;
        private readonly MapTileImageMaker _mapTileImageMaker = new(logger, progressReporter, fileOps, httpRoutines);
        private readonly ImageUtils _imageUtils = new(logger, fileOps, progressReporter);

        /// <summary>
        /// The wikipedia list items plus start and finish airports
        /// </summary>
        internal int WikiCount { get; private set; }

        /// <summary>
        /// From start to finish airport
        /// </summary>
        internal int WikiDistance { get; private set; }

        /// <summary>
        /// Table(s) of items scraped from user supplied Wikipedia URL
        /// </summary>
        internal List<List<WikiItemParams>> WikiPage { get; set; }

        /// <summary>
        /// List of user selected Wikipedia items
        /// </summary>
        internal List<WikiItemParams> WikiTour { get; private set; } 

        #region Form routines - populate UI, list of tables and route for selected table including all valid items

        /// <summary>
        /// Creates a summary string for each table in <see cref="WikiPage"/> in the form:
        /// <para>[0] first item description ... [^1] last item description (number of items)</para>
        /// </summary>
        /// <returns>List of table summary strings</returns>
        internal List<string> CreateWikiTablesDesc()
        {
            var list = new List<string>();
            string tableDesc;
            for (int tableNo = 0; tableNo < WikiPage.Count; tableNo++)
            {
                if (WikiPage[tableNo].Count == 1)
                {
                    tableDesc = $"{WikiPage[tableNo][0].title} (one item)";
                }
                else 
                {
                    tableDesc = $"{WikiPage[tableNo][0].title} ... {WikiPage[tableNo][^1].title} ({WikiPage[tableNo].Count} items)";
                }
                list.Add(tableDesc);
            }
            return list;
        }

        /// <summary>
        /// Creates a route (non-optimal) for a table in <see cref="WikiPage"/> 
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <returns>List of route leg summary strings</returns>
        internal List<string> CreateWikiTableRoute(int tableNo)
        {
            int[,] wikiTableCost = new int[WikiPage[tableNo].Count, WikiPage[tableNo].Count]; // Matrix of distances between items in miles
            List<string> route = []; // Route leg summary strings
            bool[] itemsVisited = new bool[WikiPage[tableNo].Count]; // Track addition of items to route as it's built
            int firstRouteItem = 0; // Track first item of route as it's built
            int lastRouteItem; // Track last item of route as it's built
            int itemVisitedCount; // Track how many items have been added to route as it's built

            SetWikiTableCosts(tableNo, wikiTableCost);

            // Initialise route with first leg (handles special case where only one item)
            itemsVisited[0] = true;
            itemVisitedCount = 1;
            lastRouteItem = GetNearesetWikiItem(0, wikiTableCost, itemsVisited);
            AddLegToRoute(route, tableNo, wikiTableCost, 0, firstRouteItem, lastRouteItem, itemsVisited, lastRouteItem, ref itemVisitedCount);

            while (itemVisitedCount < WikiPage[tableNo].Count)
            {
                // Find closest item to either end of current route and include in route, either as a new first leg or new last leg
                int nearestToFirstRouteItem = GetNearesetWikiItem(firstRouteItem, wikiTableCost, itemsVisited);
                int nearestToSecondRouteItem = GetNearesetWikiItem(lastRouteItem, wikiTableCost, itemsVisited);
                if (wikiTableCost[firstRouteItem, nearestToFirstRouteItem] <= wikiTableCost[lastRouteItem, nearestToSecondRouteItem])
                {
                    AddLegToRoute(route, tableNo, wikiTableCost, 0, nearestToFirstRouteItem, firstRouteItem, itemsVisited, 
                        nearestToFirstRouteItem, ref itemVisitedCount);
                    firstRouteItem = nearestToFirstRouteItem;
                }
                else
                {
                    AddLegToRoute(route, tableNo, wikiTableCost, itemVisitedCount, lastRouteItem, nearestToSecondRouteItem, 
                        itemsVisited, nearestToSecondRouteItem, ref itemVisitedCount);
                    lastRouteItem = nearestToSecondRouteItem;
                }
            }

            return route;
        }

        /// <summary>
        /// Creates a route leg string and adds it to route in the form:
        /// <para>[startItem] startItem description ... [finishItem] finishItem description (number of miles)</para>
        /// </summary>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles</param>
        /// <param name="insertionPt">New leg is inserted at front or added to end of route</param>
        /// <param name="startItem">Start item for new leg</param>
        /// <param name="finishItem">Finish item for new leg</param>
        /// <param name="itemsVisited">Tracks addition of items to route as it's built</param>
        /// <param name="newItem">Will be either the startItem or finishItem depending on which end of route</param>
        /// <param name="itemVisitedCount">Tracks how many items have been added to route as it's built</param>
        internal void AddLegToRoute(List<string> route, int tableNo, int[,] wikiTableCost, int insertionPt, 
            int startItem, int finishItem, bool[] itemsVisited, int newItem, ref int itemVisitedCount)
        {
            if (insertionPt > route.Count - 1)
            {
                route.Add($"[{startItem}] {WikiPage[tableNo][startItem].title} ... [{finishItem}] {WikiPage[tableNo][finishItem].title} " +
                    $"({wikiTableCost[startItem, finishItem]} miles)");
            }
            else
            {
                route.Insert(insertionPt, $"[{startItem}] {WikiPage[tableNo][startItem].title} ... " +
                    $"[{finishItem}] {WikiPage[tableNo][finishItem].title} ({wikiTableCost[startItem, finishItem]} miles)");
            }
            itemVisitedCount++;
            itemsVisited[newItem] = true;
        }

        /// <summary>
        /// Searches items not yet added to route and returns closest to curItem
        /// </summary>
        /// <param name="curItem">The item for which distances to be measured</param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles</param>
        /// <param name="itemsVisited">Tracks addition of items to route as it's built</param>
        /// <returns></returns>
        static internal int GetNearesetWikiItem(int curItem, int[,] wikiTableCost, bool[] itemsVisited)
        {
            int minDistance = int.MaxValue;
            int nearestWikiItem = 0;
            for (int itemIndex = 0; itemIndex < itemsVisited.Length; itemIndex++)
            {
                if (itemIndex != curItem && wikiTableCost[curItem, itemIndex] < minDistance && itemsVisited[itemIndex] == false)
                {
                    minDistance = wikiTableCost[curItem, itemIndex];
                    nearestWikiItem = itemIndex;
                }
            }
            return nearestWikiItem;
        }

        /// <summary>
        /// Creates a matrix of distances between items in measured in miles
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles</param>
        internal void SetWikiTableCosts(int tableNo, int[,] wikiTableCost)
        {
            for (int row = 0; row < WikiPage[tableNo].Count; row++)
            {
                for (int col = 0; col < WikiPage[tableNo].Count; col++)
                {
                    Coordinate coord1 = Coordinate.Parse($"{WikiPage[tableNo][row].latitude} {WikiPage[tableNo][row].longitude}");
                    Coordinate coord2 = Coordinate.Parse($"{WikiPage[tableNo][col].latitude} {WikiPage[tableNo][col].longitude}");
                    wikiTableCost[row, col] = (int)coord1.Get_Distance_From_Coordinate(coord2).Miles;
                }
            }
        }

        #endregion

        #region SetWikiTour - Populate WikiTour using methods in next section plus set airport(s) and create OSM images 

        /// <summary>
        /// Populate WikiTour using methods in next section plus set airport(s) and create OSM images
        /// </summary>
        /// <param name="formData">The scenario form data.</param>
        /// <param name="runwayManager">The runway manager instance.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task<bool> SetWikiTourAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            PopulateWikiTour(formData);

            await SetWikiAirports(formData, runwayManager);

            formData.OSMmapData = [];
            if (!await _mapTileImageMaker.CreateOverviewImageAsync(SetOverviewCoords(WikiTour), formData))
            {
                await _logger.ErrorAsync("Failed to create overview image during Wikipedia Tour setup.");
                return false;
            }

            if (!await _mapTileImageMaker.CreateLocationImageAsync(SetLocationCoords(formData), formData))
            {
                await _logger.ErrorAsync($"Failed to draw route on overview image during Wikipedia Tour setup.");
                return false;
            }

            formData.OSMmapData.Clear();
            for (int index = 0; index < WikiTour.Count - 1; index++)
            {
                int legNo = index + 1;
                if (!await _mapTileImageMaker.SetLegRouteImagesAsync(SetRouteCoords(WikiTour, index), legNo, formData))
                {
                    await _logger.ErrorAsync($"Failed to create location image for leg {index} during Wikipedia Tour setup.");
                    return false;
                }
            }

            if (!await _imageUtils.DrawRouteBulkAsync(formData))
            {
                await _logger.ErrorAsync($"Failed to draw image routes during Wikipedia Tour setup.");
                return false;
            }

            Overview overview = SetOverviewStruct(formData);
            ScenarioHTML scenarioHTML = new(_logger, _fileOps, _progressReporter);
            if (!await scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                string message = "Failed to generate HTML files during Wikipedia setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            ScenarioXML.SetSimbaseDocumentXML(formData, overview);
            await ScenarioXML.SetWikiListWorldBaseFlightXML(formData, overview, this, fileOps, progressReporter);
            await ScenarioXML.WriteXMLAsync(formData, fileOps, progressReporter);

            return true;
        }


        /// <summary>
        /// Finds and inserts/appends wiki tour start and finish airports. Adjusts <see cref="WikiDistance"/>
        /// to include airport legs
        /// </summary>
        internal async Task SetWikiAirports(ScenarioFormData formData, RunwayManager runwayManager)
        {
            Coordinate coordFirstItem = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
            WikiTour.Insert(0, await GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble(), formData, runwayManager));
            Coordinate coordStartAirport = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
            WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;
            formData.StartRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(WikiTour[0].airportIndex);

            Coordinate coordLastItem = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
            WikiTour.Add(await GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble(), formData, runwayManager));
            Coordinate coordFinishAirport = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
            WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
            formData.DestinationRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(WikiTour[^1].airportIndex);
        }

        /// <summary>
        /// Calls FindNearestRunwayAsync method in RunwaySearcher class to look for closest airport. Populates an instance of WikiItemParams
        /// with the airport information
        /// </summary>
        /// <param name="queryLat">The wiki item latitude</param>
        /// <param name="queryLon">The wiki item longitude</param>
        /// <returns></returns>
        static internal async Task<WikiItemParams> GetNearestAirport(double queryLat, double queryLon, ScenarioFormData formData, RunwayManager runwayManager)
        {
            WikiItemParams wikiItemParams = new();
            // The FindNearestRunway method is now asynchronous and must be awaited.
            // The calling method's signature must also be updated to async and return Task.
            RunwayParams nearestAirport = await runwayManager.Searcher.FindNearestRunwayAsync(queryLat, queryLon, formData);
            if (nearestAirport == null)
                return null;
            wikiItemParams.airportICAO = nearestAirport.IcaoId;
            wikiItemParams.airportID = nearestAirport.Id;
            wikiItemParams.latitude = nearestAirport.AirportLat.ToString();
            wikiItemParams.longitude = nearestAirport.AirportLon.ToString();
            wikiItemParams.airportIndex = nearestAirport.RunwaysIndex;
            return wikiItemParams;
        }


        /// <summary>
        /// Finds OSM tile numbers and offsets for a <see cref="WikiTour"/> (all items plus airports, or a pair of items)
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of start item in <see cref="WikiTour"/></param>
        /// <param name="finishItemIndex">Index of finish item in <see cref="WikiTour"/></param>
        static internal void SetWikiOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            tiles.Clear();
            for (int itemNo = startItemIndex; itemNo <= finishItemIndex; itemNo++)
            {
                zoom += 1; // to stop ide warning of unused zoom parameter
                           //        tiles.Add(MapTileCalculator.GetOSMtile(WikiTour[itemNo].longitude, WikiTour[itemNo].latitude, zoom));
            }
        }

        public Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
            briefing += " as you navigate from one Wikipedia list location to the next using IFR (I follow roads) ";
            briefing += "You'll take off, fly to a series of list locations, ";
            briefing += "and land at another airport. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            string objective = "Take off and visit a series of Wikipedia list locations before landing at ";
            objective += $"at {formData.DestinationRunway.IcaoName} (any runway)";

            // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
            double duration = WikiDistance / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Wikipedia List Tour",
                Heading1 = "Wikipedia List Tour",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Intermediate",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftTitle}",
                Briefing = briefing,
                Objective = objective,
                Tips = "Do not, under any circumstances, fly over a 'Dead-end page.' The lack of outgoing links makes for treacherous, inescapable airspace."
            };

            return overview;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of <see cref="Coordinate"/> objects
        /// representing the geographical locations (latitude and longitude) for all entries
        /// in the provided list of Wikipedia and airport parameters.
        /// </summary>
        /// <param name="wikiTour">A list of <see cref="WikiItemParams"/> objects,
        /// where each object contains latitude and longitude information for a Wikipedia list item.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the latitude and longitude for each location in the input list.
        /// </returns>
        public static IEnumerable<Coordinate> SetOverviewCoords(List<WikiItemParams> wikiTour)
        {
            // The Select method iterates over each Wikipedia list item/airport in the wikiTour
            // and projects it into a new 'Coordinate' object using the location's lat and lon.
            return wikiTour.Select(wikiItem => new Coordinate(CoordinatePart.Parse(wikiItem.latitude).DecimalDegree, CoordinatePart.Parse(wikiItem.longitude).DecimalDegree));
        }

        /// <summary>
        /// Creates and returns an enumerable collection containing a single <see cref="Coordinate"/> object
        /// that represents the geographical location (latitude and longitude) of the start runway.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// only the start runway's latitude and longitude.
        /// </returns>
        public static IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
            ];
            return coordinates;
        }

        /// <summary>
        /// Creates and returns an enumerable collection of two <see cref="Coordinate"/> objects
        /// representing a specific segment of the Wikipedia List's route.
        /// The segment starts from the Wikipedia list item at the given index and ends at the next item in the sequence.
        /// </summary>
        /// <param name="wikiTour">A list of <see cref="WikiItemParams"/> objects,
        /// representing the ordered locations (Wikipedia list items) along the tour.</param>
        /// <param name="index">The zero-based index of the starting Wikipedia list item in the <paramref name="wikiTour"/> list
        /// for which the route segment is to be generated.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Coordinate"/> containing
        /// the latitude and longitude of the Wikipedia list item at <paramref name="index"/> and the item at <paramref name="index"/> + 1.
        /// </returns>
        public static IEnumerable<Coordinate> SetRouteCoords(List<WikiItemParams> wikiTour, int index)
        {
            IEnumerable<Coordinate> coordinates =
            [
                new Coordinate(CoordinatePart.Parse(wikiTour[index].latitude).DecimalDegree, CoordinatePart.Parse(wikiTour[index].longitude).DecimalDegree),
                new Coordinate(CoordinatePart.Parse(wikiTour[index + 1].latitude).DecimalDegree, CoordinatePart.Parse(wikiTour[index + 1].longitude).DecimalDegree)
            ];
            return coordinates;
        }

        #endregion

        #region Populating WikiTour

        /// <summary>
        /// Populates WikiTour for selected table based on user specified start and finish items and current route
        /// </summary>
        /// <param name="formData">The scenario form data.</param>
        internal void PopulateWikiTour(ScenarioFormData formData)
        {
            WikiTour = [];
            bool finished = PopulateWikiTourOneItem(formData);
            if (!finished)
            {
                PopulateWikiTourMultipleItems(formData);
            }
            WikiCount = WikiTour.Count + 2; // Wiki tour items plus two airports
            WikiDistance = formData.WikiURLTourDistance;
        }

        /// <summary>
        /// Handles case where user has selected a single item.
        /// </summary>
        /// <param name="formData">The scenario form data.</param>
        /// <returns>True if this case applies</returns>
        internal bool PopulateWikiTourOneItem(ScenarioFormData formData)
        {
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(formData.WikiURLTourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(formData.WikiURLTourFinishItem.ToString());
            if (tourStartItemNo == tourFinishItemNo)
            {
                WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourStartItemNo));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles case where user has selected two or more items.
        /// </summary>
        /// <param name="formData">The scenario form data.</param>
        /// <returns>True if this case applies</returns>
        internal bool PopulateWikiTourMultipleItems(ScenarioFormData formData)
        {
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(formData.WikiURLTourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(formData.WikiURLTourFinishItem.ToString());
            int legStartItemNo, legFinishItemNo, startLegNo = 0;
            var routeLegs = formData.WikiURLRoute.GetEnumerator();

            // Find tourStartItemNo in route
            for (int legNo = 0; legNo < formData.WikiURLRoute.Count; legNo++)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                if (tourStartItemNo == legStartItemNo)
                {
                    WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourStartItemNo));
                    startLegNo = legNo;
                    if (tourFinishItemNo == legFinishItemNo)
                    {
                        WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourFinishItemNo)); // tourStartItemNo and tourFinishItemNo were in same leg
                        return false;
                    }
                    break;
                }
            }

            // Add legStartItemNo's until tourFinishItemNo == legFinishItemNo then add legFinishItemNo
            for (int legNo = startLegNo; legNo < formData.WikiURLRoute.Count; legNo++)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, legStartItemNo));
                if (tourFinishItemNo == legFinishItemNo)
                {
                    WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourFinishItemNo));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Used to extract first item number from leg route string
        /// </summary>
        /// <param name="routeLeg">A leg in route</param>
        /// <returns>Start item number of route leg</returns>
        static internal int GetWikiRouteLegFirstItemNo(string routeLeg)
        {
            int stringBegin, stringEnd;
            stringBegin = routeLeg.IndexOf('[') + 1;
            stringEnd = routeLeg.IndexOf(']');
            return int.Parse(routeLeg[stringBegin..stringEnd]);
        }

        /// <summary>
        /// Used to extract second item number from leg route string
        /// </summary>
        /// <param name="routeLeg">A leg in route</param>
        /// <returns>Finish item number of route leg</returns>
        static internal int GetWikiRouteLegLastItemNo(string routeLeg)
        {
            int stringBegin, stringEnd;
            stringBegin = routeLeg.IndexOf("...") + 4;
            stringEnd = routeLeg.LastIndexOf('(') - 1;
            return GetWikiRouteLegFirstItemNo(routeLeg[stringBegin..stringEnd]);
        }

        /// <summary>
        /// Populates an item with title, link, latitude and longitude
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="itemNo">The item no reference in <see cref="WikiPage"/></param>
        /// <returns>A populated item</returns>
        internal WikiItemParams SetWikiItem(int tableNo, int itemNo)
        {
            WikiItemParams wikiItem = new()
            {
                title = WikiPage[tableNo][itemNo].title,
                itemURL = WikiPage[tableNo][itemNo].itemURL,
                latitude = WikiPage[tableNo][itemNo].latitude,
                longitude = WikiPage[tableNo][itemNo].longitude,
                hrefs = WikiPage[tableNo][itemNo].hrefs
            };
            return wikiItem;
        }

        #endregion

        #region XML routines

        /// <summary>
        /// Calculates the position (horizontal and vertical offsets) and dimensions (width and height)
        /// for the map window based on the specified alignment and monitor properties.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the
        /// map window's desired alignment, offsets, monitor dimensions, and calculated window size.</param>
        /// <returns>
        /// A <see cref="T:System.String[]"/> array containing four elements in the order:
        /// <list type="bullet">
        /// <item><description>Window Width (string)</description></item>
        /// <item><description>Window Height (string)</description></item>
        /// <item><description>Horizontal Offset (string)</description></item>
        /// <item><description>Vertical Offset (string)</description></item>
        /// </list>
        /// These parameters are suitable for configuring the map window's display.
        /// </returns>
        static internal string[] GetMapWindowParameters(ScenarioFormData formData)
        {
            // Dimensions
            int mapWindowWidth = (int)formData.MapWindowSize;
            int mapWindowHeight = (int)formData.MapWindowSize;

            return ScenarioXML.GetWindowParameters(mapWindowWidth, mapWindowHeight, formData.MapAlignment,
            formData.MapMonitorWidth, formData.MapMonitorHeight, formData.MapOffset);
        }

        /// <summary>
        /// Calculates the position (horizontal and vertical offsets) and dimensions (width and height)
        /// for the Wiki URL window based on the specified alignment and monitor properties.
        /// </summary>
        /// <param name="formData">The <see cref="ScenarioFormData"/> object containing the
        /// Wiki UR window's desired alignment, offsets, monitor dimensions, and calculated window size.</param>
        /// <returns>
        /// A <see cref="T:System.String[]"/> array containing four elements in the order:
        /// <list type="bullet">
        /// <item><description>Window Width (string)</description></item>
        /// <item><description>Window Height (string)</description></item>
        /// <item><description>Horizontal Offset (string)</description></item>
        /// <item><description>Vertical Offset (string)</description></item>
        /// </list>
        /// These parameters are suitable for configuring the Wiki URL window's display.
        /// </returns>
        static internal string[] GetWikiURLWindowParameters(ScenarioFormData formData)
        {
            return ScenarioXML.GetWindowParameters(formData.WikiURLWindowWidth, formData.WikiURLWindowHeight, formData.WikiURLAlignment,
                formData.WikiURLMonitorWidth, formData.WikiURLMonitorHeight, formData.WikiURLOffset);
        }

        static internal string GetWikiItemWorldPosition(int legNo, Wikipedia wikipedia)
        {
            return $"{wikipedia.WikiTour[legNo].latitude}, {wikipedia.WikiTour[legNo].longitude},+0.0";
        }

        #endregion
    }
}
