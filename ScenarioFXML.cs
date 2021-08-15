
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using System.Reflection;

namespace P3D_Scenario_Generator
{
	public class ScenarioFXML
	{
		private static readonly string fxmlFilename = "source.fxml";

		static internal void GenerateFXMLfile()
		{
			SimBaseDocument simBaseDocument = ReadSourceFXML();
			EditSourceFXML(simBaseDocument);
			WriteSourceFXML(simBaseDocument);
        }

		static private SimBaseDocument ReadSourceFXML()
        {
			XmlSerializer serializer = new XmlSerializer(typeof(SimBaseDocument));
			Stream stream = Assembly.Load(Assembly.GetExecutingAssembly().GetName().Name).GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "_")}.Resources.XML.{fxmlFilename}");
			SimBaseDocument simBaseDocument = (SimBaseDocument)serializer.Deserialize(stream);
			stream.Dispose();
			return simBaseDocument;
		}

		static private void EditSourceFXML(SimBaseDocument simBaseDocument)
		{
			FlightSections fs;
			fs = simBaseDocument.FlightSections;

			// Main section
			int sectionIndex = fs.Section.FindIndex(s => s.Name == "Main");
			int propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Title");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Path.GetFileNameWithoutExtension(Parameters.SaveLocation)}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Description");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Constants.appTitle} - {Parameters.SelectedScenario}";

			// Options section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Options");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Pause");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";

			// DateTimeSeason section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "DateTimeSeason");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Season");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.Season}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Year");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.Year}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Day");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.DayOfYear}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Hours");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.Hours}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Minutes");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.Minutes}";

			// Sim.0 section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Sim.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Sim");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Parameters.SelectedAircraft}";

            // Simvars.0 section
            sectionIndex = fs.Section.FindIndex(s => s.Name == "SimVars.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Heading");
			// Convert format of runway heading from magnetic North nearest degree to plus/minus 180 degrees true North
			double convertHdg = Runway.Hdg + Runway.MagVar;
			if (convertHdg > 180)
            {
				convertHdg -= 360;
            }
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{convertHdg}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Latitude");
			string formattedLatitude = FormatCoordXML(Runway.Lat, "N", "S", false);
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLatitude}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Longitude");
			string formattedLongitude = FormatCoordXML(Runway.Lon, "E", "W", false);
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLongitude}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Altitude");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "+0";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "SimOnGround");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";

			// ObjectFile section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "ObjectFile");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "File");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Path.GetFileNameWithoutExtension(Parameters.SaveLocation)}";

			if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
				EditCelestialSourceFXML(simBaseDocument);

			}
		}

		// Celestial scenario starts in air
		static private void EditCelestialSourceFXML(SimBaseDocument simBaseDocument)
		{
			FlightSections fs;
			fs = simBaseDocument.FlightSections;

			// Simvars.0 section
			int sectionIndex = fs.Section.FindIndex(s => s.Name == "SimVars.0");
			int propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Altitude");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "+3000";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "SimOnGround");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "False";

			// Engine Parameters section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Engine Parameters.1.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Combustion");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "ThrottleLeverPct");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "1";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "PropellerLeverPct");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "1";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "MixtureLeverPct");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "1";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Pct Engine RPM");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "0.25";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "LeftMagneto");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "RightMagneto");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "GeneratorSwitch");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";
		}

		static public string FormatCoordXML(double dCoord, string sPosDir, string sNegDir, bool roundSeconds)
        {
			double dDecimalPart, dMinutes, dSeconds, dLocalCoord;
			string sCoordLine = "";

			if (dCoord > 0)
            {
				sCoordLine += sPosDir;
				dLocalCoord = dCoord;
            }
            else
            {
				sCoordLine += sNegDir;
				dLocalCoord = dCoord * -1;
            }
			// Degrees is integer part of raw coord
			sCoordLine += $"{(int)dLocalCoord}° ";
			// Minutes is decimal part of raw coord times 60
			dDecimalPart = dLocalCoord - (int)dLocalCoord;
			dMinutes = dDecimalPart * 60;
			sCoordLine += $"{(int)dMinutes}' ";
			// Seconds is decimal part of minutes times 60
			dDecimalPart = dMinutes - (int)dMinutes;
			dSeconds = dDecimalPart * 60;
			// Round seconds
			if (roundSeconds)
            {
				dSeconds = Math.Round(dSeconds);
				if (dSeconds > 60)
				{
					dSeconds = 0;
				}
            }
			sCoordLine += $"{dSeconds}\"";

			return sCoordLine;
		}

		static private void WriteSourceFXML(SimBaseDocument simBaseDocument)
        {
			XmlSerializer xmlSerializer = new XmlSerializer(simBaseDocument.GetType());

            using StreamWriter writer = new StreamWriter(Parameters.SaveLocation);
            xmlSerializer.Serialize(writer, simBaseDocument);
        }

	}

    #region Simbase.Document class definitions

    [XmlRoot(ElementName = "Property")]
	public class Property
	{

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "Value")]
		public string Value { get; set; }
	}

	[XmlRoot(ElementName = "Section")]
	public class Section
	{

		[XmlElement(ElementName = "Property")]
		public List<Property> Property { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
	}

	[XmlRoot(ElementName = "Flight.Sections")]
	public class FlightSections
	{

		[XmlElement(ElementName = "Section")]
		public List<Section> Section { get; set; }
	}

	[XmlRoot(ElementName = "SimBase.Document")]
	public class SimBaseDocument
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Filename")]
		public string Filename { get; set; }

		[XmlElement(ElementName = "Flight.Sections")]
		public FlightSections FlightSections { get; set; }

		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public double Version { get; set; }

		[XmlAttribute(AttributeName = "id")]
		public string Id { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

    #endregion
}
