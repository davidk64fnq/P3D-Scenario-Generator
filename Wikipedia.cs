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
        internal static List<List<double>> WikiLegMapEdges { get; private set; } // Lat/Lon boundaries for each OSM montage leg image
        internal static List<List<List<string>>> WikiPage { get; private set; } // Table(s) of items scraped from user supplied Wikipedia URL
        internal static List<List<string>> WikiTour { get; private set; } // List of user selected Wikipedia items

        internal static int title = 0, link = 1, latitude = 2, longitude = 3; // Wikipedia item list indexes
        internal static int xAxis = 0, yAxis = 1; // Used in bounding box to denote lists that store OSM xTile and yTile reference numbers

        #region Populating WikiPage

        /// <summary>
        /// Parses user supplied URL for table(s) identified by class='sortable wikitable'.
        /// Using specified column extracts items that have a title and link. The link must
        /// supply latitude and longitude. Stores items in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="wikiURL">User supplied Wikipedia URL</param>
        /// <param name="columnNo">User supplied column number of items in table</param>
        static internal void PopulateWikiPage(string wikiURL, int columnNo)
        {
            string message = $"Reading {wikiURL} and column {columnNo}, will advise when complete";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            WikiPage = [];
            HtmlAgilityPack.HtmlDocument htmlDoc = HttpRoutines.GetWebDoc(wikiURL);
            HtmlNodeCollection tables = null;
            HtmlNodeCollection rows = null;
            HtmlNodeCollection cells = null;
            string tableSelection = "//table[contains(@class, 'sortable wikitable') or contains(@class, 'wikitable sortable')]";
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
                        WikiPage.Add(curTable);
                    }
                }
            }
            message = $"Finished reading {wikiURL} and column {columnNo}.";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Stores one item in a table of <see cref="WikiPage"/>. Item includes a title, URL to Wikipedia item page 
        /// and latitude and longitude.
        /// </summary>
        /// <param name="cell">The cell in a table row containing item title and hyperlink</param>
        /// <param name="curTable">The current table being populated in <see cref="WikiPage"/></param>
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
        /// and retrieves them for storage in a table in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="curRow">The current row in table being populated in <see cref="WikiPage"/></param>
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
        /// Creates a summary string for each table in <see cref="WikiPage"/> in the form:
        /// <para>[0] first item description ... [^1] last item description (number of items)</para>
        /// </summary>
        /// <returns>List of table summary strings</returns>
        static internal List<string> CreateWikiTablesDesc()
        {
            var list = new List<string>();
            string tableDesc;
            for (int tableNo = 0; tableNo < WikiPage.Count; tableNo++)
            {
                if (WikiPage[tableNo].Count == 1)
                {
                    tableDesc = $"{WikiPage[tableNo][0][title]} (one item)";
                }
                else 
                {
                    tableDesc = $"{WikiPage[tableNo][0][title]} ... {WikiPage[tableNo][^1][title]} ({WikiPage[tableNo].Count} items)";
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
        static internal List<string> CreateWikiTableRoute(int tableNo)
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
        static internal void AddLegToRoute(List<string> route, int tableNo, int[,] wikiTableCost, int insertionPt, 
            int startItem, int finishItem, bool[] itemsVisited, int newItem, ref int itemVisitedCount)
        {
            if (insertionPt > route.Count - 1)
            {
                route.Add($"[{startItem}] {WikiPage[tableNo][startItem][title]} ... [{finishItem}] {WikiPage[tableNo][finishItem][title]} " +
                    $"({wikiTableCost[startItem, finishItem]} miles)");
            }
            else
            {
                route.Insert(insertionPt, $"[{startItem}] {WikiPage[tableNo][startItem][title]} ... " +
                    $"[{finishItem}] {WikiPage[tableNo][finishItem][title]} ({wikiTableCost[startItem, finishItem]} miles)");
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
        static internal void SetWikiTableCosts(int tableNo, int[,] wikiTableCost)
        {
            for (int row = 0; row < WikiPage[tableNo].Count; row++)
            {
                for (int col = 0; col < WikiPage[tableNo].Count; col++)
                {
                    Coordinate coord1 = Coordinate.Parse($"{WikiPage[tableNo][row][latitude]} {WikiPage[tableNo][row][longitude]}");
                    Coordinate coord2 = Coordinate.Parse($"{WikiPage[tableNo][col][latitude]} {WikiPage[tableNo][col][longitude]}");
                    wikiTableCost[row, col] = (int)coord1.Get_Distance_From_Coordinate(coord2).Miles;
                }
            }
        }

        #endregion

        #region SetWikiTour region

        static internal void SetWikiTour(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem, string tourDistance)
        {
            PopulateWikiTour(tableNo, route, tourStartItem, tourFinishItem, tourDistance);
            SetWikiAirports();
            SetWikiOSMImages();
        }

        /// <summary>
        /// Finds and sets <see cref="WikiStartAirport"/> and <see cref="WikiFinishAirport"/>. Adjusts <see cref="WikiDistance"/> 
        /// to include airport legs
        /// </summary>
        static internal void SetWikiAirports()
        {
            Coordinate coordFirstItem = Coordinate.Parse($"{WikiTour[0][latitude]} {WikiTour[0][longitude]}");
            WikiStartAirport = Runway.GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble());
            Coordinate coordStartAirport = Coordinate.Parse($"{WikiStartAirport.AirportLat} {WikiStartAirport.AirportLon}");
            WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;

            Coordinate coordLastItem = Coordinate.Parse($"{WikiTour[^1][latitude]} {WikiTour[^1][longitude]}");
            WikiFinishAirport = Runway.GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble());
            Coordinate coordFinishAirport = Coordinate.Parse($"{WikiFinishAirport.AirportLat} {WikiFinishAirport.AirportLon}");
            WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
        }

        static internal void SetWikiOSMImages()
        {
            SetWikiOverviewImage();
            SetWikiLocationImage();
            SetAllWikiLegRouteImages();
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers both airports and all items in <see cref="WikiTour"/>
        /// </summary>
        static internal void SetWikiOverviewImage()
        {
            List<List<int>> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            List<List<int>> boundingBox = []; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = GetBoundingBoxZoom(tiles, 2, 2, 0, WikiTour.Count - 1, true, true);
            SetWikiOSMtiles(tiles, zoom, 0, WikiTour.Count - 1, true, true);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, 2);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetWikiLocationImage()
        {
            List<List<int>> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            List<List<int>> boundingBox = []; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = 15;
            SetWikiOSMtiles(tiles, zoom, 0, -1, true, false);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox[xAxis].Count != boundingBox[yAxis].Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, 2);
            }
            if (boundingBox[xAxis].Count == 2)
            {
                Drawing.Resize("chart_thumb", 256);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all items specified by startItemIndex and finishItemIndex, 
        /// where the montage of OSM tiles doesn't exceed tilesWidth and tilesHeight in size
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for x axis</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for x axis</param>
        /// <param name="startItemIndex">Index of start item in <see cref="WikiTour"/></param>
        /// <param name="finishItemIndex">Index of finish item in <see cref="WikiTour"/></param>
        /// <param name="incStartAirport">Whether to include <see cref="WikiStartAirport"/></param>
        /// <param name="incFinishAirport">Whether to include <see cref="WikiFinishAirport"/></param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<List<int>> tiles, int tilesWidth, int tilesHeight,
            int startItemIndex, int finishItemIndex, bool incStartAirport, bool incFinishAirport)
        {
            List<List<int>> boundingBox = [];
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetWikiOSMtiles(tiles, zoom, startItemIndex, finishItemIndex, incStartAirport, incFinishAirport);
                boundingBox.Clear();
                OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
                if ((boundingBox[xAxis].Count > tilesWidth) || (boundingBox[yAxis].Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a <see cref="WikiTour"/> (all items plus airports, or a pair of items)
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of start item in <see cref="WikiTour"/></param>
        /// <param name="finishItemIndex">Index of finish item in <see cref="WikiTour"/></param>
        /// <param name="incStartAirport">Whether to include <see cref="WikiStartAirport"/></param>
        /// <param name="incFinishAirport">Whether to include <see cref="WikiFinishAirport"/></param>
        static internal void SetWikiOSMtiles(List<List<int>> tiles, int zoom, int startItemIndex, int finishItemIndex,
            bool incStartAirport, bool incFinishAirport)
        {
            tiles.Clear();
            if (incStartAirport)
            {
                tiles.Add(OSM.GetOSMtile(WikiStartAirport.AirportLon.ToString(), WikiStartAirport.AirportLat.ToString(), zoom));
            }
            for (int itemNo = startItemIndex; itemNo <= finishItemIndex; itemNo++)
            {
                tiles.Add(OSM.GetOSMtile(WikiTour[itemNo][longitude], WikiTour[itemNo][latitude], zoom));
            }
            if (incFinishAirport)
            {
                tiles.Add(OSM.GetOSMtile(WikiFinishAirport.AirportLon.ToString(), WikiFinishAirport.AirportLat.ToString(), zoom));
            }
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for all legs using a montage of OSM tiles that covers the start and finish leg items
        /// </summary>
        static internal void SetAllWikiLegRouteImages()
        {
            WikiLegMapEdges = [];
            // First leg is from start airport to first item
            SetOneWikiLegRouteImages(0, 0, true, false);

            // Middle legs which may be zero if only one item
            for (int itemNo = 0; itemNo <= WikiTour.Count - 2; itemNo++)
            {
                SetOneWikiLegRouteImages(itemNo, itemNo + 1, false, false);
            }

            // Last leg is from last item to finish airport
            SetOneWikiLegRouteImages(WikiTour.Count - 1, WikiTour.Count - 1, false, true);
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for one leg using a montage of OSM tiles that covers the start and finish leg items 
        /// </summary>
        /// <param name="startItemIndex">Index of start item in <see cref="WikiTour"/></param>
        /// <param name="finishItemIndex">Index of finish item in <see cref="WikiTour"/></param>
        /// <param name="incStartAirport">Whether to include <see cref="WikiStartAirport"/></param>
        /// <param name="incFinishAirport">Whether to include <see cref="WikiFinishAirport"/></param>
        static internal void SetOneWikiLegRouteImages(int startItemIndex, int finishItemIndex, bool incStartAirport, bool incFinishAirport)
        {
            List<List<int>> tiles = [];
            List<List<int>> boundingBox = [];
            List<List<int>> zoomInBoundingBox;

            int zoom = GetBoundingBoxZoom(tiles, 2, 2, startItemIndex, finishItemIndex, incStartAirport, incFinishAirport);
            SetWikiOSMtiles(tiles, zoom, startItemIndex, finishItemIndex, incStartAirport, incFinishAirport);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            int legNo = 1;
            if (!incStartAirport)
            {
                legNo = startItemIndex + 2; // start airport leg is 1, next leg is 2 (startItemIndex = 0 + 2)
            }
            Drawing.MontageTiles(boundingBox, zoom, $"LegRoute_{legNo:00}_zoom1");
            Drawing.DrawRoute(tiles, boundingBox, $"LegRoute_{legNo:00}_zoom1");
            zoomInBoundingBox = Drawing.MakeSquare(boundingBox, $"LegRoute_{legNo:00}_zoom1", zoom, 2);
            Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom1", "png", "jpg");

            for (int inc = 1; inc <= 2; inc++)
            {
                SetWikiOSMtiles(tiles, zoom + inc, startItemIndex, finishItemIndex, incStartAirport, incFinishAirport);
                Drawing.MontageTiles(zoomInBoundingBox, zoom + inc, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                Drawing.DrawRoute(tiles, zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                zoomInBoundingBox = Drawing.MakeSquare(zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}", zoom + inc, (int)Math.Pow(2, inc + 1));
                Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom{inc + 1}", "png", "jpg");
            }

            SetLegImageBoundaries(zoomInBoundingBox, zoom + 3);
        }

        /// <summary>
        /// Calculates leg map imageURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
        /// </summary>
        /// <param name="legNo">Leg numbers run from 0</param>
        /// <param name="boundingBox">The OSM tile numbers for x and y axis that cover the set of coordinates depicted in an image</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        static internal void SetLegImageBoundaries(List<List<int>> boundingBox, int zoom)
        {
            List<double> legEdges = new(new double[4]);
            int north = 0, east = 1, south = 2, west = 3; // Used with WikiLegMapEdges to identify leg boundaries
            List<double> latLonList;
            int latitude = 0, longitude = 1;

            // Get the lat/lon coordinates of top left corner of bounding box
            latLonList = OSM.TileNoToLatLon(boundingBox[xAxis][0], boundingBox[yAxis][0], zoom);
            legEdges[north] = latLonList[latitude];
            legEdges[west] = latLonList[longitude];

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            latLonList = OSM.TileNoToLatLon(boundingBox[xAxis][^1] + 1, boundingBox[yAxis][^1] + 1, zoom);
            legEdges[south] = latLonList[latitude];
            legEdges[east] = latLonList[longitude];

            // Assumes this method called in leg number sequence starting with first leg
            WikiLegMapEdges.Add(legEdges);
        }

        #endregion

        #region Populating wikiTour

        /// <summary>
        /// Populates WikiTour for selected table based on user specified start and finish items and current route
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <param name="tourDistance">The distance from first to last item in miles</param>
        static internal void PopulateWikiTour(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem, string tourDistance)
        {
            WikiTour = [];
            bool finished = PopulateWikiTourOneItem(tableNo, tourStartItem, tourFinishItem);
            if (!finished)
            {
                PopulateWikiTourMultipleItems(tableNo, route, tourStartItem, tourFinishItem);
            }
            WikiCount = WikiTour.Count + 2; // Wiki tour items plus two airports
            WikiDistance = int.Parse(tourDistance.Split(' ')[0]);
        }

        /// <summary>
        /// Handles case where user has selected a single item.
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <returns>True if this case applies</returns>
        static internal bool PopulateWikiTourOneItem(int tableNo, object tourStartItem, object tourFinishItem)
        {
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(tourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(tourFinishItem.ToString());
            if (tourStartItemNo == tourFinishItemNo)
            {
                WikiTour.Add(SetWikiItem(tableNo, tourStartItemNo));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles case where user has selected two or more items.
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <returns>True if this case applies</returns>
        static internal bool PopulateWikiTourMultipleItems(int tableNo, ListBox.ObjectCollection route, object tourStartItem, object tourFinishItem)
        {
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
                    WikiTour.Add(SetWikiItem(tableNo, tourStartItemNo));
                    startLegNo = legNo;
                    if (tourFinishItemNo == legFinishItemNo)
                    {
                        WikiTour.Add(SetWikiItem(tableNo, tourFinishItemNo)); // tourStartItemNo and tourFinishItemNo were in same leg
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
                WikiTour.Add(SetWikiItem(tableNo, legStartItemNo));
                if (tourFinishItemNo == legFinishItemNo)
                {
                    WikiTour.Add(SetWikiItem(tableNo, tourFinishItemNo));
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
        static internal List<string> SetWikiItem(int tableNo, int itemNo)
        {
            List<string> wikiItem = [];
            wikiItem.Add(WikiPage[tableNo][itemNo][title]);
            wikiItem.Add(WikiPage[tableNo][itemNo][link]);
            wikiItem.Add(WikiPage[tableNo][itemNo][latitude]);
            wikiItem.Add(WikiPage[tableNo][itemNo][longitude]);
            return wikiItem;
        }

        #endregion
    }
}
