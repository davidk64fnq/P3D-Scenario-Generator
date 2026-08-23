using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick;

namespace P3D_Scenario_Generator.Services
{

    /// <summary>
    /// A helper function for converting the IAU svg files sourced from the web into 
    /// png files for easier handling in the Celestial Naviagtion scenario, to use
    /// call from one of the UI buttons.
    /// </summary>
    public class ProcessConstellationSvgs()
    {
        public static void CreatePNGs(string svgSourceFolder, string pngOutputFolder)
        {
            Directory.CreateDirectory(pngOutputFolder);

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

            // Set high density so SVG vectors render sharply before scaling
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

                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine($"[WARN] Source file missing: {sourceSvgFile}");
                    continue;
                }

                // Clean output name matching app convention (e.g., "Canis_Major.bmp", "Bootes.bmp")
                string cleanOutputFilename = appName.Replace(" ", "_") + ".bmp";
                string outputPath = Path.Combine(pngOutputFolder, cleanOutputFilename);

                using (var image = new MagickImage(sourcePath, readSettings))
                {
                    // 1. Scale to fit 1720x800 maintaining aspect ratio
                    image.Resize(new MagickGeometry(1720, 800)
                    {
                        IgnoreAspectRatio = false
                    });

                    // 2. Ensure width and height are even numbers (prevents stride alignment failures)
                    uint evenWidth = (image.Width % 2 != 0) ? image.Width + 1 : image.Width;
                    uint evenHeight = (image.Height % 2 != 0) ? image.Height + 1 : image.Height;

                    if (evenWidth != image.Width || evenHeight != image.Height)
                    {
                        image.Extent(evenWidth, evenHeight, Gravity.Center, MagickColors.White);
                    }

                    // 3. Set standard 96 DPI screen density
                    image.Density = new Density(96, 96);

                    // 4. Force 32-bit BMP (8 bits/channel x 4 RGBA channels = 32-bit depth)
                    image.Alpha(AlphaOption.Set);
                    image.Depth = 8;
                    image.Format = MagickFormat.Bmp;
                    image.Write(outputPath);
                }

                Console.WriteLine($"[OK] Converted: {sourceSvgFile} -> {cleanOutputFilename}");
            }
        }
    }
}