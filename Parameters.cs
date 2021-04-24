using System;
using System.IO;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
    internal class Parameters
    {
        private static readonly Form form = (Form)Application.OpenForms[0];

        // General tab
        internal static string SelectedRunway { get; private set; }
        internal static string SaveLocation { get; private set; }
        internal static string SelectedAircraft { get; private set; }
        internal static string SelectedScenario { get; private set; }

        // Circuit tab
        internal static double BaseLeg { get; private set; }
        internal static double FinalLeg { get; private set; }
        internal static double HeightUpwind { get; private set; }
        internal static double HeightDown { get; private set; }
        internal static double HeightBase { get; private set; }
        internal static double Speed { get; private set; }
        internal static double UpwindLeg { get; private set; }

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
            SelectedScenario = form.TextBoxSelectedScenario.Text;

            // Circuit tab
            BaseLeg = Convert.ToDouble(form.TextBoxCircuitBase.Text);
            FinalLeg = Convert.ToDouble(form.TextBoxCircuitFinal.Text);
            HeightUpwind = Convert.ToDouble(form.TextBoxCircuitHeightUpwind.Text);
            HeightDown = Convert.ToDouble(form.TextBoxCircuitHeightDown.Text);
            HeightBase = Convert.ToDouble(form.TextBoxCircuitHeightBase.Text);
            Speed = Convert.ToDouble(form.TextBoxCircuitSpeed.Text);
            UpwindLeg = Convert.ToDouble(form.TextBoxCircuitUpwind.Text);

            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                string saveFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\Prepar3D v5 Files\\{form.TextBoxScenarioTitle.Text}";
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
                saveFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Documents\\Prepar3D v5 Files\\{form.TextBoxScenarioTitle.Text}";
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

        static private bool IsValidFilename(string fileName)
        {
            return !string.IsNullOrEmpty(fileName) &&
              fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }
}
