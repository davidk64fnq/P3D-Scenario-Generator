using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        static internal void GenerateOverview(Runway runway, Params parameters)
        {
            Overview overview = new Overview();
            SetOverviewStruct(runway, parameters, ref overview);
            string overviewHTML;
            overviewHTML = File.ReadAllText("OverviewSource.htm");
            overviewHTML = overviewHTML.Replace("overviewParams.title", $"{overview.title}");
            overviewHTML = overviewHTML.Replace("overviewParams.h1", $"{overview.h1}");
            overviewHTML = overviewHTML.Replace("overviewParams.h2Location", $"{overview.h2Location}");
            overviewHTML = overviewHTML.Replace("overviewParams.pDifficulty", $"{overview.pDifficulty}");
            File.WriteAllText($"{Path.GetDirectoryName(parameters.saveLocation)}\\Overview.htm", overviewHTML);
        }

        static private void SetOverviewStruct(Runway runway, Params parameters, ref Overview overview)
        {
            switch (parameters.selectedScenario)
            {
                case "Circuit":
                    overview.title = "Circuit Practise";
                    overview.h1 = "Circuit Practise";
                    overview.h2Location = $"{runway.icaoName} ({runway.icaoId}) {runway.city}, {runway.country}";
                    overview.pDifficulty = "Beginner";
                    break;
                case "Photos":
                    break;
                default:
                    break;
            }
        }
    }
}
