using System;
using System.IO;
using System.Net;

namespace P3D_Scenario_Generator
{
    class PhotoTour
    {
        static internal void SetRandomPhotoTour()
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
            reader.Dispose();

            System.Windows.Forms.MessageBox.Show($"{latitude}/{longitude}");
            Parameters.SelectedRunway = Runway.GetNearestAirport(latitude, longitude);
        }

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
