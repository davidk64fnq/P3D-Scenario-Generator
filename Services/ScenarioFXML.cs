using P3D_Scenario_Generator.ConstantsEnums;
using P3D_Scenario_Generator.Utilities;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Services
{
    public class ScenarioFXML(FileOps fileOps, FormProgressReporter progressReporter)
    {
        private readonly FileOps _fileOps = fileOps ?? throw new ArgumentNullException(nameof(fileOps));
        private readonly FormProgressReporter _progressReporter = progressReporter ?? throw new ArgumentNullException(nameof(progressReporter));

        private static readonly string fxmlFilename = "source.fxml";
        private static object formattedLatitude;
        private static object formattedLongitude;

        public async Task GenerateFXMLfileAsync(ScenarioFormData formData)
		{
			(bool success, SimBaseDocument simBaseDocument) = await TryReadSourceFXMLAsync(_progressReporter);
			if (!success)
			{
				return;
			}
            EditSourceFXML(simBaseDocument, formData);
            WriteSourceFXML(simBaseDocument, formData);
        }

        /// <summary>
        /// Attempts to read and deserialize a <see cref="SimBaseDocument"/> from its embedded resource XML file.
        /// </summary>
        /// <param name="progressReporter">IProgress<string> for reporting progress or errors to the UI.</param>
        /// <returns><see langword="true"/> and simBaseDocument if the document was successfully deserialized; otherwise, <see langword="false"/>.</returns>
        public async Task<(bool success, SimBaseDocument simBaseDocument)> TryReadSourceFXMLAsync(IProgress<string> progressReporter)
        {
            string resourceName = $"XML.{fxmlFilename}";

			(bool success, SimBaseDocument simBaseDocument) = await _fileOps.TryDeserializeXmlFromResourceAsync<SimBaseDocument>(resourceName, progressReporter);
			if (!success)
			{
				return (false, null);
			}
            return (true, simBaseDocument);
        }

		static private void EditSourceFXML(SimBaseDocument simBaseDocument, ScenarioFormData formData)
		{
			FlightSections fs;
			fs = simBaseDocument.FlightSections;

			// Main section
			int sectionIndex = fs.Section.FindIndex(s => s.Name == "Main");
			int propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Title");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.ScenarioTitle}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Description");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{Constants.appTitle} - {formData.ScenarioType}";

			// DateTimeSeason section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "DateTimeSeason");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Season");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.Season}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Year");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.DatePickerValue.Year}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Day");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.DatePickerValue.DayOfYear}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Hours");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.TimePickerValue.Hour}";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Minutes");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.TimePickerValue.Minute}";

			// Sim.0 section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "Sim.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Sim");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.AircraftSimValue}";

            // Simvars.0 section
            sectionIndex = fs.Section.FindIndex(s => s.Name == "SimVars.0");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Heading");

            // The runway object to use for calculations. Use StartRunway if available, otherwise use DestinationRunway.
            var selectedRunway = formData.StartRunway ?? formData.DestinationRunway;

            // Convert format of runway heading from magnetic North nearest degree to plus/minus 180 degrees true North
            double absTrueHdg = selectedRunway.Hdg + selectedRunway.MagVar;
            fs.Section[sectionIndex].Property[propertyIndex].Value = $"{MathRoutines.ConvertHeadingAbsoluteToRelative(absTrueHdg)}";

            propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Latitude");
            formattedLatitude = FormatCoordXML(selectedRunway.ThresholdStartLat, "N", "S", false);
            fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLatitude}";
            propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Longitude");
            formattedLongitude = FormatCoordXML(selectedRunway.ThresholdStartLon, "E", "W", false);
            fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formattedLongitude}";
            propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "Altitude");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "+0";
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "SimOnGround");
			fs.Section[sectionIndex].Property[propertyIndex].Value = "True";

			// ObjectFile section
			sectionIndex = fs.Section.FindIndex(s => s.Name == "ObjectFile");
			propertyIndex = fs.Section[sectionIndex].Property.FindIndex(p => p.Name == "File");
			fs.Section[sectionIndex].Property[propertyIndex].Value = $"{formData.ScenarioTitle}";
		}

        static public string FormatCoordXML(double dCoord, string sPosDir, string sNegDir, bool roundSeconds)
        {
            string sDirection = dCoord >= 0 ? sPosDir : sNegDir;
            double absCoord = Math.Abs(dCoord);

            int degrees = (int)absCoord;
            double totalMinutes = (absCoord - degrees) * 60.0;
            int minutes = (int)totalMinutes;
            double seconds = (totalMinutes - minutes) * 60.0;

            if (roundSeconds)
            {
                seconds = Math.Round(seconds);
                if (seconds >= 60.0)
                {
                    seconds = 0.0;
                    minutes++;
                    if (minutes >= 60)
                    {
                        minutes = 0;
                        degrees++;
                    }
                }
            }

            // Format seconds cleanly without excessive decimal noise or trailing quotes
            string sSeconds = roundSeconds ? $"{seconds:0}" : $"{seconds:0.00}";

            return $"{sDirection}{degrees}° {minutes}' {sSeconds}";
        }

        private static void WriteSourceFXML(SimBaseDocument simBaseDocument, ScenarioFormData formData)
        {
            // 1. Setup clean namespaces to prevent xsi/xsd attributes
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");

            XmlSerializer xmlSerializer = new(simBaseDocument.GetType());

            // 2. Safe path combination
            string filePath = Path.Combine(formData.ScenarioFolder, $"{formData.ScenarioTitle}.fxml");

            // 3. Serialize in one pass
            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, simBaseDocument, ns);
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
