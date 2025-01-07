using System.Xml;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Parameters for a runway sourced from the runways.xml file
    /// </summary>
    public class RunwayParams
    {
        /// <summary>
        /// Four letter code known as ICAO airport code or location indicator
        /// </summary>
        internal string IcaoId { get; set; }

        /// <summary>
        /// The name of the airport
        /// </summary>
        internal string IcaoName { get; set; }

        internal string Country { get; set; }

        internal string State { get; set; }

        internal string City { get; set; }

        /// <summary>
        /// The longitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLon { get; set; }

        /// <summary>
        /// The latitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLat { get; set; }

        /// <summary>
        /// Airport altitude (AMSL)
        /// </summary>
        internal double Altitude { get; set; }

        /// <summary>
        /// Airport magnetic variation
        /// </summary>
        internal double MagVar { get; set; }

        /// <summary>
        /// The runway Id e.g. "05L", multiply two digit number less than equal to 36 by 10 to get 0 to 360 degrees runway approx magnetic 
        /// heading if the number is greater than 36 it is code for a compass heading or pair of compass headings e.g. 37 = "N-S", 45 = "N"
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// The compass bearing of an airport runway with no leading zeros e.g. "5" is 5 degrees. Or a compass
        /// direction string e.g. "NorthWest"
        /// </summary>
        internal string Number { get; set; }

        /// <summary>
        /// One of "None", "Left", "Right", or "Center". Used in setting the airport landing trigger for a scenario
        /// </summary>
        internal string Designator { get; set; }

        /// <summary>
        /// Runway length in feet
        /// </summary>
        internal int Len { get; set; }

        /// <summary>
        /// Runway magnetic heading (add magVar for true)
        /// </summary>
        internal double Hdg { get; set; } 
        
        /// <summary>
        /// Runway surface material
        /// </summary>
        internal string Def { get; set; }

        /// <summary>
        /// Runway threshold latitude
        /// </summary>
        internal double ThresholdStartLat { get; set; }

        /// <summary>
        /// Runway threshold longitude
        /// </summary>
        internal double ThresholdStartLon { get; set; }   

        /// <summary>
        /// Index of runway in <see cref="Runway.Runways"
        /// </summary>
        internal int RunwaysIndex { get; set; }
    }

    /// <summary>
    /// Stores runway ids in range 37 to 52 which represent compass headings rather than usual degrees to nearest ten
    /// </summary>
    /// <param name="v1">The code - a number from 37 to 52 inclusive</param>
    /// <param name="v2">The fullname e.g. "NorthWest"</param>
    /// <param name="v3">The abbreviated name e.g. "NW"</param>
    public class RunwayCompassIds(string v1, string v2, string v3)
    {

        /// <summary>
        /// The code - a number from 37 to 52 inclusive
        /// </summary>
        internal string Code { get; set; } = v1;

        /// <summary>
        /// The fullname e.g. "NorthWest"
        /// </summary>
        internal string FullName { get; set; } = v2;

        /// <summary>
        /// The abbreviated name e.g. "NW"
        /// </summary>
        internal string AbbrName { get; set; } = v3;
    }

    internal class Runway
    {
        private static readonly string xmlFilename = "runways.xml";

        internal static List<RunwayParams> Runways { get; set; }

        /// <summary>
        /// Used to store runway headings that are compass direction strings rather than 01 to 36, e.g. "NorthWest"
        /// </summary>
        private static readonly List<RunwayCompassIds> runwayCompassIds = [];

        /// <summary>
        /// The scenario start runway
        /// </summary>
        internal static RunwayParams startRwy = new();

        /// <summary>
        /// The scenario destination runway
        /// </summary>
        internal static RunwayParams destRwy = new();

        /// <summary>
        /// Read in all the runways from runways.xml when the application loads
        /// </summary>
        internal static void GetRunways()
        {
            Runways = [];
            RunwayParams curAirport = new();
            using Stream stream = GetRunwayXMLstream();
            using XmlReader reader = XmlReader.Create(stream);
            SetRunwayCompassIds();
            int curIndex = 0;
            while (reader.Read())
            {
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    curAirport.IcaoId = reader.GetAttribute("id");
                    reader.ReadToFollowing("ICAOName");
                    curAirport.IcaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    curAirport.Country = reader.ReadElementContentAsString();
                    // State not always present betweeen Country and City elements
                    reader.Read();
                    if (reader.Name == "State")
                    {
                        curAirport.State = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("City");
                        curAirport.City = reader.ReadElementContentAsString();
                    }
                    else
                    {
                        curAirport.City = reader.ReadElementContentAsString();
                    }
                    reader.ReadToFollowing("Longitude");
                    curAirport.AirportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    curAirport.AirportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    curAirport.Altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    curAirport.MagVar = reader.ReadElementContentAsDouble();
                    while (reader.Read())
                    {
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            SetRunwayId(curAirport, reader.GetAttribute("id"));
                            reader.ReadToFollowing("Len");
                            curAirport.Len = reader.ReadElementContentAsInt();
                            reader.ReadToFollowing("Hdg");
                            curAirport.Hdg = reader.ReadElementContentAsDouble();
                            reader.ReadToFollowing("Def");
                            curAirport.Def = reader.ReadElementContentAsString();
                            reader.ReadToFollowing("Lat");
                            curAirport.ThresholdStartLat = reader.ReadElementContentAsDouble();
                            reader.ReadToFollowing("Lon");
                            curAirport.ThresholdStartLon = reader.ReadElementContentAsDouble();
                        }
                        if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (curAirport.Id != null)
                            {
                                curAirport.RunwaysIndex = curIndex;
                                curIndex++;
                                Runways.Add(curAirport);
                            }
                            curAirport = new();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds list of strings in format "Airport ICAO (Runway Id)" e.g. "LFGO (14L)" used to populate
        /// available runway list in General tab of application form.
        /// </summary>
        /// <returns>The list of runway strings</returns>
        static internal List<string> GetICAOids()
        {
            List<string> icaoIDs = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                icaoIDs.Add($@"{Runways[i].IcaoId} ({Runways[i].Id})");
            }
            return icaoIDs;
        }

        /// <summary>
        /// Search through the list of runways, starting from a random runway in list. Return the first runway
        /// located that is between a minimum and maximum distance from the provided reference coordinate
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <param name="minDist">The minimum distance runway can be from reference coordinate</param>
        /// <param name="maxDist">The maximum distance runway can be from reference coordinate</param>
        /// <returns>The runway that meets the minimum and maximum distance requirements</returns>
        static internal RunwayParams GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            double curDistance;
            Random r = new();
            int curIndex = r.Next(0, Runways.Count);
            for (int i = 0; i < Runways.Count; i++)
            {
                curDistance = MathRoutines.CalcDistance(Runways[curIndex].AirportLat, Runways[curIndex].AirportLon, queryLat, queryLon);
                if ((curDistance < maxDist) && (curDistance > minDist))
                {
                    return Runways[curIndex];
                }
                curIndex++;
                if (curIndex == Runways.Count)
                {
                    curIndex = 0;
                }
            }
            return null;
        }

        /// <summary>
        /// Search through the list of runways. Returns the nearest runway to the provided reference coordinate.
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <returns>The nearest runway to the provided reference cooordinate</returns>
        static internal RunwayParams GetNearestAirport(double queryLat, double queryLon)
        {
            RunwayParams minAirport = new();
            double minDistance = 9999;
            double curDistance;
            for (int curIndex = 0; curIndex < Runways.Count; curIndex++)
            {
                curDistance = MathRoutines.CalcDistance(Runways[curIndex].AirportLat, Runways[curIndex].AirportLon, queryLat, queryLon);
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    minAirport = Runways[curIndex];
                }
            }
            return minAirport;
        }

        /// <summary>
        /// The list of runways is in "runway.xml" which is an embedded resource but the user can create a local version
        /// to reflect airports installed additional to default P3D v5
        /// </summary>
        /// <returns>
        /// A stream containing runways.xml</returns>
        static private Stream GetRunwayXMLstream()
        {
            Stream stream;

            if (File.Exists(xmlFilename))
            {
                stream = new MemoryStream(File.ReadAllBytes(xmlFilename));
            }
            else
            {
                stream = Form.GetResourceStream($"XML.{xmlFilename}");
            }
            return stream;
        }

        /// <summary>
        /// Store runway ids in range 37 to 52 which represent compass headings rather than usual degrees to nearest ten
        /// </summary>
        static private void SetRunwayCompassIds()
        {
            runwayCompassIds.Add(new("37", "North", "N-S"));
            runwayCompassIds.Add(new("38", "East", "E-W"));
            runwayCompassIds.Add(new("39", "NorthWest", "NW-SE"));
            runwayCompassIds.Add(new("40", "SouthWest", "SW-NE"));
            runwayCompassIds.Add(new("41", "South", "S-N"));
            runwayCompassIds.Add(new("42", "West", "W-E"));
            runwayCompassIds.Add(new("43", "SouthEast", "SE-NW"));
            runwayCompassIds.Add(new("44", "NorthEast", "NE-SW"));
            runwayCompassIds.Add(new("45", "North", "N"));
            runwayCompassIds.Add(new("46", "West",  "W"));
            runwayCompassIds.Add(new("47", "NorthWest", "NW"));
            runwayCompassIds.Add(new("48", "SouthWest", "SW"));
            runwayCompassIds.Add(new("49", "South", "S"));
            runwayCompassIds.Add(new("50", "East", "E"));
            runwayCompassIds.Add(new("51", "SouthEast", "SE"));
            runwayCompassIds.Add(new("52", "NorthEast", "NE"));
        }

        // Delete once celestial reworked
        static internal string[] SetICAOwords(string rwyType)
        {
            string[] words = ["", ""];
            RunwayParams airport;
            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                case nameof(ScenarioTypes.SignWriting):
            //        words = Parameters.SelectedRunway.Split("\t");
                    break;
                case nameof(ScenarioTypes.PhotoTour):
            //        if (rwyType == "start")
            //            words = Parameters.SelectedRunway.Split("\t");
            //        else
            //            words = Parameters.PhotoDestRunway.Split("\t");
                    break;
                case nameof(ScenarioTypes.Celestial):
                    if (rwyType == "destination")
                    {
                        Random random = new();
                        airport = GetNearestAirport(-60 + random.Next(0, 120), -180 + random.Next(0, 360));
                        Parameters.CelestialDestRunway = $"{airport.IcaoId}\t({airport.Id})";
                        words = Parameters.CelestialDestRunway.Split("\t");
                    }
                    break;
                case nameof(ScenarioTypes.WikiList):
                    if (rwyType == "start")
                    {
                        words[0] = Wikipedia.WikiTour[0].airportICAO;
                        words[1] = Wikipedia.WikiTour[0].airportID;
                    }
                    else
                    {
                        words[0] = Wikipedia.WikiTour[^1].airportICAO;
                        words[1] = Wikipedia.WikiTour[^1].airportID;
                    }
                    break;
                default:
                    break;
            }
            words[1] = words[1].Trim('(');
            words[1] = words[1].Trim(')');
            return words;
        }

        // Delete once celestial reworked
        static internal void SetRunway(RunwayParams rwyParams, string airportICAO, string airportID)
        {
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            SetRunwayCompassIds();

            bool runwaySet = false;
            while (reader.ReadToFollowing("ICAO") && runwaySet == false)
            {
                // Check we have located selected airport
                if (reader.MoveToAttribute("id") && reader.Value == airportICAO)
                {
                    rwyParams.IcaoId = reader.Value;
                    reader.ReadToFollowing("ICAOName");
                    rwyParams.IcaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    rwyParams.Country = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("City");
                    rwyParams.City = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Longitude");
                    rwyParams.AirportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    rwyParams.AirportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    rwyParams.Altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    rwyParams.MagVar = reader.ReadElementContentAsDouble();

                    // Check we have located selected runway
                    do
                    {
                        reader.Read();
                    }
                    while (!(reader.Name == "Runway" && reader.MoveToAttribute("id") && reader.Value == airportID));
                    SetRunwayId(rwyParams, reader.Value);
                    reader.ReadToFollowing("Len");
                    rwyParams.Len = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Hdg");
                    rwyParams.Hdg = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Def");
                    rwyParams.Def = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Lat");
                    rwyParams.ThresholdStartLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Lon");
                    rwyParams.ThresholdStartLon = reader.ReadElementContentAsDouble();

                    if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
                    {
                        CelestialNav.destinationLat = rwyParams.AirportLat;
                        CelestialNav.destinationLon = rwyParams.AirportLon;
                    }

                    runwaySet = true;
                }
            }
            stream.Dispose();


            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                CelestialNav.SetCelestialStartLocation();
                BingImages.GetCelestialOverviewImage();
            }
        }

        /// <summary>
        /// Takes a runway Id and extracts the alphabetic code letter if present and the runway number which is sometimes
        /// a string. E.g. "23L" is "Left" plus "23", "45" is "North", "32W" is "Water" plus "32"
        /// </summary>
        /// <param name="rwyParams">Where the runway Id, designator and number are stored</param>
        /// <param name="runwayId">The runway Id string to be processed</param>
        static private void SetRunwayId(RunwayParams rwyParams, string runwayId)
        {
            rwyParams.Id = runwayId;

            rwyParams.Designator = "None";                
            if (runwayId.EndsWith('L'))
            {
                rwyParams.Designator = "Left";
                runwayId = runwayId.TrimEnd('L');
            }
            else if (runwayId.EndsWith('R'))
            {
                rwyParams.Designator = "Right";
                runwayId = runwayId.TrimEnd('R');
            }
            else if (runwayId.EndsWith('C'))
            {
                rwyParams.Designator = "Center";
                runwayId = runwayId.TrimEnd('C');
            }
            else if (runwayId.EndsWith('W'))
            {
                rwyParams.Designator = "Water";
                runwayId = runwayId.TrimEnd('W');
            }
            if (int.TryParse(runwayId, out int runwayNumber))
                if (runwayNumber <= 36)
                {
                    rwyParams.Number = runwayId.TrimStart('0');
                } 
                else if (runwayNumber <= 52)
                {
                    RunwayCompassIds runwayCompassId = runwayCompassIds.Find(rcID => rcID.Code == runwayId);
                    rwyParams.Number = runwayCompassId.FullName;
                }
                else
                {
                    rwyParams.Number = "0";
                }
        }
    }
}
