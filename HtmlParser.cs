using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator
{
    internal class HtmlParser
    {
        /// <summary>
        /// Selects a single HTML node from the provided <see cref="HtmlDocument"/> using an XPath expression
        /// and extracts its <see cref="HtmlNode.InnerText"/>.
        /// </summary>
        /// <param name="htmlDoc">The <see cref="HtmlDocument"/> to search within.</param>
        /// <param name="nodeSelection">The XPath expression used to select the desired node.</param>
        /// <param name="innerText">When this method returns, contains the <see cref="HtmlNode.InnerText"/>
        /// of the selected node if found, or an empty string if the node is not found or an error occurs.</param>
        /// <returns><see langword="true"/> if the node is found and its inner text is successfully extracted;
        /// otherwise, <see langword="false"/>.</returns>
        static internal bool SelectSingleNodeInnerText(HtmlDocument htmlDoc, string nodeSelection, out string innerText)
        {
            // Initialise out parameter
            innerText = string.Empty;

            // Get the node using the provided selection
            HtmlNode selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode == null)
            {
                Log.Error($"SelectSingleNodeInnerText: Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return false;
            }

            // Get the attribute value from the selected node
            innerText = selectedNode.InnerText;

            return true;
        }

        /// <summary>
        /// Selects a single HTML node from the provided <see cref="HtmlDocument"/> using an XPath expression
        /// and extracts the value of a specified attribute from that node.
        /// </summary>
        /// <param name="htmlDoc">The <see cref="HtmlDocument"/> to search within.</param>
        /// <param name="nodeSelection">The XPath expression used to select the desired node.</param>
        /// <param name="attributeSelection">The name of the attribute whose value is to be retrieved (e.g., "href", "src", "content").</param>
        /// <param name="attributeValue">When this method returns, contains the value of the specified attribute
        /// if the node and attribute are found, or an empty string if the node or attribute is not found, or an error occurs.</param>
        /// <returns><see langword="true"/> if the node and attribute are found and the attribute value is successfully extracted;
        /// otherwise, <see langword="false"/>.</returns>
        static internal bool SelectSingleNodeGetAttributeValue(HtmlDocument htmlDoc, string nodeSelection, string attributeSelection, out string attributeValue)
        {
            // Initialise out parameter
            attributeValue = string.Empty;

            // Get the node using the provided selection
            HtmlNode selectedNode = htmlDoc.DocumentNode.SelectSingleNode(nodeSelection);
            if (selectedNode == null)
            {
                Log.Error($"SelectSingleNodeGetAttributeValue: Could not find HTML node for selection: {nodeSelection}. The HTML structure might have changed or the index is out of bounds.");
                return false;
            }

            // Get the attribute value from the selected node
            attributeValue = selectedNode.GetAttributeValue(attributeSelection, "");

            return true;
        }
    }
}
