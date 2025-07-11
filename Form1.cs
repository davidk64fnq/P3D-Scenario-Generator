﻿using CoordinateSharp;
using ImageMagick;
using Microsoft.Win32;
using MS.WindowsAPICodePack.Internal;
using P3D_Scenario_Generator.CelestialScenario;
using P3D_Scenario_Generator.CircuitScenario;
using P3D_Scenario_Generator.PhotoTourScenario;
using P3D_Scenario_Generator.SignWritingScenario;
using P3D_Scenario_Generator.WikipediaScenario;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    public partial class Form : System.Windows.Forms.Form
    {
        private readonly IProgress<string> _progressReporter;
        private ScenarioTypes _selectedScenarioType = ScenarioTypes.Circuit;
        private readonly ScenarioFormData _formData;

        public Form()
        {
            InitializeComponent();

            toolTip1.Popup += ToolTip1_Popup;

            _progressReporter = new Progress<string>(message =>
            {
                if (toolStripStatusLabel1 != null)
                {
                    toolStripStatusLabel1.Text = message;
                }
            });

            _formData = new ScenarioFormData();

            PrepareFormFields();
        }

        private void ToolTip1_Popup(object sender, PopupEventArgs e)
        {
            // If the associated control has an error, cancel the display of the standard ToolTip.
            if (!string.IsNullOrEmpty(errorProvider1.GetError(e.AssociatedControl)))
            {
                e.Cancel = true;
            }
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

        /// <summary>
        /// Handles the <see cref="ComboBoxGeneralScenarioType"/> selection change event. Updates the selected scenario
        /// type and adjusts the state of related controls accordingly.
        /// </summary>
        /// <remarks>This method parses the selected text in <see cref="ComboBoxGeneralScenarioType"/>
        /// into a <see cref="ScenarioTypes"/> enum value. If the parsing succeeds, it updates the selected scenario
        /// type and modifies the enabled state of related controls (<see cref="ComboBoxGeneralRunwaySelected"/>, <see
        /// cref="TextBoxGeneralSearchRunway"/>, and <see cref="ButtonRandRunway"/>) based on the selected scenario
        /// type.</remarks>
        /// <param name="sender">The source of the event, typically the <see cref="ComboBoxGeneralScenarioType"/> control.</param>
        /// <param name="e">The event data associated with the selection change.</param>
        private void ComboBoxGeneralScenarioType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse(ComboBoxGeneralScenarioType.Text, out ScenarioTypes result))
            {
                _selectedScenarioType = result;
                _progressReporter?.Report($"Scenario type set to: {_selectedScenarioType}");

                // Adjust visibility and enabled state of controls based on selected scenario type
                if (_selectedScenarioType == ScenarioTypes.PhotoTour
                    || _selectedScenarioType == ScenarioTypes.Celestial
                    || _selectedScenarioType == ScenarioTypes.WikiList)
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
        }

        private void ButtonGenerateScenario_Click(object sender, EventArgs e)
        {
            if (GetValidatedScenarioFormData())
            {
                DisplayStartMessage();
                CheckRunwaysXMLupToDate();
                Runway.SetRunwaysSubset();
                SaveUserSettings(TabPageSettings.Controls);
                ImageUtils.DrawScenarioImages(_formData);
                DoScenarioSpecificTasks();
                ScenarioFXML.GenerateFXMLfile(_formData);
                ScenarioHTML.GenerateHTMLfiles(_formData);
                ScenarioXML.GenerateXMLfile(_formData);
                DeleteTempScenarioDirectory();
                DisplayFinishMessage();
            }
        }

        private void DisplayStartMessage()
        {
            Cursor.Current = Cursors.WaitCursor;
            string message = $"Creating scenario files in \"{_formData.ScenarioFolderBase}\" - will confirm when complete";
            _progressReporter?.Report(message);
        }

        private void DoScenarioSpecificTasks()
        {
            if (_selectedScenarioType == ScenarioTypes.Circuit)
            {
                MakeCircuit.SetCircuit(_formData);
                SaveUserSettings(TabPageCircuit.Controls);
            }
            else if (_selectedScenarioType == ScenarioTypes.PhotoTour)
            {
                PhotoTour.SetPhotoTour(_formData);
                SaveUserSettings(TabPagePhotoTour.Controls);
            }
            else if (_selectedScenarioType == ScenarioTypes.SignWriting)
            {
                SignWriting.SetSignWriting(_formData);
                SaveUserSettings(TabPageSign.Controls);
            }
            else if (_selectedScenarioType == ScenarioTypes.Celestial)
            {
                if (CelestialNav.SetCelestial(_formData))
                    SaveUserSettings(TabPageWikiList.Controls);
            }
            else if (_selectedScenarioType == ScenarioTypes.WikiList)
            {
                Wikipedia.SetWikiTour(ComboBoxWikiTableNames.SelectedIndex, ComboBoxWikiRoute.Items, ComboBoxWikiStartingItem.SelectedItem,
                    ComboBoxWikiFinishingItem.SelectedItem, TextBoxWikiDistance.Text, _formData);
                SaveUserSettings(TabPageWikiList.Controls);
                ClearWikiListSettingsFields();
            }
            else
            {
                Runway.startRwy = Runway.Runways[_formData.RunwayIndex];
                Runway.destRwy = Runway.Runways[_formData.RunwayIndex];
            }
        }

        private void DisplayFinishMessage()
        {
            Cursor.Current = Cursors.Default;
            string message = $"Scenario files created in \"{_formData.ScenarioFolderBase}\" - enjoy your flight!";
            _progressReporter?.Report(message);
        }

        internal void SetScenarioTypesComboBox()
        {
            PopulateComboBoxWithEnum<ScenarioTypes>(ComboBoxGeneralScenarioType);
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

        private void TextBoxGeneralAircraftValues_MouseEnter(object sender, EventArgs e)
        {
            TextBoxMouseEnterExpandTooltip(sender, e);
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

        private void TextBoxGeneralLocationFilters_MouseEnter(object sender, EventArgs e)
        {
            TextBoxMouseEnterExpandTooltip(sender, e);
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

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitUpwind control.
        /// Validates the input in the text box as a double representing the Circuit Upwind Leg,
        /// ensuring it falls within the specified valid range (0.0 to Constants.EarthCircumferenceMiles).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitUpwind control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitUpwind_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitUpwind,
                "Circuit Upwind Leg",
                0.0,
                Constants.EarthCircumferenceMiles,
                "miles",
                value => _formData.CircuitUpwindLeg = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitBase control.
        /// Validates the input in the text box as a double representing the Circuit Base Leg,
        /// ensuring it falls within the specified valid range (Constants.MinCircuitGateSeparation to Constants.EarthCircumferenceMiles).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitBase control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitBase_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitBase,
                "Circuit Base Leg",
                Constants.MinCircuitGateSeparation,
                Constants.EarthCircumferenceMiles,
                "miles",
                value => _formData.CircuitBaseLeg = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitFinal control.
        /// Validates the input in the text box as a double representing the Circuit Final Leg,
        /// ensuring it falls within the specified valid range (0.0 to Constants.EarthCircumferenceMiles).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitFinal control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitFinal_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitFinal,
                "Circuit Final Leg",
                0.0,
                Constants.EarthCircumferenceMiles,
                "miles",
                value => _formData.CircuitFinalLeg = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitHeightUpwind control.
        /// Validates the input in the text box as a double representing the Circuit Upwind Leg Height,
        /// ensuring it falls within the specified valid range (0.0 to Constants.MaxCircuitGateHeight).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitHeightUpwind control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitHeightUpwind_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitHeightUpwind,
                "Circuit Height Upwind Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightUpwind = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitHeightDown control.
        /// Validates the input in the text box as a double representing the Circuit Down Leg Height,
        /// ensuring it falls within the specified valid range (0.0 to Constants.MaxCircuitGateHeight).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitHeightDown control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitHeightDown_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitHeightDown,
                "Circuit Height Down Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightDown = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitHeightBase control.
        /// Validates the input in the text box as a double representing the Circuit Base Leg Height,
        /// ensuring it falls within the specified valid range (0.0 to Constants.MaxCircuitGateHeight).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitHeightBase control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitHeightBase_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitHeightBase,
                "Circuit Height Base Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightBase = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitSpeed control.
        /// Validates the input in the text box as a double representing the Circuit Speed,
        /// ensuring it falls within the specified valid range (0.0 to Constants.PracticalMaxSpeed).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitSpeed control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitSpeed_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitSpeed,
                "Circuit Speed",
                0.0,
                Constants.PracticalMaxSpeed,
                "knots",
                value => _formData.CircuitSpeed = value);
        }

        /// <summary>
        /// Handles the Leave event for the TextBoxCircuitTurnRate control.
        /// Validates the input in the text box as a double representing the Circuit Turn Rate,
        /// ensuring it falls within the specified valid range (Constants.MinTurnTime360DegreesMinutes to Constants.MaxTurnTime360DegreesMinutes).
        /// Displays an error message using an ErrorProvider if validation fails,
        /// and reports the validation status via the progress reporter.
        /// Clears any existing error if the input is valid.
        /// </summary>
        /// <param name="sender">The source of the event, which is the TextBoxCircuitTurnRate control.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TextBoxCircuitTurnRate_Leave(object sender, EventArgs e)
        {
            ValidateAndSetDouble(
                TextBoxCircuitTurnRate,
                "Circuit Turn Rate",
                Constants.MinTurnTime360DegreesMinutes,
                Constants.MaxTurnTime360DegreesMinutes,
                "minutes",
                value => _formData.CircuitTurnDuration360Degrees = value);
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
                _progressReporter.Report("Please select a Wikipedia URL.");
                return;
            }

            if (!int.TryParse(columnNumberText, out int columnNo) || columnNo <= 0)
            {
                _progressReporter.Report("Invalid column number. Please enter a positive integer.");
                return;
            }

            // In case user clicks Generate Scenario button not having selected scenario type on general tab first
            _formData.ScenarioType = ScenarioTypes.WikiList;

            _progressReporter.Report($"Reading {wikiUrlText} and column {columnNumberText}, please wait...");
            Enabled = false; // Disable entire form to prevent further interaction

            try
            {
                bool success = await Task.Run(() =>
                {
                    return WikiPageHtmlParser.PopulateWikiPage(
                        selectedWikiUrl,
                        columnNo,
                        _formData,
                        _progressReporter
                    );
                });

                if (success)
                {
                    ComboBoxWikiTableNames.DataSource = Wikipedia.CreateWikiTablesDesc();
                    _progressReporter.Report("Wiki page data loaded successfully.");
                }
                else
                {
                    _progressReporter.Report("Failed to load Wiki page data. Check logs for details.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An unhandled error occurred during Wiki page loading: {ex.Message}");
                _progressReporter.Report("An unexpected error occurred. See logs.");
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
                _progressReporter.Report("Invalid column number. Please enter a positive integer.");
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

        private void TextBoxSettingsOSMServerAPIkey_Leave(object sender, EventArgs e)
        {
            ValidateOSMServerAPIkeyField();
        }

        private void TextBoxSettingsP3DprogramInstall_MouseEnter(object sender, EventArgs e)
        {
            TextBoxMouseEnterExpandTooltip(sender, e);
        }

        private void TextBoxSettingsP3DprogramData_MouseEnter(object sender, EventArgs e)
        {
            TextBoxMouseEnterExpandTooltip(sender, e);
        }

        private void TextBoxSettingsScenarioFolderBase_MouseEnter(object sender, EventArgs e)
        {
            TextBoxMouseEnterExpandTooltip(sender, e);
        }

        private void ComboBoxSettingsSimulatorVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateComboBoxSettingsSimulatorVersion();
        }

        /// <summary>
        /// Handles the Click event for the ButtonBrowseScenarioFolder.
        /// Opens a FolderBrowserDialog to allow the user to select a scenario output folder,
        /// populates the TextBoxSettingsScenarioFolderBase, and performs immediate validation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void ButtonBrowseScenarioFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            // Set the initial selected path to the current value in the textbox if it's a valid directory
            if (Directory.Exists(TextBoxSettingsScenarioFolderBase.Text))
            {
                folderBrowserDialog.SelectedPath = TextBoxSettingsScenarioFolderBase.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                TextBoxSettingsScenarioFolderBase.Text = folderBrowserDialog.SelectedPath;
                ValidateScenarioFolderBase();
            }
        }

        /// <summary>
        /// Handles the Click event for the ButtonBrowseP3DInstallFolder.
        /// Opens a FolderBrowserDialog to allow the user to select the P3D install folder,
        /// populates the TextBoxSettingsP3DprogramInstall, and performs immediate validation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void ButtonBrowseP3DInstallFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            // Set the initial selected path to the current value in the textbox if it's a valid directory
            if (Directory.Exists(TextBoxSettingsP3DprogramInstall.Text))
            {
                folderBrowserDialog.SelectedPath = TextBoxSettingsP3DprogramInstall.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                TextBoxSettingsP3DprogramInstall.Text = folderBrowserDialog.SelectedPath;
                ValidateP3DInstallFolder();
            }
        }

        /// <summary>
        /// Handles the Click event for the ButtonBrowseP3DDataFolder.
        /// Opens a FolderBrowserDialog to allow the user to select the P3D data folder,
        /// populates the TextBoxSettingsP3DprogramData, and performs immediate validation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void ButtonBrowseP3DDataFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog folderBrowserDialog = new();
            // Set the initial selected path to the current value in the textbox if it's a valid directory
            if (Directory.Exists(TextBoxSettingsP3DprogramData.Text))
            {
                folderBrowserDialog.SelectedPath = TextBoxSettingsP3DprogramData.Text;
            }

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                TextBoxSettingsP3DprogramData.Text = folderBrowserDialog.SelectedPath;
                ValidateP3DDataFolder();
            }
        }

        internal void SetMapAlignmentComboBox()
        {
            PopulateComboBoxWithEnum<WindowAlignment>(ComboBoxSettingsMapAlignment);
        }

        internal void SetMapWindowSizeComboBox()
        {
            PopulateComboBoxWithEnum<MapWindowSizeOption>(ComboBoxSettingsMapWindowSize);
        }

        #endregion

        #region Utilities

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            // Define the path to the CHM file relative to the application's executable directory.
            // Assuming the CHM file is located in a 'Resources/Help' folder in the output directory.
            string chmFilePath = Path.Combine(Application.StartupPath, "Resources", "Help", "Help.chm");

            // Check if the file exists before attempting to open it.
            if (File.Exists(chmFilePath))
            {
                // Open the CHM file.
                Help.ShowHelp(this, chmFilePath);
            }
            else
            {
                // Handle the case where the CHM file is not found (optional)
                MessageBox.Show("Help file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            SetScenarioTypesComboBox();

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
            SetMapAlignmentComboBox();
            SetMapWindowSizeComboBox();
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
        internal void CheckRunwaysXMLupToDate()
        {
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runways.xml");
            string sceneryCFGdirectory = _formData.P3DProgramData;
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
            //   if (parameterTokens.Length > 2)
            //   {
            //       for (int index = 2; index < parameterTokens.Length; index += 2)
            //       {
            //    if (!TextboxComparison(parameterTokens[index].Trim(), parameterTokens[index + 1].Trim(), ((TextBox)sender).Text, ((TextBox)sender).AccessibleName, e))
            //                return;
            //       }
            //   }
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

        /// <summary>
        /// Handles the MouseEnter event for a TextBox. Checks if the text content of the TextBox is truncated
        /// and displays a tooltip containing the full text if truncation is detected.
        /// If the text is not truncated, the tooltip is cleared.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a TextBox control.</param>
        /// <param name="e">The EventArgs instance containing the event data.</param>
        private void TextBoxMouseEnterExpandTooltip(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            // Simple check for truncation (you might need a more robust one)
            if (TextRenderer.MeasureText(tb.Text, tb.Font).Width > tb.ClientSize.Width)
            {
                toolTip1.SetToolTip(tb, tb.Text);
            }
            else
            {
                // If not truncated, ensure no tooltip or set a default empty one
                toolTip1.SetToolTip(tb, "");
            }
        }

        /// <summary>
        /// Populates a given ComboBox with the string representations (Description attributes if available)
        /// of the values from a specified Enum type.
        /// </summary>
        /// <typeparam name="TEnum">The Enum type whose values will populate the ComboBox.</typeparam>
        /// <param name="comboBox">The ComboBox control to populate.</param>
        private static void PopulateComboBoxWithEnum<TEnum>(ComboBox comboBox) where TEnum : struct, Enum
        {
            // Clear any existing items
            comboBox.Items.Clear();

            // Loop through each value in the enum and add its Description to the ComboBox
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                comboBox.Items.Add(enumValue.GetDescription());
            }

            // Optionally, select the first item if available
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Deletes the temporary directory and its contents stored in ScenarioFormData.
        /// Reports progress and logs errors.
        /// </summary>
        private void DeleteTempScenarioDirectory()
        {
            if (!string.IsNullOrEmpty(_formData.TempScenarioDirectory) && Directory.Exists(_formData.TempScenarioDirectory))
            {
                try
                {
                    Directory.Delete(_formData.TempScenarioDirectory, true); // 'true' for recursive delete
                    _progressReporter?.Report($"Temporary directory deleted: {_formData.TempScenarioDirectory}");
                    Log.Info($"Temporary directory deleted: {_formData.TempScenarioDirectory}");
                    _formData.TempScenarioDirectory = null; // Clear the path after deletion
                }
                catch (UnauthorizedAccessException ex)
                {
                    string errorMessage = $"Access denied when deleting temporary directory. Please check permissions for '{_formData.TempScenarioDirectory}'. Error: {ex.Message}";
                    _progressReporter?.Report(errorMessage);
                    Log.Error(errorMessage, ex);
                }
                catch (IOException ex)
                {
                    string errorMessage = $"I/O error when deleting temporary directory. It might be in use. Error: {ex.Message}";
                    _progressReporter?.Report(errorMessage);
                    Log.Error(errorMessage, ex);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"An unexpected error occurred while deleting temporary directory. Error: {ex.Message}";
                    _progressReporter?.Report(errorMessage);
                    Log.Error(errorMessage, ex);
                }
            }
            else if (!string.IsNullOrEmpty(_formData.TempScenarioDirectory))
            {
                Log.Warning($"Attempted to delete temporary directory, but path was set but directory did not exist: {_formData.TempScenarioDirectory}");
            }
        }

        #endregion

        #region Populate ScenarioFormData

        /// <summary>
        /// Orchestrates the population and validation of all user input into a ScenarioFormData object.
        /// This method ensures a specific order of validation due to dependencies between tabs (e.g., General tab's
        /// scenario title validation depends on the Settings tab's scenario folder base path).
        /// </summary>
        /// <returns>True if all user input across all relevant tabs is valid; otherwise, false.</returns>
        private bool GetValidatedScenarioFormData()
        {
            errorProvider1.Clear();
            bool allValid = true;

            if (!PopulateAndValidateTempScenarioDirectory())
            {
                return false; 
            }

            // 1. Settings Tab Data - Validate first as other tabs might depend on these settings.
            if (!PopulateAndValidateSettingsTabData())
            {
                allValid = false;
            }

            // 2. General Tab Data 
            if (!PopulateAndValidateGeneralTabData(Aircraft.GetCurrentVariant()))
            {
                allValid = false;
            }

            // Derived fields based on General and/or Settings tab data
            if (allValid)
            {
                // Scenario folder path based on the scenario folder base path (settings tab) and scenario title (general tab)
                _formData.ScenarioFolder = Path.Combine(_formData.ScenarioFolderBase, _formData.ScenarioTitle);

                // Scenario image folder path based on the scenario folder path
                _formData.ScenarioImageFolder = Path.Combine(_formData.ScenarioFolder, "Images");

                // Season based on DatePickerValue
                var persianMonth = new PersianCalendar().GetMonth(_formData.DatePickerValue);
                _formData.Season = (Season)Math.Ceiling(persianMonth / 3.0);
            }

            // 3. Circuit Tab Data 
            if (!PopulateAndValidateCircuitTabData())
            {
                allValid = false;
            }

            if (!allValid)
            {
                _progressReporter?.Report("Please correct the highlighted errors on all relevant tabs. Hover over error icon(s) for details.");
                DeleteTempScenarioDirectory();
                return false;
            }
            else
            {
                _progressReporter?.Report("Scenario data validated successfully.");
            }

            return true;
        }
        
        /// <summary>
        /// Creates a unique temporary directory for scenario generation files and stores its path
        /// in the ScenarioFormData. Reports progress and logs errors.
        /// </summary>
        /// <returns>True if the temporary directory was successfully created; otherwise, false.</returns>
        private bool PopulateAndValidateTempScenarioDirectory()
        {
            try
            {
                string tempBasePath = Path.GetTempPath();
                // Use a GUID to ensure a unique directory name for each scenario generation session
                _formData.TempScenarioDirectory = Path.Combine(tempBasePath, "P3DScenarioGeneratorTemp", Guid.NewGuid().ToString());

                Directory.CreateDirectory(_formData.TempScenarioDirectory);
                _progressReporter?.Report($"Temporary directory created at: {_formData.TempScenarioDirectory}");
                Log.Info($"Temporary directory created at: {_formData.TempScenarioDirectory}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMessage = $"Access denied when creating temporary directory. Please check permissions for '{Path.GetTempPath()}'. Error: {ex.Message}";
                _progressReporter?.Report(errorMessage);
                Log.Error(errorMessage, ex);
                return false;
            }
            catch (IOException ex)
            {
                string errorMessage = $"I/O error when creating temporary directory. Error: {ex.Message}";
                _progressReporter?.Report(errorMessage);
                Log.Error(errorMessage, ex);
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred while creating temporary directory. Error: {ex.Message}";
                _progressReporter?.Report(errorMessage);
                Log.Error(errorMessage, ex);
                return false;
            }
        }

        /// <summary>
        /// Populates and validates data from the Settings tab.
        /// </summary>
        /// <returns>True if all Settings tab data is valid; otherwise, false.</returns>
        private bool PopulateAndValidateSettingsTabData()
        {
            bool allValid = true;

            // Validate and populate CacheServerAPIkey
            allValid &= ValidateOSMServerAPIkeyField();

            // Validate the populate Simulator Version
            allValid &= ValidateComboBoxSettingsSimulatorVersion();

            // Validate and populate P3DInstallFolder
            allValid &= ValidateP3DInstallFolder();

            // Validate and populate P3DDataFolder
            allValid &= ValidateP3DDataFolder();

            // Validate and populate ScenarioFolderBase
            allValid &= ValidateScenarioFolderBase();

            // Validate and populate Map Window settings as a group
            allValid &= ValidateMapWindowSettingsGroup(null);

            return allValid;
        }

        /// <summary>
        /// Validates the OSM Server API Key field and populates the <see cref="ScenarioFormData.CacheServerAPIkey"/> property.
        /// Sets an error message via <see cref="ErrorProvider"/> and reports progress if validation fails.
        /// </summary>
        /// <returns><see langword="true"/> if the API key is valid; otherwise, <see langword="false"/>.</returns>
        private bool ValidateOSMServerAPIkeyField()
        {
            bool isValid = true;
            string apiKey = TextBoxSettingsOSMServerAPIkey.Text.Trim();
            string message = string.Empty;

            if (string.IsNullOrEmpty(apiKey))
            {
                message = "OSM Server API Key cannot be empty. Check documentation for details on registering for a free key";
                isValid = false;
            }
            else
            {
                // Call the HttpRoutines.ValidateMapTileServerKey method for actual server validation
                if (!HttpRoutines.ValidateMapTileServerKey(apiKey))
                {
                    // Validation failed. Construct a user-friendly message based on the failure.
                    // The HttpRoutines.ValidateMapTileServerKey method now logs the specific error.
                    // Here, we provide a general message to the user.
                    message = "OSM Server API Key validation failed. Please check the key and your internet connection. Refer to logs for more details.";
                    isValid = false;
                }
            }

            if (!isValid)
            {
                errorProvider1.SetError(TextBoxSettingsOSMServerAPIkey, message);
                _progressReporter?.Report(message);
            }
            else
            {
                // Clear previous error for API key
                errorProvider1.SetError(TextBoxSettingsOSMServerAPIkey, "");
                _progressReporter?.Report("OSM Server API Key looks valid.");
                _formData.CacheServerAPIkey = apiKey;
            }

            return isValid;
        }

        /// <summary>
        /// Validates the selected Simulator Version from the ComboBox and populates the <see cref="ScenarioFormData.SimulatorVersion"/> property.
        /// Checks for the corresponding Prepar3D registry key, retrieves the installation, program data, and sceanrio folder paths, and populates the relevant TextBoxes.
        /// It then validates these paths and reports errors via <see cref="ErrorProvider"/> and <see cref="_progressReporter"/> if validation fails.
        /// </summary>
        /// <returns><see langword="true"/> if the selected simulator version is valid and installed; otherwise, <see langword="false"/>.</returns>
        private bool ValidateComboBoxSettingsSimulatorVersion()
        {
            bool isValid = true;
            string simulatorVersion = ComboBoxSettingsSimulatorVersion.Text;

            // Attempt to get the registry key using the refactored method
            RegistryKey simKey = Aircraft.GetSimProgramFolderKey(simulatorVersion);

            if (simKey == null)
            {
                string validationMessage = $"Problem encountered referencing simulator version '{simulatorVersion}'. " +
                                           $"Please check if Prepar3D v{simulatorVersion} is installed or select a different version.";
                errorProvider1.SetError(ComboBoxSettingsSimulatorVersion, validationMessage);
                _progressReporter?.Report(validationMessage);
                isValid = false;
            }
            else
            {
                // If the key is found, clear any previous error and store the valid version
                errorProvider1.SetError(ComboBoxSettingsSimulatorVersion, "");
                _progressReporter?.Report("");
                _formData.SimulatorVersion = simulatorVersion;

                // Populate the TextBoxSettingsP3DprogramInstall, TextBoxSettingsP3DprogramData, and TextBoxSettingsScenarioFolderBase paths
                string installPath = simKey.GetValue("SetupPath") as string;
                if (!string.IsNullOrEmpty(installPath))
                {
                    string driveLetter = Path.GetPathRoot(installPath);
                    char[] separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
                    TextBoxSettingsP3DprogramInstall.Text = installPath.TrimEnd(separators);
                    ValidateP3DInstallFolder();
                    TextBoxSettingsP3DprogramData.Text = $"{driveLetter}ProgramData\\Lockheed Martin\\Prepar3D v{simulatorVersion}";
                    ValidateP3DDataFolder();
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    TextBoxSettingsScenarioFolderBase.Text = Path.Combine(documentsPath, $"Prepar3D v{simulatorVersion} Files");
                    ValidateScenarioFolderBase();
                }

                simKey.Close(); // Always close registry keys when done
            }

            return isValid;
        }

        /// <summary>
        /// Validates the P3D Program Install path and populates the <see cref="ScenarioFormData.P3DProgramInstall"/> property.
        /// Checks if the path is not empty and if the directory exists.
        /// Sets an error message via <see cref="ErrorProvider"/> and reports progress if validation fails.
        /// </summary>
        /// <returns><see langword="true"/> if the P3D Program Install path is valid and the directory exists; otherwise, <see langword="false"/>.</returns>
        private bool ValidateP3DInstallFolder()
        {
            bool isValid = true;
            string folderPath = TextBoxSettingsP3DprogramInstall.Text.Trim(); // Get path from control

            if (string.IsNullOrEmpty(folderPath))
            {
                string message = "P3D Program Install path cannot be empty.";
                errorProvider1.SetError(TextBoxSettingsP3DprogramInstall, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else if (!Directory.Exists(folderPath))
            {
                string message = $"P3D Program Install path does not exist: '{folderPath}'. Please ensure the directory exists.";
                errorProvider1.SetError(TextBoxSettingsP3DprogramInstall, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(TextBoxSettingsP3DprogramInstall, "");
                _progressReporter?.Report("");
                _formData.P3DProgramInstall = folderPath;
            }
            return isValid;
        }

        /// <summary>
        /// Validates the P3D Data path and populates the <see cref="ScenarioFormData.P3DProgramData"/> property.
        /// Checks if the path is not empty and if the directory exists.
        /// Sets an error message via <see cref="ErrorProvider"/> and reports progress if validation fails.
        /// </summary>
        /// <returns><see langword="true"/> if the P3D Program Data path is valid and the directory exists; otherwise, <see langword="false"/>.</returns>
        private bool ValidateP3DDataFolder()
        {
            bool isValid = true;
            string folderPath = TextBoxSettingsP3DprogramData.Text.Trim(); // Get path from control

            if (string.IsNullOrEmpty(folderPath))
            {
                string message = "P3D Program Data path cannot be empty.";
                errorProvider1.SetError(TextBoxSettingsP3DprogramData, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else if (!Directory.Exists(folderPath))
            {
                string message = $"P3D Program Data path does not exist: '{folderPath}'. Please ensure the directory exists.";
                errorProvider1.SetError(TextBoxSettingsP3DprogramData, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(TextBoxSettingsP3DprogramData, "");
                _progressReporter?.Report("");
                _formData.P3DProgramInstall = folderPath;
            }
            return isValid;
        }

        /// <summary>
        /// Validates the Scenario Folder Base path and populates the <see cref="ScenarioFormData.ScenarioFolderBase"/> property.
        /// Checks if the path is not empty and if the directory exists.
        /// Sets an error message via <see cref="ErrorProvider"/> and reports progress if validation fails.
        /// </summary>
        /// <returns><see langword="true"/> if the scenario folder path is valid and the directory exists; otherwise, <see langword="false"/>.</returns>
        private bool ValidateScenarioFolderBase()
        {
            bool isValid = true;
            string folderPath = TextBoxSettingsScenarioFolderBase.Text.Trim(); // Get path from control

            if (string.IsNullOrEmpty(folderPath))
            {
                string message = "Scenario Folder Base path cannot be empty.";
                errorProvider1.SetError(TextBoxSettingsScenarioFolderBase, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else if (!Directory.Exists(folderPath))
            {
                string message = $"Scenario Folder Base path does not exist: '{folderPath}'. Please ensure the directory exists.";
                errorProvider1.SetError(TextBoxSettingsScenarioFolderBase, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(TextBoxSettingsScenarioFolderBase, "");
                _progressReporter?.Report("");
                _formData.ScenarioFolderBase = folderPath;
            }
            return isValid;
        }

        /// <summary>
        /// Validates the group of map window settings (monitor number, offset, alignment, width, height, window size).
        /// Performs individual validation for each control and then checks interdependencies.
        /// Reports errors via ErrorProvider for specific controls and _progressReporter for general messages.
        /// </summary>
        /// <param name="triggeringControl">The control that initiated this validation, or null if called from a general validation.</param>
        /// <returns>True if all map window settings are valid individually and as a group; otherwise, false.</returns>
        private bool ValidateMapWindowSettingsGroup(Control triggeringControl)
        {
            bool allValid = true;
            string groupErrorMessage = ""; // To accumulate group-level errors

            // Clear any existing errors on these controls before re-validating the group
            errorProvider1.SetError(TextBoxSettingsMapMonitorNumber, "");
            errorProvider1.SetError(TextBoxSettingsMapOffset, "");
            errorProvider1.SetError(ComboBoxSettingsMapAlignment, "");
            errorProvider1.SetError(TextBoxSettingsMapMonitorWidth, "");
            errorProvider1.SetError(TextBoxSettingsMapMonitorHeight, "");
            errorProvider1.SetError(ComboBoxSettingsMapWindowSize, "");
            _progressReporter?.Report(""); // Clear previous group messages

            // 1. Individual Validation and Population
            allValid &= ValidateAndSetInteger(
                TextBoxSettingsMapMonitorNumber,
                "Map Monitor Number",
                0,
                Constants.MaxMonitorNumber,
                "",
                value => _formData.MapMonitorNumber = value);

            allValid &= ValidateAndSetInteger(
                TextBoxSettingsMapOffset,
                "Map Offset",
                0,
                Constants.MaxWindowOffset,
                "pixels",
                value => _formData.MapOffset = value);

            allValid &= ValidateAndSetEnum<WindowAlignment>(
                ComboBoxSettingsMapAlignment,
                "Map Alignment",
                value => _formData.MapAlignment = value); 

            allValid &= ValidateAndSetInteger(
                TextBoxSettingsMapMonitorWidth,
                "Map Monitor Width",
                Constants.MinMonitorWidth,
                Constants.MaxMonitorWidth,
                "pixels",
                value => _formData.MapMonitorWidth = value);

            allValid &= ValidateAndSetInteger(
                TextBoxSettingsMapMonitorHeight,
                "Map Monitor Height",
                Constants.MinMonitorHeight,
                Constants.MaxMonitorHeight,
                "pixels",
                value => _formData.MapMonitorHeight = value);

            // Validate and set Map Window Size using the new enum
            allValid &= ValidateAndSetEnum<MapWindowSizeOption>(
                ComboBoxSettingsMapWindowSize,
                "Map Window Size",
                value => _formData.MapWindowSize = value); 

            // If any individual validation failed, return false immediately.
            // The specific error providers are already set.
            if (!allValid)
            {
                groupErrorMessage = "Please correct individual errors in Map Window settings.";
                _progressReporter?.Report(groupErrorMessage);
                // No need to set group error here, individual errors are already set.
                return false;
            }

            // 2. Interdependent Validation (only if all individual fields are valid)
            // No need for re-parsing from string; use _formData.WindowAlignment and _formData.MapWindowSize directly
            WindowAlignment currentAlignment = _formData.MapAlignment;
            int mapWindowSize = (int)_formData.MapWindowSize; // Cast enum to int for calculations

            bool groupValidationPassed = true;

            switch (currentAlignment)
            {
                case WindowAlignment.Centered:
                    // For Centered, map window just has to fit on the monitor
                    if (mapWindowSize > _formData.MapMonitorWidth || mapWindowSize > _formData.MapMonitorHeight)
                    {
                        groupErrorMessage = $"Map Window size ({mapWindowSize}px square) exceeds Monitor size ({_formData.MapMonitorWidth} x {_formData.MapMonitorHeight}px).";
                        groupValidationPassed = false;
                    }
                    break;

                case WindowAlignment.TopLeft:
                case WindowAlignment.TopRight:
                case WindowAlignment.BottomRight:
                case WindowAlignment.BottomLeft:
                    // For corner alignments, map window + 2 * offset must fit
                    if ((mapWindowSize + (2 * _formData.MapOffset)) > _formData.MapMonitorWidth ||
                        (mapWindowSize + (2 * _formData.MapOffset)) > _formData.MapMonitorHeight)
                    {
                        groupErrorMessage = $"Map Window size ({mapWindowSize}px square) + offset ({_formData.MapOffset}px) exceeds Monitor size ({_formData.MapMonitorWidth} x {_formData.MapMonitorHeight}px).";
                        groupValidationPassed = false;
                    }
                    break;
            }

            if (!groupValidationPassed)
            {
                _progressReporter?.Report(groupErrorMessage);
                // Set the error on the triggering control if available, otherwise fall back to TextBoxSettingsMapOffset
                if (triggeringControl != null)
                {
                    errorProvider1.SetError(triggeringControl, groupErrorMessage);
                }
                else
                {
                    errorProvider1.SetError(TextBoxSettingsMapOffset, groupErrorMessage);
                }
                allValid = false;
            }
            else
            {
                _progressReporter?.Report("Map Window settings are valid.");
            }

            return allValid && groupValidationPassed;
        }

        /// <summary>
        /// Orchestrates the population and validation of data from the General tab by calling smaller, focused helper methods.
        /// </summary>
        /// <param name="selectedAircraftVariant">The currently selected AircraftVariant object.</param>
        /// <returns>True if all General tab data is valid; otherwise, false.</returns>
        private bool PopulateAndValidateGeneralTabData(AircraftVariant selectedAircraftVariant)
        {
            bool allValid = true;

            // Handle direct UI element assignments and basic type parsing
            allValid &= PopulateBasicGeneralTabValues();

            // Validate and populate the Scenario Title
            allValid &= ValidateAndPopulateScenarioTitle();

            // Validate and populate Aircraft Properties sourced from the selectedAircraftVariant
            allValid &= ValidateAndPopulateAircraftDetails(selectedAircraftVariant);

            return allValid;
        }

        /// <summary>
        /// Populates basic values from General tab UI elements into formData.
        /// </summary>
        /// <returns>True if basic values are successfully populated; otherwise, false.</returns>
        private bool PopulateBasicGeneralTabValues()
        {
            bool isValid = true;

            _formData.RunwayIndex = ComboBoxGeneralRunwaySelected.SelectedIndex;

            if (Enum.TryParse(ComboBoxGeneralScenarioType.SelectedItem.ToString(), out ScenarioTypes selectedType))
            {
                _formData.ScenarioType = selectedType;
            }
            else
            {
                Log.Error("Invalid scenario type selected unexpectedly.");
                isValid = false;
                string message = "Invalid scenario type selected. An unexpected error occurred; please notify the developer.";
                _progressReporter?.Report(message);
            }

            _formData.DatePickerValue = GeneralDatePicker.Value;
            _formData.TimePickerValue = GeneralTimePicker.Value;

            return isValid;
        }

        /// <summary>
        /// Validates the General Scenario Title, including filename validity and folder existence,
        /// and populates it into formData.
        /// </summary>
        /// <returns>True if the scenario title is valid; otherwise, false.</returns>
        private bool ValidateAndPopulateScenarioTitle()
        {
            string settingsScenarioFolderBase = _formData.ScenarioFolderBase;
            string scenarioTitle = TextBoxGeneralScenarioTitle.Text;
            bool isValid = true;

            if (!IsValidFilename(scenarioTitle, out string titleFilenameErrorMessage))
            {
                errorProvider1.SetError(TextBoxGeneralScenarioTitle, titleFilenameErrorMessage);
                _progressReporter?.Report(titleFilenameErrorMessage);
                isValid = false;
            }
            else if (string.IsNullOrEmpty(settingsScenarioFolderBase))
            {
                string message = "Cannot fully validate scenario path: Scenario folder base path is not set or invalid. Please check settings tab.";
                errorProvider1.SetError(TextBoxGeneralScenarioTitle, message);
                _progressReporter?.Report(message);
                isValid = false;
            }
            else
            {
                errorProvider1.SetError(TextBoxGeneralScenarioTitle, "");
                string proposedSaveFolder = Path.Combine(settingsScenarioFolderBase, scenarioTitle);

                if (Directory.Exists(proposedSaveFolder))
                {
                    string message = $"Scenario exists: '{proposedSaveFolder}'. Delete folder (close P3D) or pick a new title.";
                    errorProvider1.SetError(TextBoxGeneralScenarioTitle, message);
                    _progressReporter?.Report(message);
                    isValid = false;
                }
                else
                {
                    _progressReporter?.Report("");
                    _formData.ScenarioTitle = scenarioTitle;
                }
            }
            return isValid;
        }

        /// <summary>
        /// Checks if the provided string is a valid filename in terms of character set and not being null or empty.
        /// This method validates against characters deemed invalid by the operating system.
        /// If validation fails, a descriptive error message is provided via an out parameter.
        /// </summary>
        /// <param name="fileName">The string to validate as a filename.</param>
        /// <param name="errorMessage">
        /// When this method returns, contains an error message if the filename is invalid;
        /// otherwise, an empty string. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the string is a valid filename (not null, not empty, and contains no invalid characters);
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsValidFilename(string fileName, out string errorMessage)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                errorMessage = "Filename cannot be empty or null.";
                return false;
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errorMessage = $"Filename {fileName} contains invalid characters. Please remove illegal characters (e.g., \\ / : * ? \" < > |).";
                return false;
            }

            errorMessage = "";
            return true;
        }

        /// <summary>
        /// Validates aircraft properties sourced from the selected AircraftVariant and populates them into formData.
        /// Specifically focuses on validating the ThumbnailImagePath existence,
        /// assuming DisplayName and CruiseSpeed are validated when the variant is initially read.
        /// Reports errors via ErrorProvider on TextBoxGeneralAircraftValues and ProgressReporter.
        /// </summary>
        /// <param name="selectedAircraftVariant">The currently selected AircraftVariant object.</param>
        /// <returns>True if aircraft details are valid; otherwise, false.</returns>
        private bool ValidateAndPopulateAircraftDetails(AircraftVariant selectedAircraftVariant)
        {
            bool isValid = true;
            // Clear any previous errors on the combined aircraft values textbox before new validation
            errorProvider1.SetError(TextBoxGeneralAircraftValues, "");

            if (selectedAircraftVariant == null)
            {
                string message = "No aircraft variant has been selected. Please select an aircraft.";
                errorProvider1.SetError(TextBoxGeneralAircraftValues, "No aircraft selected.");
                _progressReporter?.Report(message);
                isValid = false;
            }
            else
            {
                // Assign DisplayName and CruiseSpeed directly, as their validation is assumed to be handled
                // when the variant is initially read into the program in Aircraft.cs.
                _formData.AircraftTitle = selectedAircraftVariant.DisplayName;
                _formData.AircraftCruiseSpeed = selectedAircraftVariant.CruiseSpeed;

                // Validate Aircraft Image Path (ThumbnailImagePath string from AircraftVariant)
                if (!File.Exists(selectedAircraftVariant.ThumbnailImagePath))
                {
                    string message = $"Aircraft Thumbnail Image file does not exist at the specified path: '{selectedAircraftVariant.ThumbnailImagePath}'.";
                    errorProvider1.SetError(TextBoxGeneralAircraftValues, "Aircraft Image file not found.");
                    _progressReporter?.Report(message);
                    isValid = false;
                }
                else
                {
                    _formData.AircraftImagePath = selectedAircraftVariant.ThumbnailImagePath;
                }
            }
            // Only clear the error on TextBoxGeneralAircraftValues if all checks in THIS method passed.
            // If any check fails, isValid will be false, and the error will remain.
            if (isValid)
            {
                _progressReporter?.Report("");
                errorProvider1.SetError(TextBoxGeneralAircraftValues, "");
            }
            return isValid;
        }

        /// <summary>
        /// Populates and validates data from the Circuit tab.
        /// </summary>
        /// <returns>True if all Circuit tab data is valid; otherwise, false.</returns>
        private bool PopulateAndValidateCircuitTabData()
        {
            bool allValid = true;

            // CircuitUpwindLeg
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitUpwind,
                "Circuit Upwind Leg",
                0.0,
                Constants.EarthCircumferenceMiles,
                "nautical miles",
                value => _formData.CircuitUpwindLeg = value);

            // CircuitBaseLeg
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitBase,
                "Circuit Base Leg",
                Constants.MinCircuitGateSeparation,
                Constants.EarthCircumferenceMiles,
                "nautical miles",
                value => _formData.CircuitBaseLeg = value);

            // CircuitFinalLeg
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitFinal,
                "Circuit Final Leg",
                0.0,
                Constants.EarthCircumferenceMiles,
                "nautical miles",
                value => _formData.CircuitFinalLeg = value);

            // CircuitHeightUpwind
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitHeightUpwind,
                "Circuit Height Upwind Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightUpwind = value);

            // CircuitHeightDown
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitHeightDown,
                "Circuit Height Down Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightDown = value);

            // CircuitHeightBase
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitHeightBase,
                "Circuit Height Base Leg",
                0.0,
                Constants.MaxCircuitGateHeight,
                "feet",
                value => _formData.CircuitHeightBase = value);

            // CircuitSpeed
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitSpeed,
                "Circuit Speed",
                0.0,
                Constants.PracticalMaxSpeed,
                "knots",
                value => _formData.CircuitSpeed = value);

            // CircuitTurnDuration360Degrees
            allValid &= ValidateAndSetDouble(
                TextBoxCircuitTurnRate,
                "Circuit Turn Rate",
                Constants.MinTurnTime360DegreesMinutes,
                Constants.MaxTurnTime360DegreesMinutes,
                "minutes",
                value => _formData.CircuitTurnDuration360Degrees = value);

            return allValid;
        }

        /// <summary>
        /// Generic validation method for integer text boxes.
        /// Parses the text, validates its range, sets error messages, reports progress,
        /// and assigns the parsed value to the specified formData property via an Action delegate.
        /// </summary>
        /// <param name="textBox">The TextBox control to validate.</param>
        /// <param name="valueName">A descriptive name for the value (e.g., "Map Monitor Number") used in error messages.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="units">Optional: A string representing the units of the value (e.g., "pixels").</param>
        /// <param name="setFormDataProperty">An Action delegate that takes the parsed integer value and assigns it to the corresponding ScenarioFormData property.</param>
        /// <returns>True if the string was successfully parsed into an integer and is within the specified range; otherwise, false.</returns>
        private bool ValidateAndSetInteger(
            TextBox textBox,
            string valueName,
            int minValue,
            int maxValue,
            string units,
            Action<int> setFormDataProperty)
        {
            if (!ParsingHelpers.TryParseInteger(
                textBox.Text,
                valueName,
                minValue,
                maxValue,
                out int parsedValue,
                out string validationMessage,
                units))
            {
                errorProvider1.SetError(textBox, validationMessage);
                _progressReporter?.Report(validationMessage);
                return false;
            }
            else
            {
                errorProvider1.SetError(textBox, "");
                _progressReporter?.Report("");
                setFormDataProperty(parsedValue); // Assign the parsed value using the delegate
                return true;
            }
        }

        /// <summary>
        /// Generic validation method for double text boxes.
        /// Parses the text, validates its range, sets error messages, reports progress,
        /// and assigns the parsed value to the specified formData property via an Action delegate.
        /// </summary>
        /// <param name="textBox">The TextBox control to validate.</param>
        /// <param name="valueName">A descriptive name for the value (e.g., "Circuit Speed") used in error messages.</param>
        /// <param name="minValue">The minimum allowed value (inclusive).</param>
        /// <param name="maxValue">The maximum allowed value (inclusive).</param>
        /// <param name="units">Optional: A string representing the units of the value (e.g., "knots").</param>
        /// <param name="setFormDataProperty">An Action delegate that takes the parsed double value and assigns it to the corresponding ScenarioFormData property.</param>
        /// <returns>True if the string was successfully parsed into a double and is within the specified range; otherwise, false.</returns>
        private bool ValidateAndSetDouble(
            TextBox textBox,
            string valueName,
            double minValue,
            double maxValue,
            string units,
            Action<double> setFormDataProperty)
        {
            if (!ParsingHelpers.TryParseDouble(
                textBox.Text,
                valueName,
                minValue,
                maxValue,
                out double parsedValue,
                out string validationMessage,
                units))
            {
                errorProvider1.SetError(textBox, validationMessage);
                _progressReporter?.Report(validationMessage);
                return false;
            }
            else
            {
                errorProvider1.SetError(textBox, "");
                _progressReporter?.Report("");
                setFormDataProperty(parsedValue); // Assign the parsed value using the delegate
                return true;
            }
        }

        /// <summary>
        /// Handles the Leave event for TextBoxSettingsMapMonitorNumber.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void TextBoxSettingsMapMonitorNumber_Leave(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Handles the Leave event for TextBoxSettingsMapOffset.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void TextBoxSettingsMapOffset_Leave(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event for ComboBoxSettingsMapAlignment.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void ComboBoxSettingsMapAlignment_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Handles the Leave event for TextBoxSettingsMapMonitorWidth.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void TextBoxSettingsMapMonitorWidth_Leave(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Handles the Leave event for TextBoxSettingsMapMonitorHeight.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void TextBoxSettingsMapMonitorHeight_Leave(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event for ComboBoxSettingsMapWindowSize.
        /// Triggers group validation for map window settings.
        /// </summary>
        private void ComboBoxSettingsMapWindowSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateMapWindowSettingsGroup((Control)sender);
        }

        /// <summary>
        /// Generic validation method for ComboBoxes representing an Enum.
        /// Parses the selected text into the specified Enum type, sets error messages, reports progress,
        /// and assigns the parsed enum value to the specified formData property via an Action delegate.
        /// </summary>
        /// <typeparam name="TEnum">The Enum type to parse to.</typeparam>
        /// <param name="comboBox">The ComboBox control to validate.</param>
        /// <param name="valueName">A descriptive name for the value (e.g., "Map Alignment") used in error messages.</param>
        /// <param name="setFormDataProperty">An Action delegate that takes the parsed enum value and assigns it to the corresponding ScenarioFormData property.</param>
        /// <returns>True if the string was successfully parsed into the Enum type; otherwise, false.</returns>
        private bool ValidateAndSetEnum<TEnum>(
            ComboBox comboBox,
            string valueName,
            Action<TEnum> setFormDataProperty) where TEnum : struct, Enum
        {
            string selectedText = comboBox.Text.Trim();
            if (string.IsNullOrEmpty(selectedText))
            {
                string message = $"{valueName} cannot be empty.";
                errorProvider1.SetError(comboBox, message);
                _progressReporter?.Report(message);
                return false;
            }

            // Iterate through enum values to find a match by description
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                if (enumValue.GetDescription().Equals(selectedText, StringComparison.OrdinalIgnoreCase))
                {
                    errorProvider1.SetError(comboBox, "");
                    _progressReporter?.Report("");
                    setFormDataProperty(enumValue); // Assign the actual enum value
                    return true;
                }
            }

            // If no match by description or name
            string noMatchMessage = $"Invalid selection for '{valueName}'. Please select a valid option.";
            errorProvider1.SetError(comboBox, noMatchMessage);
            _progressReporter?.Report(noMatchMessage);
            return false;
        }

        #endregion
    }
}
