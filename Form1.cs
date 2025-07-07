using P3D_Scenario_Generator.CelestialScenario;
using P3D_Scenario_Generator.CircuitScenario;
using P3D_Scenario_Generator.PhotoTourScenario;
using P3D_Scenario_Generator.SignWritingScenario;
using P3D_Scenario_Generator.WikipediaScenario;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        internal static readonly Form form = (Form)Application.OpenForms[0];
        private readonly IProgress<string> _statusProgress;

        public Form()
        {
            InitializeComponent();

            _statusProgress = new Progress<string>(message =>
            {
                if (toolStripStatusLabel1 != null)
                {
                    toolStripStatusLabel1.Text = message;
                }
            });

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
            if ((ComboBoxGeneralScenarioType.SelectedItem.ToString() == Constants.scenarioNames[(int)ScenarioTypes.PhotoTour])
                || (ComboBoxGeneralScenarioType.SelectedItem.ToString() == Constants.scenarioNames[(int)ScenarioTypes.Celestial])
                || (ComboBoxGeneralScenarioType.SelectedItem.ToString() == Constants.scenarioNames[(int)ScenarioTypes.WikiList]))
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
                CheckRunwaysXMLupToDate();
                Runway.SetRunwaysSubset();
                DisplayStartMessage();
                SaveUserSettings(TabPageSettings.Controls);
                ImageUtils.DrawScenarioImages();
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
            MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoScenarioSpecificTasks()
        {
            if (ComboBoxGeneralScenarioType.Text == Constants.scenarioNames[(int)ScenarioTypes.Circuit])
            {
                MakeCircuit.SetCircuit();
                SaveUserSettings(TabPageCircuit.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Constants.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                PhotoTour.SetPhotoTour();
                SaveUserSettings(TabPagePhotoTour.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Constants.scenarioNames[(int)ScenarioTypes.SignWriting])
            {
                SignWriting.SetSignWriting();
                SaveUserSettings(TabPageSign.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Constants.scenarioNames[(int)ScenarioTypes.Celestial])
            {
                if (CelestialNav.SetCelestial())
                    SaveUserSettings(TabPageWikiList.Controls);
            }
            else if (ComboBoxGeneralScenarioType.Text == Constants.scenarioNames[(int)ScenarioTypes.WikiList])
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
            MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void ButtonAddAircraft_Click(object sender, EventArgs e)
        {
            string displayName = Aircraft.ChooseAircraftVariant(ComboBoxSettingsSimulatorVersion.Text);
            if (displayName != "")
            {
                ComboBoxGeneralAircraftSelection.DataSource = Aircraft.GetAircraftVariantDisplayNames();

                // Refreshing ComboBoxGeneralAircraftSelection.DataSource triggers ComboBoxGeneralAircraftSelection_SelectedIndexChanged
                // so restore correct Aircraft.CurrentAircraftVariantIndex
                Aircraft.ChangeCurrentAircraftVariantIndex(displayName);

                // Set selected index for ComboBoxGeneralAircraftSelection 
                ComboBoxGeneralAircraftSelection.SelectedIndex = Aircraft.CurrentAircraftVariantIndex;
            }
        }

        private void ComboBoxGeneralAircraftSelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Alter aircraftVariant instance in Aircraft.cs to reflect new display name
                string newDisplayName = ((ComboBox)sender).Text;
                Aircraft.UpdateAircraftVariantDisplayName(newDisplayName);

                // Refresh the ComboBoxGeneralAircraftSelection field list on form
                ComboBoxGeneralAircraftSelection.DataSource = Aircraft.GetAircraftVariantDisplayNames();

                // Refreshing ComboBoxGeneralAircraftSelection.DataSource triggers ComboBoxGeneralAircraftSelection_SelectedIndexChanged
                // so restore correct Aircraft.CurrentAircraftVariantIndex
                Aircraft.ChangeCurrentAircraftVariantIndex(newDisplayName);

                // Set selected index for ComboBoxGeneralAircraftSelection 
                ComboBoxGeneralAircraftSelection.SelectedIndex = Aircraft.CurrentAircraftVariantIndex;

                // Refresh TextBoxGeneralLocationFilters field on form
                TextBoxGeneralAircraftValues.Text = Aircraft.SetTextBoxGeneralAircraftValues();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Don't delete a character
                e.SuppressKeyPress = true;

                if (((ComboBox)sender).Items.Count == 0)
                {
                    ((ComboBox)sender).Text = "";
                    return;
                }

                // Delete aircraftVariant instance in Aircraft.cs
                int oldIndex = Aircraft.CurrentAircraftVariantIndex;
                string deleteDisplayName = ((ComboBox)sender).Text;
                Aircraft.DeleteAircraftVariant(deleteDisplayName);

                // Refresh the ComboBoxGeneralAircraftSelection field list on form
                ComboBoxGeneralAircraftSelection.DataSource = Aircraft.GetAircraftVariantDisplayNames();

                // Set selected index for ComboBoxGeneralAircraftSelection 
                if (oldIndex != Aircraft.CurrentAircraftVariantIndex)
                {
                    if (oldIndex == 0 && ComboBoxGeneralAircraftSelection.Items.Count > 0)
                        // There were 2 or more items and first in list was deleted 
                        ComboBoxGeneralAircraftSelection.SelectedIndex = 0;
                    else if (ComboBoxGeneralAircraftSelection.Items.Count > 0)
                        // There were two or more items so change to prior item in list
                        ComboBoxGeneralAircraftSelection.SelectedIndex = oldIndex - 1;
                    else
                    {
                        // There was one item in list (or no items)
                        ComboBoxGeneralAircraftSelection.SelectedIndex = -1;
                        ((ComboBox)sender).Text = "";
                    }
                }

                // Refresh TextBoxGeneralLocationFilters field on form
                TextBoxGeneralAircraftValues.Text = Aircraft.SetTextBoxGeneralAircraftValues();
            }
        }

        private void ComboBoxGeneralAircraftSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            AircraftVariant aircraftVariant = Aircraft.AircraftVariants.Find(aircraft => aircraft.DisplayName == ((ComboBox)sender).Text);
            Aircraft.ChangeCurrentAircraftVariantIndex(aircraftVariant.DisplayName);

            // Refresh TextBoxGeneralLocationFilters field on form
            TextBoxGeneralAircraftValues.Text = Aircraft.SetTextBoxGeneralAircraftValues();

            SetDefaultCircuitParams();
        }

        private void ButtonRandomAircraft_Click(object sender, EventArgs e)
        {
            if (Aircraft.AircraftVariants == null || Aircraft.AircraftVariants.Count == 0)
                return;
            Random random = new();
            int randomAircraftIndex = random.Next(0, Aircraft.AircraftVariants.Count);
            ComboBoxGeneralAircraftSelection.SelectedIndex = randomAircraftIndex;

            // Update CurrentAircraftVariantIndex in Aircraft.cs
            string newDisplayName = ComboBoxGeneralAircraftSelection.Text;
            Aircraft.ChangeCurrentAircraftVariantIndex(newDisplayName);

            // Refresh TextBoxGeneralAircraftValues field on form
            TextBoxGeneralAircraftValues.Text = Aircraft.SetTextBoxGeneralAircraftValues();
        }

        private void TextBoxGeneralAircraftValues_MouseHover(object sender, EventArgs e)
        {
            ToolTip t = new();
            t.Show(TextBoxGeneralAircraftValues.Text, TextBoxGeneralAircraftValues, 0, 0, 5000);
        }

        #endregion

        #region Location selection

        private void ButtonRandomLocation_Click(object sender, EventArgs e)
        {
            Random random = new();
            int randomLocationFavouriteIndex = random.Next(0, Runway.LocationFavourites.Count);
            ComboBoxGeneralLocationFavourites.SelectedIndex = randomLocationFavouriteIndex;
        }

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

                // Create a new locationFavourite instance in Runway.cs using old name and all filters copied from current selected favourite
                Runway.AddLocationFavourite(oldFavouriteName);

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
                MessageBox.Show($"Select an aircraft to calculate default values", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private async void ComboBoxWikiURL_TextChanged(object sender, EventArgs e) // Make it async
        {
            // 1. Capture UI control values while still on the UI thread
            string selectedWikiUrl = ComboBoxWikiURL.SelectedItem?.ToString();
            string wikiUrlText = ComboBoxWikiURL.Text;
            string columnNumberText = TextBoxWikiItemLinkColumn.Text;

            // Perform initial validation of captured values
            if (string.IsNullOrEmpty(selectedWikiUrl))
            {
                // Optionally update status and return if no URL selected
                _statusProgress.Report("Please select a Wikipedia URL.");
                return;
            }

            if (!int.TryParse(columnNumberText, out int columnNo) || columnNo <= 0)
            {
                _statusProgress.Report("Invalid column number. Please enter a positive integer.");
                return;
            }

            // In case user clicks Generate Scenario button not having selected scenario type on general tab first
            Parameters.SelectedScenario = Constants.scenarioNames[(int)ScenarioTypes.WikiList];

            _statusProgress.Report($"Reading {wikiUrlText} and column {columnNumberText}, please wait...");
            Enabled = false; // Disable entire form to prevent further interaction

            try
            {
                bool success = await Task.Run(() =>
                {
                    return WikiPageHtmlParser.PopulateWikiPage(
                        selectedWikiUrl,
                        columnNo,
                        _statusProgress 
                    );
                });

                if (success)
                {
                    ComboBoxWikiTableNames.DataSource = Wikipedia.CreateWikiTablesDesc();
                    _statusProgress.Report("Wiki page data loaded successfully.");
                }
                else
                {
                    _statusProgress.Report("Failed to load Wiki page data. Check logs for details.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An unhandled error occurred during Wiki page loading: {ex.Message}");
                _statusProgress.Report("An unexpected error occurred. See logs.");
            }
            finally
            {
                Enabled = true;
                ComboBox_SelectedIndexChanged(sender, e);
            }
        }

        /// <summary>
        /// Handles the TextChanged event for the ComboBoxWikiURL. This method orchestrates the retrieval
        /// and parsing of Wikipedia page data based on the selected URL and column number.
        /// It performs UI validation, offloads the data processing to a background task,
        /// provides progress updates, and updates the UI upon completion or error.
        /// </summary>
        /// <param name="sender">The source of the event, typically the ComboBoxWikiURL.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        private void TextBoxWikiItemLinkColumn_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(TextBoxWikiItemLinkColumn.Text, out int columnNo) || columnNo <= 0)
            {
                _statusProgress.Report("Invalid column number. Please enter a positive integer.");
                return;
            }
            ComboBoxWikiURL_TextChanged(ComboBoxWikiURL, EventArgs.Empty);
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
                List<string> clonedItemList = [.. itemList];
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
            Aircraft.LoadAircraftVariants();
            if (Aircraft.AircraftVariants != null && Aircraft.AircraftVariants.Count > 0)
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
        /// Compares last modified date of scenery.cfg with that of runways.xml.
        /// If scenery.cfg has been modified more recently, warns user that runways.xml
        /// may be out of date and needs to be recreated.
        /// </summary>
        /// <remarks>
        /// If a warning is displayed, the last modified date of runways.xml is updated
        /// to the current time. This action prevents the warning from being shown repeatedly
        /// on subsequent program runs until the user has addressed the underlying issue
        /// by recreating the runways.xml file.
        /// </remarks>
        internal static void CheckRunwaysXMLupToDate()
        {
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runways.xml");
            string sceneryCFGdirectory = Parameters.SettingsP3DprogramData + Parameters.SettingsSimulatorVersion;
            string sceneryCFGfilePath = Path.Combine(sceneryCFGdirectory, "scenery.cfg");

            try
            {
                // Check whether user created runways.xml exists. Exit if it doesn't as program is using the
                // default version of file. Otherwise get the last modified date.
                DateTime? xmlLastModified = GetFileLastWriteTime(xmlFilePath);
                if (!xmlLastModified.HasValue)
                {
                    return;
                }

                // Check that scenery.cfg exists. Advise user if it doesn't. Otherwise get last modified date.
                DateTime? sceneryLastModified = GetFileLastWriteTime(sceneryCFGfilePath);
                if (!sceneryLastModified.HasValue)
                {
                    string sceneryCfgMissingMessage = $"The scenery.cfg file is not in folder \"{sceneryCFGdirectory}\"." +
                        " The program uses scenery.cfg last modified date to check whether user created runways.xml is up-to-date." +
                        " You may need to update the simulator version number in settings.";
                    MessageBox.Show(sceneryCfgMissingMessage, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Do comparison of runway.xml and scenery.cfg dates and warn user if necessary
                if (sceneryLastModified > xmlLastModified)
                {
                    string runwaysXmlOutOfDateMessage = $"The scenery.cfg file has been modified more recently than the user created runways.xml file." +
                        " Consider rebuilding the runways.xml file to include recently added airports." +
                        " The program will now refresh the last modified date on runways.xml to prevent repeated warnings";
                    MessageBox.Show(runwaysXmlOutOfDateMessage, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DateTime currentTime = DateTime.Now;
                    File.SetLastWriteTime(xmlFilePath, currentTime);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Error: Access to a file is denied. Please check permissions. Details: {ex.Message}",
                                Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"An I/O error occurred while checking file dates. Details: {ex.Message}",
                                Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                                Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Gets the last write time of a file. Returns null if the file does not exist.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns>The DateTime of the last write time, or null if the file does not exist.</returns>
        private static DateTime? GetFileLastWriteTime(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return fileInfo.Exists ? fileInfo.LastWriteTime : null;
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
        /// Recursively processes all controls to copy the associated user setting values into the control's properties.
        /// Settings are retrieved by control.Name. For ComboBoxes, it restores items and SelectedIndex.
        /// </summary>
        /// <param name="controlCollection">The collection of controls to be processed, including all child control collections</param>
        private static void RestoreUserSettings(Control.ControlCollection controlCollection)
        {
            foreach (Control control in controlCollection)
            {
                // Process child controls recursively first
                if (control.Controls.Count > 0)
                {
                    RestoreUserSettings(control.Controls);
                }

                // Now process the current control
                string settingName = control.Name; // Base setting name for the control

                // Handle TextBox
                if (control is TextBox textBox)
                {
                    try
                    {
                        object settingsValue = Properties.Settings.Default[settingName];

                        if (settingsValue != null)
                        {
                            textBox.Text = settingsValue.ToString();
                        }
                        else
                        {
                            textBox.Text = string.Empty;
                        }
                    }
                    catch (SettingsPropertyNotFoundException)
                    {
                        // Use Log.Info or Log.Warning, as not finding a setting isn't necessarily an error.
                        // For example, if a new control is added but no setting for it yet.
                        Log.Info($"RestoreUserSettings: Setting '{settingName}' not found for TextBox. Skipping.");
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Error($"RestoreUserSettings: Type mismatch for TextBox setting '{settingName}'. Exception: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RestoreUserSettings: An unexpected error occurred for TextBox '{settingName}'. Exception: {ex.Message}", ex);
                    }
                }
                // Handle ComboBox
                else if (control is ComboBox comboBox)
                {
                    string itemsSettingName = settingName;
                    string selectedIndexSettingName = settingName + "SelectedIndex";

                    // Restore ComboBox Items (StringCollection)
                    try
                    {
                        object itemsValue = Properties.Settings.Default[itemsSettingName];

                        if (itemsValue is StringCollection savedItems && savedItems.Count > 0)
                        {
                            comboBox.Items.Clear();
                            foreach (string item in savedItems)
                            {
                                comboBox.Items.Add(item);
                            }
                        }
                    }
                    catch (SettingsPropertyNotFoundException)
                    {
                        Log.Info($"RestoreUserSettings: Items setting '{itemsSettingName}' not found for ComboBox. Skipping items restoration.");
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Error($"RestoreUserSettings: Type mismatch for ComboBox items setting '{itemsSettingName}'. Exception: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RestoreUserSettings: An unexpected error occurred for ComboBox items '{itemsSettingName}'. Exception: {ex.Message}", ex);
                    }

                    // Restore ComboBox SelectedIndex
                    try
                    {
                        object selectedIndexValue = Properties.Settings.Default[selectedIndexSettingName];

                        if (selectedIndexValue is int savedIndex)
                        {
                            if (savedIndex >= 0 && savedIndex < comboBox.Items.Count)
                            {
                                comboBox.SelectedIndex = savedIndex;
                            }
                            else if (comboBox.Items.Count > 0)
                            {
                                Log.Warning($"RestoreUserSettings: Invalid saved SelectedIndex for '{selectedIndexSettingName}' ({savedIndex}). Defaulted to 0.");
                            }
                            else
                            {
                                comboBox.SelectedIndex = -1;
                            }
                        }
                    }
                    catch (SettingsPropertyNotFoundException)
                    {
                        Log.Info($"RestoreUserSettings: SelectedIndex setting '{selectedIndexSettingName}' not found for ComboBox. Skipping index restoration.");
                    }
                    catch (InvalidCastException ex)
                    {
                        Log.Error($"RestoreUserSettings: Type mismatch for ComboBox SelectedIndex setting '{selectedIndexSettingName}'. Exception: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"RestoreUserSettings: An unexpected error occurred for ComboBox SelectedIndex '{selectedIndexSettingName}'. Exception: {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively processes all controls to copy their relevant property values into associated user setting values.
        /// Settings are saved by control.Name. For ComboBoxes, it saves items and SelectedIndex.
        /// All changes are saved to disk once at the end.
        /// </summary>
        /// <param name="controlCollection">The collection of controls to be processed, including all child control collections</param>
        private static void SaveUserSettings(Control.ControlCollection controlCollection)
        {
            foreach (Control control in controlCollection)
            {
                // Process child controls recursively first
                if (control.Controls.Count > 0)
                {
                    SaveUserSettings(control.Controls);
                }

                // Now process the current control
                string settingName = control.Name;

                // Handle TextBox
                if (control is TextBox textBox)
                {
                    try
                    {
                        Properties.Settings.Default[settingName] = textBox.Text;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SaveUserSettings: Error saving TextBox '{settingName}'. Exception: {ex.Message}", ex);
                    }
                }
                // Handle ComboBox
                else if (control is ComboBox comboBox)
                {
                    string itemsSettingName = settingName;
                    string selectedIndexSettingName = settingName + "SelectedIndex";

                    // Save ComboBox Items (StringCollection)
                    try
                    {
                        var newList = new StringCollection();
                        foreach (object item in comboBox.Items)
                        {
                            newList.Add(item?.ToString() ?? string.Empty);
                        }
                        Properties.Settings.Default[itemsSettingName] = newList;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SaveUserSettings: Error saving ComboBox items for '{itemsSettingName}'. Exception: {ex.Message}", ex);
                    }

                    // Save ComboBox SelectedIndex
                    try
                    {
                        Properties.Settings.Default[selectedIndexSettingName] = comboBox.SelectedIndex;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"SaveUserSettings: Error saving ComboBox SelectedIndex for '{selectedIndexSettingName}'. Exception: {ex.Message}", ex);
                    }
                }
            }

            // Save all changes to disk once after processing all controls
            try
            {
                Properties.Settings.Default.Save();
                Log.Info("SaveUserSettings: All user settings saved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"SaveUserSettings: Failed to save all user settings. Exception: {ex.Message}", ex);
            }
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

        internal static Stream GetResourceStream(string resource)
        {
            string resourceName = $"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.{resource}";
            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream(resourceName);
            return stream;
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Runway.SaveLocationFavourites();
            Aircraft.SaveAircraftVariants();
            SaveUserSettings(TabPageSettings.Controls);
        }

        #endregion
    }
}
