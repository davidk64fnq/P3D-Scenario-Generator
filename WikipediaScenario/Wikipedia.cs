using CoordinateSharp;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.MapTiles;
using P3D_Scenario_Generator.Runways;
using P3D_Scenario_Generator.Services;

namespace P3D_Scenario_Generator.WikipediaScenario
{
    /// <summary>
    /// Provides routines for the Wikipedia scenario type, including scraping tables,
    /// building flight routes between Wikipedia locations, and generating scenario files.
    /// </summary>
    /// <param name="logger">The logging service.</param>
    /// <param name="progressReporter">The UI progress reporting service.</param>
    /// <param name="mapTileImageMaker">The map tile composition service.</param>
    /// <param name="imageUtils">The image utility service.</param>
    /// <param name="assetFileGenerator">The asset file generator service.</param>
    /// <param name="scenarioXML">The scenario XML generator.</param>
    /// <param name="scenarioHTML">The scenario HTML file generator.</param>
    internal class Wikipedia(
        Logger logger,
        FormProgressReporter progressReporter,
        MapTileImageMaker mapTileImageMaker,
        ImageUtils imageUtils,
        AssetFileGenerator assetFileGenerator,
        ScenarioXML scenarioXML,
        ScenarioHTML scenarioHTML)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));
        private readonly MapTileImageMaker _mapTileImageMaker = mapTileImageMaker ?? throw new ArgumentNullException(nameof(mapTileImageMaker));
        private readonly ImageUtils _imageUtils = imageUtils ?? throw new ArgumentNullException(nameof(imageUtils));
        private readonly AssetFileGenerator _assetFileGenerator = assetFileGenerator ?? throw new ArgumentNullException(nameof(assetFileGenerator));
        private readonly ScenarioXML _xml = scenarioXML ?? throw new ArgumentNullException(nameof(scenarioXML));
        private readonly ScenarioHTML _scenarioHTML = scenarioHTML ?? throw new ArgumentNullException(nameof(scenarioHTML));

        /// <summary>
        /// Gets the count of Wikipedia list items plus departure and destination airports.
        /// </summary>
        internal int WikiCount { get; private set; }

        /// <summary>
        /// Gets the total flight distance from departure to destination airport in nautical miles.
        /// </summary>
        internal int WikiDistance { get; private set; }

        /// <summary>
        /// Gets or sets table(s) of items scraped from a user-supplied Wikipedia URL.
        /// </summary>
        internal List<List<WikiItemParams>> WikiPage { get; set; } = [];

        /// <summary>
        /// Gets the list of user-selected Wikipedia items forming the tour.
        /// </summary>
        internal List<WikiItemParams> WikiTour { get; private set; } = [];

        #region Form routines

        /// <summary>
        /// Creates a summary string for each table in <see cref="WikiPage"/>.
        /// </summary>
        /// <returns>A list of table summary strings.</returns>
        internal List<string> CreateWikiTablesDesc()
        {
            var list = new List<string>();
            for (int tableNo = 0; tableNo < WikiPage.Count; tableNo++)
            {
                string tableDesc = WikiPage[tableNo].Count == 1
                    ? $"{WikiPage[tableNo][0].title} (one item)"
                    : $"{WikiPage[tableNo][0].title} ... {WikiPage[tableNo][^1].title} ({WikiPage[tableNo].Count} items)";

                list.Add(tableDesc);
            }
            return list;
        }

        /// <summary>
        /// Creates a route for a table in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="tableNo">The table index in <see cref="WikiPage"/>.</param>
        /// <returns>A list of route leg summary strings.</returns>
        internal List<string> CreateWikiTableRoute(int tableNo)
        {
            if (tableNo < 0 || tableNo >= WikiPage.Count || WikiPage[tableNo].Count == 0)
            {
                return [];
            }

            int[,] wikiTableCost = new int[WikiPage[tableNo].Count, WikiPage[tableNo].Count];
            List<string> route = [];
            bool[] itemsVisited = new bool[WikiPage[tableNo].Count];
            int firstRouteItem = 0;
            int lastRouteItem;
            int itemVisitedCount;

            SetWikiTableCosts(tableNo, wikiTableCost);

            itemsVisited[0] = true;
            itemVisitedCount = 1;
            lastRouteItem = GetNearesetWikiItem(0, wikiTableCost, itemsVisited);
            AddLegToRoute(route, tableNo, wikiTableCost, 0, firstRouteItem, lastRouteItem, itemsVisited, lastRouteItem, ref itemVisitedCount);

            while (itemVisitedCount < WikiPage[tableNo].Count)
            {
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
        /// Creates a route leg string and adds it to the route collection.
        /// </summary>
        /// <param name="route">The route leg summary collection.</param>
        /// <param name="tableNo">The table index in <see cref="WikiPage"/>.</param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles.</param>
        /// <param name="insertionPt">The insertion index within the route collection.</param>
        /// <param name="startItem">The starting item index for the new leg.</param>
        /// <param name="finishItem">The destination item index for the new leg.</param>
        /// <param name="itemsVisited">Tracking array indicating visited items.</param>
        /// <param name="newItem">The item being marked as visited.</param>
        /// <param name="itemVisitedCount">Counter tracking the total visited items.</param>
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
        /// Searches unvisited items and returns the index of the closest item to the specified item.
        /// </summary>
        /// <param name="curItem">The origin item index.</param>
        /// <param name="wikiTableCost">The distance matrix in miles.</param>
        /// <param name="itemsVisited">Tracking array of visited items.</param>
        /// <returns>The index of the closest unvisited item.</returns>
        internal static int GetNearesetWikiItem(int curItem, int[,] wikiTableCost, bool[] itemsVisited)
        {
            int minDistance = int.MaxValue;
            int nearestWikiItem = 0;
            for (int itemIndex = 0; itemIndex < itemsVisited.Length; itemIndex++)
            {
                if (itemIndex != curItem && wikiTableCost[curItem, itemIndex] < minDistance && !itemsVisited[itemIndex])
                {
                    minDistance = wikiTableCost[curItem, itemIndex];
                    nearestWikiItem = itemIndex;
                }
            }
            return nearestWikiItem;
        }

        /// <summary>
        /// Computes and populates a distance matrix in miles between all items in the specified table.
        /// </summary>
        /// <param name="tableNo">The table index in <see cref="WikiPage"/>.</param>
        /// <param name="wikiTableCost">The matrix to populate.</param>
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

        #region SetWikiTour

        /// <summary>
        /// Populates the tour, resolves nearest departure and destination airports, and generates map images and scenario files.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns><see langword="true"/> if the scenario was created successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetWikiTourAsync(ScenarioFormData formData, RunwayManager runwayManager)
        {
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(runwayManager);

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
                await _logger.ErrorAsync("Failed to draw route on overview image during Wikipedia Tour setup.");
                return false;
            }

            formData.OSMmapData.Clear();
            for (int index = 0; index < WikiTour.Count - 1; index++)
            {
                int legNo = index + 1;
                if (!await _mapTileImageMaker.SetLegRouteImagesAsync(SetRouteCoords(WikiTour, index), legNo, formData))
                {
                    await _logger.ErrorAsync($"Failed to create location image for leg {legNo} during Wikipedia Tour setup.");
                    return false;
                }
            }

            if (!await _imageUtils.DrawRouteBulkAsync(formData))
            {
                await _logger.ErrorAsync("Failed to draw image routes during Wikipedia Tour setup.");
                return false;
            }

            Overview overview = SetOverviewStruct(formData);
            if (!await _scenarioHTML.GenerateHTMLfilesAsync(formData, overview))
            {
                const string message = "Failed to generate HTML files during Wikipedia setup.";
                await _logger.ErrorAsync(message);
                _progressReporter.Report($"ERROR: {message}");
                return false;
            }

            if (!await SetWikiItemHTMLAsync(formData))
            {
                await _logger.ErrorAsync("Failed to set item HTML during Wikipedia Tour setup.");
                return false;
            }

            if (!await SetWikiTourJSAsync(formData))
            {
                await _logger.ErrorAsync("Failed to set item Javascript during Wikipedia Tour setup.");
                return false;
            }

            _xml.SetSimbaseDocumentXML(formData, overview);
            await SetWikiListWorldBaseFlightXMLAsync(formData, overview);
            _xml.WriteXML(formData);

            return true;
        }

        /// <summary>
        /// Resolves and prepends/appends qualifying departure and destination airports to <see cref="WikiTour"/>.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SetWikiAirports(ScenarioFormData formData, RunwayManager runwayManager)
        {
            if (WikiTour.Count == 0)
            {
                return;
            }

            Coordinate coordFirstItem = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
            WikiItemParams? startAirport = await GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble(), formData, runwayManager);
            if (startAirport != null)
            {
                WikiTour.Insert(0, startAirport);
                Coordinate coordStartAirport = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
                WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;
                var startRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(WikiTour[0].airportIndex);
                if (startRunway != null)
                {
                    formData.StartRunway = startRunway;
                }
            }

            Coordinate coordLastItem = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
            WikiItemParams? finishAirport = await GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble(), formData, runwayManager);
            if (finishAirport != null)
            {
                WikiTour.Add(finishAirport);
                Coordinate coordFinishAirport = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
                WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
                var destRunway = await runwayManager.Searcher.GetRunwayByIndexAsync(WikiTour[^1].airportIndex);
                if (destRunway != null)
                {
                    formData.DestinationRunway = destRunway;
                }
            }
        }

        /// <summary>
        /// Locates the nearest airport to the specified coordinates and returns its parameters.
        /// </summary>
        /// <param name="queryLat">The origin latitude.</param>
        /// <param name="queryLon">The origin longitude.</param>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="runwayManager">The runway manager providing runway search services.</param>
        /// <returns>A populated <see cref="WikiItemParams"/> representing the airport, or <see langword="null"/> if none found.</returns>
        internal static async Task<WikiItemParams?> GetNearestAirport(double queryLat, double queryLon, ScenarioFormData formData, RunwayManager runwayManager)
        {
            RunwayParams? nearestAirport = await runwayManager.Searcher.FindNearestRunwayAsync(queryLat, queryLon, formData);
            if (nearestAirport == null)
            {
                return null;
            }

            WikiItemParams wikiItemParams = new()
            {
                airportICAO = nearestAirport.IcaoId,
                airportID = nearestAirport.Id,
                latitude = nearestAirport.AirportLat.ToString(),
                longitude = nearestAirport.AirportLon.ToString(),
                airportIndex = nearestAirport.RunwaysIndex
            };
            return wikiItemParams;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a <see cref="WikiTour"/>.
        /// </summary>
        /// <param name="tiles">List of OSM tiles to populate.</param>
        /// <param name="zoom">The zoom level to query.</param>
        /// <param name="startItemIndex">Index of the starting item.</param>
        /// <param name="finishItemIndex">Index of the finishing item.</param>
        internal static void SetWikiOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            tiles.Clear();
            for (int itemNo = startItemIndex; itemNo <= finishItemIndex; itemNo++)
            {
                zoom++;
            }
        }

        /// <summary>
        /// Generates the scenario briefing, objective, duration, and overview metadata structure.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>A populated <see cref="Overview"/> record.</returns>
        internal Overview SetOverviewStruct(ScenarioFormData formData)
        {
            string briefing = $"In this scenario you'll test your skills flying a {formData.AircraftDisplayTitle}";
            briefing += " as you navigate from one Wikipedia list location to the next using IFR (I follow roads) ";
            briefing += "You'll take off, fly to a series of list locations, ";
            briefing += "and land at another airport. The scenario begins on runway ";
            briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
            briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";

            string objective = "Take off and visit a series of Wikipedia list locations before landing at ";
            objective += $"at {formData.DestinationRunway.IcaoName} (any runway)";

            double duration = WikiDistance / formData.AircraftCruiseSpeed * 60;

            Overview overview = new()
            {
                Title = "Wikipedia List Tour",
                Heading1 = "Wikipedia List Tour",
                Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                Difficulty = "Intermediate",
                Duration = $"{string.Format("{0:0}", duration)} minutes",
                Aircraft = $"{formData.AircraftDisplayTitle}",
                Briefing = briefing,
                Objective = objective,
                Tips = "Do not, under any circumstances, fly over a 'Dead-end page.' The lack of outgoing links makes for treacherous, inescapable airspace."
            };

            return overview;
        }

        /// <summary>
        /// Creates an enumerable collection of <see cref="Coordinate"/> objects for all entries in the tour.
        /// </summary>
        /// <param name="wikiTour">The list of Wikipedia and airport parameters.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of coordinates.</returns>
        internal static IEnumerable<Coordinate> SetOverviewCoords(List<WikiItemParams> wikiTour)
        {
            return wikiTour.Select(wikiItem => new Coordinate(CoordinatePart.Parse(wikiItem.latitude).DecimalDegree, CoordinatePart.Parse(wikiItem.longitude).DecimalDegree));
        }

        /// <summary>
        /// Creates an enumerable collection containing the start runway's coordinates.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the start runway coordinate.</returns>
        internal static IEnumerable<Coordinate> SetLocationCoords(ScenarioFormData formData)
        {
            return
            [
                new Coordinate(formData.StartRunway.AirportLat, formData.StartRunway.AirportLon)
            ];
        }

        /// <summary>
        /// Creates an enumerable collection of two <see cref="Coordinate"/> objects representing a tour leg.
        /// </summary>
        /// <param name="wikiTour">The list of Wikipedia items.</param>
        /// <param name="index">The zero-based start index of the leg.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of leg coordinates.</returns>
        internal static IEnumerable<Coordinate> SetRouteCoords(List<WikiItemParams> wikiTour, int index)
        {
            return
            [
                new Coordinate(CoordinatePart.Parse(wikiTour[index].latitude).DecimalDegree, CoordinatePart.Parse(wikiTour[index].longitude).DecimalDegree),
                new Coordinate(CoordinatePart.Parse(wikiTour[index + 1].latitude).DecimalDegree, CoordinatePart.Parse(wikiTour[index + 1].longitude).DecimalDegree)
            ];
        }

        #endregion

        #region Populating WikiTour

        /// <summary>
        /// Populates <see cref="WikiTour"/> for the selected table based on user selections.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        internal void PopulateWikiTour(ScenarioFormData formData)
        {
            WikiTour = [];
            bool finished = PopulateWikiTourOneItem(formData);
            if (!finished)
            {
                PopulateWikiTourMultipleItems(formData);
            }
            WikiCount = WikiTour.Count + 2;
            WikiDistance = formData.WikiURLTourDistance;
        }

        /// <summary>
        /// Handles the scenario where the user has selected a single item.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if a single item was handled; otherwise, <see langword="false"/>.</returns>
        internal bool PopulateWikiTourOneItem(ScenarioFormData formData)
        {
            string startItemStr = formData.WikiURLTourStartItem?.ToString() ?? string.Empty;
            string finishItemStr = formData.WikiURLTourFinishItem?.ToString() ?? string.Empty;
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(startItemStr);
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(finishItemStr);
            if (tourStartItemNo == tourFinishItemNo)
            {
                WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourStartItemNo));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the scenario where the user has selected two or more items.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if all items were added; otherwise, <see langword="false"/>.</returns>
        internal bool PopulateWikiTourMultipleItems(ScenarioFormData formData)
        {
            string startItemStr = formData.WikiURLTourStartItem?.ToString() ?? string.Empty;
            string finishItemStr = formData.WikiURLTourFinishItem?.ToString() ?? string.Empty;
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(startItemStr);
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(finishItemStr);
            int startLegNo = 0;

            for (int legNo = 0; legNo < formData.WikiURLRoute.Count; legNo++)
            {
                string routeLegStr = formData.WikiURLRoute[legNo]?.ToString() ?? string.Empty;
                int legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegStr);
                int legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegStr);
                if (tourStartItemNo == legStartItemNo)
                {
                    WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourStartItemNo));
                    startLegNo = legNo;
                    if (tourFinishItemNo == legFinishItemNo)
                    {
                        WikiTour.Add(SetWikiItem(formData.WikiURLTableNo, tourFinishItemNo));
                        return false;
                    }
                    break;
                }
            }

            for (int legNo = startLegNo; legNo < formData.WikiURLRoute.Count; legNo++)
            {
                string routeLegStr = formData.WikiURLRoute[legNo]?.ToString() ?? string.Empty;
                int legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegStr);
                int legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegStr);
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
        /// Extracts the first item number from a route leg descriptor string.
        /// </summary>
        /// <param name="routeLeg">The leg summary string.</param>
        /// <returns>The extracted item index, or 0 if parsing fails.</returns>
        internal static int GetWikiRouteLegFirstItemNo(string routeLeg)
        {
            if (string.IsNullOrWhiteSpace(routeLeg))
            {
                return 0;
            }

            int stringBegin = routeLeg.IndexOf('[') + 1;
            int stringEnd = routeLeg.IndexOf(']');
            if (stringBegin <= 0 || stringEnd <= stringBegin)
            {
                return 0;
            }

            return int.TryParse(routeLeg[stringBegin..stringEnd], out int result) ? result : 0;
        }

        /// <summary>
        /// Extracts the finish item number from a route leg descriptor string.
        /// </summary>
        /// <param name="routeLeg">The leg summary string.</param>
        /// <returns>The extracted item index, or 0 if parsing fails.</returns>
        internal static int GetWikiRouteLegLastItemNo(string routeLeg)
        {
            if (string.IsNullOrWhiteSpace(routeLeg))
            {
                return 0;
            }

            int dotsIndex = routeLeg.IndexOf("...");
            int parenIndex = routeLeg.LastIndexOf('(');
            if (dotsIndex < 0 || parenIndex <= dotsIndex + 4)
            {
                return 0;
            }

            int stringBegin = dotsIndex + 4;
            int stringEnd = parenIndex - 1;
            return GetWikiRouteLegFirstItemNo(routeLeg[stringBegin..stringEnd]);
        }

        /// <summary>
        /// Populates a <see cref="WikiItemParams"/> with details from the specified table and item indices.
        /// </summary>
        /// <param name="tableNo">The table index.</param>
        /// <param name="itemNo">The item index within the table.</param>
        /// <returns>A populated <see cref="WikiItemParams"/> object.</returns>
        internal WikiItemParams SetWikiItem(int tableNo, int itemNo)
        {
            return new WikiItemParams
            {
                title = WikiPage[tableNo][itemNo].title,
                itemURL = WikiPage[tableNo][itemNo].itemURL,
                latitude = WikiPage[tableNo][itemNo].latitude,
                longitude = WikiPage[tableNo][itemNo].longitude,
                hrefs = WikiPage[tableNo][itemNo].hrefs
            };
        }

        #endregion

        #region XML routines

        /// <summary>
        /// Configures XML entities, triggers, scripts, and goal actions for the Wikipedia flight scenario.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="overview">The overview metadata structure.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SetWikiListWorldBaseFlightXMLAsync(ScenarioFormData formData, Overview overview)
        {
            _xml.SetDisabledTrafficAirports($"{formData.StartRunway.IcaoId}");
            _xml.SetRealismOverrides();
            _xml.SetScenarioMetadata(formData, overview);
            _xml.SetDialogAction("Intro01", overview.Briefing, "2", "Text-To-Speech");
            _xml.SetDialogAction("Intro02", overview.Tips, "2", "Text-To-Speech");
            _xml.SetGoal("Goal01", overview.Objective);
            _xml.SetGoalResolutionAction("Goal01");

            _xml.SetScenarioVariable("ScenarioVariable01", "currentLegNo", "1");
            _xml.SetScenarioVariableTriggerValue(0.0, 0, "ScenarioVariable01");

            SetWikiTourScriptActions();

            _xml.SetUIPanelWindow(1, "UIpanelWindow", "False", "True", "images\\MovingMap.html", "False", "False");
            _xml.SetUIPanelWindow(2, "UIpanelWindow", "False", "True", "images\\WikipediaItem.html", "False", "False");

            await _assetFileGenerator.WriteAssetFileAsync("HTML.MovingMap.html", "MovingMap.html", formData.ScenarioImageFolder);
            await _assetFileGenerator.GenerateMovingMapScriptAsync(WikiCount, formData);
            await _assetFileGenerator.WriteAssetFileAsync("CSS.styleMovingMap.css", "styleMovingMap.css", formData.ScenarioImageFolder);

            _xml.SetOpenWindowAction(1, "UIPanelWindow", "UIpanelWindow", GetMapWindowParameters(formData), formData.MapMonitorNumber.ToString());
            _xml.SetCloseWindowAction(1, "UIPanelWindow", "UIpanelWindow");
            _xml.SetOpenWindowAction(2, "UIPanelWindow", "UIpanelWindow", GetWikiURLWindowParameters(formData), formData.WikiURLMonitorNumber.ToString());
            _xml.SetCloseWindowAction(2, "UIPanelWindow", "UIpanelWindow");

            for (int legNo = 1; legNo <= WikiCount - 2; legNo++)
            {
                _xml.SetCylinderArea(legNo, "CylinderArea", "0.0,0.0,0.0", "300", "18520.0", "None");
                string pwp = GetWikiItemWorldPosition(legNo, this);
                AttachedWorldPosition awp = ScenarioXML.GetAttachedWorldPosition(pwp, "True");
                _xml.SetAttachedWorldPosition("CylinderArea", $"CylinderArea{legNo:00}", awp);

                _xml.SetProximityTrigger(legNo, "ProximityTrigger", "False");
                _xml.SetProximityTriggerArea(legNo, "CylinderArea", $"CylinderArea{legNo:00}", "ProximityTrigger");

                _xml.SetObjectActivationAction(legNo, "ProximityTrigger", "ProximityTrigger", "ActProximityTrigger", "True");
                _xml.SetObjectActivationAction(legNo, "ProximityTrigger", "ProximityTrigger", "DeactProximityTrigger", "False");
                if (legNo == 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(2, "OpenWindowAction", "OpenUIpanelWindow", 1, "ProximityTrigger");
                }

                _xml.SetProximityTriggerOnEnterAction(legNo, "ObjectActivationAction", "DeactProximityTrigger", legNo, "ProximityTrigger");
            }

            for (int legNo = 1; legNo <= WikiCount - 2; legNo++)
            {
                if (legNo + 1 < WikiCount - 1)
                {
                    _xml.SetProximityTriggerOnEnterAction(legNo + 1, "ObjectActivationAction", "ActProximityTrigger", legNo, "ProximityTrigger");
                }

                _xml.SetProximityTriggerOnEnterAction(1, "ScriptAction", "ScriptAction", legNo, "ProximityTrigger");
            }

            _xml.SetTimerTrigger("TimerTrigger01", 1.0, "False", "True");
            _xml.SetTimerTriggerAction("OpenWindowAction", "OpenUIpanelWindow01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro01", "TimerTrigger01");
            _xml.SetTimerTriggerAction("DialogAction", "Intro02", "TimerTrigger01");
            _xml.SetTimerTriggerAction("ObjectActivationAction", "ActProximityTrigger01", "TimerTrigger01");

            _xml.SetAreaLandingTrigger("AreaLandingTrigger01", "Any", "False");
            _xml.SetSphereArea("SphereArea01", Constants.AirportAreaTriggerRadiusMetres.ToString());
            string dwp = ScenarioXML.GetCoordinateWorldPosition(formData.DestinationRunway.AirportLat, formData.DestinationRunway.AirportLon, formData.DestinationRunway.Altitude);
            AttachedWorldPosition adwp = ScenarioXML.GetAttachedWorldPosition(dwp, "False");
            _xml.SetAttachedWorldPosition("SphereArea", "SphereArea01", adwp);
            _xml.SetAreaLandingTriggerArea("SphereArea", "SphereArea01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", "CloseUIpanelWindow01", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("CloseWindowAction", "CloseUIpanelWindow02", "AreaLandingTrigger01");
            _xml.SetAreaLandingTriggerAction("GoalResolutionAction", "Goal01", "AreaLandingTrigger01");
            _xml.SetObjectActivationAction(1, "AreaLandingTrigger", "AreaLandingTrigger", "ActAreaLandingTrigger", "True");

            _xml.SetProximityTriggerOnEnterAction(1, "ObjectActivationAction", "ActAreaLandingTrigger", WikiCount - 2, "ProximityTrigger");
        }

        /// <summary>
        /// Generates the window parameters for the moving map display.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An array of parameter strings configuring the map window.</returns>
        internal static string[] GetMapWindowParameters(ScenarioFormData formData)
        {
            int mapWindowWidth = (int)formData.MapWindowSize;
            int mapWindowHeight = (int)formData.MapWindowSize;

            return ScenarioXML.GetWindowParameters(mapWindowWidth, mapWindowHeight, formData.MapAlignment,
                formData.MapMonitorWidth, formData.MapMonitorHeight, formData.MapOffset);
        }

        /// <summary>
        /// Generates the window parameters for the Wikipedia item display.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns>An array of parameter strings configuring the item window.</returns>
        internal static string[] GetWikiURLWindowParameters(ScenarioFormData formData)
        {
            return ScenarioXML.GetWindowParameters(formData.WikiURLWindowWidth, formData.WikiURLWindowHeight, formData.WikiURLAlignment,
                formData.WikiURLMonitorWidth, formData.WikiURLMonitorHeight, formData.WikiURLOffset);
        }

        /// <summary>
        /// Generates the XML world coordinate position string for a Wikipedia tour item.
        /// </summary>
        /// <param name="legNo">The leg number.</param>
        /// <param name="wikipedia">The Wikipedia tour service instance.</param>
        /// <returns>An XML world position coordinate string.</returns>
        internal static string GetWikiItemWorldPosition(int legNo, Wikipedia wikipedia)
        {
            return $"{wikipedia.WikiTour[legNo].latitude}, {wikipedia.WikiTour[legNo].longitude},+0.0";
        }

        #endregion

        /// <summary>
        /// Configures the Lua script action for incrementing the leg number in scenario XML.
        /// </summary>
        internal void SetWikiTourScriptActions()
        {
            string[] scripts =
            [
                "!lua local currentLegNo = varget(\"S:currentLegNo\", \"NUMBER\") " +
                "currentLegNo = currentLegNo + 1 varset(\"S:currentLegNo\", \"NUMBER\", currentLegNo)"
            ];

            _xml.SetScriptActions(scripts);
        }

        /// <summary>
        /// Prepares and writes the Wikipedia Item HTML file with configured dimensions.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if the asset file was written successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetWikiItemHTMLAsync(ScenarioFormData formData)
        {
            string ApplyHtmlReplacements(string content)
            {
                return content
                    .Replace("widthX", formData.WikiURLWindowWidth.ToString())
                    .Replace("heightX", (formData.WikiURLWindowHeight - 50).ToString());
            }

            return await _assetFileGenerator.WriteAssetFileAsync(
                resourceName: "HTML.WikipediaItem.html",
                fileName: "WikipediaItem.html",
                saveLocation: formData.ScenarioImageFolder,
                replacements: null,
                customLogic: ApplyHtmlReplacements);
        }

        /// <summary>
        /// Prepares and writes the Wikipedia Tour JavaScript file configuring URLs and links.
        /// </summary>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <returns><see langword="true"/> if the asset file was written successfully; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> SetWikiTourJSAsync(ScenarioFormData formData)
        {
            var relevantTourItems = WikiTour.Skip(1).Take(WikiCount - 2).ToList();

            if (relevantTourItems.Count == 0)
            {
                return false;
            }

            var lastItem = relevantTourItems[^1];

            var urlList = relevantTourItems
                .ConvertAll(item => $"\"{item.itemURL}\"")
;
            urlList.Add($"\"{lastItem.itemURL}\"");
            string itemURLs = string.Join(", ", urlList);

            var hrefList = relevantTourItems
                .ConvertAll(item => FormatHrefsForJs(item.hrefs))
;
            hrefList.Add(FormatHrefsForJs(lastItem.hrefs));
            string itemHREFs = string.Join(", ", hrefList);

            var replacements = new Dictionary<string, string>
            {
                { "itemURLsX", $"[{itemURLs}]" },
                { "itemHREFsX", $"[{itemHREFs}]" }
            };

            return await _assetFileGenerator.WriteAssetFileAsync(
                resourceName: "Javascript.scriptsWikipediaItem.js",
                fileName: "scriptsWikipediaItem.js",
                saveLocation: formData.ScenarioImageFolder,
                replacements: replacements);

            static string FormatHrefsForJs(List<string>? hrefs)
            {
                if (hrefs == null || hrefs.Count == 0)
                {
                    return "[]";
                }
                return "[" + string.Join(", ", hrefs.Select(h => $"\"{h}\"")) + "]";
            }
        }
    }
}