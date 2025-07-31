namespace P3D_Scenario_Generator
{
    // Assuming UIHelpers.TruncateTextForDisplay is accessible
    // If not, you can move it into this class or make sure UIHelpers is public static.

    /// <summary>
    /// Initializes a new instance of the <see cref="FormProgressReporter"/> class.
    /// </summary>
    /// <param name="statusLabel">The <see cref="ToolStripStatusLabel"/> to update with progress messages.</param>
    /// <param name="parentForm">The parent <see cref="Form"/> to use for thread-safe UI updates.</param>
    public class FormProgressReporter(ToolStripStatusLabel statusLabel, Form parentForm) : IProgress<string>
    {
        private readonly ToolStripStatusLabel _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
        private readonly Form _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm)); // Reference to the parent form for Invoke/BeginInvoke

        /// <summary>
        /// Reports a progress update, displaying the message on the status bar and logging it as Info.
        /// The message is truncated for display and the full message is provided via a tooltip.
        /// This method is thread-safe and ensures UI updates occur on the UI thread.
        /// </summary>
        /// <param name="value">The progress message to report.</param>
        public void Report(string value)
        {
            // Log the full message for debugging/auditing only if the value is not null
            if (!string.IsNullOrEmpty(value))
            {
                Log.Info(value);
            }

            // Ensure UI update happens on the UI thread
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
        /// Internal method to update the ToolStripStatusLabel, including truncation and tooltip.
        /// This method assumes it's being called on the UI thread.
        /// </summary>
        /// <param name="message">The message to display.</param>
        private void UpdateStatusLabel(string message)
        {
            // Guard against null if somehow disposed or not fully initialized
            if (_statusLabel == null)
            {
                Log.Error("Attempted to update a null ToolStripStatusLabel in FormProgressReporter.");
                return;
            }

            // Get the current effective width of the status label for truncation
            // Consider its 'Spring' property and actual rendered width after layout.
            // A small buffer might be good for padding/borders.
            int availableWidth = _statusLabel.Width > 0 ? _statusLabel.Width : _parentForm.Width - 20; // Fallback if width is 0 during early init
            if (_statusLabel.Owner != null)
            {
                // If the label is part of a ToolStrip (like StatusStrip), its width might be dynamic.
                // It's generally safest to get its width directly.
                // Subtracting from the parent strip's width might be needed if other items are present.
                // For a single spring label, its own width will reflect its current size.
                // We'll use _statusLabel.Width directly here, assuming it reflects the allocated space.
                availableWidth = _statusLabel.Width;
            }


            string displayMessage = UIHelpers.TruncateTextForDisplay(message, _statusLabel.Font, availableWidth);

            _statusLabel.Text = displayMessage;
            _statusLabel.ToolTipText = message; // Always set the full message for the tooltip
        }
    }
}