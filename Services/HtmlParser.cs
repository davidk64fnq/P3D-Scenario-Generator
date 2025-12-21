using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for parsing HTML documents.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the HtmlParser class.
    /// </remarks>
    /// <param name="log">The logging service to use for reporting errors.</param>
    public class HtmlParser(Logger log)
    {
        private readonly Logger _log = log;

        /// <inheritdoc/>
        public async Task<(bool success, string innerText)> SelectSingleNodeInnerTextAsync(HtmlDocument htmlDoc, string nodeSelection)
        {
            HtmlNode selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode == null)
            {
                await _log.ErrorAsync($"Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return (false, string.Empty);
            }

            return (true, selectedNode.InnerText);
        }

        /// <inheritdoc/>
        public async Task<(bool success, string attributeValue)> SelectSingleNodeGetAttributeValueAsync(HtmlDocument htmlDoc, string nodeSelection, string attributeSelection)
        {
            HtmlNode selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode == null)
            {
                await _log.ErrorAsync($"Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return (false, string.Empty);
            }

            string attributeValue = selectedNode.GetAttributeValue(attributeSelection, "");

            return (true, attributeValue);
        }
    }
}
