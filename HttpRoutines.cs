﻿using System.Net;

namespace P3D_Scenario_Generator
{
    internal class HttpRoutines
    {
        internal static void GetWebDoc(string url, string saveFolder, string saveFile)
        {
            try
            {
#pragma warning disable SYSLIB0014
                WebClient webClient = new();
                webClient.DownloadFile(url, Path.Combine(saveFolder, saveFile));
                webClient.Dispose();
#pragma warning restore SYSLIB0014
            }
            catch
            {
                string errorMessage = "Encountered issues obtaining web doc, try generating a new scenario";
                MessageBox.Show(errorMessage, "Web document download", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        internal static string GetWebString(string url)
        {
            try
            {
#pragma warning disable SYSLIB0014
                WebClient webClient = new();
                string webString = webClient.DownloadString(url);
                webClient.Dispose();
                return webString;
#pragma warning restore SYSLIB0014
            }
            catch
            {
                string errorMessage = "Encountered issues obtaining web string, try generating a new scenario";
                MessageBox.Show(errorMessage, "Web string download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return string.Empty;
            }
        }
    }
}