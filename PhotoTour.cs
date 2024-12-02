using CoordinateSharp;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Stores information pertaining to a photo location in the photo tour, also used for start and destination airports
    /// </summary>
    public class PhotoLocParams
    {
        public string photoURL;         //  URL of photo this leg travels to
        public string legId;            //  Unique id string used by pic2map for each photo
        public string airportICAO;      //  Only used for start and destination airport instances
        public string airportID;        //  Only used for start and destination airport instances
        public string location;         //  Used to filter on location string for starting photo in tour
        public double forwardDist;      //  Distance from this instance location to next location in photo tour
        public double latitude;         //  Latitude for this instance location
        public double longitude;        //  Longitude for this instance location
        public double forwardBearing;   //  Bearing to get from this instance location to next location in photo tour
        public double northEdge;
        public double eastEdge;
        public double southEdge;
        public double westEdge;         // Delete these once Bing.cs goes
        public double zoom;
        public double centreLat;
        public double centreLon;
    }

    /// <summary>
    /// Stores latitude and longitude boundaries of OSM image depicting coordinates, used by HTML Javascript moving map code
    /// </summary>
    public class MapEdges
    {
        public CoordinatePart north;
        public CoordinatePart east;
        public CoordinatePart south;
        public CoordinatePart west;
    }

    internal class PhotoTour
    {
        private static readonly List<PhotoLocParams> PhotoLocations = [];

        internal static List<MapEdges> PhotoTourLegMapEdges { get; private set; } // Lat/Lon boundaries for each OSM montage leg image

        /// <summary>
        /// Includes start and finish airports
        /// </summary>
        internal static int PhotoCount { get; private set; }

        static internal void SetPhotoTour()
        {
            SetRandomPhotoTour();
            SetPhotoTourOverviewImage();
            SetPhotoTourLocationImage();
            SetAllPhotoTourLegRouteImages();
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport(s) and photo locations/>
        /// </summary>
        static internal void SetPhotoTourOverviewImage()
        {
            List<Tile> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = GetBoundingBoxZoom(tiles, 2, 2, 0, PhotoCount - 1);
            SetPhotoTourOSMtiles(tiles, zoom, 0, PhotoCount - 1);
            boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "Charts_01");
            Drawing.DrawRoute(tiles, boundingBox, "Charts_01");
            Drawing.MakeSquare(boundingBox, "Charts_01", zoom, Con.tileFactor);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetPhotoTourLocationImage()
        {
            List<Tile> tiles = []; // List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile
            BoundingBox boundingBox; // List of x axis and y axis tile numbers that make up montage of tiles to cover set of coords
            int zoom = 15;
            SetPhotoTourOSMtiles(tiles, zoom, 0, 0);
            boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            Drawing.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.xAxis.Count != boundingBox.yAxis.Count)
            {
                Drawing.MakeSquare(boundingBox, "chart_thumb", zoom, Con.locTileFactor);
            }
            if (boundingBox.xAxis.Count == 2)
            {
                Drawing.Resize("chart_thumb.png", 256, 0);
            }
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
        static internal int GetBoundingBoxZoom(List<Tile> tiles, int tilesWidth, int tilesHeight, int startPhotoIndex, int finishPhotoIndex)
        {
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= 18; zoom++)
            {
                tiles.Clear();
                SetPhotoTourOSMtiles(tiles, zoom, startPhotoIndex, finishPhotoIndex);
                boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
                if ((boundingBox.xAxis.Count > tilesWidth) || (boundingBox.yAxis.Count > tilesHeight))
                {
                    return zoom - 1;
                }
            }
            return 18;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a photo tour (all photos plus airports)
        /// </summary>
        /// <param name="tiles">List of OSM tiles defined by x and y tile numbers plus x and y offsets for coordinate on tile</param>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first photo in photo tour</param>
        /// <param name="finishItemIndex">Index of last photo in photo tour</param>
        static internal void SetPhotoTourOSMtiles(List<Tile> tiles, int zoom, int startItemIndex, int finishItemIndex)
        {
            tiles.Clear();
            for (int photoIndex = startItemIndex; photoIndex <= finishItemIndex; photoIndex++)
            {
                tiles.Add(OSM.GetOSMtile(GetPhotoLocation(photoIndex).longitude.ToString(), GetPhotoLocation(photoIndex).latitude.ToString(), zoom));
            }
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for all legs using a montage of OSM tiles that covers the start and finish leg items
        /// </summary>
        static internal void SetAllPhotoTourLegRouteImages()
        {
            PhotoTourLegMapEdges = [];
            for (int itemNo = 0; itemNo <= PhotoCount - 2; itemNo++)
            {
                SetOnePhotoLegRouteImages(itemNo, itemNo + 1);
            }
        }

        /// <summary>
        /// Creates "LegRoute_XX.jpg" images for one leg using a montage of OSM tiles that covers the start and finish leg items 
        /// </summary>
        /// <param name="startItemIndex">Index of start item in <see cref="PhotoLocations"/></param>
        /// <param name="finishItemIndex">Index of finish item in <see cref="PhotoLocations"/></param>
        /// <param name="incStartAirport">Whether to include start airport</param>
        /// <param name="incFinishAirport">Whether to include finish airport</param>
        static internal void SetOnePhotoLegRouteImages(int startItemIndex, int finishItemIndex)
        {
            List<Tile> tiles = [];
            BoundingBox boundingBox;
            BoundingBox zoomInBoundingBox;

            int zoom = GetBoundingBoxZoom(tiles, 2, 2, startItemIndex, finishItemIndex);
            SetPhotoTourOSMtiles(tiles, zoom, startItemIndex, finishItemIndex);
            boundingBox = OSM.GetTilesBoundingBox(tiles, zoom);
            int legNo = startItemIndex + 1; 

            // zoom 1 image
            Drawing.MontageTiles(boundingBox, zoom, $"LegRoute_{legNo:00}_zoom1");
            Drawing.DrawRoute(tiles, boundingBox, $"LegRoute_{legNo:00}_zoom1");
            zoomInBoundingBox = Drawing.MakeSquare(boundingBox, $"LegRoute_{legNo:00}_zoom1", zoom, Con.tileFactor);
            Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom1", "png", "jpg");

            // zoom 2 and 3 images
            for (int inc = 1; inc <= 2; inc++)
            {
                SetPhotoTourOSMtiles(tiles, zoom + inc, startItemIndex, finishItemIndex);
                Drawing.MontageTiles(zoomInBoundingBox, zoom + inc, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                Drawing.DrawRoute(tiles, zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}");
                zoomInBoundingBox = Drawing.MakeSquare(zoomInBoundingBox, $"LegRoute_{legNo:00}_zoom{inc + 1}", zoom + inc, (int)Math.Pow(2, inc + 1));
                Drawing.ConvertImageformat($"LegRoute_{legNo:00}_zoom{inc + 1}", "png", "jpg");
            }

            SetLegImageBoundaries(zoomInBoundingBox, zoom + 3);
        }

        /// <summary>
        /// Calculates leg map photoURL lat/lon boundaries, assumes called in leg number sequence starting with first leg
        /// </summary>
        /// <param name="legNo">Leg numbers run from 0</param>
        /// <param name="boundingBox">The OSM tile numbers for x and y axis that cover the set of coordinates depicted in an image</param>
        /// <param name="zoom">The OSM tile zoom level for the boundingBox</param>
        static internal void SetLegImageBoundaries(BoundingBox boundingBox, int zoom)
        {
            MapEdges legEdges = new();
            Coordinate c;

            // Get the lat/lon coordinates of top left corner of bounding box
            c = OSM.TileNoToLatLon(boundingBox.xAxis[0], boundingBox.yAxis[0], zoom);
            legEdges.north = c.Latitude;
            legEdges.west = c.Longitude;

            // Get the lat/lon coordinates of top left corner of tile immediately below and right of bottom right corner of bounding box
            c = OSM.TileNoToLatLon(boundingBox.xAxis[^1] + 1, boundingBox.yAxis[^1] + 1, zoom);
            legEdges.south = c.Latitude;
            legEdges.east = c.Longitude;

            // Assumes this method called in leg number sequence starting with first leg
            PhotoTourLegMapEdges.Add(legEdges);
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

                // Try to add more photos up to Parameters.MaxNoLegs in total 
                while (legAdded && PhotoLocations.Count < Parameters.MaxNoLegs)
                {
                    legAdded = SetNextLeg();
                }

                // If candidate route has enough legs try to locate a destination airport
                if (PhotoLocations.Count >= Parameters.MinNoLegs)
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
            PhotoLocParams photoLocation;
            PhotoLocParams airportLocation;
            string saveLocation = $"{Parameters.ImageFolder}\\random_pic2map.html";

            // Clear last attempt
            PhotoLocations.Clear();

            // Get starting random photo page
            HttpRoutines.GetWebDoc("https://www.pic2map.com/random.php", saveLocation);
            photoLocation = ExtractPhotoParams(saveLocation);
            if (!photoLocation.location.Contains(Parameters.PhotoLocation))
                return false;

            // Find nearby airport to starting random photo
            airportLocation = GetNearbyAirport(photoLocation.latitude, photoLocation.longitude, Parameters.MinLegDist, Parameters.MaxLegDist);
            if (airportLocation == null)
                return false;
            Parameters.SelectedAirportICAO = airportLocation.airportICAO;
            Parameters.SelectedAirportID = airportLocation.airportID;
            Runway.SetRunway(Runway.startRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            airportLocation.forwardBearing = MathRoutines.GetReciprocalHeading(airportLocation.forwardBearing);
            PhotoLocations.Add(airportLocation);
            PhotoLocations.Add(photoLocation);
            return true;
        }

        /// <summary>
        /// Downloads the next in series of nearest photo pages to the original random photo
        /// and adds another location to the photo tour if it meets distance and bearing constraints
        /// </summary>
        /// <returns>True if another photo location was added to photo tour</returns>
        static internal bool SetNextLeg()
        {
            double distance = 9999;
            double bearing = 0;
            string saveLocation = $"{Parameters.ImageFolder}\\random_pic2map.html";
            string url;
            PhotoLocParams photoLocation;

            // Get next nearest unselected photo
            url = GetNextPhoto(saveLocation, ref distance, ref bearing);
            if (url == "")
                return false;

            // Add forward distance and bearing for this next nearest unselected photo to last selected photo location
            PhotoLocations[^1].forwardDist = distance;
            PhotoLocations[^1].forwardBearing = bearing;

            // Extract next nearest unselected photo location parameters
            File.Delete(saveLocation);
            HttpRoutines.GetWebDoc(url, saveLocation);
            photoLocation = ExtractPhotoParams(saveLocation);
            PhotoLocations.Add(photoLocation);
            return true;
        }

        /// <summary>
        /// Tries to find a nearby airport to the last photo location within the required distance range.
        /// If found adds the airport to the photo tour.
        /// </summary>
        /// <returns>True if last leg to finish airport NOT created</returns>
        static internal bool SetLastLeg()
        {
            PhotoLocParams airportLocation;

            // Find nearby airport to last photo
            airportLocation = GetNearbyAirport(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, 
                Parameters.MinLegDist, Parameters.MaxLegDist);
            File.Delete($"{Parameters.ScenarioFolder}\\random_pic2map.html"); // no longer needed
            if (airportLocation != null)
            {
                int headingChange = MathRoutines.CalcHeadingChange(PhotoLocations[^2].forwardBearing, airportLocation.forwardBearing);
                // Ignore bearing constraint if only one photo, allows backtrack to starting airport
                if ((Math.Abs(headingChange) < Parameters.MaxBearingChange) || (Parameters.MaxNoLegs == 2)) 
                {
                    Runway.SetRunway(Runway.destRwy, airportLocation.airportICAO, airportLocation.airportID);
                    PhotoLocations.Add(airportLocation);
                    PhotoCount = PhotoLocations.Count;
                    GetPhotos();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calls GetNearbyAirport method in Runway class to look for an airport within required distance
        /// from a photo location. Populates an instance of PhotoLocParams with the airport information
        /// </summary>
        /// <param name="queryLat">The photo location latitude</param>
        /// <param name="queryLon">The photo location longitude</param>
        /// <param name="minDist">Minimum required distance between photo location and airport</param>
        /// <param name="maxDist">Maximum required distance between photo location and airport</param>
        /// <returns></returns>
        static internal PhotoLocParams GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            PhotoLocParams photoLocationParams = new();
            Params nearbyAirport = Runway.GetNearbyAirport(queryLat, queryLon, minDist, maxDist);
            if (nearbyAirport == null)
                return null;
            photoLocationParams.legId = nearbyAirport.Id; // So that field isn't null when searching photo tour for photo locations included
            photoLocationParams.airportICAO = nearbyAirport.IcaoId;
            photoLocationParams.airportID = nearbyAirport.Id;
            photoLocationParams.forwardDist = MathRoutines.CalcDistance(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.latitude = nearbyAirport.AirportLat;
            photoLocationParams.longitude = nearbyAirport.AirportLon;
            photoLocationParams.forwardBearing = MathRoutines.CalcBearing(queryLat, queryLon, nearbyAirport.AirportLat, nearbyAirport.AirportLon);
            photoLocationParams.location = nearbyAirport.State + nearbyAirport.City + nearbyAirport.Country;
            return photoLocationParams;
        }

        /// <summary>
        /// Extracts image URL, latitude and longitude as decimal degrees from downloaded pic2map page
        /// </summary>
        /// <param name="saveLocation">Where the downloaded pic2map page is located</param>
        /// <returns></returns>
        static private PhotoLocParams ExtractPhotoParams(string saveLocation)
        {
            PhotoLocParams photoLocation = new();
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(saveLocation);

            photoLocation.photoURL = htmlDoc.DocumentNode.SelectSingleNode("//meta[9]").GetAttributeValue("content", "");
            photoLocation.location = htmlDoc.DocumentNode.SelectSingleNode("//meta[2]").GetAttributeValue("content", "");
            photoLocation.legId = Path.GetFileNameWithoutExtension(photoLocation.photoURL);
            string latitudeSelection = "//ul[@class='details'][4]/li[1]/div[@class='dbox'][1]/span[@class='dvalue'][1]";
            photoLocation.latitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode(latitudeSelection).InnerText);
            string longitudeSelection = "//ul[@class='details'][4]/li[2]/div[@class='dbox'][1]/span[@class='dvalue'][1]";
            photoLocation.longitude = Convert.ToDouble(htmlDoc.DocumentNode.SelectSingleNode(longitudeSelection).InnerText);

            return photoLocation;
        }

        /// <summary>
        /// Extract lat/lon of next photo location from current photo location download file
        /// </summary>
        /// <param name="htmlDoc">The current photo html document</param>
        /// <param name="id">The id of the next photo location to be extracted</param>
        /// <param name="latitude">The extracted latitude of next photo location</param>
        /// <param name="longitude">The extracted longitude of next photo location</param>
        static private void ExtractNextPhotoCoords(HtmlDocument htmlDoc, string id, ref double latitude, ref double longitude)
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
            foreach (PhotoLocParams location in PhotoLocations)
            {
                distance += location.forwardDist;
            }

            return distance;
        }

        /// <summary>
        /// Searches latest photo location page in it's table of 18 nearest photo locations for one that meets
        /// distance and bearing constraints. Also has to check that the candidate next photo has not already
        /// been added to the photo tour.
        /// </summary>
        /// <param name="curPhotoFileLocation">The location of latest photo location download file</param>
        /// <param name="distance">The distance to new photo if found</param>
        /// <param name="bearing">The bearing to new photo if found</param>
        /// <returns>URL of next photo if found else empty string</returns>
        static private string GetNextPhoto(string curPhotoFileLocation, ref double distance, ref double bearing)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(curPhotoFileLocation);

            int index = 1;
            while (index <= 18)
            {
                // Get the URL of candidate next photo
                string nextPhotoSelection = $"//li[{index}]/div[@class='dbox'][1]/a[1]";
                string nextPhotoURL = htmlDoc.DocumentNode.SelectSingleNode(nextPhotoSelection).GetAttributeValue("href", "");

                // Get the distance to candidate next photo
                string nextDistSelection = $"//li[{index}]/div[@class='dbox'][1]/p[@class='undertitletext'][1]";
                string nextDist = htmlDoc.DocumentNode.SelectSingleNode(nextDistSelection).InnerText;
                string[] words = nextDist.Split('/');
                distance = Convert.ToDouble(words[1][..^11]);

                // Get id of candidate next photo - used to check hasn't already been included in tour
                string id = Path.GetFileNameWithoutExtension(nextPhotoURL);

                // Get candidate next photo coordinates and bearing
                double nextLat = 0;
                double nextLon = 0;
                ExtractNextPhotoCoords(htmlDoc, id, ref nextLat, ref nextLon);
                bearing = MathRoutines.CalcBearing(PhotoLocations[^1].latitude, PhotoLocations[^1].longitude, nextLat, nextLon);

                // Calculate candidate next photo bearing change from current photo location
                int headingChange = MathRoutines.CalcHeadingChange(PhotoLocations[^2].forwardBearing, bearing);

                // Does candidate next photo satisfy distance and bearing constraints and has not already been included
                if (distance <= Parameters.MaxLegDist && distance >= Parameters.MinLegDist && 
                    Math.Abs(headingChange) < Parameters.MaxBearingChange && 
                    PhotoLocations.FindIndex(leg => nextPhotoURL.Contains(leg.legId)) == -1)
                {
                    return nextPhotoURL;
                }
                index++;
            }
            return "";
        }

        /// <summary>
        /// Retrieves instance of <see cref="PhotoLocParams"/> from PhotoTour.PhotoLocations
        /// </summary>
        /// <param name="index">Identifies which instance to retrieve</param>
        /// <returns></returns>
        static internal PhotoLocParams GetPhotoLocation(int index)
        {
            return PhotoLocations[index];
        }

        /// <summary>
        /// Downloads all photos to scenario images directory. Checks that each photo width/height is less than 95%
        /// of the monitor it will initially be displayed on. If the width or height exceeds 95% the photo is proportionally
        /// resized to be atleast 40 pixels less than either monitor dimension.
        /// </summary>
        static private void GetPhotos()
        {
            for (int index = 1; index < PhotoLocations.Count - 1; index++)
            {
                HttpRoutines.GetWebImage(PhotoLocations[index].photoURL, $"{Parameters.ImageFolder}\\photo_{index:00}.jpg");

                // Load the photo in order to access its width and height
                string bitmapFilename = $"{Parameters.ImageFolder}\\photo_{index:00}.jpg";
                using Bitmap drawing = new(bitmapFilename);

                // Get percentage of photo width and height relative to monitor dimensions that the photo will be displayed on
                double photoWidthMonitorPercent = drawing.Width / (double)Parameters.PhotoTourPhotoMonitorWidth;
                double photoHeightMonitorPercent = drawing.Height / (double)Parameters.PhotoTourPhotoMonitorHeight;

                // Determin which photo dimension is largest relative to corresponding monitor dimension
                int newSize = 0;
                bool newWidth = false, newHeight = false;
                if (photoWidthMonitorPercent > photoHeightMonitorPercent && photoWidthMonitorPercent > 0.95)
                {
                    newSize = Parameters.PhotoTourPhotoMonitorWidth - 40 ;
                    newWidth = true;
                }
                else if (photoHeightMonitorPercent > 0.95)
                {
                    newSize = Parameters.PhotoTourPhotoMonitorHeight - 40;
                    newHeight = true;
                }

                // If photo is too big (within 95 percent of one of the dimensions) then resize it
                if (newWidth)
                {
                    Drawing.Resize($"{Parameters.ImageFolder}\\photo_{index:00}.jpg", newSize, 0);
                }
                else if (newHeight)
                {
                    Drawing.Resize($"{Parameters.ImageFolder}\\photo_{index:00}.jpg", 0, newSize);
                }
            }
        }

        static internal void SaveSettings()
        {
            Properties.Settings.Default.TextBoxPhotoTourMapMonitorNumber = Parameters.PhotoTourMapMonitorNumber;
            Properties.Settings.Default.TextBoxPhotoTourMapOffset = Parameters.PhotoTourMapOffset;
            Properties.Settings.Default.ListBoxPhotoTourMapAlignment = Parameters.PhotoTourMapAlignment;
            Properties.Settings.Default.TextBoxPhotoTourMapMonitorWidth = Parameters.PhotoTourMapMonitorWidth;
            Properties.Settings.Default.TextBoxPhotoTourMapMonitorHeight = Parameters.PhotoTourMapMonitorHeight;
            Properties.Settings.Default.TextBoxPhotoTourPhotoMonitorNumber = Parameters.PhotoTourPhotoMonitorNumber;
            Properties.Settings.Default.TextBoxPhotoTourPhotoOffset = Parameters.PhotoTourPhotoOffset;
            Properties.Settings.Default.ListBoxPhotoTourPhotoAlignment = Parameters.PhotoTourPhotoAlignment;
            Properties.Settings.Default.TextBoxPhotoTourPhotoMonitorWidth = Parameters.PhotoTourPhotoMonitorWidth;
            Properties.Settings.Default.TextBoxPhotoTourPhotoMonitorHeight = Parameters.PhotoTourPhotoMonitorHeight;
            Properties.Settings.Default.Save();
        }
    }
}
