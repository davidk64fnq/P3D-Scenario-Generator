using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Manages the persistence of UI control states to a JSON file in the AppData Roaming folder.
    /// Provides safe-save functionality with backups and the ability to revert to designer defaults.
    /// </summary>
    internal class SettingsManager
    {
        private readonly string _settingsFilePath;
        private readonly string _backupFilePath;
        private Dictionary<string, object?> _settingsCache;
        private readonly Dictionary<string, object?> _designerDefaults;
        private readonly Logger _logger;
        private readonly FileOps _fileOps;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="logger">The logging service.</param>
        /// <param name="fileOps">The file operations service.</param>
        internal SettingsManager(Logger logger, FileOps fileOps)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
            _designerDefaults = [];

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
        /// <param name="control">The control whose default values are to be captured.</param>
        internal void CaptureDefaults(Control control)
        {
            if (control is null) return;

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

        /// <summary>
        /// Captures the hardcoded defaults set in the Visual Studio Designer for a collection of controls.
        /// </summary>
        /// <param name="controls">The collection of controls whose default values are to be captured.</param>
        internal void CaptureDefaults(Control.ControlCollection controls)
        {
            foreach (Control control in controls) CaptureDefaults(control);
        }

        /// <summary>
        /// Logic for the "Reset to Defaults" button. Identifies the active tab and resets relevant controls.
        /// </summary>
        /// <param name="tabControl">The main TabControl containing scenario tabs.</param>
        /// <param name="layoutWiki">The layout container for Wikipedia list controls.</param>
        /// <param name="layoutMap">The layout container for Map tile controls.</param>
        internal void RestoreActiveTab(TabControl tabControl, Control layoutWiki, Control layoutMap)
        {
            if (tabControl?.SelectedTab is null) return;

            string tabName = tabControl.SelectedTab.Name;

            if (tabName == "TabPageSettings")
            {
                if (layoutMap is not null) RestoreDefaults(layoutMap.Controls);
            }
            else if (tabName == "TabPageWikiList ")
            {
                if (layoutWiki is not null) RestoreDefaults(layoutWiki.Controls);
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
        /// <param name="control">The control to revert.</param>
        internal void RestoreDefaults(Control control)
        {
            if (control is null) return;

            if (control.Controls.Count > 0) RestoreDefaults(control.Controls);

            if (control is TextBox textBox && _designerDefaults.TryGetValue(control.Name, out var text))
            {
                textBox.Text = text?.ToString() ?? string.Empty;
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

        /// <summary>
        /// Reverts a collection of controls to Designer defaults.
        /// </summary>
        /// <param name="controls">The control collection to revert.</param>
        internal void RestoreDefaults(Control.ControlCollection controls)
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
                        _ = _logger.WarningAsync($"SettingsManager: Settings file at '{_settingsFilePath}' was empty. Attempting backup restore.");
                        HandleBackupRestoreResult();
                        return;
                    }

                    _settingsCache = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json) ?? [];
                    _ = _logger.InfoAsync("SettingsManager: UI settings successfully loaded.");
                }
                else
                {
                    HandleBackupRestoreResult();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.ErrorAsync($"SettingsManager: Failed to parse settings file '{_settingsFilePath}'. Attempting backup restore.", ex);
                HandleBackupRestoreResult();
            }
        }

        private void HandleBackupRestoreResult()
        {
            if (TryRestoreFromBackup())
            {
                _ = _logger.InfoAsync("SettingsManager: Successfully restored UI settings from backup.");
            }
            else
            {
                _ = _logger.WarningAsync("SettingsManager: No valid backup settings found. Reverting to empty/designer defaults.");
            }
        }

        /// <summary>
        /// Attempts to restore settings from the backup file.
        /// </summary>
        /// <returns><see langword="true"/> if settings were restored from backup; otherwise, <see langword="false"/>.</returns>
        private bool TryRestoreFromBackup()
        {
            if (!FileOps.FileExists(_backupFilePath))
            {
                _settingsCache = [];
                return false;
            }

            try
            {
                string json = FileOps.ReadAllText(_backupFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _settingsCache = [];
                    return false;
                }

                var restored = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json);
                if (restored is null)
                {
                    _settingsCache = [];
                    return false;
                }

                _settingsCache = restored;
                return true;
            }
            catch (Exception ex)
            {
                _ = _logger.ErrorAsync($"SettingsManager: Failed to restore settings from backup '{_backupFilePath}'.", ex);
                _settingsCache = [];
                return false;
            }
        }

        /// <summary>
        /// Saves the state of a single UI control (and its children) to the JSON file.
        /// </summary>
        /// <param name="control">The control whose state is to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SaveSettingsAsync(Control control)
        {
            UpdateCacheFromControls(control);
            await CommitCacheToFileAsync();
        }

        /// <summary>
        /// Saves the current state of UI controls to the JSON file using a safe temp-write-and-replace pattern.
        /// </summary>
        /// <param name="controls">The control collection whose state is to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SaveSettingsAsync(Control.ControlCollection controls)
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

                if (!await _fileOps.TryWriteAllTextAsync(tempPath, json))
                {
                    await _logger.ErrorAsync("Failed to write temporary settings file.");
                    return;
                }

                if (FileOps.FileExists(_settingsFilePath))
                {
                    if (!await _fileOps.TryDeleteFileAsync(_backupFilePath))
                    {
                        await _logger.WarningAsync($"SettingsManager: Could not remove old backup '{_backupFilePath}'. Move may fail.");
                    }

                    if (!await _fileOps.TryMoveFileAsync(_settingsFilePath, _backupFilePath))
                    {
                        await _logger.WarningAsync("Could not create backup of settings file, proceeding with save.");
                    }
                }

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
                if (FileOps.FileExists(tempPath))
                {
                    if (!await _fileOps.TryDeleteFileAsync(tempPath))
                    {
                        await _logger.WarningAsync($"SettingsManager: Failed to delete lingering temporary file '{tempPath}'.");
                    }
                }
            }
        }

        #endregion

        #region Restore Logic

        /// <summary>
        /// Populates a single UI control (and its children) with values stored in the JSON cache.
        /// </summary>
        /// <param name="control">The control to restore.</param>
        internal void RestoreSettings(Control control)
        {
            if (control is null) return;

            if (control.Controls.Count > 0) RestoreSettings(control.Controls);

            if (!_settingsCache.TryGetValue(control.Name, out object? value)) return;

            try
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = value?.ToString() ?? string.Empty;
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
                    if (_settingsCache.TryGetValue(indexKey, out object? indexValue) && indexValue is not null)
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

        /// <summary>
        /// Populates a collection of UI controls with values stored in the JSON cache.
        /// </summary>
        /// <param name="controls">The collection of controls to restore.</param>
        internal void RestoreSettings(Control.ControlCollection controls)
        {
            foreach (Control control in controls) RestoreSettings(control);
        }

        #endregion

        #region Update Logic

        private void UpdateCacheFromControls(Control control)
        {
            if (control is null) return;

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
        /// <param name="controls">The control collection to persist.</param>
        /// <param name="aircraftTitle">The title of the aircraft used as the cache scoping key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SaveCircuitSettingsAsync(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls is null) return;

            string prefix = $"Circuit_{aircraftTitle.Trim()}_";
            UpdateCacheWithPrefix(controls, prefix);
            await CommitCacheToFileAsync();
        }

        /// <summary>
        /// Restores Circuit tab controls for a specific aircraft title.
        /// </summary>
        /// <param name="controls">The control collection to restore.</param>
        /// <param name="aircraftTitle">The title of the aircraft used as the cache scoping key.</param>
        /// <returns><see langword="true"/> if saved custom parameters were found; otherwise, <see langword="false"/>.</returns>
        internal bool RestoreCircuitSettings(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls is null) return false;

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
                    textBox.Text = value?.ToString() ?? string.Empty;
                    foundAnyKey = true;
                }
            }

            return foundAnyKey;
        }

        /// <summary>
        /// Saves taxi sign controls under a key scoped to the unique Aircraft Title.
        /// </summary>
        /// <param name="controls">The control collection to persist.</param>
        /// <param name="aircraftTitle">The title of the aircraft used as the cache scoping key.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task SaveSignSettingsAsync(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls is null) return;

            string prefix = $"Sign_{aircraftTitle.Trim()}_";
            UpdateCacheWithPrefix(controls, prefix);
            await CommitCacheToFileAsync();
        }

        /// <summary>
        /// Restores taxi sign controls for a specific aircraft title.
        /// </summary>
        /// <param name="controls">The control collection to restore.</param>
        /// <param name="aircraftTitle">The title of the aircraft used as the cache scoping key.</param>
        /// <returns><see langword="true"/> if saved custom parameters were found; otherwise, <see langword="false"/>.</returns>
        internal bool RestoreSignSettings(Control.ControlCollection controls, string aircraftTitle)
        {
            if (string.IsNullOrWhiteSpace(aircraftTitle) || controls is null) return false;

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
                    textBox.Text = value?.ToString() ?? string.Empty;
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