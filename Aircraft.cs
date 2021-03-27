using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public static class Aircraft
    {
        private static string path;
        private static string cruiseSpeed;
        private static List<string> imagePath = new List<string>();

        internal static List<string> GetUIvariations()
        {
            List<string> uiVariations = new List<string>();

            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Lockheed Martin\\Prepar3D v5");
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Aircraft configuration file location",
                DefaultExt = "cfg",
                Filter = "CFG files (*.cfg)|*.cfg|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = $"{key.GetValue("SetupPath")}SimObjects\\Airplanes",
                RestoreDirectory = false
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Check whether user has selected AI aircraft
                if (Directory.GetDirectories($"{Path.GetDirectoryName(openFileDialog1.FileName)}", "panel*").Length == 0)
                {
                    MessageBox.Show("This is an AI aircraft", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return uiVariations;
                }
                path = openFileDialog1.FileName;
                string aircraftCFG = File.ReadAllText(path);
                using StringReader reader = new StringReader(aircraftCFG);
                string currentLine;
                imagePath.Clear();
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine.StartsWith("title="))
                    {
                        uiVariations.Add(currentLine[6..^0]);
                    }
                    else if (currentLine.StartsWith("texture="))
                    {
                        imagePath.Add($"{Path.GetDirectoryName(openFileDialog1.FileName)}\\Texture.{currentLine[8..^0]}\\thumbnail.jpg");
                    }
                    else if (currentLine.StartsWith("cruise_speed"))
                    {
                        string[] words1 = currentLine.Split("= ");
                        string[] words2 = words1[1].Split(null);
                        cruiseSpeed = words2[0];
                    }
                }
                return uiVariations;
            }
            return uiVariations;
        }
    }
}
