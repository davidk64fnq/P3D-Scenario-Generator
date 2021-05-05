using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    class BingImages
    {
        private static readonly string urlBingBase = "https://dev.virtualearth.net/REST/v1/Imagery/Map/";
        private static readonly string urlKey = "&key=95MBlXbvWXxTKi68aPQA~v9ISwpDCewOlSTQWVHFWWA~AtBDc3Ar7Wh3dy-_6ZnRAOYycWbDfnKmTS8aLwaLYrjJ7mfgZ1K_uazGhZMurFtr";
        private static readonly string[] urlZoom = { "14", "15", "16", "16", "16", "16" };
        private static readonly string[] urlMapSize = { "920,135", "300,180", "750,565", "380,232", "380,232", "86,86" };
        private static readonly string[] urlFilename = { "header.jpg", "chart_thumb.jpg", "Charts_01.jpg", "temp1", "temp2", "temp3" };
        private static readonly string[] urlIcon = { "", "", "", "success-icon.png", "failure-icon.png", "exit-icon.png" };
        private static readonly string[] urlIconAdded = { "", "", "", "imgM_c.bmp", "imgM_i.bmp", "exitMission.bmp" };

        internal static void CreateHTMLImages()
        {
            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            }

            // Download Bing images
            using WebClient client = new WebClient();
            string url;

            for (int index = 0; index < urlZoom.Length; index++)
            {
                url = $"{urlBingBase}Aerial/{Runway.AirportLat},{Runway.AirportLon}/{urlZoom[index]}?mapSize={urlMapSize[index]}{urlKey}";
                GetBingImage(client, url, $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}");
            }

            if (Parameters.SelectedScenario == Constants.scenarioNames[(int)ScenarioTypes.PhotoTour])
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
                    Stream iconStream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.{urlIcon[index]}");
                    using Image imageIcon = Image.FromStream(iconStream);
                    iconStream.Dispose();
                    graphic.DrawImage(imageIcon, 20, 20);
                }
                image.Save($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlIconAdded[index]}");
            }
            for (int index = 3; index < urlZoom.Length; index++)
            {
                File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}");
            }
        }

        internal static bool GetPhotoTourLegImages()
        {
            using WebClient client = new WebClient();
            string url;
            string[] startRunwayWords = Parameters.SelectedRunway.Split("\t");
            string[] finishRunwayWords = Parameters.DestRunway.Split("\t");

            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            }

            // For photo tour get Bing route image for each leg
            for (int index = 0; index < PhotoTour.PhotoCount - 1; index++)
            {
                PhotoLegParams curPhoto = PhotoTour.GetPhotoLeg(index);
                string wayPoints = $"wp.0={curPhoto.latitude},{curPhoto.longitude};1;{(index == 0 ? startRunwayWords[0] : index.ToString())}&";
                curPhoto = PhotoTour.GetPhotoLeg(index + 1);
                wayPoints += $"wp.1={curPhoto.latitude},{curPhoto.longitude};1;{((index + 1) == (PhotoTour.PhotoCount - 1) ? finishRunwayWords[0] : (index + 1).ToString())}";
                url = $"{urlBingBase}Road/Routes/Walking?{wayPoints}&mapSize={urlMapSize[2]}{urlKey}";
                if (!GetBingImage(client, url, $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\LegRoute_{index + 1}.jpg"))
                {
                    return false;
                }
            }

            return true;
        }

        internal static void GetPhotoTourOverviewImage()
        {
            using WebClient client = new WebClient();
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
            words = Parameters.DestRunway.Split("\t");
            curPhoto = PhotoTour.GetPhotoLeg(PhotoTour.PhotoCount - 1);
            pushpins += $"pp={curPhoto.latitude},{curPhoto.longitude};1;{words[0]}";

            url = $"{urlBingBase}Road/Routes/Walking?{pushpins}&mapSize={urlMapSize[2]}{urlKey}";
            GetBingImage(client, url, $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[2]}");
        }

        private static bool GetBingImage(WebClient client, string url, string saveLocation)
        {
            try
            {
                client.DownloadFile(new Uri(url), saveLocation);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Encountered issues obtaining Bing images, try generating a new scenario", "Bing image download", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return false;
            }

            return true;
        }
    }
}
