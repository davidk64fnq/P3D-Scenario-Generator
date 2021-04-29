using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        public Form()
        {
            InitializeComponent();

            // Populate ICAO listbox
            ListBoxRunways.DataSource = Runway.GetICAOids();
            ListBoxScenarioType.DataSource = Constants.scenarioNames;

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

        private void ListBoxScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
            if (TextBoxSelectedScenario.Text == Constants.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                ListBoxRunways.Enabled = false;
                TextBoxSearchRunway.Enabled = false;
                ButtonRandRunway.Enabled = false;
                SetDefaultPhotoTourParams();
            }
            else
            {
                ListBoxRunways.Enabled = true;
                TextBoxSearchRunway.Enabled = true;
                ButtonRandRunway.Enabled = true;
            }
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            if (Parameters.SetParams() == false)
            {
                return;
            }
            string message = $"Creating scenario files in \"{Path.GetDirectoryName(Parameters.SaveLocation)}\" - will confirm when complete";
            MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (TextBoxSelectedScenario.Text == Constants.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                PhotoTour.SetRandomPhotoTour();
            }
            Runway.SetRunway();
            ScenarioFXML.GenerateFXMLfile();
            ScenarioHTML.GenerateHTMLfiles();
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
                TextBoxCircuitHeightUpwind.Text = "500";
                TextBoxCircuitHeightBase.Text = "500";
                // Upwind distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to climb 1000ft at 800ft/min)
                TextBoxCircuitUpwind.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 1.25 / 60);
                // Base distance (miles) approx by speed (knots) * number of minutes / 60 (assume 30 seconds to prepare for next gate after completing turn)
                TextBoxCircuitBase.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 0.5 / 60);
                // Final distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to descend 1000ft at 800ft/min)
                TextBoxCircuitFinal.Text = string.Format("{0:0.0}", Aircraft.CruiseSpeed * 1.25 / 60);
            }
        }

        static private bool ValidateCircuitDoubleParameters()
        {
            if ((Convert.ToDouble(form.TextBoxCircuitHeightDown.Text) < Convert.ToDouble(form.TextBoxCircuitHeightUpwind.Text)) || (Convert.ToDouble(form.TextBoxCircuitHeightDown.Text) < Convert.ToDouble(form.TextBoxCircuitHeightBase.Text)))
            {
                MessageBox.Show($"Gates 1/2 and 7/8 must be lower than the downwind leg height (Gates 3 to 6)", "Circuit Scenario: heights", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        static private bool ValidatePhotoDoubleParameters()
        {
            if (Convert.ToDouble(form.TextBoxPhotoMaxLegDist.Text) < Convert.ToDouble(form.TextBoxPhotoMinLegDist.Text))
            {
                MessageBox.Show($"Maximum leg distance to be greater than or equal to minimum leg distance", "Photo Tour Scenario: leg distances", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Photo Tour Tab

        private void ButtonPhotoTourDefault_Click(object sender, EventArgs e)
        {
            SetDefaultPhotoTourParams();
        }

        private void SetDefaultPhotoTourParams()
        {
            TextBoxPhotoMaxLegDist.Text = "10";
            TextBoxPhotoMinLegDist.Text = "3";
            TextBoxPhotoMinNoLegs.Text = "3";
            TextBoxPhotoMaxNoLegs.Text = "7";
        }

        static private bool ValidatePhotoIntegerParameters()
        {
            if (Convert.ToInt32(form.TextBoxPhotoMinNoLegs.Text) > 15)
            {
                MessageBox.Show($"Minimum number of legs must be less than 16", "Photo Tour Scenario: minimum number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMaxNoLegs.Text) > 15)
            {
                MessageBox.Show($"Maximum number of legs must be less than 16", "Photo Tour Scenario: maximum number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMaxNoLegs.Text) < Convert.ToInt32(form.TextBoxPhotoMinNoLegs.Text))
            {
                MessageBox.Show($"Maximum number of legs to be greater than or equal to minimum number of legs", "Photo Tour Scenario: number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Utilities

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "P3D Scenario Generator.chm", "introduction.htm");
        }

        private void TextBoxDouble_Validating(object sender, CancelEventArgs e)
        {
            double paramAsDouble;
            try
            {
                paramAsDouble = Convert.ToDouble(((TextBox)sender).Text);
                if (paramAsDouble <= 0)
                {
                    MessageBox.Show($"Numeric value greater than zero expected", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Numeric value expected", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            if(((TextBox)sender).Name.Contains("TextBoxCircuitHeight"))
            {
                if (e.Cancel == false && !ValidateCircuitDoubleParameters())
                {
                    e.Cancel = true;
                }
            }
            if (((TextBox)sender).Name.Contains("TextBoxPhoto") && ((TextBox)sender).Name.Contains("LegDist"))
            {
                if (e.Cancel == false && !ValidatePhotoDoubleParameters())
                {
                    e.Cancel = true;
                }
            }
        }
        
        private void TextBoxInteger_Validating(object sender, CancelEventArgs e)
        {
            int paramAsInt;
            try
            {
                paramAsInt = Convert.ToInt32(((TextBox)sender).Text);
                if (paramAsInt <= 0)
                {
                    MessageBox.Show($"Integer value greater than zero expected", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Integer value expected", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            if (((TextBox)sender).Name.Contains("TextBoxPhoto") && ((TextBox)sender).Name.Contains("NoLegs"))
            {
                if (e.Cancel == false && !ValidatePhotoIntegerParameters())
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion

    }
}
