using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public class Aircraft
    {
        internal string path;
        internal double cruiseSpeed;
        internal List<string[]> uiVariations = new List<string[]>();

        internal List<string> GetUIvariations()
        {
            List<string> uiName = new List<string>();

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
                    return uiName;
                }
                path = openFileDialog1.FileName;
                string aircraftCFG = File.ReadAllText(path);
                using StringReader reader = new StringReader(aircraftCFG);
                string currentLine;
                uiVariations.Clear();
                int index = 0;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine.StartsWith("title="))
                    {
                        uiName.Add(currentLine[6..^0]);
                        string[] newUIvariation = { $"{currentLine[6..^0]}", "" };
                        uiVariations.Add(newUIvariation);
                    }
                    else if (currentLine.StartsWith("texture="))
                    {
                        uiVariations[index][1] = $"{Path.GetDirectoryName(openFileDialog1.FileName)}\\Texture.{currentLine[8..^0]}\\thumbnail.jpg";
                        index++;
                    }
                    else if (currentLine.StartsWith("cruise_speed"))
                    {
                        string[] words1 = currentLine.Split("= ");
                        string[] words2 = words1[1].Split(null);
                        cruiseSpeed = Convert.ToDouble(words2[0]);
                    }
                }
                return uiName;
            }
            return uiName;
        }

        internal string GetImagename(string selectedAircraft)
        {
            for (int index = 0; index < uiVariations.Count; index++)
            {
                if (uiVariations[index][0] == selectedAircraft)
                {
                    return uiVariations[index][1];
                }
            }
            return "";
        }
    }
}
