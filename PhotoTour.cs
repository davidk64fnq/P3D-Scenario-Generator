using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace P3D_Scenario_Generator
{
    public class PhotoLegParams
    {
        public string image;
        public string id;
        public double forwardDist;
        public double latitude;
        public double longitude;
        public double forwardBearing;
    }

    internal class PhotoTour
    {
        private static readonly List<PhotoLegParams> photoLegs = new List<PhotoLegParams>();

        /// <summary>
        /// Includes start and finish airports
        /// </summary>
        internal static int PhotoCount { get; private set; }

        static internal void SetRandomPhotoTour()
        {
            double distance = 9999;
            double bearing = 0;
            string saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html";
            string url;
            WebClient client = new WebClient();
            PhotoLegParams photoLeg;
            PhotoLegParams airportLeg;

            bool continueSearching = true;
            while (continueSearching)
            {
                // Clear last attempt
                photoLegs.Clear();

                // Get starting random photo
                client.DownloadFile(new Uri("https://www.pic2map.com/random.php"), saveLocation);

                // Extract starting random photo leg parameters
                StreamReader reader = new StreamReader(saveLocation);
                photoLeg = ExtractLegParams(saveLocation);

                // Find nearest airport to starting random photo
                airportLeg = new PhotoLegParams();
                Parameters.SelectedRunway = Runway.GetNearestAirport(photoLeg.latitude, photoLeg.longitude, ref airportLeg.forwardDist, ref airportLeg.latitude, ref airportLeg.longitude);
                airportLeg.forwardBearing = MathRoutines.CalcBearing(airportLeg.latitude, airportLeg.longitude, photoLeg.latitude, photoLeg.longitude);
                airportLeg.id = "ZZZZZZ";
                photoLegs.Add(airportLeg);
                photoLegs.Add(photoLeg);

                if (airportLeg.forwardDist <= Parameters.MaxLegDist && airportLeg.forwardDist >= Parameters.MinLegDist)
                {
                    bool addLegs = true;
                    while (photoLegs.Count - 1 < Parameters.MaxNoLegs && addLegs)
                    {
                        // Get next nearest unselected photo
                        url = GetNextLeg(saveLocation, ref distance, ref bearing);
                        if (url != "")
                        {
                            // Add forward distance and bearing for this next nearest unselected photo to last selected photo location
                            photoLegs[^1].forwardDist = distance;
                            photoLegs[^1].forwardBearing = bearing;

                            // Extract next nearest unselected photo leg parameters
                            reader.Dispose();
                            File.Delete(saveLocation);
                            client.DownloadFile(new Uri(url), saveLocation);
                            reader = new StreamReader(saveLocation);
                            photoLeg = ExtractLegParams(saveLocation);
                            photoLegs.Add(photoLeg);
                        }
                        else
                        {
                            addLegs = false;
                        }
                    }

                    // If candidate route has enough legs try to locate a destination airport
                    if (photoLegs.Count - 1 >= Parameters.MinNoLegs)
                    {
                        airportLeg = new PhotoLegParams();
                        Parameters.DestRunway = Runway.GetNearestAirport(photoLegs[^1].latitude, photoLegs[^1].longitude, ref distance, ref airportLeg.latitude, ref airportLeg.longitude);

                        // Add forward distance and bearing for destination airport to last selected photo location
                        photoLegs[^1].forwardDist = distance;
                        photoLegs[^1].forwardBearing = MathRoutines.CalcBearing(photoLegs[^1].latitude, photoLegs[^1].longitude, airportLeg.latitude, airportLeg.longitude);
                        int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^2].forwardBearing, photoLegs[^1].forwardBearing);
                        if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist && Math.Abs(headingChange) < Parameters.MaxBearingChange)
                        { 
                            photoLegs.Add(airportLeg);
                            PhotoCount = photoLegs.Count;
                            if (BingImages.GetPhotoTourLegImages())
                            {
                                GetPhotos();
                                continueSearching = false;
                            }
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

        static internal double GetPhotoTourDistance()
        {
            double distance = 0;
            foreach (PhotoLegParams leg in photoLegs)
            {
                distance += leg.forwardDist;
            }

            return distance;
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
                int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^2].forwardBearing, bearing);
                if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist && Math.Abs(headingChange) < Parameters.MaxBearingChange && photoLegs.FindIndex(leg => nextLeg.Contains(leg.id)) == -1)
                {
                    return nextLeg;
                }
                index++;
            }

            return "";
        }
        
        static internal PhotoLegParams GetPhotoLeg(int index)
        {
            return photoLegs[index];
        }
       
        static private void GetPhotos()
        {
            using WebClient client = new WebClient();
            string url;
            string saveLocation;

            for (int index = 1; index < photoLegs.Count - 1; index++)
            {
                url = photoLegs[index].image;
                saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\photo_{index}.jpg";
                client.DownloadFile(new Uri(url), saveLocation);
            }
        }
    }
}
