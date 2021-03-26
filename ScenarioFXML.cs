
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace P3D_Scenario_Generator
{
	public class ScenarioFXML
    {
        static internal void GenerateFXMLfile(RunwayStruct runway, string saveLocationPath, string aircraftTitle)
		{
			SimBaseDocument simBaseDocument = ReadSourceFXML();
			EditSourceFXML(runway, simBaseDocument, saveLocationPath, aircraftTitle);
			WriteSourceFXML(simBaseDocument, saveLocationPath);
        }

		static private SimBaseDocument ReadSourceFXML()
        {
			XmlSerializer serializer = new XmlSerializer(typeof(SimBaseDocument));
			string xml = File.ReadAllText("source.fxml");
            using StringReader reader = new StringReader(xml);
            return (SimBaseDocument)serializer.Deserialize(reader);
		}

		static private void EditSourceFXML(RunwayStruct runway, SimBaseDocument simBaseDocument, string saveLocationPath, string aircraftTitle)
		{
			FlightSections fs = new FlightSections();
			fs = simBaseDocument.FlightSections;

			// Main section
			int sectionIndex = fs.Section.FindIndex(s => s.Name == "Main");
			int propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Title");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Path.GetFileNameWithoutExtension(saveLocationPath)}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Description");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Constants.appTitle} - Circuit";

			// Options section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Options");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Pause");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";

			// DateTimeSeason section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "DateTimeSeason");
			DateTime now = DateTime.Now;
			var persianMonth = new PersianCalendar().GetMonth(DateTime.Now);
			var season = (Season)Math.Ceiling(persianMonth / 3.0);
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Season");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{season}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Year");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{now.Year}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Day");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{now.DayOfYear}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Hours");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{now.Hour}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Minutes");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{now.Minute}";

			// Sim.0 section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Sim.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Sim");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{aircraftTitle}";

            // Simvars.0 section
            sectionIndex = fs.Section.FindIndex(s => s.Name == "SimVars.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Heading");
			// Convert format of runway heading from magnetic North nearest degree to plus/minus 180 degrees true North
			double convertHdg = runway.hdg + runway.magVar;
			if (convertHdg > 180)
            {
				convertHdg -= 360;
            }
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{convertHdg}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Latitude");
			string formattedLatitude = FormatCoordXML(runway.lat, "N", "S");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLatitude}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Longitude");
			string formattedLongitude = FormatCoordXML(runway.lon, "E", "W");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLongitude}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Altitude");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "+0";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "SimOnGround");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";
		}

		static string FormatCoordXML(double dCoord, string sPosDir, string sNegDir)
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
			sCoordLine += $"{dSeconds}\"";

			return sCoordLine;
		}

		static private void WriteSourceFXML(SimBaseDocument simBaseDocument, string saveLocationPath)
        {
			XmlSerializer xmlSerializer = new XmlSerializer(simBaseDocument.GetType());

            using StreamWriter writer = new StreamWriter(saveLocationPath);
            xmlSerializer.Serialize(writer, simBaseDocument);
        }

	}

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
}
