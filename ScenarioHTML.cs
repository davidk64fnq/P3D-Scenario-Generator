using System;
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

        static internal void GenerateOverview(Runway runway, Params parameters, Aircraft aircraft)
        {
            Overview overview = SetOverviewStruct(runway, parameters);
            string overviewHTML = SetOverviewHTML(overview);
            File.WriteAllText($"{Path.GetDirectoryName(parameters.saveLocation)}\\Overview.htm", overviewHTML);
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
                    overview.pBriefing = $"In this sceanrio you'll test your skills flying a {parameters.selectedAircraft}";
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

        static private void CreateImages(Runway runway, Params parameters, Aircraft aircraft)
        {
            string urlBase = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Aerial/";
            string urlKey = "&key=95MBlXbvWXxTKi68aPQA~v9ISwpDCewOlSTQWVHFWWA~AtBDc3Ar7Wh3dy-_6ZnRAOYycWbDfnKmTS8aLwaLYrjJ7mfgZ1K_uazGhZMurFtr";
            string[] urlZoom = { "14", "15", "16" };
            string[] urlMapSize = { "920,135", "300,180", "750,565" };

            if (!Directory.Exists($"{Path.GetDirectoryName(parameters.saveLocation)}\\images"))
            { 
                Directory.CreateDirectory($"{Path.GetDirectoryName(parameters.saveLocation)}\\images");
            } 

            // Download Bing images
            using WebClient client = new WebClient();
            string url = $"{urlBase}{runway.airportLat},{runway.airportLon}/{urlZoom[0]}?mapSize={urlMapSize[0]}{urlKey}";
            client.DownloadFile(new Uri(url), $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\header.jpg");
            url = $"{urlBase}{runway.airportLat},{runway.airportLon}/{urlZoom[1]}?mapSize={urlMapSize[1]}{urlKey}";
            client.DownloadFile(new Uri(url), $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\chart_thumb.jpg");
            url = $"{urlBase}{runway.airportLat},{runway.airportLon}/{urlZoom[2]}?mapSize={urlMapSize[2]}{urlKey}";
            client.DownloadFile(new Uri(url), $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\Charts_01.jpg");

            // Copy selected aircraft thumbnail image from P3D instal
            string aircraftImageSource = $"{aircraft.GetImagename(parameters.selectedAircraft)}";
            string aircraftImageDest = $"{Path.GetDirectoryName(parameters.saveLocation)}\\images\\Overview_01.jpg";
            File.Copy(aircraftImageSource, aircraftImageDest, true);

            // Copy style files
            File.Copy("style_kneeboard.css", $"{Path.GetDirectoryName(parameters.saveLocation)}\\style_kneeboard.css", true);
            File.Copy("style_load_flight.css", $"{Path.GetDirectoryName(parameters.saveLocation)}\\style_load_flight.css", true);
        }
    }
}
