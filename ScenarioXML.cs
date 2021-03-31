
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator
{
	public struct DialogAction
    {
		public string descr;
		public string text;
		public string delaySeconds;
		public string soundType;
		public string instanceID;

		public DialogAction(string s1, string s2, string s3, string s4, string s5)
		{
			descr = s1;
			text = s2;
			delaySeconds = s3;
			soundType = s4;
			instanceID = s5;
		}
	}

	public struct Gate
	{
		public double lat;
		public double lon;

		public Gate(double d1, double d2)
		{
			lat = d1;
			lon = d2;
		}
	}

	public struct LibraryObject
	{
		public string instanceID;
		public string descr;
		public string mdlGUID;
		public string worldPosition;
		public string orientation;
		public string altitudeIsAGL;
		public string scale;
		public string activated;

		public LibraryObject(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8)
		{
			instanceID = s1;
			descr = s2;
			mdlGUID = s3;
			worldPosition = s4;
			orientation = s5;
			altitudeIsAGL = s6;
			scale = s7;
			activated = s8;
		}
	}

	public class ScenarioXML
    {
		static private List<Gate> gates;
        static internal void GenerateXMLfile(Runway runway, Params parameters)
		{
			SimBaseDocumentXML simBaseDocumentXML = ReadSourceXML();
			gates = GateCalcs.SetGateCoords(runway, parameters);
			EditSourceXML(runway, simBaseDocumentXML, parameters);
			WriteSourceXML(simBaseDocumentXML, parameters);
		}

		static private SimBaseDocumentXML ReadSourceXML()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SimBaseDocumentXML));
			string xml = File.ReadAllText("source.xml");
			using StringReader reader = new StringReader(xml);
			return (SimBaseDocumentXML)serializer.Deserialize(reader);
		}

		static private void EditSourceXML(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			SetDialogAction(simBaseDocumentXML, parameters);
			SetLibraryObject(runway, simBaseDocumentXML, parameters);
			SetScenarioMetadata(runway, simBaseDocumentXML, parameters);
		}

		static private void SetDialogAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
        {
			List<DialogAction> dialogActions = new List<DialogAction>();
			switch (parameters.selectedScenario)
            {
				case nameof(ScenarioTypes.Circuit):
                    dialogActions.Add(new DialogAction("Dialog_Intro1", ScenarioHTML.GetBriefing(), "2", "Text-To-Speech", GetGUID()));
					dialogActions.Add(new DialogAction("Dialog_Intro2", ScenarioHTML.GetTips(), "2", "Text-To-Speech", GetGUID()));
					break;
				default:
					break;
			}
			List<SimMissionDialogAction> daList = simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction;
			daList.Clear();
			for (int index = 0; index < dialogActions.Count; index++)
            {
                SimMissionDialogAction da = new SimMissionDialogAction
                {
                    Descr = dialogActions[index].descr,
                    Text = dialogActions[index].text,
                    DelaySeconds = dialogActions[index].delaySeconds,
                    SoundType = dialogActions[index].soundType,
                    InstanceId = dialogActions[index].instanceID
                };
				daList.Add(da);
            }
		}

		static private void SetLibraryObject(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<LibraryObject> libraryObjects = new List<LibraryObject>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					libraryObjects.Add(new LibraryObject(GetGUID(), "GEN_game_1_blue", "{6079F842-015B-4017-A391-7C0F23BCBCD1}", SetWorldPosition(gates[0], parameters), $"0.0,0.0,{string.Format("{0:0.0}", runway.hdg)}", "True", "1", "True"));
					libraryObjects.Add(new LibraryObject(GetGUID(), "GEN_game_2_blue", "{3D49D581-9163-4A7A-B957-3CB7B7D4BAF4}", SetWorldPosition(gates[1], parameters), $"0.0,0.0,{string.Format("{0:0.0}", runway.hdg + 90)}", "True", "1", "False"));
					break;
				default:
					break;
			}
			List<SceneryObjectsLibraryObject> loList = simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject;
			loList.Clear();
			for (int index = 0; index < libraryObjects.Count; index++)
			{
				SceneryObjectsLibraryObject lo = new SceneryObjectsLibraryObject
				{
					InstanceId = libraryObjects[index].instanceID,
					Descr = libraryObjects[index].descr,
					MDLGuid = libraryObjects[index].mdlGUID,
					WorldPosition = libraryObjects[index].worldPosition,
					Orientation = libraryObjects[index].orientation,
					AltitudeIsAGL = libraryObjects[index].altitudeIsAGL,
					Scale = libraryObjects[index].scale,
					Activated = libraryObjects[index].activated
				};
				loList.Add(lo);
			}
		}

		static private string SetWorldPosition(Gate gate, Params parameters)
        {
			return $"{ScenarioFXML.FormatCoordXML(gate.lat, "N", "S")},{ScenarioFXML.FormatCoordXML(gate.lon, "E", "W")},+{parameters.height}";
		}

		static private void SetScenarioMetadata(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
        {
			SimMissionUIScenarioMetadata md;
			md = simBaseDocumentXML.WorldBaseFlight.SimMissionUIScenarioMetadata;
			md.InstanceId = GetGUID();
			md.SkillLevel = ScenarioHTML.GetDifficulty();
			md.LocationDescr = $"{runway.icaoName} ({runway.icaoId}) {runway.city}, {runway.country}";
			md.DifficultyLevel = 1;
			md.EstimatedTime = ScenarioHTML.GetDuration();
			md.UncompletedImage = "images\\ImgM_i.bmp";
			md.CompletedImage = "images\\ImgM_c.bmp";
			md.ExitMissionImage = "images\\exitMission.bmp";
			md.MissionBrief = "Overview.htm";
			md.AbbreviatedMissionBrief = $"{Path.GetFileNameWithoutExtension(parameters.saveLocation)}.htm";
			md.SuccessMessage = $"Success! You completed the \"{parameters.selectedScenario}\" scenario objectives.";
			md.FailureMessage = $"Better luck next time! You failed to complete the \"{parameters.selectedScenario}\" scenario objectives.";
			md.UserCrashMessage = $"Yikes! You crashed and therefore failed the \"{parameters.selectedScenario}\" scenario objectives.";
        }

        static private string GetGUID()
        {
			System.Guid guid = System.Guid.NewGuid();
			string guidUpper = guid.ToString().ToUpper();
			return $"{{{guidUpper}}}";
		}

		static private void WriteSourceXML(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(simBaseDocumentXML.GetType());

			using StreamWriter writer = new StreamWriter(parameters.saveLocation.Replace("fxml", "xml"));
			xmlSerializer.Serialize(writer, simBaseDocumentXML);
		}
	}

    #region Simbase.Document class definitions

    [XmlRoot(ElementName = "ObjectReference")]
	public class ObjectReference
	{

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "ObjectReferenceList")]
	public class ObjectReferenceList
	{

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.ObjectActivationAction")]
	public class SimMissionObjectActivationAction
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "ObjectReferenceList")]
		public ObjectReferenceList ObjectReferenceList { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "NewObjectState")]
		public string NewObjectState { get; set; }

	}

	[XmlRoot(ElementName = "SimMission.PointOfInterestActivationAction")]
	public class SimMissionPointOfInterestActivationAction
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "ObjectReferenceList")]
		public ObjectReferenceList ObjectReferenceList { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "NewObjectState")]
		public string NewObjectState { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.DialogAction")]
	public class SimMissionDialogAction
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Text")]
		public string Text { get; set; }

		[XmlElement(ElementName = "DelaySeconds")]
		public string DelaySeconds { get; set; }

		[XmlElement(ElementName = "SoundType")]
		public string SoundType { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.OneShotSoundAction")]
	public class SimMissionOneShotSoundAction
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "SoundFileName")]
		public string SoundFileName { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "Goals")]
	public class Goals
	{

		[XmlElement(ElementName = "ObjectReference")]
		public ObjectReference ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.GoalResolutionAction")]
	public class SimMissionGoalResolutionAction
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Goals")]
		public Goals Goals { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "AttachedWorldPosition")]
	public class AttachedWorldPosition
	{

		[XmlElement(ElementName = "WorldPosition")]
		public string WorldPosition { get; set; }

		[XmlElement(ElementName = "AltitudeIsAGL")]
		public string AltitudeIsAGL { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.RectangleArea")]
	public class SimMissionRectangleArea
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Orientation")]
		public string Orientation { get; set; }

		[XmlElement(ElementName = "Length")]
		public double Length { get; set; }

		[XmlElement(ElementName = "Width")]
		public double Width { get; set; }

		[XmlElement(ElementName = "Height")]
		public double Height { get; set; }

		[XmlElement(ElementName = "AttachedWorldPosition")]
		public AttachedWorldPosition AttachedWorldPosition { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "AttachedWorldObject")]
		public AttachedWorldObject AttachedWorldObject { get; set; }
	}

	[XmlRoot(ElementName = "AttachedWorldObject")]
	public class AttachedWorldObject
	{

		[XmlElement(ElementName = "ObjectReference")]
		public ObjectReference ObjectReference { get; set; }

		[XmlElement(ElementName = "OffsetXYZ")]
		public string OffsetXYZ { get; set; }
	}

	[XmlRoot(ElementName = "Actions")]
	public class Actions
	{

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.AreaLandingTrigger")]
	public class SimMissionAreaLandingTrigger
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "LandingType")]
		public string LandingType { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }

		[XmlElement(ElementName = "Actions")]
		public Actions Actions { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "Areas")]
		public Areas Areas { get; set; }
	}

	[XmlRoot(ElementName = "Areas")]
	public class Areas
	{

		[XmlElement(ElementName = "ObjectReference")]
		public ObjectReference ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "Property")]
	public class PropertyXML
	{

		[XmlElement(ElementName = "Name")]
		public string Name { get; set; }

		[XmlElement(ElementName = "Units")]
		public string Units { get; set; }

		[XmlElement(ElementName = "ObjectReference")]
		public ObjectReference ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "LHS")]
	public class LHS
	{

		[XmlElement(ElementName = "Property")]
		public PropertyXML PropertyXML { get; set; }
	}

	[XmlRoot(ElementName = "Constant")]
	public class Constant
	{

		[XmlElement(ElementName = "Double")]
		public double Double { get; set; }
	}

	[XmlRoot(ElementName = "RHS")]
	public class RHS
	{

		[XmlElement(ElementName = "Constant")]
		public Constant Constant { get; set; }
	}

	[XmlRoot(ElementName = "GreaterOrEqual")]
	public class GreaterOrEqual
	{

		[XmlElement(ElementName = "LHS")]
		public LHS LHS { get; set; }

		[XmlElement(ElementName = "RHS")]
		public RHS RHS { get; set; }
	}

	[XmlRoot(ElementName = "And")]
	public class And
	{

		[XmlElement(ElementName = "GreaterOrEqual")]
		public GreaterOrEqual GreaterOrEqual { get; set; }

		[XmlElement(ElementName = "GreaterThan")]
		public GreaterThan GreaterThan { get; set; }
	}

	[XmlRoot(ElementName = "Condition")]
	public class Condition
	{

		[XmlElement(ElementName = "And")]
		public And And { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.PropertyTrigger")]
	public class SimMissionPropertyTrigger
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }

		[XmlElement(ElementName = "Actions")]
		public Actions Actions { get; set; }

		[XmlElement(ElementName = "Condition")]
		public Condition Condition { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "GreaterThan")]
	public class GreaterThan
	{

		[XmlElement(ElementName = "LHS")]
		public LHS LHS { get; set; }

		[XmlElement(ElementName = "RHS")]
		public RHS RHS { get; set; }
	}

	[XmlRoot(ElementName = "OnEnterActions")]
	public class OnEnterActions
	{

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.ProximityTrigger")]
	public class SimMissionProximityTrigger
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Areas")]
		public Areas Areas { get; set; }

		[XmlElement(ElementName = "OnEnterActions")]
		public OnEnterActions OnEnterActions { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }

		[XmlElement(ElementName = "OnExitActions")]
		public OnExitActions OnExitActions { get; set; }
	}

	[XmlRoot(ElementName = "OnExitActions")]
	public class OnExitActions
	{

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.TimerTrigger")]
	public class SimMissionTimerTrigger
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "StopTime")]
		public double StopTime { get; set; }

		[XmlElement(ElementName = "OnScreenTimer")]
		public string OnScreenTimer { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "Actions")]
		public Actions Actions { get; set; }
	}

	[XmlRoot(ElementName = "SceneryObjects.LibraryObject")]
	public class SceneryObjectsLibraryObject
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "MDLGuid")]
		public string MDLGuid { get; set; }

		[XmlElement(ElementName = "WorldPosition")]
		public string WorldPosition { get; set; }

		[XmlElement(ElementName = "Orientation")]
		public string Orientation { get; set; }

		[XmlElement(ElementName = "AltitudeIsAGL")]
		public string AltitudeIsAGL { get; set; }

		[XmlElement(ElementName = "Scale")]
		public string Scale { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }
	}

	[XmlRoot(ElementName = "GroundVehicleAI")]
	public class GroundVehicleAI
	{

		[XmlElement(ElementName = "GroundCruiseSpeed")]
		public double GroundCruiseSpeed { get; set; }

		[XmlElement(ElementName = "GroundTurnSpeed")]
		public double GroundTurnSpeed { get; set; }

		[XmlElement(ElementName = "GroundTurnTime")]
		public double GroundTurnTime { get; set; }

		[XmlElement(ElementName = "YieldToUser")]
		public string YieldToUser { get; set; }

		[XmlElement(ElementName = "WaypointListReference")]
		public string WaypointListReference { get; set; }

		[XmlElement(ElementName = "Unit_Mode")]
		public string UnitMode { get; set; }
	}

	[XmlRoot(ElementName = "SimContain.Container")]
	public class SimContainContainer
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "WorldPosition")]
		public string WorldPosition { get; set; }

		[XmlElement(ElementName = "Orientation")]
		public string Orientation { get; set; }

		[XmlElement(ElementName = "ContainerTitle")]
		public string ContainerTitle { get; set; }

		[XmlElement(ElementName = "ContainerID")]
		public int ContainerID { get; set; }

		[XmlElement(ElementName = "IdentificationNumber")]
		public int IdentificationNumber { get; set; }

		[XmlElement(ElementName = "IsOnGround")]
		public string IsOnGround { get; set; }

		[XmlElement(ElementName = "AIType")]
		public string AIType { get; set; }

		[XmlElement(ElementName = "GroundVehicleAI")]
		public GroundVehicleAI GroundVehicleAI { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.PointOfInterest")]
	public class SimMissionPointOfInterest
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "TargetName")]
		public string TargetName { get; set; }

		[XmlElement(ElementName = "CurrentSelection")]
		public string CurrentSelection { get; set; }

		[XmlElement(ElementName = "AttachedWorldObject")]
		public AttachedWorldObject AttachedWorldObject { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "Activated")]
		public string Activated { get; set; }

		[XmlElement(ElementName = "CycleOrder")]
		public int CycleOrder { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.RealismOverrides")]
	public class SimMissionRealismOverrides
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "FlyingTips")]
		public string FlyingTips { get; set; }

		[XmlElement(ElementName = "CrashBehavior")]
		public string CrashBehavior { get; set; }

		[XmlElement(ElementName = "ATCMenuDisabled")]
		public string ATCMenuDisabled { get; set; }

		[XmlElement(ElementName = "FlightRealism")]
		public string FlightRealism { get; set; }

		[XmlElement(ElementName = "WorldRealism")]
		public string WorldRealism { get; set; }
	}

	[XmlRoot(ElementName = "SimMissionUI.ScenarioMetadata")]
	public class SimMissionUIScenarioMetadata
	{
		[XmlElement(ElementName = "SkillLevel")]
		public string SkillLevel { get; set; }

		[XmlElement(ElementName = "LocationDescr")]
		public string LocationDescr { get; set; }

		[XmlElement(ElementName = "DifficultyLevel")]
		public int DifficultyLevel { get; set; }

		[XmlElement(ElementName = "EstimatedTime")]
		public int EstimatedTime { get; set; }

		[XmlElement(ElementName = "UncompletedImage")]
		public string UncompletedImage { get; set; }	

		[XmlElement(ElementName = "CompletedImage")]
		public string CompletedImage { get; set; }

		[XmlElement(ElementName = "ExitMissionImage")]
		public string ExitMissionImage { get; set; }

		[XmlElement(ElementName = "MissionBrief")]
		public string MissionBrief { get; set; }

		[XmlElement(ElementName = "AbbreviatedMissionBrief")]
		public string AbbreviatedMissionBrief { get; set; }

		[XmlElement(ElementName = "SuccessMessage")]
		public string SuccessMessage { get; set; }

		[XmlElement(ElementName = "FailureMessage")]
		public string FailureMessage { get; set; }

		[XmlElement(ElementName = "UserCrashMessage")]
		public string UserCrashMessage { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.Goal")]
	public class SimMissionGoal
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Text")]
		public string Text { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.DisabledTrafficAirports")]
	public class SimMissionDisabledTrafficAirports
	{

		[XmlElement(ElementName = "AirportIdent")]
		public string AirportIdent { get; set; }
	}

	[XmlRoot(ElementName = "WorldBase.Flight")]
	public class WorldBaseFlight
	{

		[XmlElement(ElementName = "SimMission.ObjectActivationAction")]
		public List<SimMissionObjectActivationAction> SimMissionObjectActivationAction { get; set; }

		[XmlElement(ElementName = "SimMission.PointOfInterestActivationAction")]
		public List<SimMissionPointOfInterestActivationAction> SimMissionPointOfInterestActivationAction { get; set; }

		[XmlElement(ElementName = "SimMission.DialogAction")]
		public List<SimMissionDialogAction> SimMissionDialogAction { get; set; }

		[XmlElement(ElementName = "SimMission.OneShotSoundAction")]
		public List<SimMissionOneShotSoundAction> SimMissionOneShotSoundAction { get; set; }

		[XmlElement(ElementName = "SimMission.GoalResolutionAction")]
		public SimMissionGoalResolutionAction SimMissionGoalResolutionAction { get; set; }

		[XmlElement(ElementName = "SimMission.RectangleArea")]
		public List<SimMissionRectangleArea> SimMissionRectangleArea { get; set; }

		[XmlElement(ElementName = "SimMission.AreaLandingTrigger")]
		public List<SimMissionAreaLandingTrigger> SimMissionAreaLandingTrigger { get; set; }

		[XmlElement(ElementName = "SimMission.PropertyTrigger")]
		public List<SimMissionPropertyTrigger> SimMissionPropertyTrigger { get; set; }

		[XmlElement(ElementName = "SimMission.ProximityTrigger")]
		public List<SimMissionProximityTrigger> SimMissionProximityTrigger { get; set; }

		[XmlElement(ElementName = "SimMission.TimerTrigger")]
		public List<SimMissionTimerTrigger> SimMissionTimerTrigger { get; set; }

		[XmlElement(ElementName = "SceneryObjects.LibraryObject")]
		public List<SceneryObjectsLibraryObject> SceneryObjectsLibraryObject { get; set; }

		[XmlElement(ElementName = "SimContain.Container")]
		public List<SimContainContainer> SimContainContainer { get; set; }

		[XmlElement(ElementName = "SimMission.PointOfInterest")]
		public List<SimMissionPointOfInterest> SimMissionPointOfInterest { get; set; }

		[XmlElement(ElementName = "SimMission.RealismOverrides")]
		public SimMissionRealismOverrides SimMissionRealismOverrides { get; set; }

		[XmlElement(ElementName = "SimMissionUI.ScenarioMetadata")]
		public SimMissionUIScenarioMetadata SimMissionUIScenarioMetadata { get; set; }

		[XmlElement(ElementName = "SimMission.Goal")]
		public SimMissionGoal SimMissionGoal { get; set; }

		[XmlElement(ElementName = "SimMission.DisabledTrafficAirports")]
		public SimMissionDisabledTrafficAirports SimMissionDisabledTrafficAirports { get; set; }
	}

	[XmlRoot(ElementName = "Waypoint")]
	public class Waypoint
	{

		[XmlElement(ElementName = "AltitudeIsAGL")]
		public string AltitudeIsAGL { get; set; }

		[XmlElement(ElementName = "WorldPosition")]
		public string WorldPosition { get; set; }

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "WaypointID")]
		public int WaypointID { get; set; }

		[XmlElement(ElementName = "Orientation")]
		public string Orientation { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }

		[XmlElement(ElementName = "SpeedKnots")]
		public double SpeedKnots { get; set; }
	}

	[XmlRoot(ElementName = "SimContain.WaypointList")]
	public class SimContainWaypointList
	{

		[XmlElement(ElementName = "WrapWaypoints")]
		public string WrapWaypoints { get; set; }

		[XmlElement(ElementName = "CurrentWaypoint")]
		public int CurrentWaypoint { get; set; }

		[XmlElement(ElementName = "BackupToFirst")]
		public string BackupToFirst { get; set; }

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Waypoint")]
		public List<Waypoint> Waypoint { get; set; }

		[XmlAttribute(AttributeName = "InstanceId")]
		public string InstanceId { get; set; }
	}

	[XmlRoot(ElementName = "WorldBase.Waypoints")]
	public class WorldBaseWaypoints
	{

		[XmlElement(ElementName = "SimContain.WaypointList")]
		public List<SimContainWaypointList> SimContainWaypointList { get; set; }
	}

	[XmlRoot(ElementName = "SimBase.Document")]
	public class SimBaseDocumentXML
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Title")]
		public string Title { get; set; }

		[XmlElement(ElementName = "WorldBase.Flight")]
		public WorldBaseFlight WorldBaseFlight { get; set; }

		[XmlElement(ElementName = "MissionBuilder.MissionBuilder")]
		public object MissionBuilderMissionBuilder { get; set; }

		[XmlElement(ElementName = "WorldBase.AreasOfInterest")]
		public object WorldBaseAreasOfInterest { get; set; }

		[XmlElement(ElementName = "WorldBase.Waypoints")]
		public WorldBaseWaypoints WorldBaseWaypoints { get; set; }

		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public double Version { get; set; }
	}

	#endregion
}
