using System.Globalization;
using static P3D_Scenario_Generator.Runway;

namespace P3D_Scenario_Generator
{
    internal class Parameters
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        // General tab
        internal static string SelectedRunway { get; set; }
        internal static string SaveLocation { get; private set; }
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
        internal static double UpwindLeg { get; private set; }
        internal static double BaseLeg { get; private set; }
        internal static double FinalLeg { get; private set; }
        internal static double HeightUpwind { get; private set; }
        internal static double HeightDown { get; private set; }
        internal static double HeightBase { get; private set; }
        internal static double Speed { get; private set; }
        internal static double TurnRate { get; private set; }

        // Photo Tour
        internal static double MaxLegDist { get; private set; }
        internal static double MinLegDist { get; private set; }
        internal static double MinNoLegs { get; private set; }
        internal static double MaxNoLegs { get; private set; }
        internal static string PhotoDestRunway { get; set; }
        internal static double PhotoLegWindowSize { get; set; }
        internal static double MaxBearingChange { get; set; }
        internal static double HotspotRadius { get; set; }

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
            SelectedRunway = form.TextBoxSelectedRunway.Text;
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
            if (SelectedScenario == nameof(ScenarioTypes.Circuit))
            {
                UpwindLeg = Convert.ToDouble(form.TextBoxCircuitUpwind.Text);
                BaseLeg = Convert.ToDouble(form.TextBoxCircuitBase.Text);
                FinalLeg = Convert.ToDouble(form.TextBoxCircuitFinal.Text);
                HeightUpwind = Convert.ToDouble(form.TextBoxCircuitHeightUpwind.Text);
                HeightDown = Convert.ToDouble(form.TextBoxCircuitHeightDown.Text);
                HeightBase = Convert.ToDouble(form.TextBoxCircuitHeightBase.Text);
                Speed = Convert.ToDouble(form.TextBoxCircuitSpeed.Text);
                TurnRate = Convert.ToDouble(form.TextBoxCircuitTurnRate.Text);
            }

            // Photo Tour
            if (SelectedScenario == nameof(ScenarioTypes.PhotoTour))
            {
                MaxLegDist = Convert.ToDouble(form.TextBoxPhotoMaxLegDist.Text);
                MinLegDist = Convert.ToDouble(form.TextBoxPhotoMinLegDist.Text);
                MinNoLegs = Convert.ToDouble(form.TextBoxPhotoMinNoLegs.Text);
                MaxNoLegs = Convert.ToDouble(form.TextBoxPhotoMaxNoLegs.Text);
                PhotoLegWindowSize = Convert.ToDouble(form.TextBoxPhotoWindowSize.Text);
                MaxBearingChange = Convert.ToDouble(form.TextBoxPhotoMaxBearingChange.Text);
                HotspotRadius = Convert.ToDouble(form.TextBoxPhotoHotspotRadius.Text) * 0.3084; // Convert feet to metres
            }

            // Sign Writing
            if (SelectedScenario == nameof(ScenarioTypes.SignWriting))
            {
                Message = form.TextBoxSignMessage.Text;
                TiltAngle = Convert.ToDouble(form.TextBoxSignTilt.Text);
                MessageWindowWidth = Convert.ToDouble(form.TextBoxSignWindowWidth.Text);
                GateHeight = Convert.ToDouble(form.TextBoxSignGateHeight.Text);
                SegmentLengthDeg = Convert.ToDouble(form.TextBoxSignSegmentLength.Text) / Con.degreeLatFeet;
                SegmentRadiusDeg = Convert.ToDouble(form.TextBoxSignSegmentRadius.Text) / Con.degreeLatFeet;
            }

            // Celestial Navigation
            if (SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                CelestialMinDistance = Convert.ToDouble(form.TextBoxCelestialMinDist.Text);
                CelestialMaxDistance = Convert.ToDouble(form.TextBoxCelestialMaxDist.Text);
            }

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
                string saveFolder = $"{form.TextBoxP3Dv5Files.Text}\\{form.TextBoxScenarioTitle.Text}";
                SaveLocation = saveFolder + $"\\{form.TextBoxScenarioTitle.Text}.fxml";
                Directory.CreateDirectory(saveFolder);
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
