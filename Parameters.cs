using System;
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
        internal static double Height { get; private set; }
        internal static double Speed { get; private set; }
        internal static double UpwindLeg { get; private set; }

        static internal Boolean SetParams()
        {
            // General tab
            string errorMsg = "";
            SaveLocation = form.textBoxSaveLocation.Text;
            if (SaveLocation == "")
            {
                errorMsg += "\n\tSelect a save location";
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
            Height = Convert.ToDouble(form.TextBoxCircuitHeight.Text);
            Speed = Convert.ToDouble(form.TextBoxCircuitSpeed.Text);
            UpwindLeg = Convert.ToDouble(form.TextBoxCircuitUpwind.Text);

            if (errorMsg != "")
            {
                MessageBox.Show($"Please attend to the following:\n{errorMsg}", Constants.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
