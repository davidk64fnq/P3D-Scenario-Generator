using System.Reflection;
using OfficeOpenXml;

namespace P3D_Scenario_Generator
{
    public class Star(string s1, string s2, string s3, string s4, string s5, string s6, string s7, double d8, double d9, double d10, double d11, double d12, double d13, double d14)
    {
        public string constellation = s1;
        public string id = s2;
        public string connectedId = s3;
        public string starNumber = s4;
        public string starName = s5;
        public string wikiLink = s6;
        public string bayer = s7;
        public double raH = d8;
        public double raM = d9;
        public double raS = d10;
        public double decD = d11;
        public double decM = d12;
        public double decS = d13;
        public double visMag = d14;
    }

    class CelestialNav
    {
        private static readonly List<Star> stars = [];
        internal static List<string> navStarNames = [];
        internal static int noStars = 0;
        internal static double[,] ariesGHAd = new double[3, 24];
        internal static double[,] ariesGHAm = new double[3, 24];
        internal static double[] starsSHAd = new double[57];
        internal static double[] starsSHAm = new double[57];
        internal static double[] starsDECd = new double[57];
        internal static double[] starsDECm = new double[57];
        internal static double midairStartHdg;
        internal static double midairStartLat;
        internal static double midairStartLon;
        internal static double destinationLat;
        internal static double destinationLon;

        static internal void CreateStarsDat()
        {
            string starsDat = $"[Star Settings]\nIntensity=230\nNumStars={noStars}\n[Star Locations]\n";

            // If stars.dat exists rename it
            if (File.Exists("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat"))
            {
                Form.DeleteFile("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat.P3DscenarioGenerator.backup");
                File.Move("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat", "C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat.P3DscenarioGenerator.backup");
            }

            for (int index = 0; index < noStars; index++)
            {
                starsDat += $"Star.{index} = {index + 1}";
                starsDat += $",{stars[index].raH}";
                starsDat += $",{stars[index].raM}";
                starsDat += $",{stars[index].raS}";
                starsDat += $",{stars[index].decD}";
                starsDat += $",{stars[index].decM}";
                starsDat += $",{stars[index].decS}";
                starsDat += $",{stars[index].visMag}\n";
            }

            File.WriteAllText("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat", starsDat);
        }

        static internal Star GetStar(int index)
        {
            return stars[index];
        }

        // http://www.tecepe.com.br/scripts/AlmanacPagesISAPI.dll
        static internal void GetAlmanacData()
        {
            string url;
            string almanacData = "";
            int[] navStarMapping = [6, 4, 29, 18, -1, 9, 31, 33, 54, 14, 24, 40, 0, 50, 1, 41, 36, 42, 21, 12, 15, 16, 11, -1, 52, 27, 3, 26, 13, 46, 53, 55, 30, 28, 34, 5, 47, 39, 56, 7, 35, 23, 8, 49, 51, -1, 20, 19, 45, 25, 10, 37, 43, 2, 44, 17, 32, 22, 48, 38];

            DateTime startDate = new(Parameters.Year, Parameters.Month, Parameters.Day, Parameters.Hours, Parameters.Minutes, Parameters.Seconds, DateTimeKind.Local);
            startDate = startDate.AddDays(-1);
            url = $"http://www.tecepe.com.br/scripts/AlmanacPagesISAPI.dll/pages?date={startDate.Month}%2F{startDate.Day}%2F{startDate.Year}";
            almanacData = HttpRoutines.GetWebString(url);

            // handle return from getwebstring of ""

            string ariesGHAdata = almanacData[almanacData.IndexOf("G.M.T")..];
            string[] hours = ariesGHAdata.Split("\n");
            int day = 0;
            int hour = 0;
            for (int line = 2; line < 85; line++)
            {
                if (hours[line].Length > 6 && hours[line][6] == '|')
                {
                    string[] pipes = hours[line].Split('|');
                    string[] spaces = pipes[1].Trim().Split(' ');
                    ariesGHAd[day, hour] = Convert.ToDouble(spaces[0]);
                    ariesGHAm[day, hour++] = Convert.ToDouble(spaces[1]);
                    if (hour == 24)
                    {
                        hour = 0;
                        day++;
                    }
                }
            }

            string starsData = almanacData[almanacData.IndexOf(" | Acamar")..];
            string[] stars = starsData.Split("\n");
            int sequenceNo = 0;
            int alphaNo = 0;
            int decSign;
            for (int line = 0; line < 71; line++)
            {
                string[] pipes = stars[line].Split('|');
                if (!String.Equals(pipes[^1], " \r"))
                {
                    string starData = pipes[^1][12..];
                    string[] spaces = starData.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (sequenceNo != 4 && sequenceNo != 23 && sequenceNo != 45) // these stars not included
                    {
                        if (sequenceNo == 8)
                        {
                            alphaNo = 4; // put "Al Nair" in sort order that C# uses
                        }
                        starsSHAd[alphaNo] = Convert.ToDouble(spaces[0]);
                        starsSHAm[alphaNo] = Convert.ToDouble(spaces[1]);
                        if (spaces[2].StartsWith('S'))
                        {
                            decSign = -1;
                        }
                        else
                        {
                            decSign = 1;
                        }
                        if (spaces[2].Length == 1)
                        {
                            starsDECd[alphaNo] = Convert.ToDouble(spaces[3]) * decSign;
                            starsDECm[alphaNo] = Convert.ToDouble(spaces[4]);
                        }
                        else
                        {
                            starsDECd[alphaNo] = Convert.ToDouble(spaces[2][1..]) * decSign;
                            starsDECm[alphaNo] = Convert.ToDouble(spaces[3]);
                        }
                        if (alphaNo == 3)
                        {
                            alphaNo = 5; // leave room for "Al Nair"
                        }
                        else if (alphaNo == 4)
                        {
                            alphaNo = 8; // skip back to usual alphabetical order
                        }
                        else
                        {
                            alphaNo++;
                        }
                    }
                    sequenceNo++;
                }
            }
        }

        static internal void InitStars()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Excel.CelestialNavStars.xlsx");
            using ExcelPackage package = new(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int index = 2; // skip header row
            while (worksheet.Cells[index, 1].Value != null)
            {
                stars.Add(new Star(Convert.ToString(worksheet.Cells[index, 1].Value),
                    Convert.ToString(worksheet.Cells[index, 2].Value),
                    Convert.ToString(worksheet.Cells[index, 3].Value),
                    Convert.ToString(worksheet.Cells[index, 4].Value),
                    Convert.ToString(worksheet.Cells[index, 5].Value),
                    Convert.ToString(worksheet.Cells[index, 6].Value),
                    Convert.ToString(worksheet.Cells[index, 7].Value),
                    (double)worksheet.Cells[index, 8].Value,
                    (double)worksheet.Cells[index, 9].Value,
                    (double)worksheet.Cells[index, 10].Value,
                    (double)worksheet.Cells[index, 11].Value,
                    (double)worksheet.Cells[index, 12].Value,
                    (double)worksheet.Cells[index, 13].Value,
                    (double)worksheet.Cells[index, 14].Value
                    ));
                if (Convert.ToString(worksheet.Cells[index, 5].Value) != "")
                {
                    navStarNames.Add(worksheet.Cells[index, 5].Value.ToString());
                }
                noStars++;
                index++;
            }
            navStarNames.Sort();
        }

        static internal double GetCelestialDistance()
        {
            return MathRoutines.CalcDistance(midairStartLat, midairStartLon, destinationLat, destinationLon);
        }

        static internal void SetCelestialStartLocation()
        {
            Random random = new();
            midairStartHdg = -180 + random.Next(0, 360);

            // Position plane in random direction from destination runway between min/max distance parameter
            double randomAngle = random.Next(0, 90) * Math.PI / 180.0;
            double randomRadius = Parameters.CelestialMinDistance + random.Next(0, (int)(Parameters.CelestialMaxDistance - Parameters.CelestialMinDistance));
            double randomLatAdj = randomRadius * Con.feetInNM / Con.degreeLatFeet * Math.Cos(randomAngle) * (random.Next(0, 2) * 2 - 1);
            double randomLonAdj = randomRadius * Con.feetInNM / Con.degreeLatFeet * Math.Sin(randomAngle) * (random.Next(0, 2) * 2 - 1);
            midairStartLat = Runway.destRwy.AirportLat + randomLatAdj;
            if (midairStartLat > 90)
            {
                midairStartLat -= 180;
            }
            else if (midairStartLat < -90)
            {
                midairStartLat += 180;
            }
            midairStartLon = Runway.destRwy.AirportLon + randomLonAdj;
            if (midairStartLon > 180)
            {
                midairStartLon -= 360;
            }
            else if (midairStartLon < -180)
            {
                midairStartLon += 360;
            }
        }

        static internal void SetCelestialSextantHTML(string saveLocation)
        {
            string celestialHTML;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.HTML.CelestialSextant.html");
            StreamReader reader = new(stream);
            celestialHTML = reader.ReadToEnd();
            string starOptions = "<option>Select Star</option>";
            for (int index = 0; index < CelestialNav.navStarNames.Count; index++)
            {
                starOptions += $"<option>{CelestialNav.navStarNames[index]}</option>";
            }
            celestialHTML = celestialHTML.Replace("starOptionsX", starOptions);
            File.WriteAllText(saveLocation, celestialHTML);
            stream.Dispose();
        }

        static internal void SetCelestialSextantJS(string saveLocation)
        {
            string celestialJS;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Javascript.scriptsCelestialSextant.js");
            StreamReader reader = new(stream);
            celestialJS = reader.ReadToEnd();
            string constellation = "";
            string id = "";
            string starNumber = "";
            string starName = "";
            string bayer = "";
            string raH = "";
            string raM = "";
            string raS = "";
            string decD = "";
            string decM = "";
            string decS = "";
            string visMag = "";
            string lines = "";
            Star star;
            for (int index = 0; index < CelestialNav.noStars; index++)
            {
                star = CelestialNav.GetStar(index);
                constellation += $"\"{star.constellation}\"";
                id += $"\"{star.id}\"";
                starNumber += $"\"{star.starNumber}\"";
                starName += $"\"{star.starName}\"";
                bayer += $"\"{star.bayer}\"";
                raH += star.raH.ToString();
                raM += star.raM.ToString();
                raS += star.raS.ToString();
                decD += star.decD.ToString();
                decM += star.decM.ToString();
                decS += star.decS.ToString();
                visMag += star.visMag.ToString();
                if (star.connectedId != "")
                {
                    if (lines != "")
                    {
                        lines += ", ";
                    }
                    lines += $"\"{star.id}\", \"{star.connectedId}\"";
                }
                if (index < CelestialNav.noStars - 1)
                {
                    constellation += ",";
                    id += ",";
                    starNumber += ",";
                    starName += ",";
                    bayer += ",";
                    raH += ",";
                    raM += ",";
                    raS += ",";
                    decD += ",";
                    decM += ",";
                    decS += ",";
                    visMag += ",";
                }
            }
            celestialJS = celestialJS.Replace("constellationX", constellation);
            celestialJS = celestialJS.Replace("idX", id);
            celestialJS = celestialJS.Replace("starNumberX", starNumber);
            celestialJS = celestialJS.Replace("starNameX", starName);
            celestialJS = celestialJS.Replace("bayerX", bayer);
            celestialJS = celestialJS.Replace("raHX", raH);
            celestialJS = celestialJS.Replace("raMX", raM);
            celestialJS = celestialJS.Replace("raSX", raS);
            celestialJS = celestialJS.Replace("decDX", decD);
            celestialJS = celestialJS.Replace("decMX", decM);
            celestialJS = celestialJS.Replace("decSX", decS);
            celestialJS = celestialJS.Replace("visMagX", visMag);
            celestialJS = celestialJS.Replace("linesX", lines);
            celestialJS = celestialJS.Replace("destLatX", CelestialNav.destinationLat.ToString());
            celestialJS = celestialJS.Replace("destLonX", CelestialNav.destinationLon.ToString());
            string ariesGHAd = "";
            string ariesGHAm = "";
            for (int day = 0; day < 3; day++)
            {
                ariesGHAd += "[";
                ariesGHAm += "[";
                for (int hour = 0; hour < 24; hour++)
                {
                    ariesGHAd += CelestialNav.ariesGHAd[day, hour];
                    ariesGHAm += CelestialNav.ariesGHAm[day, hour];
                    if (hour < 23)
                    {
                        ariesGHAd += ",";
                        ariesGHAm += ",";
                    }
                }
                ariesGHAd += "]";
                ariesGHAm += "]";
                if (day < 2)
                {
                    ariesGHAd += ",";
                    ariesGHAm += ",";
                }
            }
            celestialJS = celestialJS.Replace("ariesGHAdX", ariesGHAd);
            celestialJS = celestialJS.Replace("ariesGHAmX", ariesGHAm);
            string starsSHAd = "";
            string starsSHAm = "";
            for (int starIndex = 0; starIndex < CelestialNav.starsSHAd.Length; starIndex++)
            {
                starsSHAd += CelestialNav.starsSHAd[starIndex];
                starsSHAm += CelestialNav.starsSHAm[starIndex];
                if (starIndex < CelestialNav.starsSHAd.Length - 1)
                {
                    starsSHAd += ",";
                    starsSHAm += ",";
                }
            }
            celestialJS = celestialJS.Replace("starsSHAdX", starsSHAd);
            celestialJS = celestialJS.Replace("starsSHAmX", starsSHAm);
            string starsDECd = "";
            string starsDECm = "";
            for (int starIndex = 0; starIndex < CelestialNav.starsDECd.Length; starIndex++)
            {
                starsDECd += CelestialNav.starsDECd[starIndex];
                starsDECm += CelestialNav.starsDECm[starIndex];
                if (starIndex < CelestialNav.starsDECd.Length - 1)
                {
                    starsDECd += ",";
                    starsDECm += ",";
                }
            }
            celestialJS = celestialJS.Replace("starsDECdX", starsDECd);
            celestialJS = celestialJS.Replace("starsDECmX", starsDECm);
            string starNameList = "";
            for (int starIndex = 0; starIndex < CelestialNav.navStarNames.Count; starIndex++)
            {
                starNameList += $"\"{CelestialNav.navStarNames[starIndex]}\"";
                if (starIndex < CelestialNav.navStarNames.Count - 1)
                {
                    starNameList += ",";
                }
            }
            celestialJS = celestialJS.Replace("starNameListX", starNameList);
            string startDate = $"\"{Parameters.Month}/{Parameters.Day}/{Parameters.Year}\"";
            celestialJS = celestialJS.Replace("startDateX", startDate);
            celestialJS = celestialJS.Replace("northEdgeX", Parameters.CelestialImageNorth.ToString());
            celestialJS = celestialJS.Replace("eastEdgeX", Parameters.CelestialImageEast.ToString());
            celestialJS = celestialJS.Replace("southEdgeX", Parameters.CelestialImageSouth.ToString());
            celestialJS = celestialJS.Replace("westEdgeX", Parameters.CelestialImageWest.ToString());
            File.WriteAllText($"{saveLocation}\\scriptsCelestialSextant.js", celestialJS);
            stream.Dispose();

            stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Javascript.scriptsCelestialAstroCalcs.js");
            reader = new StreamReader(stream);
            celestialJS = reader.ReadToEnd();
            File.WriteAllText($"{saveLocation}\\scriptsCelestialAstroCalcs.js", celestialJS);
            stream.Dispose();
        }

        static internal void SetCelestialSextantCSS(string saveLocation)
        {
            string signWritingCSS;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.CSS.styleCelestialSextant.css");
            StreamReader reader = new(stream);
            signWritingCSS = reader.ReadToEnd();
            File.WriteAllText(saveLocation, signWritingCSS);
            stream.Dispose();
        }
    }
}
