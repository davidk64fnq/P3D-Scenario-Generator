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
        private Dictionary<string, object?> _settingsCache;
        private readonly Dictionary<string, object?> _designerDefaults;
        private readonly Logger _logger;
        private readonly FileOps _fileOps;

        public SettingsManager(Logger logger, FileOps fileOps)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
            _designerDefaults = [];

            // Use FileOps to retrieve and ensure the AppData folder exists
            string folder = FileOps.GetApplicationDataDirectory();

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
                if (FileOps.FileExists(_settingsFilePath))
                {
                    string json = FileOps.ReadAllText(_settingsFilePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        TryRestoreFromBackup();
                        return;
                    }
                    _settingsCache = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json) ?? [];
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
            if (FileOps.FileExists(_backupFilePath))
            {
                try
                {
                    string json = FileOps.ReadAllText(_backupFilePath);
                    _settingsCache = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json) ?? [];
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

                // 1. Write the new settings to the temporary file
                if (!await _fileOps.TryWriteAllTextAsync(tempPath, json))
                {
                    await _logger.ErrorAsync("Failed to write temporary settings file.");
                    return;
                }

                // 2. Rotate existing settings file to backup if present
                if (FileOps.FileExists(_settingsFilePath))
                {
                    // Clean up old backup with retry
                    await _fileOps.TryDeleteFileAsync(_backupFilePath);

                    // Move current file to backup with retry
                    if (!await _fileOps.TryMoveFileAsync(_settingsFilePath, _backupFilePath))
                    {
                        await _logger.WarningAsync("Could not create backup of settings file, proceeding with save.");
                    }
                }

                // 3. Move the temporary file into place
                if (await _fileOps.TryMoveFileAsync(tempPath, _settingsFilePath))
                {
                    await _logger.InfoAsync("UI Settings saved safely to JSON (with backup rotation).");
                }
                else
                {
                    await _logger.ErrorAsync($"Failed to move temp settings file '{tempPath}' to '{_settingsFilePath}'.");
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to save JSON settings: {ex.Message}", ex);
            }
            finally
            {
                // Ensure temp file is cleaned up if it still remains
                if (FileOps.FileExists(tempPath))
                {
                    await _fileOps.TryDeleteFileAsync(tempPath);
                }
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

            if (!_settingsCache.TryGetValue(control.Name, out object? value)) return;

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
                    if (_settingsCache.TryGetValue(indexKey, out object? indexValue) && indexValue != null)
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
                foreach (var item in comboBox.Items)
                {
                    items.Add(item?.ToString() ?? string.Empty);
                }
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

        #region Per-Aircraft Circuit Settings

        /// <summary>
        /// Saves controls on the Circuit tab under a key scoped to the unique Aircraft Title.
        /// </summary>
        public async Task SaveCircuitSettingsAsync(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls == null) return;

            string prefix = $"Circuit_{aircraftTitle.Trim()}_";
            UpdateCacheWithPrefix(controls, prefix);
            await CommitCacheToFileAsync();
        }

        /// <summary>
        /// Restores Circuit tab controls for a specific aircraft title.
        /// Returns true if saved custom parameters were found; false otherwise.
        /// </summary>
        public bool RestoreCircuitSettings(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls == null) return false;

            string prefix = $"Circuit_{aircraftTitle.Trim()}_";
            bool foundAnyKey = false;

            foreach (Control control in controls)
            {
                if (control.Controls.Count > 0)
                {
                    if (RestoreCircuitSettings(control.Controls, aircraftTitle))
                        foundAnyKey = true;
                }

                string key = prefix + control.Name;
                if (_settingsCache.TryGetValue(key, out object? value) && control is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? "";
                    foundAnyKey = true;
                }
            }

            return foundAnyKey;
        }

        public async Task SaveSignSettingsAsync(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls == null) return;

            string prefix = $"Sign_{aircraftTitle.Trim()}_";
            UpdateCacheWithPrefix(controls, prefix);
            await CommitCacheToFileAsync();
        }

        public bool RestoreSignSettings(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls == null) return false;

            string prefix = $"Sign_{aircraftTitle.Trim()}_";
            bool foundAnyKey = false;

            foreach (Control control in controls)
            {
                if (control.Controls.Count > 0)
                {
                    if (RestoreSignSettings(control.Controls, aircraftTitle))
                        foundAnyKey = true;
                }

                string key = prefix + control.Name;
                if (_settingsCache.TryGetValue(key, out object? value) && control is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? "";
                    foundAnyKey = true;
                }
            }

            return foundAnyKey;
        }

        /// <summary>
        /// Recursively iterates controls to populate the settings cache with prefixed keys.
        /// </summary>
        private void UpdateCacheWithPrefix(Control.ControlCollection controls, string prefix)
        {
            foreach (Control control in controls)
            {
                if (control.Controls.Count > 0)
                {
                    UpdateCacheWithPrefix(control.Controls, prefix);
                }

                if (control is TextBox textBox)
                {
                    _settingsCache[prefix + control.Name] = textBox.Text;
                }
            }
        }

        #endregion
    }
}