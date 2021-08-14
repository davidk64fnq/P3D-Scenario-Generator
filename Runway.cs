using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace P3D_Scenario_Generator
{
    internal class Runway
    {
        private static readonly string xmlFilename = "runways.xml";
        private static readonly List<string[]> quadrantStringLookup = new List<string[]>();

        internal static string IcaoId { get; private set; }
        internal static string IcaoName { get; private set; }
        internal static string Country { get; private set; }
        internal static string State { get; private set; }
        internal static string City { get; private set; }
        internal static double AirportLon { get; private set; }
        internal static double AirportLat { get; private set; }
        internal static double Altitude { get; private set; }
        internal static double MagVar { get; private set; }
        internal static string Id { get; private set; }
        internal static string Number { get; private set; }
        internal static string Designator { get; private set; }
        internal static int Len { get; private set; }     // feet
        internal static double Hdg { get; private set; }  // magnetic (add magVar for true)
        internal static string Def { get; private set; }  // surface
        internal static double Lat { get; private set; }  // threshold latitude
        internal static double Lon { get; private set; }  // threshold longitude

        static internal List<string> GetICAOids()
        {
            List<string> icaoIDs = new List<string>();
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

        static internal string GetNearestAirport(double latitude, double longitude, ref double distance, ref double airportLat, ref double airportLon)
        {
            string minId = "";
            string minAirport = "";
            string currentId;
            string currentAirport;
            double minDifference = 9999;
            double minLatitude = 0;
            double minLongitude = 0;
            double difference;
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            while (reader.Read())
            {
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    currentAirport = reader.GetAttribute("id");
                    while (reader.Read())
                    {
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            currentId = reader.GetAttribute("id");
                            reader.ReadToFollowing("Lat");
                            airportLat = reader.ReadElementContentAsDouble();
                            reader.ReadToFollowing("Lon");
                            airportLon = reader.ReadElementContentAsDouble();
                            difference = MathRoutines.CalcDistance(airportLat, airportLon, latitude, longitude);
                            if (difference < minDifference)
                            {
                                minDifference = difference;
                                minLatitude = airportLat;
                                minLongitude = airportLon;
                                minId = currentId;
                                minAirport = currentAirport;
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

            distance = minDifference;
            airportLat = minLatitude;
            airportLon = minLongitude;
            return $"{minAirport}\t({minId})";
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
            quadrantStringLookup.Add(new string[] { "37", "North" });
            quadrantStringLookup.Add(new string[] { "38", "East" });
            quadrantStringLookup.Add(new string[] { "39", "NorthWest" });
            quadrantStringLookup.Add(new string[] { "40", "SouthWest" });
            quadrantStringLookup.Add(new string[] { "41", "South" });
            quadrantStringLookup.Add(new string[] { "42", "West" });
            quadrantStringLookup.Add(new string[] { "43", "SouthEast" });
            quadrantStringLookup.Add(new string[] { "44", "NorthEast" });
            quadrantStringLookup.Add(new string[] { "45", "North"});
            quadrantStringLookup.Add(new string[] { "46", "West" });
            quadrantStringLookup.Add(new string[] { "47", "NorthWest" });
            quadrantStringLookup.Add(new string[] { "48", "SouthWest" });
            quadrantStringLookup.Add(new string[] { "49", "South" });
            quadrantStringLookup.Add(new string[] { "50", "East" });
            quadrantStringLookup.Add(new string[] { "51", "SouthEast" });
            quadrantStringLookup.Add(new string[] { "52", "NorthEast" });
        }

        static internal void SetRunway()
        {
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            string[] words;
            double unused1 = 0, unused2 = 0, unused3 = 0;
            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                Random random = new Random();
                Parameters.CelestialDestRunway = GetNearestAirport(-90 + random.Next(0, 180), -180 + random.Next(0, 360), ref unused1, ref unused2, ref unused3);
                words = Parameters.CelestialDestRunway.Split("\t");
            }
            else
            {
                words = Parameters.SelectedRunway.Split("\t");
            }
            IcaoId = words[0];
            SetQuadrantStrings();

            bool runwaySet = false;
            while (reader.ReadToFollowing("ICAO") && runwaySet == false)
            {
                // Check we have located selected airport
                if (reader.MoveToAttribute("id") && reader.Value == IcaoId)
                {
                    reader.ReadToFollowing("ICAOName");
                    IcaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    Country = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("City");
                    City = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Longitude");
                    AirportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    AirportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    Altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    MagVar = reader.ReadElementContentAsDouble();

                    // Check we have located selected runway
                    do
                    {
                        reader.Read();
                    }
                    while (!(reader.Name == "Runway" && reader.MoveToAttribute("id") && $"({reader.Value})" == words[1]));
                    SetRunwayId(reader.Value);
                    reader.ReadToFollowing("Len");
                    Len = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Hdg");
                    Hdg = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Def");
                    Def = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Lat");
                    Lat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Lon");
                    Lon = reader.ReadElementContentAsDouble();

                    runwaySet = true;
                }
            }
            stream.Dispose();


            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                SetCelestialStartLocation();
            }
        }

        static private void SetCelestialStartLocation()
        {
            Random random = new Random();
            Hdg = -180 + random.Next(0, 360);

            // Position plane in random direction from destination runway between min/max distance parameter
            double randomAngle = random.Next(0, 90) * Math.PI / 180.0;
            double randomRadius = Parameters.CelestialMinDistance + random.Next(0, (int)(Parameters.CelestialMaxDistance - Parameters.CelestialMinDistance));
            double randomLatAdj = randomRadius * Math.Cos(randomAngle) * random.Next(0, 2) * 2 - 1;
            double randomLonAdj = randomRadius * Math.Sin(randomAngle) * random.Next(0, 2) * 2 - 1;
            Lat += randomLatAdj;
            if (Lat > 180)
            {
                Lat -= 360;
            }
            else if (Lat < -180)
            {
                Lat += 360;
            }
            Lon += randomLonAdj;
            if (Lon > 90)
            {
                Lon -= 180;
            }
            else if (Lon < -90)
            {
                Lon += 180;
            }
        }

        static private void SetRunwayId(string runwayId)
        {
            Id = runwayId;

            Designator = "None";                
            if (runwayId.EndsWith('L'))
            {
                Designator = "Left";
                runwayId = runwayId.TrimEnd('L');
            }
            else if (runwayId.EndsWith('R'))
            {
                Designator = "Right";
                runwayId = runwayId.TrimEnd('R');
            }
            else if (runwayId.EndsWith('C'))
            {
                Designator = "Center";
                runwayId = runwayId.TrimEnd('C');
            }
            int.TryParse(runwayId, out int number);
            if (number <= 36)
            {
                Number = runwayId.TrimStart('0');
            } 
            else if (number <= 52)
            {
                int index = quadrantStringLookup.FindIndex(quad => quad[0] == number.ToString());
                Number = quadrantStringLookup[index][1];
            }
            else
            {
                Number = "0";
            }
        }
    }
}
