using System.Globalization;

namespace P3D_Scenario_Generator
{
    internal class Parameters
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        #region General tab

        internal static string SelectedAirportICAO { get; set; }
        internal static string SelectedAirportID { get; set; }
        internal static string ImageFolder { get; private set; }
        internal static string SelectedAircraft { get; private set; }
        internal static string SelectedScenario { get; private set; }
        internal static int Second { get; private set; }
        internal static int Minute { get; private set; }
        internal static int Hour { get; private set; }
        internal static int DayOfYear { get; private set; }
        internal static int Day { get; private set; }
        internal static int Month { get; private set; }
        internal static int Year { get; private set; }
        internal static Season Season { get; private set; }
        internal static int Hours { get; private set; }
        internal static int Minutes { get; private set; }
        internal static string GeneralScenarioTitle { get; private set; }

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
        internal static double CelestialImageNorth { get; set; }
        internal static double CelestialImageEast { get; set; }
        internal static double CelestialImageSouth { get; set; }
        internal static double CelestialImageWest { get; set; }

        #endregion

        #region Wikipedia

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
        internal static string SettingsScenarioFolder { get; private set; }

        #endregion

        #region Common

        internal static int CommonMovingMapWindowSize { get; private set; }

        #endregion

        /// <summary>
        /// Copies form fields for chosen scenario into Parameter class fields for ease of access, 
        /// some error checking, and creates scenario and images directories
        /// </summary>
        /// <returns>True if parameters okay and directories created</returns>
        static internal bool SetParams()
        {
            // General tab
            string errorMsg = "";
            if (!ValidateScenarioTitle())
            {
                return false;
            }
            if (form.ListBoxAircraft.Items.Count == 0)
            {
                errorMsg += "\n\tSelect an aircraft";
            }
            else
            {
                SelectedAircraft = form.ListBoxAircraft.Items[form.ListBoxAircraft.SelectedIndex].ToString();
            }
            SelectedAirportICAO = form.TextBoxSelectedRunway.Text.Split("\t")[0];
            if (form.TextBoxSelectedRunway.Text != "")
            {
                SelectedAirportID = form.TextBoxSelectedRunway.Text.Split("\t")[1][1..^1]; // Strip '(' and ')'
            }
            int index = Array.FindIndex(Con.scenarioNames, s => s == form.TextBoxSelectedScenario.Text);
            SelectedScenario = Enum.GetNames(typeof(ScenarioTypes))[index];
            Second = form.DatePicker.Value.Second;
            Minute = form.DatePicker.Value.Minute;
            Hour = form.DatePicker.Value.Hour;
            DayOfYear = form.DatePicker.Value.DayOfYear;
            Day = form.DatePicker.Value.Day;
            Month = form.DatePicker.Value.Month;
            Year = form.DatePicker.Value.Year;
            var persianMonth = new PersianCalendar().GetMonth(form.DatePicker.Value);
            Season = (Season)Math.Ceiling(persianMonth / 3.0);
            Hours = form.TimePicker.Value.Hour;
            Minutes = form.TimePicker.Value.Minute;
            GeneralScenarioTitle = form.TextBoxScenarioTitle.Text;

            // Circuit tab
            UpwindLeg = Convert.ToDouble(form.TextBoxCircuitUpwind.Text);
            BaseLeg = Convert.ToDouble(form.TextBoxCircuitBase.Text);
            FinalLeg = Convert.ToDouble(form.TextBoxCircuitFinal.Text);
            HeightUpwind = Convert.ToDouble(form.TextBoxCircuitHeightUpwind.Text);
            HeightDown = Convert.ToDouble(form.TextBoxCircuitHeightDown.Text);
            HeightBase = Convert.ToDouble(form.TextBoxCircuitHeightBase.Text);
            Speed = Convert.ToDouble(form.TextBoxCircuitSpeed.Text);
            TurnRate = Convert.ToDouble(form.TextBoxCircuitTurnRate.Text);

            // Photo Tour
            PhotoTourConstraintsMaxLegDist = Convert.ToDouble(form.TextBoxPhotoTourConstraintsMaxLegDist.Text);
            PhotoTourConstraintsMinLegDist = Convert.ToDouble(form.TextBoxPhotoTourConstraintsMinLegDist.Text);
            PhotoTourConstraintsMinNoLegs = Convert.ToDouble(form.TextBoxPhotoTourConstraintsMinNoLegs.Text);
            PhotoTourConstraintsMaxNoLegs = Convert.ToDouble(form.TextBoxPhotoTourConstraintsMaxNoLegs.Text);
            PhotoTourConstraintsMaxBearingChange = Convert.ToDouble(form.TextBoxPhotoTourConstraintsMaxBearingChange.Text);
            PhotoTourConstraintsHotspotRadius = Convert.ToDouble(form.TextBoxPhotoTourMapHotspotRadius.Text) * 0.3084; // Convert feet to metres
            PhotoTourPhotoMonitorNumber = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorNumber.Text);
            PhotoTourPhotoMonitorWidth = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorWidth.Text);
            PhotoTourPhotoMonitorHeight = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorHeight.Text);
            PhotoTourPhotoOffset = Convert.ToInt32(form.TextBoxPhotoTourPhotoOffset.Text);
            PhotoTourPhotoAlignment = form.ComboBoxPhotoTourPhotoAlignment.GetItemText(form.ComboBoxPhotoTourPhotoAlignment.SelectedItem);
            PhotoTourMapMonitorNumber = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorNumber.Text);
            PhotoTourMapMonitorWidth = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorWidth.Text);
            PhotoTourMapMonitorHeight = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorHeight.Text);
            PhotoTourMapOffset = Convert.ToInt32(form.TextBoxPhotoTourMapOffset.Text);
            PhotoTourMapAlignment = form.ComboBoxPhotoTourMapAlignment.GetItemText(form.ComboBoxPhotoTourMapAlignment.SelectedItem);
            PhotoTourMapWindowSize = Convert.ToInt32(form.ComboBoxPhotoTourMapWindowSize.Text);

            // Sign Writing
            SignMessage = form.ComboBoxSignMessage.GetItemText(form.ComboBoxSignMessage.SelectedItem);
            SignTiltAngle = Convert.ToDouble(form.TextBoxSignTilt.Text);
            SignGateHeight = Convert.ToDouble(form.TextBoxSignGateHeight.Text);
            SignSegmentLengthDeg = Convert.ToDouble(form.TextBoxSignSegmentLength.Text) / Con.degreeLatFeet;
            SignSegmentRadiusDeg = Convert.ToDouble(form.TextBoxSignSegmentRadius.Text) / Con.degreeLatFeet;
            SignMonitorNumber = Convert.ToInt32(form.TextBoxSignMonitorNumber.Text);
            SignMonitorWidth = Convert.ToInt32(form.TextBoxSignMonitorWidth.Text);
            SignMonitorHeight = Convert.ToInt32(form.TextBoxSignMonitorHeight.Text);
            SignOffset = Convert.ToInt32(form.TextBoxSignOffset.Text);
            SignAlignment = form.ComboBoxSignAlignment.GetItemText(form.ComboBoxSignAlignment.SelectedItem);

            // Celestial Navigation
            CelestialMinDistance = Convert.ToDouble(form.TextBoxCelestialMinDist.Text);
            CelestialMaxDistance = Convert.ToDouble(form.TextBoxCelestialMaxDist.Text);

            // Wikipedia List
            if (SelectedScenario == nameof(ScenarioTypes.WikiList))
            {
                if (form.TextBoxWikiDistance.Text == "")
                {
                    errorMsg += "\n\tSelect a list of Wikipedia items first";
                };
            }
            WikiMapMonitorNumber = Convert.ToInt32(form.TextBoxWikiMapMonitorNumber.Text);
            WikiMapMonitorWidth = Convert.ToInt32(form.TextBoxWikiMapMonitorWidth.Text);
            WikiMapMonitorHeight = Convert.ToInt32(form.TextBoxWikiMapMonitorHeight.Text);
            WikiMapOffset = Convert.ToInt32(form.TextBoxWikiMapOffset.Text);
            WikiMapAlignment = form.ComboBoxWikiMapAlignment.GetItemText(form.ComboBoxWikiMapAlignment.SelectedItem);
            WikiMapWindowSize = Convert.ToInt32(form.ComboBoxWikiMapWindowSize.Text);

            // Settings
            SettingsScenarioFolder = $"{form.ComboBoxSettingsScenarioFolder.Text}\\{form.TextBoxScenarioTitle.Text}";
            ImageFolder = $"{SettingsScenarioFolder}\\images";
            SettingsCacheServerURL = form.ComboBoxSettingsCacheServers.Text.Split(',')[0].Trim();
            if (form.ComboBoxSettingsCacheServers.Text.Split(',').Length > 1)
                SettingsCacheServerAPIkey = form.ComboBoxSettingsCacheServers.Text.Split(',')[1].Trim();
            SettingsCacheUsage = form.TextBoxSettingsCacheUsage.Text;
            SettingsCacheDailyTotal = Convert.ToInt32(form.TextBoxSettingsCacheDailyTotal.Text);

            // Common
            if (SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                CommonMovingMapWindowSize = PhotoTourMapWindowSize;
            }

            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                Directory.CreateDirectory(SettingsScenarioFolder);
                Directory.CreateDirectory(ImageFolder);
                return true;
            }
        }

        /// <summary>
        /// Checks that the user supplied scenario title will make a valid filename and that a folder of that name doesn't
        /// already exist.
        /// </summary>
        /// <returns>True for valid scenario title.</returns>
        static private bool ValidateScenarioTitle()
        {
            string saveFolder;
            if (IsValidFilename(form.TextBoxScenarioTitle.Text))
            {
                saveFolder = $"{form.ComboBoxSettingsScenarioFolder.Text}\\{form.TextBoxScenarioTitle.Text}";
                if (Directory.Exists(saveFolder))
                {
                    string message = $"A scenario with the same title already exists. Either delete the folder \"{saveFolder}\" (you'll need to shut down Prepar3D first if it's running) or choose a different scenario title.";
                    MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"Invalid scenario title", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        static private bool IsValidFilename(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
