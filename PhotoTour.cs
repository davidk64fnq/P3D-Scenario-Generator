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
        public string id;
        public double lastLegDist;
        public double latitude;
        public double longitude;
        public double bearing;
    }

    internal class PhotoTour
    {
        private static readonly List<PhotoLegParams> photoLegs = new List<PhotoLegParams>();

        static internal void SetRandomPhotoTour()
        {
            double distance = 9999;
            double bearing = 0;
            double airportLat = 0;
            double airportLon = 0;

            bool continueSearching = true;
            while (continueSearching)
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
                PhotoLegParams photoLeg;
                photoLeg = ExtractLegParams(saveLocation);
                Parameters.SelectedRunway = Runway.GetNearestAirport(photoLeg.latitude, photoLeg.longitude, ref distance, ref airportLat, ref airportLon);
                photoLeg.lastLegDist = distance;
                bearing = MathRoutines.CalcBearing(airportLat, airportLon, photoLeg.latitude, photoLeg.longitude);
                photoLeg.bearing = bearing;
                photoLegs.Add(photoLeg);
                if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist)
                {
                    bool addLegs = true;
                    while (photoLegs.Count < Parameters.MaxNoLegs && addLegs)
                    {
                        // Get next nearest unselected photo
                        url = GetNextLeg(saveLocation, ref distance, ref bearing);
                        if (url != "")
                        {
                            reader.Dispose();
                            File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html");
                            client.DownloadFile(new Uri(url), saveLocation);
                            reader = new StreamReader(saveLocation);
                            photoLeg = ExtractLegParams(saveLocation);
                            photoLeg.lastLegDist = distance;
                            photoLeg.bearing = bearing;
                            photoLegs.Add(photoLeg);
                        }
                        else
                        {
                            addLegs = false;
                        }
                    }
                    if (photoLegs.Count >= Parameters.MinNoLegs)
                    {
                        string destRunway = Runway.GetNearestAirport(photoLegs[^1].latitude, photoLegs[^1].longitude, ref distance, ref airportLat, ref airportLon);
                        if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist)
                        {
                            Parameters.DestRunway = destRunway;
                            Parameters.DestDistance = distance;
                            continueSearching = false;
                        }
                    }
                }
                reader.Dispose();
                File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html");
            }
        }

        static private PhotoLegParams ExtractLegParams(string saveLocation)
        {
            PhotoLegParams photoLeg = new PhotoLegParams();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            photoLeg.image = htmlDoc.DocumentNode.SelectSingleNode("//meta[8]").GetAttributeValue("content", "");
            photoLeg.id = Path.GetFileNameWithoutExtension(photoLeg.image);
            photoLeg.latitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='details'][4]/li[1]/div[@class='dbox'][1]/span[@class='dvalue'][1]").InnerText);
            photoLeg.longitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='details'][4]/li[2]/div[@class='dbox'][1]/span[@class='dvalue'][1]").InnerText);

            return photoLeg;
        }

        static private void ExtractNextLegCoords(HtmlDocument htmlDoc, string id, ref double latitude, ref double longitude)
        {
            string script = htmlDoc.DocumentNode.SelectSingleNode($"//body[1]/script[1]").InnerText;
            int idIndex = script.IndexOf(id);
            script = script.Remove(0, idIndex);
            string[] words = script.Split(',');
            latitude = Convert.ToDouble(words[1]);
            longitude = Convert.ToDouble(words[2]);
        }

        static private string GetNextLeg(string saveLocation, ref double distance, ref double bearing)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            int index = 1;
            while (index <= 15)
            {
                string nextLeg = htmlDoc.DocumentNode.SelectSingleNode($"//li[{index}]/div[@class='dbox'][1]/a[1]").GetAttributeValue("href", "");
                string nextDist = htmlDoc.DocumentNode.SelectSingleNode($"//li[{index}]/div[@class='dbox'][1]/p[@class='undertitletext'][1]").InnerText;
                string[] words = nextDist.Split('/');
                distance = Convert.ToDouble(words[1][..^11]);
                string id = Path.GetFileNameWithoutExtension(nextLeg);
                double nextLat = 0;
                double nextLon = 0;
                ExtractNextLegCoords(htmlDoc, id, ref nextLat, ref nextLon);
                bearing = MathRoutines.CalcBearing(photoLegs[^1].latitude, photoLegs[^1].longitude, nextLat, nextLon);
                int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^1].bearing, bearing);
                if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist && Math.Abs(headingChange) < 90 && photoLegs.FindIndex(leg => nextLeg.Contains(leg.id)) == -1)
                {
                    return nextLeg;
                }
                index++;
            }

            return "";
        }
    }
}
