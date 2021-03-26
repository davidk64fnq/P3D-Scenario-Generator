using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();

            // Populate ICAO listbox
            ListBoxRunways.DataSource = RunwaysXML.GetICAOids();
            ListBoxScenarioType.DataSource = Constants.scenarios;
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            RunwayStruct runway = new RunwayStruct();
            RunwaysXML.SetRunway(TextBoxSelectedRunway.Text, ref runway);
            if (ValidateForm() == false)
            {
                return;
            }
            ScenarioFXML.GenerateFXMLfile(runway, textBoxSaveLocation.Text, listBoxAircraft.Items[listBoxAircraft.SelectedIndex].ToString());
        }

        private Boolean ValidateForm()
        {
            if (textBoxSaveLocation.Text == "")
            {
                buttonSaveLocation.PerformClick();
                if (textBoxSaveLocation.Text == "")
                {
                    return false;
                }
            }
            if (listBoxAircraft.Items.Count == 0)
            {
                buttonAircraft.PerformClick();
                if (listBoxAircraft.Items.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void TextBoxSearchRunway_TextChanged(object sender, EventArgs e)
        {
            int searchIndex = ListBoxRunways.FindString(TextBoxSearchRunway.Text);
            if (searchIndex >= 0)
            {
                ListBoxRunways.SelectedIndex = searchIndex;
            }
        }

        private void ButtonRandRunway_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            ListBoxRunways.SelectedIndex = random.Next(0, ListBoxRunways.Items.Count);
        }

        private void ButtonSaveLocation_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Title = "Scenario files save location",
                DefaultExt = "fxml",
                Filter = "FXML files (*.fxml)|*.fxml|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\Prepar3D v5 Files",
                RestoreDirectory = false
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxSaveLocation.Text = saveFileDialog1.FileName;
            }
        }

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
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
                if (Directory.GetDirectories($"{Path.GetDirectoryName(openFileDialog1.FileName)}", "panel*").Length == 0){
                    MessageBox.Show("It's an AI aircraft!");
                    return;
                }
                string aircraftCFG = File.ReadAllText(openFileDialog1.FileName);
                using StringReader reader = new StringReader(aircraftCFG);
                string currentLine;
                listBoxAircraft.Items.Clear();
                while ((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine.StartsWith("title="))
                    {
                        listBoxAircraft.Items.Add(currentLine[6..^0]);
                    }
                }
                listBoxAircraft.SelectedIndex = 0;
            }
        }

        private void ListBoxScenarioType_Click(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
        }

        private void ListBoxRunways_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedRunway.Text = ListBoxRunways.SelectedItem.ToString();
        }

        private void ListBoxScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
        }
    }
}
