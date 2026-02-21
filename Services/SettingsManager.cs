using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the persistence of UI control states to a JSON file in the AppData Roaming folder.
    /// Provides safe-save functionality with backups and the ability to revert to designer defaults.
    /// </summary>
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        private readonly string _backupFilePath;
        private Dictionary<string, object> _settingsCache;
        private readonly Dictionary<string, object> _designerDefaults;
        private readonly Logger _logger;

        public SettingsManager(Logger logger)
        {
            _logger = logger;
            _designerDefaults = [];

            // Matches the path logic in your Logger class
            string appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            _settingsFilePath = Path.Combine(folder, "ui_settings.json");
            _backupFilePath = Path.Combine(folder, "ui_settings.json.bak");

            _settingsCache = [];
            LoadSettingsFromFile();
        }

        #region Capture & Reset Defaults

        /// <summary>
        /// Captures the hardcoded defaults set in the Visual Studio Designer for a single control and its children.
        /// </summary>
        public void CaptureDefaults(Control control)
        {
            if (control == null) return;

            if (control.Controls.Count > 0) CaptureDefaults(control.Controls);

            if (control is TextBox textBox)
            {
                _designerDefaults[control.Name] = textBox.Text;
            }
            else if (control is ComboBox comboBox)
            {
                _designerDefaults[control.Name + "SelectedIndex"] = comboBox.SelectedIndex;
            }
            else if (control is CheckBox checkBox)
            {
                _designerDefaults[control.Name] = checkBox.Checked;
            }
        }

        public void CaptureDefaults(Control.ControlCollection controls)
        {
            foreach (Control control in controls) CaptureDefaults(control);
        }

        /// <summary>
        /// Logic for the "Reset to Defaults" button. 
        /// Identifies the active tab and resets relevant controls based on your specific requirements.
        /// </summary>
        public void RestoreActiveTab(TabControl tabControl, Control layoutWiki, Control layoutMap)
        {
            if (tabControl?.SelectedTab == null) return;

            string tabName = tabControl.SelectedTab.Name;

            if (tabName == "TabPageSettings")
            {
                if (layoutMap != null) RestoreDefaults(layoutMap.Controls);
            }
            else if (tabName == "TabPageWikiList ")
            {
                if (layoutWiki != null) RestoreDefaults(layoutWiki.Controls);
            }
            else if (tabName == "TabPagePhotoTour" ||
                     tabName == "TabPageSign" ||
                     tabName == "TabPageCelestial")
            {
                RestoreDefaults(tabControl.SelectedTab.Controls);
            }
        }

        /// <summary>
        /// Reverts a specific control (and its children) to Designer defaults.
        /// </summary>
        public void RestoreDefaults(Control control)
        {
            if (control == null) return;

            if (control.Controls.Count > 0) RestoreDefaults(control.Controls);

            if (control is TextBox textBox && _designerDefaults.TryGetValue(control.Name, out var text))
            {
                textBox.Text = text?.ToString() ?? "";
            }
            else if (control is CheckBox checkBox && _designerDefaults.TryGetValue(control.Name, out var check))
            {
                checkBox.Checked = check is bool b && b;
            }
            else if (control is ComboBox comboBox && _designerDefaults.TryGetValue(control.Name + "SelectedIndex", out var index))
            {
                comboBox.SelectedIndex = index is int i ? i : -1;
            }
        }

        public void RestoreDefaults(Control.ControlCollection controls)
        {
            foreach (Control control in controls) RestoreDefaults(control);
        }

        #endregion

        #region Load & Save Persistence

        private void LoadSettingsFromFile()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        TryRestoreFromBackup();
                        return;
                    }
                    _settingsCache = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? [];
                }
                else
                {
                    TryRestoreFromBackup();
                }
            }
            catch
            {
                TryRestoreFromBackup();
            }
        }

        private void TryRestoreFromBackup()
        {
            if (File.Exists(_backupFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_backupFilePath);
                    _settingsCache = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? [];
                }
                catch
                {
                    _settingsCache = [];
                }
            }
            else
            {
                _settingsCache = [];
            }
        }

        /// <summary>
        /// Saves the state of a single UI control (and its children) to the JSON file.
        /// </summary>
        public async Task SaveSettingsAsync(Control control)
        {
            UpdateCacheFromControls(control);
            await CommitCacheToFileAsync();
        }

        /// <summary>
        /// Saves the current state of UI controls to the JSON file using a safe temp-write-and-replace pattern.
        /// </summary>
        public async Task SaveSettingsAsync(Control.ControlCollection controls)
        {
            UpdateCacheFromControls(controls);
            await CommitCacheToFileAsync();
        }

        private async Task CommitCacheToFileAsync()
        {
            string tempPath = _settingsFilePath + ".tmp";
            try
            {
                string json = JsonConvert.SerializeObject(_settingsCache, Formatting.Indented);

                await File.WriteAllTextAsync(tempPath, json);

                if (File.Exists(_settingsFilePath))
                {
                    if (File.Exists(_backupFilePath)) File.Delete(_backupFilePath);
                    File.Move(_settingsFilePath, _backupFilePath);
                }

                File.Move(tempPath, _settingsFilePath);

                await _logger.InfoAsync("UI Settings saved safely to JSON (with backup rotation).");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to save JSON settings: {ex.Message}", ex);
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        #endregion

        #region Restore Logic

        /// <summary>
        /// Populates a single UI control (and its children) with values stored in the JSON cache.
        /// </summary>
        public void RestoreSettings(Control control)
        {
            if (control == null) return;

            if (control.Controls.Count > 0) RestoreSettings(control.Controls);

            // CA1854: Prefer 'TryGetValue' over 'ContainsKey' followed by the indexer
            if (!_settingsCache.TryGetValue(control.Name, out object value)) return;

            try
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? "";
                }
                else if (control is CheckBox checkBox && value is bool boolVal)
                {
                    checkBox.Checked = boolVal;
                }
                else if (control is ComboBox comboBox)
                {
                    if (value is JArray jArray)
                    {
                        comboBox.Items.Clear();
                        foreach (var item in jArray) comboBox.Items.Add(item.ToString());
                    }

                    string indexKey = control.Name + "SelectedIndex";
                    if (_settingsCache.TryGetValue(indexKey, out object indexValue))
                    {
                        int idx = Convert.ToInt32(indexValue);
                        if (idx >= -1 && idx < comboBox.Items.Count)
                        {
                            comboBox.SelectedIndex = idx;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring {control.Name}: {ex.Message}");
            }
        }

        public void RestoreSettings(Control.ControlCollection controls)
        {
            foreach (Control control in controls) RestoreSettings(control);
        }

        #endregion

        #region Update Logic

        private void UpdateCacheFromControls(Control control)
        {
            if (control == null) return;

            if (control.Controls.Count > 0) UpdateCacheFromControls(control.Controls);

            if (control is TextBox textBox)
            {
                _settingsCache[control.Name] = textBox.Text;
            }
            else if (control is ComboBox comboBox)
            {
                var items = new List<string>();
                foreach (var item in comboBox.Items) items.Add(item.ToString());
                _settingsCache[control.Name] = items;
                _settingsCache[control.Name + "SelectedIndex"] = comboBox.SelectedIndex;
            }
            else if (control is CheckBox checkBox)
            {
                _settingsCache[control.Name] = checkBox.Checked;
            }
        }

        private void UpdateCacheFromControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls) UpdateCacheFromControls(control);
        }

        #endregion
    }
}