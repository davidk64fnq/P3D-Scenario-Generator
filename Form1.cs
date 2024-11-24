using System.ComponentModel;
using System.Reflection;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        public Form()
        {
            InitializeComponent();
            PrepareFormFields();
            Cache.CheckCacheMonth();
        }

        #region Menu Tab

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
            Random random = new();
            ListBoxRunways.SelectedIndex = random.Next(0, ListBoxRunways.Items.Count);
        }

        #endregion

        #region Scenario selection

        private void ListBoxScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxSelectedScenario.Text = ListBoxScenarioType.SelectedItem.ToString();
            if ((TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.PhotoTour])
                || (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.Celestial])
                || (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.WikiList]))
            {
                ListBoxRunways.Enabled = false;
                TextBoxSearchRunway.Enabled = false;
                ButtonRandRunway.Enabled = false;
                TextBoxSelectedRunway.Text = "";
            }
            else
            {
                ListBoxRunways.Enabled = true;
                TextBoxSearchRunway.Enabled = true;
                ButtonRandRunway.Enabled = true;
                TextBoxSelectedRunway.Text = ListBoxRunways.SelectedItem.ToString();
            }
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            if (Parameters.SetParams())
            {
                DisplayStartMessage();
                Drawing.DrawScenarioImages();
                DoScenarioSpecificTasks();

                // Delete next three lines once re-write complete
            //    Runway.SetRunway(Runway.startRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            //    Runway.SetRunway(Runway.destRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            //    Gates.SetGates();

                ScenarioFXML.GenerateFXMLfile();
                ScenarioHTML.GenerateHTMLfiles();
                ScenarioXML.GenerateXMLfile();
                DisplayFinishMessage();
            }
        }

        private static void DisplayStartMessage()
        {
            Cursor.Current = Cursors.WaitCursor;
            string message = $"Creating scenario files in \"{Parameters.ImageFolder}\" - will confirm when complete";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoScenarioSpecificTasks()
        {
            if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.Circuit])
            {
                Circuit.SetCircuit();
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                PhotoTour.SetPhotoTour();
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.SignWriting])
            {
                SignWriting.InitLetterPaths();
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.Celestial])
            {
                CelestialNav.GetAlmanacData();
                CelestialNav.InitStars();
                CelestialNav.CreateStarsDat();
                string saveLocation = $"{Parameters.ImageFolder}\\htmlCelestialSextant.html";
                CelestialNav.SetCelestialSextantHTML(saveLocation);
                saveLocation = $"{Parameters.ImageFolder}\\images";
                CelestialNav.SetCelestialSextantJS(saveLocation);
                saveLocation = $"{Parameters.ImageFolder}\\styleCelestialSextant.css";
                CelestialNav.SetCelestialSextantCSS(saveLocation);
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.WikiList])
            {
                Wikipedia.SetWikiTour(ListBoxWikiTableNames.SelectedIndex, ListBoxWikiRoute.Items, ComboBoxWikiStartingItem.SelectedItem,
                    ComboBoxWikiFinishingItem.SelectedItem, TextBoxWikiDistance.Text);
            }
            else
            {
                Runway.SetRunway(Runway.startRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
                Runway.SetRunway(Runway.destRwy, Parameters.SelectedAirportICAO, Parameters.SelectedAirportID);
            }
        }

        private static void DisplayFinishMessage()
        {
            Cursor.Current = Cursors.Default;
            string message = $"Scenario files created in \"{Parameters.ImageFolder}\" - enjoy your flight!";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonP3Dv5Files_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new()
            {
                InitialDirectory = "C:\\Users",
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TextBoxP3Dv5Files.Text = dialog.FileName;
                Properties.Settings.Default.Prepar3Dv5Files = TextBoxP3Dv5Files.Text;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            List<string> uiVariations = Aircraft.GetUIvariations();
            if (uiVariations.Count > 0)
            {
                Properties.Settings.Default.CruiseSpeed = Aircraft.CruiseSpeed;
                ListBoxAircraft.DataSource = uiVariations;
                ListBoxAircraft.SelectedIndex = 0;
                SetDefaultCircuitParams();
            }
        }

        private void ListBoxAircraft_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectedAircraft = ListBoxAircraft.Text;
            Properties.Settings.Default.AircraftImage = Aircraft.GetImagename(ListBoxAircraft.Text);
            Properties.Settings.Default.Save();
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
            if (Properties.Settings.Default.CruiseSpeed <= 0)
            {
                MessageBox.Show($"Select an aircraft to calculate default values", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                double cruiseSpeed = Properties.Settings.Default.CruiseSpeed;
                TextBoxCircuitSpeed.Text = string.Format("{0:0.0}", cruiseSpeed);
                TextBoxCircuitHeightDown.Text = "1000";
                TextBoxCircuitHeightUpwind.Text = "500";
                TextBoxCircuitHeightBase.Text = "500";
                // Upwind distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to climb 1000ft at 800ft/min)
                TextBoxCircuitUpwind.Text = string.Format("{0:0.0}", cruiseSpeed * 1.25 / 60);
                // Base distance (miles) approx by speed (knots) * number of minutes / 60 (assume 30 seconds to prepare for next gate after completing turn)
                TextBoxCircuitBase.Text = string.Format("{0:0.0}", cruiseSpeed * 0.5 / 60);
                // Final distance (miles) approx by speed (knots) * number of minutes / 60 (assume 1.25 minutes to descend 1000ft at 800ft/min)
                TextBoxCircuitFinal.Text = string.Format("{0:0.0}", cruiseSpeed * 1.25 / 60);
                TextBoxCircuitTurnRate.Text = "2.0";
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
            if (Convert.ToDouble(form.TextBoxPhotoMaxLegDist.Text) < Convert.ToDouble(form.TextBoxPhotoMinLegDist.Text) + 1)
            {
                MessageBox.Show($"Maximum leg distance to be 1 mile greater than minimum leg distance", "Photo Tour Scenario: leg distances", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Photo Tour Tab

        private void ButtonPhotoTourDefault_Click(object sender, EventArgs e)
        {
            SetDefaultParams(((Button)sender).Parent.Controls);
        //    SetDefaultPhotoTourParams();
        }

        static private bool ValidatePhotoIntegerParameters()
        {
            if (Convert.ToInt32(form.TextBoxPhotoMinNoLegs.Text) > 18)
            {
                MessageBox.Show($"Minimum number of legs must be less than 18", "Photo Tour Scenario: minimum number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMinNoLegs.Text) < 2)
            {
                MessageBox.Show($"Minimum number of legs must be greater than 1", "Photo Tour Scenario: minimum number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMaxNoLegs.Text) > 18)
            {
                MessageBox.Show($"Maximum number of legs must be less than 18", "Photo Tour Scenario: maximum number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMaxNoLegs.Text) < Convert.ToInt32(form.TextBoxPhotoMinNoLegs.Text))
            {
                MessageBox.Show($"Maximum number of legs to be greater than or equal to minimum number of legs", "Photo Tour Scenario: number of legs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if (Convert.ToInt32(form.TextBoxPhotoMaxBearingChange.Text) < 0 || Convert.ToInt32(form.TextBoxPhotoMaxBearingChange.Text) > 180)
            {
                MessageBox.Show($"Maximum bearing change is limited to between 0 and 180 degress", "Photo Tour Scenario: max bearing change", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            if ((Convert.ToInt32(form.TextBoxPhotoWindowSize.Text) < 375) || (Convert.ToInt32(form.TextBoxPhotoWindowSize.Text) > 1500))
            {
                MessageBox.Show($"Window size is limited to between 375 and 1500 pixels inclusive", "Photo Tour Scenario: window size", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Sign Writing Tab

        static private bool ValidateSignWritingParameters()
        {
            if (Convert.ToInt32(form.TextBoxSignTilt.Text) > 90)
            {
                MessageBox.Show($"Tilt angle is limited to between 1 and 90 degrees inclusive", "Sign Writing Scenario: tilt angle", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Celestial Navigation Tab
        static private bool ValidateCelestialIntegerParameters()
        {
            if (Convert.ToInt32(form.TextBoxCelestialMaxDist.Text) < Convert.ToInt32(form.TextBoxCelestialMinDist.Text))
            {
                MessageBox.Show($"Maximum distance from start position to destination must be greater than minimum distance", "Celestial Navigation Scenario: leg distance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        #endregion

        #region Wikipedia Lists Tab
        private void TextBoxWikiURL_TextChanged(object sender, EventArgs e)
        {
            if (TextBoxWikiURL.Text != null)
            {
                Wikipedia.PopulateWikiPage(TextBoxWikiURL.Text, int.Parse(ListBoxWikiColumn.Text));
                ListBoxWikiTableNames.DataSource = Wikipedia.CreateWikiTablesDesc();
            }
        }

        private void ListBoxWikiTableNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxWikiDistance.Text = "";
            if (ListBoxWikiTableNames.Items.Count > 0)
            {
                ListBoxWikiRoute.DataSource = Wikipedia.CreateWikiTableRoute(ListBoxWikiTableNames.SelectedIndex);
                List<string> itemList = [];
                for (int index = 0; index < ListBoxWikiRoute.Items.Count; index++)
                {
                    itemList.Add(GetWikiRouteLegFirstItem(ListBoxWikiRoute.Items[index].ToString()));
                }
                if (!itemList.Contains(GetWikiRouteLegLastItem(ListBoxWikiRoute.Items[^1].ToString())))
                {
                    itemList.Add(GetWikiRouteLegLastItem(ListBoxWikiRoute.Items[^1].ToString()));
                }
                ComboBoxWikiStartingItem.DataSource = itemList;
                ComboBoxWikiStartingItem.SelectedIndex = 0;
                List<string> clonedItemList = new(itemList);
                ComboBoxWikiFinishingItem.DataSource = clonedItemList;
                ComboBoxWikiFinishingItem.SelectedIndex = ComboBoxWikiFinishingItem.Items.Count - 1;
            }
        }

        private void ComboBoxWikiStartingItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxWikiStartingItem.SelectedIndex > ComboBoxWikiFinishingItem.SelectedIndex && ComboBoxWikiFinishingItem.SelectedIndex >= 0)
            {
                (ComboBoxWikiFinishingItem.SelectedIndex, ComboBoxWikiStartingItem.SelectedIndex) = 
                    (ComboBoxWikiStartingItem.SelectedIndex, ComboBoxWikiFinishingItem.SelectedIndex);
            }
            else
            {
                TextBoxWikiDistance.Text = GetWikiDistance();
            }
        }

        private string GetWikiDistance()
        {
            if (ComboBoxWikiFinishingItem.SelectedIndex > ListBoxWikiRoute.Items.Count)
            {
                return "";
            }
            TextBoxWikiDistance.Text = "";
            if (ComboBoxWikiStartingItem.SelectedIndex == ComboBoxWikiFinishingItem.SelectedIndex)
            {
                return "0 miles";
            }
            int distStrStart, distStrFinish, legDistance, routeDistance = 0;
            for (int legNo = ComboBoxWikiStartingItem.SelectedIndex + 1; legNo <= ComboBoxWikiFinishingItem.SelectedIndex; legNo++)
            {
                distStrStart = ListBoxWikiRoute.Items[legNo - 1].ToString().LastIndexOf('(') + 1;
                distStrFinish = ListBoxWikiRoute.Items[legNo - 1].ToString().IndexOf(" miles)");
                legDistance = int.Parse(ListBoxWikiRoute.Items[legNo - 1].ToString()[distStrStart..distStrFinish]);
                routeDistance += legDistance;
            }
            return routeDistance.ToString() + " miles";
        }

        private static string GetWikiRouteLegFirstItem(string route)
        {
            int stringBegin, stringEnd;
            stringBegin = route.IndexOf('[');
            stringEnd = route.IndexOf("...") - 1;
            return route[stringBegin..stringEnd];
        }

        private static string GetWikiRouteLegLastItem(string route)
        {
            int stringBegin, stringEnd;
            stringBegin = route.IndexOf("...") + 4;
            stringEnd = route.LastIndexOf('(') - 1;
            return route[stringBegin..stringEnd];
        }

        #endregion

        #region Utilities

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "Resources/help/index.htm");
        }

        private void PrepareFormFields()
        {
            // General tab
            ListBoxRunways.DataSource = Runway.GetICAOids();

            // Circuit tab
            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Images.circuitTab.jpg");
            PictureBoxCircuit.Image = new Bitmap(stream);
            SetDefaultCircuitParams();

            // PhotoTour tab
            SetDefaultParams(TabPagePhotoTour.Controls);

            // Signwriting tab
            stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Images.signTabSegment22Font.jpg");
            PictureBoxSignWriting.Image = new Bitmap(stream);

            // Wikipedia Lists tab
            ListBoxWikiColumn.SetSelected(0, true);
        }

        /// <summary>
        /// Recursively processes all controls to copy the default value stored in the tag field if it exists into the text field.
        /// </summary>
        /// <param name="controlCollection">The collection of controls to be processed, including all child control collections</param>
        private static void SetDefaultParams(Control.ControlCollection controlCollection)
        {
            foreach (Control control in controlCollection)
            {
                if (control.Controls.Count == 0)
                {
                    if (control.Tag != null)
                        control.Text = control.Tag.ToString().Split(',')[0];
                }
                else
                {
                    foreach (Control childControl in control.Controls)
                    {
                        SetDefaultParams(childControl.Controls);
                    }
                }
            }
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
            if (((TextBox)sender).Name.Contains("TextBoxCircuitHeight"))
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
            string[] validatePhotoTourIntegerFields =
            [
                "TextBoxPhotoMinNoLegs", "TextBoxPhotoMaxNoLegs",
                "TextBoxPhotoMaxBearingChange",
                "TextBoxPhotoWindowSize",
                "TextBoxPhotoTourMapMonitorNumber", "TextBoxPhotoTourMapHorizontalOffset", "TextBoxPhotoTourMapVerticalOffset",
                "TextBoxPhotoTourMapMonitorWidth", "TextBoxPhotoTourMapMonitorHeight",
                "TextBoxPhotoTourPhotoMonitorNumber", "TextBoxPhotoTourPhotoHorizontalOffset", "TextBoxPhotoTourPhotoVerticalOffset",
                "TextBoxPhotoTourPhotoMonitorWidth", "TextBoxPhotoTourPhotoMonitorHeight"
            ];
            int paramAsInt;
            try
            {
                paramAsInt = Convert.ToInt32(((TextBox)sender).Text);
            }
            catch (Exception)
            {
                MessageBox.Show($"Integer value expected", ((TextBox)sender).AccessibleName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            if (validatePhotoTourIntegerFields.Contains(((TextBox)sender).Name))
            {
                if (e.Cancel == false && !ValidatePhotoIntegerParameters())
                {
                    e.Cancel = true;
                }
            }
            else if (((TextBox)sender).Name.Contains("TextBoxSignTilt"))
            {
                if (e.Cancel == false && !ValidateSignWritingParameters())
                {
                    e.Cancel = true;
                }
            }
            else if (((TextBox)sender).Name.Contains("TextBoxCelestial") && ((TextBox)sender).Name.Contains("Dist"))
            {
                if (e.Cancel == false && !ValidateCelestialIntegerParameters())
                {
                    e.Cancel = true;
                }
            }
        }

        private void TextBoxString_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                MessageBox.Show($"Alphabetic string expected", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }

            for (int i = 0; i < ((TextBox)sender).Text.Length; i++)
                if (!char.IsLetter(((TextBox)sender).Text[i]) && ((TextBox)sender).Text[i] != ' ' && ((TextBox)sender).Text[i] != '@')
                {
                    MessageBox.Show($"Alphabetic string expected, 'A' to 'Z' and 'a' to 'z' only", ((TextBox)sender).Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                }
        }

        private void Init(object sender, EventArgs e)
        {
            string[] newUIvariation = [Properties.Settings.Default.SelectedAircraft, Properties.Settings.Default.AircraftImage];
            Aircraft.uiVariations.Add(newUIvariation);
            List<string> aircraftList = [Properties.Settings.Default.SelectedAircraft];
            ListBoxAircraft.DataSource = aircraftList;
            Aircraft.CruiseSpeed = Properties.Settings.Default.CruiseSpeed;
            TextBoxP3Dv5Files.Text = Properties.Settings.Default.Prepar3Dv5Files;
        }
        #endregion
    }
}
