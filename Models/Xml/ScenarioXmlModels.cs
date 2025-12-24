#nullable enable
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Models.Xml;

#region 1. Root Containers
[XmlRoot("SimBase.Document")]
public class SimBaseDocumentXML
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("Title")] public string? Title { get; set; }
    [XmlElement("WorldBase.Flight")] public WorldBaseFlight? WorldBaseFlight { get; set; }
    [XmlElement("WorldBase.Waypoints")] public WorldBaseWaypoints? WorldBaseWaypoints { get; set; }
    [XmlAttribute("Type")] public string? Type { get; set; }
    [XmlAttribute("version")] public double Version { get; set; } = 1.0;
}

[XmlRoot("WorldBase.Flight")]
public class WorldBaseFlight
{
    [XmlElement("SimContain.Container")] public List<SimContainContainer> SimContainContainer { get; set; } = [];
    [XmlElement("SceneryObjects.LibraryObject")] public List<SceneryObjectsLibraryObject> SceneryObjectsLibraryObject { get; set; } = [];
    [XmlElement("SimMission.AirportLandingTrigger")] public List<SimMissionAirportLandingTrigger> SimMissionAirportLandingTrigger { get; set; } = [];
    [XmlElement("SimMission.AreaLandingTrigger")] public List<SimMissionAreaLandingTrigger> SimMissionAreaLandingTrigger { get; set; } = [];
    [XmlElement("SimMission.CloseWindowAction")] public List<SimMissionCloseWindowAction> SimMissionCloseWindowAction { get; set; } = [];
    [XmlElement("SimMission.CylinderArea")] public List<SimMissionCylinderArea> SimMissionCylinderArea { get; set; } = [];
    [XmlElement("SimMission.DialogAction")] public List<SimMissionDialogAction> SimMissionDialogAction { get; set; } = [];
    [XmlElement("SimMission.DisabledTrafficAirports")] public SimMissionDisabledTrafficAirports? SimMissionDisabledTrafficAirports { get; set; }
    [XmlElement("SimMission.Goal")] public List<SimMissionGoal> SimMissionGoal { get; set; } = [];
    [XmlElement("SimMission.GoalResolutionAction")] public List<SimMissionGoalResolutionAction> SimMissionGoalResolutionAction { get; set; } = [];
    [XmlElement("SimMission.ObjectActivationAction")] public List<SimMissionObjectActivationAction> SimMissionObjectActivationAction { get; set; } = [];
    [XmlElement("SimMission.OneShotSoundAction")] public List<SimMissionOneShotSoundAction> SimMissionOneShotSoundAction { get; set; } = [];
    [XmlElement("SimMission.OnScreenText")] public List<SimMissionOnScreenText> SimMissionOnScreenText { get; set; } = [];
    [XmlElement("SimMission.OpenWindowAction")] public List<SimMissionOpenWindowAction> SimMissionOpenWindowAction { get; set; } = [];
    [XmlElement("SimMission.PointOfInterest")] public List<SimMissionPointOfInterest> SimMissionPointOfInterest { get; set; } = [];
    [XmlElement("SimMission.PointOfInterestActivationAction")] public List<SimMissionPointOfInterestActivationAction> SimMissionPointOfInterestActivationAction { get; set; } = [];
    [XmlElement("SimMission.PropertyTrigger")] public List<SimMissionPropertyTrigger> SimMissionPropertyTrigger { get; set; } = [];
    [XmlElement("SimMission.ProximityTrigger")] public List<SimMissionProximityTrigger> SimMissionProximityTrigger { get; set; } = [];
    [XmlElement("SimMission.RealismOverrides")] public SimMissionRealismOverrides? SimMissionRealismOverrides { get; set; }
    [XmlElement("SimMission.RectangleArea")] public List<SimMissionRectangleArea> SimMissionRectangleArea { get; set; } = [];
    [XmlElement("SimMission.ScaleformPanelWindow")] public List<SimMissionScaleformPanelWindow> SimMissionScaleformPanelWindow { get; set; } = [];
    [XmlElement("SimMission.ScenarioVariable")] public List<SimMissionScenarioVariable> SimMissionScenarioVariable { get; set; } = [];
    [XmlElement("SimMission.ScriptAction")] public List<SimMissionScriptAction> SimMissionScriptAction { get; set; } = [];
    [XmlElement("SimMission.TimerTrigger")] public List<SimMissionTimerTrigger> SimMissionTimerTrigger { get; set; } = [];
    [XmlElement("SimMissionUI.ScenarioMetadata")] public SimMissionUIScenarioMetadata? SimMissionUIScenarioMetadata { get; set; }
    [XmlElement("SimMission.UIPanelWindow")] public List<SimMissionUIPanelWindow> SimMissionUIPanelWindow { get; set; } = [];
}
#endregion

#region 2. Triggers
[XmlRoot("SimMission.AirportLandingTrigger")]
public class SimMissionAirportLandingTrigger
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("LandingType")] public string? LandingType { get; set; }
    [XmlElement("Activated")] public string? Activated { get; set; }
    [XmlElement("Actions")] public Actions? Actions { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
    [XmlElement("AirportIdent")] public string? AirportIdent { get; set; }
    [XmlElement("RunwayFilter")] public RunwayFilter? RunwayFilter { get; set; }
}

[XmlRoot("SimMission.AreaLandingTrigger")]
public class SimMissionAreaLandingTrigger
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("LandingType")] public string? LandingType { get; set; }
    [XmlElement("Activated")] public string? Activated { get; set; }
    [XmlElement("Actions")] public Actions? Actions { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
    [XmlElement("Areas")] public Areas? Areas { get; set; }
}

[XmlRoot("SimMission.ProximityTrigger")]
public class SimMissionProximityTrigger(string? descr = null, Areas? areas = null, OnEnterActions? enter = null, string? id = null, string? active = null, OnExitActions? exit = null)
{
    public SimMissionProximityTrigger() : this(null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Areas")] public Areas? Areas { get; set; } = areas;
    [XmlElement("OnEnterActions")] public OnEnterActions? OnEnterActions { get; set; } = enter;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("Activated")] public string? Activated { get; set; } = active;
    [XmlElement("OnExitActions")] public OnExitActions? OnExitActions { get; set; } = exit;
}

[XmlRoot("SimMission.TimerTrigger")]
public class SimMissionTimerTrigger(string? descr = null, double stop = 0, string? screen = null, string? active = null, string? id = null, Actions? actions = null)
{
    public SimMissionTimerTrigger() : this(null, 0, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("StopTime")] public double StopTime { get; set; } = stop;
    [XmlElement("OnScreenTimer")] public string? OnScreenTimer { get; set; } = screen;
    [XmlElement("Activated")] public string? Activated { get; set; } = active;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("Actions")] public Actions? Actions { get; set; } = actions;
}

[XmlRoot("SimMission.PropertyTrigger")]
public class SimMissionPropertyTrigger
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("Activated")] public string? Activated { get; set; }
    [XmlElement("Actions")] public Actions? Actions { get; set; }
    [XmlElement("Condition")] public Condition? Condition { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
}
#endregion

#region 3. Actions
[XmlRoot("SimMission.DialogAction")]
public class SimMissionDialogAction(string? descr = null, string? text = null, string? delay = null, string? sound = null, string? id = null)
{
    public SimMissionDialogAction() : this(null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Text")] public string? Text { get; set; } = text;
    [XmlElement("DelaySeconds")] public string? DelaySeconds { get; set; } = delay;
    [XmlElement("SoundType")] public string? SoundType { get; set; } = sound;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.GoalResolutionAction")]
public class SimMissionGoalResolutionAction(string? resolution = null, string? descr = null, Goals? goals = null, string? id = null)
{
    public SimMissionGoalResolutionAction() : this(null, null, null, null) { }
    [XmlElement("GoalResolution")] public string? GoalResolution { get; set; } = resolution;
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Goals")] public Goals? Goals { get; set; } = goals;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.ObjectActivationAction")]
public class SimMissionObjectActivationAction(string? descr = null, ObjectReferenceList? list = null, string? id = null, string? state = null)
{
    public SimMissionObjectActivationAction() : this(null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("ObjectReferenceList")] public ObjectReferenceList? ObjectReferenceList { get; set; } = list;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("NewObjectState")] public string? NewObjectState { get; set; } = state;
}

[XmlRoot("SimMission.OneShotSoundAction")]
public class SimMissionOneShotSoundAction(string? descr = null, string? fileName = null, string? id = null)
{
    public SimMissionOneShotSoundAction() : this(null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("SoundFileName")] public string? SoundFileName { get; set; } = fileName;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.ScriptAction")]
public class SimMissionScriptAction(string? descr = null, string? script = null, string? id = null, string? text = null)
{
    public SimMissionScriptAction() : this(null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Script")] public string? Script { get; set; } = script;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlText] public string? Text { get; set; } = text;
}
#endregion

#region 4. Areas and World Objects
[XmlRoot("SimMission.CylinderArea")]
public class SimMissionCylinderArea(string? descr = null, string? orient = null, string? radius = null, string? height = null, string? draw = null, AttachedWorldPosition? awp = null, string? id = null)
{
    public SimMissionCylinderArea() : this(null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Orientation")] public string? Orientation { get; set; } = orient;
    [XmlElement("AreaRadius")] public string? AreaRadius { get; set; } = radius;
    [XmlElement("Height")] public string? Height { get; set; } = height;
    [XmlElement("DrawStyle")] public string? DrawStyle { get; set; } = draw;
    [XmlElement("AttachedWorldPosition")] public AttachedWorldPosition? AttachedWorldPosition { get; set; } = awp;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.RectangleArea")]
public class SimMissionRectangleArea(string? descr = null, string? orient = null, string? len = null, string? width = null, string? height = null, AttachedWorldPosition? awp = null, string? id = null)
{
    public SimMissionRectangleArea() : this(null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Orientation")] public string? Orientation { get; set; } = orient;
    [XmlElement("Length")] public string? Length { get; set; } = len;
    [XmlElement("Width")] public string? Width { get; set; } = width;
    [XmlElement("Height")] public string? Height { get; set; } = height;
    [XmlElement("AttachedWorldPosition")] public AttachedWorldPosition? AttachedWorldPosition { get; set; } = awp;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("AttachedWorldObject")] public AttachedWorldObject? AttachedWorldObject { get; set; }
}

[XmlRoot("SceneryObjects.LibraryObject")]
public class SceneryObjectsLibraryObject(string? descr = null, string? guid = null, string? pos = null, string? orient = null, string? agl = null, string? scale = null, string? id = null, string? active = null)
{
    public SceneryObjectsLibraryObject() : this(null, null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("MDLGuid")] public string? MDLGuid { get; set; } = guid;
    [XmlElement("WorldPosition")] public string? WorldPosition { get; set; } = pos;
    [XmlElement("Orientation")] public string? Orientation { get; set; } = orient;
    [XmlElement("AltitudeIsAGL")] public string? AltitudeIsAGL { get; set; } = agl;
    [XmlElement("Scale")] public string? Scale { get; set; } = scale;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("Activated")] public string? Activated { get; set; } = active;
}

[XmlRoot("SimContain.Container")]
public class SimContainContainer
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("WorldPosition")] public string? WorldPosition { get; set; }
    [XmlElement("Orientation")] public string? Orientation { get; set; }
    [XmlElement("ContainerTitle")] public string? ContainerTitle { get; set; }
    [XmlElement("ContainerID")] public int ContainerID { get; set; }
    [XmlElement("IdentificationNumber")] public int IdentificationNumber { get; set; }
    [XmlElement("IsOnGround")] public string? IsOnGround { get; set; }
    [XmlElement("AIType")] public string? AIType { get; set; }
    [XmlElement("GroundVehicleAI")] public GroundVehicleAI? GroundVehicleAI { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
}
#endregion

#region 5. UI and Metadata
[XmlRoot("SimMission.OnScreenText")]
public class SimMissionOnScreenText(string? descr = null, string? text = null, string? loc = null, string? color = null, string? active = null, string? bg = null, string? id = null)
{
    public SimMissionOnScreenText() : this(null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Text")] public string? Text { get; set; } = text;
    [XmlElement("OnScreenTextDisplayLocation")] public string? OnScreenTextDisplayLocation { get; set; } = loc;
    [XmlElement("RGBColor")] public string? RGBColor { get; set; } = color;
    [XmlElement("Activated")] public string? Activated { get; set; } = active;
    [XmlElement("OnScreenTextBackgroundColor")] public string? OnScreenTextBackgroundColor { get; set; } = bg;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.OpenWindowAction")]
public class SimMissionOpenWindowAction(string? descr = null, SetWindowSize? size = null, SetWindowLocation? loc = null, string? rel = null, ObjectReference? obj = null, string? id = null)
{
    public SimMissionOpenWindowAction() : this(null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("SetWindowSize")] public SetWindowSize? SetWindowSize { get; set; } = size;
    [XmlElement("SetWindowLocation")] public SetWindowLocation? SetWindowLocation { get; set; } = loc;
    [XmlElement("RelativeTo")] public string? RelativeTo { get; set; } = rel;
    [XmlElement("ObjectReference")] public ObjectReference? ObjectReference { get; set; } = obj;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.CloseWindowAction")]
public class SimMissionCloseWindowAction(string? descr = null, ObjectReference? objRef = null, string? id = null)
{
    public SimMissionCloseWindowAction() : this(null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("ObjectReference")] public ObjectReference? ObjectReference { get; set; } = objRef;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMissionUI.ScenarioMetadata")]
public class SimMissionUIScenarioMetadata
{
    [XmlElement("SkillLevel")] public string? SkillLevel { get; set; }
    [XmlElement("LocationDescr")] public string? LocationDescr { get; set; }
    [XmlElement("DifficultyLevel")] public int DifficultyLevel { get; set; }
    [XmlElement("EstimatedTime")] public int EstimatedTime { get; set; }
    [XmlElement("UncompletedImage")] public string? UncompletedImage { get; set; }
    [XmlElement("CompletedImage")] public string? CompletedImage { get; set; }
    [XmlElement("ExitMissionImage")] public string? ExitMissionImage { get; set; }
    [XmlElement("MissionBrief")] public string? MissionBrief { get; set; }
    [XmlElement("AbbreviatedMissionBrief")] public string? AbbreviatedMissionBrief { get; set; }
    [XmlElement("SuccessMessage")] public string? SuccessMessage { get; set; }
    [XmlElement("FailureMessage")] public string? FailureMessage { get; set; }
    [XmlElement("UserCrashMessage")] public string? UserCrashMessage { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
}

[XmlRoot("SimMission.UIPanelWindow")]
public class SimMissionUIPanelWindow(string? descr = null, string? locked = null, string? mouse = null, string? id = null, string? file = null, string? docked = null, string? keys = null)
{
    public SimMissionUIPanelWindow() : this(null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Locked")] public string? Locked { get; set; } = locked;
    [XmlElement("HasMouseInteractivity")] public string? HasMouseInteractivity { get; set; } = mouse;
    [XmlElement("FlashFileName")] public string? FlashFileName { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("UIPanelFileName")] public string? UIPanelFileName { get; set; } = file;
    [XmlElement("Docked")] public string? Docked { get; set; } = docked;
    [XmlElement("HasKeyboardInteractivity")] public string? HasKeyboardInteractivity { get; set; } = keys;
}

[XmlRoot("SimMission.ScaleformPanelWindow")]
public class SimMissionScaleformPanelWindow(string? descr = null, string? locked = null, string? mouse = null, string? flash = null, string? id = null, string? ui = null, string? docked = null, string? keys = null)
{
    public SimMissionScaleformPanelWindow() : this(null, null, null, null, null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Locked")] public string? Locked { get; set; } = locked;
    [XmlElement("HasMouseInteractivity")] public string? HasMouseInteractivity { get; set; } = mouse;
    [XmlElement("FlashFileName")] public string? FlashFileName { get; set; } = flash;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("UIPanelFileName")] public string? UIPanelFileName { get; set; } = ui;
    [XmlElement("Docked")] public string? Docked { get; set; } = docked;
    [XmlElement("HasKeyboardInteractivity")] public string? HasKeyboardInteractivity { get; set; } = keys;
}
#endregion

#region 6. Helper Classes and Logic
[XmlRoot("ObjectReference")]
public class ObjectReference(string? instanceId = null)
{
    public ObjectReference() : this(null) { }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = instanceId;
}

[XmlRoot("ObjectReferenceList")]
public class ObjectReferenceList(List<ObjectReference>? objectReference = null)
{
    public ObjectReferenceList() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("Actions")]
public class Actions(List<ObjectReference>? objectReference = null)
{
    public Actions() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("Goals")]
public class Goals(List<ObjectReference>? objectReference = null)
{
    public Goals() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("Areas")]
public class Areas(List<ObjectReference>? objectReference = null)
{
    public Areas() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("OnEnterActions")]
public class OnEnterActions(List<ObjectReference>? objectReference = null)
{
    public OnEnterActions() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("OnExitActions")]
public class OnExitActions(List<ObjectReference>? objectReference = null)
{
    public OnExitActions() : this(null) { }
    [XmlElement("ObjectReference")] public List<ObjectReference> ObjectReference { get; set; } = objectReference ?? [];
}

[XmlRoot("RunwayFilter")]
public class RunwayFilter(string? num = null, string? designator = null)
{
    public RunwayFilter() : this(null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("RunwayNumber")] public string? RunwayNumber { get; set; } = num;
    [XmlElement("RunwayDesignator")] public string? RunwayDesignator { get; set; } = designator;
}

[XmlRoot("AttachedWorldPosition")]
public class AttachedWorldPosition(string? worldPosition = null, string? altitudeIsAGL = null)
{
    public AttachedWorldPosition() : this(null, null) { }
    [XmlElement("WorldPosition")] public string? WorldPosition { get; set; } = worldPosition;
    [XmlElement("AltitudeIsAGL")] public string? AltitudeIsAGL { get; set; } = altitudeIsAGL;
}

[XmlRoot("AttachedWorldObject")]
public class AttachedWorldObject(ObjectReference? objRef = null, string? offset = null)
{
    public AttachedWorldObject() : this(null, null) { }
    [XmlElement("ObjectReference")] public ObjectReference? ObjectReference { get; set; } = objRef;
    [XmlElement("OffsetXYZ")] public string? OffsetXYZ { get; set; } = offset;
    [XmlElement("BodyRelativeOffset")] public string? BodyRelativeOffset { get; set; } = "False, False, False";
    [XmlElement("BodyRelativeRotation")] public string? BodyRelativeRotation { get; set; } = "False, False, False";
}

[XmlRoot("GroundVehicleAI")]
public class GroundVehicleAI
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("WaypointList")] public ObjectReference? WaypointList { get; set; }
}

[XmlRoot("Condition")]
public class Condition { [XmlElement("And")] public And? And { get; set; } }

[XmlRoot("And")]
public class And
{
    [XmlElement("GreaterOrEqual")] public GreaterOrEqual? GreaterOrEqual { get; set; }
    [XmlElement("GreaterThan")] public GreaterThan? GreaterThan { get; set; }
}

[XmlRoot("GreaterOrEqual")]
public class GreaterOrEqual
{
    [XmlElement("LHS")] public LHS? LHS { get; set; }
    [XmlElement("RHS")] public RHS? RHS { get; set; }
}

[XmlRoot("GreaterThan")]
public class GreaterThan
{
    [XmlElement("LHS")] public LHS? LHS { get; set; }
    [XmlElement("RHS")] public RHS? RHS { get; set; }
}

[XmlRoot("LHS")]
public class LHS { [XmlElement("Property")] public PropertyXML? PropertyXML { get; set; } }

[XmlRoot("RHS")]
public class RHS { [XmlElement("Constant")] public Constant? Constant { get; set; } }

[XmlRoot("Property")]
public class PropertyXML
{
    [XmlElement("Name")] public string? Name { get; set; }
    [XmlElement("Units")] public string? Units { get; set; }
    [XmlElement("ObjectReference")] public ObjectReference? ObjectReference { get; set; }
}

[XmlRoot("Constant")]
public class Constant(double value = 0)
{
    public Constant() : this(0) { }
    [XmlElement("Double")] public double Double { get; set; } = value;
}

[XmlRoot("SetWindowSize")]
public class SetWindowSize(int x = 0, int y = 0)
{
    public SetWindowSize() : this(0, 0) { }
    [XmlAttribute("X")] public int X { get; set; } = x;
    [XmlAttribute("Y")] public int Y { get; set; } = y;
}

[XmlRoot("SetWindowLocation")]
public class SetWindowLocation(int x = 0, int y = 0)
{
    public SetWindowLocation() : this(0, 0) { }
    [XmlAttribute("X")] public int X { get; set; } = x;
    [XmlAttribute("Y")] public int Y { get; set; } = y;
}

[XmlRoot("WorldBase.Waypoints")]
public class WorldBaseWaypoints
{
    [XmlElement("SimContain.WaypointList")] public List<SimContainWaypointList> SimContainWaypointList { get; set; } = [];
}

[XmlRoot("SimContain.WaypointList")]
public class SimContainWaypointList
{
    [XmlElement("WrapWaypoints")] public string? WrapWaypoints { get; set; }
    [XmlElement("CurrentWaypoint")] public int CurrentWaypoint { get; set; }
    [XmlElement("BackupToFirst")] public string? BackupToFirst { get; set; }
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("Waypoint")] public List<Waypoint> Waypoint { get; set; } = [];
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
}

[XmlRoot("Waypoint")]
public class Waypoint
{
    [XmlElement("AltitudeIsAGL")] public string? AltitudeIsAGL { get; set; }
    [XmlElement("WorldPosition")] public string? WorldPosition { get; set; }
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("WaypointID")] public int WaypointID { get; set; }
    [XmlElement("Orientation")] public string? Orientation { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
    [XmlElement("SpeedKnots")] public double SpeedKnots { get; set; }
}

[XmlRoot("SimMission.Goal")]
public class SimMissionGoal(string? descr = null, string? text = null, string? id = null)
{
    public SimMissionGoal() : this(null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Text")] public string? Text { get; set; } = text;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
}

[XmlRoot("SimMission.PointOfInterestActivationAction")]
public class SimMissionPointOfInterestActivationAction(string? descr = null, ObjectReferenceList? list = null, string? id = null, string? state = null)
{
    public SimMissionPointOfInterestActivationAction() : this(null, null, null, null) { }
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("ObjectReferenceList")] public ObjectReferenceList? ObjectReferenceList { get; set; } = list;
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlElement("NewObjectState")] public string? NewObjectState { get; set; } = state;
}

[XmlRoot("SimMission.ScenarioVariable")]
public class SimMissionScenarioVariable(List<TriggerCondition>? conditions = null, string? id = null, string? text = null, string? descr = null, string? name = null, string? val = null)
{
    public SimMissionScenarioVariable() : this(null, null, null, null, null, null) { }
    [XmlElement("TriggerCondition")] public List<TriggerCondition> TriggerCondition { get; set; } = conditions ?? [];
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; } = id;
    [XmlText] public string? Text { get; set; } = text;
    [XmlElement("Descr")] public string? Descr { get; set; } = descr;
    [XmlElement("Name")] public string? Name { get; set; } = name;
    [XmlElement("VariableValue")] public string? VariableValue { get; set; } = val;
}

[XmlRoot("TriggerCondition")]
public class TriggerCondition(List<Actions>? actions = null, TriggerValue? val = null)
{
    public TriggerCondition() : this(null, null) { }
    [XmlElement("Actions")] public List<Actions> Actions { get; set; } = actions ?? [];
    [XmlElement("TriggerValue")] public TriggerValue? TriggerValue { get; set; } = val;
}

[XmlRoot("TriggerValue")]
public class TriggerValue(Constant? constant = null)
{
    public TriggerValue() : this(null) { }
    [XmlElement("Constant")] public Constant? Constant { get; set; } = constant;
}

[XmlRoot("SimMission.DisabledTrafficAirports")]
public class SimMissionDisabledTrafficAirports(string? ident = null)
{
    public SimMissionDisabledTrafficAirports() : this(null) { }
    [XmlElement("AirportIdent")] public string? AirportIdent { get; set; } = ident;
}

[XmlRoot("SimMission.RealismOverrides")]
public class SimMissionRealismOverrides
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("UserTips")] public string? UserTips { get; set; }
    [XmlElement("CrashBehavior")] public string? CrashBehavior { get; set; }
    [XmlElement("ATCMenuDisabled")] public string? ATCMenuDisabled { get; set; }
    [XmlElement("FlightRealism")] public string? FlightRealism { get; set; }
    [XmlElement("WorldRealism")] public string? WorldRealism { get; set; }
    [XmlElement("UnlimitedFuel")] public string? UnlimitedFuel { get; set; }
    [XmlElement("AircraftLabels")] public string? AircraftLabels { get; set; }
    [XmlElement("AvatarNoCollision")] public string? AvatarNoCollision { get; set; }
}

[XmlRoot("SimMission.PointOfInterest")]
public class SimMissionPointOfInterest
{
    [XmlElement("Descr")] public string? Descr { get; set; }
    [XmlElement("Activated")] public string? Activated { get; set; }
    [XmlElement("SelectedModelGuid")] public string? SelectedModelGuid { get; set; }
    [XmlElement("UnselectedModelGuid")] public string? UnselectedModelGuid { get; set; }
    [XmlElement("CurrentSelection")] public string? CurrentSelection { get; set; }
    [XmlElement("CycleOrder")] public int CycleOrder { get; set; }
    [XmlElement("TargetName")] public string? TargetName { get; set; }
    [XmlElement("AttachedWorldObject")] public AttachedWorldObject? AttachedWorldObject { get; set; }
    [XmlElement("AttachedWorldPosition")] public AttachedWorldPosition? AttachedWorldPosition { get; set; }
    [XmlAttribute("InstanceId")] public string? InstanceId { get; set; }
}
#endregion