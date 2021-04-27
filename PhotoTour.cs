using System;
using System.IO;
using System.Net;

namespace P3D_Scenario_Generator
{
    class PhotoTour
    {
        static internal void SetRandomPhotoTour()
        {
            double distance = 9999;

            while (distance > Parameters.MaxLegDist)
            {
                // Get random photo
                using WebClient client = new WebClient();
                string url;
                url = $"https://www.pic2map.com/random.php";
                string saveLocation = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\random_pic2map.html";
                client.DownloadFile(new Uri(url), saveLocation);

                // Extract lat/long
                string photoHTML = "";
                StreamReader reader = new StreamReader(saveLocation);
                while (!photoHTML.Contains("GPS INFORMATION"))
                {
                    photoHTML = reader.ReadLine();
                }
                photoHTML = reader.ReadLine();
                photoHTML = reader.ReadLine();
                double latitude = ExtractDouble(photoHTML);
                photoHTML = reader.ReadLine();
                double longitude = ExtractDouble(photoHTML);
                Parameters.SelectedRunway = Runway.GetNearestAirport(latitude, longitude, ref distance);
            System.Windows.Forms.MessageBox.Show($"{distance}");

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
    }
}
