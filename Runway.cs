﻿using System;
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

        static public List<string> GetICAOids()
        {
            List<string> icaoIDs = new List<string>();
            Stream stream;
            XmlReader reader;

            if (File.Exists(xmlFilename))
            {
                stream = new MemoryStream(File.ReadAllBytes(xmlFilename));
                reader = XmlReader.Create(stream);
            }
            else
            {
                stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.{xmlFilename}");
                reader = XmlReader.Create(stream);
            }
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
            stream.Dispose();

            return icaoIDs;
        }

        static public void SetRunway()
        {
            Stream stream;
            XmlReader reader;
            if (File.Exists(xmlFilename))
            {
                stream = new MemoryStream(File.ReadAllBytes(xmlFilename));
                reader = XmlReader.Create(stream);
            }
            else
            {
                stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.{xmlFilename}");
                reader = XmlReader.Create(stream);
            }
            string[] words = Parameters.SelectedRunway.Split("\t");
            IcaoId = words[0];
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
                Number = runwayId;
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