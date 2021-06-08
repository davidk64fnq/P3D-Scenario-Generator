using System;
using System.IO;
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
            File.WriteAllText($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\Overview.htm", overviewHTML);

            MissionBrief missionBrief = SetMissionBriefStruct(overview);
            string missionBriefHTML = SetMissionBriefHTML(missionBrief);
            File.WriteAllText($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\{Path.GetFileNameWithoutExtension(Parameters.SaveLocation)}.htm", missionBriefHTML);

            BingImages.CreateHTMLImages();

            CopyFiles();
        }

        static private Overview SetOverviewStruct()
        {
            Overview overview = new Overview();

            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    overview.Title = "Circuit Practise";
                    overview.Heading1 = "Circuit Practise";
                    overview.Location = $"{Runway.IcaoName} ({Runway.IcaoId}) {Runway.City}, {Runway.Country}";
                    overview.Difficulty = "Beginner";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    double duration = ((Parameters.FinalLeg + (Runway.Len / Constants.feetInKnot) + Parameters.UpwindLeg) * 2 + (Parameters.BaseLeg * 2)) / Parameters.Speed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.SelectedAircraft}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.SelectedAircraft}";
                    overview.Briefing += " by doing that most fundamental of tasks, flying a circuit! ";
                    overview.Briefing += "You'll take off, fly through eight gates as you complete a circuit, ";
                    overview.Briefing += "and land back on the runway. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.Id} at {Runway.IcaoName} ({Runway.IcaoId}) in ";
                    overview.Briefing += $"{Runway.City}, {Runway.Country}.";
                    overview.Objective = "Take off and fly through the eight gates before landing on the same runway.";
                    overview.Tips = "Each pair of gates marks the start and finish of a standard rate left turn of 90 degrees. ";
                    break;
                case nameof(ScenarioTypes.PhotoTour):
                    overview.Title = "Photo Tour";
                    overview.Heading1 = "Photo Tour";
                    overview.Location = $"{Runway.IcaoName} ({Runway.IcaoId}) {Runway.City}, {Runway.Country}";
                    overview.Difficulty = "Intermediate";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = PhotoTour.GetPhotoTourDistance() / Aircraft.CruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.SelectedAircraft}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.SelectedAircraft}";
                    overview.Briefing += " as you navigate from one photo location to the next using IFR (I follow roads) ";
                    overview.Briefing += "You'll take off, fly to a series of photo locations, ";
                    overview.Briefing += "and land at another airport. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.Id} at {Runway.IcaoName} ({Runway.IcaoId}) in ";
                    overview.Briefing += $"{Runway.City}, {Runway.Country}.";
                    string[] words = Parameters.DestRunway.Split('\t');
                    overview.Objective = "Take off and visit a series of photo locations before landing ";
                    overview.Objective += $"at {words[0]}, runway {words[1]}";
                    overview.Tips = "Never do today what you can put off till tomorrow";
                    break;
                case nameof(ScenarioTypes.SignWriting):
                    overview.Title = "Sign Writing";
                    overview.Heading1 = "Sign Writing";
                    overview.Location = $"{Runway.IcaoName} ({Runway.IcaoId}) {Runway.City}, {Runway.Country}";
                    overview.Difficulty = "Advanced";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = SignWriting.GetSignWritingDistance() / Aircraft.CruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{Parameters.SelectedAircraft}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {Parameters.SelectedAircraft}";
                    overview.Briefing += " as you take on the role of sign writer in the sky!";
                    overview.Briefing += "You'll take off, fly through a series of gates to spell out a message ";
                    overview.Briefing += "and land again when you've finished. The scenario begins on runway ";
                    overview.Briefing += $"{Runway.Id} at {Runway.IcaoName} ({Runway.IcaoId}) in ";
                    overview.Briefing += $"{Runway.City}, {Runway.Country}.";
                    overview.Objective = "Take off and fly through a series of gates before landing on the same runway.";
                    overview.Tips = "When life gives you lemons, squirt someone in the eye.";
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
                case nameof(ScenarioTypes.PhotoTour):
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

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.OverviewSource.htm");
            StreamReader reader = new StreamReader(stream);
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

            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.MissionBriefSource.htm");
            StreamReader reader = new StreamReader(stream);
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
            // Copy selected aircraft thumbnail image from P3D instal
            string aircraftImageSource = $"{Aircraft.GetImagename(Parameters.SelectedAircraft)}";
            string aircraftImageDest = $"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\Overview_01.jpg";
            File.Copy(aircraftImageSource, aircraftImageDest, true);

            // Copy style files
            Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.style_kneeboard.css"); 
            using (FileStream outputFileStream = new FileStream($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\style_kneeboard.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.style_load_flight.css");
            using (FileStream outputFileStream = new FileStream($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\style_load_flight.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy sound files
            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound");
            }
            stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.ThruHoop.wav");
            using (FileStream outputFileStream = new FileStream($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\sound\\ThruHoop.wav", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy PhotoTour leg route html aircraft image
            if (!Directory.Exists($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images"))
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images");
            }
            stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.aircraft.png");
            using (FileStream outputFileStream = new FileStream($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\aircraft.png", FileMode.Create))
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
