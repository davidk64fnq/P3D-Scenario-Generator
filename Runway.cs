using System.Reflection;
using System.Xml;

namespace P3D_Scenario_Generator
{
    public class Params
    {
        internal string IcaoId { get; set; }
        internal string IcaoName { get; set; }
        internal string Country { get; set; }
        internal string State { get; set; }
        internal string City { get; set; }
        internal double AirportLon { get; set; }
        internal double AirportLat { get; set; }
        internal double Altitude { get; set; }
        internal double MagVar { get; set; }
        internal string Id { get; set; }
        internal string Number { get; set; }
        internal string Designator { get; set; }
        internal int Len { get; set; }     // feet
        internal double Hdg { get; set; }  // magnetic (add magVar for true)
        internal string Def { get; set; }  // surface
        internal double ThresholdStartLat { get; set; }  // threshold latitude 
        internal double ThresholdStartLon { get; set; }  // threshold longitude 
    }

    internal class Runway
    {
        private static readonly string xmlFilename = "runways.xml";
        private static readonly List<string[]> quadrantStringLookup = [];

        internal static Params startRwy = new();
        internal static Params destRwy = new();

        static internal List<string> GetICAOids()
        {
            List<string> icaoIDs = [];
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    string currentAirport = reader.GetAttribute("id");
                    while (reader.Read())
                    {
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            icaoIDs.Add($"{currentAirport}\t({reader.GetAttribute("id")})");
                        }
                        if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                }
            }
            stream.Dispose();

            return icaoIDs;
        }

        static internal Params GetNearbyAirport(double queryLat, double queryLon, double minDist, double maxDist)
        {
            double curDistance;
            Params curAirport = new();
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    curAirport.IcaoId = reader.GetAttribute("id");
                    while (reader.Read())
                    {
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            curAirport.Id = reader.GetAttribute("id");
                            reader.ReadToFollowing("Lat");
                            curAirport.AirportLat = reader.ReadElementContentAsDouble();
                            reader.ReadToFollowing("Lon");
                            curAirport.AirportLon = reader.ReadElementContentAsDouble();
                            curDistance = MathRoutines.CalcDistance(curAirport.AirportLat, curAirport.AirportLon, queryLat, queryLon);
                            if ((curDistance < maxDist) && (curDistance > minDist))
                            {
                                return curAirport;
                            }
                        }
                        if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                }
            }
            stream.Dispose();

            return null;
        }

        static internal Params GetNearestAirport(double queryLat, double queryLon)
        {
            double minDistance = 9999;
            double curDistance;
            Params minAirport = new();
            Params curAirport = new();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    curAirport.IcaoId = reader.GetAttribute("id");
                    while (reader.Read())
                    {
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            curAirport.Id = reader.GetAttribute("id");
                            reader.ReadToFollowing("Lat");
                            curAirport.AirportLat = reader.ReadElementContentAsDouble();
                            reader.ReadToFollowing("Lon");
                            curAirport.AirportLon = reader.ReadElementContentAsDouble();
                            curDistance = MathRoutines.CalcDistance(curAirport.AirportLat, curAirport.AirportLon, queryLat, queryLon);
                            if (curDistance < minDistance)
                            {
                                minDistance = curDistance;
                                minAirport.AirportLat = curAirport.AirportLat;
                                minAirport.AirportLon = curAirport.AirportLon;
                                minAirport.Id = curAirport.Id;
                                minAirport.IcaoId = curAirport.IcaoId;
                            }
                        }
                        if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                    }
                }
            }
            stream.Dispose();

            return minAirport;
        }

        static private Stream GetRunwayXMLstream()
        {
            Stream stream;

            if (File.Exists(xmlFilename))
            {
                stream = new MemoryStream(File.ReadAllBytes(xmlFilename));
            }
            else
            {
                stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.XML.{xmlFilename}");
            }
            return stream;
        }

        static private void SetQuadrantStrings()
        {
            quadrantStringLookup.Add(["37", "North"]);
            quadrantStringLookup.Add(["38", "East"]);
            quadrantStringLookup.Add(["39", "NorthWest"]);
            quadrantStringLookup.Add(["40", "SouthWest"]);
            quadrantStringLookup.Add(["41", "South"]);
            quadrantStringLookup.Add(["42", "West"]);
            quadrantStringLookup.Add(["43", "SouthEast"]);
            quadrantStringLookup.Add(["44", "NorthEast"]);
            quadrantStringLookup.Add(["45", "North"]);
            quadrantStringLookup.Add(["46", "West"]);
            quadrantStringLookup.Add(["47", "NorthWest"]);
            quadrantStringLookup.Add(["48", "SouthWest"]);
            quadrantStringLookup.Add(["49", "South"]);
            quadrantStringLookup.Add(["50", "East"]);
            quadrantStringLookup.Add(["51", "SouthEast"]);
            quadrantStringLookup.Add(["52", "NorthEast"]);
        }

        static internal string[] SetICAOwords(string rwyType)
        {
            string[] words = ["", ""];
            Params airport;
            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                case nameof(ScenarioTypes.SignWriting):
                    words = Parameters.SelectedRunway.Split("\t");
                    break;
                case nameof(ScenarioTypes.PhotoTour):
                    if (rwyType == "start")
                        words = Parameters.SelectedRunway.Split("\t");
                    else
                        words = Parameters.PhotoDestRunway.Split("\t");
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
                        words[0] = WikiList.WikiStartAirport.IcaoId;
                        words[1] = WikiList.WikiStartAirport.Id;
                    }
                    else
                    {
                        words[0] = WikiList.WikiFinishAirport.IcaoId;
                        words[1] = WikiList.WikiFinishAirport.Id;
                    }
                    break;
                default:
                    break;
            }
            return words;
        }

        static internal void SetRunway(Params rwyParams, string rwyType)
        {
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            string[] icaoWords = SetICAOwords(rwyType);
            if (icaoWords.Length > 0)
                rwyParams.IcaoId = icaoWords[0];
            else
                return;
            SetQuadrantStrings();

            bool runwaySet = false;
            while (reader.ReadToFollowing("ICAO") && runwaySet == false)
            {
                // Check we have located selected airport
                if (reader.MoveToAttribute("id") && reader.Value == rwyParams.IcaoId)
                {
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
                    while (!(reader.Name == "Runway" && reader.MoveToAttribute("id") && reader.Value == icaoWords[1]));
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

        static private void SetRunwayId(Params rwyParams, string runwayId)
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
            if (int.TryParse(runwayId, out int number))
                if (number <= 36)
                {
                    rwyParams.Number = runwayId.TrimStart('0');
                } 
                else if (number <= 52)
                {
                    int index = quadrantStringLookup.FindIndex(quad => quad[0] == number.ToString());
                    rwyParams.Number = quadrantStringLookup[index][1];
                }
                else
                {
                    rwyParams.Number = "0";
                }
        }
    }
}
