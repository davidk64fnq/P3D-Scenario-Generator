using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides utility methods for parsing HTML documents.
    /// </summary>
    /// <param name="log">The logging service to use for reporting errors.</param>
    internal class HtmlParser(Logger log)
    {
        private readonly Logger _log = log ?? throw new ArgumentNullException(nameof(log));

        /// <summary>
        /// Selects a single HTML node matching the specified XPath query and retrieves its inner text.
        /// </summary>
        /// <param name="htmlDoc">The HTML document to query.</param>
        /// <param name="nodeSelection">The XPath expression specifying the node to select.</param>
        /// <returns>
        /// A tuple where <c>success</c> is <see langword="true"/> if the node was found; otherwise, <see langword="false"/>.
        /// <c>innerText</c> contains the node text if found, or an empty string.
        /// </returns>
        internal async Task<(bool success, string innerText)> SelectSingleNodeInnerTextAsync(HtmlDocument htmlDoc, string nodeSelection)
        {
            ArgumentNullException.ThrowIfNull(htmlDoc);

            HtmlNode? selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode is null)
            {
                await _log.ErrorAsync($"Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return (false, string.Empty);
            }

            return (true, selectedNode.InnerText ?? string.Empty);
        }

        /// <summary>
        /// Selects a single HTML node matching the specified XPath query and retrieves the value of the specified attribute.
        /// </summary>
        /// <param name="htmlDoc">The HTML document to query.</param>
        /// <param name="nodeSelection">The XPath expression specifying the node to select.</param>
        /// <param name="attributeSelection">The name of the attribute whose value is to be retrieved.</param>
        /// <returns>
        /// A tuple where <c>success</c> is <see langword="true"/> if the node was found; otherwise, <see langword="false"/>.
        /// <c>attributeValue</c> contains the retrieved attribute value, or an empty string if not found.
        /// </returns>
        internal async Task<(bool success, string attributeValue)> SelectSingleNodeGetAttributeValueAsync(HtmlDocument htmlDoc, string nodeSelection, string attributeSelection)
        {
            ArgumentNullException.ThrowIfNull(htmlDoc);

            HtmlNode? selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode is null)
            {
                await _log.ErrorAsync($"Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return (false, string.Empty);
            }

            string attributeValue = selectedNode.GetAttributeValue(attributeSelection, string.Empty);

            return (true, attributeValue);
        }
    }
}