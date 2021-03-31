using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace P3D_Scenario_Generator
{
    public class ScenarioHTML
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

        static internal void GenerateOverview(Runway runway, Params parameters, Aircraft aircraft)
        {
            overview = SetOverviewStruct(runway, parameters);
            string overviewHTML = SetOverviewHTML(overview);
            File.WriteAllText($"{Path.GetDirectoryName(parameters.saveLocation)}\\Overview.htm", overviewHTML);

            MissionBrief missionBrief = SetMissionBriefStruct(parameters, overview);
            string missionBriefHTML = SetMissionBriefHTML(missionBrief);
            File.WriteAllText($"{Path.GetDirectoryName(parameters.saveLocation)}\\{Path.GetFileNameWithoutExtension(parameters.saveLocation)}.htm", missionBriefHTML);

            CreateImages(runway, parameters, aircraft);
        }

        static private Overview SetOverviewStruct(Runway runway, Params parameters)
        {
            Overview overview = new Overview();

            switch (parameters.selectedScenario)
            {
                case "Circuit":
                    overview.title = "Circuit Practise";
                    overview.h1 = "Circuit Practise";
                    overview.h2Location = $"{runway.icaoName} ({runway.icaoId}) {runway.city}, {runway.country}";
                    overview.pDifficulty = "Beginner";
                    // Duration (minutes) approximately sum of leg distances (miles) * speed (knots) * 60 minutes
                    double duration = ((parameters.finalLeg + (runway.len / Constants.feetInKnot) + parameters.upwindLeg) * 2 + (parameters.baseLeg * 2)) / parameters.speed * 60;
                    overview.pDuration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.h2Aircraft = $"{parameters.selectedAircraft}";
                    overview.pBriefing = $"In this scenario you'll test your skills flying a {parameters.selectedAircraft}";
                    overview.pBriefing += " by doing that most fundamental of tasks, flying a circuit! ";
                    overview.pBriefing += "You'll take off, fly through eight gates as you complete a circuit, ";
                    overview.pBriefing += "and land back on the runway. The scenario begins on runway ";
                    overview.pBriefing += $"{runway.id} at {runway.icaoName} ({runway.icaoId}) in ";
                    overview.pBriefing += $"{runway.city}, {runway.country}.";
                    overview.liObjective = "Take off and fly through the eight gates before landing on the same runway.";
                    overview.liTips = "Each pair of gates marks the start and finish of a standard rate turn of 90 degrees. ";
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

        static private MissionBrief SetMissionBriefStruct(Params parameters, Overview overview)
        {
            MissionBrief missionBrief = new MissionBrief();

            switch (parameters.selectedScenario)
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

        static private void CreateImages(Runway runway, Params parameters, Aircraft aircraft)
        {
            string urlBase = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/";
            string urlKey = "&key=95MBlXbvWXxTKi68aPQA~v9ISwpDCewOlSTQWVHFWWA~AtBDc3Ar7Wh3dy-_6ZnRAOYycWbDfnKmTS8aLwaLYrjJ7mfgZ1K_uazGhZMurFtr";
            string[] urlZoom = { "14", "15", "16", "16", "16", "16" };
            string[] urlMapSize = { "920,135", "300,180", "750,565", "380,232", "380,232", "86,86" };
            string[] urlFilename = { "header.jpg", "chart_thumb.jpg", "Charts_01.jpg", "temp1", "temp2", "temp3" };
            string[] urlIcon = { "", "", "", "success-icon.png", "failure-icon.png", "exit-icon.png" };
            string[] urlIconAdded = { "", "", "", "imgM_c.bmp", "imgM_i.bmp", "exitMission.bmp" };

            if (!Directory.Exists($"{Path.GetDirectoryName(parameters.saveLocation)}\\images"))
            { 
                Directory.CreateDirectory($"{Path.GetDirectoryName(parameters.saveLocation)}\\images");
            } 

            // Download Bing images
            using WebClient client = new WebClient();
            string url;
            for (int index = 0; index < urlZoom.Length; index++)
            {
                url = $"{urlBase}{runway.airportLat},{runway.airportLon}/{urlZoom[index]}?mapSize={urlMapSize[index]}{urlKey}";
                client.DownloadFile(new Uri(url), $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\{urlFilename[index]}");
            }

            // Copy selected aircraft thumbnail image from P3D instal
            string aircraftImageSource = $"{aircraft.GetImagename(parameters.selectedAircraft)}";
            string aircraftImageDest = $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\Overview_01.jpg";
            File.Copy(aircraftImageSource, aircraftImageDest, true);

            // Create completion and exit images
            for (int index = 3; index < urlZoom.Length; index++) 
            { 
                string imageDest = $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\{urlFilename[index]}";
                using Image image = Image.FromFile(imageDest);
                using (Graphics graphic = Graphics.FromImage(image))
                {
                    using Image imageIcon = Image.FromFile($"{urlIcon[index]}");
                    graphic.DrawImage(imageIcon, 20, 20);
                }
                image.Save($"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\{urlIconAdded[index]}");
            }
            for (int index = 3; index < urlZoom.Length; index++)
            {
                File.Delete($"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\{urlFilename[index]}");
            }

            // Copy style files
            File.Copy("style_kneeboard.css", $"{Path.GetDirectoryName(parameters.saveLocation)}\\style_kneeboard.css", true);
            File.Copy("style_load_flight.css", $"{Path.GetDirectoryName(parameters.saveLocation)}\\style_load_flight.css", true);
        }

        static public int GetDuration()
        {
            string[] words = overview.pDuration.Split(" ");
            return Convert.ToInt32(words[0]);
        }

        static public string GetDifficulty()
        {
            return overview.pDifficulty;
        }

        static public string GetBriefing()
        {
            return overview.pBriefing;
        }

        static public string GetTips()
        {
            return overview.liTips;
        }
    }
}
