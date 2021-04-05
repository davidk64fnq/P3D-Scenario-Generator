

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace P3D_Scenario_Generator
{
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

	public class ScenarioXML
    {
		static private List<Gate> gates;
		static private readonly double[] circuitHeadingAdj = { 360, 90, 90, 180, 180, 270, 270, 360 };

		static internal void GenerateXMLfile(Runway runway, Params parameters)
		{
			SimBaseDocumentXML simBaseDocumentXML = new SimBaseDocumentXML();
			WorldBaseFlight worldBaseFlight = new WorldBaseFlight();
			simBaseDocumentXML.WorldBaseFlight = worldBaseFlight;
			gates = GateCalcs.SetGateCoords(runway, parameters);
			SetXML(runway, simBaseDocumentXML, parameters);
			WriteXML(simBaseDocumentXML, parameters);
		}

		static private void SetXML(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			SetOneShotSoundAction(simBaseDocumentXML, parameters);
			SetDialogAction(simBaseDocumentXML, parameters);
			SetLibraryObject(runway, simBaseDocumentXML, parameters);
			SetRectangleArea(runway, simBaseDocumentXML, parameters);
			SetObjectActivationAction(simBaseDocumentXML, parameters);
			SetScenarioMetadata(runway, simBaseDocumentXML, parameters);
			SetRealismOverrides(simBaseDocumentXML);
			SetGoal(simBaseDocumentXML, parameters);
			SetGoalResolutionAction(simBaseDocumentXML, parameters);
			SetAirportLandingTrigger(runway, simBaseDocumentXML);
			SetTimerTrigger(simBaseDocumentXML);
			SetProximityTrigger(simBaseDocumentXML);
			SetTriggerActivationAction(simBaseDocumentXML, parameters);
			SetTimerTriggerFirstGate(simBaseDocumentXML);
			SetAirportLandingTriggerActivation(simBaseDocumentXML);
			SetLastGateLandingTrigger(simBaseDocumentXML);
			SetProximityTriggerActivation(simBaseDocumentXML);
			SetDisabledTrafficAirports(runway, simBaseDocumentXML);
		}

		static private void SetOneShotSoundAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
            List<SimMissionOneShotSoundAction> saList = new List<SimMissionOneShotSoundAction>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					saList.Add(new SimMissionOneShotSoundAction("OneShotSound_ThruHoop_01", "ThruHoop.wav", GetGUID()));
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction = saList;
		}

		static private void SetDialogAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
        {
			List<SimMissionDialogAction> daList = new List<SimMissionDialogAction>();
			switch (parameters.selectedScenario)
            {
				case nameof(ScenarioTypes.Circuit):
					daList.Add(new SimMissionDialogAction("Dialog_Intro_01", ScenarioHTML.GetBriefing(), "2", "Text-To-Speech", GetGUID()));
					daList.Add(new SimMissionDialogAction("Dialog_Intro_02", ScenarioHTML.GetTips(), "2", "Text-To-Speech", GetGUID()));
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction = daList;
		}

		static private void SetLibraryObject(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SceneryObjectsLibraryObject> loList = new List<SceneryObjectsLibraryObject>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					SetGateLibraryObjects(runway, parameters, loList, circuitHeadingAdj);
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject = loList;
		}

		static private void SetGateLibraryObjects(Runway runway, Params parameters, List<SceneryObjectsLibraryObject> loList, double[] headingAdj)
		{
            for (int index = 0; index < gates.Count; index++)
            {
				string orientation = SetOrientation(runway, headingAdj[index]);

				// Number objects
				string descr = Constants.genGameNumBlueDesc.Replace("X", (index + 1).ToString());
				string mdlGUID = Constants.genGameNumBlueMDLguid[index];
				double vOffset = Constants.genGameNumBlueVertOffset;
				string worldPosition = SetWorldPosition(gates[index], parameters, vOffset);
				loList.Add(new SceneryObjectsLibraryObject(descr, mdlGUID, worldPosition, orientation, "True", "1", GetGUID(), "True"));

				// Hoop active objects
				descr = Constants.genGameHoopNumActiveDesc.Replace("X", (index + 1).ToString());
				mdlGUID = Constants.genGameHoopNumActiveMDLguid;
				vOffset = Constants.genGameHoopNumActiveVertOffset;
				worldPosition = SetWorldPosition(gates[index], parameters, vOffset);
				loList.Add(new SceneryObjectsLibraryObject(descr, mdlGUID, worldPosition, orientation, "True", "1", GetGUID(), "True"));

				// Hoop inactive objects
				descr = Constants.genGameHoopNumInactiveDesc.Replace("X", (index + 1).ToString());
				mdlGUID = Constants.genGameHoopNumInactiveMDLguid;
				vOffset = Constants.genGameHoopNumInactiveVertOffset;
				worldPosition = SetWorldPosition(gates[index], parameters, vOffset);
				loList.Add(new SceneryObjectsLibraryObject(descr, mdlGUID, worldPosition, orientation, "True", "1", GetGUID(), "True"));
			}
		}

		static private void SetRectangleArea(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SimMissionRectangleArea> raList = new List<SimMissionRectangleArea>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					SetGateRectangleAreas(runway, parameters, raList, circuitHeadingAdj);
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea = raList;
		}

		static private void SetGateRectangleAreas(Runway runway, Params parameters, List<SimMissionRectangleArea> raList, double[] headingAdj)
		{
			for (int index = 0; index < gates.Count; index++)
			{
				string descr = $"Area_Hoop_0{index + 1}";
				string orientation = SetOrientation(runway, headingAdj[index]);
				double vOffset = Constants.genGameHoopNumActiveVertOffset;
				string worldPosition = SetWorldPosition(gates[index], parameters, vOffset);
				AttachedWorldPosition wp = new AttachedWorldPosition(worldPosition, "True");
				raList.Add(new SimMissionRectangleArea(descr, orientation, "100.0", "25.0", "100.0", wp, GetGUID()));
			}
		}

		static private string SetWorldPosition(Gate gate, Params parameters, double vertOffset)
        {
			return $"{ScenarioFXML.FormatCoordXML(gate.lat, "N", "S")},{ScenarioFXML.FormatCoordXML(gate.lon, "E", "W")},+{parameters.height + vertOffset}";
		}

		static private string SetOrientation(Runway runway, double headingAdj)
		{
			return $"0.0,0.0,{string.Format("{0:0.0}", (runway.hdg + runway.magVar + headingAdj) % 360)}";
		}

		static private void SetObjectActivationAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SimMissionObjectActivationAction> oaaList = new List<SimMissionObjectActivationAction>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					SetGateObjectActivations(simBaseDocumentXML, Constants.genGameHoopNumActiveDesc, oaaList, "Activate_Hoop_Active_0", "True");
					SetGateObjectActivations(simBaseDocumentXML, Constants.genGameHoopNumActiveDesc, oaaList, "Deactivate_Hoop_Active_0", "False");
					SetGateObjectActivations(simBaseDocumentXML, Constants.genGameHoopNumInactiveDesc, oaaList, "Activate_Hoop_Inactive_0", "True");
					SetGateObjectActivations(simBaseDocumentXML, Constants.genGameHoopNumInactiveDesc, oaaList, "Deactivate_Hoop_Inactive_0", "False");
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction = oaaList;
		}

		static private void SetGateObjectActivations(SimBaseDocumentXML simBaseDocumentXML, string objectName, List<SimMissionObjectActivationAction> oaaList, string descr, string newObjectState)
		{
			for (int index = 0; index < gates.Count; index++)
            {
				string search = objectName.Replace("X", (index + 1).ToString());
				int idIndex = simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject.FindIndex(lo => lo.Descr == search);
				ObjectReference or = new ObjectReference
				{
					InstanceId = simBaseDocumentXML.WorldBaseFlight.SceneryObjectsLibraryObject[idIndex].InstanceId
				};
                List<ObjectReference> orList = new List<ObjectReference>
                {
                    or
                };
                ObjectReferenceList orl = new ObjectReferenceList
                {
                    ObjectReference = orList
                };
                SimMissionObjectActivationAction oaa = new SimMissionObjectActivationAction
				{
					InstanceId = GetGUID(),
					Descr = $"{descr}{index + 1}",
					NewObjectState = newObjectState,
					ObjectReferenceList = orl
				};
				oaaList.Add(oaa);
			}
		}

		static private void SetScenarioMetadata(Runway runway, SimBaseDocumentXML simBaseDocumentXML, Params parameters)
        {
            SimMissionUIScenarioMetadata md = new SimMissionUIScenarioMetadata
            {
                InstanceId = GetGUID(),
                SkillLevel = ScenarioHTML.GetDifficulty(),
                LocationDescr = $"{runway.icaoName} ({runway.icaoId}) {runway.city}, {runway.country}",
                DifficultyLevel = 1,
                EstimatedTime = ScenarioHTML.GetDuration(),
                UncompletedImage = "images\\imgM_i.bmp",
                CompletedImage = "images\\imgM_c.bmp",
                ExitMissionImage = "images\\exitMission.bmp",
                MissionBrief = "Overview.htm",
                AbbreviatedMissionBrief = $"{Path.GetFileNameWithoutExtension(parameters.saveLocation)}.htm",
                SuccessMessage = $"Success! You completed the \"{parameters.selectedScenario}\" scenario objectives.",
                FailureMessage = $"Better luck next time! You failed to complete the \"{parameters.selectedScenario}\" scenario objectives.",
                UserCrashMessage = $"Yikes! You crashed and therefore failed the \"{parameters.selectedScenario}\" scenario objectives."
            };
            simBaseDocumentXML.WorldBaseFlight.SimMissionUIScenarioMetadata = md;
		}

		static private void SetRealismOverrides(SimBaseDocumentXML simBaseDocumentXML)
		{
            SimMissionRealismOverrides ro = new SimMissionRealismOverrides
            {
                Descr = "RealismOverrides",
                UserTips = "UserSpecified",
                CrashBehavior = "UserSpecified",
                ATCMenuDisabled = "False",
                FlightRealism = "UserSpecified",
                WorldRealism = "UserSpecified",
                AircraftLabels = "UserSpecified",
                AvatarNoCollision = "UserSpecified",
                UnlimitedFuel = "UserSpecified"
            };
            simBaseDocumentXML.WorldBaseFlight.SimMissionRealismOverrides = ro;
		}

		static private void SetGoal(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SimMissionGoal> gList = new List<SimMissionGoal>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					gList.Add(new SimMissionGoal("Goal_1", ScenarioHTML.GetObjective(), GetGUID()));
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionGoal = gList;
		}

		static private void SetGoalResolutionAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SimMissionGoalResolutionAction> graList = new List<SimMissionGoalResolutionAction>();
			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					string search = "Goal_1";
					int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionGoal.FindIndex(g => g.Descr == search);
					List<ObjectReference> orList = new List<ObjectReference>
					{
						new ObjectReference(simBaseDocumentXML.WorldBaseFlight.SimMissionGoal[idIndex].InstanceId)
					};
					SimMissionGoalResolutionAction gra = new SimMissionGoalResolutionAction
					{
						Descr = "Resolve_Goal_1",
						GoalResolution = "Completed",
						InstanceId = GetGUID(),
						Goals = new Goals(orList)
					};
					graList.Add(gra);
					break;
				default:
					break;
			}
			simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction = graList;
		}

		static private void SetAirportLandingTrigger(Runway runway, SimBaseDocumentXML simBaseDocumentXML)
		{
			RunwayFilter rf = new RunwayFilter
			{
				RunwayNumber = runway.id
			};
			List<ObjectReference> orList = new List<ObjectReference>();
			SetGoalResolutionReference(simBaseDocumentXML, "Resolve_Goal_X", 1, orList);
			Actions a = new Actions
			{
				ObjectReference = orList
			};
			SimMissionAirportLandingTrigger alt = new SimMissionAirportLandingTrigger
			{
				InstanceId = GetGUID(),
				Descr = "Airport_Landing_Trigger_01",
				Activated = "False",
				AirportIdent = runway.icaoId,
				RunwayFilter = rf,
				Actions = a
			};
			List<SimMissionAirportLandingTrigger> altList = simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger;
			altList.Clear();
			altList.Add(alt);
		}

		static private void SetTimerTrigger(SimBaseDocumentXML simBaseDocumentXML)
		{
			List<ObjectReference> orList = new List<ObjectReference>();
			SetObjectActivationReference(simBaseDocumentXML, "Activate_Hoop_Active_0X", 1, orList);
			SetObjectActivationReference(simBaseDocumentXML, "Deactivate_Hoop_Inactive_0X", 1, orList);
			SetDialogReference(simBaseDocumentXML, "Dialog_Intro_0X", 1, orList);
			SetDialogReference(simBaseDocumentXML, "Dialog_Intro_0X", 2, orList);
			Actions a = new Actions
			{
				ObjectReference = orList
			};
			SimMissionTimerTrigger tt = new SimMissionTimerTrigger
			{
				InstanceId = GetGUID(),
				Descr = "Timer_Trigger_01",
				StopTime = 1.0,
				Activated = "True",
				Actions = a
			};
			List<SimMissionTimerTrigger> ttList = simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger;
			ttList.Clear();
			ttList.Add(tt);
		}

		static private void SetProximityTrigger(SimBaseDocumentXML simBaseDocumentXML)
		{
			List<SimMissionProximityTrigger> ptList = simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger;
			ptList.Clear();
			for (int index = 0; index < gates.Count; index++)
            {
				List<ObjectReference> orAreaList = new List<ObjectReference>();
				SetRectangleAreaReference(simBaseDocumentXML, "Area_Hoop_0X", index + 1, orAreaList);
				Areas a = new Areas
				{
					ObjectReference = orAreaList
				};
				List<ObjectReference> orActionList = new List<ObjectReference>();
				SetObjectActivationReference(simBaseDocumentXML, "Activate_Hoop_Inactive_0X", index + 1, orActionList);
				SetObjectActivationReference(simBaseDocumentXML, "Deactivate_Hoop_Active_0X", index + 1, orActionList);
				if (index + 1 < gates.Count)
                {
					SetObjectActivationReference(simBaseDocumentXML, "Activate_Hoop_Active_0X", index + 2, orActionList);
					SetObjectActivationReference(simBaseDocumentXML, "Deactivate_Hoop_Inactive_0X", index + 2, orActionList);
				}
				SetSoundAction(simBaseDocumentXML, "OneShotSound_ThruHoop_0X", 1, orActionList);
				OnEnterActions oea = new OnEnterActions
				{
					ObjectReference = orActionList
				};
				SimMissionProximityTrigger pt = new SimMissionProximityTrigger
				{
					InstanceId = GetGUID(),
					Descr = $"Proximity_Trigger_0{index + 1}",
					Activated = "False",
					Areas = a,
					OnEnterActions = oea
				};
				ptList.Add(pt);
			}
		}

		static private void SetTriggerActivationAction(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
		{
			List<SimMissionObjectActivationAction> oaaList = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction;

			switch (parameters.selectedScenario)
			{
				case nameof(ScenarioTypes.Circuit):
					SetGateTriggerActivations(simBaseDocumentXML, $"Proximity_Trigger_0X", oaaList, "Activate_Proximity_Trigger_0", "True");
					SetGateTriggerActivations(simBaseDocumentXML, $"Proximity_Trigger_0X", oaaList, "Deactivate_Proximity_Trigger_0", "False");
					break;
				default:
					break;
			}
		}

		static private void SetTimerTriggerFirstGate(SimBaseDocumentXML simBaseDocumentXML)
        {
			string search = "Activate_Proximity_Trigger_01";
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.FindIndex(pt => pt.Descr == search);
            ObjectReference or = new ObjectReference
            {
                InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction[idIndex].InstanceId
            };
			search = "Timer_Trigger_01";
			idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger.FindIndex(tt => tt.Descr == search);
			simBaseDocumentXML.WorldBaseFlight.SimMissionTimerTrigger[idIndex].Actions.ObjectReference.Add(or);
		}

		static private void SetAirportLandingTriggerActivation(SimBaseDocumentXML simBaseDocumentXML)
		{
			string search = "Airport_Landing_Trigger_01";
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger.FindIndex(alt => alt.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionAirportLandingTrigger[idIndex].InstanceId
			};
			List<ObjectReference> orList = new List<ObjectReference>
			{
				or
			};
			ObjectReferenceList orl = new ObjectReferenceList
			{
				ObjectReference = orList
			};
			SimMissionObjectActivationAction oaa = new SimMissionObjectActivationAction
			{
				InstanceId = GetGUID(),
				Descr = "Activate_Airport_Landing_Trigger_01",
				NewObjectState = "True",
				ObjectReferenceList = orl
			};
			simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.Add(oaa);
		}

		static private void SetLastGateLandingTrigger(SimBaseDocumentXML simBaseDocumentXML)
		{
			string search = "Activate_Airport_Landing_Trigger_01";
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.FindIndex(oa => oa.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction[idIndex].InstanceId
			};
			search = $"Proximity_Trigger_0{gates.Count}";
			idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(pt => pt.Descr == search);
			simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions.ObjectReference.Add(or);
		}

		static private void SetGateTriggerActivations(SimBaseDocumentXML simBaseDocumentXML, string objectName, List<SimMissionObjectActivationAction> oaaList, string descr, string newObjectState)
		{
			for (int index = 0; index < gates.Count; index++)
			{
				string search = objectName.Replace("X", (index + 1).ToString());
				int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(lo => lo.Descr == search);
				ObjectReference or = new ObjectReference
				{
					InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].InstanceId
				};
				List<ObjectReference> orList = new List<ObjectReference>
				{
					or
				};
				ObjectReferenceList orl = new ObjectReferenceList
				{
					ObjectReference = orList
				};
				SimMissionObjectActivationAction oaa = new SimMissionObjectActivationAction
				{
					InstanceId = GetGUID(),
					Descr = $"{descr}{index + 1}",
					NewObjectState = newObjectState
				};
				oaa.ObjectReferenceList = orl;
				oaaList.Add(oaa);
			}
		}

		static private void SetProximityTriggerActivation(SimBaseDocumentXML simBaseDocumentXML)
		{
			for (int index = 0; index < gates.Count; index++)
			{
				List<ObjectReference> orActionList = new List<ObjectReference>();
				SetObjectActivationReference(simBaseDocumentXML, "Deactivate_Proximity_Trigger_0X", index + 1, orActionList);
				if (index + 1 < gates.Count)
				{
					SetObjectActivationReference(simBaseDocumentXML, "Activate_Proximity_Trigger_0X", index + 2, orActionList);
				}
				string search = $"Proximity_Trigger_0X".Replace("X", (index + 1).ToString());
				int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger.FindIndex(pt => pt.Descr == search);
				simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions.ObjectReference.Add(orActionList[0]);
				if (index + 1 < gates.Count)
                {
					simBaseDocumentXML.WorldBaseFlight.SimMissionProximityTrigger[idIndex].OnEnterActions.ObjectReference.Add(orActionList[1]);
                }
			}
		}

		static private void SetSoundAction(SimBaseDocumentXML simBaseDocumentXML, string objectName, int index, List<ObjectReference> orList)
		{
			string search = objectName.Replace("X", index.ToString());
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction.FindIndex(sa => sa.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionOneShotSoundAction[idIndex].InstanceId
			};
			orList.Add(or);
		}

		static private void SetRectangleAreaReference(SimBaseDocumentXML simBaseDocumentXML, string objectName, int index, List<ObjectReference> orList)
		{
			string search = objectName.Replace("X", index.ToString());
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea.FindIndex(oaa => oaa.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionRectangleArea[idIndex].InstanceId
			};
			orList.Add(or);
		}

		static private void SetObjectActivationReference(SimBaseDocumentXML simBaseDocumentXML, string objectName, int index, List<ObjectReference> orList)
		{
			string search = objectName.Replace("X", index.ToString());
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction.FindIndex(oaa => oaa.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionObjectActivationAction[idIndex].InstanceId
			};
			orList.Add(or);
		}

		static private void SetGoalResolutionReference(SimBaseDocumentXML simBaseDocumentXML, string objectName, int index, List<ObjectReference> orList)
		{
			string search = objectName.Replace("X", index.ToString());
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction.FindIndex(gra => gra.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionGoalResolutionAction[idIndex].InstanceId
			};
			orList.Add(or);
		}

		static private void SetDialogReference(SimBaseDocumentXML simBaseDocumentXML, string objectName, int index, List<ObjectReference> orList)
		{
			string search = objectName.Replace("X", index.ToString());
			int idIndex = simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction.FindIndex(da => da.Descr == search);
			ObjectReference or = new ObjectReference
			{
				InstanceId = simBaseDocumentXML.WorldBaseFlight.SimMissionDialogAction[idIndex].InstanceId
			};
			orList.Add(or);
		}

		static private void SetDisabledTrafficAirports(Runway runway, SimBaseDocumentXML simBaseDocumentXML)
		{
			SimMissionDisabledTrafficAirports ta;
			ta = simBaseDocumentXML.WorldBaseFlight.SimMissionDisabledTrafficAirports;
			ta.AirportIdent = $"{runway.icaoId}";
		}

		static private string GetGUID()
        {
			System.Guid guid = System.Guid.NewGuid();
			string guidUpper = guid.ToString().ToUpper();
			return $"{{{guidUpper}}}";
		}

		static private void WriteXML(SimBaseDocumentXML simBaseDocumentXML, Params parameters)
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
		public ObjectReference(string v1)
		{
			InstanceId = v1;
		}
		public ObjectReference()
		{
		}

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
		public SimMissionDialogAction(string v1, string v2, string v3, string v4, string v5)
		{
			Descr = v1;
			Text = v2;
			DelaySeconds = v3;
			SoundType = v4;
			InstanceId = v5;
		}
		public SimMissionDialogAction()
		{
		}

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
        public SimMissionOneShotSoundAction(string v1, string v2, string v3)
        {
			Descr = v1;
			SoundFileName = v2;
			InstanceId = v3;
		}
		public SimMissionOneShotSoundAction()
		{
		}

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
		public Goals(List<ObjectReference> v1)
		{
			ObjectReference = v1;
		}
		public Goals()
		{
		}

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.GoalResolutionAction")]
	public class SimMissionGoalResolutionAction
	{

		[XmlElement(ElementName = "GoalResolution")]
		public string GoalResolution { get; set; }

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
		public AttachedWorldPosition(string v1, string v2)
		{
			WorldPosition = v1;
			AltitudeIsAGL = v2;
		}
		public AttachedWorldPosition()
		{
		}

		[XmlElement(ElementName = "WorldPosition")]
		public string WorldPosition { get; set; }

		[XmlElement(ElementName = "AltitudeIsAGL")]
		public string AltitudeIsAGL { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.RectangleArea")]
	public class SimMissionRectangleArea
	{
		public SimMissionRectangleArea(string v1, string v2, string v3, string v4, string v5, AttachedWorldPosition v6, string v7)
		{
			Descr = v1;
			Orientation = v2;
			Length = v3;
			Width = v4;
			Height = v5;
			AttachedWorldPosition = v6;
			InstanceId = v7;
		}
		public SimMissionRectangleArea()
		{
		}

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "Orientation")]
		public string Orientation { get; set; }

		[XmlElement(ElementName = "Length")]
		public string Length { get; set; }

		[XmlElement(ElementName = "Width")]
		public string Width { get; set; }

		[XmlElement(ElementName = "Height")]
		public string Height { get; set; }

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

	[XmlRoot(ElementName = "SimMission.AirportLandingTrigger")]
	public class SimMissionAirportLandingTrigger
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

		[XmlElement(ElementName = "AirportIdent")]
		public string AirportIdent { get; set; }

		[XmlElement(ElementName = "RunwayFilter")]
		public RunwayFilter RunwayFilter { get; set; }
	}

	[XmlRoot(ElementName = "SimMission.RunwayFilter")]
	public class RunwayFilter
	{

		[XmlElement(ElementName = "Descr")]
		public string Descr { get; set; }

		[XmlElement(ElementName = "RunwayNumber")]
		public string RunwayNumber { get; set; }

		[XmlElement(ElementName = "RunwayDesignator")]
		public string RunwayDesignator { get; set; }
	}

	[XmlRoot(ElementName = "Areas")]
	public class Areas
	{

		[XmlElement(ElementName = "ObjectReference")]
		public List<ObjectReference> ObjectReference { get; set; }
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
		public SceneryObjectsLibraryObject(string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8)
		{
			Descr = v1;
			MDLGuid = v2;
			WorldPosition = v3;
			Orientation = v4;
			AltitudeIsAGL = v5;
			Scale = v6;
			InstanceId = v7;
			Activated = v8;
		}
		public SceneryObjectsLibraryObject()
		{
		}

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

		[XmlElement(ElementName = "UserTips")]
		public string UserTips { get; set; }

		[XmlElement(ElementName = "CrashBehavior")]
		public string CrashBehavior { get; set; }

		[XmlElement(ElementName = "ATCMenuDisabled")]
		public string ATCMenuDisabled { get; set; }

		[XmlElement(ElementName = "FlightRealism")]
		public string FlightRealism { get; set; }

		[XmlElement(ElementName = "WorldRealism")]
		public string WorldRealism { get; set; }

		[XmlElement(ElementName = "UnlimitedFuel")]
		public string UnlimitedFuel { get; set; }

		[XmlElement(ElementName = "AircraftLabels")]
		public string AircraftLabels { get; set; }

		[XmlElement(ElementName = "AvatarNoCollision")]
		public string AvatarNoCollision { get; set; }
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
		public SimMissionGoal(string v1, string v2, string v3)
		{
			Descr = v1;
			Text = v2;
			InstanceId = v3;
		}
		public SimMissionGoal()
		{
		}

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
		public List<SimMissionGoalResolutionAction> SimMissionGoalResolutionAction { get; set; }

		[XmlElement(ElementName = "SimMission.RectangleArea")]
		public List<SimMissionRectangleArea> SimMissionRectangleArea { get; set; }

		[XmlElement(ElementName = "SimMission.AreaLandingTrigger")]
		public List<SimMissionAreaLandingTrigger> SimMissionAreaLandingTrigger { get; set; }

		[XmlElement(ElementName = "SimMission.AirportLandingTrigger")]
		public List<SimMissionAirportLandingTrigger> SimMissionAirportLandingTrigger { get; set; }

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
		public List<SimMissionGoal> SimMissionGoal { get; set; }

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
