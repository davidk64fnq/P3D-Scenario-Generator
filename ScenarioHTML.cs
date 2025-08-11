using P3D_Scenario_Generator.CelestialScenario;
using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.PhotoTourScenario;
using P3D_Scenario_Generator.SignWritingScenario;
using P3D_Scenario_Generator.WikipediaScenario;

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

        static internal void GenerateHTMLfiles(ScenarioFormData formData)
        {
            overview = SetOverviewStruct(formData);
            TrySetOverviewHTML(overview, out string overviewHTML, null);
            File.WriteAllText($"{formData.ScenarioFolder}\\Overview.htm", overviewHTML);

            MissionBrief missionBrief = SetMissionBriefStruct(overview, formData);
            string missionBriefHTML = SetMissionBriefHTML(missionBrief);
            File.WriteAllText($"{formData.ScenarioFolder}\\{formData.ScenarioTitle}.htm", missionBriefHTML);

            CopyFiles(formData);
        }

        static private Overview SetOverviewStruct(ScenarioFormData formData)
        {
            Overview overview = new();

            switch (formData.ScenarioType)
            {
                case ScenarioTypes.Circuit:
                    overview.Title = "Circuit Practise";
                    overview.Heading1 = "Circuit Practise";
                    overview.Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}";
                    overview.Difficulty = "Beginner";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    double duration = ((formData.CircuitFinalLeg + (formData.StartRunway.Len / Constants.FeetInNauticalMile) + formData.CircuitUpwindLeg) 
                        * 2 + (formData.CircuitBaseLeg * 2)) / formData.CircuitSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{formData.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
                    overview.Briefing += " by doing that most fundamental of tasks, flying a circuit! ";
                    overview.Briefing += "You'll take off, fly through eight gates as you complete a circuit, ";
                    overview.Briefing += "and land back on the runway. The scenario begins on runway ";
                    overview.Briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
                    overview.Briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";
                    overview.Objective = "Take off and fly through the eight gates before landing on the same runway.";
                    overview.Tips = "Each pair of gates marks the start and finish of a 90 degree turn. ";
                    break;
                case ScenarioTypes.PhotoTour:
                    overview.Title = "Photo Tour";
                    overview.Heading1 = "Photo Tour";
                    overview.Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}";
                    overview.Difficulty = "Intermediate";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = PhotoTourUtilities.GetPhotoTourDistance(PhotoTour.PhotoLocations) / formData.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{formData.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
                    overview.Briefing += " as you navigate from one photo location to the next using IFR (I follow roads) ";
                    overview.Briefing += "You'll take off, fly to a series of photo locations, ";
                    overview.Briefing += "and land at another airport. The scenario begins on runway ";
                    overview.Briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
                    overview.Briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";
                    overview.Objective = $"Take off and visit a series of photo locations before landing at {formData.DestinationRunway.IcaoName} (any runway)";
                    overview.Tips = "Never do today what you can put off till tomorrow";
                    break;
                case ScenarioTypes.SignWriting:
                    overview.Title = "Sign Writing";
                    overview.Heading1 = "Sign Writing";
                    overview.Location = $"{formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) {formData.StartRunway.City}, {formData.StartRunway.Country}";
                    overview.Difficulty = "Advanced";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = SignWriting.GetSignWritingDistance(formData) / formData.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{formData.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
                    overview.Briefing += " as you take on the role of sign writer in the sky! ";
                    overview.Briefing += "You'll take off, fly through a series of gates to spell out a message ";
                    overview.Briefing += "and land again when you've finished. The scenario begins on runway ";
                    overview.Briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
                    overview.Briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";
                    overview.Objective = "Take off and fly through a series of gates before landing on the same runway.";
                    overview.Tips = "When life gives you lemons, squirt someone in the eye.";
                    break;
                case ScenarioTypes.Celestial:
                    overview.Title = "Celestial Navigation";
                    overview.Heading1 = "Celestial Navigation";
                    overview.Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}";
                    overview.Difficulty = "Advanced";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = CelestialNav.GetCelestialDistance(formData) / formData.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{formData.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll dust off your sextant and look to the stars ";
                    overview.Briefing += $"as you test your navigation skills flying a {formData.AircraftTitle}.";
                    overview.Briefing += $" The scenario finishes at {formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) in ";
                    overview.Briefing += $"{formData.DestinationRunway.City}, {formData.DestinationRunway.Country}.";
                    overview.Objective = "Navigate using celestial navigation before landing at the destination airport (any runway)";
                    overview.Tips = "Never go to bed mad. Stay up and fight.";
                    break;
                case ScenarioTypes.WikiList:
                    overview.Title = "Wikipedia List Tour";
                    overview.Heading1 = "Wikipedia List Tour";
                    overview.Location = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}";
                    overview.Difficulty = "Intermediate";
                    // Duration (minutes) approximately sum of leg distances (miles) / speed (knots) * 60 minutes
                    duration = Wikipedia.WikiDistance / formData.AircraftCruiseSpeed * 60;
                    overview.Duration = $"{string.Format("{0:0}", duration)} minutes";
                    overview.Aircraft = $"{formData.AircraftTitle}";
                    overview.Briefing = $"In this scenario you'll test your skills flying a {formData.AircraftTitle}";
                    overview.Briefing += " as you navigate from one Wikipedia list location to the next using IFR (I follow roads) ";
                    overview.Briefing += "You'll take off, fly to a series of list locations, ";
                    overview.Briefing += "and land at another airport. The scenario begins on runway ";
                    overview.Briefing += $"{formData.StartRunway.Number} at {formData.StartRunway.IcaoName} ({formData.StartRunway.IcaoId}) in ";
                    overview.Briefing += $"{formData.StartRunway.City}, {formData.StartRunway.Country}.";
                    overview.Objective = "Take off and visit a series of Wikipedia list locations before landing ";
                    overview.Objective += $"at {formData.DestinationRunway.IcaoName} (any runway)";
                    overview.Tips = "The early bird gets the worm, but the second mouse gets the cheese.";
                    break;
                default:
                    break;
            }

            return overview;
        }

        static private MissionBrief SetMissionBriefStruct(Overview overview, ScenarioFormData formData)
        {
            MissionBrief missionBrief = new();

            switch (formData.ScenarioType)
            {
                case ScenarioTypes.Circuit:
                case ScenarioTypes.PhotoTour:
                case ScenarioTypes.SignWriting:
                case ScenarioTypes.Celestial:
                case ScenarioTypes.WikiList:
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
        
        /// <summary>
        /// Attempts to load an HTML template from an embedded resource, populate it with
        /// data from an <see cref="Overview"/> object, and return the final HTML string.
        /// </summary>
        /// <param name="overview">The object containing the data to populate the HTML template.</param>
        /// <param name="overviewHTML">When this method returns, contains the fully populated HTML string if successful; otherwise, null.</param>
        /// <param name="progressReporter">Optional IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns>True if the HTML was successfully generated; otherwise, false.</returns>
        static internal bool TrySetOverviewHTML(Overview overview, out string overviewHTML, IProgress<string> progressReporter = null)
        {
            string resourceName = "HTML.OverviewSource.htm";

            Log.Info("Starting HTML template processing for overview.");

            // Use the new FileOps method to safely get the template content
            if (!FileOps.TryReadAllTextFromResource(resourceName, progressReporter, out overviewHTML))
            {
                Log.Error($"Failed to get HTML template from resource '{resourceName}'. HTML generation failed.");
                return false;
            }

            try
            {
                // Perform all string replacements
                overviewHTML = overviewHTML.Replace("overviewParams.title", overview.Title ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.h1", overview.Heading1 ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.h2Location", overview.Location ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.pDifficulty", overview.Difficulty ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.pDuration", overview.Duration ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.h2Aircraft", overview.Aircraft ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.pBriefing", overview.Briefing ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.liObjective", overview.Objective ?? "");
                overviewHTML = overviewHTML.Replace("overviewParams.liTips", overview.Tips ?? "");

                Log.Info("Successfully populated overview HTML template.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"An unexpected error occurred during HTML string replacement. Details: {ex.Message}", ex);
                progressReporter?.Report("ERROR: Failed to populate overview HTML.");
                return false;
            }
        }

        static private string SetMissionBriefHTML(MissionBrief missionBrief)
        {
            string missionBriefHTML;

            FileOps.TryGetResourceStream($"HTML.MissionBriefSource.htm", null, out Stream stream);
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

        static private void CopyFiles(ScenarioFormData formData)
        {
            // Copy selected aircraft thumbnail imageURL from P3D instal
            string aircraftImageSource = formData.AircraftImagePath;
            string aircraftImageDest = $"{formData.ScenarioImageFolder}\\Overview_01.jpg";
            if (File.Exists(aircraftImageSource))
            {
                File.Copy(aircraftImageSource, aircraftImageDest, true);
            }
            else
            {
                FileOps.TryGetResourceStream($"Images.thumbnail.jpg", null, out Stream thumbnailStream);
                using (FileStream outputFileStream = new(aircraftImageDest, FileMode.Create))
                {
                    thumbnailStream.CopyTo(outputFileStream);
                }
                thumbnailStream.Dispose();
            }

            // Copy style files
            FileOps.TryGetResourceStream($"CSS.style_kneeboard.css", null, out Stream stream); 
            using (FileStream outputFileStream = new($"{formData.ScenarioFolder}\\style_kneeboard.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            FileOps.TryGetResourceStream($"CSS.style_load_flight.css", null, out stream);
            using (FileStream outputFileStream = new($"{formData.ScenarioFolder}\\style_load_flight.css", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy sound files
            if (!Directory.Exists($"{formData.ScenarioFolder}\\sound"))
            {
                Directory.CreateDirectory($"{formData.ScenarioFolder}\\sound");
            }
            FileOps.TryGetResourceStream($"Sounds.ThruHoop.wav", null, out stream);
            using (FileStream outputFileStream = new($"{formData.ScenarioFolder}\\sound\\ThruHoop.wav", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy aircraft imageURL used in moving maps
            FileOps.TryGetResourceStream($"Images.aircraft.png", null, out stream);
            using (FileStream outputFileStream = new($"{formData.ScenarioImageFolder}\\aircraft.png", FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }
            stream.Dispose();

            // Copy header banner imageURL
            FileOps.TryGetResourceStream($"Images.header.png", null, out stream);
            using (FileStream outputFileStream = new($"{formData.ScenarioImageFolder}\\header.png", FileMode.Create))
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
