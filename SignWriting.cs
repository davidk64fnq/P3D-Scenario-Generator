namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Methods for creating a signwriting message scenario. Creation of gates done in gates.cs, also some methods
    /// relating to creation of xml file specific to sign writing in ScenarioXML.cs
    /// </summary>
    internal class SignWriting
    {
        /// <summary>
        /// Used to store decimal equivalent of the 22 digit binary representation of a character. Each letter
        /// is made up of a subset of the 22 possible segments used for displaying the letter. The 22 digit binary
        /// string shows a "1" for segments turned on and "0" for segments turned off. The decimal equivalent
        /// of the 22 digit binary string for each uppercase and lowercase character is stored in this array.
        /// The letter lowercase "z" has ascii code of 122 so the binary segment string for "z" is stored as
        /// its decimal equivalent in letterPath[122]
        /// </summary>
        private static readonly int[] letterPath = new int[123];

        /// <summary>
        /// The gates comprising the message for the signwriting scenario. Methods for setting gates are in gates.cs
        /// </summary>
        static internal List<Gate> gates = [];

        /// <summary>
        /// Called from Form1.cs to do the scenario specific work in creating a signwriting scenario
        /// </summary>
        static internal void SetSignWriting()
        {
            Runway.startRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            Runway.destRwy = Runway.Runways[Parameters.SelectedAirportIndex];
            InitLetterPaths();
            gates = Gates.SetSignGatesMessage();
            SetSignWritingOverviewImage(gates);
            SetSignWritingLocationImage(gates);
        }

        /// <summary>
        /// To code a character in letterPath, work out the 22 digit binary number which shows which segments
        /// need to be displayed. The 22 segments are always considered for inclusion in the same order. The first
        /// segment of the 22 segment path is coded at the right hand end of the binary number i.e. the bit representing
        /// 0 or 1. Then convert the binary number to decimal.
        /// </summary>
        static internal void InitLetterPaths()
        {
            letterPath['@'] = 4194303;  // Test character all segments turned on
            letterPath['A'] = 3948336;
            letterPath['B'] = 4178723;
            letterPath['C'] = 16131;
            letterPath['D'] = 4178691;
            letterPath['E'] = 16179;
            letterPath['F'] = 16176;
            letterPath['G'] = 3161891;
            letterPath['H'] = 3947568;
            letterPath['I'] = 246531;
            letterPath['J'] = 3944451;
            letterPath['K'] = 2473044;
            letterPath['L'] = 15363;
            letterPath['M'] = 4144896;
            letterPath['N'] = 4045956;
            letterPath['O'] = 3948291;
            letterPath['P'] = 802608;
            letterPath['Q'] = 3981099;
            letterPath['R'] = 2932532;
            letterPath['S'] = 2197383;
            letterPath['T'] = 246528;
            letterPath['U'] = 3947523;
            letterPath['V'] = 3968020;
            letterPath['W'] = 3996675;
            letterPath['X'] = 2467020;
            letterPath['Y'] = 838704;
            letterPath['Z'] = 369483;
            letterPath['a'] = 3684592;
            letterPath['b'] = 3784931;
            letterPath['c'] = 14531;
            letterPath['d'] = 3784899;
            letterPath['e'] = 14579;
            letterPath['f'] = 14576;
            letterPath['g'] = 2111687;
            letterPath['h'] = 3684400;
            letterPath['i'] = 114883;
            letterPath['j'] = 3678211;
            letterPath['k'] = 2668588;
            letterPath['l'] = 14339;
            letterPath['m'] = 3782848;
            letterPath['n'] = 3717140;
            letterPath['o'] = 3684547;
            letterPath['p'] = 538864;
            letterPath['q'] = 3717355;
            letterPath['r'] = 2668788;
            letterPath['s'] = 2132183;
            letterPath['t'] = 114880;
            letterPath['u'] = 3684355;
            letterPath['v'] = 3704852;
            letterPath['w'] = 3733507;
            letterPath['x'] = 2664508;
            letterPath['y'] = 575536;
            letterPath['z'] = 565483;
        }

        /// <summary>
        /// Creates "Charts_01.jpg" using a montage of OSM tiles that covers airport and sign writing gates/>
        /// </summary>
        static internal void SetSignWritingOverviewImage(List<Gate> gates)
        {
            int zoom = GetBoundingBoxZoom(gates, 0, gates.Count - 1);
            List<Tile> tiles = SetSignWritingOSMtiles(gates, zoom, 0, gates.Count - 1);
            BoundingBox boundingBox = MapTileBoundingBoxCalculator.GetBoundingBox(tiles, zoom);
            MapTileMontager.MontageTiles(boundingBox, zoom, "Charts_01");
            ImageUtils.DrawRoute(tiles, boundingBox, "Charts_01");
            ImageUtils.MakeSquare(boundingBox, "Charts_01", zoom, Constants.tileFactor);
        }

        /// <summary>
        /// Creates "chart_thumb.jpg" using an OSM tile that covers the starting airport
        /// </summary>
        static internal void SetSignWritingLocationImage(List<Gate> gates)
        {
            int zoom = 15;
            List<Tile> tiles = SetSignWritingOSMtiles(gates, zoom, 0, 0);
            BoundingBox boundingBox = MapTileBoundingBoxCalculator.GetBoundingBox(tiles, zoom);
            MapTileMontager.MontageTiles(boundingBox, zoom, "chart_thumb");
            if (boundingBox.XAxis.Count != boundingBox.YAxis.Count)
            {
                ImageUtils.MakeSquare(boundingBox, "chart_thumb", zoom, Constants.locationImageTileFactor);
            }
            if (boundingBox.XAxis.Count == Constants.tileFactor)
            {
                ImageUtils.Resize("chart_thumb.png", Constants.tileSize, 0);
            }
        }

        /// <summary>
        /// Works out most zoomed in level that includes all gates specified by startGateIndex and finishGateIndex, 
        /// plus airport where the montage of OSM tiles doesn't exceed <see cref="Constants.tileFactor"/> in size
        /// </summary>
        /// <param name="startGateIndex">Index of first gate in sign writing message</param>
        /// <param name="finishGateIndex">Index of last gate in sign writing message</param>
        /// <returns>The maximum zoom level that meets constraints</returns>
        static internal int GetBoundingBoxZoom(List<Gate> gates, int startGateIndex, int finishGateIndex)
        {
            List<Tile> tiles;
            BoundingBox boundingBox;
            for (int zoom = 2; zoom <= Constants.maxZoomLevel; zoom++) // zoom of 1 is map of the world!
            {
                tiles = SetSignWritingOSMtiles(gates, zoom, startGateIndex, finishGateIndex);
                boundingBox = MapTileBoundingBoxCalculator.GetBoundingBox(tiles, zoom);
                if ((boundingBox.XAxis.Count > Constants.tileFactor) || (boundingBox.YAxis.Count > Constants.tileFactor))
                {
                    return zoom - 1;
                }
            }
            return Constants.maxZoomLevel;
        }

        /// <summary>
        /// Finds OSM tile numbers and offsets for a sign writing message (all gates plus airport)
        /// </summary>
        /// <param name="zoom">The zoom level to get OSM tiles at</param>
        /// <param name="startItemIndex">Index of first gate in sign writing message</param>
        /// <param name="finishItemIndex">Index of last gate in sign writing message</param>
        /// <returns>The list of tiles</returns>
        static internal List<Tile> SetSignWritingOSMtiles(List<Gate> gates, int zoom, int startItemIndex, int finishItemIndex)
        {
            List<Tile> tiles = [];
            for (int gateIndex = startItemIndex; gateIndex <= finishItemIndex; gateIndex++)
            {
                tiles.Add(MapTileCalculator.GetOSMtile(gates[gateIndex].lon.ToString(), gates[gateIndex].lat.ToString(), zoom));
            }
            return tiles;
        }

        /// <summary>
        /// A bitPosition parameter of '0' would mean test the righthand most bit in the binary segment representation
        /// of the letter. Returns true if the bit at bitPosition is a "1" which means the segment indicated by 
        /// bitPosition forms part of the letter in the message currently being processed.
        /// </summary>
        /// <param name="letter">Ascii letter to check</param>
        /// <param name="bitPosition">Which bit in binary representation of letter to check whether set (equals "1")</param>
        /// <returns></returns>
        static internal bool SegmentIsSet(char letter, int bitPosition)
        {
            string letterBinary = Convert.ToString(letterPath[letter], 2);
            int letterLength = letterBinary.Length;

            if (bitPosition >= letterLength)
            {
                return false;
            }
            else
            {
                if (letterBinary[letterLength - 1 - bitPosition] == '1')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Approximate distance flown in miles as number of segments (number of gates divided by two) times length of a segment
        /// plus 50% for flying between segments.
        /// </summary>
        /// <returns></returns>
        static internal double GetSignWritingDistance()
        {
            return gates.Count / 2 * Parameters.SignSegmentLengthDeg * Constants.degreeLatFeet / Constants.feetInNM * 1.5;
        }
    }
}
