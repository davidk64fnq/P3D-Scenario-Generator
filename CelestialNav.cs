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
        public double starNumber;
        public string starName;
        public string wikiLink;
        public string bayer;
        public string notUsed;
        public double raH;
        public double raM;
        public double raS;
        public double decD;
        public double decM;
        public double decS;
        public double visMag;

        public Star(string s1, double d2, string s3, string s4, string s5, string s6, double d7, double d8, double d9, double d10, double d11, double d12, double d13)
        {
            constellation = s1;
            starNumber = d2;
            starName = s3;
            wikiLink = s4;
            bayer = s5;
            notUsed = s6;
            raH = d7;
            raM = d8;
            raS = d9;
            decD = d10;
            decM = d11;
            decS = d12;
            visMag = d13;
        }
    }

    class CelestialNav
    {
        private static readonly List<Star> stars = new List<Star>();
        internal static readonly int noStars = 492;

        static internal void InitStars()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.Excel.CelestialNavStars.xlsx");
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                for (int index = 2; index < noStars + 2; index++)
                {
                    stars.Add(new Star(worksheet.Cells[index, 1].Value.ToString(),
                                        Convert.ToDouble(worksheet.Cells[index, 2].Value),
                                        Convert.ToString(worksheet.Cells[index, 3].Value),
                                        Convert.ToString(worksheet.Cells[index, 4].Value),
                                        Convert.ToString(worksheet.Cells[index, 5].Value),
                                        Convert.ToString(worksheet.Cells[index, 6].Value),
                                        (double)worksheet.Cells[index, 7].Value,
                                        (double)worksheet.Cells[index, 8].Value,
                                        (double)worksheet.Cells[index, 9].Value,
                                        (double)worksheet.Cells[index, 10].Value,
                                        (double)worksheet.Cells[index, 11].Value,
                                        (double)worksheet.Cells[index, 12].Value,
                                        (double)worksheet.Cells[index, 13].Value
                                        ));
                }
                System.Windows.Forms.MessageBox.Show(stars[noStars - 1].notUsed);
            }
        }
    }
}
