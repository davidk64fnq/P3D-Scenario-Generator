using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            string message = $"Creating scenario files in \"{Parameters.ImageFolder}\" - will confirm when complete";
            MessageBox.Show(message, Con.appTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoScenarioSpecificTasks()
        {
            if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.Circuit])
            {
                Circuit.SetCircuit();
                SaveUserSettings(TabPageCircuit.Controls);
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.PhotoTour])
            {
                PhotoTour.SetPhotoTour();
                SaveUserSettings(TabPagePhotoTour.Controls);
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.SignWriting])
            {
                SignWriting.SetSignWriting();
                SaveUserSettings(TabPageSign.Controls);
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
                SaveUserSettings(TabPageWikiList.Controls);
            }
            else if (TextBoxSelectedScenario.Text == Con.scenarioNames[(int)ScenarioTypes.WikiList])
            {
                Wikipedia.SetWikiTour(ComboBoxWikiTableNames.SelectedIndex, ComboBoxWikiRoute.Items, ComboBoxWikiStartingItem.SelectedItem,
                    ComboBoxWikiFinishingItem.SelectedItem, TextBoxWikiDistance.Text);
                SaveUserSettings(TabPageWikiList.Controls);
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

        #endregion

        #region Aircraft selection

        private void ButtonAircraft_Click(object sender, EventArgs e)
        {
            List<string> uiVariations = Aircraft.GetUIvariations();
            if (uiVariations.Count > 0)
            {
                Properties.Settings.Default.CruiseSpeed = Aircraft.CruiseSpeed;
                Properties.Settings.Default.Save();
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

        #endregion

        #region Photo Tour Tab

        #endregion

        #region Sign Writing Tab

        #endregion

        #region Celestial Navigation Tab

        #endregion

        #region Wikipedia Lists Tab

        private void ComboBoxWikiURL_TextChanged(object sender, EventArgs e)
        {
            ComboBoxWikiURL_TextChanged();
            ComboBox_SelectedIndexChanged(sender, e);
        }

        private void ComboBoxWikiURL_TextChanged()
        {
            if (ComboBoxWikiURL.SelectedItem != null)
            {
                Parameters.SelectedScenario = Con.scenarioNames[(int)ScenarioTypes.WikiList];
                Wikipedia.PopulateWikiPage(ComboBoxWikiURL.SelectedItem.ToString(), int.Parse(TextBoxWikiItemLinkColumn.Text));
                ComboBoxWikiTableNames.DataSource = Wikipedia.CreateWikiTablesDesc();
            }
        }

        private void ComboBoxWikiTableNames_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            ListBoxRunways.DataSource = Runway.GetICAOids();

            // Circuit tab
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string appPath = Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_");
            Stream stream = Assembly.Load(appName).GetManifestResourceStream($"{appPath}.Resources.Images.circuitTab.jpg");
            PictureBoxCircuit.Image = new Bitmap(stream);
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
                            if (control is TextBox && control.Tag != null)
                                control.Text = settingsValue.ToString();
                            else if (control is ComboBox box && control.Tag != null)
                            {
                                box.Items.Clear();
                                foreach (string item in (System.Collections.Specialized.StringCollection)Properties.Settings.Default[control.Name])
                                {
                                    box.Items.Add(item);
                                }
                                box.SelectedIndex = (int)Properties.Settings.Default[control.Name + "SelectedIndex"];
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

        private void Init(object sender, EventArgs e)
        {
            string[] newUIvariation = [Properties.Settings.Default.SelectedAircraft, Properties.Settings.Default.AircraftImage];
            Aircraft.uiVariations.Add(newUIvariation);
            List<string> aircraftList = [Properties.Settings.Default.SelectedAircraft];
            ListBoxAircraft.DataSource = aircraftList;
            Aircraft.CruiseSpeed = Properties.Settings.Default.CruiseSpeed;
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

        #endregion
    }
}
