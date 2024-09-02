using CoordinateSharp;
using HtmlAgilityPack;
using System.Web;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Provides routines for the Wikipedia scenario type
    /// </summary>
    internal class Wikipedia()
    {
        internal static int WikiCount { get; private set; } // The wikipedia list items plus start and finish airports
        internal static int WikiDistance { get; private set; } // From start to finish airport
        internal static Params WikiStartAirport { get; private set; }
        internal static Params WikiFinishAirport { get; private set; }
        internal static List<List<List<string>>> wikiPage = []; // Table(s) of items scraped from user supplied Wikipedia URL
        internal static List<List<string>> wikiTour = []; // List of user selected Wikipedia items
        internal static int title = 0, link = 1, latitude = 2, longitude = 3; // Wikipedia item list indexes
        internal static int xAxis = 0, yAxis = 1; // Used in bounding box to denote lists that store OSM xTile and yTile reference numbers

        #region Populating wikiPage

        /// <summary>
        /// Parses user supplied URL for table(s) identified by class='sortable wikitable'.
        /// Using specified column extracts items that have a title and link. The link must
        /// supply latitude and longitude. Stores items in <see cref="wikiPage"/>.
        /// </summary>
        /// <param name="wikiURL">User supplied Wikipedia URL</param>
        /// <param name="columnNo">User supplied column number of items in table</param>
        static internal void PopulateWikiPage(string wikiURL, int columnNo)
        {
            string message = $"Reading {wikiURL} and column {columnNo}, will advise when complete";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            wikiPage.Clear();
            HtmlAgilityPack.HtmlDocument htmlDoc = HttpRoutines.GetWebDoc(wikiURL);
            HtmlNodeCollection tables = null;
            HtmlNodeCollection rows = null;
            HtmlNodeCollection cells = null;
            string tableSelection = "//table[@class='sortable wikitable' or @class='wikitable sortable']";
            if (htmlDoc != null && HttpRoutines.GetNodeCollection(htmlDoc.DocumentNode, ref tables, tableSelection, true))
            {
                foreach (var table in tables)
                {
                    List<List<string>> curTable = [];
                    if (HttpRoutines.GetNodeCollection(table, ref rows, ".//tr", true))
                    {
                        foreach (var row in rows)
                        {
                            if (HttpRoutines.GetNodeCollection(row, ref cells, ".//th | .//td", true) && cells.Count >= columnNo)
                            {
                                ReadWikiCell(cells[columnNo - 1], curTable);
                            }
                        }
                    }
                    if (curTable.Count > 0)
                    {
                        wikiPage.Add(curTable);
                    }
                }
            }
            message = $"Finished reading {wikiURL} and column {columnNo}.";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Stores one item in a table of <see cref="wikiPage"/>. Item includes a title, URL to Wikipedia item page 
        /// and latitude and longitude.
        /// </summary>
        /// <param name="cell">The cell in a table row containing item title and hyperlink</param>
        /// <param name="curTable">The current table being populated in <see cref="wikiPage"/></param>
        static internal void ReadWikiCell(HtmlNode cell, List<List<string>> curTable)
        {
            List<string> wikiItem = [];
            List<HtmlNode> cellDescendants = cell.Descendants("a").ToList();
            string title = "", link = "";
            if (cellDescendants.Count > 0)
            {
                title = cellDescendants[0].GetAttributeValue("title", "");
                link = cellDescendants[0].GetAttributeValue("href", "");
            }
            if (title != "" && link != "")
            {
                wikiItem.Add(HttpUtility.HtmlDecode(title));
                wikiItem.Add(link);
                if (GetWikiItemCoordinates(wikiItem))
                {
                    curTable.Add(wikiItem);
                }
            }
        }

        /// <summary>
        /// Checks that the item hyperlink is pointing to a page with lat/long coordinate in expected place
        /// and retrieves them for storage in a table in <see cref="wikiPage"/>.
        /// </summary>
        /// <param name="curRow">The current row in table being populated in <see cref="wikiPage"/></param>
        /// <returns></returns>
        static internal bool GetWikiItemCoordinates(List<string> curRow)
        {
            var htmlDoc = HttpRoutines.GetWebDoc($"https://en.wikipedia.org/{curRow[link]}");
            HtmlNodeCollection spans = null;
            if (htmlDoc != null && HttpRoutines.GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='latitude']", false))
            {
                if (spans != null && spans.Count > 0)
                {
                    curRow.Add(ConvertWikiCoOrd(spans[0].InnerText));
                    HttpRoutines.GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='longitude']", false);
                    curRow.Add(ConvertWikiCoOrd(spans[0].InnerText));
                    return true;
                }
            }
            return false;
        }

        // Convert Wikipedia coordinate format string to format that can be parsed by CoordinateSharp package
        static internal string ConvertWikiCoOrd(string wikiCoOrd)
        {
            // Insert space after degree symbol
            int degPos = wikiCoOrd.IndexOf('°');
            wikiCoOrd = wikiCoOrd.Insert(degPos + 1, " ");

            // Insert space after minute symbol
            int minPos = degPos + 2;
            while (Char.IsDigit(wikiCoOrd[minPos]) || wikiCoOrd[minPos] == '.')
            {
                minPos++;
            }
            wikiCoOrd = wikiCoOrd.Insert(minPos + 1, " ");

            // Copy last char N/S/E/W to front with space after it
            char final = wikiCoOrd[^1];
            wikiCoOrd = $"{final} {wikiCoOrd}";

            // Delete last char
            wikiCoOrd = wikiCoOrd.Remove(wikiCoOrd.Length - 1);

            return wikiCoOrd;
        }

        #endregion

        #region Form routines - populate UI, list of tables and route for selected table

        /// <summary>
        /// Creates a summary string for each table in <see cref="wikiPage"/> in the form:
        /// <para>[0] first item description ... [^1] last item description (number of items)</para>
        /// </summary>
        /// <returns>List of table summary strings</returns>
        static internal List<string> CreateWikiTablesDesc()
        {
            var list = new List<string>();
            string tableDesc;
            for (int tableNo = 0; tableNo < wikiPage.Count; tableNo++)
            {
                if (wikiPage[tableNo].Count == 1)
                {
                    tableDesc = $"{wikiPage[tableNo][0][title]} (one item)";
                }
                else 
                {
                    tableDesc = $"{wikiPage[tableNo][0][title]} ... {wikiPage[tableNo][^1][title]} ({wikiPage[tableNo].Count} items)";
                }
                list.Add(tableDesc);
            }
            return list;
        }

        /// <summary>
        /// Creates a route (non-optimal) for a table in <see cref="wikiPage"/> 
        /// </summary>
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <returns>List of route leg summary strings</returns>
        static internal List<string> CreateWikiTableRoute(int tableNo)
        {
            int[,] wikiTableCost = new int[wikiPage[tableNo].Count, wikiPage[tableNo].Count]; // Matrix of distances between items in miles
            List<string> route = []; // Route leg summary strings
            bool[] itemsVisited = new bool[wikiPage[tableNo].Count]; // Track addition of items to route as it's built
            int firstRouteItem = 0; // Track first item of route as it's built
            int lastRouteItem; // Track last item of route as it's built
            int itemVisitedCount; // Track how many items have been added to route as it's built

            SetWikiTableCosts(tableNo, wikiTableCost);

            // Initialise route with first leg (handles special case where only one item)
            itemsVisited[0] = true;
            itemVisitedCount = 1;
            lastRouteItem = GetNearesetWikiItem(0, wikiTableCost, itemsVisited);
            AddLegToRoute(route, tableNo, wikiTableCost, 0, firstRouteItem, lastRouteItem, itemsVisited, lastRouteItem, ref itemVisitedCount);

            while (itemVisitedCount < wikiPage[tableNo].Count)
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
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles</param>
        /// <param name="insertionPt">New leg is inserted at front or added to end of route</param>
        /// <param name="startItem">Start item for new leg</param>
        /// <param name="finishItem">Finish item for new leg</param>
        /// <param name="itemsVisited">Tracks addition of items to route as it's built</param>
        /// <param name="newItem">Will be either the startItem or finishItem depending on which end of route</param>
        /// <param name="itemVisitedCount">Tracks how many items have been added to route as it's built</param>
        static internal void AddLegToRoute(List<string> route, int tableNo, int[,] wikiTableCost, int insertionPt, 
            int startItem, int finishItem, bool[] itemsVisited, int newItem, ref int itemVisitedCount)
        {
            if (insertionPt > route.Count - 1)
            {
                route.Add($"[{startItem}] {wikiPage[tableNo][startItem][title]} ... [{finishItem}] {wikiPage[tableNo][finishItem][title]} " +
                    $"({wikiTableCost[startItem, finishItem]} miles)");
            }
            else
            {
                route.Insert(insertionPt, $"[{startItem}] {wikiPage[tableNo][startItem][title]} ... " +
                    $"[{finishItem}] {wikiPage[tableNo][finishItem][title]} ({wikiTableCost[startItem, finishItem]} miles)");
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
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="wikiTableCost">Matrix of distances between items in miles</param>
        static internal void SetWikiTableCosts(int tableNo, int[,] wikiTableCost)
        {
            for (int row = 0; row < wikiPage[tableNo].Count; row++)
            {
                for (int col = 0; col < wikiPage[tableNo].Count; col++)
                {
                    Coordinate coord1 = Coordinate.Parse($"{wikiPage[tableNo][row][latitude]} {wikiPage[tableNo][row][longitude]}");
                    Coordinate coord2 = Coordinate.Parse($"{wikiPage[tableNo][col][latitude]} {wikiPage[tableNo][col][longitude]}");
                    wikiTableCost[row, col] = (int)coord1.Get_Distance_From_Coordinate(coord2).Miles;
                }
            }
        }

        #endregion

        #region Populating wikiTour

        /// <summary>
        /// Populates wikiTour for selected table based on user specified start and finish items and current route
        /// </summary>
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <param name="tourDistance">The distance from first to last item in miles</param>
        static internal void PopulateWikiTour(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem, string tourDistance)
        {
            wikiTour.Clear();
            bool finished = PopulateWikiTourOneItem(tableNo, route, tourStartItem, tourFinishItem);
            if (!finished)
            {
                PopulateWikiTourMultipleItems(tableNo, route, tourStartItem, tourFinishItem);
            }
            WikiCount = wikiTour.Count + 2; // Wiki tour items plus two airports
            WikiDistance = int.Parse(tourDistance.Split(' ')[0]);
        }

        /// <summary>
        /// Handles case where user has selected a single item.
        /// </summary>
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <returns>True if this case applies</returns>
        static internal bool PopulateWikiTourOneItem(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem)
        {
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(tourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(tourFinishItem.ToString());
            if (tourStartItemNo == tourFinishItemNo)
            {
                wikiTour.Add(SetWikiItem(tableNo, tourStartItemNo));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles case where user has selected two or more items.
        /// </summary>
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <returns>True if this case applies</returns>
        static internal bool PopulateWikiTourMultipleItems(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem)
        {
            wikiTour.Clear();
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(tourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(tourFinishItem.ToString());
            int legStartItemNo, legFinishItemNo, startLegNo = 0;
            var routeLegs = route.GetEnumerator();

            // Find tourStartItemNo in route
            for (int legNo = 0; legNo < route.Count; legNo++)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                if (tourStartItemNo == legStartItemNo)
                {
                    wikiTour.Add(SetWikiItem(tableNo, tourStartItemNo));
                    startLegNo = legNo;
                    if (tourFinishItemNo == legFinishItemNo)
                    {
                        wikiTour.Add(SetWikiItem(tableNo, tourFinishItemNo)); // tourStartItemNo and tourFinishItemNo were in same leg
                        return false;
                    }
                    break;
                }
            }

            // Add legStartItemNo's until tourFinishItemNo == legFinishItemNo then add legFinishItemNo
            for (int legNo = startLegNo; legNo < route.Count; legNo++)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                wikiTour.Add(SetWikiItem(tableNo, legStartItemNo));
                if (tourFinishItemNo == legFinishItemNo)
                {
                    wikiTour.Add(SetWikiItem(tableNo, tourFinishItemNo));
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
            stringEnd = routeLeg.IndexOf('(') - 1;
            return GetWikiRouteLegFirstItemNo(routeLeg[stringBegin..stringEnd]);
        }
        /// <summary>
        /// Populates an item with title, link, latitude and longitude
        /// </summary>
        /// <param name="tableNo">The table in <see cref="wikiPage"/></param>
        /// <param name="itemNo">The item no reference in <see cref="wikiPage"/></param>
        /// <returns>A populated item</returns>
        static internal List<string> SetWikiItem(int tableNo, int itemNo)
        {
            List<string> wikiItem = [];
            wikiItem.Add(wikiPage[tableNo][itemNo][title]);
            wikiItem.Add(wikiPage[tableNo][itemNo][link]);
            wikiItem.Add(wikiPage[tableNo][itemNo][latitude]);
            wikiItem.Add(wikiPage[tableNo][itemNo][longitude]);
            return wikiItem;
        }

        #endregion

        #region SetWikiTour region

        static internal void SetWikiTour()
        {
            SetWikiAirports();
            SetWikiOSMImages();
        }

        static internal void SetWikiAirports()
        {
            Coordinate coordFirstItem = Coordinate.Parse($"{wikiTour[0][latitude]} {wikiTour[0][longitude]}");
            WikiStartAirport = Runway.GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble());
            Coordinate coordStartAirport = Coordinate.Parse($"{WikiStartAirport.AirportLat} {WikiStartAirport.AirportLon}");
            WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;

            Coordinate coordLastItem = Coordinate.Parse($"{wikiTour[^1][latitude]} {wikiTour[^1][longitude]}");
            WikiFinishAirport = Runway.GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble());
            Coordinate coordFinishAirport = Coordinate.Parse($"{WikiFinishAirport.AirportLat} {WikiFinishAirport.AirportLon}");
            WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
        }

        static internal void SetWikiOSMImages()
        {
            SetWikiOverviewImage();
            SetWikiLegRoutesImages();
        }

        static internal void SetWikiOverviewImage()
        {
            List<List<int>> tiles = [];
            List<List<int>> boundingBox = [];
            int zoom = GetBoundingBoxZoom(tiles, 2, 2, 0, wikiTour.Count - 1, true, true);
            SetWikiOSMtiles(tiles, zoom, 0, wikiTour.Count - 1, true, true);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, 2);
            Drawing.ConvertImageformat("Charts_01", "png", "jpg");
        }

        static internal void SetWikiLegRoutesImages()
        {
            // First leg is from start airport to first item
            SetWikiLegRouteImages(0, 0, true, false);

            // Middle legs which may be zero if only one item
            for (int itemNo = 0; itemNo <= wikiTour.Count - 2; itemNo++)
            {
                SetWikiLegRouteImages(itemNo, itemNo + 1, false, false);
            }

            // Last leg is from last item to finish airport
            SetWikiLegRouteImages(wikiTour.Count - 1, wikiTour.Count - 1, false, true);
        }

        static internal void SetWikiLegRouteImages(int startItemNo, int finishItemNo, bool incStartAirport, bool incFinishAirport)
        {
            List<List<int>> tiles = [];
            List<List<int>> boundingBox = [];

            int zoom = GetBoundingBoxZoom(tiles, 2, 2, startItemNo, finishItemNo, incStartAirport, incFinishAirport);
            SetWikiOSMtiles(tiles, zoom, startItemNo, finishItemNo, incStartAirport, incFinishAirport);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            int legNo = 1;
            if (!incStartAirport)
            {
                legNo = startItemNo + 2;
            }
            Drawing.MontageTiles(boundingBox, zoom, $"LegRoute_{legNo:00}");
            Drawing.DrawRoute(tiles, boundingBox, $"LegRoute_{legNo:00}");
            Drawing.MakeSquare(boundingBox, $"LegRoute_{legNo:00}", zoom, 2);
            Drawing.ConvertImageformat($"LegRoute_{legNo:00}", "png", "jpg");
        }

        static internal int GetBoundingBoxZoom(List<List<int>> tiles, int tilesWidth, int tilesHeight, 
            int startItemNo, int finishItemNo, bool incStartAirport, bool incFinishAirport)
        {
            List<List<int>> boundingBox = [];
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetWikiOSMtiles(tiles, zoom, startItemNo, finishItemNo, incStartAirport, incFinishAirport);
                boundingBox.Clear();
                OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
                if ((boundingBox[xAxis].Count > tilesWidth) || (boundingBox[yAxis].Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        // Finds OSM tile numbers and offsets for a Wiki list (all items plus airports, or a pair of items) 
        static internal void SetWikiOSMtiles(List<List<int>> tiles, int zoom, int startItemNo, int finishItemNo, 
            bool incStartAirport, bool incFinishAirport)
        {
            tiles.Clear();
            if (incStartAirport)
            {
                tiles.Add(OSM.GetOSMtile(WikiStartAirport.AirportLon.ToString(), WikiStartAirport.AirportLat.ToString(), zoom));
            }
            for (int itemNo = startItemNo; itemNo <= finishItemNo; itemNo++)
            {
                tiles.Add(OSM.GetOSMtile(wikiTour[itemNo][longitude], wikiTour[itemNo][latitude], zoom));
            }
            if (incFinishAirport)
            {
                tiles.Add(OSM.GetOSMtile(WikiFinishAirport.AirportLon.ToString(), WikiFinishAirport.AirportLat.ToString(), zoom));
            }
        }

        #endregion
    }
}
