using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace P3D_Scenario_Generator
{
    class BingImages
    {
        private static readonly string urlBingBase = "https://dev.virtualearth.net/REST/v1/Imagery/Map/";
        private static readonly string urlKey = "&key=95MBlXbvWXxTKi68aPQA~v9ISwpDCewOlSTQWVHFWWA~AtBDc3Ar7Wh3dy-_6ZnRAOYycWbDfnKmTS8aLwaLYrjJ7mfgZ1K_uazGhZMurFtr";
        private static readonly string[] urlZoom = ["14", "15", "16", "16", "16", "16"];
        private static readonly string[] urlMapSize = ["920,135", "300,180", "750,565", "380,232", "380,232", "86,86"];
        private static readonly string[] urlFilename = ["header.jpg", "chart_thumb.jpg", "Charts_01.jpg", "temp1", "temp2", "temp3"];
        private static readonly string[] urlIcon = ["", "", "", "success-icon.png", "failure-icon.png", "exit-icon.png"];
        private static readonly string[] urlIconAdded = ["", "", "", "imgM_c.bmp", "imgM_i.bmp", "exitMission.bmp"];

        internal static void CreateHTMLImages()
        {
            // Download Bing images
            string url;
            for (int index = 0; index < urlZoom.Length; index++)
            {
                url = $"{urlBingBase}Aerial/{Runway.destRwy.AirportLat},{Runway.destRwy.AirportLon}/{urlZoom[index]}?mapSize={urlMapSize[index]}{urlKey}";
                HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\{urlFilename[index]}");
            }

            if (Parameters.SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                GetPhotoTourOverviewImage();
            }

            // Create completion and exit images
            for (int index = 3; index < urlZoom.Length; index++)
            {
                string imageDest = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}";
                using Image image = Image.FromFile(imageDest);
                using (Graphics graphic = Graphics.FromImage(image))
                {
                    Stream iconStream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Images.{urlIcon[index]}");
                    using Image imageIcon = Image.FromStream(iconStream);
                    iconStream.Dispose();
                    graphic.DrawImage(imageIcon, 20, 20);
                    imageIcon.Dispose();
                    graphic.Dispose();
                }
                image.Save($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlIconAdded[index]}");
                image.Dispose();
                File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}");
            }
        }

        internal static void GetCelestialOverviewImage()
        {
            string url;

            string[] words = Parameters.CelestialDestRunway.Split("\t");
            string pushpins = $"&pp={CelestialNav.destinationLat},{CelestialNav.destinationLon};1;{words[0]}";
            int zoomLevel = 5;
            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            }
            PhotoLegParams celestialImage = new();
            bool startInImage = true;
            while (startInImage)
            {
                url = $"{urlBingBase}Road/{CelestialNav.destinationLat},{CelestialNav.destinationLon}/{zoomLevel}?mapSize=960,540{pushpins}{urlKey}";
                HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), "images\\plotImage.jpg");

                // Get meta data
                GetBingMetadata(url, celestialImage);

                // Check whether start position is in Bing imageURL
                double lonDelta = celestialImage.eastEdge - celestialImage.westEdge;
                double latDelta = celestialImage.northEdge - celestialImage.southEdge;
                if ((CelestialNav.midairStartLon < (celestialImage.westEdge + lonDelta * 0.1)) || (CelestialNav.midairStartLon > (celestialImage.eastEdge - lonDelta * 0.1)) ||
                    (CelestialNav.midairStartLat < (celestialImage.southEdge + latDelta * 0.1)) || (CelestialNav.midairStartLon > (celestialImage.northEdge - latDelta * 0.1)))
                {
                    startInImage = false;
                }
                else
                {
                    zoomLevel += 1;
                }
            }

            // Step back one zoom level
            zoomLevel -= 1;
            url = $"{urlBingBase}Road/{CelestialNav.destinationLat},{CelestialNav.destinationLon}/{zoomLevel}?mapSize=960,540{pushpins}{urlKey}";
            HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), "images\\plotImage.jpg");

            // Get meta data
            GetBingMetadata(url, celestialImage);
            Parameters.CelestialImageNorth = celestialImage.northEdge;
            Parameters.CelestialImageEast = celestialImage.eastEdge;
            Parameters.CelestialImageSouth = celestialImage.southEdge;
            Parameters.CelestialImageWest = celestialImage.westEdge;
        }

        internal static void GetPhotoTourLegImages()
        {
            string url;
            string mapArea, mapCentre, mapZoom;
            string[] imageryTypes = ["Aerial", "AerialWithLabels", "Road"];

            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            }

            // For photo tour get Bing route images for each leg
            for (int index = 0; index < PhotoTour.PhotoCount - 1; index++)
            {
                PhotoLegParams curPhoto = PhotoTour.GetPhotoLeg(index);
                mapArea = GetMapBoundingBox(curPhoto, index);
                url = $"{urlBingBase}Aerial?{mapArea}&mapSize=375,375{urlKey}";
                GetBingMetadata(url, PhotoTour.GetPhotoLeg(index));
                mapCentre = $"/{curPhoto.centreLat},{curPhoto.centreLon}";
                for (int typeIndex = 0; typeIndex < imageryTypes.Length; typeIndex++)
                {
                    mapZoom = $"/{curPhoto.zoom}";
                    url = $"{urlBingBase}{imageryTypes[typeIndex]}{mapCentre}{mapZoom}?mapSize={Parameters.PhotoLegWindowSize},{Parameters.PhotoLegWindowSize}{urlKey}";
                    HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\LegRoute_{index:00}_{typeIndex + 1}_zoom1.jpg");
                }
                for (int typeIndex = 0; typeIndex < imageryTypes.Length; typeIndex++)
                {
                    mapZoom = $"/{curPhoto.zoom + 1}";
                    url = $"{urlBingBase}{imageryTypes[typeIndex]}{mapCentre}{mapZoom}?mapSize=1500,1500{urlKey}";
                    HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\LegRoute_{index:00}_{typeIndex + 1}_zoom2.jpg");
                }
                for (int typeIndex = 0; typeIndex < imageryTypes.Length; typeIndex++)
                {
                    mapZoom = $"/{curPhoto.zoom + 2}";
                    url = $"{urlBingBase}{imageryTypes[typeIndex]}{mapCentre}{mapZoom}?mapSize=1500,1500{urlKey}";
                    HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\LegRoute_{index:00}_{typeIndex + 1}_zoom4.jpg");
                }
            }
        }

        private static string GetMapBoundingBox(PhotoLegParams curPhoto, int legIndex)
        {
            string mapArea = "&mapArea=";
            PhotoLegParams nextPhoto = PhotoTour.GetPhotoLeg(legIndex + 1);

            double deltaLat = Math.Abs(curPhoto.latitude - nextPhoto.latitude);
            double deltaLon = Math.Abs(curPhoto.longitude - nextPhoto.longitude);
            double tenPercent;
            double eastEdge, westEdge, northEdge, southEdge, latMiddle, lonMiddle;

            if (deltaLat >= deltaLon) 
            {
                tenPercent = deltaLat * 0.1;
                if (curPhoto.latitude >= nextPhoto.latitude)
                {
                    northEdge = curPhoto.latitude + tenPercent;
                    southEdge = nextPhoto.latitude - tenPercent;
                }
                else
                {
                    northEdge = nextPhoto.latitude + tenPercent;
                    southEdge = curPhoto.latitude - tenPercent;
                }
                if (curPhoto.longitude >= nextPhoto.longitude)
                {
                    lonMiddle = nextPhoto.longitude + (curPhoto.longitude - nextPhoto.longitude) / 2;
                }
                else
                {
                    lonMiddle = curPhoto.longitude + (nextPhoto.longitude - curPhoto.longitude) / 2;
                }
                eastEdge = lonMiddle + deltaLat * 0.5;
                westEdge = lonMiddle - deltaLat * 0.5;
            }
            else 
            {
                tenPercent = deltaLon * 0.1;
                if (curPhoto.longitude >= nextPhoto.longitude)
                {
                    eastEdge = curPhoto.longitude + tenPercent;
                    westEdge = nextPhoto.longitude - tenPercent;
                }
                else
                {
                    eastEdge = nextPhoto.longitude + tenPercent;
                    westEdge = curPhoto.longitude - tenPercent;
                }
                if (curPhoto.latitude >= nextPhoto.latitude)
                {
                    latMiddle = nextPhoto.latitude + (curPhoto.latitude - nextPhoto.latitude) / 2;
                }
                else
                {
                    latMiddle = curPhoto.latitude + (nextPhoto.latitude - curPhoto.latitude) / 2;
                }
                northEdge = latMiddle + deltaLon * 0.5;
                southEdge = latMiddle - deltaLon * 0.5;
            }

            // Handle east/west calculated points exceeding +180/-180 degrees
            if (eastEdge > 180)
            {
                eastEdge -= 360;
            }
            if (westEdge < -180)
            {
                westEdge += 360;
            }

            // Handle north/south calculated points exceeding +90/-90 degrees
            if (northEdge > 90)
            {
                northEdge = 90;
            }
            if (southEdge < -90)
            {
                southEdge = -90;
            }

            return mapArea += $"{southEdge},{westEdge},{northEdge},{eastEdge}";
        }

        internal static void GetPhotoTourOverviewImage()
        {
            string url;

            // For photo tour do pushpin version of Charts_01.jpg
            PhotoLegParams curPhoto;
            string[] words = Parameters.SelectedRunway.Split("\t");
            curPhoto = PhotoTour.GetPhotoLeg(0);
            string pushpins = $"pp={curPhoto.latitude},{curPhoto.longitude};1;{words[0]}&";
            for (int index = 1; index < PhotoTour.PhotoCount - 1; index++)
            {
                curPhoto = PhotoTour.GetPhotoLeg(index);
                pushpins += $"pp={curPhoto.latitude},{curPhoto.longitude};1;{index}&";
            }
            words = Parameters.PhotoDestRunway.Split("\t");
            curPhoto = PhotoTour.GetPhotoLeg(PhotoTour.PhotoCount - 1);
            pushpins += $"pp={curPhoto.latitude},{curPhoto.longitude};1;{words[0]}";

            url = $"{urlBingBase}Aerial?{pushpins}&mapSize={urlMapSize[2]}{urlKey}";
            HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), $"images\\{urlFilename[2]}");
        }

        private static void GetBingMetadata(string url, PhotoLegParams curPhoto)
        {
            url += "&mapMetadata=1&o=xml";
            HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SaveLocation), "images\\temp.xml");
            XmlDocument doc = new();
            doc.Load($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\temp.xml");
            string xml = doc.OuterXml;
            string strXMLPattern = @"xmlns(:\w+)?=""([^""]+)""|xsi(:\w+)?=""([^""]+)""";
            xml = Regex.Replace(xml, strXMLPattern, "");
            doc.LoadXml(xml);
            curPhoto.northEdge = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/BoundingBox/NorthLatitude/text()").Value);
            curPhoto.eastEdge = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/BoundingBox/EastLongitude/text()").Value);
            curPhoto.southEdge = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/BoundingBox/SouthLatitude/text()").Value);
            curPhoto.westEdge = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/BoundingBox/WestLongitude/text()").Value);
            curPhoto.zoom = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/Zoom/text()").Value);
            curPhoto.centreLat = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/MapCenter/Latitude/text()").Value);
            curPhoto.centreLon = Convert.ToDouble(doc.SelectSingleNode("/Response/ResourceSets/ResourceSet/Resources/StaticMapMetadata/MapCenter/Longitude/text()").Value);
            File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\temp.xml");
        }
    }
}
