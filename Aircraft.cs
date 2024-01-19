using Microsoft.Win32;

namespace P3D_Scenario_Generator
{
    internal class Aircraft
    {
        static internal string Path { get; private set; }
        static internal double CruiseSpeed { get; set; }
        static internal List<string[]> uiVariations = [];

        static internal List<string> GetUIvariations()
        {
            List<string> uiName = [];

            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Lockheed Martin\\Prepar3D v5");
            OpenFileDialog openFileDialog1 = new()
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
                // Check user has selected a file "aircraft.cfg"
                if (!System.IO.Path.GetFileName(openFileDialog1.FileName).Equals("aircraft.cfg", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Please select an \"aircraft.cfg\" file.", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return uiName;
                }
                // Check whether user has selected AI aircraft
                if (Directory.GetDirectories($"{System.IO.Path.GetDirectoryName(openFileDialog1.FileName)}", "panel*").Length == 0)
                {
                    MessageBox.Show("This is an AI aircraft", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return uiName;
                }
                Path = openFileDialog1.FileName;
                string aircraftCFG = File.ReadAllText(Path);
                using StringReader reader = new(aircraftCFG);
                string currentLine;
                uiVariations.Clear();
                int index = 0;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    if (currentLine.StartsWith("title="))
                    {
                        uiName.Add(currentLine[6..^0].Trim());
                        string[] newUIvariation = [$"{currentLine[6..^0].Trim()}", ""];
                        uiVariations.Add(newUIvariation);
                    }
                    else if (currentLine.StartsWith("texture="))
                    {
                        uiVariations[index][1] = $"{System.IO.Path.GetDirectoryName(openFileDialog1.FileName)}\\Texture.{currentLine[8..^0].Trim()}\\thumbnail.jpg";
                        index++;
                    }
                    else if (currentLine.StartsWith("cruise_speed"))
                    {
                        string[] words1 = currentLine.Split("=");
                        string[] words2 = words1[1].Trim().Split(null);
                        CruiseSpeed = Convert.ToDouble(words2[0].Trim());
                    }
                }
                return uiName;
            }
            return uiName;
        }

        static internal string GetImagename(string selectedAircraft)
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
