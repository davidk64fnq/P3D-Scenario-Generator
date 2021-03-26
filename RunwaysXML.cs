using System;
using System.Collections.Generic;
using System.Xml;

namespace P3D_Scenario_Generator
{
    internal class RunwaysXML
    {

        private static readonly string xmlFilename = "runways.xml";

        static public List<string> GetICAOids()
        {
            List<string> icaoIDs = new List<string>();

            XmlReader reader = XmlReader.Create(xmlFilename);
            while (reader.ReadToFollowing("ICAO") && reader.NodeType == XmlNodeType.Element)
            {
                reader.MoveToAttribute("id");
                string currentAirport = reader.Value;
                do
                {
                    reader.Read();
                    if (reader.Name == "Runway" && reader.MoveToAttribute("id"))
                    {
                        icaoIDs.Add($"{currentAirport}\t({reader.Value})");
                    }
                }
                while (reader.Name != "ICAO");
            }

            return icaoIDs;
        }

        static public void SetRunway(string selectedRunway, ref RunwayStruct runway)
        {
            XmlReader reader = XmlReader.Create(xmlFilename);
            string[] words = selectedRunway.Split("\t");
            runway.icaoId = words[0];

            Boolean runwaySet = false;
            while (reader.ReadToFollowing("ICAO") && runwaySet == false)
            {
                // Check we have located selected airport
                if (reader.MoveToAttribute("id") && reader.Value == runway.icaoId)
                {
                    reader.ReadToFollowing("ICAOName");
                    runway.icaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    runway.country = reader.ReadElementContentAsString();
                    //runway.state = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("City");
                    runway.city = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Longitude");
                    runway.airportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    runway.airportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    runway.altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    runway.magVar = reader.ReadElementContentAsDouble();

                    // Check we have located selected runway
                    do
                    {
                        reader.Read();
                    }
                    while (!(reader.Name == "Runway" && reader.MoveToAttribute("id") && $"({reader.Value})" == words[1]));
                    runway.id = reader.Value;
                    reader.ReadToFollowing("Len");
                    runway.len = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Hdg");
                    runway.hdg = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Def");
                    runway.def = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Lat");
                    runway.lat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Lon");
                    runway.lon = reader.ReadElementContentAsDouble();

                    runwaySet = true;
                }
            }
        }
    }
}
