using System.Net;
using HtmlAgilityPack;

namespace P3D_Scenario_Generator
{
    internal class HttpRoutines
    {
        internal static HtmlAgilityPack.HtmlDocument GetWebDoc(string url)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = null;
            try
            {
                HtmlWeb web = new();
                htmlDoc = web.Load(url);
            }
            catch
            {
                string errorMessage = $"Encountered issues obtaining web document for the URL \"{url}\"";
                MessageBox.Show(errorMessage, "Web document download", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return htmlDoc;
        }

        internal static void GetWebDoc(string url, string saveFile)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = GetWebDoc(url);
            using FileStream newfs = new(saveFile, FileMode.Create);
            htmlDoc.Save(newfs, htmlDoc.Encoding);
        }

        internal static void GetWebImage(string url, string saveFile)
        {
            try
            {
#pragma warning disable SYSLIB0014
                WebClient webClient = new();
                webClient.DownloadFile(url, saveFile);
                webClient.Dispose();
#pragma warning restore SYSLIB0014
            }
            catch
            {
                string errorMessage = $"Encountered issues obtaining web image for the URL \"{url}\"";
                MessageBox.Show(errorMessage, "Web image download", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        internal static string GetWebString(string url)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = GetWebDoc(url);
            return $"{htmlDoc}";
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
    }
}
