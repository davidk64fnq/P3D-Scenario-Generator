using P3D_Scenario_Generator.ConstantsEnums;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator.Services
{
    internal class ScenarioXML()
    {
        private readonly SimBaseDocumentXML _simBaseDocumentXML = new()
        {
            WorldBaseFlight = new WorldBaseFlight()
        };

        private WorldBaseFlight Flight => _simBaseDocumentXML.WorldBaseFlight ??= new WorldBaseFlight();

        #region Actions

        public void SetAreaLandingTriggerAction(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionAreaLandingTrigger == null) return;

            int idIndex = Flight.SimMissionAreaLandingTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionAreaLandingTrigger[idIndex];
            if (trigger.Actions?.ObjectReference != null)
                trigger.Actions.ObjectReference.Add(or);
            else
                trigger.Actions = new Actions([or]);
        }

        public void SetDialogAction(string descr, string text, string delay, string soundType)
        {
            SimMissionDialogAction da = new(descr, text, delay, soundType, GetGUID());
            if (Flight.SimMissionDialogAction != null)
                Flight.SimMissionDialogAction.Add(da);
            else
                Flight.SimMissionDialogAction = [da];
        }

        public void SetGoalResolutionAction(string search)
        {
            ObjectReference or = GetObjectReference("Goal", search);
            List<ObjectReference> orList = [or];
            SimMissionGoalResolutionAction gra = new("Completed", search, new Goals(orList), GetGUID());
            if (Flight.SimMissionGoalResolutionAction != null)
                Flight.SimMissionGoalResolutionAction.Add(gra);
            else
                Flight.SimMissionGoalResolutionAction = [gra];
        }

        public void SetObjectActivationAction(int index, string objName, string search, string descr, string newObjectState)
        {
            search = $"{search}{index:00}";
            descr = $"{descr}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
            ObjectReferenceList orList = new([or]);
            SimMissionObjectActivationAction oaa = new(descr, orList, GetGUID(), newObjectState);
            if (Flight.SimMissionObjectActivationAction != null)
                Flight.SimMissionObjectActivationAction.Add(oaa);
            else
                Flight.SimMissionObjectActivationAction = [oaa];
        }

        public void SetOneShotSoundAction(int index, string descr, string soundFile)
        {
            descr = $"{descr}{index:00}";
            SimMissionOneShotSoundAction ossa = new(descr, soundFile, GetGUID());
            if (Flight.SimMissionOneShotSoundAction != null)
                Flight.SimMissionOneShotSoundAction.Add(ossa);
            else
                Flight.SimMissionOneShotSoundAction = [ossa];
        }

        public void SetPOIactivationAction(int index, string objName, string search, string descr, string newObjectState)
        {
            search = $"{search}{index:00}";
            descr = $"{descr}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
            ObjectReferenceList orList = new([or]);
            SimMissionPointOfInterestActivationAction paa = new(descr, orList, GetGUID(), newObjectState);
            if (Flight.SimMissionPointOfInterestActivationAction != null)
                Flight.SimMissionPointOfInterestActivationAction.Add(paa);
            else
                Flight.SimMissionPointOfInterestActivationAction = [paa];
        }

        public void SetScriptActions(string[] scripts)
        {
            List<SimMissionScriptAction> saList = [];

            for (int index = 0; index < scripts.Length; index++)
            {
                string actionName = $"ScriptAction{index + 1:D2}";
                saList.Add(new SimMissionScriptAction(actionName, scripts[index], GetGUID()));
            }

            Flight.SimMissionScriptAction = saList;
        }

        #endregion

        #region Airports

        public void SetAirportLandingTrigger(string descr, string landingType, string activated, string airportIdent)
        {
            SimMissionAirportLandingTrigger alt = new()
            {
                Descr = descr,
                LandingType = landingType,
                Activated = activated,
                Actions = new Actions([]),
                InstanceId = GetGUID(),
                AirportIdent = airportIdent,
                RunwayFilter = null
            };

            Flight.SimMissionAirportLandingTrigger ??= [];
            Flight.SimMissionAirportLandingTrigger.Add(alt);
        }

        public void SetAirportLandingTriggerAction(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionAirportLandingTrigger == null) return;

            int idIndex = Flight.SimMissionAirportLandingTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionAirportLandingTrigger[idIndex];
            if (trigger.Actions?.ObjectReference != null)
                trigger.Actions.ObjectReference.Add(or);
            else
                trigger.Actions = new Actions([or]);
        }


        public void SetDisabledTrafficAirports(string airportIdent)
        {
            SimMissionDisabledTrafficAirports dta = new(airportIdent);
            Flight.SimMissionDisabledTrafficAirports = dta;
        }

        #endregion

        #region Area Definitions

        public void SetCylinderArea(int index, string descr, string orientation, string radius, string height, string drawStyle)
        {
            descr = $"{descr}{index:00}";
            SimMissionCylinderArea ca = new(descr, orientation, radius, height, drawStyle, new AttachedWorldPosition(), GetGUID());
            if (Flight.SimMissionCylinderArea != null)
                Flight.SimMissionCylinderArea.Add(ca);
            else
                Flight.SimMissionCylinderArea = [ca];
        }

        public void SetRectangleArea(string descr, string orientation, string length, string width, string height)
        {
            SimMissionRectangleArea ra = new(descr, orientation, length, width, height, new AttachedWorldPosition(), GetGUID());
            if (Flight.SimMissionRectangleArea != null)
                Flight.SimMissionRectangleArea.Add(ra);
            else
                Flight.SimMissionRectangleArea = [ra];
        }

        public void SetSphereArea(string descr, string areaRadius)
        {
            SimMissionSphereArea sa = new(descr, areaRadius, new AttachedWorldPosition(), GetGUID());
            if (Flight.SimMissionSphereArea != null)
                Flight.SimMissionSphereArea.Add(sa);
            else
                Flight.SimMissionSphereArea = [sa];
        }

        public void SetAttachedWorldPosition(string objName, string search, AttachedWorldPosition wp)
        {
            int idIndex;
            switch (objName)
            {
                case "CylinderArea":
                    if (Flight.SimMissionCylinderArea == null) break;
                    idIndex = Flight.SimMissionCylinderArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) Flight.SimMissionCylinderArea[idIndex].AttachedWorldPosition = wp;
                    break;
                case "RectangleArea":
                    if (Flight.SimMissionRectangleArea == null) break;
                    idIndex = Flight.SimMissionRectangleArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) Flight.SimMissionRectangleArea[idIndex].AttachedWorldPosition = wp;
                    break;
                case "SphereArea":
                    if (Flight.SimMissionSphereArea == null) break;
                    idIndex = Flight.SimMissionSphereArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) Flight.SimMissionSphereArea[idIndex].AttachedWorldPosition = wp;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Goals

        public void SetGoal(string descr, string text)
        {
            SimMissionGoal g = new(descr, text, GetGUID());
            if (Flight.SimMissionGoal != null)
                Flight.SimMissionGoal.Add(g);
            else
                Flight.SimMissionGoal = [g];
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
                    if (Flight.SimMissionAirportLandingTrigger == null) break;
                    idIndex = Flight.SimMissionAirportLandingTrigger.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionAirportLandingTrigger[idIndex].InstanceId);
                    break;
                case "AreaLandingTrigger":
                    if (Flight.SimMissionAreaLandingTrigger == null) break;
                    idIndex = Flight.SimMissionAreaLandingTrigger.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionAreaLandingTrigger[idIndex].InstanceId);
                    break;
                case "CloseWindowAction":
                    if (Flight.SimMissionCloseWindowAction == null) break;
                    idIndex = Flight.SimMissionCloseWindowAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionCloseWindowAction[idIndex].InstanceId);
                    break;
                case "CylinderArea":
                    if (Flight.SimMissionCylinderArea == null) break;
                    idIndex = Flight.SimMissionCylinderArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionCylinderArea[idIndex].InstanceId);
                    break;
                case "DialogAction":
                    if (Flight.SimMissionDialogAction == null) break;
                    idIndex = Flight.SimMissionDialogAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionDialogAction[idIndex].InstanceId);
                    break;
                case "Goal":
                    if (Flight.SimMissionGoal == null) break;
                    idIndex = Flight.SimMissionGoal.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionGoal[idIndex].InstanceId);
                    break;
                case "GoalResolutionAction":
                    if (Flight.SimMissionGoalResolutionAction == null) break;
                    idIndex = Flight.SimMissionGoalResolutionAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionGoalResolutionAction[idIndex].InstanceId);
                    break;
                case "LibraryObject":
                    if (Flight.SceneryObjectsLibraryObject == null) break;
                    idIndex = Flight.SceneryObjectsLibraryObject.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SceneryObjectsLibraryObject[idIndex].InstanceId);
                    break;
                case "ObjectActivationAction":
                    if (Flight.SimMissionObjectActivationAction == null) break;
                    idIndex = Flight.SimMissionObjectActivationAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionObjectActivationAction[idIndex].InstanceId);
                    break;
                case "OneShotSoundAction":
                    if (Flight.SimMissionOneShotSoundAction == null) break;
                    idIndex = Flight.SimMissionOneShotSoundAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionOneShotSoundAction[idIndex].InstanceId);
                    break;
                case "OnScreenText":
                    if (Flight.SimMissionOnScreenText == null) break;
                    idIndex = Flight.SimMissionOnScreenText.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionOnScreenText[idIndex].InstanceId);
                    break;
                case "OpenWindowAction":
                    if (Flight.SimMissionOpenWindowAction == null) break;
                    idIndex = Flight.SimMissionOpenWindowAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionOpenWindowAction[idIndex].InstanceId);
                    break;
                case "PointOfInterest":
                    if (Flight.SimMissionPointOfInterest == null) break;
                    idIndex = Flight.SimMissionPointOfInterest.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionPointOfInterest[idIndex].InstanceId);
                    break;
                case "PointOfInterestActivationAction":
                    if (Flight.SimMissionPointOfInterestActivationAction == null) break;
                    idIndex = Flight.SimMissionPointOfInterestActivationAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionPointOfInterestActivationAction[idIndex].InstanceId);
                    break;
                case "ProximityTrigger":
                    if (Flight.SimMissionProximityTrigger == null) break;
                    idIndex = Flight.SimMissionProximityTrigger.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionProximityTrigger[idIndex].InstanceId);
                    break;
                case "RectangleArea":
                    if (Flight.SimMissionRectangleArea == null) break;
                    idIndex = Flight.SimMissionRectangleArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionRectangleArea[idIndex].InstanceId);
                    break;
                case "ScaleformPanelWindow":
                    if (Flight.SimMissionScaleformPanelWindow == null) break;
                    idIndex = Flight.SimMissionScaleformPanelWindow.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionScaleformPanelWindow[idIndex].InstanceId);
                    break;
                case "ScriptAction":
                    if (Flight.SimMissionScriptAction == null) break;
                    idIndex = Flight.SimMissionScriptAction.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionScriptAction[idIndex].InstanceId);
                    break;
                case "SphereArea":
                    if (Flight.SimMissionSphereArea == null) break;
                    idIndex = Flight.SimMissionSphereArea.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionSphereArea[idIndex].InstanceId);
                    break;
                case "TimerTrigger":
                    if (Flight.SimMissionTimerTrigger == null) break;
                    idIndex = Flight.SimMissionTimerTrigger.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionTimerTrigger[idIndex].InstanceId);
                    break;
                case "UIPanelWindow":
                    if (Flight.SimMissionUIPanelWindow == null) break;
                    idIndex = Flight.SimMissionUIPanelWindow.FindIndex(o => o.Descr == search);
                    if (idIndex >= 0) or = new(Flight.SimMissionUIPanelWindow[idIndex].InstanceId);
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

        public static string GetCoordinateWorldPosition(double latitude, double longitude, double elevation)
        {
            return $"{ScenarioFXML.FormatCoordXML(latitude, "N", "S", false)},{ScenarioFXML.FormatCoordXML(longitude, "E", "W", false)},+{elevation}";
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
            if (Flight.SimMissionScenarioVariable != null)
                Flight.SimMissionScenarioVariable.Add(sv);
            else
                Flight.SimMissionScenarioVariable = [sv];
        }

        public void SetScenarioVariableAction(string objName, string orSearch, int tcIndex, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            List<ObjectReference> orList = [or];
            Actions a = new(orList);
            List<Actions> aList = [a];

            if (Flight.SimMissionScenarioVariable == null) return;
            int idIndex = Flight.SimMissionScenarioVariable.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            if (Flight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions.Count != 0)
                Flight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions[0].ObjectReference.Add(or);
            else
                Flight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].Actions = aList;
        }

        public void SetScenarioVariableTriggerValue(double value, int tcIndex, string tSearch)
        {
            Constant constant = new(value);
            TriggerValue tv = new(constant);

            if (Flight.SimMissionScenarioVariable == null) return;
            int idIndex = Flight.SimMissionScenarioVariable.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            if (value == 0)
                Flight.SimMissionScenarioVariable[idIndex].TriggerCondition = null!;
            else
                Flight.SimMissionScenarioVariable[idIndex].TriggerCondition[tcIndex].TriggerValue = tv;
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
            XmlSerializerNamespaces ns = new();
            ns.Add("", "");

            XmlSerializer xmlSerializer = new(_simBaseDocumentXML.GetType());
            string filePath = Path.Combine(formData.ScenarioFolder, $"{formData.ScenarioTitle}.xml");

            using StreamWriter writer = new(filePath);
            xmlSerializer.Serialize(writer, _simBaseDocumentXML, ns);
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
            Flight.SimMissionRealismOverrides = ro;
        }

        public void SetScenarioMetadata(ScenarioFormData formData, Overview overview)
        {
            var destination = formData.DestinationRunway;
            string locationDescr = destination != null
                ? $"{destination.IcaoName} ({destination.IcaoId}) {destination.City}, {destination.Country}"
                : "Unknown Airport";

            SimMissionUIScenarioMetadata md = new()
            {
                InstanceId = GetGUID(),
                SkillLevel = overview.Difficulty,
                LocationDescr = locationDescr,
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
            Flight.SimMissionUIScenarioMetadata = md;
        }

        #endregion

        #region Scenario Objects - UI

        public void SetCloseWindowAction(int index, string objName, string search)
        {
            search = $"{search}{index:00}";
            ObjectReference or = GetObjectReference(objName, search);
            SimMissionCloseWindowAction cwa = new($"Close{search}", or, GetGUID());
            if (Flight.SimMissionCloseWindowAction != null)
                Flight.SimMissionCloseWindowAction.Add(cwa);
            else
                Flight.SimMissionCloseWindowAction = [cwa];
        }

        public void SetOnScreenText(string descr, string text, string onScrLoc, string RGBcol, string activated, string backCol)
        {
            SimMissionOnScreenText ost = new(descr, text, onScrLoc, RGBcol, activated, backCol, GetGUID());
            if (Flight.SimMissionOnScreenText != null)
                Flight.SimMissionOnScreenText.Add(ost);
            else
                Flight.SimMissionOnScreenText = [ost];
        }

        public void SetOpenWindowAction(int index, string objName, string search, string[] windowParameters, string monitorNo)
        {
            search = $"{search}{index:00}";

            SetWindowSize sws = new(int.Parse(windowParameters[0]), int.Parse(windowParameters[1]));
            SetWindowLocation swl = new(int.Parse(windowParameters[2]), int.Parse(windowParameters[3]));

            ObjectReference or = GetObjectReference(objName, search);

            SimMissionOpenWindowAction owa = new()
            {
                Descr = $"Open{search}",
                SetWindowSize = sws,
                SetWindowLocation = swl,
                RelativeTo = monitorNo,
                ObjectReference = or,
                InstanceId = GetGUID()
            };

            Flight.SimMissionOpenWindowAction ??= [];
            Flight.SimMissionOpenWindowAction.Add(owa);
        }

        public void SetUIPanelWindow(int index, string descr, string locked, string mouseI, string panel, string docked, string keyboardI)
        {
            descr = $"{descr}{index:00}";
            SimMissionUIPanelWindow upw = new(descr, locked, mouseI, GetGUID(), panel, docked, keyboardI);
            if (Flight.SimMissionUIPanelWindow != null)
                Flight.SimMissionUIPanelWindow.Add(upw);
            else
                Flight.SimMissionUIPanelWindow = [upw];
        }

        public static string[] GetWindowParameters(int windowWidth, int windowHeight, WindowAlignment alignment, int monitorWidth, int monitorHeight, int offset)
        {
            int horizontalOffset, verticalOffset;
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
            else
            {
                horizontalOffset = (monitorWidth / 2) - (windowWidth / 2);
                verticalOffset = (monitorHeight / 2) - (windowHeight / 2);
            }

            return [windowWidth.ToString(), windowHeight.ToString(), horizontalOffset.ToString(), verticalOffset.ToString()];
        }

        #endregion

        #region Scenario Objects - World

        public void SetLibraryObject(int index, string descr, string mdlGUID, string worldPos, string orient, string altIsAGL, string scale, string isAct)
        {
            descr = $"{descr}{index:00}";
            SceneryObjectsLibraryObject lo = new(descr, mdlGUID, worldPos, orient, altIsAGL, scale, GetGUID(), isAct);
            if (Flight.SceneryObjectsLibraryObject != null)
                Flight.SceneryObjectsLibraryObject.Add(lo);
            else
                Flight.SceneryObjectsLibraryObject = [lo];
        }

        public void SetPointOfInterest(int index, string objName, string search, string offsetXYZ, string curSel, string activated, string targetName)
        {
            search = $"{search}{index:00}";
            targetName = $"{targetName}{index:00}";

            ObjectReference or = GetObjectReference(objName, search);
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

            Flight.SimMissionPointOfInterest ??= [];
            Flight.SimMissionPointOfInterest.Add(poi);
        }

        #endregion

        #region Triggers

        public void SetAreaLandingTrigger(string descr, string landingType, string activated)
        {
            SimMissionAreaLandingTrigger alt = new()
            {
                Descr = descr,
                LandingType = landingType,
                Activated = activated,
                Areas = new Areas([]),
                InstanceId = GetGUID()
            };

            Flight.SimMissionAreaLandingTrigger ??= [];
            Flight.SimMissionAreaLandingTrigger.Add(alt);
        }

        public void SetAreaLandingTriggerArea(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionAreaLandingTrigger == null) return;

            int idIndex = Flight.SimMissionAreaLandingTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionAreaLandingTrigger[idIndex];
            if (trigger.Areas?.ObjectReference != null)
                trigger.Areas.ObjectReference.Add(or);
            else
                trigger.Areas = new Areas([or]);
        }

        public void SetProximityTrigger(int index, string descr, string activated)
        {
            descr = $"{descr}{index:00}";
            List<ObjectReference> aList = [];
            List<ObjectReference> enterList = [];
            List<ObjectReference> exitList = [];
            SimMissionProximityTrigger pt = new(descr, new Areas(aList), new OnEnterActions(enterList), GetGUID(), activated, new OnExitActions(exitList));
            if (Flight.SimMissionProximityTrigger != null)
                Flight.SimMissionProximityTrigger.Add(pt);
            else
                Flight.SimMissionProximityTrigger = [pt];
        }

        public void SetProximityTriggerArea(int index, string objName, string orSearch, string tSearch)
        {
            tSearch = $"{tSearch}{index:00}";
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionProximityTrigger == null) return;

            int idIndex = Flight.SimMissionProximityTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionProximityTrigger[idIndex];
            if (trigger.Areas?.ObjectReference != null)
                trigger.Areas.ObjectReference.Add(or);
            else
                trigger.Areas = new Areas([or]);
        }

        public void SetProximityTriggerOnEnterAction(int oIndex, string objName, string orSearch, int tIndex, string tSearch)
        {
            orSearch = $"{orSearch}{oIndex:00}";
            tSearch = $"{tSearch}{tIndex:00}";
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionProximityTrigger == null) return;

            int idIndex = Flight.SimMissionProximityTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionProximityTrigger[idIndex];
            if (trigger.OnEnterActions?.ObjectReference != null)
                trigger.OnEnterActions.ObjectReference.Add(or);
            else
                trigger.OnEnterActions = new OnEnterActions([or]);
        }


        public void SetTimerTrigger(string descr, double stopTime, string timer, string activated)
        {
            List<ObjectReference> orList = [];
            SimMissionTimerTrigger tt = new(descr, stopTime, timer, activated, GetGUID(), new Actions(orList));
            if (Flight.SimMissionTimerTrigger != null)
                Flight.SimMissionTimerTrigger.Add(tt);
            else
                Flight.SimMissionTimerTrigger = [tt];
        }

        public void SetTimerTriggerAction(string objName, string orSearch, string tSearch)
        {
            ObjectReference or = GetObjectReference(objName, orSearch);
            if (Flight.SimMissionTimerTrigger == null) return;

            int idIndex = Flight.SimMissionTimerTrigger.FindIndex(o => o.Descr == tSearch);
            if (idIndex < 0) return;

            var trigger = Flight.SimMissionTimerTrigger[idIndex];
            if (trigger.Actions?.ObjectReference != null)
                trigger.Actions.ObjectReference.Add(or);
            else
                trigger.Actions = new Actions([or]);
        }

        #endregion
    }
}