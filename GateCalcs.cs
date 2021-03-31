using System;
using System.Collections.Generic;
using System.Text;

namespace P3D_Scenario_Generator
{
    public struct LegParams
    {
        double heading;
        double distance;

        public LegParams(double d1, double d2)
        {
            heading = d1;
            distance = d2;
        }
    }

    internal class GateCalcs
    {
        static public void SetGatePositions(Runway runway, Params parameters, List<Gate> gates)
        {
            // Turn radius approx speed/180, then use pythagorus to get turn distance
            double turnDistance = parameters.speed / 180 / Math.Sin(45 * Math.PI / 180);

            List<LegParams> legParams = new List<LegParams>();
            switch (parameters.selectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                    legParams.Add(new LegParams(runway.hdg + runway.magVar, (runway.len / 2) + (parameters.upwindLeg / Constants.feetInKnot)));
                    break;
                default:
                    break;
            }
        }
    }
}
