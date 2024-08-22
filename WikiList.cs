using CoordinateSharp;
using System.Web;

namespace P3D_Scenario_Generator
{
    public class WikiListParams
    {
        
    }
    internal class WikiList()
    {
        internal static int WikiCount { get; private set; }
        internal static int WikiDistance { get; private set; }
        internal static Params WikiStartAirport { get; private set; }
        internal static Params WikiFinishAirport { get; private set; }
        internal static List<List<List<string>>> wikiPage = [];
        internal static int title = 0, link = 1, latitude = 2, longitude = 3;
        internal static int xAxis = 0, yAxis = 1;
        internal static List<List<string>> wikiTourList = [];

        // tableRoute is a list of legs to get from first item in table to last item in table, each leg is a string containing start and end items,
        // the start and end items are only the same in the special case of a one item table, the number of legs in tableRoute is always
        // one less than the the number of table items (except where only one item).
        // tourStartItem is the first item of user specified subset of items
        // tourFinishItem is the last item of user specified subset of items
        // To get a list of items corresponding to the user specified subset of items, step through the tableRoute legs and:
        // 1. Find the first leg that has the tourStartItem as first leg item or tourFinishItem as last leg item
        // 2. If tourStartItem is the same as tourFinishItem tour list is complete - case where tour consists of one item only
        // 3. If first leg finish item is the same as tourFinishItem tour list is complete - case where tour consists of two items only
        // 4. For subsequent legs add leg start item to tour list, then:
        // 5. Check whether leg finish item is the same as tourFinishItem, if so, add leg finish item to the tour list. Tour list is complete.
        static internal void SetWikiTourList(int tableNo, ListBox.ObjectCollection tableRoute, object tourStartItem, object tourFinishItem, string tourDistance)
        {
            int tourStartItemNo = GetWikiRouteLegFirstItemNo(tourStartItem.ToString());
            int tourFinishItemNo = GetWikiRouteLegFirstItemNo(tourFinishItem.ToString());
            var routeLegs = tableRoute.GetEnumerator();
            int legStartItemNo, legFinishItemNo;
            wikiTourList.Clear();
            bool checkNextLeg = true;
            // Steps 1 and 2 and 3
            int legCount = 1;
            if (wikiPage[tableNo].Count > 1)
            {
                legCount = wikiPage[tableNo].Count - 1;
            }
            for (int legNo = 0; legNo < legCount; legNo++)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                if (legStartItemNo == tourStartItemNo || legFinishItemNo == tourFinishItemNo) // Step 1
                {
                    if (tourStartItemNo == tourFinishItemNo && tourStartItemNo == legStartItemNo) // Step 2 - tour of one item, leg start
                    {
                        wikiTourList.Add(SetWikiItem(tableNo, legStartItemNo));
                        checkNextLeg = false;
                    } 
                    else if (tourStartItemNo == tourFinishItemNo && tourStartItemNo == legFinishItemNo) // Step 2 - tour of one item, leg finish
                    {
                        wikiTourList.Add(SetWikiItem(tableNo, legFinishItemNo));
                        checkNextLeg = false;
                    }
                    else if (legStartItemNo == tourStartItemNo && legFinishItemNo == tourFinishItemNo) //Step 3 - tour of two items, one leg
                    {
                        wikiTourList.Add(SetWikiItem(tableNo, tourStartItemNo));
                        wikiTourList.Add(SetWikiItem(tableNo, tourFinishItemNo));
                        checkNextLeg = false;
                    }
                    else if (legStartItemNo == tourStartItemNo) // Step 1 - tour of three or more items and tour started
                    {
                        wikiTourList.Add(SetWikiItem(tableNo, tourStartItemNo));
                        checkNextLeg = true;
                    }
                    else // Step 1 - tour of three or more items but tour not started yet
                    {
                        checkNextLeg = true;
                    }
                    break;
                }
            }
            // Steps 4 and 5
            while (checkNextLeg)
            {
                routeLegs.MoveNext();
                legStartItemNo = GetWikiRouteLegFirstItemNo(routeLegs.Current.ToString());
                wikiTourList.Add(SetWikiItem(tableNo, legStartItemNo)); // Step 4
                legFinishItemNo = GetWikiRouteLegLastItemNo(routeLegs.Current.ToString());
                if (legFinishItemNo == tourFinishItemNo) // Step 5
                {
                    wikiTourList.Add(SetWikiItem(tableNo, legFinishItemNo));
                    checkNextLeg = false;
                }
            }
            WikiCount = wikiTourList.Count + 2; // Wiki tour items plus two airports
            WikiDistance = int.Parse(tourDistance.Split(' ')[0]);
        }

        static internal void SetWikiTour()
        {
            SetWikiAirports();
            SetWikiOSMtiles();
        }

        static internal void SetWikiOSMtiles()
        {
            SetWikiOSMtilesOverview();
        }

        static internal void SetWikiOSMtilesOverview()
        {
            List<List<int>> tiles = [];
            List<List<int>> boundingBox = [];
            int zoom = GetBoundingBoxZoom(tiles, 2, 2);
            SetWikiOSMtiles(tiles, zoom);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            OSM.MakeSquare(boundingBox, "Charts_01", zoom);
            Drawing.ConvertImageformat("Charts_01", "png", "jpg");
        }

        static internal int GetBoundingBoxZoom(List<List<int>> tiles, int tilesWidth, int tilesHeight)
        {
            List<List<int>> boundingBox = [];
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetWikiOSMtiles(tiles, zoom);
                boundingBox.Clear();
                OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
                if ((boundingBox[xAxis].Count > tilesWidth) || (boundingBox[yAxis].Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        // Finds OSM tile numbers and offsets for a Wiki list (items plus airports) 
        static internal void SetWikiOSMtiles(List<List<int>> tiles, int zoom)
        {
            tiles.Clear();
            tiles.Add(OSM.GetOSMtile(WikiStartAirport.AirportLon.ToString(), WikiStartAirport.AirportLat.ToString(), zoom));
            for (int itemNo = 0; itemNo < wikiTourList.Count; itemNo++)
            {
                tiles.Add(OSM.GetOSMtile(wikiTourList[itemNo][longitude], wikiTourList[itemNo][latitude], zoom));
            }
            tiles.Add(OSM.GetOSMtile(WikiFinishAirport.AirportLon.ToString(), WikiFinishAirport.AirportLat.ToString(), zoom));
        }

        static internal void SetWikiAirports()
        {
            Coordinate coordFirstItem = Coordinate.Parse($"{wikiTourList[0][latitude]} {wikiTourList[0][longitude]}");
            WikiStartAirport = Runway.GetNearestAirport(coordFirstItem.Latitude.ToDouble(), coordFirstItem.Longitude.ToDouble());
            Coordinate coordStartAirport = Coordinate.Parse($"{WikiStartAirport.AirportLat} {WikiStartAirport.AirportLon}");
            WikiDistance += (int)coordFirstItem.Get_Distance_From_Coordinate(coordStartAirport).Miles;

            Coordinate coordLastItem = Coordinate.Parse($"{wikiTourList[^1][latitude]} {wikiTourList[^1][longitude]}");
            WikiFinishAirport = Runway.GetNearestAirport(coordLastItem.Latitude.ToDouble(), coordLastItem.Longitude.ToDouble());
            Coordinate coordFinishAirport = Coordinate.Parse($"{WikiFinishAirport.AirportLat} {WikiFinishAirport.AirportLon}");
            WikiDistance += (int)coordLastItem.Get_Distance_From_Coordinate(coordFinishAirport).Miles;
        }

        static internal List<string> SetWikiItem(int tableNo, int itemNo)
        {
            List<string> wikiItem = [];
            wikiItem.Add(wikiPage[tableNo][itemNo][title]);
            wikiItem.Add(wikiPage[tableNo][itemNo][link]);
            wikiItem.Add(wikiPage[tableNo][itemNo][latitude]);
            wikiItem.Add(wikiPage[tableNo][itemNo][longitude]);
            return wikiItem;
        }

        // Used to extract first item number from leg route string and item list string
        static internal int GetWikiRouteLegFirstItemNo(string routeLeg)
        {
            int stringBegin, stringEnd;
            stringBegin = routeLeg.IndexOf('[') + 1;
            stringEnd = routeLeg.IndexOf(']');
            return int.Parse(routeLeg[stringBegin..stringEnd]);
        }

        // Used to extract second item number from leg route string
        static internal int GetWikiRouteLegLastItemNo(string routeLeg)
        {
            int stringBegin, stringEnd;
            stringBegin = routeLeg.IndexOf("...") + 4;
            stringEnd = routeLeg.IndexOf('(') - 1;
            return GetWikiRouteLegFirstItemNo(routeLeg[stringBegin..stringEnd]);
        }

        // Populate wikiPage from user supplied URL
        static internal void SetWikiPage(string wikiListURL, string cellName, string[] attribute)
        {
            var htmlDoc = HttpRoutines.GetWebDoc(wikiListURL);
            if (htmlDoc == null)
            {
                return;
            }
            wikiPage.Clear();
            string[] refIncList = ["href", "title"];
            string[] refExcList = ["latitude", "redlink", "span style"];
            foreach (var table in htmlDoc.DocumentNode.SelectNodes("//table[@class='sortable wikitable' or @class='wikitable sortable']"))
            {
                List<List<string>> curTable = [];
                var rows = table.SelectNodes(".//tr");
                foreach (var row in rows)
                {
                    List<string> curRow = [];
                    var cells = row.SelectNodes(".//th | .//td");
                    if (cells == null)
                    {
                        continue;
                    }
                    foreach (var cell in cells)
                    {
                        if (refIncList.All(cell.InnerHtml.Contains) && (cell.Name == cellName) 
                            && (cell.GetAttributeValue(attribute[0], "") == attribute[1]))
                        {
                            if (refExcList.Any(cell.InnerHtml.Contains))
                            {
                                break;
                            }
                            var title = cell.Descendants("a").ToList()[0].GetAttributeValue("title", null);
                            var link = cell.Descendants("a").ToList()[0].GetAttributeValue("href", null);
                            if (title != null && link != null)
                            {
                                curRow.Add(HttpUtility.HtmlDecode(title));
                                curRow.Add(link);
                                if (GetWikiItemCoordinates(curRow))
                                {
                                    curTable.Add(curRow);
                                    break;
                                }
                            }
                        }
                    }
                }
                if (curTable.Count > 0)
                {
                    wikiPage.Add(curTable);
                }
            }
        }

        static internal bool GetWikiItemCoordinates(List<string> curRow)
        {
            var htmlDoc = HttpRoutines.GetWebDoc($"https://en.wikipedia.org/{curRow[link]}");
            if (htmlDoc == null)
            {
                return false;
            }
            var latSpans = htmlDoc.DocumentNode.SelectNodes(".//span[@class='latitude']");
            if (latSpans != null)
            {
                curRow.Add(ConvertWikiCoOrd(latSpans[0].InnerText));
                var lonSpans = htmlDoc.DocumentNode.SelectNodes(".//span[@class='longitude']");
                curRow.Add(ConvertWikiCoOrd(lonSpans[0].InnerText));
                return true;
            }
            return false;
        }

        static internal List<string> GetWikiTableList()
        {
            var list = new List<string>();
            for (int tableNo = 0; tableNo < wikiPage.Count; tableNo++)
            {
                string tableDesc = $"{wikiPage[tableNo][0][title]} ... {wikiPage[tableNo][^1][title]} ({wikiPage[tableNo].Count} items)";
                list.Add(tableDesc);
            }
            return list;
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
            wikiCoOrd = wikiCoOrd.Remove(wikiCoOrd.Length-1);

            return wikiCoOrd;
        }

        static internal List<string> SortWikiTable(int tableNo)
        {
            int[,] wikiTableCost = new int[wikiPage[tableNo].Count, wikiPage[tableNo].Count];

            SetWikiTableCosts(tableNo, wikiTableCost);

            // Initialise route as first item
            List<string> route = [];
            int[] itemsVisited = new int[wikiPage[tableNo].Count];
            itemsVisited[0] = 1;
            int firstEndItem = 0;
            if (wikiPage[tableNo].Count == 1)
            {
                route.Add($"[0] {wikiPage[tableNo][0][title]} ... [0] {wikiPage[tableNo][0][title]} ({wikiTableCost[0, 0]} miles)");
                return route;
            }

            // Find closest item to first item and append to route
            int nearestItem = GetNearesetWikiItem(0, wikiTableCost, itemsVisited);
            route.Add($"[0] {wikiPage[tableNo][0][title]} ... [{nearestItem}] {wikiPage[tableNo][nearestItem][title]} " +
                $"({wikiTableCost[0, nearestItem]} miles)");
            itemsVisited[nearestItem] = 1;
            int secondEndItem = nearestItem;

            int itemVisitedCount = 2;
            while (itemVisitedCount < wikiPage[tableNo].Count)
            {
                // Find closest item to either end of current route and append to route
                int nearestFirstEndItem = GetNearesetWikiItem(firstEndItem, wikiTableCost, itemsVisited);
                int nearestSecondEndItem = GetNearesetWikiItem(secondEndItem, wikiTableCost, itemsVisited);
                if (wikiTableCost[firstEndItem, nearestFirstEndItem] <= wikiTableCost[secondEndItem, nearestSecondEndItem])
                {
                    route.Insert(0, $"[{nearestFirstEndItem}] {wikiPage[tableNo][nearestFirstEndItem][title]} ... " +
                        $"[{firstEndItem}] {wikiPage[tableNo][firstEndItem][title]} ({wikiTableCost[nearestFirstEndItem, firstEndItem]} miles)");
                    itemsVisited[nearestFirstEndItem] = 1;
                    firstEndItem = nearestFirstEndItem;
                }
                else
                {
                    route.Add($"[{secondEndItem}] {wikiPage[tableNo][secondEndItem][title]} ... " +
                        $"[{nearestSecondEndItem}] {wikiPage[tableNo][nearestSecondEndItem][title]} ({wikiTableCost[secondEndItem, nearestSecondEndItem]} miles)");
                    itemsVisited[nearestSecondEndItem] = 1;
                    secondEndItem = nearestSecondEndItem;
                }
                itemVisitedCount++;
            }

            return route;
        }

        static internal int GetNearesetWikiItem(int curItem, int[,] wikiTableCost, int[] itemsVisited)
        {
            int minDistance = int.MaxValue;
            int nearestWikiItem = -1;
            for (int itemIndex = 0; itemIndex < itemsVisited.Length; itemIndex++)
            {
                if (itemIndex != curItem && wikiTableCost[curItem, itemIndex] < minDistance && itemsVisited[itemIndex] == 0)
                {
                    minDistance = wikiTableCost[curItem, itemIndex];
                    nearestWikiItem = itemIndex;
                }
            }
            return nearestWikiItem;
        }

        static internal void SetWikiTableCosts(int tableNo, int[,] wikiTableCost)
        {
            int maxCost = 0;
            for (int row = 0; row < wikiPage[tableNo].Count; row++)
            {
                for (int col = 0; col < wikiPage[tableNo].Count; col++)
                {
                    Coordinate coord1 = Coordinate.Parse($"{wikiPage[tableNo][row][latitude]} {wikiPage[tableNo][row][longitude]}");
                    Coordinate coord2 = Coordinate.Parse($"{wikiPage[tableNo][col][latitude]} {wikiPage[tableNo][col][longitude]}");
                    wikiTableCost[row, col] = (int)coord1.Get_Distance_From_Coordinate(coord2).Miles;
                    if (wikiTableCost[row, col] > maxCost)
                    {
                        maxCost = wikiTableCost[row, col];
                    }
                }
            }
            for (int row = 0; row < wikiPage[tableNo].Count; row++)
            {
                for (int col = 0; col < wikiPage[tableNo].Count; col++)
                {
                    if (row == col && wikiPage[tableNo].Count > 1)
                    {
                        wikiTableCost[row, col] = maxCost + 1;
                    }
                }
            }
        }
    }
}
