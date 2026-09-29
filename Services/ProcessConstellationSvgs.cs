using ImageMagick;

namespace P3D_Scenario_Generator.Services
{
    /// <summary>
    /// Provides a static utility to convert IAU SVG constellation files into PNG/BMP images
    /// suitable for use in the Celestial Navigation scenario.
    /// </summary>
    internal static class ProcessConstellationSvgs
    {
        /// <summary>
        /// Converts a predefined set of IAU SVG constellation files from a source folder
        /// into 32-bit BMP images in a specified output folder.
        /// Images are scaled, ensured to have even dimensions, and set to 96 DPI.
        /// </summary>
        /// <param name="fileOps">The file operations service to use for directory and file management.</param>
        /// <param name="logger">The logging service for reporting warnings and errors.</param>
        /// <param name="svgSourceFolder">The path to the folder containing the source SVG files.</param>
        /// <param name="pngOutputFolder">The path to the folder where the converted BMP files will be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal static async Task CreatePNGs(FileOps fileOps, Logger logger, string svgSourceFolder, string pngOutputFolder)
        {
            if (!FileOps.DirectoryExists(pngOutputFolder))
            {
                if (!await fileOps.TryCreateDirectoryAsync(pngOutputFolder, null))
                {
                    await logger.ErrorAsync($"Failed to create output directory for constellation images: '{pngOutputFolder}'");
                    return;
                }
            }

            // Dictionary mapping: App Constellation Name -> Source SVG Filename
            // Only constellations in this dictionary will be converted (culling the rest)
            var targetConstellations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Andromeda", "Andromeda_IAU.svg" },
                { "Aquila", "Aquila_IAU.svg" },
                { "Aries", "Aries_IAU.svg" },
                { "Auriga", "Auriga_IAU.svg" },
                { "Bootes", "Boötes_IAU.svg" },
                { "Canis Major", "Canis_Major_IAU.svg" },
                { "Canis Minor", "Canis_Minor_IAU.svg" },
                { "Carina", "Carina_IAU.svg" },
                { "Cassiopeia", "Cassiopeia_IAU.svg" },
                { "Centaurus", "Centaurus_IAU.svg" },
                { "Cetus", "Cetus_IAU.svg" },
                { "Corona Borealis", "Corona_Borealis_IAU.svg" },
                { "Corvus", "Corvus_IAU.svg" },
                { "Crux", "Crux_IAU.svg" },
                { "Cygnus", "Cygnus_IAU.svg" },
                { "Draco", "Draco_IAU.svg" },
                { "Eridanus", "Eridanus_IAU.svg" },
                { "Gemini", "Gemini_IAU.svg" },
                { "Grus", "Grus_IAU.svg" },
                { "Hydra", "Hydra_IAU.svg" },
                { "Leo", "Leo_IAU.svg" },
                { "Libra", "Libra_IAU.svg" },
                { "Lyra", "Lyra_IAU.svg" },
                { "Ophiuchus", "Ophiuchus_IAU.svg" },
                { "Orion", "Orion_IAU.svg" },
                { "Pavo", "Pavo_IAU.svg" },
                { "Pegasus", "Pegasus_IAU.svg" },
                { "Perseus", "Perseus_IAU.svg" },
                { "Phoenix", "Phoenix_IAU.svg" },
                { "Piscis Austrinus", "Piscis_Austrinus_IAU.svg" },
                { "Sagittarius", "Sagittarius_IAU.svg" },
                { "Scorpius", "Scorpius_IAU.svg" },
                { "Taurus", "Taurus_IAU.svg" },
                { "Triangulum Australe", "Triangulum_Australe_IAU.svg" },
                { "Ursa Major", "Ursa_Major_IAU.svg" },
                { "Ursa Minor", "Ursa_Minor_IAU.svg" },
                { "Vela", "Vela_IAU.svg" },
                { "Virgo", "Virgo_IAU.svg" }
            };

            var readSettings = new MagickReadSettings
            {
                Density = new Density(300, 300),
                Format = MagickFormat.Svg
            };

            foreach (var entry in targetConstellations)
            {
                string appName = entry.Key;
                string sourceSvgFile = entry.Value;
                string sourcePath = Path.Combine(svgSourceFolder, sourceSvgFile);

                if (!FileOps.FileExists(sourcePath))
                {
                    await logger.WarningAsync($"Source SVG file missing for '{appName}': '{sourceSvgFile}' at '{sourcePath}'");
                    continue;
                }

                string cleanOutputFilename = appName.Replace(" ", "_") + ".bmp";
                string outputPath = Path.Combine(pngOutputFolder, cleanOutputFilename);

                try
                {
                    using (var image = new MagickImage(sourcePath, readSettings))
                    {
                        image.Resize(new MagickGeometry(1720, 800)
                        {
                            IgnoreAspectRatio = false
                        });

                        // Ensure width and height are even numbers (prevents stride alignment failures)
                        uint evenWidth = (image.Width % 2 != 0) ? image.Width + 1 : image.Width;
                        uint evenHeight = (image.Height % 2 != 0) ? image.Height + 1 : image.Height;

                        if (evenWidth != image.Width || evenHeight != image.Height)
                        {
                            image.Extent(evenWidth, evenHeight, Gravity.Center, MagickColors.White);
                        }

                        image.Density = new Density(96, 96);
                        image.Alpha(AlphaOption.Set);
                        image.Depth = 8;
                        image.Format = MagickFormat.Bmp;
                        image.Write(outputPath);
                    }
                    await logger.InfoAsync($"Converted: {sourceSvgFile} -> {cleanOutputFilename}");
                }
                catch (Exception ex)
                {
                    await logger.ErrorAsync($"Failed to convert SVG '{sourceSvgFile}' to BMP. Output: '{cleanOutputFilename}'.", ex);
                }
            }
        }
    }
}