using Newtonsoft.Json;
using System.Xml;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Parameters for a runway sourced from the runways.xml file
    /// </summary>
    public class RunwayParams : ICloneable
    {
        /// <summary>
        /// Four letter code known as ICAO airport code or location indicator
        /// </summary>
        internal string IcaoId { get; set; }

        /// <summary>
        /// The name of the airport
        /// </summary>
        internal string IcaoName { get; set; }

        internal string Country { get; set; }

        internal string State { get; set; }

        internal string City { get; set; }

        /// <summary>
        /// The longitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLon { get; set; }

        /// <summary>
        /// The latitude of the approximate center of the airport's useable runways
        /// </summary>
        internal double AirportLat { get; set; }

        /// <summary>
        /// Airport altitude (AMSL)
        /// </summary>
        internal double Altitude { get; set; }

        /// <summary>
        /// Airport magnetic variation
        /// </summary>
        internal double MagVar { get; set; }

        /// <summary>
        /// The runway Id e.g. "05L", the two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N". The number is extracted and stored as "Number" field.
        /// The letter which distinguishes parallel runways is extracted and stored as "Designator" field.
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, two digit number is 10's of degrees so 05 is 50 degrees approximate
        /// magnetic runway heading. If the number is greater than 36 it is code for a compass heading or pair 
        /// of compass headings e.g. 37 = "N-S", 45 = "N".
        /// </summary>
        internal string Number { get; set; }

        /// <summary>
        /// See <see cref="Id"/>, one of "None", "Left", "Right", "Center", or "Water". Used in setting the airport landing trigger for a scenario
        /// </summary>
        internal string Designator { get; set; }

        /// <summary>
        /// Runway length in feet
        /// </summary>
        internal int Len { get; set; }

        /// <summary>
        /// Runway magnetic heading (add magVar for true)
        /// </summary>
        internal double Hdg { get; set; } 
        
        /// <summary>
        /// Runway surface material
        /// </summary>
        internal string Def { get; set; }

        /// <summary>
        /// Runway threshold latitude
        /// </summary>
        internal double ThresholdStartLat { get; set; }

        /// <summary>
        /// Runway threshold longitude
        /// </summary>
        internal double ThresholdStartLon { get; set; }   

        /// <summary>
        /// Index of runway in <see cref="Runway.Runways"></see>
        /// </summary>
        internal int RunwaysIndex { get; set; }

        /// <summary>
        /// Clones the airport level runway information prior to reading in each runway for the current airport
        /// </summary>
        /// <returns>Cloned version of <see cref="RunwayParams"/></returns>
        public object Clone()
        {
            var clonedRunwayParams = new RunwayParams
            {
                IcaoId = IcaoId,
                IcaoName = IcaoName,
                Country = Country,
                State = State,
                City = City,
                AirportLon = AirportLon,
                AirportLat = AirportLat,
                Altitude = Altitude,
                MagVar = MagVar,
                RunwaysIndex = RunwaysIndex
            };
            return clonedRunwayParams;
        }
    }

    /// <summary>
    /// Stores runway ids in range 37 to 52 which represent compass headings rather than usual degrees to nearest ten
    /// </summary>
    /// <param name="v1">The code - a number from 37 to 52 inclusive</param>
    /// <param name="v2">The fullname e.g. "NorthWest"</param>
    /// <param name="v3">The abbreviated name e.g. "NW"</param>
    public class RunwayCompassIds(string v1, string v2, string v3, string v4)
    {
        /// <summary>
        /// The code - a number from 37 to 52 inclusive
        /// </summary>
        internal string Code { get; set; } = v1;

        /// <summary>
        /// The fullname e.g. "NorthWest"
        /// </summary>
        internal string FullName { get; set; } = v2;

        /// <summary>
        /// The abbreviated name e.g. "NW"
        /// </summary>
        internal string AbbrName { get; set; } = v3;

        /// <summary>
        /// The number equivalent used in setting AirportLandingTrigger in ScenarioXML.cs e.g. for "NW" it would be 315
        /// </summary>
        internal string Number { get; set; } = v4;
    }

    /// <summary>
    /// Stores the Country/State/City location filter values for a location favourite
    /// </summary>
    public class LocationFavourite()
    {
        /// <summary>
        /// The name of the favourite
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The list of valid country strings for this favourite 
        /// </summary>
        public List<string> Countries { get; set; } = [];

        /// <summary>
        /// The list of valid state strings for this favourite 
        /// </summary>
        public List<string> States { get; set; } = [];

        /// <summary>
        /// The list of valid city strings for this favourite 
        /// </summary>
        public List<string> Cities { get; set; } = [];

        // Copy constructor 
        public LocationFavourite(LocationFavourite original) : this() // Calls the primary constructor for initialization
        {
            this.Name = original.Name;
            this.Countries = original.Countries?.ToList() ?? [];
            this.States = original.States?.ToList() ?? [];
            this.Cities = original.Cities?.ToList() ?? [];
        }
    }

    internal class Runway
    {
        /// <summary>
        /// The list of runways loaded from "runways.xml" file on application startup
        /// </summary>
        internal static List<RunwayParams> Runways { get; set; }

        /// <summary>
        /// The list of runways loaded from "runways.xml" file on application startup filtered by current 
        /// Country/State/Location strings on General tab
        /// </summary>
        internal static List<RunwayParams> RunwaysSubset { get; set; }

        /// <summary>
        /// Used to store runway headings that are compass direction strings rather than 01 to 36, e.g. "NorthWest"
        /// </summary>
        internal static readonly List<RunwayCompassIds> RunwayCompassIds = [];

        /// <summary>
        /// The scenario start runway
        /// </summary>
        internal static RunwayParams startRwy = new();

        /// <summary>
        /// The scenario destination runway
        /// </summary>
        internal static RunwayParams destRwy = new();

        /// <summary>
        /// User created location favourites built from combinations of Country/State/City strings in "runways.xml" file
        /// </summary>
        internal static List<LocationFavourite> LocationFavourites = [];

        /// <summary>
        /// Currently selected location favourite displayed on form
        /// </summary>
        internal static int CurrentLocationFavouriteIndex;

        #region Load runways from "runways.xml" and build list for General tab region

        /// <summary>
        /// Read in all the runways from runways.xml when the application loads
        /// </summary>
        internal static void GetRunways()
        {
            Runways = [];
            RunwayParams curAirport = new();
            using Stream stream = GetRunwayXMLstream();
            using XmlReader reader = XmlReader.Create(stream);
            SetRunwayCompassIds();
            int curIndex = 0;

            // Read "runways.xml" from start to finish
            while (reader.Read())
            {
                // Read to the start of an aiport section
                if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.Element)
                {
                    // Store airport specific information
                    curAirport.IcaoId = reader.GetAttribute("id");
                    reader.ReadToFollowing("ICAOName");
                    curAirport.IcaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    curAirport.Country = reader.ReadElementContentAsString();
                    // State not always present betweeen Country and City elements
                    reader.Read();
                    if (reader.Name == "State")
                    {
                        curAirport.State = reader.ReadElementContentAsString();
                        reader.ReadToFollowing("City");
                        curAirport.City = reader.ReadElementContentAsString();
                    }
                    else
                    {
                        curAirport.City = reader.ReadElementContentAsString();
                    }
                    reader.ReadToFollowing("Longitude");
                    curAirport.AirportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    curAirport.AirportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    curAirport.Altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    curAirport.MagVar = reader.ReadElementContentAsDouble();

                    // Read current airport runway subsections
                    while (reader.Read())
                    {
                        // Store runway subsection specific information
                        if (reader.Name == "Runway" && reader.NodeType == XmlNodeType.Element)
                        {
                            // Each runway has a copy of the airport information
                            RunwayParams newRunway = (RunwayParams)curAirport.Clone();
                            SetRunwayId(newRunway, reader.GetAttribute("id"));

                            // Read to end of current runway subsection and store a new runway record
                            if (newRunway.Id != null)
                            {
                                reader.ReadToFollowing("Len");
                                newRunway.Len = reader.ReadElementContentAsInt();
                                reader.ReadToFollowing("Hdg");
                                newRunway.Hdg = reader.ReadElementContentAsDouble();
                                reader.ReadToFollowing("Def");
                                newRunway.Def = reader.ReadElementContentAsString();
                                reader.ReadToFollowing("Lat");
                                newRunway.ThresholdStartLat = reader.ReadElementContentAsDouble();
                                reader.ReadToFollowing("Lon");
                                newRunway.ThresholdStartLon = reader.ReadElementContentAsDouble();
                                newRunway.RunwaysIndex = curIndex;
                                curIndex++;
                                Runways.Add(newRunway);
                            }
                        }

                        // We've reached end of current airport section so break out of reading runway subsections loop
                        if (reader.Name == "ICAO" && reader.NodeType == XmlNodeType.EndElement)
                        {
                            curAirport = new();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The list of runways is in "runway.xml" which is an embedded resource but the user can create a local version
        /// to reflect airports installed additional to default P3D v5
        /// </summary>
        /// <returns>
        /// A stream containing runways.xml</returns>
        static private Stream GetRunwayXMLstream()
        {
            Stream stream;
            string xmlFilename = "runways.xml";

            if (File.Exists(xmlFilename))
            {
                stream = new MemoryStream(File.ReadAllBytes(xmlFilename));
            }
            else
            {
                stream = Form.GetResourceStream($"XML.{xmlFilename}");
            }
            return stream;
        }

        /// <summary>
        /// Store runway ids in range 37 to 52 which represent compass headings rather than usual degrees to nearest ten
        /// </summary>
        static private void SetRunwayCompassIds()
        {
            RunwayCompassIds.Add(new("37", "North-South", "N-S", "36"));
            RunwayCompassIds.Add(new("38", "East-West", "E-W", "9"));
            RunwayCompassIds.Add(new("39", "Northwest-Southeast", "NW-SE", "32"));
            RunwayCompassIds.Add(new("40", "Southwest-Northeast", "SW-NE", "23"));
            RunwayCompassIds.Add(new("41", "South-North", "S-N", "18"));
            RunwayCompassIds.Add(new("42", "West-East", "W-E", "27"));
            RunwayCompassIds.Add(new("43", "Southeast-Northwest", "SE-NW", "14"));
            RunwayCompassIds.Add(new("44", "Northeast-Southwest", "NE-SW", "5"));
            RunwayCompassIds.Add(new("45", "North", "N", "36"));
            RunwayCompassIds.Add(new("46", "West", "W", "27"));
            RunwayCompassIds.Add(new("47", "Northwest", "NW", "32"));
            RunwayCompassIds.Add(new("48", "Southwest", "SW", "23"));
            RunwayCompassIds.Add(new("49", "South", "S", "18"));
            RunwayCompassIds.Add(new("50", "East", "E", "9"));
            RunwayCompassIds.Add(new("51", "Southeast", "SE", "14"));
            RunwayCompassIds.Add(new("52", "Northeast", "NE", "5"));
        }

        /// <summary>
        /// Takes a runway Id and extracts the alphabetic code letter if present and the runway number which is sometimes
        /// a string. E.g. "23L" is "Left" plus "23", "45" is "North", "32W" is "Water" plus "32"
        /// </summary>
        /// <param name="rwyParams">Where the runway Id, designator and number are stored</param>
        /// <param name="runwayId">The runway Id string to be processed</param>
        static private void SetRunwayId(RunwayParams rwyParams, string runwayId)
        {
            rwyParams.Id = runwayId;

            rwyParams.Designator = "None";
            if (runwayId.EndsWith('L'))
            {
                rwyParams.Designator = "Left";
                runwayId = runwayId.TrimEnd('L');
            }
            else if (runwayId.EndsWith('R'))
            {
                rwyParams.Designator = "Right";
                runwayId = runwayId.TrimEnd('R');
            }
            else if (runwayId.EndsWith('C'))
            {
                rwyParams.Designator = "Center";
                runwayId = runwayId.TrimEnd('C');
            }
            else if (runwayId.EndsWith('W'))
            {
                rwyParams.Designator = "Water";
                runwayId = runwayId.TrimEnd('W');
            }
            if (int.TryParse(runwayId, out int runwayNumber))
                if (runwayNumber <= 36)
                {
                    rwyParams.Number = runwayId.TrimStart('0');
                }
                else if (runwayNumber <= 52)
                {
                    RunwayCompassIds runwayCompassId = RunwayCompassIds.Find(rcID => rcID.Code == runwayId);
                    rwyParams.Number = runwayCompassId.AbbrName;
                }
        }

        /// <summary>
        /// Builds list of strings in format "Airport ICAO (Runway Id)" e.g. "LFGO (14L)" used to populate
        /// available runway list in General tab of application form.
        /// </summary>
        /// <returns>The list of runway strings</returns>
        static internal List<string> GetICAOids()
        {
            List<string> icaoIDs = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (int.TryParse(Runways[i].Number, out int runwayNumber) && runwayNumber <= 36)
                {
                    icaoIDs.Add($"{Runways[i].IcaoId} ({Runways[i].Id})");
                }
                else
                {
                    string runwayId = $"{Runways[i].IcaoId} ({Runways[i].Number})";
                    if (Runways[i].Designator != null && Runways[i].Designator != "None")
                    {
                        // Append designator if not "None"
                        runwayId = $"{runwayId}[{Runways[i].Designator[0..1]}]";
                    }
                    icaoIDs.Add(runwayId);
                }
            }
            return icaoIDs;
        }

        /// <summary>
        /// Get a sorted list of the country strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the country strings in "runways.xml"</returns>
        static internal List<string> GetRunwayCountries()
        {
            List<string> countries = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].Country != "" && countries.IndexOf(Runways[i].Country) == -1)
                {
                    countries.Add(Runways[i].Country);
                }
            }
            countries.Sort();
            countries.Insert(0, "None");
            return countries;
        }

        /// <summary>
        /// Get a sorted list of the state strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the state strings in "runways.xml"</returns>
        static internal List<string> GetRunwayStates()
        {
            List<string> states = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].State != null && Runways[i].State != "" && states.IndexOf(Runways[i].State) == -1)
                {
                    states.Add(Runways[i].State);
                }
            }
            states.Sort();
            states.Insert(0, "None");
            return states;
        }

        /// <summary>
        /// Get a sorted list of the city strings in "runways.xml"
        /// </summary>
        /// <returns>Sorted list of the city strings in "runways.xml"</returns>
        static internal List<string> GetRunwayCities()
        {
            List<string> cities = [];

            for (int i = 0; i < Runways.Count; i++)
            {
                if (Runways[i].City != "" && cities.IndexOf(Runways[i].City) == -1)
                {
                    cities.Add(Runways[i].City);
                }
            }
            cities.Sort();
            cities.Insert(0, "None");
            return cities;
        }

        #endregion

        #region Manage Location favourites region

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddFilterValueToLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                // Clear existing filter strings and add "None"
                ClearLocationFavouriteList(locationType);
                AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                // Add filter string
                AddToLocationFavouriteList(locationType, locationValue);
                // Handle case where filter list was "None"
                DeleteFromLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        /// Adds filter string to one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to add to</param>
        /// <param name="locationValue">The filter string to be added</param>
        static internal void AddToLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Countries = LocationFavourites[CurrentLocationFavouriteIndex].Countries.Distinct().ToList();
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Sort();
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].States = LocationFavourites[CurrentLocationFavouriteIndex].States.Distinct().ToList();
                LocationFavourites[CurrentLocationFavouriteIndex].States.Sort();
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Add(locationValue);
                LocationFavourites[CurrentLocationFavouriteIndex].Cities = LocationFavourites[CurrentLocationFavouriteIndex].Cities.Distinct().ToList();
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Sort();
            }
        }

        /// <summary>
        /// Clears filter string list for one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which Country/State/City filter string list to clear </param>
        static internal void ClearLocationFavouriteList(string locationType)
        {
            if (locationType == "Country")
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Clear();
            else if (locationType == "State")
                LocationFavourites[CurrentLocationFavouriteIndex].States.Clear();
            else
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Clear();
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFromLocationFavouriteList(string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Countries.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Countries.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else if (locationType == "State")
            {
                LocationFavourites[CurrentLocationFavouriteIndex].States.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].States.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
            else
            {
                LocationFavourites[CurrentLocationFavouriteIndex].Cities.Remove(locationValue);
                if (LocationFavourites[CurrentLocationFavouriteIndex].Cities.Count == 0)
                    AddToLocationFavouriteList(locationType, "None");
            }
        }

        /// <summary>
        ///  Gets a filter string to display in the Country/State/City fields on the General tab of form.
        ///  The current favourite may include more than one filter value for one or more of these fields.
        ///  Displays the first of the filter values as the list is maintained sorted.
        /// </summary>
        /// <param name="locationType">Which of Country/State/City fields the display filter value is for</param>
        /// <returns>The Country/State/City field display filter value</returns>
        static internal string GetLocationFavouriteDisplayFilterValue(string locationType)
        {
            if (locationType == "Country")
                return LocationFavourites[CurrentLocationFavouriteIndex].Countries[0];
            else if (locationType == "State")
                return LocationFavourites[CurrentLocationFavouriteIndex].States[0];
            else
                return LocationFavourites[CurrentLocationFavouriteIndex].Cities[0];
        }

        /// <summary>
        /// Get a sorted list of the location favourite names
        /// </summary>
        /// <returns>Sorted list of the location favourite names</returns>
        static internal List<string> GetLocationFavouriteNames()
        {
            List<string> locationFavouriteNames = [];

            for (int i = 0; i < LocationFavourites.Count; i++)
            {
                locationFavouriteNames.Add(LocationFavourites[i].Name);
            }
            locationFavouriteNames.Sort();
            return locationFavouriteNames;
        }

        /// <summary>
        /// Deletes filter string from one of Country/State/City for <see cref="CurrentLocationFavouriteIndex"/> in 
        /// <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationType">Which of Country/State/City to delete from</param>
        /// <param name="locationValue">The filter string to be deleted</param>
        static internal void DeleteFilterValueFromLocationFavourite(string locationType, string locationValue)
        {
            if (locationValue == "None")
            {
                return;
            }
            else
            {
                DeleteFromLocationFavouriteList(locationType, locationValue);
            }
        }

        /// <summary>
        /// Combines the Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <returns>Country/State/City location filters combined into a single string</returns>
        static internal string SetTextBoxGeneralLocationFilters()
        {
            string filters;

            filters = "Countries = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Countries);
            filters += "\" \nStates = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].States);
            filters += "\" \nCities = \"";
            filters += SetTextBoxGeneralLocationFilter(LocationFavourites[CurrentLocationFavouriteIndex].Cities);
            filters += "\"";
            return filters;
        }

        /// <summary>
        /// Updates current location favourite with new name if that name has not already been used
        /// </summary>
        /// <param name="newLocationFavouriteName">The new location favourite name</param>
        /// <returns>The replaced current location favourite name</returns>
        static internal string UpdateLocationFavouriteName(string newLocationFavouriteName)
        {
            // Save name of current location favourite before changing
            string oldLocationFavouriteName = LocationFavourites[CurrentLocationFavouriteIndex].Name;

            // Make sure new name is not already in use and then change in current location favourite
            if (LocationFavourites.FindAll(favourite => favourite.Name ==  newLocationFavouriteName).Count == 0)
                LocationFavourites[CurrentLocationFavouriteIndex].Name = newLocationFavouriteName;

            // Return old favourite name so a new location favourite of that name can be created
            return oldLocationFavouriteName;
        }

        /// <summary>
        /// Add a location favourite to end of <see cref="LocationFavourites"/>, no need to
        /// adjust <see cref="CurrentLocationFavouriteIndex"/> as it is unaffected by adding to
        /// end of the list unless it's the first favourite to be added in which case set
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero.
        /// </summary>
        /// <param name="name">The name for the new <see cref="LocationFavourite"/> to be added</param>
        static internal void AddLocationFavourite(string name)
        {
            // Create a deep copy using the extension method
            LocationFavourite deepCopyFav = new(LocationFavourites[CurrentLocationFavouriteIndex])
            {
                Name = name
            };
            LocationFavourites.Add(deepCopyFav);
            if (LocationFavourites.Count == 1)
                CurrentLocationFavouriteIndex = 0;
        }

        /// <summary>
        /// Delete a location favourite from <see cref="LocationFavourites"/> and adjust 
        /// <see cref="CurrentLocationFavouriteIndex"/> to zero
        /// </summary>
        /// <param name="deleteLocationFavouriteName">The name of the  <see cref="LocationFavourite"/> to be deleted</param>
        /// <returns>The name of the zero index <see cref="LocationFavourites"/> location favourite</returns>
        static internal string DeleteLocationFavourite(string deleteLocationFavouriteName)
        {
            if (LocationFavourites.Count > 1)
            {
                LocationFavourite deleteLocationFavourite = LocationFavourites.Find(favourite => favourite.Name == deleteLocationFavouriteName);
                if (deleteLocationFavourite != null)
                {
                    LocationFavourites.Remove(deleteLocationFavourite);
                    CurrentLocationFavouriteIndex = 0;
                }
            }
            return LocationFavourites[CurrentLocationFavouriteIndex].Name;
        }

        /// <summary>
        /// Combines one of Country/State/City location filters into a single string for display using
        /// a tooltip with MouseHover event over TextBoxGeneralLocationFilters
        /// </summary>
        /// <param name="locationFilterStrings">The list of location filter strings to be combined</param>
        /// <returns>One of Country/State/City location filters combined into a single string</returns>
        static private string SetTextBoxGeneralLocationFilter(List<string> locationFilterStrings)
        {
            string filters = "";

            foreach (string filterString in locationFilterStrings)
                filters += $"{filterString}, ";
            filters = filters.Trim();
            filters = filters.Trim(',');

            return filters;
        }

        /// <summary>
        /// Get the <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/>
        /// </summary>
        /// <returns>The <see cref="CurrentLocationFavouriteIndex"/> instance in <see cref="LocationFavourites"/></returns>
        static internal LocationFavourite GetCurrentLocationFavourite()
        {
            return LocationFavourites[CurrentLocationFavouriteIndex];
        }

        /// <summary>
        /// Reset <see cref="CurrentLocationFavouriteIndex"/> to the instance of <see cref="LocationFavourite"/>
        /// with newFavouriteName
        /// </summary>
        /// <param name="newFavouriteName">The name of the new instance to be set as <see cref="CurrentLocationFavouriteIndex"/></param>
        static internal void ChangeCurrentLocationFavouriteIndex(string newFavouriteName)
        {
            LocationFavourite locationFavourite = LocationFavourites.Find(favourite => favourite.Name == newFavouriteName);
            CurrentLocationFavouriteIndex = LocationFavourites.IndexOf(locationFavourite);
        }

        /// <summary>
        /// Save <see cref="LocationFavourites"/> to file in JSON format
        /// </summary>
        internal static void SaveLocationFavourites()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "LocationFavouritesJSON.txt");

            JsonSerializer serializer = new()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            using StreamWriter sw = new(directory);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, LocationFavourites);
        }

        /// <summary>
        /// Load <see cref="LocationFavourites"/> from file stored in JSON format
        /// </summary>
        internal static void LoadLocationFavourites()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directory = Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory = Path.Combine(directory, "LocationFavouritesJSON.txt");

            string input;
            if (!File.Exists(directory))
            {
                Stream stream = Form.GetResourceStream("Text.LocationFavouritesJSON.txt");
                StreamReader reader = new(stream);
                input = reader.ReadToEnd();
            }
            else
            {
                input = File.ReadAllText(directory);
            }

            LocationFavourites = JsonConvert.DeserializeObject<List<LocationFavourite>>(input);
        }

        /// <summary>
        /// Checks a location string (Country/State/City) against the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="location">The location string to be checked</param>
        /// <returns>True if the location string to be checked matches one of the Country/State/City filter values</returns>
        internal static bool CheckLocationFilters(string location)
        {
            // If Country/State/City filters all set to "None" return true as no filtering on location is required
            if (LocationFavourites[CurrentLocationFavouriteIndex].Countries[0] == "None" && 
                LocationFavourites[CurrentLocationFavouriteIndex].States[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].Cities[0] == "None")
            {
                return true;
            }

            // Check each of Country/State/City filter strings against location
            bool checkCountry = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].Countries);
            bool checkState = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].States);
            bool checkCity = CheckLocationFilter(location, LocationFavourites[CurrentLocationFavouriteIndex].Cities);
            if (checkCountry || checkState || checkCity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks a location string against one of Country/State/City in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="location">The location string to be checked</param>
        /// <param name="locationStrings">The strings to be checked against in <see cref="LocationFavourites"/></param>
        /// <returns>True if the location string to be checked matches one of the filter values in locationStrings</returns>
        internal static bool CheckLocationFilter(string location, List<string> locationStrings)
        {
            if (locationStrings[0] != "None")
            {
                return locationStrings.Contains(location);
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Search for a runway(s) that meets constraints region

        /// <summary>
        /// Selects and returns a random RunwayParams object from the predefined subset of runways.
        /// </summary>
        /// <returns>A randomly selected RunwayParams object.</returns>
        static internal RunwayParams GetRandomRunway()
        {
            Random random = new();
            int randomSubsetIndex = random.Next(0, Runway.RunwaysSubset.Count);
            return RunwaysSubset[randomSubsetIndex];
        }

        /// <summary>
        /// Search through the list of runways, starting from a random runway in list. Return the first runway
        /// located that is between a minimum and maximum distance from the provided reference coordinate
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <param name="minDist">The minimum distance runway can be from reference coordinate</param>
        /// <param name="maxDist">The maximum distance runway can be from reference coordinate</param>
        /// <returns>The runway that meets the minimum and maximum distance requirements</returns>
        static internal RunwayParams GetNearbyRunway(double queryLat, double queryLon, double minDist, double maxDist)
        {
            double curDistance;
            Random r = new();
            int curIndex = r.Next(0, RunwaysSubset.Count);
            for (int i = 0; i < Runways.Count; i++)
            {
                curDistance = MathRoutines.CalcDistance(RunwaysSubset[curIndex].AirportLat, RunwaysSubset[curIndex].AirportLon, queryLat, queryLon);
                if ((curDistance < maxDist) && (curDistance > minDist))
                {
                    return RunwaysSubset[curIndex];
                }
                curIndex++;
                if (curIndex == RunwaysSubset.Count)
                {
                    curIndex = 0;
                }
            }
            return null;
        }

        /// <summary>
        /// Search through the list of runways. Returns the nearest runway to the provided reference coordinate.
        /// </summary>
        /// <param name="queryLat">Latitude of query coordinate</param>
        /// <param name="queryLon">Longitute of query coordinate</param>
        /// <returns>The nearest runway to the provided reference cooordinate</returns>
        static internal RunwayParams GetNearestRunway(double queryLat, double queryLon)
        {
            RunwayParams minAirport = new();
            double minDistance = 9999;
            double curDistance;
            for (int curIndex = 0; curIndex < RunwaysSubset.Count; curIndex++)
            {
                curDistance = MathRoutines.CalcDistance(RunwaysSubset[curIndex].AirportLat, RunwaysSubset[curIndex].AirportLon, queryLat, queryLon);
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    minAirport = RunwaysSubset[curIndex];
                }
            }
            return minAirport;
        }

        /// <summary>
        /// Creates a subset of the <see cref="Runways"/> list of runways filtering on the Country/State/City
        /// location strings in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        static internal void SetRunwaysSubset()
        {
            // If Country/State/City filters all set to "None" just return full list of runways
            if (LocationFavourites[CurrentLocationFavouriteIndex].Countries[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].States[0] == "None" &&
                LocationFavourites[CurrentLocationFavouriteIndex].Cities[0] == "None")
            {
                RunwaysSubset = CloneRunwayParamsList(Runways);
                return;
            }

            // Get the subset of runways that match each of the Country/State/City filters and take the union of them - removes duplicates
            List<RunwayParams> runwaysCountrySubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].Countries, "Country");
            List<RunwayParams> runwaysStateSubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].States, "State");
            List<RunwayParams> runwaysCitySubset = GetRunwaysLocationSubset(LocationFavourites[CurrentLocationFavouriteIndex].Cities, "City");
            List<RunwayParams> runwaysSubset = [.. runwaysCountrySubset.Union(runwaysStateSubset)];
            runwaysSubset = [.. runwaysSubset.Union(runwaysCitySubset)];

            if (runwaysSubset.Count > 0)
            {
                RunwaysSubset = runwaysSubset;
            }
            else 
            {
                MessageBox.Show("No runways match the location filters combination, using all runways instead", "Random Runway: Location filters", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                RunwaysSubset = CloneRunwayParamsList(Runways);
            }
        }

        /// <summary>
        /// Clones a list of <see cref="RunwayParams"/>
        /// </summary>
        /// <param name="sourceList">The list of <see cref="RunwayParams"/> to be cloned</param>
        /// <returns>The cloned list of <see cref="RunwayParams"/></returns>
        static internal List<RunwayParams> CloneRunwayParamsList(List<RunwayParams> sourceList)
        {
            List<RunwayParams> runwaysSubset = [];
            sourceList.ForEach((item) =>
            {
                runwaysSubset.Add((RunwayParams)item.Clone());
            });
            return runwaysSubset;
        }

        /// <summary>
        /// Gets the subset of runways from <see cref="Runways"/> filtered on one location string type
        /// in the current location favourite in <see cref="LocationFavourites"/>
        /// </summary>
        /// <param name="locationStrings">The collection of location strings</param>
        /// <param name="locationType">Which of Country/State/City filtering on</param>
        /// <returns>The subset of runways filtered from <see cref="Runways"/></returns>
        static internal List<RunwayParams> GetRunwaysLocationSubset(List<string> locationStrings, string locationType)
        {
            List<RunwayParams> runwaysSubset = [];
            if (locationStrings[0] != "None")
            {
                if (locationType == "Country")
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.Country));
                }
                else if (locationType == "State")
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.State));
                }
                else
                {
                    runwaysSubset = Runways.FindAll(runway => locationStrings.Contains(runway.City));
                }
            }
            return runwaysSubset;
        }

        #endregion region
    }
}
