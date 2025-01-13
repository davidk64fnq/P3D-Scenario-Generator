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
        /// The ltter is extracted and stored as "Designator" field.
        /// </summary>
        internal string Id { get; set; }

        /// <summary>
        /// The compass bearing of an airport runway with no leading zeros e.g. "5" is 5 degrees. Or a compass
        /// direction string e.g. "Northwest"
        /// </summary>
        internal string Number { get; set; }

        /// <summary>
        /// One of "None", "Left", "Right", "Center", or "Water". Used in setting the airport landing trigger for a scenario
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
                MagVar = MagVar
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
    public class RunwayCompassIds(string v1, string v2, string v3)
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
    }

    /// <summary>
    /// Stores the Country/State/City location filter values for a location favourite
    /// </summary>
    public class LocationFavourite(string v1, List<string> v2, List<string> v3, List<string> v4)
    {
        /// <summary>
        /// The name of the favourite
        /// </summary>
        internal string Name { get; set; } = v1;

        /// <summary>
        /// The list of valid country strings for this favourite 
        /// </summary>
        internal List<string> Countries { get; set; } = v2;

        /// <summary>
        /// The list of valid state strings for this favourite 
        /// </summary>
        internal List<string> States { get; set; } = v3;

        /// <summary>
        /// The list of valid city strings for this favourite 
        /// </summary>
        internal List<string> Cities { get; set; } = v4;
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
        private static readonly List<RunwayCompassIds> runwayCompassIds = [];

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
            runwayCompassIds.Add(new("37", "North-South", "N-S"));
            runwayCompassIds.Add(new("38", "East-West", "E-W"));
            runwayCompassIds.Add(new("39", "Northwest-Southeast", "NW-SE"));
            runwayCompassIds.Add(new("40", "Southwest-Northeast", "SW-NE"));
            runwayCompassIds.Add(new("41", "South-North", "S-N"));
            runwayCompassIds.Add(new("42", "West-East", "W-E"));
            runwayCompassIds.Add(new("43", "Southeast-Northwest", "SE-NW"));
            runwayCompassIds.Add(new("44", "Northeast-Southwest", "NE-SW"));
            runwayCompassIds.Add(new("45", "North", "N"));
            runwayCompassIds.Add(new("46", "West", "W"));
            runwayCompassIds.Add(new("47", "Northwest", "NW"));
            runwayCompassIds.Add(new("48", "Southwest", "SW"));
            runwayCompassIds.Add(new("49", "South", "S"));
            runwayCompassIds.Add(new("50", "East", "E"));
            runwayCompassIds.Add(new("51", "Southeast", "SE"));
            runwayCompassIds.Add(new("52", "Northeast", "NE"));
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
                    RunwayCompassIds runwayCompassId = runwayCompassIds.Find(rcID => rcID.Code == runwayId);
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
            countries.Insert(0, "All");
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
            states.Insert(0, "All");
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
            cities.Insert(0, "All");
            cities.Insert(0, "None");
            return cities;
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

        static internal void AddLocationToLocationFavourite(string locationType, string locationValue)
        {
            // Get index of locationFavourite to be added to
            string selectedFavouriteName = Form.form.ComboBoxGeneralLocationFavourites.SelectedItem.ToString();
            int locationFavouriteIndex = LocationFavourites.FindIndex(favourite => favourite.Name == selectedFavouriteName);

            if (locationValue == "None")
            {
                ClearLocationFavouriteList(locationFavouriteIndex, locationType);
            }
            else if (locationValue == "All")
            {
                AddToLocationFavouriteList(locationFavouriteIndex, locationType, "All");
            }
            else
            {
                AddToLocationFavouriteList(locationFavouriteIndex, locationType, locationValue);
            }
            SetTextBoxGeneralLocationFilters(locationFavouriteIndex);
        }

        static internal void ClearLocationFavouriteList(int locationFavouriteIndex, string locationType)
        {
            if (locationType == "Country")
                LocationFavourites[locationFavouriteIndex].Countries.Clear();
            else if (locationType == "State")
                LocationFavourites[locationFavouriteIndex].States.Clear();
            else
                LocationFavourites[locationFavouriteIndex].Cities.Clear();
        }

        static internal void AddToLocationFavouriteList(int locationFavouriteIndex, string locationType, string locationValue)
        {
            if (locationType == "Country")
            {
                LocationFavourites[locationFavouriteIndex].Countries.Add(locationValue);
                LocationFavourites[locationFavouriteIndex].Countries = LocationFavourites[locationFavouriteIndex].Countries.Distinct().ToList();
                LocationFavourites[locationFavouriteIndex].Countries.Sort();
            }
            else if (locationType == "State")
            {
                LocationFavourites[locationFavouriteIndex].States.Add(locationValue);
                LocationFavourites[locationFavouriteIndex].States = LocationFavourites[locationFavouriteIndex].States.Distinct().ToList();
                LocationFavourites[locationFavouriteIndex].States.Sort();
            }
            else
            {
                LocationFavourites[locationFavouriteIndex].Cities.Add(locationValue);
                LocationFavourites[locationFavouriteIndex].Cities = LocationFavourites[locationFavouriteIndex].Cities.Distinct().ToList();
                LocationFavourites[locationFavouriteIndex].Cities.Sort();
            }
        }

        static internal void DeleteLocationFromLocationFavourite(string locationType, string locationValue)
        {
            // Get index of locationFavourite to be removed from
            string selectedFavouriteName = Form.form.ComboBoxGeneralLocationFavourites.SelectedItem.ToString();
            int locationFavouriteIndex = LocationFavourites.FindIndex(favourite => favourite.Name == selectedFavouriteName);

            if (locationValue == "None")
            {
                return;
            }
            else if (locationValue == "All")
            {
                ClearLocationFavouriteList(locationFavouriteIndex, locationType);
            }
            else
            {
                DeleteFromLocationFavouriteList(locationFavouriteIndex, locationType, locationValue);
            }
            SetTextBoxGeneralLocationFilters(locationFavouriteIndex);
        }

        static internal void DeleteFromLocationFavouriteList(int locationFavouriteIndex, string locationType, string locationValue)
        {
            if (locationType == "Country")
                LocationFavourites[locationFavouriteIndex].Countries.Remove(locationValue);
            else if (locationType == "State")
                LocationFavourites[locationFavouriteIndex].States.Remove(locationValue);
            else
                LocationFavourites[locationFavouriteIndex].Cities.Remove(locationValue);
        }

        static internal void SetTextBoxGeneralLocationFilters(int locationFavouriteIndex)
        {
            string filters;
        }

        #endregion

        #region Search for a runway(s) that meets constraints region

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
            SetRunwaysSubset();
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
            SetRunwaysSubset();
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
        /// location strings in the General tab
        /// </summary>
        static internal void SetRunwaysSubset()
        {
            // If Country/State/City filters all set to "None" just return full list of runways
            if (Form.form.ComboBoxGeneralLocationCountry.Text == "None" && Form.form.ComboBoxGeneralLocationState.Text == "None" &&
                Form.form.ComboBoxGeneralLocationCity.Text == "None")
            {
                RunwaysSubset = CloneRunwayParamsList(Runways);
            }

            // Get the subset of runways that match each of the Country/State/City filters and take the union of them - removes duplicates
            List<RunwayParams> runwaysCountrySubset = GetRunwaysLocationSubset(Form.form.ComboBoxGeneralLocationCountry.Items, 
            Form.form.ComboBoxGeneralLocationCountry.Text, "Country");
            List<RunwayParams> runwaysStateSubset = GetRunwaysLocationSubset(Form.form.ComboBoxGeneralLocationState.Items,
                Form.form.ComboBoxGeneralLocationState.Text, "State");
            List<RunwayParams> runwaysCitySubset = GetRunwaysLocationSubset(Form.form.ComboBoxGeneralLocationCity.Items,
                Form.form.ComboBoxGeneralLocationCity.Text, "City");
            List<RunwayParams> runwaysSubset = runwaysCountrySubset.Union(runwaysStateSubset).ToList();
            runwaysSubset = runwaysSubset.Union(runwaysCitySubset).ToList();

            if (runwaysCountrySubset.Count > 0)
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
        /// </summary>
        /// <param name="locationStrings">The collection of location strings</param>
        /// <param name="selectedLocation">The selected item in the collection of location strings</param>
        /// <param name="locationType">Which of Country/State/City filtering on</param>
        /// <returns>The subset of runways filtered from <see cref="Runways"/></returns>
        static internal List<RunwayParams> GetRunwaysLocationSubset(ComboBox.ObjectCollection locationStrings, string selectedLocation, string locationType)
        {
            List<RunwayParams> runwaysSubset = [];
            if (selectedLocation == "All")
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
            else if (selectedLocation != "None")
            {
                if (locationType == "Country")
                {
                    runwaysSubset = Runways.FindAll(runway => selectedLocation.Contains(runway.Country));
                }
                else if (locationType == "State")
                {
                    runwaysSubset = Runways.FindAll(runway => selectedLocation.Contains(runway.State));
                }
                else
                {
                    runwaysSubset = Runways.FindAll(runway => selectedLocation.Contains(runway.City));
                }
            }
            return runwaysSubset;
        }

        #endregion region

        // Delete once celestial reworked
        static internal string[] SetICAOwords(string rwyType)
        {
            string[] words = ["", ""];
            RunwayParams airport;
            switch (Parameters.SelectedScenario)
            {
                case nameof(ScenarioTypes.Circuit):
                case nameof(ScenarioTypes.SignWriting):
            //        words = Parameters.SelectedRunway.Split("\t");
                    break;
                case nameof(ScenarioTypes.PhotoTour):
            //        if (rwyType == "start")
            //            words = Parameters.SelectedRunway.Split("\t");
            //        else
            //            words = Parameters.PhotoDestRunway.Split("\t");
                    break;
                case nameof(ScenarioTypes.Celestial):
                    if (rwyType == "destination")
                    {
                        Random random = new();
                        airport = GetNearestRunway(-60 + random.Next(0, 120), -180 + random.Next(0, 360));
                        Parameters.CelestialDestRunway = $"{airport.IcaoId}\t({airport.Id})";
                        words = Parameters.CelestialDestRunway.Split("\t");
                    }
                    break;
                case nameof(ScenarioTypes.WikiList):
                    if (rwyType == "start")
                    {
                        words[0] = Wikipedia.WikiTour[0].airportICAO;
                        words[1] = Wikipedia.WikiTour[0].airportID;
                    }
                    else
                    {
                        words[0] = Wikipedia.WikiTour[^1].airportICAO;
                        words[1] = Wikipedia.WikiTour[^1].airportID;
                    }
                    break;
                default:
                    break;
            }
            words[1] = words[1].Trim('(');
            words[1] = words[1].Trim(')');
            return words;
        }

        // Delete once celestial reworked
        static internal void SetRunway(RunwayParams rwyParams, string airportICAO, string airportID)
        {
            Stream stream = GetRunwayXMLstream();
            XmlReader reader = XmlReader.Create(stream);
            SetRunwayCompassIds();

            bool runwaySet = false;
            while (reader.ReadToFollowing("ICAO") && runwaySet == false)
            {
                // Check we have located selected airport
                if (reader.MoveToAttribute("id") && reader.Value == airportICAO)
                {
                    rwyParams.IcaoId = reader.Value;
                    reader.ReadToFollowing("ICAOName");
                    rwyParams.IcaoName = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Country");
                    rwyParams.Country = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("City");
                    rwyParams.City = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Longitude");
                    rwyParams.AirportLon = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Latitude");
                    rwyParams.AirportLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Altitude");
                    rwyParams.Altitude = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("MagVar");
                    rwyParams.MagVar = reader.ReadElementContentAsDouble();

                    // Check we have located selected runway
                    do
                    {
                        reader.Read();
                    }
                    while (!(reader.Name == "Runway" && reader.MoveToAttribute("id") && reader.Value == airportID));
                    SetRunwayId(rwyParams, reader.Value);
                    reader.ReadToFollowing("Len");
                    rwyParams.Len = reader.ReadElementContentAsInt();
                    reader.ReadToFollowing("Hdg");
                    rwyParams.Hdg = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Def");
                    rwyParams.Def = reader.ReadElementContentAsString();
                    reader.ReadToFollowing("Lat");
                    rwyParams.ThresholdStartLat = reader.ReadElementContentAsDouble();
                    reader.ReadToFollowing("Lon");
                    rwyParams.ThresholdStartLon = reader.ReadElementContentAsDouble();

                    if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
                    {
                        CelestialNav.destinationLat = rwyParams.AirportLat;
                        CelestialNav.destinationLon = rwyParams.AirportLon;
                    }

                    runwaySet = true;
                }
            }
            stream.Dispose();


            if (Parameters.SelectedScenario == nameof(ScenarioTypes.Celestial))
            {
                CelestialNav.SetCelestialStartLocation();
                BingImages.GetCelestialOverviewImage();
            }
        }
    }
}
