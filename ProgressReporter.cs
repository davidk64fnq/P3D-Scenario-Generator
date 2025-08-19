namespace P3D_Scenario_Generator
{
    /// <summary>
    /// A custom <see cref="IProgress{T}"/> implementation for reporting progress updates to a <see cref="ToolStripStatusLabel"/> on a <see cref="Form"/>.
    /// </summary>
    /// <param name="statusLabel">The status label to update.</param>
    /// <param name="parentForm">The parent form that owns the status label.</param>
    public class FormProgressReporter(ToolStripStatusLabel statusLabel, Form parentForm) : IProgress<string>
    {
        private readonly ToolStripStatusLabel _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
        private readonly Form _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));

        // Fields for throttling
        private DateTime _lastUpdateTime = DateTime.Now;
        private readonly TimeSpan _minUpdateInterval = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets or sets a value indicating whether progress message throttling is enabled.
        /// When <see langword="false"/> (the default), every call to <see cref="Report(string)"/> will result in an immediate UI update.
        /// </summary>
        public bool IsThrottlingEnabled { get; set; } = false;

        /// <summary>
        /// Reports a progress update, displaying the message on the status bar and logging it as Info.
        /// </summary>
        /// <param name="value">The progress message to display and log.</param>
        public void Report(string value)
        {
            if (value == null)
            {
                return;
            }

            if (_parentForm.InvokeRequired)
            {
                _parentForm.BeginInvoke(new Action(() => UpdateStatusLabel(value)));
            }
            else
            {
                UpdateStatusLabel(value);
            }
        }

        /// <summary>
        /// Updates the text of the status label on the UI thread, with optional throttling.
        /// </summary>
        /// <param name="message">The message to display in the status label.</param>
        private void UpdateStatusLabel(string message)
        {
            // Apply throttling only if enabled.
            if (IsThrottlingEnabled && (DateTime.Now - _lastUpdateTime) < _minUpdateInterval)
            {
                return;
            }

            if (_statusLabel == null)
            {
                return;
            }

            int availableWidth = _statusLabel.Width > 0 ? _statusLabel.Width : _parentForm.Width - 20;
            if (_statusLabel.Owner != null)
            {
                availableWidth = _statusLabel.Width;
            }

            string displayMessage = UIHelpers.TruncateTextForDisplay(message, _statusLabel.Font, availableWidth);
            _statusLabel.Text = displayMessage;
            _statusLabel.ToolTipText = message;

            _lastUpdateTime = DateTime.Now;
        }
    }
}