using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator
{
    public class BingMetadata
    {
		static internal BoundingBox GetBoundingBox()
        {
			XmlSerializer serializer = new XmlSerializer(typeof(BoundingBox));
			FileStream fs = new FileStream($"{Path.GetDirectoryName(Parameters.SaveLocation)}\\images\\temp.xml", FileMode.Open);
			BoundingBox bb;
			bb = (BoundingBox)serializer.Deserialize(fs);
			return bb;
		}
    }

	// using System.Xml.Serialization;
	// XmlSerializer serializer = new XmlSerializer(typeof(Response));
	// using (StringReader reader = new StringReader(xml))
	// {
	//    var test = (Response)serializer.Deserialize(reader);
	// }

	[XmlRoot(ElementName = "BoundingBox")]
	public class BoundingBox
	{

		[XmlElement(ElementName = "SouthLatitude")]
		public double SouthLatitude { get; set; }

		[XmlElement(ElementName = "WestLongitude")]
		public double WestLongitude { get; set; }

		[XmlElement(ElementName = "NorthLatitude")]
		public double NorthLatitude { get; set; }

		[XmlElement(ElementName = "EastLongitude")]
		public double EastLongitude { get; set; }
	}

	[XmlRoot(ElementName = "MapCenter")]
	public class MapCenter
	{

		[XmlElement(ElementName = "Latitude")]
		public double Latitude { get; set; }

		[XmlElement(ElementName = "Longitude")]
		public double Longitude { get; set; }
	}

	[XmlRoot(ElementName = "Point")]
	public class Point
	{

		[XmlElement(ElementName = "Latitude")]
		public double Latitude { get; set; }

		[XmlElement(ElementName = "Longitude")]
		public double Longitude { get; set; }
	}

	[XmlRoot(ElementName = "Anchor")]
	public class Anchor
	{

		[XmlElement(ElementName = "X")]
		public int X { get; set; }

		[XmlElement(ElementName = "Y")]
		public int Y { get; set; }
	}

	[XmlRoot(ElementName = "TopLeftOffset")]
	public class TopLeftOffset
	{

		[XmlElement(ElementName = "X")]
		public int X { get; set; }

		[XmlElement(ElementName = "Y")]
		public int Y { get; set; }
	}

	[XmlRoot(ElementName = "BottomRightOffset")]
	public class BottomRightOffset
	{

		[XmlElement(ElementName = "X")]
		public int X { get; set; }

		[XmlElement(ElementName = "Y")]
		public int Y { get; set; }
	}

	[XmlRoot(ElementName = "PushpinMetadata")]
	public class PushpinMetadata
	{

		[XmlElement(ElementName = "Point")]
		public Point Point { get; set; }

		[XmlElement(ElementName = "Anchor")]
		public Anchor Anchor { get; set; }

		[XmlElement(ElementName = "TopLeftOffset")]
		public TopLeftOffset TopLeftOffset { get; set; }

		[XmlElement(ElementName = "BottomRightOffset")]
		public BottomRightOffset BottomRightOffset { get; set; }
	}

	[XmlRoot(ElementName = "Pushpins")]
	public class Pushpins
	{

		[XmlElement(ElementName = "PushpinMetadata")]
		public List<PushpinMetadata> PushpinMetadata { get; set; }
	}

	[XmlRoot(ElementName = "StaticMapMetadata")]
	public class StaticMapMetadata
	{

		[XmlElement(ElementName = "BoundingBox")]
		public BoundingBox BoundingBox { get; set; }

		[XmlElement(ElementName = "MapCenter")]
		public MapCenter MapCenter { get; set; }

		[XmlElement(ElementName = "ImageWidth")]
		public int ImageWidth { get; set; }

		[XmlElement(ElementName = "ImageHeight")]
		public int ImageHeight { get; set; }

		[XmlElement(ElementName = "Zoom")]
		public int Zoom { get; set; }

		[XmlElement(ElementName = "Pushpins")]
		public Pushpins Pushpins { get; set; }
	}

	[XmlRoot(ElementName = "Resources")]
	public class Resources
	{

		[XmlElement(ElementName = "StaticMapMetadata")]
		public StaticMapMetadata StaticMapMetadata { get; set; }
	}

	[XmlRoot(ElementName = "ResourceSet")]
	public class ResourceSet
	{

		[XmlElement(ElementName = "EstimatedTotal")]
		public int EstimatedTotal { get; set; }

		[XmlElement(ElementName = "Resources")]
		public Resources Resources { get; set; }
	}

	[XmlRoot(ElementName = "ResourceSets")]
	public class ResourceSets
	{

		[XmlElement(ElementName = "ResourceSet")]
		public ResourceSet ResourceSet { get; set; }
	}

	[XmlRoot(ElementName = "Response")]
	public class Response
	{

		[XmlElement(ElementName = "Copyright")]
		public string Copyright { get; set; }

		[XmlElement(ElementName = "BrandLogoUri")]
		public string BrandLogoUri { get; set; }

		[XmlElement(ElementName = "StatusCode")]
		public int StatusCode { get; set; }

		[XmlElement(ElementName = "StatusDescription")]
		public string StatusDescription { get; set; }

		[XmlElement(ElementName = "AuthenticationResultCode")]
		public string AuthenticationResultCode { get; set; }

		[XmlElement(ElementName = "TraceId")]
		public string TraceId { get; set; }

		[XmlElement(ElementName = "ResourceSets")]
		public ResourceSets ResourceSets { get; set; }

		[XmlAttribute(AttributeName = "xsd")]
		public string Xsd { get; set; }

		[XmlAttribute(AttributeName = "xsi")]
		public string Xsi { get; set; }

		[XmlAttribute(AttributeName = "xmlns")]
		public string Xmlns { get; set; }

		[XmlText]
		public string Text { get; set; }
	}


}
