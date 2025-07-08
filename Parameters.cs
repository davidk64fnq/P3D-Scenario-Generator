using System.Globalization;
using HtmlAgilityPack;

namespace P3D_Scenario_Generator
{
    internal class Parameters
    {
        #region General tab

        internal static int SelectedRunwayIndex { get; set; }

        /// <summary>
        /// The selected scenario type as an enum value.
        /// </summary>
        internal static ScenarioTypes SelectedScenarioType { get; set; }
        internal static int DayOfYear { get; private set; }
        internal static int Day { get; private set; }
        internal static int Month { get; private set; }
        internal static int Year { get; private set; }
        internal static Season Season { get; private set; }
        internal static int Hours { get; private set; }
        internal static int Minutes { get; private set; }
        internal static int Seconds { get; private set; }
        internal static string GeneralScenarioTitle { get; private set; }
        internal static string AircraftTitle { get; private set; }
        internal static double AircraftCruiseSpeed { get; private set; }
        internal static string AircraftImagePath { get; private set; }

        #endregion

        #region Circuit tab

        /// <summary>
        /// Distance between runway and gate 1 in miles
        /// </summary>
        internal static double UpwindLeg { get; private set; }

        /// <summary>
        /// Distance from gate 2 to gate 3 and from gate 6 to gate 7
        /// </summary>
        internal static double BaseLeg { get; private set; }

        /// <summary>
        /// Distance between gate 8 and runway in miles
        /// </summary>
        internal static double FinalLeg { get; private set; }

        /// <summary>
        /// User specified height of gate 1
        /// </summary>
        internal static double HeightUpwind { get; private set; }

        /// <summary>
        /// User specified height of gates 3 to 6
        /// </summary>
        internal static double HeightDown { get; private set; }

        /// <summary>
        /// User specified height of gate 8
        /// </summary>
        internal static double HeightBase { get; private set; }

        /// <summary>
        /// User specified cruise speed of <see cref="SelectedAircraft"/> in knots nm/hr (default value read from aircraft.cfg)
        /// </summary>
        internal static double Speed { get; private set; }

        /// <summary>
        /// User specified turn rate of <see cref="SelectedAircraft"/> in minutes (standard turn rate is 360 degrees in 2 minutes 
        /// but default value of 4 minutes seems to work better)
        /// </summary>
        internal static double TurnRate { get; private set; }

        #endregion

        # region PhotoTour tab

        /// <summary>
        /// Maximum photo tour leg distance, between airport and photo location or between two photo locations, in miles.
        /// </summary>
        internal static double PhotoTourConstraintsMaxLegDist { get; private set; }

        /// <summary>
        /// Minimum photo tour leg distance, between airport and photo location or between two photo locations, in miles.
        /// </summary>
        internal static double PhotoTourConstraintsMinLegDist { get; private set; }

        /// <summary>
        /// A photo tour from starting airport to one photo location and then onto destination airport, 
        /// which may be the same as starting airport, is 2 legs. Every additional photo location adds one leg.
        /// </summary>
        internal static double PhotoTourConstraintsMinNoLegs { get; private set; }

        /// <summary>
        /// The maximum number of legs in the photo tour where the leg from last photo location to destination airport is included. 
        /// So for a given value of this parameter the maximum number of photo locations will be one less.
        /// </summary>
        internal static double PhotoTourConstraintsMaxNoLegs { get; private set; }

        /// <summary>
        /// User can select a map window size of either 512 or 1024 pixels square.
        /// </summary>
        internal static int PhotoTourMapWindowSize { get; set; }

        /// <summary>
        /// Refers to the maximum bearing change allowed between successive legs of the photo tour. The smaller this number 
        /// the less likely the tour will entail circling back towards the starting airport. This parameter doesn't apply 
        /// for a one photo location tour to allow return to starting airport as a possibility.
        /// </summary>
        internal static double PhotoTourConstraintsMaxBearingChange { get; set; }

        /// <summary>
        /// The radius of a column user has to fly into at a photo location to set off the proximity trigger in feet.
        /// </summary>
        internal static double PhotoTourConstraintsHotspotRadius { get; set; }

        /// <summary>
        /// Reference integer for the monitor that photo window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        internal static int PhotoTourPhotoMonitorNumber { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of photo window is relative to monitor
        /// </summary>
        internal static int PhotoTourPhotoMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of photo window is relative to monitor
        /// </summary>
        internal static int PhotoTourPhotoMonitorHeight { get; set; }

        /// <summary>
        /// Specifies how close the corner of photo window is relative to the monitor corner. Values between 0 and 20 excepted.
        /// </summary>
        internal static int PhotoTourPhotoOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position photo window relative to or else in the center of monitor.
        /// </summary>
        internal static string PhotoTourPhotoAlignment { get; set; }

        /// <summary>
        /// Reference integer for the monitor that map window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        internal static int PhotoTourMapMonitorNumber { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of map window is relative to monitor
        /// </summary>
        internal static int PhotoTourMapMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of map window is relative to monitor
        /// </summary>
        internal static int PhotoTourMapMonitorHeight { get; set; }

        /// <summary>
        /// Specifies how close the corner of map window is relative to the monitor corner. Values between 0 and 20 excepted.
        /// </summary>
        internal static int PhotoTourMapOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position map window relative to or else in the center of monitor.
        /// </summary>
        internal static string PhotoTourMapAlignment { get; set; }

        /// <summary>
        /// Max number of attempts at creating photo tour.
        /// </summary>
        internal static int PhotoTourMaxSearchAttempts { get; set; }

        #endregion

        # region Sign Writing tab

        /// <summary>
        /// The message to be written, consisting of uppercase and lower case characters plus spaces
        /// </summary>
        internal static string SignMessage { get; private set; }

        /// <summary>
        /// The message can be tilted in the plane of the vertical segments. Gates for vertical segments
        /// have the pitch altered up or down depending on whether gate sequence is gaining or losing
        /// altitude. Horizontal segment gates have neutral pitch.
        /// </summary>
        internal static double SignTiltAngle { get; private set; }

        /// <summary>
        /// Height ASML for the lowest altitude gates.
        /// </summary>
        internal static double SignGateHeight { get; set; }

        /// <summary>
        /// The length of a segment measured in degrees of latitude. Includes straight portion of segment
        /// excludes pointy caps.
        /// </summary>
        internal static double SignSegmentLengthDeg { get; set; }

        /// <summary>
        /// The radius of pointy caps of segment measured in degrees of latitude.
        /// </summary>
        internal static double SignSegmentRadiusDeg { get; set; }

        /// <summary>
        /// Reference integer for the monitor that sign writing window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        internal static int SignMonitorNumber { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of sign writing window is relative to monitor
        /// </summary>
        internal static int SignMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of sign writing window is relative to monitor
        /// </summary>
        internal static int SignMonitorHeight { get; set; }

        /// <summary>
        /// Specifies how close the corner of sign writing window is relative to the monitor corner. Values between 0 and 20 excepted.
        /// </summary>
        internal static int SignOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position sign writing window relative to or else in the center of monitor.
        /// </summary>
        internal static string SignAlignment { get; set; }

        #endregion

        #region Celestial Navigation tab
        internal static string CelestialDestRunway { get; set; }
        internal static double CelestialMinDistance { get; set; }
        internal static double CelestialMaxDistance { get; set; }

        #endregion

        #region Wikipedia

        /// <summary>
        /// The Wikipedia URL for the list or table you want to select a subset of items from. List or table needs a column 
        /// containing a link to the individual items.
        /// </summary>
        internal static string WikiURL { get; set; }

        /// <summary>
        /// The column in Wikipedia URL tables or lists containing the link to each item
        /// </summary>
        internal static int WikiItemLinkColumn { get; set; }

        /// <summary>
        /// Useable tables (or lists) found in the user supplied Wikipedia URL.
        /// </summary>
        internal static string WikiTableNames { get; set; }

        /// <summary>
        /// Proposed visit sequence for items in selected table (list) for the user supplied Wikipedia URL
        /// </summary>
        internal static string WikiRoute { get; set; }

        /// <summary>
        /// Starting item from Visit Sequence for subset to be visited
        /// </summary>
        internal static string WikiStartingItem { get; set; }

        /// <summary>
        /// Finishing item from Visit Sequence for subset to be visited
        /// </summary>
        internal static string WikiFinishingItem { get; set; }

        /// <summary>
        /// Reference integer for the monitor that URL window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        internal static int WikiURLMonitorNumber { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of URL window is relative to monitor
        /// </summary>
        internal static int WikiURLMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of URL window is relative to monitor
        /// </summary>
        internal static int WikiURLMonitorHeight { get; set; }

        /// <summary>
        /// Specifies how close the corner of URL window is relative to the monitor corner. Values between 0 and 20 excepted.
        /// </summary>
        internal static int WikiURLOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position URL window relative to or else in the center of monitor.
        /// </summary>
        internal static string WikiURLAlignment { get; set; }

        /// <summary>
        /// The width of URL window in pixels
        /// </summary>
        internal static int WikiURLWindowWidth { get; set; }

        /// <summary>
        /// The height of URL window in pixels
        /// </summary>
        internal static int WikiURLWindowHeight { get; set; }

        /// <summary>
        /// Reference integer for the monitor that map window is to be displayed in initially. Values from 0 to the number of 
        /// monitors minus 1 expected.
        /// </summary>
        internal static int WikiMapMonitorNumber { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of map window is relative to monitor
        /// </summary>
        internal static int WikiMapMonitorWidth { get; set; }

        /// <summary>
        /// In pixels, used to aid in calculating where top left hand corner of map window is relative to monitor
        /// </summary>
        internal static int WikiMapMonitorHeight { get; set; }

        /// <summary>
        /// Specifies how close the corner of map window is relative to the monitor corner. Values between 0 and 20 excepted.
        /// </summary>
        internal static int WikiMapOffset { get; set; }

        /// <summary>
        /// Which of four corners of monitor to position map window relative to or else in the center of monitor.
        /// </summary>
        internal static string WikiMapAlignment { get; set; }

        /// <summary>
        /// User can select a map window size of either 512 or 1024 pixels square.
        /// </summary>
        internal static int WikiMapWindowSize { get; set; }

        #endregion

        #region Settings tab
        internal static string SettingsCacheServerURL { get; set; }
        internal static string SettingsCacheServerAPIkey { get; set; }
        internal static string SettingsCacheUsage { get; set; }
        internal static int SettingsCacheDailyTotal { get; set; }
        internal static string SettingsScenarioFolder { get; set; }
        internal static string SettingsSimulatorVersion { get; set; }
        internal static string SettingsP3DprogramData { get; set; }
        internal static string SettingsImageFolder { get; set; }

        #endregion

        #region Common

        internal static int CommonMovingMapWindowSize { get; private set; }

        #endregion

        /// <summary>
        /// Copies form fields for chosen scenario into Parameter class fields for ease of access, 
        /// some error checking, and creates scenario and images directories
        /// </summary>
        /// <returns>True if parameters okay and directories created</returns>
        internal static bool SetParams(ScenarioFormData formData)
        {
            // General tab
            string errorMsg = "";
            if (!ValidateScenarioTitle())
            {
                return false;
            }
            if (Form.form.ComboBoxGeneralAircraftSelection.Items.Count == 0)
            {
                errorMsg += "\n\tSelect an aircraft";
            }
            else
            {
                AircraftVariant aircraftVariant = Aircraft.AircraftVariants[Aircraft.CurrentAircraftVariantIndex];
                AircraftTitle = aircraftVariant.Title;
                AircraftCruiseSpeed = Convert.ToDouble(aircraftVariant.CruiseSpeed);
                AircraftImagePath = aircraftVariant.ThumbnailImagePath;
            }
            SelectedRunwayIndex = Form.form.ComboBoxGeneralRunwaySelected.SelectedIndex;
            SelectedScenarioType = formData.SelectedScenarioType;
            DayOfYear = Form.form.GeneralDatePicker.Value.DayOfYear;
            Day = Form.form.GeneralDatePicker.Value.Day;
            Month = Form.form.GeneralDatePicker.Value.Month;
            Year = Form.form.GeneralDatePicker.Value.Year;
            var persianMonth = new PersianCalendar().GetMonth(Form.form.GeneralDatePicker.Value);
            Season = (Season)Math.Ceiling(persianMonth / 3.0);
            Hours = Form.form.GeneralTimePicker.Value.Hour;
            Minutes = Form.form.GeneralTimePicker.Value.Minute;
            Seconds = Form.form.GeneralTimePicker.Value.Second;
            GeneralScenarioTitle = Form.form.TextBoxGeneralScenarioTitle.Text;

            // Circuit tab
            UpwindLeg = Convert.ToDouble(Form.form.TextBoxCircuitUpwind.Text);
            BaseLeg = Convert.ToDouble(Form.form.TextBoxCircuitBase.Text);
            FinalLeg = Convert.ToDouble(Form.form.TextBoxCircuitFinal.Text);
            HeightUpwind = Convert.ToDouble(Form.form.TextBoxCircuitHeightUpwind.Text);
            HeightDown = Convert.ToDouble(Form.form.TextBoxCircuitHeightDown.Text);
            HeightBase = Convert.ToDouble(Form.form.TextBoxCircuitHeightBase.Text);
            Speed = Convert.ToDouble(Form.form.TextBoxCircuitSpeed.Text);
            TurnRate = Convert.ToDouble(Form.form.TextBoxCircuitTurnRate.Text);

            // Photo Tour
            PhotoTourConstraintsMaxLegDist = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsMaxLegDist.Text);
            PhotoTourConstraintsMinLegDist = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsMinLegDist.Text);
            PhotoTourConstraintsMinNoLegs = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsMinNoLegs.Text);
            PhotoTourConstraintsMaxNoLegs = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsMaxNoLegs.Text);
            PhotoTourConstraintsMaxBearingChange = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsMaxBearingChange.Text);
            PhotoTourConstraintsHotspotRadius = Convert.ToDouble(Form.form.TextBoxPhotoTourConstraintsHotspotRadius.Text) * 0.3084; // Convert feet to metres
            PhotoTourPhotoMonitorNumber = Convert.ToInt32(Form.form.TextBoxPhotoTourPhotoMonitorNumber.Text);
            PhotoTourPhotoMonitorWidth = Convert.ToInt32(Form.form.TextBoxPhotoTourPhotoMonitorWidth.Text);
            PhotoTourPhotoMonitorHeight = Convert.ToInt32(Form.form.TextBoxPhotoTourPhotoMonitorHeight.Text);
            PhotoTourPhotoOffset = Convert.ToInt32(Form.form.TextBoxPhotoTourPhotoOffset.Text);
            PhotoTourPhotoAlignment = Form.form.ComboBoxPhotoTourPhotoAlignment.GetItemText(Form.form.ComboBoxPhotoTourPhotoAlignment.SelectedItem);
            PhotoTourMapMonitorNumber = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorNumber.Text);
            PhotoTourMapMonitorWidth = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorWidth.Text);
            PhotoTourMapMonitorHeight = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorHeight.Text);
            PhotoTourMapOffset = Convert.ToInt32(Form.form.TextBoxSettingsMapOffset.Text);
            PhotoTourMapAlignment = Form.form.ComboBoxSettingsMapAlignment.GetItemText(Form.form.ComboBoxSettingsMapAlignment.SelectedItem);
            PhotoTourMapWindowSize = Convert.ToInt32(Form.form.ComboBoxSettingsMapWindowSize.Text);
            PhotoTourMaxSearchAttempts = Convert.ToInt32(Form.form.TextBoxPhotoTourConstraintsMaxAttempts.Text); 

            // Sign Writing
            SignMessage = Form.form.ComboBoxSignMessage.GetItemText(Form.form.ComboBoxSignMessage.SelectedItem);
            SignTiltAngle = Convert.ToDouble(Form.form.TextBoxSignTilt.Text);
            SignGateHeight = Convert.ToDouble(Form.form.TextBoxSignGateHeight.Text);
            SignSegmentLengthDeg = Convert.ToDouble(Form.form.TextBoxSignSegmentLength.Text) / Constants.degreeLatFeet;
            SignSegmentRadiusDeg = Convert.ToDouble(Form.form.TextBoxSignSegmentRadius.Text) / Constants.degreeLatFeet;
            SignMonitorNumber = Convert.ToInt32(Form.form.TextBoxSignMonitorNumber.Text);
            SignMonitorWidth = Convert.ToInt32(Form.form.TextBoxSignMonitorWidth.Text);
            SignMonitorHeight = Convert.ToInt32(Form.form.TextBoxSignMonitorHeight.Text);
            SignOffset = Convert.ToInt32(Form.form.TextBoxSignOffset.Text);
            SignAlignment = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxSignAlignment.SelectedItem);

            // CelestialScenario Navigation
            CelestialMinDistance = Convert.ToDouble(Form.form.TextBoxCelestialMinDist.Text);
            CelestialMaxDistance = Convert.ToDouble(Form.form.TextBoxCelestialMaxDist.Text);

            // Wikipedia List
            if (SelectedScenarioType == ScenarioTypes.WikiList)
            {
                if (Form.form.TextBoxWikiDistance.Text == "")
                {
                    errorMsg += "\n\tSelect a list of Wikipedia items first";
                };
            }
            WikiURL = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxWikiURL.SelectedItem);
            WikiItemLinkColumn = Convert.ToInt32(Form.form.TextBoxWikiItemLinkColumn.Text);
            WikiTableNames = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxWikiTableNames.SelectedItem);
            WikiRoute = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxWikiRoute.SelectedItem);
            WikiStartingItem = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxWikiStartingItem.SelectedItem);
            WikiFinishingItem = Form.form.ComboBoxSignAlignment.GetItemText(Form.form.ComboBoxWikiFinishingItem.SelectedItem);
            WikiURLMonitorNumber = Convert.ToInt32(Form.form.TextBoxWikiURLMonitorNumber.Text);
            WikiURLMonitorWidth = Convert.ToInt32(Form.form.TextBoxWikiURLMonitorWidth.Text);
            WikiURLMonitorHeight = Convert.ToInt32(Form.form.TextBoxWikiURLMonitorHeight.Text);
            WikiURLOffset = Convert.ToInt32(Form.form.TextBoxWikiURLOffset.Text);
            WikiURLAlignment = Form.form.ComboBoxWikiURLAlignment.GetItemText(Form.form.ComboBoxWikiURLAlignment.SelectedItem);
            WikiURLWindowWidth = Convert.ToInt32(Form.form.TextBoxWikiURLWindowWidth.Text);
            WikiURLWindowHeight = Convert.ToInt32(Form.form.TextBoxWikiURLWindowHeight.Text);
            WikiMapMonitorNumber = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorNumber.Text);
            WikiMapMonitorWidth = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorWidth.Text);
            WikiMapMonitorHeight = Convert.ToInt32(Form.form.TextBoxSettingsMapMonitorHeight.Text);
            WikiMapOffset = Convert.ToInt32(Form.form.TextBoxSettingsMapOffset.Text);
            WikiMapAlignment = Form.form.ComboBoxSettingsMapAlignment.GetItemText(Form.form.ComboBoxSettingsMapAlignment.SelectedItem);
            WikiMapWindowSize = Convert.ToInt32(Form.form.ComboBoxSettingsMapWindowSize.Text);

            // Settings
            SettingsScenarioFolder = $"{Form.form.ComboBoxSettingsScenarioFolder.Text}\\{Form.form.TextBoxGeneralScenarioTitle.Text}";
            SettingsImageFolder = $"{SettingsScenarioFolder}\\images";
            SettingsCacheUsage = Form.form.TextBoxSettingsCacheUsage.Text;
            SettingsCacheDailyTotal = Convert.ToInt32(Form.form.TextBoxSettingsCacheDailyTotal.Text);
            SettingsSimulatorVersion = Form.form.ComboBoxSettingsSimulatorVersion.GetItemText(Form.form.ComboBoxSettingsSimulatorVersion.SelectedItem);
            SettingsP3DprogramData = Form.form.TextBoxSettingsP3DprogramData.Text;
            SettingsCacheServerAPIkey = Form.form.TextBoxSettingsOSMServerAPIkey.Text;
            if (!HttpRoutines.ValidateMapTileServerKey())
            {
                return false;
            }

            // Common
            if (SelectedScenarioType == ScenarioTypes.PhotoTour)
            {
                CommonMovingMapWindowSize = PhotoTourMapWindowSize;
            }
            else if (SelectedScenarioType == ScenarioTypes.WikiList)
            {
                CommonMovingMapWindowSize = WikiMapWindowSize;
            }

            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                Directory.CreateDirectory(SettingsScenarioFolder);
                Directory.CreateDirectory(SettingsImageFolder);
                return true;
            }
        }

        /// <summary>
        /// Checks that the user supplied scenario title will make a valid filename and that a folder of that name doesn't
        /// already exist.
        /// </summary>
        /// <returns>True for valid scenario title.</returns>
        internal static bool ValidateScenarioTitle()
        {
            string saveFolder;

            if (IsValidFilename(Form.form.TextBoxGeneralScenarioTitle.Text))
            {
                saveFolder = $"{Form.form.ComboBoxSettingsScenarioFolder.Text}\\{Form.form.TextBoxGeneralScenarioTitle.Text}";
                if (Directory.Exists(saveFolder))
                {
                    string message = $"A scenario with the same title already exists. Either delete the folder \"{saveFolder}\" (you'll need to shut down Prepar3D first if it's running) or choose a different scenario title.";
                    MessageBox.Show(message, Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"Invalid scenario title", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        internal static bool IsValidFilename(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
