using HtmlAgilityPack;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using System.Web;

namespace P3D_Scenario_Generator.WikipediaScenario
{
    // Populating WikiPage when user pastes in Wikipedia URL, called from main form
    public class WikiPageHtmlParser(Logger logger, FileOps fileOps, HttpRoutines httpRoutines, FormProgressReporter progressReporter)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        /// <summary>
        /// Parses user supplied URL for table(s) identified by class='sortable wikitable'.
        /// Using specified column extracts items that have a title and link. The link must
        /// supply latitude and longitude. Stores items in <see cref="WikiPage"/>.
        /// </summary>
        /// <param name="wikiURL">User supplied Wikipedia URL</param>
        /// <param name="columnNo">User supplied column number of items in table</param>
        public async Task<bool> PopulateWikiPageAsync(string wikiURL, int columnNo, CoordinateSource coordinateSource, ScenarioFormData formData, IProgress<string> progressReporter, Wikipedia wikipedia)
        {
            // Report initial status when starting the overall operation
            progressReporter?.Report($"Fetching data from {wikiURL}, please wait...");

            wikipedia.WikiPage = [];
            HtmlAgilityPack.HtmlDocument htmlDoc = await _httpRoutines.GetWebDocAsync(wikiURL);
            HtmlNodeCollection tables = null;
            HtmlNodeCollection rows = null;
            HtmlNodeCollection cells = null;
            string tableSelection = "//table[contains(@class, 'sortable wikitable') or contains(@class, 'wikitable sortable') or contains(@class, 'wikitable')]";

            if (htmlDoc == null)
            {
                progressReporter?.Report($"Failed to retrieve HTML document from {wikiURL}.");
                await _logger.ErrorAsync($"Failed to retrieve HTML document from {wikiURL}");
                return false; // Return false on failure to get HTML
            }

            if (!GetNodeCollection(htmlDoc.DocumentNode, ref tables, tableSelection, false, formData))
            {
                progressReporter?.Report($"No relevant tables found at {wikiURL}.");
                await _logger.WarningAsync($"No tables matching selection '{tableSelection}' found at {wikiURL}.");
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

                if (GetNodeCollection(table, ref rows, ".//tr", false, formData))
                {
                    // You could add row-level progress here if needed, but it might be too chatty
                    // int totalRows = rows.Count;
                    // int currentRowIndex = 0;

                    foreach (var row in rows)
                    {
                        // currentRowIndex++;
                        if (GetNodeCollection(row, ref cells, ".//th | .//td", false, formData) && cells.Count >= columnNo)
                        {
                            await ReadWikiCellAsync(cells[columnNo - 1], curTable, formData, coordinateSource);
                        }
                    }
                }

                if (curTable.Count > 0)
                {
                    wikipedia.WikiPage.Add(curTable);
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
        /// <param name="childNodeCollection">The collection of HtmlNodes resulting from selection string</param>
        /// <param name="selection">The string used to collect child HtmlNodes from the parent HtmlNode</param>
        /// <param name="verbose">Whether to display a UI error dialog on failure</param>
        /// <param name="formData">Form data context for the error dialog title</param>
        /// <returns>True if nodes were found; false if the node collection is null.</returns>
        static internal bool GetNodeCollection(HtmlNode parentNode, ref HtmlNodeCollection childNodeCollection, string selection, bool verbose, ScenarioFormData formData)
        {
            childNodeCollection = parentNode.SelectNodes(selection);

            if (childNodeCollection == null)
            {
                if (verbose)
                {
                    string errorMessage = $"Node collection failed for {selection}";
                    MessageBox.Show(errorMessage, $"{formData.ScenarioType}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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
        public async Task ReadWikiCellAsync(HtmlNode cell, List<WikiItemParams> curTable, ScenarioFormData formData, CoordinateSource coordinateSource)
        {
            WikiItemParams wikiItem = new();
            List<HtmlNode> cellDescendants = [.. cell.Descendants("a")];
            string title = "", link = "";
            if (cellDescendants.Count > 0)
            {
                string visibleText = cellDescendants[0].InnerText?.Trim() ?? "";
                title = !string.IsNullOrEmpty(visibleText)
                        ? visibleText
                        : cellDescendants[0].GetAttributeValue("title", "");
                link = CleanWikiLinkURL(cellDescendants[0].GetAttributeValue("href", ""));
            }
            if (title != "" && link != "")
            {
                wikiItem.title = HttpUtility.HtmlDecode(title);
                wikiItem.itemURL = link; bool coordinatesFound = false;

                // Choice 1: Look in the Table Row first
                if (coordinateSource == CoordinateSource.TableColumn)
                {
                    var row = cell.Ancestors("tr").FirstOrDefault();
                    HtmlNode latNode = row?.SelectSingleNode(".//span[@class='latitude']");
                    HtmlNode lonNode = row?.SelectSingleNode(".//span[@class='longitude']");

                    if (latNode != null && lonNode != null)
                    {
                        wikiItem.latitude = ConvertWikiCoOrd(latNode.InnerText);
                        wikiItem.longitude = ConvertWikiCoOrd(lonNode.InnerText);
                        coordinatesFound = true;
                    }
                }

                // Choice 2: Look at the Target Item Page (or Fallback if Table was selected but empty)
                if (!coordinatesFound)
                {
                    // If the link has a '#' fragment and we are forced to fetch the item page,
                    // be aware it will fetch the generic parent page coordinates.
                    coordinatesFound = await GetWikiItemCoordinatesAsync(wikiItem, formData);
                }

                // If we successfully resolved coordinates from either method, finalize the item
                if (coordinatesFound)
                {
                    wikiItem.hrefs = await GetWikiItemHREFsAsync(wikiItem);
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
        public async Task<bool> GetWikiItemCoordinatesAsync(WikiItemParams wikiItem, ScenarioFormData formData)
        {
            var htmlDoc = await _httpRoutines.GetWebDocAsync(wikiItem.itemURL);
            HtmlNodeCollection spans = null;
            if (htmlDoc != null && GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='latitude']", false, formData))
            {
                if (spans != null && spans.Count > 0)
                {
                    wikiItem.latitude = ConvertWikiCoOrd(spans[0].InnerText);
                    GetNodeCollection(htmlDoc.DocumentNode, ref spans, ".//span[@class='longitude']", false, formData);
                    wikiItem.longitude = ConvertWikiCoOrd(spans[0].InnerText);
                    return true;
                }
            }
            return false;
        }

        public static string CleanWikiLinkURL(string dirtyUrl)
        {
            if (string.IsNullOrWhiteSpace(dirtyUrl))
            {
                return string.Empty;
            }

            string cleanUrl = dirtyUrl.Trim();

            if (cleanUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                cleanUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return cleanUrl;
            }

            if (cleanUrl.StartsWith("//"))
            {
                return "https:" + cleanUrl;
            }

            // Prefix the standard domain for absolute/relative paths
            return "https://en.wikipedia.org" + (cleanUrl.StartsWith('/') ? "" : "/") + cleanUrl;
        }

        /// <summary>
        /// Retrieves any href="# links in the item. These are used to allow user to step through sections
        /// of the item page using joystick mapped buttons as an alternative to scrolling with a mouse.
        /// </summary>
        /// <param name="wikiItem">The current row in table being populated in <see cref="WikiPage"/></param>
        public async Task<List<string>> GetWikiItemHREFsAsync(WikiItemParams wikiItem)
        {
            var htmlDoc = await _httpRoutines.GetWebDocAsync(wikiItem.itemURL);
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
