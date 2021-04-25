using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();

            // Populate ICAO listbox
            ListBoxRunways.DataSource = Runway.GetICAOids();
            ListBoxScenarioType.DataSource = Constants.scenarios;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.circuitTab.jpg");
            PictureBoxCircuit.Image = new Bitmap(stream);

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
            if (Parameters.SetParams() == false)
            {
                return;
            }
            string message = $"Creating scenario files in \"{Path.GetDirectoryName(Parameters.SaveLocation)}\" - will confirm when complete";
            MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Runway.SetRunway();
            ScenarioFXML.GenerateFXMLfile();
            ScenarioHTML.GenerateOverview();
            ScenarioXML.GenerateXMLfile();
            message = $"Scenario files created in \"{Path.GetDirectoryName(Parameters.SaveLocation)}\" - enjoy your flight!";
            MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            List<string> uiVariations = Aircraft.GetUIvariations(); 
            if (uiVariations.Count > 0)
            {
                ListBoxAircraft.DataSource = uiVariations;
                ListBoxAircraft.SelectedIndex = 0;
                SetDefaultCircuitParams();
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
                TextBoxCircuitSpeed.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed);
                TextBoxCircuitHeightDown.Text = "1000";
                TextBoxCircuitHeightUpwind.Text = "800";
                TextBoxCircuitHeightBase.Text = "800";
                // Upwind distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to climb 1000ft at 800ft/min)
                TextBoxCircuitUpwind.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 1.25 / 60);
                // Base distance (miles) approx by speed (knots) * number of minutes / 60 (assume 30 seconds to prepare for next gate after completing turn)
                TextBoxCircuitBase.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 0.5 / 60);
                // Final distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to descend 1000ft at 800ft/min)
                TextBoxCircuitFinal.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 1.25 / 60);
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
            if (!ValidateCircuitParam(TextBoxCircuitHeightDown.Text))
            {
                TextBoxCircuitHeightDown.Text = "0";
            }
            if ((Convert.ToDouble(TextBoxCircuitHeightDown.Text) < Convert.ToDouble(TextBoxCircuitHeightUpwind.Text)) || (Convert.ToDouble(TextBoxCircuitHeightDown.Text) < Convert.ToDouble(TextBoxCircuitHeightBase.Text)))
            {
                MessageBox.Show($"Program expects gates 1/2 and 7/8 to be lower than the downwind leg height, strange results may occur", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "P3D Scenario Generator Help\\P3D Scenario Generator.chm", "introduction.htm");
        }
    }
}
