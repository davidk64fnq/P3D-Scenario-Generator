using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace P3D_Scenario_Generator
{
    public class Star
    {
        public string constellation;
        public string id;
        public string connectedId;
        public string starNumber;
        public string starName;
        public string wikiLink;
        public string bayer;
        public double raH;
        public double raM;
        public double raS;
        public double decD;
        public double decM;
        public double decS;
        public double visMag;

        public Star(string s1, string s2, string s3, string s4, string s5, string s6, string s7, double d8, double d9, double d10, double d11, double d12, double d13, double d14)
        {
            constellation = s1;
            id = s2;
            connectedId = s3;
            starNumber = s4;
            starName = s5;
            wikiLink = s6;
            bayer = s7;
            raH = d8;
            raM = d9;
            raS = d10;
            decD = d11;
            decM = d12;
            decS = d13;
            visMag = d14;
        }
    }

    class CelestialNav
    {
        private static readonly List<Star> stars = new List<Star>();
        internal static int noStars = 0;
        internal static double[,] ariesGHAd = new double [3, 24];
        internal static double[,] ariesGHAm = new double [3, 24];
        internal static double[] starsSHAd = new double[57];
        internal static double[] starsSHAm = new double[57];
        internal static double[] starsDECd = new double[57];
        internal static double[] starsDECm = new double[57];

        static internal void CreateStarsDat()
        {
            string starsDat = $"[Star Settings]\nIntensity=230\nNumStars={noStars}\n[Star Locations]\n";

            // If stars.dat exists rename it
            if (File.Exists("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat"))
            {
                File.Delete("C:\\ProgramData\\Lockheed Martin\\Prepar3D v5\\stars.dat.P3DscenarioGenerator.backup");
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
            using WebClient client = new WebClient();
            string url;
            string almanacData = "";
            int[] navStarMapping = {6, 4, 29, 18, -1, 9, 31, 33, 54, 14, 24, 40, 0, 50, 1, 41, 36, 42, 21, 12, 15, 16, 11, -1, 52, 27, 3, 26, 13, 46, 53, 55, 30, 28, 34, 5, 47, 39, 56, 7, 35, 23, 8, 49, 51, -1, 20, 19, 45, 25, 10, 37, 43, 2, 44, 17, 32, 22, 48, 38};

            url = "http://www.tecepe.com.br/scripts/AlmanacPagesISAPI.dll/pages?date=08%2F11%2F2021";

            try
            {
                almanacData = client.DownloadString(new Uri(url));
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Encountered issues obtaining almanac data", "Almanac data download", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }

            string ariesGHAdata = almanacData.Substring(almanacData.IndexOf("G.M.T"));
            string[] days = ariesGHAdata.Split(" 0 ");
            for (int day = 0; day < 3; day++)
            {
                string[] hours = days[day + 1].Split("\n");
                int hour = 0;
                for (int line = 0; line < 27; line++)
                {
                    if (hours[line].Contains('|'))
                    {
                        string[] pipes = hours[line].Split('|');
                        string[] spaces = pipes[1].Trim().Split(' ');
                        ariesGHAd[day, hour] = Convert.ToDouble(spaces[0]);
                        ariesGHAm[day, hour++] = Convert.ToDouble(spaces[1]);
                    }
                }
            }

            string starsData = almanacData.Substring(almanacData.IndexOf(" | Acamar"));
            string[] stars = starsData.Split("\n");
            int starNo = 0;
            int decSign;
            for (int line = 0; line < 71; line++)
            {
                string[] pipes = stars[line].Split('|');
                if (!String.Equals(pipes[pipes.Length - 1], " \r"))
                {
                    string starData = pipes[pipes.Length - 1].Substring(12);
                    string[] spaces = starData.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int mappedStarNo = navStarMapping[starNo];
                    if (mappedStarNo != -1)
                    {
                        starsSHAd[mappedStarNo] = Convert.ToDouble(spaces[0]);
                        starsSHAm[mappedStarNo] = Convert.ToDouble(spaces[1]);
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
                            starsDECd[mappedStarNo] = Convert.ToDouble(spaces[3]) * decSign;
                            starsDECm[mappedStarNo] = Convert.ToDouble(spaces[4]);
                        }
                        else
                        {
                            starsDECd[mappedStarNo] = Convert.ToDouble(spaces[2].Substring(1)) * decSign;
                            starsDECm[mappedStarNo] = Convert.ToDouble(spaces[3]);
                        }
                    }
                    starNo++;
                }
            }
        }

        static internal void InitStars()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Excel.CelestialNavStars.xlsx");
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                int index = 2; // skip header row
                while (worksheet.Cells[index, 1].Value != null)
                {
                    stars.Add(new Star(worksheet.Cells[index, 1].Value.ToString(),
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
                    noStars++;
                    index++;

                }
            }
        }
    }
}
