using P3D_Scenario_Generator.ConstantsEnums;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Services
{
    public class ScenarioXML()
    {
        private readonly SimBaseDocumentXML _simBaseDocumentXML = new();

        #region Actions

        public void SetDialogAction(string descr, string text, string delay, string soundType)
        {
            SimMissionDialogAction da = new(descr, text, delay, soundType, GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction.Add(da);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction = [da];
        }

        public void SetGoalResolutionAction(string search)
        {
            ObjectReference or = GetObjectReference("Goal", search);
            List <ObjectReference> orList = new([or]);
			SimMissionGoalResolutionAction gra = new("Completed", search, new Goals(orList), GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction.Add(gra);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction = [gra];
        }

        public void SetObjectActivationAction(int index, string objName, string search, string descr, string newObjectState)
        {
            search = $"{search}{index:00}";
            descr = $"{descr}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
			ObjectReferenceList orList = new([or]);
			SimMissionObjectActivationAction oaa = new(descr, orList, GetGUID(), newObjectState);
			if (_simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction != null )
				_simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.Add(oaa);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction = [oaa];
        }

        public void SetOneShotSoundAction(int index, string descr, string soundFile)
        {
            descr = $"{descr}{index:00}";
            SimMissionOneShotSoundAction ossa = new(descr, soundFile, GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction.Add(ossa);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction = [ossa];
        }

        public void SetPOIactivationAction(int index, string objName, string search, string descr, string newObjectState)
        {
            search = $"{search}{index:00}";
            descr = $"{descr}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
            ObjectReferenceList orList = new([or]);
			SimMissionPointOfInterestActivationAction paa = new(descr, orList, GetGUID(), newObjectState);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterestActivationAction != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterestActivationAction.Add(paa);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterestActivationAction = [paa];
        }

        public void SetScriptActions(string[] scripts)
        {
            List<SimMissionScriptAction> saList = [];

            for (int index = 0; index < scripts.Length; index++)
            {
                // Use an interpolated string with math for the ID
                string actionName = $"ScriptAction{index + 1:D2}";

                saList.Add(new SimMissionScriptAction(
                    actionName,
                    scripts[index],
                    GetGUID()
                ));
            }

            _simBaseDocumentXML.WorldBaseFlight.SimMissionScriptAction = saList;
        }

        #endregion

        #region Airports

        public void SetAirportLandingTrigger(string descr, string landingType, string activated, string airportIdent)
        {
            // Initialize the trigger using property initializers instead of a constructor
            SimMissionAirportLandingTrigger alt = new()
            {
                Descr = descr,
                LandingType = landingType,
                Activated = activated,
                Actions = new Actions([]), // Shortened list initialization
                InstanceId = GetGUID(),
                AirportIdent = airportIdent,
                RunwayFilter = null
            };

            // Using C# 12 null-coalescing assignment for cleaner list management
            _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger ??= [];
            _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger.Add(alt);
        }

        public void SetAirportLandingTriggerAction(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger.FindIndex(o => o.Descr == tSearch);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger[idIndex].Actions != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger[idIndex].Actions.ObjectReference.Add(or);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger[idIndex].Actions = new Actions([or]);
        }

        public void SetDisabledTrafficAirports(string airportIdent)
        {
            SimMissionDisabledTrafficAirports dta = new(airportIdent);
            _simBaseDocumentXML.WorldBaseFlight.SimMissionDisabledTrafficAirports = dta;
        }

        #endregion

        #region Area Definitions

        public void SetCylinderArea(int index, string descr, string orientation, string radius, string height, string drawStyle)
        {
            descr = $"{descr}{index:00}";
            SimMissionCylinderArea ca = new(descr, orientation, radius, height, drawStyle, new AttachedWorldPosition(), GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea.Add(ca);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea = [ca];
        }

        public void SetRectangleArea(string descr, string orientation, string length, string width, string height)
        {
            SimMissionRectangleArea ra = new(descr, orientation, length, width, height, new AttachedWorldPosition(), GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea.Add(ra);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea = [ra];
        }

        public void SetAttachedWorldPosition(string objName, string search, AttachedWorldPosition wp)
        {
            int idIndex;
            switch (objName)
            {
                case "CylinderArea":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea.FindIndex(o => o.Descr == search);
                    _simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea[idIndex].AttachedWorldPosition = wp;
                    break;
                case "RectangleArea":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea.FindIndex(o => o.Descr == search);
                    _simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea[idIndex].AttachedWorldPosition = wp;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Entities

        #endregion

        #region Goals

        public void SetGoal(string descr, string text)
        {
            SimMissionGoal g = new(descr, text, GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionGoal != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionGoal.Add(g);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionGoal = [g];
        }

        #endregion

        #region Helpers

        public ObjectReference GetObjectReference(string objName, string search)
        {
            ObjectReference or = new();
            int idIndex;
            switch (objName)
            {
                case "AirportLandingTrigger":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger[idIndex].InstanceId);
                    break;
                case "CloseWindowAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionCloseWindowAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionCloseWindowAction[idIndex].InstanceId);
                    break;
                case "CylinderArea":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionCylinderArea[idIndex].InstanceId);
                    break;
                case "DialogAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction[idIndex].InstanceId);
                    break;
                case "Goal":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionGoal.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionGoal[idIndex].InstanceId);
                    break;
                case "GoalResolutionAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction[idIndex].InstanceId);
                    break;
                case "LibraryObject":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject[idIndex].InstanceId);
                    break;
                case "ObjectActivationAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction[idIndex].InstanceId);
                    break;
                case "OneShotSoundAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction[idIndex].InstanceId);
                    break;
                case "OnScreenText":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionOnScreenText.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionOnScreenText[idIndex].InstanceId);
                    break;
                case "OpenWindowAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionOpenWindowAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionOpenWindowAction[idIndex].InstanceId);
                    break;
                case "PointOfInterest":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterest.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterest[idIndex].InstanceId);
                    break;
                case "PointOfInterestActivationAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterestActivationAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterestActivationAction[idIndex].InstanceId);
                    break;
                case "ProximityTrigger":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].InstanceId);
                    break;
                case "RectangleArea":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea[idIndex].InstanceId);
                    break;
                case "ScaleformPanelWindow":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionScaleformPanelWindow.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionScaleformPanelWindow[idIndex].InstanceId);
                    break;
                case "ScriptAction":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionScriptAction.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionScriptAction[idIndex].InstanceId);
                    break;
                case "TimerTrigger":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger[idIndex].InstanceId);
                    break;
                case "UIPanelWindow":
                    idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionUIPanelWindow.FindIndex(o => o.Descr == search);
                    or = new(_simBaseDocumentXML.WorldBaseFlight.SimMissionUIPanelWindow[idIndex].InstanceId);
                    break;
                default:
                    break;
            }
            return or;
        }

        public static AttachedWorldPosition GetAttachedWorldPosition(string worldPosition, string AltitudeIsAGL)
        {
            AttachedWorldPosition awp = new(worldPosition, AltitudeIsAGL);
            return awp;
        }

        public static string GetGateOrientation(Gate gate)
        {
            return $"{string.Format("{0:0.0}", gate.pitch)},0.0,{string.Format("{0:0.0}", gate.orientation)}";
        }

        public static string GetGateWorldPosition(Gate gate, double vertOffset)
        {
            return $"{ScenarioFXML.FormatCoordXML(gate.lat, "N", "S", false)},{ScenarioFXML.FormatCoordXML(gate.lon, "E", "W", false)},+{gate.amsl + vertOffset}";
        }

        public static string GetGUID()
        {
            Guid guid = Guid.NewGuid();
            string guidUpper = guid.ToString().ToUpper();
            return $"{{{guidUpper}}}";
        }

        #endregion

        #region Logic & Scenario Variables

        public void SetScenarioVariable(string descr, string name, string value)
        {
            TriggerCondition tc = new()
            {
                Actions = [],
                TriggerValue = new()
            };
            SimMissionScenarioVariable sv = new([tc], GetGUID(), descr, name, value);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable.Add(sv);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable = [sv];
        }

        public void SetScenarioVariableAction(string objName, string orSearch, int tcIndex, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            List<ObjectReference> orList = [or];
            Actions a = new(orList);
            List<Actions> aList = [a];
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable.FindIndex(o => o.Descr == tSearch);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions.Count != 0)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions[0].ObjectReference.Add(or);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions = aList;
        }

        public void SetScenarioVariableTriggerValue(double value, int tcIndex, string tSearch)
        {
            Constant constant = new(value);
            TriggerValue tv = new(constant);
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable.FindIndex(o => o.Descr == tSearch);
            if (value == 0) // reset trigger value to NULL
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable[idIndex].TriggerCondition = null;
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].TriggerValue = tv;
        }

        #endregion

        #region Root Containers

        public void SetSimbaseDocumentXML(ScenarioFormData formData, Overview overview)
        {
            _simBaseDocumentXML.Type = "MissionFile";
            _simBaseDocumentXML.Descr = $"This is a {formData.ScenarioImageFolder} scenario generated by {Constants.appTitle}. Estimated time to complete: {ScenarioHTML.GetDuration(overview)} minutes.";
            _simBaseDocumentXML.Title = $"{overview.Title}";

            _simBaseDocumentXML.WorldBaseFlight = new WorldBaseFlight();
        }

        public void WriteXML(ScenarioFormData formData)
        {
            // 1. Setup clean namespaces to avoid xmlns:xsi and xmlns:xsd
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");

            XmlSerializer xmlSerializer = new(_simBaseDocumentXML.GetType());

            // 2. Use Path.Combine for path safety
            string filePath = Path.Combine(formData.ScenarioFolder, $"{formData.ScenarioTitle}.xml");

            // 3. Serialize directly with empty namespaces
            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, _simBaseDocumentXML, ns);
            // StreamWriter is closed automatically by 'using'
        }

        #endregion

        #region Scenario Objects - Metadata & Settings

        public void SetRealismOverrides()
        {
            SimMissionRealismOverrides ro = new()
            {
                Descr = "RealismOverrides",
                CrashBehavior = "UserSpecified",
                ATCMenuDisabled = "False",
                FlightRealism = "UserSpecified",
                WorldRealism = "UserSpecified",
                AircraftLabels = "UserSpecified",
                AvatarNoCollision = "UserSpecified",
                UnlimitedFuel = "UserSpecified"
            };
            _simBaseDocumentXML.WorldBaseFlight.SimMissionRealismOverrides = ro;
        }

        public void SetScenarioMetadata(ScenarioFormData formData, Overview overview)
        {
            SimMissionUIScenarioMetadata md = new()
            {
                InstanceId = GetGUID(),
                SkillLevel = overview.Difficulty,
                LocationDescr = $"{formData.DestinationRunway.IcaoName} ({formData.DestinationRunway.IcaoId}) {formData.DestinationRunway.City}, {formData.DestinationRunway.Country}",
                DifficultyLevel = 1,
                EstimatedTime = ScenarioHTML.GetDuration(overview),
                UncompletedImage = "images\\imgM_i.bmp",
                CompletedImage = "images\\imgM_c.bmp",
                MissionBrief = "Overview.htm",
                AbbreviatedMissionBrief = $"{formData.ScenarioTitle}.htm",
                SuccessMessage = $"Success! You completed the \"{formData.ScenarioImageFolder}\" scenario objectives.",
                FailureMessage = $"Better luck next time! You failed to complete the \"{formData.ScenarioImageFolder}\" scenario objectives.",
                UserCrashMessage = $"Yikes! You crashed and therefore failed the \"{formData.ScenarioImageFolder}\" scenario objectives."
            };
            _simBaseDocumentXML.WorldBaseFlight.SimMissionUIScenarioMetadata = md;
        }

        #endregion

        #region Scenario Objects - Navigation

        #endregion

        #region Scenario Objects - UI

        public void SetCloseWindowAction(int index, string objName, string search)
        {
            search = $"{search}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
            SimMissionCloseWindowAction cwa = new($"Close{search}", or, GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionCloseWindowAction != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionCloseWindowAction.Add(cwa);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionCloseWindowAction = [cwa];
        }

        public void SetOnScreenText(string descr, string text, string onScrLoc, string RGBcol, string activated, string backCol)
        {
            SimMissionOnScreenText ost = new(descr, text, onScrLoc, RGBcol, activated, backCol, GetGUID());
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionOnScreenText != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionOnScreenText.Add(ost);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionOnScreenText = [ost];
        }

        public void SetOpenWindowAction(int index, string objName, string search, string[] windowParameters, string monitorNo)
        {
            search = $"{search}{index:00}";

            // Convert string array values to integers for the constructors
            // windowParameters expected: [0]=Width, [1]=Height, [2]=X, [3]=Y
            SetWindowSize sws = new(int.Parse(windowParameters[0]), int.Parse(windowParameters[1]));
            SetWindowLocation swl = new(int.Parse(windowParameters[2]), int.Parse(windowParameters[3]));

            ObjectReference or = GetObjectReference(objName, search);

            // Using Object Initializer to avoid constructor argument order/count issues
            SimMissionOpenWindowAction owa = new()
            {
                Descr = $"Open{search}",
                SetWindowSize = sws,
                SetWindowLocation = swl,
                RelativeTo = monitorNo,
                ObjectReference = or,
                InstanceId = GetGUID()
            };

            _simBaseDocumentXML.WorldBaseFlight.SimMissionOpenWindowAction ??= [];
            _simBaseDocumentXML.WorldBaseFlight.SimMissionOpenWindowAction.Add(owa);
        }

        public void SetUIPanelWindow(int index, string descr, string locked, string mouseI, string panel, string docked, string keyboardI)
        {
            descr = $"{descr}{index:00}";
            SimMissionUIPanelWindow upw = new(descr, locked, mouseI, GetGUID(), panel, docked, keyboardI);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionUIPanelWindow != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionUIPanelWindow.Add(upw);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionUIPanelWindow = [upw];
        }

        static public string[] GetWindowParameters(int windowWidth, int windowHeight, WindowAlignment alignment, int monitorWidth, int monitorHeight, int offset)
        {
            int horizontalOffset, verticalOffset;
            // Offsets
            if (alignment == WindowAlignment.TopLeft)
            {
                horizontalOffset = offset;
                verticalOffset = offset;
            }
            else if (alignment == WindowAlignment.TopRight)
            {
                horizontalOffset = monitorWidth - offset - windowWidth;
                verticalOffset = offset;
            }
            else if (alignment == WindowAlignment.BottomRight)
            {
                horizontalOffset = monitorWidth - offset - windowWidth;
                verticalOffset = monitorHeight - offset - windowHeight;
            }
            else if (alignment == WindowAlignment.BottomLeft)
            {
                horizontalOffset = offset;
                verticalOffset = monitorHeight - offset - windowHeight;
            }
            else // alignment == "Centered"
            {
                horizontalOffset = monitorWidth / 2 - windowWidth / 2;
                verticalOffset = monitorHeight / 2 - windowHeight / 2;
            }

            return [windowWidth.ToString(), windowHeight.ToString(), horizontalOffset.ToString(), verticalOffset.ToString()];
        }

        #endregion

        #region Scenario Objects - World

        public void SetLibraryObject(int index, string descr, string mdlGUID, string worldPos, string orient, string altIsAGL, string scale, string isAct)
        {
            descr = $"{descr}{index:00}";
            SceneryObjectsLibraryObject lo = new(descr, mdlGUID, worldPos, orient, altIsAGL, scale, GetGUID(), isAct);
            if (_simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject != null)
                _simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject.Add(lo);
            else
                _simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject = [lo];
        }

        public void SetPointOfInterest(int index, string objName, string search, string offsetXYZ, string curSel, string activated, string targetName)
        {
            search = $"{search}{index:00}";
            targetName = $"{targetName}{index:00}";

            ObjectReference or = GetObjectReference(objName, search);

            // Create the attached object with the OffsetXYZ from your loop call
            AttachedWorldObject awo = new(or, offsetXYZ);

            SimMissionPointOfInterest poi = new()
            {
                Descr = $"POI{index:00}",
                Activated = activated,
                TargetName = targetName,
                CurrentSelection = curSel,
                CycleOrder = index,
                AttachedWorldObject = awo,
                InstanceId = GetGUID(),
                SelectedModelGuid = "{0e41be96-3a8a-49ad-a1f9-429013e27ca0}",
                UnselectedModelGuid = "{00000000-0000-0000-0000-000000000000}"
            };

            _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterest ??= [];
            _simBaseDocumentXML.WorldBaseFlight.SimMissionPointOfInterest.Add(poi);
        }

        #endregion

        #region Triggers

        public void SetProximityTrigger(int index, string descr, string activated)
        {
            descr = $"{descr}{index:00}";
            List<ObjectReference> aList = [];
            List<ObjectReference> enterList = [];
            List<ObjectReference> exitList = [];
            SimMissionProximityTrigger pt = new(descr, new Areas(aList), new OnEnterActions(enterList), GetGUID(), activated, new OnExitActions(exitList));
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.Add(pt);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger = [pt];
        }

        public void SetProximityTriggerArea(int index, string objName, string orSearch, string tSearch)
        {
            tSearch = $"{tSearch}{index:00}";
            ObjectReference or = GetObjectReference(objName, orSearch);
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(o => o.Descr == tSearch);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].Areas != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].Areas.ObjectReference.Add(or);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].Areas = new Areas([or]);
        }

        public void SetProximityTriggerOnEnterAction(int oIndex, string objName, string orSearch, int tIndex, string tSearch)
        {
            orSearch = $"{orSearch}{oIndex:00}";
            tSearch = $"{tSearch}{tIndex:00}";
            ObjectReference or = GetObjectReference(objName, orSearch);
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(o => o.Descr == tSearch);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions.ObjectReference.Add(or);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions = new OnEnterActions([or]);
        }

        public void SetTimerTrigger(string descr, double stopTime, string timer, string activated)
        {
            List<ObjectReference> orList = [];
            SimMissionTimerTrigger tt = new(descr, stopTime, timer, activated, GetGUID(), new Actions(orList));
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger.Add(tt);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger = [tt];
        }

        public void SetTimerTriggerAction(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            int idIndex;
            idIndex = _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger.FindIndex(o => o.Descr == tSearch);
            if (_simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger[idIndex].Actions != null)
                _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger[idIndex].Actions.ObjectReference.Add(or);
            else
                _simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger[idIndex].Actions = new Actions([or]);
        }

        #endregion
    }
}
