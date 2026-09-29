using HtmlAgilityPack;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Models;
using P3D_Scenario_Generator.Services;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace P3D_Scenario_Generator.WikipediaScenario
{
    /// <summary>
    /// Parses Wikipedia HTML pages containing tables of locations to extract titles, links, and coordinates.
    /// </summary>
    /// <param name="logger">The logger service for recording diagnostic and error events.</param>
    /// <param name="httpRoutines">The HTTP utility service for downloading web documents.</param>
    internal class WikiPageHtmlParser(Logger logger, HttpRoutines httpRoutines)
    {
        private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpRoutines _httpRoutines = httpRoutines ?? throw new ArgumentNullException(nameof(httpRoutines));

        /// <summary>
        /// Parses a Wikipedia URL for table(s) identified by class 'sortable wikitable' or 'wikitable sortable'.
        /// Extracts items from the specified column that have titles and links with valid coordinates,
        /// and populates the results into <see cref="Wikipedia.WikiPage"/>.
        /// </summary>
        /// <param name="wikiURL">The user-supplied Wikipedia URL to scrape.</param>
        /// <param name="columnNo">The 1-based column index containing the target links.</param>
        /// <param name="coordinateSource">The coordinate format strategy to extract coordinates from the page or subpages.</param>
        /// <param name="formData">The scenario form configuration data.</param>
        /// <param name="progressReporter">Optional progress reporter for publishing UI feedback.</param>
        /// <param name="wikipedia">The Wikipedia scenario engine instance whose WikiPage collection is populated.</param>
        /// <returns><see langword="true"/> if items were successfully extracted; otherwise, <see langword="false"/>.</returns>
        public async Task<bool> PopulateWikiPageAsync(
            string wikiURL,
            int columnNo,
            CoordinateSource coordinateSource,
            ScenarioFormData formData,
            IProgress<string>? progressReporter,
            Wikipedia wikipedia)
        {
            // Boundary Guard: Validate inputs crossing from UI
            ArgumentException.ThrowIfNullOrWhiteSpace(wikiURL);
            ArgumentNullException.ThrowIfNull(formData);
            ArgumentNullException.ThrowIfNull(wikipedia);

            // Report initial status when starting the overall operation
            progressReporter?.Report($"Fetching data from {wikiURL}, please wait...");

            wikipedia.WikiPage = [];
            HtmlAgilityPack.HtmlDocument? htmlDoc = await _httpRoutines.GetWebDocAsync(wikiURL);
            const string tableSelection = "//table[contains(@class, 'sortable wikitable') or contains(@class, 'wikitable sortable') or contains(@class, 'wikitable')]";

            if (htmlDoc?.DocumentNode == null)
            {
                progressReporter?.Report($"Failed to retrieve HTML document from {wikiURL}.");
                await _logger.ErrorAsync($"Failed to retrieve HTML document from {wikiURL}");
                return false;
            }

            if (!GetNodeCollection(htmlDoc.DocumentNode, out HtmlNodeCollection? tables, tableSelection, false, formData))
            {
                progressReporter?.Report($"No relevant tables found at {wikiURL}.");
                await _logger.WarningAsync($"No tables matching selection '{tableSelection}' found at {wikiURL}.");
                return true;
            }

            int totalTables = tables.Count;
            int currentTableIndex = 0;

            foreach (var table in tables)
            {
                currentTableIndex++;
                List<WikiItemParams> curTable = [];

                progressReporter?.Report($"Reading table {currentTableIndex} of {totalTables}, please wait...");

                if (GetNodeCollection(table, out HtmlNodeCollection? rows, ".//tr", false, formData))
                {
                    foreach (var row in rows)
                    {
                        if (GetNodeCollection(row, out HtmlNodeCollection? cells, ".//th | .//td", false, formData) && cells.Count >= columnNo)
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

            progressReporter?.Report($"Finished parsing {totalTables} table(s) from {wikiURL}.");
            return true;
        }

        /// <summary>
        /// Parses parent HtmlNode using specified selection string for collection of child HtmlNodes.
        /// </summary>
        /// <param name="parentNode">The HtmlNode to be searched.</param>
        /// <param name="childNodeCollection">The collection of HtmlNodes resulting from selection string.</param>
        /// <param name="selection">The string used to collect child HtmlNodes from the parent HtmlNode.</param>
        /// <param name="verbose">Whether to display a UI error dialog on failure.</param>
        /// <param name="formData">Form data context for the error dialog title.</param>
        /// <returns><see langword="true"/> if nodes were found; otherwise, <see langword="false"/>.</returns>
        internal static bool GetNodeCollection(
            HtmlNode parentNode,
            [NotNullWhen(true)] out HtmlNodeCollection? childNodeCollection,
            string selection,
            bool verbose,
            ScenarioFormData formData)
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
        /// Stores one item in a table of <see cref="Wikipedia.WikiPage"/>. Item includes a title, URL to Wikipedia item page
        /// and latitude and longitude.
        /// </summary>
        /// <param name="cell">The cell in a table row containing item title and hyperlink.</param>
        /// <param name="curTable">The current table being populated in <see cref="Wikipedia.WikiPage"/>.</param>
        /// <param name="formData">The scenario configuration data.</param>
        /// <param name="coordinateSource">Strategy for extracting coordinates from row vs item page.</param>
        internal async Task ReadWikiCellAsync(HtmlNode cell, List<WikiItemParams> curTable, ScenarioFormData formData, CoordinateSource coordinateSource)
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
                wikiItem.itemURL = link;
                bool coordinatesFound = false;

                if (coordinateSource == CoordinateSource.TableColumn)
                {
                    var row = cell.Ancestors("tr").FirstOrDefault();
                    HtmlNode? latNode = row?.SelectSingleNode(".//span[@class='latitude']");
                    HtmlNode? lonNode = row?.SelectSingleNode(".//span[@class='longitude']");

                    if (latNode != null && lonNode != null)
                    {
                        wikiItem.latitude = ConvertWikiCoOrd(latNode.InnerText);
                        wikiItem.longitude = ConvertWikiCoOrd(lonNode.InnerText);
                        coordinatesFound = true;
                    }
                }

                if (!coordinatesFound)
                {
                    coordinatesFound = await GetWikiItemCoordinatesAsync(wikiItem, formData);
                }

                if (coordinatesFound)
                {
                    wikiItem.hrefs = await GetWikiItemHREFsAsync(wikiItem);
                    curTable.Add(wikiItem);
                }
            }
        }

        /// <summary>
        /// Checks that the item hyperlink is pointing to a page with lat/long coordinate in expected place
        /// and retrieves them for storage in a table in <see cref="Wikipedia.WikiPage"/>.
        /// </summary>
        /// <param name="wikiItem">The current row in table being populated in <see cref="Wikipedia.WikiPage"/>.</param>
        /// <param name="formData">The scenario configuration data.</param>
        /// <returns><see langword="true"/> if coordinates were found and set; otherwise, <see langword="false"/>.</returns>
        internal async Task<bool> GetWikiItemCoordinatesAsync(WikiItemParams wikiItem, ScenarioFormData formData)
        {
            var htmlDoc = await _httpRoutines.GetWebDocAsync(wikiItem.itemURL);
            if (htmlDoc?.DocumentNode != null &&
                GetNodeCollection(htmlDoc.DocumentNode, out HtmlNodeCollection? latSpans, ".//span[@class='latitude']", false, formData) &&
                latSpans.Count > 0 &&
                GetNodeCollection(htmlDoc.DocumentNode, out HtmlNodeCollection? lonSpans, ".//span[@class='longitude']", false, formData) &&
                lonSpans.Count > 0)
            {
                wikiItem.latitude = ConvertWikiCoOrd(latSpans[0].InnerText);
                wikiItem.longitude = ConvertWikiCoOrd(lonSpans[0].InnerText);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cleans and formats a Wikipedia link to ensure an absolute HTTPS URL.
        /// </summary>
        /// <param name="dirtyUrl">The raw URL or href extracted from HTML anchor.</param>
        /// <returns>A fully-qualified HTTPS Wikipedia URL.</returns>
        internal static string CleanWikiLinkURL(string dirtyUrl)
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

            return "https://en.wikipedia.org" + (cleanUrl.StartsWith('/') ? "" : "/") + cleanUrl;
        }

        /// <summary>
        /// Retrieves internal section anchor links (<c>href="#..."</c>) from the Wikipedia item page.
        /// </summary>
        /// <param name="wikiItem">The Wikipedia item whose page is parsed for section anchors.</param>
        /// <returns>A list of section anchor strings.</returns>
        internal async Task<List<string>> GetWikiItemHREFsAsync(WikiItemParams wikiItem)
        {
            var htmlDoc = await _httpRoutines.GetWebDocAsync(wikiItem.itemURL);
            if (htmlDoc?.Text == null)
            {
                return [];
            }

            string htmlDocContents = htmlDoc.Text;
            int indexSearchFrom = 0;
            const string hrefTag = "href=\"#";
            List<string> hrefs = [];
            int indexHREFtagStart = htmlDocContents.IndexOf(hrefTag, indexSearchFrom);
            while (indexHREFtagStart >= 0)
            {
                int indexHREFvalueStart = indexHREFtagStart + hrefTag.Length;
                int indexHREFvalueFinish = htmlDocContents.IndexOf('\"', indexHREFvalueStart);
                if (indexHREFvalueFinish < 0)
                {
                    break;
                }
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
        /// Converts Wikipedia coordinate format strings into a format readable by the CoordinateSharp package.
        /// </summary>
        /// <param name="wikiCoOrd">The Wikipedia coordinate format string.</param>
        /// <returns>A coordinate string formatted for CoordinateSharp parsing.</returns>
        internal static string ConvertWikiCoOrd(string wikiCoOrd)
        {
            if (string.IsNullOrWhiteSpace(wikiCoOrd))
            {
                return string.Empty;
            }

            int degPos = wikiCoOrd.IndexOf('°');
            if (degPos >= 0)
            {
                wikiCoOrd = wikiCoOrd.Insert(degPos + 1, " ");
            }

            int minPos = degPos >= 0 ? degPos + 2 : 0;
            while (minPos < wikiCoOrd.Length && (char.IsDigit(wikiCoOrd[minPos]) || wikiCoOrd[minPos] == '.'))
            {
                minPos++;
            }
            if (minPos < wikiCoOrd.Length)
            {
                wikiCoOrd = wikiCoOrd.Insert(minPos + 1, " ");
            }

            char final = wikiCoOrd[^1];
            wikiCoOrd = $"{final} {wikiCoOrd}";

            wikiCoOrd = wikiCoOrd[..^1];

            return wikiCoOrd;
        }
    }
}