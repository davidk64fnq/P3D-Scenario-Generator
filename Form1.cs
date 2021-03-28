using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        readonly Aircraft aircraft = new Aircraft();

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
            ScenarioHTML.GenerateOverview(runway, parameters, aircraft);
        }

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            List<string> uiVariations = aircraft.GetUIvariations(); 
            if (uiVariations.Count > 0)
            {
                ListBoxAircraft.DataSource = uiVariations;
                ListBoxAircraft.SelectedIndex = 0;
                SetDefaultCircuitParams();
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
            // General tab
            string errorMsg = "";
            parameters.saveLocation = textBoxSaveLocation.Text;
            if (parameters.saveLocation == "")
            {
                errorMsg += "\n\tSelect a save location";
            }
            if (ListBoxAircraft.Items.Count == 0)
            {
                errorMsg += "\n\tSelect an aircraft";
            }
            else
            {
                parameters.selectedAircraft = ListBoxAircraft.Items[ListBoxAircraft.SelectedIndex].ToString();
            }
            parameters.selectedRunway = TextBoxSelectedRunway.Text;
            parameters.selectedScenario = TextBoxSelectedScenario.Text;

            // Circuit tab
            parameters.baseLeg = Convert.ToDouble(TextBoxCircuitBase.Text);
            parameters.finalLeg = Convert.ToDouble(TextBoxCircuitFinal.Text);
            parameters.height = Convert.ToDouble(TextBoxCircuitHeight.Text);
            parameters.speed = Convert.ToDouble(TextBoxCircuitSpeed.Text);
            parameters.upwindLeg = Convert.ToDouble(TextBoxCircuitUpwind.Text);

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

        #region Circuit Tab

        private void ButtonCircuitDefault_Click(object sender, EventArgs e)
        {
            SetDefaultCircuitParams();
        }

        private void SetDefaultCircuitParams()
        {
            if (ListBoxAircraft.Items.Count == 0)
            {
                MessageBox.Show($"Select an aircraft to calculate default values", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                TextBoxCircuitSpeed.Text = string.Format("{0:0.0}", aircraft.cruiseSpeed);
                TextBoxCircuitHeight.Text = "1000";
                // Upwind distance (miles) approx by speed (knots) * number of minutes / 60 (assume 2 minutes to climb 1000ft at 500ft/min plus 30 seconds to prepare for gate 1)
                TextBoxCircuitUpwind.Text = string.Format("{0:0.0}", aircraft.cruiseSpeed * 2.5 / 60);
                // Base distance (miles) approx by speed (knots) * number of minutes / 60 (assume 30 seconds to prepare for next gate after completing turn)
                TextBoxCircuitBase.Text = string.Format("{0:0.0}", aircraft.cruiseSpeed * 0.5 / 60);
                // Final distance (miles) approx by speed (knots) * number of minutes / 60 (assume 2 minutes to descend 1000ft at 500ft/min plus 30 seconds wriggle room)
                TextBoxCircuitFinal.Text = string.Format("{0:0.0}", aircraft.cruiseSpeed * 2.5 / 60);
            }
        }

        private void TextBoxCircuitBase_TextChanged(object sender, EventArgs e)
        {
            if (!ValidateCircuitParam(TextBoxCircuitBase.Text))
            {
                TextBoxCircuitBase.Text = "0";
            }
        }

        private void TextBoxCircuitFinal_TextChanged(object sender, EventArgs e)
        {
            if (!ValidateCircuitParam(TextBoxCircuitFinal.Text))
            {
                TextBoxCircuitFinal.Text = "0";
            }
        }

        private void TextBoxCircuitHeight_TextChanged(object sender, EventArgs e)
        {
            if (!ValidateCircuitParam(TextBoxCircuitHeight.Text))
            {
                TextBoxCircuitHeight.Text = "0";
            }
        }

        private void TextBoxCircuitSpeed_TextChanged(object sender, EventArgs e)
        {
            if (!ValidateCircuitParam(TextBoxCircuitSpeed.Text))
            {
                TextBoxCircuitSpeed.Text = "0";
            }
        }

        private void TextBoxCircuitUpwind_TextChanged(object sender, EventArgs e)
        {
            if (!ValidateCircuitParam(TextBoxCircuitUpwind.Text))
            {
                TextBoxCircuitUpwind.Text = "0";
            }
        }

        private bool ValidateCircuitParam(string paramValue)
        {
            double paramAsDouble;
            try 
            {
                paramAsDouble = Convert.ToDouble(paramValue);
                if (paramAsDouble < 0)
                {
                    MessageBox.Show($"Numeric value greater than zero expected", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Numeric value expected", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        #endregion

    }
}
