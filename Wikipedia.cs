using CoordinateSharp;
using HtmlAgilityPack;
using System.Web;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Stores information pertaining to a Wikipedia item in the Wikipedia list tour, also used for start and destination airports
    /// </summary>
    public class WikiItemParams
    {
        /// <summary>
        /// Wiki item HTML title tag value
        /// </summary>
        public string title;

        /// <summary>
        /// Wiki item page URL
        /// </summary>
        public string itemURL;

        /// <summary>
        /// Latitude for this Wiki item
        /// </summary>
        public string latitude;

        /// <summary>
        /// Longitude for this Wiki item
        /// </summary>
        public string longitude;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportICAO;

        /// <summary>
        /// Only used for start and destination airport instances
        /// </summary>
        public string airportID;

        /// <summary>
        /// Was to be used for navigating Wiki item html document
        /// </summary>
        public List<string> hrefs;     
    }

    /// <summary>
    /// Provides routines for the Wikipedia scenario type
    /// </summary>
    internal class Wikipedia()
    {
        /// <summary>
        /// The wikipedia list items plus start and finish airports
        /// </summary>
        internal static int WikiCount { get; private set; }

        /// <summary>
        /// From start to finish airport
        /// </summary>
        internal static int WikiDistance { get; private set; }

        /// <summary>
        /// Lat/Lon boundaries for each OSM montage leg image
        /// </summary>
        internal static List<MapEdges> WikiLegMapEdges { get; private set; }

        /// <summary>
        /// Table(s) of items scraped from user supplied Wikipedia URL
        /// </summary>
        internal static List<List<WikiItemParams>> WikiPage { get; private set; }

        /// <summary>
        /// List of user selected Wikipedia items
        /// </summary>
        internal static List<WikiItemParams> WikiTour { get; private set; } 

        #region Populating WikiPage when user pastes in Wikipedia URL, called from main form

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
                    List<WikiItemParams> curTable = [];
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
        static internal void ReadWikiCell(HtmlNode cell, List<WikiItemParams> curTable)
        {
            WikiItemParams wikiItem = new();
            List<HtmlNode> cellDescendants = cell.Descendants("a").ToList();
            string title = "", link = "";
            if (cellDescendants.Count > 0)
            {
                title = cellDescendants[0].GetAttributeValue("title", "");
                link = cellDescendants[0].GetAttributeValue("href", "");
            }
            if (title != "" && link != "")
            {
                wikiItem.title = HttpUtility.HtmlDecode(title);
                wikiItem.itemURL = link;
                if (GetWikiItemCoordinates(wikiItem))
                {
                    wikiItem.hrefs = GetWikiItemHREFs(wikiItem);
                    curTable.Add(wikiItem);
                }
            }
        }

        /// <summary>
        /// Retrieves any href="# links in the item. These are used to allow user to step through sections
        /// of the item page using joystick mapped buttons as an alternative to scrolling with a mouse.
        /// </summary>
        /// <param name="wikiItem">The current row in table being populated in <see cref="WikiPage"/></param>
        static internal List<string> GetWikiItemHREFs(WikiItemParams wikiItem)
        {
            var htmlDoc = HttpRoutines.GetWebDoc($"https://en.wikipedia.org/{wikiItem.itemURL}");
            string htmlDocContents = htmlDoc.Text;
            int indexSearchFrom = 0;
            string hrefTag = "href=\"#";
            List<string> hrefs = [];
            int indexHREFtagStart = htmlDocContents.IndexOf(hrefTag, indexSearchFrom);
            while (indexHREFtagStart >= 0)
            {
                int indexHREFvalueStart = indexHREFtagStart + hrefTag.Length;
                int indexHREFvalueFinish = htmlDocContents.IndexOf('\"', indexHREFvalueStart);
                string hrefValue = htmlDocContents[indexHREFvalueStart..indexHREFvalueFinish];
                if (hrefValue.Length > 0 && !hrefValue.Contains("cite", StringComparison.OrdinalIgnoreCase))
                {
                    hrefs.Add(hrefValue);
                }
                indexSearchFrom = indexHREFvalueFinish + 1;
                indexHREFtagStart = htmlDocContents.IndexOf(hrefTag, indexSearchFrom);
            }
            return hrefs;
        }

        /// <summary>
        /// Checks that the item hyperlink is pointing to a page with lat/long coordinate in expected place
        /// and retrieves them for storage in a table in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="wikiItem">The current row in table being populated in <see cref="WikiPage"/></param>
        /// <returns></returns>
        static internal bool GetWikiItemCoordinates(WikiItemParams wikiItem)
        {
            var htmlDoc = HttpRoutines.GetWebDoc($"https://en.wikipedia.org/{wikiItem.itemURL}");
            HtmlNodeCollection spans = null;
            if (htmlDoc != null && HttpRoutines.GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='latitude']", false))
            {
                if (spans != null && spans.Count > 0)
                {
                    wikiItem.latitude = ConvertWikiCoOrd(spans[0].InnerText);
                    HttpRoutines.GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='longitude']", false);
                    wikiItem.longitude = ConvertWikiCoOrd(spans[0].InnerText);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convert Wikipedia coordinate format string to format that can be parsed by CoordinateSharp package
        /// </summary>
        /// <param name="wikiCoOrd">Wikipedia coordinate format string</param>
        /// <returns>CoordinateSharp package format readable string</returns>
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

        #region Form routines - populate UI, list of tables and route for selected table including all valid items

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
        static internal void SetWikiTableCosts(int tableNo, int[,] wikiTableCost)
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
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <param name="tourDistance">The distance from first to last item in miles</param>
        static internal void SetWikiTour(int tableNo, ComboBox.ObjectCollection route, object tourStartItem, object tourFinishItem, string tourDistance)
        {
            PopulateWikiTour(tableNo, route, tourStartItem, tourFinishItem, tourDistance);
            SetWikiAirports();
            Common.SetOverviewImage();
            Common.SetLocationImage();
            WikiLegMapEdges = [];
            Common.SetAllLegRouteImages(0, WikiTour.Count - 2);
        }

        /// <summary>
        /// Finds and inserts/appends wiki tour start and finish airports. Adjusts <see cref="WikiDistance"/> 
        /// to include airport legs
        /// </summary>
        static internal void SetWikiAirports()
        {
            Coordinate coordFirstItem = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
            WikiTour.Insert(0, GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble()));
            Coordinate coordStartAirport = Coordinate.Parse($"{WikiTour[0].latitude} {WikiTour[0].longitude}");
            WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;
            Runway.SetRunway(Runway.startRwy, WikiTour[0].airportICAO, WikiTour[0].airportID);

            Coordinate coordLastItem = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
            WikiTour.Add(GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble()));
            Coordinate coordFinishAirport = Coordinate.Parse($"{WikiTour[^1].latitude} {WikiTour[^1].longitude}");
            WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
            Runway.SetRunway(Runway.destRwy, WikiTour[^1].airportICAO, WikiTour[^1].airportID);
        }

        /// <summary>
        /// Calls GetNearestAirport method in Runway class to look for closest airport. Populates an instance of WikiItemParams 
        /// with the airport information
        /// </summary>
        /// <param name="queryLat">The wiki item latitude</param>
        /// <param name="queryLon">The wiki item longitude</param>
        /// <returns></returns>
        static internal WikiItemParams GetNearestAirport(double queryLat, double queryLon)
        {
            WikiItemParams wikiItemParams = new();
            RunwayParams nearestAirport = Runway.GetNearestAirport(queryLat, queryLon);
            if (nearestAirport == null)
                return null;
            wikiItemParams.airportICAO = nearestAirport.IcaoId;
            wikiItemParams.airportID = nearestAirport.Id;
            wikiItemParams.latitude = nearestAirport.AirportLat.ToString();
            wikiItemParams.longitude = nearestAirport.AirportLon.ToString();
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
                tiles.Add(OSM.GetOSMtile(WikiTour[itemNo].longitude, WikiTour[itemNo].latitude, zoom));
            }
        }

        #endregion

        #region Populating WikiTour

        /// <summary>
        /// Populates WikiTour for selected table based on user specified start and finish items and current route
        /// </summary>
        /// <param name="tableNo">The table in <see cref="WikiPage"/></param>
        /// <param name="route">Route leg summary strings</param>
        /// <param name="tourStartItem">User specified first item of tour</param>
        /// <param name="tourFinishItem">User specified last item of tour</param>
        /// <param name="tourDistance">The distance from first to last item in miles</param>
        static internal void PopulateWikiTour(int tableNo, ComboBox.ObjectCollection route, object tourStartItem, object tourFinishItem, string tourDistance)
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
        static internal bool PopulateWikiTourMultipleItems(int tableNo, ComboBox.ObjectCollection route, object tourStartItem, object tourFinishItem)
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
        static internal WikiItemParams SetWikiItem(int tableNo, int itemNo)
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
    }
}
