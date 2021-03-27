using System;
using System.Collections.Generic;
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

        #region General Tab

        #region Runway selection

        private void ListBoxRunways_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedRunway.Text = ListBoxRunways.SelectedItem.ToString();
        }

        private void TextBoxSearchRunway_TextChanged(object sender, EventArgs e)
        {
            int searchIndex = ListBoxRunways.FindString(TextBoxSearchRunway.Text);
            if (searchIndex != ListBox.NoMatches)
            {
                ListBoxRunways.SelectedIndex = searchIndex;
            }
        }

        private void ButtonRandRunway_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            ListBoxRunways.SelectedIndex = random.Next(0, ListBoxRunways.Items.Count);
        }

        #endregion

        #region Scenario selection

        private void ListBoxScenarioType_Click(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
        }

        private void ListBoxScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            Params parameters = new Params();
            if (ValidateParams(ref parameters) == false)
            {
                return;
            }
            Runway runway = new Runway();
            RunwaysXML.SetRunway(ref runway, parameters);
            ScenarioFXML.GenerateFXMLfile(runway, parameters);
            ScenarioHTML.GenerateOverview(runway, parameters);
        }

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            List<string> uiVariations = Aircraft.GetUIvariations(); 
            if (uiVariations.Count > 0)
            {
                listBoxAircraft.DataSource = uiVariations;
                listBoxAircraft.SelectedIndex = 0;
            }
        }

        #endregion

        #region Save location selection

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

        #endregion

        #region Form validation

        private Boolean ValidateParams(ref Params parameters)
        {
            string errorMsg = "";
            parameters.saveLocation = textBoxSaveLocation.Text;
            if (parameters.saveLocation == "")
            {
                errorMsg += "\n\tSelect a save location";
            }
            if (listBoxAircraft.Items.Count == 0)
            {
                errorMsg += "\n\tSelect an aircraft";
            }
            else
            {
                parameters.selectedAircraft = listBoxAircraft.Items[listBoxAircraft.SelectedIndex].ToString();
            }
            parameters.selectedRunway = TextBoxSelectedRunway.Text;
            parameters.selectedScenario = TextBoxSelectedScenario.Text;
            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #endregion
    }
}
