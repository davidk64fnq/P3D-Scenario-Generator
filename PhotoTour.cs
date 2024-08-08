using System.Drawing.Imaging;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator
{
    public class PhotoLegParams
    {
        public string image;
        public string id;
        public string icao;
        public double forwardDist;
        public double latitude;
        public double longitude;
        public double forwardBearing;
        public double northEdge;
        public double eastEdge;
        public double southEdge;
        public double westEdge;
        public double zoom;
        public double centreLat;
        public double centreLon;
    }

    internal class PhotoTour
    {
        private static readonly List<PhotoLegParams> photoLegs = [];

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
            PhotoLegParams photoLeg;
            PhotoLegParams airportLeg;

            bool continueSearching = true;
            while (continueSearching)
            {
                // Clear last attempt
                photoLegs.Clear();

                // Get starting random photo page
                HttpRoutines.GetWebDoc("https://www.pic2map.com/random.php", Path.GetDirectoryName(Parameters.SaveLocation), "random_pic2map.html");
                photoLeg = ExtractLegParams(saveLocation);

                // Find nearby airport to starting random photo
                airportLeg = GetNearbyAirport(photoLeg.latitude, photoLeg.longitude, Parameters.MinLegDist, Parameters.MaxLegDist);
                if (airportLeg == null)
                    continue;
                Parameters.SelectedRunway = $"{airportLeg.icao}\t({airportLeg.id})";
                Runway.SetRunway(Runway.startRwy, "start");
                airportLeg.forwardBearing = MathRoutines.GetReciprocalHeading(airportLeg.forwardBearing);
                photoLegs.Add(airportLeg);
                photoLegs.Add(photoLeg);

                // Try to add more photos up to Parameters.MaxNoLegs - 1 in total (last leg is to destination airport)
                while (photoLegs.Count < Parameters.MaxNoLegs - 1)
                {
                    // Get next nearest unselected photo
                    url = GetNextLeg(saveLocation, ref distance, ref bearing);
                    if (url == "")
                        break;

                    // Add forward distance and bearing for this next nearest unselected photo to last selected photo location
                    photoLegs[^1].forwardDist = distance;
                    photoLegs[^1].forwardBearing = bearing;

                    // Extract next nearest unselected photo leg parameters
                    File.Delete(saveLocation);
                    HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), "random_pic2map.html");
                    photoLeg = ExtractLegParams(saveLocation);
                    photoLegs.Add(photoLeg);
                }

                // If candidate route has enough legs try to locate a destination airport
                if (photoLegs.Count + 1 >= Parameters.MinNoLegs)
                {
                    // Find nearby airport to last photo
                    airportLeg = GetNearbyAirport(photoLegs[^1].latitude, photoLegs[^1].longitude, Parameters.MinLegDist, Parameters.MaxLegDist);
                    if (airportLeg != null)
                    {
                        int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^2].forwardBearing, airportLeg.forwardBearing);
                        if ((Math.Abs(headingChange) < Parameters.MaxBearingChange) || (Parameters.MaxNoLegs == 3))
                        {
                            Parameters.PhotoDestRunway = $"{airportLeg.icao}\t({airportLeg.id})";
                            Runway.SetRunway(Runway.destRwy, "destination");
                            photoLegs.Add(airportLeg);
                            PhotoCount = photoLegs.Count;
                            BingImages.GetPhotoTourLegImages();
                            SetLegRouteMarkers();
                            GetPhotos();
                            continueSearching = false;
                        }
                    }
                }
                File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html");
            }
        }

        static internal PhotoLegParams GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            PhotoLegParams photoLegParams = new();
            Params nearbyAirport = Runway.GetNearbyAirport(queryLat, queryLon, minDist, maxDist);
            if (nearbyAirport == null)
                return null;
            photoLegParams.id = nearbyAirport.Id;
            photoLegParams.icao = nearbyAirport.IcaoId;
            photoLegParams.forwardDist = MathRoutines.CalcDistance(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLegParams.latitude = nearbyAirport.AirportLat;
            photoLegParams.longitude = nearbyAirport.AirportLon;
            photoLegParams.forwardBearing = MathRoutines.CalcBearing(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            return photoLegParams;
        }

        static internal void SetLegRouteMarkers()
        {
            for (int index = 0; index < PhotoCount - 1; index++)
            {
                // Draw starting marker on zoom1 maps
                SetLegRouteMarker(index, index, 1, "_zoom1");

                // Draw finishing marker on zoom1 maps
                if (index > 0)
                {
                    SetLegRouteMarker(index, index - 1, 1, "_zoom1");
                }

                // Draw starting marker on zoom2 maps
                SetLegRouteMarker(index, index, 2, "_zoom2");

                // Draw finishing marker on zoom2 maps
                if (index > 0)
                {
                    SetLegRouteMarker(index, index - 1, 2, "_zoom2");
                }

                // Draw starting marker on zoom4 maps
                SetLegRouteMarker(index, index, 4, "_zoom4");

                // Draw finishing marker on zoom4 maps
                if (index > 0)
                {
                    SetLegRouteMarker(index, index - 1, 4, "_zoom4");
                }
            }

            // Draw finishing marker on last zoom1 maps
            SetLegRouteMarker(PhotoCount - 1, PhotoCount - 2, 1, "_zoom1");

            // Draw finishing marker on last zoom2 maps
            SetLegRouteMarker(PhotoCount - 1, PhotoCount - 2, 2, "_zoom2");

            // Draw finishing marker on last zoom4 maps
            SetLegRouteMarker(PhotoCount - 1, PhotoCount - 2, 4, "_zoom4");
        }

        static internal void SetLegRouteMarker(int sourcePhotoIndex, int destPhotoIndex, int zoomFactor, string zoomSuffix)
        {
            Bitmap bm;
            double latDeltaAbs;
            double pixelSize;
            int yCoord;
            int xCoord;

            // Calculate circle radius in pixels
            PhotoLegParams sourcePhoto = GetPhotoLeg(sourcePhotoIndex);
            PhotoLegParams destPhoto = GetPhotoLeg(destPhotoIndex);
            if (zoomFactor > 1)
            {
                latDeltaAbs = Math.Abs(destPhoto.northEdge - destPhoto.southEdge) * 4 / zoomFactor;
                pixelSize = latDeltaAbs * Con.degreeLatFeet / 1500;
            }
            else
            {
                latDeltaAbs = Math.Abs(destPhoto.northEdge - destPhoto.southEdge) * (1 + (Parameters.PhotoLegWindowSize - 375) / 375);
                pixelSize = latDeltaAbs * Con.degreeLatFeet / Parameters.PhotoLegWindowSize;
            }
            int markerRadiusPixels = Convert.ToInt32(Parameters.HotspotRadius * 3.2808399 / pixelSize);

            // Calculate y coordinate of top left corner of bounding box
            double latDeltaCentre = destPhoto.centreLat - sourcePhoto.latitude;
            if (zoomFactor > 1)
            {
                double latDeltaPixels = latDeltaCentre / latDeltaAbs * 1500;
                yCoord = Convert.ToInt32(750 + latDeltaPixels) - markerRadiusPixels;
            }
            else
            {
                double latDeltaPixels = latDeltaCentre / latDeltaAbs * Parameters.PhotoLegWindowSize;
                yCoord = Convert.ToInt32(Parameters.PhotoLegWindowSize / 2 + latDeltaPixels) - markerRadiusPixels;
            }

            // Calculate x coordinate of top left corner of bounding box
            double longDeltaCentre = sourcePhoto.longitude - destPhoto.centreLon;
            if (zoomFactor > 1)
            {
                double longDeltaAbs = Math.Abs(destPhoto.westEdge - destPhoto.eastEdge) * 4 / zoomFactor;
                double longDeltaPixels = longDeltaCentre / longDeltaAbs * 1500;
                xCoord = Convert.ToInt32(750 + longDeltaPixels) - markerRadiusPixels;
            }
            else
            {
                double longDeltaAbs = Math.Abs(destPhoto.westEdge - destPhoto.eastEdge) * (1 + (Parameters.PhotoLegWindowSize - 375) / 375);
                double longDeltaPixels = longDeltaCentre / longDeltaAbs * Parameters.PhotoLegWindowSize;
                xCoord = Convert.ToInt32(Parameters.PhotoLegWindowSize / 2 + longDeltaPixels) - markerRadiusPixels;
            }

            // Draw starting marker on overview maps
            for (int typeIndex = 0; typeIndex < 3; typeIndex++)
            {
                using (FileStream fs = new($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\LegRoute_{destPhotoIndex:00}_{typeIndex + 1}{zoomSuffix}.jpg", FileMode.Open))
                {
                    Bitmap bitmap = new(fs);
                    bm = bitmap;
                    fs.Close();
                }
                Graphics g = Graphics.FromImage(bm);
                Pen pen = new(Color.Magenta, 3);
                g.DrawEllipse(pen, xCoord, yCoord, markerRadiusPixels * 2, markerRadiusPixels * 2);
                bm.Save($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\LegRoute_{destPhotoIndex:00}_{typeIndex + 1}{zoomSuffix}.jpg", ImageFormat.Jpeg);
                bm.Dispose();
                g.Dispose();
            }
        }

        static private PhotoLegParams ExtractLegParams(string saveLocation)
        {
            PhotoLegParams photoLeg = new();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            photoLeg.image = htmlDoc.DocumentNode.SelectSingleNode("//meta[8]").GetAttributeValue("content", "");
            photoLeg.id = Path.GetFileNameWithoutExtension(photoLeg.image);
            photoLeg.latitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='details'][4]/li[1]/div[@class='dbox'][1]/span[@class='dvalue'][1]").InnerText);
            photoLeg.latitude = MathRoutines.RoundDecDegreesToNearestSec(photoLeg.latitude);
            photoLeg.longitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='details'][4]/li[2]/div[@class='dbox'][1]/span[@class='dvalue'][1]").InnerText);
            photoLeg.longitude = MathRoutines.RoundDecDegreesToNearestSec(photoLeg.longitude);

            return photoLeg;
        }

        static private void ExtractNextLegCoords(HtmlDocument htmlDoc, string id, ref double latitude, ref double longitude)
        {
            string script = htmlDoc.DocumentNode.SelectSingleNode($"//body[1]/script[1]").InnerText;
            int idIndex = script.IndexOf(id);
            script = script.Remove(0, idIndex);
            string[] words = script.Split(',');
            latitude = MathRoutines.RoundDecDegreesToNearestSec(Convert.ToDouble(words[1]));
            longitude = MathRoutines.RoundDecDegreesToNearestSec(Convert.ToDouble(words[2]));
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
            for (int index = 1; index < photoLegs.Count - 1; index++)
            {
                HttpRoutines.GetWebDoc(photoLegs[index].image, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\photo_{index:00}.jpg");
            }
        }
    }
}
