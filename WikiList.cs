using HtmlAgilityPack;
using System.Globalization;
using CoordinateSharp;
using Microsoft.VisualBasic;
using System.Linq;

namespace P3D_Scenario_Generator
{
    public class WikiListParams
    {
        
    }
    internal class WikiList()
    {
        internal static int WikiCount { get; private set; }
        internal static List<List<List<string>>> wikiPage = [];
        internal static int title = 0, link = 1, latitude = 2, longitude = 3;

        static internal void SetWikiTour()
        {
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
                                curRow.Add(title);
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

        static internal string SortWikiTable(int tableNo)
        {
            int[,] wikiTableCost = new int[wikiPage[tableNo].Count, wikiPage[tableNo].Count];

            SetWikiTableCosts(tableNo, wikiTableCost);

            // Initialise route as first item
            string route = "0";
            int[] itemsVisited = new int[wikiPage[tableNo].Count];
            itemsVisited[0] = 1;
            int itemVisitedCount = 1;
            int firstEndItem = 0;

            // Find closest item to first item and append to route
            int nearestItem = GetNearesetWikiItem(0, wikiTableCost, itemsVisited);
            route += $" ({wikiTableCost[0, nearestItem]}) {nearestItem}";
            itemsVisited[nearestItem] = 1;
            itemVisitedCount = 2;
            int secondEndItem = nearestItem;

            while (itemVisitedCount < wikiPage[tableNo].Count)
            {
                // Find closest item to either end of current route
                int nearestFirstEndItem = GetNearesetWikiItem(firstEndItem, wikiTableCost, itemsVisited);
                int nearestSecondEndItem = GetNearesetWikiItem(secondEndItem, wikiTableCost, itemsVisited);
                if (wikiTableCost[firstEndItem, nearestFirstEndItem] <= wikiTableCost[secondEndItem, nearestSecondEndItem])
                {
                    route = $"{nearestFirstEndItem} ({wikiTableCost[firstEndItem, nearestFirstEndItem]}) {route}";
                    itemsVisited[nearestFirstEndItem] = 1;
                    firstEndItem = nearestFirstEndItem;
                }
                else
                {
                    route = $"{route} ({wikiTableCost[secondEndItem, nearestSecondEndItem]}) {nearestSecondEndItem}";
                    itemsVisited[nearestSecondEndItem] = 1;
                    secondEndItem = nearestSecondEndItem;
                }
                itemVisitedCount++;
            }

            return route;
        }

        static internal int GetNearesetWikiItem(int curItem, int[,] wikiTableCost, int[] itemsVisited)
        {
            int minDistance = Int32.MaxValue;
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
                    Coordinate coord1 = Coordinate.Parse($"{wikiPage[tableNo][row][2]} {wikiPage[tableNo][row][3]}");
                    Coordinate coord2 = Coordinate.Parse($"{wikiPage[tableNo][col][2]} {wikiPage[tableNo][col][3]}");
                    wikiTableCost[row, col] = (int)coord1.Get_Distance_From_Coordinate(coord2).Feet;
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
                    if (row == col)
                    {
                        wikiTableCost[row, col] = maxCost + 1;
                    }
                }
            }
        }
    }
}
