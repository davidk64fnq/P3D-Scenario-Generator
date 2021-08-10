using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
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

        static internal void getAlmanacData()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Excel.2021 en.xls");
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                System.Windows.Forms.MessageBox.Show(worksheet.Cells[15, 10].Value.ToString());
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
