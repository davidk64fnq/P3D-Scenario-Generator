namespace P3D_Scenario_Generator
{
    public static class UIHelpers
    {
        /// <summary>
        /// Truncates a string with an ellipsis (...) if it exceeds a maximum width,
        /// considering the specified font for accurate measurement.
        /// </summary>
        /// <param name="text">The original text to potentially truncate.</param>
        /// <param name="font">The font to use for measuring the text's width.</param>
        /// <param name="maxWidth">The maximum allowed width in pixels for the text.</param>
        /// <returns>The original text if it fits, or a truncated version with "..." appended.</returns>
        public static string TruncateTextForDisplay(string text, Font font, int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Measure the full text with the given font
            Size textSize = TextRenderer.MeasureText(text, font);

            // If the text already fits within the maximum width, return it as is.
            if (textSize.Width <= maxWidth)
            {
                return text;
            }

            // Calculate the width of the ellipsis "..."
            Size ellipsisSize = TextRenderer.MeasureText("...", font);

            // If even the ellipsis itself doesn't fit within the max width,
            // just return the ellipsis to indicate content is present but too long.
            if (ellipsisSize.Width >= maxWidth)
            {
                return "...";
            }

            // Iterate backward, shortening the text until it fits with the ellipsis
            string truncatedText = text;
            int lastIndex = text.Length;
            while (TextRenderer.MeasureText(truncatedText + "...", font).Width > maxWidth && lastIndex > 0)
            {
                lastIndex--;
                truncatedText = text[..lastIndex];
            }

            // Append the ellipsis and return the truncated string
            return truncatedText + "...";
        }
    }
}