using System;
using System.Collections.Generic;
using System.Xml;

namespace P3D_Scenario_Generator
{
    internal class Runway
    {

        private static readonly string xmlFilename = "runways.xml";

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
        internal static int Len { get; private set; }     // feet
        internal static double Hdg { get; private set; }  // magnetic (add magVar for true)
        internal static string Def { get; private set; }  // surface
        internal static double Lat { get; private set; }  // threshold latitude
        internal static double Lon { get; private set; }  // threshold longitude

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

        static public void SetRunway()
        {
            XmlReader reader = XmlReader.Create(xmlFilename);
            string[] words = Parameters.SelectedRunway.Split("\t");
            IcaoId = words[0];

            Boolean runwaySet = false;
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
                    Id = reader.Value;
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
        }
    }
}
