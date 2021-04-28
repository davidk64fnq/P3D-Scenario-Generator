using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace P3D_Scenario_Generator
{
    public struct PhotoLegParams
    {
        public string image;
        public string locationDesc;
        public double lastLegDist;
        public string id;
        public double latitude;
        public double longitude;
    }

    internal class PhotoTour
    {
        static private List<PhotoLegParams> photoLegs = new List<PhotoLegParams>();

        static internal void SetRandomPhotoTour()
        {
            double distance = 9999;

            while (distance > Parameters.MaxLegDist)
            {
                // Get starting random photo
                using WebClient client = new WebClient();
                string url;
                url = $"https://www.pic2map.com/random.php";
                string saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html";
                client.DownloadFile(new Uri(url), saveLocation);

                // Extract photo leg parameters
                StreamReader reader = new StreamReader(saveLocation);
                photoLegs.Clear();
                ExtractLegParams(saveLocation);
                Parameters.SelectedRunway = Runway.GetNearestAirport(photoLegs[0].latitude, photoLegs[0].longitude, ref distance);

                reader.Dispose();
            //    File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html");
            }
        }

        /// <summary>
        /// Assumes required double in source string is first "double.TryParse" embedded between ">" and "<" characters
        /// </summary>
        static internal double ExtractDouble(string source)
        {
            double number = 0;
            string[] words = source.Split(">");
            foreach (string word in words)
            {
                string[] subWords = word.Split("<");
                if (double.TryParse(subWords[0], out number))
                {
                    break;
                }
            }
            return number;
        }

        static private void ExtractLegParams(string saveLocation)
        {
            PhotoLegParams photoLeg = new PhotoLegParams();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            string latitude = htmlDoc.DocumentNode
                .SelectSingleNode("//span[@class='sp_latitude-longitude']/following::span[@class='dtab']")
                .Attributes["dvalue"].Value;

            System.Windows.Forms.MessageBox.Show(latitude);

            /*
            string photoHTML = "";

            while (!photoHTML.Contains("meta property=\"og: title\""))
            {
                photoHTML = reader.ReadLine();
            }
            while (!photoHTML.Contains("GPS INFORMATION"))
            {
                photoHTML = reader.ReadLine();
            }
            _ = reader.ReadLine();
            photoHTML = reader.ReadLine();
            photoLeg.latitude = ExtractDouble(photoHTML);
            photoHTML = reader.ReadLine();
            photoLeg.longitude = ExtractDouble(photoHTML);
            photoLegs.Add(photoLeg);
            */
        }
    }
}
