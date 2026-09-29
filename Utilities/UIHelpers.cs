namespace P3D_Scenario_Generator.Utilities
{
    /// <summary>
    /// Provides helper routines for desktop UI dialogs and text rendering measurements.
    /// </summary>
    internal static class UIHelpers
    {
        /// <summary>
        /// Displays a confirmation dialog to the user with a "Yes" or "No" option.
        /// </summary>
        /// <param name="message">The message to display in the dialog.</param>
        /// <returns><see langword="true"/> if the user clicks "Yes"; otherwise, <see langword="false"/>.</returns>
        internal static bool ConfirmAction(string message)
        {
            DialogResult result = MessageBox.Show(
                message,
                "Confirm Action",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Truncates a string with an ellipsis (...) if it exceeds a maximum width,
        /// considering the specified font for accurate measurement.
        /// </summary>
        /// <param name="text">The original text to potentially truncate.</param>
        /// <param name="font">The font to use for measuring the text's width.</param>
        /// <param name="maxWidth">The maximum allowed width in pixels for the text.</param>
        /// <returns>The original text if it fits, or a truncated version with "..." appended.</returns>
        internal static string TruncateTextForDisplay(string text, Font font, int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            Size textSize = TextRenderer.MeasureText(text, font);

            if (textSize.Width <= maxWidth)
            {
                return text;
            }

            Size ellipsisSize = TextRenderer.MeasureText("...", font);

            if (ellipsisSize.Width >= maxWidth)
            {
                return "...";
            }

            string truncatedText = text;
            int lastIndex = text.Length;
            while (TextRenderer.MeasureText(truncatedText + "...", font).Width > maxWidth && lastIndex > 0)
            {
                lastIndex--;
                truncatedText = text[..lastIndex];
            }

            return truncatedText + "...";
        }
    }
}