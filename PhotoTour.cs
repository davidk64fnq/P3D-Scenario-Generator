using System.Drawing.Imaging;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator
{
    public class PhotoLegParams
    {
        public string imageURL;
        public string legId;
        public string airportICAO;
        public string airportID;
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
        internal static int xAxis = 0, yAxis = 1; // Used in bounding box to denote lists that store OSM xTile and yTile reference numbers

        /// <summary>
        /// Includes start and finish airports
        /// </summary>
        internal static int PhotoCount { get; private set; }

        static internal void SetPhotoTour()
        {
            SetRandomPhotoTour();
            SetPhotoTourOverviewImage();
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and photo locations/>
        /// </summary>
        static internal void SetPhotoTourOverviewImage()
        {
            List<List<int>> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            List<List<int>> boundingBox = []; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = GetBoundingBoxZoom(tiles, 2, 2, 1, PhotoCount - 2, true, true);
            SetPhotoTourOSMtiles(tiles, zoom, 1, PhotoCount - 2, true, true);
            OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, 2);
        }

        /// <summary>
        /// Works out most zoomed in level that includes all photo locations specified by startPhotoIndex and finishPhotoIndex, 
        /// plus airport(s) where the montage of OSM tiles doesn't exceed tilesWidth and tilesHeight in size
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="tilesWidth">Maximum number of tiles allowed for x axis</param>
        /// <param name="tilesHeight">Maximum number of tiles allowed for x axis</param>
        /// <param name="startPhotoIndex">Index of first photo in photo tour</param>
        /// <param name="finishPhotoIndex">Index of last photo in photo tour</param>
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<List<int>> tiles, int tilesWidth, int tilesHeight,
            int startPhotoIndex, int finishPhotoIndex, bool incStartAirport, bool incFinishAirport)
        {
            List<List<int>> boundingBox = [];
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetPhotoTourOSMtiles(tiles, zoom, startPhotoIndex, finishPhotoIndex, incStartAirport, incFinishAirport);
                boundingBox.Clear();
                OSM.GetTilesBoundingBox(tiles, boundingBox, zoom);
                if ((boundingBox[xAxis].Count > tilesWidth) || (boundingBox[yAxis].Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a photo tour (all photos plus airport)
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first photo in photo tour</param>
        /// <param name="finishItemIndex">Index of last photo in photo tour</param>
        /// <param name="incStartAirport">Whether to include the start airport</param>
        /// <param name="incFinishAirport">Whether to include the finish airport</param>
        static internal void SetPhotoTourOSMtiles(List<List<int>> tiles, int zoom, int startItemIndex, int finishItemIndex,
            bool incStartAirport, bool incFinishAirport)
        {
            tiles.Clear();
            if (incStartAirport)
            {
                tiles.Add(OSM.GetOSMtile(Runway.startRwy.AirportLon.ToString(), Runway.startRwy.AirportLat.ToString(), zoom));
            }
            for (int photoIndex = startItemIndex; photoIndex <= finishItemIndex; photoIndex++)
            {
                tiles.Add(OSM.GetOSMtile(GetPhotoLeg(photoIndex).longitude.ToString(), GetPhotoLeg(photoIndex).latitude.ToString(), zoom));
            }
            if (incFinishAirport)
            {
                tiles.Add(OSM.GetOSMtile(Runway.destRwy.AirportLon.ToString(), Runway.destRwy.AirportLat.ToString(), zoom));
            }
        }

        /// <summary>
        /// Creates the photo tour by finding a random pic2map photo page with nearby airport, then
        /// looking for a series of photo pages within bearing and distance constraints, then that 
        /// a finish airport is the required distance range from the last photo location.
        /// </summary>
        static internal void SetRandomPhotoTour()
        {
            bool continueSearching = true;
            while (continueSearching)
            {
                // Try to find a random photo location with nearby airport in required range
                bool legAdded = SetFirstLeg();

                // Try to add more photos up to Parameters.MaxNoLegs + 1 in total (last leg is to destination airport)
                while (legAdded && photoLegs.Count < Parameters.MaxNoLegs)
                {
                    legAdded = SetNextLeg();
                }

                // If candidate route has enough legs try to locate a destination airport
                if (photoLegs.Count >= Parameters.MinNoLegs)
                {
                    continueSearching = SetLastLeg();
                }
            }
        }

        /// <summary>
        /// Downloads a random photo page from pic2map site and tries to find nearby airport within
        /// the required distance range. If found adds the starting airport and first photo to the photo tour.
        /// </summary>
        /// <returns>True if first leg created</returns>
        static internal bool SetFirstLeg()
        {
            PhotoLegParams photoLeg;
            PhotoLegParams airportLeg;
            string saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html";

            // Clear last attempt
            photoLegs.Clear();

            // Get starting random photo page
            HttpRoutines.GetWebDoc("https://www.pic2map.com/random.php", Path.GetDirectoryName(Parameters.SaveLocation), "random_pic2map.html");
            photoLeg = ExtractLegParams(saveLocation);

            // Find nearby airport to starting random photo
            airportLeg = GetNearbyAirport(photoLeg.latitude, photoLeg.longitude, Parameters.MinLegDist, Parameters.MaxLegDist);
            if (airportLeg == null)
                return false;
            Parameters.SelectedRunway = $"{airportLeg.airportICAO}\t({airportLeg.airportID})";
            Runway.SetRunway(Runway.startRwy, "start");
            airportLeg.forwardBearing = MathRoutines.GetReciprocalHeading(airportLeg.forwardBearing);
            photoLegs.Add(airportLeg);
            photoLegs.Add(photoLeg);
            return true;
        }

        /// <summary>
        /// Downloads the next in series of nearest photo pages to the original random photo
        /// and adds another leg to the photo tour if it meets distance and bearing constraints
        /// </summary>
        /// <returns>True if another photo location was added to photo tour</returns>
        static internal bool SetNextLeg()
        {
            double distance = 9999;
            double bearing = 0;
            string saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html";
            string url;
            PhotoLegParams photoLeg;

            // Get next nearest unselected photo
            url = GetNextLeg(saveLocation, ref distance, ref bearing);
            if (url == "")
                return false;

            // Add forward distance and bearing for this next nearest unselected photo to last selected photo location
            photoLegs[^1].forwardDist = distance;
            photoLegs[^1].forwardBearing = bearing;

            // Extract next nearest unselected photo leg parameters
            File.Delete(saveLocation);
            HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), "random_pic2map.html");
            photoLeg = ExtractLegParams(saveLocation);
            photoLegs.Add(photoLeg);
            return true;
        }

        /// <summary>
        /// Tries to find a nearby airport to the last photo location within the required distance range.
        /// If found adds the airport to the photo tour.
        /// </summary>
        /// <returns>True if last leg to finish airport NOT created</returns>
        static internal bool SetLastLeg()
        {
            PhotoLegParams airportLeg;

            // Find nearby airport to last photo
            airportLeg = GetNearbyAirport(photoLegs[^1].latitude, photoLegs[^1].longitude, Parameters.MinLegDist, Parameters.MaxLegDist);
            File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html"); // no longer needed
            if (airportLeg != null)
            {
                int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^2].forwardBearing, airportLeg.forwardBearing);
                if ((Math.Abs(headingChange) < Parameters.MaxBearingChange) || (Parameters.MaxNoLegs == 2))
                {
                    Parameters.PhotoDestRunway = $"{airportLeg.airportICAO}\t({airportLeg.airportID})";
                    Runway.SetRunway(Runway.destRwy, "destination");
                    photoLegs.Add(airportLeg);
                    PhotoCount = photoLegs.Count;
                //    BingImages.GetPhotoTourLegImages();
                //    SetLegRouteMarkers();
                    GetPhotos();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calls GetNearbyAirport method in Runway class to llok for an airport within required distance
        /// from a photo location. Populates an instance of PhotoLegParams with the airport information
        /// </summary>
        /// <param name="queryLat">The photo location latitude</param>
        /// <param name="queryLon">The photo location longitude</param>
        /// <param name="minDist">Minimum required distance between photo location and airport</param>
        /// <param name="maxDist">Maximum required distance between photo location and airport</param>
        /// <returns></returns>
        static internal PhotoLegParams GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            PhotoLegParams photoLegParams = new();
            Params nearbyAirport = Runway.GetNearbyAirport(queryLat, queryLon, minDist, maxDist);
            if (nearbyAirport == null)
                return null;
            photoLegParams.legId = nearbyAirport.Id; // So that field isn't null when searching photo tour for photo locations included
            photoLegParams.airportICAO = nearbyAirport.IcaoId;
            photoLegParams.airportID = nearbyAirport.Id;
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

        /// <summary>
        /// Extracts imageURL URL, latitude and longitude as decimal degrees from downloaded pic2map page
        /// </summary>
        /// <param name="saveLocation">Where the downloaded pic2map page is located</param>
        /// <returns></returns>
        static private PhotoLegParams ExtractLegParams(string saveLocation)
        {
            PhotoLegParams photoLeg = new();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            photoLeg.imageURL = htmlDoc.DocumentNode.SelectSingleNode("//meta[8]").GetAttributeValue("content", "");
            photoLeg.legId = Path.GetFileNameWithoutExtension(photoLeg.imageURL);
            string latitudeSelection = "//ul[@class='details'][4]/li[1]/div[@class='dbox'][1]/span[@class='dvalue'][1]";
            photoLeg.latitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode(latitudeSelection).InnerText);
            string longitudeSelection = "//ul[@class='details'][4]/li[2]/div[@class='dbox'][1]/span[@class='dvalue'][1]";
            photoLeg.longitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode(longitudeSelection).InnerText);

            return photoLeg;
        }

        /// <summary>
        /// Extract lat/lon of next leg photo location from current photo location download file
        /// </summary>
        /// <param name="htmlDoc">The current photo html document</param>
        /// <param name="id">The id of the next photo location to be extracted</param>
        /// <param name="latitude">The extracted latitude of next photo location</param>
        /// <param name="longitude">The extracted longitude of next photo location</param>
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

        /// <summary>
        /// Searches latest photo location page in it's table of 18 nearest photo locations for one that meets
        /// distance and bearing constraints. Also has to check that the candidate next leg has not already
        /// been added to the photo tour.
        /// </summary>
        /// <param name="curLegFileLocation">The location of latest photo location download file</param>
        /// <param name="distance">The distance to new leg if found</param>
        /// <param name="bearing">The bearing to new leg if found</param>
        /// <returns>URL of next leg if found else empty string</returns>
        static private string GetNextLeg(string curLegFileLocation, ref double distance, ref double bearing)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(curLegFileLocation);

            int index = 1;
            while (index <= 18)
            {
                // Get the URL of candidate next leg
                string nextLegSelection = $"//li[{index}]/div[@class='dbox'][1]/a[1]";
                string nextLegURL = htmlDoc.DocumentNode.SelectSingleNode(nextLegSelection).GetAttributeValue("href", "");

                // Get the distance to candidate next leg
                string nextDistSelection = $"//li[{index}]/div[@class='dbox'][1]/p[@class='undertitletext'][1]";
                string nextDist = htmlDoc.DocumentNode.SelectSingleNode(nextDistSelection).InnerText;
                string[] words = nextDist.Split('/');
                distance = Convert.ToDouble(words[1][..^11]);

                // Get id of candidate next leg - used to check hasn't already been included in tour
                string id = Path.GetFileNameWithoutExtension(nextLegURL);

                // Get candidate next leg coordinates and bearing
                double nextLat = 0;
                double nextLon = 0;
                ExtractNextLegCoords(htmlDoc, id, ref nextLat, ref nextLon);
                bearing = MathRoutines.CalcBearing(photoLegs[^1].latitude, photoLegs[^1].longitude, nextLat, nextLon);

                // Calculate candidate next leg bearing change from current photo location
                int headingChange = MathRoutines.CalcHeadingChange(photoLegs[^2].forwardBearing, bearing);

                // Does candidate next leg satisfy distance and bearing constraints and has not already been included
                if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist && 
                    Math.Abs(headingChange) < Parameters.MaxBearingChange && 
                    photoLegs.FindIndex(leg => nextLegURL.Contains(leg.legId)) == -1)
                {
                    return nextLegURL;
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
                HttpRoutines.GetWebDoc(photoLegs[index].imageURL, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\photo_{index:00}.jpg");
            }
        }
    }
}
