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
            //    HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SettingsScenarioFolder), $"images\\{urlFilename[index]}");
            }

            // Create completion and exit images
            for (int index = 3; index < urlZoom.Length; index++)
            {
                string imageDest = $"{Parameters.ImageFolder}\\{urlFilename[index]}";
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
                image.Save($"{Parameters.ImageFolder}\\{urlIconAdded[index]}");
                image.Dispose();
                Form.DeleteFile($"{Parameters.ImageFolder}\\{urlFilename[index]}");
            }
        }

        internal static void GetCelestialOverviewImage()
        {
            string url;

            string[] words = Parameters.CelestialDestRunway.Split("\t");
            string pushpins = $"&pp={CelestialNav.destinationLat},{CelestialNav.destinationLon};1;{words[0]}";
            int zoomLevel = 5;
            PhotoLocParams celestialImage = new();
            bool startInImage = true;
            while (startInImage)
            {
                url = $"{urlBingBase}Road/{CelestialNav.destinationLat},{CelestialNav.destinationLon}/{zoomLevel}?mapSize=960,540{pushpins}{urlKey}";
                HttpRoutines.GetWebDoc(url, $"{Parameters.ImageFolder}\\plotImage.jpg");

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
         //   HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SettingsScenarioFolder), "images\\plotImage.jpg");

            // Get meta data
            GetBingMetadata(url, celestialImage);
            Parameters.CelestialImageNorth = celestialImage.northEdge;
            Parameters.CelestialImageEast = celestialImage.eastEdge;
            Parameters.CelestialImageSouth = celestialImage.southEdge;
            Parameters.CelestialImageWest = celestialImage.westEdge;
        }

        private static void GetBingMetadata(string url, PhotoLocParams curPhoto)
        {
            url += "&mapMetadata=1&o=xml";
         //   HttpRoutines.GetWebDoc(url, Path.GetDirectoryName(Parameters.SettingsScenarioFolder), "images\\temp.xml");
            XmlDocument doc = new();
            doc.Load($"{Parameters.ImageFolder}\\temp.xml");
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
            Form.DeleteFile($"{Parameters.ImageFolder}\\temp.xml");
        }
    }
}
