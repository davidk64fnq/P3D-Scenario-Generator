using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator.Interfaces
{
    /// <summary>
    /// Defines the contract for an asynchronous HTML parsing service.
    /// </summary>
    public interface IHtmlParser
    {
        /// <summary>
        /// Asynchronously selects a single HTML node and extracts its inner text.
        /// </summary>
        /// <param name="htmlDoc">The HTML document to search.</param>
        /// <param name="nodeSelection">The XPath expression for node selection.</param>
        /// <returns>A tuple containing a boolean indicating success and the extracted inner text.</returns>
        Task<(bool success, string innerText)> SelectSingleNodeInnerTextAsync(HtmlDocument htmlDoc, string nodeSelection);

        /// <summary>
        /// Asynchronously selects a single HTML node and extracts the value of a specified attribute.
        /// </summary>
        /// <param name="htmlDoc">The HTML document to search.</param>
        /// <param name="nodeSelection">The XPath expression for node selection.</param>
        /// <param name="attributeSelection">The name of the attribute to extract.</param>
        /// <returns>A tuple containing a boolean indicating success and the extracted attribute value.</returns>
        Task<(bool success, string attributeValue)> SelectSingleNodeGetAttributeValueAsync(HtmlDocument htmlDoc, string nodeSelection, string attributeSelection);
    }
}
