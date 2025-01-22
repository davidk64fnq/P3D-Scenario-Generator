using System.ComponentModel;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        internal static readonly Form form = (Form)Application.OpenForms[0];

        public Form()
        {
            InitializeComponent();
            PrepareFormFields();
        }

        #region General Tab

        #region Runway selection

        private void TextBoxSearchRunway_TextChanged(object sender, EventArgs e)
        {
            int searchIndex = ComboBoxGeneralRunwaySelected.FindString(TextBoxGeneralSearchRunway.Text);
            if (searchIndex != ListBox.NoMatches)
            {
                ComboBoxGeneralRunwaySelected.SelectedIndex = searchIndex;
            }
        }

        private void ButtonRandRunway_Click(object sender, EventArgs e)
        {
            Runway.SetRunwaysSubset();
            Random random = new();
            int randomSubsetIndex = random.Next(0, Runway.RunwaysSubset.Count);
            int randomRunwayIndex = Runway.RunwaysSubset[randomSubsetIndex].RunwaysIndex;
            ComboBoxGeneralRunwaySelected.SelectedIndex = randomRunwayIndex;
            TextBoxGeneralSearchRunway.Text = "";
        }

        #endregion

        #region Scenario selection

        private void ButtonRandomScenario_Click(object sender, EventArgs e)
        {
            Random random = new();
            ComboBoxGeneralScenarioType.SelectedIndex = random.Next(0, ComboBoxGeneralScenarioType.Items.Count);
        }

        private void ComboBoxGeneralScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((ComboBoxGeneralScenarioType.SelectedItem.ToString() == Con.scenarioNames[(int)ScenarioTypes.PhotoTour])
                || (ComboBoxGeneralScenarioType.SelectedItem.ToString() == Con.scenarioNames[(int)ScenarioTypes.Celestial])
                || (ComboBoxGeneralScenarioType.SelectedItem.ToString() == Con.scenarioNames[(int)ScenarioTypes.WikiList]))
            {
                ComboBoxGeneralRunwaySelected.Enabled = false;
                TextBoxGeneralSearchRunway.Enabled = false;
                ButtonRandRunway.Enabled = false;
            }
            else
            {
                ComboBoxGeneralRunwaySelected.Enabled = true;
                TextBoxGeneralSearchRunway.Enabled = true;
                ButtonRandRunway.Enabled = true;
            }
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            if (Parameters.SetParams())
            {
                DisplayStartMessage();
                SaveUserSettings(TabPageSettings.Controls);
                Drawing.DrawScenarioImages();
                DoScenarioSpecificTasks();
                ScenarioFXML.GenerateFXMLfile();
                ScenarioHTML.GenerateHTMLfiles();
                ScenarioXML.GenerateXMLfile();
                DisplayFinishMessage();
            }
        }

        private static void DisplayStartMessage()
        {
            Cursor.Current = Cursors.WaitCursor;
            string message = $"Creating scenario files in \"{Parameters.SettingsScenarioFolder}\" - will confirm when complete";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoScenarioSpecificTasks()
        {
            if (ComboBoxGeneralScenarioType.Text == Con.scenarioNames[(int)ScenarioTypes.Circuit])
            {
                Circuit.SetCircuit();
                SaveUserSettings(TabPageCircuit.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Con.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                PhotoTour.SetPhotoTour();
                SaveUserSettings(TabPagePhotoTour.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Con.scenarioNames[(int)ScenarioTypes.SignWriting])
            {
                SignWriting.SetSignWriting();
                SaveUserSettings(TabPageSign.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Con.scenarioNames[(int)ScenarioTypes.Celestial])
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
                SaveUserSettings(TabPageWikiList.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Con.scenarioNames[(int)ScenarioTypes.WikiList])
            {
                Wikipedia.SetWikiTour(ComboBoxWikiTableNames.SelectedIndex, ComboBoxWikiRoute.Items, ComboBoxWikiStartingItem.SelectedItem,
                    ComboBoxWikiFinishingItem.SelectedItem, TextBoxWikiDistance.Text);
                SaveUserSettings(TabPageWikiList.Controls);
                ClearWikiListSettingsFields();
            }
            else
            {
                Runway.startRwy = Runway.Runways[Parameters.SelectedAirportIndex];
                Runway.destRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            }
        }

        private static void DisplayFinishMessage()
        {
            Cursor.Current = Cursors.Default;
            string message = $"Scenario files created in \"{Parameters.SettingsScenarioFolder}\" - enjoy your flight!";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Date and Time selection

        private void ButtonRandomDate_Click(object sender, EventArgs e)
        {
            DateTime start = new(2000, 1, 1);
            int range = (DateTime.Today - start).Days;
            Random random = new();
            GeneralDatePicker.Text = start.AddDays(random.Next(range)).ToString();
        }

        private void ButtonRandomTime_Click(object sender, EventArgs e)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var year = rnd.Next(2000, DateTime.Today.Year);
            var month = rnd.Next(1, 13);
            var days = rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);
            GeneralTimePicker.Text = new DateTime(year, month, days,
                    rnd.Next(0, 24), rnd.Next(0, 60), rnd.Next(0, 60), rnd.Next(0, 1000)).ToString();
        }

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            if (Aircraft.ChooseAircraftVariant(ComboBoxSettingsSimulatorVersion.Text))
            {
                ComboBoxGeneralAircraftSelection.DataSource = Aircraft.GetAircraftVariantDisplayNames();
                ComboBoxGeneralAircraftSelection.SelectedIndex = ComboBoxGeneralAircraftSelection.Items.Count - 1;
            }
        }

        private void ComboBoxGeneralAircraftSelection_KeyDown(object sender, KeyEventArgs e)
        {
            string deletedItem = ((ComboBox)sender).Text;

            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                int deletedIndex = Properties.Settings.Default.AircraftTitles.IndexOf(deletedItem);
                Properties.Settings.Default.AircraftTitles.RemoveAt(deletedIndex);
                if (Properties.Settings.Default.AircraftTitles.Count > 0)
                    Properties.Settings.Default.AircraftTitlesSelectedIndex = 0;
                Properties.Settings.Default.AircraftCruiseSpeeds.RemoveAt(deletedIndex);
                Properties.Settings.Default.AircraftImages.RemoveAt(deletedIndex);
                Properties.Settings.Default.Save();
                ((ComboBox)sender).DataSource = null;
                ((ComboBox)sender).DataSource = Properties.Settings.Default.AircraftTitles;
                if (deletedIndex > 0)
                    ((ComboBox)sender).SelectedIndex = 0;
            }
        }

        private void ComboBoxGeneralAircraftSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDefaultCircuitParams();
        }

        private void ButtonRandomAircraft_Click(object sender, EventArgs e)
        {
            if (Aircraft.AircraftVariants == null)
                return;
            Random random = new();
            int randomAircraftIndex = random.Next(0, Aircraft.AircraftVariants.Count);
            ComboBoxGeneralAircraftSelection.SelectedIndex = randomAircraftIndex;

            // Update CurrentAircraftVariantIndex in Aircraft.cs
            string newFavouriteAircraft = ComboBoxGeneralAircraftSelection.Text;
            Aircraft.ChangeCurrentAircraftVariantIndex(newFavouriteAircraft);

            // Refresh TextBoxGeneralAircraftValues field on form
        //    TextBoxGeneralAircraftValues.Text = Aircraft.SetTextBoxGeneralAircraftValues();
        }

        #endregion

        #region Location selection

        /// <summary>
        /// Add or delete an entry in <see cref="ComboBoxGeneralLocationFavourites"/> list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxGeneralLocationFavourites_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Alter locationFavourite instance in Runway.cs to reflect new name
                string newFavouriteName = ((ComboBox)sender).Text;
                string oldFavouriteName = Runway.UpdateLocationFavouriteName(newFavouriteName);

                // Create a new locationFavourite instance in Runway.cs using old name and all filters set to "None"
                Runway.AddLocationFavourite(oldFavouriteName, ["None"], ["None"], ["None"]);

                // Refresh the ComboBoxGeneralLocationFavourites field list on form
                ComboBoxGeneralLocationFavourites.DataSource = Runway.GetLocationFavouriteNames();

                // Set selected index for ComboBoxGeneralLocationFavourites
                int newFavouriteIndex = ComboBoxGeneralLocationFavourites.Items.IndexOf(newFavouriteName);
                ComboBoxGeneralLocationFavourites.SelectedIndex = newFavouriteIndex;

                // Refresh Country/State/City fields on form
                LocationFavourite currentLocationFavourite = Runway.GetCurrentLocationFavourite();
                ComboBoxGeneralLocationCountry.Text = currentLocationFavourite.Countries[0];
                ComboBoxGeneralLocationState.Text = currentLocationFavourite.States[0];
                ComboBoxGeneralLocationCity.Text = currentLocationFavourite.Cities[0];

                // Refresh TextBoxGeneralLocationFilters field on form
                TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Don't delete a character
                e.SuppressKeyPress = true;

                // Delete locationFavourite instance in Runway.cs
                string deleteFavouriteName = ((ComboBox)sender).Text;
                string currentFavouriteName = Runway.DeleteLocationFavourite(deleteFavouriteName);

                // Refresh the ComboBoxGeneralLocationFavourites field list on form
                ComboBoxGeneralLocationFavourites.DataSource = Runway.GetLocationFavouriteNames();

                // Set selected index for ComboBoxGeneralLocationFavourites 
                int currentFavouriteIndex = ComboBoxGeneralLocationFavourites.Items.IndexOf(currentFavouriteName);
                ComboBoxGeneralLocationFavourites.SelectedIndex = currentFavouriteIndex;

                // Refresh Country/State/City fields on form
                LocationFavourite currentLocationFavourite = Runway.GetCurrentLocationFavourite();
                ComboBoxGeneralLocationCountry.Text = currentLocationFavourite.Countries[0];
                ComboBoxGeneralLocationState.Text = currentLocationFavourite.States[0];
                ComboBoxGeneralLocationCity.Text = currentLocationFavourite.Cities[0];

                // Refresh TextBoxGeneralLocationFilters field on form
                TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
            }
        }

        /// <summary>
        /// Display tooltip on mouseover of TextBoxGeneralLocationFilters field to show wide content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxGeneralLocationFilters_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new();
            t.Show(TextBoxGeneralLocationFilters.Text, TextBoxGeneralLocationFilters, 0, 0, 5000);
        }

        /// <summary>
        /// When user selects a different location favourite set the Country/State/City location fields
        /// to correct values and refresh the TextBoxGeneralLocationFilters field value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxGeneralLocationFavourites_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update CurrentLocationFavouriteIndex in Runway.cs
            string newFavouriteName = ((ComboBox)sender).Text;
            Runway.ChangeCurrentLocationFavouriteIndex(newFavouriteName);

            // Refresh Country/State/City fields on form
            LocationFavourite currentLocationFavourite = Runway.GetCurrentLocationFavourite();
            ComboBoxGeneralLocationCountry.Text = currentLocationFavourite.Countries[0];
            ComboBoxGeneralLocationState.Text = currentLocationFavourite.States[0];
            ComboBoxGeneralLocationCity.Text = currentLocationFavourite.Cities[0];

            // Refresh TextBoxGeneralLocationFilters field on form
            TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
        }

        /// <summary>
        /// The Country/State/City field lists are derived from the values in "runways.xml". When the user
        /// presses Enter or Delete key with the focus in one of these fields that currently selected filter
        /// string is added to or deleted from the currently selected location favourite and the TextBoxGeneralLocationFilters
        /// field is refreshed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxGeneralLocation_KeyDown(object sender, KeyEventArgs e)
        {
            string selectedItem = ((ComboBox)sender).SelectedItem.ToString();
            string locationType;

            if (((ComboBox)sender).Name.Contains("Country"))
                locationType = "Country";
            else if (((ComboBox)sender).Name.Contains("State"))
                locationType = "State";
            else
                locationType = "City";

            if (e.KeyCode == Keys.Enter)
            {
                Runway.AddFilterValueToLocationFavourite(locationType, selectedItem);
                TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
                ((ComboBox)sender).Text = Runway.GetLocationFavouriteDisplayFilterValue(locationType);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Runway.DeleteFilterValueFromLocationFavourite(locationType, selectedItem);
                TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
                ((ComboBox)sender).Text = Runway.GetLocationFavouriteDisplayFilterValue(locationType);
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
            if (ComboBoxGeneralAircraftSelection.Items.Count == 0)
            {
                MessageBox.Show($"Select an aircraft to calculate default values", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (Aircraft.AircraftVariants.Count == 0)
                    return;
                double cruiseSpeed = Convert.ToDouble(Aircraft.AircraftVariants[Aircraft.CurrentAircraftVariantIndex].CruiseSpeed);
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

        #endregion

        #region Photo Tour Tab

        #endregion

        #region Sign Writing Tab

        #endregion

        #region Celestial Navigation Tab

        #endregion

        #region Wikipedia Lists Tab

        /// <summary>
        /// After saving all of the Wikipedia scenario related settings this method is called to clear the settings
        /// relating to setting up the wiki tour. Otherwise the tour would get recreated on application load everytime.
        /// </summary>
        private static void ClearWikiListSettingsFields()
        {
            Properties.Settings.Default.ComboBoxWikiURL.Clear();
            Properties.Settings.Default.ComboBoxWikiURLSelectedIndex = -1;
            Properties.Settings.Default.TextBoxWikiItemLinkColumn = "1";
            Properties.Settings.Default.ComboBoxWikiTableNames.Clear();
            Properties.Settings.Default.ComboBoxWikiTableNamesSelectedIndex = -1;
            Properties.Settings.Default.ComboBoxWikiRoute.Clear();
            Properties.Settings.Default.ComboBoxWikiRouteSelectedIndex = -1;
            Properties.Settings.Default.ComboBoxWikiStartingItem.Clear();
            Properties.Settings.Default.ComboBoxWikiStartingItemSelectedIndex = -1;
            Properties.Settings.Default.ComboBoxWikiFinishingItem.Clear();
            Properties.Settings.Default.ComboBoxWikiFinishingItemSelectedIndex = -1;
            Properties.Settings.Default.TextBoxWikiDistance = "";
            Properties.Settings.Default.Save();
        }

        private void ComboBoxWikiURL_TextChanged(object sender, EventArgs e)
        {
            ComboBoxWikiURL_TextChanged();
            ComboBox_SelectedIndexChanged(sender, e);
        }

        private void ComboBoxWikiURL_TextChanged()
        {
            if (ComboBoxWikiURL.SelectedItem.ToString() != "")
            {
                Parameters.SelectedScenario = Con.scenarioNames[(int)ScenarioTypes.WikiList];
                Wikipedia.PopulateWikiPage(ComboBoxWikiURL.SelectedItem.ToString(), int.Parse(TextBoxWikiItemLinkColumn.Text));
                ComboBoxWikiTableNames.DataSource = Wikipedia.CreateWikiTablesDesc();
            }
        }

        private void ComboBoxWikiTableNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxWikiURL.Items.Count == 0)
                return;
            TextBoxWikiDistance.Text = "";
            if (ComboBoxWikiTableNames.Items.Count > 0)
            {
                ComboBoxWikiRoute.DataSource = Wikipedia.CreateWikiTableRoute(ComboBoxWikiTableNames.SelectedIndex);
                List<string> itemList = [];
                for (int index = 0; index < ComboBoxWikiRoute.Items.Count; index++)
                {
                    itemList.Add(GetWikiRouteLegFirstItem(ComboBoxWikiRoute.Items[index].ToString()));
                }
                if (!itemList.Contains(GetWikiRouteLegLastItem(ComboBoxWikiRoute.Items[^1].ToString())))
                {
                    itemList.Add(GetWikiRouteLegLastItem(ComboBoxWikiRoute.Items[^1].ToString()));
                }
                ComboBoxWikiStartingItem.DataSource = itemList;
                ComboBoxWikiStartingItem.SelectedIndex = 0;
                List<string> clonedItemList = new(itemList);
                ComboBoxWikiFinishingItem.DataSource = clonedItemList;
                ComboBoxWikiFinishingItem.SelectedIndex = ComboBoxWikiFinishingItem.Items.Count - 1;
                ComboBox_SelectedIndexChanged(sender, e);
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
            ComboBox_SelectedIndexChanged(sender, e);
        }

        private string GetWikiDistance()
        {
            if (ComboBoxWikiFinishingItem.SelectedIndex > ComboBoxWikiRoute.Items.Count)
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
                distStrStart = ComboBoxWikiRoute.Items[legNo - 1].ToString().LastIndexOf('(') + 1;
                distStrFinish = ComboBoxWikiRoute.Items[legNo - 1].ToString().IndexOf(" miles)");
                legDistance = int.Parse(ComboBoxWikiRoute.Items[legNo - 1].ToString()[distStrStart..distStrFinish]);
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

        #region Settings Tab

        #endregion

        #region Utilities

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "Resources/help/index.htm");
        }

        private void ButtonDefault_Click(object sender, EventArgs e)
        {
            SetDefaultParams(((Button)sender).Parent.Controls);
        }

        private void ButtonSaved_Click(object sender, EventArgs e)
        {
            RestoreUserSettings(((Button)sender).Parent.Controls);
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            string s = ((ComboBox)sender).Text;

            if (e.KeyCode == Keys.Enter)
            {
                if (!((ComboBox)sender).Items.Contains(s))
                {
                    ((ComboBox)sender).Items.Add(s);
                    ((ComboBox)sender).SelectedIndex = ((ComboBox)sender).Items.Count - 1;
                    UpdateComboBoxSelectedIndex(((ComboBox)sender).Name, ((ComboBox)sender).SelectedIndex);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                if (((ComboBox)sender).Items.Contains(s))
                {
                    ((ComboBox)sender).Items.Remove(s);
                    if (((ComboBox)sender).Items.Count > 0)
                        ((ComboBox)sender).SelectedIndex = 0;
                    UpdateComboBoxSelectedIndex(((ComboBox)sender).Name, ((ComboBox)sender).SelectedIndex);
                }
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateComboBoxSelectedIndex(((ComboBox)sender).Name, ((ComboBox)sender).SelectedIndex);
        }

        private static void UpdateComboBoxSelectedIndex(string comboBoxName, int selectedIndex)
        {
            Properties.Settings.Default[comboBoxName + "SelectedIndex"] = selectedIndex;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load information such as runway list, circuit image, sign writing alphabet image, set default field values.
        /// </summary>
        private void PrepareFormFields()
        {
            // General tab

            //  Runways
            Runway.GetRunways();
            ComboBoxGeneralRunwaySelected.DataSource = Runway.GetICAOids();

            //  Locations
            ComboBoxGeneralLocationCountry.DataSource = Runway.GetRunwayCountries();
            ComboBoxGeneralLocationState.DataSource = Runway.GetRunwayStates();
            ComboBoxGeneralLocationCity.DataSource = Runway.GetRunwayCities();
            Runway.LoadLocationFavourites();
            Runway.CurrentLocationFavouriteIndex = 0;
            TextBoxGeneralLocationFilters.Text = Runway.SetTextBoxGeneralLocationFilters();
            ComboBoxGeneralLocationFavourites.DataSource = Runway.GetLocationFavouriteNames();

            //  Scenario type
            ComboBoxGeneralScenarioType.SelectedIndex = 0;

            //  Aircraft variants
            if (Aircraft.AircraftVariants != null)
            {
                ComboBoxGeneralAircraftSelection.DataSource = Aircraft.GetAircraftVariantDisplayNames();
                ComboBoxGeneralAircraftSelection.SelectedIndex = 0;
            }

            // Circuit tab
            SetDefaultParams(TabPageCircuit.Controls);
            RestoreUserSettings(TabPageCircuit.Controls);

            // PhotoTour tab
            SetDefaultParams(TabPagePhotoTour.Controls);
            RestoreUserSettings(TabPagePhotoTour.Controls);

            // Signwriting tab
            SetDefaultParams(TabPageSign.Controls);
            RestoreUserSettings(TabPageSign.Controls);

            // Wikipedia Lists tab
            SetDefaultParams(TabPageWikiList.Controls);
            RestoreUserSettings(TabPageWikiList.Controls);

            // Settings tab
            Cache.CheckCache();
            RestoreUserSettings(TabPageSettings.Controls);
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
                        control.Text = control.Tag.ToString().Split(',')[0].Trim();
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

        /// <summary>
        /// Recursively processes all controls to copy the associated user setting values if they exist into the text field.
        /// </summary>
        /// <param name="controlCollection">The collection of controls to be processed, including all child control collections</param>
        private static void RestoreUserSettings(Control.ControlCollection controlCollection)
        {
            foreach (Control control in controlCollection)
            {
                if (control.Controls.Count == 0)
                {
                    try
                    {
                        var settingsValue = Properties.Settings.Default[control.Name];
                        if (settingsValue != null)
                            if (control is TextBox)
                                control.Text = settingsValue.ToString();
                            else if (control is ComboBox box)
                            {
                                int itemCount = ((System.Collections.Specialized.StringCollection)Properties.Settings.Default[control.Name]).Count;
                                if (itemCount > 0)
                                {
                                    box.Items.Clear();
                                    foreach (string item in (System.Collections.Specialized.StringCollection)Properties.Settings.Default[control.Name])
                                    {
                                        box.Items.Add(item);
                                    }
                                    box.SelectedIndex = (int)Properties.Settings.Default[control.Name + "SelectedIndex"];
                                }
                            }
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    foreach (Control childControl in control.Controls)
                    {
                        RestoreUserSettings(childControl.Controls);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively processes all controls to copy the text field into the associated user setting value if it exists.
        /// </summary>
        /// <param name="controlCollection">The collection of controls to be processed, including all child control collections</param>
        private static void SaveUserSettings(Control.ControlCollection controlCollection)
        {
            foreach (Control control in controlCollection)
            {
                if (control.Controls.Count == 0)
                {
                    try
                    {
                        if (control is TextBox)
                        {
                            Properties.Settings.Default[control.Name] = control.Text;
                        }
                        else if (control is ComboBox box)
                        {
                            var newList = new System.Collections.Specialized.StringCollection();
                            foreach (object item in box.Items)
                            {
                                newList.Add(item.ToString());
                            }
                            Properties.Settings.Default[control.Name] = newList;
                            Properties.Settings.Default[control.Name + "SelectedIndex"] = box.SelectedIndex;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    foreach (Control childControl in control.Controls)
                    {
                        SaveUserSettings(childControl.Controls);
                    }
                }
            }
            Properties.Settings.Default.Save();
        }

        private void TextBox_Validating(object sender, CancelEventArgs e)
        {
            int parameterTypeIndex = 1;
            string[] parameterTokens = ((TextBox)sender).Tag.ToString().Split(',');
            string parameterType = parameterTokens[parameterTypeIndex].Trim();

            // First two tokens of textbox.Tag are the default value and value type, they should always be coded by developer!
            if (parameterTokens.Length < 2 || parameterTokens.Length % 2 == 1)
            {
                DisplayParameterValidationMsg($"Developer has incorrectly set tag field of textbox control!", ((TextBox)sender).AccessibleName, e);
                return;
            }


            // If it's one of the integer/whole/natural types
            string integerTypes = "integer whole natural";
            if (integerTypes.Contains(parameterType))
            {
                // Check it is an integer before looking at whole and natural types
                if (!TextboxIsInteger(((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                    return;

                // Check whole type i.e. 0, 1, 2, ...
                if (parameterType.Equals("whole") && !TextboxIsWhole(((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                    return;

                // Check natural type i.e. 1, 2, 3, ...
                if (parameterType.Equals("natural") && !TextboxIsNatural(((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                    return;
            }

            // If it's a double
            if (parameterType.Equals("double") && !TextboxIsDouble(((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                return;

            // If it's a string
            if (parameterType.Equals("string") && !TextboxIsString(((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                return;

            // Do any custom checks
            if (parameterTokens.Length > 2)
            {
                for (int index = 2; index < parameterTokens.Length; index += 2)
                {
                    if (!TextboxComparison(parameterTokens[index].Trim(), parameterTokens[index + 1].Trim(), ((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
                        return;
                }
            }
        }

        private static bool TextboxComparison(string operatorToken, string comparedAgainstToken, string comparedToToken, string title, CancelEventArgs e)
        {
            // Get the value to be compared against, either a numeric value or the name of a textbox containing a numeric value
            double comparedAgainstDouble;
            string comparedAgainstString = "";
            string accessibleName = "";
            try
            {
                comparedAgainstDouble = Convert.ToDouble(comparedAgainstToken);
            }
            catch
            {
                GetFormTextField(form.Controls, comparedAgainstToken, ref comparedAgainstString, ref accessibleName);
                comparedAgainstDouble = Convert.ToDouble(comparedAgainstString);
            }

            // Get the value to be compared
            double comparedToDouble = Convert.ToDouble(comparedToToken);


            // Do comparison
            accessibleName = string.IsNullOrEmpty(accessibleName) ? comparedAgainstDouble.ToString() : accessibleName;
            switch (operatorToken)
            {
                case "<":
                    if (comparedToDouble >= comparedAgainstDouble)
                    {
                        DisplayParameterValidationMsg($"Value must be less than {accessibleName}", title, e);
                        return false;
                    }
                    break;
                case "<=":
                    if (comparedToDouble > comparedAgainstDouble)
                    {
                        DisplayParameterValidationMsg($"Value must be less than or equal to {accessibleName}", title, e);
                        return false;
                    }
                    break;
                case ">":
                    if (comparedToDouble <= comparedAgainstDouble)
                    {
                        DisplayParameterValidationMsg($"Value must be greater than {accessibleName}", title, e);
                        return false;
                    }
                    break;
                case ">=":
                    if (comparedToDouble < comparedAgainstDouble)
                    {
                        DisplayParameterValidationMsg($"Value must be greater than or equal to {accessibleName}", title, e);
                        return false;
                    }
                    break;
                default:
                    DisplayParameterValidationMsg($"Developer has incorrectly set tag field of textbox control!", title, e);
                    return false;
            }
            return true;
        }

        private static bool TextboxIsDouble(string text, string title, CancelEventArgs e)
        {
            try
            {
                double paramAsDouble = Convert.ToDouble(text);
                if (paramAsDouble <= 0)
                {
                    DisplayParameterValidationMsg($"Numeric value greater than zero expected", title, e);
                    return false;
                }
            }
            catch (Exception)
            {
                DisplayParameterValidationMsg($"Numeric value expected", title, e);
                return false;
            }
            return true;
        }

        private static bool TextboxIsInteger(string text, string title, CancelEventArgs e)
        {
            try
            {
                int paramAsInteger = Convert.ToInt32(text);
            }
            catch (Exception)
            {
                DisplayParameterValidationMsg($"Integer value expected", title, e);
                return false;
            }
            return true;
        }

        private static bool TextboxIsNatural(string text, string title, CancelEventArgs e)
        {
            int paramAsInteger = Convert.ToInt32(text);
            if (paramAsInteger <= 0)
            {
                DisplayParameterValidationMsg($"Integer value greater than zero expected", title, e);
                return false;
            }
            return true;
        }

        private static bool TextboxIsString(string text, string title, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(text))
            {
                DisplayParameterValidationMsg($"Alphabetic string expected", title, e);
                return false;
            }

            for (int i = 0; i < text.Length; i++)
                if (!char.IsLetter(text[i]) && text[i] != ' ') // Include  && text[i] != '@' if using test character, see InitLetterPaths()
                {
                    DisplayParameterValidationMsg($"Alphabetic string expected, 'A' to 'Z' and 'a' to 'z' only", title, e);
                    return false;
                }
            return true;
        }

        private static bool TextboxIsWhole(string text, string title, CancelEventArgs e)
        {
            int paramAsInteger = Convert.ToInt32(text);
            if (paramAsInteger < 0)
            {
                DisplayParameterValidationMsg($"Integer value greater than or equal to zero expected", title, e);
                return false;
            }
            return true;
        }

        private static void DisplayParameterValidationMsg(string message, string title, CancelEventArgs e)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            e.Cancel = true;
        }

        private static void GetFormTextField(Control.ControlCollection controlCollection, string fieldName, ref string fieldValue, ref string accessibleName)
        {
            foreach (Control control in controlCollection)
            {
                if (control.Controls.Count == 0)
                {
                    GetFormTextField(control, fieldName, ref fieldValue, ref accessibleName);
                }
                else
                {
                    foreach (Control childControl in control.Controls)
                    {
                        if (childControl.Controls.Count == 0)
                        {
                            GetFormTextField(childControl, fieldName, ref fieldValue, ref accessibleName);
                        }
                        GetFormTextField(childControl.Controls, fieldName, ref fieldValue, ref accessibleName);
                    }
                }
            }
        }

        private static void GetFormTextField(Control control, string fieldName, ref string fieldValue, ref string accessibleName)
        {
            if (control is TextBox && control.Name == fieldName)
            {
                fieldValue = control.Text;
                accessibleName = control.AccessibleName;
            }
        }

        internal static void DeleteFile(string filename)
        {
            if (!File.Exists(filename))
                return;
            var started = DateTime.UtcNow;
            while ((DateTime.UtcNow - started).TotalMilliseconds < 2000)
            {
                try
                {
                    File.Delete(filename);
                    return;
                }
                catch (IOException)
                {
                    // Ignore
                }
            }
        }

        internal static Stream GetResourceStream(string resource)
        {
            string resourceName = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.{resource}";
            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream(resourceName);
            return stream;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Runway.SaveLocationFavourites();
        }

        #endregion
    }
}
