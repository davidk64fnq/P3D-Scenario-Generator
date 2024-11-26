using System.Globalization;

namespace P3D_Scenario_Generator
{
    internal class Parameters
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        // General tab
        internal static string SelectedAirportICAO { get; set; }
        internal static string SelectedAirportID { get; set; }
        internal static string ScenarioTitle { get; private set; }
        internal static string ScenarioFolder { get; private set; }
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

        // Circuit tab

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

        // Photo Tour
        internal static double MaxLegDist { get; private set; }
        internal static double MinLegDist { get; private set; }
        internal static double MinNoLegs { get; private set; }
        internal static double MaxNoLegs { get; private set; }
        internal static double PhotoLegWindowSize { get; set; }
        internal static double MaxBearingChange { get; set; }
        internal static double HotspotRadius { get; set; }
        internal static string PhotoLocation { get; set; }
        internal static int PhotoTourPhotoMonitorNumber { get; set; }
        internal static int PhotoTourPhotoMonitorWidth { get; set; }
        internal static int PhotoTourPhotoMonitorHeight { get; set; }
        internal static int PhotoTourPhotoHorizontalOffset { get; set; }
        internal static int PhotoTourPhotoVerticalOffset { get; set; }
        internal static int PhotoTourMapMonitorNumber { get; set; }
        internal static int PhotoTourMapMonitorWidth { get; set; }
        internal static int PhotoTourMapMonitorHeight { get; set; }
        internal static int PhotoTourMapHorizontalOffset { get; set; }
        internal static int PhotoTourMapVerticalOffset { get; set; }

        // Sign Writing
        internal static string Message { get; private set; }
        internal static double TiltAngle { get; private set; }
        internal static double MessageWindowWidth { get; set; }
        internal static double GateHeight { get; set; }
        internal static double SegmentLengthDeg { get; set; }
        internal static double SegmentRadiusDeg { get; set; }

        // Celestial Navigation
        internal static string CelestialDestRunway { get; set; }
        internal static double CelestialMinDistance { get; set; }
        internal static double CelestialMaxDistance { get; set; }
        internal static double CelestialImageNorth { get; set; }
        internal static double CelestialImageEast { get; set; }
        internal static double CelestialImageSouth { get; set; }
        internal static double CelestialImageWest { get; set; }

        static private bool IsValidFilename(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

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
            MaxLegDist = Convert.ToDouble(form.TextBoxPhotoMaxLegDist.Text);
            MinLegDist = Convert.ToDouble(form.TextBoxPhotoMinLegDist.Text);
            MinNoLegs = Convert.ToDouble(form.TextBoxPhotoMinNoLegs.Text);
            MaxNoLegs = Convert.ToDouble(form.TextBoxPhotoMaxNoLegs.Text);
            PhotoLegWindowSize = Convert.ToDouble(form.TextBoxPhotoWindowSize.Text);
            MaxBearingChange = Convert.ToDouble(form.TextBoxPhotoMaxBearingChange.Text);
            HotspotRadius = Convert.ToDouble(form.TextBoxPhotoHotspotRadius.Text) * 0.3084; // Convert feet to metres
            PhotoLocation = form.TextBoxPhotoLocation.Text;
            PhotoTourPhotoMonitorNumber = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorNumber.Text);
            PhotoTourPhotoMonitorWidth = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorWidth.Text);
            PhotoTourPhotoMonitorHeight = Convert.ToInt32(form.TextBoxPhotoTourPhotoMonitorHeight.Text);
            PhotoTourPhotoHorizontalOffset = Convert.ToInt32(form.TextBoxPhotoTourPhotoHorizontalOffset.Text);
            PhotoTourPhotoVerticalOffset = Convert.ToInt32(form.TextBoxPhotoTourPhotoVerticalOffset.Text);
            PhotoTourMapMonitorNumber = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorNumber.Text);
            PhotoTourMapMonitorWidth = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorWidth.Text);
            PhotoTourMapMonitorHeight = Convert.ToInt32(form.TextBoxPhotoTourMapMonitorHeight.Text);
            PhotoTourMapHorizontalOffset = Convert.ToInt32(form.TextBoxPhotoTourMapHorizontalOffset.Text);
            PhotoTourMapVerticalOffset = Convert.ToInt32(form.TextBoxPhotoTourMapVerticalOffset.Text);

            // Sign Writing
            Message = form.TextBoxSignMessage.Text;
            TiltAngle = Convert.ToDouble(form.TextBoxSignTilt.Text);
            MessageWindowWidth = Convert.ToDouble(form.TextBoxSignWindowWidth.Text);
            GateHeight = Convert.ToDouble(form.TextBoxSignGateHeight.Text);
            SegmentLengthDeg = Convert.ToDouble(form.TextBoxSignSegmentLength.Text) / Con.degreeLatFeet;
            SegmentRadiusDeg = Convert.ToDouble(form.TextBoxSignSegmentRadius.Text) / Con.degreeLatFeet;

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

            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                ScenarioTitle = form.TextBoxScenarioTitle.Text;
                ScenarioFolder = $"{form.TextBoxP3Dv5Files.Text}\\{form.TextBoxScenarioTitle.Text}";
                Directory.CreateDirectory(ScenarioFolder);
                ImageFolder = $"{ScenarioFolder}\\images";
                Directory.CreateDirectory(ImageFolder);
                return true;
            }

        }

        static private bool ValidateScenarioTitle()
        {
            string saveFolder;
            if (IsValidFilename(form.TextBoxScenarioTitle.Text))
            {
                saveFolder = $"{form.TextBoxP3Dv5Files.Text}\\{form.TextBoxScenarioTitle.Text}";
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
    }
}
