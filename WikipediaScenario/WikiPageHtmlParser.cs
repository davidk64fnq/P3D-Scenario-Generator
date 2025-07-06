using HtmlAgilityPack;
using System.Web;
using System.Windows.Forms;

namespace P3D_Scenario_Generator.WikipediaScenario
{
    // Populating WikiPage when user pastes in Wikipedia URL, called from main form
    internal class WikiPageHtmlParser
    {

        /// <summary>
        /// Parses user supplied URL for table(s) identified by class='sortable wikitable'.
        /// Using specified column extracts items that have a title and link. The link must
        /// supply latitude and longitude. Stores items in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="wikiURL">User supplied Wikipedia URL</param>
        /// <param name="columnNo">User supplied column number of items in table</param>
        static internal bool PopulateWikiPage(string wikiURL, int columnNo, IProgress<string> progressReporter = null)
        {
            // Report initial status when starting the overall operation
            progressReporter?.Report($"Fetching data from {wikiURL}, please wait...");

            Wikipedia.WikiPage = [];
            HtmlAgilityPack.HtmlDocument htmlDoc = HttpRoutines.GetWebDoc(wikiURL);
            HtmlNodeCollection tables = null;
            HtmlNodeCollection rows = null;
            HtmlNodeCollection cells = null;
            string tableSelection = "//table[contains(@class, 'sortable wikitable') or contains(@class, 'wikitable sortable') or contains(@class, 'wikitable')]";

            if (htmlDoc == null)
            {
                progressReporter?.Report($"Failed to retrieve HTML document from {wikiURL}.");
                Log.Error($"Failed to retrieve HTML document from {wikiURL}");
                return false; // Return false on failure to get HTML
            }

            if (!GetNodeCollection(htmlDoc.DocumentNode, ref tables, tableSelection, true))
            {
                progressReporter?.Report($"No relevant tables found at {wikiURL}.");
                Log.Warning($"No tables matching selection '{tableSelection}' found at {wikiURL}.");
                return true; // Return true if no tables, as it's not strictly an error, just no data. Adjust based on your definition of success.
            }

            int totalTables = tables.Count;
            int currentTableIndex = 0; // Initialize a counter for tables

            foreach (var table in tables)
            {
                currentTableIndex++; // Increment for each table
                List<WikiItemParams> curTable = [];

                // Report progress for the current table
                progressReporter?.Report($"Reading table {currentTableIndex} of {totalTables}, please wait...");

                if (GetNodeCollection(table, ref rows, ".//tr", true))
                {
                    // You could add row-level progress here if needed, but it might be too chatty
                    // int totalRows = rows.Count;
                    // int currentRowIndex = 0;

                    foreach (var row in rows)
                    {
                        // currentRowIndex++;
                        if (GetNodeCollection(row, ref cells, ".//th | .//td", true) && cells.Count >= columnNo)
                        {
                            ReadWikiCell(cells[columnNo - 1], curTable);
                        }
                    }
                }

                if (curTable.Count > 0)
                {
                    Wikipedia.WikiPage.Add(curTable);
                }
            }

            // Final success message after all tables are processed
            progressReporter?.Report($"Finished parsing {totalTables} table(s) from {wikiURL}.");
            return true; // Indicates overall success
        }

        /// <summary>
        /// Parses parent HtmlNode using specified selection string for collection of child HtmlNodes
        /// </summary>
        /// <param name="parentNode">The HtmlNode to be searched</param>
        /// <param name="childNodeCollection">The collection of HtmlNodes resulting from selction string</param>
        /// <param name="selection">The string used to collect child HtmlNodes from the parent HtmlNode</param>
        /// <returns></returns>
        static internal bool GetNodeCollection(HtmlNode parentNode, ref HtmlNodeCollection childNodeCollection, string selection, bool verbose)
        {
            childNodeCollection = parentNode.SelectNodes(selection);
            if (childNodeCollection == null && verbose)
            {
                string errorMessage = $"Node collection failed for {selection}";
                MessageBox.Show(errorMessage, $"{Parameters.SelectedScenario}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
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
            List<HtmlNode> cellDescendants = [.. cell.Descendants("a")];
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
        /// Checks that the item hyperlink is pointing to a page with lat/long coordinate in expected place
        /// and retrieves them for storage in a table in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="wikiItem">The current row in table being populated in <see cref="WikiPage"/></param>
        /// <returns></returns>
        static internal bool GetWikiItemCoordinates(WikiItemParams wikiItem)
        {
            var htmlDoc = HttpRoutines.GetWebDoc($"https://en.wikipedia.org/{wikiItem.itemURL}");
            HtmlNodeCollection spans = null;
            if (htmlDoc != null && GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='latitude']", false))
            {
                if (spans != null && spans.Count > 0)
                {
                    wikiItem.latitude = ConvertWikiCoOrd(spans[0].InnerText);
                    GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='longitude']", false);
                    wikiItem.longitude = ConvertWikiCoOrd(spans[0].InnerText);
                    return true;
                }
            }
            return false;
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
            while (char.IsDigit(wikiCoOrd[minPos]) || wikiCoOrd[minPos] == '.')
            {
                minPos++;
            }
            wikiCoOrd = wikiCoOrd.Insert(minPos + 1, " ");

            // Copy last char N/S/E/W to front with space after it
            char final = wikiCoOrd[^1];
            wikiCoOrd = $"{final} {wikiCoOrd}";

            // Delete last char
            wikiCoOrd = wikiCoOrd[..^1];

            return wikiCoOrd;
        }
    }
}
