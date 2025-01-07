using System.Reflection;

namespace P3D_Scenario_Generator
{
    internal class ScenarioHTML
    {
        internal struct Overview
        {
            internal string Title { get; set; }
            internal string Heading1 { get; set; }
            internal string Location { get; set; }
            internal string Difficulty { get; set; }
            internal string Duration { get; set; }
            internal string Aircraft { get; set; }
            internal string Briefing { get; set; }
            internal string Objective { get; set; }
            internal string Tips { get; set; }
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

        internal static Overview overview;

        static internal void GenerateHTMLfiles()
        {
            overview = SetOverviewStruct();
            string overviewHTML = SetOverviewHTML(overview);
            File.WriteAllText($"{Parameters.SettingsScenarioFolder}\\Overview.htm", overviewHTML);

            MissionBrief missionBrief = SetMissionBriefStruct(overview);
            string missionBriefHTML = SetMissionBriefHTML(missionBrief);
            File.WriteAllText($"{Parameters.SettingsScenarioFolder}\\{Parameters.GeneralScenarioTitle}.htm", missionBriefHTML);

            if (Parameters.SelectedScenario != nameof(ScenarioTypes.WikiList) && Parameters.SelectedScenario != nameof(ScenarioTypes.Circuit)
                && Parameters.SelectedScenario != nameof(ScenarioTypes.SignWriting)
                && Parameters.SelectedScenario != nameof(ScenarioTypes.PhotoTour) && Parameters.SelectedScenario != nameof(ScenarioTypes.Testing))
            {
                BingImages.CreateHTMLImages();
            }

            CopyFiles();
        }

        static private Overview SetOverviewStruct()
        {
            Overview overview = new();

            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    overview.Title = "Circuit Practise";
                    overview.Heading1 = "Circuit Practise";
                    overview.Location = $"{Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) {Runway.startRwy.City}, {Runway.startRwy.Country}";
                    overview.Difficulty = "Beginner";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    double duration = ((Parameters.FinalLeg + (Runway.startRwy.Len / Con.feetInNM) + Parameters.UpwindLeg) * 2 + (Parameters.BaseLeg * 2)) / Parameters.Speed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.AircraftTitle}";
                    overview.Briefing += " by doing that most fundamental of tasks, flying a circuit! ";
                    overview.Briefing += "You'll take off, fly through eight gates as you complete a circuit, ";
                    overview.Briefing += "and land back on the runway. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.startRwy.Id} at {Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) in ";
                    overview.Briefing += $"{Runway.startRwy.City}, {Runway.startRwy.Country}.";
                    overview.Objective = "Take off and fly through the eight gates before landing on the same runway.";
                    overview.Tips = "Each pair of gates marks the start and finish of a 90 degree turn. ";
                    break;
                case nameof(ScenarioTypes.PhotoTour):
                    overview.Title = "Photo Tour";
                    overview.Heading1 = "Photo Tour";
                    overview.Location = $"{Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) {Runway.startRwy.City}, {Runway.startRwy.Country}";
                    overview.Difficulty = "Intermediate";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = PhotoTour.GetPhotoTourDistance() / Parameters.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.AircraftTitle}";
                    overview.Briefing += " as you navigate from one photo location to the next using IFR (I follow roads) ";
                    overview.Briefing += "You'll take off, fly to a series of photo locations, ";
                    overview.Briefing += "and land at another airport. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.startRwy.Id} at {Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) in ";
                    overview.Briefing += $"{Runway.startRwy.City}, {Runway.startRwy.Country}.";
                    overview.Objective = "Take off and visit a series of photo locations before landing ";
                    overview.Objective += $"at {Runway.destRwy.IcaoName}, runway {Runway.destRwy.IcaoId}";
                    overview.Tips = "Never do today what you can put off till tomorrow";
                    break;
                case nameof(ScenarioTypes.SignWriting):
                    overview.Title = "Sign Writing";
                    overview.Heading1 = "Sign Writing";
                    overview.Location = $"{Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) {Runway.startRwy.City}, {Runway.startRwy.Country}";
                    overview.Difficulty = "Advanced";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = SignWriting.GetSignWritingDistance() / Parameters.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.AircraftTitle}";
                    overview.Briefing += " as you take on the role of sign writer in the sky! ";
                    overview.Briefing += "You'll take off, fly through a series of gates to spell out a message ";
                    overview.Briefing += "and land again when you've finished. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.startRwy.Id} at {Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) in ";
                    overview.Briefing += $"{Runway.startRwy.City}, {Runway.startRwy.Country}.";
                    overview.Objective = "Take off and fly through a series of gates before landing on the same runway.";
                    overview.Tips = "When life gives you lemons, squirt someone in the eye.";
                    break;
                case nameof(ScenarioTypes.Celestial):
                    overview.Title = "Celestial Navigation";
                    overview.Heading1 = "Celestial Navigation";
                    overview.Location = $"{Runway.destRwy.IcaoName} ({Runway.destRwy.IcaoId}) {Runway.destRwy.City}, {Runway.destRwy.Country}";
                    overview.Difficulty = "Advanced";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = CelestialNav.GetCelestialDistance() / Parameters.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll dust off your sextant and look to the stars ";
                    overview.Briefing += $"as you test your navigation skills flying a {Parameters.AircraftTitle}.";
                    overview.Briefing += " The scenario finishes on runway ";
                    overview.Briefing += $"{Runway.destRwy.Id} at {Runway.destRwy.IcaoName} ({Runway.destRwy.IcaoId}) in ";
                    overview.Briefing += $"{Runway.destRwy.City}, {Runway.destRwy.Country}.";
                    overview.Objective = "Navigate using celestial navigation before landing at the destination runway";
                    overview.Tips = "Never go to bed mad. Stay up and fight.";
                    break;
                case nameof(ScenarioTypes.WikiList):
                    overview.Title = "Wikipedia List Tour";
                    overview.Heading1 = "Wikipedia List Tour";
                    overview.Location = $"{Runway.destRwy.IcaoName} ({Runway.destRwy.IcaoId}) {Runway.destRwy.City}, {Runway.destRwy.Country}";
                    overview.Difficulty = "Intermediate";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = Wikipedia.WikiDistance / Parameters.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.AircraftTitle}";
                    overview.Briefing += " as you navigate from one Wikipedia list location to the next using IFR (I follow roads) ";
                    overview.Briefing += "You'll take off, fly to a series of list locations, ";
                    overview.Briefing += "and land at another airport. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.startRwy.Id} at {Runway.startRwy.IcaoName} ({Runway.startRwy.IcaoId}) in ";
                    overview.Briefing += $"{Runway.startRwy.City}, {Runway.startRwy.Country}.";
                    overview.Objective = "Take off and visit a series of Wikipedia list locations before landing ";
                    overview.Objective += $"at {Runway.destRwy.IcaoName}, runway {Runway.destRwy.IcaoId}";
                    overview.Tips = "The early bird gets the worm, but the second mouse gets the cheese.";
                    break;
                default:
                    break;
            }

            return overview;
        }

        static private MissionBrief SetMissionBriefStruct(Overview overview)
        {
            MissionBrief missionBrief = new();

            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                case nameof(ScenarioTypes.PhotoTour):
                case nameof(ScenarioTypes.SignWriting):
                case nameof(ScenarioTypes.Celestial):
                case nameof(ScenarioTypes.WikiList):
                    missionBrief.title = overview.Title;
                    missionBrief.h1 = overview.Title;
                    missionBrief.h2Location = overview.Location;
                    missionBrief.h2Difficulty = overview.Difficulty;
                    missionBrief.h2Duration = overview.Duration;
                    missionBrief.h2Aircraft = overview.Aircraft;
                    missionBrief.pBriefing = overview.Briefing;
                    missionBrief.liObjective = overview.Objective;
                    missionBrief.h2Tips = overview.Tips;
                    break;
                default:
                    break;
            }

            return missionBrief;
        }

        static private string SetOverviewHTML(Overview overview)
        {
            string overviewHTML;

            Stream stream = Form.GetResourceStream($"HTML.OverviewSource.htm");
            StreamReader reader = new(stream);
            overviewHTML = reader.ReadToEnd();
            stream.Dispose();
            overviewHTML = overviewHTML.Replace("overviewParams.title", $"{overview.Title}");
            overviewHTML = overviewHTML.Replace("overviewParams.h1", $"{overview.Heading1}");
            overviewHTML = overviewHTML.Replace("overviewParams.h2Location", $"{overview.Location}");
            overviewHTML = overviewHTML.Replace("overviewParams.pDifficulty", $"{overview.Difficulty}");
            overviewHTML = overviewHTML.Replace("overviewParams.pDuration", $"{overview.Duration}");
            overviewHTML = overviewHTML.Replace("overviewParams.h2Aircraft", $"{overview.Aircraft}");
            overviewHTML = overviewHTML.Replace("overviewParams.pBriefing", $"{overview.Briefing}");
            overviewHTML = overviewHTML.Replace("overviewParams.liObjective", $"{overview.Objective}");
            overviewHTML = overviewHTML.Replace("overviewParams.liTips", $"{overview.Tips}");

            return overviewHTML;
        }

        static private string SetMissionBriefHTML(MissionBrief missionBrief)
        {
            string missionBriefHTML;

            Stream stream = Form.GetResourceStream($"HTML.MissionBriefSource.htm");
            StreamReader reader = new(stream);
            missionBriefHTML = reader.ReadToEnd();
            stream.Dispose();
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

        static private void CopyFiles()
        {
            // Copy selected aircraft thumbnail imageURL from P3D instal
            string aircraftImageSource = Parameters.AircraftImagePath;
            string aircraftImageDest = $"{Parameters.ImageFolder}\\Overview_01.jpg";
            if (File.Exists(aircraftImageSource))
            {
                File.Copy(aircraftImageSource, aircraftImageDest, true);
            }
            else
            {
                Stream thumbnailStream = Form.GetResourceStream($"Images.thumbnail.jpg");
                using (FileStream outputFileStream = new(aircraftImageDest, FileMode.Create))
                {
                    thumbnailStream.CopyTo(outputFileStream);
                }
                thumbnailStream.Dispose();
            }

            // Copy style files
            Stream stream = Form.GetResourceStream($"CSS.style_kneeboard.css"); 
            using (FileStream outputFileStream = new($"{Parameters.SettingsScenarioFolder}\\style_kneeboard.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            stream = Form.GetResourceStream($"CSS.style_load_flight.css");
            using (FileStream outputFileStream = new($"{Parameters.SettingsScenarioFolder}\\style_load_flight.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy sound files
            if (!Directory.Exists($"{Parameters.SettingsScenarioFolder}\\sound"))
            {
                Directory.CreateDirectory($"{Parameters.SettingsScenarioFolder}\\sound");
            }
            stream = Form.GetResourceStream($"Sounds.ThruHoop.wav");
            using (FileStream outputFileStream = new($"{Parameters.SettingsScenarioFolder}\\sound\\ThruHoop.wav", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy aircraft imageURL used in moving maps
            stream = Form.GetResourceStream($"Images.aircraft.png");
            using (FileStream outputFileStream = new($"{Parameters.ImageFolder}\\aircraft.png", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy header banner imageURL
            stream = Form.GetResourceStream($"Images.header.png");
            using (FileStream outputFileStream = new($"{Parameters.ImageFolder}\\header.png", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();
        }

        static internal int GetDuration()
        {
            if (overview.Duration != null)
            {
                string[] words = overview.Duration.Split(" ");
                return Convert.ToInt32(words[0]);
            }
            else
            {
                return 0;
            }
        }

    }
}
