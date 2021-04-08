using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace P3D_Scenario_Generator
{
    internal class ScenarioHTML
    {
        internal struct Overview
        {
            internal string title;
            internal string h1;
            internal string h2Location;
            internal string pDifficulty;
            internal string pDuration;
            internal string h2Aircraft;
            internal string pBriefing;
            internal string liObjective;
            internal string liTips;
        }
        internal struct MissionBrief
        {
            internal string title;
            internal string h1;
            internal string h2Location;
            internal string h2Difficulty;
            internal string h2Duration;
            internal string h2Aircraft;
            internal string pBriefing;
            internal string liObjective;
            internal string h2Tips;
        }

        private static Overview overview;

        static internal void GenerateOverview()
        {
            overview = SetOverviewStruct();
            string overviewHTML = SetOverviewHTML(overview);
            File.WriteAllText($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\Overview.htm", overviewHTML);

            MissionBrief missionBrief = SetMissionBriefStruct(overview);
            string missionBriefHTML = SetMissionBriefHTML(missionBrief);
            File.WriteAllText($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\{Path.GetFileNameWithoutExtension(Parameters.SaveLocation)}.htm", missionBriefHTML);

            CreateImages();
        }

        static private Overview SetOverviewStruct()
        {
            Overview overview = new Overview();

            switch (Parameters.SelectedScenario)
            {
                case "Circuit":
                    overview.title = "Circuit Practise";
                    overview.h1 = "Circuit Practise";
                    overview.h2Location = $"{Runway.IcaoName} ({Runway.IcaoId}) {Runway.City}, {Runway.Country}";
                    overview.pDifficulty = "Beginner";
                    // Duration (minutes) approximately sum of leg distances (miles) * speed (knots) * 60 minutes
                    double duration = ((Parameters.FinalLeg + (Runway.Len / Constants.feetInKnot) + Parameters.UpwindLeg) * 2 + (Parameters.BaseLeg * 2)) / Parameters.Speed * 60;
                    overview.pDuration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.h2Aircraft = $"{Parameters.SelectedAircraft}";
                    overview.pBriefing = $"In this scenario you'll test your skills flying a {Parameters.SelectedAircraft}";
                    overview.pBriefing += " by doing that most fundamental of tasks, flying a circuit! ";
                    overview.pBriefing += "You'll take off, fly through eight gates as you complete a circuit, ";
                    overview.pBriefing += "and land back on the runway. The scenario begins on runway ";
                    overview.pBriefing += $"{Runway.Id} at {Runway.IcaoName} ({Runway.IcaoId}) in ";
                    overview.pBriefing += $"{Runway.City}, {Runway.Country}.";
                    overview.liObjective = "Take off and fly through the eight gates before landing on the same runway.";
                    overview.liTips = "Each pair of gates marks the start and finish of a standard rate left turn of 90 degrees. ";
                    overview.liTips += "The gates are all the same altitude so you'll want to be established in ";
                    overview.liTips += "level flight before you reach the first gate.";
                    break;
                case "Photos":
                    break;
                default:
                    break;
            }

            return overview;
        }

        static private MissionBrief SetMissionBriefStruct(Overview overview)
        {
            MissionBrief missionBrief = new MissionBrief();

            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    missionBrief.title = overview.title;
                    missionBrief.h1 = overview.title;
                    missionBrief.h2Location = overview.h2Location;
                    missionBrief.h2Difficulty = overview.pDifficulty;
                    missionBrief.h2Duration = overview.pDuration;
                    missionBrief.h2Aircraft = overview.h2Aircraft;
                    missionBrief.pBriefing = overview.pBriefing;
                    missionBrief.liObjective = overview.liObjective;
                    missionBrief.h2Tips = overview.liTips;
                    break;
                default:
                    break;
            }

            return missionBrief;
        }

        static private string SetOverviewHTML(Overview overview)
        {
            string overviewHTML;

            overviewHTML = File.ReadAllText("OverviewSource.htm");
            overviewHTML = overviewHTML.Replace("overviewParams.title", $"{overview.title}");
            overviewHTML = overviewHTML.Replace("overviewParams.h1", $"{overview.h1}");
            overviewHTML = overviewHTML.Replace("overviewParams.h2Location", $"{overview.h2Location}");
            overviewHTML = overviewHTML.Replace("overviewParams.pDifficulty", $"{overview.pDifficulty}");
            overviewHTML = overviewHTML.Replace("overviewParams.pDuration", $"{overview.pDuration}");
            overviewHTML = overviewHTML.Replace("overviewParams.h2Aircraft", $"{overview.h2Aircraft}");
            overviewHTML = overviewHTML.Replace("overviewParams.pBriefing", $"{overview.pBriefing}");
            overviewHTML = overviewHTML.Replace("overviewParams.liObjective", $"{overview.liObjective}");
            overviewHTML = overviewHTML.Replace("overviewParams.liTips", $"{overview.liTips}");

            return overviewHTML;
        }

        static private string SetMissionBriefHTML(MissionBrief missionBrief)
        {
            string missionBriefHTML;

            missionBriefHTML = File.ReadAllText("MissionBriefSource.htm");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.title", $"{missionBrief.title}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h1", $"{missionBrief.h1}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h2Location", $"{missionBrief.h2Location}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h2Difficulty", $"{missionBrief.h2Difficulty}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h2Duration", $"{missionBrief.h2Duration}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h2Aircraft", $"{missionBrief.h2Aircraft}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.pBriefing", $"{missionBrief.pBriefing}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.liObjective", $"{missionBrief.liObjective}");
            missionBriefHTML = missionBriefHTML.Replace("missionBriefParams.h2Tips", $"{missionBrief.h2Tips}");

            return missionBriefHTML;
        }

        static private void CreateImages()
        {
            string urlBase = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/";
            string urlKey = "&key=95MBlXbvWXxTKi68aPQA~v9ISwpDCewOlSTQWVHFWWA~AtBDc3Ar7Wh3dy-_6ZnRAOYycWbDfnKmTS8aLwaLYrjJ7mfgZ1K_uazGhZMurFtr";
            string[] urlZoom = { "14", "15", "16", "16", "16", "16" };
            string[] urlMapSize = { "920,135", "300,180", "750,565", "380,232", "380,232", "86,86" };
            string[] urlFilename = { "header.jpg", "chart_thumb.jpg", "Charts_01.jpg", "temp1", "temp2", "temp3" };
            string[] urlIcon = { "", "", "", "success-icon.png", "failure-icon.png", "exit-icon.png" };
            string[] urlIconAdded = { "", "", "", "imgM_c.bmp", "imgM_i.bmp", "exitMission.bmp" };

            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            { 
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            } 

            // Download Bing images
            using WebClient client = new WebClient();
            string url;
            for (int index = 0; index < urlZoom.Length; index++)
            {
                url = $"{urlBase}{Runway.AirportLat},{Runway.AirportLon}/{urlZoom[index]}?mapSize={urlMapSize[index]}{urlKey}";
                client.DownloadFile(new Uri(url), $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}");
            }

            // Copy selected aircraft thumbnail image from P3D instal
            string aircraftImageSource = $"{Aircraft.GetImagename(Parameters.SelectedAircraft)}";
            string aircraftImageDest = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\Overview_01.jpg";
            File.Copy(aircraftImageSource, aircraftImageDest, true);

            // Create completion and exit images
            for (int index = 3; index < urlZoom.Length; index++) 
            { 
                string imageDest = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}";
                using Image image = Image.FromFile(imageDest);
                using (Graphics graphic = Graphics.FromImage(image))
                {
                    using Image imageIcon = Image.FromFile($"{urlIcon[index]}");
                    graphic.DrawImage(imageIcon, 20, 20);
                }
                image.Save($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlIconAdded[index]}");
            }
            for (int index = 3; index < urlZoom.Length; index++)
            {
                File.Delete($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\{urlFilename[index]}");
            }

            // Copy style files
            File.Copy("style_kneeboard.css", $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\style_kneeboard.css", true);
            File.Copy("style_load_flight.css", $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\style_load_flight.css", true);

            // Copy sound files
            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound");
            }
            File.Copy("ThruHoop.wav", $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound\\ThruHoop.wav", true);
        }

        static internal int GetDuration()
        {
            if (overview.pDuration != null)
            {
                string[] words = overview.pDuration.Split(" ");
                return Convert.ToInt32(words[0]);
            }
            else
            {
                return 0;
            }
        }

        static internal string GetDifficulty()
        {
            return overview.pDifficulty;
        }

        static internal string GetBriefing()
        {
            return overview.pBriefing;
        }

        static internal string GetTips()
        {
            return overview.liTips;
        }

        static internal string GetObjective()
        {
            return overview.liObjective;
        }

        static internal string GetTitle()
        {
            return overview.title;
        }
    }
}
